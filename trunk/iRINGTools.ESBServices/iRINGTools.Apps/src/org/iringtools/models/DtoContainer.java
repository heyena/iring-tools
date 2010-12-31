package org.iringtools.models;

import java.util.List;
import java.util.ArrayList;

import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.Filter;
import org.iringtools.ui.widgets.grid.Column;
import org.iringtools.ui.widgets.grid.Header;
import java.util.HashMap;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.TemplateObject;

import org.iringtools.ui.widgets.grid.Rows;

import org.iringtools.utility.HttpClient;

public class DtoContainer {

	private String rType = ""; // role type
	private String tName = ""; // template name

	private String rcName = ""; // related class name
	private String rValue = ""; // role value
	private String dtoUrl = "";
	private List<TemplateObject> tObjList;
	private List<RoleObject> roList;
	private List<ClassObject> claList;
	private RoleObject roObj;
	private List<DataTransferObject> dtoList;
	private List<HashMap<String, String>> dataList = new ArrayList<HashMap<String, String>>();
	private HashMap<String, String> data;
	private List<String> hList = new ArrayList<String>();
	private List<Header> ghList = new ArrayList<Header>(); // list of headers
	private List<Filter> filterList = new ArrayList<Filter>();
	
	private List<Column> cList = new ArrayList<Column>(); // list of column
															// description
	private String classId = ""; // role id of the related object

	private String identifier = "", tempName = "", clsName;
	private Filter filter;
	private Column column;
	private Header header;
	private int page = 20;

	private static int total;
	private static HashMap<String, String> dunit;

	public void populate(String URI) {
		try {
			HttpClient httpClient = new HttpClient(URI);
			DataTransferObjects dto = httpClient.get(DataTransferObjects.class,
					dtoUrl);
			setDtoList(dto.getDataTransferObjectList().getItems());
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public void populatePage(String URI, List<DataTransferIndex> dtiList) {
		try {
			DataTransferIndices dti = new DataTransferIndices();
			DataTransferIndexList list = new DataTransferIndexList();
			dti.setDataTransferIndexList(list);
			list.setItems(dtiList);
			HttpClient httpClient = new HttpClient(URI);
			DataTransferObjects dto = httpClient.post(
					DataTransferObjects.class, dtoUrl, dti);
			setDtoList(dto.getDataTransferObjectList().getItems());
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public void setUrl(String url, String end) {
		this.dtoUrl = url + "/" + end;
	}

	public void setUrl(String url)
	{
		this.dtoUrl = url;
	}
	
	public void setRcName(String rcName) {
		this.rcName = rcName;
	}

	public void setClsName(String clsName) {
		this.clsName = clsName;
	}
	
	public void setClassId(String classId) {
		this.classId = classId;
	}

	public void setRoObj(RoleObject roObj) {
		this.roObj = roObj;
	}

	public void setIdentifier(String identifier) {
		this.identifier = identifier;
		tempName = "Identifier";
		if (!hListHas())
			hList.add(tempName);
	}
	
	public String getIdentifier(){
		return identifier;
	}
	
	public boolean hListHas() {		
		for (String head : hList) {			
			if (head.equals(tempName))
				return true;
		}
		return false;
	}
	
	public void setRIdentifier(String identifier) {
		this.identifier = identifier;
		data.put("Identifier", identifier);
	}
	
	public void addToGrid(String name) {
		setTempName(name);
		if (!hListHas()) {
			hList.add(tempName);
		}
	}

	public void fillExchConfig() {
		int ti = 0;

		for (DataTransferObject dto : dtoList) {
			setClaList(dto.getClassObjects().getItems());
			addToGrid("TransferType");
			for (ClassObject clo : claList) {

				setTObjList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : tObjList) {
					setTName(tObj.getName());
					setRoList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roList) {
						setRType(rObj.getType().toString());
						if (ti < 1 && rType.equals("PROPERTY")) {
							setClsName(clo.getName());
							setRoObj(rObj);
							setIdentifier(rObj.getValue());
							addToGrid(tName);
						} else if (rType.equals("OBJECT_PROPERTY")
								|| rType.equals("DATA_PROPERTY")) {
							setRoObj(rObj);
							addToGrid(rObj.getName());
						} 
					}
					if (ti < 1)
						ti++;
				}

			}
		}

		
	}
	
	public void setRType(String rType) {
		this.rType = rType.toUpperCase();
	}

	public void setTName(String tName) {
		this.tName = tName;
	}

	public void addToRow(String name) {
		setRValue(roObj.getValue());
		setTempName(name);
		data.put(tempName, rValue);
	}

	public void fillExchRelPage()  {		
		total = 0;		
		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();
			data.put("TransferType", dto.getTransferType().value());
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {
				if (hList.size()>0) {
					if (hList.size() > dataList.size()) {
						setClsName(clo.getName());	
						for (String head : hList) {						
							if (clsName.equals(head)) {
								setClassId(clo.getClassId());
								addRelRow(head);						
							}	
						}
					} else {
						return;
					}
				}
				setTObjList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : tObjList) {
					setTName(tObj.getName());
					setRoList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roList) {
						setRType(rObj.getType().toString());
						if (rType.equals("REFERENCE")) {
							setRcName(rObj.getRelatedClassName());
							if (rcName != null) {								
								addToGrid(rcName);
							}
						}
					}
					
				}
			}
			
			
		}
	}
	
	
	public void fillExchDetailRelPage() {		
		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();
			data.put("TransferType", dto.getTransferType().value());
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {
				if (!clo.getClassId().equals(classId)) 
					continue;
				setTObjList(clo.getTemplateObjects().getItems());
				TemplateObject tObj = tObjList.get(0);
				addToGrid(tObj.getName());
				addDetailRelRow(tObj.getName(), clo.getIdentifier());
				return;
			}
		}		
	}	
	
	
	public void fillExchPage() {
		int ti = 0;

		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();
			setClaList(dto.getClassObjects().getItems());
			data.put("TransferType", dto.getTransferType().value());
			for (ClassObject clo : claList) {

				setTObjList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : tObjList) {
					setTName(tObj.getName());
					setRoList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roList) {
						setRType(rObj.getType().toString());
						if (ti < 1 && rType.equals("PROPERTY")) {
							setRoObj(rObj);
							setRIdentifier(rObj.getValue());
							addToRow(tName);
						} else if (rType.equals("OBJECT_PROPERTY")
								|| rType.equals("DATA_PROPERTY")) {
							setRoObj(rObj);
							addToRow(rObj.getName());
						}

					}
					if (ti < 1)
						ti++;
				}
			}
			dataList.add(data);
			ti = 0;
		}
	}
	
	public void retriveRValue(String rval) {
		RefContainer refCon = new RefContainer();
		refCon.populate(rval);
		tempName = refCon.getValue();
		dunit.put(rValue, tempName);
	}

	public void setRValue(String rval) {
	/*	if (rType.equals("OBJECT_PROPERTY")) {
			if (dunit == null) {
				dunit = new HashMap<String, String>();
				retriveRValue(rval.substring(4));
				this.rValue = tempName;
			} else {
				if (dunit.containsKey(rval))
					this.rValue = dunit.get(rval);
				else {
					retriveRValue(rval);
					this.rValue = tempName;
				}

			}
		}*/
		this.rValue = rval;
	}
	
	public void fillPage() {
		int ti = 0;

		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {

				setTObjList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : tObjList) {
					setTName(tObj.getName());
					setRoList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roList) {
						setRType(rObj.getType().toString());
						if (ti < 1 && rType.equals("PROPERTY")) {
							setRoObj(rObj);
							setRIdentifier(rObj.getValue());
							addToRow(tName);
						} else if (rType.equals("OBJECT_PROPERTY")
								|| rType.equals("DATA_PROPERTY")) {
							setRoObj(rObj);
							addToRow(rObj.getName());
						}

					}
					if (ti < 1)
						ti++;
				}
			}
			dataList.add(data);
			ti = 0;
		}
	}
	
	public void setClaList(List<ClassObject> claList) {
		this.claList = claList;
	}

	public void setRoList(List<RoleObject> roList) {
		this.roList = roList;
	}

	public List<RoleObject> getRoList() {
		return roList;
	}

	public void setTObjList(List<TemplateObject> tObjList) {
		this.tObjList = tObjList;
	}

	public void setDtoList(List<DataTransferObject> dtoList) {
		this.dtoList = dtoList;
	}

	

	public void fillConfig() {
		int ti = 0;

		for (DataTransferObject dto : dtoList) {
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {

				setTObjList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : tObjList) {
					setTName(tObj.getName());
					setRoList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roList) {
						setRType(rObj.getType().toString());
						if (ti < 1 && rType.equals("PROPERTY")) {
							setClsName(clo.getName());
							setRoObj(rObj);
							setIdentifier(rObj.getValue());
							addToGrid(tName);
						} else if (rType.equals("OBJECT_PROPERTY")
								|| rType.equals("DATA_PROPERTY")) {
							setRoObj(rObj);
							addToGrid(rObj.getName());
						} 
					}
					if (ti < 1)
						ti++;
				}

			}
		}

		
	}

	
	public void fillExchDetailRelConfig() {
		addToGrid("TransferType");
		fillDetailRelConfig();	
	}
	
	public void fillDetailRelConfig() {
		//addToGrid("Related_Items");
		for (DataTransferObject dto : dtoList) {
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {
				if (!clo.getClassId().equals(classId)) 
					continue;
				setTObjList(clo.getTemplateObjects().getItems());
				TemplateObject tObj = tObjList.get(0);
				addToGrid(tObj.getName());
				return;
			}
		}		
	}
	
	
	
	
	public void fillDetailRelPage() {
		//addToGrid("Related_Items");
		for (DataTransferObject dto : dtoList) {
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {
				if (!clo.getClassId().equals(classId)) 
					continue;
				setTObjList(clo.getTemplateObjects().getItems());
				TemplateObject tObj = tObjList.get(0);
				addToGrid(tObj.getName());
				data = new HashMap<String, String>();		
				addDetailRelRow(tObj.getName(), clo.getIdentifier());
				return;
			}
		}		
	}	
		
	public void addDetailRelRow(String x, String y) {		
		data.put(x, y);		
		dataList.add(data);
		total++;
	}
	
	public void fillRelConfig () {
		int ti = 0;

		addToGrid("Related_Items");
		for (DataTransferObject dto : dtoList) {
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {

				setTObjList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : tObjList) {
					setTName(tObj.getName());
					setRoList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roList) {
						setRType(rObj.getType().toString());
						if (ti < 1 && rType.equals("PROPERTY")) {
							setClsName(clo.getName());	
							return;
						} 
					}
					if (ti < 1)
						ti++;
				}

			}
		}		
	}
	
	
	
	public void fillRelPage() {		
		total = 0;		
		for (DataTransferObject dto : dtoList) {			
			setClaList(dto.getClassObjects().getItems());
			for (ClassObject clo : claList) {
				if (hList.size()>0) {
					if (hList.size() > dataList.size()) {
						setClsName(clo.getName());	
						for (String head : hList) {						
							if (clsName.equals(head)) {
								setClassId(clo.getClassId());
								addRelRow(head);						
							}	
						}
					} else {
						return;
					}
				}
				setTObjList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : tObjList) {
					setTName(tObj.getName());
					setRoList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roList) {
						setRType(rObj.getType().toString());
						if (rType.equals("REFERENCE")) {
							setRcName(rObj.getRelatedClassName());
							if (rcName != null) {								
								addToGrid(rcName);
							}
						}
					}
					
				}
			}
			
			
		}
	}
	
	
	

	
	public void addRelRow(String head) {
		data = new HashMap<String, String>();		
		data.put("id", classId);
		data.put("label", head);
		dataList.add(data);
		total++;
	}

	public void setTotal(int total) {
		DtoContainer.total = total;
	}

	public void setRowsList(Rows rows) {
		rows.setData(dataList);
		rows.setSuccess("true");
		rows.setTotal(total);
	}
	
	public void setGridList(Grid grid) {
	
		for (String head : hList) {			
			column = new Column();
			column.setDataIndex(head);
			column.setHeader(head.replace('_', ' '));
			column.setId(head);
			column.setSortable("true");
			column.setWidth(Math.ceil(6.8 * head.length()));			
			
			cList.add(column);

			header = new Header();
			header.setName(head);
			ghList.add(header);

			filter = new Filter();
			filter.setType("string");
			filter.setDataIndex(head);
			filterList.add(filter);
		}
		grid.setFilterSets(filterList);
		grid.setColumnDatas(cList);
		grid.setHeaderLists(ghList);

		grid.setCacheData("true"); // Add rules in the future
		if (filterList.size() > 0 && cList.size() > 0 && ghList.size() > 0)
			grid.setSuccess("true");
		grid.setPageSize(page);
		grid.setClassObjName(clsName);
	}

	public void setTempName(String name) {
		if (rType.equals("OBJECT_PROPERTY") || rType.equals("DATA_PROPERTY"))
			tempName = tName + '_' + name;
		else {
			tempName = name;
		}
	}	

	public void readGrid(Grid grid) {
		// TODO: Read the grid!
	}
}
