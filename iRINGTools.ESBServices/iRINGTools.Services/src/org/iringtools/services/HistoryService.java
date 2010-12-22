package org.iringtools.services;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;

import org.apache.log4j.Logger;
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
      logger.error("Error getting exchange definition for [" + exchangeId + "]: " + ex);
    }
    
    return history;
  }
}