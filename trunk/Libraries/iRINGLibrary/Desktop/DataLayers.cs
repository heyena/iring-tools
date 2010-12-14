using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Name = "dataLayers", Namespace = "http://www.iringtools.org/library", ItemName = "dataLayer")]
  public class DataLayers : List<string>
  {
  }
}
