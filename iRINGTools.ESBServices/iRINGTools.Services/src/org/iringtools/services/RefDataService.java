package org.iringtools.services;

import java.io.FileNotFoundException;
import java.io.IOException;
//import java.util.ArrayList;
import java.util.Hashtable;
//import java.util.List;

import javax.servlet.ServletContext;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
//import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
//import org.iringtools.refdata.queries.Queries;
//import org.iringtools.refdata.queries.Query;
//import org.iringtools.refdata.queries.QueryBindings;
//import org.iringtools.refdata.queries.QueryItem;
//import org.iringtools.refdataentities.RefDataEntities;
import org.iringtools.services.core.RefDataProvider;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class RefDataService
{
  private static final Logger logger = Logger.getLogger(RefDataService.class);

  @Context
  private ServletContext context;  
  private Hashtable<String, String> settings;
  
//  private Hashtable<String, RefDataEntities> searchHistory = new Hashtable<String, RefDataEntities>();
//  private Queries queries = null;
  
  private void init()
  {
	settings.put("baseDirectory", context.getRealPath("/"));
  }
  
  public RefDataService() throws JAXBException, IOException, FileNotFoundException
  {
    settings = new Hashtable<String, String>();
//    init();
//    RefDataProvider refDataProvider = new RefDataProvider(settings);
//    queries = refDataProvider.getQueries(); 
  }
  
  @GET
  @Path("/federation")
  public Federation getFederation() throws JAXBException, IOException
  {
	  Federation federation = null;
	    
	    try
	    {
	      init();
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
	    init();
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
	    init();
	    RefDataProvider refDataProvider = new RefDataProvider(settings);
	    response = refDataProvider.saveNamespace(namespace);
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
	    init();
	    RefDataProvider refDataProvider = new RefDataProvider(settings);
	    response = refDataProvider.saveIdGenerator(idgenerator);
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
	    init();
	    RefDataProvider refDataProvider = new RefDataProvider(settings);
	    response = refDataProvider.saveRepository(repository);
	  }
	  catch (Exception ex)
	  {
	    logger.error("Error while saving Repository: " + ex);
	  }
	  
	  return response;
  }
  
}
