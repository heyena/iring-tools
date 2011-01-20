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
    long Count { get; set; }

    XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects);
    IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument);
  }
}
