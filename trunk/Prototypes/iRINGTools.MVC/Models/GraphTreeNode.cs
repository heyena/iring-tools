using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using org.iringtools.client.Models.Ext;

namespace org.iringtools.client.Models
{
  public class GraphTreeNode : TreeNode<GraphTreeNode>
  {
     public string GraphName {get; set; }

     public override string text
    {
      get
      {
        if (!string.IsNullOrEmpty(GraphName))
        {
          return "Graph " + " [" + GraphName + "]";
        }
        else
        {
          return GraphName;
        }
      }      
    }

     public GraphTreeNode(string graphMap)
    {
      this.GraphName = graphMap;
      this.id = graphMap + new Guid();
      this.icon = "Content/img/applications-internet.png";
      this.leaf = true;
    }
  }
}