using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.client.Models.Ext;

namespace org.iringtools.client.Models
{
  public class ProjectTreeNode : TreeNode
  {
    public ProjectTreeNode()
    {
      children = new List<TreeNode>();
    }

    public string name { get; set; }
    public string description { get; set; }
    public override string id
    {
      get
      {
        return this.name;
      }
      set
      {
        this.name = value;
      }
    }
    public override string text
    {
      get
      {
        return name + ": " + description;
      }      
    }
  }
  
}