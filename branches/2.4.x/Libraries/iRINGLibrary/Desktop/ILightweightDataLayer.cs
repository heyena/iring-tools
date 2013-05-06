using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.library
{
  public interface ILightweightDataLayer
  {
    DataDictionary Dictionary(string objectType, bool refresh, DataFilter inFilter, out DataFilter outFilter);
    IList<IDataObject> Get(string objectType);
    Response Update(IList<IDataObject> dataObjects);
    IList<IContentObject> GetContents(IDictionary<string, string> idFormats);
  }
}
