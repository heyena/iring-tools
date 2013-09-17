package org.iringtools.controllers;

import java.util.Iterator;
import java.util.Map;
import java.util.Map.Entry;

import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.directory.Directory;
import org.iringtools.models.DataModel;
import org.iringtools.models.DirectoryModel;
import org.iringtools.utility.IOUtils;
import org.iringtools.widgets.tree.Tree;

public class DirectoryController extends BaseController {
	private static final long serialVersionUID = 1L;

	private Tree directoryTree;
	private String dtoContext;
	private String dxfrUri, scope, app, graph, cacheUri;
	private Response refreshResult, importResult;

	public DirectoryController() throws Exception {
		super();
		authorize("exchangeAdmins");
	}

	public String getDirectory() throws Exception {
		try {
			Iterator<Entry<String, Object>> iterator = session.entrySet()
					.iterator();

			while (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				String key = entry.getKey();

				if (key.startsWith(DataModel.APP_PREFIX)
						|| key.startsWith(DataModel.EXCHANGE_PREFIX)) {
					iterator.remove();
				}
			}

			DirectoryModel directoryModel = new DirectoryModel(settings);
			Directory directory = directoryModel.getDirectory();

			session.put(DataModel.DIRECTORY_KEY, directory);
			directoryTree = directoryModel.directoryToTree(directory);
		} catch (Exception e) {
			e.printStackTrace();
			throw new Exception(e.getMessage());
		}

		return SUCCESS;
	}

	public Tree getDirectoryTree() {
		return directoryTree;
	}

	public String resetDtoContext() {
		Map<String, String> map = IOUtils.splitQueryParams(dtoContext);

		String dtoContextKey = (map.containsKey("xid")) ? map.get("scope")
				+ "." + map.get("xid") : map.get("scope") + "."
				+ map.get("app") + "." + map.get("graph");

		for (String key : session.keySet()) {
			if (key.contains(dtoContextKey))
				session.remove(key);
		}

		return SUCCESS;
	}

	public void setDtoContext(String dtoContext) {
		this.dtoContext = dtoContext;
	}

	private void appendMessages(Response response) {
		if (response.getMessages() == null) {
			response.setMessages(new Messages());
		}

		if (response != null && response.getStatusList() != null) {
			for (Status status : response.getStatusList().getItems()) {
				if (status.getMessages() != null) {
					for (String message : status.getMessages().getItems()) {
						response.getMessages().getItems().add(message);
					}
				}
			}
		}
	}

	public String refreshCache() {
		DirectoryModel directoryModel = new DirectoryModel(settings);
		refreshResult = directoryModel.refreshCache(dxfrUri, scope, app, graph);
		appendMessages(refreshResult);

		return SUCCESS;
	}

	public String importCache() {
		DirectoryModel directoryModel = new DirectoryModel(settings);
		importResult = directoryModel.importCache(dxfrUri, scope, app, graph,
				cacheUri);
		appendMessages(importResult);

		return SUCCESS;
	}

	public Response getRefreshResult() {
		return refreshResult;
	}

	public Response getImportResult() {
		return importResult;
	}

	public String getDxfrUri() {
		return dxfrUri;
	}

	public void setDxfrUri(String dxfrUri) {
		if (!dxfrUri.endsWith("/")) {
			dxfrUri += "/";
		}

		this.dxfrUri = dxfrUri;
	}

	public String getScope() {
		return scope;
	}

	public void setScope(String scope) {
		this.scope = scope;
	}

	public String getApp() {
		return app;
	}

	public void setApp(String app) {
		this.app = app;
	}

	public String getGraph() {
		return graph;
	}

	public void setGraph(String graph) {
		this.graph = graph;
	}

	public String getCacheUri() {
		return cacheUri;
	}

	public void setCacheUri(String cacheUri) {
		if (!cacheUri.endsWith("/"))
			cacheUri += "/";

		this.cacheUri = cacheUri;
	}
}
