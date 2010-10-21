﻿using System;
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

    public ScopeTreeNode(ScopeProject scope)
    {
      this.Scope = scope;
      this.icon = "Content/img/system-file-manager.png";
      this.id = scope.Name;
      this.children = new List<ApplicationTreeNode>();
      this.expanded = true;
      this.leaf = false;
    }
  }
  
}