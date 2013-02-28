using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory")]
  public enum NodeIconCls
  {
    [EnumMember]
    @folder,
    [EnumMember]
    @project,
    [EnumMember]
    @application,
    [EnumMember]
    @resource,
    [EnumMember]
    @scope,
    [EnumMember]
    @key,
    [EnumMember]
    @property,
    [EnumMember]
    @relation
  }
}