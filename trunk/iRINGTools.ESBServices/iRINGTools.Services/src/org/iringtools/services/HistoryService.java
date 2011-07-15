package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;

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
  public History getExchange(
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
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      HistoryProvider historyProvider = new HistoryProvider(settings);
      history = historyProvider.getExchangeHistory(scope, exchangeId);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return history;
  }
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}/{timestamp}")
  public ExchangeResponse getExchange(
      @PathParam("scope") String scope, 
      @PathParam("exchangeId") String exchangeId,
      @PathParam("timestamp") String timestamp) 
  {
    ExchangeResponse response = null;
    
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
      HistoryProvider historyProvider = new HistoryProvider(settings);
      response = historyProvider.getExchangeResponse(scope, exchangeId, timestamp);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return response;
  }
}