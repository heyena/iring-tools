package org.iringtools.services.dirsvc;

import java.util.Hashtable;
import javax.servlet.ServletContext;
import javax.ws.rs.ConsumeMime;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import org.iringtools.exchange.library.directory.Directory;
import org.iringtools.exchange.library.directory.ExchangeDefinition;

@Path("/")
@Produces("application/xml")
@ConsumeMime("application/xml")
public class DirectoryService
{
  @Context 
  private ServletContext context;
  private Hashtable<String, String> settings;
  
  public DirectoryService()
  {
    settings = new Hashtable<String, String>();
  }
    
  @GET
  @Path("/exchanges")
  public Directory getExchangeList()
  {
    init();
    DirectoryProvider directoryProvider = new DirectoryProvider(settings);
    return directoryProvider.getExchangeList();
  }
  
  @GET
  @Path("/exchanges/{id}")
  public ExchangeDefinition getExchange(@PathParam("id") String id) 
  {   
    init();
    DirectoryProvider directoryProvider = new DirectoryProvider(settings);
    return directoryProvider.getExchangeDefinition(id);
  }
  
  private void init()
  {
    settings.put("baseDir", context.getRealPath("/"));
  }
}