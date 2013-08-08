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
//using Ingr.SP3D.Common.Client;
//using Ingr.SP3D.Common.Client.Services;
//using Ingr.SP3D.Structure.Middle;
//using Ingr.SP3D.ReferenceData.Middle;
//using Ingr.SP3D.Systems.Middle;
//using Ingr.SP3D.ReferenceData.Middle.Services;
using NHibernate;
using Ninject.Extensions.Xml;
using System.Collections.ObjectModel;
using System.Reflection;

namespace iringtools.sdk.sp3ddatalayer
{
  public class SP3DProvider
  {
    public static readonly ILog _logger = LogManager.GetLogger(typeof(SP3DProvider));
    public string _dataPath = string.Empty;
    public string _scope = string.Empty;
    public string _dictionaryPath = string.Empty;
    public string _databaseDictionaryPath = string.Empty;
    public string _communityName = string.Empty;
    public string _configurationPath = string.Empty;
    public string _verifiedConfigurationPath = string.Empty;
    public DatabaseDictionary _databaseDictionary = null;
    public DataDictionary _dataDictionary = null, _dataDictionaryOfBC = null;
    public Catalog SP3DCatalog = null;
    public Model SP3DModel = null;
    public Report SP3DReport = null;
    public MetadataManager metadataManagerReport = null, metadataManagerModel = null, metadataManagerCatalog = null;
    public BusinessObjectConfiguration _config = null, _sp3dDataBaseDictionary = null;
    public string projectNameSpace = null;
    public AdapterSettings _settings = null;
    public Dictionary<string, IList<IDataObject>> _sourceDataObjects = null;
    public List<string> _filtertedKeys = null;

    //public static void main()
    //{
    //    AppDomain currentDomain = AppDomain.CurrentDomain;       
        
    //    //InstantiateMyType(currentDomain);    // Failed!

    //    currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);

    //   // InstantiateMyType(currentDomain);    // OK!
    //}

    public SP3DProvider(AdapterSettings settings)
    {
       
        //CommonMiddleLib.Class1 objcl1 = new CommonMiddleLib.Class1();
        //AppDomain currentDomain = AppDomain.CurrentDomain;
        //currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);
        
      _settings = settings;

      if (_settings["DataLayerPath"] != null)
        _dataPath = _settings["DataLayerPath"];
      else
        _dataPath = _settings["AppDataPath"];

      _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];
      _settings["BinaryPath"] = @".\Bin\";
      //_settings["BinaryPath"] = @"C:\Program Files (x86)\SmartPlant\3D\Core\Container\Bin\Assemblies\Release\";

      _configurationPath = string.Format("{0}Configuration.{1}.xml", _dataPath, _scope);
      _verifiedConfigurationPath = string.Format("{0}VerifiedConfiguration.{1}.xml", _dataPath, _scope);
      projectNameSpace = "org.iringtools.adapter.datalayer.proj_" + _scope;

      _dictionaryPath = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);
      _databaseDictionaryPath = string.Format("{0}DataBaseDictionary.{1}.xml", _dataPath, _scope);

      readDictionary();
      readDataBaseDictionary();
      readBusinessObjects();
    }

    //public Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    //{
    //    CommonMiddleLib.Class1 objcl1 = new CommonMiddleLib.Class1();
    //    return objcl1.MyResolveEventHandler(sender, args);
    //}

    public SP3DProvider(AdapterSettings settings, string purpose)
    {
        //CommonMiddleLib.Class1 objcl1 = new CommonMiddleLib.Class1();
        //objcl1.MyResolveEventHandler(null, null);
      _settings = new AdapterSettings();
      _settings.AppendSettings(settings);

      if (_settings["DataLayerPath"] != null)
        _dataPath = _settings["DataLayerPath"];
      else
        _dataPath = _settings["AppDataPath"];

      _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];
      _settings["scope"] = _scope;
     // _settings["BinaryPath"] = @".\Bin\Release\";
      _settings["BinaryPath"] = @".\Bin\Debug\";

#if DEBUG
      _settings["BinaryPath"] = @".\Bin\Debug\";
#endif
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

    public Response CreateCachingTables(string businessCommodityName, DataFilter filter)
    {
      Response response = new Response();
      List<IDataObject> dataObjects = new List<IDataObject>();
      CreateCachingDBTables(businessCommodityName, filter);

      if (string.IsNullOrEmpty(businessCommodityName))
        foreach (BusinessCommodity bc in _config.businessCommodities)
        {
          dataObjects.AddRange(GetSP3D(bc.commodityName, filter));
          response.Append(PostCachingDataObjects(dataObjects, bc.commodityName.ToLower()));
        }
      else
      {
        dataObjects.AddRange(GetSP3D(businessCommodityName, filter));
        response.Append(PostCachingDataObjects(dataObjects, businessCommodityName.ToLower()));
      }

      return response;
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

    public Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          string objectType = dataObjects[0].GetType().Name;

          foreach (IDataObject dataObject in dataObjects)
          {
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

    private bool hasPostHelp(List<PostSP3DHelp> postHelpList, string objectName)
    {
      foreach (PostSP3DHelp postHelp in postHelpList)
        if (postHelp.objectName.ToLower() == objectName.ToLower())
          return true;

      return false;
    }

    private List<PostSP3DHelp> getPostHelpList(IList<string> dIdentifiers, string commodityName)
    {
      string objectName = string.Empty;
      BusinessCommodity businessCommodity = null;
      PostSP3DHelp postHelp = null;
      List<PostSP3DHelp> postHelpList = new List<PostSP3DHelp>();

      _dataDictionaryOfBC = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _dataPath, _scope, commodityName));

      if (_sp3dDataBaseDictionary.GetBusinessCommoditiy(commodityName) != null)
      {
        businessCommodity = _sp3dDataBaseDictionary.GetBusinessCommoditiy(commodityName);        
      }

      foreach (string identifier in dIdentifiers)
      {
        foreach (BusinessObject bo in businessCommodity.businessObjects)
        {
          objectName = bo.objectName;

          if (!hasPostHelp(postHelpList, objectName))
          {
            postHelp = new PostSP3DHelp();
            postHelp.objectName = objectName;
            postHelp.dataObject = _dataDictionaryOfBC.dataObjects.First(c => c.objectName.ToUpper() == objectName.ToUpper());
            postHelpList.Add(postHelp);
          }
          else
          {
            postHelp = postHelpList.FirstOrDefault<PostSP3DHelp>(o => o.objectName.ToLower() == objectName.ToLower());
          }

          postHelp.updateIdentifiers.Add(identifier);
        }
      }

      return postHelpList;
    }

    private List<PostSP3DHelp> getPostHelpList(IList<IDataObject> dataObjects, string commodityNameLower, string commodityName)
    {
      string objectName = string.Empty, identifier = string.Empty;
      bool multipleSP3DClasses = false;
      BusinessCommodity businessCommodity = null;
      PostSP3DHelp postHelp = null;

      List<PostSP3DHelp> postHelpList = new List<PostSP3DHelp>();

      _dataDictionaryOfBC = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _dataPath, _scope, commodityNameLower));

      if (_sp3dDataBaseDictionary.GetBusinessCommoditiy(commodityName) != null)
      {
        businessCommodity = _sp3dDataBaseDictionary.GetBusinessCommoditiy(commodityName);
        if (businessCommodity.businessObjects.Count > 1)
          multipleSP3DClasses = true;
      }

      foreach (IDataObject dataObject in dataObjects)
      {
        if (multipleSP3DClasses)
        {
          objectName = dataObject.GetPropertyValue("className").ToString();
        }
        else
        {
          objectName = businessCommodity.businessObjects.First().objectName;
        }

        if (!hasPostHelp(postHelpList, objectName))
        {
          postHelp = new PostSP3DHelp();
          postHelp.objectName = objectName;
          postHelp.dataObject = _dataDictionaryOfBC.dataObjects.First(c => c.objectName.ToUpper() == objectName.ToUpper());
          postHelp.businessObject = businessCommodity.GetBusinessObject(objectName);
          postHelpList.Add(postHelp);
        }
        else
        {
          postHelp = postHelpList.FirstOrDefault<PostSP3DHelp>(o => o.objectName.ToLower() == objectName.ToLower());
        }

        identifier = dataObject.GetPropertyValue("Id").ToString();
        postHelp.updateIdentifiers.Add(identifier);
        postHelp.identiferNewDataObjectPairs.Add(identifier, dataObject);
      }

      return postHelpList;
    }

    public Response PostSP3DBusinessObjects(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      ISession session = null;
      List<PostSP3DHelp> postHelpList = null;
      string objectName = string.Empty, identifier = string.Empty, commodityName = string.Empty;      
      
      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          commodityName = dataObjects[0].GetType().Name;
          string commodityNameToLower = commodityName.ToLower();          
          postHelpList = getPostHelpList(dataObjects, commodityNameToLower, commodityName);

          session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], "posting_" + _settings["Scope"], commodityNameToLower);

          foreach (PostSP3DHelp pHelp in postHelpList)
          {
            pHelp.updateDataObjects = CreateSP3D(commodityName, pHelp);

            foreach (IDataObject pDataObject in pHelp.updateDataObjects)
            {
              Status status = new Status();
              status.Messages = new Messages();

              if (pDataObject != null)
              {
                try
                {
                  // NOTE: Id property is not available if it's not mapped and will cause exception
                  identifier = pDataObject.GetPropertyValue("Id").ToString();
                }
                catch (Exception ex)
                {
                  _logger.Error(string.Format("Error in Post: {0}", ex));
                }  // no need to handle exception because identifier is only used for statusing

                status.Identifier = identifier;

                try
                {
                  try
                  {
                    session.SaveOrUpdate(pDataObject);
                    session.Flush();
                    status.Messages.Add(string.Format("Record [{0}] inserted successfully.", identifier));
                  }
                  catch(Exception)
                  {
                    session.SaveOrUpdateCopy(pDataObject);
                    session.Flush();
                  }
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
                status.Messages.Add("Data object that are going to post is null. See log for details.");
              }

              response.Append(status);
            }
          }
        }
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

      return response;
    }

    public Response PostCachingDataObjects(IList<IDataObject> dataObjects, string commodityName)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], "sp3ddl_" + _settings["Scope"] + "." + commodityName);
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
        response.Append(CreateCachingTables(businessCommodityName, null));
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

      //Utility.Write<Response>(response, string.Format("{0}Response.{1}.xml", _dataPath, _scope));
      return response;
    }

    public Response RefreshCachingTable(string businessCommodityName, DataFilter dataFilter)
    {
      Response response = new Response();
      getConfig();
      response.Append(CreateCachingTables(businessCommodityName, dataFilter));
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
        generator.GenerateSP3D(compilerVersion, sp3dDataBaseDictionary, _databaseDictionary, projectName, applicationname, "posting");
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

          if (businessObject.businessProperties != null)
            foreach (BusinessProperty businessProperty in businessObject.businessProperties)
            {
              DataProperty dataProperty = new DataProperty();
              propertyName = businessProperty.propertyName;
              dataProperty.propertyName = propertyName;
              dataProperty.dataType = DataType.String;
              dataProperty.columnName = propertyName;
              dataProperty.isNullable = true;
              dataProperty.isHidden = businessProperty.hidden;
              businessProperty.isNative = true;
              businessProperty.inClass = true;

              if (!dataObject.dataProperties.Contains(dataProperty))
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
              dataProperty.isNullable = false;
              dataProperty.keyType = KeyType.assigned;
              dataProperty.isHidden = businessKeyProerpty.hidden;

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
                dataProperty.isReadOnly = businessProperty.isReadOnly;
                dataProperty.isHidden = businessProperty.hidden;
                businessProperty.isNative = false;
                businessProperty.inClass = true;

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

              if (relatedObject.businessProperties != null)
                if (relatedObject.businessProperties.Count > 0)
                  foreach (BusinessProperty businessProperty in relatedObject.businessProperties)
                  {
                    DataProperty dataProperty = new DataProperty();
                    propertyName = businessProperty.propertyName;
                    dataProperty.propertyName = propertyName;
                    dataProperty.dataType = DataType.String;
                    dataProperty.columnName = propertyName;
                    dataProperty.isNullable = true;
                    dataProperty.isHidden = businessProperty.hidden;
                    businessProperty.isNative = false;
                    businessProperty.inClass = false;

                    if (!dataObject.dataProperties.Contains(dataProperty))
                      dataObject.dataProperties.Add(dataProperty);
                  }

              if (relatedObject.businessKeyProperties != null)
                if (relatedObject.businessKeyProperties.Count > 0)
                  foreach (BusinessKeyProperty businessKeyProerpty in relatedObject.businessKeyProperties)
                  {
                    DataProperty dataProperty = new DataProperty();
                    BusinessProperty businessp = businessKeyProerpty.convertKeyPropertyToProperty();
                    BusinessProperty bbusinessp = businessp.copyBusinessProperty();
                    dataProperty.propertyName = relatedObjectName + "_" + businessKeyProerpty.keyPropertyName;
                    businessKeyProerpty.keyPropertyName = dataProperty.propertyName;
                    dataProperty.columnName = dataProperty.propertyName;
                    businessp.propertyName = dataProperty.propertyName;
                    bbusinessp.propertyName = dataProperty.propertyName;
                    bbusinessp.columnName = dataProperty.propertyName;
                    dataProperty.dataType = DataType.String;
                    dataProperty.isNullable = false;
                    dataProperty.isHidden = businessKeyProerpty.hidden;
                    bbusinessp.isNative = false;
                    bbusinessp.inClass = false;

                    if (!businessObject.businessProperties.Contains(businessp))
                      businessObject.businessProperties.Add(bbusinessp);

                    if (!relatedObject.businessProperties.Contains(businessp))
                      relatedObject.businessProperties.Add(businessp);

                    if (!dataObject.dataProperties.Contains(dataProperty))
                      dataObject.dataProperties.Add(dataProperty);
                  }

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
                    businessRelationProperty.inClass = false;
                    dataProperty.dataType = businessRelationProperty.dataType;
                    dataProperty.columnName = dataProperty.propertyName;
                    dataProperty.isNullable = businessRelationProperty.isNullable;
                    dataProperty.isHidden = businessRelationProperty.hidden;
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

      Utility.Write<BusinessObjectConfiguration>(config, _verifiedConfigurationPath);
      return config;
    }

    public Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = Create(objectType, identifiers);

        foreach (IDataObject dataObject in dataObjects)
        {
          string identifier = dataObject.GetPropertyValue("Id").ToString();          
          session.Delete(dataObject);
          session.Flush();
          Status status = new Status();
          status.Messages = new Messages();
          status.Identifier = identifier;
          status.Messages.Add(string.Format("Record [{0}] deleted successfully.", identifier));
          response.Append(status);
        }        
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

    public Response DeleteSP3DIdentifiers(string commodityName, IList<string> identifiers)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName.ToLower());
      DataProperty dataProp = new DataProperty();
      dataProp.propertyName = "className";      
   
      try
      {
        if (identifiers != null && identifiers.Count > 0)
        {
          List<PostSP3DHelp> postHelpList = getPostHelpList(identifiers, commodityName);

          foreach (PostSP3DHelp pHelp in postHelpList)
          {
            pHelp.updateDataObjects = CreateSP3D(commodityName, pHelp.objectName, pHelp.updateIdentifiers, pHelp.dataObject);

            foreach (IDataObject pDataObject in pHelp.updateDataObjects)
            {
              string identifier = pDataObject.GetPropertyValue("Id").ToString();              
              session.Delete(pDataObject);
              session.Flush();
              Status status = new Status();
              status.Messages = new Messages();
              status.Identifier = identifier;
              status.Messages.Add(string.Format("Record [{0}] deleted successfully.", identifier));
              response.Append(status);
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error while deleting data objects of type [{0}]. {1}", commodityName, ex));
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
      string commodityNameLower = objectType.ToLower();
      ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityNameLower);
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
        databaseDictionarySP3D = Utility.Read<DatabaseDictionary>(string.Format("{0}DatabaseDictionary.{1}.{2}.xml", _dataPath, _scope, commodityNameLower));

        foreach (BusinessObject bo in bc.businessObjects)
        {
          StringBuilder queryString = new StringBuilder();
          queryString.Append("from " + bo.objectName);

          if (filter.Expressions.Count > 0)
          {
            //DataObject dataObject = _databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
            string whereClause = filter.ToSqlWhereClause(databaseDictionarySP3D, bo.tableName, String.Empty);
            queryString.Append(whereClause);
          }

          session.Delete(queryString.ToString());
          session.Flush();
          status.Messages.Add(string.Format("Records of type [{0}] deleted succesfully.", bo.objectName));
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

              if (businessObject.businessProperties != null)
                if (businessObject.businessProperties.Count > 0)
                  foreach (BusinessProperty businessProperty in businessObject.businessProperties)
                  {
                    businessProperty.dataType = GetDatatype(businessProperty.datatype);
                    businessProperty.datatype = businessProperty.dataType.ToString();
                    businessProperty.isNullable = true;
                    if (businessProperty.propertyName.ToLower() == "name")
                      businessProperty.columnName = "ItemName";
                    else if (string.IsNullOrEmpty(businessProperty.columnName))
                      businessProperty.columnName = businessProperty.propertyName;
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
                          businessProperty.isNullable = true;
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
        }
      }

      return filter;
    }

    public Plant Connect(BusinessObjectConfiguration config)
    {
      Site SP3DSite = null;

      if (MiddleServiceProvider.SiteMgr.ActiveSite != null)
          if (MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant != null)
          {
              //SP3DCatalog = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantCatalog;
              //SP3DModel = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantModel;
              //SP3DReport = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantReport;
              //metadataManagerCatalog = SP3DCatalog.MetadataMgr;
              //metadataManagerReport = SP3DReport.MetadataMgr;
              //metadataManagerModel = SP3DModel.MetadataMgr; 
              return MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant;
          }

          SP3DSite = MiddleServiceProvider.SiteMgr.ConnectSite(GetDataSource(config.ConnectionString), config.SiteDataBaseName, (config.Provider.ToUpper().Contains("MSSQL") ? SiteManager.eDBProviderTypes.MSSQL : SiteManager.eDBProviderTypes.Oracle), config.SiteDataBaseName + "_SCHEMA");
          Plant SP3DPlant = null;

          if (SP3DSite != null)
          {
              if (SP3DSite.Plants.Count > 0)
              {
                  if (!string.IsNullOrEmpty(config.PlantName))
                      SP3DPlant = SP3DSite.Plants.FirstOrDefault<Plant>(o => o.Name.ToLower() == config.PlantName.ToLower());
                  else
                      SP3DPlant = (Plant)SP3DSite.Plants[0];
                     // SP3DPlant = (Plant)SP3DSite.Plants[7];

                  MiddleServiceProvider.SiteMgr.ActiveSite.OpenPlant(SP3DPlant);
              }
          }

      SP3DCatalog = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantCatalog;
      SP3DModel = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantModel;
      SP3DReport = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantReport;
      metadataManagerCatalog = SP3DCatalog.MetadataMgr;
      metadataManagerReport = SP3DReport.MetadataMgr;
      metadataManagerModel = SP3DModel.MetadataMgr;      
      return SP3DPlant;
    }

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
        //GetSP3DFilteredKeys(businessCommodity);

        dataObjects = new List<IDataObject>();
        DataDictionary dataDictionary = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _dataPath, _scope, commodityName));
        DatabaseDictionary databaseDictionary = Utility.Read<DatabaseDictionary>(string.Format("{0}DatabaseDictionary.{1}.{2}.xml", _dataPath, _scope, commodityName));

        getSourceDataObjects(commodityName, businessCommodity, dataDictionary, databaseDictionary);

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

    public long GetSP3DCount(string objectType, DataFilter filter, DataDictionary dataDictionary, DatabaseDictionary databaseDictionary, BusinessCommodity businessCommodity)
    {
      long totalCount = 0, objectRowNumber = 0;
      string objectName = string.Empty;
      BusinessObject businessObject = null;

      try
      {
        if (_sp3dDataBaseDictionary != null)
          if (_sp3dDataBaseDictionary.businessCommodities != null)
          {
            if (dataDictionary == null)
              dataDictionary = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _settings["AppDataPath"], _settings["Scope"], objectType));

            DataObject dataObject = dataDictionary.dataObjects.First();

            objectName = dataObject.objectName;

            if (businessCommodity.GetBusinessObject(objectName) != null)
            {
              businessObject = businessCommodity.GetBusinessObject(objectName);
              objectRowNumber = GetCountSP3D(dataObject.objectName, filter, objectType, dataDictionary, databaseDictionary);
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
          long totalCount = GetCount(objectType, filter);
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

    public long GetCount(string objectType, DataFilter filter)
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

    
    //session: sp3dSession, objectName: equipment, type: HasEqpAsChild, identifier: oid
    private IDataObject CreateIDataObject(ISession session, string objectName, Type type, string identifier)
    {         
      if (!String.IsNullOrEmpty(identifier))
      {
        IQuery query = session.CreateQuery("from " + objectName + " where Id = ?");
        query.SetString(0, identifier);
        IDataObject dataObject = null;
        int count = query.List<IDataObject>().Count;
        if (count >= 1)
          dataObject = query.List<IDataObject>().First();

        if (dataObject == null)
        {
          dataObject = (IDataObject)Activator.CreateInstance(type);
          dataObject.SetPropertyValue("Id", identifier);
        }
        return dataObject;
      }
      return null;
    }

    private void SetPostDataObject(ISession session, IList<IDataObject> dataObjects, IDataObject sourceDO, IDataObject createdDO, BusinessObject bObj)
    {
      string relationName = string.Empty, ns = string.Empty, identifier = string.Empty, keyStr = string.Empty, parentKeyStr = string.Empty;
      DataObject objectDefinition = null;
      BusinessRelation relation = null;
      Type type = null;
      RelatedObject relatedObject = null;
      IDataObject relationDataObject = null, relatedDataObject = null;
      string childId = string.Empty;
      setPostDOValue(createdDO, bObj, sourceDO);

      if (bObj.rightClassNames != null)
        foreach (string connectedEntityName in bObj.rightClassNames)
        {
          objectDefinition = _dataDictionaryOfBC.GetDataObject(connectedEntityName);
          relation = bObj.GetRelation(connectedEntityName);

          ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

          type = Type.GetType(ns + connectedEntityName + ", " + _settings["ExecutingAssemblyName"]);
  
          if (relation.rightClassNames != null)
            foreach (string con in relation.rightClassNames)
            {
              relatedObject = bObj.GetRelatedObject(con);    
              keyStr = relatedObject.businessKeyProperties.First().keyPropertyName;
              identifier = sourceDO.GetPropertyValue(keyStr).ToString();
              //relationDataObject = CreateIDataObject(session, connectedEntityName, type, identifier);              
              //relationDataObject.SetPropertyValue(relation.businessProperties.Last().propertyName, identifier);
              //createdDO.SetPropertyValue(connectedEntityName, relationDataObject);

              type = Type.GetType(ns + con + ", " + _settings["ExecutingAssemblyName"]);
              relatedDataObject = CreateIDataObject(session, con, type, identifier);
              setPostDOValue(relatedDataObject, relatedObject, sourceDO);
              dataObjects.Add(relatedDataObject);

              setPostDO(session, dataObjects, sourceDO, relatedObject, bObj, relatedDataObject);
            }          
        }

      dataObjects.Add(createdDO);
    }

    private void setPostDOValue(IDataObject cdo, BusinessObject ro, IDataObject sdo)
    {
      string value = string.Empty;
      foreach (BusinessProperty bp in ro.businessProperties)
      {
        value = sdo.GetPropertyValue(bp.propertyName).ToString();
        cdo.SetPropertyValue(bp.propertyName, value);
      }
    }

    private void setPostDOValue(IDataObject cdo, RelatedObject ro, IDataObject sdo)
    {
      string value = string.Empty;
      foreach (BusinessProperty bp in ro.businessProperties)
      {
        value = sdo.GetPropertyValue(bp.propertyName).ToString();
        cdo.SetPropertyValue(bp.propertyName, value);
      }
    }

    private void setPostDO(ISession session, IList<IDataObject> dataObjects, IDataObject sourceDO, RelatedObject relatedObject, BusinessObject bObj, IDataObject treatingDo)
    {
      string ns = string.Empty, identifier = string.Empty, keyStr = string.Empty;
      DataObject objectDefinition = null;
      BusinessRelation relation = null;
      Type type = null;
      IDataObject relatedDataObject = null;
      string relationName = relatedObject.relationName;
      relation = bObj.GetRelation(relationName);
      
      if (relatedObject.rightClassNames != null)
        foreach (string connectedEntityName in relatedObject.rightClassNames)
        {
          objectDefinition = _dataDictionaryOfBC.GetDataObject(connectedEntityName);
          relation = bObj.GetRelation(connectedEntityName);

          ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

          type = Type.GetType(ns + connectedEntityName + ", " + _settings["ExecutingAssemblyName"]);

          if (relation.rightClassNames != null)
            foreach (string con in relation.rightClassNames)
            {
              relatedObject = bObj.GetRelatedObject(con);
              //relationDataObject = CreateIDataObject(session, connectedEntityName, type, parentOid);
              keyStr = relatedObject.businessKeyProperties.First().keyPropertyName;
              identifier = sourceDO.GetPropertyValue(keyStr).ToString();
              //relationDataObject.SetPropertyValue(relation.businessProperties.Last().propertyName, identifier);
              //treatingDo.SetPropertyValue(connectedEntityName, relationDataObject);

              type = Type.GetType(ns + con + ", " + _settings["ExecutingAssemblyName"]);
              relatedDataObject = CreateIDataObject(session, connectedEntityName, type, identifier);
              setPostDOValue(relatedDataObject, relatedObject, sourceDO);
              dataObjects.Add(relatedDataObject);

              setPostDO(session, dataObjects, sourceDO, relatedObject, bObj, relatedDataObject);
            }
        } 
    }

    private IList<IDataObject> CreateSP3D(string objectType, string objectName, List<string> identifiers, DataObject objectDefinition)
    {
      string commodityName = objectType.ToLower();
      ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

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
              IQuery query = session.CreateQuery("from " + objectName + " where Id = ?");
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

    private IList<IDataObject> CreateSP3D(string objectType, PostSP3DHelp pHelp)
    {
      string commodityName = objectType.ToLower();      
      ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], "posting_" + _settings["Scope"], commodityName);

      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        string ns = String.IsNullOrEmpty(pHelp.dataObject.objectNamespace)
          ? String.Empty : (pHelp.dataObject.objectNamespace + ".");

        Type type = Type.GetType(ns + pHelp.dataObject.objectName + ", " + _settings["ExecutingAssemblyName"]);
        IDataObject dataObject = null;

        if (pHelp.updateIdentifiers != null)
        {
          foreach (string identifier in pHelp.updateIdentifiers)
          {
            if (!String.IsNullOrEmpty(identifier))
            {
              IQuery query = session.CreateQuery("from " + pHelp.objectName + " where Id = ?");
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

            SetPostDataObject(session, dataObjects, pHelp.identiferNewDataObjectPairs[identifier], dataObject, pHelp.businessObject);
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
                  businessProperty.isNullable = true;

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

        totalCount = GetSP3DCount(commodityName, filter, dataDictionary, databaseDictionary, bo);
        //int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 99999999;
        int internalPageSize = 50;
        int numOfRows = 0;

        //while (numOfRows < totalCount)
        while (numOfRows < 50)
        {
          criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
          dataObjects.AddRange(criteria.List<IDataObject>());
          numOfRows += internalPageSize;
        }

        foreach (IDataObject row in dataObjects)
        {
          //if (_filtertedKeys.Contains(row.GetPropertyValue(key).ToString().ToLower()))
          //{
          filteredDataObjects.Add(row);
          foreach (string connectedEntityName in bObj.rightClassNames)
          {
            BusinessRelation relation = bObj.GetRelation(connectedEntityName);
            oidOrigin = row.GetPropertyValue(relation.relationName + "_" + relation.businessKeyProperties.First().columnName).ToString();
            
            foreach (string con in relation.rightClassNames)
            {
              RelatedObject relatedObject = bObj.GetRelatedObject(con);
              row.SetPropertyValue(relatedObject.businessKeyProperties.First().keyPropertyName, oidOrigin);
              setRow(row, oidOrigin, relatedObject, bObj);
            }
          }
          //}
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
      string relationKey = bObj.GetRelation(relatedObject.relationName).businessKeyProperties.First().keyPropertyName;

      foreach (IDataObject row1 in _sourceDataObjects[relatedObject.objectName])
      {
        if (row1.GetPropertyValue(relatedObject.businessKeyProperties.First().keyPropertyName).ToString().ToLower() == oidOrigin.ToLower())
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

    private void getSourceDataObjects(string lowerCasecommodityName, BusinessCommodity bco, DataDictionary dataDictionary, DatabaseDictionary databaseDictionary)
    {
      ISession session = null;
      DataObject loopDobj = null;
      string propertyName = string.Empty;
      List<IDataObject> dataObjects = null;
      string commodityName = bco.commodityName, objectName = string.Empty;

      if (_sourceDataObjects == null)
        _sourceDataObjects = new Dictionary<string, IList<IDataObject>>();
      else
        return;

      long totalCount = GetSP3DCount(lowerCasecommodityName, null, dataDictionary, databaseDictionary, bco);

      try
      {
        session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

        foreach (BusinessObject mainBObj in bco.businessObjects)
        {
          for (int i = 0; i < dataDictionary.dataObjects.Count; i++)
          {
            loopDobj = dataDictionary.dataObjects[i];

            //get related objects from datadictionary
            if (mainBObj.objectName.ToLower() != loopDobj.objectName.ToLower() && mainBObj.GetRelation(loopDobj.objectName) == null)
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

      if (_sp3dDataBaseDictionary == null)
        return true;

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

