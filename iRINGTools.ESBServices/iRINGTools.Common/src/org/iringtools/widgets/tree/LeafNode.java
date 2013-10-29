package org.iringtools.widgets.tree;

public class LeafNode extends Node
{
  protected boolean leaf;
  
  public LeafNode()
  {
    leaf = true;
  }

  public boolean isLeaf()
  {
    return leaf;
  }

  public void setLeaf(boolean value)
  {
    this.leaf = value;
  }
}
