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
    private static readonly string DATALAYER_NS = "org.iringtools.adapter.datalayer";

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
	      ?bnode {{3}} ?literals 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RdfProjectionEngine));

    private IDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private DataDictionary _dataDictionary = null;
    private IList<IDataObject> _dataObjects = null;
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private List<Dictionary<string, string>> _xPathValuePairs = null;  // dictionary of property xpath and value pairs
    private Dictionary<string, List<string>> _hierachicalDTOClasses = null;  // dictionary of class rdlUri and identifiers
    private Dictionary<string, List<string>> _classInstances = null;  // dictionary of class ids and list instances/individuals
    private TripleStore _memoryStore = null;
    private XNamespace _graphNs = String.Empty;
    private string _dataObjectsAssemblyName = String.Empty;
    private string _dataObjectNs = String.Empty;

    [Inject]
    public RdfProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping, DataDictionary dataDictionary)
    {
      _dataLayer = dataLayer;
      _dataObjects = new List<IDataObject>();
      _dataDictionary = dataDictionary;
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

      _dataObjectNs = String.Format("{0}.proj_{1}", 
        DATALAYER_NS, 
        settings["Scope"]
      );

      _dataObjectsAssemblyName = settings["ExecutingAssemblyName"];
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
      List<XElement> roleElements = new List<XElement>();
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
      XElement templateElement = new XElement(OWL_THING);
      templateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

      #region Possessor RoleTypes
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.Possessor))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);
        roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
        templateElement.Add(roleElement);
      }
      #endregion

      #region Reference RoleTypes
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

          roleElement.Add(new XAttribute(RDF_RESOURCE, _graphNs.NamespaceName + "/" + _graphMap.name + "/" + identifierValue));
        }
        else
        {
          roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS.NamespaceName)));
        }

        templateElement.Add(roleElement);
      }
      #endregion

      #region FixedValue RoleTypes
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.FixedValue))
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        roleMapValues.Append(roleMap.value);

        dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
        roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
        roleElement.Add(new XText(roleMap.value));

        templateElement.Add(roleElement);
      }
      #endregion

      #region Property RoleTypes
      foreach (RoleMap roleMap in templateMap.roleMaps.Where(o => o.type == RoleType.Property))
      {
        string[] property = roleMap.propertyName.Split('.');
        string propertyName = property[property.Length - 1].Trim();

        List<IDataObject> dataObjects = GetRelatedObjects(roleMap.propertyName, dataObject);

        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;        
        XElement roleElement;

        foreach (IDataObject dataObj in dataObjects)
        {
          roleElement = new XElement(TPL_NS + roleId);
          string value = Convert.ToString(dataObj.GetPropertyValue(propertyName));
          
          if (String.IsNullOrEmpty(roleMap.valueList))
          {
            if (String.IsNullOrEmpty(value))
            {
              roleElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
            }
            else
            {
              //roleMapValues.Append(value);
              dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
              roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
              roleElement.Add(new XText(value));
            }
          }
          else // resolve value list to uri
          {
            string valueListUri = _mapping.ResolveValueList(roleMap.valueList, value);

            //roleMapValues.Append(valueListUri);
            roleElement.Add(new XAttribute(RDF_RESOURCE, valueListUri));
          }

          roleElements.Add(roleElement);
        }        
        
      }
      #endregion

      List<XElement> templateElements = new List<XElement>();
      XElement pattern;

      foreach (XElement roleElement in roleElements)
      {
        pattern = new XElement(templateElement);
        pattern.Add(roleElement);
        templateElements.Add(pattern);        
      }
      
      //string hashCode = Utility.ComputeHash(templateId + roleMapValues.ToString());
      //templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));      

      return templateElements;
    }

    private void PopulateDataObjects(int classInstanceCount)
    {
      _dataObjects.Clear();

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        List<TemplateMap> templateMaps = pair.Value;
        int dupTemplatePos = 0;

        foreach (TemplateMap templateMap in templateMaps)
        {
          List<RoleMap> propertyMapRoles = new List<RoleMap>();
          string classRoleId = String.Empty;

          #region find propertyMapRoles and classRoleId
          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            if (roleMap.type == RoleType.Possessor)
            {
              classRoleId = roleMap.roleId;
            }
            else if (roleMap.type == RoleType.Property)
            {
              propertyMapRoles.Add(roleMap);
            }
          }
          #endregion

          #region query for property values and save them into dataObjects
          foreach (RoleMap roleMap in propertyMapRoles)
          {
            string query = String.Format(LITERAL_QUERY_TEMPLATE, classMap.classId, classRoleId, templateMap.templateId, roleMap.roleId);
            object results = _memoryStore.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();

              if (_dataObjects.Count == 0)
              {
                string objectType = _dataObjectNs + "." + objectName + ", " + _dataObjectsAssemblyName;
                _dataObjects = _dataLayer.Create(objectType, new string[classInstanceCount]);
              }

              SparqlResultSet resultSet = (SparqlResultSet)results;
              if (resultSet.Count > classInstanceCount)
              {
                dupTemplatePos++;
              }

              int objectIndex = 0;
              int resultSetIndex = (dupTemplatePos == 0) ? 0 : dupTemplatePos - 1;

              while (resultSetIndex < resultSet.Count)
              {
                string value = Regex.Replace(resultSet[resultSetIndex].ToString(), @".*= ", String.Empty);

                if (value == RDF_NIL)
                  value = String.Empty;
                else if (value.Contains("^^"))
                  value = value.Substring(0, value.IndexOf("^^"));
                else if (!String.IsNullOrEmpty(roleMap.valueList))
                  value = _mapping.ResolveValueMap(roleMap.valueList, value);

                _dataObjects[objectIndex++].SetPropertyValue(propertyName, value);

                if (dupTemplatePos == 0)
                  resultSetIndex++;
                else if (dupTemplatePos < 3)
                  resultSetIndex += 2;
                else
                  resultSetIndex += dupTemplatePos;
              }
            }
            else
            {
              throw new Exception("Error querying in-memory triple store.");
            }
          }
          #endregion
        }
      }
    }
    #endregion
  }
}
