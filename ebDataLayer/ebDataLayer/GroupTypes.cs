using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace org.iringtools.adaper.datalayer.eb
{
  [XmlType("grouptypes")]
  public class GroupTypes : List<GroupType> {}

  [XmlType("grouptype")]
  public class GroupType
  {
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("value")]
    public int Value { get; set; }
  }
}
