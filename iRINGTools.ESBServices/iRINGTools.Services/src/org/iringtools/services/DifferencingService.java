package org.iringtools.services;

import java.util.Hashtable;

import javax.ws.rs.Consumes;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;

import org.apache.log4j.Logger;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.common.request.DxiRequest;
import org.iringtools.common.request.DxoRequest;
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
  @Path("/dxi")
  public DataTransferIndices diff(DxiRequest dxiRequest)
  {
    DataTransferIndices dxis = null;
    
    try
    {
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      dxis = diffProvider.diff(dxiRequest);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer indices: " + ex);
    }
    
    return dxis;
  }
  
  @POST
  @Path("/dxo")
  public DataTransferObjects diff(DxoRequest dxoRequest)
  {
    DataTransferObjects dxos = null;
    
    try
    {    
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      dxos = diffProvider.diff(dxoRequest);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer objects: " + ex);
    }
    
    return dxos;
  }
}