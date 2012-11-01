using System;
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
    public string _databaseDictionaryPath = string.Empty;
    public string _communityName = string.Empty;
    public string _configurationPath = string.Empty;
    public string _verifiedConfigurationPath = string.Empty;
    public DatabaseDictionary _databaseDictionary = null;
    public DataDictionary _dataDictionary = null;
    public Catalog SP3DCatalog = null;
    public Model SP3DModel = null;
    public Report SP3DReport = null;
    public MetadataManager metadataManagerReport = null, metadataManagerModel = null, metadataManagerCatalog = null;
    public BusinessObjectConfiguration _config = null, _sp3dDataBaseDictionary = null;
    public string projectNameSpace = null;
    public AdapterSettings _settings = null;
    public Dictionary<string, IList<IDataObject>> _sourceDataObjects = null;
    public List<string> _filtertedKeys = null;

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
      _verifiedConfigurationPath = string.Format("{0}VerifiedConfiguration.{1}.xml", _dataPath, _scope);
      projectNameSpace = "org.iringtools.adapter.datalayer.proj_" + _scope;

      _dictionaryPath = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);
      _databaseDictionaryPath = string.Format("{0}DataBaseDictionary.{1}.xml", _dataPath, _scope);

      readDictionary();
      readDataBaseDictionary();
      readBusinessObjects();
    }

    public DataDictionary GetDictionary()
    {
      try
      {
        if (ModifiedConfig())
        {
          createArtifacts(string.Empty);
          readDictionary();
        }
        
        return _dataDictionary;
      }
      catch (Exception ex)
      {
        _logger.Error("connect SP3D: " + ex.ToString());
        throw ex;
      }
    }

    

    public void readDictionary()
    {
      if (File.Exists(_dictionaryPath))
      {
        _dataDictionary = new DataDictionary();
        _dataDictionary = Utility.Read<DataDictionary>(_dictionaryPath);
      }
    }

    public void readDataBaseDictionary()
    {
      if (File.Exists(_databaseDictionaryPath))
      {
        _databaseDictionary = new DatabaseDictionary();
        _databaseDictionary = Utility.Read<DatabaseDictionary>(_databaseDictionaryPath);
      }
    }

    public void readBusinessObjects()
    {
      if (File.Exists(_verifiedConfigurationPath))
      {
        _sp3dDataBaseDictionary = new BusinessObjectConfiguration();
        _sp3dDataBaseDictionary = Utility.Read<BusinessObjectConfiguration>(_verifiedConfigurationPath);
      }
    }    

    public void CreateCachingTables(string businessCommodityName, DataFilter filter)
    {
      Response response = new Response();
      List<IDataObject> dataObjects = new List<IDataObject>();       

      foreach (BusinessCommodity bc in _config.businessCommodities)
      {
        dataObjects.AddRange(GetSP3D(bc.commodityName, filter));        
      }

      CreateCachingDBTables(businessCommodityName, filter);
      response.Append(PostCachingDataObjects(dataObjects));

      if (response.Level == StatusLevel.Error)
        throw new Exception(response.Messages.ToString());
    }

    public Response CreateCachingDBTables(string businessCommodityName, DataFilter filter)
    {
      Response response = new Response();
      Status status = new Status();
      status.Messages = new Messages();
      ISession session = null;

      if (filter == null)
      {
        try
        {
          if (businessCommodityName == string.Empty)
            session = NHibernateSessionManager.Instance.CreateCachingTables(_settings["AppDataPath"], _settings["Scope"]);
          else
            session = NHibernateSessionManager.Instance.CreateCachingTables(_settings["AppDataPath"], _settings["Scope"], businessCommodityName);
          session.Flush();
        }
        catch (Exception ex)
        {
          status.Level = StatusLevel.Error;
          status.Messages.Add(string.Format("Error while creating caching tables", ex));
          status.Results.Add("ResultTag", "create caching tables");
          _logger.Error("Error posting data object to data layer: " + ex);
        }
        finally
        {
          CloseSession(session);
        }
      }

      return response;   
    }

    public Response PostSP3DBusinessObjects(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      DataObject realDataObject = null;
      DataProperty dataProperty = new DataProperty();
      dataProperty.propertyName = "className";

      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          string objectType = dataObjects[0].GetType().Name;

          foreach (IDataObject dataObject in dataObjects)
          {
            realDataObject = (DataObject)dataObject;

            if (realDataObject.dataProperties.Contains(dataProperty))
              realDataObject.objectNamespace = realDataObject.objectNamespace + "." + realDataObject.objectName;
 
            Status status = new Status();
            status.Messages = new Messages();

            if (dataObject != null)
            {
              string identifier = String.Empty;

              try
              {
                // NOTE: Id property is not available if it's not mapped and will cause exception
                identifier = dataObject.GetPropertyValue("Id").ToString();
              }
              catch (Exception ex)
              {
                _logger.Error(string.Format("Error in Post: {0}", ex));
              }  // no need to handle exception because identifier is only used for statusing

              status.Identifier = identifier;

              try
              {
                session.SaveOrUpdate(dataObject);
                session.Flush();
                status.Messages.Add(string.Format("Record [{0}] saved successfully.", identifier));
              }
              catch (Exception ex)
              {
                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error while posting record [{0}]. {1}", identifier, ex));
                status.Results.Add("ResultTag", identifier);
                _logger.Error("Error posting data object to data layer: " + ex);
              }
            }
            else
            {
              status.Level = StatusLevel.Error;
              status.Identifier = String.Empty;
              status.Messages.Add("Data object is null or duplicate. See log for details.");
            }

            response.Append(status);
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception(string.Format("Error while posting data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public Response PostCachingDataObjects(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], "sp3ddl_" + _settings["Scope"]);
      int index = 0;

      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          string objectType = dataObjects[0].GetType().Name;          

          foreach (IDataObject dataObject in dataObjects)
          {
            index++;
            Status status = new Status();
            status.Messages = new Messages();           
            
            if (dataObject != null)
            {
              string identifier = String.Empty;

              try
              {
                // NOTE: Id property is not available if it's not mapped and will cause exception
                identifier = dataObject.GetPropertyValue("Id").ToString();
              }
              catch (Exception ex)
              {
                _logger.Error(string.Format("Error in Post: {0}", ex));
              }  // no need to handle exception because identifier is only used for statusing

              status.Identifier = identifier;             

              try
              {
                session.SaveOrUpdate(dataObject);
                session.Flush();
                status.Messages.Add(string.Format("Record [{0}] saved successfully.", identifier));
              }
              catch (Exception ex)
              {
                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error while posting record [{0}]. {1}", identifier, ex));
                status.Results.Add("ResultTag", identifier);
                _logger.Error("Error posting data object to data layer: " + ex);
              }
            }
            else
            {
              status.Level = StatusLevel.Error;
              status.Identifier = String.Empty;
              status.Messages.Add("Data object is null or duplicate. See log for details.");
            }

            response.Append(status);
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception(string.Format("Error while posting data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public Response RefreshCachingTables(string businessCommodityName)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;
      getConfig();

      try
      {
        createArtifacts(businessCommodityName);
        CreateCachingTables(businessCommodityName, null);
        response.StatusList.Add(new Status()
        {
          Messages = new Messages()
            {
              "Success"
            }
        });
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.StatusList.Add(new Status()
        {
          Messages = new Messages()
            {
              e.Message.ToString()
            }
        });        
      }

      MiddleServiceProvider.TransactionMgr.Abort();
      MiddleServiceProvider.Cleanup();

      Utility.Write<Response>(response, string.Format("{0}Response.{1}.xml", _dataPath, _scope));
      return response;
    }

    public Response RefreshCachingTable(string businessCommodityName, DataFilter dataFilter)
    {
      Response response = new Response();
      getConfig();

      try
      {
        CreateCachingTables(businessCommodityName, dataFilter);       
        response.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages = new Messages() { e.Message };
      }

      MiddleServiceProvider.TransactionMgr.Abort();
      MiddleServiceProvider.Cleanup();
      return response;
    }

    public void GenerateSP3D(AdapterSettings _settings, BusinessObjectConfiguration sp3dDataBaseDictionary)
    {
      string projectName = "", applicationname = "";
      projectName = _settings["ProjectName"];
      applicationname = _settings["ApplicationName"];

      if (_databaseDictionary != null && _databaseDictionary.dataObjects != null && sp3dDataBaseDictionary != null && sp3dDataBaseDictionary.businessCommodities != null)
      {
        EntityGenerator generator = new EntityGenerator(_settings);

        string compilerVersion = "v4.0";
        if (!String.IsNullOrEmpty(_settings["CompilerVersion"]))
        {
          compilerVersion = _settings["CompilerVersion"];
        }

        generator.GenerateSP3D(compilerVersion, sp3dDataBaseDictionary, _databaseDictionary, projectName, applicationname, "sp3d");

        sp3dDataBaseDictionary.ConnectionString = GetCachingConnectionString(sp3dDataBaseDictionary.ConnectionString, sp3dDataBaseDictionary.Provider, sp3dDataBaseDictionary.StagingDataBaseName);
        generator.GenerateSP3D(compilerVersion, sp3dDataBaseDictionary, _databaseDictionary, projectName, applicationname, "caching");
      }
    }

    public BusinessObjectConfiguration CreateDataBaseDictionary(BusinessObjectConfiguration config, string businessCommodityName)
    {      
      string propertyName = string.Empty, keyPropertyName = string.Empty, relatedPropertyName = string.Empty;
      string relatedTable = string.Empty, relationType = string.Empty, relatedObjectName = string.Empty;
      string relationName = string.Empty, relatedInterfaceName = string.Empty, startInterfaceName = string.Empty;
      string objectName = string.Empty;
      bool addClassName = false, ifReplace = false;
      DataObject dataObject = null, existingDataObject = null;

      if (string.IsNullOrEmpty(businessCommodityName) || !File.Exists(_databaseDictionaryPath))
      {
        _databaseDictionary = new DatabaseDictionary();
        _databaseDictionary.ConnectionString = GetCachingConnectionString(config.ConnectionString, config.Provider, config.StagingDataBaseName);
        _databaseDictionary.Provider = config.Provider;
        _databaseDictionary.SchemaName = config.SchemaName;
        _databaseDictionary.dataObjects = new List<DataObject>();
        businessCommodityName = string.Empty;
      }
      else
      {
        ifReplace = true;

        if (_databaseDictionary == null)
          _databaseDictionary = Utility.Read<DatabaseDictionary>(_databaseDictionaryPath);

        if (_databaseDictionary.GetDataObject(businessCommodityName) == null)
          throw new Exception("DataObject [" + businessCommodityName + "] does exist in existing database dictionary");
      }

      foreach (BusinessCommodity businessCommodity in config.businessCommodities)
      {
        if (businessCommodityName != string.Empty)
        {
          if (businessCommodity.commodityName.ToLower() != businessCommodityName.ToLower())
            continue;
          else
            dataObject = _databaseDictionary.GetDataObject(businessCommodityName);
        }
        else
          dataObject = new DataObject();

        dataObject.objectName = businessCommodity.commodityName;
        dataObject.tableName = businessCommodity.commodityName.ToLower();
        dataObject.objectNamespace = projectNameSpace;
        dataObject.dataRelationships = businessCommodity.dataRelationships;

        if (businessCommodity.dataFilter != null)
          dataObject.dataFilter = businessCommodity.dataFilter;

        if (businessCommodity.businessObjects.Count > 1)
          addClassName = true;
        else
          addClassName = false;

        foreach (BusinessObject businessObject in businessCommodity.businessObjects)
        {
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

                dataProperty.columnName = propertyName;
                dataProperty.propertyName = propertyName;
                dataProperty.dataType = businessProperty.dataType;
                dataProperty.isNullable = businessProperty.isNullable;
                dataProperty.isReadOnly = businessObject.isReadOnly;
                businessProperty.isNative = false;

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
                    businessRelationProperty.isNative = false;
                    dataProperty.dataType = businessRelationProperty.dataType;
                    dataProperty.columnName = dataProperty.propertyName;
                    dataObject.dataProperties.Add(dataProperty);
                    businessObject.businessProperties.Add(businessRelationProperty);
                  }
                }
              }
            }
          }
        }

        if (ifReplace)
        {
          existingDataObject = _databaseDictionary.GetDataObject(dataObject.objectName);
          _databaseDictionary.dataObjects[_databaseDictionary.dataObjects.IndexOf(existingDataObject)] = dataObject;
        }
        else 
        {
          _databaseDictionary.dataObjects.Add(dataObject);
        }       
      }

      if (File.Exists(_verifiedConfigurationPath))
        File.Delete(_verifiedConfigurationPath);
      Utility.Write<BusinessObjectConfiguration>(config, _verifiedConfigurationPath);  
    
      return config;
    }

    public Response DeleteSP3DIdentifiers(string objectType, IList<string> identifiers)
    {
      IList<IDataObject> dataObjects = Create(objectType, identifiers);
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      DataObject realDataObject = null;
      DataProperty dataProp = new DataProperty();
      dataProp.propertyName = "className";

      try
      {
        foreach (IDataObject dataObject in dataObjects)
        {
          realDataObject = (DataObject)dataObject;

          if (realDataObject.dataProperties.Contains(dataProp))
            realDataObject.objectNamespace = realDataObject.objectNamespace + "." + dataObject.GetPropertyValue("className");

          string identifier = dataObject.GetPropertyValue("Id").ToString();
          session.Delete(dataObject);
          Status status = new Status();
          status.Messages = new Messages();
          status.Identifier = identifier;
          status.Messages.Add(string.Format("Record [{0}] deleted successfully.", identifier));
          response.Append(status);
        }

        session.Flush();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        response.Append(status);
      }
      finally
      {
        CloseSession(session);
      }

      return response;
    }

    public Response DeleteSP3DBusinessObjects(string objectType, DataFilter filter)
    {     
      Response response = new Response();
      response.StatusList = new List<Status>();
      Status status = new Status();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      BusinessCommodity bc = _sp3dDataBaseDictionary.GetBusinessCommoditiy(objectType);
      DatabaseDictionary databaseDictionarySP3D = null;

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        status.Identifier = objectType;

        foreach (BusinessObject bo in bc.businessObjects)
        {
          StringBuilder queryString = new StringBuilder();
          queryString.Append("from " + bo.objectName);

          if (filter.Expressions.Count > 0)
          {
            //DataObject dataObject = _databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
            databaseDictionarySP3D = Utility.Read<DatabaseDictionary>(this._databaseDictionaryPath.Substring(0, _databaseDictionaryPath.LastIndexOf('.')) + "." + bo.objectName + ".xml");
            string whereClause = filter.ToSqlWhereClause(databaseDictionarySP3D, bo.tableName, String.Empty);
            queryString.Append(whereClause);
          }

          session.Delete(queryString.ToString());
          session.Flush();
          status.Messages.Add(string.Format("Records of type [{0}] deleted succesfully.", objectType));
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        //no need to status, thrown exception will be statused above.
      }
      finally
      {
        CloseSession(session);
      }

      response.Append(status);
      return response;
    }

    //public DataObject GetDataObject(string objectName)
    //{
    //  DataDictionary dictionary = GetDictionary();
    //  DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectName.ToLower());
    //  return dataObject;
    //}


    // flatten out recursive related objects and put them into starting business objects
    public BusinessObjectConfiguration VerifyConfiguration(BusinessObjectConfiguration config, string businessCommodityName)
    {
      Connect(config);
      string relationshipName = string.Empty, relatedObjectName = string.Empty, objectName = string.Empty;
      string relatedInterfaceName = string.Empty, startInterfaceName = string.Empty, relatedPropertyName = string.Empty;
      string objectNamespace = string.Empty;
      BusinessObject targetBusinessObject = null;
      bool addToAllBusinessObject = false;
      bool addClassName = false;
      config.ConnectionString = config.ConnectionString;
      ClassInformation classInfo = null;
      PropertyInformation propertyInfo = null;
      RelationshipInformation relationInfo = null;

      if (businessCommodityName != string.Empty)
      {
        if (config.GetBusinessCommoditiy(businessCommodityName) == null)
          throw new Exception("DataObject [" + businessCommodityName + "] does not exist in the configuration file");

        businessCommodityName = businessCommodityName.ToLower();
      }

      if (config.businessCommodities != null)
        if (config.businessCommodities.Count > 0)
          foreach (BusinessCommodity businessCommodity in config.businessCommodities)
          {
            if (businessCommodityName != string.Empty)
            {
              if (businessCommodity.commodityName.ToLower() != businessCommodityName)
                continue;
            }

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
              classInfo = GetClassInformation(objectName);             

              if (classInfo == null)
                throw new Exception("class [" + objectName + "] does not exist in SP3D databases.");

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
                    InterfaceInformation interfaceInfo = GetInterfaceInformation(classInfo, businessInterface.interfaceName);

                    foreach (BusinessProperty businessProperty in businessInterface.businessProperties)
                    {
                      if (interfaceInfo != null)
                      {
                        propertyInfo = GetPropertyInfo(interfaceInfo, businessProperty.propertyName);
                        if (propertyInfo == null)
                        {
                          throw new Exception("Property [" + businessProperty.propertyName + "] for class [" + businessObject.objectName + "] interface [" + businessInterface.interfaceName + "] does not exist.");
                        }
                        else
                        {
                          businessProperty.dataType = GetDatatype(businessProperty.datatype);
                          businessProperty.datatype = businessProperty.dataType.ToString();
                          businessProperty.codeList = propertyInfo.CodeListInfo.CodelistMembers;
                          if (businessProperty.propertyName.ToLower() == "name")
                            businessProperty.columnName = "ItemName";
                          else if (string.IsNullOrEmpty(businessProperty.columnName))
                            businessProperty.columnName = businessProperty.propertyName;
                        }
                      }
                      else
                        throw new Exception("Interface [" + businessInterface.interfaceName + "] for class [" + businessObject.objectName + "] does not exist in SP3D databases.");
                    }
                  }
            }

            if (businessCommodity.relatedObjects != null)
              if (businessCommodity.relatedObjects.Count > 0)
                foreach (RelatedObject relatedObject in businessCommodity.relatedObjects)
                {
                  relationInfo = GetRealtionInformation(relatedObject.relationName, classInfo);

                  if (relationInfo == null)
                  {
                    throw new Exception("Relationship [" + relatedObject.relationName + "] does not exist in SP3D databases.");
                  }
                  else
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
          }
      return config;
    }

    public FilterBase FindFilter(string filterName)
    {
      if (filterName == null)
        return null;

      var names = filterName.Split('\\', '/');
      if (names.Count() < 2)  // need top level folder name plus filter name
        return null;

      FilterBase filter = null;
      FilterFolder afld = null;

      // "Plant Filters" or "Catalog Filters"
      if (names[0] == "Plant Filters")
      {
        if (SP3DModel == null)
          return null;

        afld = (FilterFolder)SP3DModel.Folders.FirstOrDefault(fld => fld.Name == names[0]);
      }
      else if (names[0] == "Catalog Filters")
      {
        if (SP3DCatalog == null)
          return null;

        afld = (FilterFolder)SP3DCatalog.Folders.FirstOrDefault(fld => fld.Name == names[0]);
      }

      // Traversal logic
      if (afld != null)
      {
        // Already matched first filter name with top-level folder name.
        int n = 1;
        for (; n < (names.Count() - 1); ++n)    // child folders name matches; last name is filter name
        {
          if (afld.ChildFolders.FirstOrDefault(fld => fld.Name == names[n]) != null)
            afld = afld.ChildFolders.FirstOrDefault(fld => fld.Name == names[n]);
          else
            break;
        }

        if (afld != null && n < names.Count())    // child filter name match
        {
          filter = afld.ChildFilters.FirstOrDefault(flt => flt.Name == names[n]);
        }
      }

      return filter;
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

    public ClassInformation GetClassInformation(string className)
    {
      ClassInformation classInfo = null;
      ReadOnlyDictionary<ClassInformation> classes = metadataManagerModel.Classes;
      string key = LookIntoICollection(classes.Keys, className);
      classes.TryGetValue(key, out classInfo);

      if (classes == null)
      {
        classes = metadataManagerCatalog.Classes;
        key = LookIntoICollection(classes.Keys, className);
        classes.TryGetValue(key, out classInfo);
      }

      if (classes == null)
      {
        classes = metadataManagerReport.Classes;
        key = LookIntoICollection(classes.Keys, className);
        classes.TryGetValue(key, out classInfo);
      }

      return classInfo;
    }

    public RelationshipInformation GetRelationInformation(string relationName, ClassInformation classInfo)
    {
      RelationshipInformation relationInfo = null;
      return relationInfo;
    }
    
    public InterfaceInformation GetInterfaceInformation(ClassInformation classInfo, string propertyInterfaceName)
    {
      InterfaceInformation interfaceInfo = classInfo.GetInterfaceInfo(propertyInterfaceName);      
      return interfaceInfo;
    }

    public PropertyInformation GetPropertyInfo(InterfaceInformation interfaceInfo, string propertyName)
    {
      string realPropName = propertyName.Substring(0, 1).ToUpper() + propertyName.Substring(1, propertyName.Length - 1);
      PropertyInformation aPropertyInfo = interfaceInfo.GetPropertyInfo(realPropName);
      return aPropertyInfo;
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

    public void createArtifacts(string businessCommodityName)
    {
      if (_config != null)
        if (_config.businessCommodities != null)
        {
          _sp3dDataBaseDictionary = VerifyConfiguration(_config, businessCommodityName);
          _sp3dDataBaseDictionary = CreateDataBaseDictionary(_sp3dDataBaseDictionary, businessCommodityName);
        }

      GenerateSP3D(_settings, _sp3dDataBaseDictionary);
      Generate(_settings);    
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

    public Plant Connect(BusinessObjectConfiguration config)
    {
      Site SP3DSite = null;      

      if (MiddleServiceProvider.SiteMgr.ActiveSite != null)
        if (MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant != null)
          return MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant;

      /********************
       * SP3D ProviderType="MSSQL" DBServer="HOUS01013\SP3D_GLNG" SiteDB="GLNG_SDB" SiteSchemaDB="GLNG_SDB_SCHEMA" PlantName="GLNG" DebugMessages="0" DebugFile="c:\sp3d_adapter.log" DebugMaxParamLen="100000" CodelistAttrLength="40" AsmHierarchyStop="CPConfigProjectRoot" DgnHierarchyStop="CPConfigProjectRoot"
       * <provider>MsSql2008</provider>
       * <connectionString>Data Source=HOUS01014\sp3d_2011_gbl;Initial Catalog=gbl_mdb;Integrated Security=true</connectionString>
       * <stagingDataBaseName>SP3DStaging</stagingDataBaseName>
       * <schemaName>dbo</schemaName>      
       * m_site = MiddleServiceProvider.SiteMgr.ConnectSite(spDBServer, spSiteDB, (spDBProviderType.ToUpper() == "MSSQL" ? SiteManager.eDBProviderTypes.MSSQL : SiteManager.eDBProviderTypes.Oracle), spSiteSchemaDB);
       *********************/

      SP3DSite = MiddleServiceProvider.SiteMgr.ConnectSite(GetDataSource(config.ConnectionString), config.SiteDataBaseName, (config.Provider.ToUpper().Contains("MSSQL") ? SiteManager.eDBProviderTypes.MSSQL : SiteManager.eDBProviderTypes.Oracle), config.SiteDataBaseName + "_SCHEMA");
      Plant SP3DPlant = null;

      if (SP3DSite != null)
      {
        if (SP3DSite.Plants.Count > 0)
        {
          if (!string.IsNullOrEmpty(config.PlantName))
            SP3DPlant = SP3DSite.Plants.FirstOrDefault<Plant>(o=>o.Name.ToLower() == config.PlantName.ToLower());
          else
            SP3DPlant = (Plant)SP3DSite.Plants[0];

          MiddleServiceProvider.SiteMgr.ActiveSite.OpenPlant(SP3DPlant);
        }
      }

      # region sp3d API
      
      SP3DCatalog = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantCatalog;
      SP3DModel = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantModel;
      SP3DReport = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantReport;
      metadataManagerCatalog = SP3DCatalog.MetadataMgr;
      metadataManagerReport = SP3DReport.MetadataMgr;
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
      return SP3DPlant;
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

    public RelationshipInformation GetRealtionInformation(string relationName, ClassInformation classInfo)
    {
      RelationshipInformation relationInfo = null;
      relationInfo = metadataManagerModel.GetRelationshipInfo(relationName, classInfo.Namespace);

      if (relationInfo == null)
      {
        relationInfo = metadataManagerCatalog.GetRelationshipInfo(relationName, classInfo.Namespace);
      }

      if (relationInfo == null)
      {
        relationInfo = metadataManagerReport.GetRelationshipInfo(relationName, classInfo.Namespace);
      }

      return relationInfo;
    }

    

    public IList<IDataObject> GetSP3D(string objectType, DataFilter filter)
    {
      string commodityName = string.Empty;
      int numberOfObjects = 0;
      List<IDataObject> dataObjects = null;

      try
      { 
        BusinessCommodity businessCommodity = _sp3dDataBaseDictionary.GetBusinessCommoditiy(objectType);
        numberOfObjects = businessCommodity.businessObjects.Count;
        commodityName = objectType.ToLower();
        GetSP3DFilteredKeys(businessCommodity);       

        dataObjects = new List<IDataObject>();
        DataDictionary dataDictionary = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _dataPath, _scope, commodityName));
        DatabaseDictionary databaseDictionary = Utility.Read<DatabaseDictionary>(string.Format("{0}DatabaseDictionary.{1}.{2}.xml", _dataPath, _scope, commodityName));

        getSourceDataObjects(businessCommodity, dataDictionary, databaseDictionary);

        foreach (BusinessObject bo in businessCommodity.businessObjects)
          dataObjects.AddRange(getDataObjectRows(businessCommodity, bo.objectName, commodityName, filter, dataDictionary, databaseDictionary));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      
      return dataObjects;      
    }

    public long GetCountSP3D(string objectType, DataFilter filter, string commodityName, DataDictionary dataDictionary, DatabaseDictionary databaseDictionary)
    {
      ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

      try
      {
        if (databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select count(*) from " + objectType);

        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          DataFilter clonedFilter = Utility.CloneDataContractObject<DataFilter>(filter);
          clonedFilter.OrderExpressions = null;
          DataObject dataObject = dataDictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
          string whereClause = clonedFilter.ToSqlWhereClause(dataDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        long count = query.List<long>().First();
        return count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public long GetSP3DCount(string objectType, DataFilter filter, DataDictionary dataDictionary, DatabaseDictionary databaseDictionary)
    {
      long totalCount = 0, objectRowNumber = 0;
      string objectName = string.Empty;
      BusinessObject businessObject = null;

      try
      {
        if (_sp3dDataBaseDictionary != null)
          if (_sp3dDataBaseDictionary.businessCommodities != null)
          {
            BusinessCommodity businessCommodity = _sp3dDataBaseDictionary.GetBusinessCommoditiy(objectType);
            string commodityName = objectType.ToLower();

            if (dataDictionary == null)
              dataDictionary = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _settings["AppDataPath"], _settings["Scope"], commodityName));

            DataObject dataObject = dataDictionary.dataObjects.First();
           
            objectName = dataObject.objectName;

            if (businessCommodity.GetBusinessObject(objectName) != null)
            {
              businessObject = businessCommodity.GetBusinessObject(objectName);
              objectRowNumber = GetCountSP3D(dataObject.objectName, filter, commodityName, dataDictionary, databaseDictionary);
              businessObject.rowNumber = objectRowNumber;
              totalCount += objectRowNumber;
            }
            
            return totalCount;
          }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);

        throw new Exception(
          "Error while getting a count of type [" + objectType + "].",
          ex
        );
      }
      return -1;
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        DataObject objectDefinition = _dataDictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectDefinition.objectName + ", " + _settings["ExecutingAssemblyName"]);
        IDataObject dataObject = null;

        if (identifiers != null)
        {
          foreach (string identifier in identifiers)
          {
            if (!String.IsNullOrEmpty(identifier))
            {
              IQuery query = session.CreateQuery("from " + objectType + " where Id = ?");
              query.SetString(0, identifier);
              dataObject = query.List<IDataObject>().FirstOrDefault<IDataObject>();

              if (dataObject == null)
              {
                dataObject = (IDataObject)Activator.CreateInstance(type);
                dataObject.SetPropertyValue("Id", identifier);
              }
            }
            else
            {
              dataObject = (IDataObject)Activator.CreateInstance(type);
            }

            dataObjects.Add(dataObject);
          }
        }
        else
        {
          dataObject = (IDataObject)Activator.CreateInstance(type);
          dataObjects.Add(dataObject);
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in CreateList: " + ex);
        throw new Exception(string.Format("Error while creating a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          DataObject dataObjectDef = (from DataObject o in _databaseDictionary.dataObjects
                                      where o.objectName == objectType
                                      select o).FirstOrDefault();

          if (dataObjectDef == null)
            return null;

          if (dataObjectDef.keyProperties.Count == 1)
          {
            queryString.Append(" where Id in ('" + String.Join("','", identifiers.ToArray()) + "')");
          }
          else if (dataObjectDef.keyProperties.Count > 1)
          {
            string[] keyList = null;
            int identifierIndex = 1;
            foreach (string identifier in identifiers)
            {
              string[] idParts = identifier.Split(dataObjectDef.keyDelimeter.ToCharArray()[0]);

              keyList = new string[idParts.Count()];

              int partIndex = 0;
              foreach (string part in idParts)
              {
                if (identifierIndex == identifiers.Count())
                {
                  keyList[partIndex] += part;
                }
                else
                {
                  keyList[partIndex] += part + ", ";
                }

                partIndex++;
              }

              identifierIndex++;
            }

            int propertyIndex = 0;
            foreach (KeyProperty keyProperty in dataObjectDef.keyProperties)
            {
              string propertyValues = keyList[propertyIndex];

              if (propertyIndex == 0)
              {
                queryString.Append(" where " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }
              else
              {
                queryString.Append(" and " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }

              propertyIndex++;
            }
          }
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        IList<IDataObject> dataObjects = query.List<IDataObject>();

        // order data objects as list of identifiers
        if (identifiers != null)
        {
          IList<IDataObject> orderedDataObjects = new List<IDataObject>();

          foreach (string identifier in identifiers)
          {
            if (identifier != null)
            {
              foreach (IDataObject dataObject in dataObjects)
              {
                if (dataObject.GetPropertyValue("Id").ToString().ToLower() == identifier.ToLower())
                {
                  orderedDataObjects.Add(dataObject);
                  //break;  // include dups also
                }
              }
            }
          }

          return orderedDataObjects;
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        StringBuilder queryString = new StringBuilder();
        queryString.Append("select Id from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          DataObject dataObject = _databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = filter.ToSqlWhereClause(_databaseDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        return query.List<string>();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        DataObject objectDefinition = _databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());

        if (objectDefinition == null)
        {
          throw new Exception("Object type [" + objectType + "] not found.");
        }

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectType + ", " + _settings["ExecutingAssemblyName"]);

        // make an exception for tests
        if (type == null)
        {
          type = Type.GetType(ns + objectType + ", NUnit.Tests");
        }

        ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, objectDefinition, filter);

        if (pageSize == 0 && startIndex == 0)
        {
          List<IDataObject> dataObjects = new List<IDataObject>();
          long totalCount = GetCountSP3D(objectType, filter);
          int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 1000;
          int numOfRows = 0;

          while (numOfRows < totalCount)
          {
            criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
            dataObjects.AddRange(criteria.List<IDataObject>());
            numOfRows += internalPageSize;
          }

          return dataObjects;
        }
        else
        {
          criteria.SetFirstResult(startIndex).SetMaxResults(pageSize);
          IList<IDataObject> dataObjects = criteria.List<IDataObject>();
          return dataObjects;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }   

    public long GetCountSP3D(string objectType, DataFilter filter)
    {

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];

          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select count(*) from " + objectType);

        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          DataFilter clonedFilter = Utility.CloneDataContractObject<DataFilter>(filter);
          clonedFilter.OrderExpressions = null;
          DataObject dataObject = _databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = clonedFilter.ToSqlWhereClause(_databaseDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        long count = query.List<long>().First();
        return count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    #region private functions for sp3dprovider
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
      PropertyInformation propertyInfo = null;
      InterfaceInformation interfaceInfo = null;
      ClassInformation classInfo = null;

      classInfo = GetClassInformation(robj.objectName);

      if (classInfo == null)
        throw new Exception("class [" + robj.objectName + "] does not exist in SP3D databases.");

      if (robj.businessKeyProperties == null)
        robj.businessKeyProperties = new List<BusinessKeyProperty>();

      if (robj.rightClassNames == null)
        robj.rightClassNames = new List<string>();

      if (robj.leftClassNames == null)
        robj.leftClassNames = new List<string>();

      robj.objectNamespace = rootObject.objectNamespace;
      robj.rowNumber = -1;

      if (robj.relatedObjects != null && robj.relatedObjects.Count > 0)
        robj.nodeType = NodeType.MiddleObject;
      else
        robj.nodeType = NodeType.EndObject;

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
            interfaceInfo = GetInterfaceInformation(classInfo, businessInterface.interfaceName);
            if (interfaceInfo == null)
              throw new Exception("Interface [" + businessInterface.interfaceName + "] for class [" + robj.objectName + "] does not exist in SP3D databases.");

            if (businessInterface.businessProperties != null)
              if (businessInterface.businessProperties.Count > 0)
                foreach (BusinessProperty businessProperty in businessInterface.businessProperties)
                {
                  propertyInfo = GetPropertyInfo(interfaceInfo, businessProperty.propertyName);

                  //if (propertyInfo == null)
                  //  throw new Exception("Property [" + businessProperty.propertyName + "] for interface [" + businessInterface.interfaceName + "] does not exist in SP3D databases.");

                  businessProperty.dataType = GetDatatype(businessProperty.datatype);
                  businessProperty.datatype = businessProperty.dataType.ToString();
                  businessProperty.codeList = propertyInfo.CodeListInfo.CodelistMembers;

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
      //if (!relation.unique)  --add finding relation info from originRelationName

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

        if (newPareendObj.relatedObjects != null && newPareendObj.relatedObjects.Count > 0)
          traverseRelatedObjects(newPareendObj, newPareendObj.relatedObjects, businessObject);
        else
          newPareendObj.nodeType = NodeType.EndObject;
      }
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

    private IList<IDataObject> getDataObjectRows(BusinessCommodity bo, string objectName, string commodityName, DataFilter filter, DataDictionary dataDictionary, DatabaseDictionary databaseDictionary)
    {
      ISession session = null;
      long totalCount = 0;
      string propertyName = string.Empty;
      List<IDataObject> dataObjects = new List<IDataObject>();
      List<IDataObject> filteredDataObjects = new List<IDataObject>();
      string rowPropertyName = string.Empty;
      string oidOrigin = string.Empty;

      BusinessObject bObj = bo.GetBusinessObject(objectName);
      DataObject objectDefinition = dataDictionary.GetDataObject(objectName);
      string key = bObj.businessKeyProperties.First().columnName;

      try
      {
        session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

        if (databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = databaseDictionary.IdentityConfiguration[commodityName];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectName, filter, identityProperties);
          }
        }

        if (objectDefinition == null)
        {
          throw new Exception("Object type [" + commodityName + "." + objectName + "] is not found.");
        }

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectName + ", " + _settings["ExecutingAssemblyName"]);

        // make an exception for tests
        if (type == null)
        {
          type = Type.GetType(ns + objectName + ", NUnit.Tests");
        }

        ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, objectDefinition, filter);

        totalCount = GetSP3DCount(commodityName, filter, dataDictionary, databaseDictionary);
        int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 99999999;
        //int internalPageSize = 50;
        int numOfRows = 0;

        while (numOfRows < totalCount)
        //while (numOfRows < 50)
        {
          criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
          dataObjects.AddRange(criteria.List<IDataObject>());
          numOfRows += internalPageSize;
        }

        foreach (IDataObject row in dataObjects)
        {
          if (_filtertedKeys.Contains(row.GetPropertyValue(key).ToString().ToLower()))
          {
            filteredDataObjects.Add(row);
            foreach (string connectedEntityName in bObj.rightClassNames)
            {
              BusinessRelation relation = bObj.GetRelation(connectedEntityName);
              oidOrigin = row.GetPropertyValue(relation.relationName + "_" + relation.businessKeyProperties.First().columnName).ToString();

              foreach (string con in relation.rightClassNames)
              {
                RelatedObject relatedObject = bObj.GetRelatedObject(con);
                setRow(row, oidOrigin, relatedObject, bObj);
              }
            }
          }
        }

        return filteredDataObjects;
        //return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}.{1}]. {2}", commodityName, objectName, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    private void GetSP3DFilteredKeys(BusinessCommodity bco)
    {
      FilterBase filterBase = FindFilter(bco.businessFilter.filterName);
      System.Collections.ObjectModel.ReadOnlyCollection<Ingr.SP3D.Common.Middle.BusinessObject> filteredObjects = filterBase.Apply();
      _filtertedKeys = new List<string>();
      string temp = string.Empty;
      int i = 0;  // debugging purpose

      foreach (Ingr.SP3D.Common.Middle.BusinessObject businessObj in filteredObjects)
      {
        if (i > 10)  // debugging purpose
          break;     // debugging purpose

        _filtertedKeys.Add(businessObj.ObjectID.Substring(1, businessObj.ObjectID.Length - 2).ToLower());
        i++;   // debugging purpose
      }

      //Utility.Write<List<string>>(_filtertedKeys, "C:\\temp\\filteredOids.txt"); // debugging purpose
    }

    private void setRow(IDataObject targetRow, string oidOrigin, RelatedObject relatedObject, BusinessObject bObj)
    {
      string newOidOrigin = string.Empty;
      string relationKey = bObj.GetRelation(relatedObject.relationName).businessKeyProperties.First().columnName;

      foreach (IDataObject row1 in _sourceDataObjects[relatedObject.objectName])
      {
        if (row1.GetPropertyValue(relatedObject.businessKeyProperties.First().columnName).ToString().ToLower() == oidOrigin.ToLower())
        {
          if (row1.GetPropertyValue(relationKey) != null)
            newOidOrigin = row1.GetPropertyValue(relationKey).ToString();

          foreach (BusinessProperty bp in relatedObject.businessProperties)
          {
            targetRow.SetPropertyValue(bp.propertyName, row1.GetPropertyValue(bp.propertyName));
          }
          break;
        }
      }

      if (relatedObject.relatedObjects != null && !string.IsNullOrEmpty(newOidOrigin))
      {
        if (relatedObject.relatedObjects.Count > 0)
        {
          foreach (RelatedObject newRelatedObject in relatedObject.relatedObjects)
          {
            setRow(targetRow, newOidOrigin, newRelatedObject, bObj);
          }
        }
      }
      else
        return;
    }

    private void getSourceDataObjects(BusinessCommodity bco, DataDictionary dataDictionary, DatabaseDictionary databaseDictionary)
    {
      ISession session = null;      
      DataObject loopDobj = null;
      string propertyName = string.Empty;
      List<IDataObject> dataObjects = null;
      string commodityName = bco.commodityName, objectName = string.Empty;
      
      long totalCount = GetSP3DCount(commodityName, null, dataDictionary, databaseDictionary);

      if (_sourceDataObjects == null)
        _sourceDataObjects = new Dictionary<string, IList<IDataObject>>();

      BusinessObject mainBObj = bco.GetBusinessObject(dataDictionary.dataObjects.First().objectName);

      try
      {
        session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

        for (int i = 1; i < dataDictionary.dataObjects.Count; i++)
        {
          loopDobj = dataDictionary.dataObjects[i];

          if (mainBObj.GetRelation(loopDobj.objectName) == null)
          {
            if (loopDobj == null)
            {
              throw new Exception("Object type [" + commodityName + "." + objectName + "] is not found.");
            }

            objectName = loopDobj.objectName;
            string ns = String.IsNullOrEmpty(loopDobj.objectNamespace)
              ? String.Empty : (loopDobj.objectNamespace + ".");

            Type type = Type.GetType(ns + objectName + ", " + _settings["ExecutingAssemblyName"]);

            // make an exception for tests
            if (type == null)
            {
              type = Type.GetType(ns + objectName + ", NUnit.Tests");
            }

            ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, loopDobj, null);

            dataObjects = new List<IDataObject>();
            int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 2000;
            int numOfRows = 0;

            while (numOfRows < totalCount)
            {
              criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
              dataObjects.AddRange(criteria.List<IDataObject>());
              numOfRows += internalPageSize;
            }
            _sourceDataObjects.Add(objectName, dataObjects);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}.{1}]. {2}", commodityName, objectName, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    private string GetDataSource(string connectionString)
    {
      string parsedConnStr = string.Empty, dbServer = string.Empty, dataBase = string.Empty;
      char[] ch = { ';' };
      string[] connStrKeyValuePairs = connectionString.Split(ch, StringSplitOptions.RemoveEmptyEntries);

      foreach (string connStrKeyValuePair in connStrKeyValuePairs)
      {
        string[] connStrKeyValuePairTemp = connStrKeyValuePair.Split('=');
        string connStrKey = connStrKeyValuePairTemp[0].Trim();
        string connStrValue = connStrKeyValuePairTemp[1].Trim();

        if (connStrKey.ToUpper() == "DATA SOURCE")
        {
          return connStrValue;            
        }        
      }
      return null;
    }

    private string GetCachingConnectionString(string connStr, string dbProvider, string cachingDataBaseName)
    {
      try
      {
        string parsedConnStr = String.Empty;
        char[] ch = { ';' };
        string[] connStrKeyValuePairs = connStr.Split(ch, StringSplitOptions.RemoveEmptyEntries);

        foreach (string connStrKeyValuePair in connStrKeyValuePairs)
        {
          string[] connStrKeyValuePairTemp = connStrKeyValuePair.Split('=');
          string connStrKey = connStrKeyValuePairTemp[0].Trim();
          string connStrValue = connStrKeyValuePairTemp[1].Trim();

          switch (connStrKey.ToUpper())
          {
            case "DATA SOURCE":
            case "USER ID":
            case "PASSWORD":
            case "INTEGRATED SECURITY":
              parsedConnStr += connStrKey + "=" + connStrValue + ";";
              break;
            case "INITIAL CATALOG":
              parsedConnStr += connStrKey + "=" + cachingDataBaseName + ";";
              break;
          }                
        }

        return parsedConnStr;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in ParseConnectionString: {0}", ex));
        throw ex;
      }
    }

    private bool ModifiedConfig()
    {
      BusinessCommodity currentBC = null;

      getConfig();

      if (_config.businessCommodities.Count != _sp3dDataBaseDictionary.businessCommodities.Count)
      {
        return true;
      }

      foreach (BusinessCommodity bc in _config.businessCommodities)
      {
        if (_sp3dDataBaseDictionary.GetBusinessCommoditiy(bc.commodityName) == null)
        {
          return true;
        }

        currentBC = _sp3dDataBaseDictionary.GetBusinessCommoditiy(bc.commodityName);

        if (currentBC.businessObjects.Count != bc.businessObjects.Count)
          return true;

        foreach (BusinessObject bo in currentBC.businessObjects)
        {
          if (currentBC.GetBusinessObject(bo.objectName) == null)
          {
            return true;
          }
        }
      }

      return false;
    }

    private void getConfig()
    {
      if (!File.Exists(_configurationPath))
        throw new Exception("Configuration file [" + _configurationPath + "] does not exist.");

      _config = Utility.Read<BusinessObjectConfiguration>(_configurationPath);
    }
    # endregion 
  }
}

