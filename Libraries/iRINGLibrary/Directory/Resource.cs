using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "resource")]
  public class Resource
  {
    [DataMember(Name = "baseUrl", Order = 0)]
    public string BaseUrl { get; set; }

    [DataMember(Name = "locators", Order = 1, EmitDefaultValue = false)]
    public Locators Locators { get; set; }
  }
}