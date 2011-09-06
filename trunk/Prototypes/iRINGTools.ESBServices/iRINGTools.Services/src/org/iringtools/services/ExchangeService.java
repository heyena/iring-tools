package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;

import org.iringtools.directory.Directory;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.security.AuthorizationException;
import org.iringtools.services.core.ExchangeProvider;

@Path("/")
@Produces("application/xml")
public class ExchangeService extends AbstractService
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
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      directory = exchangeProvider.getDirectory();
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
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
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      manifest = exchangeProvider.getManifest(scope, id);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
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
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
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
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    return dataTransferIndices;
  }
  
  @POST
  @Path("/{scope}/exchanges/{id}/page")
  @Consumes("application/xml")
  public DataTransferObjects getDataTransferObjects(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      DxoRequest dxoRequest)
  {
    DataTransferObjects dataTransferObjects = null;
    
    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      dataTransferObjects = exchangeProvider.getDataTransferObjects(scope, id, dxoRequest);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    return dataTransferObjects;
  }

  @POST
  @Path("/{scope}/exchanges/{id}/submit")
  @Consumes("application/xml")
  public ExchangeResponse submitExchange(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      ExchangeRequest exchangeRequest)
  {
    ExchangeResponse exchangeResponse = null;
  
    try
    {
      initService(SERVICE_TYPE);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      exchangeResponse = exchangeProvider.submitExchange(scope, id, exchangeRequest);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    return exchangeResponse;
  }
}