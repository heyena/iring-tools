using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using org.iringtools.utility;
using org.iringtools.library;
using System.Text.RegularExpressions;
using NHibernate;

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
        string adapterServiceUri = String.Empty;
        string projectName = String.Empty;
        string applicationName = String.Empty;
        string dbDictionaryFilePath = String.Empty;

        if (args.Length >= 1)
        {
          method = args[0];

          if (method.ToUpper() == "CREATE")
          {
            if (args.Length >= 3)
            {
              connStr = args[1];
              dbDictionaryFilePath = args[2];

              if (String.IsNullOrEmpty(connStr) || String.IsNullOrEmpty(dbDictionaryFilePath))
              {
                PrintUsage();
              }
              else
              {
                DoCreate(connStr, dbDictionaryFilePath);
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
            dbDictionaryFilePath = ConfigurationManager.AppSettings["DBDictionaryFilePath"];

            if (String.IsNullOrEmpty(connStr) || String.IsNullOrEmpty(dbDictionaryFilePath))
            {
              PrintUsage();
            }
            else
            {
              DoCreate(connStr, dbDictionaryFilePath);
            }            
          }
          else if (method.ToUpper() == "POST")
          {
            adapterServiceUri = ConfigurationManager.AppSettings["AdapterServiceUri"];
            projectName = ConfigurationManager.AppSettings["ProjectName"];
            applicationName = ConfigurationManager.AppSettings["ApplicationName"];
            dbDictionaryFilePath = ConfigurationManager.AppSettings["DBDictionaryFilePath"];

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

    static void DoCreate(string connStr, string dbDictionaryFilePath)
    {
      Console.WriteLine("Creating database dictionary from connection string...");

      string sqlServerMetadataQuery =
          "select t1.table_name, t1.column_name, t1.data_type, t5.constraint_type, t2.is_identity from information_schema.columns t1 " +
          "inner join sys.columns t2 on t2.name = t1.column_name " +
          "inner join sys.tables t3 on t3.name = t1.table_name and t3.object_id = t2.object_id " +
          "left join information_schema.key_column_usage t4 on t4.table_name = t1.table_name and t4.column_name = t1.column_name " +
          "left join information_schema.table_constraints t5 on t5.constraint_name = t4.constraint_name " +
          "order by t1.table_name, t5.constraint_type, t1.column_name ";

      Dictionary<string, string> properties = new Dictionary<string, string>();
      properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
      properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
      properties.Add("connection.connection_string", connStr);
      properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
      properties.Add("dialect", "Dialect.MsSql2008Dialect");

      NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
      config.AddProperties(properties);
      ISessionFactory sessionFactory = config.BuildSessionFactory();
      ISession session = sessionFactory.OpenSession();

      ISQLQuery query = session.CreateSQLQuery(sqlServerMetadataQuery);
      IList<object[]> metadataList = query.List<object[]>();

      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      dbDictionary.connectionString = connStr;
      dbDictionary.provider = Provider.MsSql2008;
      dbDictionary.tables = new List<Table>();
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
            columnType = SQLServerTypeToCSharpType(dataType),
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
          else if (constraint.ToLower() == "foreign key")
          {
            keyType = KeyType.foreign;
          }

          Key key = new Key()
          {
            columnName = columnName,
            columnType = SQLServerTypeToCSharpType(dataType),
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

    static ColumnType SQLServerTypeToCSharpType(string columnDataType)
    {
      switch (columnDataType.ToLower())
      {
        case "bit":
          return ColumnType.Boolean;
        case "byte":
          return ColumnType.Byte;
        case "char":
          return ColumnType.String;
        case "varchar":
          return ColumnType.String;
        case "nvarchar":
          return ColumnType.String;
        case "text":
          return ColumnType.String;
        case "ntext":
          return ColumnType.String;
        case "xml":
          return ColumnType.String;
        case "date":
          return ColumnType.DateTime;
        case "datetime":
          return ColumnType.DateTime;
        case "smalldatetime":
          return ColumnType.DateTime;
        case "time":
          return ColumnType.DateTime;
        case "timestamp":
          return ColumnType.DateTime;
        case "decimal":
          return ColumnType.Double;
        case "money":
          return ColumnType.Double;
        case "smallmoney":
          return ColumnType.Double;
        case "numeric":
          return ColumnType.Double;
        case "float":
          return ColumnType.Double;
        case "real":
          return ColumnType.Double;
        case "int":
          return ColumnType.Int32;
        case "bigint":
          return ColumnType.Int64;
        case "smallint":
          return ColumnType.Int16;
        case "tinyint":
          return ColumnType.Int16;
        default:
          throw new Exception("Column data type not handled.");
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
