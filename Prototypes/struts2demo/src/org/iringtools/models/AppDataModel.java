package org.iringtools.models;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import javax.xml.bind.JAXBException;

import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.RoleType;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.utility.HttpClient;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;

public class AppDataModel
{
  private HashMap<String, String> settings;
  
  public AppDataModel(HashMap<String, String> settings)
  {
    this.settings = settings;    
  }
  
  public DataTransferIndices getDti(String scope, String app, String graph) throws JAXBException, IOException
  {
    String serviceUri = settings.get("DXFRServiceUri").toString();
    HttpClient httpClient = new HttpClient(serviceUri);
    return httpClient.get(DataTransferIndices.class, "/" + scope + "/" + app + "/" + graph);
  }
  
  public Grid getPageDto(List<DataTransferIndex> dtiPage, String scope, String app, String graph) throws JAXBException, IOException
  {
    DataTransferIndices dti = new DataTransferIndices();
    DataTransferIndexList dtiList = new DataTransferIndexList();
    dtiList.setItems(dtiPage);
    dti.setDataTransferIndexList(dtiList);
    
    String serviceUri = settings.get("DXFRServiceUri");
    HttpClient httpClient = new HttpClient(serviceUri);
    String relativePath = "/" + scope + "/" + app + "/" + graph + "/page";   
    DataTransferObjects dto = httpClient.post(DataTransferObjects.class, relativePath, dti);     
    return dtoToGrid(dto.getDataTransferObjectList().getItems());
  }
  
  public Grid dtoToGrid(List<DataTransferObject> dtoList)
  {
    Grid pageDtoGrid = new Grid();
    
    List<List<String>> gridData = new ArrayList<List<String>>();  
    pageDtoGrid.setData(gridData);
    
    List<Field> fields = new ArrayList<Field>();
    boolean firstDto = true;
    
    for (DataTransferObject dto : dtoList)
    {
      List<String> row = new ArrayList<String>();
      ClassObject firstClassObject = null;
      
      for (ClassObject classObject : dto.getClassObjects().getItems())
      {
        if (firstClassObject == null) {
          pageDtoGrid.setType(classObject.getName());
          firstClassObject = classObject;
        }
        
        for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
        {
          for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
          {
            if (roleObject.getType() == RoleType.PROPERTY ||
                roleObject.getType() == RoleType.DATA_PROPERTY ||
                roleObject.getType() == RoleType.OBJECT_PROPERTY)
            {
              if (firstDto)
              {
                Field field = new Field();
                field.setName((classObject.getName() + "." + roleObject.getName()).toUpperCase());                
                field.setType(roleObject.getDataType().replace("xsd:", "")); 
                fields.add(field);
              }
              
              row.add(roleObject.getValue());
            }
          }
        }
      }
      
      row.add(0, "<input type=\"image\" src=\"resources/images/info-small.png\" onClick=\"javascript:getRelatedItems('" + firstClassObject.getClassId() + "','" + dto.getIdentifier() + "')\">");      
      gridData.add(row);

      if (firstDto)
      {        
        Field infoField = new Field();
        infoField.setName("&nbsp;");                
        infoField.setType("string"); 
        infoField.setWidth(28);
        infoField.setFixed(true);
        fields.add(0, infoField); 
        
        pageDtoGrid.setFields(fields);
        firstDto = false;
      }
    }
    
    return pageDtoGrid;
  }
}
