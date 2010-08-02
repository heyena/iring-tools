// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
using org.iringtools.adapter.datalayer;

namespace org.iringtools.application
{
  public class ApplicationProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ApplicationProvider));

    private Response _response = null;
    private IKernel _kernel = null;
    private ApplicationSettings _settings = null;
    //private WebProxyCredentials _proxyCredentials = null;

    bool _isScopeInitialized = false;

    AdapterClient _adapterClient = null;

    [Inject]
    public ApplicationProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(new ApplicationModule());
      _settings = _kernel.Get<ApplicationSettings>();
      _settings.AppendSettings(settings);

      _settings["AdapterXmlPath"] = Path.Combine(_settings["AdapterPath"], _settings["XmlPath"]);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      _adapterClient = new AdapterClient(_settings);

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
              new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateDataLayer")
            )
          );

          _response.Append(_adapterClient.PostBinding(binding));

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

        databaseDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
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
        string connString = dbDictionary.connectionString;
        string dbProvider = dbDictionary.provider.ToString();
        dbProvider = dbProvider.ToUpper();
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        dbDictionary = new DatabaseDictionary();
        Dictionary<string, string> properties = new Dictionary<string, string>();
        string metadataQuery = string.Empty;
        dbDictionary.connectionString = parsedConnStr;
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
              dbDictionary.provider = Provider.MsSql2008;
              properties.Add("dialect", "NHibernate.Dialect.MsSql2008Dialect");
              break;

            case "MSSQL2005":
              dbDictionary.provider = Provider.MsSql2005;
              properties.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");
              break;

            case "MSSQL2000":
              dbDictionary.provider = Provider.MsSql2000;
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
              dbDictionary.provider = Provider.Oracle10g;
              properties.Add("dialect", "NHibernate.Dialect.Oracle10gDialect");
              break;

            case "ORACLE9I":
              dbDictionary.provider = Provider.Oracle9i;
              properties.Add("dialect", "NHibernate.Dialect.Oracle9iDialect");
              break;

            case "ORACLE8I":
              dbDictionary.provider = Provider.Oracle8i;
              properties.Add("dialect", "NHibernate.Dialect.Oracle8iDialect");
              break;

            case "ORACLELITE":
              dbDictionary.provider = Provider.OracleLite;
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
              dbDictionary.provider = Provider.MySql3;
              properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
              break;
            case "MYSQL4":
              dbDictionary.provider = Provider.MySql4;
              properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
              break;
            case "MYSQL5":
              dbDictionary.provider = Provider.MySql5;
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
              // dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
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

    public List<string> GetRelationships()
    {
      try
      {
        return Enum.GetNames(typeof(RelationshipType)).ToList();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetRelationships: " + ex);
        return null;
      }
    }

    public List<string> GetProviders()
    {
      try
      {
        return Enum.GetNames(typeof(Provider)).ToList();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetProviders: " + ex);
        return null;
      }
    }

    public List<string> GetSchemaObjects(string projectName, string applicationName)
    {
      List<string> tableNames = new List<string>();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      try
      {
        InitializeScope(projectName, applicationName);
        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);

        string connString = dbDictionary.connectionString;
        string dbProvider = dbDictionary.provider.ToString();
        dbProvider = dbProvider.ToUpper();
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.connectionString = parsedConnStr;
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
        ISQLQuery query = session.CreateSQLQuery(GetDatabaseMetaquery(dbProvider, parsedConnStr.Split(';')[1].Split('=')[1]));

        IList<string> metadataList = query.List<string>();
        session.Close();

        tableNames = metadataList as List<string>;
        return tableNames;
      }
      catch (Exception)
      {
        return tableNames;
      }
    }

    public org.iringtools.library.DataObject GetSchemaObjectSchema(string projectName, string applicationName, string schemaObjectName)
    {
      List<string> tableNames = new List<string>();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      org.iringtools.library.DataObject dataObject = new org.iringtools.library.DataObject
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

        string connString = dbDictionary.connectionString;
        string dbProvider = dbDictionary.provider.ToString();
        dbProvider = dbProvider.ToUpper();
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.connectionString = parsedConnStr;
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

        ISQLQuery query = session.CreateSQLQuery(GetTableMetaQuery(dbProvider, parsedConnStr.Split(';')[1].Split('=')[1], schemaObjectName));
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


    #endregion

    #region private methods

    private string GetTableMetaQuery(string dbProvider, string databaseName, string objectName)
    {
      string tableQuery = string.Empty;

      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        tableQuery = string.Format("SELECT t1.COLUMN_NAME, t1.DATA_TYPE, t2.max_length, t2.is_identity, t2.is_nullable, t5.CONSTRAINT_TYPE " +
                                   " FROM INFORMATION_SCHEMA.COLUMNS AS t1 INNER JOIN sys.columns AS t2 ON t2.name = t1.COLUMN_NAME INNER JOIN " +
                                   " sys.tables AS t3 ON t3.name = t1.TABLE_NAME AND t3.object_id = t2.object_id LEFT OUTER JOIN " +
                                   " INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS t4 ON t4.TABLE_NAME = t1.TABLE_NAME AND t4.COLUMN_NAME = t1.COLUMN_NAME LEFT OUTER JOIN " +
                                   " INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS t5 ON t5.CONSTRAINT_NAME = t4.CONSTRAINT_NAME " +
                                   " WHERE (t1.DATA_TYPE NOT IN ('image')) AND (t1.TABLE_CATALOG = '{0}') AND (t1.TABLE_NAME = '{1}') "
                                   , databaseName
                                   , objectName);
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        tableQuery = string.Format("select t1.COLUMN_NAME, t1.DATA_TYPE, t1.CHARACTER_MAXIMUM_LENGTH, t1.COLUMN_KEY, t1.IS_NULLABLE, c1.CONSTRAINT_TYPE " +
                                   " from INFORMATION_SCHEMA.COLUMNS t1 join KEY_COLUMN_USAGE u1 on u1.TABLE_NAME = t1.TABLE_NAME and u1.TABLE_SCHEMA = t1.TABLE_SCHEMA and " +
                                   " t1.COLUMN_NAME = u1.COLUMN_NAME join INFORMATION_SCHEMA.TABLE_CONSTRAINTS c1 on u1.CONSTRAINT_NAME = c1.CONSTRAINT_NAME and u1.TABLE_NAME = c1.TABLE_NAME " +
                                   " where t1.TABLE_SCHEMA = '{0}' and t1.TABLE_NAME = '{1}'"
                                   , databaseName
                                   , objectName
                                   );
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        tableQuery = string.Format(" select t2.column_name, t2.data_type, t2.data_length, 0 as is_sequence, t2.nullable, t4.constraint_type " +
                                   " from user_objects t1 inner join all_tab_cols t2 on t2.table_name = t1.object_name " +
                                   " left join all_cons_columns t3 on t3.table_name = t2.table_name and t3.column_name = t2.column_name " +
                                   " left join all_constraints t4 on t4.constraint_name = t3.constraint_name and (t4.constraint_type = 'P' or t4.constraint_type = 'R') " +
                                   " where t1.object_type = 'TABLE' and t1.object_name = '{0}' order by t1.object_name, t4.constraint_type, t2.column_name"
                                   , objectName
                                   );

      }
      return tableQuery;
    }

    private string GetDatabaseMetaquery(string dbProvider, string database)
    {
      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        return "select table_name from INFORMATION_SCHEMA.TABLES order by table_name";
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        return string.Format("select table_name from INFORMATION_SCHEMA.TABLES where table_schema = '{0}' order by table_name;",database);
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        return "select object_name from user_objects where object_type = 'TABLE' order by object_name";
      }
      else
        throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));
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

          _settings.Add("ProjectName", projectName);
          _settings.Add("ApplicationName", applicationName);
          _settings.Add("Scope", scope);

          _settings["DBDictionaryPath"] = String.Format("{0}DatabaseDictionary.{1}.xml",
            _settings["AdapterXmlPath"],
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

    private void RemoveDups(DataObject dataObject)
    {
      try
      {
        /* GvR
        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          for (int j = 0; j < dataObject.dataProperties.Count; j++)
          {
            // remove columns that are already in keys
            if (dataObject.dataProperties[j].propertyName.ToLower() == dataObject.keyProperties[i].propertyName.ToLower())
            {
              dataObject.dataProperties.Remove(dataObject.dataProperties[j--]);
              continue;
            }

            // remove duplicate columns
            for (int jj = j + 1; jj < dataObject.dataProperties.Count; jj++)
            {
              if (dataObject.dataProperties[jj].propertyName.ToLower() == dataObject.dataProperties[j].propertyName.ToLower())
              {
                dataObject.dataProperties.Remove(dataObject.dataProperties[jj--]);
              }
            }
          }

          // remove duplicate keys (in order of foreign - assigned - iddataObject/sequence)
          for (int ii = i + 1; ii < dataObject.keyProperties.Count; ii++)
          {
            if (dataObject.keyProperties[ii].columnName.ToLower() == dataObject.keyProperties[i].columnName.ToLower())
            {
              if (dataObject.keyProperties[ii].keyType != KeyType.foreign)
              {
                if (((dataObject.keyProperties[ii].keyType == KeyType.identity || dataObject.keyProperties[ii].keyType == KeyType.sequence) && dataObject.keyProperties[i].keyType == KeyType.assigned) ||
                      dataObject.keyProperties[ii].keyType == KeyType.assigned && dataObject.keyProperties[i].keyType == KeyType.foreign)
                {
                  dataObject.keyProperties[i].keyType = dataObject.keyProperties[ii].keyType;
                }
              }

              dataObject.keyProperties.Remove(dataObject.keyProperties[ii--]);
            }
          }
        } */
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private bool ValidateDatabaseDictionary(DatabaseDictionary dbDictionary)
    {
      ISession session = null;

      try
      {
        // Validate connection string
        string connectionString = dbDictionary.connectionString;
        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        Dictionary<string, string> properties = new Dictionary<string, string>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("connection.connection_string", dbDictionary.connectionString);
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("dialect", "NHibernate.Dialect." + dbDictionary.provider + "Dialect");

        if (dbDictionary.provider.ToString().ToUpper().Contains("MSSQL"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
        }
        else if (dbDictionary.provider.ToString().ToUpper().Contains("ORACLE"))
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