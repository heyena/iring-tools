package org.iringtools.adapter;

import javax.servlet.ServletContext;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;

@Path("/")
@Produces("application/xml")
public class AdapterService
{
  @Context private ServletContext context;
  
  @GET
  @Path("/{projName}/{appName}/{graphName}/diff")
  public String diff(@PathParam("projName") String projName, 
                     @PathParam("appName") String appName, 
                     @PathParam("graphName") String graphName)
  {
    context.setAttribute("projName", projName);
    context.setAttribute("appName", appName);
    
    AdapterProvider adapterProvider = new AdapterProvider(context);
    return adapterProvider.diff(graphName);
  }
}
