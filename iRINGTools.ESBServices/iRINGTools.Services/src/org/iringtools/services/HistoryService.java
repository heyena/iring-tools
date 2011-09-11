package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Response;

import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.security.AuthorizationException;
import org.iringtools.services.core.HistoryProvider;

@Path("/")
@Produces("application/xml")
public class HistoryService extends AbstractService
{  
  private final String SERVICE_TYPE = "coreService";
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}")
  public Response getExchange(
      @PathParam("scope") String scope, 
      @PathParam("exchangeId") String exchangeId) 
  {   
    History history = null;
    
    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      HistoryProvider historyProvider = new HistoryProvider(settings);
      history = historyProvider.getExchangeHistory(scope, exchangeId);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return Response.ok().entity(history).build();
  }
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}/{timestamp}")
  public Response getExchange(
      @PathParam("scope") String scope, 
      @PathParam("exchangeId") String exchangeId,
      @PathParam("timestamp") String timestamp) 
  {
    ExchangeResponse xResponse = null;
    
    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      HistoryProvider historyProvider = new HistoryProvider(settings);
      xResponse = historyProvider.getExchangeResponse(scope, exchangeId, timestamp);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return Response.ok().entity(xResponse).build();
  }
}