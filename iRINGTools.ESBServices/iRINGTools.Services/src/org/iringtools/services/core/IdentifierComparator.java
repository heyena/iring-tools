package org.iringtools.services.core;

import java.util.Comparator;
import org.iringtools.dxfr.dti.DataTransferIndex;

public class IdentifierComparator implements Comparator<DataTransferIndex>
{
  @Override
  public int compare(DataTransferIndex dti1, DataTransferIndex dti2)
  {
    return dti1.getIdentifier().toLowerCase().compareTo(dti2.getIdentifier().toLowerCase());
  }
}
