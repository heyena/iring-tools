﻿using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using Ingr.SP3D.Common.Middle.Services;
using Ingr.SP3D.Common.Middle;
using Ingr.SP3D.Structure.Middle;
using Ingr.SP3D.ReferenceData.Middle;
using Ingr.SP3D.Systems.Middle;
using Ingr.SP3D.ReferenceData.Middle.Services;
using NHibernate;
using Ninject.Extensions.Xml;


namespace iringtools.sdk.sp3ddatalayer
{
  public class SP3DProvider
  {
    public static readonly ILog _logger = LogManager.GetLogger(typeof(SP3DDataLayer));
    public string _dataPath = string.Empty;
    public string _scope = string.Empty;
    public string _dictionaryPath = string.Empty;
    public string _communityName = string.Empty;
    public string _configurationPath = string.Empty;
    public DatabaseDictionary _databaseDictionary = null;
    public DataDictionary _dataDictionary = null;
    MetadataManager metadataManagerReport = null, metadataManagerModel = null, metadataManagerCatalog = null;

    public BusinessObjectConfiguration _config = null;
    public BusinessObjectConfiguration _sp3dDataBaseDictionary = null;
    public string projectNameSpace = null;
    public AdapterSettings _settings = null;

    public SP3DProvider(AdapterSettings settings)
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(settings);

      if (_settings["DataLayerPath"] != null)
        _dataPath = _settings["DataLayerPath"];
      else
        _dataPath = _settings["AppDataPath"];

      _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];
      _settings["BinaryPath"] = @".\Bin\";

      _configurationPath = string.Format("{0}Configuration.{1}.xml", _dataPath, _scope);
      projectNameSpace = "org.iringtools.adapter.datalayer.proj_" + _scope;
      //"org.iringtools.adapter.datalayer.proj_12345_000.ABC"

      _dictionaryPath = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);
    }

    public DataDictionary GetDictionary()
    {
      if (_dataDictionary == null)
      {
        try
        {
          getConfigure();
          //Connect();

          if (_config != null)
            if (_config.businessCommodities != null)
            {
              _sp3dDataBaseDictionary = VerifyConfiguration(_config);
              _databaseDictionary = CreateDataBaseDictionary(_sp3dDataBaseDictionary);
            }

          GenerateSP3D(_settings, _sp3dDataBaseDictionary, _databaseDictionary);
          //Generate(_settings);
          readDictionary();
        }
        catch (Exception ex)
        {
          _logger.Error("connect SP3D: " + ex.ToString());
          throw ex;
        }
      }

      return _dataDictionary;
    }

    public void readDictionary()
    {
      _dataDictionary = new DataDictionary();
      _dataDictionary = Utility.Read<DataDictionary>(_dictionaryPath);
    }

    public void GenerateSP3D(AdapterSettings _settings, BusinessObjectConfiguration sp3dDataBaseDictionary, DatabaseDictionary databaseDictionary)
    {
      string projectName = "", applicationname = "";
      projectName = _settings["ProjectName"];
      applicationname = _settings["ApplicationName"];

      if (databaseDictionary != null && databaseDictionary.dataObjects != null && sp3dDataBaseDictionary != null && sp3dDataBaseDictionary.businessCommodities != null)
      {
        EntityGenerator generator = new EntityGenerator(_settings);

        string compilerVersion = "v4.0";
        if (!String.IsNullOrEmpty(_settings["CompilerVersion"]))
        {
          compilerVersion = _settings["CompilerVersion"];
        }

        generator.GenerateSP3D(compilerVersion, sp3dDataBaseDictionary, databaseDictionary, projectName, applicationname);
      }
    }

    public DatabaseDictionary CreateDataBaseDictionary(BusinessObjectConfiguration config)
    {
      _databaseDictionary = new DatabaseDictionary();
      _databaseDictionary.ConnectionString = config.ConnectionString;
      _databaseDictionary.Provider = config.Provider;
      _databaseDictionary.SchemaName = config.SchemaName;
      _databaseDictionary.dataObjects = new List<DataObject>();
      string propertyName = string.Empty, keyPropertyName = string.Empty, relatedPropertyName = string.Empty;
      string relatedTable = string.Empty, relationType = string.Empty, relatedObjectName = string.Empty;
      string relationName = string.Empty, relatedInterfaceName = string.Empty, startInterfaceName = string.Empty;
      string objectName = string.Empty;
      bool addClassName = false;

      foreach (BusinessCommodity businessCommodity in config.businessCommodities)
      {
        DataObject dataObject = new DataObject();
        dataObject.objectName = businessCommodity.commodityName;
        dataObject.objectNamespace = projectNameSpace;

        if (businessCommodity.dataFilter != null)
          dataObject.dataFilter = businessCommodity.dataFilter;

        if (businessCommodity.businessObjects.Count > 1)
          addClassName = true;
        else
          addClassName = false;

        foreach (BusinessObject businessObject in businessCommodity.businessObjects)
        {
          objectName = businessObject.objectName;
          //dataObject.objectName = objectName;

          //dataObject.tableName = objectName;
          dataObject.keyProperties = new List<KeyProperty>();
          dataObject.dataProperties = new List<DataProperty>();
          dataObject.dataRelationships = new List<DataRelationship>();

          if (addClassName)
          {
            DataProperty dataProperty = new DataProperty();
            dataProperty.propertyName = "className";
            dataProperty.dataType = DataType.String;
            dataProperty.columnName = string.Empty;
            dataProperty.isNullable = false;
            dataObject.dataProperties.Add(dataProperty);
          }

          if (businessObject.businessKeyProperties != null)
            foreach (BusinessKeyProperty businessKeyProerpty in businessObject.businessKeyProperties)
            {
              KeyProperty keyProperty = new KeyProperty();
              DataProperty dataProperty = new DataProperty();
              keyPropertyName = businessKeyProerpty.keyPropertyName; ;
              keyProperty.keyPropertyName = keyPropertyName;
              dataProperty.propertyName = keyPropertyName;
              dataProperty.dataType = DataType.String;
              dataProperty.columnName = keyPropertyName;
              dataProperty.isNullable = true;
              dataProperty.keyType = KeyType.assigned;

              if (!dataObject.keyProperties.Contains(keyProperty))
                dataObject.keyProperties.Add(keyProperty);

              if (!dataObject.dataProperties.Contains(dataProperty))
                dataObject.dataProperties.Add(dataProperty);
            }

          if (businessObject.businessInterfaces != null)
          {
            foreach (BusinessInterface businessInterface in businessObject.businessInterfaces)
            {
              foreach (BusinessProperty businessProperty in businessInterface.businessProperties)
              {
                propertyName = businessProperty.propertyName;
                DataProperty dataProperty = new DataProperty();

                if (!String.IsNullOrEmpty(businessProperty.columnName))
                  dataProperty.columnName = businessProperty.columnName;
                else
                  dataProperty.columnName = propertyName;
                dataProperty.propertyName = propertyName;

                dataProperty.dataType = businessProperty.dataType;
                dataProperty.isNullable = businessProperty.isNullable;
                dataProperty.isReadOnly = businessObject.isReadOnly;

                if (!String.IsNullOrEmpty(businessProperty.description))
                  dataProperty.description = businessObject.description;

                if (!dataObject.dataProperties.Contains(dataProperty))
                  dataObject.dataProperties.Add(dataProperty);

                if (!businessObject.businessProperties.Contains(businessProperty))
                  businessObject.businessProperties.Add(businessProperty);
              }
            }
          }

          if (businessObject.relatedObjects != null)
          {
            foreach (RelatedObject relatedObject in businessObject.relatedObjects)
            {
              relatedObjectName = relatedObject.objectName;

              if (relatedObject.businessInterfaces != null)
              {
                foreach (BusinessInterface businessInterface in relatedObject.businessInterfaces)
                {
                  foreach (BusinessProperty businessRelationProperty in businessInterface.businessProperties)
                  {
                    relatedPropertyName = businessRelationProperty.propertyName;
                    DataProperty dataProperty = new DataProperty();
                    dataProperty.propertyName = relatedObjectName + "_" + relatedPropertyName;
                    businessRelationProperty.propertyName = dataProperty.propertyName;
                    dataProperty.dataType = businessRelationProperty.dataType;

                    if (!String.IsNullOrEmpty(businessRelationProperty.columnName))
                      dataProperty.columnName = businessRelationProperty.columnName;
                    else
                      dataProperty.columnName = relatedPropertyName;

                    dataObject.dataProperties.Add(dataProperty);
                    businessObject.businessProperties.Add(businessRelationProperty);
                  }
                }
              }
            }
          }
        }
        _databaseDictionary.dataObjects.Add(dataObject);
      }
      return _databaseDictionary;
    }

    //public DataObject GetDataObject(string objectName)
    //{
    //  DataDictionary dictionary = GetDictionary();
    //  DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectName.ToLower());
    //  return dataObject;
    //}

    // flatten out recursive related objects and put them into starting business objects
    public BusinessObjectConfiguration VerifyConfiguration(BusinessObjectConfiguration config)
    {
      string relationshipName = string.Empty, relatedObjectName = string.Empty, objectName = string.Empty;
      string relatedInterfaceName = string.Empty, startInterfaceName = string.Empty, relatedPropertyName = string.Empty;
      string objectNamespace = string.Empty;
      BusinessObject targetBusinessObject = null;
      bool addToAllBusinessObject = false;
      bool addClassName = false;

      config.ConnectionString = config.ConnectionString;      

      if (config.businessCommodities != null)
        if (config.businessCommodities.Count > 0)
          foreach (BusinessCommodity businessCommodity in config.businessCommodities)
          {
            if (businessCommodity.businessObjects.Count == 1)
              businessCommodity.soleBusinessObject = true;

            businessCommodity.objectNamespace = projectNameSpace;

            if (businessCommodity.businessObjects.Count > 1)
              addClassName = true;
            else
              addClassName = false;

            foreach (BusinessObject businessObject in businessCommodity.businessObjects)
            {
              objectNamespace = projectNameSpace + "." + businessCommodity.commodityName.ToLower();
              initializeBO(businessObject, objectNamespace);
              objectName = businessObject.objectName;
              //classInfo = GetClassInformation(objectName);
              //if (classInfo == null)
              //  throw new Exception("class [" + objectName + "] is not found.");
              businessObject.rowNumber = -1;

              if (businessObject.businessProperties == null)
                businessObject.businessProperties = new List<BusinessProperty>();

              if (addClassName)
              {
                BusinessProperty bProperty = new BusinessProperty();
                bProperty.propertyName = "className";
                bProperty.dataType = DataType.String;
                bProperty.columnName = string.Empty;
                bProperty.isNullable = false;
                businessObject.businessProperties.Add(bProperty);
              }

              if (businessObject.businessInterfaces != null)
                if (businessObject.businessInterfaces.Count > 0)
                  foreach (BusinessInterface businessInterface in businessObject.businessInterfaces)
                  {
                    //InterfaceInformation interfaceInfo = GetInterfaceInformation(objectName, businessInterface.interfaceName);

                    foreach (BusinessProperty businessProperty in businessInterface.businessProperties)
                    {
                      //if (interfaceInfo != null)
                      //{
                      //if (!HasProperty(interfaceInfo, businessProperty.propertyName))
                      //{
                      //  throw new Exception("Property [" + businessProperty.propertyName + "] for class [" + businessObject.objectName + "] interface [" + businessInterface.interfaceName + "] is not found.");
                      //}
                      //else
                      businessProperty.dataType = GetDatatype(businessProperty.datatype);
                      businessProperty.datatype = businessProperty.dataType.ToString();

                      if (businessProperty.propertyName.ToLower() == "name")
                        businessProperty.columnName = "ItemName";
                      else if (string.IsNullOrEmpty(businessProperty.columnName))
                        businessProperty.columnName = businessProperty.propertyName;
                      //}
                      //else
                      //  throw new Exception("Interface [" + businessInterface.interfaceName + "] for class [" + businessObject.objectName + "] is not found.");
                    }
                  }
            }

            if (businessCommodity.relatedObjects != null)
              if (businessCommodity.relatedObjects.Count > 0)
                foreach (RelatedObject relatedObject in businessCommodity.relatedObjects)
                {
                  addToAllBusinessObject = false;

                  if (businessCommodity.soleBusinessObject)
                  {
                    targetBusinessObject = businessCommodity.businessObjects.First();
                    addToAllBusinessObject = true;
                  }
                  else if (relatedObject.startObjectName != null)
                  {
                    if (businessCommodity.GetBusinessObject(relatedObject.startObjectName) != null)
                      targetBusinessObject = businessCommodity.GetBusinessObject(relatedObject.startObjectName);
                    addToAllBusinessObject = true;
                  }

                  initializeBO(targetBusinessObject, objectNamespace);
                  initializeRelatedObject(relatedObject, targetBusinessObject);
                  AddRelatedObject(targetBusinessObject, relatedObject, targetBusinessObject);

                  if (relatedObject.relatedObjects != null)
                    if (relatedObject.relatedObjects.Count > 0)
                    {
                      traverseRelatedObjects(relatedObject, relatedObject.relatedObjects, targetBusinessObject);

                      if (!addToAllBusinessObject)
                      {
                        foreach (BusinessObject targetBO in businessCommodity.businessObjects)
                        {
                          initializeBO(targetBO, objectNamespace);
                          targetBO.relations = targetBusinessObject.relations;
                          targetBO.relatedObjects = targetBusinessObject.relatedObjects;
                          targetBO.rightClassNames = targetBusinessObject.rightClassNames;
                        }
                      }
                    }
                }
          }
      return config;
    }

    private void initializeBO(BusinessObject bo, string objectNamespace)
    {
      if (bo.relations == null)
        bo.relations = new List<BusinessRelation>();

      if (bo.relatedObjects == null)
        bo.relatedObjects = new List<RelatedObject>();

      if (bo.nodeType != NodeType.StartObject)
        bo.nodeType = NodeType.StartObject;

      if (bo.rightClassNames == null)
        bo.rightClassNames = new List<string>();

      if (bo.objectNamespace == null)
        bo.objectNamespace = objectNamespace;

      if (bo.businessKeyProperties != null)
        bo.businessKeyProperties = new List<BusinessKeyProperty>();

      if (bo.businessKeyProperties.Count == 0 || (bo.businessKeyProperties.First() != null && bo.businessKeyProperties.First().keyType != KeyType.assigned))
        checkKeyProperties(bo.businessKeyProperties, "BusinessObject", null);
    }

    private void initializeRelatedObject(RelatedObject robj, BusinessObject rootObject)
    {
      if (robj.businessKeyProperties == null)
        robj.businessKeyProperties = new List<BusinessKeyProperty>();

      if (robj.rightClassNames == null)
        robj.rightClassNames = new List<string>();

      if (robj.leftClassNames == null)
        robj.leftClassNames = new List<string>();

      robj.objectNamespace = rootObject.objectNamespace;
      robj.rowNumber = -1;

      if (robj.nodeType != NodeType.MiddleObject)
        robj.nodeType = NodeType.MiddleObject;

      if (robj.businessProperties == null)
        robj.businessProperties = new List<BusinessProperty>();

      if (robj.businessKeyProperties.Count == 0 || (robj.businessKeyProperties.First() != null && robj.businessKeyProperties.First().keyType != KeyType.assigned))
        checkKeyProperties(robj.businessKeyProperties, "RelatedObject", null);

      if (!string.IsNullOrEmpty(robj.relationName) && string.IsNullOrEmpty(robj.relationTableName))
      {
        robj.relationTableName = "X" + robj.relationName;
      }
      else if (string.IsNullOrEmpty(robj.relationName) && !string.IsNullOrEmpty(robj.relationTableName))
      {
        robj.relationName = robj.relationTableName.Substring(1, robj.relationTableName.Length - 1);
      }

      if (robj.businessInterfaces != null)
        if (robj.businessInterfaces.Count > 0)
          foreach (BusinessInterface businessInterface in robj.businessInterfaces)
          {
            if (businessInterface.businessProperties != null)
              if (businessInterface.businessProperties.Count > 0)
                foreach (BusinessProperty businessProperty in businessInterface.businessProperties)
                {
                  businessProperty.dataType = GetDatatype(businessProperty.datatype);
                  businessProperty.datatype = businessProperty.dataType.ToString();

                  if (businessProperty.propertyName.ToLower() == "name")
                    businessProperty.columnName = "ItemName";
                  else if (string.IsNullOrEmpty(businessProperty.columnName))
                    businessProperty.columnName = businessProperty.propertyName;

                  robj.businessProperties.Add(businessProperty);
                }
          }

      rootObject.setUniqueRelation(robj);
    }

    private void initializeRlation(BusinessRelation relation)
    {
      if (relation.businessKeyProperties == null)
        relation.businessKeyProperties = new List<BusinessKeyProperty>();

      if (relation.businessKeyProperties.Count == 0 || (relation.businessKeyProperties.First() != null && relation.businessKeyProperties.First().keyType != KeyType.assigned))
        checkKeyProperties(relation.businessKeyProperties, "BusinessRelation", relation);
    }

    private void traverseRelatedObjects(RelatedObject parentObject, List<RelatedObject> relatedObjects, BusinessObject businessObject)
    {
      foreach (RelatedObject newPareendObj in relatedObjects)
      {
        initializeRelatedObject(newPareendObj, businessObject);
        AddRelatedObject(parentObject, newPareendObj, businessObject);

        if (newPareendObj.relatedObjects != null)
        {
          traverseRelatedObjects(newPareendObj, newPareendObj.relatedObjects, businessObject);
        }
        else
          newPareendObj.nodeType = NodeType.EndObject;
      }
    }

    public void addUniqueClassName(string name, List<string> classNames)
    {
      int suffix = 0;

      if (classNames.Contains(name))
      {
        name += "_";

        do
        {
          name = name.Substring(0, name.LastIndexOf('_'));
          name += "_" + suffix;
          suffix++;
        }
        while (classNames.Contains(name));
      }

      classNames.Add(name);
    }    

    private void AddRelatedObject(RootBusinessObject parentObject, RelatedObject relatedObject, BusinessObject businessObject)
    {
      businessObject.addUniqueRelatedObject(relatedObject);

      if (relatedObject.nodeType != NodeType.StartObject)
        addUniqueClassName(relatedObject.relationName, relatedObject.leftClassNames);

      BusinessRelation relation = businessObject.addUniqueRelation(relatedObject, parentObject);
      initializeRlation(relation);

      if (relatedObject.relatedObjects != null)
        if (relatedObject.relatedObjects.Count > 0)
          foreach (RelatedObject relatedChild in relatedObject.relatedObjects)
          {
            initializeRelatedObject(relatedChild, businessObject);
            addUniqueClassName(relatedChild.relationName, relatedObject.rightClassNames);            
          }

      if (relation.leftClassNames != null)
        addUniqueClassName(parentObject.objectName, relation.leftClassNames);
      checkKeyProperties(relation.businessKeyProperties, "BusinessRelation", relation);

      
    }

    //create keyProperty 
    private void createKeyProperties(List<BusinessKeyProperty> businessKeyProperties, string type, BusinessRelation relation)
    {
      BusinessKeyProperty keyProperty = new BusinessKeyProperty();

      switch (type.ToLower())
      {
        case "businessobject":
        case "relatedobject": keyProperty.keyPropertyName = "oid";
          break;
        case "businessrelation":
          keyProperty.keyPropertyName = "oidOrigin";
          relation.createRelationBusinessProperty("oidDestination");
          break;
      }

      keyProperty.dataType = DataType.String;
      keyProperty.datatype = keyProperty.dataType.ToString();       
      keyProperty.columnName = keyProperty.keyPropertyName;
      keyProperty.isNullable = false;
      keyProperty.keyType = KeyType.assigned;
      businessKeyProperties.Add(keyProperty);
    }

    //check keypropertiese
    private void checkKeyProperties(List<BusinessKeyProperty> keyProperties, string type, BusinessRelation relation)
    {
      if (keyProperties.Count == 0)
      {
        createKeyProperties(keyProperties, type, relation);
      }
      else
      {
        foreach (BusinessKeyProperty businessKeyProerpty in keyProperties)
        {
          if (businessKeyProerpty.keyType != KeyType.assigned)
          {            
            businessKeyProerpty.dataType = DataType.String;
            businessKeyProerpty.datatype = businessKeyProerpty.dataType.ToString();
            businessKeyProerpty.columnName = businessKeyProerpty.keyPropertyName;
            businessKeyProerpty.isNullable = false;
            businessKeyProerpty.keyType = KeyType.assigned;
          }
        }
      }
    }

    public bool IsRelated(string relationName, string relatedObjectName, string objectName, string relatedInterfaceName, string startInterfaceName)
    {
      bool isRelated = false;
      string relationshipName = relationName.ToLower();

      if (relatedObjectName != null)
      {
        switch (relationshipName)
        {
          case "designparent":
          case "designparents":
            isRelated = IsDesignParent(objectName, relatedObjectName);
            break;
          case "assemblyparents":
          case "assemblyparent":
            isRelated = IsAssemblyParent(relatedObjectName, objectName);
            break;
          case "supportedpipe":
          case "supportedpipes":
            isRelated = IsSupportPipe(relatedObjectName, objectName);
            break;
          case "supportedpipedesignparents":
          case "supportedpipedesignparent":
          case "supportpipedesignparent":
          case "supportpipedesignparents":
          case "supportpipesdesignparent":
          case "supportpipesdesignparents":
          case "supportedpipesdesignparents":
          case "supportedpipesdesignparent":
            isRelated = IsSupportDesignParent(relatedObjectName, objectName);
            break;

          case "part":
            isRelated = isMadeFrom(relatedObjectName, objectName);
            break;
          default:
            isRelated = false;
            break;
        }
      }
      else if (!String.IsNullOrEmpty(relatedInterfaceName) && !String.IsNullOrEmpty(startInterfaceName))
      {
        isRelated = isTraversRelated(relatedObjectName, objectName, relatedInterfaceName, startInterfaceName);
      }

      return isRelated;
    }

    public bool IsDesignParent(string objectName, string relatedObjectName)
    {
      bool isParent = hasInterfaces(relatedObjectName, objectName, "IJDesignParent", "IJDesignChild");
      return isParent;
    }

    public bool IsAssemblyParent(string relatedObjectName, string objectName)
    {
      bool isParent = false;
      return isParent;
    }

    public bool IsSupportPipe(string relatedObjectName, string objectName)
    {
      bool isSupport = false;
      return isSupport;
    }

    public bool IsSupportDesignParent(string relatedObjectName, string objectName)
    {
      bool isParent = false;
      return isParent;
    }

    public bool isTraversRelated(string relatedObjectName, string objectName, string relatedInterfaceName, string startedInterfaceName)
    {
      bool isRelated = hasInterfaces(relatedObjectName, objectName, relatedInterfaceName, startedInterfaceName);
      return isRelated;
    }

    private bool hasInterfaces(string relatedObjectName, string objectName, string relatedInterfaceName, string startedInterfaceName)
    {
      string key = "", relatedKey = "";
      bool hasInterfaces = false;
      ClassInformation classInfo = null, relatedClassInfo = null;

      classInfo = GetClassInformation(objectName);
      relatedClassInfo = GetClassInformation(relatedObjectName);

      ReadOnlyDictionary<InterfaceInformation> interfaces = classInfo.Interfaces;
      ReadOnlyDictionary<InterfaceInformation> relatedInterfaces = relatedClassInfo.Interfaces;
      key = LookIntoICollection(interfaces.Keys, startedInterfaceName);
      relatedKey = LookIntoICollection(interfaces.Keys, relatedInterfaceName);

      if (key != null && relatedKey != null)
        hasInterfaces = true;

      return hasInterfaces;
    }

    public bool isMadeFrom(string relatedObjectName, string objectName)
    {
      bool isMadeFrom = false;
      return isMadeFrom;
    }

    private DataType GetDatatype(string datatype)
    {
      if (datatype == null)
        return DataType.String;

      switch (datatype.ToLower())
      {
        case "string":
          return DataType.String;
        case "bool":
        case "boolean":
          return DataType.Boolean;
        case "float":
        case "decimal":
        case "double":
          return DataType.Double;
        case "integer":
        case "int":
        case "number":
          return DataType.Int64;
        default:
          return DataType.String;
      }
    }

    private Provider GetProvider(string provider)
    {
      if (provider == null)
        return Provider.MsSql2008;

      switch (provider.ToLower())
      {
        case "mssql2008":
          return Provider.MsSql2008;
        case "mssql2005":
          return Provider.MsSql2005;
        case "mssql2000":
          return Provider.MsSql2000;
        case "oracle8i":
          return Provider.Oracle8i;
        case "oracle9i":
          return Provider.Oracle9i;
        case "oracle10g":
          return Provider.Oracle10g;
        default:
          return Provider.MsSql2008;
      }
    }

    public ClassInformation GetClassInformation(string className)
    {
      ClassInformation classInfo = null;
      ReadOnlyDictionary<ClassInformation> classes = metadataManagerModel.Classes;
      string key = LookIntoICollection(classes.Keys, className);
      classes.TryGetValue(key, out classInfo);
      return classInfo;
    }

    //public InterfaceInformation GetInterfaceInformation(string propertyInterfaceName)
    //{
    //  InterfaceInformation interfaceInfo = null;
    //  ReadOnlyDictionary<InterfaceInformation> interfaces = metadataManagerModel.Interfaces;
    //  string key = LookIntoICollection(interfaces.Keys, propertyInterfaceName);

    //  if (key != null)
    //    interfaces.TryGetValue(key, out interfaceInfo);
    //  return interfaceInfo;
    //}

    public InterfaceInformation GetInterfaceInformation(string className, string propertyInterfaceName)
    {
      ClassInformation classInfo = GetClassInformation(className);

      InterfaceInformation interfaceInfo = null;
      ReadOnlyDictionary<InterfaceInformation> interfaces = classInfo.Interfaces;
      string key = LookIntoICollection(interfaces.Keys, propertyInterfaceName);

      if (key != null)
        interfaces.TryGetValue(key, out interfaceInfo);
      return interfaceInfo;
    }

    public bool HasProperty(InterfaceInformation interfaceInfo, string propertyName)
    {
      ReadOnlyDictionary<PropertyInformation> propertyInformation = interfaceInfo.Properties;
      if (LookIntoICollection(propertyInformation.Keys, propertyName) != null)
        return true;
      else
        return false;
    }

    public string LookIntoICollection(ICollection<string> collection, string target)
    {
      string fullKey = null;
      System.Collections.IEnumerator keyIe = collection.GetEnumerator();

      while (keyIe.MoveNext())
      {
        fullKey = keyIe.Current.ToString();
        if (fullKey.ToLower().Contains(target.ToLower()))
        {
          return fullKey;
        }
      }

      return null;
    }

    public void getConfigure()
    {
      if (_config == null)
        _config = Utility.Read<BusinessObjectConfiguration>(_configurationPath);
    }

    public void CloseSession(ISession session)
    {
      try
      {
        if (session != null)
        {
          session.Close();
          session = null;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error closing NHibernate session: " + ex);
      }
    }

    public DataFilter FilterByIdentity(string objectType, DataFilter filter, IdentityProperties identityProperties)
    {
      DataObject dataObject = _databaseDictionary.dataObjects.Find(d => d.objectName == objectType);
      DataProperty dataProperty = dataObject.dataProperties.Find(p => p.columnName == identityProperties.IdentityProperty);

      if (dataProperty != null)
      {
        if (filter == null)
        {
          filter = new DataFilter();
        }

        //bool hasExistingExpression = false;

        if (filter.Expressions == null)
        {
          filter.Expressions = new List<org.iringtools.library.Expression>();
        }
        else if (filter.Expressions.Count > 0)
        {
          org.iringtools.library.Expression firstExpression = filter.Expressions.First();
          org.iringtools.library.Expression lastExpression = filter.Expressions.Last();
          firstExpression.OpenGroupCount++;
          lastExpression.CloseGroupCount++;
          //hasExistingExpression = true;
        }

        //string identityValue = _keyRing[identityProperties.KeyRingProperty].ToString();

        //org.iringtools.library.Expression expression = new org.iringtools.library.Expression
        //{
        //  PropertyName = dataProperty.propertyName,
        //  RelationalOperator = RelationalOperator.EqualTo,
        //  Values = new Values
        //  {
        //    identityValue,
        //  },
        //  IsCaseSensitive = identityProperties.IsCaseSensitive
        //};

        //if (hasExistingExpression)
        //  expression.LogicalOperator = LogicalOperator.And;
        //filter.Expressions.Add(expression);
      }

      return filter;
    }

    public void Connect()
    {
      Site SP3DSite = null;
      SP3DSite = MiddleServiceProvider.SiteMgr.ConnectSite();

      if (SP3DSite != null)
      {
        if (SP3DSite.Plants.Count > 0)
          MiddleServiceProvider.SiteMgr.ActiveSite.OpenPlant((Plant)SP3DSite.Plants[0]);
      }

      # region sp3d API
      Catalog SP3DCatalog = null;
      Model SP3DModel = null;
      Report SP3DReport = null;
      //SP3DCatalog = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantCatalog;

      SP3DModel = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantModel;

      //SP3DReport = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantReport;

      //metadataManagerCatalog = SP3DCatalog.MetadataMgr;
      // metadataManagerReport = SP3DReport.MetadataMgr;
      metadataManagerModel = SP3DModel.MetadataMgr;

      //MiddleServiceProvider.SiteMgr.ActiveSite.MetadataMgr


      //string displayName, name, showupMsg = "", category, iid, interfaceInfoNamespace, propertyName, propertyDescriber;
      //string propertyInterfaceInformationStr, unitTypeString;
      //Type type;
      //ReadOnlyDictionary<InterfaceInformation> interfactInfo, commonInterfaceInfo;
      //ReadOnlyDictionary<PropertyInformation> properties;
      //ReadOnlyDictionary<BOCInformation> oSystemsByName = metadataManager.BOCs;
      //bool complex, comAccess, displayedOnPage, isvalueRequired, metaDataAccess, metadataReadOnly, SqlAccess;
      //string propertyDisplayName, proPropertyName, uomType;
      //CodelistInformation codeListInfo;
      //InterfaceInformation propertyInterfaceInformation;
      //SP3DPropType sp3dProType;
      //UnitType unitType;
      //string showupPropertyMessage = "";
      //string showupProMsg = "";
      //foreach (string key in oSystemsByName.Keys)
      //{
      //  BOCInformation bocInfo = null;
      //  oSystemsByName.TryGetValue(key, out bocInfo);
      //  displayName = bocInfo.DisplayName;
      //  name = bocInfo.Name;
      //  type = bocInfo.GetType();
      //  interfactInfo = bocInfo.DefiningInterfaces;
      //  foreach (string infoKey in interfactInfo.Keys)
      //  {
      //    InterfaceInformation itemInterfaceInfo;
      //    interfactInfo.TryGetValue(infoKey, out itemInterfaceInfo);
      //    interfaceInfoNamespace = itemInterfaceInfo.Namespace;
      //    category = itemInterfaceInfo.Category;
      //    iid = itemInterfaceInfo.IID;
      //    properties = itemInterfaceInfo.Properties;

      //    foreach (string propertyKey in properties.Keys)
      //    {
      //      PropertyInformation propertyInfo;
      //      properties.TryGetValue(propertyKey, out propertyInfo);
      //      complex = propertyInfo.Complex;

      //      codeListInfo = propertyInfo.CodeListInfo;
      //      comAccess = propertyInfo.COMAccess;
      //      displayedOnPage = propertyInfo.DisplayedOnPropertyPage;
      //      propertyDisplayName = propertyInfo.DisplayName;
      //      propertyInterfaceInformation = propertyInfo.InterfaceInfo;
      //      propertyInterfaceInformationStr = propertyInterfaceInformation.ToString();
      //      isvalueRequired = propertyInfo.IsValueRequired;
      //      metaDataAccess = propertyInfo.MetadataAccess;
      //      metadataReadOnly = propertyInfo.MetadataReadOnly;
      //      proPropertyName = propertyInfo.Name;
      //      sp3dProType = propertyInfo.PropertyType;
      //      SqlAccess = propertyInfo.SQLAccess;
      //      unitType = propertyInfo.UOMType;
      //      unitTypeString = unitType.ToString();

      //      showupPropertyMessage = showupPropertyMessage + "\n propertyInfo.key: " + propertyKey + "\n"
      //                            + "CodeListInfo.DisplayName: " + codeListInfo.DisplayName + "\n"
      //                            + "comAccess: " + comAccess + "\n"
      //                            + "propertyDisplayName: " + propertyDisplayName + "\n"
      //                            + "propertyInterfaceInformation: " + propertyInterfaceInformation.Name + "\n"
      //                            + "proPropertyName: " + proPropertyName;


      //    }
      //  }

      //  commonInterfaceInfo = bocInfo.CommonInterfaces;
      //  foreach (string comInfoKey in commonInterfaceInfo.Keys)
      //  {
      //    InterfaceInformation comItemInterfaceInfo;
      //    commonInterfaceInfo.TryGetValue(comInfoKey, out comItemInterfaceInfo);
      //    interfaceInfoNamespace = comItemInterfaceInfo.Namespace;
      //    category = comItemInterfaceInfo.Category;
      //    iid = comItemInterfaceInfo.IID;
      //    properties = comItemInterfaceInfo.Properties;

      //    foreach (string propertyKey in properties.Keys)
      //    {
      //      PropertyInformation propertyInfo;
      //      properties.TryGetValue(propertyKey, out propertyInfo);
      //      complex = propertyInfo.Complex;

      //      codeListInfo = propertyInfo.CodeListInfo;
      //      comAccess = propertyInfo.COMAccess;
      //      displayedOnPage = propertyInfo.DisplayedOnPropertyPage;
      //      propertyDisplayName = propertyInfo.DisplayName;
      //      propertyInterfaceInformation = propertyInfo.InterfaceInfo;
      //      propertyInterfaceInformationStr = propertyInterfaceInformation.ToString();
      //      isvalueRequired = propertyInfo.IsValueRequired;
      //      metaDataAccess = propertyInfo.MetadataAccess;
      //      metadataReadOnly = propertyInfo.MetadataReadOnly;
      //      proPropertyName = propertyInfo.Name;
      //      sp3dProType = propertyInfo.PropertyType;
      //      SqlAccess = propertyInfo.SQLAccess;
      //      unitType = propertyInfo.UOMType;
      //      unitTypeString = unitType.ToString();

      //      showupProMsg = showupProMsg + "\n propertyInfo.key: " + propertyKey + "\n"
      //                            + "CodeListInfo.DisplayName: " + codeListInfo.DisplayName + "\n"
      //                            + "comAccess: " + comAccess + "\n"
      //                            + "propertyDisplayName: " + propertyDisplayName + "\n"
      //                            + "propertyInterfaceInformation: " + propertyInterfaceInformation.Name + "\n"
      //                            + "proPropertyName: " + proPropertyName;          


      //    }


      //  }

      //  showupMsg = showupMsg + "\n bocInfo.key: " + key + "\n"
      //            + "bocInfo.DisplayName: " + displayName + "\n"
      //            + "bocInfo.Name: " + name + "\n"
      //            + "bocInfo.type: " + type.FullName + "\n"
      //           // + "bocInfo.DefiningInterfaces: " + showupPropertyMessage + "\n"
      //            + "bocInfo.commonInterfaceInfo: " + showupProMsg + "\n";                 

      //}
      //File.WriteAllText(@"C:\temp\sp3d.txt", showupMsg);

      //System.Windows.Forms.MessageBox.Show(showupMsg);
      //oSystemsByName

      # endregion sp3d API
    }

    # region LoadConfiguratoin and LoadDataObjects functions
    //public void LoadConfiguration()
    //{
    //    if (_config == null)
    //    {
    //        string uri = String.Format(
    //            "{0}Configuration.{1}.xml",
    //            _settings["XmlPath"],
    //            _settings["ApplicationName"]
    //        );

    //        XElement configDocument = Utility.ReadXml(uri);
    //        _config = Utility.DeserializeDataContract<BusinessObjectConfiguration>(configDocument.ToString());
    //    }
    //}

    //public IList<IDataObject> LoadDataObjects(string objectType)
    //{
    //    try
    //    {
    //        IList<IDataObject> dataObjects = new List<IDataObject>();

    //        //Get Path from Scope.config ({project}.{app}.config)
    //        string path = String.Format(
    //            "{0}{1}\\{2}.csv",
    //             _settings["BaseDirectoryPath"],
    //            _settings["SP3DFolderPath"],
    //            objectType
    //        );

    //        IDataObject dataObject = null;
    //        TextReader reader = new StreamReader(path);
    //        while (reader.Peek() >= 0)
    //        {
    //            string csvRow = reader.ReadLine();

    //            dataObject = FormDataObject(objectType, csvRow);

    //            if (dataObject != null)
    //                dataObjects.Add(dataObject);
    //        }
    //        reader.Close();

    //        return dataObjects;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.Error("Error in LoadDataObjects: " + ex);
    //        throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
    //    }
    //}

    # endregion LoadConfiguratoin and LoadDataObjects functions

    public IDataObject FormDataObject(string objectType, string csvRow)
    {
      try
      {
        IDataObject dataObject = new GenericDataObject
        {
          ObjectType = objectType,
        };

        XElement commodityElement = new XElement("a");
        //GetCommodityConfig(objectType);

        if (!String.IsNullOrEmpty(csvRow))
        {
          IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

          string[] csvValues = csvRow.Split(',');

          int index = 0;
          foreach (var attributeElement in attributeElements)
          {
            string name = attributeElement.Attribute("name").Value;
            string dataType = attributeElement.Attribute("dataType").Value.ToLower();
            string value = csvValues[index++].Trim();

            // if data type is not nullable, make sure it has a value
            if (!(dataType.EndsWith("?") && value == String.Empty))
            {
              if (dataType.Contains("bool"))
              {
                if (value.ToUpper() == "TRUE" || value.ToUpper() == "YES")
                {
                  value = "1";
                }
                else
                {
                  value = "0";
                }
              }
              else if (value == String.Empty && (
                       dataType.StartsWith("int") ||
                       dataType == "double" ||
                       dataType == "single" ||
                       dataType == "float" ||
                       dataType == "decimal"))
              {
                value = "0";
              }
            }

            dataObject.SetPropertyValue(name, value);
          }
        }

        return dataObject;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in FormDataObject: " + ex);

        throw new Exception(
          "Error while forming a dataObject of type [" + objectType + "] from SPPID.",
          ex
        );
      }
    }



    public void Generate(AdapterSettings _settings)
    {
      string projectName = "", applicationname = "";
      projectName = _settings["ProjectName"];
      applicationname = _settings["ApplicationName"];

      if (_databaseDictionary != null && _databaseDictionary.dataObjects != null)
      {
        EntityGenerator generator = new EntityGenerator(_settings);

        string compilerVersion = "v4.0";
        if (!String.IsNullOrEmpty(_settings["CompilerVersion"]))
        {
          compilerVersion = _settings["CompilerVersion"];
        }

        generator.Generate(compilerVersion, _databaseDictionary, projectName, applicationname);
      }
    }





    //public Response SaveDataObjects(string objectType, IList<IDataObject> dataObjects)
    //{
    //    try
    //    {
    //        Response response = new Response();

    //        // Create data object directory in case it does not exist
    //        Directory.CreateDirectory(_settings["SP3DFolderPath"]);

    //        string path = String.Format(
    //         "{0}{1}\\{2}.csv",
    //           _settings["BaseDirectoryPath"],
    //          _settings["SP3DFolderPath"],
    //          objectType
    //        );

    //        //TODO: Need to update file, not replace it!
    //        TextWriter writer = new StreamWriter(path);

    //        foreach (IDataObject dataObject in dataObjects)
    //        {
    //            Status status = new Status();

    //            try
    //            {
    //                string identifier = GetIdentifier(dataObject);
    //                status.Identifier = identifier;

    //                List<string> csvRow = new List<string>();
    //                  //FormCSVRow(objectType, dataObject);

    //                writer.WriteLine(String.Join(", ", csvRow.ToArray()));
    //                status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
    //            }
    //            catch (Exception ex)
    //            {
    //                status.Level = StatusLevel.Error;

    //                string message = String.Format(
    //                  "Error while posting dataObject [{0}]. {1}",
    //                  dataObject.GetPropertyValue("Tag"),
    //                  ex.ToString()
    //                );

    //                status.Messages.Add(message);
    //            }

    //            response.Append(status);
    //        }

    //        writer.Close();

    //        return response;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.Error("Error in LoadDataObjects: " + ex);
    //        throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
    //    }
    //}       
  }
}

