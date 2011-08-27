using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Data;

namespace org.iringtools.adapter.datalayer 
{
  public class MSSQLDatarow : IDataObject
  {
    private DataRow _dataRow = null;

    public MSSQLDatarow(DataRow dataRow)
    {
      _dataRow = dataRow;
    }
    public object GetPropertyValue(string propertyName)
    {
      return _dataRow[propertyName];
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      _dataRow[propertyName] = value;
    }
  }
}
