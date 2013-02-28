using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text;

namespace org.iringtools.library
{
  [CollectionDataContract(Name = "baseUrls", Namespace = "http://www.iringtools.org/directory", ItemName = "baseUrl")]
  public class Urls : List<Url>
  {
  }
}
