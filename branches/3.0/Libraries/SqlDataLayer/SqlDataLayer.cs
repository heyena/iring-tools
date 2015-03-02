namespace org.iringtools.adapter.datalayer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using log4net;
    using Oracle.DataAccess;
    using Oracle.DataAccess.Client;
    using org.iringtools.adapter;
    using org.iringtools.library;
    using org.iringtools.utility;
    using StaticDust.Configuration;
    using System.Collections;

    /// <summary>
    /// Class responsible for containing all methods and properties required to perform operations on sql data layer
    /// </summary>
    public class SqlDataLayer : BaseLightweightDataLayer2
    {
        #region Enumerations

        /// <summary>
        /// Enumeration for user choice to handle duplicate rows(data)
        /// </summary>
        enum DuplicateHandlingActions
        {
            DuplicateAllowed,
            DuplicateNotAllowed,
            TerminateExecution
        }

        #endregion

        #region Private Data Members

        /// <summary>
        /// Variable to hold database dictionary path
        /// </summary>
        string databaseDictionaryPath = string.Empty;

        /// <summary>
        /// Variable to hold data dictionary path
        /// </summary>
        string dataDictionaryPath = string.Empty;

        /// <summary>
        /// Variable to hold name of the type of database
        /// </summary>
        string dbTypeName = string.Empty;

        /// <summary>
        /// Variable to hold name of the schema to be used in database
        /// </summary>
        string schemaName = string.Empty;

        /// <summary>
        /// User selected duplicate handling action
        /// </summary>
        DuplicateHandlingActions selectedDuplicateHandlingActions;

        /// <summary>
        /// Database dictionary object
        /// </summary>
        DatabaseDictionary databaseDictionary;

        /// <summary>
        /// Data dictionary object
        /// </summary>
        DataDictionary dataDictionary;

        /// <summary>
        /// Database connection object
        /// </summary>
        IDbConnection _sqlDatalayerDBConnection;

        /// <summary>
        /// Database command object
        /// </summary>
        IDbCommand _sqlDatalayerDbCommand;

        /// <summary>
        /// Data adapter object
        /// </summary>
        IDataAdapter _sqlDatalayerDataAdapter;

        #endregion

        #region Private Static Data Members

        private static readonly byte[] BMP = { 66, 77 };
        private static readonly byte[] MSO = { 208, 207, 17, 224, 161, 177, 26, 225 }; //MSO means all Microsoft Office products
        private static readonly byte[] MSOX = { 80, 75, 3, 4 };//, 20, 0, 6, 0, 8, 0, 0, 0, 33, 0 };
        //private static readonly byte[] XLS = { 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 69, 120, 99, 101, 108, 0 }; NOT REQUIRED
        private static readonly byte[] DOC = { 87, 111, 114, 100, 46, 68, 111, 99, 117, 109, 101, 110, 116, 46 };
        private static readonly byte[] DOCX = { 145, 26, 183, 243, 0, 0, 0, 78, 2, 0, 0, 11, 0 };
        private static readonly byte[] GIF = { 71, 73, 70, 56 };
        private static readonly byte[] JPG = { 255, 216, 255 };
        private static readonly byte[] PDF = { 37, 80, 68, 70, 45, 49, 46 };
        private static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };
        private static readonly byte[] TIFF = { 73, 73, 42, 0 };

        private static string DefaultMimeType = "application/pdf";

        #endregion

        #region Static Data Members

        /// <summary>
        /// Log file handler object
        /// </summary>
        static readonly ILog _logger = LogManager.GetLogger(typeof(SqlDataLayer));

        /// <summary>
        /// Dictionary collection to hold parameter symbols based on database types
        /// </summary>
        static Dictionary<string, string> dbParameterDictionary = new Dictionary<string, string>();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor taking adapter settings as parameter
        /// </summary>
        /// <param name="adaptersettings">Instance of AdapterSettings</param>
        public SqlDataLayer(AdapterSettings adaptersettings)
            : base(adaptersettings)
        {
            try
            {
                string relativePath = string.Empty;

                PrepareDBParametersDictionary();

                relativePath = String.Format("{0}{1}.{2}.config", _settings["AppDataPath"], _settings["projectName"], _settings["applicationName"]);

                //Adding config files entry to existing adapter settings
                if (File.Exists(relativePath))
                {
                    AppSettingsReader appSettings = new AppSettingsReader(relativePath);
                    _settings.AppendSettings(appSettings);
                }

                databaseDictionaryPath = _settings["AppDataPath"] + @"DatabaseDictionary." + _settings["projectName"] + "." + _settings["applicationName"] + ".xml";
                dataDictionaryPath = _settings["AppDataPath"] + @"DataDictionary." + _settings["projectName"] + "." + _settings["applicationName"] + ".xml";

                selectedDuplicateHandlingActions = (DuplicateHandlingActions)Enum.Parse(typeof(DuplicateHandlingActions), _settings["DuplicateHandlingAction"].ToString());
            }
            catch (Exception exceptionDuringInitialization)
            {
                _logger.Error("Error in initialization: " + exceptionDuringInitialization.Message);
            }
        }

        #endregion

        #region Dictionary

        /// <summary>
        /// Reads the existing data dictionary
        /// </summary>
        /// <param name="refresh"></param>
        /// <param name="objectType">string type containing name of the DataObject</param>
        /// <param name="dataFilter">Instance of DataFilter</param>
        /// <returns>Instance of DataDictionary</returns>
        public override DataDictionary Dictionary(bool refresh, string objectType, out DataFilter dataFilter)
        {
            //TODO: this should be ref, as with current implementation, it serves no purpose
            dataFilter = new DataFilter();

            if (File.Exists(dataDictionaryPath))
            {
                dataDictionary = Utility.Read<DataDictionary>(dataDictionaryPath);

                return dataDictionary;
            }
            else if (File.Exists(databaseDictionaryPath))
            {
                DatabaseDictionary databaseDictionary = Utility.Read<DatabaseDictionary>(databaseDictionaryPath);

                dataDictionary = new DataDictionary();
                dataDictionary.dataObjects = databaseDictionary.dataObjects;

                Utility.Write<DataDictionary>(dataDictionary, dataDictionaryPath);

                return dataDictionary;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Get

        /// <summary>
        /// Method responsible for GET operation
        /// </summary>
        /// <param name="objectType">Instance of DataObject containing object to get</param>
        /// <returns>Instance of List lt; SerializableDataObject gt; containing output of GET operation</returns>
        public override List<SerializableDataObject> Get(DataObject objectType)
        {
            //List of objects containing data for each row of a data object
            List<SerializableDataObject> listOfSerializableDataObjects = new List<SerializableDataObject>();

            try
            {
                DataFilter useLessDataFilter;

                if (dataDictionary == null)
                {
                    Dictionary(false, string.Empty, out useLessDataFilter); //Fetching data dictionary
                }

                if (dataDictionary != null)
                {
                    DataObject dataObjectToBeUsedForGet = dataDictionary.dataObjects.Where<DataObject>(P => P.objectName.ToUpper() == objectType.objectName.ToUpper()).FirstOrDefault();

                    string composeExtensionPropertiesQuery = ExtenstionPropertiesAsQuery(dataObjectToBeUsedForGet);

                    StringBuilder columnNames = new StringBuilder();

                    //Getting column names as string
                    foreach (KeyProperty eachKeyProperty in dataObjectToBeUsedForGet.keyProperties)
                    {
                        columnNames.Append(objectType.dataProperties.Find(dProp => dProp.propertyName == eachKeyProperty.keyPropertyName).columnName + ",");
                    }

                    columnNames = columnNames.Remove(columnNames.ToString().LastIndexOf(","), 1);

                    StringBuilder getQuery = new StringBuilder("SELECT " + dataObjectToBeUsedForGet.tableName + ".* " + composeExtensionPropertiesQuery + " FROM schemaName." + dataObjectToBeUsedForGet.tableName);

                    //Executing GET operation to fetch data
                    DataTable getDataTable = DuplicatesHandlingOnUserChoice(getQuery, columnNames);

                    if (getDataTable != null)
                    {
                        CheckContentInGet(objectType, getDataTable);

                        foreach (DataRow eachDataRowFromGet in getDataTable.Rows)
                        {
                            SerializableDataObject serializableDataObject = new SerializableDataObject();

                            if (objectType.hasContent)
                            {
                                serializableDataObject.HasContent = true;
                            }

                            foreach (DataProperty eachDataProperty in dataObjectToBeUsedForGet.dataProperties)
                            {
                                serializableDataObject.SetPropertyValue(eachDataProperty.propertyName, eachDataRowFromGet[eachDataProperty.columnName]);
                            }

                            if (dataObjectToBeUsedForGet.extensionProperties != null && dataObjectToBeUsedForGet.extensionProperties.Count > 0)
                            {
                                foreach (ExtensionProperty eachExtensionProperty in dataObjectToBeUsedForGet.extensionProperties)
                                {
                                    serializableDataObject.SetPropertyValue(eachExtensionProperty.propertyName, eachDataRowFromGet[eachExtensionProperty.columnName]);
                                }
                            }

                            listOfSerializableDataObjects.Add(serializableDataObject);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException("Database dictionary not present.");
                }

                Utility.Write<DataDictionary>(dataDictionary, dataDictionaryPath);

                return listOfSerializableDataObjects;
            }
            catch (Exception exceptionInGet)
            {
                throw exceptionInGet;
            }
        }

        /// <summary>
        /// Method responsible for getting the content as data
        /// </summary>
        /// <param name="objectType">Instance of DataObject containing object for which the content is required</param>
        /// <param name="idFormats">Instance of Dictionary lt; string, string gt;</param>
        /// <returns>List lt; IContentObject gt;</returns>
        public override List<IContentObject> GetContents(DataObject objectType, Dictionary<string, string> idFormats)
        {
            try
            {
                if (objectType.hasContent)
                {
                    List<IContentObject> tempSQLContentObjectList = new List<IContentObject>();

                    DataFilter useLessDataFilter;

                    if (dataDictionary == null)
                    {
                        Dictionary(false, string.Empty, out useLessDataFilter); //Fetching data dictionary
                    }

                    if (dataDictionary != null)
                    {
                        foreach (KeyValuePair<string, string> eachId in idFormats)
                        {
                            DataObject dataObjectToBeUsedForGet = dataDictionary.dataObjects.Where<DataObject>(P => P.objectName.ToUpper() == objectType.objectName.ToUpper()).FirstOrDefault();

                            StringBuilder columnNamesInWhereClause = new StringBuilder();

                            string[] columnValues = eachId.Key.Split(objectType.keyDelimeter.ToCharArray());

                            for (int i = 0; i < columnValues.Length; i++)
                            {
                                columnNamesInWhereClause.Append(objectType.dataProperties.Find(prop => prop.propertyName == objectType.keyProperties[i].keyPropertyName).columnName + " = " + columnValues[i] + " AND ");
                            }

                            columnNamesInWhereClause.Remove(columnNamesInWhereClause.Length - 4, 4);

                            //Executing GET operation to fetch data
                            DataTable getDataTable = ExecuteGetDatabaseQuery("SELECT * FROM schemaName." + dataObjectToBeUsedForGet.tableName + " WHERE " + columnNamesInWhereClause);

                            int columnNumber = 0;

                            //Setting values for content objects
                            foreach (DataColumn eachDataColumn in getDataTable.Columns)
                            {
                                if (getDataTable.Rows[0][columnNumber] != DBNull.Value && eachDataColumn.DataType == typeof(byte[]))
                                {
                                    IContentObject contentObject = new SQLContentObject(dataObjectToBeUsedForGet, getDataTable, (byte[])getDataTable.Rows[0][columnNumber]);

                                    contentObject.ContentType = GetMimeFromBytes((byte[])getDataTable.Rows[0][columnNumber]);
                                    contentObject.Content.Position = 0;
                                    contentObject.Identifier = eachId.Key;
                                    contentObject.ObjectType = objectType.objectName;

                                    tempSQLContentObjectList.Add(contentObject);
                                }

                                columnNumber++;
                            }
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException("Database dictionary not present.");
                    }

                    return tempSQLContentObjectList;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception getContentException)
            {
                throw getContentException;
            }
        }

        /// <summary>
        /// Method responsible for getting the index for any data object
        /// </summary>
        /// <param name="objectType">Instance of DataObject containing object whose indexes will be received</param>
        /// <returns>Instance of List lt; SerializableDataObject gt; containing indexes</returns>
        public override List<SerializableDataObject> GetIndex(DataObject objectType)
        {
            //List of objects containing data for each row of a data object
            List<SerializableDataObject> listOfSerializableDataObjects = new List<SerializableDataObject>();

            try
            {
                DataFilter useLessDataFilter;

                if (dataDictionary == null)
                {
                    Dictionary(false, string.Empty, out useLessDataFilter); //Fetching data dictionary
                }

                if (dataDictionary != null)
                {
                    DataObject dataObjectToBeUsedForGet = dataDictionary.dataObjects.Where<DataObject>(P => P.objectName.ToUpper() == objectType.objectName.ToUpper()).FirstOrDefault();

                    //Preparing query for GET operation
                    StringBuilder getIdQuery = new StringBuilder();

                    getIdQuery.Append("SELECT ");

                    StringBuilder columnNames = new StringBuilder();

                    foreach (KeyProperty eachKeyProperty in dataObjectToBeUsedForGet.keyProperties)
                    {
                        columnNames.Append(dataObjectToBeUsedForGet.dataProperties.Find(prop => prop.propertyName == eachKeyProperty.keyPropertyName).columnName + ",");
                    }

                    columnNames = columnNames.Remove(columnNames.ToString().LastIndexOf(","), 1);
                    getIdQuery.Append(columnNames);
                    getIdQuery.Append(" FROM schemaName." + objectType.tableName);

                    DataTable dataTableContainingIdentifiers = DuplicatesHandlingOnUserChoice(getIdQuery, columnNames);

                    foreach (DataRow eachDataRow in dataTableContainingIdentifiers.Rows)
                    {
                        SerializableDataObject serializableDataObject = new SerializableDataObject();

                        foreach (KeyProperty eachKeyProperty in dataObjectToBeUsedForGet.keyProperties)
                        {
                            serializableDataObject.SetPropertyValue(eachKeyProperty.keyPropertyName, eachDataRow[dataObjectToBeUsedForGet.dataProperties.Find(prop => prop.propertyName == eachKeyProperty.keyPropertyName).columnName]);
                        }

                        listOfSerializableDataObjects.Add(serializableDataObject);
                    }
                }
                else
                {
                    throw new FileNotFoundException("Database dictionary not present.");
                }

                return listOfSerializableDataObjects;
            }
            catch (Exception exceptionWhileGettingIndexes)
            {
                throw exceptionWhileGettingIndexes;
            }
        }

        /// <summary>
        /// Method responsible for GET operation with paging
        /// </summary>
        /// <param name="objectType">Instance of DataObject containing object to get</param>
        /// <param name="identifiers">Instance of List lt; SerializableDataObject gt; containing indexes</param>
        /// <returns>Instance of List lt; SerializableDataObject gt; containing ouput of GET operation</returns>
        public override List<SerializableDataObject> GetPage(DataObject objectType, List<SerializableDataObject> identifiers)
        {
            //List of objects containing data for each row of a data object
            List<SerializableDataObject> listOfSerializableDataObjects = new List<SerializableDataObject>();

            try
            {
                DataFilter useLessDataFilter;

                if (dataDictionary == null)
                {
                    Dictionary(false, string.Empty, out useLessDataFilter); //Reading database dictionary
                }

                if (dataDictionary != null)
                {
                    StringBuilder getPageQuery = new StringBuilder();
                    DataObject dataObjectToBeUsedForGet = dataDictionary.dataObjects.Where<DataObject>(P => P.objectName.ToUpper() == objectType.objectName.ToUpper()).FirstOrDefault();

                    string composeExtensionPropertiesQuery = ExtenstionPropertiesAsQuery(dataObjectToBeUsedForGet);

                    getPageQuery.Append("SELECT " + dataObjectToBeUsedForGet.tableName + ".* " + composeExtensionPropertiesQuery + " FROM schemaName." + dataObjectToBeUsedForGet.tableName + " WHERE ( (");

                    StringBuilder columnNames = new StringBuilder();

                    //Getting column names as string
                    foreach (KeyProperty eachKeyProperty in dataObjectToBeUsedForGet.keyProperties)
                    {
                        columnNames.Append(objectType.dataProperties.Find(dProp => dProp.propertyName == eachKeyProperty.keyPropertyName).columnName + ",");
                    }

                    columnNames = columnNames.Remove(columnNames.ToString().LastIndexOf(","), 1);

                    //Preparing query for GET operation
                    foreach (SerializableDataObject eachIdentifier in identifiers)
                    {
                        foreach (KeyValuePair<string, object> eachDictionaryEntry in eachIdentifier.Dictionary)
                        {
                            getPageQuery.Append(objectType.dataProperties.Find(dProp => dProp.propertyName == eachDictionaryEntry.Key).columnName + "= '" + eachDictionaryEntry.Value + "' AND ");
                        }

                        getPageQuery = getPageQuery.Remove(getPageQuery.ToString().LastIndexOf("AND"), 4);
                        getPageQuery.Append(") ");
                        getPageQuery.Append(" OR (");
                    }

                    getPageQuery = getPageQuery.Remove(getPageQuery.ToString().LastIndexOf("OR"), 4);
                    getPageQuery.Append(") ");

                    //Executing GET operation to fetch a page of data based on list of identifiers
                    DataTable getDataTable = DuplicatesHandlingOnUserChoice(getPageQuery, columnNames);

                    if (getDataTable != null)
                    {
                        CheckContentInGet(objectType, getDataTable);

                        foreach (DataRow eachDataRowFromGet in getDataTable.Rows)
                        {
                            SerializableDataObject serializableDataObject = new SerializableDataObject();

                            if (objectType.hasContent)
                            {
                                serializableDataObject.HasContent = true;
                            }

                            foreach (DataProperty eachDataProperty in dataObjectToBeUsedForGet.dataProperties)
                            {
                                serializableDataObject.SetPropertyValue(eachDataProperty.propertyName, eachDataRowFromGet[eachDataProperty.columnName]);
                            }

                            if (dataObjectToBeUsedForGet.extensionProperties != null && dataObjectToBeUsedForGet.extensionProperties.Count > 0)
                            {
                                foreach (ExtensionProperty eachExtensionProperty in dataObjectToBeUsedForGet.extensionProperties)
                                {
                                    serializableDataObject.SetPropertyValue(eachExtensionProperty.propertyName, eachDataRowFromGet[eachExtensionProperty.columnName]);
                                }
                            }

                            listOfSerializableDataObjects.Add(serializableDataObject);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException("Database dictionary not present.");
                }

                Utility.Write<DataDictionary>(dataDictionary, dataDictionaryPath);

                return listOfSerializableDataObjects;
            }
            catch (Exception exceptionWhileGettingPage)
            {
                throw exceptionWhileGettingPage;
            }
        }

        #endregion

        #region Post

        /// <summary>
        /// Performs the post operation in data layer.
        /// </summary>
        /// <param name="objectType">Instance of DataObject containing object to be posted</param>
        /// <param name="dataObjects">Instance of List lt; SerializableDataObject gt; containing values to be posted</param>
        /// <returns>Instance of Response which contains the complete status of POST</returns>
        public override Response Update(DataObject objectType, List<SerializableDataObject> dataObjects)
        {
            Response responseOfPost = new Response();

            try
            {
                string postTableName = string.Empty;
                bool isExecutionSuccessful = true;

                DataFilter useLessDataFilter;

                if (dataDictionary == null)
                {
                    Dictionary(false, string.Empty, out useLessDataFilter); //Reading database dictionary
                }

                if (dataDictionary != null)
                {
                    //Setting default values
                    responseOfPost.Level = StatusLevel.Success;
                    responseOfPost.StatusCode = System.Net.HttpStatusCode.Accepted;

                    //Getting post data object from data Dictionary to avoid any descrepencies in data object which is received in the method.
                    objectType = dataDictionary.dataObjects.Find(dObj => dObj.objectName == objectType.objectName);

                    //Setting the object name for post operation
                    if (objectType.aliasDictionary != null && objectType.aliasDictionary["TABLE_NAME_IN"] != null)
                    {
                        postTableName = objectType.aliasDictionary["TABLE_NAME_IN"];
                    }
                    else
                    {
                        postTableName = objectType.tableName;
                    }

                    // The database connection should be opened once in the beginning of all transactions to save database execution time
                    OpenDBConnection();

                    if (dataObjects != null)
                    {
                        foreach (SerializableDataObject serializableDataObject in dataObjects)
                        {
                            if (serializableDataObject.State == ObjectState.Create)
                            {
                                //POST operation - INSERT
                                InsertSerializableDataObjectInDataSource(objectType, serializableDataObject, postTableName, ref responseOfPost, ref isExecutionSuccessful);
                            }
                            else if (serializableDataObject.State == ObjectState.Update)
                            {
                                //POST operation - UPDATE
                                UpdateSerializableDataObjectInDataSource(objectType, serializableDataObject, postTableName, ref responseOfPost, ref isExecutionSuccessful);
                            }
                            else if (serializableDataObject.State == ObjectState.Delete)
                            {
                                //POST operation - DELETE
                                DeleteSerializableDataObjectInDataSource(objectType, serializableDataObject, postTableName, ref responseOfPost, ref isExecutionSuccessful);
                            }
                            else
                            {
                                //Corrupted data
                                string errorMessage = "Object state is readonly for object with id " + serializableDataObject.Id;
                                StatusLevel responseStatusLevel = StatusLevel.Error;
                                HttpStatusCode responseStatusCode = HttpStatusCode.BadRequest;

                                SetResponse(serializableDataObject.Id, responseStatusLevel, errorMessage, responseStatusCode, ref responseOfPost);

                                isExecutionSuccessful = false;

                                _logger.Error(errorMessage);
                            }
                        }
                    }

                    // After complete database transaction, as it is response date and time. The database transaction will have it's own execution time.
                    responseOfPost.DateTimeStamp = DateTime.Now;
                    responseOfPost.StatusText = isExecutionSuccessful ? "Successfully finished POST" : "Finished POST with error";

                    return responseOfPost;
                }
                else
                {
                    throw new FileNotFoundException("Database dictionary not present.");
                }
            }
            catch (Exception exceptionInPost)
            {
                //Creating response for POST in case of an exception
                string errorMessage = "Error in POST operation: " + exceptionInPost.Message;
                StatusLevel responseStatusLevel = StatusLevel.Error;
                HttpStatusCode responseStatusCode = HttpStatusCode.BadRequest;

                SetResponse(null, responseStatusLevel, errorMessage, responseStatusCode, ref responseOfPost);

                responseOfPost.DateTimeStamp = DateTime.Now;
                responseOfPost.StatusText = errorMessage;

                _logger.Error(errorMessage);

                return responseOfPost;
            }
            finally
            {
                // Closing the database connection after the complete database transaction is completed.
                CloseDBConnection();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Prepares dbParameterDictionary collection
        /// </summary>
        private void PrepareDBParametersDictionary()
        {
            dbParameterDictionary = new Dictionary<string, string>();

            dbParameterDictionary.Add("Oracle", ":");
            dbParameterDictionary.Add("Sql", "@");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetMimeFromBytes(byte[] content)
        {
            var mime = DefaultMimeType;

            try
            {
                if (content.Take(2).SequenceEqual(BMP))
                {
                    mime = "image/bmp";
                }
                else if (content.Take(8).SequenceEqual(MSO))
                {
                    mime = IsOfType(content, DOC) ? "application/msword" : "application/vnd.ms-excel";
                }
                else if (content.Take(4).SequenceEqual(MSOX))
                {
                    mime = IsOfType(content, DOCX) ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else if (content.Take(4).SequenceEqual(GIF))
                {
                    mime = "image/gif";
                }
                else if (content.Take(3).SequenceEqual(JPG))
                {
                    mime = "image/jpeg";
                }
                else if (content.Take(7).SequenceEqual(PDF))
                {
                    mime = "application/pdf";
                }
                else if (content.Take(16).SequenceEqual(PNG))
                {
                    mime = "image/png";
                }
                else if (content.Take(4).SequenceEqual(TIFF))
                {
                    mime = "image/tiff";
                }

                return mime ?? DefaultMimeType;
            }
            catch
            {
                return DefaultMimeType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private bool IsOfType(byte[] contents, byte[] pattern)
        {
            int i = 0;
            foreach (byte content in contents)
            {
                if (content.Equals(pattern[i]))
                {
                    i++;
                    if (pattern.Length.Equals(i))
                        return true;
                }
                else
                    i = 0;
            }
            return false;
        }

        /// <summary>
        /// Adds extension properties to the database query.
        /// </summary>
        /// <param name="dataObjectToBeUsedForGet">Instance of type DataObject</param>
        /// <returns>string type containg database query</returns>
        private string ExtenstionPropertiesAsQuery(DataObject dataObjectToBeUsedForGet)
        {
            if (dataObjectToBeUsedForGet.extensionProperties != null && dataObjectToBeUsedForGet.extensionProperties.Count > 0)
            {
                List<ExtensionProperty> extensionPropertyList = dataObjectToBeUsedForGet.extensionProperties;

                StringBuilder extensionPropertiesQuery = new StringBuilder();

                foreach (ExtensionProperty eachExtensionProperty in extensionPropertyList)
                {
                    StringBuilder defintion = new StringBuilder(eachExtensionProperty.definition);

                    if (defintion.Length > 0)
                    {
                        extensionPropertiesQuery.Append(", (" + defintion + ") AS " + eachExtensionProperty.columnName);
                    }
                }

                return extensionPropertiesQuery.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Handles the duplicate rows based on user choice
        /// </summary>
        /// <param name="getQuery">Holds get query</param>
        /// <param name="columnNames">Holds column names</param>
        /// <returns></returns>
        private DataTable DuplicatesHandlingOnUserChoice(StringBuilder getQuery, StringBuilder columnNames)
        {
            DataTable dataTableContainingIdentifiers;

            if (selectedDuplicateHandlingActions == DuplicateHandlingActions.DuplicateAllowed)
            {
                //Executing GET operation to fetch identifiers
                dataTableContainingIdentifiers = ExecuteGetDatabaseQuery(getQuery.ToString());
            }
            else
            {
                StringBuilder checkDuplicateQuery = new StringBuilder();
                checkDuplicateQuery.Append(getQuery + " GROUP BY " + columnNames + " HAVING COUNT(*) > 1;");

                DataTable checkDuplicateDataTable = ExecuteGetDatabaseQuery(checkDuplicateQuery.ToString());

                if (checkDuplicateDataTable.Rows.Count > 0)
                {
                    if (selectedDuplicateHandlingActions == DuplicateHandlingActions.DuplicateNotAllowed)
                    {
                        StringBuilder getIdWithoutDuplicateQuery = checkDuplicateQuery.Replace('>', '=');
                        dataTableContainingIdentifiers = ExecuteGetDatabaseQuery(getIdWithoutDuplicateQuery.ToString());
                    }
                    else
                    {
                        throw new Exception("Dupliate entries were found for key properties. Execution terminated.");
                    }
                }
                else
                {
                    dataTableContainingIdentifiers = ExecuteGetDatabaseQuery(getQuery.ToString());
                }
            }

            return dataTableContainingIdentifiers;
        }

        /// <summary>
        /// Checks for the database column return type to check whether the column holds any content or not
        /// </summary>
        /// <param name="objectType">Instance of DataObject</param>
        /// <param name="getDataTable">Data table returned after database query exeuction</param>
        private void CheckContentInGet(DataObject objectType, DataTable getDataTable)
        {
            foreach (DataColumn eachDataColumn in getDataTable.Columns)
            {
                if (eachDataColumn.DataType == typeof(byte[]))
                {
                    objectType.hasContent = true;
                }
            }

        }

        /// <summary>
        /// Sets the response object after POST of SerializableDataObject
        /// </summary>
        /// <param name="id">string type containing id of the SerializableDataObject</param>
        /// <param name="statusLevel">Instance of StatusLevel containing status of the POST operation</param>
        /// <param name="message">string type containing message generated for POST operation</param>
        /// <param name="statusCode">Instance of HttpStatusCode containing http status</param>
        /// <param name="response">Instance of Response which has to be set</param>
        private void SetResponse(string id, StatusLevel statusLevel, string message, HttpStatusCode statusCode, ref Response response)
        {
            response.Level = statusLevel;
            response.Messages.Add(message);
            response.StatusCode = statusCode;

            Status tempStatus = new Status();

            tempStatus.Identifier = id;
            tempStatus.Level = statusLevel;
            tempStatus.Messages.Add(message);
            tempStatus.Results.Add(statusLevel.ToString(), message);

            response.StatusList.Add(tempStatus);
        }

        /// <summary>
        /// Method responsible to delete serializable data object in data source.
        /// </summary>
        /// <param name="postDataObject">Instance of DataObject containing value for the object to be posted</param>
        /// <param name="serializableDataObject">Instance of SerializableDataObject containing value of the columns to be posted</param>
        /// <param name="response">Instance of Response containing output of the post operation</param>
        /// <param name="isExecutionSuccessful">bool type containing value based on post operation success</param>
        private void DeleteSerializableDataObjectInDataSource(DataObject postDataObject, SerializableDataObject serializableDataObject, string postTableName, ref Response response, ref bool isExecutionSuccessful)
        {
            try
            {
                StringBuilder columnsInWhereClause = new StringBuilder();

                //Preparing parameters names string used in WHERE clause
                foreach (KeyProperty postKeyProperty in postDataObject.keyProperties)
                {
                    DataProperty postDataProperty = postDataObject.dataProperties.Find(dProp => dProp.propertyName == postKeyProperty.keyPropertyName);

                    if (postDataProperty != null)
                    {
                        if (postDataProperty.aliasDictionary != null && postDataProperty.aliasDictionary["COLUMN_NAME_IN"] != null)
                        {
                            columnsInWhereClause.Append(postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + " = " + dbParameterDictionary[dbTypeName] + postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + " AND ");
                        }
                        else
                        {
                            columnsInWhereClause.Append(postDataProperty.columnName + " = " + dbParameterDictionary[dbTypeName] + postDataProperty.columnName + " AND ");
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                //Removing extra string added at the end
                if (columnsInWhereClause.Length > 5)
                {
                    columnsInWhereClause.Remove(columnsInWhereClause.Length - 5, 5);
                }

                //Preparing DELETE query
                string postSqlQuery = "DELETE FROM schemaName." + postTableName + " WHERE " + columnsInWhereClause;

                //Executing POST operation
                int noOfRowsPosted = ExecutePostDatabaseQuery(postDataObject, serializableDataObject, postSqlQuery, true);

                //Setting response object for successful posting
                string successMessage = "Deleted object with id " + serializableDataObject.Id;
                StatusLevel responseStatusLevel = StatusLevel.Success;
                HttpStatusCode responseStatusCode = HttpStatusCode.Accepted;

                SetResponse(serializableDataObject.Id, responseStatusLevel, successMessage, responseStatusCode, ref response);
            }
            catch (Exception exceptionInDelete)
            {
                //Failed operation
                string errorMessage = "Unable to delete object with id " + serializableDataObject.Id + ". " + exceptionInDelete.Message;
                StatusLevel responseStatusLevel = StatusLevel.Error;
                HttpStatusCode responseStatusCode = HttpStatusCode.InternalServerError;

                SetResponse(serializableDataObject.Id, responseStatusLevel, errorMessage, responseStatusCode, ref response);

                isExecutionSuccessful = false;
            }
        }

        /// <summary>
        /// Method responsible to update serializable data object in data source.
        /// </summary>
        /// <param name="postDataObject">Instance of DataObject containing value for the object to be posted</param>
        /// <param name="serializableDataObject">Instance of SerializableDataObject containing value of the columns to be posted</param>
        /// <param name="response">Instance of Response containing output of the post operation</param>
        /// <param name="isExecutionSuccessful">bool type containing value based on post operation success</param>
        private void UpdateSerializableDataObjectInDataSource(DataObject postDataObject, SerializableDataObject serializableDataObject, string postTableName, ref Response response, ref bool isExecutionSuccessful)
        {
            try
            {
                StringBuilder columnNamesWithValues = new StringBuilder();
                StringBuilder columnsInWhereClause = new StringBuilder();

                //Preparing columns and parameters names string
                foreach (KeyValuePair<string, object> columnDetails in serializableDataObject.Dictionary)
                {
                    DataProperty postDataProperty = postDataObject.dataProperties.Find(dProp => dProp.propertyName == columnDetails.Key);

                    if (postDataProperty != null)
                    {
                        if (postDataProperty.aliasDictionary != null && postDataProperty.aliasDictionary["COLUMN_NAME_IN"] != null)
                        {
                            columnNamesWithValues.Append(postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + " = " + dbParameterDictionary[dbTypeName] + postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + ", ");
                        }
                        else
                        {
                            columnNamesWithValues.Append(postDataProperty.columnName + " = " + dbParameterDictionary[dbTypeName] + postDataProperty.columnName + ", ");
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                //Preparing parameters names string used in WHERE clause
                foreach (KeyProperty columnDetails in postDataObject.keyProperties)
                {
                    DataProperty postDataProperty = postDataObject.dataProperties.Find(dProp => dProp.propertyName == columnDetails.keyPropertyName);

                    if (postDataProperty != null)
                    {
                        if (postDataProperty.aliasDictionary != null && postDataProperty.aliasDictionary["COLUMN_NAME_IN"] != null)
                        {
                            columnsInWhereClause.Append(postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + " = " + dbParameterDictionary[dbTypeName] + postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + " AND ");
                        }
                        else
                        {
                            columnsInWhereClause.Append(postDataProperty.columnName + " = " + dbParameterDictionary[dbTypeName] + postDataProperty.columnName + " AND ");
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                //Removing extra string added at the end
                if (columnNamesWithValues.Length > 2)
                {
                    columnNamesWithValues.Remove(columnNamesWithValues.Length - 2, 2);
                }

                //Removing extra string added at the end
                if (columnsInWhereClause.Length > 5)
                {
                    columnsInWhereClause.Remove(columnsInWhereClause.Length - 5, 5);
                }

                //Preparing UPDATE query
                string postSqlQuery = "UPDATE schemaName." + postTableName + " SET " + columnNamesWithValues + " WHERE " + columnsInWhereClause;

                //Executing POST operation
                int noOfRowsPosted = ExecutePostDatabaseQuery(postDataObject, serializableDataObject, postSqlQuery, false);

                //Setting response object for successful posting
                string successMessage = "Updated object with id " + serializableDataObject.Id;
                StatusLevel responseStatusLevel = StatusLevel.Success;
                HttpStatusCode responseStatusCode = HttpStatusCode.Accepted;

                SetResponse(serializableDataObject.Id, responseStatusLevel, successMessage, responseStatusCode, ref response);
            }
            catch (Exception exceptionInUpdate)
            {
                //Failed operation
                string errorMessage = "Unable to update object with id " + serializableDataObject.Id + ". " + exceptionInUpdate.Message;
                StatusLevel responseStatusLevel = StatusLevel.Error;
                HttpStatusCode responseStatusCode = HttpStatusCode.InternalServerError;

                SetResponse(serializableDataObject.Id, responseStatusLevel, errorMessage, responseStatusCode, ref response);

                isExecutionSuccessful = false;
            }
        }

        /// <summary>
        /// Method responsible to insert serializable data object in data source.
        /// </summary>
        /// <param name="postDataObject">Instance of DataObject containing value for the object to be posted</param>
        /// <param name="serializableDataObject">Instance of SerializableDataObject containing value of the columns to be posted</param>
        /// <param name="response">Instance of Response containing output of the post operation</param>
        /// <param name="isExecutionSuccessful">bool type containing value based on post operation success</param>
        private void InsertSerializableDataObjectInDataSource(DataObject postDataObject, SerializableDataObject serializableDataObject, string postTableName, ref Response response, ref bool isExecutionSuccessful)
        {
            try
            {
                StringBuilder columnNames = new StringBuilder();
                StringBuilder parameterNames = new StringBuilder();

                //Preparing columns and parameters names string
                foreach (KeyValuePair<string, object> columnDetails in serializableDataObject.Dictionary)
                {
                    DataProperty postDataProperty = postDataObject.dataProperties.Find(dProp => dProp.propertyName == columnDetails.Key);

                    if (postDataProperty != null)
                    {
                        if (postDataProperty.aliasDictionary != null && postDataProperty.aliasDictionary["COLUMN_NAME_IN"] != null)
                        {
                            columnNames.Append(postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + ", ");
                            parameterNames.Append(dbParameterDictionary[dbTypeName] + postDataProperty.aliasDictionary["COLUMN_NAME_IN"] + ", ");
                        }
                        else
                        {
                            columnNames.Append(postDataProperty.columnName + ", ");
                            parameterNames.Append(dbParameterDictionary[dbTypeName] + postDataProperty.columnName + ", ");
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                //Removing extra string added at the end
                if (columnNames.Length > 2)
                {
                    columnNames.Remove(columnNames.Length - 2, 2);
                }

                //Removing extra string added at the end
                if (parameterNames.Length > 2)
                {
                    parameterNames.Remove(parameterNames.Length - 2, 2);
                }

                //Preparing INSERT query
                string postSqlQuery = "INSERT INTO schemaName." + postTableName + " (" + columnNames + ") VALUES (" + parameterNames + ")";

                //Executing POST operation
                int noOfRowsPosted = ExecutePostDatabaseQuery(postDataObject, serializableDataObject, postSqlQuery, false);

                //Setting response object for successful posting
                string successMessage = "Inserted object with id " + serializableDataObject.Id;
                StatusLevel responseStatusLevel = StatusLevel.Success;
                HttpStatusCode responseStatusCode = HttpStatusCode.Accepted;

                SetResponse(serializableDataObject.Id, responseStatusLevel, successMessage, responseStatusCode, ref response);
            }
            catch (Exception exceptionInInsert)
            {
                //Failed operation
                string errorMessage = "Unable to insert object with id " + serializableDataObject.Id + ". " + exceptionInInsert.Message;
                StatusLevel responseStatusLevel = StatusLevel.Error;
                HttpStatusCode responseStatusCode = HttpStatusCode.InternalServerError;

                SetResponse(serializableDataObject.Id, responseStatusLevel, errorMessage, responseStatusCode, ref response);

                isExecutionSuccessful = false;
            }
        }

        /// <summary>
        /// Opens the database connection based on its state
        /// </summary>
        private void OpenDBConnection()
        {
            databaseDictionary = Utility.Read<DatabaseDictionary>(databaseDictionaryPath);

            if (databaseDictionary != null)
            {
                string providerName = databaseDictionary.Provider;
                string connectionString = utility.EncryptionUtility.Decrypt(databaseDictionary.ConnectionString);

                if (providerName.Contains("Oracle"))
                {
                    _sqlDatalayerDBConnection = new OracleConnection(connectionString);

                    dbTypeName = "Oracle";
                }
                else if (providerName.Replace(" ", "").Contains("Sql"))
                {
                    _sqlDatalayerDBConnection = new SqlConnection(connectionString);

                    dbTypeName = "Sql";
                }

                if (_sqlDatalayerDBConnection.State == ConnectionState.Broken || _sqlDatalayerDBConnection.State == ConnectionState.Closed)
                {
                    _sqlDatalayerDBConnection.Open();
                }

                schemaName = databaseDictionary.SchemaName;
            }
            else
            {
                throw new FileNotFoundException("Database dictionary not found");
            }
        }

        /// <summary>
        /// Executes get operation's database query
        /// </summary>
        /// <param name="getSqlQuery">Instance of string containing values for SQL query</param>
        /// <returns>Instance of DataTable containing tables holding as a result of query execution</returns>
        private DataTable ExecuteGetDatabaseQuery(string getSqlQuery)
        {
            try
            {
                //Opening database connection
                OpenDBConnection();

                getSqlQuery = getSqlQuery.Replace("schemaName", schemaName);

                //Creating command objects based on database type
                if (dbTypeName == "Oracle")
                {
                    _sqlDatalayerDbCommand = new OracleCommand(getSqlQuery, (OracleConnection)_sqlDatalayerDBConnection);
                    _sqlDatalayerDataAdapter = new OracleDataAdapter((OracleCommand)_sqlDatalayerDbCommand);
                }
                else if (dbTypeName == "Sql")
                {
                    _sqlDatalayerDbCommand = new SqlCommand(getSqlQuery, (SqlConnection)_sqlDatalayerDBConnection);
                    _sqlDatalayerDataAdapter = new SqlDataAdapter((SqlCommand)_sqlDatalayerDbCommand);
                }

                DataSet tempDataSet = new DataSet();

                //_sqlDatalayerDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                tempDataSet.EnforceConstraints = false;

                //Getting values from the database
                _sqlDatalayerDataAdapter.FillSchema(tempDataSet, SchemaType.Source);
                _sqlDatalayerDataAdapter.Fill(tempDataSet);

                return tempDataSet.Tables[0];
            }
            catch (SqlException)
            {
                return null;
            }
            catch (OracleException)
            {
                return null;
            }
            catch (Exception exceptionInGet)
            {
                throw exceptionInGet;
            }
            finally
            {
                //Closing database connection
                CloseDBConnection();
            }
        }

        /// <summary>
        /// Executes post operation's database query
        /// </summary>
        /// <param name="serializableDataObject">Instance of SerializableDataObject containing value of the columns to be posted</param>
        /// <param name="postSqlQuery">Instance of string containing values for SQL query</param>
        /// <param name="isDeleteCommand">bool type deciding if the command is delete command or not</param>
        /// <returns></returns>
        private int ExecutePostDatabaseQuery(DataObject objectType, SerializableDataObject serializableDataObject, string postSqlQuery, bool isDeleteCommand)
        {
            try
            {
                postSqlQuery = postSqlQuery.Replace("schemaName", schemaName);

                //Creating command objects based on database type
                if (dbTypeName == "Oracle")
                {
                    _sqlDatalayerDbCommand = new OracleCommand(postSqlQuery, (OracleConnection)_sqlDatalayerDBConnection);
                }
                else if (dbTypeName == "Sql")
                {
                    _sqlDatalayerDbCommand = new SqlCommand(postSqlQuery, (SqlConnection)_sqlDatalayerDBConnection);
                }

                if (isDeleteCommand)
                {
                    string[] columnValues;

                    if (!string.IsNullOrEmpty(objectType.keyDelimeter))
                    {
                        columnValues = serializableDataObject.Id.Split(objectType.keyDelimeter.ToCharArray());
                    }
                    else
                    {
                        throw new ArgumentNullException(objectType.objectName, "Delimiter value not present in target.");
                    }

                    for (int i = 0; i < columnValues.Length; i++)
                    {
                        DataProperty columnDataproperty = objectType.dataProperties.Find(col => col.propertyName == objectType.keyProperties[i].keyPropertyName);

                        if (columnDataproperty != null)
                        {
                            string columnName = string.Empty;

                            //Setting column name for POST operation
                            if (columnDataproperty.aliasDictionary != null && columnDataproperty.aliasDictionary["COLUMN_NAME_IN"] != null)
                            {
                                columnName = columnDataproperty.aliasDictionary["COLUMN_NAME_IN"];
                            }
                            else
                            {
                                columnName = columnDataproperty.columnName;
                            }

                            //Handling null values for DELETE command
                            if (string.IsNullOrEmpty(columnValues[i]))
                            {
                                _sqlDatalayerDbCommand.CommandText = _sqlDatalayerDbCommand.CommandText.Replace(columnName + " = " + dbParameterDictionary[dbTypeName] + columnName, columnName + " IS NULL");
                            }
                            else
                            {
                                //Adding parameters based on database type
                                if (dbTypeName == "Oracle")
                                {
                                    _sqlDatalayerDbCommand.Parameters.Add(new OracleParameter(dbParameterDictionary[dbTypeName] + columnName, columnValues[i]));
                                }
                                else if (dbTypeName == "Sql")
                                {
                                    _sqlDatalayerDbCommand.Parameters.Add(new SqlParameter(dbParameterDictionary[dbTypeName] + columnName, columnValues[i]));
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, object> columnDetails in serializableDataObject.Dictionary)
                    {
                        DataProperty columnDataproperty = objectType.dataProperties.Find(col => col.propertyName == columnDetails.Key);

                        if (columnDataproperty != null)
                        {
                            string columnName = string.Empty;

                            //Setting column name for POST operation
                            if (columnDataproperty.aliasDictionary != null && columnDataproperty.aliasDictionary["COLUMN_NAME_IN"] != null)
                            {
                                columnName = columnDataproperty.aliasDictionary["COLUMN_NAME_IN"];
                            }
                            else
                            {
                                columnName = columnDataproperty.columnName;
                            }

                            //Adding parameters based on database type
                            if (dbTypeName == "Oracle")
                            {
                                _sqlDatalayerDbCommand.Parameters.Add(new OracleParameter(dbParameterDictionary[dbTypeName] + columnName, columnDetails.Value));
                            }
                            else if (dbTypeName == "Sql")
                            {
                                _sqlDatalayerDbCommand.Parameters.Add(new SqlParameter(dbParameterDictionary[dbTypeName] + columnName, columnDetails.Value));
                            }
                        }
                    }
                }

                return _sqlDatalayerDbCommand.ExecuteNonQuery();
            }
            catch (Exception exceptionInPost)
            {
                throw exceptionInPost;
            }
        }

        /// <summary>
        /// Closes the database connection
        /// </summary>
        private void CloseDBConnection()
        {
            if (_sqlDatalayerDBConnection.State != ConnectionState.Closed)
            {
                _sqlDatalayerDBConnection.Close();
            }
        }

        #endregion
    }
}