package org.iringtools.utility;

import java.util.Comparator;
import org.iringtools.refdata.response.Entity;

public final class EntityComparator implements Comparator<Entity>
{
  @Override
  public int compare(Entity entity1, Entity entity2)
  {
    return entity1.getLabel().compareToIgnoreCase(entity2.getLabel());          
  }
}
