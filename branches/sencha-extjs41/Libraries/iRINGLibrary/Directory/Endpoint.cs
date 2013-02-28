using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "endpoint")]
  public class Endpoint
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }   

    [DataMember(Name = "type", Order = 1, EmitDefaultValue = false)]
    public string Type { get; set; }

    [DataMember(Name = "description", Order = 2, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "context", Order = 3, EmitDefaultValue = false)]
    public string Context { get; set; }

    [DataMember(Name = "baseUrl", Order = 4, EmitDefaultValue = false)]
    public string BaseUrl { get; set; }

    [DataMember(Name = "securityRole", Order = 5, EmitDefaultValue = false)]
    public string SecurityRole { get; set; }

    [DataMember(Name = "user", Order = 6, EmitDefaultValue = false)]
    public string User { get; set; }
  }
}