using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace org.iringtools.library
{
  [XmlRoot]  
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "dataLayer")]
  public class DataLayer
  {
    [XmlElement]
    [DataMember(Name = "assembly", Order = 1, EmitDefaultValue = false)]
    public string Assembly { get; set; }

    [XmlElement]
    [DataMember(Name = "name", Order = 2, EmitDefaultValue = false)]
    public string Name { get; set; }

    [XmlElement]
    [DataMember(Name = "configurable", Order = 3, EmitDefaultValue = false)]
    public bool Configurable { get; set; }
  }

  [CollectionDataContract(Name = "dataLayers", Namespace = "http://www.iringtools.org/library", ItemName = "dataLayer")]
  public class DataLayers : List<DataLayer>
  {
  }

  [XmlRoot]
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "baseUrl")]
  public class BaseUrl
  {
    [XmlElement]
    [DataMember(Name = "url", Order = 1, EmitDefaultValue = false)]
    public string Url { get; set; }
  }

  [CollectionDataContract(Name = "baseUrls", Namespace = "http://www.iringtools.org/library", ItemName = "baseUrl")]
  public class BaseUrls : List<BaseUrl>
  {
  }
}
