package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.DfiRequest;
import org.iringtools.dxfr.request.DfoRequest;
import org.iringtools.security.AuthorizationException;
import org.iringtools.services.core.DifferencingProvider;

@Path("/")
@Produces("application/xml")
@Consumes("application/xml")
public class DifferencingService extends AbstractService
{  
  private final String SERVICE_TYPE = "coreService";
  
  @POST
  @Path("/dxi")
  public DataTransferIndices diff(DfiRequest dxiRequest)
  {
    DataTransferIndices dxis = null;
        
    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      dxis = diffProvider.diff(dxiRequest);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return dxis;
  }
  
  @POST
  @Path("/dxo")
  public DataTransferObjects diff(DfoRequest dxoRequest) 
  {
    DataTransferObjects dxos = null;
    
    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      DifferencingProvider diffProvider = new DifferencingProvider(settings);
      dxos = diffProvider.diff(dxoRequest);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return dxos;
  }
}