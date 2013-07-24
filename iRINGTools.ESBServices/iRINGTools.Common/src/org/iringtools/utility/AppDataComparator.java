package org.iringtools.utility;

import java.util.Comparator;

import org.iringtools.directory.Application;

public class AppDataComparator implements Comparator<Application>
{  
  @Override
  public int compare(Application left, Application right)
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
