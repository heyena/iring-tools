package org.iringtools.services;

import java.io.IOException;
import java.util.Hashtable;

import javax.servlet.ServletContext;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.services.core.ESBServiceProvider;

@Path("/")
@Produces("application/xml")
public class ESBService
{
  private static final Logger logger = Logger.getLogger(ESBService.class);
  
  @Context
  private ServletContext context;
  private Hashtable<String, String> settings;
  
//  @Context
//  private javax.ws.rs.core.SecurityContext securityContext;
//  @Context 
//  private org.apache.cxf.jaxrs.ext.MessageContext messageContext;

  public ESBService()
  {
    settings = new Hashtable<String, String>();
  }

  @GET
  @Path("/directory")
  public Directory getDirectory() throws JAXBException, IOException
  {
    Directory directory = null;
    
    try
    {
      init();
      ESBServiceProvider serviceProvider = new ESBServiceProvider(settings);
      directory = serviceProvider.getDirectory();
    }
    catch (Exception ex)
    {
      logger.error("Error in getDirectory(): " + ex);
    }
    
    return directory;
  }

  @GET
  @Path("/{scope}/exchanges/{id}")
  public DataTransferIndices getDataTransferIndices(@PathParam("scope") String scope, @PathParam("id") String id)
  {
    DataTransferIndices dataTransferIndices = null;
    
    try
    {
      init();
      ESBServiceProvider serviceProvider = new ESBServiceProvider(settings);
      dataTransferIndices = serviceProvider.getDataTransferIndices(scope, id);
    }
    catch (Exception ex)
    {
      logger.error("Error in getDataTransferIndices(): " + ex);
    }
    
    return dataTransferIndices;
  }

  @POST
  @Path("/{scope}/exchanges/{id}")
  @Consumes("application/xml")
  public DataTransferObjects getDataTransferObjects(@PathParam("scope") String scope, @PathParam("id") String id,
      DataTransferIndices dataTransferIndices)
  {
    DataTransferObjects dataTransferObjects = null;
    
    try
    {
      init();
      ESBServiceProvider serviceProvider = new ESBServiceProvider(settings);
      dataTransferObjects = serviceProvider.getDataTransferObjects(scope, id, dataTransferIndices);
    }
    catch (Exception ex)
    {
      logger.error("Error in getDataTransferObjects(): " + ex);
    }
    
    return dataTransferObjects;
  }

  @POST
  @Path("/{scope}/exchanges/{id}/submit")
  @Consumes("application/xml")
  public ExchangeResponse submitExchange(@PathParam("scope") String scope, @PathParam("id") String id,
      ExchangeRequest exchangeRequest) throws IOException, JAXBException
  {
    ExchangeResponse exchangeResponse = null;
  
    try
    {
      init();
      ESBServiceProvider serviceProvider = new ESBServiceProvider(settings);
      exchangeResponse = serviceProvider.submitExchange(scope, id, exchangeRequest);
    }
    catch (Exception ex)
    {
      logger.error("Error in submitExchange(): " + ex);
    }
    
    return exchangeResponse;
  }
  
  private void init()
  {
    settings.put("baseDirectory", context.getRealPath("/"));

    String directoryServiceUri = context.getInitParameter("directoryServiceUri");
    if (directoryServiceUri == null || directoryServiceUri.equals(""))
      directoryServiceUri = "http://localhost:8080/iringtools/services/dirsvc";
    settings.put("directoryServiceUri", directoryServiceUri);

    String differencingServiceUri = context.getInitParameter("differencingServiceUri");
    if (differencingServiceUri == null || differencingServiceUri.equals(""))
      differencingServiceUri = "http://localhost:8080/iringtools/services/diffsvc";
    settings.put("differencingServiceUri", differencingServiceUri);

    String poolSize = context.getInitParameter("poolSize");
    if (poolSize == null || poolSize.equals(""))
      poolSize = "50";
    settings.put("poolSize", poolSize);
  }
}