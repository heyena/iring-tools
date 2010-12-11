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
using System.Web;

namespace org.iringtools.adapter.projection
{
  public class RdfProjectionEngine : BaseProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(RdfProjectionEngine));
    private Dictionary<string, List<IDataObject>> _relatedObjectsCache = null;
    private XElement _rdfXml = null;
    
    [Inject]
    public RdfProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _settings = settings;
      _dataLayer = dataLayer;
      _mapping = mapping;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XDocument rdfXml = null;

      try
      {
        _graphBaseUri = String.Format("{0}{1}/{2}/{3}/",
          _settings["GraphBaseUri"],
          HttpUtility.UrlEncode(_settings["ProjectName"]),
          HttpUtility.UrlEncode(_settings["ApplicationName"]),
          HttpUtility.UrlEncode(graphName)
        );

        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 && 
          _dataObjects != null && _dataObjects.Count > 0)
        {
          SetClassIdentifiers(DataDirection.Outbound);
          rdfXml = new XDocument(BuildRdfXml());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return rdfXml;
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument)
    {
      _dataObjects = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 && xDocument != null)
        {

          XmlDocument xmlDocument = new XmlDocument();
          using(XmlReader xmlReader = xDocument.CreateReader())
          {
              xmlDocument.Load(xmlReader);
          }
          xDocument.Root.RemoveAll();
          
          RdfXmlParser parser = new RdfXmlParser();
          Graph graph = new Graph();
          parser.Load(graph, xmlDocument);
          xmlDocument.RemoveAll();

          // load graph to memory store to allow querying locally
          _memoryStore = new TripleStore();
          _memoryStore.Add(graph);
          graph.Dispose();

          SetClassIdentifiers(DataDirection.InboundSparql);

          if (_classIdentifiers.Count > 0)
          {
            var rootClassTemplatesMap = _classIdentifiers.First();
            string rootClassId = rootClassTemplatesMap.Key;
            List<string> rootClassInstances = rootClassTemplatesMap.Value;
            int classInstanceCount = rootClassInstances.Count;
            _dataObjects = _dataLayer.Create(_graphMap.dataObjectMap, new string[classInstanceCount]);
            _relatedObjects = new Dictionary<string, IList<IDataObject>>[classInstanceCount];
            _relatedObjectPaths = new List<string>();

            if (_memoryStore != null)
            {
              for (int i = 0; i < rootClassInstances.Count; i++)
              {
                _relatedObjects[i] = new Dictionary<string, IList<IDataObject>>();
                CreateDataObjects(rootClassId, rootClassInstances[i], i);
              }

              // add related data objects
              if (_relatedObjectPaths != null && _relatedObjectPaths.Count > 0)
              {
                SetRelatedObjects();

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
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return _dataObjects;
    }

    #region helper methods
    private XElement BuildRdfXml()
    {
      Dictionary<string, List<string>> classInstancesCache = new Dictionary<string, List<string>>();

      _rdfXml = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;

        for (int i = 0; i < _dataObjects.Count; i++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          string classInstance = _graphBaseUri + _classIdentifiers[classMap.classId][i];
          bool classExists = true;

          _relatedObjectsCache = new Dictionary<string, List<IDataObject>>();
          
          if (!classInstancesCache.ContainsKey(classId))
          {
            classInstancesCache[classId] = new List<string> { classInstance };
            AddRdfClassElement(classId, classInstance);
            classExists = false;
          }
          else if (!classInstancesCache[classId].Contains(classInstance))
          {
            classInstancesCache[classId].Add(classInstance);
            AddRdfClassElement(classId, classInstance);
            classExists = false;
          }

          if (!classExists)
          {
            foreach (TemplateMap templateMap in pair.Value)
              AddRdfTemplateElements(classInstance, templateMap, i);
          }
        }
      }

      return _rdfXml;
    }

    private void AddRdfClassElement(string classId, string classInstance)
    {
      XElement classElement = new XElement(OWL_THING, new XAttribute(RDF_ABOUT, classInstance),
        new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));

      _rdfXml.Add(classElement);
    }

    private void AddRdfTemplateElements(string classInstance, TemplateMap templateMap, int dataObjectIndex)
    {
      IDataObject dataObject = _dataObjects[dataObjectIndex];
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);

      List<RoleMap> propertyRoles = new List<RoleMap>();
      XElement baseTemplateElement = new XElement(OWL_THING);
      baseTemplateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));
      StringBuilder baseValues = new StringBuilder();
      RoleMap classRole = null;
      
      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        XElement roleElement = new XElement(TPL_NS + roleId);
            
        switch (roleMap.type)
        {
          case RoleType.Possessor:
            roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
            baseTemplateElement.Add(roleElement);
            baseValues.Append(classInstance);
            break;

          case RoleType.FixedValue:
            string dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
            roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
            roleElement.Add(new XText(roleMap.value));
            baseTemplateElement.Add(roleElement);
            baseValues.Append(roleMap.value);
            break;

          case RoleType.Reference:
            if (roleMap.classMap != null)
              classRole = roleMap;
            else
            {
              roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS.NamespaceName)));
              baseTemplateElement.Add(roleElement);
              baseValues.Append(roleMap.value);
            }
            break;

          case RoleType.Property:
          case RoleType.DataProperty:
          case RoleType.ObjectProperty:
            propertyRoles.Add(roleMap);
            break;
        }
      }

      if (classRole != null)
      {
        string identifier = _classIdentifiers[classRole.classMap.classId][dataObjectIndex];
        baseValues.Append(identifier);

        string hashCode = Utility.MD5Hash(templateId + baseValues.ToString());
        baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

        string roleId = classRole.roleId.Substring(classRole.roleId.IndexOf(":") + 1);
        XElement roleElement = new XElement(TPL_NS + roleId);
        roleElement.Add(new XAttribute(RDF_RESOURCE, _graphBaseUri + identifier));
        baseTemplateElement.Add(roleElement);

        _rdfXml.Add(baseTemplateElement);        
      }
      else
      {
        List<List<XElement>> multiPropertyElements = new List<List<XElement>>();
        List<IDataObject> valueObjects = null;

        foreach (RoleMap propertyRole in propertyRoles)
        {
          List<XElement> propertyElements = new List<XElement>();
          multiPropertyElements.Add(propertyElements);

          string propertyMap = propertyRole.propertyName;
          int lastDotPos = propertyMap.LastIndexOf('.');
          string propertyName = propertyMap.Substring(lastDotPos + 1);
          string objectPath = propertyMap.Substring(0, lastDotPos);

          if (propertyMap.Split('.').Length > 2)  // related property
          {
            if (!_relatedObjectsCache.TryGetValue(objectPath, out valueObjects))
            {
              valueObjects = GetRelatedObjects(propertyRole.propertyName, _dataObjects[dataObjectIndex]);
              _relatedObjectsCache.Add(objectPath, valueObjects);
            }
          }
          else  // direct property
          {
            valueObjects = new List<IDataObject> { _dataObjects[dataObjectIndex] };
          }

          foreach (IDataObject valueObject in valueObjects)
          {
            string value = Convert.ToString(valueObject.GetPropertyValue(propertyName));

            XElement propertyElement = new XElement(TPL_NS + propertyRole.roleId.Replace(TPL_PREFIX, String.Empty));
            propertyElements.Add(propertyElement);

            if (String.IsNullOrEmpty(propertyRole.valueList))
            {
              if (String.IsNullOrEmpty(value))
                propertyElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
              else
              {
                propertyElement.Add(new XAttribute(RDF_DATATYPE, propertyRole.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName)));

                if (propertyRole.dataType.Contains("dateTime"))
                  value = Utility.ToXsdDateTime(value);

                propertyElement.Add(new XText(value));
              }
            }
            else // resolve value list to uri
            {
              value = _mapping.ResolveValueList(propertyRole.valueList, value);

              if (value == null)
                value = RDF_NIL;
              else
                value = value.Replace("rdl:", RDL_NS.NamespaceName);

              propertyElement.Add(new XAttribute(RDF_RESOURCE, value));
            }
          }
        }

        if (valueObjects != null)
        {
          for (int i = 0; i < valueObjects.Count; i++)
          {
            XElement templateElement = new XElement(baseTemplateElement);
            _rdfXml.Add(templateElement);
            
            StringBuilder templateValue = new StringBuilder(baseValues.ToString());
            for (int j = 0; j < propertyRoles.Count; j++)
            {
              XElement propertyElement = multiPropertyElements[j][i];
              templateElement.Add(propertyElement);

              if (!String.IsNullOrEmpty(propertyElement.Value))
                templateValue.Append(propertyElement.Value);
              else
                templateValue.Append(propertyElement.Attribute(RDF_RESOURCE).Value);
            }

            string hashCode = Utility.MD5Hash(templateId + templateValue.ToString());
            templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
          }
        }
      }
    }

    private void CreateDataObjects(string classId, string classInstance, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> pair = _graphMap.GetClassTemplateListMap(classId);
      List<TemplateMap> templateMaps = pair.Value;

      foreach (TemplateMap templateMap in templateMaps)
      {
        string possessorRoleId = String.Empty;
        RoleMap referenceRole = null;
        RoleMap classRole = null;
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
                classRole = roleMap;
              else
                referenceRole = roleMap;
              break;

            case RoleType.Property:
            case RoleType.DataProperty:
            case RoleType.ObjectProperty:
              propertyRoleMaps.Add(roleMap);
              break;
          }
        }

        string referenceVariable = String.Empty;
        string referenceRoleId = String.Empty;
        string referenceRoleValue = String.Empty;
        string referenceEndStmt = String.Empty;

        if (referenceRole != null)
        {
          referenceVariable = BLANK_NODE;
          referenceRoleId = referenceRole.roleId;
          referenceRoleValue = referenceRole.value;
          referenceEndStmt = END_STATEMENT;
        }

        if (classRole != null)
        {
          string query = String.Format(SUBCLASS_INSTANCE_QUERY_TEMPLATE, possessorRoleId, classInstance,
              templateMap.templateId, referenceVariable, referenceRoleId, referenceRoleValue, referenceEndStmt, classRole.roleId);

          Utility.WriteString(query, "./Logs/Sparql.log", true);

          object results = _memoryStore.ExecuteQuery(query);

          if (results is SparqlResultSet)
          {
            SparqlResultSet resultSet = (SparqlResultSet)results;

            foreach (SparqlResult result in resultSet)
            {
              string subclassInstance = result.ToString().Remove(0, ("?class = ").Length);
              CreateDataObjects(classRole.classMap.classId, subclassInstance, dataObjectIndex);
              break;  // should be one result only
            }
          }
        }
        else // query for property roleMaps values
        {
          foreach (RoleMap roleMap in propertyRoleMaps)
          {
            string query = String.Format(LITERAL_QUERY_TEMPLATE, possessorRoleId, classInstance,
                templateMap.templateId, referenceVariable, referenceRoleId, referenceRoleValue, referenceEndStmt, roleMap.roleId);

            Utility.WriteString(query, "./Logs/Sparql.log", true);

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
                SetObjects(dataObjectIndex, roleMap.propertyName, values);
              }
            }
          }
        }
      }
    }
    #endregion
  }
}
