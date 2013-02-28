using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Name = "resources", Namespace = "http://www.iringtools.org/directory", ItemName = "resource")]
  public class Resources : List<Resource>
  {
  }
}