package org.iringtools.models;

import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.dxfr.content.ContentObject;
import org.iringtools.dxfr.content.ContentObjects;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.grid.Grid;

public class AppDataModel extends DataModel
{  
  private static final Logger logger = Logger.getLogger(AppDataModel.class);
  
  public AppDataModel(Map<String, Object> session, String refDataServiceUri, FieldFit fieldFit, 
      boolean isAsync, long timeout, long interval)
  {
    super(DataMode.APP, refDataServiceUri, fieldFit, isAsync, timeout, interval);
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String scopeName, String appName, String graphName, 
      String filter, String sortBy, String sortOrder, int start, int limit) throws DataModelException
  {
    String graphPath = "/" + scopeName + "/" + appName + "/" + graphName;
     
    try
    {
      HttpClient httpClient = new HttpClient(serviceUri);
      HttpUtils.addHttpHeaders(session, httpClient);
    
      String manifestPath = graphPath + "/manifest";
      Manifest manifest = getManifest(serviceUri, manifestPath);
      
      if (manifest != null && manifest.getGraphs() != null)
      {
        Graph graph = getGraph(manifest, graphName);
        
        if (graph != null)
        {
          DataFilter dataFilter = createDataFilter(filter, sortBy, sortOrder);
              
          String dtiPath = graphPath + "/dti?start=" + start + "&limit=" + limit;
          DataTransferIndices pageDtis = null;
          
          if (dataFilter == null)
          {
            pageDtis = httpClient.get(DataTransferIndices.class, dtiPath);
          }
          else
          {
            pageDtis = httpClient.post(DataTransferIndices.class, dtiPath, dataFilter);
          }
          
          Grid grid = null;
          
          if (pageDtis != null && pageDtis.getDataTransferIndexList() != null && 
              pageDtis.getDataTransferIndexList().getItems().size() > 0)
          {
            logger.debug("Getting page (" + pageDtis.getDataTransferIndexList().getItems().size() + ") of DTOs...");
            String dtoPath = graphPath + "/page";
            DataTransferObjects pageDtos = httpClient.post(DataTransferObjects.class, dtoPath, pageDtis);
                        
            grid = getDtoGrid(serviceUri, graphPath, manifest, graph, pageDtos);
            grid.setTotal(pageDtis.getTotalCount());
          }
          else
          {
            grid = getDtoGrid(serviceUri, graphPath, manifest, graph, null);
          }
          
          return grid;
        }
      }
    }
    catch (HttpClientException e)
    {
      throw new DataModelException(e);
    }
    
    return null;
  }  
  
  public Grid getRelatedDtoGrid(String serviceUri, String scopeName, String appName, String graphName, String dtoIdentifier, 
      String classId, String classIdentifier, String filter, String sortBy, String sortOrder, int start, int limit) 
      throws DataModelException
  {
    String appRelativePath = "/" + scopeName + "/" + appName + "/" + graphName;
    String dtiRelativePath = appRelativePath + "/" + "/dxi/filter";
    String dtoRelativePath = appRelativePath + "/" + "/dxo";
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
  
  public ContentObject getContent(String targetUri) throws HttpClientException
  {
    HttpClient httpClient = new HttpClient(targetUri);
    HttpUtils.addHttpHeaders(session, httpClient);

    ContentObjects contentObjects = httpClient.get(ContentObjects.class);
    
    return contentObjects.getContentObject().get(0);
  }
}
