package org.iringtools.utility;

import java.util.Comparator;

import org.iringtools.directory.Application;

public class AppDataComparator implements Comparator<Application>
{
  
  @Override
  public int compare(Application leftScope, Application rightScope)
  {  
	  String p1Name = leftScope.getName();  
		 String p2Name = rightScope.getName();
	    return p1Name.compareTo(p2Name);
  }
}
