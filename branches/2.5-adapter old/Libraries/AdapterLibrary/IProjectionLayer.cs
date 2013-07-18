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
    long Count { get; set; }
    int Start { get; set; }
    int Limit { get; set; }
    bool FullIndex { get; set; }
    string BaseURI { get; set; }
    XDocument ToXml(string graphName, ref List<IDataObject> dataObjects);
    XDocument ToXml(string graphName, ref List<IDataObject> dataObjects, string className, string classIdentifier);
    List<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument);
  }
}
