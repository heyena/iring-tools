package org.iringtools.adapter.dti;

import java.util.Comparator;
import org.iringtools.adapter.dti.DataTransferIndex;

public class IdentifierComparator implements Comparator<DataTransferIndex>
{
  @Override
  public int compare(DataTransferIndex dti1, DataTransferIndex dti2)
  {
    return dti1.getIdentifier().compareTo(dti2.getIdentifier());
  }
}
