package org.iringtools.services;

import java.io.IOException;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.directory.Directory;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.services.core.ESBServiceProvider;

@Path("/")
@Produces("application/xml")
public class ESBService extends AbstractService
{
  private static final Logger logger = Logger.getLogger(ESBService.class);

  @GET
  @Path("/directory")
  public Directory getDirectory() throws JAXBException, IOException
  {
    Directory directory = null;
    
    try
    {
      initService();
      ESBServiceProvider serviceProvider = new ESBServiceProvider(settings);
      directory = serviceProvider.getDirectory();
    }
    catch (Exception ex)
    {
      logger.error("Error in getDirectory(): " + ex);
    }
    
    return directory;
  }

  @POST
  @Path("/{scope}/exchanges/{id}")
  @Consumes("application/xml")
  public DataTransferIndices getDataTransferIndices(
		  @PathParam("scope") String scope, 
		  @PathParam("id") String id, 
		  DataFilter dataFilter) 
  {
    DataTransferIndices dataTransferIndices = null;
    
    try
    {
      initService();
      ESBServiceProvider serviceProvider = new ESBServiceProvider(settings);
      dataTransferIndices = serviceProvider.getDataTransferIndices(scope, id, dataFilter);
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
      initService();
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
      initService();
      ESBServiceProvider serviceProvider = new ESBServiceProvider(settings);
      exchangeResponse = serviceProvider.submitExchange(scope, id, exchangeRequest);
    }
    catch (Exception ex)
    {
      logger.error("Error in submitExchange(): " + ex);
    }
    
    return exchangeResponse;
  }
}