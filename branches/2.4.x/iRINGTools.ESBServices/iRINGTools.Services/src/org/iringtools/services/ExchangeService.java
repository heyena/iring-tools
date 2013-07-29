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

import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Scope;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.library.RequestStatus;
import org.iringtools.services.ServiceProviderException;
import org.iringtools.library.exchange.ExchangeProvider;
import org.iringtools.library.directory.DirectoryProvider;

@Path("/")
@Produces(MediaType.APPLICATION_XML)
public class ExchangeService extends AbstractService {
	private final String SERVICE_NAME = "ExchangeService";

	private static final Logger logger = Logger.getLogger(ExchangeProvider.class);

	@POST
	@Path("/{scope}/exchanges/{id}/differences")
	@Consumes(MediaType.APPLICATION_XML)
	@Produces({ MediaType.APPLICATION_XML, MediaType.APPLICATION_JSON })
	public Response getDifferences(@PathParam("scope") String scope,
			@PathParam("id") String id,
			@DefaultValue("0") @QueryParam("start") int start,
			@DefaultValue("25") @QueryParam("limit") int limit,
			@DefaultValue("false") @QueryParam("Sync") boolean sync,
			@DefaultValue("true") @QueryParam("Add") boolean add,
			@DefaultValue("true") @QueryParam("Change") boolean change,
			@DefaultValue("true") @QueryParam("Delete") boolean delete,
			DataFilter filter) {
		
		logger.debug("getDataTransferObjectsFiltered(" + scope + ", " + id
				+ ", " + start + ", " + limit + ", " + sync + ", " + add + ", "
				+ change + ", " + delete + ", " + ", filter)");
		
		try {
			initService(SERVICE_NAME);
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
		}

		try {
			ExchangeProvider exchangeProvider = new ExchangeProvider(settings);

			Manifest manifest = null;
			DataTransferIndices dtis = null;
			DxiRequest dxiRequest = new DxiRequest();
			ExchangeResponse xr = new ExchangeResponse();

			Exchange exchange = getExchangeFromDirectory(scope, id);
			manifest = exchangeProvider.getCrossedManifest(exchange);

			dxiRequest.setManifest(manifest);
			dxiRequest.setDataFilter(filter);
			dtis = exchangeProvider.getDataTransferIndices(exchange, dxiRequest);

			int itemCount = 0;
			int itemCountSync = 0;
			int itemCountAdd = 0;
			int itemCountChange = 0;
			int itemCountDelete = 0;

			DataTransferIndices actionDtis = new DataTransferIndices();
			DataTransferIndexList actionDtiList = new DataTransferIndexList();
			actionDtis.setDataTransferIndexList(actionDtiList);
			List<DataTransferIndex> actionDtiListItems = actionDtiList.getItems();

			// Depending on the 'actions' we'll limit the dti's we send to the
			// exchange request
			for (DataTransferIndex dxi : dtis.getDataTransferIndexList().getItems()) {
				TransferType transferType = dxi.getTransferType();

				// gather counts for the exchange response
				if (transferType == TransferType.SYNC)
					itemCountSync++;
				if (transferType == TransferType.ADD)
					itemCountAdd++;
				if (transferType == TransferType.CHANGE)
					itemCountChange++;
				if (transferType == TransferType.DELETE)
					itemCountDelete++;

				if ((transferType == TransferType.SYNC && sync)
						|| (transferType == TransferType.ADD && add)
						|| (transferType == TransferType.CHANGE && change)
						|| (transferType == TransferType.DELETE && delete)) {
					itemCount++;
					// get just the requested page of changes
					if ((itemCount > start) && (itemCount <= start + limit))
					{
						actionDtiListItems.add(dxi);
					}
				}
			}

			DataTransferObjects dtos = exchangeProvider.getDataTransferObjects(	exchange, manifest, actionDtiListItems);

			xr.setExchangeId(id);
			xr.setSenderUri(exchange.getSourceUri());
			xr.setSenderScope(exchange.getSourceScope());
			xr.setSenderApp(exchange.getSourceApp());
			xr.setSenderGraph(exchange.getSourceGraph());
			xr.setReceiverUri(exchange.getTargetUri());
			xr.setReceiverScope(exchange.getTargetScope());
			xr.setReceiverApp(exchange.getTargetApp());
			xr.setReceiverGraph(exchange.getTargetGraph());
			// <xs:element name="startTime" type="xs:dateTime" />
			// <xs:element name="endTime" type="xs:dateTime" />
			// NB the local itemCount is only the items on the page that are
			// different, where as the xr's itemCount should
			// reflect everything
			xr.setItemCount(itemCountSync + itemCountAdd + itemCountChange
					+ itemCountDelete);
			xr.setItemCountSync(itemCountSync);
			xr.setItemCountAdd(itemCountAdd);
			xr.setItemCountChange(itemCountChange);
			xr.setItemCountDelete(itemCountDelete);
			xr.setSummary("Page of differences.");

			dtos.setVersion(id);
			dtos.setSenderAppName(exchange.getSourceApp());
			dtos.setSenderScopeName(exchange.getSourceScope());
			dtos.setAppName(exchange.getTargetApp());
			dtos.setScopeName(exchange.getTargetScope());

			return Response.ok().entity(dtos).build();
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}
	}

	@POST
	@Path("/{scope}/exchanges/{id}/differences/summary")
	@Consumes(MediaType.APPLICATION_XML)
	@Produces({ MediaType.APPLICATION_XML, MediaType.APPLICATION_JSON })
	public Response getDifferencesSummary(@PathParam("scope") String scope,
			@PathParam("id") String id, DataFilter filter) {
		
		logger.debug("getDifferencesSummary(" + scope + ", " + id + ", filter)");
		
		try {
			initService(SERVICE_NAME);
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
		}

		try {
			ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
			
			Manifest manifest = null;
			DataTransferIndices dtis = null;
			DxiRequest dxiRequest = new DxiRequest();

			Exchange exchange = getExchangeFromDirectory(scope, id);
			manifest = exchangeProvider.getCrossedManifest(exchange);

			dxiRequest.setManifest(manifest);
			dxiRequest.setDataFilter(filter);
			dtis = exchangeProvider.getDataTransferIndices(exchange, dxiRequest);

			int iCountSync = 0;
			int iCountAdd = 0;
			int iCountChange = 0;
			int iCountDelete = 0;
			for (DataTransferIndex dxi : dtis.getDataTransferIndexList().getItems()) {
				TransferType transferType = dxi.getTransferType();
				if (transferType == TransferType.ADD) {
					iCountAdd++;
				} else if (transferType == TransferType.CHANGE) {
					iCountChange++;
				} else if (transferType == TransferType.DELETE) {
					iCountDelete++;
				} else {
					iCountSync++;
				}
			}

			ExchangeResponse xRes = new ExchangeResponse();
			xRes.setExchangeId(id);
			xRes.setSenderUri(exchange.getSourceUri());
			xRes.setSenderScope(exchange.getSourceScope());
			xRes.setSenderApp(exchange.getSourceApp());
			xRes.setSenderGraph(exchange.getSourceGraph());
			xRes.setReceiverUri(exchange.getTargetUri());
			xRes.setReceiverScope(exchange.getTargetScope());
			xRes.setReceiverApp(exchange.getTargetApp());
			xRes.setReceiverGraph(exchange.getTargetGraph());
			// <xs:element name="startTime" type="xs:dateTime" />
			// <xs:element name="endTime" type="xs:dateTime" />
			xRes.setLevel(Level.WARNING);
			xRes.setItemCount(iCountSync + iCountAdd + iCountChange + iCountDelete);
			xRes.setItemCountSync(iCountSync);
			xRes.setItemCountAdd(iCountAdd);
			xRes.setItemCountChange(iCountChange);
			xRes.setItemCountDelete(iCountDelete);
			xRes.setSummary("Difference Summary only, this was not a data exchange request.");
			return Response.ok().entity(xRes).build();
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}
	}

	@POST
	@Path("/{scope}/exchanges/{id}/unattended")
	@Consumes(MediaType.APPLICATION_XML)
	public Response submitUnattended(@PathParam("scope") String scope,
			@PathParam("id") String id,
			@DefaultValue("false") @QueryParam("Add") boolean add,
			@DefaultValue("false") @QueryParam("Change") boolean change,
			@DefaultValue("false") @QueryParam("Delete") boolean delete,
			DataFilter filter) {
		
		try {
			initService(SERVICE_NAME);
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
		}

		ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
		
		Manifest manifest = null;
		DataTransferIndices dtis = null;
		DxiRequest dxiRequest = new DxiRequest();
		ExchangeRequest exchangeRequest = new ExchangeRequest();

		DataTransferIndices actionDtis = new DataTransferIndices();
		DataTransferIndexList actionDtiList = new DataTransferIndexList();
		actionDtis.setDataTransferIndexList(actionDtiList);
		List<DataTransferIndex> actionDtiListItems = actionDtiList.getItems();
		
		try {
			Exchange exchange = getExchangeFromDirectory(scope, id);
			manifest = exchangeProvider.getCrossedManifest(exchange);

			dxiRequest.setManifest(manifest);
			dxiRequest.setDataFilter(filter);
			dtis = exchangeProvider.getDataTransferIndices(exchange, dxiRequest);

			// Depending on the 'actions' we'll limit the dti's we send to the
			// exchange request
			for (DataTransferIndex dxi : dtis.getDataTransferIndexList()
					.getItems()) {
				TransferType transferType = dxi.getTransferType();

				if ((transferType == TransferType.ADD && add)
						|| (transferType == TransferType.CHANGE && change)
						|| (transferType == TransferType.DELETE && delete)) {
					actionDtiListItems.add(dxi);
				}
			}

			// are there any items left to exchange ?
			// This optimization has been commented out because we want the "history" of the exchange created, even if there is nothing to do
			//if (actionDtiListItems.size() == 0) {
			//	ExchangeResponse xRes = new ExchangeResponse();
			//	xRes.setLevel(Level.WARNING);
			//	xRes.setSummary("No items to exchange.");
			//	return Response.ok().entity(xRes).build();
			//}

			exchangeRequest.setManifest(manifest);
			exchangeRequest.setDataTransferIndices(actionDtis);
			exchangeRequest.setReviewed(true);

			boolean async = isAsync();
			ExchangeResponse exchangeResponse = exchangeProvider.submitExchange(async, scope, id, exchange, exchangeRequest);
			return Response.ok().entity(exchangeResponse).build();

		} catch (Exception e) {
			return prepareErrorResponse(
					HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}
	}

	@GET
	@Path("/requests/{id}")
	public Response getRequestStatus(@PathParam("id") String id) {
		try {
			initService(SERVICE_NAME);
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
		}

		try {
			ExchangeProvider exchangeProvider = new ExchangeProvider(settings);
			RequestStatus requestStatus = exchangeProvider.getRequestStatus(id);

			return Response.ok().entity(requestStatus).build();
		} catch (Exception e) {
			return prepareErrorResponse(
					HttpServletResponse.SC_INTERNAL_SERVER_ERROR,
					e.getMessage());
		}
	}
	
	private Exchange getExchangeFromDirectory(String scopeName, String xId)
			throws ServiceProviderException {
		Exchange exchange = null;

		try {
			DirectoryProvider directoryProvider = new DirectoryProvider(settings);
			Directory directory = directoryProvider.getDirectory();

			for (Scope scope : directory.getScope()) {
				if (scope.getName().equalsIgnoreCase(scopeName)) {
					if (scope.getDataExchanges() != null) {
						for (Commodity commodity : scope.getDataExchanges()
								.getCommodity()) {
							for (Exchange xchange : commodity.getExchange()) {
								if (xId.equalsIgnoreCase(xchange.getId())) {
									if (xchange.getPoolSize() == null) {
										Integer defaultPoolSize = Integer.parseInt(settings.get("poolSize").toString());
										xchange.setPoolSize(defaultPoolSize);
									}
									return xchange;
								}
							}
						}
					}
					break;
				}
			}
		} catch (Exception e) {
			String message = "Error exchange from directory: " + e;
			//logger.error(message);
			throw new ServiceProviderException(message);
		}

		return exchange;
	}

	private boolean isAsync() {
		String asyncHeader = "http-header-async";
		boolean async = settings.containsKey(asyncHeader)
				&& Boolean.parseBoolean(settings.get(asyncHeader).toString());

		return async;
	}
}