package org.iringtools.utility;

import java.util.Comparator;
import org.iringtools.directory.Commodity;

public class CommodityComparator implements Comparator<Commodity>
{
  @Override
  public int compare(Commodity left, Commodity right)
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
