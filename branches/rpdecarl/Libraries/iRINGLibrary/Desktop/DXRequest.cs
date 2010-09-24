using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.library;
using org.iringtools.protocol.manifest;

//TODO: fix namespace
namespace org.iringtools.library
{
  [DataContract(Namespace = "http://iringtools.org/exchange", Name = "dxRequest")]
  public class DxRequest
  {
    [DataMember(Name = "manifest", Order = 0)]
    public Manifest Manifest { get; set; }

    [DataMember(Name = "identifiers", Order = 1)]
    public Identifiers Identifiers { get; set; }

    [DataMember(Name = "hashAlgorithm", Order = 2)]
    public String HashAlgorithm { get; set; }
  }

  [CollectionDataContract(Namespace = "http://iringtools.org/exchange", Name = "identifiers", ItemName = "identifier")]
  public class Identifiers : List<string> { }
}
