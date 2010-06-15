using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.library;

namespace org.iringtools.adapter
{
  public interface IProjectionLayer
  {
    XElement GetXml(ref GraphMap graphMap, ref DataDictionary dataDictionary, ref IList<IDataObject> dataObjects);
    IList<IDataObject> GetDataObjects(ref GraphMap graphMap, ref DataDictionary dataDictionary, ref XElement xml);
  }
}
