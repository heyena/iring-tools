package org.iringtools.adapter;

import java.util.Hashtable;

import javax.servlet.ServletContext;
import javax.ws.rs.DefaultValue;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.Context;

@Path("/")
@Produces("application/xml")
public class ExchangeService
{
  @Context private ServletContext context;
    
  @GET
  @Path("/{projName}/{appName}/{graphName}/diff")
  public String diff(@PathParam("projName") String projName,
                     @PathParam("appName") String appName,
                     @PathParam("graphName") String graphName,
                     @QueryParam("format") @DefaultValue("dto") String format)
  {
    Hashtable<String, String> settings = new Hashtable<String, String>();
    settings.put("baseDirectory", context.getRealPath("/"));
    settings.put("projName", projName);
    settings.put("appName", appName);
    
    ExchangeProvider exchangeProvider = new ExchangeProvider(settings);    
    return exchangeProvider.diff(graphName, format);
  }
}
