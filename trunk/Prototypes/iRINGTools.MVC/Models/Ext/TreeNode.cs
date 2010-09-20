using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.iringtools.client.Models.Ext
{
  /*
   [{
        id: 1,
        text: 'A leaf Node',
        leaf: true
    },{
        id: 2,
        text: 'A folder Node',
        children: [{
            id: 3,
            text: 'A child Node',
            leaf: true
        }]
   }] 
   
  */

  public class TreeNode<T>
  {
    public virtual string id { get; set; }
    public virtual string text { get; set; }
    public string icon { get; set; }
    public bool leaf { get; set; }
    public bool expanded { get; set; }
    public List<T> children { get; set; }
  }
}