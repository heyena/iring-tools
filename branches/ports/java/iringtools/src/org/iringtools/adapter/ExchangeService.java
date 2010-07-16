package org.iringtools.adapter;

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
    
  private void init(String projName, String appName)
  {    
    context.setAttribute("projName", projName);
    context.setAttribute("appName", appName);
  }
  
  @GET
  @Path("/{projName}/{appName}/{graphName}/diff")
  public String diff(@PathParam("projName") String projName,
                     @PathParam("appName") String appName,
                     @PathParam("graphName") String graphName,
                     @QueryParam("format") @DefaultValue("dto") String format)
  {
    init(projName, appName);    
    ExchangeProvider exchangeProvider = new ExchangeProvider(context);
    
    if (format.equalsIgnoreCase("dto"))
    {
      return exchangeProvider.diffDto(graphName);
    }
    
    return exchangeProvider.diffRdf(graphName);
  }
}
