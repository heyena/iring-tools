using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iRINGTools.Web.Helpers
{
    public class Node
    {
        public string text { get; set; }
        public string type { get; set; }
        public string iconCls { get; set; }
        public bool leaf { get; set; }
        public bool expanded { get; set; }
        public List<Node> children { get; set; }
        public Dictionary<string, object> properties { get; set; }
    }
}