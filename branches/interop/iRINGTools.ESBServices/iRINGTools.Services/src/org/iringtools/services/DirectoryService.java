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
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.services.core.DirectoryProvider;

@Path("/")
@Produces("application/xml")
public class DirectoryService
{
  private static final Logger logger = Logger.getLogger(DirectoryService.class);
  
  @Context 
  private ServletContext context;
  private Hashtable<String, String> settings;
  
  public DirectoryService()
  {
    settings = new Hashtable<String, String>();
  }
    
  @GET
  @Path("/directory")
  public Directory getExchanges()
  {
    Directory directory = null;
    
    try
    {
      init();
      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      directory = directoryProvider.getExchanges();
    }
    catch (Exception ex)
    {
      logger.error("Error getting directory information: " + ex);
    }
    
    return directory;
  }
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}")
  public ExchangeDefinition getExchange(@PathParam("scope") String scope, @PathParam("exchangeId") String exchangeId) 
  {   
    ExchangeDefinition xDef = null;
    
    try
    {
      init();
      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      xDef = directoryProvider.getExchangeDefinition(scope, exchangeId);
    }
    catch (Exception ex)
    {
      logger.error("Error getting exchange definition for [" + exchangeId + "]: " + ex);
    }
    
    return xDef;
  }
  
  private void init()
  {
    settings.put("baseDirectory", context.getRealPath("/"));
  }
}