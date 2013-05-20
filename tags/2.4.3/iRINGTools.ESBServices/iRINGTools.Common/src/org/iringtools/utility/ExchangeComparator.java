package org.iringtools.utility;

import java.util.Comparator;
import org.iringtools.directory.Exchange;

public class ExchangeComparator implements Comparator<Exchange>
{
  
  @Override
  public int compare(Exchange leftScope, Exchange rightScope)
  {  
	  String p1Name = leftScope.getName();  
		 String p2Name = rightScope.getName();
	    return p1Name.compareTo(p2Name);
  }
}
