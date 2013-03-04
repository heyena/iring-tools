using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace org.iringtools.library
{
  public class GenericContentObject : GenericDataObject, IContentObject
  {
    [IgnoreDataMember]
    public Stream Content { get; set; }

    public string ContentType { get; set; }

    public string HashValue { get; set; }

    public string HashType { get; set; }

    public string URL { get; set; }

    public string Identifier { get; set; }
  }
}