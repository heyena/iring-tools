using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://iringtools.org/common", Name = "objects", ItemName = "object")]
  public class DataObjects : List<String>
  {
  }
}
