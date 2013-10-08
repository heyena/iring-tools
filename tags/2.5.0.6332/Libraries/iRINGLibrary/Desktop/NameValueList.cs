using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "nameValueList")]
  public class NameValueList : List<ListItem> {}

  [DataContract(Namespace = "http://www.iringtools.org/library", Name="listItem")]
  public class ListItem
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "value", Order = 1)]
    public string Value { get; set; }
  }
}
