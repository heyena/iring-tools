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
    public ScopeProject Scope { get; set; }
    public ScopeApplication _application { get; set; }
    public string GraphName {get; set; }

    public override string text
    {
      get
      {
        if (!string.IsNullOrEmpty(GraphName))
        {
          return GraphName;
        }
        else
        {
          return GraphName;
        }
      }      
    }

    public GraphTreeNode(string graphMap, ScopeProject scope, ScopeApplication application)
    {
      Scope = scope;
      _application = application;
      this.GraphName = graphMap;
      this.id = Scope.Name + _application.Name + GraphName;
      this.icon = "Content/img/applications-internet.png";
      this.leaf = true;
    }
  }
}