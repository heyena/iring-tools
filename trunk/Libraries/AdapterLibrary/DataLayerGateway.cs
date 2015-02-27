using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using log4net;
using Ninject;
using org.iringtools.library;
using org.iringtools.utility;

namespace org.iringtools.adapter
{
    //
    // this class determines the appropriate data layer (cache, actual, or both) to call
    //
    public class DataLayerGateway
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DataLayerGateway));

        private const string NO_CACHE_ERROR = "Cache entry has not been created. You need to refresh or import cache files.";
        private const string PROP_SEPARATOR = ", ";
        private const string CACHE_ID_PREFIX = "c";

        private AdapterSettings _settings;
        private string _scope;
        private string _app;
        private DataDictionary _dictionary;
        private string _dataPath;
        private string _cacheConnStr;
        private IDataLayer _dataLayer;
        private ILightweightDataLayer _lwDataLayer;
        public ILightweightDataLayer2 _lwDataLayer2;
        private LoadingType _loadingType = LoadingType.Lazy;

        public DataLayerGateway(IKernel kernel)
        {
            _settings = kernel.Get<AdapterSettings>();
            _scope = _settings["ProjectName"].ToLower();
            _app = _settings["ApplicationName"].ToLower();
            _dataPath = Path.Combine(_settings["BaseDirectoryPath"], _settings["AppDataPath"]);
            _cacheConnStr = _settings[BaseProvider.CACHE_CONNSTR];

            string loadingType = _settings["http-header-LoadingType"];
            if (loadingType != null && loadingType.ToUpper() == "EAGER")
                _loadingType = LoadingType.Eager;

            if (Utility.IsBase64Encoded(_cacheConnStr))
            {
                string keyFile = (_settings[BaseProvider.CACHE_CONNSTR_LEVEL] == "Scope")
                ? string.Format("{0}{1}.key", _settings["AppDataPath"], _scope)
                : string.Format("{0}adapter.key", _settings["AppDataPath"]);

                _cacheConnStr = EncryptionUtility.Decrypt(_cacheConnStr, keyFile);
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
                ////  //if (System.Configuration.ConfigurationManager.AppSettings["CachePageSize"] == null)
                if (dlBinding.Element("bind").Attribute("service").Value == "org.iringtools.library.ILightweightDataLayer, iRINGLibrary")
                {
                    _lwDataLayer = kernel.Get<ILightweightDataLayer>();
                }
                else if (dlBinding.Element("bind").Attribute("service").Value == "org.iringtools.library.ILightweightDataLayer2, iRINGLibrary")
                {

                    _lwDataLayer2 = kernel.Get<ILightweightDataLayer2>();
                }
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
                    _logger.Info("DataLayer Class: LightWeight");

                    dictionary = _lwDataLayer.Dictionary(refresh, objectType, out filter);
                }
                if (_lwDataLayer2 != null)
                {

                    _logger.Info("DataLayer Class: LightWeight2");

                    dictionary = _lwDataLayer2.Dictionary(refresh, objectType, out filter);
                }
                else if (_dataLayer != null)
                {
                    _logger.Info("DataLayer Class: Legacy");

                    if (refresh)
                    {
                        _logger.Info("Refreshing Dictionary");

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
                            Response response = ((IDataLayer2)_dataLayer).Refresh(objectType);

                            if (response.Level != StatusLevel.Success)
                            {
                                throw new Exception("Error refreshing dictionary: " + response.Messages);
                            }
                        }
                    }

                    dictionary = _dataLayer.GetDictionary();
                }

                // injecting external filters to dictionary
                if (dictionary != null && dictionary.dataObjects != null)
                {
                    foreach (DataObject dataObject in dictionary.dataObjects)
                    {
                        string filterPath = string.Format("{0}Filter.{1}.{2}.{3}.xml", _dataPath, _scope, _app, dataObject.objectName);

                        if (File.Exists(filterPath))
                        {
                            DataFilter dataFilter = Utility.Read<DataFilter>(filterPath);
                            dataObject.dataFilter = dataFilter;
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

                _dictionary = GetDictionary(updateDictionary);
                cacheId = CheckCache();

                if (!string.IsNullOrEmpty(cacheId))
                {
                    SetCacheState(cacheId, CacheState.Busy);
                }
                else
                {
                    cacheId = CreateCacheEntry();
                }

                foreach (DataObject objectType in _dictionary.dataObjects)
                {
                    Response objectTypeRefresh = RefreshCache(cacheId, objectType, false);
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

        public Response RefreshCache(bool updateDictionary, string objectType, bool includeRelated)
        {
            Response response = new Response();
            response.Level = StatusLevel.Success;

            string cacheId = string.Empty;

            try
            {
                _dictionary = GetDictionary(updateDictionary, objectType);
                DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

                if (dataObject == null)
                {
                    throw new Exception("Object type [" + objectType + "] not found.");
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

                Response objectTypeRefresh = RefreshCache(cacheId, dataObject, includeRelated);
                response.Append(objectTypeRefresh);
            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;

                string error = "Error refreshing cache for [" + objectType + "]: " + e.Message;
                _logger.Error(error);
                response.Messages.Add(error);
            }
            finally
            {
                SetCacheState(cacheId, CacheState.Ready);
            }

            return response;
        }

        protected Response RefreshCache(string cacheId, DataObject objectType, bool includeRelated)
        {

            Response response = null;

            if (_lwDataLayer != null || _dataLayer != null)
            // //if (cacheSize == null)
            {

                response = DoRefreshCache(cacheId, objectType);

            }
            else
            {
                int cacheSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CachePageSize"]);
                if (cacheSize != 0)
                {
                    response = DoRefreshCacheWithIdentifier(cacheId, objectType, cacheSize);
                }
            }

            try
            {
                if (includeRelated && objectType.dataRelationships != null)
                {
                    foreach (DataRelationship relationship in objectType.dataRelationships)
                    {
                        DataObject relatedObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == relationship.relatedObjectName.ToLower());

                        if (relatedObject != null)
                        {
                            Response relatedObjectRefresh = DoRefreshCache(cacheId, relatedObject);
                            response.Append(relatedObjectRefresh);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error refreshing cache for type [" + objectType.objectName + "]: " + e.Message);
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            return response;
        }

        protected Response DoRefreshCache(string cacheId, DataObject objectType)
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
                string strCachetable = cacheId + "_" + objectType.objectName.Replace("_", "");
                _logger.Debug(string.Format("{0} cache table created.", strCachetable));

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
                DataTable table = DBManager.Instance.ExecuteQuery(_cacheConnStr, tableSQL);

                if (_lwDataLayer != null)
                {
                    IList<SerializableDataObject> dataObjects = _lwDataLayer.Get(objectType);
                    _logger.Debug(string.Format("Populating  cache data for cache table {0}. Total number of rows = {1}", strCachetable, dataObjects.Count));

                    if (dataObjects != null && dataObjects.Count > 0)
                    {
                        foreach (SerializableDataObject dataObj in dataObjects)
                        {
                            DataRow newRow = table.NewRow();

                            foreach (var pair in dataObj.Dictionary)
                            {
                                if (pair.Value == null)
                                    newRow[pair.Key] = DBNull.Value;
                                else
                                    newRow[pair.Key] = pair.Value;
                            }

                            if (dataObj.HasContent)
                            {
                                newRow[BaseLightweightDataLayer.HAS_CONTENT] = true;
                            }

                            table.Rows.Add(newRow);
                        }

                        SqlBulkCopy bulkCopy = new SqlBulkCopy(_cacheConnStr);
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(table);
                        _logger.Debug(string.Format("Caching completed for cache table {0}.", strCachetable));

                        string msg = "Cache data for [" + objectType.objectName + "] populated successfully.";
                        status.Messages.Add(msg);
                        response.Messages.Add(msg);
                    }
                }
                else if (_dataLayer != null)
                {
                    int page = 100;
                    string cachePage = Convert.ToString(_settings["CachePageSize"]);

                    if (!string.IsNullOrEmpty(cachePage))
                    {
                        page = int.Parse(cachePage);
                    }

                    long objCount = _dataLayer.GetCount(objectType.objectName, null);
                    int start = 0;
                    long limit = 0;
                    long nRowCounter = 0;

                    _logger.Debug(string.Format("Populating  cache data for cache table {0}. Total number of rows = {1}. Cache size= {2}", strCachetable, objCount, page));
                    while (start < objCount)
                    {
                        limit = page;// (start + page < objCount) ? page : objCount - start;
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

                        nRowCounter = (objCount <= page) ? objCount : page;
                        _logger.Debug(string.Format("Saving rows {0} to {1}  in memory for table {2}.", start, start + nRowCounter, strCachetable));
                        start += page;

                    }


                    SqlBulkCopy bulkCopy = new SqlBulkCopy(_cacheConnStr);
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(table);

                    _logger.Debug(string.Format("Data cached for table {0}", strCachetable));

                    string msg = "Cache data for [" + objectType.objectName + "] populated successfully.";
                    status.Messages.Add(msg);
                    response.Messages.Add(msg);
                }
            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;

                string error = "Error refreshing cache [" + objectType.objectName + "]: " + e.Message;
                _logger.Error(error);
                response.Messages.Add(error);
            }

            return response;
        }

        public Response ImportCache(string baseUri, bool updateDictionary)
        {
            Response response = new Response();
            response.Level = StatusLevel.Success;

            string cacheId = string.Empty;

            try
            {
                if (!baseUri.EndsWith("/")) baseUri += "/";

                _dictionary = GetDictionary(updateDictionary);
                cacheId = CheckCache();

                if (!string.IsNullOrEmpty(cacheId))
                {
                    SetCacheState(cacheId, CacheState.Busy);
                }
                else
                {
                    cacheId = CreateCacheEntry();
                }

                foreach (DataObject objectType in _dictionary.dataObjects)
                {
                    string importURI = baseUri + objectType.objectName + ".dat";
                    Response objectTypeImport = ImportCache(cacheId, objectType, importURI, false);
                    response.Append(objectTypeImport);
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

        public Response ImportCache(string objectType, string importURI, bool updateDictionary, bool includeRelated)
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

                _dictionary = GetDictionary(updateDictionary, objectType);
                DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

                if (dataObject == null)
                {
                    throw new Exception("Object type [" + objectType + "] not found.");
                }

                Response objectTypeImport = ImportCache(cacheId, dataObject, importURI, includeRelated);
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

        protected Response ImportCache(string cacheId, DataObject objectType, string importURI, bool includeRelated)
        {
            Response response = DoImportCache(cacheId, objectType, importURI);

            try
            {
                if (includeRelated && objectType.dataRelationships != null)
                {
                    foreach (DataRelationship relationship in objectType.dataRelationships)
                    {
                        DataObject relatedObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == relationship.relatedObjectName.ToLower());

                        if (relatedObject != null)
                        {
                            Response relatedObjectImport = DoImportCache(cacheId, relatedObject, importURI);
                            response.Append(relatedObjectImport);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error importing cached data from URI [" + importURI + "]: " + e.Message);
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            return response;
        }

        protected Response DoImportCache(string cacheId, DataObject objectType, string importURI)
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

                if (!importURI.ToLower().EndsWith(".dat"))
                {
                    if (!importURI.EndsWith("/"))
                    {
                        importURI += "/";
                    }

                    importURI += objectType.objectName + ".dat";
                }

                WebHttpClient client = new WebHttpClient(importURI);
                Stream stream = client.GetStream(string.Empty);
                List<SerializableDataObject> dataObjects = BaseLightweightDataLayer.ReadDataObjects(stream);

                DeleteCacheTable(cacheId, objectType);
                CreateCacheTable(cacheId, objectType);

                //
                // populate cache data
                //
                string tableName = GetCacheTableName(cacheId, objectType.objectName);
                string tableSQL = "SELECT * FROM " + tableName + " WHERE 0=1";
                DataTable table = DBManager.Instance.ExecuteQuery(_cacheConnStr, tableSQL);

                if (dataObjects == null || dataObjects.Count == 0)
                {
                    status.Level = StatusLevel.Warning;

                    string msg = "Cache data for [" + objectType.objectName + "] is empty:";
                    status.Messages.Add(msg);
                    response.Messages.Add(msg);
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

                            string error = "Cached data for [" + objectType.objectName + "] is invalid.";
                            status.Messages.Add(error);
                            response.Messages.Add(error);
                            break;
                        }
                    }
                }

                if (status.Level == StatusLevel.Success)
                {
                    SqlBulkCopy bulkCopy = new SqlBulkCopy(_cacheConnStr);
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(table);

                    string msg = "Cached data for [" + objectType.objectName + "] imported successfully.";
                    status.Messages.Add(msg);
                    response.Messages.Add(msg);
                }

                response.Append(status);
            }
            catch (Exception e)
            {
                _logger.Error("Error importing cached data from URI [" + importURI + "]: " + e.Message);
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
                    _dictionary = GetDictionary();

                    foreach (DataObject objectType in _dictionary.dataObjects)
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
                _logger.Error("Error deleting cache: " + e.Message);
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
                    _dictionary = GetDictionary();
                    DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

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
            string cacheId = string.Empty;

            try
            {
                if (_settings["DataMode"] == DataMode.Cache.ToString() || _lwDataLayer != null || _lwDataLayer2 != null)
                {
                    cacheId = CheckCache();

                    if (string.IsNullOrEmpty(cacheId))
                    {
                        throw new Exception(NO_CACHE_ERROR);
                    }
                }
                else if (_dataLayer != null)
                {
                    return _dataLayer.GetCount(objectType.objectName, filter);
                }

                DataObject cachedObjectType = GetCachedObjectType(cacheId, objectType);

                if (filter == null) filter = new DataFilter();
                filter.AppendFilter(cachedObjectType.dataFilter);

                string whereClause = filter.ToSqlWhereClause("SQLServer", cachedObjectType);

                int orderByIndex = whereClause.ToUpper().IndexOf("ORDER BY");

                if (orderByIndex != -1)
                {
                    whereClause = whereClause.Remove(orderByIndex);
                }

                string query = string.Format(BaseLightweightDataLayer.SELECT_COUNT_SQL_TPL, cachedObjectType.tableName, whereClause);

                DataTable dt = DBManager.Instance.ExecuteQuery(_cacheConnStr, query);

                if (dt != null && dt.Rows.Count > 0)
                {
                    count = long.Parse(Convert.ToString(dt.Rows[0][0]));
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error getting data objects count for type [" + objectType.objectName + "]: " + e.Message);
                throw e;
            }

            return count;
        }

        public List<IDataObject> Get(DataObject objectType, DataFilter filter, int start, int limit)
        {
            List<IDataObject> dataObjects = new List<IDataObject>();

            try
            {
                string cacheId = string.Empty;


                //    if (_settings["DataMode"] == DataMode.Cache.ToString() || _lwDataLayer != null)
                if (_settings["DataMode"] == DataMode.Cache.ToString() || _lwDataLayer != null || _lwDataLayer2 != null)
                {
                    cacheId = CheckCache();

                    if (string.IsNullOrEmpty(cacheId))
                    {
                        throw new Exception(NO_CACHE_ERROR);
                    }
                }
                else if (_dataLayer != null)
                {
                    IList<IDataObject> iDataObjects = _dataLayer.Get(objectType.objectName, filter, limit, start);

                    if (iDataObjects != null)
                    {
                        dataObjects = iDataObjects.ToList();
                    }

                    if (_loadingType == LoadingType.Eager) // include related object in collection
                    {
                        List<IDataObject> relatedObjects = GetRelatedObjects(objectType, dataObjects);
                        dataObjects.AddRange(relatedObjects);
                    }
                    return dataObjects;
                }

                DataObject cachedObjectType = GetCachedObjectType(cacheId, objectType);

                if (filter == null) filter = new DataFilter();
                filter.AppendFilter(cachedObjectType.dataFilter);

                string whereClause = filter.ToSqlWhereClause("SQLServer", cachedObjectType);

                int orderByIndex = whereClause.ToUpper().IndexOf("ORDER BY");
                string orderByClause = "ORDER BY current_timestamp";

                if (orderByIndex != -1)
                {
                    orderByClause = whereClause.Substring(orderByIndex);
                    whereClause = whereClause.Remove(orderByIndex);
                }

                string query = string.Format(@"
            SELECT * FROM (SELECT row_number() OVER ({2}) as __rn, * 
            FROM {0} {1}) as __t",
                    cachedObjectType.tableName, whereClause, orderByClause);

                if (!(start == 0 && limit == 0))
                {
                    query += string.Format(" WHERE __rn between {0} and {1}", start + 1, start + limit);
                }

                DataTable dt = DBManager.Instance.ExecuteQuery(_cacheConnStr, query);
                dataObjects = BaseLightweightDataLayer.ToDataObjects(cachedObjectType, dt);

                if (_loadingType == LoadingType.Eager) // include related object in collection
                {
                    List<IDataObject> relatedObjects = GetRelatedObjects(objectType, dataObjects);
                    dataObjects.AddRange(relatedObjects);
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
                string cacheId = string.Empty;

                if (_settings["DataMode"] == DataMode.Cache.ToString() || _lwDataLayer != null || _lwDataLayer2 != null)
                {
                    cacheId = CheckCache();

                    if (string.IsNullOrEmpty(cacheId))
                    {
                        throw new Exception(NO_CACHE_ERROR);
                    }
                }
                else if (_dataLayer != null)
                {
                    IList<IDataObject> iDataObjects = _dataLayer.Get(objectType.objectName, identifiers);

                    if (iDataObjects != null)
                    {
                        dataObjects = iDataObjects.ToList();
                    }

                    if (_loadingType == LoadingType.Eager) // include related object in collection
                    {
                        List<IDataObject> relatedObjects = GetRelatedObjects(objectType, dataObjects);
                        dataObjects.AddRange(relatedObjects);
                    }

                    return dataObjects;
                }

                string tableName = GetCacheTableName(cacheId, objectType.objectName);

                string whereClause = BaseLightweightDataLayer.FormWhereClause(objectType, identifiers);
                string query = "SELECT * FROM " + tableName + whereClause;

                DataTable dt = DBManager.Instance.ExecuteQuery(_cacheConnStr, query);
                dataObjects = BaseLightweightDataLayer.ToDataObjects(objectType, dt);

                if (_loadingType == LoadingType.Eager) // include related object in collection
                {
                    List<IDataObject> relatedObjects = GetRelatedObjects(objectType, dataObjects);
                    dataObjects.AddRange(relatedObjects);
                }

            }
            catch (Exception e)
            {
                _logger.Error("Error getting data objects for type [" + objectType.objectName + "]: " + e.Message);
                throw e;
            }

            return dataObjects;
        }

        private List<IDataObject> GetRelatedObjects(DataObject parentObjectType, List<IDataObject> parentDataObjects)
        {
            List<IDataObject> relatedObjects = new List<IDataObject>();
            _dictionary = GetDictionary();
            foreach (DataRelationship relationship in parentObjectType.dataRelationships)
            {

                DataObject relatedObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == relationship.relatedObjectName.ToLower());
                if (relatedObject != null)
                {
                    foreach (IDataObject parentDataObject in parentDataObjects)
                    {
                        DataFilter filter = new DataFilter();
                        foreach (PropertyMap propMap in relationship.propertyMaps)
                        {
                            filter.Expressions.Add(new Expression()
                            {
                                PropertyName = propMap.relatedPropertyName,
                                RelationalOperator = RelationalOperator.EqualTo,
                                LogicalOperator = LogicalOperator.And,
                                Values = new Values() { Convert.ToString(parentDataObject.GetPropertyValue(propMap.dataPropertyName)) }
                            });
                        }

                        List<IDataObject> tempRelatedObjects = Get(relatedObject, filter, 0, 0);
                        relatedObjects.AddRange(tempRelatedObjects);
                    }
                }
            }
            return relatedObjects;
        }

        public List<string> GetIdentifiers(DataObject objectType, DataFilter filter)
        {
            List<string> identifiers = new List<string>();

            try
            {
                string cacheId = string.Empty;

                if (_settings["DataMode"] == DataMode.Cache.ToString() || _lwDataLayer != null)
                {
                    cacheId = CheckCache();

                    if (string.IsNullOrEmpty(cacheId))
                    {
                        throw new Exception(NO_CACHE_ERROR);
                    }
                }
                else if (_dataLayer != null)
                {
                    IList<string> iIdentifiers = _dataLayer.GetIdentifiers(objectType.objectName, filter);

                    if (iIdentifiers != null)
                    {
                        identifiers = iIdentifiers.ToList();
                    }

                    return identifiers;
                }

                DataObject cachedObjectType = GetCachedObjectType(cacheId, objectType);

                if (filter == null) filter = new DataFilter();
                filter.AppendFilter(cachedObjectType.dataFilter);

                string whereClause = filter.ToSqlWhereClause("SQLServer", cachedObjectType);

                string query = string.Format(BaseLightweightDataLayer.SELECT_SQL_TPL, cachedObjectType.tableName, whereClause);

                DataTable dt = DBManager.Instance.ExecuteQuery(_cacheConnStr, query);
                identifiers = BaseLightweightDataLayer.FormIdentifiers(cachedObjectType, dt);
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
            IDictionary<string, string> objectTableMap = new Dictionary<string, string>();
            Response response = new Response();

            try
            {
                _logger.Info("Update owner: " + _settings["DomainName"] + "\\" + _settings["UserName"]);

                string cacheId = string.Empty;
                string tableName = string.Empty;
                _dictionary = GetDictionary();

                bool enableCacheUpdate = _settings["EnableCacheUpdate"] == null ||
                  _settings["EnableCacheUpdate"].ToString().ToLower() == "true";

                if (_settings["DataMode"] == DataMode.Cache.ToString() || _lwDataLayer != null || _lwDataLayer2 != null)
                {
                    cacheId = CheckCache();

                    if (string.IsNullOrEmpty(cacheId))
                    {
                        throw new Exception(NO_CACHE_ERROR);
                    }

                    tableName = GetCacheTableName(cacheId, objectType.objectName);
                    objectTableMap.Add(objectType.objectName, tableName);

                    if (_loadingType == LoadingType.Eager && objectType.dataRelationships != null)
                    {

                        foreach (DataRelationship relationship in objectType.dataRelationships)
                        {
                            DataObject relatedObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == relationship.relatedObjectName.ToLower());
                            if (relatedObject != null)
                            {
                                tableName = GetCacheTableName(cacheId, relatedObject.objectName);
                                objectTableMap.Add(relatedObject.objectName, tableName);
                            }
                        }
                    }

                }

                //
                // call data layer to perform update then update cache
                //        
                if (_lwDataLayer != null || _lwDataLayer2 != null)
                {
                    List<SerializableDataObject> sdos = new List<SerializableDataObject>();

                    foreach (IDataObject dataObject in dataObjects)
                    {
                        sdos.Add((SerializableDataObject)dataObject);
                    }

                    if (_lwDataLayer != null)
                        response = _lwDataLayer.Update(objectType, sdos);
                    else
                        response = _lwDataLayer2.Update(objectType, sdos);

                    if (enableCacheUpdate)
                    {
                        //
                        // if overall status is success, then we can assume that 
                        // all data objects have been updated successfully. Otherwise,
                        // inspect every object status and perform update only upon succeeded ones.
                        //
                        if (response.Level == StatusLevel.Success)
                        {
                            foreach (SerializableDataObject sdo in dataObjects)
                            {
                                DataObject localObjectType = objectType;
                                if (_loadingType == LoadingType.Eager && objectType.dataRelationships != null)
                                {
                                    localObjectType = _dictionary.dataObjects.Where(x => x.objectName.ToUpper() == sdo.Type.ToUpper()).SingleOrDefault();
                                }
                                tableName = objectTableMap[localObjectType.objectName];
                                idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, localObjectType, sdo);
                            }

                            DBManager.Instance.ExecuteUpdate(_cacheConnStr, idSQLMap);
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
                                        DataObject localObjectType = objectType;
                                        if (_loadingType == LoadingType.Eager && objectType.dataRelationships != null)
                                        {
                                            localObjectType = _dictionary.dataObjects.Where(x => x.objectName.ToUpper() == sdo.Type.ToUpper()).SingleOrDefault();
                                        }
                                        tableName = objectTableMap[localObjectType.objectName];

                                        idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, localObjectType, sdo);
                                    }
                                    else
                                    {
                                        status.Messages.Add(
                                          string.Format("Object id is out of sync. It should be {0} instead of {1}.",
                                            sdo.Id, status.Identifier));
                                    }
                                }
                            }

                            DBManager.Instance.ExecuteUpdate(_cacheConnStr, idSQLMap);
                        }
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
                            DataObject localObjectType = objectType;
                            IDataObject idataObject = null;

                            if (_loadingType == LoadingType.Eager && objectType.dataRelationships != null)
                            {
                                localObjectType = _dictionary.dataObjects.Where(x => x.objectName.ToUpper() == sdo.Type.ToUpper()).SingleOrDefault();
                            }

                            if (string.IsNullOrEmpty(sdo.Id))
                            {
                                idataObject = _dataLayer.Create(localObjectType.objectName, null).First();
                            }
                            else
                            {
                                idataObject = _dataLayer.Create(localObjectType.objectName, new List<string>() { sdo.Id }).First();
                            }

                            if (idataObject == null)
                            {
                                response.Messages.Add("Data object can not be null.");
                                continue;
                            }

                            //
                            // create identifier for data object
                            // 
                            if (string.IsNullOrEmpty(sdo.Id))
                            {
                                StringBuilder builder = new StringBuilder();
                                string delimiter = localObjectType.keyDelimeter ?? string.Empty;

                                foreach (KeyProperty keyProp in localObjectType.keyProperties)
                                {
                                    string propName = keyProp.keyPropertyName;
                                    string propValue = idataObject.GetPropertyValue(propName).ToString();
                                    builder.Append(delimiter + propValue);
                                }

                                sdo.Id = builder.ToString().Remove(0, delimiter.Length);
                            }

                            // copy properies
                            for (int i = 0; i < sdo.Dictionary.Keys.Count; i++)
                            {
                                string key = sdo.Dictionary.Keys.ElementAt(i);
                                object value = sdo.Dictionary[key];

                                if (value != null && value != "")
                                {
                                    DataProperty prop = localObjectType.dataProperties.Find(x => x.propertyName.ToLower() == key.ToLower());

                                    if (prop == null)
                                    {
                                        throw new Exception("Property [" + key + "] not found in data dictionary.");
                                    }

                                    if (prop.dataType == DataType.Date || prop.dataType == DataType.DateTime)
                                    {
                                        if (value.ToString() != string.Empty)
                                        {
                                            value = Utility.ToXsdDateTime(value.ToString());
                                        }
                                        else
                                        {
                                            value = null;
                                        }
                                    }
                                    else if (prop.dataType != DataType.String)
                                    {
                                        decimal decValue = 0;
                                        Decimal.TryParse(value.ToString(), out decValue);
                                        value = decValue;
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

                    if (_settings["DataMode"] == DataMode.Cache.ToString() && enableCacheUpdate)
                    {
                        if (response.Level == StatusLevel.Success)
                        {
                            foreach (SerializableDataObject sdo in dataObjects)
                            {
                                DataObject localObjectType = objectType;
                                if (_loadingType == LoadingType.Eager && objectType.dataRelationships != null)
                                {
                                    localObjectType = _dictionary.dataObjects.Where(x => x.objectName.ToUpper() == sdo.Type.ToUpper()).SingleOrDefault();
                                }
                                tableName = objectTableMap[localObjectType.objectName];
                                idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, localObjectType, sdo);
                            }

                            DBManager.Instance.ExecuteUpdate(_cacheConnStr, idSQLMap);
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
                                        DataObject localObjectType = objectType;
                                        if (_loadingType == LoadingType.Eager && objectType.dataRelationships != null)
                                        {
                                            localObjectType = _dictionary.dataObjects.Where(x => x.objectName.ToUpper() == sdo.Type.ToUpper()).SingleOrDefault();
                                        }
                                        tableName = objectTableMap[localObjectType.objectName];
                                        idSQLMap[sdo.Id] = BaseLightweightDataLayer.CreateUpdateSQL(tableName, localObjectType, sdo);
                                    }
                                    else
                                    {
                                        status.Messages.Add(
                                          string.Format("Object id is out of sync. It should be {0} instead of {1}.",
                                            sdo.Id, status.Identifier));
                                    }
                                }
                            }

                            DBManager.Instance.ExecuteUpdate(_cacheConnStr, idSQLMap);
                        }
                    }
                }
            }
            catch (Exception e)
            {

                string error = "Error updating data objects for type [" + objectType.objectName + "]: " + e.Message;
                _logger.Error(error);

                if (e is WebFaultException && e.Data != null)
                {
                    error = string.Empty;

                    foreach (DictionaryEntry entry in e.Data)
                    {
                        error += e.Data[entry.Key].ToString();
                    }
                }

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
                else if (_lwDataLayer2 != null)
                {
                    contents = _lwDataLayer2.GetContents(objectType, idFormats).ToList();
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

        public List<IDataObject> GetRelatedObjects(IDataObject parentDataObject, DataObject relatedObjectType, DataFilter filter,
          int limit, int start)
        {
            if (_dataLayer != null && relatedObjectType.isRelatedOnly)
            {
                IList<IDataObject> objects = _dataLayer.GetRelatedObjects(parentDataObject, relatedObjectType.objectName, filter, limit, start);
                return objects.ToList<IDataObject>();
            }

            throw new Exception("Data layer is not bound or related object type does not have IsRelatedOnly set.");
        }

        public long GetRelatedCount(IDataObject parentDataObject, DataObject relatedObjectType, DataFilter filter)
        {
            if (_dataLayer != null && relatedObjectType.isRelatedOnly)
            {
                return _dataLayer.GetRelatedCount(parentDataObject, relatedObjectType.objectName);
            }

            throw new Exception("Data layer is not bound or related object type does not have IsRelatedOnly set.");
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

        protected DataObject GetCachedObjectType(string cacheId, DataObject objectType)
        {
            DataObject cachedObjectType = Utility.CloneDataContractObject<DataObject>(objectType);

            if (cachedObjectType != null)
            {
                string tableName = GetCacheTableName(cacheId, objectType.objectName);
                cachedObjectType.tableName = tableName;

                foreach (DataProperty prop in cachedObjectType.dataProperties)
                {
                    prop.columnName = prop.propertyName;
                }
            }

            return cachedObjectType;
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
                DataTable dt = DBManager.Instance.ExecuteQuery(_cacheConnStr, checkCacheSQL);

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

            return DBManager.Instance.ExecuteNonQuery(_cacheConnStr, deleteCacheSQL);
        }

        protected void SetCacheState(string cacheId, CacheState state)
        {
            if (!string.IsNullOrEmpty(cacheId))
            {
                string setCacheStateSQL = string.Format(
                  "UPDATE Caches SET State = '{2}' WHERE Context = '{0}' AND Application = '{1}'",
                    _scope, _app, state.ToString());
                bool success = DBManager.Instance.ExecuteNonQuery(_cacheConnStr, setCacheStateSQL);

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
                DBManager.Instance.ExecuteNonQuery(_cacheConnStr, deleteTableSQL);
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

            bool success = DBManager.Instance.ExecuteNonQuery(_cacheConnStr, createCacheSQL);
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
                string dataType = ToSQLType(prop);
                string nullable = prop.isNullable ? "NULL" : "NOT NULL";

                tableBuilder.AppendFormat("{0} {1} {2}{3}", columnName, dataType, nullable, PROP_SEPARATOR);
            }
            if (objectType.extensionProperties != null)
            {
                foreach (ExtensionProperty prop in objectType.extensionProperties)
                {
                    string columnName = "[" + prop.propertyName + "]";
                    string dataType = ToSQLType(prop);
                    string nullable = prop.isNullable ? "NULL" : "NOT NULL";

                    tableBuilder.AppendFormat("{0} {1} {2}{3}", columnName, dataType, nullable, PROP_SEPARATOR);
                }
            }

            tableBuilder.AppendFormat("[_hasContent_] bit NULL");
            tableBuilder.Append(")");

            try
            {
                DBManager.Instance.ExecuteNonQuery(_cacheConnStr, tableBuilder.ToString());
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

        protected string ToSQLType(DataProperty prop)
        {
            DataType dataType = prop.dataType;
            switch (dataType)
            {
                case DataType.Boolean:
                    return "bit";

                case DataType.Char:
                    return string.Format("varchar({0})", prop.dataLength);

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

                case DataType.Decimal:
                    if (prop.precision == 0 && prop.scale == 0)
                    {
                        prop.precision = 38;
                        prop.scale = 19;
                    }
                    return string.Format("decimal({0},{1})", prop.precision, prop.scale);

                case DataType.Date:
                    return "date";

                case DataType.DateTime:
                    return "datetime";

                case DataType.TimeStamp:
                    return "timestamp";

                case DataType.String:
                    if (prop.dataLength == 0 || prop.dataLength > 4000)
                    {
                        return "nvarchar(MAX)";
                    }
                    return string.Format("nvarchar({0})", prop.dataLength);

                default:
                    return "nvarchar(MAX)";
            }
        }

        protected string ToSQLType(ExtensionProperty prop)
        {
            DataType dataType = prop.dataType;
            switch (dataType)
            {
                case DataType.Boolean:
                    return "bit";

                case DataType.Char:
                    return string.Format("varchar({0})", prop.dataLength);

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

                case DataType.Decimal:
                    if (prop.precision == 0 && prop.scale == 0)
                    {
                        prop.precision = 38;
                        prop.scale = 19;
                    }
                    return string.Format("decimal({0},{1})", prop.precision, prop.scale);

                case DataType.Date:
                    return "date";

                case DataType.DateTime:
                    return "datetime";

                case DataType.TimeStamp:
                    return "timestamp";

                case DataType.String:
                    if (prop.dataLength == 0 || prop.dataLength > 4000)
                    {
                        return "nvarchar(MAX)";
                    }
                    return string.Format("nvarchar({0})", prop.dataLength);

                default:
                    return "nvarchar(MAX)";
            }
        }

        protected Response DoRefreshCacheWithIdentifier(string cacheId, DataObject objectType, int CachePageSize)
        {
            Response response = new Response();
            response.Level = StatusLevel.Success;

            try
            {
                string strCachetable = cacheId + "_" + objectType.objectName.Replace("_", "");
                //
                // create new cache table
                //
                DeleteCacheTable(cacheId, objectType);
                _logger.Debug(string.Format("{0} cache table deleted.", strCachetable));

                CreateCacheTable(cacheId, objectType);
                _logger.Debug(string.Format("{0} cache table created.", strCachetable));

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
                DataTable table = DBManager.Instance.ExecuteQuery(_cacheConnStr, tableSQL);
                //   int dataCount = CachePageSize;
                int pageIndex = 0;
                int start = 0;
                int nRowCounter = 0;
                if (_lwDataLayer2 != null)
                {
                    //get all identifier from datalayer   for idatalayer2 interface
                    List<SerializableDataObject> dataObjects2 = _lwDataLayer2.GetIndex(objectType);

                    _logger.Debug(string.Format("Populating  cache data for cache table {0}, using light weight datalayer 2. ", strCachetable));
                    _logger.Debug(string.Format("Set page size = {0}. Total number of rows = {1}", CachePageSize, dataObjects2.Count));

                    if (dataObjects2.Count < CachePageSize)
                    {

                        List<SerializableDataObject> getdataObjects2 = _lwDataLayer2.Get(objectType);
                        foreach (SerializableDataObject dataObj in getdataObjects2)
                        {
                            DataRow newRow = table.NewRow();

                            int indexCounter = 0;
                            foreach (var pair in dataObj.Dictionary)
                            {
                                string val = String.Empty;
                                if (pair.Value != null)
                                    val = pair.Value.ToString();
             
                                if (indexCounter < objectType.dataProperties.Count() && (objectType.dataProperties[indexCounter].dataType.ToString()) == "String")
                                {
                                    if (pair.Value == null)
                                        newRow[pair.Key] = DBNull.Value;
                                    else
                                        newRow[pair.Key] = pair.Value;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(val))
                                        newRow[pair.Key] = DBNull.Value;
                                    else
                                        newRow[pair.Key] = pair.Value;
                                }

                                indexCounter++;

                            }

                            if (dataObj.HasContent)
                            {
                                newRow[BaseLightweightDataLayer.HAS_CONTENT] = true;
                            }
                            table.Rows.Add(newRow);
                        }

                        SqlBulkCopy bulkCopy = new SqlBulkCopy(_cacheConnStr);
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(table);

                        nRowCounter = (dataObjects2.Count <= CachePageSize) ? dataObjects2.Count : CachePageSize;
                        _logger.Debug(string.Format("Saving rows {0} to {1}  from table {2} to cache table {3}", start, start + nRowCounter, objectType.objectName, strCachetable));

                        table.Clear();
                        bulkCopy.Close();

                    }
                    else
                    {

                        while (start < dataObjects2.Count)
                        {
                            List<SerializableDataObject> data = dataObjects2.Skip(CachePageSize * pageIndex).Take(CachePageSize).ToList();
                            List<SerializableDataObject> getdataObjects2 = _lwDataLayer2.GetPage(objectType, data);
                            foreach (SerializableDataObject dataObj in getdataObjects2)
                            {
                                DataRow newRow = table.NewRow();

                                int indexCounter = 0;
                                foreach (var pair in dataObj.Dictionary)
                                {
                                    string val = String.Empty;
                                    if (pair.Value != null)
                                        val = pair.Value.ToString();

                                    if (indexCounter < objectType.dataProperties.Count() && (objectType.dataProperties[indexCounter].dataType.ToString()) == "String")
                                    {
                                        if (pair.Value == null)
                                            newRow[pair.Key] = DBNull.Value;
                                        else
                                            newRow[pair.Key] = pair.Value;
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(val))
                                            newRow[pair.Key] = DBNull.Value;
                                        else
                                            newRow[pair.Key] = pair.Value;
                                    }

                                    indexCounter++;

                                }
                                if (dataObj.HasContent)
                                {
                                    newRow[BaseLightweightDataLayer.HAS_CONTENT] = true;
                                }
                                table.Rows.Add(newRow);
                            }

                            SqlBulkCopy bulkCopy = new SqlBulkCopy(_cacheConnStr);
                            bulkCopy.DestinationTableName = tableName;
                            bulkCopy.WriteToServer(table);

                            nRowCounter = (dataObjects2.Count <= CachePageSize) ? dataObjects2.Count : CachePageSize;
                            _logger.Debug(string.Format("Saving rows {0} to {1}  from table {2} to cache table {3}", start, start + nRowCounter, objectType.objectName, strCachetable));

                            start += CachePageSize;
                            pageIndex = pageIndex + 1;
                            table.Clear();
                            bulkCopy.Close();

                        }
                    }
                    _logger.Debug(string.Format("Caching completed for cache table {0}", strCachetable));

                    string msg = "Cache data for [" + objectType.objectName + "] populated successfully.";
                    status.Messages.Add(msg);
                    response.Messages.Add(msg);

                }


            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;

                string error = "Error refreshing cache [" + objectType.objectName + "]: " + e.Message;
                _logger.Error(error);
                response.Messages.Add(error);
            }

            return response;
        }

    }



    public enum CacheState { Dirty, Busy, Ready }
}




