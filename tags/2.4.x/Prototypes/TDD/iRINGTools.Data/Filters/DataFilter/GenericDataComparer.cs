using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public class GenericDataComparer : IEqualityComparer<string>, IComparer<string>
  {
    private DataType _dataType { get; set; }

    public GenericDataComparer(DataType dataType)
    {
      _dataType = dataType;
    }

    // Implement the IComparable interface. 
    public bool Equals(string str1, string str2)
    {
      return Compare(str1, str2) == 0;
    }

    public int Compare(string str1, string str2)
    {
      switch (_dataType)
      {
        case DataType.Boolean:
          bool bool1 = false;
          Boolean.TryParse(str1, out bool1);

          bool bool2 = false;
          Boolean.TryParse(str2, out bool1);

          if (Boolean.Equals(bool1, bool2))
          {
            return 0;
          }
          else if (bool1)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Byte:
          byte byte1 = 0;
          Byte.TryParse(str1, out byte1);

          byte byte2 = 0;
          Byte.TryParse(str2, out byte2);

          if (byte1 == byte2)
          {
            return 0;
          }
          else if (byte1 > byte2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Char:
          char char1 = Char.MinValue;
          Char.TryParse(str1, out char1);

          char char2 = Char.MinValue;
          Char.TryParse(str2, out char2);

          if (char1 == char2)
          {
            return 0;
          }
          else if (char1 > char2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.DateTime:
          DateTime dateTime1 = DateTime.MinValue;
          DateTime.TryParse(str1, out dateTime1);

          DateTime dateTime2 = DateTime.MinValue;
          DateTime.TryParse(str2, out dateTime2);

          return DateTime.Compare(dateTime1, dateTime2);

        case DataType.Decimal:
          decimal decimal1 = 0;
          Decimal.TryParse(str1, out decimal1);

          decimal decimal2 = 0;
          Decimal.TryParse(str2, out decimal2);

          return Decimal.Compare(decimal1, decimal2);

        case DataType.Double:
          double double1 = 0;
          Double.TryParse(str1, out double1);

          double double2 = 0;
          Double.TryParse(str2, out double2);

          if (Double.Equals(double1, double2))
          {
            return 0;
          }
          else if (double1 > double2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int16:
          Int16 int161 = 0;
          Int16.TryParse(str1, out int161);

          Int16 int162 = 0;
          Int16.TryParse(str2, out int162);

          if (Int16.Equals(int161, int162))
          {
            return 0;
          }
          else if (int161 > int162)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int32:
          int int1 = 0;
          Int32.TryParse(str1, out int1);

          int int2 = 0;
          Int32.TryParse(str2, out int2);

          if (Int32.Equals(int1, int2))
          {
            return 0;
          }
          else if (int1 > int2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int64:
          Int64 int641 = 0;
          Int64.TryParse(str1, out int641);

          Int64 int642 = 0;
          Int64.TryParse(str2, out int642);

          if (Int16.Equals(int641, int642))
          {
            return 0;
          }
          else if (int641 > int642)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Single:
          Single single1 = 0;
          Single.TryParse(str1, out single1);

          Single single2 = 0;
          Single.TryParse(str2, out single2);

          if (Single.Equals(single1, single2))
          {
            return 0;
          }
          else if (single1 > single2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        //Case Insensitive!
        case DataType.String:

          return String.Compare(str1, str2, true);

        case DataType.TimeSpan:
          TimeSpan span1 = TimeSpan.MinValue;
          TimeSpan.TryParse(str1, out span1);

          TimeSpan span2 = TimeSpan.MinValue;
          TimeSpan.TryParse(str2, out span2);

          return TimeSpan.Compare(span1, span2);

        default:
          throw new Exception("Invalid property datatype.");
      }
    }

    public int GetHashCode(string obj)
    {
      throw new NotImplementedException();
    }
  }
}
