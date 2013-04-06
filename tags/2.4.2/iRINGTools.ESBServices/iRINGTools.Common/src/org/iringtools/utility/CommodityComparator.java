package org.iringtools.utility;

import java.util.Comparator;
import org.iringtools.directory.Commodity;

public class CommodityComparator implements Comparator<Commodity>
{
  
  @Override
  public int compare(Commodity leftScope, Commodity rightScope)
  {  
	  String p1Name = leftScope.getName();  
		 String p2Name = rightScope.getName();
	    return p1Name.compareTo(p2Name);
  }
}
