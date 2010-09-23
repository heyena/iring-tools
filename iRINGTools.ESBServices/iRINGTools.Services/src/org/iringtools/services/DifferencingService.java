package org.iringtools.services;

import java.util.Hashtable;
import javax.ws.rs.Consumes;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;

import org.apache.log4j.Logger;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.exchange.DifferencingProvider;
import org.iringtools.exchange.DxiRequest;
import org.iringtools.exchange.DxoRequest;

@Path("/")
@Produces("application/xml")
@Consumes("application/xml")
public class DifferencingService
{
  private static final Logger logger = Logger.getLogger(DifferencingService.class);
  
  //@Context private ServletContext context;
  private Hashtable<String, String> settings;
  
  public DifferencingService()
  {
    settings = new Hashtable<String, String>();
  }
  
  @POST
  @Path("/dxi")
  public DataTransferIndices diff(DxiRequest dxiRequest)
  {
    DataTransferIndices indices = null;
    
    try
    {
      DataTransferIndices sourceDtis = dxiRequest.getSourceDataTransferIndicies();
      DataTransferIndices targetDtis = dxiRequest.getTargetDataTransferIndicies();
      
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      indices = diffProvider.diff(sourceDtis, targetDtis);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer indices: " + ex);
    }
    
    return indices;
  }
  
  @POST
  @Path("/dxo")
  public DataTransferObjects diff(DxoRequest dxoRequest)
  {
    DataTransferObjects diffDtoList = null;
    
    try
    {    
      DataTransferObjects sourceDtos = dxoRequest.getSourceDataTransferObjects(); 
      DataTransferObjects targetDtos = dxoRequest.getTargetDataTransferObjects();
      
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      diffDtoList = diffProvider.diff(sourceDtos, targetDtos);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer objects: " + ex);
    }
    
    return diffDtoList;
  }
}