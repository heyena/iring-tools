package org.iringtools.services.core;

import java.util.Comparator;

import org.iringtools.dxfr.dto.DataTransferObject;

public class DataTransferObjectComparator implements Comparator<DataTransferObject>
{
  @Override
  public int compare(DataTransferObject dto1, DataTransferObject dto2)
  {
    if (dto1.getClassObjects().getItems().size() > 0 && dto2.getClassObjects().getItems().size() > 0)
    {
      String identifier1 = dto1.getClassObjects().getItems().get(0).getIdentifier();
      String identifier2 = dto2.getClassObjects().getItems().get(0).getIdentifier();
      return identifier1.compareTo(identifier2);
    }

    return -1;
  }
}
