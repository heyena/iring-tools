package org.iringtools.controllers;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.models.ExchDataModel;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.GridAndRows;
import org.iringtools.ui.widgets.grid.Rows;
import com.opensymphony.xwork2.Action;

import java.util.HashMap;

public class ExchDataController {

	private ExchDataModel exchdata;
	private Grid grid;
	private Rows rows;
	private GridAndRows gridAndRows;
	
	
	private History history;
	private String scopeName;
	private String idName;
	private String id;
	private String classId;
    private String relatedId;
	private int start=0;
	private int limit=20;
	private String key;
	private String hasReviewed;
	private String historyId;
	
	static private HashMap<String, DataTransferIndices> dtiMap = null;

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
	
	public void setGridAndRows(GridAndRows value) {
		this.gridAndRows = value;
	}

	public GridAndRows getGridAndRows() {
		return gridAndRows;
	}

	public void setHistory(History history) {
		this.history = history;
	}

	public History getHistory() {
		return history;
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
	
	public void setHistoryId(String value) {
		this.historyId = value;
	}

	public String getHistoryId() {
		return historyId;
	}
	
	public void setHasReviewed(String val) {
		this.hasReviewed = val;
	}

	public String getHasReviewed() {
		return hasReviewed;
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

	public void getExchDtiList() {
		key = scopeName + idName;
		if (dtiMap == null)
			dtiMap = new HashMap<String, DataTransferIndices>();
		if (dtiMap.get(key) == null)
			dtiMap.put(key, exchdata.populate(scopeName, idName));
		else {
			exchdata.setDtiList(dtiMap.get(key).getDataTransferIndexList().getItems());
			exchdata.setDtoUrl("/" + scopeName + "/exchanges/" + idName);
		}
	}
	
	public String getExchDataGrid() {
		getExchDtiList();
		grid = exchdata.toGrid();
		return Action.SUCCESS;
	}

	public String getExchDataRows() {
		getExchDtiList();		
		rows = exchdata.toRows(start, limit);		
		return Action.SUCCESS;
	}

	public String cleanHashMap() {			
		dtiMap.put(key, null);		
		rows = null;
		return Action.SUCCESS;
	}
	
	public String getRelatedExchRows() {		
		getExchDtiList();
		rows = exchdata.toRelRows(id);		
		return Action.SUCCESS;
	}

	public String getDetailExchGrid() {
		getExchDtiList();
		grid = exchdata.toDetailRelGrid(id, classId);
		return Action.SUCCESS;
	}
	
	public String getDetailExchRows() {
		getExchDtiList();
		rows = exchdata.toDetailRelRows(id, classId);
		return Action.SUCCESS;
	}
	
	public String getRelRelationExchRows() {
		getExchDtiList();
		rows = exchdata.toDetailRelRows(id, classId, relatedId);
		return Action.SUCCESS;
	}
	
	public String setExchangeData() {
		getExchDtiList();
		rows = exchdata.toExResponse(hasReviewed);
		dtiMap.put(key, null);
		return Action.SUCCESS;
	}
	
	public void prePareHistory() {
		exchdata.getHistoryUrl();
		exchdata.setDtoUrl("/" + scopeName + "/exchanges/" + idName);
	}
	
	public String getExchangeHistory() {
		prePareHistory();
		gridAndRows = exchdata.getExchHistory();
		return Action.SUCCESS;
	}
	
	public String getExchangeHistoryDetail() {
		prePareHistory();
		gridAndRows = exchdata.getExchHistoryDetail(historyId);
		return Action.SUCCESS;
	}	
	
	public String showExchangeHistory() {
		prePareHistory();
		history = exchdata.showExchHistory();
		return Action.SUCCESS;
	}
}
