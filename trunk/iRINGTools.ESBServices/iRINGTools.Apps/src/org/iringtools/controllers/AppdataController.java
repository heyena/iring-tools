package org.iringtools.controllers;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.models.AppdataModel;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Rows;
import com.opensymphony.xwork2.Action;

import java.util.HashMap;

public class AppdataController {

	private AppdataModel appdata;
	private Grid grid;
	private Rows rows;
	private String scopeName;
	private String appName;
	private String graphName;
	private String pageName;
	private String id;
	private String classId;
	private String relatedId;
	private String key;
	private int start=0;
	private int limit=20;
	static private HashMap<String, DataTransferIndices> dtiMap = null;

	public AppdataController() {
		appdata = new AppdataModel();
	}

	public void setGrid(Grid grid) {
		this.grid = grid;
	}

	public Grid getGrid() {
		return grid;
	}

	public void setRows(Rows rows) {
		this.rows = rows;
	}

	public Rows getRows() {
		return rows;
	}

	public void setPageName(String pageName) {
		this.pageName = pageName;
	}

	public String getPageName() {
		return pageName;
	}
	
	public void setClassId(String classId) {
		this.classId = classId;
	}

	public String getClassId() {
		return classId;
	}
	
	public void setRelatedId(String relatedId) {
		this.relatedId = relatedId;
	}

	public String getRelatedId() {
		return relatedId;
	}

	public void setScopeName(String scopeName) {
		this.scopeName = scopeName;
	}

	public String getScopeName() {
		return scopeName;
	}

	public void setAppName(String appName) {
		this.appName = appName;
	}

	public String getAppName() {
		return appName;
	}

	public void setGraphName(String graphName) {
		this.graphName = graphName;
	}

	public String getGraphName() {
		return graphName;
	}
	
	public void setId(String id) {
		this.id = id;
	}

	public String getId() {
		return id;
	}

	public void setStart(int start) {
		this.start = start;
	}

	public int getStart() {
		return start;
	}

	public void setLimit(int limit) {
		this.limit = limit;
	}

	public int getLimit() {
		return limit;
	}

	public void getExchDtiList() {
		key = scopeName + appName + graphName;
		if (dtiMap == null)
			dtiMap = new HashMap<String, DataTransferIndices>();
		if (dtiMap.get(key) == null)
			dtiMap.put(key, appdata.populate(scopeName, appName, graphName));
		else {
			appdata.setDtiList(dtiMap.get(key).getDataTransferIndexList().getItems());
			appdata.setDtoUrl("/" + scopeName + "/" + appName + "/" + graphName);
		}
	}
	
	public String getAppDataGrid() {
		getExchDtiList();
		grid = appdata.toGrid();
		return Action.SUCCESS;
	}

	public String getAppDataRows() {
		getExchDtiList();
		rows = appdata.toRows(start, limit);		
		return Action.SUCCESS;
	}

	public String cleanHashMap() {
		dtiMap.put(key, null);
		AppdataModel.setURI("");
		rows = null;
		return Action.SUCCESS;
	}
	
	public String getRelatedAppRows() {		
		appdata.populate(scopeName, appName, graphName);
		rows = appdata.toRelRows(id);		
		return Action.SUCCESS;
	}

	public String getDetailRelAppGrid() {
		appdata.populate(scopeName, appName, graphName);
		grid = appdata.toDetailRelGrid(id, classId);
		return Action.SUCCESS;
	}
	
	public String getDetailRelAppRows() {
		appdata.populate(scopeName, appName, graphName);
		rows = appdata.toDetailRelRows(id, classId);
		return Action.SUCCESS;
	}
	
	public String getRelRelationAppRows() {
		appdata.populate(scopeName, appName, graphName);
		rows = appdata.toRelRelationRows(id, classId, relatedId);
		return Action.SUCCESS;
	}

}
