package org.iringtools.models;

import java.util.Map;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.widgets.grid.Grid;

public class AppDataModel extends DataModel
{  
  public AppDataModel(Map<String, Object> session, String refDataServiceUri, FieldFit fieldFit)
  {
    super(DataMode.APP, refDataServiceUri, fieldFit);
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String scopeName, String appName, String graphName, String filter, 
      String sortBy, String sortOrder, int start, int limit) throws DataModelException
  {
    String appRelativePath = "/" + scopeName + "/" + appName;
    String dtiRelativePath = appRelativePath + "/" + graphName + "/dxi/filter";
    String dtoRelativePath = appRelativePath + "/" + graphName + "/dxo";  
    String manifestRelativePath = appRelativePath + "/manifest";
    String dataFilterRelativePath = appRelativePath + "/datafilter";
    
    Grid pageDtoGrid = null;
    Manifest manifest = getManifest(serviceUri, manifestRelativePath);
    
    if (manifest != null && manifest.getGraphs() != null)
    {
      Graph graph = getGraph(manifest, graphName);
      
      if (graph != null)
      {
        DataTransferObjects pageDtos = getPageDtos(serviceUri, manifestRelativePath, dtiRelativePath, 
            dtoRelativePath, filter, sortBy, sortOrder, start, limit, dataFilterRelativePath);
        
        pageDtoGrid = getDtoGrid(appRelativePath, manifest, graph, pageDtos);
        DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
        pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());  
      }
      else
      {
        throw new DataModelException("Graph [" + graphName + "] not found.");
      }
    }
    
    return pageDtoGrid;
  }
  
  public Grid getRelatedDtoGrid(String serviceUri, String scopeName, String appName, String graphName, String dtoIdentifier, 
      String classId, String classIdentifier, String filter, String sortBy, String sortOrder, int start, int limit) 
      throws DataModelException
  {
    String appRelativePath = "/" + scopeName + "/" + appName;
    String dtiRelativePath = appRelativePath + "/" + graphName + "/dxi/filter";
    String dtoRelativePath = appRelativePath + "/" + graphName + "/dxo";
    String manifestRelativePath = appRelativePath + "/manifest";
    
    Grid pageDtoGrid = null;
    Manifest manifest = getManifest(serviceUri, manifestRelativePath);    
    Graph graph = getGraph(manifest, graphName);
    
    if (graph != null)
    {
      DataTransferObjects dtos = getRelatedItems(serviceUri, manifestRelativePath, dtiRelativePath, dtoRelativePath, 
          dtoIdentifier, filter, sortBy, sortOrder, start, limit);
      
      pageDtoGrid = getRelatedItemGrid(appRelativePath, manifest, graph, dtos, classId, classIdentifier);
    }
    
    return pageDtoGrid;
  }
}
