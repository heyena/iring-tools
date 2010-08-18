using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.library;
using VDS.RDF;

namespace org.iringtools.adapter
{
  public interface IProjectionLayer
  {
    XElement GetXml(string graphName, ref IList<IDataObject> dataObjects);
    IList<IDataObject> GetDataObjects(string graphName, ref XElement xml);
  }
}
