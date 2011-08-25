package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;

import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.security.AuthorizationException;
import org.iringtools.services.core.DirectoryProvider;

@Path("/")
@Produces(MediaType.APPLICATION_XML)
public class DirectoryService extends AbstractService
{
  private final String SERVICE_TYPE = "coreService";
  
  @GET
  @Path("/directory")
  public Directory getDirectory()
  {
    Directory directory = null;

    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    DirectoryProvider directoryProvider = new DirectoryProvider(settings);

    try
    {
      directory = directoryProvider.getExchanges();
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return directory;
  }

  @GET
  @Path("/{scope}/exchanges/{exchangeId}")
  public ExchangeDefinition getExchange(@PathParam("scope") String scope, @PathParam("exchangeId") String exchangeId)
  {
    ExchangeDefinition xdef = null;

    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    DirectoryProvider directoryProvider = new DirectoryProvider(settings);

    try
    {
      xdef = directoryProvider.getExchangeDefinition(scope, exchangeId);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return xdef;
  }
}