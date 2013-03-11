package org.iringtools.controllers;

import java.util.Map;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.models.AppdataModel;
import org.iringtools.models.DataFilterContainer;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Rows;

import com.opensymphony.xwork2.Action;
import com.opensymphony.xwork2.ActionSupport;

public class AppdataController extends ActionSupport implements SessionAware 
{	
	private static final long serialVersionUID = 1L;
	private Map<String, Object> session;
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
	private String filter; 
	private String sort;
	private String dir;
	private DataFilterContainer dataFilterContainer;
	
	


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
	
	public void setFilter(String filter) {
		
		this.filter = filter;
		
	}

	public String getFilter() {
		return filter;
	}	
	
	public void setDir(String dir) {
		
		this.dir = dir;
		
	}

	public String getDir() {
		return dir;
	}
	
	public void setSort(String sort) {
		
		this.sort = sort;
		
	}

	public String getSort() {
		return sort;
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
		key = scopeName + appName + graphName + filter + sort + dir;		
		dataFilterContainer = new DataFilterContainer(filter, dir, sort);
		if (session.get(key) == null)
			session.put(key, appdata.populateFilter(scopeName, appName, graphName, dataFilterContainer.getDataFilter()));
		else {
			appdata.setDtiList(((DataTransferIndices)session.get(key)).getDataTransferIndexList().getItems());
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

	public void CleanDtiMap() {
		session.remove(key);
	}	

	public String cleanHashMap() {
		key = scopeName + appName + graphName;
		CleanDtiMap();
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

	@Override
	public void setSession(Map<String, Object> session) {
		this.session = session;		
	}
}