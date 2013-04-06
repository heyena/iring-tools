package org.iringtools.utility;

import java.util.Comparator;
import org.iringtools.directory.Scope;

public class ScopeComparator implements Comparator<Scope>
{
  
  @Override
  public int compare(Scope leftScope, Scope rightScope)
  {  
   
	 String p1Name = leftScope.getName();  
	 String p2Name = rightScope.getName();
    return p1Name.compareTo(p2Name);
  }
}
