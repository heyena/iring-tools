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
//using WebClient = System.Net.Http;


namespace Bechtel.DataLayer
{
    public class RestDataLayer:BaseDataLayer
    {
        private DataDictionary _dataDictionary = null;
     //   private DatabaseDictionary _dictionary = null;

        private string _applicationName = string.Empty;
        private string _projectName = string.Empty;
        private string _xmlPath = string.Empty;
        private string _baseDirectory = string.Empty;
        private string _keyDelimiter;

        string _authToken = string.Empty;
        string _appKey = string.Empty;
        string _baseUrl = string.Empty;
        
        IWebClient _webClient = null;

        private ILog _logger = LogManager.GetLogger(typeof(RestDataLayer));


        private Dictionary<string, string> _objectyDictionary = null;

        public RestDataLayer(AdapterSettings settings):base(settings)
        {
            _xmlPath = _settings["xmlPath"];
            _projectName = _settings["projectName"];
            _applicationName = _settings["applicationName"];
            _baseDirectory = _settings["BaseDirectoryPath"];
            _authToken = _settings["AuthToken"];
            _appKey = _settings["AppKey"];
            _baseUrl = _settings["BaseUrl"];
            _keyDelimiter = Convert.ToString(_settings["DefaultKeyDelimiter"]) ?? string.Empty;
            _objectyDictionary = LoadEndPointInDictionary();

            _webClient = new IringWebClient(_baseUrl, _appKey, _authToken);
         //  _webClient = new WebClient(_baseUrl, _appKey, _authToken);
            //_webClient = new MockWebClient(_baseUrl, _appKey, _authToken);

        }

        public override IList<IDataObject> Get(string objectType, DataFilter filter, int limit, int start)
        {
            int lStart = start;
            int lLimit = limit;
            IList<IDataObject> dataObjects = null;

            try
            {
               
                string url = GenerateUrl(objectType, filter, limit, start);

                string filterUrl = filter.ToFilterExpression(_dataDictionary, objectType);
                if (!String.IsNullOrEmpty(filterUrl))
                {
                    url = url + "&" + filterUrl;
                }

                string jsonString = GetJsonResponseFrom(url);
                DataTable dataTable = GetDataTableFrom(jsonString, objectType);

                
                //Sorting 
                if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
                {
                    string orderExpression = filter.ToOrderExpression(_dataDictionary, objectType);
                    dataTable.DefaultView.Sort = orderExpression;
                    dataTable = dataTable.DefaultView.ToTable();
                    
                    dataObjects = ToDataObjects(dataTable, objectType);

                    if (lStart >= dataObjects.Count)
                        lStart = dataObjects.Count;

                    if (lLimit == 0 || (lLimit + lStart) >= dataObjects.Count)
                        lLimit = dataObjects.Count - lStart;

                    dataObjects = ((List<IDataObject>)dataObjects).GetRange(lStart, lLimit);
                    

                }
                else
                {
                    dataObjects = ToDataObjects(dataTable, objectType);
                }

                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error get data table: " + ex);
                throw ex;
            }
        }

        public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
            try
            {
                string url = GenerateUrl(objectType, identifiers);
                string jsonString = GetJsonResponseFrom(url);
                DataTable datatable = GetDataTableFrom(jsonString, objectType);
                IList<IDataObject> dataObjects = ToDataObjects(datatable, objectType);
                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);
                throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            }
        }

        public override long GetCount(string objectType, DataFilter filter)
        {
            try
            {
                try
                {
                    return GetObjectCount(objectType, filter);
                }
                catch
                {
                    IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);
                    return dataObjects.Count();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetCount: " + ex);
                throw new Exception("Error while getting a count of type [" + objectType + "].", ex);
            }
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

                    _dataDictionary = LoadDataObjects();

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

                string pID = Convert.ToString(dataObject.GetPropertyValue(parentDataObject.keyProperties[0].ToString()));

                string url = GenerateReletedUrl(objectType, pID, relatedObjectType, pageSize, startIndex);


                string jsonString = GetJsonResponseFrom(url);
                DataTable dataTable = GetDataTableFrom(jsonString, objectType);
                dataObjects = ToDataObjects(dataTable, objectType);

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
            bool isNew = false;
            string identifier = String.Empty; 
             
            objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;
            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());

            if (dataObjects == null || dataObjects.Count == 0)
            {
                Status status = new Status();
                status.Level = StatusLevel.Warning;
                status.Messages.Add("Data object list provided was empty.");
                response.Append(status);
                return response;
            }

            try
            {

                foreach (IDataObject dataObject in dataObjects)
                {
                    identifier = String.Empty;
                    Status status = new Status();
                    string message = String.Empty;

                    try
                    {
                        String objectString = FormJsonObjectString(dataObject);
                        foreach (KeyProperty dataProperty in objDef.keyProperties)
                        {
                            string value = Convert.ToString(dataObject.GetPropertyValue(dataProperty.keyPropertyName));
                            if (String.IsNullOrEmpty(value))
                                isNew = true;
                            else
                                identifier = value;

                            break;
                        }
                        if (!String.IsNullOrEmpty(identifier))
                        {
                            int count = Get(objectType, new List<string>() { identifier }).Count;
                            if (count > 0)
                                isNew = false;
                            else
                                isNew = true;
                        }

                        string url = GenerateUrl(objectType);
                        if (isNew) ///Post data
                        {
                            _webClient.MakePostRequest(url, objectString);
                        }
                        else ///put data
                        {
                            _webClient.MakePutRequest(url, objectString);
                        }

                        message = String.Format("Data object [{0}] posted successfully.", identifier);
                        status.Messages.Add(message);
                        response.Append(status);

                    }
                    catch (Exception ex)
                    {
                        message = String.Format("Error while posting data object [{0}].", identifier);
                        status.Messages.Add(message);
                        response.Append(status);
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error while processing a list of data objects of type [{0}]: {1}", objectType, ex);
                throw new Exception("Error while processing a list of data objects of type [" + objectType + "].", ex);
            }

            return response;
        }

        private string FormJsonObjectString(IDataObject dataObject)
        {
            string objectType = ((GenericDataObject)dataObject).ObjectType;
            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            if (objDef !=null)
            {

                
                 JsonWriter jsonWriter = new JsonTextWriter(sw);
                 jsonWriter.Formatting = Formatting.Indented;
                 jsonWriter.WriteStartObject();

                foreach(var entry in ((GenericDataObject)dataObject).Dictionary)
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

                        string url = GenerateUrl(objectType, new List<string>() { identifier });
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
        private Dictionary<string, string> LoadEndPointInDictionary()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                foreach (var obj in _settings)
                {
                    var abc = (string)obj;
                    if (abc.StartsWith(Constants.OBJECT_PREFIX))
                        dict.Add(abc, _settings[abc].ToString());
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return dict;

        }

        /// <summary>
        /// It will Parse json string and then fill a list with their properties
        /// </summary>
        private void FillDataPropertiesFrom(string jsonString, List<DataProp> dataPrpCollection, string objectName)
        {

            List<DataProp> dataPrpCollectionTemp = new List<DataProp>();

            JObject o = JObject.Parse(jsonString);
            JArray items = (JArray)o["Items"];
            JObject item = (JObject)items[0];
            bool isKeyAssigned = false;

            foreach (var jp in item.Properties())
            {
                DataProp dp = new DataProp();
                dp.Object_Name = objectName;
                dp.columnName = jp.Name;
                dp.propertyName = jp.Name;
                dp.keyType = "unassigned";
                dp.isNullable = "false";

                if (dp.columnName.ToUpper() == "ID" && isKeyAssigned == false)
                {
                    isKeyAssigned = true;
                    dp.isKey = true;

                }

                dp.dataType = ResolveDataType(jp.Value.Type);
                dp.dataLength = GetDefaultSize(jp.Value.Type).ToString();
                dataPrpCollectionTemp.Add(dp);
            }


            if (isKeyAssigned == false)
            {
                foreach (var dp in dataPrpCollectionTemp)
                {
                    if (dp.columnName.ToUpper().EndsWith("_ID"))
                    {
                        isKeyAssigned = true;
                        dp.isKey = true;

                    }

                }
            }

            if (isKeyAssigned == false)
            {
                isKeyAssigned = true;
                dataPrpCollectionTemp[0].isKey = true;
            }


            foreach (var dp in dataPrpCollectionTemp)
            {
                dataPrpCollection.Add(dp);
            }





        }

        private DataDictionary LoadDataObjects()
        {
            try
            {
                string Object_Name = string.Empty;
                DataObject _dataObject = new DataObject();
                KeyProperty _keyproperties = new KeyProperty();
                DataProperty _dataproperties = new DataProperty();
                DataDictionary _dataDictionary = new DataDictionary();
                List<DataProp> dataPrpCollection = new List<DataProp>();


                foreach (var dic in _objectyDictionary)
                {
                    string objectName = dic.Key.Split('_')[1];
                    string url = dic.Value;
                    string jsonString = GetJsonResponseFrom(url);
                    FillDataPropertiesFrom(jsonString, dataPrpCollection, objectName);
                }

                foreach (DataProp dp in dataPrpCollection)
                {
                    if (Object_Name != dp.Object_Name)
                    {
                        if (!string.IsNullOrEmpty(Object_Name))
                            _dataDictionary.dataObjects.Add(_dataObject);
                        _dataObject = new DataObject();
                        Object_Name = dp.Object_Name;
                        _dataObject.objectName = Object_Name;
                        _dataObject.tableName = Object_Name;
                        _dataObject.keyDelimeter = _keyDelimiter;
                    }

                    _dataproperties = new DataProperty();
                    _dataproperties.columnName = dp.columnName;

                    if (dp.isKey)
                    {
                        KeyProperty keyProperty = new KeyProperty();
                        keyProperty.keyPropertyName = dp.columnName;
                        _dataObject.keyProperties.Add(keyProperty);

                        _dataproperties.keyType = KeyType.assigned;
                        _dataproperties.isNullable = false;
                    }
                    else
                    {
                        _dataproperties.keyType = KeyType.unassigned;
                        _dataproperties.isNullable = true;
                    }


                    _dataproperties.propertyName = dp.propertyName;
                    _dataproperties.dataLength = Convert.ToInt32(dp.dataLength);

                    _dataproperties.dataType = dp.dataType;



                    _dataObject.dataProperties.Add(_dataproperties);
                }
                _dataDictionary.dataObjects.Add(_dataObject);


                return _dataDictionary;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in loading data dictionary : " + ex);
                throw ex;
            }
            finally
            {
                //Disconnect();
            }
        }

        /// <summary>
        /// It returns url for restfull service of specified object
        /// </summary>
        /// <param name="objectName">object name/table name</param>
        /// <returns></returns>
        private string GetObjectUrl(string objectName)
        {
            var url = (from dicEntry in _objectyDictionary
                       where dicEntry.Key.ToUpper() == (Constants.OBJECT_PREFIX + objectName).ToUpper()
                       select dicEntry.Value).SingleOrDefault<string>();

            return url;
        }

        /// <summary>
        /// It will make a request on URL and retuen json string.
        /// </summary>
        private string GetJsonResponseFrom(string url)
        {
            try
            {
                //org.iringtools.utility.WebHttpClient client = new org.iringtools.utility.WebHttpClient(_baseUrl);
                //client.AppKey = _appKey;
                //client.AccessToken = _authToken;
                //string aaa = client.GetMessage(url);
                return _webClient.MakeGetRequest(url);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error in connectiong rest server");
            }
        }

        private DataTable GetDataTableFrom(string jsonString, string objectType, string collectionName = "Items")
        {
            if (jsonString.IndexOf("{\"status_text\":\"Record Not Found.\",\"status_code\":\"202\"}") >= 0)
            {
                return GetDataTableSchema(objectType);
            }
            else
            {
                JObject o = JObject.Parse(jsonString);
                JArray items = (JArray)o[collectionName];
                DataTable dt = items.ToObject<DataTable>();
                dt.TableName = objectType;
                return dt;
            }
        }

        private DataTable GetDataTableSchema(string objectType)
        {
            DataObject objDef = _dataDictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());
            DataTable dataTable = new DataTable();
            dataTable.TableName = objectType;
            foreach (DataProperty property in objDef.dataProperties)
            {
                DataColumn dataColumn = new DataColumn();
                dataColumn.ColumnName = property.columnName;
                dataColumn.DataType = Type.GetType("System." + property.dataType.ToString());
                dataTable.Columns.Add(dataColumn);
            }


            return dataTable;
        }

        private string GenerateUrl(string objectType, DataFilter filter, int limit, int start)
        {
            string url = GetObjectUrl(objectType);
            if ((limit == 0)  || (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0))
            {
                url = url + @"?start=" + Convert.ToString(0) + @"&limit=" + Convert.ToString(10000000);
            }
            else
            {
                url = url + @"?start=" + Convert.ToString(start) + @"&limit=" + Convert.ToString(limit);
            }
            
            return url;
        }

        private string GenerateUrl(string objectType, IList<string> identifiers)
        {
            string url = GetObjectUrl(objectType);

            if (identifiers != null)
            {
                foreach (string id in identifiers)
                {
                    url = url + @"/" + id;
                    break;
                }
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

        private string GenerateReletedUrl(string parentObject, string pId, string relatedObject, int limit, int start)
        {
            string url = GenerateReletedUrl(parentObject, pId, relatedObject);

            if (limit == 0)
            {
                url = url + @"?start=" + Convert.ToString(0) + @"&limit=" + Convert.ToString(10000000);
            }
            else
            {
                url = url + @"?start=" + Convert.ToString(start) + @"&limit=" + Convert.ToString(limit);
            }

            return url;
        }
        

        private long GetObjectCount(string objectType, DataFilter filter)
        {
            string url = GetObjectUrl(objectType);
            url = url + @"?start=0&limit=1";
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

        private DataType ResolveDataType(JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Integer:
                    return DataType.Int32;
                case JTokenType.Date:
                    return DataType.DateTime;
                case JTokenType.String:
                    return DataType.String;
                case JTokenType.Float:
                    return DataType.Double;
                case JTokenType.Boolean:
                    return DataType.Boolean;
                case JTokenType.Bytes:
                    return DataType.Byte;
                default:
                    return DataType.String;
            }

        }

        private int GetDefaultSize(JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Integer:
                    return 16;
                case JTokenType.Date:
                    return 50;
                case JTokenType.String:
                    return 128;
                case JTokenType.Float:
                    return 32;
                case JTokenType.Boolean:
                    return 1;
                default:
                    return 128;
            }

        }

        private System.Type ResolveDataType(DataType dataType)
        {


            switch (dataType)
            {
                case DataType.Boolean:
                    return typeof(bool);
                case DataType.Byte:
                    return typeof(bool);
                case DataType.Char:
                    return typeof(bool);
                case DataType.DateTime:
                    return typeof(bool);
                case DataType.Decimal:
                    return typeof(bool);
                case DataType.Double:
                    return typeof(bool);
                case DataType.Int16:
                    return typeof(bool);
                case DataType.Int32:
                    return typeof(bool);
                case DataType.Int64:
                    return typeof(bool);
                case DataType.Reference:
                    return typeof(bool);
                case DataType.String:
                    return typeof(bool);
                case DataType.Single:
                    return typeof(bool);


            }
            return typeof(string);
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

    internal class DataProp
    {
        public string Object_Name;
        public string columnName;
        public string propertyName;
        public DataType dataType;
        public string dataLength;
        public string isNullable;
        public string keyType;
        public bool isKey;
    }

    public static class DataFilterExtension
    {
        public static string ToFilterExpression(this DataFilter dataFilter,DataDictionary dataDictionary, string objectName)
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

            return filterUrl.ToString();
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
                    filterExpression.Append(qualColumnName + "=" + expression.Values.FirstOrDefault());
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

        #endregion
    }
}
