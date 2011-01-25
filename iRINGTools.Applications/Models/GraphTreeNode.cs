using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
  public class GraphTreeNode : TreeNode<String>
  {
    private String GraphMap = null;

    public override string type
    {
      get
      {
        return "Graph";
      }
    }

    public override string text
    {
      get
      {
        if (!string.IsNullOrEmpty(GraphMap))
        {
          return GraphMap;
        }
        else
        {
          return GraphMap;
        }
      }      
    }

    public GraphTreeNode(string graphMap)
    { 
      this.GraphMap = graphMap;      
      this.icon = "Content/img/applications-internet.png";
      this.children = null;      
      this.leaf = true;
    }
  }
}