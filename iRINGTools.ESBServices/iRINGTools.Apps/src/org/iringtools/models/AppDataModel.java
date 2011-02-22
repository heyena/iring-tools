package org.iringtools.models;

import java.util.Map;

import org.iringtools.widgets.grid.Grid;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;

public class AppDataModel extends DataModel
{  
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String refServiceUri, String scope, String app, String graph, String filter, 
      String sortBy, String sortOrder, int start, int limit)
  {
    String dtiRelativePath = "/" + scope + "/" + app + "/" + graph + "/filter";
    String dtoRelativePath = "/" + scope + "/" + app + "/" + graph + "/page";    
    DataTransferObjects pageDtos = getPageDtos(serviceUri, dtiRelativePath, dtoRelativePath, 
        filter, sortBy, sortOrder, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.APP, pageDtos, refServiceUri);
    DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());      
    return pageDtoGrid;
  }
  
  public Grid getRelatedItemGrid(String serviceUri, String scope, String app, String graph, 
      String dtoIdentifier, String classId, String classIdentifier, String filter, String sortBy,
      String sortOrder, int start, int limit)
  {
    String dtiRelativePath = "/" + scope + "/" + app + "/" + graph + "/filter";
    String dtoRelativePath = "/" + scope + "/" + app + "/" + graph + "/page";
    DataTransferObject dto = getDto(serviceUri, dtiRelativePath, dtoRelativePath, dtoIdentifier, filter,
        sortBy, sortOrder, start, limit);  
    return getRelatedItemGrid(DataType.APP, dto, classId, classIdentifier, start, limit);
  }
}
