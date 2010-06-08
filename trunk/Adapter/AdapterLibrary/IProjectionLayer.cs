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
    XElement GetRdf(string graphName);
    XElement GetQtxf(string graphName);
    XElement GetHierachicalDTOList(string graphName);
    List<Dictionary<string, string>> GetDTOList(string graphName);
  }
}
