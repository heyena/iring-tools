using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "idgenerators")]
  public class IdGenerators : List<IdGenerator>
  {
    [DataMember(Name = "sequenceid")]
    public int SequenceId { get; set; }
  }
}