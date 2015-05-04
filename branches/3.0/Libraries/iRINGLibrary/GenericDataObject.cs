using System.Collections.Generic;
using System;

namespace org.iringtools.library
{
  public class GenericDataObject : IDataObject
  {
    protected IDictionary<string, object> _dictionary = null;

    public GenericDataObject()
    {
      _dictionary = new Dictionary<string, object>();
    }

    public GenericDataObject(IDictionary<string, object> dict)
    {
      _dictionary = dict;
    }

    public IDictionary<string, object> Dictionary
    {
      get
      {
        return _dictionary;
      }
    }

    public object GetPropertyValue(string propertyName)
    {
      if (_dictionary.ContainsKey(propertyName) && _dictionary[propertyName] != null)
        return _dictionary[propertyName];

      return null;
    }

    public void SetPropertyNumericValue(string propertyName, object value)
    {
        decimal decDecimalValue = 0;
        Decimal.TryParse(value.ToString(), out decDecimalValue);
        _dictionary[propertyName] = decDecimalValue.ToString("0.################################");
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      _dictionary[propertyName] = value;
    }

    public string ObjectNamespace { get; set; }

    public string ObjectType { get; set; }

    public bool HasContent { get; set; }
  }
}