package org.iringtools.adapter.library.dto;

import java.util.Comparator;
import org.iringtools.adapter.library.dto.DataTransferIndices.DataTransferIndex;

public class IdentifierComparator implements Comparator<DataTransferIndex>
{
  @Override
  public int compare(DataTransferIndex dxi1, DataTransferIndex dxi2)
  {
    return dxi1.getIdentifier().compareTo(dxi2.getIdentifier());
  }
}
