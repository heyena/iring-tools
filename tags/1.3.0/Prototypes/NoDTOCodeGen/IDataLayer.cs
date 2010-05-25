using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdapterPrototype
{
  public interface IDataLayer
  {
    object Create(string typeName);
    object GetPropertyValue(object dataObject, string propertyName);
    void SetPropertyValue(object dataObject, string propertyName, object value);
    object Get(string dataObjectName, string filter);
    IList<object> GetList(string dataObjectName, string filter);
    void Post(object dataObject);
    void PostList(List<object> dataObjects);
  }
}
