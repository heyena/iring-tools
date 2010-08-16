using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.library;
using org.iringtools.library.manifest;

namespace org.iringtools.library
{
  [CollectionDataContract]
  public class DXRequest : Request
  {
    [DataMember]
    public Manifest Manifest { get; set; }

    [DataMember]
    public Identifiers Identifiers { get; set; }
  }
}
