using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Tests
{
  public class TestDictionary: Dictionary
  {
    public TestDictionary()
    {
      DictionaryObjects = new LazyList<DictionaryObject>();

      var dataObject = new DictionaryObject();
      dataObject.ObjectName = "LINES";
      dataObject.TableName = "LINES";
      dataObject.DataProperties = new LazyList<DataProperty>();
      dataObject.KeyProperties = new LazyList<KeyProperty>();

      dataObject.AddKeyProperty(new DataProperty
      {
        ColumnName = "TAG",
        PropertyName = "TAG",
        DataType = DataType.String,
        DataLength = 100,
        IsNullable = false,
        KeyType = KeyType.Assigned
      });

      dataObject.DataProperties.Add(new DataProperty
      {
        ColumnName = "AREA",
        PropertyName = "AREA",
        DataType = DataType.String,
        DataLength = 10,
        IsNullable = true,
        KeyType = KeyType.Unassigned
      });

      dataObject.DataProperties.Add(new DataProperty
      {
        ColumnName = "UOM_NOMDIAMETER",
        PropertyName = "UOM_NOMDIAMETER",
        DataType = DataType.String,
        DataLength = 20,
        IsNullable = true,
        KeyType = KeyType.Unassigned
      });

      dataObject.DataProperties.Add(new DataProperty
      {
        ColumnName = "NOMDIAMETER",
        PropertyName = "NOMDIAMETER",
        DataType = DataType.Double,
        DataLength = 15,
        IsNullable = true,
        KeyType = KeyType.Unassigned
      });

      dataObject.DataProperties.Add(new DataProperty
      {
        ColumnName = "PIDNUMBER",
        PropertyName = "PIDNUMBER",
        DataType = DataType.String,
        DataLength = 50,
        IsNullable = true,
        KeyType = KeyType.Unassigned
      });

      DictionaryObjects.Add(dataObject);
    }
  }
}
