package org.iringtools.adapter.services;


	import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;

import org.iringtools.adapter.services.core.AdapterProvider;
import org.iringtools.common.Version;
import org.iringtools.common.response.Response;
import org.iringtools.library.Application;
import org.iringtools.library.Scope;
import org.iringtools.library.Scopes;
import org.iringtools.mapping.Mapping;
import org.iringtools.security.AuthorizationException;

	@Path("/")
	@Consumes(MediaType.APPLICATION_XML)
	@Produces(MediaType.APPLICATION_XML)
	public class AdapterService extends AbstractService {
	
	private AdapterProvider _adapterProvider = null;
	private final String SERVICE_TYPE = "adapterService";

	@GET
	@Path("/version")
	@Produces("application/xml")
	public Version getVersion(){
		
		Version version = null;
		
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
	    	_adapterProvider = new AdapterProvider(settings);
	    	version = _adapterProvider.getVersion();
	    }
	    catch (Exception e)
	    {
	      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
	    }
	    
	    return version;
	    
	}

	@GET
	@Path("/scopes")
	@Produces("application/xml")
	public Scopes getScope(){
	
		Scopes scopeList = null;
		
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
	    	_adapterProvider = new AdapterProvider(settings);
	    	scopeList = _adapterProvider.getScopes();
	    }
	    catch (Exception e)
	    {
	      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
	    }

	    return scopeList;
	}
	
	@POST
	@Path("/scopes")
	@Produces("application/xml")
	public Response updateScopes(Scopes scopes)
	{
		
		Response response = null;
		
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
	    	_adapterProvider = new AdapterProvider(settings);
	    	response = _adapterProvider.updateScopes(scopes);
	    }
	    catch (Exception e)
	    {
	      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
	    }

	    return response;
	}
	
	@POST
	@Path("/scope")
	@Produces("application/xml")
	public Response updateScope(Scope updatedScope)
	{
		
		Response response = null;
		
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
	    	_adapterProvider = new AdapterProvider(settings);
	    	
	    	response = _adapterProvider.updateScope(updatedScope);
	    }
	    catch (Exception e)
	    {
	      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
	    }

	    return response;
	}
	
	@POST
	@Path("/{scopeName}")
	public Response updateApplication(@PathParam("scopeName")String scopeName, Application application)
	{
		Response response = null;
		
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
	    	_adapterProvider = new AdapterProvider(settings);
	    	
	    	response = _adapterProvider.updateApplication(scopeName, application);
	    }
	    catch (Exception e)
	    {
	      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
	    }

	    return response;
	}
	
	@GET
	@Path("/{scopeName}/{applicationName}/mapping")
	@Produces("application/xml")
	
	public Mapping getMapping(@PathParam("scopeName") String scopeName, @PathParam("applicationName") String applicationName)
    {
      return _adapterProvider.getMapping(scopeName, applicationName);
    }
}