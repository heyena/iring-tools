using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "applications")]
  public class EndpointApplications : List<EndpointApplication>
  {
  }
}