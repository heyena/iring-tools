package org.iringtools.controllers;

import org.iringtools.models.ExchDataModel;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Rows;
import com.opensymphony.xwork2.Action;

import java.util.HashMap;

public class ExchDataController {

	private ExchDataModel exchdata;
	private Grid grid;
	private Rows rows;
	private String scopeName;
	private String idName;
	private String id;
	private String classId;
    private String relatedId;
	private int start=0;
	private int limit=20;
	private HashMap<String, Rows> rowsMap = null;

	public ExchDataController() {
		exchdata = new ExchDataModel();
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



	public void setScopeName(String scopeName) {
		this.scopeName = scopeName;
	}

	public String getScopeName() {
		return scopeName;
	}

	

	public void setIdName(String idName) {
		this.idName = idName;
	}

	public String getIdName() {
		return idName;
	}
	
	public void setId(String id) {
		this.id = id;
	}

	public String getId() {
		return id;
	}
	
	public void setClassId(String classId) {
		this.classId = classId;
	}

	public String gettClassId() {
		return classId;
	}

	public void setRelatedId(String relatedId) {
		this.relatedId = relatedId;
	}

	public String getRelatedId() {
		return relatedId;
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

	public String getExchDataGrid() {
		exchdata.populate(scopeName, idName);
		grid = exchdata.toGrid();
		return Action.SUCCESS;
	}

	public String getExchDataRows() {
		if (rowsMap == null)
			rowsMap = new HashMap<String, Rows>();
		if (rowsMap.size() <= start / limit) {
			exchdata.populate(scopeName, idName);
			rows = exchdata.toRows(start, limit);
			rowsMap.put(String.valueOf(start), rows);
		} else {
			rows = rowsMap.get(String.valueOf(start));
		}
		return Action.SUCCESS;
	}

	public String cleanHashMap() {
		if (rowsMap != null)
			rowsMap.clear();
		ExchDataModel.setURI("");		
		return Action.SUCCESS;
	}


	
	public String getRelatedExchRows() {		
		exchdata.populate(scopeName, idName);
		rows = exchdata.toRelRows(id);		
		return Action.SUCCESS;
	}

	public String getDetailExchGrid() {
		exchdata.populate(scopeName, idName);
		grid = exchdata.toDetailRelGrid(id, classId);
		return Action.SUCCESS;
	}
	
	public String getDetailExchRows() {
		exchdata.populate(scopeName, idName);
		rows = exchdata.toDetailRelRows(id, classId);
		return Action.SUCCESS;
	}
	
	public String getRelRelationExchRows() {
		exchdata.populate(scopeName, idName);
		rows = exchdata.toDetailRelRows(id, classId, relatedId);
		return Action.SUCCESS;
	}
	
}
