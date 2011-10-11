using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "dataItems")]
  public class DataItems
  {
    [DataMember(Name = "type", Order = 0)]
    public string type { get; set; }

    [DataMember(Name = "total", Order = 1, EmitDefaultValue = false)]
    public long total { get; set; }

    [DataMember(Name = "items", Order = 2, EmitDefaultValue = false)]
    public List<DataItem> items { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "dataItem")]
  public class DataItem
  {
    [DataMember(Name = "id", Order = 0, EmitDefaultValue = false)]
    public string id { get; set; }

    [DataMember(Name = "properties", Order = 1, EmitDefaultValue = false)]
    public Dictionary<string, string> properties { get; set; }

    [DataMember(Name = "relatedItems", Order = 2, EmitDefaultValue = false)]
    public List<DataItems> relatedItems { get; set; }
  }
}
