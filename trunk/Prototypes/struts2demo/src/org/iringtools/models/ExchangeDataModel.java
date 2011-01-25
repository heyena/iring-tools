package org.iringtools.models;

import java.util.Map;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;

public class ExchangeDataModel extends DataModel
{  
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }
  
  public Grid getDtoGrid(String dtiUrl, String dtoUrl, int start, int limit)
  {
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.EXCHANGE, pageDtos);
    DataTransferIndices dtis = (DataTransferIndices)session.get(dtiUrl);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());    
    return pageDtoGrid;
  }
  
  public Grid getRelatedItemGrid(String dtiUrl, String dtoUrl, String individual, 
      String classId, String classIdentifier, int start, int limit)
  {
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);  
    return getRelatedItemGrid(DataType.EXCHANGE, pageDtos, individual, classId, classIdentifier, start, limit);
  }
}
