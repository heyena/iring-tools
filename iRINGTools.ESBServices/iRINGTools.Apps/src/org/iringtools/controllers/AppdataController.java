package org.iringtools.controllers;

import org.iringtools.models.AppdataModel;
import org.iringtools.grid.Grid;
import org.iringtools.grid.Rows;
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
	private int start;
	private int limit;
	static private HashMap<String,Rows> rowsMap = null;
	
	
	public AppdataController()
	{
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
	
	public String getAppDataGrid() {
		appdata.populate(scopeName, appName, graphName);
		grid = appdata.toGrid();
		return Action.SUCCESS;
	}
	
	public String getAppDataRows() {
		if (rowsMap == null)
			rowsMap = new HashMap<String,Rows>();
		if (rowsMap.size()<= start/limit) {
			appdata.populate(scopeName, appName, graphName);
			rows = appdata.toRows(start, limit);
			rowsMap.put(String.valueOf(start), rows);			
		}else {
			rows = rowsMap.get(String.valueOf(start));
		}
		return Action.SUCCESS;
	}
	
	public String cleanHashMap() {
		rowsMap.clear();
		return Action.SUCCESS;
	}
	
	
//	public String postAppDataGrid() {		
//		appdata.readGrid(grid);		
//        return Action.SUCCESS;
//	}
}
