using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace iRINGTools.Data
{
  public interface IProjectionEngine
  {
    int Id { get; set; }
    string Format { get; set; }
    long Count { get; set; }
    bool FullIndex { get; set; }

    XDocument ToXml(Application application, string graphName, ref IList<IDataObject> dataObjects);
    XDocument ToXml(Application application, string graphName, string className, string classIdentifier, ref IDataObject dataObject);
    IList<IDataObject> ToDataObjects(Application application, string graphName, ref XDocument xDocument);
  }
}
