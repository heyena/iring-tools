package org.iringtools.widgets.tree;

import java.util.ArrayList;
import java.util.List;

public class TreeNode extends Node
{
  protected boolean expanded;
  protected List<Node> children;
  
  public TreeNode()
  {
    expanded = false;
    children = new ArrayList<Node>();
  }
  
  public boolean getExpanded()
  {
    return expanded;
  }
  
  public void setExpanded(boolean value)
  {
    expanded = value;
  }

  public List<Node> getChildren()
  {
    if (children == null)
    {
      children = new ArrayList<Node>();
    }
    return this.children;
  }

  public void setChildren(List<Node> children)
  {
    this.children = children;
  }
}
