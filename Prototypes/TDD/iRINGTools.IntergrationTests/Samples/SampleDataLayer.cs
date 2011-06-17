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

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
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

    public IDictionary GetDictionary()
    {
      throw new NotImplementedException();
    }

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }


    public IQueryable<IDataObject> DataObjects
    {
      get { throw new NotImplementedException(); }
    }
  }
}
