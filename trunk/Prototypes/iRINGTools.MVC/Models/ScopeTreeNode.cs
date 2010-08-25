using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.client.Models.Ext;

namespace org.iringtools.client.Models
{
  public class ScopeTreeNode : TreeNode<ApplicationTreeNode>
  {
    public string Name { get; set; }    
    public string Description { get; set; }        
    public override string text
    {
      get
      {
        if (Description != null && !Description.Equals(String.Empty))
        {
          return Name + " [" + Description + "]";
        }
        else
        {
          return Name;
        }
      }      
    }

    public ScopeTreeNode()
    {
      children = new List<ApplicationTreeNode>();      
    }
  }
  
}