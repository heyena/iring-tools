using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using System.Xml.Linq;
using org.iringtools.library;
using VDS.RDF.Query;
using VDS.RDF;
using System.Text.RegularExpressions;
using org.iringtools.utility;
using org.iringtools.mapping;
using log4net;

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

    protected AdapterSettings _settings = null;
    protected Mapping _mapping = null;
    protected DataDictionary _dictionary = null;
    protected GraphMap _graphMap = null;
    protected string _graphBaseUri = null;
    protected IDataLayer _dataLayer = null;
    protected IList<IDataObject> _dataObjects = null;
    protected Dictionary<string, string>[] _dataRecords = null;
    protected Dictionary<string, List<string>> _classIdentifiers = null;
    protected List<string> _relatedObjectPaths = null;

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

    public BasePart7ProjectionEngine()
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      _relatedObjectsCache = new Dictionary<string, List<IDataObject>>();
    }

    public abstract XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects);
    public abstract IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument);

    //propertyPath = "Instrument.LineItems.Tag";
    protected List<IDataObject> GetRelatedObjects(string propertyPath, IDataObject dataObject)
    {
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
    protected void SetRelatedRecords(int dataObjectIndex, string relatedPropertyPath, List<string> relatedValues)
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
          else
          {
            relatedRecords = new List<Dictionary<string, string>>();
          }

          relatedRecordsMap[relatedObjectType] = relatedRecords;
        }

        // only fill last related object values now; values of intermediate related objects' parent might not be available yet.
        if (i == objectNames.Length - 1)
        {
          for (int j = 0; j < relatedValues.Count; j++)
          {
            relatedRecords[j][property] = relatedValues[j];
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
    protected void FillRelatedRecords()
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
    protected void AppendRelatedObjects()
    {
      // dictonary cache of related object types and list of identifiers
      Dictionary<string, List<string>> relatedObjectTypeIdentifiers = new Dictionary<string, List<string>>();

      foreach (Dictionary<string, List<Dictionary<string, string>>> relatedRecordsMap in _relatedRecordsMaps)
      {
        foreach (var relatedRecordsMapPair in relatedRecordsMap)
        {
          string relatedObjectType = relatedRecordsMapPair.Key;
          List<KeyProperty> keyProperties = GetKeyProperties(relatedObjectType);

          foreach (Dictionary<string, string> relatedRecord in relatedRecordsMapPair.Value)
          {
            string relatedObjectIdentifier = String.Empty;
            foreach (KeyProperty keyProperty in keyProperties)
            {
              relatedObjectIdentifier += relatedRecord[keyProperty.keyPropertyName];
            }

            if (!relatedObjectTypeIdentifiers.ContainsKey(relatedObjectType))
            {
              List<string> relatedObjectIdentifiers = new List<string> { relatedObjectIdentifier };
              relatedObjectTypeIdentifiers.Add(relatedObjectType, relatedObjectIdentifiers);

              IDataObject relatedObject = _dataLayer.Create(relatedObjectType, new List<string> { relatedObjectIdentifier }).First();
              foreach (var relatedRecordPair in relatedRecord)
              {
                relatedObject.SetPropertyValue(relatedRecordPair.Key, relatedRecordPair.Value);
              }

              _dataObjects.Add(relatedObject);
            }
            else if (!relatedObjectTypeIdentifiers[relatedObjectType].Contains(relatedObjectIdentifier))
            {
              relatedObjectTypeIdentifiers[relatedObjectType].Add(relatedObjectIdentifier);

              IDataObject relatedObject = _dataLayer.Create(relatedObjectType, new List<string> { relatedObjectIdentifier }).First();
              foreach (var relatedRecordPair in relatedRecord)
              {
                relatedObject.SetPropertyValue(relatedRecordPair.Key, relatedRecordPair.Value);
              }

              _dataObjects.Add(relatedObject);
            }
          }
        }
      }
    }

    protected List<KeyProperty> GetKeyProperties(string objectType)
    {
      DataObject dataObject = _dictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());
      return dataObject.keyProperties;
    }

    // turn data record into data object
    protected IDataObject CreateDataObject(string objectType, int objectIndex)
    {
      Dictionary<string, string> dataRecord = _dataRecords[objectIndex];
      List<KeyProperty> keyProperties = GetKeyProperties(objectType);
      string identifier = String.Empty;

      foreach (KeyProperty keyProperty in keyProperties)
      {
        identifier += dataRecord[keyProperty.keyPropertyName];
      }

      IDataObject dataObject = _dataLayer.Create(objectType, new List<string>{identifier}).First<IDataObject>();

      foreach (var pair in dataRecord)
      {
        dataObject.SetPropertyValue(pair.Key, pair.Value);
      }

      return dataObject;
    }

    protected void SetClassIdentifiers(DataDirection direction)
    {
      switch (direction)
      {
        case DataDirection.Outbound:
          SetOutboundClassIdentifiers();
          break;

        case DataDirection.InboundSparql:
          SetInboundSparqlClassIdentifiers();
          break;

        case DataDirection.InboundDto:
          SetInboundDtoClassIdentifiers();
          break;
      }
    }

    private void SetOutboundClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassTemplateMap classTemplateMap in _graphMap.classTemplateMaps)
      {
        ClassMap classMap = classTemplateMap.classMap;
        List<string> identifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
              if (identifiers.Count == i)
              {
                identifiers.Add(value);
              }
              else
              {
                identifiers[i] += classMap.identifierDelimiter + value;
              }
            }
          }
          else if (_dataObjects != null)  // identifier comes from a property
          {
            for (int i = 0; i < _dataObjects.Count; i++)
            {
              IDataObject valueObject = GetValueObjects(identifier, i).First();
              string propertyName = identifier.Substring(identifier.LastIndexOf('.') + 1);
              string value = Convert.ToString(valueObject.GetPropertyValue(propertyName));

              if (identifiers.Count == i)
              {
                identifiers.Add(value);
              }
              else
              {
                identifiers[i] += classMap.identifierDelimiter + value;
              }
            }
          }
        }

        _classIdentifiers[classMap.id] = identifiers;
      }
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

    protected string GetClassIdentifierValue(List<string> identifiers, string delimiter, int dataObjectIndex)
    {
      string classIdentifierValue = String.Empty;

      foreach (string identifier in identifiers)
      {
        if (classIdentifierValue.Length > 0)
          classIdentifierValue += delimiter;

        // identifier is a fixed value
        if (identifier.StartsWith("#") && identifier.EndsWith("#"))
        {
          string value = identifier.Substring(1, identifier.Length - 2);
          classIdentifierValue += value;
        }
        else  // identifier is a property map
        {
          List<IDataObject> identifierValueObjects = GetValueObjects(identifier, dataObjectIndex);

          if (identifierValueObjects != null && identifierValueObjects.Count > 0)
          {
            IDataObject identifierValueObject = identifierValueObjects.First();
            string propertyName = identifier.Substring(identifier.LastIndexOf('.') + 1);
            string value = Convert.ToString(identifierValueObject.GetPropertyValue(propertyName));
            classIdentifierValue += value;
          }
        }
      }

      return classIdentifierValue;
    }

    private void SetInboundSparqlClassIdentifiers()
    {
      _classIdentifiers.Clear();

      if (_memoryStore != null)
      {
        ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.First();
        string classId = classTemplateMap.classMap.id;
        string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classId);
        _logger.Debug(query);
        object results = _memoryStore.ExecuteQuery(query);

        if (results != null)
        {
          SparqlResultSet resultSet = (SparqlResultSet)results;

          foreach (SparqlResult result in resultSet)
          {
            string classInstance = result.Value("class").ToString();

            if (!String.IsNullOrEmpty(classInstance))
            {
              if (!_classIdentifiers.ContainsKey(classId))
              {
                _classIdentifiers[classId] = new List<string> { classInstance };
              }
              else if (!_classIdentifiers[classId].Contains(classInstance))
              {
                _classIdentifiers[classId].Add(classInstance);
              }
            }
            else
            {
              throw new Exception("Class identifier of [" + classId + "] is null");
            }
          }
        }
      }
    }

    protected void SetInboundDtoClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassTemplateMap classTemplateMap in _graphMap.classTemplateMaps)
      {
        ClassMap classMap = classTemplateMap.classMap;
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
                classIdentifiers[i] += classMap.identifierDelimiter + value;
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
                  classIdentifiers[i] += classMap.identifierDelimiter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.id] = classIdentifiers;
      }
    }

    // resolve the dataFilter into data object terms
    public void ProjectDataFilter(DataDictionary dictionary, ref DataFilter filter, string graph)
    {
      try
      {
        if (filter != null && (filter.Expressions != null || filter.OrderExpressions != null))
        {
          _graphMap = _mapping.FindGraphMap(graph);

          DataObject _dataObject = dictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);

          if (filter.Expressions != null)
          {
            foreach (Expression expression in filter.Expressions)
            {
              string[] propertyNameParts = expression.PropertyName.Split('.');
              Values values = expression.Values;
              string dataPropertyName = ProjectProperty(propertyNameParts, ref values);
              expression.PropertyName = RemoveDataPropertyAlias(dataPropertyName);

              #region already done in Project Property
              //if (_roleType == RoleType.ObjectProperty)
              //{
              //  if (expression.RelationalOperator == RelationalOperator.EqualTo)
              //  {
              //    expression.Values = ProjectPropertValues(expression.Values);
              //    expression.RelationalOperator = RelationalOperator.In;
              //  }
              //  else if (expression.RelationalOperator == RelationalOperator.In)
              //  {
              //    expression.Values = ProjectPropertValues(expression.Values);
              //  }
              //  else
              //  {
              //    throw new Exception(
              //      "Invalid Expression in DataFilter. " +
              //      "Object Property Roles can only use EqualTo and In in the expression."
              //    );
              //  }
              //}
              #endregion 
            }
          }

          if (filter.OrderExpressions != null)
          {
            foreach (OrderExpression orderExpression in filter.OrderExpressions)
            {
              string[] propertyNameParts = orderExpression.PropertyName.Split('.');   
              string dataPropertyName = ProjectPropertyName(propertyNameParts);
              orderExpression.PropertyName = RemoveDataPropertyAlias(dataPropertyName);
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Error while projecting a DataFilter for use with DataLayer.", ex);
      }
    }

    public string RemoveDataPropertyAlias(string dataPropertyName)
    {
      char[] dot = { '.' };
      string[] dataPropertyNameParts = dataPropertyName.Split(dot, 2);

      if (dataPropertyNameParts.Count() > 1)
        return dataPropertyNameParts[1];
      else
        return dataPropertyNameParts[0];
    }

    //public Values ProjectPropertValues(Values values)
    //{
    //  Values dataValues = new Values();

    //  ValueListMap valueList = _mapping.valueListMaps.Find(vl => vl.name == _valueListName);

    //  foreach (string value in values)
    //  {
    //    List<ValueMap> valueMaps = valueList.valueMaps.FindAll(vm => vm.uri == value);
    //    foreach (ValueMap valueMap in valueMaps)
    //    {
    //      string dataValue = valueMap.internalValue;
    //      dataValues.Add(dataValue);
    //    }
    //  }

    //  return dataValues;
    //}

    //THIS ASSUMES CLASS IS ONLY USED ONCE
    //resolve the propertyName expression into data object propertyName
    public string ProjectProperty(string[] propertyNameParts, ref Values values)
    {
      string dataPropertyName = String.Empty;

      string className = propertyNameParts[0];
      string templateName = propertyNameParts[1];
      string roleName = propertyNameParts[2];

      ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.Find(
        cm => Utility.TitleCase(cm.classMap.name).ToUpper() == className.ToUpper());

      List<TemplateMap> templateMaps = classTemplateMap.templateMaps;
      TemplateMap templateMap = templateMaps.Find(tm => tm.name == templateName);

      RoleMap roleMap = templateMap.roleMaps.Find(rm => rm.name == roleName);

      switch (roleMap.type)
      {
        case RoleType.DataProperty:
          dataPropertyName = roleMap.propertyName;
          _valueListName = null;
          break;

        case RoleType.FixedValue:
          throw new Exception(String.Format(
            "Invalid PropertyName Expression in DataFilter.  Fixed Value Role ({0}) is not allowed in the expression.",
            roleName)
           );

        case RoleType.ObjectProperty:
          dataPropertyName = roleMap.propertyName;
          _valueListName = roleMap.valueListName;
          break;

        case RoleType.Possessor:
          throw new Exception(String.Format(
            "Invalid PropertyName Expression in DataFilter.  Possessor Role ({0}) is not allowed in the expression.",
            roleName)
          );

        case RoleType.Property:
          //if last part...
          dataPropertyName = roleMap.propertyName;

          if (String.IsNullOrEmpty(roleMap.valueListName))
          {
            dataPropertyName = roleMap.propertyName;
            _valueListName = null;
          }
          else
          {
            dataPropertyName = roleMap.propertyName;
            _valueListName = roleMap.valueListName;

            for (int i = 0; i < values.Count; i++)
            {
              string value = values[i];
              value = _mapping.ResolveValueMap(_valueListName, value);
              values[i] = value;
            }
          }
          break;

        case RoleType.Reference:
          throw new Exception(String.Format(
            "Invalid PropertyName Expression in DataFilter.  Reference Role ({0}) is not allowed in the expression.",
            roleName)
          );
      }

      return dataPropertyName;
    }

    public string ProjectPropertyName(string[] propertyNameParts)
    {
      Values values = new Values();
      return ProjectProperty(propertyNameParts, ref values);
    }
  }

  public enum DataDirection
  {
    InboundSparql,
    InboundDto,
    Outbound,
  }
}
