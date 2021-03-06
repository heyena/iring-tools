﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using org.iringtools.library;
using VDS.RDF.Query;
using VDS.RDF;
using org.iringtools.utility;
using org.iringtools.mapping;
using log4net;
using System.IO;
using System.Web;
using org.iringtools.library.tip;

namespace org.iringtools.adapter.projection
{
  public abstract class BasePart7ProjectionEngine : IProjectionLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BasePart7ProjectionEngine));

    protected static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    protected static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    protected static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    protected static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    protected static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    protected static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    protected static readonly XName OWL_THING = OWL_NS + "Thing";
    protected static readonly XName RDF_ABOUT = RDF_NS + "about";
    protected static readonly XName RDF_DESCRIPTION = RDF_NS + "Description";
    protected static readonly XName RDF_TYPE = RDF_NS + "type";
    protected static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    protected static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    protected static readonly string XSD_PREFIX = "xsd:";
    protected static readonly string RDF_PREFIX = "rdf:";
    protected static readonly string RDL_PREFIX = "rdl:";
    protected static readonly string TPL_PREFIX = "tpl:";
    protected static readonly string RDF_NIL = RDF_PREFIX + "nil";
    protected static readonly string BLANK_NODE = "?bnode";
    protected static readonly string END_STATEMENT = ".";

    protected static readonly string CLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      SELECT ?class
      WHERE {{{{ 
        ?class rdf:type {{0}} . 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName);

    protected static readonly string SUBCLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}>
      SELECT ?class 
      WHERE {{{{
	      {3} {{0}} <{{1}}> . 
	      {3} rdf:type {{2}} . 
	      {{3}} {{4}} {{5}} {{6}}
	      {3} {{7}} ?class 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName, BLANK_NODE);

    protected static readonly string LITERAL_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}>
      SELECT ?literals 
      WHERE {{{{
	      {3} {{0}} <{{1}}> . 
	      {3} rdf:type {{2}} . 
	      {{3}} {{4}} {{5}} {{6}}
	      {3} {{7}} ?literals 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName, BLANK_NODE);

    protected ClassificationStyle _primaryClassificationStyle;
    protected ClassificationStyle _secondaryClassificationStyle;
    protected ClassificationTemplate _classificationConfig;

    protected AdapterSettings _settings = null;
    protected DataDictionary _dictionary = null;
    protected Mapping _mapping = null;
    protected GraphMap _graphMap = null;
    protected List<IDataObject> _dataObjects = null;
    protected Dictionary<string, string>[] _dataRecords = null;
    protected List<string> _relatedObjectPaths = null;
    protected string _fixedIdentifierBoundary = "#";
    protected Properties _uriMaps;

    protected TipMapping _tipMapping = null;
    protected TipMap _tipMap = null;

    // key is related object type at a data object index and value is list of related objects 
    protected Dictionary<string, IList<IDataObject>>[] _relatedObjects = null;

    // key is related object type and value is a list of related data records
    protected Dictionary<string, List<Dictionary<string, string>>>[] _relatedRecordsMaps = null;

    // key is related object path and value is list of related objects
    protected Dictionary<string, List<IDataObject>> _relatedObjectsCache = null;

    protected TripleStore _memoryStore = null;
    private string _valueListName = null;

    public bool FullIndex { get; set; }
    public long Count { get; set; }
    public int Start { get; set; }
    public int Limit { get; set; }
    public string BaseURI { get; set; }
    public DataLayerGateway dataLayerGateway { get; set; }

    public BasePart7ProjectionEngine(AdapterSettings settings, DataDictionary dictionary, TipMapping tipMapping)
        : this(settings, dictionary, (Mapping)null)
    {
        _tipMapping = tipMapping;
    }

    public BasePart7ProjectionEngine(AdapterSettings settings, DataDictionary dictionary, Mapping mapping)
    {
      _dataObjects = new List<IDataObject>();
      _relatedObjectsCache = new Dictionary<string, List<IDataObject>>();

      _settings = settings;
      _dictionary = dictionary;
      _mapping = mapping;

      if (!String.IsNullOrEmpty(_settings["fixedIdentifierBoundary"]))
      {
        _fixedIdentifierBoundary = _settings["fixedIdentifierBoundary"];
      }

      // get classification settings
      _primaryClassificationStyle = (ClassificationStyle)Enum.Parse(typeof(ClassificationStyle),
        _settings["PrimaryClassificationStyle"].ToString());

      _secondaryClassificationStyle = (ClassificationStyle)Enum.Parse(typeof(ClassificationStyle),
        _settings["SecondaryClassificationStyle"].ToString());

      string classificationTemplateFile = _settings["ClassificationTemplateFile"];

      if (File.Exists(classificationTemplateFile))
      {
        _classificationConfig = Utility.Read<ClassificationTemplate>(classificationTemplateFile);
      }

      // load uri maps config
      _uriMaps = new Properties();

      string uriMapsFilePath = _settings["AppDataPath"] + "UriMaps.conf";

      if (File.Exists(uriMapsFilePath))
      {
        try
        {
          _uriMaps.Load(uriMapsFilePath);
        }
        catch (Exception e)
        {
          _logger.Info("Error loading [UriMaps.config]: " + e);
        }
      }
    }

    public abstract XDocument ToXml(string graphName, ref List<IDataObject> dataObjects);
    public abstract XDocument ToXml(string graphName, ref List<IDataObject> dataObjects, string className, string classIdentifier);
    public abstract List<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument);

    //propertyPath = "Instrument.LineItems.Tag";
    protected List<IDataObject> GetRelatedObjects(string propertyPath, IDataObject dataObject)
    {
      List<IDataObject> dataObjects = new List<IDataObject>();
      string[] objectPath = propertyPath.Split('.');

      dataObjects.Add(dataObject);

      for (int i = 0; i < objectPath.Length - 2; i++)
      {
        foreach (IDataObject parentObject in dataObjects)
        {
          string objectType = parentObject.GetType().Name;

          if (objectType == typeof(GenericDataObject).Name)
          {
            objectType = ((GenericDataObject)parentObject).ObjectType;
          }

          DataObject parentObjectType = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
          DataRelationship dataRelationship = parentObjectType.dataRelationships.First(c => c.relationshipName.ToLower() == objectPath[i + 1].ToLower());
          DataObject relatedObjectType = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == dataRelationship.relatedObjectName.ToLower());

          DataFilter filter = new DataFilter();

          foreach (PropertyMap propMap in dataRelationship.propertyMaps)
          {
            filter.Expressions.Add(new Expression()
            {
              PropertyName = propMap.relatedPropertyName,
              RelationalOperator = RelationalOperator.EqualTo,
              LogicalOperator = LogicalOperator.And,
              Values = new Values() { Convert.ToString(parentObject.GetPropertyValue(propMap.dataPropertyName)) }
            });
          }

          List<IDataObject> relatedObjects = dataLayerGateway.Get(relatedObjectType, filter, 0, 0);
          dataObjects = relatedObjects;
        }
      }

      return dataObjects;
    }

    // senario (assume no circular relationships - should be handled by AppEditor): 
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property1
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property2
    protected void SetRelatedRecords(int dataObjectIndex, int classInstanceIndex, string relatedPropertyPath, List<string> relatedValues)
    {
      Dictionary<string, List<Dictionary<string, string>>> relatedRecordsMap = _relatedRecordsMaps[dataObjectIndex];
      int lastDotPosition = relatedPropertyPath.LastIndexOf('.');
      string property = relatedPropertyPath.Substring(lastDotPosition + 1);
      string objectPath = relatedPropertyPath.Substring(0, lastDotPosition);  // exclude property
      string[] objectNames = objectPath.Split('.');

      if (!_relatedObjectPaths.Contains(objectPath))
        _relatedObjectPaths.Add(objectPath);

      // top level data objects are processed separately, so start with 1
      for (int i = 1; i < objectNames.Length; i++)
      {
        string relatedObjectType = objectNames[i];
        List<Dictionary<string, string>> relatedRecords = null;

        if (relatedRecordsMap.ContainsKey(relatedObjectType))
        {
          relatedRecords = relatedRecordsMap[relatedObjectType];
        }
        else
        {
          if (i == objectNames.Length - 1)  // last related object in the chain
          {
            relatedRecords = new List<Dictionary<string, string>>();

            for (int j = 0; j < relatedValues.Count; j++)
            {
              relatedRecords.Add(new Dictionary<string, string>());
            }
          }
          else  // intermediate related object
          {
            relatedRecords = new List<Dictionary<string, string>>();
          }

          relatedRecordsMap[relatedObjectType] = relatedRecords;
        }

        // fill last related object values
        if (i == objectNames.Length - 1)
        {
          if (relatedValues.Count > 1)
          {
            for (int j = 0; j < relatedValues.Count; j++)
            {
              relatedRecords[j][property] = relatedValues[j];
            }
          }
          else
          {
            if (relatedRecords.Count <= classInstanceIndex)
            {
              relatedRecords.Add(new Dictionary<string, string>());
            }

            relatedRecords[classInstanceIndex][property] = relatedValues.First();
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
    protected void ProcessRelatedItems()
    {
      for (int i = 0; i < _dataObjects.Count; i++)
      {
        Dictionary<string, List<Dictionary<string, string>>> relatedObjectsMap = _relatedRecordsMaps[i];

        foreach (string relatedObjectPath in _relatedObjectPaths)
        {
          string[] relatedObjectPathElements = relatedObjectPath.Split('.');

          for (int j = 0; j < relatedObjectPathElements.Length - 1; j++)
          {
            string parentObjectType = relatedObjectPathElements[j];
            string relatedObjectType = relatedObjectPathElements[j + 1];

            if (relatedObjectsMap.ContainsKey(relatedObjectType))
            {
              List<Dictionary<string, string>> relatedRecords = relatedObjectsMap[relatedObjectType];

              if (j == 0)
              {
                List<IDataObject> parentObjects = new List<IDataObject> { _dataObjects[i] };

                foreach (IDataObject parentObject in parentObjects)
                {
                  DataObject dataObject = _dictionary.dataObjects.First(c => c.objectName == parentObjectType);
                  DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relatedObjectName == relatedObjectType);

                  foreach (Dictionary<string, string> relatedRecord in relatedRecords)
                  {
                    foreach (PropertyMap map in dataRelationship.propertyMaps)
                    {
                      relatedRecord[map.relatedPropertyName] = parentObject.GetPropertyValue(map.dataPropertyName).ToString();
                    }
                  }
                }
              }
              else
              {
                List<Dictionary<string, string>> parentObjects = relatedObjectsMap[parentObjectType];

                foreach (Dictionary<string, string> parentObject in parentObjects)
                {
                  DataObject dataObject = _dictionary.dataObjects.First(c => c.objectName == parentObjectType);
                  DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relatedObjectName == relatedObjectType);

                  foreach (Dictionary<string, string> relatedRecord in relatedRecords)
                  {
                    foreach (PropertyMap map in dataRelationship.propertyMaps)
                    {
                      relatedRecord[map.relatedPropertyName] = parentObject[map.dataPropertyName];
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    // turn related records into data objects, remove duplicates, and append them to top level data objects
    protected void CreateRelatedObjects()
    {
      // dictonary cache of related object types and list of identifiers
      Dictionary<string, List<string>> relatedObjectTypeIdentifiers = new Dictionary<string, List<string>>();

      foreach (Dictionary<string, List<Dictionary<string, string>>> relatedRecordsMap in _relatedRecordsMaps)
      {
        foreach (var relatedRecordsMapPair in relatedRecordsMap)
        {
          string relatedObjectType = relatedRecordsMapPair.Key;
          List<KeyProperty> keyProperties = GetKeyProperties(relatedObjectType);
          string keyDelimeter = GetKeyDelimeter(relatedObjectType);

          foreach (Dictionary<string, string> relatedRecord in relatedRecordsMapPair.Value)
          {
            bool firstSegment = true;
            string relatedObjectIdentifier = String.Empty;
            foreach (KeyProperty keyProperty in keyProperties)
            {
              if (firstSegment)
              {
                firstSegment = false;
              }
              else  // add the configured delimeter prior to concatenating subsequent segments of a composite key
              {
                relatedObjectIdentifier += keyDelimeter;
              }
              relatedObjectIdentifier += relatedRecord[keyProperty.keyPropertyName];
            }

            if (!relatedObjectTypeIdentifiers.ContainsKey(relatedObjectType))
            {
              List<string> relatedObjectIdentifiers = new List<string> { relatedObjectIdentifier };

              relatedObjectTypeIdentifiers.Add(relatedObjectType, relatedObjectIdentifiers);
              AddRelatedObject(relatedObjectType, relatedObjectIdentifier, relatedRecord);
            }
            else if (!relatedObjectTypeIdentifiers[relatedObjectType].Contains(relatedObjectIdentifier))
            {
              relatedObjectTypeIdentifiers[relatedObjectType].Add(relatedObjectIdentifier);
              AddRelatedObject(relatedObjectType, relatedObjectIdentifier, relatedRecord);
            }
          }
        }
      }
    }

    protected void AddRelatedObject(string relatedObjectType, string relatedObjectIdentifier, Dictionary<string, string> relatedRecord)
    {
      //TODO: set state
      IDataObject relatedObject = new SerializableDataObject()
      {
        Type = relatedObjectType,
        Id = relatedObjectIdentifier
      };

      if (relatedObject.GetType() == typeof(GenericDataObject))
      {
        ((GenericDataObject)relatedObject).ObjectType = relatedObjectType;
      }

      foreach (var relatedRecordPair in relatedRecord)
      {
        relatedObject.SetPropertyValue(relatedRecordPair.Key, relatedRecordPair.Value);
      }

      _dataObjects.Add(relatedObject);
    }

    protected void ProcessInboundClassIdentifiers(int dataObjectIndex, ClassMap classMap, int classObjectIndex, string identifierValue)
    {
      if (classMap.identifiers.Count == 1)
      {
        if (classMap.identifiers[0].Split('.').Length > 2)  // related property
        {
          SetRelatedRecords(dataObjectIndex, classObjectIndex, classMap.identifiers[0], new List<string> { identifierValue });
        }
        else  // direct property
        {
          _dataRecords[dataObjectIndex][classMap.identifiers[0].Substring(classMap.identifiers[0].LastIndexOf('.') + 1)] = identifierValue;
        }
      }
      else if (classMap.identifiers.Count > 1)
      {
        string[] identifierValueParts = !String.IsNullOrEmpty(classMap.identifierDelimiter)
            ? identifierValue.Split(new string[] { classMap.identifierDelimiter }, StringSplitOptions.None)
            : new string[] { identifierValue };

        for (int identifierPartIndex = 0; identifierPartIndex < identifierValueParts.Length; identifierPartIndex++)
        {
          string identifierPartName = classMap.identifiers[identifierPartIndex];
          string identifierPartValue = identifierValueParts[identifierPartIndex];

          if (identifierPartName.StartsWith(_fixedIdentifierBoundary) && identifierPartName.EndsWith(_fixedIdentifierBoundary))
            continue;

          if (identifierPartName.Split('.').Length > 2)  // related property
          {
            SetRelatedRecords(dataObjectIndex, classObjectIndex, identifierPartName, new List<string> { identifierPartValue });
          }
          else  // direct property
          {
            _dataRecords[dataObjectIndex][identifierPartName.Substring(identifierPartName.LastIndexOf('.') + 1)] = identifierPartValue;
          }
        }
      }
    }

    protected List<KeyProperty> GetKeyProperties(string objectType)
    {
      DataObject dataObject = _dictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());
      return dataObject.keyProperties;
    }

    protected string GetKeyDelimeter(string objectType)
    {
      DataObject dataObject = _dictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());
      return dataObject.keyDelimeter;
    }

    protected bool IsFixedIdentifier(string identifier)
    {
      return identifier.StartsWith(_fixedIdentifierBoundary) && identifier.EndsWith(_fixedIdentifierBoundary);
    }

    protected bool ContainsAssignedKey(DataObject dictionaryObject)
    {
      foreach (KeyProperty keyProperty in dictionaryObject.keyProperties)
      {
        foreach (DataProperty dataProperty in dictionaryObject.dataProperties)
        {
          if (dataProperty.propertyName.ToLower() == keyProperty.keyPropertyName.ToLower())
          {
            if (dataProperty.keyType == KeyType.assigned)
            {
              return true;
            }

            break;
          }
        }
      }

      return false;
    }

    private void SetPropertyValue(DataObject objDef, KeyValuePair<String, String> pair, IDataObject dataObject)
    {
      DataProperty objProp = objDef.dataProperties.Find(p => p.propertyName.ToLower() == pair.Key.ToLower());

      if (objProp == null)
      {
        _logger.Error("Object property [" + pair.Key + "] not found.");
      }

      try
      {
        if (pair.Value == null)
        {
          if (objProp.dataType == DataType.String || objProp.isNullable)
          {
            dataObject.SetPropertyValue(objProp.propertyName, null);
          }
          else
          {
            Type t = Type.GetType("System." + objProp.dataType.ToString());
            dataObject.SetPropertyValue(objProp.propertyName, Activator.CreateInstance(t));
          }
        }
        else if (objProp.dataType == DataType.String)
        {
          if (objProp.dataLength > 0 && objProp.dataLength < pair.Value.ToString().Length)
          {
            string value = pair.Value.Substring(0, objProp.dataLength);
            dataObject.SetPropertyValue(objProp.propertyName, value);
          }
          else
          {
            dataObject.SetPropertyValue(objProp.propertyName, pair.Value);
          }
        }
        else
        {
          dataObject.SetPropertyValue(objProp.propertyName, pair.Value);
        }
      }
      catch (Exception e)
      {
        string error = "Error setting value for property [" + objProp.propertyName + "]. " + e;
        _logger.Error(error);
      }
    }

    protected void SetAppCode(IDataObject dataObject)
    {
      string senderContext = _settings["SenderProjectName"];
      string senderApp = _settings["SenderApplicationName"];
      string appCodeProperty = _settings["AppCodeProperty"];
      string includeAppCodeContext = _settings["IncludeAppCodeContext"];

      if (appCodeProperty != null && senderApp != null)
      {
        if (includeAppCodeContext != null && includeAppCodeContext.ToLower() == "true" && senderContext != null)
        {
          dataObject.SetPropertyValue(appCodeProperty, senderContext + "." + senderApp);
        }
        else
        {
          dataObject.SetPropertyValue(appCodeProperty, senderApp);
        }
      }
    }

    // create data object from data record
    protected IDataObject CreateDataObject(string objectType, int objectIndex)
    {
      Dictionary<string, string> dataRecord = _dataRecords[objectIndex];
      DataObject objDef = _dictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());
      List<KeyProperty> keyProperties = objDef.keyProperties;
      string keyDelimeter = objDef.keyDelimeter;
      string identifier = String.Empty;
      bool firstSegment = true;

      foreach (KeyProperty keyProperty in keyProperties)
      {
        if (dataRecord.ContainsKey(keyProperty.keyPropertyName))
        {
          if (firstSegment)
          {
            firstSegment = false;
          }
          else  // add the configured delimeter prior to concatenating subsequent segments of a composite key
          {
            identifier += keyDelimeter;
          }

          identifier += dataRecord[keyProperty.keyPropertyName];
        }
      }

      SerializableDataObject dataObject = new SerializableDataObject()
      {
        Type = objDef.objectName
      };

      if (!string.IsNullOrEmpty(identifier))
      {
        dataObject.Id = identifier;
      }

      SetAppCode(dataObject);

      foreach (var pair in dataRecord)
      {
        SetPropertyValue(objDef, pair, dataObject);
      }

      return dataObject;
    }

    protected List<IDataObject> GetValueObjects(string propertyMap, int dataObjectIndex)
    {
      List<IDataObject> valueObjects = null;

      int lastDotPos = propertyMap.LastIndexOf('.');
      string propertyName = propertyMap.Substring(lastDotPos + 1);
      string objectPath = propertyMap.Substring(0, lastDotPos);

      if (propertyMap.Split('.').Length > 2)  // related property
      {
        string key = objectPath + "." + dataObjectIndex;

        if (!_relatedObjectsCache.TryGetValue(key, out valueObjects))
        {
          valueObjects = GetRelatedObjects(propertyMap, _dataObjects[dataObjectIndex]);
          _relatedObjectsCache.Add(key, valueObjects);
        }
      }
      else  // direct property
      {
        valueObjects = new List<IDataObject> { _dataObjects[dataObjectIndex] };
      }

      return valueObjects;
    }

    protected List<string> GetClassIdentifiers(ClassMap classMap, int dataObjectIndex, out bool hasRelatedProperty)
    {
      string[] classIdentifiers = null;
      hasRelatedProperty = false;
      bool initialized = false;

      foreach (string identifier in classMap.identifiers)
      {
        // identifier is a property map
        if (!IsFixedIdentifier(identifier))
        {
          string[] identifierParts = identifier.Split('.');
          string propertyName = identifierParts[identifierParts.Length - 1];

          if (identifierParts.Length > 2)  // related property
          {
            List<IDataObject> valueObjects = GetValueObjects(identifier, dataObjectIndex);

            if (!initialized)
            {
              if (classIdentifiers == null)
              {
                classIdentifiers = new string[valueObjects.Count];                
              }
              else
              {
                string value = classIdentifiers[0];
                classIdentifiers = new string[valueObjects.Count];

                for (int i = 0; i < classIdentifiers.Length; i++)
                {
                  classIdentifiers[i] = value;
                }
              }

              initialized = true;
            }

            for (int i = 0; i < valueObjects.Count; i++)
            {
              string value = Convert.ToString(valueObjects[i].GetPropertyValue(propertyName));

              if (classIdentifiers[i] == null)
              {
                classIdentifiers[i] = value;
              }
              else
              {
                classIdentifiers[i] += classMap.identifierDelimiter + value;
              }
            }

            hasRelatedProperty = true;
          }
          else  // direct property
          {
            string value = Convert.ToString(_dataObjects[dataObjectIndex].GetPropertyValue(propertyName));
            
            if (classIdentifiers == null)
            {
              classIdentifiers = new string[1];
              classIdentifiers[0] = value;
            }
            else
            {
              classIdentifiers[0] += classMap.identifierDelimiter + value;
            }
          }
        }
        else  // identifier is a fixed value
        {
          string value = identifier.Substring(1, identifier.Length - 2);

          if (classIdentifiers == null)
          {
            classIdentifiers = new string[1];
            classIdentifiers[0] = value;
          }
          else
          {
            for (int i = 0; i < classIdentifiers.Length; i++)
            {
              classIdentifiers[i] += classMap.identifierDelimiter + value;
            }
          }
        }
      }

      if (classIdentifiers == null)
        return new List<string>();

      return classIdentifiers.ToList<string>();
    }

    public void ProjectDataFilter(DataObject dataObject, ref DataFilter filter, string graph)
    {
      GraphMap graphMap = _mapping.FindGraphMap(graph);
      ProjectDataFilter(dataObject, ref filter, graphMap);
    }

    // get real property name
    public void ProjectDataFilter(DataObject dataObject, ref DataFilter filter, GraphMap graphMap)
    {
      try
      {
        if (filter != null && (filter.Expressions != null || filter.OrderExpressions != null))
        {
          _graphMap = graphMap;

          DataObject _dataObject = dataObject;

          if (filter.Expressions != null)
          {
            for (int i = 0; i < filter.Expressions.Count; i++)
            {
              Expression expression = filter.Expressions[i];
            
              try
              {
                string[] propertyNameParts = expression.PropertyName.Split('.');
                Values values = expression.Values;
                string dataPropertyName = ProjectProperty(propertyNameParts, ref values);
                expression.PropertyName = dataPropertyName.Substring(dataPropertyName.LastIndexOf('.') + 1);
              }
              catch (Exception e)
              {
                _logger.Error("Error projecting data filter expression [" + expression.ToString() + "]: " + e.Message);
                filter.Expressions.RemoveAt(i--);
              }
            }
          }

          if (filter.OrderExpressions != null)
          {
            for (int i = 0; i < filter.OrderExpressions.Count; i++)
            {
              OrderExpression orderExpression = filter.OrderExpressions[i];

              try
              {
                string[] propertyNameParts = orderExpression.PropertyName.Split('.');
                string dataPropertyName = ProjectProperty(propertyNameParts);
                orderExpression.PropertyName = dataPropertyName.Substring(dataPropertyName.LastIndexOf('.') + 1);
              }
              catch (Exception e)
              {
                _logger.Error("Error projecting data filter expression [" + orderExpression.ToString() + "]: " + e.Message);
                filter.OrderExpressions.RemoveAt(i--);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        string error = "Error while projecting a DataFilter: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public string ProjectProperty(string[] propertyNameParts, ref Values values)
    {
      string dataPropertyName = String.Empty;

      string className = propertyNameParts[0];
      string templateName = propertyNameParts[1];
      string roleName = propertyNameParts[2];
      string classPath = String.Empty;

      if (className.Contains("$"))
      {
          string[] temp = className.Split('$');
          className = temp[0];
          classPath = temp[1];
      }

      ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.Find(
        cm => cm.classMap.name.Replace(" ", "").ToLower() == className.Replace(" ", "").ToLower());

      if (classTemplateMap == null)
        throw new Exception("Classmap [" + className + "] not found.");

      List<TemplateMap> templateMaps = classTemplateMap.templateMaps;
      TemplateMap templateMap = templateMaps.Find(tm => tm.name.ToLower() == templateName.ToLower());
      RoleMap roleMap = templateMap.roleMaps.Find(rm => rm.name.ToLower() == roleName.ToLower());

      switch (roleMap.type)
      {
        case RoleType.DataProperty:
          dataPropertyName = roleMap.propertyName;
          _valueListName = null;
          break;

        case RoleType.ObjectProperty:
          dataPropertyName = roleMap.propertyName;
          _valueListName = roleMap.valueListName;
          break;

        case RoleType.Property:
          //if last part...
          dataPropertyName = roleMap.propertyName;

          if (String.IsNullOrEmpty(roleMap.valueListName))
          {
            _valueListName = null;
          }
          else
          {
            _valueListName = roleMap.valueListName;

            for (int i = 0; i < values.Count; i++)
            {
              string value = values[i];       
              ValueListMap valueListMap = _mapping.valueListMaps.Find(x => x.name.ToLower() == roleMap.valueListName.ToLower());

              if (valueListMap != null && valueListMap.valueMaps != null)
              {
               /// ValueMap valueMap = valueListMap.valueMaps.Find(x => x.uri == value); 

                ValueMap valueMap = valueListMap.valueMaps.Find(x => x.label == value);

                if (valueMap != null)
                {
                  value = valueMap.internalValue;
                }
                else
                {
                  value = valueListMap.valueMaps[0].internalValue;
                }
              }

              values[i] = value;
            }
          }
          break;

        case RoleType.FixedValue:
        case RoleType.Possessor:
        case RoleType.Reference:
          throw new Exception("Role " + roleName + " can not be projected to property.");
      }

      return dataPropertyName;
    }

    public string ProjectProperty(string[] propertyNameParts)
    {
      Values values = new Values();
      return ProjectProperty(propertyNameParts, ref values);
    }
  }
}
