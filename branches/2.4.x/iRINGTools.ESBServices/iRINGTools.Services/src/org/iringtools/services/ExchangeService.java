package org.iringtools.services;

import java.util.List;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.DefaultValue;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;

import org.iringtools.common.response.Level;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.security.AuthorizationException;
import org.iringtools.services.core.ExchangeProvider;

@Path("/")
@Produces(MediaType.APPLICATION_XML)
public class ExchangeService extends AbstractService
{
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
    catch (AuthorizationException e)
    {
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
    catch (AuthorizationException e)
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
      @QueryParam("dtiOnly") boolean dtiOnly,
      DxiRequest dxiRequest) 
  {
    DataTransferIndices dataTransferIndices = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);      
      dataTransferIndices = exchangeProvider.getDataTransferIndices(scope, id, dxiRequest, dtiOnly);
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
    catch (AuthorizationException e)
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
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      Response response = exchangeProvider.submitExchange(scope, id, exchangeRequest);
      return response;
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
  }
  
  @POST
  @Path("/{scope}/exchanges/{id}/differences/summary")
  @Consumes(MediaType.APPLICATION_XML)
  public Response getDifferencesSummary(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      DataFilter filter
)
  {
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      Response response = exchangeProvider.getDifferencesSummary(scope, id, filter);
      return response;
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
  }

  @POST
  @Path("/{scope}/exchanges/{id}/unattended")
  @Consumes(MediaType.APPLICATION_XML)
  public Response submitUnattended(
      @PathParam("scope") String scope, 
      @PathParam("id") String id,
      @DefaultValue("false") @QueryParam("Add") boolean add,
      @DefaultValue("false") @QueryParam("Change") boolean change,
      @DefaultValue("false") @QueryParam("Delete") boolean delete,
      DataFilter filter
)
  {
    Manifest manifest = null;
    DataTransferIndices dtis = null;
    DxiRequest dxiRequest = new DxiRequest();
    ExchangeRequest exchangeRequest = new ExchangeRequest();
    
    DataTransferIndices actionDtis = new DataTransferIndices();
    DataTransferIndexList actionDtiList = new DataTransferIndexList();
    actionDtis.setDataTransferIndexList(actionDtiList);
    List<DataTransferIndex> actionDtiListItems = actionDtiList.getItems();
        
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      manifest = exchangeProvider.getManifest(scope, id);
      
      dxiRequest.setManifest(manifest);
      dxiRequest.setDataFilter(filter);
      dtis = exchangeProvider.getDataTransferIndices(scope, id, dxiRequest, false);
      
      // Depending on the 'actions' we'll limit the dti's we send to the exchange request
      for (DataTransferIndex dxi : dtis.getDataTransferIndexList().getItems())
      {
        TransferType transferType = dxi.getTransferType();
        
        if ( (transferType == TransferType.ADD && add) ||
        	 (transferType == TransferType.CHANGE && change) ||
        	 (transferType == TransferType.DELETE && delete) ) 
        {
        	actionDtiListItems.add(dxi);        
        }
      }    

      // are there any items left to exchange ?
      if (actionDtiListItems.size() == 0)
      {
          ExchangeResponse xRes = new ExchangeResponse();
          xRes.setLevel(Level.WARNING);
    	  xRes.setSummary("No items to exchange.");
          return Response.ok().entity(xRes).build();
      }      
      
      exchangeRequest.setManifest(manifest);
      exchangeRequest.setDataTransferIndices(actionDtis);
      exchangeRequest.setReviewed(true);
      
      Response response = exchangeProvider.submitExchange(scope, id, exchangeRequest);
      return response;
      
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);      
    }
  }

  @GET
  @Path("/{scope}/exchanges/{id}")
  public Response getExchange(@PathParam("scope") String scope, @PathParam("id") String id)
  {
    ExchangeDefinition xdef = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      xdef = exchangeProvider.getExchangeDefinition(scope, id);
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return Response.ok().entity(xdef).build();
  }
  
  @GET
  @Path("/results/{xtoken}")
  public Response getExchangeResult(@PathParam("xtoken") String xtoken) 
  {
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }
    
    try
    {      
      ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
      ExchangeResponse result = exchangeProvider.getExchangeResult(xtoken);
      
      if (result == null)
        return Response.status(Status.NOT_FOUND).build();
      
      return Response.ok().entity(result).build();
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e.getMessage());
    }
  }  
}