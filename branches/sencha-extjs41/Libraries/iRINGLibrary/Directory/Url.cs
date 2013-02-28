using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "baseUrl")]
  public class Url
  {    
    [DataMember(Name = "url", Order = 1, EmitDefaultValue = false)]
    public string Urlocator { get; set; }
  }
}