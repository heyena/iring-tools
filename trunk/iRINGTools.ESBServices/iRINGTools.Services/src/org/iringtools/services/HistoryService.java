package org.iringtools.services;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.log4j.Logger;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.services.core.HistoryProvider;

@Path("/")
@Produces("application/xml")
public class HistoryService extends AbstractService
{
  private static final Logger logger = Logger.getLogger(HistoryService.class);
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}")
  public History getExchange(@PathParam("scope") String scope, @PathParam("exchangeId") String exchangeId) 
  {   
    History history = null;
    
    try
    {
      initService();
      HistoryProvider historyProvider = new HistoryProvider(settings);
      history = historyProvider.getExchangeHistory(scope, exchangeId);
    }
    catch (Exception ex)
    {
      logger.error("Error getting exchange logs for [" + scope + ", " + exchangeId + "]: " + ex);
    }
    
    return history;
  }
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}/{timestamp}")
  public ExchangeResponse getExchange(@PathParam("scope") String scope, @PathParam("exchangeId") String exchangeId,
      @PathParam("timestamp") XMLGregorianCalendar timestamp) 
  {
    ExchangeResponse response = null;
    
    try
    {
      initService();
      HistoryProvider historyProvider = new HistoryProvider(settings);
      response = historyProvider.getExchangeResponse(scope, exchangeId, timestamp);
    }
    catch (Exception ex)
    {
      logger.error("Error getting exchange log for [" + scope + ", " + exchangeId + ", " + timestamp + "]: " + ex);
    }
    
    return response;
  }
}