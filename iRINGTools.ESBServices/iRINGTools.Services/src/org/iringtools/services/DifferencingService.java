package org.iringtools.services;

import javax.ws.rs.Consumes;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;

import org.apache.log4j.Logger;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.DiffDxoRequest;
import org.iringtools.dxfr.request.DiffDxiRequest;
import org.iringtools.services.core.DifferencingProvider;

@Path("/")
@Produces("application/xml")
@Consumes("application/xml")
public class DifferencingService extends AbstractService
{
  private static final Logger logger = Logger.getLogger(DifferencingService.class);
  
  @POST
  @Path("/dxi")
  public DataTransferIndices diff(DiffDxiRequest dxiRequest)
  {
    DataTransferIndices dxis = null;
    
    try
    {
      initService();
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      dxis = diffProvider.diff(dxiRequest);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer indices: " + ex);
      dxis = null;
    }
    
    return dxis;
  }
  
  @POST
  @Path("/dxo")
  public DataTransferObjects diff(DiffDxoRequest dxoRequest) 
  {
    DataTransferObjects dxos = null;
    
    try
    {    
      initService();
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      dxos = diffProvider.diff(dxoRequest);
    }
    catch (Exception ex)
    {
      logger.error("Error while comparing data transfer objects: " + ex);
      dxos = null;
    }
    
    return dxos;
  }
}