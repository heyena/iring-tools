using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "locator")]
  public class Locator
  {
    [DataMember(Name = "context", Order = 0)]
    public string Context { get; set; }

    [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "applications", Order = 2, EmitDefaultValue = false)]
    public EndpointApplications Applications { get; set; }
  }
}