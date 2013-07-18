using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  /// <remarks/>
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repositoryType")]
  public enum RepositoryType
  {
    [EnumMember]
    Part8,
    [EnumMember]
    RDSWIP,
    [EnumMember]
    Camelot,
    [EnumMember]
    JORD,
  }
}