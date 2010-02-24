using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using org.iringtools.utility;
using org.iringtools.library;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Dialect;

namespace DBDictionaryUtil
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        string method = String.Empty;
        string connStr = String.Empty;
        string dbProvider = String.Empty;
        string adapterServiceUri = String.Empty;
        string projectName = String.Empty;
        string applicationName = String.Empty;
        string dbDictionaryFilePath = String.Empty;

        if (args.Length >= 1)
        {
          method = args[0];

          if (method.ToUpper() == "CREATE")
          {
            if (args.Length >= 4)
            {
              connStr = args[1];
              dbProvider = args[2];
              dbDictionaryFilePath = args[3];

              if (String.IsNullOrEmpty(connStr) || String.IsNullOrEmpty(dbDictionaryFilePath))
              {
                PrintUsage();
              }
              else
              {
                DoCreate(connStr, dbProvider, dbDictionaryFilePath);
              }
            }
            else
            {
              PrintUsage();
            }
          }
          else if (method.ToUpper() == "POST")
          {
            if (args.Length >= 5)
            {
              adapterServiceUri = args[1];
              projectName = args[2];
              applicationName = args[3];
              dbDictionaryFilePath = args[4];

              if (String.IsNullOrEmpty(adapterServiceUri) ||
                  String.IsNullOrEmpty(projectName) ||
                  String.IsNullOrEmpty(applicationName) ||
                  String.IsNullOrEmpty(dbDictionaryFilePath))
              {
                PrintUsage();
              }
              else
              {
                DoPost(adapterServiceUri, projectName, applicationName, dbDictionaryFilePath);
              }
            }
            else
            {
              PrintUsage();
            }
          }
          else
          {
            throw new Exception("Method not supported.");
          }
        }
        else
        {
          method = ConfigurationManager.AppSettings["Method"];

          if (method.ToUpper() == "CREATE")
          {
            connStr = ConfigurationManager.AppSettings["ConnectionString"];
            dbProvider = ConfigurationManager.AppSettings["DBProvider"];
            dbDictionaryFilePath = ConfigurationManager.AppSettings["DBDictionaryOutFilePath"];

            if (String.IsNullOrEmpty(connStr) || String.IsNullOrEmpty(dbDictionaryFilePath))
            {
              PrintUsage();
            }
            else
            {
              DoCreate(connStr, dbProvider, dbDictionaryFilePath);
            }            
          }
          else if (method.ToUpper() == "POST")
          {
            adapterServiceUri = ConfigurationManager.AppSettings["AdapterServiceUri"];
            projectName = ConfigurationManager.AppSettings["ProjectName"];
            applicationName = ConfigurationManager.AppSettings["ApplicationName"];
            dbDictionaryFilePath = ConfigurationManager.AppSettings["DBDictionaryInFilePath"];

            if (String.IsNullOrEmpty(adapterServiceUri) ||
                String.IsNullOrEmpty(projectName) ||
                String.IsNullOrEmpty(applicationName) ||
                String.IsNullOrEmpty(dbDictionaryFilePath))
            {
              PrintUsage();
            }
            else
            {
              DoPost(adapterServiceUri, projectName, applicationName, dbDictionaryFilePath);
            }
          }
          else
          {
            throw new Exception("Method not supported.");
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("*** ERROR ***\n " + ex);
      }

      Console.WriteLine("\nPress any key to continue ...");
      Console.ReadKey();
    }

    static void DoCreate(string connStr, string dbProvider, string dbDictionaryFilePath)
    {
      Console.WriteLine("Creating database dictionary from connection string...");

      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      dbDictionary.connectionString = connStr;
      dbDictionary.tables = new List<Table>();

      string metadataQuery = String.Empty;
      Dictionary<string, string> properties = new Dictionary<string, string>();

      properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
      properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
      properties.Add("connection.connection_string", connStr);

      dbProvider = dbProvider.ToUpper();
      if (dbProvider.Contains("MSSQL"))
      {
        metadataQuery =
            "select t1.table_name, t1.column_name, t1.data_type, t5.constraint_type, t2.is_identity from information_schema.columns t1 " +
            "inner join sys.columns t2 on t2.name = t1.column_name " +
            "inner join sys.tables t3 on t3.name = t1.table_name and t3.object_id = t2.object_id " +
            "left join information_schema.key_column_usage t4 on t4.table_name = t1.table_name and t4.column_name = t1.column_name " +
            "left join information_schema.table_constraints t5 on t5.constraint_name = t4.constraint_name " +
            "order by t1.table_name, t5.constraint_type, t1.column_name";
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
          "select t1.object_name, t2.column_name, t2.data_type, t4.constraint_type, 0 as is_sequence from user_objects t1 " +
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

      NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
      config.AddProperties(properties);
      
      ISessionFactory sessionFactory = config.BuildSessionFactory();
      ISession session = sessionFactory.OpenSession();
      ISQLQuery query = session.CreateSQLQuery(metadataQuery);
      IList<object[]> metadataList = query.List<object[]>();
      session.Close();

      Table table = null;
      string prevTableName = String.Empty;
      foreach (object[] metadata in metadataList)
      {
        string tableName = Convert.ToString(metadata[0]);
        string columnName = Convert.ToString(metadata[1]);
        string dataType = Convert.ToString(metadata[2]);
        string constraint = Convert.ToString(metadata[3]);
        bool isIdentity = Convert.ToBoolean(metadata[4]);

        if (tableName != prevTableName)
        {
          table = new Table()
          {
            tableName = tableName,
            columns = new List<Column>(),
            keys = new List<Key>(),
            associations = new List<Association>(), // to be supported in the future
            entityName = NameSafe(tableName)
          };

          dbDictionary.tables.Add(table);
          prevTableName = tableName;
        }

        if (String.IsNullOrEmpty(constraint)) // process columns
        {
          Column column = new Column()
          {
            columnName = columnName,
            columnType = SqlTypeToCSharpType(dataType),
            propertyName = NameSafe(columnName)
          };

          table.columns.Add(column);
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

          Key key = new Key()
          {
            columnName = columnName,
            columnType = SqlTypeToCSharpType(dataType),
            keyType = keyType,
            propertyName = NameSafe(columnName),
          };

          table.keys.Add(key);
        }
      }

      Utility.Write<DatabaseDictionary>(dbDictionary, dbDictionaryFilePath);
      Console.WriteLine("Database dictionary created successfully.");
      Console.WriteLine("See result file at \"" + dbDictionaryFilePath + "\"");
    }

    static void DoPost(string adapterServiceUri, string projectName, string applicationName, string dbDictionaryFilePath)
    {
      Console.WriteLine("Posting " + dbDictionaryFilePath + " to iRING Adapter Service...");
             
      string relativeUri = "/" + projectName + "/" + applicationName + "/dbdictionary";
      DatabaseDictionary dbDictionary = Utility.Read<DatabaseDictionary>(dbDictionaryFilePath);
      WebHttpClient httpClient = new WebHttpClient(adapterServiceUri, null);
      Response response = httpClient.Post<DatabaseDictionary, Response>(relativeUri, dbDictionary, true);
      
      foreach (string line in response)
      {
        Console.WriteLine(line);
      }
    }

    static ColumnType SqlTypeToCSharpType(string columnDataType)
    {
      switch (columnDataType.ToUpper())
      {
        case "BIT":
          return ColumnType.Boolean;
        case "BYTE":
          return ColumnType.Byte;
        case "CHAR":
          return ColumnType.String;
        case "VARCHAR":
          return ColumnType.String;
        case "VARCHAR2":
          return ColumnType.String;
        case "NVARCHAR":
          return ColumnType.String;
        case "NVARCHAR2":
          return ColumnType.String;
        case "TEXT":
          return ColumnType.String;
        case "NTEXT":
          return ColumnType.String;
        case "XML":
          return ColumnType.String;
        case "DATE":
          return ColumnType.DateTime;
        case "DATETIME":
          return ColumnType.DateTime;
        case "SMALLDATETIME":
          return ColumnType.DateTime;
        case "TIME":
          return ColumnType.DateTime;
        case "TIMESTAMP":
          return ColumnType.DateTime;
        case "DECIMAL":
          return ColumnType.Double;
        case "MONEY":
          return ColumnType.Double;
        case "SMALLMONEY":
          return ColumnType.Double;
        case "NUMERIC":
          return ColumnType.Double;
        case "FLOAT":
          return ColumnType.Double;
        case "REAL":
          return ColumnType.Double;
        case "INT":
          return ColumnType.Int32;
        case "NUMBER":
          return ColumnType.Int32;
        case "BIGINT":
          return ColumnType.Int64;
        case "SMALLINT":
          return ColumnType.Int16;
        case "TINYINT":
          return ColumnType.Int16;
        default:
          throw new Exception("Column data type not supported.");
      }
    }

    static string NameSafe(string name)
    {
      return Regex.Replace(name, @"^\d*|\W", "");
    }

    static void PrintUsage()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("\n\tDBDictionaryUtil.exe CREATE ConnectionString DatabaseDictionaryFilePath");
      Console.WriteLine("\n\tDBDictionaryUtil.exe POST AdapterServiceUri ProjectName ApplicationName DatabaseDictionaryFilePath\n");
    }
  }
}
