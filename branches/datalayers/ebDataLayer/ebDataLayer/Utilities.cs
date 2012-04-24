using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Text.RegularExpressions;
using org.iringtools.utility;

namespace org.iringtools.adaper.datalayer.eb
{
  public static class Utilities
  {
    public static string RELATED_ATTRIBUTE_TOKEN = "(Related)";
    public static string SYSTEM_ATTRIBUTE_TOKEN = "(System)";

    public static DataType ToCSharpType(string ebType)
    {
      switch (ebType)
      {
        case "CH":
        case "PD":
          ebType = "String";
          break;
        case "NU":
          ebType = "Decimal";
          break;
        case "DA":
          ebType = "DateTime";
          break;
      }

      return (DataType)Enum.Parse(typeof(DataType), ebType, true);
    }

    public static string ToPropertyName(string columnName)
    {
      return Regex.Replace(columnName, @" |\.", "");
    }

    public static string ToSqlWhereClause(DataFilter filter, DataObject objDef)
    {
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      dbDictionary.Provider = "SQL Server";

      DataObject newObjDef = Utility.CloneDataContractObject<DataObject>(objDef);
      
      foreach (DataProperty prop in newObjDef.dataProperties)
      {
        if (prop.columnName.EndsWith(SYSTEM_ATTRIBUTE_TOKEN))
        {
          prop.columnName = prop.columnName.Replace(SYSTEM_ATTRIBUTE_TOKEN, string.Empty);
        }
        else if (prop.columnName.EndsWith(RELATED_ATTRIBUTE_TOKEN))
        {
          prop.columnName = prop.columnName.Replace(RELATED_ATTRIBUTE_TOKEN, string.Empty);
        }
        else
        {
          prop.columnName = string.Format("Attributes[\"Global\", \"{0}\"].Value", prop.columnName);
        }
      }
      
      dbDictionary.dataObjects.Add(newObjDef);

      return filter.ToSqlWhereClause(dbDictionary, newObjDef.tableName, string.Empty);
    }

    public static void Append(ref Status status, Status newStatus)
    {
      if (status.Level < newStatus.Level)
      {
        status.Level = newStatus.Level;
      }

      status.Messages.AddRange(newStatus.Messages);
    }
  }
}
