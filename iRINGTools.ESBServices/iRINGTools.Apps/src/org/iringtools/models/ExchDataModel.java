package org.iringtools.models;

import java.util.List;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Rows;
import org.iringtools.utility.HttpClient;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.DataTransferIndex;
import com.opensymphony.xwork2.ActionContext;

public class ExchDataModel {

	private Grid grid = null;
	private static String URI="";	
	private String identifier;
	private String dtoUrl;
	private List<DataTransferIndex> dtiList=null;
	private List<DataTransferIndex> dtiPage;
	private DtoContainer dtoCtr;
	private int rInd=0;
	
	public ExchDataModel() {
		if (URI.equals("")) {
			try {
				URI = ActionContext.getContext().getApplication()
						.get("ExchangeDataServiceUri").toString();
			} catch (Exception e) {
				System.out.println("Exception in ExchangeDataServiceUri :" + e);
			}
			grid = null;
		}
	}	
	
	public void populate(String scopeName, String id) {
		try {
			HttpClient httpClient = new HttpClient(URI);
			DataTransferIndices dti = httpClient.get(DataTransferIndices.class,
					"/" + scopeName + "/exchanges/" + id);
			setDtiList(dti.getDataTransferIndexList().getItems());
			setDtoUrl("/" + scopeName + "/exchanges/" + id);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public static void setURI (String uri) {
		ExchDataModel.URI = uri;
	}
	
	public List<DataTransferIndex> getDtiList()
	  {
	    return dtiList;
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
	
	public void setIdentifier(String identifier)
	  {
	    this.identifier = identifier;
	  }

	public String getIdentifier()
	  {
	    return identifier;
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
		//DataTransferIndex dti = dtiList.get(0);
		dtiPage = dtiList.subList(0,1);
	    //setIdentifier(dti.getIdentifier());
	    dtoCtr.setUrl(dtoUrl);
	    dtoCtr.populatePage(URI, dtiPage);	      
	    dtoCtr.fillExchConfig();
	    dtoCtr.setGridList(grid);
		return grid;
	}
	
    public Rows toRows(int start, int limit) {	
    	dtoCtr = new DtoContainer();
    	dtoCtr.setTotal(dtiList.size());
    	dtiPage = dtiList.subList(start, Math.min(limit+start, dtiList.size()-1));
    	Rows rows = new Rows();    	
    	dtoCtr.setUrl(dtoUrl);
    	dtoCtr.populatePage(URI, dtiPage);	
    	dtoCtr.fillExchPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }
    
   
    
    public Grid toRelGrid(String id) {    	
    	grid = new Grid();    	
		dtoCtr = new DtoContainer();
		dtiPage = dtiList.subList(0,1);
		dtoCtr.setUrl(dtoUrl);
		dtoCtr.populatePage(URI, dtiPage);	            
	    dtoCtr.fillRelConfig();	   
	    dtoCtr.setGridList(grid);
    	return grid;
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
		dtiPage = dtiList.subList(ind, ind+1);  
    }
    
    public Rows toRelRows(int start, int limit, String id) {	
    	Rows rows = new Rows();
    	dtoCtr = new DtoContainer();    	
		SetDtiPage(id);	
    	dtoCtr.setUrl(dtoUrl);
    	dtoCtr.populatePage(URI, dtiPage);
    	dtoCtr.fillExchRelPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }
    
    
    public Grid toDetailRelGrid(String id, String classId) {
    	grid = new Grid();    	
		dtoCtr = new DtoContainer();	
		SetDtiPage(id);	
	    dtoCtr.setUrl(dtoUrl);
	    dtoCtr.populatePage(URI, dtiPage);
	    dtoCtr.setClassId(classId);
	    dtoCtr.fillExchDetailRelConfig();	   
	    dtoCtr.setGridList(grid);
    	return grid;
    }
    
    public Rows toDetailRelRows(String id, String classId) {
    	Rows rows = new Rows();
    	dtoCtr = new DtoContainer();
    	SetDtiPage(id);	
    	dtoCtr.setUrl(dtoUrl);
    	dtoCtr.populatePage(URI, dtiPage);
    	dtoCtr.setClassId(classId);
    	dtoCtr.fillExchDetailRelPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }
    
   
    
	public void readGrid(Grid grid) {
		//TODO: Read the grid!
	}
}
