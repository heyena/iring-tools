package org.iringtools.models;

import java.util.Map;

import org.iringtools.widgets.grid.Grid;
import org.iringtools.dxfr.dto.DataTransferObjects;

public class AppDataModel extends DataModel
{  
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }
  
  public Grid getDtoGrid(String dtiUrl, String dtoUrl, int start, int limit)
  {
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.APP, pageDtos);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());      
    return pageDtoGrid;
  }
  
  public Grid getRelatedItemGrid(String dtiUrl, String dtoUrl, String individual, 
      String classId, String classIdentifier, int start, int limit)
  {
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);  
    return getRelatedItemGrid(DataType.APP, pageDtos, individual, classId, classIdentifier, start, limit);
  }
}
