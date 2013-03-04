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
    public Guid Id { get; set; }

    [DataMember(Name = "nodeType")]
    public string NodeType { get; set; }

    [DataMember(Name = "expanded")]
    public Boolean Expanded { get; set; }
        
    [DataMember(Name = "text")]
    public string Text { get; set; }

    [DataMember(Name = "leaf")]
    [DefaultValue(false)]
    public bool Leaf { get; set; }

    [DataMember(Name = "iconCls")]
    public string IconCls { get; set; }

    [DataMember(Name = "children")]
    public IEnumerable<TreeNodeViewModel> Children { get; set; }
  }
}