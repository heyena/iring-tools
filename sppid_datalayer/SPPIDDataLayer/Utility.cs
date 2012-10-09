using System;
using System.Text.RegularExpressions;
using log4net;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer.sppid
{
  public class Constants
  {
    public const string DATA_PATH = "AppDataPath";
    public const string PROJECT = "ProjectName";
    public const string APPLICATION = "ApplicationName";

    public const string SPPID_STAGING = "SPPID.Staging";
    public const string SPPID_SITE_SCHEMA = "SPPID.SiteSchema";
    public const string SPPID_SITE_DICTIONARY = "SPPID.SiteDictionary";
    public const string SPPID_PLANT_SCHEMA = "SPPID.PlantSchema";
    public const string SPPID_PLANT_DICTIONARY = "SPPID.PlantDictionary";
    public const string SPPID_PID_SCHEMA = "SPPID.PIDSchema";
    public const string SPPID_PID_DICTIONARY = "SPPID.PIDDictionary";

    public const string TEMPLATE_QUERY = "!Template";
    public const string SITE_DATA_QUERY = "!SiteData";

    public const string SEQUENCE_SUFFIX = "_Sequence";
    public const string TRIGGER_SUFFIX = "_Trigger";

    public const string ORACLE_GET_CURRENT_SCHEMA = "SELECT sys_context('userenv', 'CURRENT_SCHEMA') FROM dual";
    public const string ORACLE_GET_TABLES = "SELECT * FROM tab WHERE TABTYPE = 'TABLE'";

    // {0} trigger name
    // {1} table name
    // {2} sequence name
    public const string ORACLE_SEQUENCE_TRIGGER_TEMPLATE = @"{0}
      BEFORE INSERT ON {1} 
      REFERENCING NEW AS NEW 
      FOR EACH ROW
      BEGIN
      SELECT {2}.nextval INTO :NEW.ID FROM dual;
      END;";

    // {0} table name
    public const string SQLSERVER_DELETE_TEMPLATE = @"
      IF EXISTS (SELECT object_id FROM sys.objects 
      WHERE object_id = OBJECT_ID(N'dbo.[{0}]') AND type in (N'U')) 
      DROP TABLE dbo.[{0}]";

    // {0} privilege name
    // {1} object name
    // {2} user name
    public const string GRANT_PRIVILEGE_TEMPLATE = "GRANT {0} ON {1} TO {2}";

    // {0} privilege name
    // {1} object name
    // {2} user name
    public const string REVOKE_PRIVILEGE_TEMPLATE = "REVOKE {0} ON {1} FROM {2}";

    public const string SQLSERVER_GET_USER_TABLES = "SELECT * FROM sys.objects WHERE type='U'";
    public const string ORACLE_GET_USER_TABLES = "SELECT * FROM tab WHERE TABTYPE='TABLE'";

    // {0} privilge list concatenated by a comma
    // {1} user name
    public const string ORACLE_GRANT_PRVILEGES_TEMPLATE = @"
      BEGIN FOR x IN (" + ORACLE_GET_USER_TABLES + @") LOOP  
      EXECUTE IMMEDIATE 'GRANT {0}
      ON ' || x.Tname || ' TO {1}'; END LOOP; END;";

    // {0} privilge list concatenated by a comma
    // {1} user name
    public const string ORACLE_REVOKE_PRVILEGES_TEMPLATE = @"
      BEGIN FOR x IN (" + ORACLE_GET_USER_TABLES + @") LOOP 
      EXECUTE IMMEDIATE 'REVOKE {0} 
      ON ' || x.Tname || ' FROM {1}'; END LOOP; END;";

    // {0} privilge list concatenated by a comma
    // {1} user name
    public const string SQLSERVER_GRANT_PRVILEGES_TEMPLATE = @"
      BEGIN FOR x IN (" + SQLSERVER_GET_USER_TABLES + @") LOOP  
      EXECUTE IMMEDIATE 'GRANT {0}
      ON ' || x.Tname || ' TO {1}'; END LOOP; END;";

    // {0} privilge list concatenated by a comma
    // {1} user name
    public const string SQLSERVER_REVOKE_PRVILEGES_TEMPLATE = @"
      BEGIN FOR x IN (" + SQLSERVER_GET_USER_TABLES + @") LOOP 
      EXECUTE IMMEDIATE 'REVOKE {0} 
      ON ' || x.Tname || ' FROM {1}'; END LOOP; END;";

    // {0} table name
    // {1} column names
    // {2} column values
    public const string SQL_INSERT_TEMPLATE = "INSERT INTO {0} ({1}) VALUES ({2})";

    // {0} table name
    // {1} column1=value1, column2=value2, columnN=valueN
    // {2} where clause
    public const string SQL_UPDATE_TEMPLATE = "UPDATE {0} SET {1} {2}";

    // {0} table name
    // {1} where clause
    public const string SQL_DELETE_TEMPLATE = "DELETE FROM {0} {1}";
  }

  public class Utility
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Utility));
    
    public static DBType GetDBType(string connStr)
    {
      string str = Regex.Replace(connStr, @"\s+", " ").ToUpper();

      if (str.Contains("SERVICE_NAME=") || str.Contains("SID="))
        return DBType.ORACLE;

      return DBType.SQLServer;
    }

    public static bool IsBase64Encoded(string text)
    {
      string pattern = "^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{4}|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)$";
      return Regex.IsMatch(text, pattern);
    }

    public static bool IsNumeric(string sqlDataType)
    {
      string dataType = sqlDataType.ToLower();

      return (
        dataType == "smallint" ||
        dataType == "bigint" ||
        dataType == "bit" ||
        dataType == "decimal" ||
        dataType == "int" ||
        dataType == "float" ||
        dataType == "double" ||
        dataType.Contains("number")
      );
    }

    public static bool IsNumeric(Type dataType)
    {
      return (
        dataType == typeof(double) ||
        dataType == typeof(float) ||
        dataType == typeof(int) ||
        dataType == typeof(long) ||
        dataType == typeof(decimal) ||
        dataType == typeof(short) ||
        dataType == typeof(uint) ||
        dataType == typeof(ushort) ||
        dataType == typeof(ulong) ||
        dataType == typeof(byte) ||
        dataType == typeof(sbyte)
      );
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

    public static DataType ResolveDataType(string dataType)
    {
      switch (dataType.ToLower())
      {
        case "date":
        case "datatime":
          return DataType.DateTime;

        case "int":
          return DataType.Int32;

        case "float":
          return DataType.Single;

        case "double":
          return DataType.Double;

        case "decimal":
          return DataType.Decimal;

        case "bigint":
        case "long":
        case "number":
          return DataType.Int64;

        case "char":
          return DataType.Char;

        case "boolean":
        case "bit":
          return DataType.Boolean;

        case "smallint":
          return DataType.Int16;

        default: return DataType.String;
      }
    }
  }
}
