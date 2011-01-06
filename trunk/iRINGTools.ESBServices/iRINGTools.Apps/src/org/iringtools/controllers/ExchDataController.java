package org.iringtools.controllers;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.models.ExchDataModel;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Rows;
import com.opensymphony.xwork2.Action;

import java.util.HashMap;

public class ExchDataController {

	private ExchDataModel exchdata;
	private Grid grid;
	private Rows rows;
	private ExchangeResponse exchangeResponse;
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
	
	static private HashMap<String, DataTransferIndices> dtiMap = null;
	static private HashMap<String, DataTransferIndices> dtoMap = null;

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
	
	public void setExchangeResponse(ExchangeResponse exchangeResponse) {
		this.exchangeResponse = exchangeResponse;
	}

	public ExchangeResponse getExchangeResponse() {
		return exchangeResponse;
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
		exchangeResponse = exchdata.toExResponse(hasReviewed);
		dtiMap.put(key, null);
		return Action.SUCCESS;
	}
	
	public String getExchangeHistory() {
		exchdata.getHistoryUrl();
		exchdata.setDtoUrl("/" + scopeName + "/exchanges/" + idName);
		history = exchdata.getExchHistory();
		return Action.SUCCESS;
	}
}
