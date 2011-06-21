package org.iringtools.services;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;

import org.apache.log4j.Logger;
import org.iringtools.directory.Authorized;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.services.core.DirectoryProvider;

@Path("/")
@Produces(MediaType.APPLICATION_XML)
public class DirectoryService extends AbstractService
{
  private static final Logger logger = Logger.getLogger(DirectoryService.class);

  @GET
  @Path("/directory")
  public Directory getDirectory()
  {
    Directory directory = null;

    try
    {
      initService();
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
    ExchangeDefinition xdef = null;

    try
    {
      initService();
      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      xdef = directoryProvider.getExchangeDefinition(scope, exchangeId);
    }
    catch (Exception ex)
    {
      logger.error("Error getting exchange definition for [" + exchangeId + "]: " + ex);
    }

    return xdef;
  }
  
  @GET
  @Path("/auth/{scope}/{app}/{userId}/")
  public Authorized isAuthorized(@PathParam("scope") String scope,
                                 @PathParam("app") String app, 
                                 @PathParam("userId") String userId) {
    Authorized authorized = null;
    
    try
    {
      initService();
      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      authorized = directoryProvider.isAuthorized(scope, app, userId);
    }
    catch (Exception ex)
    {
      logger.error("Error getting user authorization: " + ex);
    }
    
    return authorized;
  }
}