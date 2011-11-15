using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public static class DataTypeExtentions
  {
    public static bool IsNumeric(this DataType dataType)
    {
      bool isNumeric = false;

      var numericTypes = new DataType[] {
          DataType.Byte,
          DataType.Decimal,
          DataType.Double,
          DataType.Int16,
          DataType.Int32,
          DataType.Int64,
          DataType.Single,
      };

      if (numericTypes.Contains(dataType))
      {
        isNumeric = true;
      }

      return isNumeric;
    }
  }
}
