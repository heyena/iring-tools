// Copyright (c) 2010, iringtools.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL + exEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using log4net;
using Ninject;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using NHibernate;
using org.iringtools.adapter;

namespace org.iringtools.nhibernate
{
  public class NHibernateProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateProvider));

    private Response _response = null;
    private IKernel _kernel = null;
    private NHibernateSettings _settings = null;
    //private WebProxyCredentials _proxyCredentials = null;

    bool _isScopeInitialized = false;

    AdapterProvider _adapterProvider = null;

    [Inject]
    public NHibernateProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(new NHibernateModule());
      _settings = _kernel.Get<NHibernateSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      _adapterProvider = new AdapterProvider(_settings);

      _response = new Response();
      _kernel.Bind<Response>().ToConstant(_response);
    }

    #region public methods
    public Response Generate(string projectName, string applicationName)
    {
      Status status = new Status();

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        DatabaseDictionary dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(applicationName))
        {
          status.Messages.Add("Error project name and application name can not be null");
        }
        else if (ValidateDatabaseDictionary(dbDictionary))
        {
          EntityGenerator generator = _kernel.Get<EntityGenerator>();
          _response.Append(generator.Generate(dbDictionary, projectName, applicationName));
          
          // Update binding configuration
          XElement binding = new XElement("module",
            new XAttribute("name", _settings["Scope"]),
            new XElement("bind",
              new XAttribute("name", "DataLayer"),
              new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
              new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary")
            )
          );

          Response localResponse = _adapterProvider.UpdateBinding(projectName, applicationName, binding);

          _response.Append(localResponse);

          status.Messages.Add("Database dictionary updated successfully.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateDatabaseDictionary: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error updating database dictionary: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    public DatabaseDictionary GetDictionary(string projectName, string applicationName)
    {
      DatabaseDictionary databaseDictionary = new DatabaseDictionary();
      try
      {
        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
        {
          databaseDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        }
        else
        {
          databaseDictionary = new DatabaseDictionary();
          Utility.Write<DatabaseDictionary>(databaseDictionary, _settings["DBDictionaryPath"], true);
        }

      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetDbDictionary: " + ex);
        return null;
      }
      return databaseDictionary;
    }

    public Response PostDictionary(string projectName, string applicationName, DatabaseDictionary databaseDictionary)
    {
      Status status = new Status();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        Utility.Write<DatabaseDictionary>(databaseDictionary, _settings["DBDictionaryPath"]);

        status.Messages.Add("Database Dictionary saved successfully");
      }
      catch (Exception ex)
      {
        _logger.Error("Error in SaveDatabaseDictionary: " + ex);
        status.Messages.Add("Error in saving database dictionary" + ex.Message);
      }

      _response.Append(status);
      return _response;
    }

    public DatabaseDictionary GetDatabaseSchema(string projectName, string applicationName)
    {
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      try
      {

        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        else
        {
          Utility.Write<DatabaseDictionary>(dbDictionary, _settings["DBDictionaryPath"], true);
          return dbDictionary;
        }
        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString();
        dbProvider = dbProvider.ToUpper();
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        dbDictionary = new DatabaseDictionary();
        Dictionary<string, string> properties = new Dictionary<string, string>();
        string metadataQuery = string.Empty;
        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);

        if (dbProvider.Contains("MSSQL"))
        {
          metadataQuery =
              "select t1.table_name, t1.column_name, t1.data_type, t2.max_length, t2.is_identity, t2.is_nullable, t5.constraint_type " +
              "from information_schema.columns t1 " +
              "inner join sys.columns t2 on t2.name = t1.column_name " +
              "inner join sys.tables t3 on t3.name = t1.table_name and t3.object_id = t2.object_id " +
              "left join information_schema.key_column_usage t4 on t4.table_name = t1.table_name and t4.column_name = t1.column_name " +
              "left join information_schema.table_constraints t5 on t5.constraint_name = t4.constraint_name " +
              "where t1.data_type not in ('image') " +
              "order by t1.table_name, t5.constraint_type, t1.column_name";// +
          properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");

          switch (dbProvider)
          {
            case "MSSQL2008":
              dbDictionary.Provider = Provider.MsSql2008.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MsSql2008Dialect");
              break;

            case "MSSQL2005":
              dbDictionary.Provider = Provider.MsSql2005.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");
              break;

            case "MSSQL2000":
              dbDictionary.Provider = Provider.MsSql2000.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MsSql2000Dialect");
              break;

            default:
              throw new Exception("Database provider not supported.");
          }
        }
        else if (dbProvider.Contains("ORACLE"))
        {
          metadataQuery =
            "select t1.object_name, t2.column_name, t2.data_type, t2.data_length, 0 as is_sequence, t2.nullable, t4.constraint_type " +
            "from user_objects t1 " +
            "inner join all_tab_cols t2 on t2.table_name = t1.object_name " +
            "left join all_cons_columns t3 on t3.table_name = t2.table_name and t3.column_name = t2.column_name " +
            "left join all_constraints t4 on t4.constraint_name = t3.constraint_name and (t4.constraint_type = 'P' or t4.constraint_type = 'R') " +
            "where t1.object_type = 'TABLE' order by t1.object_name, t4.constraint_type, t2.column_name";
          properties.Add("connection.driver_class", "NHibernate.Driver.OracleClientDriver");

          switch (dbProvider)
          {
            case "ORACLE10G":
              dbDictionary.Provider = Provider.Oracle10g.ToString();
              properties.Add("dialect", "NHibernate.Dialect.Oracle10gDialect");
              break;

            case "ORACLE9I":
              dbDictionary.Provider = Provider.Oracle9i.ToString();
              properties.Add("dialect", "NHibernate.Dialect.Oracle9iDialect");
              break;

            case "ORACLE8I":
              dbDictionary.Provider = Provider.Oracle8i.ToString();
              properties.Add("dialect", "NHibernate.Dialect.Oracle8iDialect");
              break;

            case "ORACLELITE":
              dbDictionary.Provider = Provider.OracleLite.ToString();
              properties.Add("dialect", "NHibernate.Dialect.OracleLiteDialect");
              break;

            default:
              throw new Exception("Database provider not supported.");
          }
        }
        else if (dbProvider.Contains("MYSQL"))
        {
          metadataQuery = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, COLUMN_KEY, IS_NULLABLE " +
                          "FROM INFORMATION_SCHEMA.COLUMNS " +
                          string.Format("WHERE TABLE_SCHEMA = '{0}'", connString.Split(';')[1].Split('=')[1]);
          properties.Add("connection.driver_class", "NHibernate.Driver.MySqlDataDriver");

          switch (dbProvider)
          {
            case "MYSQL3":
              dbDictionary.Provider = Provider.MySql3.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
              break;
            case "MYSQL4":
              dbDictionary.Provider = Provider.MySql4.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
              break;
            case "MYSQL5":
              dbDictionary.Provider = Provider.MySql5.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MySQL5Dialect");
              break;
          }
        }


        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        config.AddProperties(properties);

        ISessionFactory sessionFactory = config.BuildSessionFactory();
        ISession session = sessionFactory.OpenSession();
        ISQLQuery query = session.CreateSQLQuery(metadataQuery);
        IList<object[]> metadataList = query.List<object[]>();
        session.Close();

        DataObject table = null;
        string prevTableName = String.Empty;
        foreach (object[] metadata in metadataList)
        {
          string tableName = Convert.ToString(metadata[0]);
          string columnName = Convert.ToString(metadata[1]);
          string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[2]));
          int dataLength = Convert.ToInt32(metadata[3]);
          bool isIdentity = Convert.ToBoolean(metadata[4]);
          string nullable = Convert.ToString(metadata[5]).ToUpper();
          bool isNullable = (nullable == "Y" || nullable == "TRUE");
          string constraint = Convert.ToString(metadata[6]);

          if (tableName != prevTableName)
          {
            table = new DataObject()
            {
              tableName = tableName,
              dataProperties = new List<DataProperty>(),
              keyProperties = new List<KeyProperty>(),
              dataRelationships = new List<DataRelationship>(), // to be supported in the future
              objectName = Utility.NameSafe(tableName)
            };

            dbDictionary.dataObjects.Add(table);
            prevTableName = tableName;
          }

          if (String.IsNullOrEmpty(constraint)) // process columns
          {
            DataProperty column = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              propertyName = Utility.NameSafe(columnName)
            };

            table.dataProperties.Add(column);
          }
          else // process keys
          {
            KeyType keyType = KeyType.assigned;

            if (isIdentity)
            {
              keyType = KeyType.identity;
            }
            else if (constraint.ToUpper() == "FOREIGN KEY" || constraint.ToUpper() == "R")
            {
              keyType = KeyType.foreign;
            }

            DataProperty key = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              keyType = keyType,
              propertyName = Utility.NameSafe(columnName),
            };

            table.addKeyProperty(key);
          }
        }
        return dbDictionary;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetDatabaseSchema: " + ex);
        return dbDictionary;
      }
    }

    public DataRelationships GetRelationships()
    {
      try
      {
        return new DataRelationships();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetRelationships: " + ex);
        return null;
      }
    }

    public DataProviders GetProviders()
    {
      try
      {
        return new DataProviders();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetProviders: " + ex);
        return null;
      }
    }

    public DataObjects GetSchemaObjects(string projectName, string applicationName)
    {
      DataObjects tableNames = new DataObjects();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();

      try
      {
          _logger.Debug("I'm in!!!");

        InitializeScope(projectName, applicationName);
        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        else
          return tableNames;

        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString().ToUpper();
        string schemaName = dbDictionary.SchemaName;
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        _logger.DebugFormat("ConnectString: {0} \r\n Provider: {1} \r\n SchemaName: {2} \r\n Parsed: {3}",
            connString,
            dbProvider,
            schemaName,
            parsedConnStr);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);

        string connDriver = GetConnectionDriver(dbProvider);

        _logger.DebugFormat("connection.driver_class: {0}", connDriver);

        properties.Add("connection.driver_class", connDriver);

        string databaseDialect = GetConnectionDriver(dbProvider);

        _logger.DebugFormat("dialect: {0}", databaseDialect);

        properties.Add("dialect", databaseDialect);

        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();

        _logger.Debug("Adding Properties to Config");

        config.AddProperties(properties);

        _logger.Debug("Building Session Factory");

        ISessionFactory sessionFactory = config.BuildSessionFactory();

        _logger.Debug("About to Open Session");

        ISession session = sessionFactory.OpenSession();

        _logger.Debug("Session Open");

        string sql = GetDatabaseMetaquery(dbProvider, parsedConnStr.Split(';')[1].Split('=')[1], schemaName);

        _logger.DebugFormat("SQL: {0}",
            sql);

        ISQLQuery query = session.CreateSQLQuery(sql);

        DataObjects metadataList = new DataObjects();
        foreach (string tableName in query.List<string>())
        {
          metadataList.Add(tableName);
        }
        session.Close();

        tableNames = metadataList;
        return tableNames;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while Getting Schema Objects from database. {0}",
              ex.ToString());

        return tableNames;
      }
    }

    public DataObject GetSchemaObjectSchema(string projectName, string applicationName, string schemaObjectName)
    {
      List<string> tableNames = new List<string>();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      DataObject dataObject = new DataObject
      {
        tableName = schemaObjectName,
        dataProperties = new List<DataProperty>(),
        keyProperties = new List<KeyProperty>(),
        dataRelationships = new List<DataRelationship>(),
        objectName = Utility.NameSafe(schemaObjectName)
      };
      try
      {
        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);

        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString().ToUpper();
        string schemaName = dbDictionary.SchemaName;
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);
        properties.Add("connection.driver_class", GetConnectionDriver(dbProvider));
        properties.Add("dialect", GetDatabaseDialect(dbProvider));

        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        config.AddProperties(properties);

        ISessionFactory sessionFactory = config.BuildSessionFactory();
        ISession session = sessionFactory.OpenSession();
        ISQLQuery query = session.CreateSQLQuery(GetTableMetaQuery(dbProvider, parsedConnStr.Split(';')[1].Split('=')[1], schemaName, schemaObjectName));
        IList<object[]> metadataList = query.List<object[]>();        
        session.Close();

        foreach (object[] metadata in metadataList)
        {
          string columnName = Convert.ToString(metadata[0]);
          string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[1]));
          int dataLength = Convert.ToInt32(metadata[2]);
          bool isIdentity = Convert.ToBoolean(metadata[3]);
          string nullable = Convert.ToString(metadata[4]).ToUpper();
          bool isNullable = (nullable == "Y" || nullable == "TRUE");
          string constraint = Convert.ToString(metadata[5]);

          if (String.IsNullOrEmpty(constraint)) // process columns
          {
            DataProperty column = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              propertyName = Utility.NameSafe(columnName)
            };

            dataObject.dataProperties.Add(column);
          }
          else
          {
            KeyType keyType = KeyType.assigned;

            if (isIdentity)
            {
              keyType = KeyType.identity;
            }
            else if (constraint.ToUpper() == "FOREIGN KEY" || constraint.ToUpper() == "R")
            {
              keyType = KeyType.foreign;
            }

            DataProperty key = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              keyType = keyType,
              propertyName = Utility.NameSafe(columnName),
            };
            dataObject.addKeyProperty(key);
          }
        }
        return dataObject;
      }
      catch (Exception)
      {
        return dataObject;
      }
    }    

    public VersionInfo GetVersion()
    {
      Version version = this.GetType().Assembly.GetName().Version;
      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }

    public List<string> GetTableNames(string projectName, string applicationName, string dbProvider, 
      string dbServer, string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword)
    {
      List<string> tableNames = new List<string>();
      
      try
      {
        InitializeScope(projectName, applicationName);

        List<DataObject> dataObjects = new List<DataObject>();
        ISession session = GetNHSession(dbProvider, dbServer, dbInstance, dbName, dbSchema, dbUserName, dbPassword);
        string sql = GetDatabaseMetaquery(dbProvider, dbName, dbSchema);
        ISQLQuery query = session.CreateSQLQuery(sql);

        foreach (string tableName in query.List<string>())
        {
          tableNames.Add(tableName);
        }

        session.Close();
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return tableNames;
    }

    public List<DataObject> GetDBObjects(string projectName, string applicationName, string dbProvider, string dbServer,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames)
    {
      List<DataObject> dataObjects = new List<DataObject>();
      ISession session = GetNHSession(dbProvider, dbServer, dbInstance, dbName, dbSchema, dbUserName, dbPassword);

      foreach (string tableName in tableNames.Split(','))
      {
        DataObject dataObject = new DataObject() 
        {
          tableName = tableName,
          objectName = Utility.NameSafe(tableName) 
        };

        string sql = GetTableMetaQuery(dbProvider, dbName, dbSchema, tableName);
        ISQLQuery query = session.CreateSQLQuery(sql);
        IList<object[]> metadataList = query.List<object[]>();  

        foreach (object[] metadata in metadataList)
        {
          string columnName = Convert.ToString(metadata[0]);
          string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[1]));
          int dataLength = Convert.ToInt32(metadata[2]);
          bool isIdentity = Convert.ToBoolean(metadata[3]);
          string nullable = Convert.ToString(metadata[4]).ToUpper();
          bool isNullable = (nullable == "Y" || nullable == "TRUE");
          string constraint = Convert.ToString(metadata[5]);

          if (String.IsNullOrEmpty(constraint)) // process columns
          {
            DataProperty column = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              propertyName = Utility.NameSafe(columnName) 
            };

            dataObject.dataProperties.Add(column);
          }
          else
          {
            KeyType keyType = KeyType.assigned;

            if (isIdentity)
            {
              keyType = KeyType.identity;
            }
            else if (constraint.ToUpper() == "FOREIGN KEY" || constraint.ToUpper() == "R")
            {
              keyType = KeyType.foreign;
            }

            DataProperty key = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              keyType = keyType,
              propertyName = Utility.NameSafe(columnName),
            };

            dataObject.addKeyProperty(key);
          }
        }

        dataObjects.Add(dataObject);
      }

      session.Close();

      return dataObjects;
    }
    #endregion

    #region private methods
    private ISession GetNHSession(string dbProvider, string dbServer, string dbInstance, string dbName, string dbSchema, 
      string dbUserName, string dbPassword)
    {
      string connStr = String.Format("Data Source={0}\\{1};Initial Catalog={2};User ID={3};Password={4}", 
        dbServer, dbInstance, dbName, dbUserName, dbPassword);
      Dictionary<string, string> properties = new Dictionary<string, string>();

      properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
      properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
      properties.Add("connection.connection_string", connStr);
      properties.Add("connection.driver_class", GetConnectionDriver(dbProvider));
      properties.Add("dialect", GetDatabaseDialect(dbProvider));

      NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
      config.AddProperties(properties);

      ISessionFactory sessionFactory = config.BuildSessionFactory();
      return sessionFactory.OpenSession();
    }

    private string GetTableMetaQuery(string dbProvider, string databaseName, string schemaName, string objectName)
    {
      string tableQuery = string.Empty;

      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        tableQuery = string.Format(
          "SELECT t1.COLUMN_NAME, t1.DATA_TYPE, t2.max_length, t2.is_identity, t2.is_nullable, t5.CONSTRAINT_TYPE " +
          "FROM INFORMATION_SCHEMA.COLUMNS AS t1 INNER JOIN sys.columns AS t2 ON t2.name = t1.COLUMN_NAME INNER JOIN  sys.schemas AS ts ON " +
          "ts.name = t1.TABLE_SCHEMA INNER JOIN  sys.tables AS t3 ON t3.schema_id = ts.schema_id AND t3.name = t1.TABLE_NAME AND " +
          "t3.object_id = t2.object_id LEFT OUTER JOIN  INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS t4 ON t4.TABLE_SCHEMA = t1.TABLE_SCHEMA AND " +
          "t4.TABLE_NAME = t1.TABLE_NAME AND t4.COLUMN_NAME = t1.COLUMN_NAME LEFT OUTER JOIN  INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS t5 ON " +
          "t5.CONSTRAINT_NAME = t4.CONSTRAINT_NAME AND t5.CONSTRAINT_SCHEMA = t4.TABLE_SCHEMA WHERE (t1.DATA_TYPE NOT IN ('image')) AND " +
          "(t1.TABLE_CATALOG = '{0}') AND (t1.TABLE_SCHEMA = '{1}') AND (t1.TABLE_NAME = '{2}')",
          databaseName,
          schemaName,
          objectName
        );
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        tableQuery = string.Format(
          "select t1.COLUMN_NAME, t1.DATA_TYPE, t1.CHARACTER_MAXIMUM_LENGTH, t1.COLUMN_KEY, t1.IS_NULLABLE, c1.CONSTRAINT_TYPE " +
          " from INFORMATION_SCHEMA.COLUMNS t1 join KEY_COLUMN_USAGE u1 on u1.TABLE_NAME = t1.TABLE_NAME and u1.TABLE_SCHEMA = t1.TABLE_SCHEMA and " +
          " t1.COLUMN_NAME = u1.COLUMN_NAME join INFORMATION_SCHEMA.TABLE_CONSTRAINTS c1 on u1.CONSTRAINT_NAME = c1.CONSTRAINT_NAME and u1.TABLE_NAME = c1.TABLE_NAME " +
          " where t1.TABLE_SCHEMA = '{0}' and t1.TABLE_NAME = '{1}'",
          schemaName,
          objectName
        );
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {       
        tableQuery = string.Format(@"
          select distinct * from (SELECT t2.column_name, t2.data_type, t2.data_length,
          0 AS is_sequence, t2.nullable, t4.constraint_type
          FROM dba_objects t1 INNER JOIN all_tab_cols t2
          ON t2.table_name = t1.object_name AND t2.owner = t2.owner 
          LEFT JOIN all_cons_columns t3 ON t3.table_name = t2.table_name
          AND t3.column_name = t2.column_name AND t3.owner = t2.owner
          AND SUBSTR(t3.constraint_name, 0, 3) != 'SYS' LEFT JOIN all_constraints t4
          ON t4.constraint_name = t3.constraint_name AND t4.owner = t3.owner
          AND (t4.constraint_type = 'P' OR t4.constraint_type = 'R')
          WHERE UPPER(t1.owner) = '{0}' AND UPPER(t1.object_name) = '{1}')", 
          schemaName.ToUpper(),
          objectName.ToUpper());
      }
      return tableQuery;
    }

    private string GetDatabaseMetaquery(string dbProvider, string database, string schemaName)
    {
      string metaQuery = String.Empty;

      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        metaQuery = String.Format("select table_name from INFORMATION_SCHEMA.TABLES WHERE table_schema = '{0}' order by table_name", schemaName);
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        metaQuery = String.Format("select table_name from INFORMATION_SCHEMA.TABLES where table_schema = '{0}' order by table_name;", schemaName);
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        metaQuery = String.Format("select object_name from dba_objects where object_type in ('TABLE', 'VIEW', 'SYNONYM') and UPPER(owner) = '{0}' order by object_name", schemaName.ToUpper());
      }
      else
        throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));

      return metaQuery;
    }

    private string GetDatabaseDialect(string dbProvider)
    {
      switch (dbProvider.ToUpper())
      {
        case "MSSQL2008":
          return "NHibernate.Dialect.MsSql2008Dialect";

        case "MSSQL2005":
          return "NHibernate.Dialect.MsSql2005Dialect";

        case "MSSQL2000":
          return "NHibernate.Dialect.MsSql2000Dialect";

        case "ORACLE10G":
          return "NHibernate.Dialect.Oracle10gDialect";

        case "ORACLE9I":
          return "NHibernate.Dialect.Oracle9iDialect";

        case "ORACLE8I":
          return "NHibernate.Dialect.Oracle8iDialect";

        case "ORACLELITE":
          return "NHibernate.Dialect.OracleLiteDialect";

        case "MYSQL3":
        case "MYSQL4":
        case "MYSQL5":
          return "NHibernate.Dialect.MySQL5Dialect";

        default:
          throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));
      }
    }

    private string GetConnectionDriver(string dbProvider)
    {
      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        return "NHibernate.Driver.SqlClientDriver";
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        return "NHibernate.Driver.MySqlDataDriver";
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        return "NHibernate.Driver.OracleClientDriver";
      }
      else
        throw new Exception(string.Format("Database provider {0} is not supported", dbProvider));
    }

    private void InitializeScope(string projectName, string applicationName)
    {
      try
      {
        if (!_isScopeInitialized)
        {
          string scope = string.Format("{0}.{1}", projectName, applicationName);

          _settings["ProjectName"] = projectName;
          _settings["ApplicationName"] = applicationName;
          _settings["Scope"] = scope;

          _settings["DBDictionaryPath"] = String.Format("{0}DatabaseDictionary.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private bool ValidateDatabaseDictionary(DatabaseDictionary dbDictionary)
    {
      ISession session = null;

      try
      {
        // Validate connection string
        string connectionString = dbDictionary.ConnectionString;
        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        Dictionary<string, string> properties = new Dictionary<string, string>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("connection.connection_string", dbDictionary.ConnectionString);
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("default_schema", dbDictionary.SchemaName);
        properties.Add("dialect", "NHibernate.Dialect." + dbDictionary.Provider + "Dialect");

        if (dbDictionary.Provider.ToString().ToUpper().Contains("MSSQL"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
        }
        else if (dbDictionary.Provider.ToString().ToUpper().Contains("ORACLE"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.OracleClientDriver");
        }
        else
        {
          throw new Exception("Database not supported.");
        }

        config.AddProperties(properties);
        ISessionFactory factory = config.BuildSessionFactory();

        session = factory.OpenSession();
      }
      catch (Exception ex)
      {
        throw new Exception("Invalid connection string: " + ex.Message);
      }
      finally
      {
        if (session != null) session.Close();
      }

      // Validate table key
      foreach (DataObject dataObject in dbDictionary.dataObjects)
      {
        if (dataObject.keyProperties == null || dataObject.keyProperties.Count == 0)
        {
          throw new Exception(string.Format("Table \"{0}\" has no key.", dataObject.tableName));
        }
      }

      return true;
    }

    private static string ParseConnectionString(string connStr, string dbProvider)
    {
      try
      {
        string parsedConnStr = String.Empty;
        char[] ch = { ';' };
        string[] connStrKeyValuePairs = connStr.Split(ch, StringSplitOptions.RemoveEmptyEntries);

        foreach (string connStrKeyValuePair in connStrKeyValuePairs)
        {
          string[] connStrKeyValuePairTemp = connStrKeyValuePair.Split('=');
          string connStrKey = connStrKeyValuePairTemp[0].Trim();
          string connStrValue = connStrKeyValuePairTemp[1].Trim();

          if (connStrKey.ToUpper() == "DATA SOURCE" ||
              connStrKey.ToUpper() == "USER ID" ||
              connStrKey.ToUpper() == "PASSWORD")
          {
            parsedConnStr += connStrKey + "=" + connStrValue + ";";
          }

          if (dbProvider.ToUpper().Contains("MSSQL"))
          {
            if (connStrKey.ToUpper() == "INITIAL CATALOG" ||
                connStrKey.ToUpper() == "INTEGRATED SECURITY")
            {
              parsedConnStr += connStrKey + "=" + connStrValue + ";";
            }
          }
          else if (dbProvider.ToUpper().Contains("MYSQL"))
          {
            parsedConnStr += connStrKey + "=" + connStrValue + ";";
          }
        }

        return parsedConnStr;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
    #endregion
  }
}