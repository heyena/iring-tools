using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  [KnownType(typeof(Dictionary<string, object>))]
  public class StoreViewModel : ResponseViewModel
  {
    [DataMember(Name = "metaData")]
    public MetaDataViewModel MetaData { get; set; }
  }
}