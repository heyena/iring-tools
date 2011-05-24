package org.iringtools.models;

import java.util.Map;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.widgets.grid.Grid;

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
    DataTransferObjects pageDtos = getPageDtos(DataType.APP, serviceUri, dtiRelativePath, dtoRelativePath, 
        filter, sortBy, sortOrder, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.APP, pageDtos, refServiceUri);
    DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());      
    return pageDtoGrid;
  }
  
  public Grid getRelatedItemGrid(String serviceUri, String scope, String app, String graph, String dtoIdentifier, 
      String classId, String classIdentifier, String filter, String sortBy, String sortOrder, int start, int limit)
  {
    String dtiRelativePath = "/" + scope + "/" + app + "/" + graph + "/filter";
    String dtoRelativePath = "/" + scope + "/" + app + "/" + graph + "/page";
    DataTransferObjects dtos = getRelatedDtos(serviceUri, dtiRelativePath, dtoRelativePath, dtoIdentifier, filter,
        sortBy, sortOrder, start, limit);
    return getRelatedItemGrid(DataType.APP, dtos, classId, classIdentifier);
  }
}
