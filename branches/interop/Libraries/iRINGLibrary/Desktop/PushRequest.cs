using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "pushRequest")]
  public class PushRequest : Request
  {
    [DataMember(Name = "expectedResults")]
    public ExpectedResults ExpectedResults { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "expectedResults")]
  public class ExpectedResults : Dictionary<string, string>
  {
    [DataMember(Name = "dataObjectName")]
    public string DataObjectName { get; set; }
  }
}