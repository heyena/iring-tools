using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  /// <remarks/>
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "namespaces")]
  public class Namespaces : List<Namespace>
  {
    [DataMember(Name = "sequenceid")]
    public int SequenceId { get; set; }
  }
}