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
    protected static readonly string QUALIFIED_RDF_NIL = RDF_NS.NamespaceName + "nil";

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

      // get classification settings
      _primaryClassificationStyle = (ClassificationStyle)Enum.Parse(typeof(ClassificationStyle),
        _settings["PrimaryClassificationStyle"].ToString());

      _secondaryClassificationStyle = (ClassificationStyle)Enum.Parse(typeof(ClassificationStyle),
        _settings["SecondaryClassificationStyle"].ToString());

      if (File.Exists(_settings["ClassificationTemplateFile"]))
      {
        _classificationConfig = Utility.Read<ClassificationTemplate>(_settings["ClassificationTemplateFile"]);
      }
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
          rdfXml = new XDocument(BuildRdfXml());
        }
        else
        {
          rdfXml = new XDocument(_rdfXml);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return rdfXml;
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument)
    {
      _dataObjects = null;

      try
      {
        if (xDocument != null)
        {
          _graphMap = _mapping.FindGraphMap(graphName);

          if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0)
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

            if (_memoryStore != null)
            {
              ClassMap rootClassMap = _graphMap.classTemplateListMaps.First().Key;
              string rootClassId = rootClassMap.classId;
              List<string> rootClassInstances = GetClassInstances(rootClassId);

              if (rootClassInstances.Count > 0)
              {
                _dataObjects = new List<IDataObject>();
                _dataRecords = new Dictionary<string, string>[rootClassInstances.Count];
                _relatedRecordsMaps = new Dictionary<string, List<Dictionary<string, string>>>[rootClassInstances.Count];
                _relatedObjectPaths = new List<string>();

                for (int i = 0; i < rootClassInstances.Count; i++)
                {
                  List<string> rootClassInstance = new List<string> {rootClassInstances[i]};

                  _dataRecords[i] = new Dictionary<string, string>();
                  _relatedRecordsMaps[i] = new Dictionary<string, List<Dictionary<string, string>>>();

                  ProcessClass(i, rootClassMap, rootClassInstance);

                  if (_primaryClassificationStyle == ClassificationStyle.Both)
                  {
                    TemplateMap classificationTemplate = _classificationConfig.TemplateMap;
                    ProcessTemplates(i, rootClassInstance, new List<TemplateMap> { classificationTemplate });
                  }

                  IDataObject dataObject = CreateDataObject(_graphMap.dataObjectMap, i);
                  _dataObjects.Add(dataObject);
                }

                // fill related data objects and append them to top level data objects
                if (_relatedObjectPaths != null && _relatedObjectPaths.Count > 0)
                {
                  ProcessRelatedItems();
                  CreateRelatedObjects();
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToDataObjects: " + ex);
        throw ex;
      }

      return _dataObjects;
    }

    #region outbound helper methods
    private XElement BuildRdfXml()
    {
      Dictionary<string, List<string>> classInstancesCache = new Dictionary<string, List<string>>();

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        List<TemplateMap> templateMaps = pair.Value;

        for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          bool hasRelatedProperty;
          List<string> classIdentifiers = GetClassIdentifiers(classMap, dataObjectIndex, out hasRelatedProperty);

          for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
          {
            string classIdentifier = classIdentifiers[classIdentifierIndex];

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
                // add individual          
                XElement classElement = new XElement(OWL_THING, new XAttribute(RDF_ABOUT, classInstance));
                classElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));
                _rdfXml.Add(classElement);

                // add primary classification template
                if (_primaryClassificationStyle == ClassificationStyle.Both)
                {
                  TemplateMap classificationTemplate = _classificationConfig.TemplateMap;
                  AddTemplateElements(classPrefix, classIdentifier, classIdentifierIndex, classificationTemplate,
                        dataObjectIndex, hasRelatedProperty);
                }

                foreach (TemplateMap templateMap in templateMaps)
                {
                  if ((_secondaryClassificationStyle == ClassificationStyle.Type ||
                       _secondaryClassificationStyle == ClassificationStyle.Both) && 
                      _classificationConfig.TemplateIds.Contains(templateMap.templateId))
                  {                   
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                      if (roleMap.type == RoleType.Reference)
                      {
                        string value = GetReferenceRoleValue(roleMap);
                        classElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, value)));
                      }
                    }

                    continue;
                  }

                  AddTemplateElements(classPrefix, classIdentifier, classIdentifierIndex, templateMap,
                      dataObjectIndex, hasRelatedProperty);
                }
              }
            }
          }
        }
      }

      return _rdfXml;
    }

    private void AddTemplateElements(string classPrefix, string classIdentifier, int classIdentifierIndex,
      TemplateMap templateMap, int dataObjectIndex, bool classIdentifierHasRelatedProperty)
    {
      string classInstance = classPrefix + classIdentifier;
      IDataObject dataObject = _dataObjects[dataObjectIndex];
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);
      List<RoleMap> propertyRoles = new List<RoleMap>();
      XElement baseTemplateElement = new XElement(OWL_THING);
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
            {
              classRole = roleMap;
            }
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

      if (propertyRoles.Count > 0)  // property template
      {
        List<List<XElement>> matrixPropertyElements = new List<List<XElement>>();

        // create property elements
        foreach (RoleMap propertyRole in propertyRoles)
        {
          List<XElement> propertyElements = new List<XElement>();
          matrixPropertyElements.Add(propertyElements);

          string[] propertyParts = propertyRole.propertyName.Split('.');
          string propertyName = propertyParts[propertyParts.Length - 1];

          int lastDotPos = propertyRole.propertyName.LastIndexOf('.');
          string objectPath = propertyRole.propertyName.Substring(0, lastDotPos);

          if (propertyParts.Length == 2)  // direct property
          {
            string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
            propertyElements.Add(CreatePropertyElement(propertyRole, propertyValue));
          }
          else  // related property
          {
            string key = objectPath + "." + dataObjectIndex;
            List<IDataObject> relatedObjects = null;

            if (!_relatedObjectsCache.TryGetValue(key, out relatedObjects))
            {
              relatedObjects = GetRelatedObjects(propertyRole.propertyName, _dataObjects[dataObjectIndex]);
              _relatedObjectsCache.Add(key, relatedObjects);
            }

            if (classIdentifierHasRelatedProperty)  // reference class identifier has related property
            {
              IDataObject relatedObject = relatedObjects[classIdentifierIndex];
              string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
              propertyElements.Add(CreatePropertyElement(propertyRole, propertyValue));
            }
            else  // related property is property map
            {
              foreach (IDataObject relatedObject in relatedObjects)
              {
                string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                propertyElements.Add(CreatePropertyElement(propertyRole, propertyValue));
              }
            }
          }
        }

        // add property elements to template element(s)
        if (matrixPropertyElements.Count > 0 && matrixPropertyElements[0].Count > 0)
        {
          // used to enforce dotNetRDF to store/retrieve template triples in order as the RDF
          string hashPrefixFormat = Regex.Replace(matrixPropertyElements[0].Count.ToString(), "\\d", "0") + "0";

          for (int i = 0; i < matrixPropertyElements[0].Count; i++)
          {
            XElement templateElement = new XElement(baseTemplateElement);
            _rdfXml.Add(templateElement);

            StringBuilder templateValue = new StringBuilder(baseValues.ToString());
            for (int j = 0; j < matrixPropertyElements.Count; j++)
            {
              XElement propertyElement = matrixPropertyElements[j][i];
              templateElement.Add(propertyElement);

              if (!String.IsNullOrEmpty(propertyElement.Value))
                templateValue.Append(propertyElement.Value);
              else
                templateValue.Append(propertyElement.Attribute(RDF_RESOURCE).Value);
            }

            string hashCode = Utility.MD5Hash(templateValue.ToString());
            hashCode = i.ToString(hashPrefixFormat) + hashCode.Substring(hashPrefixFormat.Length);
            templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
          }
        }
      }
      else if (classRole != null)  // reference template with known class role
      {
        bool refClassHasRelatedProperty;
        List<string> refClassIdentifiers = GetClassIdentifiers(classRole.classMap, dataObjectIndex, 
          out refClassHasRelatedProperty);

        if (refClassHasRelatedProperty)
        {
          string refClassBaseValues = baseValues.ToString();
          string roleId = classRole.roleId.Substring(classRole.roleId.IndexOf(":") + 1);
          string baseRelatedClassUri = _graphBaseUri + Utility.TitleCase(classRole.classMap.name) + "/";

          foreach (string refClassIdentifier in refClassIdentifiers)
          {
            if (!String.IsNullOrEmpty(refClassIdentifier))
            {
              XElement refBaseTemplateElement = new XElement(baseTemplateElement);

              string hashCode = Utility.MD5Hash(refClassBaseValues + refClassIdentifier);
              refBaseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

              XElement roleElement = new XElement(TPL_NS + roleId);
              roleElement.Add(new XAttribute(RDF_RESOURCE, baseRelatedClassUri + refClassIdentifier));
              refBaseTemplateElement.Add(roleElement);

              _rdfXml.Add(refBaseTemplateElement);
            }
          }
        }
        else
        {
          string refClassIdentifier = refClassIdentifiers.First();

          if (!String.IsNullOrEmpty(refClassIdentifier))
          {
            baseValues.Append(refClassIdentifier);

            string hashCode = Utility.MD5Hash(baseValues.ToString());
            baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

            string roleId = classRole.roleId.Substring(classRole.roleId.IndexOf(":") + 1);
            XElement roleElement = new XElement(TPL_NS + roleId);
            roleElement.Add(new XAttribute(RDF_RESOURCE, _graphBaseUri +
              Utility.TitleCase(classRole.classMap.name) + "/" + refClassIdentifier));
            baseTemplateElement.Add(roleElement);

            _rdfXml.Add(baseTemplateElement);
          }
        }
      }
      else  // reference template with no class role (primary classification template)
      {
        string hashCode = Utility.MD5Hash(baseValues.ToString());
        baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
        _rdfXml.Add(baseTemplateElement);
      }
    }

    private XElement CreatePropertyElement(RoleMap propertyRole, string propertyValue)
    {
      XElement propertyElement = new XElement(TPL_NS + propertyRole.roleId.Replace(TPL_PREFIX, String.Empty));

      if (String.IsNullOrEmpty(propertyRole.valueList))
      {
        if (String.IsNullOrEmpty(propertyValue))
        {
          propertyElement.Add(new XAttribute(RDF_RESOURCE, QUALIFIED_RDF_NIL));
        }
        else
        {
          propertyElement.Add(new XAttribute(RDF_DATATYPE, 
            propertyRole.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName)));

          if (propertyRole.dataType.Contains("dateTime"))
            propertyValue = Utility.ToXsdDateTime(propertyValue);

          propertyElement.Add(new XText(propertyValue));
        }
      }
      else  // resolve value list to uri
      {
        propertyValue = _mapping.ResolveValueList(propertyRole.valueList, propertyValue);

        if (String.IsNullOrEmpty(propertyValue))
          propertyValue = QUALIFIED_RDF_NIL;
        else
          propertyValue = propertyValue.Replace(RDL_PREFIX, RDL_NS.NamespaceName);

        propertyElement.Add(new XAttribute(RDF_RESOURCE, propertyValue));
      }

      return propertyElement;
    }

    private string GetReferenceRoleValue(RoleMap referenceRole)
    {
      string value = referenceRole.value;

      if (!String.IsNullOrEmpty(referenceRole.valueList))
        value = _mapping.ResolveValueList(referenceRole.valueList, value);

      return value.Replace(RDL_PREFIX, RDL_NS.NamespaceName);
    }
    #endregion

    #region inbound helper methods
    private List<string> GetClassInstances(string classId)
    {
      List<string> classInstances = new List<string>();
      string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classId);
      object results = _memoryStore.ExecuteQuery(query);

      if (results != null)
      {
        SparqlResultSet resultSet = (SparqlResultSet)results;

        foreach (SparqlResult result in resultSet)
        {
          string classInstance = result.Value("class").ToString();

          if (!String.IsNullOrEmpty(classInstance))
          {
            classInstances.Add(classInstance);
          }
          else
          {
            _logger.Debug(query);
            throw new Exception("Individual of class [" + classId + "] not found!");
          }
        }
      }

      return classInstances;
    }

    private void ProcessClass(int dataObjectIndex, ClassMap classMap, List<string> classInstances)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> pair = _graphMap.GetClassTemplateListMap(classMap.classId);
      List<TemplateMap> templateMaps = pair.Value;

      for (int classInstanceIndex = 0; classInstanceIndex < classInstances.Count; classInstanceIndex++)
      {
        string classInstance = classInstances[classInstanceIndex];
        string identifier = classInstance.Substring(classInstance.LastIndexOf("/") + 1);

        string[] identifierParts = !String.IsNullOrEmpty(classMap.identifierDelimiter)
          ? identifier.Split(new string[] { classMap.identifierDelimiter }, StringSplitOptions.None)
          : new string[] { identifier };

        for (int i = 0; i < identifierParts.Length; i++)
        {
          string identifierPart = identifierParts[i];

          // remove fixed values from identifier
          foreach (string clsIdentifier in classMap.identifiers)
          {
            if (clsIdentifier.StartsWith("#") && clsIdentifier.EndsWith("#"))
            {
              identifierPart = identifierPart.Replace(clsIdentifier.Substring(1, clsIdentifier.Length - 2), "");
            }
          }

          // set identifier value to mapped property
          foreach (string clsIdentifier in classMap.identifiers)
          {
            if (clsIdentifier.Split('.').Length > 2)  // related property
            {
              SetRelatedRecords(dataObjectIndex, classInstanceIndex, clsIdentifier, new List<string> { identifierPart });
            }
            else  // direct property
            {
              _dataRecords[dataObjectIndex][clsIdentifier.Substring(clsIdentifier.LastIndexOf('.') + 1)] = identifierPart;
            }
          }          
        }
      }

      if (templateMaps != null && templateMaps.Count > 0)
      {
        ProcessTemplates(dataObjectIndex, classInstances, templateMaps);
      }
    }

    private void ProcessTemplates(int dataObjectIndex, List<string> classInstances, List<TemplateMap> templateMaps)
    {
      foreach (TemplateMap templateMap in templateMaps)
      {
        string possessorRoleId = String.Empty;
        RoleMap referenceRole = null;
        RoleMap classRole = null;
        List<RoleMap> propertyRoles = new List<RoleMap>();

        // find property roles
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
              propertyRoles.Add(roleMap);
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

        for (int classInstanceIndex = 0; classInstanceIndex < classInstances.Count; classInstanceIndex++)
        {
          if (classRole != null)
          {
            string query = String.Format(SUBCLASS_INSTANCE_QUERY_TEMPLATE, possessorRoleId, 
              classInstances[classInstanceIndex], templateMap.templateId, referenceVariable, referenceRoleId, 
              referenceRoleValue, referenceEndStmt, classRole.roleId);

            object results = _memoryStore.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
              SparqlResultSet resultSet = (SparqlResultSet)results;
              List<string> subclassInstances = new List<string>();

              foreach (SparqlResult result in resultSet)
              {
                string subclassInstance = result.Value("class").ToString();
                subclassInstances.Add(subclassInstance);
              }

              ProcessClass(dataObjectIndex, classRole.classMap, subclassInstances);
            }
          }
          else // query for property values
          {
            foreach (RoleMap propertyRole in propertyRoles)
            {
              List<string> values = new List<string>();
              string[] propertyPath = propertyRole.propertyName.Split('.');
              string property = propertyPath[propertyPath.Length - 1];

              string query = String.Format(LITERAL_QUERY_TEMPLATE, possessorRoleId, classInstances[classInstanceIndex], 
                templateMap.templateId, referenceVariable, referenceRoleId, referenceRoleValue, referenceEndStmt, 
                propertyRole.roleId);

              object results = _memoryStore.ExecuteQuery(query);

              if (results is SparqlResultSet)
              {
                SparqlResultSet resultSet = (SparqlResultSet)results;

                foreach (SparqlResult result in resultSet)
                {
                  string value = Regex.Replace(result.ToString(), @".*= ", String.Empty);

                  if (value == QUALIFIED_RDF_NIL)
                    value = String.Empty;
                  else if (value.Contains("^^"))
                    value = value.Substring(0, value.IndexOf("^^"));
                  else if (!String.IsNullOrEmpty(propertyRole.valueList))
                    value = _mapping.ResolveValueMap(propertyRole.valueList, value);

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
                  SetRelatedRecords(dataObjectIndex, classInstanceIndex, propertyRole.propertyName, values);
                }
              }
            }
          }
        }
      }
    }
    #endregion
  }
}
