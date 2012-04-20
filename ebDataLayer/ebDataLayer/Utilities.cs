using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Text.RegularExpressions;

namespace org.iringtools.adaper.datalayer.eb
{
  public static class Utilities
  {
    public static string RELATED_COLUMN_SUFFIX = "(Related)";

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
  }
}
