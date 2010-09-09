﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace org.iringtools.adapter.projection
{
  public class RdfProjectionEngine : IProjectionLayer
  {
    private static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    private static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    private static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    private static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly XName OWL_THING = OWL_NS + "Thing";
    private static readonly XName RDF_ABOUT = RDF_NS + "about";
    private static readonly XName RDF_DESCRIPTION = RDF_NS + "Description";
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string XSD_PREFIX = "xsd:";
    private static readonly string RDF_PREFIX = "rdf:";
    private static readonly string RDL_PREFIX = "rdl:";
    private static readonly string TPL_PREFIX = "tpl:";
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    private static readonly string CLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      SELECT ?class
      WHERE {{{{ 
        ?class rdf:type {{0}} . 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName);

    private static readonly string SUBCLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}> 
      PREFIX i:   <{{0}}>
      SELECT ?subclass 
      WHERE {{{{
	      ?bnode {{1}} i:{{2}} . 
	      ?bnode rdf:type {{3}} . 
	      ?bnode {{4}} {{5}} . 
	      ?bnode {{6}} ?subclass 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    private static readonly string LITERAL_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}> 
      PREFIX i:   <{{0}}>
      SELECT ?literals 
      WHERE {{{{
	      ?bnode {{1}} i:{{2}} . 
	      ?bnode rdf:type {{3}} . 
	      ?bnode {{4}} {{5}} . 
	      ?bnode {{6}} ?literals 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RdfProjectionEngine));

    private AdapterSettings _settings;
    private IDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private IList<IDataObject> _dataObjects = null;
    private List<string> _relatedObjectPaths = null;
    private Dictionary<string, IList<IDataObject>>[] _relatedObjects = null;
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private List<Dictionary<string, string>> _xPathValuePairs = null;  // dictionary of property xpath and value pairs
    private Dictionary<string, List<string>> _hierachicalDTOClasses = null;  // dictionary of class rdlUri and identifiers
    private Dictionary<string, List<string>> _classInstances = null;  // dictionary of class ids and list instances/individuals
    private TripleStore _memoryStore = null;
    private XNamespace _graphNs = String.Empty;
    private string _graphBaseUri = String.Empty;

    [Inject]
    public RdfProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _dataLayer = dataLayer;
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      _xPathValuePairs = new List<Dictionary<string, string>>();
      _hierachicalDTOClasses = new Dictionary<string, List<string>>();
      _classInstances = new Dictionary<string, List<string>>();
      _mapping = mapping;

      _graphNs = String.Format("{0}/{1}/{2}",
        settings["GraphBaseUri"],
        settings["ProjectName"],
        settings["ApplicationName"]
      );

      _settings = settings;
    }

    public XElement GetXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        return GetRdf();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public IList<IDataObject> GetDataObjects(string graphName, ref XElement xml)
    {
      _graphBaseUri = _settings["TargetGraphBaseUri"];
      if (!_graphBaseUri.EndsWith("/"))
      {
        _graphBaseUri += "/";
      }

      _graphMap = _mapping.FindGraphMap(graphName);

      XmlDocument xdoc = new XmlDocument();
      xdoc.LoadXml(xml.ToString());
      xml.RemoveAll();

      RdfXmlParser parser = new RdfXmlParser();
      Graph graph = new Graph();
      parser.Load(graph, xdoc);
      xdoc.RemoveAll();

      // load graph to memory store to allow querying locally
      _memoryStore = new TripleStore();
      _memoryStore.Add(graph);
      graph.Dispose();

      // fill data objects and return
      ResetClassInstances();

      var rootClassTemplatesMap = _classInstances.First();
      string rootClassId = rootClassTemplatesMap.Key;
      List<string> rootClassInstances = rootClassTemplatesMap.Value;
      int classInstanceCount = rootClassInstances.Count;
      _dataObjects = _dataLayer.Create(_graphMap.dataObjectMap, new string[classInstanceCount]);
      _relatedObjects = new Dictionary<string, IList<IDataObject>>[classInstanceCount];
      _relatedObjectPaths = new List<string>();

      for (int i = 0; i < rootClassInstances.Count; i++)
      {
        _relatedObjects[i] = new Dictionary<string, IList<IDataObject>>();
        SetDataObjects(rootClassId, rootClassInstances[i], i);
      }

      SetIntermediateRelatedObjects();

      // add related data objects to top level data objects
      foreach (Dictionary<string, IList<IDataObject>> relatedObjectDictionary in _relatedObjects)
      {
        foreach (var pair in relatedObjectDictionary)
        {
          foreach (IDataObject relatedObject in pair.Value)
          {
            _dataObjects.Add(relatedObject);
          }
        }
      }

      return _dataObjects;
    }

    #region helper methods
    private void ResetClassInstances()
    {
      _classInstances.Clear();

      if (_graphMap.classTemplateListMaps.Count == 0)
        return;

      var pair = _graphMap.classTemplateListMaps.First();
      string classId = pair.Key.classId;
      string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classId);
      object results = _memoryStore.ExecuteQuery(query);

      if (results != null)
      {
        SparqlResultSet resultSet = (SparqlResultSet)results;

        foreach (SparqlResult result in resultSet)
        {
          string classInstance = result.ToString().Remove(0, ("?class = " + _graphBaseUri).Length);

          if (!String.IsNullOrEmpty(classInstance))
          {
            if (!_classInstances.ContainsKey(classId))
            {
              _classInstances[classId] = new List<string> { classInstance };
            }
            else if (!_classInstances[classId].Contains(classInstance))
            {
              _classInstances[classId].Add(classInstance);
            }
          }
        }
      }
    }

    private void ResetClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
      {
        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
              if (classIdentifiers.Count == i)
              {
                classIdentifiers.Add(value);
              }
              else
              {
                classIdentifiers[i] += classMap.identifierDelimeter + value;
              }
            }
          }
          else  // identifier comes from a property
          {
            string[] property = identifier.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();

            if (_dataObjects != null)
            {
              for (int i = 0; i < _dataObjects.Count; i++)
              {
                string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (classIdentifiers.Count == i)
                {
                  classIdentifiers.Add(value);
                }
                else
                {
                  classIdentifiers[i] += classMap.identifierDelimeter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.classId] = classIdentifiers;
      }
    }

    private XElement GetRdf()
    {
      XElement graphElement = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      ResetClassIdentifiers();
      _classInstances.Clear();

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;

        for (int i = 0; i < _dataObjects.Count; i++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          string classInstance = _graphNs.NamespaceName + "/" + _graphMap.name + "/" + _classIdentifiers[classMap.classId][i];

          if (!_classInstances.ContainsKey(classId))
          {
            _classInstances[classId] = new List<string> { classInstance };
            graphElement.Add(CreateRdfClassElement(classId, classInstance));
          }
          else if (!_classInstances[classId].Contains(classInstance))
          {
            _classInstances[classId].Add(classInstance);
            graphElement.Add(CreateRdfClassElement(classId, classInstance));
          }

          foreach (TemplateMap templateMap in pair.Value)
          {
            foreach (XElement template in CreateRdfTemplateElement(classInstance, templateMap, _dataObjects[i]))
            {
              graphElement.Add(template);
            }
          }
        }
      }

      return graphElement;
    }

    private XElement CreateRdfClassElement(string classId, string classInstance)
    {
      return new XElement(OWL_THING, new XAttribute(RDF_ABOUT, classInstance),
        new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));
    }

    private List<XElement> CreateRdfTemplateElement(string classInstance, TemplateMap templateMap, IDataObject dataObject)
    {
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);
      StringBuilder roleMapValues = new StringBuilder();

      XElement preElement = new XElement(OWL_THING);
      preElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

      #region RoleType.Possessor
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.Possessor))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
        preElement.Add(roleElement);
      }
      #endregion

      #region RoleType.Reference
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.Reference))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        if (roleMap.classMap != null)
        {
          string identifierValue = String.Empty;

          foreach (string identifier in roleMap.classMap.identifiers)
          {
            if (identifier.StartsWith("#") && identifier.EndsWith("#"))
            {
              identifierValue += identifier.Substring(1, identifier.Length - 2);
            }
            else
            {
              string[] property = identifier.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();

              if (dataObject != null)
              {
                string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                if (identifierValue != String.Empty)
                  identifierValue += roleMap.classMap.identifierDelimeter;

                identifierValue += value;
              }
            }
          }

          roleMapValues.Append(identifierValue);
          roleElement.Add(new XAttribute(RDF_RESOURCE, _graphNs.NamespaceName + "/" + _graphMap.name + "/" + identifierValue));
        }
        else
        {
          roleMapValues.Append(roleMap.value);
          roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS.NamespaceName)));
        }

        preElement.Add(roleElement);
      }
      #endregion

      #region RoleType.FixedValue
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.FixedValue))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        roleMapValues.Append(roleMap.value);
        dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
        roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
        roleElement.Add(new XText(roleMap.value));

        preElement.Add(roleElement);
      }
      #endregion

      #region RoleType.Property

      //RelatedObject cache
      Dictionary<string, List<IDataObject>> relatedObjects = new Dictionary<string, List<IDataObject>>();

      List<RdfElement> templateElements = new List<RdfElement>();
      List<RdfElement> propertyElements = new List<RdfElement>();

      //Add orignal template without property values to parent array
      templateElements.Add(new RdfElement { Element = preElement, Values = roleMapValues.ToString() });

      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.Property))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);

        #region Process PropertyMapping
        string propertyMap = roleMap.propertyName;
        string propertyName = propertyMap.Substring(propertyMap.LastIndexOf('.') + 1);
        string objectPath = propertyMap.Substring(0, propertyMap.LastIndexOf('.'));
        List<IDataObject> valueObjects;

        //Get Related Object(s)
        if (!relatedObjects.TryGetValue(objectPath, out valueObjects))
        {
          valueObjects = GetRelatedObjects(roleMap.propertyName, dataObject);
          relatedObjects.Add(objectPath, valueObjects);
        }
        #endregion

        foreach (RdfElement rdfElement in templateElements)
        {
          foreach (IDataObject valueObject in valueObjects)
          {
            string dataType = String.Empty;
            XElement roleElement = new XElement(TPL_NS + roleId);

            #region Process PropertyValue
            string value = Convert.ToString(valueObject.GetPropertyValue(propertyName));
            if (String.IsNullOrEmpty(roleMap.valueList))
            {
              if (String.IsNullOrEmpty(value))
              {
                roleElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
              }
              else
              {
                rdfElement.Append(value);
                dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
                roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
                roleElement.Add(new XText(value));
              }
            }
            else // resolve value list to uri
            {
              string valueListUri = _mapping.ResolveValueList(roleMap.valueList, value);

              rdfElement.Append(valueListUri);
              roleElement.Add(new XAttribute(RDF_RESOURCE, valueListUri));
            }
            #endregion

            //Copy the template and add the current property value
            RdfElement copyElement = (RdfElement)rdfElement.Clone();
            copyElement.Element.Add(roleElement);
            propertyElements.Add(copyElement);
          }
        }

        //Swap the arrays around for futher processing
        templateElements = propertyElements.GetRange(0, propertyElements.Count);
        propertyElements.Clear();
      }
      #endregion

      List<XElement> templates = new List<XElement>();
      foreach (RdfElement rdfElement in templateElements)
      {
        XElement template = rdfElement.Element;
        string hashCode = Utility.MD5Hash(templateId + rdfElement.Values);
        template.Add(new XAttribute(RDF_ABOUT, hashCode));
        templates.Add(template);
      }

      return templates;
    }

    private List<IDataObject> GetRelatedObjects(string propertyPath, IDataObject dataObject)
    {
      //propertyPath = "Instrument.LineItems.Tag";
      List<IDataObject> parentObjects = new List<IDataObject>();
      string[] objectPath = propertyPath.Split('.');

      parentObjects.Add(dataObject);

      for (int i = 0; i < objectPath.Length - 1; i++)
      {
        foreach (IDataObject parentObj in parentObjects)
        {
          if (parentObj.GetType().Name != objectPath[i])
          {
            List<IDataObject> relatedObjects = new List<IDataObject>();

            foreach (IDataObject relatedObj in _dataLayer.GetRelatedObjects(parentObj, objectPath[i]))
            {
              if (!relatedObjects.Contains(relatedObj))
              {
                relatedObjects.Add(relatedObj);
              }
            }

            parentObjects = relatedObjects;
          }
        }
      }

      return parentObjects;
    }

    // senario (assume no circular relationships - should be handled by AppEditor): 
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property1
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property2
    private void SetLastRelatedObjects(int dataObjectIndex, string propertyPath, List<string> relatedValues)
    {
      Dictionary<string, IList<IDataObject>> relatedObjectDictionary = _relatedObjects[dataObjectIndex];
      int lastDotPosition = propertyPath.LastIndexOf('.');
      string property = propertyPath.Substring(lastDotPosition + 1);
      string objectPathString = propertyPath.Substring(0, lastDotPosition);  // exclude property
      string[] objectPath = objectPathString.Split('.');

      if (!_relatedObjectPaths.Contains(objectPathString))
        _relatedObjectPaths.Add(objectPathString);

      // top level data objects are processed separately, so start with 1
      for (int i = 1; i < objectPath.Length; i++)
      {
        string relatedObjectType = objectPath[i];
        IList<IDataObject> relatedObjects = null;

        if (relatedObjectDictionary.ContainsKey(relatedObjectType))
        {
          relatedObjects = relatedObjectDictionary[relatedObjectType];
        }
        else
        {
          if (i == objectPath.Length - 1)  // last related object in the chain
          {
            relatedObjects = _dataLayer.Create(relatedObjectType, new string[relatedValues.Count]);
          }
          else // intermediate related object
          {
            relatedObjects = _dataLayer.Create(relatedObjectType, null);
          }

          relatedObjectDictionary.Add(relatedObjectType, relatedObjects);
        }

        // only fill last related object values now; values of intermediate related objects' parent might not be available yet.
        if (i == objectPath.Length - 1)
        {
          for (int j = 0; j < relatedValues.Count; j++)
          {
            relatedObjects[j].SetPropertyValue(property, relatedValues[j]);
          }
        }
      }
    }

    // senario:
    //  dataObject1.L1RelatedDataObjects.property1
    //  dataObject1.L1RelatedDataObjects.property2
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.property3.value1
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.property4.value2
    //
    // L2RelatedDataObjects result:
    //  dataObject1.L1RelatedDataObjects[1].L2RelatedDataObjects[1].property3.value1
    //  dataObject1.L1RelatedDataObjects[1].L2RelatedDataObjects[2].property4.value2
    //  dataObject1.L1RelatedDataObjects[2].L2RelatedDataObjects[1].property3.value1
    //  dataObject1.L1RelatedDataObjects[2].L2RelatedDataObjects[2].property4.value2
    private void SetIntermediateRelatedObjects()
    {
      DataDictionary dictionary = _dataLayer.GetDictionary();

      for (int i = 0; i < _dataObjects.Count; i++)
      {
        Dictionary<string, IList<IDataObject>> relatedObjectDictionary = _relatedObjects[i];

        foreach (string relatedObjectPath in _relatedObjectPaths)
        {
          string[] relatedObjectPathElements = relatedObjectPath.Split('.');

          for (int j = 0; j < relatedObjectPathElements.Length - 1; j++)
          {
            string parentObjectType = relatedObjectPathElements[j];
            string relatedObjectType = relatedObjectPathElements[j + 1];

            if (relatedObjectDictionary.ContainsKey(relatedObjectType))
            {
              IList<IDataObject> parentObjects = null;

              if (j == 0)
                parentObjects = new List<IDataObject> { _dataObjects[i] };
              else
                parentObjects = relatedObjectDictionary[parentObjectType];

              IList<IDataObject> relatedObjects = relatedObjectDictionary[relatedObjectType];

              foreach (IDataObject parentObject in parentObjects)
              {
                DataObject dataObject = dictionary.dataObjects.First(c => c.objectName == parentObjectType);
                DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relationshipName == relatedObjectType);

                foreach (IDataObject relatedObject in relatedObjects)
                {
                  foreach (PropertyMap map in dataRelationship.propertyMaps)
                  {
                    relatedObject.SetPropertyValue(map.relatedPropertyName, parentObject.GetPropertyValue(map.dataPropertyName));
                  }
                }
              }
            }
          }
        }
      }
    }

    private void SetDataObjects(string classId, string classInstance, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> pair = _graphMap.GetClassTemplateListMap(classId);
      List<TemplateMap> templateMaps = pair.Value;

      foreach (TemplateMap templateMap in templateMaps)
      {
        string possessorRoleId = String.Empty;
        string referenceRoleId = String.Empty;
        string referenceClassId = String.Empty;
        string nextClassReferenceId = String.Empty;
        ClassMap nextClassMap = null;
        List<RoleMap> propertyRoleMaps = new List<RoleMap>();

        // find property roleMaps
        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          switch (roleMap.type)
          {
            case RoleType.Possessor:
              possessorRoleId = roleMap.roleId;
              break;

            case RoleType.Reference:
              if (roleMap.classMap != null)
              {
                nextClassMap = roleMap.classMap;
                nextClassReferenceId = roleMap.roleId;
              }
              else
              {
                referenceRoleId = roleMap.roleId;
                referenceClassId = roleMap.value;
              }

              break;

            case RoleType.Property:
              propertyRoleMaps.Add(roleMap);
              break;
          }
        }

        if (nextClassMap != null)
        {
          string query = String.Format(SUBCLASS_INSTANCE_QUERY_TEMPLATE, _graphBaseUri, possessorRoleId, classInstance,
              templateMap.templateId, referenceRoleId, referenceClassId, nextClassReferenceId);

          object results = _memoryStore.ExecuteQuery(query);

          if (results is SparqlResultSet)
          {
            SparqlResultSet resultSet = (SparqlResultSet)results;

            foreach (SparqlResult result in resultSet)
            {
              string nextClassInstance = result.ToString().Remove(0, ("?subclass = " + _graphBaseUri).Length);
              SetDataObjects(nextClassMap.classId, nextClassInstance, dataObjectIndex);
              break;  // should be one result only
            }
          }
        }
        else // query for property roleMaps values
        {
          foreach (RoleMap roleMap in propertyRoleMaps)
          {
            string query = String.Format(LITERAL_QUERY_TEMPLATE, _graphBaseUri, possessorRoleId, classInstance,
                templateMap.templateId, referenceRoleId, referenceClassId, roleMap.roleId);

            object results = _memoryStore.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
              string[] propertyPath = roleMap.propertyName.Split('.');
              string property = propertyPath[propertyPath.Length - 1].Trim();
              List<string> values = new List<string>();

              SparqlResultSet resultSet = (SparqlResultSet)results;

              foreach (SparqlResult result in resultSet)
              {
                string value = Regex.Replace(result.ToString(), @".*= ", String.Empty);

                if (value == RDF_NIL)
                  value = String.Empty;
                else if (value.Contains("^^"))
                  value = value.Substring(0, value.IndexOf("^^"));
                else if (!String.IsNullOrEmpty(roleMap.valueList))
                  value = _mapping.ResolveValueMap(roleMap.valueList, value);

                if (propertyPath.Length > 2)
                {
                  values.Add(value);
                }
                else
                {
                  _dataObjects[dataObjectIndex].SetPropertyValue(property, value);
                }
              }

              if (propertyPath.Length > 2)
              {
                SetLastRelatedObjects(dataObjectIndex, roleMap.propertyName, values);
              }
            }
          }
        }
      }
    }
    #endregion
  }

  public class RdfElement : ICloneable
  {
    private StringBuilder builder;
    public XElement Element { get; set; }
    public String Values
    {
      get
      {
        if (builder != null)
        {
          return builder.ToString();
        }
        else
        {
          return String.Empty;
        }
      }
      set
      {
        builder = new StringBuilder(value);
      }
    }

    public void Append(String value)
    {
      builder.Append(value);
    }

    public object Clone()
    {
      RdfElement clone = new RdfElement();
      clone.Element = new XElement(Element);
      clone.Values = Values;
      return clone;
    }
  }
}
