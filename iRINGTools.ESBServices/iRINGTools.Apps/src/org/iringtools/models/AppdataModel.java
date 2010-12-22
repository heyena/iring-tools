package org.iringtools.models;

import java.util.List;
import org.iringtools.grid.Grid;
import org.iringtools.grid.Rows;
import org.iringtools.utility.HttpClient;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.DataTransferIndex;
import com.opensymphony.xwork2.ActionContext;

public class AppdataModel {

	private Grid grid = null;
	private static String URI="";	
	private String identifier;
	private String dtoUrl;
	private List<DataTransferIndex> dtiList=null;
	private List<DataTransferIndex> dtiPage;
	private DtoContainer dtoCtr;
	private int rInd=0;
	
	public AppdataModel() {
		if (URI.equals("")) {
			try {
				URI = ActionContext.getContext().getApplication()
						.get("AppDataServiceUri").toString();
			} catch (Exception e) {
				System.out.println("Exception in AppDataServiceUri :" + e);
			}
			grid = null;
		}
	}	
	
	public void populate(String scopeName, String appName, String graphName) {
		try {
			HttpClient httpClient = new HttpClient(URI);
			DataTransferIndices dti = httpClient.get(DataTransferIndices.class,
					"/" + scopeName + "/" + appName + "/" + graphName);
			setDtiList(dti.getDataTransferIndexList().getItems());
			setDtoUrl("/" + scopeName + "/" + appName + "/" + graphName);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public static void setURI (String uri) {
		AppdataModel.URI = uri;
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
	    dtoCtr.setTotal(dtiList.size());
		DataTransferIndex dti = dtiList.get(0);
	    setIdentifier(dti.getIdentifier());
	    dtoCtr.setUrl(dtoUrl, identifier);
	    dtoCtr.populate(URI);	      
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
    	dtoCtr.fillPage();	
    	dtoCtr.setRowsList(rows);
    	return rows;
    }
    
	public void readGrid(Grid grid) {
		//TODO: Read the grid!
	}
}
