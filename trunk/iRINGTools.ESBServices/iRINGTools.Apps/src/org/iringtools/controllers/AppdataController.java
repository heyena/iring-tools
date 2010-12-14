package org.iringtools.controllers;

import org.iringtools.models.AppdataModel;
import org.iringtools.grid.Grid;
import org.iringtools.rows.Rows;
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
	
	public String getAppdataModel() {
		appdata.populate(scopeName, appName, graphName);
		if (pageName.equals(""))
			grid = appdata.toGrid();
		else
			rows = appdata.toRows();
        return Action.SUCCESS;
	}
	
	public String postAppdataModel() {		
		appdata.readGrid(grid);		
        return Action.SUCCESS;
	}
}
