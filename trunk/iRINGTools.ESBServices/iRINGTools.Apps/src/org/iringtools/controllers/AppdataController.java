package org.iringtools.controllers;

import org.iringtools.models.AppdataModel;
import org.iringtools.grid.Grid;
import org.iringtools.grid.Rows;
import com.opensymphony.xwork2.Action;

public class AppdataController {
	
	private AppdataModel appdata;
	private Grid grid;
	private Rows rows;
	private String scopeName;
	private String appName;
	private String graphName;
	private String pageName;
	
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
	
	public String getAppDataGrid() {
		appdata.populate(scopeName, appName, graphName);
		grid = appdata.toGrid();
		return Action.SUCCESS;
	}
	
	public String getAppDataRows() {
		appdata.populate(scopeName, appName, graphName);
		rows = appdata.toRows();
		return Action.SUCCESS;
	}
	
	
	
//	public String postAppDataGrid() {		
//		appdata.readGrid(grid);		
//        return Action.SUCCESS;
//	}
}
