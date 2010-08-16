using System;
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
      SELECT ?instance
      WHERE {{{{ 
        ?instance rdf:type {{0}} . 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName);

    private static readonly string LITERAL_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}> 
      SELECT ?literals 
      WHERE {{{{
	      ?instance rdf:type {{0}} . 
	      ?bnode {{1}} ?instance . 
	      ?bnode rdf:type {{2}} . 
	      ?bnode {{3}} {{4}} . 
	      ?bnode {{5}} ?literals 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RdfProjectionEngine));

    private IDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private IList<IDataObject> _dataObjects = null;
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private List<Dictionary<string, string>> _xPathValuePairs = null;  // dictionary of property xpath and value pairs
    private Dictionary<string, List<string>> _hierachicalDTOClasses = null;  // dictionary of class rdlUri and identifiers
    private Dictionary<string, List<string>> _classInstances = null;  // dictionary of class ids and list instances/individuals
    private TripleStore _memoryStore = null;
    private XNamespace _graphNs = String.Empty;

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

      _graphNs = String.Format("{0}{1}/{2}",
        settings["GraphBaseUri"],
        settings["ProjectName"],
        settings["ApplicationName"]
      );
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
      PopulateDataObjects(GetClassInstanceCount());
      return _dataObjects;
    }

    #region helper methods
    private int GetClassInstanceCount()
    {
      ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
      string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classMap.classId);
      object results = _memoryStore.ExecuteQuery(query);

      if (results is SparqlResultSet)
      {
        SparqlResultSet resultSet = (SparqlResultSet)results;
        return resultSet.Count;
      }

      throw new Exception("Error querying instances of class [" + classMap.name + "].");
    }

    private void PopulateClassIdentifiers()
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

      PopulateClassIdentifiers();
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

    private List<IDataObject> GetRelatedObjects(string propertyPath, IDataObject dataObject)
    {
      //propertyPath = "Instrument.LineItems.Tag";
      List<IDataObject> parentObjects = new List<IDataObject>();
      List<IDataObject> relatedObjects = null;

      string[] objectPath = propertyPath.Split('.');

      parentObjects.Add(dataObject);

      for (int i = 0; i < objectPath.Length - 1; i++)
      {
        foreach (IDataObject parentObj in parentObjects)
        {
          if (parentObj.GetType().Name != objectPath[i])
          {
            relatedObjects = new List<IDataObject>();

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

    private List<XElement> CreateRdfTemplateElement(string classInstance, TemplateMap templateMap, IDataObject dataObject)
    {
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);
      StringBuilder roleMapValues = new StringBuilder();

      //RelatedObject cache
      Dictionary<string, List<IDataObject>> relatedObjects = new Dictionary<string, List<IDataObject>>();

      XElement preElement = new XElement(OWL_THING);
      preElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));
      //preElement.Add(new XAttribute(RDF_ABOUT, ""));

      #region RoleType.Possessor
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.Possessor))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
        preElement.Add(roleElement);
        break;
      }
      #endregion

      #region RoleType.Reference
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.Reference))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        roleMapValues.Append(roleMap.value);
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
              //string[] property = identifier.Split('.');
              //string objectName = property[0].Trim();
              //string propertyName = property[1].Trim();

              #region Process RoleMapping
              string propertyName = identifier.Substring(identifier.LastIndexOf('.') + 1);
              string objectPath = identifier.Substring(0, identifier.LastIndexOf('.'));
              List<IDataObject> valueObjects;

              //Get Related Object(s)
              if (!relatedObjects.TryGetValue(objectPath, out valueObjects))
              {
                valueObjects = GetRelatedObjects(identifier, dataObject);
                relatedObjects.Add(objectPath, valueObjects);
              }
              #endregion

              foreach (IDataObject valueObject in valueObjects)
              {
                string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                if (identifierValue != String.Empty)
                  identifierValue += roleMap.classMap.identifierDelimeter;

                identifierValue += value;
              }
            }
          }

          roleElement.Add(new XAttribute(RDF_RESOURCE, _graphNs.NamespaceName + "/" + _graphMap.name + "/" + identifierValue));
        }
        else
        {
          roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS.NamespaceName)));
        }

        preElement.Add(roleElement);
        //break;

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
        break;
      }
      #endregion

      #region RoleType.Property
      List<RoleMap> roleMaps = templateMap.roleMaps.Where(o => o.type == RoleType.Property).ToList<RoleMap>();
      if (roleMaps.Count == 0)
      {
        return new List<XElement> { preElement };
      }

      int maxRelatedObjectsCount = 0;
      //Dictionary<string, List<IDataObject>> relatedObjects = new Dictionary<string, List<IDataObject>>();
      Dictionary<string, RoleMap> objectPathRoleMaps = new Dictionary<string, RoleMap>();

      foreach (RoleMap roleMap in roleMaps)
      {
        string propertyMap = roleMap.propertyName;
        string propertyName = propertyMap.Substring(propertyMap.LastIndexOf('.') + 1);
        string objectPath = propertyMap.Substring(0, propertyMap.IndexOf('.'));
        List<IDataObject> valueObjects;

        //Get Related Object(s)
        if (!relatedObjects.TryGetValue(objectPath, out valueObjects))
        {
          valueObjects = GetRelatedObjects(propertyName, dataObject);
          relatedObjects.Add(propertyMap, valueObjects);
          objectPathRoleMaps.Add(propertyMap, roleMap);

          if (valueObjects.Count > maxRelatedObjectsCount)
            maxRelatedObjectsCount = valueObjects.Count;
        }
      }

      XElement[] templateElements = new XElement[maxRelatedObjectsCount];
      for (int i = 0; i < maxRelatedObjectsCount; i++)
      {
        templateElements[i] = new XElement(preElement);

        foreach (var pair in relatedObjects)
        {
          string propertyMap = pair.Key;
          List<IDataObject> valueObjects = pair.Value;
          RoleMap roleMap = objectPathRoleMaps[propertyMap];

          string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
          string propertyName = propertyMap.Substring(propertyMap.LastIndexOf('.') + 1);

          XElement roleElement = null;
          for (int j = 0; j < maxRelatedObjectsCount; j++)
          {
            IDataObject valueObject = null;
            if (j < valueObjects.Count)
            {
              valueObject = valueObjects[j];
              roleElement = new XElement(TPL_NS + roleId);

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
                  roleMapValues.Append(value);
                  string dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
                  roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
                  roleElement.Add(new XText(value));
                }
              }
              else // resolve value list to uri
              {
                string valueListUri = _mapping.ResolveValueList(roleMap.valueList, value);

                roleMapValues.Append(valueListUri);
                roleElement.Add(new XAttribute(RDF_RESOURCE, valueListUri));
              }
              #endregion

              templateElements[i].Add(roleElement);
            }
            else
            {
              templateElements[i].Add(roleElement);
            }
          }
        }
      }
      #endregion

      //GvR not sure how to resolve this
      //string hashCode = Utility.ComputeHash(templateId + roleMapValues.ToString());
      //templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

      return templateElements.ToList<XElement>();
    }

    private void PopulateDataObjects(int classInstanceCount)
    {
      _dataObjects = _dataLayer.Create(_graphMap.dataObjectMap, new string[classInstanceCount]);

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        List<TemplateMap> templateMaps = pair.Value;

        foreach (TemplateMap templateMap in templateMaps)
        {
          string possessorRoleId = String.Empty;
          string referenceRoleId = String.Empty;
          string referenceClassId = String.Empty;
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
                referenceRoleId = roleMap.roleId;
                referenceClassId = roleMap.value;
                break;

              case RoleType.Property:
                propertyRoleMaps.Add(roleMap);
                break;
            }
          }

          // query for property roleMaps values
          foreach (RoleMap roleMap in propertyRoleMaps)
          {
            string[] property = roleMap.propertyName.Split('.');
            string propertyName = property[property.Length - 1].Trim();
            string query = String.Format(LITERAL_QUERY_TEMPLATE,
              classMap.classId, possessorRoleId, templateMap.templateId, referenceRoleId, referenceClassId, roleMap.roleId);
            object results = _memoryStore.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
              SparqlResultSet sparqlResultSet = (SparqlResultSet)results;
              int dataObjectIndex = 0;

              foreach (SparqlResult sparqlResult in sparqlResultSet)
              {
                string value = Regex.Replace(sparqlResult.ToString(), @".*= ", String.Empty);

                if (value == RDF_NIL)
                  value = String.Empty;
                else if (value.Contains("^^"))
                  value = value.Substring(0, value.IndexOf("^^"));
                else if (!String.IsNullOrEmpty(roleMap.valueList))
                  value = _mapping.ResolveValueMap(roleMap.valueList, value);

                _dataObjects[dataObjectIndex++].SetPropertyValue(propertyName, value);
              }
            }
          }
        }
      }
    }
    #endregion
  }
}
