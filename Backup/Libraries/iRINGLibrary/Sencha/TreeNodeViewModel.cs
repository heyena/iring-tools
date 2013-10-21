using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Name = "nodeInterface")]
  public class TreeNodeViewModel
  {
    [DataMember(Name = "id")]
    public Guid id { get; set; }

    [DataMember(Name = "nodeType")]
    public string nodeType { get; set; }

    [DataMember(Name = "expanded")]
    public Boolean expanded { get; set; }
        
    [DataMember(Name = "text")]
    public string text { get; set; }

    [DataMember(Name = "leaf")]
    [DefaultValue(false)]
    public bool leaf { get; set; }

    [DataMember(Name = "iconCls")]
    public string iconCls { get; set; }

    [DataMember(Name = "children")]
    public IEnumerable<TreeNodeViewModel> children { get; set; }
  }
}