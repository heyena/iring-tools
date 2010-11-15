package org.iringtools.services;

import java.util.Hashtable;
import javax.servlet.ServletContext;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.history.History;
import org.iringtools.services.core.HistoryProvider;

@Path("/")
@Produces("application/xml")
public class HistoryService
{
  private static final Logger logger = Logger.getLogger(HistoryService.class);
  
  @Context 
  private ServletContext context;
  private Hashtable<String, String> settings;
  
  public HistoryService()
  {
    settings = new Hashtable<String, String>();
  }
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}")
  public History getExchange(@PathParam("scope") String scope, @PathParam("exchangeId") String exchangeId) 
  {   
    History history = null;
    
    try
    {
      init();
      HistoryProvider historyProvider = new HistoryProvider(settings);
      history = historyProvider.getExchangeHistory(scope, exchangeId);
    }
    catch (Exception ex)
    {
      logger.error("Error getting exchange definition for [" + exchangeId + "]: " + ex);
    }
    
    return history;
  }
  
  private void init()
  {
    settings.put("baseDirectory", context.getRealPath("/"));
  }
}