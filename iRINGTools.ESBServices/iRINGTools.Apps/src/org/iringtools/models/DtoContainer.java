package org.iringtools.models;




import java.util.List;
import java.util.ArrayList;

import org.iringtools.grid.Grid;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.TemplateObject;

import org.iringtools.grid.Rows;
import org.iringtools.utility.HttpClient;

public class DtoContainer {

	private String rType = "";    //role type
	private String tName = "";    //template name
	private String rName = "";    //role name
	private String rcName = "";   //related class name
	private String rValue = "";   //role value
	private String dtoUrl = "";
	private List<TemplateObject> tObjList;
	private List<RoleObject> roList;
	private List<ClassObject> claList;
	private RoleObject roObj;
	private List<DataTransferObject> dtoList;
	private List<String> hList = new ArrayList<String>(); 
	private String ghList = "[";	  //list of headers
	private List<String> rowList = new ArrayList<String>();
	private String relateList = "[";   //list of related object
	private String cList = "[";   //list of column description
	private String rlData = "";	  //related class object
	private String row = "";      //row data in grid	
	private String chData = ""; //column description or headerList data
	private String identifier = "", tempName = "";
	private int ind=1;
	
		
	public void populate(String URI) {
		try {
			HttpClient httpClient = new HttpClient(URI);			
			DataTransferObjects dto = httpClient.get(DataTransferObjects.class, dtoUrl);
			setDtoList(dto.getDataTransferObjectList().getItems());
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public void setUrl(String url, String identifier)
	  {
	    this.dtoUrl = url + "/" + identifier;
	  }
	
	public void setGhList(String ghList)
	  {
	    this.ghList = ghList;
	  }

	public String getGhList()
	  {
	    return ghList;
	  }
	
	public void setCList(String cList)
	  {
	    this.cList = cList;
	  }

	public String getCList()
	  {
	    return cList;
	  }
	
	public void setRow(String row)
	  {
	    this.row = row;
	  }

	public void setRlData(String rlData)
	{
		this.rlData = rlData;
	}
	
	public String getRow()
	  {
	    return row;
	  }
	
	public void setDtoUrl(String dtoUrl)
	  {
	    this.dtoUrl = dtoUrl;
	  }

	public String getDtoUrl()
	  {
	    return dtoUrl;
	  }
	
	public void setRcName(String rcName)
	  {
	    this.rcName = rcName;
	  }

	public String getRcName()
	  {
	    return rcName;
	  }
	
	public void setRoObj(RoleObject roObj)
	  {
	    this.roObj = roObj;
	  }	
	
	public void setIdentifier(String identifier)
	  {
	    this.identifier = identifier;
	  }

	public String getIdentifier()
	  {
	    return identifier;
	  }
		
	public void setRName(String rName)
	  {
	    this.rName = rName;
	  }

	public String getRName()
	  {
	    return rName;
	  }
	
	public void setRType(String rType)
	  {
	    this.rType = rType.toUpperCase();
	  }

	public String getRType()
	  {
	    return rType;
	  }
	
	public void setTName(String tName)
	  {
	    this.tName = tName;
	  }

	public String getTName()
	  {
	    return tName;
	  }

	public void setRValue(String rValue)
	  {
	    this.rValue = rValue;
	  }

	public String getRValue()
	  {
	    return rValue;
	  }

	public void setClaList(List<ClassObject> claList)
	  {
	    this.claList = claList;
	  }

	public List<ClassObject> getClaList()
	  {
	    return claList;
	  }
	
	public void setRoList(List<RoleObject> roList)
	  {
	    this.roList = roList;
	  }

	public List<RoleObject> getRoList()
	  {
	    return roList;
	  }
	
	public void setTObjList(List<TemplateObject> tObjList)
	  {
	    this.tObjList = tObjList;
	  }

	public List<TemplateObject> getTObjList()
	  {
	    return tObjList;
	  }

	
	public void setDtoList(List<DataTransferObject> dtoList)
	  {
	    this.dtoList = dtoList;
	  }

	public List<DataTransferObject> getDtoList()
	  {
	    return dtoList;
	  }	    

	public boolean hListHas ()
	{
		String head;
		for (int i = 0; i < hList.size(); i++)
		{	
			head = hList.get(i);
			if (head.equals(tempName))
				return true;			
		}
		return false;		
	}
	
	public void setChData (String cd)
	{
		this.chData = cd;
	}
	
	public void setRowList(List<String> rowList)
	{
		this.rowList = rowList;
	}
	
	public void setColumnList(String colList)
	{
		this.cList = colList;
	}
	
	public void setRelateList(String reList)
	{
		this.relateList = reList;
	}
	
	public String addEnd (String list)
	{
		if (!list.equals("["))
			list=list + "]";
		return list;
	
	}
	public void setRowsList(Rows rows)
	{
		rows.setRows(rowList);
	}
	
	public void setLists (Grid grid)
	{
		//rowList=addEnd(rowList);
		relateList=addEnd(relateList); 
		//grid.setRows(rowList);
		//grid.setRelatedItemList(relateList);
	}
	
	public String addComma(String list)
	{
		if (!list.equals("["))
			list = list + ",";
		return list;
	}
	 
	public void setGridColumnHeader(Grid grid)
	{		
		String head; 
		for (int i = 0; i < hList.size(); i++)
		{	
			head = hList.get(i);			
			chData = "{id:" + head + ",header:" + head + ","
			           + "width:" + 440 + ",sortable:true,dataIndex:" + head + "}";
			
			cList = addComma(cList); 
			cList = cList+chData;
			setChData("");
			chData = "{name:" + head + "}";
			ghList = addComma(ghList);
			ghList = ghList+chData;
			setChData("");
		}

		cList = addEnd(cList);
		ghList = addEnd(ghList); 
		grid.setColumnData(cList);		
		grid.setHeaderList(ghList);
	}
	
	public void addToGrid(String name)
	{		
		setRValue(roObj.getValue());
		if(rType.equals("OBJECT_PROPERTY") || rType.equals("DATA_PROPERTY"))
			tempName = tName + '.' + name;
		else
			tempName = name;
		
		if (!row.equals(""))
			row = row + ",{";
		else
			row = row + "{";
		
		row = row + tempName + ":" + rValue + "}";
		
		if (!hListHas())
		{			
			hList.add(tempName);
		}		
	}
	
	public void addRList(int ri)
	{		
		if (!rlData.equals(""))
			rlData = rlData + ",\"text" + ri + "\":\"" + rcName + "\"";
		else
			rlData = rlData + "{\"id\":" + ind + "," + "\"identifier\":\"" + identifier + "\",\"text" + ri + "\":\"" + rcName + "\"";
	}
	
    public void fillRow()
    {   
    	int ti = 0, ri = 1;   	
		
	    for (DataTransferObject dto : dtoList)
	    {
	      setClaList(dto.getClassObjects().getItems());
	      for (ClassObject clo : claList)
	      {
	    	  
	    	  setTObjList(clo.getTemplateObjects().getItems());
	    	  for (TemplateObject tObj : tObjList)
	    	  {
	    		  setTName(tObj.getName());
	    		  setRoList(tObj.getRoleObjects().getItems());
	    		  for (RoleObject rObj : roList)
	    		  {	    			  
	    			  setRType(rObj.getType().toString());
	    			  if (ti < 1 && rType.equals("PROPERTY"))
	    			  {
	    				  setRoObj(rObj);
	    				  setIdentifier(rObj.getValue());
	    				  addToGrid(tName);	    				  
	    			  }
	    			  else if (rType.equals("OBJECT_PROPERTY") || rType.equals("DATA_PROPERTY"))
	    			  {	  
	    				  setRoObj(rObj);  				  
	    				  addToGrid(rObj.getName());	    				 
	    			  }
	    			  else if (rType.equals("REFERENCE"))
	    			  {	    				  
	    				  setRcName(rObj.getRelatedClassName());
	    				  if(rcName != null)
	    				  {
	    					  addRList(ri);
	    					  ri++;
	    				  }
	    			  }
	    		  }
	    		  if (ti<1)  
	    			  ti++;
	    	  }
	    	  
	      }
	    }
	    
	  /*  if(!rowList.equals("["))
	    	rowList = rowList+",";
	    if(!relateList.equals("["))
	    	relateList = relateList+",";*/
	    row = row + "}";
	    rowList.add(row);
	    rlData = rlData + "}";	
	    //rowList=rowList+row;		
		//relateList=relateList+rlData;	
		setRow("");
		setRlData("");
		ind++;

	}
    
	
	public void readGrid(Grid grid) {
		//TODO: Read the grid!
	}
}
