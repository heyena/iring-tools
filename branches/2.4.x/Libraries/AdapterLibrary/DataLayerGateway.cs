using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using log4net;
using Ninject;
using org.iringtools.library;
using org.iringtools.utility;
using System.Xml;

namespace org.iringtools.adapter
{
  //
  // this class determines the appropriate data layer (cache, actual, or both) to call
  //
  public class DataLayerGateway
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataLayerGateway));

    private const string NO_CACHE_ERROR = "Lightweight data layer requires cache to be built prior to this operation.";
    private const string PROP_SEPARATOR = ", ";
    private const string CACHE_ID_PREFIX = "c";

    private AdapterSettings _settings;
    private string _scope;
    private string _app;
    private string _dataPath;
    private string _connStr;
    private IDataLayer _dataLayer;
    private ILightweightDataLayer _lwDataLayer;

    public DataLayerGateway(IKernel kernel)
    {
      _settings = kernel.Get<AdapterSettings>();
      _scope = _settings["ProjectName"].ToLower();
      _app = _settings["ApplicationName"].ToLower();
      _dataPath = Path.Combine(_settings["BaseDirectoryPath"], _settings["AppDataPath"]);

      _connStr = _settings["iRINGCacheConnStr"];
      if (_connStr != null && IsBase64Encoded(_connStr))
      {
        _connStr = EncryptionUtility.Decrypt(_connStr);
      }

      string dlBindingPath = string.Format("{0}BindingConfiguration.{1}.{2}.xml", _dataPath, _scope, _app);
      XElement dlBinding = null;

      if (File.Exists(dlBindingPath))
      {
        dlBinding = XElement.Load(dlBindingPath);
        kernel.Load(dlBindingPath);
      }
      else if (utility.Utility.isLdapConfigured && utility.Utility.FileExistInRepository<XElementClone>(dlBindingPath))
      {
        XElement bindingConfig = Utility.GetxElementObject(dlBindingPath);
        string fileName = Path.GetFileName(dlBindingPath);
        string tempPath = Path.GetTempPath() + fileName;
        bindingConfig.Save(tempPath);
        kernel.Load(tempPath);
      }

      if (dlBinding.Element("bind").Attribute("service").Value.Replace(" ", "").ToLower()
        == (typeof(IDataLayer).FullName + "," + typeof(IDataLayer).Assembly.GetName().Name).ToLower())
      {
        _dataLayer = kernel.Get<IDataLayer>();
      }
      else
      {
        _lwDataLayer = kernel.Get<ILightweightDataLayer>();
      }
    }

    public DataDictionary GetDictionary()
    {
      try
      {
        return GetDictionary(false);
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data dictionary: " + e);
        throw e;
      }
    }

    public DataDictionary GetDictionary(bool refresh)
    {
      try
      {
        return GetDictionary(refresh, null);
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data dictionary: " + e);
        throw e;
      }
    }

    public DataDictionary GetDictionary(bool refresh, string objectType)
    {
      try
      {
        DataFilter filter = null;
        return GetDictionary(refresh, objectType, out filter);
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data dictionary: " + e);
        throw e;
      }
    }

    public DataDictionary GetDictionary(bool refresh, string objectType, out DataFilter filter)
    {
      DataDictionary dictionary = null;
      filter = null;
      DataFilter externalFilter = null;

      try
      {
        if (objectType != null)         //get the external filter file and append into the existing one ...
        {
          DirectoryInfo appDataDir = new DirectoryInfo(_dataPath);
          string filterFilePattern = String.Format("Filter.{0}.{1}.{2}.xml", _scope, _app, objectType);
          FileInfo[] filterFiles = appDataDir.GetFiles(filterFilePattern);

          foreach (FileInfo file in filterFiles)
          {
            externalFilter = Utility.Read<DataFilter>(file.FullName);
            if (filter == null)
              filter = externalFilter;
            else
              filter.AppendFilter(externalFilter);
            break;
          }
        }

        if (_lwDataLayer != null)
        {
          dictionary = _lwDataLayer.Dictionary(refresh, objectType, out filter);
        }
        else if (_dataLayer != null)
        {
          if (refresh)
          {
            if (!string.IsNullOrEmpty(objectType))
            {
              Response response = ((IDataLayer2)_dataLayer).Refresh(objectType);

              if (response.Level != StatusLevel.Success)
              {
                throw new Exception("Error refreshing dictionary for object type " +
                  objectType + ": " + response.Messages);
              }
            }
            else
            {
              Response response = ((IDataLayer2)_dataLayer).RefreshAll();

              if (response.Level != StatusLevel.Success)
              {
                throw new Exception("Error refreshing dictionary: " + response.Messages);
              }
            }
          }

          dictionary = _dataLayer.GetDictionary();
        }
        // Injecting the external filter in to the dictionary.
        if (dictionary != null)
        {
          if (objectType != null)
          {
            var obj = (from dataObjects in dictionary.dataObjects
                       where dataObjects.objectName.ToLower() == objectType.ToLower()
                       select dataObjects).First();

            if (externalFilter != null)
            {
              if (obj.dataFilter == null)
                obj.dataFilter = externalFilter;
              else
                obj.dataFilter.AppendFilter(externalFilter);
            }
          }
          else
          {
            // if objectType is not specified then pick all filter files for that scope and inject it into the dictionary.
            DirectoryInfo appDataDir = new DirectoryInfo(_dataPath);
            string filterFilePattern = String.Format("Filter.{0}.{1}.{2}.later.xml", _scope, _app, "*");
            FileInfo[] filterFiles = appDataDir.GetFiles(filterFilePattern);

            foreach (FileInfo file in filterFiles)
            {
              string fileName = Path.GetFileNameWithoutExtension(file.Name);
              string objectName = fileName.Substring(fileName.LastIndexOf('.') + 1);

              var obj = (from dataObjects in dictionary.dataObjects
                         where dataObjects.objectName.ToLower() == objectName.ToLower()
                         select dataObjects).First();

              if (obj != null)
              {
                if (obj.dataFilter == null)
                  obj.dataFilter = Utility.Read<DataFilter>(file.FullName);
                else
                  obj.dataFilter.AppendFilter(Utility.Read<DataFilter>(file.FullName));
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data dictionary: " + e);
        throw e;
      }

      return dictionary;
    }

    public Response RefreshCache(bool updateDictionary)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      string cacheId = string.Empty;

      try
      {
        DataDictionary dictionary = GetDictionary(updateDictionary);
        cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          SetCacheState(cacheId, CacheState.Busy);
        }
        else
        {
          cacheId = CreateCacheEntry();
        }

        foreach (DataObject objectType in dictionary.dataObjects)
        {
          Response objectTypeRefresh = RefreshCache(cacheId, objectType);
          response.Append(objectTypeRefresh);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error refreshing cache: " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }
      finally
      {
        SetCacheState(cacheId, CacheState.Ready);
      }

      return response;
    }

    public Response RefreshCache(bool updateDictionary, string objectType)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      string cacheId = string.Empty;

      try
      {
        DataDictionary dictionary = GetDictionary(updateDictionary, objectType);
        DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        if (dataObject == null)
        {
          throw new Exception("Object type " + objectType + " not found.");
        }

        cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          SetCacheState(cacheId, CacheState.Busy);
        }
        else
        {
          cacheId = CreateCacheEntry();
        }

        Response objectTypeRefresh = RefreshCache(cacheId, dataObject);
        response.Append(objectTypeRefresh);
        return response;
      }
      catch (Exception e)
      {
        string error = "Error refreshing cache for [" + objectType + "]: " + e.Message;
        _logger.Error(error);
        response.Level = StatusLevel.Error;
        response.Messages.Add(error);
        return response;
      }
      finally
      {
        SetCacheState(cacheId, CacheState.Ready);
      }
    }

    protected Response RefreshCache(string cacheId, DataObject objectType)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        //
        // create new cache table
        //
        DeleteCacheTable(cacheId, objectType);
        CreateCacheTable(cacheId, objectType);

        Status status = new Status()
        {
          Identifier = objectType.tableName,
          Level = StatusLevel.Success,
          Messages = new Messages() { "Cache table " + objectType.tableName + " created successfully." }
        };

        response.Append(status);

        //
        // populate cache data
        //
        string tableName = GetCacheTableName(cacheId, objectType.objectName);
        string tableSQL = "SELECT * FROM " + tableName + " WHERE 0=1";
        DataTable table = DBManager.Instance.ExecuteQuery(_connStr, tableSQL);

        if (_lwDataLayer != null)
        {
          IList<SerializableDataObject> dataObjects = _lwDataLayer.Get(objectType);

          if (dataObjects != null && dataObjects.Count > 0)
          {
            foreach (SerializableDataObject dataObj in dataObjects)
            {
              DataRow newRow = table.NewRow();

              foreach (var pair in dataObj.Dictionary)
              {
                newRow[pair.Key] = pair.Value;
              }

              if (dataObj.HasContent)
              {
                newRow[BaseLightweightDataLayer.HAS_CONTENT] = true;
              }

              table.Rows.Add(newRow);
            }

            SqlBulkCopy bulkCopy = new SqlBulkCopy(_connStr);
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.WriteToServer(table);
            status.Messages.Add("Cache data populated successfully.");
          }
        }
        else if (_dataLayer != null)
        {
          int page = 100;
          string cachePage = Convert.ToString(_settings["CachePage"]);

          if (!string.IsNullOrEmpty(cachePage))
          {
            page = int.Parse(cachePage);
          }

          long objCount = _dataLayer.GetCount(objectType.objectName, null);
          int start = 0;
          long limit = 0;

          while (start < objCount)
          {
            limit = (start + page < objCount) ? page : start + page - objCount;
            IList<IDataObject> dataObjects = _dataLayer.Get(objectType.objectName, null, (int)limit, start);

            if (dataObjects != null && dataObjects.Count > 0)
            {
              foreach (IDataObject dataObj in dataObjects)
              {
                DataRow newRow = table.NewRow();

                foreach (DataProperty prop in objectType.dataProperties)
                {
                  object value = dataObj.GetPropertyValue(prop.propertyName);

                  if (value == null)
                  {
                    value = DBNull.Value;
                  }

                  newRow[prop.propertyName] = value;
                }

                if (typeof(GenericDataObject).IsAssignableFrom(dataObj.GetType()))
                {
                  newRow[BaseLightweightDataLayer.HAS_CONTENT] = ((GenericDataObject)dataObj).HasContent;
                }

                table.Rows.Add(newRow);
              }
            }

            start += page;
          }

          SqlBulkCopy bulkCopy = new SqlBulkCopy(_connStr);
          bulkCopy.DestinationTableName = tableName;
          bulkCopy.WriteToServer(table);
          status.Messages.Add("Cache data populated successfully.");
        }

        return response;
      }
      catch (Exception e)
      {
        string error = "Error refreshing cache for object type " + objectType.objectName + ": " + e.Message;
        _logger.Error(error);
        response.Level = StatusLevel.Error;
        response.Messages.Add(error);
        return response;
      }
    }

    public Response ImportCache(string baseUri, bool updateDictionary)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      string cacheId = string.Empty;

      try
      {
        if (!baseUri.EndsWith("/")) baseUri += "/";

        DataDictionary dictionary = GetDictionary(updateDictionary);
        cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          SetCacheState(cacheId, CacheState.Busy);
        }
        else
        {
          cacheId = CreateCacheEntry();
        }

        foreach (DataObject objectType in dictionary.dataObjects)
        {
          string url = baseUri + objectType.objectName + ".dat";
          Response objectTypeRefresh = ImportCache(cacheId, objectType, url);
          response.Append(objectTypeRefresh);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error importing cache from " + baseUri + ": " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }
      finally
      {
        SetCacheState(cacheId, CacheState.Ready);
      }

      return response;
    }

    public Response ImportCache(string objectType, string url, bool updateDictionary)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      string cacheId = string.Empty;

      try
      {
        cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          SetCacheState(cacheId, CacheState.Busy);
        }
        else
        {
          cacheId = CreateCacheEntry();
        }

        DataDictionary dictionary = GetDictionary(updateDictionary, objectType);
        DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType);

        if (dataObject == null)
        {
          throw new Exception("Object type " + objectType + " not found.");
        }

        Response objectTypeImport = ImportCache(cacheId, dataObject, url);
        response.Append(objectTypeImport);
      }
      catch (Exception e)
      {
        _logger.Error("Error importing cache: " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }
      finally
      {
        SetCacheState(cacheId, CacheState.Ready);
      }

      return response;
    }

    protected Response ImportCache(string cacheId, DataObject objectType, string url)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        Status status = new Status()
        {
          Identifier = objectType.tableName,
          Level = StatusLevel.Success,
        };

        if (!url.ToLower().EndsWith(".dat"))
        {
          if (!url.EndsWith("/"))
          {
            url += "/";
          }

          url += objectType.objectName + ".dat";
        }

        WebHttpClient client = new WebHttpClient(url);
        Stream stream = client.GetStream(string.Empty);
        List<SerializableDataObject> dataObjects = BaseLightweightDataLayer.ReadDataObjects(stream);

        DeleteCacheTable(cacheId, objectType);
        CreateCacheTable(cacheId, objectType);

        //
        // populate cache data
        //
        string tableName = GetCacheTableName(cacheId, objectType.objectName);
        string tableSQL = "SELECT * FROM " + tableName + " WHERE 0=1";
        DataTable table = DBManager.Instance.ExecuteQuery(_connStr, tableSQL);

        if (dataObjects == null || dataObjects.Count == 0)
        {
          status.Level = StatusLevel.Warning;
          status.Messages.Add("Cached data is empty.");
        }
        else
        {
          foreach (SerializableDataObject dataObj in dataObjects)
          {
            if (dataObj.Type != null && dataObj.Type.ToLower() == objectType.objectName.ToLower())
            {
              DataRow newRow = table.NewRow();

              foreach (var pair in dataObj.Dictionary)
              {
                object value = pair.Value;
                if (value == null)
                {
                    value = DBNull.Value;
                }
                newRow[pair.Key] = value;
              }

              table.Rows.Add(newRow);
            }
            else
            {
              status.Level = StatusLevel.Error;
              status.Messages.Add("Cached data object is invalid.");
              break;
            }
          }
        }

        if (status.Level == StatusLevel.Success)
        {
          SqlBulkCopy bulkCopy = new SqlBulkCopy(_connStr);
          bulkCopy.DestinationTableName = tableName;
          bulkCopy.WriteToServer(table);
          status.Messages.Add("Cached data imported successfully.");
        }

        response.Append(status);
      }
      catch (Exception e)
      {
        _logger.Error("Error importing cached data from URL [" + url + "]: " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      return response;
    }

    public Response DeleteCache()
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        string cacheId = CheckCache(false);

        if (!string.IsNullOrEmpty(cacheId))
        {
          DataDictionary dictionary = GetDictionary();

          foreach (DataObject objectType in dictionary.dataObjects)
          {
            Response objectTypeDelete = DeleteCache(cacheId, objectType);
            response.Append(objectTypeDelete);
          }

          bool success = DeleteCacheEntry();

          if (success)
          {
            response.Messages.Add("Cache deleted successfully.");
          }
        }
        else
        {
          throw new Exception("Cache not found.");
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error refreshing cache: " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      return response;
    }

    public Response DeleteCache(string objectType)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        string cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          DataDictionary dictionary = GetDictionary();
          DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType);

          if (dataObject == null)
          {
            throw new Exception("Object type " + objectType + " not found.");
          }

          Response deleteResponse = DeleteCache(cacheId, dataObject);
          response.Append(deleteResponse);
        }
        else
        {
          throw new Exception("Cache not found.");
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error deleting cache: " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      return response;
    }

    public Response DeleteCache(string cacheId, DataObject objectType)
    {
      Response response = new Response();
      Status status = new Status()
      {
        Identifier = objectType.objectName
      };
      response.StatusList.Add(status);

      try
      {
        DeleteCacheTable(cacheId, objectType);
        status.Messages.Add("Cache for " + objectType.objectName + " deleted successfully.");
      }
      catch (Exception e)
      {
        _logger.Error("Error deleting cache: " + e.Message);
        status.Level = StatusLevel.Error;
        status.Messages.Add(e.Message);
      }

      return response;
    }

    public long GetCount(DataObject objectType, DataFilter filter)
    {
      long count = 0;

      try
      {
        string cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          string tableName = GetCacheTableName(cacheId, objectType.objectName);

          if (filter == null) filter = new DataFilter();
          string whereClause = filter.ToSqlWhereClause("SQLServer", objectType);

          int orderByIndex = whereClause.ToUpper().IndexOf("ORDER BY");

          if (orderByIndex != -1)
          {
            whereClause = whereClause.Remove(orderByIndex);
          }

          string query = string.Format(BaseLightweightDataLayer.SELECT_COUNT_SQL_TPL, tableName, whereClause);

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);

          if (dt != null && dt.Rows.Count > 0)
          {
            count = long.Parse(Convert.ToString(dt.Rows[0][0]));
          }
        }
        else if (_dataLayer != null)
        {
          return _dataLayer.GetCount(objectType.objectName, filter);
        }
        else
        {
          throw new Exception(NO_CACHE_ERROR);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data object count for type [" + objectType.objectName + "]: " + e.Message);
        throw e;
      }

      return count;
    }

    public List<IDataObject> Get(DataObject objectType, DataFilter filter, int start, int limit)
    {
      List<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        string cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          string tableName = GetCacheTableName(cacheId, objectType.objectName);

          if (filter == null) filter = new DataFilter();
          filter.AppendFilter(objectType.dataFilter);

          string whereClause = filter.ToSqlWhereClause("SQLServer", objectType);

          int orderByIndex = whereClause.ToUpper().IndexOf("ORDER BY");
          string orderByClause = "ORDER BY current_timestamp";

          if (orderByIndex != -1)
          {
            orderByClause = whereClause.Substring(orderByIndex);
            whereClause = whereClause.Remove(orderByIndex);
          }

          string query = string.Format(@"
              SELECT * FROM (SELECT row_number() OVER ({4}) as __rn, * 
              FROM {0} {1}) as __t WHERE __rn between {2} and {3}",
              tableName, whereClause, start + 1, start + limit, orderByClause);

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);
          dataObjects = BaseLightweightDataLayer.ToDataObjects(objectType, dt);
        }
        else if (_dataLayer != null)
        {
          IList<IDataObject> iDataObjects = _dataLayer.Get(objectType.objectName, filter, limit, start);

          if (iDataObjects != null)
          {
            dataObjects = iDataObjects.ToList();
          }
        }
        else
        {
          throw new Exception(NO_CACHE_ERROR);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting a page of data objects for type [" + objectType.objectName + "]: " + e.Message);
        throw e;
      }

      return dataObjects;
    }

    public List<IDataObject> Get(DataObject objectType, List<string> identifiers)
    {
      List<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        string cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          string tableName = GetCacheTableName(cacheId, objectType.objectName);

          string whereClause = BaseLightweightDataLayer.FormWhereClause(objectType, identifiers);
          string query = "SELECT * FROM " + tableName + whereClause;

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);
          dataObjects = BaseLightweightDataLayer.ToDataObjects(objectType, dt);
        }
        else if (_dataLayer != null)
        {
          dataObjects = _dataLayer.Get(objectType.objectName, identifiers).ToList();
        }
        else
        {
          throw new Exception(NO_CACHE_ERROR);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data objects for type [" + objectType.objectName + "]: " + e.Message);
        throw e;
      }

      return dataObjects;
    }

    public List<string> GetIdentifiers(DataObject objectType, DataFilter filter)
    {
      List<string> identifiers = new List<string>();

      try
      {
        string cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          string tableName = GetCacheTableName(cacheId, objectType.objectName);

          if (filter == null) filter = new DataFilter();
          filter.AppendFilter(objectType.dataFilter);

          string whereClause = filter.ToSqlWhereClause("SQLServer", objectType);

          string query = string.Format(BaseLightweightDataLayer.SELECT_SQL_TPL, tableName, whereClause);

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);
          identifiers = BaseLightweightDataLayer.FormIdentifiers(objectType, dt);
        }
        else if (_dataLayer != null)
        {
          identifiers = _dataLayer.GetIdentifiers(objectType.objectName, filter).ToList();
        }
        else
        {
          throw new Exception(NO_CACHE_ERROR);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting a list of identifiers for type [" + objectType.objectName + "]: " + e.Message);
        throw e;
      }

      return identifiers;
    }

    public List<IDataObject> Create(DataObject objectType, List<string> identifiers)
    {
      List<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        if (identifiers == null || identifiers.Count == 0)
        {
          IDataObject dataObject = new SerializableDataObject() { Type = objectType.objectName };
          dataObjects.Add(dataObject);
        }
        else
        {
          foreach (string identifier in identifiers)
          {
            IDataObject dataObject = new SerializableDataObject()
            {
              Id = identifier,
              Type = objectType.objectName
            };

            dataObjects.Add(dataObject);
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data objects for type [" + objectType.objectName + "]: " + e.Message);
        throw e;
      }

      return dataObjects;
    }

    public Response Update(DataObject objectType, List<IDataObject> dataObjects)
    {
      Dictionary<string, string> idSQLMap = new Dictionary<string, string>();
      Response response = new Response();

      try
      {
        string cacheId = CheckCache();
        string tableName = string.Empty;
        bool hasCache = false;

        if (!string.IsNullOrEmpty(cacheId))
        {
          tableName = GetCacheTableName(cacheId, objectType.objectName);
          hasCache = true;
        }
        else if (_lwDataLayer != null)
        {
          throw new Exception(NO_CACHE_ERROR);
        }

        //
        // call data layer to perform update then update cache
        //        
        if (_lwDataLayer != null)
        {
          List<SerializableDataObject> sdos = new List<SerializableDataObject>();

          foreach (IDataObject dataObject in dataObjects)
          {
            sdos.Add((SerializableDataObject)dataObject);
          }

          response = _lwDataLayer.Update(objectType, sdos);

          //
          // if overall status is success, then we can assume that 
          // all data objects have been updated successfully. Otherwise,
          // inspect every object status and perform update only against succeeded ones.
          //
          if (response.Level == StatusLevel.Success)
          {
            foreach (SerializableDataObject sdo in dataObjects)
            {
              idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, objectType, sdo);
            }

            DBManager.Instance.ExecuteUpdate(_connStr, idSQLMap);
          }
          else
          {
            foreach (Status status in response.StatusList)
            {
              if (status.Level == StatusLevel.Success)
              {
                SerializableDataObject sdo = null;

                foreach (IDataObject dataObject in dataObjects)
                {
                  if (((SerializableDataObject)dataObject).Id.ToLower() == status.Identifier.ToLower())
                  {
                    sdo = (SerializableDataObject)dataObject;
                    break;
                  }
                }

                if (sdo != null)
                {
                  idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, objectType, sdo);
                }
                else
                {
                  status.Messages.Add(
                    string.Format("Object id is out of sync. It should be {0} instead of {1}.",
                      sdo.Id, status.Identifier));
                }
              }
            }

            DBManager.Instance.ExecuteUpdate(_connStr, idSQLMap);
          }
        }
        else if (_dataLayer != null)
        {
          IList<IDataObject> updatedDataObjects = new List<IDataObject>();
          IList<string> deletedIdentifiers = new List<string>();

          foreach (SerializableDataObject sdo in dataObjects)
          {
            switch (sdo.State)
            {
              case ObjectState.Delete:
                deletedIdentifiers.Add(sdo.Id);
                break;

              default:
                updatedDataObjects.Add(sdo);
                break;
            }
          }

          if (deletedIdentifiers.Count > 0)
          {
            Response deleteResponse = _dataLayer.Delete(objectType.objectName, deletedIdentifiers);
            response.Append(deleteResponse);
          }

          if (updatedDataObjects.Count > 0)
          {
            // 
            // convert serializable data objects to IDataObjects
            //
            IList<IDataObject> idataObjects = new List<IDataObject>();

            foreach (SerializableDataObject sdo in updatedDataObjects)
            {
              IDataObject idataObject = null;

              if (string.IsNullOrEmpty(sdo.Id))
              {
                idataObject = _dataLayer.Create(objectType.objectName, null).First();
              }
              else
              {
                idataObject = _dataLayer.Create(objectType.objectName, new List<string>() { sdo.Id }).First();
              }

              // copy properies
              for (int i = 0; i < sdo.Dictionary.Keys.Count; i++)
              {
                string key = sdo.Dictionary.Keys.ElementAt(i);
                object value = sdo.Dictionary[key];

                if (value != null)
                {
                  DataProperty prop = objectType.dataProperties.Find(x => x.propertyName.ToLower() == key.ToLower());

                  if (prop.dataType == DataType.Date || prop.dataType == DataType.DateTime)
                  {
                    if (value.ToString() != string.Empty)
                    {
                      value = XmlConvert.ToDateTime(value.ToString(), XmlDateTimeSerializationMode.Utc);
                    }
                    else
                    {
                      value = null;
                    }
                  }
                }

                idataObject.SetPropertyValue(key, value);
              }

              if (sdo.HasContent)
              {
                ((IContentObject)idataObject).Content = sdo.Content;
                ((IContentObject)idataObject).ContentType = sdo.ContentType;
              }

              idataObjects.Add(idataObject);
            }

            Response updateResponse = _dataLayer.Post(idataObjects);
            response.Append(updateResponse);
          }

          if (hasCache)
          {
            if (response.Level == StatusLevel.Success)
            {
              foreach (SerializableDataObject sdo in dataObjects)
              {
                idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, objectType, sdo);
              }

              DBManager.Instance.ExecuteUpdate(_connStr, idSQLMap);
            }
            else
            {
              foreach (Status status in response.StatusList)
              {
                if (status.Level == StatusLevel.Success)
                {
                  SerializableDataObject sdo = null;

                  foreach (IDataObject dataObject in dataObjects)
                  {
                    if (((SerializableDataObject)dataObject).Id.ToLower() == status.Identifier.ToLower())
                    {
                      sdo = (SerializableDataObject)dataObject;
                      break;
                    }
                  }

                  if (sdo != null)
                  {
                    idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, objectType, sdo);
                  }
                  else
                  {
                    status.Messages.Add(
                      string.Format("Object id is out of sync. It should be {0} instead of {1}.",
                        sdo.Id, status.Identifier));
                  }
                }
              }

              DBManager.Instance.ExecuteUpdate(_connStr, idSQLMap);
            }
          }
        }
      }
      catch (Exception e)
      {
        string error = "Error updating data objects for type [" + objectType.objectName + "]: " + e.Message;
        _logger.Error(error);

        response.Level = StatusLevel.Error;
        response.Messages.Add(error);
      }

      return response;
    }

    public List<IContentObject> GetContents(DataObject objectType, Dictionary<string, string> idFormats)
    {
      List<IContentObject> contents = null;

      try
      {
        if (_lwDataLayer != null)
        {
          contents = _lwDataLayer.GetContents(objectType, idFormats).ToList();
        }
        else if (_dataLayer != null)
        {
          contents = _dataLayer.GetContents(objectType.objectName, idFormats).ToList();
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting contents for type [" + objectType.objectName + "]: " + e.Message);
        throw e;
      }

      return contents;
    }

    public Picklists GetPicklist(string picklistName, int start, int limit)
    {
      if (_dataLayer == null)
      {
        throw new Exception("Data layer does not support picklists.");
      }

      return ((IDataLayer2)_dataLayer).GetPicklist(picklistName, start, limit);
    }

    public List<IDataObject> Search(string objectType, string query, DataFilter filter, int start, int limit)
    {
      if (_dataLayer == null)
      {
        throw new Exception("Data layer does not support search.");
      }

      return ((IDataLayer2)_dataLayer).Search(objectType, query, filter, limit, start).ToList();
    }

    public long SearchCount(string objectType, string query, DataFilter filter)
    {
      if (_dataLayer == null)
      {
        throw new Exception("Data layer does not support search.");
      }

      return ((IDataLayer2)_dataLayer).GetSearchCount(objectType, query, filter);
    }

    protected string GetCacheTableName(string cacheId, string objectName)
    {
      string safeObjectName = Regex.Replace(objectName, "[^0-9a-zA-Z]+", "");
      return cacheId + "_" + safeObjectName.ToLower();
    }

    protected string CheckCache()
    {
      return CheckCache(true);
    }

    protected string CheckCache(bool validateState)
    {
      try
      {
        string checkCacheSQL = string.Format("SELECT * FROM Caches WHERE Context = '{0}' AND Application = '{1}'", _scope, _app);
        DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, checkCacheSQL);

        if (dt.Rows.Count > 0)
        {
          DataRow row = dt.Rows[0];

          if (validateState)
          {
            CacheState cacheState = (CacheState)Enum.Parse(typeof(CacheState), row["State"].ToString());

            if (cacheState != CacheState.Ready)
            {
              throw new Exception("Operation can't be done at this time. Other activity to this cache is underway.");
            }
          }

          return row["CacheId"].ToString();
        }
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
      }

      return string.Empty;
    }

    protected bool DeleteCacheEntry()
    {
      string deleteCacheSQL = string.Format(
        "DELETE FROM Caches WHERE Context = '{0}' AND Application = '{1}'", _scope, _app);

      return DBManager.Instance.ExecuteNonQuery(_connStr, deleteCacheSQL);
    }

    protected void SetCacheState(string cacheId, CacheState state)
    {
      if (!string.IsNullOrEmpty(cacheId))
      {
        string setCacheStateSQL = string.Format(
          "UPDATE Caches SET State = '{2}' WHERE Context = '{0}' AND Application = '{1}'",
            _scope, _app, state.ToString());
        bool success = DBManager.Instance.ExecuteNonQuery(_connStr, setCacheStateSQL);

        if (!success)
        {
          throw new Exception("Error updating cache state for cache [" + cacheId + "].");
        }
      }
    }

    protected void DeleteCacheTable(string cacheId, DataObject objectType)
    {
      string tableName = GetCacheTableName(cacheId, objectType.objectName);
      string deleteTableSQL = string.Format("DROP TABLE {0}", tableName);

      try
      {
        DBManager.Instance.ExecuteNonQuery(_connStr, deleteTableSQL);
      }
      catch (Exception e)
      {
        // it is OK if cache table does not exist
        _logger.Warn("Error deleting cache table: " + e);
      }
    }

    protected string CreateCacheEntry()
    {
      string cacheId = CACHE_ID_PREFIX + Guid.NewGuid().ToString("N").Remove(0, 1);

      string createCacheSQL = string.Format(@"INSERT INTO Caches (CacheId, Context, Application, Timestamp, State)
          VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", cacheId, _scope, _app, DateTime.Now.ToUniversalTime(), CacheState.Busy.ToString());

      bool success = DBManager.Instance.ExecuteNonQuery(_connStr, createCacheSQL);
      if (!success)
      {
        throw new Exception("Error creating cache entry.");
      }

      return cacheId;
    }

    protected void CreateCacheTable(string cacheId, DataObject objectType)
    {
      StringBuilder tableBuilder = new StringBuilder();
      string tableName = GetCacheTableName(cacheId, objectType.objectName);
      tableBuilder.Append("CREATE TABLE " + tableName + "( ");

      foreach (DataProperty prop in objectType.dataProperties)
      {
        string columnName = "[" + prop.propertyName + "]";
        string dataType = ToSQLType(prop.dataType);
        string nullable = prop.isNullable ? "NULL" : "NOT NULL";

        tableBuilder.AppendFormat("{0} {1} {2}{3}", columnName, dataType, nullable, PROP_SEPARATOR);
      }

      tableBuilder.AppendFormat("[_hasContent_] bit NULL");
      tableBuilder.Append(")");

      try
      {
        DBManager.Instance.ExecuteNonQuery(_connStr, tableBuilder.ToString());
      }
      catch (Exception e)
      {
        throw new Exception("Error creating cache table [" + tableName + "]. " + e);
      }
    }

    protected bool IsBase64Encoded(string text)
    {
      string pattern = "^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{4}|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)$";
      return Regex.IsMatch(text, pattern);
    }

    protected string ToSQLType(DataType dataType)
    {
      switch (dataType)
      {
        case DataType.Boolean:
          return "bit";

        case DataType.Char:
          return "varchar(1)";

        case DataType.Byte:
        case DataType.Int16:
          return "smallint";

        case DataType.Int32:
          return "int";

        case DataType.Int64:
          return "bigint";

        case DataType.Single:
        case DataType.Double:
          return "float";

        case DataType.Date:
          return "date";

        case DataType.DateTime:
          return "datetime";

        case DataType.TimeStamp:
          return "timestamp";

        default:
          return "nvarchar(MAX)";
      }
    }
  }

  public enum CacheState { Dirty, Busy, Ready }
}
