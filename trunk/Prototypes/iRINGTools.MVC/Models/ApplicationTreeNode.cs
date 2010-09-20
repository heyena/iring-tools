using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using org.iringtools.client.Models.Ext;

namespace org.iringtools.client.Models
{
  public class ApplicationTreeNode : TreeNode<ApplicationTreeNode>
  {
    public ScopeApplication Application { get; set; }

    public string ScopeHeader { get; set; }
    public string Configure { get; set; }
    public string Mapping { get; set; }
    
    public override string text
    {
      get
      {
        if (Application.Description != null && !Application.Description.Equals(String.Empty))
        {
          return Application.Name + " [" + Application.Description + "]";
        }
        else
        {
          return Application.Name;
        }
      }      
    }

    public ApplicationTreeNode(ScopeApplication application)
    {
      this.Application = application;
      this.id = application.Name;
      this.icon = "Content/img/applications-internet.png";
      this.leaf = true;
    }
  }
}