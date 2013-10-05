package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.DefaultValue;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.Response;
import org.iringtools.history.History;
import org.iringtools.library.exchange.ExchangeProvider;

@Path("/")
@Produces("application/xml")
public class HistoryService extends AbstractService
{  
	private final String SERVICE_NAME = "HistoryService";
  
	@GET
	@Path("/{scope}/exchanges/{exchangeId}")
	public Response getExchange(
			@PathParam("scope") String scope, 
			@PathParam("exchangeId") String exchangeId,
			@DefaultValue("0") @QueryParam("limit") int limit) 
	{   
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
			ExchangeProvider historyProvider = new ExchangeProvider(settings);
			History history = historyProvider.getExchangeHistory(scope, exchangeId, limit);
			return Response.ok().entity(history).build();
		}
		catch (Exception e)
		{
			return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}
	}
  
	@GET
	@Path("/{scope}/exchanges/{exchangeId}/{timestamp}/{start}/{limit}")
	public Response getExchange(
			@PathParam("scope") String scope, 
			@PathParam("exchangeId") String exchangeId,
			@PathParam("timestamp") String timestamp,
			@PathParam("start") int start,
			@PathParam("limit") int limit) 
	{
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
			ExchangeProvider historyProvider = new ExchangeProvider(settings);
			org.iringtools.common.response.Response xResponse = historyProvider.getExchangeResponse(scope, exchangeId, timestamp, start, limit);
			return Response.ok().entity(xResponse).build();
		}
		catch (Exception e)
		{
			return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}
	}
	
}