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

public class ExchangeDataModel
{
  private HashMap<String, String> settings;
  
  public ExchangeDataModel(HashMap<String, String> settings)
  {
    this.settings = settings;    
  }
  
  public DataTransferIndices getDti(String scope, int exchangeId) throws JAXBException, IOException
  {
    String serviceUri = settings.get("ESBServiceUri");
    HttpClient httpClient = new HttpClient(serviceUri);
    return httpClient.get(DataTransferIndices.class, "/" + scope + "/exchanges/" + exchangeId);
  }
  
  public Grid getPageDto(String scope, int exchangeId, List<DataTransferIndex> dtiPage) throws JAXBException, IOException
  { 
    DataTransferIndices dti = new DataTransferIndices();
    DataTransferIndexList dtiList = new DataTransferIndexList();
    dtiList.setItems(dtiPage);
    dti.setDataTransferIndexList(dtiList);
    
    String serviceUri = settings.get("ESBServiceUri");
    HttpClient httpClient = new HttpClient(serviceUri);
    String relativePath = "/" + scope + "/exchanges/" + exchangeId;    
    DataTransferObjects dto = httpClient.post(DataTransferObjects.class, relativePath, dti);     
    return dtoToGrid(scope, exchangeId, dto.getDataTransferObjectList().getItems());
  }
  
  public Grid dtoToGrid(String scope, int exchangeId, List<DataTransferObject> dtoList)
  {
    Grid pageDtoGrid = new Grid();
    
    List<List<String>> gridData = new ArrayList<List<String>>();
    pageDtoGrid.setData(gridData); 
    
    HashMap<String, String> relatedClasses = new HashMap<String, String>();
    pageDtoGrid.setProperties(relatedClasses);
    
    List<Field> fields = new ArrayList<Field>();    
    boolean firstDto = true;
    
    for (DataTransferObject dto : dtoList)
    {
      List<String> row = new ArrayList<String>();      
      boolean firstClass = true;
      
      String transferType = dto.getTransferType().toString(); 
      row.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
      
      for (ClassObject classObject : dto.getClassObjects().getItems())
      {        
        if (firstClass) {
          pageDtoGrid.setType(classObject.getName());
          
          row.add(0, "<input type=\"image\" src=\"resources/images/info-small.png\" " +
              "onClick=\"javascript:showIndividualInfo('" + classObject.getName() + "','" + 
              classObject.getClassId() + "','" + dto.getIdentifier() + "')\">");
          
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
                  field.setName(templateObject.getName() + "." + roleObject.getName());                
                  field.setType(roleObject.getDataType().replace("xsd:", "")); 
                  fields.add(field);
                }
                
                if (roleObject.getOldValue() == null || roleObject.getOldValue().equals(roleObject.getValue()))
                  row.add(roleObject.getValue());
                else
                  row.add("<span class=\"change\">" + roleObject.getOldValue() + " -> " + roleObject.getValue() + "</span>");
              }
              else if (firstDto && roleObject.getRelatedClassId() != null && 
                       roleObject.getRelatedClassId().length() > 0)
              {
                relatedClasses.put(roleObject.getRelatedClassId(), roleObject.getRelatedClassName());
              }
            }
          }
          
          firstClass = false;
        }
      }
      
      gridData.add(row);     

      if (firstDto)
      {
        Field transferTypeField = new Field();
        transferTypeField.setName("Transfer Type");                
        transferTypeField.setType("string"); 
        fields.add(0, transferTypeField);
        
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
