using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace org.iringtools.library
{
  public class GenericContentObject : GenericDataObject, IContentObject
  {
    [IgnoreDataMember]
    public Stream content { get; set; }

    public string contentType { get; set; }

    public string hash { get; set; }

    public string hashType { get; set; }

    public string url { get; set; }

    public string identifier { get; set; }
  }
}