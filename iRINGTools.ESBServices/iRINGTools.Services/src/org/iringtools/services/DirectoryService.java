package org.iringtools.services;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.directory.User;
import org.iringtools.services.core.DirectoryProvider;

@Path("/")
@Produces("application/xml")
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
  @Path("/users/{userId}")
  public User getUser(@PathParam("userId") String userId)
  {
    User user = null;

    try
    {
      initService();
      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      user = directoryProvider.getUser(userId);
    }
    catch (Exception ex)
    {
      logger.error("Error getting user [" + userId + "]: " + ex);
    }
    
    return user;
  }
}