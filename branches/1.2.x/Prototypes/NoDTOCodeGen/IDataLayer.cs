using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdapterPrototype
{
  public interface IDataLayer
  {
    object Create(string dataObjectName);
    T GetPropertyValue<T>(object dataObject, string propertyName);
    void SetPropertyValue<T>(object dataObject, string propertyName, T value);
    object Get(string dataObjectName, string filter);
    IList<object> GetList(string dataObjectName, string filter);
    void Post(object dataObject);
    void PostList(List<object> dataObjects);
  }
}
