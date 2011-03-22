using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace org.iringtools.library
{
  public abstract class BaseDataLayer : IDataLayer
  {
    public virtual IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public virtual long GetCount(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public virtual IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public virtual IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public virtual IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public virtual Response Post(IList<IDataObject> dataObjects)
    {
      throw new NotImplementedException();
    }

    public virtual Response Delete(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public virtual Response Delete(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public virtual DataDictionary GetDictionary()
    {
      throw new NotImplementedException();
    }

    public virtual IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      return new List<IDataObject>();
    }

    public virtual Response Configure(XElement configuration)
    {
      throw new NotImplementedException();
    }
  }
}
