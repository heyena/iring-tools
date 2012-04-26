using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using org.iringtools.client.Models.Ext;

namespace org.iringtools.client.Models
{
  public class ScopeTreeNode : TreeNode<ApplicationTreeNode>
  {
    public ScopeProject Scope { get; set; }
    public ScopeApplication _application { get; set; }
    public string GraphName { get; set; }
    public override string text
    {
      get
      {
        if (Scope.Description != null && !Scope.Description.Equals(String.Empty))
        {
          return Scope.Name + " [" + Scope.Description + "]";
        }
        else
        {
          return Scope.Name;
        }
      }      
    }

    public ScopeTreeNode(ScopeProject scope, ScopeApplication application, string graphName)
    {
      Scope = scope;
      _application = application;
      GraphName = graphName;
      this.icon = "Content/img/system-file-manager.png";
      this.id = scope.Name;
      this.children = new List<ApplicationTreeNode>();
      this.expanded = true;
      this.leaf = false;
    }
  }
  
}