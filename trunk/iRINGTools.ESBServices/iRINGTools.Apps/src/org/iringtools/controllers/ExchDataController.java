package org.iringtools.controllers;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.history.History;
import org.iringtools.models.ExchDataModel;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Rows;
import org.iringtools.ui.widgets.grid.GridDefinition;
import com.opensymphony.xwork2.Action;
import com.opensymphony.xwork2.ActionSupport;

import java.util.Map;

public class ExchDataController extends ActionSupport implements SessionAware 
{
	private static final long serialVersionUID = 1L;

	private Map<String, Object> session;
	
	private ExchDataModel exchdata;
	private Grid grid;
	private Rows rows;
	private GridDefinition gridDefinition;
	
	
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
	
	public void setGridDefinition(GridDefinition value) {
		this.gridDefinition = value;
	}

	public GridDefinition getGridDefinition() {
		return gridDefinition;
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
		
		if (session.get(key) == null)
			session.put(key, exchdata.populate(scopeName, idName));
		else {
			exchdata.setDtiList(((DataTransferIndices)session.get(key)).getDataTransferIndexList().getItems());
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

	public void CleanDtiMap() {
		session.remove(key);
	}
	
	public String cleanHashMap() {	
		key = scopeName + idName;
		CleanDtiMap();		
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
		CleanDtiMap();		
		return Action.SUCCESS;
	}
	
	public void prePareHistory() {
		exchdata.getHistoryUrl();
		exchdata.setDtoUrl("/" + scopeName + "/exchanges/" + idName);
	}
	
	public String getExchangeHistory() {
		prePareHistory();
		gridDefinition = exchdata.getExchHistory();
		return Action.SUCCESS;
	}
	
	public String getExchangeHistoryDetail() {
		prePareHistory();
		gridDefinition = exchdata.getExchHistoryDetail(historyId);
		return Action.SUCCESS;
	}	
	
	public String showExchangeHistory() {
		
		prePareHistory();
		history = exchdata.showExchHistory();
		return Action.SUCCESS;
	}

	@Override
	public void setSession(Map<String, Object> session) {
		this.session = session;		
	}
}
