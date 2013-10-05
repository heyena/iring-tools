package org.iringtools.utility;

import java.util.Comparator;
import org.iringtools.directory.Scope;

public class ScopeComparator implements Comparator<Scope>
{  
  @Override
  public int compare(Scope left, Scope right)
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
