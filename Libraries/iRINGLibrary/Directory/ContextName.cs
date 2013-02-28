using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "context")]
  public class ContextName
  {
    [DataMember(Name = "context", Order = 1, EmitDefaultValue = false)]
    public string Context { get; set; }
  }
}