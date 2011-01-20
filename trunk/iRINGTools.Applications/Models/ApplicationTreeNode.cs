using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
  public class ApplicationTreeNode : TreeNode<GraphTreeNode>
  {
    public ScopeApplication ScopeApplication { get; set; }
    
    public override string text
    {
      get
      {
        if (ScopeApplication.Description != null && !ScopeApplication.Description.Equals(String.Empty))
        {
          return ScopeApplication.Name + " [" + ScopeApplication.Description + "]";
        }
        else
        {
          return ScopeApplication.Name;
        }
      }      
    }

    public ApplicationTreeNode(ScopeApplication application)
    {
      this.ScopeApplication = application;
      this.icon = "Content/img/applications-internet.png";
      this.children = new List<GraphTreeNode>();
      this.expanded = true;
      this.leaf = false;
    }
  }
}