package org.iringtools.models;

import java.util.List;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Rows;
import org.iringtools.utility.HttpClient;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.DataTransferIndex;
import com.opensymphony.xwork2.ActionContext;

public class AppdataModel {

	private Grid grid = null;
	private Rows rows = null;
	private String URI="";		
	private String dtoUrl;	
	private List<DataTransferIndex> dtiList=null;
	private List<DataTransferIndex> dtiPage=null;
	
	private DtoContainer dtoCtr=null;
	private int rInd=0;
	
	public AppdataModel() {

		try {
			URI = ActionContext.getContext().getApplication()
					.get("AppDataServiceUri").toString();
		} catch (Exception e) {
			System.out.println("Exception in AppDataServiceUri :" + e);
		}
		grid = null;

	}
	
	public DataTransferIndices populate(String scopeName, String appName, String graphName) {
		DataTransferIndices dti = null;
		try {
			HttpClient httpClient = new HttpClient(URI);
			dti = httpClient.get(DataTransferIndices.class,
					"/" + scopeName + "/" + appName + "/" + graphName);
			setDtiList(dti.getDataTransferIndexList().getItems());
			setDtoUrl("/" + scopeName + "/" + appName + "/" + graphName);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
		return dti;
	}

	
	public DataTransferIndices populateFilter(String scopeName, String appName, String graphName, 
			                                  DataFilter dataFilter) {
		DataTransferIndices dti = null;
		try {
			DataFilter theDataFilter = new DataFilter();
			theDataFilter.setExpressions(dataFilter.getExpressions());
			theDataFilter.setOrderExpressions(dataFilter.getOrderExpressions());
			
			HttpClient httpClient = new HttpClient(URI);
			dti = httpClient.post(DataTransferIndices.class,
					"/" + scopeName + "/" + appName + "/" + graphName + "/filter?", theDataFilter);
			setDtiList(dti.getDataTransferIndexList().getItems());
			setDtoUrl("/" + scopeName + "/" + appName + "/" + graphName);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
		return dti;
	}
	
	
	public void setURI (String uri) {
		this.URI = uri;
	}
	

	
	public void setDtiList(List<DataTransferIndex> dtiList)
	  {
		this.dtiList = dtiList;
	  }

	
	
	public void setDtoUrl(String dtoUrl)
	  {
	    this.dtoUrl = dtoUrl;
	  }

	public String getDtoUrl()
	  {
	    return dtoUrl;
	  }
	
	public void setRInd(int rInd)
	  {
	    this.rInd = rInd;
	  }

	public int getRInd()
	  {
	    return rInd;
	  }
	
	public Grid getGrid(){
		return grid;
	}
	
	public void setGrid(Grid grid){
		this.grid = grid;
	}
	
    public Grid toGrid() {		
    	grid = new Grid();    	
		dtoCtr = new DtoContainer();		
	    dtoCtr.setTotal(dtiList.size());
		DataTransferIndex dti = dtiList.get(0);	    
	    dtoCtr.setUrl(dtoUrl, dti.getIdentifier());
	    dtoCtr.populate(URI);	
	    dtoCtr.initialHList();
	    dtoCtr.fillConfig();	    
	   
	    dtoCtr.setGridList(grid);
	    
		return grid;
	}
    
    
	
    public Rows toRows(int start, int limit) {	
    	dtiPage = dtiList.subList(start, Math.min(limit+start, dtiList.size()-1));
    	Rows rows = new Rows();
    	dtoCtr = new DtoContainer();
    	dtoCtr.setUrl(dtoUrl, "page");
    	dtoCtr.populatePage(URI, dtiPage);	
    	dtoCtr.initialDataList();    	
    	dtoCtr.fillPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }  
    
    public Grid toDetailRelGrid(String id, String classId) {
    	prepareGrid(id);	  
	    dtoCtr.setClassId(classId);
	    dtoCtr.fillDetailRelConfig();	   
	    dtoCtr.setGridList(grid);
    	return grid;
    }
    
    public Rows toDetailRelRows(String id, String classId) {
    	prepareRows(id);	
    	dtoCtr.setClassId(classId);
    	dtoCtr.initialHList();
    	dtoCtr.fillDetailRelPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }
    
    public Rows toRelRows(String id) {	
    	prepareRows(id);	
    	dtoCtr.fillRelPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }
    
    public void prepareRows(String id) {
    	rows = new Rows();
    	dtoCtr = new DtoContainer();
    	dtoCtr.setUrl(dtoUrl, id);
    	dtoCtr.populate(URI);
    	dtoCtr.initialDataList();
    }
    public void prepareGrid(String id) {
    	grid = new Grid();    	
		dtoCtr = new DtoContainer();		
	    dtoCtr.setUrl(dtoUrl, id);
	    dtoCtr.populate(URI);	
	    dtoCtr.initialHList();
    }
    
 
    public Rows toRelRelationRows(String id, String classId, String relatedId) {
    	prepareRows(id);	
    	dtoCtr.setClassId(classId);
    	dtoCtr.setRelatedId(relatedId);
    	dtoCtr.fillRelRelationPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }
    
	public void readGrid(Grid grid) {
		//TODO: Read the grid!
	}
}
