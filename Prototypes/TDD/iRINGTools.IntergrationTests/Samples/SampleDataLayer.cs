using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

using iRINGTools.Data;

namespace iRINGTools.IntergrationTests
{
  public class SampleDataLayer : IDataLayer
  {
    public string Name { get; private set; }

    public SampleDataLayer()
    {
      Name = "Sample DataLayer";
    }

    public IQueryable<IDataObject> DataObjects
    {
      get { throw new NotImplementedException(); }
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public long GetCount(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public Response Post(IList<IDataObject> dataObjects)
    {
      throw new NotImplementedException();
    }

    public Response Delete(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public Response Delete(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public Dictionary GetDictionary()
    {
      throw new NotImplementedException();
    }

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }
  }
}
