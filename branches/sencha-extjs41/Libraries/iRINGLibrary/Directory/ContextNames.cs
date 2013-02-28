using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Name = "contexts", Namespace = "http://www.iringtools.org/directory", ItemName = "context")]
  public class ContextNames : List<ContextName>
  {
  }
}