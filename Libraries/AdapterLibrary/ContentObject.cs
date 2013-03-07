using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace org.iringtools.adapter
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/dxfr/content", Name = "contentObjects")]
  public class ContentObjects : List<ContentObject> { }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/content", Name = "contentObject")]
  public class ContentObject
  {
    public ContentObject()
    {
      Attributes = new Attributes();
    }

    [DataMember(Name = "identifier", Order = 0)]
    public string Identifier { get; set; }

    [DataMember(Name = "mimeType", Order = 1, EmitDefaultValue = false)]
    public string MimeType { get; set; }

    [DataMember(Name = "content", Order = 2, EmitDefaultValue = false)]
    public byte[] Content { get; set; }

    [DataMember(Name = "attributes", Order = 3, EmitDefaultValue = false)]
    public Attributes Attributes { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/dxfr/content", Name = "attributes")]
  public class Attributes : List<Attribute> { }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/content", Name = "attribute")]
  public class Attribute
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "value", Order = 1)]
    public string Value { get; set; }
  }
}