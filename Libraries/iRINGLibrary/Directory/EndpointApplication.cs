using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "application")]
  public class EndpointApplication
  {
    [DataMember(Name = "endpoint", Order = 0)]
    public string Endpoint { get; set; }

    [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "assembly", Order = 2, EmitDefaultValue = false)]
    public string Assembly { get; set; }

    [DataMember(Name = "lpath", Order = 3, EmitDefaultValue = false)]
    public string Path { get; set; }  
  }
}