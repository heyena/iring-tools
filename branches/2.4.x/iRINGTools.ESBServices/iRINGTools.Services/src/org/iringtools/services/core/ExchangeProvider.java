package org.iringtools.services.core;

import java.io.File;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;

import org.apache.commons.codec.digest.DigestUtils;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.directory.Scope;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.RoleType;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Graphs;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.manifest.TransferOption;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.mapping.ValueListMaps;
import org.iringtools.utility.AppDataComparator;
import org.iringtools.utility.CommodityComparator;
import org.iringtools.utility.ExchangeComparator;
import org.iringtools.utility.GraphComparator;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.utility.ScopeComparator;

public class ExchangeProvider {
	public static final String POOL_PREFIX = "_pool_";
	public static final String CHANGE_SEPARATOR = "->";

	private static final Logger logger = Logger
			.getLogger(ExchangeProvider.class);

	private Map<String, Object> settings;
	private HttpClient httpClient = null;

	private String sourceUri = null;
	private String sourceScopeName = null;
	private String sourceAppName = null;
	private String sourceGraphName = null;
	private String targetUri = null;
	private String targetScopeName = null;
	private String targetAppName = null;
	private String targetGraphName = null;

	private static ConcurrentMap<String, RequestStatus> requests = new ConcurrentHashMap<String, RequestStatus>();

	public ExchangeProvider(Map<String, Object> settings)
			throws ServiceProviderException {
		this.settings = settings;
		this.httpClient = new HttpClient();

		HttpUtils.addHttpHeaders(settings, httpClient);
	}

	public Directory getDirectory() throws ServiceProviderException {
		logger.debug("getDirectory()");

		String path = settings.get("baseDirectory")
				+ "/WEB-INF/data/directory.xml";

		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				// Sorting Scopes...
				List<Scope> directorytList = directory.getItems();
				ScopeComparator scopeCompare = new ScopeComparator();
				Collections.sort(directorytList, scopeCompare);

				for (Scope scope : directorytList) {
					ApplicationData appData = scope.getApplicationData();

					if (appData != null) {
						// sorting Application Data
						List<Application> AppDataList = appData.getItems();
						AppDataComparator appCompare = new AppDataComparator();
						Collections.sort(AppDataList, appCompare);

						for (Application app : AppDataList) {
							// sorting graphs
							List<org.iringtools.directory.Graph> graphList = app
									.getGraphs().getItems();
							GraphComparator graphCompare = new GraphComparator();
							Collections.sort(graphList, graphCompare);

						}
					}
					DataExchanges exchangeData = scope.getDataExchanges();
					// sorting exchangeData
					List<Commodity> commodityList = exchangeData.getItems();
					CommodityComparator commodityCompare = new CommodityComparator();
					Collections.sort(commodityList, commodityCompare);

					for (Commodity commodity : commodityList) {
						// sorting exchanges
						List<Exchange> exchangeList = commodity.getExchanges()
								.getItems();
						ExchangeComparator exchangeCompare = new ExchangeComparator();
						Collections.sort(exchangeList, exchangeCompare);
					}
				}
				// write it back JaxbUtils.write(dir, path);
				JaxbUtils.write(directory, path, false);
				return directory;
			}

			Directory directory = new Directory();
			JaxbUtils.write(directory, path, false);
			return directory;
		} catch (Exception e) {
			String message = "Error getting exchange definitions: " + e;
			logger.error(message);
			throw new ServiceProviderException(message);
		}
	}

	public ExchangeDefinition getExchangeDefinition(String scope, String id)
			throws ServiceProviderException {
		String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-"
				+ scope + "-" + id + ".xml";
		try {
			return JaxbUtils.read(ExchangeDefinition.class, path);
		} catch (Exception e) {
			String message = "Error getting exchange definition of [" + scope
					+ "." + id + "]: " + e;
			logger.error(message);
			throw new ServiceProviderException(message);
		}
	}

	public DataFilter getDataFilter(String scope, String id)
			throws ServiceProviderException {
		String path = settings.get("baseDirectory") + "/WEB-INF/data/Filter-"
				+ scope + "-" + id + ".xml";
		File file = new File(path);

		if (file.exists()) {
			try {
				return JaxbUtils.read(DataFilter.class, path);
			} catch (Exception e) {
				String message = "Error getting Data Filter of [" + scope + "."
						+ id + "]: " + e;
				logger.error(message);
				throw new ServiceProviderException(message);
			}
		} else {
			return new DataFilter();
		}
	}

	public Manifest getManifest(String scope, String id)
			throws ServiceProviderException {
		logger.debug("getManifest(" + scope + "," + id + ")");
		initExchangeDefinition(scope, id);
		return createCrossedManifest();
	}

	public DataTransferIndices getDataTransferIndices(String scope, String id,
			DxiRequest dxiRequest, boolean dtiOnly)
			throws ServiceProviderException {
		
		try {
			ExchangeDefinition xdef = getExchangeDefinition(scope, id);
			ExecutorService executor = Executors.newSingleThreadExecutor();
	
			DtiTask dtiTask = new DtiTask(settings, xdef, dxiRequest, dtiOnly, null);
			executor.execute(dtiTask);
			executor.shutdown();
	
			executor.awaitTermination(60, TimeUnit.MINUTES);
			
			DataTransferIndices dtis = dtiTask.getDataTransferIndices();
			return dtis;
		}
		catch (Exception e) {
			logger.error(e);
			throw new ServiceProviderException(e.getMessage());
		}
	}

	public Response processDxiRequest(String scope, String id,
			DxiRequest dxiRequest, boolean dtiOnly)
			throws ServiceProviderException {
		ExchangeDefinition xdef = getExchangeDefinition(scope, id);
		ExecutorService executor = Executors.newSingleThreadExecutor();

		if (isAsync()) {
			String requestId = UUID.randomUUID().toString().replace("-", "");
			RequestStatus requestStatus = new RequestStatus();
			requestStatus.setState(State.IN_PROGRESS);
			requests.put(requestId, requestStatus);

			DtiTask dtiTask = new DtiTask(settings, xdef, dxiRequest, dtiOnly,
					requestStatus);
			executor.execute(dtiTask);
			executor.shutdown();

			String statusURL = "/requests/" + requestId;
			return Response.status(Status.ACCEPTED)
					.header("location", statusURL).build();
		} else {
			DataTransferIndices dtis = getDataTransferIndices(scope, id,
					dxiRequest, dtiOnly);
			return Response.ok().entity(dtis).build();
		}
	}

	public DataTransferObjects getDataTransferObjects(String scope, String id,
			DxoRequest dxoRequest) throws ServiceProviderException {
		ExchangeDefinition xdef = getExchangeDefinition(scope, id);
		ExecutorService executor = Executors.newSingleThreadExecutor();

		DtoTask dtoTask = new DtoTask(settings, xdef, dxoRequest, null);
		executor.execute(dtoTask);
		executor.shutdown();

		try {
			executor.awaitTermination(60, TimeUnit.MINUTES);
		} catch (InterruptedException e) {
			logger.error(e);
			throw new ServiceProviderException(e.getMessage());
		}

		DataTransferObjects dtos = dtoTask.getDataTransferObjects();
		return dtos;
	}

	public Response processDxoRequest(String scope, String id,
			DxoRequest dxoRequest) throws ServiceProviderException {
		ExchangeDefinition xdef = getExchangeDefinition(scope, id);
		ExecutorService executor = Executors.newSingleThreadExecutor();

		if (isAsync()) {
			String requestId = UUID.randomUUID().toString().replace("-", "");
			RequestStatus requestStatus = new RequestStatus();
			requestStatus.setState(State.IN_PROGRESS);
			requests.put(requestId, requestStatus);

			DtoTask dtoTask = new DtoTask(settings, xdef, dxoRequest,
					requestStatus);
			executor.execute(dtoTask);
			executor.shutdown();

			String statusURL = "/requests/" + requestId;
			return Response.status(Status.ACCEPTED)
					.header("location", statusURL).build();
		} else {
			DataTransferObjects dtos = getDataTransferObjects(scope, id,
					dxoRequest);
			return Response.ok().entity(dtos).build();
		}
	}

	public DataTransferObjects getDataTransferObjectsFiltered(String scope,
			String id, int start, int limit, boolean sync, boolean add,
			boolean change, boolean delete, DataFilter filter)
			throws ServiceProviderException {
		logger.debug("getDataTransferObjectsFiltered(" + scope + ", " + id
				+ ", " + start + ", " + limit + ", " + sync + ", " + add + ", "
				+ change + ", " + delete + ", " + ", filter)");

		Manifest manifest = null;
		DataTransferIndices dtis = null;
		DxiRequest dxiRequest = new DxiRequest();
		ExchangeResponse xr = new ExchangeResponse();

		try {
			manifest = getManifest(scope, id);

			dxiRequest.setManifest(manifest);
			dxiRequest.setDataFilter(filter);
			dtis = getDataTransferIndices(scope, id, dxiRequest, false);

			int itemCount = 0;
			int itemCountSync = 0;
			int itemCountAdd = 0;
			int itemCountChange = 0;
			int itemCountDelete = 0;

			DataTransferIndices actionDtis = new DataTransferIndices();
			DataTransferIndexList actionDtiList = new DataTransferIndexList();
			actionDtis.setDataTransferIndexList(actionDtiList);
			List<DataTransferIndex> actionDtiListItems = actionDtiList
					.getItems();

			// Depending on the 'actions' we'll limit the dti's we send to the
			// exchange request
			for (DataTransferIndex dxi : dtis.getDataTransferIndexList()
					.getItems()) {
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
					if ((itemCount > start) && (itemCount <= start + limit))// get
																			// just
																			// the
																			// requested
																			// page
																			// of
																			// changes
					{
						actionDtiListItems.add(dxi);
					}
				}
			}

			DxoRequest dxoRequest = new DxoRequest();
			dxoRequest.setManifest(manifest);
			dxoRequest.setDataTransferIndices(actionDtis);

			DataTransferObjects dtos = getDataTransferObjects(scope, id,
					dxoRequest);

			xr.setExchangeId(id);
			xr.setSenderUri(sourceUri);
			xr.setSenderScope(sourceScopeName);
			xr.setSenderApp(sourceAppName);
			xr.setSenderGraph(sourceGraphName);
			xr.setReceiverUri(targetUri);
			xr.setReceiverScope(targetScopeName);
			xr.setReceiverApp(targetAppName);
			xr.setReceiverGraph(targetGraphName);
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

			dtos.setSummary(xr);
			dtos.setVersion(id);
			dtos.setSenderAppName(sourceAppName);
			dtos.setSenderScopeName(sourceScopeName);
			dtos.setAppName(targetAppName);
			dtos.setScopeName(targetScopeName);

			return dtos;
		} catch (Exception e) {
			throw new ServiceProviderException(e.getMessage());
		}
	}

	public Response getDifferencesSummary(String scope, String id,
			DataFilter filter) {
		logger.debug("getDifferencesSummary(" + scope + ", " + id + ", filter)");

		Manifest manifest = null;
		DataTransferIndices dtis = null;
		DxiRequest dxiRequest = new DxiRequest();

		try {
			manifest = getManifest(scope, id);

			dxiRequest.setManifest(manifest);
			dxiRequest.setDataFilter(filter);
			dtis = getDataTransferIndices(scope, id, dxiRequest, false);

			int iCountSync = 0;
			int iCountAdd = 0;
			int iCountChange = 0;
			int iCountDelete = 0;
			for (DataTransferIndex dxi : dtis.getDataTransferIndexList()
					.getItems()) {
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
			xRes.setSenderUri(sourceUri);
			xRes.setSenderScope(sourceScopeName);
			xRes.setSenderApp(sourceAppName);
			xRes.setSenderGraph(sourceGraphName);
			xRes.setReceiverUri(targetUri);
			xRes.setReceiverScope(targetScopeName);
			xRes.setReceiverApp(targetAppName);
			xRes.setReceiverGraph(targetGraphName);
			// <xs:element name="startTime" type="xs:dateTime" />
			// <xs:element name="endTime" type="xs:dateTime" />
			xRes.setLevel(Level.WARNING);
			xRes.setItemCount(iCountSync + iCountAdd + iCountChange
					+ iCountDelete);
			xRes.setItemCountSync(iCountSync);
			xRes.setItemCountAdd(iCountAdd);
			xRes.setItemCountChange(iCountChange);
			xRes.setItemCountDelete(iCountDelete);
			xRes.setSummary("Difference Summary only, this was not a data exchange request.");
			return Response.ok().entity(xRes).build();
		} catch (Exception e) {
			return Response.serverError().entity(e.getMessage()).build();
		}
	}

	public Response submitExchange(String scope, String id, ExchangeRequest xReq) {
		logger.debug("submitExchange(" + scope + ", " + id
				+ ", exchangeRequest)");

		String directoryServiceUrl = settings.get("directoryServiceUri") + "/"
				+ scope + "/exchanges/" + id;
		ExchangeDefinition xDef;

		try {
			xDef = httpClient
					.get(ExchangeDefinition.class, directoryServiceUrl);
		} catch (HttpClientException e) {
			ExchangeResponse exchangeResponse = new ExchangeResponse();
			logger.error(e.getMessage());
			exchangeResponse.setLevel(Level.ERROR);
			exchangeResponse.setSummary(e.getMessage());
			return Response.ok().entity(exchangeResponse).build();
		}

		if (isAsync()) {
			String requestId = UUID.randomUUID().toString().replace("-", "");
			RequestStatus requestStatus = new RequestStatus();
			requestStatus.setState(State.IN_PROGRESS);
			requests.put(requestId, requestStatus);

			ExecutorService executor = Executors.newSingleThreadExecutor();
			ExchangeTask exchangeTask = new ExchangeTask(settings, scope, id,
					xReq, xDef, requestStatus);
			executor.execute(exchangeTask);
			executor.shutdown();

			String statusURL = "/requests/" + requestId;
			return Response.status(Status.ACCEPTED)
					.header("location", statusURL).build();
		} else {
			ExecutorService executor = Executors.newSingleThreadExecutor();
			ExchangeTask exchangeTask = new ExchangeTask(settings, scope, id,
					xReq, xDef, null);
			executor.execute(exchangeTask);
			executor.shutdown();

			try {
				executor.awaitTermination(24, TimeUnit.HOURS);
			} catch (InterruptedException e) {
				logger.error("Exchange Task Executor interrupted: "
						+ e.getMessage());
				return Response.serverError().entity(e.getMessage()).build();
			}

			ExchangeResponse exchangeResponse = exchangeTask
					.getExchangeResponse();
			return Response.ok().entity(exchangeResponse).build();
		}
	}

	public RequestStatus getRequestStatus(String id) {
		RequestStatus requestStatus = null;

		try {
			if (requests.containsKey(id)) {
				requestStatus = requests.get(id);
			} else {
				requestStatus = new RequestStatus();
				requestStatus.setState(State.NOT_FOUND);
				requestStatus.setMessage("Request [" + id + "] not found.");
			}

			if (requestStatus.getState() == State.COMPLETED) {
				requests.remove(id);
			}
		} catch (Exception e) {
			requestStatus.setState(State.ERROR);
			requestStatus.setMessage(e.getMessage());
			requests.remove(id);
		}

		return requestStatus;
	}

	private void initExchangeDefinition(String scope, String id)
			throws ServiceProviderException {
		ExchangeDefinition xdef = getExchangeDefinition(scope, id);

		sourceUri = xdef.getSourceUri();
		sourceScopeName = xdef.getSourceScopeName();
		sourceAppName = xdef.getSourceAppName();
		sourceGraphName = xdef.getSourceGraphName();

		targetUri = xdef.getTargetUri();
		targetScopeName = xdef.getTargetScopeName();
		targetAppName = xdef.getTargetAppName();
		targetGraphName = xdef.getTargetGraphName();
	}

	public String md5Hash(DataTransferObject dataTransferObject) {
		StringBuilder values = new StringBuilder();

		List<ClassObject> classObjects = dataTransferObject.getClassObjects()
				.getItems();
		for (ClassObject classObject : classObjects) {
			List<TemplateObject> templateObjects = classObject
					.getTemplateObjects().getItems();
			for (TemplateObject templateObject : templateObjects) {
				List<RoleObject> roleObjects = templateObject.getRoleObjects()
						.getItems();
				for (RoleObject roleObject : roleObjects) {
					RoleType roleType = roleObject.getType();

					if (roleType == null
							|| // bug in v2.0 of c# service
							roleType == RoleType.PROPERTY
							|| roleType == RoleType.OBJECT_PROPERTY
							|| roleType == RoleType.DATA_PROPERTY
							|| roleType == RoleType.FIXED_VALUE
							|| (roleType == RoleType.REFERENCE
									&& roleObject.getRelatedClassId() != null && // self-join
									roleObject.getValue() != null && !roleObject
									.getValue().startsWith("#"))) {
						String value = roleObject.getValue();

						if (value != null)
							values.append(value);
					}
				}
			}
		}

		return DigestUtils.md5Hex(values.toString());
	}

	private Manifest createCrossedManifest() throws ServiceProviderException {
		Manifest crossedManifest = new Manifest();

		String sourceManifestUrl = sourceUri + "/" + sourceScopeName + "/"
				+ sourceAppName + "/" + sourceGraphName + "/manifest";

		String targetManifestUrl = targetUri + "/" + targetScopeName + "/"
				+ targetAppName + "/" + targetGraphName + "/manifest";

		ExecutorService executor = Executors.newFixedThreadPool(2);

		ManifestTask sourceManifestTask = new ManifestTask(settings,
				sourceManifestUrl);
		executor.execute(sourceManifestTask);

		ManifestTask targetManifestTask = new ManifestTask(settings,
				targetManifestUrl);
		executor.execute(targetManifestTask);

		executor.shutdown();

		try {
			executor.awaitTermination(Long.parseLong((String) settings
					.get("manifestTaskTimeout")), TimeUnit.SECONDS);
		} catch (InterruptedException e) {
			logger.error("Manifest Task Executor interrupted: "
					+ e.getMessage());
		}

		Manifest sourceManifest = sourceManifestTask.getManifest();
		Manifest targetManifest = targetManifestTask.getManifest();

		if (targetManifest == null
				|| targetManifest.getGraphs().getItems().size() == 0)
			return null;

		Graph sourceGraph = getGraph(sourceManifest, sourceGraphName);
		Graph targetGraph = getGraph(targetManifest, targetGraphName);

		if (sourceGraph != null && sourceGraph.getClassTemplatesList() != null
				&& targetGraph != null
				&& targetGraph.getClassTemplatesList() != null) {
			Graphs crossGraphs = new Graphs();
			crossGraphs.getItems().add(targetGraph);
			crossedManifest.setGraphs(crossGraphs);

			List<ClassTemplates> sourceClassTemplatesList = sourceGraph
					.getClassTemplatesList().getItems();
			List<ClassTemplates> targetClassTemplatesList = targetGraph
					.getClassTemplatesList().getItems();

			for (int i = 0; i < targetClassTemplatesList.size(); i++) {
				org.iringtools.dxfr.manifest.Class targetClass = targetClassTemplatesList
						.get(i).getClazz();
				ClassTemplates sourceClassTemplates = getClassTemplates(
						sourceClassTemplatesList, targetClass.getId());

				if (sourceClassTemplates != null
						&& sourceClassTemplates.getTemplates() != null) {
					List<Template> targetTemplates = targetClassTemplatesList
							.get(i).getTemplates().getItems();
					List<Template> sourceTemplates = sourceClassTemplates
							.getTemplates().getItems();

					for (int j = 0; j < targetTemplates.size(); j++) {
						Template targetTemplate = targetTemplates.get(j);
						Template sourceTemplate = getTemplate(sourceTemplates,
								targetTemplate.getId());

						if (sourceTemplate == null) {
							if (targetTemplate.getTransferOption() == TransferOption.REQUIRED) {
								throw new ServiceProviderException(
										"Required template ["
												+ targetTemplate.getId()
												+ "] not found");
							} else {
								targetTemplates.remove(j--);
							}
						} else if (targetTemplate.getRoles() != null
								&& sourceTemplate.getRoles() != null) {
							List<Role> targetRoles = targetTemplate.getRoles()
									.getItems();
							List<Role> sourceRoles = sourceTemplate.getRoles()
									.getItems();

							for (int k = 0; k < targetRoles.size(); k++) {
								Role sourceRole = getRole(sourceRoles,
										targetRoles.get(k).getId());

								if (sourceRole == null) {
									targetRoles.remove(k--);
								}
							}
						}
					}
				} else {
					targetClassTemplatesList.remove(i--);
				}
			}
		}

		// add source and target value-list maps
		ValueListMaps valueListMaps = new ValueListMaps();

		if (sourceManifest.getValueListMaps() != null) {
			valueListMaps.getItems().addAll(
					sourceManifest.getValueListMaps().getItems());
		}

		if (targetManifest.getValueListMaps() != null) {
			valueListMaps.getItems().addAll(
					targetManifest.getValueListMaps().getItems());
		}

		crossedManifest.setValueListMaps(valueListMaps);

		return crossedManifest;
	}

	private Graph getGraph(Manifest manifest, String graphName) {
		for (Graph graph : manifest.getGraphs().getItems()) {
			if (graph.getName().equalsIgnoreCase(graphName))
				return graph;
		}

		return null;
	}

	private ClassTemplates getClassTemplates(
			List<ClassTemplates> classTemplatesList, String classId) {
		for (ClassTemplates classTemplates : classTemplatesList) {
			org.iringtools.dxfr.manifest.Class clazz = classTemplates
					.getClazz();

			if (clazz.getId().equalsIgnoreCase(classId)) {
				return classTemplates;
			}
		}

		return null;
	}

	private Template getTemplate(List<Template> templates, String templateId) {
		for (Template template : templates) {
			if (template.getId().equals(templateId))
				return template;
		}

		return null;
	}

	private Role getRole(List<Role> roles, String roleId) {
		for (Role role : roles) {
			if (role.getId().equals(roleId))
				return role;
		}

		return null;
	}

	private boolean isAsync() {
		String asyncHeader = "http-header-async";
		boolean async = settings.containsKey(asyncHeader)
				&& Boolean.parseBoolean(settings.get(asyncHeader).toString());

		return async;
	}
}
