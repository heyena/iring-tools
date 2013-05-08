using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using org.iringtools.library;
using Ninject;
using Ninject.Injection;
using log4net;
using System.Data;
using System.IO;
using System.Collections.Concurrent;
using System.Collections;
using Microsoft.Win32;
using System.ServiceModel.Web;


namespace org.iringtools.adapter.datalayer
{
    public class iModelDatalayer:BaseSQLDataLayer
    {
          #region Variable Declaration
        private string applicationName = string.Empty;
        private string projectName = string.Empty;
        private string _xmlPath = string.Empty;
       
        private DatabaseDictionary _dictionary = null;
        private static readonly ILog logger = LogManager.GetLogger(typeof(iModelDatalayer));
        #endregion

        #region Constructor

        [Inject]
        public iModelDatalayer(AdapterSettings settings)
            : base(settings)
        {
            try
            {
                _settings = settings;
                projectName = _settings["ProjectName"];
                applicationName = _settings["ApplicationName"];
                string _userName = _settings["BechtelUserName"];
                
                _dictionary = this.GetDatabaseDictionary();
                
            }
            catch (WebFaultException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                string error = String.Format("Error in initiating construcotr");
                logger.Error(error);               
                throw ThrowWebFaultException(error, HttpStatusCode.OK);
            }
        }


        #endregion

        #region Public Functions

        #region Implemented

        /// <summary>
        ///  GetDatabaseDictionary()
        ///  This function read database dictionary , If not exist then create that and read.
        /// </summary>
        /// <returns></returns>
        public override DatabaseDictionary GetDatabaseDictionary()
        {
            string scope = string.Format("DatabaseDictionary.{0}.{1}.xml", projectName, applicationName);
            _xmlPath = Path.Combine(_settings["AppDataPath"], scope);

            if (!File.Exists(_xmlPath))
                this.GenerateDataDictionary();

            _dictionary = org.iringtools.utility.Utility.Read<DatabaseDictionary>(_xmlPath);

            return _dictionary;

            //throw new NotImplementedException();
        }

        #endregion


        #region Not Implemented

        public override DataTable CreateDataTable(string tableName, IList<string> identifiers)
        {
            throw new NotImplementedException();
        }

        public override Response DeleteDataTable(string tableName, IList<string> identifiers)
        {
            throw new NotImplementedException();
        }

        public override Response DeleteDataTable(string tableName, string whereClause)
        {
            throw new NotImplementedException();
        }

        public override long GetCount(string tableName, string whereClause)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetDataTable(string tableName, string whereClause, long start, long limit)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetDataTable(string tableName, IList<string> identifiers)
        {
            throw new NotImplementedException();
        }

        public override IList<string> GetIdentifiers(string tableName, string whereClause)
        {
            throw new NotImplementedException();
        }

        public override long GetRelatedCount(DataRow dataRow, string relatedTableName)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName, long start, long limit)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName)
        {
            throw new NotImplementedException();
        }

        public override Response PostDataTables(IList<DataTable> dataTables)
        {
            throw new NotImplementedException();
        }

        public override Response RefreshDataTable(string tableName)
        {
            throw new NotImplementedException();
        }



        #endregion       

        #endregion

        #region Private Functions

        /// <summary>
        /// ThrowWebFaultException()
        /// THis function takes error string and HTTP status code as input and return web fault exception.
        /// </summary>
        /// <param name="strError"></param>
        /// <param name="objCode"></param>
        /// <returns></returns>
        private WebFaultException ThrowWebFaultException(string strError, HttpStatusCode objCode)
        {
            WebFaultException FaultExcepObj = new WebFaultException(objCode);
            FaultExcepObj.Data.Add("StatusText", strError);
            logger.Error(strError);
            return FaultExcepObj;
        }


        private void GenerateDataDictionary()
        {
            try
            {
                DatabaseDictionary _databaseDictionary = new DatabaseDictionary();

                //Dictionary<string, int> dttye = new Dictionary<string, int>();

                //bool a = System.IO.File.Exists(_settings["AppDataPath"] + _settings["FilePath"]);
                //Model _objModel = new Model(_settings["AppDataPath"] +_settings["FilePath"] );
                Model _objModel = new Model( _settings["FilePath"]);

                //if (_objModel.Schemas.Count == 0)
                //{ 
                //    _objModel.Schemas.Add("rac_basic_sample_project_rvt_ThreeD_Aerial_01_00");
                //}
                List<string> _Schemas = _objModel.Schemas;



                foreach (string schema in _Schemas)
                {
                    List<string> _Tables = _objModel.GetTableNames(schema);
                    foreach (string tbName in _Tables)
                    {
                        DataObject dtObj = new DataObject();
                        dtObj.tableName = tbName;
                        dtObj.objectName = tbName;
                        KeyProperty _keyProperty = new KeyProperty();
                        _keyProperty.keyPropertyName = _objModel.GetBusinessKey(tbName);
                        if (_keyProperty.keyPropertyName != null)
                            dtObj.keyProperties.Add(_keyProperty);
                        
                        List<FieldInfo> _objFiledInfo = _objModel.GetFieldInfo(schema, tbName);
                        foreach (FieldInfo fieldinfo in _objFiledInfo)
                        {
                            DataProperty dtProperty = new DataProperty();
                            dtProperty.columnName = fieldinfo.Name;
                            dtProperty.propertyName = fieldinfo.Name;
                            dtProperty.dataType = this.getKeyType(fieldinfo.FiledType);
                            dtProperty.dataLength = fieldinfo.FiledLength;
                            dtProperty.isNullable = fieldinfo.isNullable;
                            dtObj.dataProperties.Add(dtProperty);

                            //if (!dttye.ContainsKey(fieldinfo.FiledType))
                            //    dttye.Add(fieldinfo.FiledType, fieldinfo.FiledLength);
                        }
                        _databaseDictionary.dataObjects.Add(dtObj);
                    }
                   
                }

                
                _databaseDictionary.Provider = Provider.MsSql2005.ToString();
                _databaseDictionary.SchemaName = "dbo";
                _databaseDictionary.ConnectionString = utility.Encryption.EncryptString(_objModel.connection.ConnectionString);    

                utility.Utility.Write<DatabaseDictionary>(_databaseDictionary, _xmlPath);
            }
            catch (WebFaultException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                string error = String.Format("Error in GenerateDataDictionary");
                logger.Error(error);
                throw ThrowWebFaultException(error, HttpStatusCode.OK);
            }
        }

        #region getKeyType

        /// <summary>
        /// getKeyType
        /// This function convert SQL datatype to compatible datatype of iRing.
        /// </summary>
        /// <param name="datatype"></param>
        /// <returns></returns>
        private DataType getKeyType(string datatype)
        {
            DataType dtype = new DataType();
            switch (datatype.ToUpper())
            {
                #region Assiging SQL DbType to SQL parameter

                case "WVARCHAR":
                    dtype = DataType.String;
                    break;
                case "WCHAR":
                    dtype = DataType.Char;
                    break;
                case "INTEGER":
                    dtype = DataType.Int32;
                    break;
                case "DOUBLE":
                    dtype = DataType.Double;
                    break;
                case "VARCHAR":
                    dtype = DataType.String;
                    break;
                case "NVARCHAR":
                    dtype = DataType.String;
                    break;
                case "INT":
                    dtype = DataType.Int32;
                    break;
                case "DECIMAL":
                    dtype = DataType.Decimal;
                    break;
                case "BINARY":
                    dtype = DataType.Byte;
                    break;
                case "BIT":
                    dtype = DataType.Boolean;
                    break;
                case "CHAR":
                    dtype = DataType.Char;
                    break;
                case "DATE;":
                    dtype = DataType.DateTime;
                    break;
                case "DATETIME":
                    dtype = DataType.DateTime;
                    break;
                case "DATETIME2":
                    dtype = DataType.DateTime;
                    break;
                case "DATETIMEOFFSET":
                    dtype = DataType.DateTime;
                    break;
                case "FLOAT":
                    dtype = DataType.Decimal;
                    break;
                case "IMAGE":
                    dtype = DataType.Byte;
                    break;
                case "MONEY":
                    dtype = DataType.Decimal;
                    break;
                case "NCHAR":
                    dtype = DataType.Char;
                    break;
                case "NTEXT":
                    dtype = DataType.String;
                    break;
                case "REAL":
                    dtype = DataType.Decimal;
                    break;
                case "SMALLDATETIME":
                    dtype = DataType.DateTime;
                    break;
                case "SMALLINT":
                    dtype = DataType.Int32;
                    break;
                case "SMALLMONEY":
                    dtype = DataType.Decimal;
                    break;
                case "STRUCTURED":
                    dtype = DataType.Reference;
                    break;
                case "TEXT":
                    dtype = DataType.String;
                    break;
                case "Time":
                    dtype = DataType.DateTime;
                    break;
                case "TIMESTAMP":
                    dtype = DataType.TimeStamp;
                    break;
                case "TINYINT":
                    dtype = DataType.Int32;
                    break;
                case "UDT":
                    dtype = DataType.String;
                    break;
                case "UNIQUEIDENTIFIER":
                    dtype = DataType.Int32;
                    break;
                case "VARBINARY":
                    dtype = DataType.Byte;
                    break;
                case "VARIANT":
                    dtype = DataType.Int32;
                    break;
                case "XML":
                    dtype = DataType.String;
                    break;

                #endregion
            }

            return dtype;
        }
        #endregion      

        #endregion
    }
}
