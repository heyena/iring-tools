package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

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
@Produces(MediaType.APPLICATION_XML)
public class ExchangeService extends AbstractService
{
  private static final Logger logger = Logger.getLogger(AbstractService.class);
  private final String SERVICE_NAME = "ExchangeService";
  
  @GET
  @Path("/directory")
  public Response getDirectory() 
  {
    Directory directory = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (Exception e)
    {
      logger.error("Error initializing ExchangeService: " + e);
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {      
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      directory = exchangeProvider.getDirectory();
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return Response.ok().entity(directory).build();
  }
  
  @GET
  @Path("/{scope}/exchanges/{id}/manifest")
  @Consumes(MediaType.APPLICATION_XML)
  public Response getManifest(
      @PathParam("scope") String scope, 
      @PathParam("id") String id)
  {
    Manifest manifest = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      manifest = exchangeProvider.getManifest(scope, id);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);      
    }
    
    return Response.ok().entity(manifest).build();
  }
  
  @POST
  @Path("/{scope}/exchanges/{id}")
  @Consumes(MediaType.APPLICATION_XML)
  public Response getDataTransferIndices(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      @QueryParam("destination") String destination,
      DxiRequest dxiRequest) 
  {
    DataTransferIndices dataTransferIndices = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
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
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return Response.ok().entity(dataTransferIndices).build();
  }
  
  @POST
  @Path("/{scope}/exchanges/{id}/page")
  @Consumes(MediaType.APPLICATION_XML)
  public Response getDataTransferObjects(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      DxoRequest dxoRequest)
  {
    DataTransferObjects dataTransferObjects = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      dataTransferObjects = exchangeProvider.getDataTransferObjects(scope, id, dxoRequest);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return Response.ok().entity(dataTransferObjects).build();
  }

  @POST
  @Path("/{scope}/exchanges/{id}/submit")
  @Consumes(MediaType.APPLICATION_XML)
  public Response submitExchange(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      ExchangeRequest exchangeRequest)
  {
    ExchangeResponse exchangeResponse = null;
  
    try
    {
      initService(SERVICE_NAME);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      exchangeResponse = exchangeProvider.submitExchange(scope, id, exchangeRequest);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return Response.ok().entity(exchangeResponse).build();
  }
}