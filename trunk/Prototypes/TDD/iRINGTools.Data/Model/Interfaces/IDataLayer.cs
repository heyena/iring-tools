using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace iRINGTools.Data
{
  public interface IDataObject
  {
    string ObjectType { get; }
    object GetPropertyValue(string propertyName);
    void SetPropertyValue(string propertyName, object value);
  }

  public interface IDataLayer
  {
    string Name { get; }

    IQueryable<IDataObject> DataObjects { get; }

    IList<IDataObject> Create(string objectType, IList<string> identifiers);

    IList<IDataObject> Get(string objectType, IList<string> identifiers);

    IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex);

    long GetCount(string objectType, DataFilter filter);

    IList<string> GetIdentifiers(string objectType, DataFilter filter);
    
    Response Post(IList<IDataObject> dataObjects);

    Response Delete(string objectType, IList<string> identifiers);

    Response Delete(string objectType, DataFilter filter);
    
    Dictionary GetDictionary();

    IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType);
  }

  public interface IConfigurableDataLayer : IDataLayer
  {
    Response Configure(XElement configuration);
    XElement GetConfiguration();
  }

}
