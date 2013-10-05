package org.iringtools.utility;

import java.util.Comparator;

import org.iringtools.directory.Graph;

public class GraphComparator implements Comparator<Graph>
{  
  @Override
  public int compare(Graph left, Graph right)
  {  
    String leftParam = left.getName();    
    if (leftParam != null) 
      leftParam = leftParam.toLowerCase();
    
    String rightParam = right.getName();
    if (rightParam != null)
      rightParam = rightParam.toLowerCase();
    
    return leftParam.compareTo(rightParam);
  }
}
