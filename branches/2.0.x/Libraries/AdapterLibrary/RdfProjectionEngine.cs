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
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using System.Web;

namespace org.iringtools.adapter.projection
{
  public class RdfProjectionEngine : BasePart7ProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(RdfProjectionEngine));

    private ClassificationStyle _primaryClassificationStyle;
    private ClassificationStyle _secondaryClassificationStyle;
    private ClassificationTemplate _classificationConfig = null;
    private XElement _rdfXml = null;

    [Inject]
    public RdfProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _settings = settings;
      _dataLayer = dataLayer;
      _mapping = mapping;
      _dictionary = _dataLayer.GetDictionary();
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XDocument rdfXml = null;

      _rdfXml = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      try
      {
        _graphBaseUri = String.Format("{0}{1}/{2}/",
        _settings["GraphBaseUri"],
        HttpUtility.UrlEncode(_settings["ProjectName"]),
        HttpUtility.UrlEncode(_settings["ApplicationName"]));

        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
          SetClassIdentifiers(DataDirection.Outbound);
          rdfXml = new XDocument(BuildRdfXml());
        }
        else
        {
          rdfXml = new XDocument(_rdfXml);
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
          using (XmlReader xmlReader = xDocument.CreateReader())
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

            _dataObjects = new List<IDataObject>();
            _dataRecords = new Dictionary<string, string>[rootClassInstances.Count];

            for (int i = 0; i < _dataRecords.Length; i++)
            {
              _dataRecords[i] = new Dictionary<string, string>();
            }

            _relatedRecordsMaps = new Dictionary<string, List<Dictionary<string, string>>>[rootClassInstances.Count];
            _relatedObjectPaths = new List<string>();

            if (_memoryStore != null)
            {
              for (int i = 0; i < rootClassInstances.Count; i++)
              {
                _relatedRecordsMaps[i] = new Dictionary<string, List<Dictionary<string, string>>>();
                ProcessClassInstance(_graphMap.classTemplateListMaps.First().Key, rootClassInstances[i], i);                
                _dataObjects.Add(CreateDataObject(_graphMap.dataObjectMap, i));
              }

              // add related data objects to the top level data objects
              if (_relatedObjectPaths != null && _relatedObjectPaths.Count > 0)
              {
                FillRelatedRecords();
                AppendRelatedObjects();
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

      // get classification settings
      _primaryClassificationStyle = (ClassificationStyle)Enum.Parse(typeof(ClassificationStyle), _settings["PrimaryClassificationStyle"].ToString());
      _secondaryClassificationStyle = (ClassificationStyle)Enum.Parse(typeof(ClassificationStyle), _settings["SecondaryClassificationStyle"].ToString());

      if (File.Exists(_settings["ClassificationTemplateFile"]))
      {
        _classificationConfig = Utility.Read<ClassificationTemplate>(_settings["ClassificationTemplateFile"]);
      }

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        List<TemplateMap> templateMaps = pair.Value;

        for (int i = 0; i < _dataObjects.Count; i++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          string classIdentifier = _classIdentifiers[classMap.classId][i];

          if (!String.IsNullOrEmpty(classIdentifier))
          {
            string classPrefix = _graphBaseUri + Utility.TitleCase(classMap.name) + "/";
            string classInstance = classPrefix + classIdentifier;
            bool classInstanceExists = true;

            if (!classInstancesCache.ContainsKey(classId))
            {
              classInstancesCache[classId] = new List<string> { classInstance };
              classInstanceExists = false;
            }
            else if (!classInstancesCache[classId].Contains(classInstance))
            {
              classInstancesCache[classId].Add(classInstance);
              classInstanceExists = false;
            }

            if (!classInstanceExists)
            {
              // _primaryClassificationStyle == ClassificationStyle.Type || _primaryClassificationStyle == ClassificationStyle.Both          
              XElement classElement = new XElement(OWL_THING, new XAttribute(RDF_ABOUT, classInstance));
              classElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));
              _rdfXml.Add(classElement);

              AddRdfClassificationElement(classId, classPrefix, classIdentifier);
              
              foreach (TemplateMap templateMap in templateMaps)
              {
                if (_classificationConfig.TemplateIds.Contains(templateMap.templateId))
                {
                  if (_secondaryClassificationStyle == ClassificationStyle.Type || _secondaryClassificationStyle == ClassificationStyle.Both)
                  {
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                      if (roleMap.type == RoleType.Reference)
                      {
                        string value = GetReferenceRoleValue(roleMap);
                        classElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, value)));
                        break;
                      }
                    }
                  }

                  if (_secondaryClassificationStyle == ClassificationStyle.Template || _secondaryClassificationStyle == ClassificationStyle.Both)
                  {
                    string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);

                    XElement templateElement = new XElement(OWL_THING);
                    templateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

                    StringBuilder values = new StringBuilder(templateMap.templateId);

                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                      string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
                      XElement roleElement = new XElement(TPL_NS + roleId);

                      values.Append(roleMap.value);

                      if (roleMap.type == RoleType.Possessor)
                      {
                        roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
                      }
                      else if (roleMap.type == RoleType.Reference)
                      {
                        string value = GetReferenceRoleValue(roleMap);
                        roleElement.Add(new XAttribute(RDF_RESOURCE, value));
                      }

                      templateElement.Add(roleElement);
                    }

                    string hashCode = Utility.MD5Hash(values.ToString());
                    templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

                    _rdfXml.Add(templateElement);
                  }
                }
                else
                {
                  AddRdfTemplateElements(classPrefix, classIdentifier, templateMap, i);
                }
              }
            }
          }
        }
      }

      return _rdfXml;
    }

    private void AddRdfClassificationElement(string classId, string classPrefix, string classIdentifier)
    {
      string classInstance = classPrefix + classIdentifier;

      if (_primaryClassificationStyle == ClassificationStyle.Both)
      {
        TemplateMap classificationTemplate = _classificationConfig.TemplateMap;
        XElement templateElement = new XElement(OWL_THING);
        _rdfXml.Add(templateElement);

        templateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, TPL_NS.NamespaceName + classificationTemplate.templateId)));

        StringBuilder values = new StringBuilder(classificationTemplate.templateId);
        foreach (RoleMap roleMap in classificationTemplate.roleMaps)
        {
          string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
          XElement roleElement = new XElement(TPL_NS + roleId);
          templateElement.Add(roleElement);

          if (roleMap.type == RoleType.Possessor)
          {
            roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
            values.Append(classIdentifier);
          }
          else if (roleMap.type == RoleType.Reference)
          {
            roleElement.Add(new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId));
            values.Append(classId);
          }
        }

        string hashCode = Utility.MD5Hash(values.ToString());
        templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
      }
    }

    private string GetReferenceRoleValue(RoleMap referenceRole)
    {
      string value = referenceRole.value;

      if (!String.IsNullOrEmpty(referenceRole.valueList))
        value = _mapping.ResolveValueList(referenceRole.valueList, value);
      
      value = value.Replace(RDL_PREFIX, RDL_NS.NamespaceName);

      return value;
    }

    private void AddRdfTemplateElements(string classPrefix, string classIdentifier, TemplateMap templateMap, int dataObjectIndex)
    {
      string classInstance = classPrefix + classIdentifier;

      IDataObject dataObject = _dataObjects[dataObjectIndex];
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);

      List<RoleMap> propertyRoles = new List<RoleMap>();
      XElement baseTemplateElement = new XElement(OWL_THING);
      bool isBaseTemplateValid = true;
      StringBuilder baseValues = new StringBuilder(templateMap.templateId);
      RoleMap classRole = null;

      baseTemplateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        XElement roleElement = new XElement(TPL_NS + roleId);

        switch (roleMap.type)
        {
          case RoleType.Possessor:
            roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
            baseTemplateElement.Add(roleElement);
            baseValues.Append(classIdentifier);
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
              string value = GetReferenceRoleValue(roleMap);
              roleElement.Add(new XAttribute(RDF_RESOURCE, value));
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
        List<string> identifiers = classRole.classMap.identifiers;
        string delimiter = classRole.classMap.identifierDelimiter;
        string refClassIdentifier = GetClassIdentifierValue(identifiers, delimiter, dataObjectIndex);

        if (!String.IsNullOrEmpty(refClassIdentifier))
        {
          baseValues.Append(refClassIdentifier);

          string hashCode = Utility.MD5Hash(baseValues.ToString());
          baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

          string roleId = classRole.roleId.Substring(classRole.roleId.IndexOf(":") + 1);
          XElement roleElement = new XElement(TPL_NS + roleId);
          roleElement.Add(new XAttribute(RDF_RESOURCE, _graphBaseUri + Utility.TitleCase(classRole.classMap.name) + "/" + refClassIdentifier));
          baseTemplateElement.Add(roleElement);

          _rdfXml.Add(baseTemplateElement);
        }
        else
        {
          isBaseTemplateValid = false;
        }
      }
      else
      {
        List<List<XElement>> multiPropertyElements = new List<List<XElement>>();
        List<IDataObject> valueObjects = null;

        foreach (RoleMap propertyRole in propertyRoles)
        {
          List<XElement> propertyElements = new List<XElement>();
          multiPropertyElements.Add(propertyElements);

          valueObjects = GetValueObjects(propertyRole.propertyName, dataObjectIndex);

          foreach (IDataObject valueObject in valueObjects)
          {
            int lastDotPos = propertyRole.propertyName.LastIndexOf('.');
            string propertyName = propertyRole.propertyName.Substring(lastDotPos + 1);
            string value = Convert.ToString(valueObject.GetPropertyValue(propertyName));

            XElement propertyElement = new XElement(TPL_NS + propertyRole.roleId.Replace(TPL_PREFIX, String.Empty));
            propertyElements.Add(propertyElement);

            if (String.IsNullOrEmpty(propertyRole.valueList))
            {
              if (String.IsNullOrEmpty(value))
              {
                propertyElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
              }
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

        if (valueObjects != null && isBaseTemplateValid)
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

            string hashCode = Utility.MD5Hash(templateValue.ToString());
            templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
          }
        }
      }
    }

    private void ProcessClassInstance(ClassMap classMap, string classInstance, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> pair = _graphMap.GetClassTemplateListMap(classMap.classId);
      List<TemplateMap> templateMaps = pair.Value;

      if (templateMaps == null || templateMaps.Count == 0)
      {
        string identifierValue = classInstance.Substring(classInstance.LastIndexOf("/") + 1);

        if (pair.Key == null || String.IsNullOrEmpty(pair.Key.identifierDelimiter))
        {
          string classIdentifier = classMap.identifiers.First();
          _dataRecords[dataObjectIndex][classIdentifier.Substring(classIdentifier.LastIndexOf('.') + 1)] = identifierValue;
        }
        else
        {
          string[] identifierValueParts = identifierValue.Split(new string[] { pair.Key.identifierDelimiter }, StringSplitOptions.None);
          List<string> identifiers = classMap.identifiers;

          for (int i = 0; i < identifiers.Count; i++)
          {
            string identifierVal = identifierValueParts[i];
            _dataRecords[dataObjectIndex][identifiers[i]] = identifierVal;
          }
        }
      }
      else
      {
        FillDataRecord(classInstance, dataObjectIndex, templateMaps);
      }
    }

    private void FillDataRecord(string classInstance, int dataObjectIndex, List<TemplateMap> templateMaps)
    {
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

          object results = _memoryStore.ExecuteQuery(query);

          if (results is SparqlResultSet)
          {
            SparqlResultSet resultSet = (SparqlResultSet)results;

            foreach (SparqlResult result in resultSet)
            {
              string subclassInstance = result.Value("class").ToString();
              ProcessClassInstance(classRole.classMap, subclassInstance, dataObjectIndex);
              break;  // should be one result only
            }
          }
        }
        else // query for property mapped values
        {
          foreach (RoleMap roleMap in propertyRoleMaps)
          {
            List<string> values = new List<string>();
            string[] propertyPath = roleMap.propertyName.Split('.');
            string property = propertyPath[propertyPath.Length - 1].Trim();

            string query = String.Format(LITERAL_QUERY_TEMPLATE, possessorRoleId, classInstance, templateMap.templateId, 
              referenceVariable, referenceRoleId, referenceRoleValue, referenceEndStmt, roleMap.roleId);

            object results = _memoryStore.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
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

                if (propertyPath.Length > 2)  // related property
                {
                  values.Add(value);
                }
                else  // direct property
                {
                  _dataRecords[dataObjectIndex][property] = value;
                }
              }

              if (propertyPath.Length > 2)
              {
                SetRelatedRecords(dataObjectIndex, roleMap.propertyName, values);
              }
            }
          }
        }
      }
    }
    #endregion
  }
}
