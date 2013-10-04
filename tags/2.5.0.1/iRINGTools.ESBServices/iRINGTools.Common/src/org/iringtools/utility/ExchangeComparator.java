package org.iringtools.utility;

import java.util.Comparator;
import org.iringtools.directory.Exchange;

public class ExchangeComparator implements Comparator<Exchange>
{  
  @Override
  public int compare(Exchange left, Exchange right)
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
