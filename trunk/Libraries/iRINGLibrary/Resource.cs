using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "resource")]
  public class Resource 
  {
    [DataMember(Name = "baseUrl", Order = 0)]
    public string baseUrl { get; set; }

    [DataMember(Name = "locators", Order = 1)]
    public Locators locators { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "locators")]
  public class Locators : List<Locator>
  {
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "applications")]
  public class EndpointApplications : List<EndpointApplication>
  {
  }
  
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "locator")]
  public class Locator
  {
    
    [DataMember(Name = "context", Order = 0)]
    public string context { get; set; }    
   
    [DataMember(Name = "applications", Order = 1)]
    public EndpointApplications applications { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "application")]
  public class EndpointApplication
  {
   
    [DataMember(Name = "endpoint", Order = 0)]
    public string endpoint { get; set; }    
  }
}

