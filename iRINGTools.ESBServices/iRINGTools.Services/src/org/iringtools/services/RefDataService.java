package org.iringtools.services;

import java.io.IOException;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.services.core.RefDataProvider;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class RefDataService extends AbstractService
{
  private static final Logger logger = Logger.getLogger(RefDataService.class);

  @GET
  @Path("/federation")
  public Federation getFederation() throws JAXBException, IOException
  {
    Federation federation = null;

    try
    {
      initService();
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      federation = refDataProvider.getFederation();
    }
    catch (Exception ex)
    {
      logger.error("Error getting federation information: " + ex);
    }

    return federation;
  }

  @POST
  @Path("/federation")
  public Response saveFederation(Federation federation)
  {
    Response response = null;
    
    try
    {
      initService();
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveFederation(federation);
    }
    catch (Exception ex)
    {
      logger.error("Error while saving federation xml: " + ex);
    }

    return response;
  }

  @POST
  @Path("/namespace")
  public Response saveNamespace(Namespace namespace)
  {
    Response response = null;
    
    try
    {
      initService();
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveNamespace(namespace, false);
    }
    catch (Exception ex)
    {
      logger.error("Error while saving namespace: " + ex);
    }

    return response;
  }

  @POST
  @Path("/idgenerator")
  public Response saveIDGenerator(IDGenerator idgenerator)
  {
    Response response = null;
    
    try
    {
      initService();
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveIdGenerator(idgenerator, false);
    }
    catch (Exception ex)
    {
      logger.error("Error while saving ID Generator: " + ex);
    }

    return response;
  }

  @POST
  @Path("/repository")
  public Response saveRepository(Repository repository)
  {
    Response response = null;
    
    try
    {
      initService();
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveRepository(repository, false);
    }
    catch (Exception ex)
    {
      logger.error("Error while saving Repository: " + ex);
    }

    return response;
  }
}
