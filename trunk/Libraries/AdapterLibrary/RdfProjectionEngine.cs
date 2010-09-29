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
  //TODO: use entity for graph base uri
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

    public override XElement ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement rdfXml = null;

      try
      {
        _graphBaseUri = String.Format("{0}{1}/{2}/{3}/",
          HttpUtility.UrlEncode(_settings["GraphBaseUri"]),
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
          rdfXml = BuildRdfXml();
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return rdfXml;
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XElement xml)
    {
      _dataObjects = null;

      try
      {
        _graphBaseUri = _settings["TargetGraphBaseUri"];
        if (!_graphBaseUri.EndsWith("/")) _graphBaseUri += "/";

        _graphMap = _mapping.FindGraphMap(graphName);

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 && xml != null)
        {
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
            propertyRoles.Add(roleMap);
            break;
        }
      }

      if (classRole != null)
      {
        string identifier = _classIdentifiers[classRole.classMap.classId][dataObjectIndex];
        baseValues.Append(identifier);

        string hashCode = Utility.md5Hash(templateId + baseValues.ToString());
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
                propertyElement.Add(new XText(value));
              }
            }
            else // resolve value list to uri
            {
              value = _mapping.ResolveValueList(propertyRole.valueList, value);

              if (value == null)
                value = RDF_NIL;

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

            string hashCode = Utility.md5Hash(templateId + templateValue.ToString());
            templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
          }
        }
      }
    }
    #endregion
  }
}
