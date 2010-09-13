﻿using System;
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
    XElement ToXml(string graphName, ref IList<IDataObject> dataObjects);
    IList<IDataObject> ToDataObjects(string graphName, ref XElement xml);
  }
}
