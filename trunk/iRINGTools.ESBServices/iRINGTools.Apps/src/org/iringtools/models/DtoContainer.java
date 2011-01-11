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
	private List<TemplateObject> templatObjectList;
	private List<RoleObject> roleObjectList;
	private List<ClassObject> classObjectList;
	private RoleObject roObj;
	private List<DataTransferObject> dtoList;
	private List<HashMap<String, String>> dataList;
	private HashMap<String, String> data, roleNameMap; 
	private List<String> hList, roleNameList; 
	private String classId = "", relatedId = "", roleValue = "", oldRoleValue = "";

	private String identifier = "", tempName = "", clsName, transferType = "";
	
	private int page = 20, ti = 0, roleNumber = 0;
	private TemplateObject tObj;	
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

	public void setUrl(String url) {
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

	public void setRelatedId(String relatedId) {
		this.relatedId = relatedId;
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

	public String getIdentifier() {
		return identifier;
	}

	public boolean hListHas() {
		for (String head : hList) {
			if (head.equals(tempName))
				return true;
		}
		return false;
	}
	
	public boolean roleNameListHas() {
		for (String head : roleNameList) {
			if (head.equals(tempName))
				return true;
		}
		return false;
	}

	public String addHtmlUnderLine(String name) {
		return "<span class=\"underLineLink\">"
		+ name + "</span>";
	}
	
	public void setRIdentifier(String identif) {
		this.identifier = addHtmlUnderLine(identif);;

		/*
		 this.identifier = "<span onMouseOver=\"javascript:this.style.textDecoration=\'underline\'\" onMouseOut=\"javascript:this.style.textDecoration=\'none\'\">"
				+ identif + "</span>";
		 */
		data.put("Identifier", identifier);
	}
	
	public void setNoHtmlIdentifier(String value) {
		this.identifier = value;
		data.put("Identifier", identifier);
	}

	public void addToGrid(String name) {
		setTempName(name);
		if (!hListHas()) {
			hList.add(tempName);
		}
	}

	public void setTempName(String name) {
		if (rType.equals("OBJECT_PROPERTY") || rType.equals("DATA_PROPERTY") || rType.equals("PROPERTY")) {
			if (!name.toLowerCase().equals("identificationbytag"))
				tempName = tName + '_' + name;
			else
				tempName = name;
		}
		else {
			tempName = name;
		}
	}
	
	
	
	public void addHList(String name) {
		tempName = name;
		if (!hListHas())
			hList.add(tempName);
	}
	
	public void addRoleNameToHList() {
		if (roleNameList.size() > 0) {
			if (roleNameList.size() > 1)
				for (String str : roleNameList)
					addHList(str);
			else {
				String columnName = roleNameList.get(0);				
				tempName = columnName.substring(0, columnName.indexOf("_"));
				addHList(tempName);
			}
		}
		roleNameList.clear();
		roleNumber = 0;
	}
	
	public void setRType(String rType) {
		this.rType = rType.toUpperCase();
	}

	public void setTName(String tName) {
		this.tName = tName;
	}

	

	public void initialDataList() {
		dataList = new ArrayList<HashMap<String, String>>();		
	}

	public void initialRoleNameList() {
		roleNameMap = new HashMap<String, String>();
		roleNameList = new ArrayList<String>();
	}
	
	public void initialHList() {
		hList = new ArrayList<String>();
	}

	public void retrieveRValue(String rval) {
		RefContainer refCon = new RefContainer();
		refCon.populate(rval);
		tempName = refCon.getValue();
		dunit.put(rValue, tempName);
	}

	public void setRValue(String rval) {
		/*
		 * if (rType.equals("OBJECT_PROPERTY")) { if (dunit == null) { dunit =
		 * new HashMap<String, String>(); retriveRValue(rval.substring(4));
		 * this.rValue = tempName; } else { if (dunit.containsKey(rval))
		 * this.rValue = dunit.get(rval); else { retriveRValue(rval);
		 * this.rValue = tempName; }
		 * 
		 * } }
		 */
		this.rValue = rval;
	}

	public void addRoleNameToMap() {
		if (roleNameList.size() > 0) {
			if (roleNameList.size() == 1) {				
				String columnName = roleNameList.get(0);				
				if (!roleNameMap.containsKey(tempName))
					roleNameMap.put(tempName, columnName.substring(0, columnName.indexOf("_")));
			}
		}
		roleNameList.clear();
		roleNumber = 0;
	}
	
	public void addToRoleNameList(String name) {		
		tempName = tName + '_' + name;
		if (!roleNameListHas())
			roleNameList.add(tempName);
	}
	
	public void findColumnName() {
		for (RoleObject rObj : roleObjectList) {
			setRType(rObj.getType().toString());
			if (rType.equals("OBJECT_PROPERTY")
					|| rType.equals("DATA_PROPERTY") || rType.equals("PROPERTY")) {
				if (!tName.toLowerCase().equals("identificationbytag")) 	
					addToRoleNameList(rObj.getName());
			}
		}
		addRoleNameToMap();
	}
	public void getObjectDataProperty(DataTransferObject dto) {
		ClassObject classObject;
		classObject = dto.getClassObjects().getItems().get(0);
		settemplatObjectList(classObject.getTemplateObjects().getItems());
		for (TemplateObject tObj : templatObjectList) {
			setTName(tObj.getName());
			setroleObjectList(tObj.getRoleObjects().getItems());
			findColumnName();
			for (RoleObject rObj : roleObjectList) {
				setRType(rObj.getType().toString());
				if (ti < 1 && rType.equals("PROPERTY")) {
					setRoObj(rObj);
					setRIdentifier(rObj.getValue());
					addToRow(tName);
				} else if (rType.equals("OBJECT_PROPERTY")
						|| rType.equals("DATA_PROPERTY") || rType.equals("PROPERTY")) {
					setRoObj(rObj);					
					addToRow(rObj.getName());
				}

			}
			if (ti < 1)
				ti++;
		}

	}

	public void fillExchPage() {	
		initialRoleNameList();
		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();
			transferType = dto.getTransferType().value();
			data.put("TransferType", transferType);
			getObjectDataProperty(dto);
			dataList.add(data);
			ti = 0;
		}
	}

	public void fillPage() {	
		initialRoleNameList();
		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();				
			getObjectDataProperty(dto);
			dataList.add(data);
			ti = 0;
		}
	}

	public void setclassObjectList(List<ClassObject> classObjectList) {
		this.classObjectList = classObjectList;
	}

	public void setroleObjectList(List<RoleObject> roleObjectList) {
		this.roleObjectList = roleObjectList;
	}

	public List<RoleObject> getroleObjectList() {
		return roleObjectList;
	}

	public void settemplatObjectList(List<TemplateObject> templatObjectList) {
		this.templatObjectList = templatObjectList;
	}

	public void setDtoList(List<DataTransferObject> dtoList) {
		this.dtoList = dtoList;
	}

	public void fillExchConfig() {
		addToGrid("TransferType");
		fillConfig();
	}

	public void fillConfig() {
		roleNameList = new ArrayList<String>();
		ClassObject classObject;
		for (DataTransferObject dto : dtoList) {
			classObject = dto.getClassObjects().getItems().get(0);
			settemplatObjectList(classObject.getTemplateObjects().getItems());
			for (TemplateObject tObj : templatObjectList) {
				setTName(tObj.getName());
				setroleObjectList(tObj.getRoleObjects().getItems());
				for (RoleObject rObj : roleObjectList) {
					setRType(rObj.getType().toString());
					if (ti < 1 && rType.equals("PROPERTY")) {
						setClsName(classObject.getName());
						setRoObj(rObj);
						setIdentifier(rObj.getValue());
						addToGrid(tName);
					} else if (rType.equals("OBJECT_PROPERTY")
							|| rType.equals("DATA_PROPERTY") || rType.equals("PROPERTY")) {
						if (!tName.toLowerCase().equals("identificationbytag")) {
							setRoObj(rObj);
							addToRoleNameList(rObj.getName());
						}
					}
				}
				addRoleNameToHList();
				if (ti < 1)
					ti++;
			}

		}
	}

	public void fillExchDetailRelConfig() {
		addToGrid("TransferType");
		fillDetailRelConfig();
	}

	public void fillDetailRelConfig() {
		setIdentifier("Identifier");
		for (DataTransferObject dto : dtoList) {
			setclassObjectList(dto.getClassObjects().getItems());
			for (ClassObject clo : classObjectList) {
				if (!clo.getClassId().equals(classId))
					continue;
				settemplatObjectList(clo.getTemplateObjects().getItems());
				tObj = templatObjectList.get(0);
				addToGrid(tObj.getName());
				return;
			}
		}
	}

	public void getRelatedItemsDetails() {
		for (ClassObject clo : classObjectList) {
			if (!clo.getClassId().equals(classId))
				continue;
			settemplatObjectList(clo.getTemplateObjects().getItems());
			tObj = templatObjectList.get(0);
			addToGrid(tObj.getName());
			tName = clo.getIdentifier();
			setRIdentifier(tName);			
			addDetailRelRow(tObj.getName(), tName);
			return;
		}
	}
	
	public void setExchangeRoleValue() {
		roleValue = roObj.getValue();
		oldRoleValue = roObj.getOldValue();		
		if (oldRoleValue != null
				&& !oldRoleValue.equals(roleValue)) {
			setRValue("<span class=\"highLightChange\">" + oldRoleValue
					+ " -> " + roleValue + "</span>");
		} else {
			setRValue(roleValue);
		}
	}
	
	public void addToRow(String name) {
		
		if (!transferType.toLowerCase().equals("sync"))
			setExchangeRoleValue();
		else
			setRValue(roObj.getValue());
		setTempName(name);
		if (roleNameMap.containsKey(tempName))
			tempName = roleNameMap.get(tempName);
		data.put(tempName, rValue);
	}
	
	public void getExchRelatedItemsDetails() {
		for (ClassObject clo : classObjectList) {
			if (!clo.getClassId().equals(classId))
				continue;
			settemplatObjectList(clo.getTemplateObjects().getItems());
			tObj = templatObjectList.get(0);
			if (tObj.getTransferType() == null)
				transferType = "sync";
			else
				transferType = tObj.getTransferType().value();
			data.put("TransferType", transferType);
			for (RoleObject roleObject : tObj.getRoleObjects().getItems()) {
				rType = roleObject.getType().toString();
				if (!(rType.equals("OBJECT_PROPERTY")
						|| rType.equals("DATA_PROPERTY") || rType
						.equals("PROPERTY")))
					continue;				

				setRoObj(roleObject);
				tName = roleObject.getValue();
				if (!transferType.toLowerCase().equals("sync")) {
					setExchangeRoleValue();
				}else
					setRValue(tName);
				data.put("Identifier", addHtmlUnderLine(tName));
				addDetailRelRow(tObj.getName(), rValue);
				return;
			}
		}
	}
	
	

	public void fillExchDetailRelPage() {
		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();			
			setclassObjectList(dto.getClassObjects().getItems());
			getExchRelatedItemsDetails();
		}
	}

	public void fillDetailRelPage() {
		for (DataTransferObject dto : dtoList) {
			data = new HashMap<String, String>();
			setclassObjectList(dto.getClassObjects().getItems());
			getRelatedItemsDetails();
		}
	}

	public void addDetailRelRow(String x, String y) {
		data.put(x, y);
		dataList.add(data);
		total++;
	}

	public void getClassName() {
		ti = 0;
		for (DataTransferObject dto : dtoList) {
			setclassObjectList(dto.getClassObjects().getItems());
			for (ClassObject clo : classObjectList) {
				settemplatObjectList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : templatObjectList) {
					setTName(tObj.getName());
					setroleObjectList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roleObjectList) {
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

	public void getRelatedItems() {
		for (ClassObject clo : classObjectList) {
			if (hList.size() > 0) {
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
			settemplatObjectList(clo.getTemplateObjects().getItems());
			for (TemplateObject tObj : templatObjectList) {
				setTName(tObj.getName());
				setroleObjectList(tObj.getRoleObjects().getItems());
				for (RoleObject rObj : roleObjectList) {
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

	public void fillRelPage() {
		total = 0;
		initialHList();
		for (DataTransferObject dto : dtoList) {
			setclassObjectList(dto.getClassObjects().getItems());
			getRelatedItems();
		}
	}

	public void fillRelRelationPage() {
		total = 0;
		initialHList();
		for (DataTransferObject dto : dtoList) {
			setclassObjectList(dto.getClassObjects().getItems());
			getRelatedRelationItems();
		}
	}

	public void getRelatedRelationItems() {
		for (ClassObject clo : classObjectList) {
			if (clo.getClassId().equals(classId)
					&& clo.getIdentifier().equals(relatedId)) {
				settemplatObjectList(clo.getTemplateObjects().getItems());
				for (TemplateObject tObj : templatObjectList) {
					setTName(tObj.getName());
					setroleObjectList(tObj.getRoleObjects().getItems());
					for (RoleObject rObj : roleObjectList) {
						setRType(rObj.getType().toString());
						if (rType.equals("REFERENCE")) {
							setRcName(rObj.getRelatedClassName());
							if (rcName != null) {
								addRelRow(rcName);
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
		data.put("label", addHtmlUnderLine(head));
		dataList.add(data);
		total++;
	}

	public void setTotal(int total) {
		DtoContainer.total = total;
	}

	public void setGridList(Grid grid) {
		List<Header> ghList = new ArrayList<Header>(); // list of headers
		List<Filter> filterList = new ArrayList<Filter>();
		List<Column> cList = new ArrayList<Column>(); // list of column
		Filter filter;
		Column column;
		Header header;
		double width;
		
		for (String head : hList) {
			column = new Column();
			column.setDataIndex(head);
			column.setHeader(head.replace('_', '.'));
			column.setId(head);
			column.setSortable("true");
			width = head.length();
			if (width < 20)
				width = 110;
			else
				width = width + 150;
			column.setWidth(width);
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
		grid.setColumnData(cList);
		grid.setHeaderLists(ghList);
		grid.setCacheData("true"); // Add rules in the future
		grid.setSortBy(hList.get(0));
		grid.setSortOrder("DESC");
		if (filterList.size() > 0 && cList.size() > 0 && ghList.size() > 0)
			grid.setSuccess("true");
		grid.setPageSize(page);
		grid.setClassObjName(clsName);
	}

	public void setRowsList(Rows rows) {
		rows.setData(dataList);
		rows.setSuccess("true");
		rows.setTotal(total);
	}

	

	public void readGrid(Grid grid) {
		// TODO: Read the grid!
	}
}
