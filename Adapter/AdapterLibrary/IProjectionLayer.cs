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
    XElement GetXml(ref Mapping mapping, string graphName,
      ref DataDictionary dataDictionary, ref Dictionary<string, IList<IDataObject>> dataObjects);

    Dictionary<string, IList<IDataObject>> GetDataObjects(ref Mapping mapping, string graphName,
      ref DataDictionary dataDictionary, ref XElement xml);
  }
}
