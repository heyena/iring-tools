package org.iringtools.services;

import java.util.Hashtable;

import javax.ws.rs.Consumes;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;

import org.apache.log4j.Logger;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.common.request.DiffDtiRequest;
import org.iringtools.common.request.DiffDtoRequest;
import org.iringtools.services.core.DifferencingProvider;

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
  @Path("/dti")
  public DataTransferIndices diff(DiffDtiRequest diffDtiRequest)
  {
    DataTransferIndices diffDtis = null;
    
    try
    {
      DataTransferIndices sourceDtis = diffDtiRequest.getSourceDataTransferIndicies();
      DataTransferIndices targetDtis = diffDtiRequest.getTargetDataTransferIndicies();
      
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      diffDtis = diffProvider.diff(sourceDtis, targetDtis);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer indices: " + ex);
    }
    
    return diffDtis;
  }
  
  @POST
  @Path("/dto")
  public DataTransferObjects diff(DiffDtoRequest diffDtoRequest)
  {
    DataTransferObjects diffDtos = null;
    
    try
    {    
      DataTransferObjects sourceDtos = diffDtoRequest.getSourceDataTransferObjects(); 
      DataTransferObjects targetDtos = diffDtoRequest.getTargetDataTransferObjects();
      
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      diffDtos = diffProvider.diff(sourceDtos, targetDtos);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer objects: " + ex);
    }
    
    return diffDtos;
  }
}