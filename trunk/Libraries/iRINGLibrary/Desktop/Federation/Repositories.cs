using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "repositories")]
  public class Repositories : List<Repository>
  {
    [DataMember(Name = "sequenceid")]
    public int SequenceId { get; set; }

  }
}