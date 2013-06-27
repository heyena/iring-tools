package org.iringtools.utility;

import java.util.Comparator;

import org.iringtools.directory.Graph;

public class GraphComparator implements Comparator<Graph>
{
  
  @Override
  public int compare(Graph leftScope, Graph rightScope)
  {  
	  String p1Name = leftScope.getName();  
		 String p2Name = rightScope.getName();
	    return p1Name.compareTo(p2Name);
  }
}
