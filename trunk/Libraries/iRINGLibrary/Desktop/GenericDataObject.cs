using System;
using System.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.library
{
  public class GenericDataObject : IDataObject
  {
    private Dictionary<string, object> _dictionary = null;

    public GenericDataObject()
    {
      _dictionary = new Dictionary<string, object>();
    }

    public GenericDataObject(Dictionary<string, object> dict)
    {
      _dictionary = dict;
    }

    public object GetPropertyValue(string propertyName)
    {
      return _dictionary[propertyName];
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      _dictionary[propertyName] = value;
    }

    public string ObjectType { get; set; }
  }
}