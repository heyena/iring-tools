using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using log4net;
using org.iringtools.adapter;
using org.iringtools.utility;

namespace org.iringtools.library
{
    public enum ObjectState { Create, Read, Update, Delete }

    public enum LoadingType { Lazy, Eager }

    [Serializable]
    public class SerializableDataObject : IDataObject
    {
        protected IDictionary<string, object> _dictionary = new Dictionary<string, object>();

        public IDictionary<string, object> Dictionary
        {
            get
            {
                return _dictionary;
            }
        }

        public string Id { get; set; }

        public string Type { get; set; }

        public ObjectState State { get; set; }

        public bool HasContent { get; set; }

        public Stream Content { get; set; }

        public string ContentType { get; set; }

        public object GetPropertyValue(string propertyName)
        {
            if (_dictionary.ContainsKey(propertyName))
                return _dictionary[propertyName];

            return null;
        }

        public void SetPropertyValue(string propertyName, object propertyValue)
        {
            _dictionary[propertyName] = propertyValue;
        }
    }

    public interface ILightweightDataLayer
    {
        /// <summary>
        ///  Provides information types to expose to iRING adapter
        /// </summary>
        /// <param name="refresh">whether to rebuild or use previous dictionary</param>
        /// <param name="objectType">refresh a specific object type or all if null</param>
        /// <param name="dataFilter">any filter to apply to data objects if any</param>
        /// <returns>objects schema</returns>
        DataDictionary Dictionary(bool refresh, string objectType, out DataFilter dataFilter);

        /// <summary>
        /// Gets all data records for a given object type
        /// </summary>
        /// <param name="objectType">required - one of the object types in data dictionary</param>
        /// <returns>list of data objects</returns>
        List<SerializableDataObject> Get(DataObject objectType);

        /// <summary>
        /// Updates list of data objects being modified and their related objects if configured
        /// </summary>
        /// <param name="dataObjects">list of data objects</param>
        /// <returns>detail status of each data record being modified</returns>
        Response Update(DataObject objectType, List<SerializableDataObject> dataObjects);

        /// <summary>
        /// Gets a list of content objects by identifiers and optionally their renditions
        /// </summary>
        /// <param name="idFormats">list of identifiers and renditions</param>
        /// <returns>binary contents with mime types and metadata</returns>
        List<IContentObject> GetContents(DataObject objectType, Dictionary<string, string> idFormats);
    }

    public abstract class BaseLightweightDataLayer : ILightweightDataLayer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseLightweightDataLayer));
        public const string SELECT_SQL_TPL = "SELECT * FROM {0} {1}";
        public const string SELECT_COUNT_SQL_TPL = "SELECT COUNT(*) FROM {0} {1}";
        public const string INSERT_SQL_TPL = "INSERT INTO {0} ({1}) VALUES ({2})";
        public const string UPDATE_SQL_TPL = "UPDATE {0} SET {1} {2}";
        public const string DELETE_SQL_TPL = "DELETE FROM {0} {1}";
        public const string HAS_CONTENT = "_hasContent_";

        protected AdapterSettings _settings;

        public BaseLightweightDataLayer(AdapterSettings settings)
        {
            _settings = settings;
        }

        public abstract DataDictionary Dictionary(bool refresh, string objectType, out DataFilter dataFilter);

        public abstract List<SerializableDataObject> Get(DataObject objectType);

        public abstract Response Update(DataObject objectType, List<SerializableDataObject> dataObjects);

        public abstract List<IContentObject> GetContents(DataObject objectType, Dictionary<string, string> idFormats);

        public static string CreateUpdateSQL(string tableName, DataObject objectType, SerializableDataObject dataObject)
        {
            switch (dataObject.State)
            {
                case ObjectState.Create:
                    {
                        StringBuilder colsBuilder = new StringBuilder();
                        StringBuilder valsBuilder = new StringBuilder();

                        foreach (var prop in dataObject.Dictionary)
                        {
                            DataProperty dataProp = objectType.dataProperties.Find(x => x.propertyName == prop.Key);
                            colsBuilder.Append("," + prop.Key);

                            if (BaseLightweightDataLayer.IsNumeric(dataProp.dataType))
                            {
                                if (prop.Value != null && prop.Value!="")
                                {
                                    valsBuilder.Append("," + prop.Value);
                                }
                                else
                                {
                                    valsBuilder.Append("," + "null");
                                }
                            }
                            else if (dataProp.dataType == DataType.Date || dataProp.dataType == DataType.DateTime)
                            {
                                DateTime dateTime = Utility.FromXsdDateTime(prop.Value.ToString());
                                valsBuilder.Append("," + "'" + dateTime.ToString() + "'");
                            }
                            else
                            {
                                valsBuilder.Append("," + "'" + prop.Value + "'");
                            }
                        }

                        if (dataObject.HasContent)
                        {
                            colsBuilder.Append("," + HAS_CONTENT);
                            valsBuilder.Append("," + "'" + dataObject.HasContent + "'");
                        }

                        string insertSQL = string.Format(INSERT_SQL_TPL, tableName, colsBuilder.Remove(0, 1), valsBuilder.Remove(0, 1));
                        return insertSQL;
                    }
                case ObjectState.Update:
                    {
                        StringBuilder builder = new StringBuilder();
                        string whereClause = BaseLightweightDataLayer.FormWhereClause(objectType, dataObject.Id);

                        foreach (var prop in dataObject.Dictionary)
                        {
                            DataProperty dataProp = objectType.dataProperties.Find(x => x.propertyName == prop.Key);

                            if (BaseLightweightDataLayer.IsNumeric(dataProp.dataType))
                            {
                                if (prop.Value != null && prop.Value != "")
                                {
                                    builder.Append("," + prop.Key + "=" + prop.Value);
                                }
                                else
                                {
                                    builder.Append("," + prop.Key + "=" + "null");
                                }
                               
                            }
                            else if (dataProp.dataType == DataType.Date || dataProp.dataType == DataType.DateTime)
                            {
                                DateTime dateTime = Utility.FromXsdDateTime(prop.Value.ToString());
                                builder.Append("," + prop.Key + "='" + dateTime.ToString() + "'");
                            }
                            else
                            {
                                builder.Append("," + prop.Key + "='" + prop.Value + "'");
                            }
                        }

                        if (dataObject.HasContent)
                        {
                            builder.Append("," + HAS_CONTENT + "='" + dataObject.HasContent + "'");
                        }

                        string updateSQL = string.Format(UPDATE_SQL_TPL, tableName, builder.Remove(0, 1), whereClause);
                        return updateSQL;
                    }
                case ObjectState.Delete:
                    {
                        string whereClause = BaseLightweightDataLayer.FormWhereClause(objectType, dataObject.Id);
                        string deleteSQL = string.Format(DELETE_SQL_TPL, tableName, whereClause);
                        return deleteSQL;
                    }
            }

            return string.Empty;
        }

        public static string GetIdentifier(DataObject objectType, IDataObject dataObject)
        {
            string[] identifierParts = new string[objectType.keyProperties.Count];

            int i = 0;
            foreach (KeyProperty keyProperty in objectType.keyProperties)
            {
                object value = dataObject.GetPropertyValue(keyProperty.keyPropertyName);
                if (value != null)
                {
                    identifierParts[i] = value.ToString();
                }
                else
                {
                    identifierParts[i] = String.Empty;
                }

                i++;
            }

            return String.Join(objectType.keyDelimeter, identifierParts);
        }

        public static string FormWhereClause(DataObject objectType, string identifier)
        {
            return FormWhereClause(objectType, new List<string> { identifier });
        }

        public static string FormWhereClause(DataObject objectType, IList<string> identifiers)
        {
            try
            {
                StringBuilder clauseBuilder = new StringBuilder();
                string[] delim = new string[] { objectType.keyDelimeter };

                foreach (string id in identifiers)
                {
                    StringBuilder exprBuilder = new StringBuilder();
                    string[] idParts = id.Split(delim, StringSplitOptions.None);

                    if (objectType.keyProperties.Count > 1)
                    {
                        idParts = id.Split(delim, StringSplitOptions.None);
                    }
                    else
                    {
                        idParts[0] = id;
                    }



                    for (int i = 0; i < objectType.keyProperties.Count; i++)
                    {
                        string key = objectType.keyProperties[i].keyPropertyName;
                        DataProperty prop = objectType.dataProperties.Find(x => x.propertyName.ToLower() == key.ToLower());

                        string expr = string.Empty;

                        if (IsNumeric(prop.dataType))
                        {
                            expr = string.Format("{0} = {1}", key, idParts[i]);
                        }
                        else if (string.IsNullOrEmpty(idParts[i]))
                        {
                            expr = string.Format("({0} = '' OR {0} IS NULL)", key);
                        }
                        else
                        {
                            expr = string.Format("{0} = '{1}'", key, idParts[i]);
                        }

                        if (exprBuilder.Length > 0)
                        {
                            exprBuilder.Append(" AND ");
                        }

                        exprBuilder.Append(expr);
                    }

                    if (clauseBuilder.Length > 0)
                    {
                        clauseBuilder.Append(" OR ");
                    }

                    clauseBuilder.Append("(" + exprBuilder.ToString() + ")");
                }

                if (clauseBuilder.Length > 0)
                {
                    clauseBuilder.Insert(0, " WHERE ");
                }

                return clauseBuilder.ToString();
            }
            catch (Exception ex)
            {
                string error = "Error forming WHERE clause: " + ex;
                _logger.Error(error);
                throw new Exception(error);
            }
        }

        public static List<string> FormIdentifiers(DataObject objectType, DataTable dataTable)
        {
            List<string> identifiers = new List<string>();

            try
            {
                if (objectType != null && dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        identifiers.Add(FormIdentifier(objectType, row));
                    }
                }
            }
            catch (Exception ex)
            {
                string error = "Error forming identifiers from data rows: " + ex.Message;
                _logger.Error(error);
                throw new Exception(error);
            }

            return identifiers;
        }

        public static string FormIdentifier(DataObject objectType, DataRow dataRow)
        {
            try
            {
                string[] identifierParts = new string[objectType.keyProperties.Count];
                IDataObject dataObject = ToDataObject(objectType, dataRow);

                for (int i = 0; i < objectType.keyProperties.Count; i++)
                {
                    KeyProperty keyProp = objectType.keyProperties[i];
                    object value = dataObject.GetPropertyValue(keyProp.keyPropertyName);

                    if (value != null)
                    {
                        identifierParts[i] = value.ToString();
                    }
                    else
                    {
                        identifierParts[i] = String.Empty;
                    }
                }

                return string.Join(objectType.keyDelimeter, identifierParts);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error forming identifier from data row: {0}", ex));
                throw ex;
            }
        }

        public static List<IDataObject> ToDataObjects(DataObject objectType, DataTable dataTable)
        {
            List<IDataObject> dataObjects = new List<IDataObject>();
            IDataObject dataObject = null;

            if (objectType != null && dataTable.Rows != null)
            {
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        try
                        {
                            dataObject = ToDataObject(objectType, row);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Error converting data row to data object: {0}", ex));
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

        public static IDataObject ToDataObject(DataObject objectType, DataRow row)
        {
            GenericDataObject dataObject = null;

            if (row != null)
            {
                try
                {
                    dataObject = new GenericDataObject() { ObjectType = objectType.objectName };
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Error instantiating data object: {0}", ex));
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
                                    _logger.Warn(String.Format("Value for column [{0}] not found in data row of table [{1}]",
                                      objectProperty.columnName, objectType.tableName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Error getting data row value: {0}", ex));
                            throw ex;
                        }
                    }
                }
            }
            else
            {
                dataObject = new GenericDataObject() { ObjectType = objectType.objectName };

                foreach (DataProperty objectProperty in objectType.dataProperties)
                {
                    dataObject.SetPropertyValue(objectProperty.propertyName, null);
                }
            }

            if (row[HAS_CONTENT] != DBNull.Value && Convert.ToBoolean(row[HAS_CONTENT]))
            {
                dataObject.HasContent = true;
            }

            return dataObject;
        }

        public static bool IsNumeric(DataType dataType)
        {
            return (
              dataType == DataType.Int16 ||
              dataType == DataType.Int32 ||
              dataType == DataType.Int64 ||
              dataType == DataType.Single ||
              dataType == DataType.Double ||
              dataType == DataType.Decimal
            );
        }

        public static List<SerializableDataObject> ReadDataObjects(string fileName)
        {
            List<SerializableDataObject> dataObjects = new List<SerializableDataObject>();
            BinaryFormatter serializer = new BinaryFormatter();

            try
            {
                Stream stream = File.OpenRead(fileName);
                return ReadDataObjects(stream);
            }
            catch (Exception e)
            {
                _logger.Error("Error reading binary object from file [" + fileName + "]: " + e.Message);
                throw e;
            }
        }

        public static List<SerializableDataObject> ReadDataObjects(Stream stream)
        {
            List<SerializableDataObject> dataObjects = new List<SerializableDataObject>();
            BinaryFormatter serializer = new BinaryFormatter();

            try
            {
                stream.Position = 0;
                //dataObjects = (List<SerializableDataObject>)serializer.Deserialize(stream);
                while (stream.Position != stream.Length)
                {
                    dataObjects.AddRange((List<SerializableDataObject>)serializer.Deserialize(stream));
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error reading binary object from stream: " + e.Message);
                throw e;
            }

            return dataObjects;
        }

        public static void WriteDataObjects(string fileName, List<SerializableDataObject> dataObjects)
        {
            BinaryFormatter serializer = new BinaryFormatter();

            try
            {
                using (var stream = File.OpenWrite(fileName))
                {
                    serializer.Serialize(stream, dataObjects);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error writing binary object from: " + fileName);
                throw e;
            }
        }
    }
}
