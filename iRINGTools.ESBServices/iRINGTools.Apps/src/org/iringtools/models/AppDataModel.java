package org.iringtools.models;

import java.util.Map;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.widgets.grid.Grid;

public class AppDataModel extends DataModel
{  
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String refServiceUri, String scopeName, String appName, 
      String graphName, String filter, String sortBy, String sortOrder, int start, int limit)
  {
    String appRelativePath = "/" + scopeName + "/" + appName;
    String dtiRelativePath = appRelativePath + "/" + graphName + "/dxi/filter";
    String dtoRelativePath = appRelativePath + "/" + graphName + "/dxo";  
    String manifestRelativePath = appRelativePath + "/manifest";
    
    Grid pageDtoGrid = null;
    Manifest manifest = getManifest(serviceUri, manifestRelativePath);
    
    if (manifest != null && manifest.getGraphs() != null)
    {
      Graph graph = getGraph(manifest, graphName);
      
      if (graph != null)
      {
        DataTransferObjects pageDtos = getPageDtos(Mode.APP, serviceUri, manifestRelativePath, dtiRelativePath, dtoRelativePath, 
            filter, sortBy, sortOrder, start, limit);
        
        pageDtoGrid = getDtoGrid(Mode.APP, graph, pageDtos, refServiceUri);
        DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
        pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());  
      }
    }
    
    return pageDtoGrid;
  }
  
  public Grid getRelatedDtoGrid(String serviceUri, String scope, String app, String graph, String dtoIdentifier, 
      String classId, String classIdentifier, String filter, String sortBy, String sortOrder, int start, int limit)
  {
    String dtiRelativePath = "/" + scope + "/" + app + "/" + graph + "/dxi/filter";
    String dtoRelativePath = "/" + scope + "/" + app + "/" + graph + "/dxo";
    String manifestRelativePath = "/" + scope + "/" + app + "/manifest";
    
    DataTransferObjects dtos = getRelatedDtos(serviceUri, manifestRelativePath, dtiRelativePath, dtoRelativePath, dtoIdentifier, filter,
        sortBy, sortOrder, start, limit);
    
    return getRelatedDtoGrid(Mode.APP, dtos, classId, classIdentifier);
  }
}
