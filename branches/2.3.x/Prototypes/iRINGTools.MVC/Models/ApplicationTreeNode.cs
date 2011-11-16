using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using org.iringtools.client.Models.Ext;

namespace org.iringtools.client.Models
{
  public class ApplicationTreeNode : TreeNode<GraphTreeNode>
  {
    public ScopeApplication Application { get; set; }

    public ScopeProject Scope { get; set; }
    public ScopeApplication _application { get; set; }
    public string GraphName { get; set; }
    public string Configure { get; set; }
    public string Mapping { get; set; }
    
    public override string text
    {
      get
      {
        if (_application.Description != null && !_application.Description.Equals(String.Empty))
        {
          return _application.Name + " [" + _application.Description + "]";
        }
        else
        {
          return _application.Name;
        }
      }      
    }

    public ApplicationTreeNode(ScopeApplication application, ScopeProject scope, string graphName)
    {
      Scope = scope;
      _application = application;
      GraphName = graphName;
      this.id = Scope.Name + application.Name;
      this.icon = "Content/img/applications-internet.png";
      this.children = new List<GraphTreeNode>();
      this.expanded = true;
      this.leaf = false;
    }
  }
}