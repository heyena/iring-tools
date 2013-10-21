using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using org.iringtools.library;
using System.Data.OleDb;
using System.IO;
using System.Data;
using log4net;
using org.iringtools.adapter;
using org.iringtools.utility;

namespace org.iringtools.adapter.datalayer
{
    public class LWDLSampleCode :BaseLightweightDataLayer   
    {
        private string _applicationName = string.Empty;
        private string _projectName = string.Empty;
        private string _xmlPath = string.Empty;
        private string _baseDirectory = string.Empty;
        private DatabaseDictionary _dictionary = null;
        private DataDictionary _dataDictionary = null;
        private string _connectionString = string.Empty;
        private SqlConnection _conn;
        private SqlDataAdapter _adapter = null;
        private SqlCommandBuilder _command = null;
        private static readonly ILog logger = LogManager.GetLogger(typeof(LWDLSampleCode));

        public LWDLSampleCode(AdapterSettings settings) : base(settings)
        {
         
            _projectName = settings["projectName"];            
            _applicationName = settings["applicationName"];
            _xmlPath = _settings["xmlPath"];
            _connectionString = _settings["DBConnectionString"];
            _conn = new SqlConnection(_connectionString);            
        }
       
        /// <summary>
        /// Dictionary : This function will create dictionary whenever required.
        /// dictionary can be generated from manual configuration file or automaatically created from database.
        /// </summary>
        /// <param name="refresh"></param>
        /// <param name="objectType"></param>
        /// <param name="dataFilter"></param>
        /// <returns></returns>
        public override DataDictionary Dictionary(bool refresh, string objectType, out DataFilter dataFilter)
        {
            dataFilter = null;
            try
            {
                string path = String.Format("{0}DataDictionary.{1}.{2}.xml", _xmlPath, _projectName, _applicationName);

                if ((File.Exists(path)))
                {
                    dynamic DataDictionary = Utility.Read<DataDictionary>(path);
                    _dataDictionary = Utility.Read<DataDictionary>(path);         
                    return _dataDictionary;
                }

                DataDictionary dataDictionary = new DataDictionary();
                string configPath = String.Format("{0}Configuration.{1}.{2}.xml", _xmlPath, _projectName, _applicationName);
                DataDictionary configDictionary = File.Exists(configPath) ? Utility.Read<DataDictionary>(configPath) : null;
                if (configDictionary != null)
                {
                    dataDictionary.dataObjects = configDictionary.dataObjects;
                    _dataDictionary = dataDictionary;
                    DatabaseDictionary _databaseDictionary = new DatabaseDictionary();
                    _databaseDictionary.dataObjects = _dataDictionary.dataObjects;
                    _databaseDictionary.ConnectionString = EncryptionUtility.Encrypt(_connectionString);
                    _databaseDictionary.Provider = "dbo";
                    _databaseDictionary.SchemaName = "dbo";
                    Utility.Write<DatabaseDictionary>(_databaseDictionary, String.Format("{0}DataBaseDictionary.{1}.{2}.xml", _xmlPath, _projectName, _applicationName));
                    Utility.Write<DataDictionary>(_dataDictionary, String.Format("{0}DataDictionary.{1}.{2}.xml", _xmlPath, _projectName, _applicationName));
                }
                return _dataDictionary;
            }
            catch (Exception ex)
            {
                string error = "Error in getting dictionary";                
                throw new Exception(error);
            }
        }

        /// <summary>
        /// Get : This function will be called by iring when ever GET request is made for any data object.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override List<SerializableDataObject> Get(DataObject objectType)
        {           
            IList<SerializableDataObject> dataObjects = null;
            DataTable result = null;         
            try
            {
                result = getDataTable(objectType);
                dataObjects = this.ToDataObjects(objectType, result);               
                return (List<SerializableDataObject>)dataObjects;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }       

        /// <summary>
        /// GetContents : This function is called by iRing if content of any content type object is requested.
        /// eg. If we have a content type object Documents and if user has requested for specific doc with format type is pdf or doc, This will return content fo that 
        /// doc.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="idFormats"></param>
        /// <returns></returns>
        public override List<IContentObject> GetContents(DataObject objectType, Dictionary<string, string> idFormats)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update : This function is called by iring whenever POST/PUT/DELETE of any object is requested.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="dataObjects"></param>
        /// <returns></returns>
        public override Response Update(DataObject objectType, List<SerializableDataObject> dataObjects)
        {
            throw new NotImplementedException();
        }

        private DataTable getDataTable(DataObject objectType)
        {
            string query = string.Empty;
            DataSet dataSet = new DataSet();
            try
            {
                // DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.objectName == objectType.objectName).FirstOrDefault();

                query = "SELECT ";
                bool isCommaNeeded = false;

                foreach (DataProperty property in objectType.dataProperties)
                {
                    if (!isCommaNeeded)
                        query = query + property.columnName;
                    else
                        query = query + " , " + property.columnName;

                    isCommaNeeded = true;
                }

                query = query + " from " + objectType.tableName;
                _conn.Open();
                _adapter = new SqlDataAdapter();
                _adapter.SelectCommand = new SqlCommand(query, _conn);
                _command = new SqlCommandBuilder(_adapter);
                _adapter.Fill(dataSet, objectType.objectName);
                DataTable dataTable = dataSet.Tables[objectType.objectName];
                return dataTable;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                query = string.Empty;
                if (_conn != null && _conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }


        }

        private new List<SerializableDataObject> ToDataObjects(DataObject objectType, DataTable dataTable)
        {
            List<SerializableDataObject> dataObjects = new List<SerializableDataObject>();
            SerializableDataObject dataObject = null;

            if (objectType != null && dataTable.Rows != null)
            {
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        try
                        {
                            dataObject = ConvertToDataObject(objectType, row);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        if (dataObjects != null)
                        {
                            dataObjects.Add(dataObject);
                        }
                    }
                }
            }

            return dataObjects;
        }

        private SerializableDataObject ConvertToDataObject(DataObject objectType, DataRow row)
        {
            SerializableDataObject dataObject = null;

            if (row != null)
            {
                try
                {
                    dataObject = new SerializableDataObject() { Type = objectType.objectName };
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (dataObject != null && objectType.dataProperties != null)
                {
                    foreach (DataProperty objectProperty in objectType.dataProperties)
                    {
                        try
                        {
                            if (objectProperty.propertyName != null)
                            {
                                if (row.Table.Columns.Contains(objectProperty.propertyName))
                                {
                                    object value = row[objectProperty.propertyName];

                                    if (value.GetType() == typeof(System.DBNull))
                                    {
                                        value = null;
                                    }

                                    dataObject.SetPropertyValue(objectProperty.propertyName, value);
                                }
                                else
                                {
                                    logger.Warn(String.Format("Value for column [{0}] not found in data row of table [{1}]", objectProperty.columnName, objectType.tableName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(string.Format("Error getting data row value: {0}", ex));
                            throw ex;
                        }
                    }
                }
            }
            else
            {
                dataObject = new SerializableDataObject() { Type = objectType.objectName };

                foreach (DataProperty objectProperty in objectType.dataProperties)
                {
                    dataObject.SetPropertyValue(objectProperty.propertyName, null);
                }
            }

            //if (row[HAS_CONTENT] != DBNull.Value && Convert.ToBoolean(row[HAS_CONTENT]))
            //{
            //    dataObject.HasContent = true;
            //}
            return dataObject;
        }
    }
}
