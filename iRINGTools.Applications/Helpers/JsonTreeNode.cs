using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
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

  public class JsonTreeNode
  {
    public JsonTreeNode()
    {
      hidden = false;
      iconCls = string.Empty;
    }

    public Dictionary<string, string> property { get; set; }
    public string id { get; set; }
    public string identifier { get; set; }
    public string text { get; set; }
    public string icon { get; set; }
    public bool leaf { get; set; }
    public bool expanded { get; set; }
    public bool hidden { get; set; }
    public int index { get; set; }
    public List<JsonTreeNode> children { get; set; }
    public string type { get; set; }
    public string nodeType { get; set; }
    public object @checked { get; set; }
    public object record { get; set; }
    public Dictionary<string, string> properties { get; set; }
    public string iconCls { get; set; }
    public string Namespace { get; set; }
      
  }  
}