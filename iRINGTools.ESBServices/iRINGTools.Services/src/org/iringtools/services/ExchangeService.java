package org.iringtools.services;

import java.io.IOException;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.services.core.ExchangeProvider;

@Path("/")
@Produces("application/xml")
public class ExchangeService extends AbstractService
{
  private static final Logger logger = Logger.getLogger(ExchangeService.class);

  @GET
  @Path("/directory")
  public Directory getDirectory() throws JAXBException, IOException
  {
    Directory directory = null;
    
    try
    {
      initService();
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      directory = exchangeProvider.getDirectory();
    }
    catch (Exception ex)
    {
      logger.error("Error in getDirectory(): " + ex);
    }
    
    return directory;
  }
  
  @GET
  @Path("/{scope}/exchanges/{id}/manifest")
  @Consumes("application/xml")
  public Manifest getManifest(
      @PathParam("scope") String scope, 
      @PathParam("id") String id)
  {
    Manifest manifest = null;
    
    try
    {
      initService();
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      manifest = exchangeProvider.getManifest(scope, id);
    }
    catch (Exception ex)
    {
      logger.error("Error in getManifest(): " + ex);
    }
    
    return manifest;
  }
  
  @POST
  @Path("/{scope}/exchanges/{id}")
  @Consumes("application/xml")
  public DataTransferIndices getDataTransferIndices(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      @QueryParam("destination") String destination,
      DxiRequest dxiRequest) 
  {
    DataTransferIndices dataTransferIndices = null;
    
    try
    {
      initService();
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      
      if (destination == null || destination.length() == 0)
      {
        dataTransferIndices = exchangeProvider.getDataTransferIndices(scope, id, dxiRequest.getManifest());
      }
      else
      {
        dataTransferIndices = exchangeProvider.getDataTransferIndices(scope, id, destination, dxiRequest);
      }
    }
    catch (Exception ex)
    {
      logger.error("Error in getDataTransferIndices(): " + ex);
    }
    
    return dataTransferIndices;
  }
  
  @POST
  @Path("/{scope}/exchanges/{id}/page")
  @Consumes("application/xml")
  public DataTransferObjects getDataTransferObjects(@PathParam("scope") String scope, @PathParam("id") String id,
      DxoRequest dxoRequest)
  {
    DataTransferObjects dataTransferObjects = null;
    
    try
    {
      initService();
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      dataTransferObjects = exchangeProvider.getDataTransferObjects(scope, id, dxoRequest);
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
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      exchangeResponse = exchangeProvider.submitExchange(scope, id, exchangeRequest);
    }
    catch (Exception ex)
    {
      logger.error("Error in submitExchange(): " + ex);
    }
    
    return exchangeResponse;
  }
}