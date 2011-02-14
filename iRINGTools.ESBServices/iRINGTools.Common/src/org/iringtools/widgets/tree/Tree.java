package org.iringtools.widgets.tree;

import java.util.ArrayList;
import java.util.List;

public class Tree
{
  protected List<Node> nodes;

  public List<Node> getNodes()
  {
    if (nodes == null)
    {
      nodes = new ArrayList<Node>();
    }
    return this.nodes;
  }

  public void setNodes(List<Node> nodes)
  {
    this.nodes = nodes;
  }
}
