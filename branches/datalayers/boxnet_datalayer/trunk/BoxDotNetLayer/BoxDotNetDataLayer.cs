using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.IO;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using log4net;


//namespace org.iringtools.adapter.datalayer
namespace Bechtel.DataLayer
{
    public class BoxDotNetDataLayer : BaseDataLayer
    {
        private DataDictionary _dataDictionary = null;
        //   private DatabaseDictionary _dictionary = null;

        private string _applicationName = string.Empty;
        private string _projectName = string.Empty;
        private string _xmlPath = string.Empty;
        private string _baseDirectory = string.Empty;
        private string _keyDelimiter;

        //  NP Start
        private string _contentType;
        private long _totalCount;
        //  NP End

        string _authToken = string.Empty;
        string _appKey = string.Empty;
        string _baseUrl = string.Empty;
        string _searchUrl = string.Empty;
        string _uploadUrl = string.Empty;
        string _folderItemsUrl = string.Empty;

        IWebClient _webClient = null;
        IWebClient _webClient_Search = null; //  NP 

        private ILog _logger = LogManager.GetLogger(typeof(BoxDotNetDataLayer));

        public BoxDotNetDataLayer(AdapterSettings settings)
            : base(settings)
        {
            _xmlPath = _settings["xmlPath"];
            _projectName = _settings["projectName"];
            _applicationName = _settings["applicationName"];
            _baseDirectory = _settings["BaseDirectoryPath"];
            _authToken = _settings["AuthToken"];
            _appKey = _settings["AppKey"];
            _baseUrl = _settings["BaseUrl"];
            _searchUrl = _settings["SearchUrl"];
            _uploadUrl = _settings["UploadUrl"];
            _folderItemsUrl = _settings["FolderItemsUrl"];
            _keyDelimiter = Convert.ToString(_settings["DefaultKeyDelimiter"]) ?? string.Empty;

            //  NP
            _webClient = new IringWebClient(_baseUrl, _appKey, _authToken);

            //_webClient_Search = new IringWebClient(_searchUrl, _appKey, _authToken, _contentType);
            // _webClient = new WebClient(_baseUrl, _appKey, _authToken);

            //  LoadEndPointSettings();
            //   _selfUrl = GetSelfUrlList();

        }

        public override DataDictionary GetDictionary()
        {

            string Connectionstring = string.Empty;

            string path = String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName);

            try
            {
                if ((File.Exists(path)))
                {
                    dynamic DataDictionary = Utility.Read<DataDictionary>(path);
                    _dataDictionary = Utility.Read<DataDictionary>(path);
                    return _dataDictionary;
                }
                else
                {

                    _dataDictionary = CreateDataDictionary();

                    DatabaseDictionary _databaseDictionary = new DatabaseDictionary();
                    _databaseDictionary.dataObjects = _dataDictionary.dataObjects;
                    _databaseDictionary.ConnectionString = EncryptionUtility.Encrypt(Connectionstring);
                    _databaseDictionary.Provider = "dummy";
                    _databaseDictionary.SchemaName = "dummy";

                    Utility.Write<DatabaseDictionary>(_databaseDictionary, String.Format("{0}{1}DataBaseDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                    Utility.Write<DataDictionary>(_dataDictionary, String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                    return _dataDictionary;
                }
            }
            catch
            {
                string error = "Error in getting dictionary";
                //  _logger.Error(error);
                throw new ApplicationException(error);
            }
        }

        public override IList<IDataObject> Get(string objectType, DataFilter filter, int limit, int offset)
        {
            int lStart = offset;
            int lLimit = limit;
            IList<IDataObject> dataObjects = null;
            string filterString = string.Empty;

            if (filter == null || filter.Expressions.Count < 1)
                return dataObjects;

            try
            {
                // Get folders/files under a parent
                if (filter.Expressions.Count == 1 && filter.Expressions[0].PropertyName.Equals("parentId"))
                {
                    dataObjects = GetItemsUnderParent(objectType, filter, limit, offset);
                    return dataObjects;
                }

                string query = string.Empty;
                //  get query parameter(first filter expression) to perform search on Box
                query = filter.Expressions[0].Values.FirstOrDefault();
                //  Perform search on Box 
                DataTable dtSearch = Search(objectType, query, limit, offset);
                DataTable dtFilteredResult = new DataTable();

                //  getting filter expression
                if (filter.Expressions.Count > 1)
                {
                    filterString = filter.ToFilterExpression(_dataDictionary, objectType);
                    // removing first condition which is default query condition
                    filterString = filterString.Substring(filterString.IndexOf("AND") + 3).Trim();
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    DataRow[] result = dtSearch.Select(filterString);
                    if (result.Length > 0)
                    {
                        dtFilteredResult = dtSearch.Clone();
                        foreach (DataRow dr in result)
                        {
                            dtFilteredResult.ImportRow(dr);
                        }
                    }
                }
                else
                {
                    dtFilteredResult = dtSearch.Copy();
                }


                if (dtFilteredResult.Rows.Count > 0)
                {
                    //  Sorting 
                    if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
                    {
                        string orderExpression = filter.ToOrderExpression(_dataDictionary, objectType);
                        dtFilteredResult.DefaultView.Sort = orderExpression;
                        dtFilteredResult = dtFilteredResult.DefaultView.ToTable();
                    }

                    dataObjects = ToDataObjects(dtFilteredResult, objectType);

                    if (lStart >= dataObjects.Count)
                        lStart = dataObjects.Count;

                    if (lLimit == 0 || (lLimit + lStart) >= dataObjects.Count)
                        lLimit = dataObjects.Count - lStart;

                    dataObjects = ((List<IDataObject>)dataObjects).GetRange(lStart, lLimit);
                }


                if (dataObjects != null)
                    _totalCount = dataObjects.Count;
                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error get data table: " + ex);
                throw ex;
            }
            //  NP End
        }

        //  NP Start
        public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
            try
            {
                DataTable datatable = datatable = new DataTable();

                foreach (string identifier in identifiers)
                {
                    string url = GenerateUrl(objectType, identifier);
                    string jsonString = GetJsonResponseFrom(url);
                    //DataTable dt = GetDataTableFrom(jsonString, objectType);
                    DataTable dt = GetDataFrom(jsonString, objectType, false);  //DataTable dt = GetDataTableBasic(jsonString, objectType);

                    datatable.Merge(dt);
                }

                // Remove duplicate data from dataTable
                DataView dView = new DataView(datatable);
                string[] arrColumns = new string[datatable.Columns.Count];

                for (int i = 0; i < datatable.Columns.Count; i++)
                    arrColumns[i] = datatable.Columns[i].ColumnName;

                datatable = dView.ToTable(true, arrColumns);
                //------

                IList<IDataObject> dataObjects = ToDataObjects(datatable, objectType);
                if (dataObjects != null)
                    _totalCount = dataObjects.Count;
                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);
                throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            }
        }
        public override IList<IDataObject> Search(string objectType, string query, DataFilter filter = null, int pageSize = 30, int startIndex = 0)
        {
            #region commented
            ////string url = GetObjectUrl("search");
            ////url += objectType == "folders" ? query + "&type=folder" : query + "&type=file";

            //string url = _searchUrl;
            //if (objectType == Constants.ObjectName.Folders)
            //{
            //    url = url.Replace("{objectType}", Constants.SearchObjectType.folder.ToString());
            //}
            //else if (objectType == Constants.ObjectName.Files)
            //{
            //    url = url.Replace("{objectType}", Constants.SearchObjectType.file.ToString());
            //}

            //url = url.Replace("{queryValue}", query);
            //url = url.Replace("{limitValue}", pageSize.ToString());
            //url = url.Replace("{offsetValue}", startIndex.ToString());

            //string jsonString = GetJsonResponseFrom(url);

            ////DataTable datatable = GetDataTableBasic(jsonString, objectType); 
            //DataTable datatable = GetDataFrom(jsonString, objectType);
            #endregion commented

            DataTable datatable = Search(objectType, query);

            DataView dView = new DataView(datatable);
            string[] arrColumns = new string[datatable.Columns.Count];

            for (int i = 0; i < datatable.Columns.Count; i++)
                arrColumns[i] = datatable.Columns[i].ColumnName;

            datatable = dView.ToTable(true, arrColumns);

            IList<IDataObject> dataObjects = ToDataObjects(datatable, objectType);
            if (dataObjects != null)
                _totalCount = dataObjects.Count;
            return dataObjects;
        }
        public override long GetCount(string objectType, DataFilter filter)
        {
            try
            {
                IList<IDataObject> dataObjects = Get(objectType, filter, 30, 0);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetCount: " + ex);
                throw new Exception("Error while getting a count of type [" + objectType + "].", ex);
            }

            return _totalCount;
        }
        //  NP End

        public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
        {
            List<string> identifiers = null;
            try
            {
                identifiers = new List<string>();

                DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());

                //IList<string> keyCols = GetKeyColumns(objDef);

                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);
                foreach (IDataObject dataObject in dataObjects)
                {
                    identifiers.Add(Convert.ToString(dataObject.GetPropertyValue(objDef.keyProperties[0].keyPropertyName)));
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error while getting a filtered list of identifiers of type [{0}]: {1}", objectType, ex);
                throw new Exception("Error while getting a filtered list of identifiers of type [" + objectType + "].", ex);
            }

            return identifiers;
        }
        public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
        {
            return GetRelatedObjects(dataObject, relatedObjectType, 0, 0);
        }
        public override long GetRelatedCount(IDataObject dataObject, string relatedObjectType)
        {
            return GetRelatedObjects(dataObject, relatedObjectType, 0, 0).Count;
        }
        public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType, int pageSize, int startIndex)
        {
            string objectType = dataObject.GetType().Name;

            IList<IDataObject> dataObjects = null;

            if (objectType == typeof(GenericDataObject).Name)
            {
                objectType = ((GenericDataObject)dataObject).ObjectType;
            }

            try
            {
                DataObject parentDataObject = _dataDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());

                if (parentDataObject == null)
                    throw new Exception("Parent data object [" + objectType + "] not found.");

                DataObject relatedObjectDefinition = _dataDictionary.dataObjects.Find(x => x.objectName.ToUpper() == relatedObjectType.ToUpper());

                if (relatedObjectDefinition == null)
                    throw new Exception("Related data object [" + relatedObjectType + "] not found.");

                DataRelationship dataRelationship = parentDataObject.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedObjectDefinition.objectName.ToLower());
                if (dataRelationship == null)
                    throw new Exception("Relationship between data object [" + objectType + "] and related data object [" + relatedObjectType + "] not found.");

                //NP
                string keyFieldValueRelation = string.Empty;
                //NP

                DataFilter filter = null;
                foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
                {
                    filter = new DataFilter();
                    string keyFieldValue = Convert.ToString(dataObject.GetPropertyValue(propertyMap.dataPropertyName));

                    Expression expression = new Expression();
                    expression.LogicalOperator = LogicalOperator.And;
                    expression.RelationalOperator = RelationalOperator.EqualTo;

                    expression.PropertyName = propertyMap.relatedPropertyName;
                    expression.Values = new Values() { keyFieldValue };

                    filter.Expressions.Add(expression);

                    //NP
                    keyFieldValueRelation += keyFieldValue;
                    //NP
                }

                //NP
                string url = GetObjectUrl(relatedObjectType);

                //DataObject objDefparent = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());
                //DataObject objDefRelated = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == relatedObjectType.ToUpper());                                             
                //string identifierChild = "";

                url = GetUrlWithKeyValues(url, objectType, keyFieldValueRelation, true);
                //url = GetUrlWithKeyValues(url, objDefRelated, identifierArrayChild, false);
                url = url.EndsWith(@"/") ? url.Substring(0, url.Length - 1) : url;

                DataTable datatable = datatable = new DataTable();
                string jsonString = GetJsonResponseFrom(url);
                DataTable dt = GetDataTableFrom(jsonString, relatedObjectType);
                datatable.Merge(dt);

                // Remove duplicate data from dataTable
                DataView dView = new DataView(datatable);
                string[] arrColumns = new string[datatable.Columns.Count];

                for (int i = 0; i < datatable.Columns.Count; i++)
                    arrColumns[i] = datatable.Columns[i].ColumnName;

                datatable = dView.ToTable(true, arrColumns);
                //------

                dataObjects = ToDataObjects(datatable, relatedObjectType);

                //NP

                //dataObjects = Get(relatedObjectType, filter, 0, 0);

            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error while geting related data objects", ex);
                throw new Exception("Error while geting related data objects", ex);
            }

            return dataObjects;
        }
        public override Response Post(IList<IDataObject> dataObjects)
        {
            Response response = new Response();
            string objectType = String.Empty;

            string fileName = string.Empty;
            string parentId = string.Empty;
            string sourcePath = string.Empty;

            objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;
            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());

            if (dataObjects == null || dataObjects.Count == 0)
            {
                Status status = new Status();
                status.Level = StatusLevel.Warning;
                status.Messages.Add("Data object list provided is empty.");
                response.Append(status);
                return response;
            }

            foreach (IDataObject dataObject in dataObjects)
            {
                fileName = Convert.ToString(dataObject.GetPropertyValue("name"));
                parentId = Convert.ToString(dataObject.GetPropertyValue("folderId"));
                sourcePath = Convert.ToString(dataObject.GetPropertyValue("path"));
                sourcePath += fileName;

                string jsonString = string.Empty;

                try
                {
                    byte[] file = File.ReadAllBytes(sourcePath);
                    jsonString = ExecutePostCommand(fileName, sourcePath, file, parentId);
                }
                catch (Exception ex)
                {
                    _logger.ErrorFormat("Error while uploading file", "document");
                    //throw new Exception("Error while uploading file", ex);
                    Status status = new Status();
                    status.Level = StatusLevel.Error;
                    status.Messages.Add("Error uploading file." + ex.Message);
                    response.Append(status);
                }
            }

            #region commented

            // NP Box Upload start            
            //fileName = _settings["fileName_Upload"];
            //parentId = _settings["parentId_Upload"];
            //sourcePath = _settings["filePath_upload"];
            //sourcePath += fileName;

            //string jsonString = string.Empty;

            //try
            //{
            //    byte[] file = File.ReadAllBytes(sourcePath);
            //    jsonString = ExecutePostCommand(fileName, sourcePath, file, parentId);
            //}
            //catch (Exception ex)
            //{
            //    _logger.ErrorFormat("Error while uploading file", "document");
            //    throw new Exception("Error while uploading file", ex);
            //    Status status = new Status();
            //    status.Level = StatusLevel.Error;
            //    status.Messages.Add("Error uploading file." + ex.Message);
            //    response.Append(status);
            //}

            //string message = string.Empty;
            //if (!string.IsNullOrEmpty(jsonString))
            //{
            //    if (jsonString.IndexOf("{\"type\":\"error\",\"status\":409}") > 0)
            //    {
            //        //  file already exist
            //        Status status = new Status();
            //        status.Level = StatusLevel.Error;
            //        status.Messages.Add("Error uploading file. Item with the same name already exists");
            //        response.Append(status);
            //    }
            //    else if (jsonString.IndexOf("{\"type\":\"error\",\"status\":\"404\"}") > 0)
            //    {
            //        //  Error                
            //        Status status = new Status();
            //        status.Level = StatusLevel.Error;
            //        status.Messages.Add("Error uploading file.");
            //        response.Append(status);
            //    }
            //    else
            //    {
            //        // uploaded sucessfully
            //        Status status = new Status();
            //        status.Level = StatusLevel.Success;
            //        message = String.Format("Document [{0}] uploaded successfully.", fileName);
            //        status.Messages.Add(message);
            //        response.Append(status);
            //    }
            //}
            #endregion commented

            return response;

            // NP Box Upload end

            #region commented
            //string objectType = String.Empty;
            //bool isNew = false;
            //string identifier = String.Empty;

            //objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;
            //DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());

            //if (dataObjects == null || dataObjects.Count == 0)
            //{
            //    Status status = new Status();
            //    status.Level = StatusLevel.Warning;
            //    status.Messages.Add("Data object list provided is empty.");
            //    response.Append(status);
            //    return response;
            //}

            //try
            //{

            //    foreach (IDataObject dataObject in dataObjects)
            //    {
            //        identifier = String.Empty;
            //        Status status = new Status();
            //        string message = String.Empty;

            //        try
            //        {
            //            String objectString = FormJsonObjectString(dataObject);
            //            foreach (KeyProperty dataProperty in objDef.keyProperties)
            //            {
            //                string value = Convert.ToString(dataObject.GetPropertyValue(dataProperty.keyPropertyName));
            //                if (String.IsNullOrEmpty(value))
            //                    isNew = true;
            //                else
            //                    identifier = value;
            //                break;
            //            }
            //            if (!String.IsNullOrEmpty(identifier))
            //            {
            //                int count = Get(objectType, new List<string>() { identifier }).Count;
            //                if (count > 0)
            //                    isNew = false;
            //                else
            //                    isNew = true;
            //            }

            //            string url = GenerateUrl(objectType);
            //            if (isNew) ///Post data
            //            {
            //                _webClient.MakePostRequest(url, objectString);
            //            }
            //            else ///put data
            //            {
            //                _webClient.MakePutRequest(url, objectString);
            //            }

            //            message = String.Format("Data object [{0}] posted successfully.", identifier);
            //            status.Messages.Add(message);
            //            response.Append(status);

            //        }
            //        catch (Exception ex)
            //        {
            //            message = String.Format("Error while posting data object [{0}].", identifier);
            //            status.Messages.Add(message);
            //            response.Append(status);
            //        }

            //    }

            //}
            //catch (Exception ex)
            //{
            //    _logger.ErrorFormat("Error while processing a list of data objects of type [{0}]: {1}", objectType, ex);
            //    throw new Exception("Error while processing a list of data objects of type [" + objectType + "].", ex);
            //}

            //return response;

            #endregion commented
        }
        public override Response Delete(string objectType, DataFilter filter)
        {
            try
            {
                IList<string> identifiers = GetIdentifiers(objectType, filter);
                Response response = Delete(objectType, identifiers);
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error while deleting a list of data objects of type [{0}]: {1}", objectType, ex);
                throw new Exception("Error while deleting a list of data objects of type [" + objectType + "].", ex);
            }
        }
        public override Response Delete(string objectType, IList<string> identifiers)
        {
            Response response = new Response();

            if (identifiers == null || identifiers.Count == 0)
            {
                Status status = new Status();
                status.Level = StatusLevel.Warning;
                status.Messages.Add("Nothing to delete.");
                response.Append(status);
                return response;
            }

            try
            {
                foreach (string identifier in identifiers)
                {
                    Status status = new Status();
                    status.Identifier = identifier;
                    string message = String.Empty;
                    try
                    {
                        if (String.IsNullOrWhiteSpace(identifier))
                            throw new ApplicationException("Identifier can not be blank or null.");

                        string url = GenerateUrl(objectType, identifier);
                        _webClient.MakeDeleteRequest(url);

                        message = String.Format("DataObject [{0}] deleted successfully.", identifier);
                        status.Messages.Add(message);
                        response.Append(status);
                    }
                    catch
                    {
                        message = String.Format("Error while deleting dataObject [{0}].", identifier);
                        status.Messages.Add(message);
                        response.Append(status);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error while deleting a list of data objects of type [{0}]: {1}", objectType, ex);
                throw new Exception("Error while deleting a list of data objects of type [" + objectType + "].", ex);
            }

            return response;
        }


        #region Private function

        /// <summary>
        /// It will Load configration detail in a Dictionary object.
        /// </summary>
        //private void LoadEndPointSettings()
        //{

        //    string json ;//= _webClient.MakeGetRequest(_endPointUrl);

        //    JObject mainObject = JObject.Parse(json);

        //    //--- Add all resources and there url into globle dictionary object
        //    if (_objectyDictionary == null)
        //    {
        //        _objectyDictionary = new Dictionary<string, string>();

        //        foreach (JObject obj in (JArray)mainObject["resources"])
        //        {
        //            string resources = obj["resource"].Value<string>();
        //            string url = obj["url"].Value<string>();
        //            _objectyDictionary.Add(resources, url);
        //        }
        //    }

        //    //--
        //    _schemaUrl = mainObject["schemaurl"].Value<string>();
        //    _baseUrl = mainObject["baseurl"].Value<string>();


        //}

        //private IDictionary<string, string> GetSelfUrlList()
        //{
        //    IDictionary<string, string> selfUrlList = new Dictionary<string, string>();
        //    try
        //    {
        //        foreach (var dic in _objectyDictionary)
        //        {
        //            string objectName = dic.Key;
        //            string url = _schemaUrl.Replace("{resource}", dic.Key);
        //            string json = GetJsonResponseFrom(url);
        //            JObject schemaObject = JObject.Parse(json);

        //            foreach (JProperty propery in schemaObject.Properties())
        //            {
        //                if (propery.Name == "links")
        //                {
        //                    selfUrlList.Add(objectName, schemaObject["links"]["self"].Value<string>());
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error in loading data dictionary : " + ex);
        //        throw ex;
        //    }

        //    return selfUrlList;

        //}


        private DataDictionary CreateDataDictionary()
        {
            string path = String.Format("{0}{1}Configuration.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName);

            DataDictionary dataDictionary = null;

            try
            {
                if ((File.Exists(path)))
                {
                    dataDictionary = Utility.Read<DataDictionary>(path);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in loading configration file : " + ex);
                throw ex;
            }
            return dataDictionary;

        }

        /// <summary>
        /// It returns url for restfull service of specified object
        /// </summary>
        /// <param name="objectName">object name/table name</param>
        /// <returns></returns>
        private string GetObjectUrl(string objectName)
        {
            //TODO : Get url agaist objectName from _dataDictionary
            string url = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectName.ToUpper()).tableName;

            return url;
        }

        /// <summary>
        /// It will make a request on URL and retuen json string.
        /// </summary>
        private string GetJsonResponseFrom(string url)
        {
            try
            {
                return _webClient.MakeGetRequest(url);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // NP Box
        /// <summary>
        /// Get folders/files under a parent
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="filter"></param>
        /// <returns>IList<IDataObject></returns>
        private IList<IDataObject> GetItemsUnderParent(string objectType, DataFilter filter, int limit, int offset)
        {
            IList<IDataObject> dObjects = null;
            int lStart = offset;
            int lLimit = limit;

            if (filter.Expressions.Count == 1 && filter.Expressions[0].PropertyName.Equals("parentId"))
            {
                string parentId = filter.Expressions[0].Values.FirstOrDefault();
                //  Get url to find items under a folder
                string url = GenerateUrl(Constants.ObjectName.Folders) + _folderItemsUrl;
                url = url.Replace("{FOLDER_ID}", parentId);
                string jsonString = GetJsonResponseFrom(url);

                if (jsonString.IndexOf("{\"code\":\"not found.\",\"status\":\"404\"}") < 0)
                {
                    JObject o = JObject.Parse(jsonString);
                    //  Get comma separetd list of folder/file ids for the parent folder supplied
                    string sItems = objectType.Equals(Constants.ObjectName.Folders) ? GetValuesFromCollection(o, "", "|", Constants.SearchObjectType.folder, "entries", "id") : GetValuesFromCollection(o, "", "|", Constants.SearchObjectType.file, "entries", "id");

                    if (!string.IsNullOrEmpty(sItems))
                    {
                        IList<string> list = new List<string>();
                        string[] Items = sItems.Split(new char[] { '|' });
                        foreach (string sId in Items)
                        {
                            if (!string.IsNullOrEmpty(sId))
                                list.Add(sId);
                        }

                        //  Get with Id's 
                        dObjects = Get(objectType, list);

                        if (lStart >= dObjects.Count)
                            lStart = dObjects.Count;

                        if (lLimit == 0 || (lLimit + lStart) >= dObjects.Count)
                            lLimit = dObjects.Count - lStart;

                        dObjects = ((List<IDataObject>)dObjects).GetRange(lStart, lLimit);

                        if (dObjects != null)
                            _totalCount = dObjects.Count;

                        return dObjects;
                    }
                }
            }

            return dObjects;
        }

        private DataTable Search(string objectType, string query, int pageSize = 30, int startIndex = 0)
        {
            string url = _searchUrl;
            if (objectType == Constants.ObjectName.Folders)
            {
                url = url.Replace("{objectType}", Constants.SearchObjectType.folder.ToString());
            }
            else if (objectType == Constants.ObjectName.Files)
            {
                url = url.Replace("{objectType}", Constants.SearchObjectType.file.ToString());
            }

            url = url.Replace("{queryValue}", query);
            url = url.Replace("{limitValue}", pageSize.ToString());
            url = url.Replace("{offsetValue}", startIndex.ToString());

            string jsonString = GetJsonResponseFrom(url);

            //DataTable datatable = GetDataTableBasic(jsonString, objectType); 
            return GetDataFrom(jsonString, objectType);
        }

        private string GetUrlWithKeyValues(string url, string objectType, string identifier, bool IsParent)
        {
            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());
            string[] identifierArray = identifier.Split(_keyDelimiter.ToCharArray());

            if (identifierArray.Count() != objDef.keyProperties.Count)
                throw new Exception("key fields are not matching with their values.");

            for (int i = 0; i < objDef.keyProperties.Count; i++)
            {
                if (IsParent)
                    url = url.Replace("{" + objDef.keyProperties[i].keyPropertyName + "}", identifierArray[i]);
                else
                {
                    if (!String.IsNullOrEmpty(identifierArray[i]))
                        url += "/" + identifierArray[i];
                }
            }
            return url;
        }

        private string FormJsonObjectString(IDataObject dataObject)
        {
            string objectType = ((GenericDataObject)dataObject).ObjectType;
            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            if (objDef != null)
            {


                JsonWriter jsonWriter = new JsonTextWriter(sw);
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.WriteStartObject();

                foreach (var entry in ((GenericDataObject)dataObject).Dictionary)
                {
                    jsonWriter.WritePropertyName(entry.Key);
                    jsonWriter.WriteValue(entry.Value);
                }
                jsonWriter.WriteEndObject();
                jsonWriter.Close();
                sw.Close();
            }
            return sw.ToString();
        }

        #region commented
        //private void PostData()
        //{
        //    WebRequest request = (HttpWebRequest)WebRequest.Create("https://upload.box.com/api/2.0/files/content");
        //    request.Method = "POST";
        //    request.Timeout = 600000;

        //    request.UseDefaultCredentials = true;

        //    request.Headers.Add("Authorization", "Bearer M4budHVDmT8ikpAoU3pcQAdpGyzyhOId");

        //    byte[] byteArray = File.ReadAllBytes(@"C:\Users\npandey1\Desktop\changes.txt");

        //    //string text = Convert.ToBase64String(byteArray);            

        //    ASCIIEncoding encoding = new ASCIIEncoding();
        //    string postData = "file_name=changes.txt&parent_id=778939632&share=1&emails[]=test@domain.com&new_file=1";
        //    byte[] parametres = Encoding.UTF8.GetBytes(postData);

        //    //_webClient = new IringWebClient("https://upload.box.com/api/2.0/files/content", _appKey, _authToken);
        //    //_webClient.MakePostRequest("https://upload.box.com/api/2.0/files/content", text);

        //    request.ContentType = "multipart/form-data";
        //    request.ContentLength = byteArray.Length + parametres.Length;

        //    Stream dataStream = request.GetRequestStream();

        //    // send the parametres
        //    dataStream.Write(parametres, 0, parametres.Length);
        //    dataStream.Write(byteArray, 0, byteArray.Length);
        //    dataStream.Close();

        //    WebResponse response = request.GetResponse();
        //    dataStream = response.GetResponseStream();
        //    StreamReader reader = new StreamReader(dataStream);
        //    string responseFromServer = reader.ReadToEnd();
        //    reader.Close();
        //    dataStream.Close();
        //    response.Close();
        //}
        #endregion commented

        protected string ExecutePostCommand(string fileName, string filenameWithPath, byte[] file, string parentId)
        {
            string sResponse = string.Empty;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_uploadUrl);

            request.PreAuthenticate = true;
            request.AllowWriteStreamBuffering = true;
            string boundary = System.Guid.NewGuid().ToString();
            request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            request.Method = "POST";

            //  PrepareHeaders
            request.Headers.Add("Authorization", _authToken);

            // Build Contents for Post 
            string header = string.Format("--{0}", boundary);
            string footer = header + "--";
            StringBuilder contents = new StringBuilder();

            // file 
            contents.AppendLine(header);
            contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"", fileName));
            contents.AppendLine("Content-Type: text/plain");
            contents.AppendLine();
            contents.AppendLine(Encoding.UTF8.GetString(file));
            // name 
            contents.AppendLine(header);
            contents.AppendLine("Content-Disposition: form-data; name=\"file_name\"");
            contents.AppendLine();
            contents.AppendLine(fileName);
            // parent_id 
            contents.AppendLine(header);
            contents.AppendLine("Content-Disposition: form-data; name=\"parent_id\"");
            contents.AppendLine();
            contents.AppendLine(parentId);
            // Footer 
            contents.AppendLine(footer);

            // This is sent to the Post 
            byte[] bytes = Encoding.UTF8.GetBytes(contents.ToString());
            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Flush();
                requestStream.Close();

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        sResponse = reader.ReadToEnd();
                    }
                }
            }

            return sResponse;

        }
       
        private DataTable GetDataFrom(string jsonString, string objectType, bool isSearch = true)
        {
            DataTable dtResult = new DataTable();

            if (jsonString.IndexOf("{\"code\":\"not found.\",\"status\":\"404\"}") < 0)
            {
                if (isSearch == true)
                {
                    JObject jsonObj = JObject.Parse(jsonString);

                    if (jsonObj.GetValue("  ") != null || int.Parse(jsonObj.GetValue("total_count").ToString()) > 0)
                    {
                        JArray item_array = (JArray)jsonObj["entries"];

                        for (int i = 0; i < item_array.Count; i++)
                        {
                            GetRecordFromJson(item_array[i].ToString(), objectType, dtResult, i);
                        }
                    }
                }
                else
                {
                    GetRecordFromJson(jsonString, objectType, dtResult, 0);
                }
                _totalCount = dtResult.Rows.Count;
            }

            return dtResult;
        }

        private void GetRecordFromJson(string jsonString, string objectType, DataTable dtResult, int recordIndex = 0)
        {
            if (jsonString.IndexOf("{\"code\":\"not found.\",\"status\":\"404\"}") < 0)
            {
                JObject o = JObject.Parse(jsonString);
                IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();
               
                #region Add column names in datatable
                // Add column names in datatable for all property names
                if (dtResult.Rows.Count <= 0)
                {
                    GetDataTableSchema(objectType, dtResult); ;
                    #region commented
                    //foreach (string pName in propertyNames)
                    //{
                    //    dtResult.Columns.Add(pName, typeof(string));
                    //}
                    //dtResult.TableName = objectType;
                    #endregion commented
                }
                #endregion Add column names in datatable
                
                dtResult.Rows.Add();
                string colName = "name";

                foreach (DataColumn dtColumnName in dtResult.Columns)
                {
                    string pName = dtColumnName.ColumnName;
                    // pick specific property from this inner objects
                    if (pName == "created_by" || pName == "modified_by" || pName == "shared_link" || pName == "path_collection" || /* pName == "item_collection" ||*/ pName == "parent")
                    {
                        if (pName == "shared_link")
                            colName = "url";
                        if (pName == "parent")
                            colName = "name";
                        //if (pName == "item_collection")
                        //    colName = "total_count";  

                        if (pName == "path_collection")
                        {
                            string sPath = GetValuesFromCollection(o, "path_collection", @"\");
                            dtResult.Rows[recordIndex][pName] = sPath;
                        }
                        else
                        {
                            if (o[pName].HasValues)
                                dtResult.Rows[recordIndex][pName] = o[pName][colName].ToString();
                        }
                    }
                    else
                    {
                        dtResult.Rows[recordIndex][pName] = o[pName].ToString();
                    }
                }
            }
        }

        private string GetValuesFromCollection(JObject oJObject, string CollectionName, string Seperator, Constants.SearchObjectType itemType = Constants.SearchObjectType.folder, string sArrayName = "entries", string sItemName = "name")
        {
            string svalue = string.Empty;

            JArray items = string.IsNullOrEmpty(CollectionName) ? (JArray)oJObject[sArrayName] : (JArray)oJObject[CollectionName][sArrayName];
            if (items.Count > 0)
            {
                DataTable dtItems = items.ToObject<DataTable>();
                if (dtItems.Rows.Count <= 1)
                    Seperator = string.Empty;

                foreach (DataRow dr in dtItems.Rows)
                {
                    if (dr["type"].ToString().Equals(itemType.ToString()))
                        svalue += dr[sItemName].ToString() + @Seperator;
                }
                svalue = svalue.Substring(0, svalue.Length - 1);
            }

            return svalue;
        }

        private DataTable GetDataTableBasic(string jsonString, string objectType)
        {
            DataTable dt = new DataTable();

            if (jsonString.IndexOf("{\"code\":\"not found.\",\"status\":\"404\"}") < 0)
            {
                JObject o = JObject.Parse(jsonString);

                IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

                // Add column names in datatable for all property names
                foreach (string pName in propertyNames)
                {
                    dt.Columns.Add(pName, typeof(string));
                }

                dt.Rows.Add();
                string colName = "name";

                foreach (string pName in propertyNames)
                {
                    // pick specific property from this inner objects
                    if (pName == "created_by" || pName == "modified_by" || pName == "shared_link" || pName == "path_collection" || /* pName == "item_collection" ||*/ pName == "parent")
                    {
                        if (pName == "shared_link")
                            colName = "url";
                        if (pName == "parent")
                            colName = "name";
                        //if (pName == "item_collection")
                        //    colName = "total_count";  

                        if (pName == "path_collection")
                        {
                            string sPath = GetValuesFromCollection(o, "path_collection", @"\");
                            dt.Rows[0][pName] = sPath;
                        }
                        else
                        {
                            if (o[pName].HasValues)
                                dt.Rows[0][pName] = o[pName][colName].ToString();
                        }
                    }
                    else
                    {
                        dt.Rows[0][pName] = o[pName].ToString();
                    }
                }

                //JArray items = (JArray)o[collectionName];
                //DataTable dt = items.ToObject<DataTable>();
                dt.TableName = objectType;

            }

            return dt;

        }


        //public long GetItemsCount(string objectType, string folderId)
        //{
        //    string url = GetObjectUrl(objectType);
        //    url = url + @"/"+folderId+@"/items";         //url = url + @"?offset=0&limit=1";            
        //    string jsonString = GetJsonResponseFrom(url);

        //    try
        //    {
        //        if (jsonString.IndexOf("{\"status_text\":\"Record Not Found.\",\"status_code\":\"202\"}") >= 0)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            JObject o = JObject.Parse(jsonString);
        //            long count = Convert.ToInt64(o["total_count"].ToString());

        //            return count;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error in GetCount: " + ex);
        //        throw new Exception("Error while getting a count of type [" + objectType + "].", ex);
        //    }
        //}

        // NP Box

        private DataTable GetDataTableFrom(string jsonString, string objectType, string collectionName = "Items")
        {
            // if (jsonString.IndexOf("{\"status_text\":\"Record Not Found.\",\"status_code\":\"202\"}") >= 0)
            if (jsonString.IndexOf("{\"total\":0,\"limit\":0,\"Items\":[]}") >= 0)
            {
                return GetDataTableSchema(objectType);
            }
            else
            {
                // NP
                collectionName = jsonString.Substring(jsonString.IndexOf("Items", StringComparison.CurrentCultureIgnoreCase), "Items".Length);
                // NP

                JObject o = JObject.Parse(jsonString);
                JArray items = (JArray)o[collectionName];
                DataTable dt = items.ToObject<DataTable>();
                dt.TableName = objectType;
                return dt;
            }
        }

        private DataTable GetDataTableSchema(string objectType, DataTable dtResults = null)
        {
            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());
            DataTable dataTable = new DataTable();
            dataTable.TableName = objectType;
            foreach (DataProperty property in objDef.dataProperties)
            {
                DataColumn dataColumn = new DataColumn();
                dataColumn.ColumnName = property.columnName;
                dataColumn.DataType = Type.GetType("System." + property.dataType.ToString());

                //  dataTable.Columns.Add(dataColumn);
                //  NP
                if (dtResults == null)
                    dataTable.Columns.Add(dataColumn);
                else
                    dtResults.Columns.Add(dataColumn);
            }

            //return dataTable;
            //  NP 
            if (dtResults == null)
                return dataTable;
            else
                return dtResults;
            //  NP
        }

        private string GenerateUrl(string objectType, DataFilter filter, int limit, int offset)
        {
            string url = GetObjectUrl(objectType);
            if ((limit == 0) || (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0))
            {
                url = url + @"?offset=" + Convert.ToString(0) + @"&limit=" + Convert.ToString(10000000);
            }
            else
            {
                url = url + @"?offset=" + Convert.ToString(offset) + @"&limit=" + Convert.ToString(limit);
            }

            return url;
        }

        private string GenerateUrl(string objectType, string identifier)
        {
            string url = GetObjectUrl(objectType);

            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());

            string[] identifierArray = identifier.Split(_keyDelimiter.ToCharArray());

            if (identifierArray.Count() != objDef.keyProperties.Count)
                throw new Exception("key fields are not matching with their values.");

            for (int i = 0; i < objDef.keyProperties.Count; i++)
            {
                url = url + "/" + identifierArray[i].ToString();
            }

            return url;
        }

        private string GenerateUrl(string objectType)
        {
            return GetObjectUrl(objectType);
        }

        private string GenerateReletedUrl(string parentObject, string pId, string relatedObject)
        {
            string url = GetObjectUrl(parentObject);
            url = url + "//" + pId + "//" + relatedObject;
            return url;
        }

        private string GenerateReletedUrl(string parentObject, string pId, string relatedObject, int limit, int offset)
        {
            string url = GenerateReletedUrl(parentObject, pId, relatedObject);

            if (limit == 0)
            {
                url = url + @"?offset=" + Convert.ToString(0) + @"&limit=" + Convert.ToString(10000000);
            }
            else
            {
                url = url + @"?offset=" + Convert.ToString(offset) + @"&limit=" + Convert.ToString(limit);
            }

            return url;
        }

        private long GetObjectCount(string objectType, DataFilter filter)
        {
            string url = GetObjectUrl(objectType);
            url = url + @"?offset=0&limit=1";
            string jsonString = GetJsonResponseFrom(url);

            if (jsonString.IndexOf("{\"status_text\":\"Record Not Found.\",\"status_code\":\"202\"}") >= 0)
            {
                return 0;
            }
            else
            {
                JObject o = JObject.Parse(jsonString);
                long count = Convert.ToInt64(o["total"].ToString());

                return count;
            }
        }

        private DataType ResolveDataType(string type)
        {
            switch (type)
            {
                case "number":
                    return DataType.Int32;
                case "date":
                    return DataType.DateTime;
                case "string":
                default:
                    return DataType.String;
            }

        }

        private IList<IDataObject> ToDataObjects(DataTable dataTable, string objectType)
        {
            return ToDataObjects(dataTable, objectType, false);
        }

        private IList<IDataObject> ToDataObjects(DataTable dataTable, string objectType, bool createsIfEmpty)
        {
            IList<IDataObject> dataObjects = new List<IDataObject>();
            // DataObject objectDefinition = GetObjectDefinition(objectType);
            DataObject objectDefinition = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());
            IDataObject dataObject = null;

            if (objectDefinition != null && dataTable.Rows != null)
            {
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        try
                        {
                            dataObject = ToDataObject(dataRow, objectDefinition);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Error converting data row to data object: " + ex);
                            throw ex;
                        }

                        if (dataObjects != null)
                        {
                            dataObjects.Add(dataObject);
                        }
                    }
                }
                else if (createsIfEmpty)
                {
                    dataObject = ToDataObject(null, objectDefinition);
                    dataObjects.Add(dataObject);
                }
            }

            return dataObjects;
        }

        private IDataObject ToDataObject(DataRow dataRow, DataObject objectDefinition)
        {
            IDataObject dataObject = null;

            if (dataRow != null)
            {
                try
                {
                    dataObject = new GenericDataObject() { ObjectType = objectDefinition.objectName };
                }
                catch (Exception ex)
                {
                    _logger.Error("Error instantiating data object: " + ex);
                    throw ex;
                }

                if (dataObject != null && objectDefinition.dataProperties != null)
                {
                    foreach (DataProperty objectProperty in objectDefinition.dataProperties)
                    {
                        try
                        {
                            if (dataRow.Table.Columns.Contains(objectProperty.columnName))
                            {
                                object value = dataRow[objectProperty.columnName];

                                if (value.GetType() == typeof(System.DBNull))
                                {
                                    value = null;
                                }

                                dataObject.SetPropertyValue(objectProperty.propertyName, value);
                            }
                            else
                            {
                                _logger.Warn(String.Format("Value for column [{0}] not found in data row of table [{1}]",
                                  objectProperty.columnName, objectDefinition.tableName));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Error getting data row value: " + ex);
                            throw ex;
                        }
                    }
                }
            }
            else
            {
                dataObject = new GenericDataObject() { ObjectType = objectDefinition.objectName };

                foreach (DataProperty objectProperty in objectDefinition.dataProperties)
                {
                    dataObject.SetPropertyValue(objectProperty.propertyName, null);
                }
            }

            return dataObject;
        }

        

        #endregion    
    }

    public static class DataFilterExtension
    {
        public static string ToFilterExpression(this DataFilter dataFilter, DataDictionary dataDictionary, string objectName)
        {
            StringBuilder filterUrl = new StringBuilder();
            DataObject dataObject = null;

            try
            {
                dataObject = dataDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectName.ToUpper());
                if (dataObject == null)
                {
                    throw new Exception("Data object not found.");
                }

                if (dataFilter != null && dataFilter.Expressions != null && dataFilter.Expressions.Count > 0)
                {
                    foreach (Expression expression in dataFilter.Expressions)
                    {
                        if (filterUrl.Length <= 0) // To avoid adding logical operator at starting.
                        {
                            expression.LogicalOperator = org.iringtools.library.LogicalOperator.None;
                        }

                        string sqlExpression = ResolveFilterExpression(dataObject, expression);
                        filterUrl.Append(sqlExpression);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating filter url .", ex);
            }

            //return filterUrl.ToString();
            return filterUrl.ToString().Replace("&", " AND ");
        }

        #region Private Methods

        private static string ResolveFilterExpression(DataObject dataObject, Expression expression)
        {
            string propertyName = expression.PropertyName;
            DataProperty dataProperty = null;

            dataProperty = dataObject.dataProperties.Find(x => x.propertyName.ToUpper() == propertyName.ToUpper());

            if (dataProperty == null)
            {
                throw new Exception("Data property [" + expression.PropertyName + "] not found.");
            }

            DataType propertyType = dataProperty.dataType;
            string columnName = dataProperty.columnName;
            string qualColumnName = columnName;

            bool isString = (propertyType == DataType.String || propertyType == DataType.Char);
            StringBuilder filterExpression = new StringBuilder();

            if (expression.LogicalOperator != LogicalOperator.None)
            {
                string logicalOperator = ResolveLogicalOperator(expression.LogicalOperator);
                filterExpression.Append("" + logicalOperator + "");
            }

            string value = String.Empty;

            switch (expression.RelationalOperator)
            {
                case RelationalOperator.EqualTo:
                    //  NP Start
                    //   filterExpression.Append(qualColumnName + "=" + expression.Values.FirstOrDefault());
                    if (isString)
                    {
                        filterExpression.Append(qualColumnName + "='" + expression.Values.FirstOrDefault() + "'");
                    }
                    else if (propertyType == DataType.Int32)
                    {
                        filterExpression.Append(qualColumnName + "=" + expression.Values.FirstOrDefault());
                    }
                    //  NP Start

                    break;
                case RelationalOperator.NotEqualTo:
                case RelationalOperator.StartsWith:
                case RelationalOperator.EndsWith:
                case RelationalOperator.GreaterThan:
                case RelationalOperator.GreaterThanOrEqual:
                case RelationalOperator.LesserThan:
                case RelationalOperator.LesserThanOrEqual:
                case RelationalOperator.In:
                case RelationalOperator.Contains:
                default:
                    throw new Exception("Relational operator [" + expression.RelationalOperator + "] not supported.");
            }

            return filterExpression.ToString();
        }

        //public static string ToFilterExpression(this DataFilter dataFilter, DataDictionary dataDictionary, string objectName)
        //{
        //    StringBuilder filterUrl = new StringBuilder();
        //    DataObject dataObject = null;

        //    try
        //    {
        //        dataObject = dataDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectName.ToUpper());
        //        if (dataObject == null)
        //        {
        //            throw new Exception("Data object not found.");
        //        }

        //        if (dataFilter != null && dataFilter.Expressions != null && dataFilter.Expressions.Count > 0)
        //        {
        //            foreach (Expression expression in dataFilter.Expressions)
        //            {
        //                if (filterUrl.Length <= 0) // To avoid adding logical operator at starting.
        //                {
        //                    expression.LogicalOperator = org.iringtools.library.LogicalOperator.None;
        //                }

        //                string sqlExpression = ResolveFilterExpression(dataObject, expression);
        //                filterUrl.Append(sqlExpression);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error generating filter url .", ex);
        //    }

        //    return filterUrl.ToString();
        //}

        //private static string ResolveFilterExpression(DataObject dataObject, Expression expression)
        //{
        //    string propertyName = expression.PropertyName;
        //    DataProperty dataProperty = null;

        //    dataProperty = dataObject.dataProperties.Find(x => x.propertyName.ToUpper() == propertyName.ToUpper());

        //    if (dataProperty == null)
        //    {
        //        throw new Exception("Data property [" + expression.PropertyName + "] not found.");
        //    }

        //    DataType propertyType = dataProperty.dataType;
        //    string columnName = dataProperty.columnName;
        //    string qualColumnName = columnName;

        //    bool isString = (propertyType == DataType.String || propertyType == DataType.Char);
        //    StringBuilder filterExpression = new StringBuilder();

        //    if (expression.LogicalOperator != LogicalOperator.None)
        //    {
        //        string logicalOperator = ResolveLogicalOperator(expression.LogicalOperator);
        //        filterExpression.Append("" + logicalOperator + "");
        //    }

        //    string value = String.Empty;

        //    switch (expression.RelationalOperator)
        //    {
        //        case RelationalOperator.EqualTo:
        //            filterExpression.Append(qualColumnName + "=" + expression.Values.FirstOrDefault());
        //            break;
        //        case RelationalOperator.NotEqualTo:
        //        case RelationalOperator.StartsWith:
        //        case RelationalOperator.EndsWith:
        //        case RelationalOperator.GreaterThan:
        //        case RelationalOperator.GreaterThanOrEqual:
        //        case RelationalOperator.LesserThan:
        //        case RelationalOperator.LesserThanOrEqual:
        //        case RelationalOperator.In:
        //        case RelationalOperator.Contains:
        //        default:
        //            throw new Exception("Relational operator [" + expression.RelationalOperator + "] not supported.");
        //    }

        //    return filterExpression.ToString();
        //}

        private static string ResolveLogicalOperator(LogicalOperator logicalOperator)
        {
            switch (logicalOperator)
            {
                case LogicalOperator.And:
                    return "&";
                default:
                    throw new Exception("Logical operator [" + logicalOperator + "] not supported.");
            }
        }

        private static string ResolveOrderExpression(OrderExpression orderExpression, string qualColumnName)
        {
            StringBuilder sqlExpression = new StringBuilder();

            switch (orderExpression.SortOrder)
            {
                case SortOrder.Asc:
                    sqlExpression.Append(qualColumnName + " ASC");
                    break;

                case SortOrder.Desc:
                    sqlExpression.Append(qualColumnName + " DESC");
                    break;

                default:
                    throw new Exception("Sort order is not specified.");
            }

            return sqlExpression.ToString();
        }
        public static string ToOrderExpression(this DataFilter dataFilter, DataDictionary dataDictionary, string objectName)
        {

            DataObject dataObject = null;
            dataObject = dataDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectName.ToUpper());
            try
            {
                if (dataObject == null)
                {
                    throw new Exception("Data object not found.");
                }

                StringBuilder orderExpression = new StringBuilder();

                if (dataFilter.OrderExpressions != null && dataFilter.OrderExpressions.Count > 0)
                {
                    foreach (OrderExpression expression in dataFilter.OrderExpressions)
                    {
                        string propertyName = expression.PropertyName;
                        DataProperty dataProperty = null;

                        dataProperty = dataObject.dataProperties.Find(x => x.propertyName.ToUpper() == propertyName.ToUpper());

                        string orderStatement = ResolveOrderExpression(expression, dataProperty.columnName);
                        orderExpression.Append("," + orderStatement);
                    }
                }

                if (orderExpression.Length > 0)
                    orderExpression.Remove(0, 1);

                return orderExpression.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating order expression.", ex);
            }
        }

        #endregion
    }
}
