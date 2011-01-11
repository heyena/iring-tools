package org.iringtools.models;

import java.util.List;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.GridDefinition;
import org.iringtools.ui.widgets.grid.Rows;
import org.iringtools.utility.HttpClient;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.history.History;

import com.opensymphony.xwork2.ActionContext;

public class ExchDataModel {
	private Grid grid = null;
	private Rows rows = null;
	private GridDefinition gridDef = null;
	private String URI = "", historyURI = "";
	private String identifier;
	private String dtoUrl;
	private List<DataTransferIndex> dtiList = null;
	private List<DataTransferIndex> dtiPage = null;
	private DtoContainer dtoContainer;
	private ExchResponseContainer exchResponseContainer;
	private HistoryContainer historyContainer;
	private int rInd = 0;

	public ExchDataModel() {
		try {
			URI = ActionContext.getContext().getApplication()
					.get("ExchangeDataServiceUri").toString();
		} catch (Exception e) {
			System.out.println("Exception in ExchangeDataServiceUri :" + e);
		}
	}

	public void getHistoryUrl() {
		try {
			historyURI = ActionContext.getContext().getApplication()
					.get("HistoryServiceUri").toString();
		} catch (Exception e) {
			System.out.println("Exception in ExchangeDataServiceUri :" + e);
		}
	}

	public DataTransferIndices populate(String scopeName, String id) {
		DataTransferIndices dti = null;
		try {
			HttpClient httpClient = new HttpClient(URI);
			dti = httpClient.get(DataTransferIndices.class, "/" + scopeName
					+ "/exchanges/" + id);
			setDtiList(dti.getDataTransferIndexList().getItems());
			setDtoUrl("/" + scopeName + "/exchanges/" + id);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
		return dti;
	}

	public void setURI(String uri) {
		this.URI = uri;
	}

	public List<DataTransferIndex> getDtiList() {
		return dtiList;
	}

	public void setDtiList(List<DataTransferIndex> dtiList) {
		this.dtiList = dtiList;
	}

	public void setDtoUrl(String dtoUrl) {
		this.dtoUrl = dtoUrl;
	}

	public String getDtoUrl() {
		return dtoUrl;
	}

	public void setIdentifier(String identifier) {
		this.identifier = identifier;
	}

	public String getIdentifier() {
		return identifier;
	}

	public void setRInd(int rInd) {
		this.rInd = rInd;
	}

	public int getRInd() {
		return rInd;
	}

	public Grid getGrid() {
		return grid;
	}

	public void setGrid(Grid grid) {
		this.grid = grid;
	}

	public Grid toGrid() {
		grid = new Grid();
		dtoContainer = new DtoContainer();
		// DataTransferIndex dti = dtiList.get(0);
		dtiPage = dtiList.subList(0, 1);
		// setIdentifier(dti.getIdentifier());
		dtoContainer.setUrl(dtoUrl);
		dtoContainer.populatePage(URI, dtiPage);
		dtoContainer.initialHList();
		dtoContainer.fillExchConfig();
		dtoContainer.setGridList(grid);
		return grid;
	}

	public Rows toRows(int start, int limit) {
		dtoContainer = new DtoContainer();
		dtoContainer.setTotal(dtiList.size());
		dtiPage = dtiList.subList(start,
				Math.min(limit + start, dtiList.size() - 1));
		rows = new Rows();
		dtoContainer.setUrl(dtoUrl);
		dtoContainer.populatePage(URI, dtiPage);
		dtoContainer.initialDataList();
		dtoContainer.fillExchPage();
		dtoContainer.setRowsList(rows);
		return rows;
	}

	public int getDti(String id) {
		for (DataTransferIndex dti : dtiList) {
			if (dti.getIdentifier().equals(id))
				return dtiList.indexOf(dti);
		}
		return -1;
	}

	public void SetDtiPage(String id) {
		int ind;
		ind = getDti(id);
		dtiPage = dtiList.subList(ind, ind + 1);
	}

	public Rows toRelRows(String id) {
		prepareRows(id);
		dtoContainer.fillRelPage();
		dtoContainer.setRowsList(rows);
		return rows;
	}

	public Grid toDetailRelGrid(String id, String classId) {
		prepareGrid(id);
		dtoContainer.setClassId(classId);
		dtoContainer.fillExchDetailRelConfig();
		dtoContainer.setGridList(grid);
		return grid;
	}

	public Rows toDetailRelRows(String id, String classId) {
		prepareRows(id);
		dtoContainer.setClassId(classId);
		dtoContainer.initialHList();
		dtoContainer.fillExchDetailRelPage();
		dtoContainer.setRowsList(rows);
		return rows;
	}

	public void prepareGrid(String id) {
		grid = new Grid();
		dtoContainer = new DtoContainer();
		SetDtiPage(id);
		dtoContainer.setUrl(dtoUrl);
		dtoContainer.populatePage(URI, dtiPage);
		dtoContainer.initialHList();
	}

	public void prepareRows(String id) {
		rows = new Rows();
		dtoContainer = new DtoContainer();
		SetDtiPage(id);
		dtoContainer.setUrl(dtoUrl);
		dtoContainer.populatePage(URI, dtiPage);
		dtoContainer.initialDataList();
	}
	            
	public Rows toDetailRelRows(String id, String classId, String relatedId) {
		prepareRows(id);
		dtoContainer.setClassId(classId);
		dtoContainer.setRelatedId(relatedId);
		dtoContainer.fillRelRelationPage();
		dtoContainer.setRowsList(rows);
		return rows;
	}

	public Rows toExResponse(String hasReviewed) {
		rows = new Rows();		
		exchResponseContainer = new ExchResponseContainer();
		exchResponseContainer.setResponseUrl(dtoUrl + "/submit");
		exchResponseContainer.setExchangeRequest(dtiList, hasReviewed);
		exchResponseContainer.populateResponse(URI);
		exchResponseContainer.setRows(rows);
		return rows;
	}

	public void prepareHistory() {
		gridDef = new GridDefinition();		
		historyContainer = new HistoryContainer();
		historyContainer.setHistoryUrl(dtoUrl);
		historyContainer.populateHistory(historyURI);
		historyContainer.initialList(gridDef);
	}
	
	public GridDefinition getExchHistory() {
		prepareHistory();
		historyContainer.setHeaderList();
		historyContainer.setGridAndRows(gridDef);
		historyContainer.setGridList(gridDef, false);			
		return gridDef;
	}

	public GridDefinition getExchHistoryDetail(String historyId) {
		prepareHistory();
		historyContainer.setDetailHeaderList();
		historyContainer.setGridList(gridDef, true);
		historyContainer.setDetailGridAndRows(gridDef, historyId);		
		return gridDef;
	}
	
	public History showExchHistory() {
		historyContainer = new HistoryContainer();
		historyContainer.setHistoryUrl(dtoUrl);
		historyContainer.populateHistory(historyURI);
		return historyContainer.getHistory();
	}
	
	public void readGrid(Grid grid) {
		// TODO: Read the grid!

	}

}
