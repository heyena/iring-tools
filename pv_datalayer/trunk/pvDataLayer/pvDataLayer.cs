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
    public class pvDataLayer : BaseSQLDataLayer
    {
        #region Variable Declaration
        private string applicationName = string.Empty;
        private string projectName = string.Empty;
        private string _xmlPath = string.Empty;
        private string _dbServer = string.Empty;
        private string _database = string.Empty;
        private string _userId = string.Empty;
        private string _Password = string.Empty;
        private string _dbInstance = string.Empty;
        private DatabaseDictionary _dictionary = null;
        private static readonly ILog logger = LogManager.GetLogger(typeof(pvDataLayer));
        #endregion

        #region Constructor

        [Inject]
        public pvDataLayer(AdapterSettings settings)
            : base(settings)
        {
            try
            {
                _settings = settings;
                projectName = _settings["ProjectName"];
                applicationName = _settings["ApplicationName"];
                string _userName = _settings["BechtelUserName"];
                 _dbServer = _settings["dbServer"];
                 _database = _settings["Database"];
                 _userId = _settings["UserId"];
                 _Password = _settings["Password"];
                 _dbInstance = _settings["dbInstance"];
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
                //port-1433
                org.iringtools.nhibernate.NHibernateProvider obj = new nhibernate.NHibernateProvider(_settings);
                DatabaseDictionary objdb = obj.GetDictionary(projectName, applicationName);
               
                List<string> _datatables = obj.GetTableNames(projectName, applicationName, Provider.MsSql2005.ToString(), _dbServer, "", _dbInstance, _database,
                      "dbo", _userId, _Password, "");

                string _tableNames = string.Empty;
                foreach (string tablename in _datatables)
                {
                    _tableNames = _tableNames + tablename + ",";
                }
                _tableNames = _tableNames.Remove(_tableNames.LastIndexOf(','));
                List<library.DataObject> _dataobjects = obj.GetDBObjects(projectName, applicationName, Provider.MsSql2005.ToString(), _dbServer, "", _dbInstance, _database,
                      "dbo", _userId, _Password, _tableNames, "");
                library.DatabaseDictionary objectsDataDictionary = new DatabaseDictionary();
                objectsDataDictionary.dataObjects.AddRange(_dataobjects);
                objectsDataDictionary.Provider = Provider.MsSql2005.ToString();
                objectsDataDictionary.SchemaName = "dbo";
                string _connectionString = @"server=" + _dbServer + "\"" + _dbInstance + ";database=" + _database + ";UserId=" + _userId + ";Password=" + _Password;
                objectsDataDictionary.ConnectionString = utility.EncryptionUtility.Encrypt(_connectionString);
                utility.Utility.Write<DatabaseDictionary>(objectsDataDictionary, _xmlPath);

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

        #endregion

    }
}
