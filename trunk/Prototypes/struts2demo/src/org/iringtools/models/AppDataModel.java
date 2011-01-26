package org.iringtools.models;

import java.util.Map;

import org.iringtools.widgets.grid.Grid;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;

public class AppDataModel extends DataModel
{  
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String scope, String app, String graph, int start, int limit)
  {
    String dtiUrl = serviceUri + "/" + scope + "/" + app + "/" + graph;
    String dtoUrl = dtiUrl + "/page";    
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.APP, pageDtos);
    DataTransferIndices dtis = getDtis(dtiUrl);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());      
    return pageDtoGrid;
  }
  
  public Grid getRelatedItemGrid(String serviceUri, String scope, String app, String graph, 
      String individual, String classId, String classIdentifier, int start, int limit)
  {
    String dtiUrl = serviceUri + "/" + scope + "/" + app + "/" + graph;
    String dtoUrl = dtiUrl + "/page";
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);  
    return getRelatedItemGrid(DataType.APP, pageDtos, individual, classId, classIdentifier, start, limit);
  }
}
