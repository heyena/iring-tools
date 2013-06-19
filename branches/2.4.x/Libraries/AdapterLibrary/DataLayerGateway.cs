using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using org.iringtools.library;
using System.IO;
using System.Xml.Linq;
using org.iringtools.utility;
using System.Data;
using log4net;
using System.Data.SqlClient;
using System.Net;
using System.Web;

namespace org.iringtools.adapter
{
  public enum CacheState { Dirty, Busy, Ready }

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
    private string _connStr;
    private IDataLayer _dataLayer;
    private ILightweightDataLayer _lwDataLayer;

    public DataLayerGateway(IKernel kernel)
    {
      _settings = kernel.Get<AdapterSettings>();
      _scope = _settings["ProjectName"];
      _app = _settings["ApplicationName"];
      _connStr = _settings["iRINGCacheConnStr"];

      //
      // Ninject requires fully qualified path
      //
      string relativePath = string.Format("{0}BindingConfiguration.{1}.{2}.xml", _settings["AppDataPath"], _scope, _app);
      string dlBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );

      XElement dlBinding = XElement.Load(dlBindingPath);

      kernel.Load(dlBindingPath);

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

      try
      {
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

      try
      {
        DataDictionary dictionary = GetDictionary(updateDictionary);

        foreach (DataObject objectType in dictionary.dataObjects)
        {
          Response objectTypeRefresh = RefreshCache(objectType);
          response.Append(objectTypeRefresh);
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

    public Response RefreshCache(bool updateDictionary, string objectType)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        DataDictionary dictionary = GetDictionary(updateDictionary, objectType);
        DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType);

        if (dataObject == null)
        {
          throw new Exception("Object type " + objectType + " not found.");
        }

        Response objectTypeRefresh = RefreshCache(dataObject);
        response.Append(objectTypeRefresh);
      }
      catch (Exception e)
      {
        _logger.Error("Error refreshing cache: " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      return response;
    }

    public Response ImportCache(DataDictionary dictionary, string baseUri)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        foreach (DataObject objectType in dictionary.dataObjects)
        {
          Response objectTypeRefresh = ImportCache(objectType, baseUri + "/" + objectType.objectName + ".dat");
          response.Append(objectTypeRefresh);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error importing cache from " + baseUri + ": " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      return response;
    }

    public Response ImportCache(DataObject objectType, string url)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        WebHttpClient client = new WebHttpClient(url);
        Stream stream = client.GetStream(string.Empty);
        List<SerializableDataObject> dataObjects = BaseLightweightDataLayer.ReadDataObjects(stream);

        if (dataObjects != null)
        {
          dataObjects = new List<SerializableDataObject>();
        }

        string cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          SetCacheState(cacheId, CacheState.Busy);
          DeleteCacheTable(cacheId, objectType);
        }
        else
        {
          CreateCacheEntry();
        }

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
        string tableName = cacheId + "_" + objectType.objectName;
        string tableSQL = "SELECT * FROM " + tableName + " WHERE 0=1";
        DataTable table = DBManager.Instance.ExecuteQuery(_connStr, tableSQL);

        foreach (SerializableDataObject dataObj in dataObjects)
        {
          DataRow newRow = table.NewRow();

          foreach (var pair in dataObj.Properties)
          {
            newRow[pair.Key] = pair.Value;
          }

          table.Rows.Add(newRow);
        }

        SqlBulkCopy bulkCopy = new SqlBulkCopy(_connStr);
        bulkCopy.DestinationTableName = tableName;
        bulkCopy.WriteToServer(table);
        status.Messages.Add("Cache data populated successfully.");

        SetCacheState(cacheId, CacheState.Ready);

        response.Messages.Add(
          string.Format("Cache {0}.{1}.{2} imported successfully.",
            _scope, _app, objectType.objectName));
      }
      catch (Exception e)
      {
        _logger.Error("Error importing cache data from URL [" + url + "]: " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
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
          DataObject cacheObjectType = Utility.CloneDataContractObject<DataObject>(objectType);
          cacheObjectType.tableName = cacheId + "_" + cacheObjectType.objectName;

          if (filter == null) filter = new DataFilter();
          string whereClause = filter.ToSqlWhereClause("SQLServer", cacheObjectType);

          string query = string.Format(BaseLightweightDataLayer.SELECT_SQL_TPL, cacheObjectType.tableName, whereClause);

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);
          count = dt.Rows.Count;
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
          DataObject cacheObjectType = Utility.CloneDataContractObject<DataObject>(objectType);
          cacheObjectType.tableName = cacheId + "_" + cacheObjectType.objectName;

          if (filter == null) filter = new DataFilter();
          string whereClause = filter.ToSqlWhereClause("SQLServer", cacheObjectType);

          int orderByIndex = whereClause.ToUpper().IndexOf("ORDER BY");
          string orderByClause = "ORDER BY current_timestamp";

          if (orderByIndex != -1)
          {
            orderByClause = whereClause.Substring(orderByIndex);
            whereClause = whereClause.Remove(orderByIndex);
          }

          string query = string.Format(@"
              SELECT * FROM (SELECT row_number() OVER ({4}) as __rn, * 
              FROM {0} {1}) as __query WHERE __rn between {2} and {3}",
              cacheObjectType.tableName, whereClause, start + 1, start + limit, orderByClause);

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);
          dataObjects = BaseLightweightDataLayer.ToDataObjects(objectType, dt);
        }
        else if (_dataLayer != null)
        {
          dataObjects = _dataLayer.Get(objectType.objectName, filter, limit, start).ToList();
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
          DataObject cacheObjectType = Utility.CloneDataContractObject<DataObject>(objectType);
          cacheObjectType.tableName = cacheId + "_" + cacheObjectType.objectName;

          string whereClause = BaseLightweightDataLayer.FormWhereClause(objectType, identifiers);
          string query = "SELECT * FROM " + cacheObjectType.tableName + whereClause;

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);
          dataObjects = BaseLightweightDataLayer.ToDataObjects(cacheObjectType, dt);
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
          DataObject cacheObjectType = Utility.CloneDataContractObject<DataObject>(objectType);
          cacheObjectType.tableName = cacheId + "_" + cacheObjectType.objectName;

          if (filter == null) filter = new DataFilter();
          string whereClause = filter.ToSqlWhereClause("SQLServer", cacheObjectType);

          string query = string.Format(BaseLightweightDataLayer.SELECT_SQL_TPL, cacheObjectType.tableName, whereClause);

          DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, query);
          identifiers = BaseLightweightDataLayer.FormIdentifiers(cacheObjectType, dt);
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

    //
    // handles add, change, and delete
    //
    public Response Update(DataObject objectType, List<SerializableDataObject> dataObjects)
    {
      Dictionary<string, string> idSQLMap = new Dictionary<string, string>();
      Response response = new Response();

      try
      {
        string cacheId = CheckCache();
        DataObject cacheObjectType = null;
        bool hasCache = false;

        if (!string.IsNullOrEmpty(cacheId))
        {
          cacheObjectType = Utility.CloneDataContractObject<DataObject>(objectType);
          cacheObjectType.tableName = cacheId + "_" + cacheObjectType.objectName;
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
          response = _lwDataLayer.Update(objectType, dataObjects);

          //
          // if overall status is success, then we can assume that 
          // all data objects have been updated successfully. Otherwise,
          // inspect every object status and perform update only against succeeded ones.
          //
          if (response.Level == StatusLevel.Success)
          {
            foreach (SerializableDataObject sdo in dataObjects)
            {
              idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(cacheObjectType, sdo);
            }

            DBManager.Instance.ExecuteUpdate(_connStr, idSQLMap);
          }
          else
          {
            foreach (Status status in response.StatusList)
            {
              if (status.Level == StatusLevel.Success)
              {
                SerializableDataObject sdo = dataObjects.Find(x => x.Id == status.Identifier);

                if (sdo != null)
                {
                  idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(cacheObjectType, sdo);
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
            Response updateResponse = _dataLayer.Post(updatedDataObjects);
            response.Append(updateResponse);
          }

          if (hasCache && response.Level == StatusLevel.Success)
          {
            foreach (SerializableDataObject sdo in dataObjects)
            {
              idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(cacheObjectType, sdo);
            }

            DBManager.Instance.ExecuteUpdate(_connStr, idSQLMap);
          }
          else
          {
            foreach (Status status in response.StatusList)
            {
              if (status.Level == StatusLevel.Success)
              {
                SerializableDataObject sdo = dataObjects.Find(x => x.Id == status.Identifier);

                if (sdo != null)
                {
                  idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(cacheObjectType, sdo);
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

    protected Response RefreshCache(DataObject objectType)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      try
      {
        string cacheId = CheckCache();

        if (!string.IsNullOrEmpty(cacheId))
        {
          SetCacheState(cacheId, CacheState.Busy);
          DeleteCacheTable(cacheId, objectType);
        }
        else
        {
          CreateCacheEntry();
        }

        //
        // create new cache table
        //
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
        string tableName = cacheId + "_" + objectType.objectName;
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

              foreach (var pair in dataObj.Properties)
              {
                newRow[pair.Key] = pair.Value;
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
          long objCount = _dataLayer.GetCount(objectType.objectName, null);
          int start = 0;
          int page = 5;
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
                  newRow[prop.propertyName] = dataObj.GetPropertyValue(prop.propertyName);
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

        SetCacheState(cacheId, CacheState.Ready);

        response.Messages.Add(
          string.Format("Cache {0}.{1}.{2} refreshed successfully.",
            _scope, _app, objectType.objectName));
      }
      catch (Exception e)
      {
        _logger.Error("Error refreshing cache for object type " + objectType.objectName + ": " + e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      return response;
    }

    protected string CheckCache()
    {
      string checkCacheSQL = string.Format("SELECT * FROM Caches WHERE Context = '{0}' AND Application = '{1}'", _scope, _app);
      DataTable dt = DBManager.Instance.ExecuteQuery(_connStr, checkCacheSQL);

      if (dt.Rows.Count > 0)
      {
        DataRow row = dt.Rows[0];
        CacheState cacheState = (CacheState)Enum.Parse(typeof(CacheState), row["State"].ToString());

        if (cacheState != CacheState.Ready)
        {
          throw new Exception("Operation can't be done at this time. Other activity to this cache is underway.");
        }

        return row["CacheId"].ToString();
      }

      return string.Empty;
    }

    protected void SetCacheState(string cacheId, CacheState state)
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

    protected void DeleteCacheTable(string cacheId, DataObject objectType)
    {
      string tableName = cacheId + "_" + objectType.objectName;
      string deleteTableSQL = string.Format("DROP TABLE {0}", tableName);

      try
      {
        DBManager.Instance.ExecuteNonQuery(_connStr, deleteTableSQL);
      }
      catch (Exception e)
      {
        _logger.Error("Error deleting cache table [" + tableName + "]. " + e);
      }
    }

    protected void CreateCacheEntry()
    {
      string cacheId = CACHE_ID_PREFIX + Guid.NewGuid().ToString("N").Remove(0, 1);

      string createCacheSQL = string.Format(@"INSERT INTO Caches (CacheId, Context, Application, Timestamp, State)
          VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", cacheId, _scope, _app, DateTime.Now.ToUniversalTime(), CacheState.Busy.ToString());

      bool success = DBManager.Instance.ExecuteNonQuery(_connStr, createCacheSQL);
      if (!success)
      {
        throw new Exception("Error creating cache entry.");
      }
    }

    protected void CreateCacheTable(string cacheId, DataObject objectType)
    {
      StringBuilder tableBuilder = new StringBuilder();
      string tableName = cacheId + "_" + objectType.objectName;
      tableBuilder.Append("CREATE TABLE " + tableName + "( ");

      foreach (DataProperty prop in objectType.dataProperties)
      {
        string columnName = "[" + prop.propertyName + "]";
        string dataType = ToSQLType(prop.dataType);
        string nullable = prop.isNullable ? "NULL" : "NOT NULL";

        tableBuilder.AppendFormat("{0} {1} {2}{3}", columnName, dataType, nullable, PROP_SEPARATOR);
      }

      tableBuilder.Remove(tableBuilder.Length - PROP_SEPARATOR.Length, PROP_SEPARATOR.Length);
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
}
