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
  
  // paging is based on number of DTIs
  public Grid getPageDto(String scope, String app, String graph, List<DataTransferIndex> dtiPage) throws JAXBException, IOException
  {
    DataTransferIndices dti = new DataTransferIndices();
    DataTransferIndexList dtiList = new DataTransferIndexList();
    dtiList.setItems(dtiPage);
    dti.setDataTransferIndexList(dtiList);
    
    String serviceUri = settings.get("DXFRServiceUri");
    HttpClient httpClient = new HttpClient(serviceUri);
    String relativePath = "/" + scope + "/" + app + "/" + graph + "/page";   
    DataTransferObjects dto = httpClient.post(DataTransferObjects.class, relativePath, dti);     
    return dtoToGrid(scope, app, graph, dto.getDataTransferObjectList().getItems());
  }
  
  public Grid dtoToGrid(String scope, String app, String graph, List<DataTransferObject> dtoList)
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
                
                row.add(roleObject.getValue());
              }
              else if (firstDto && roleObject.getRelatedClassName() != null && 
                       roleObject.getRelatedClassName().length() > 0)
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
  
  // paging is based on number of templates of the related class
  public Grid getPageRelatedClass(List<DataTransferObject> dtoList, String dtoIdentifier, String relatedClassId,
      int start, int limit)
  {
    Grid relatedClassGrid = new Grid();
    
    for (DataTransferObject dto : dtoList)
    {
      if (dto.getIdentifier().equalsIgnoreCase(dtoIdentifier))
      {
        for (ClassObject classObject : dto.getClassObjects().getItems())
        {
          if (classObject.getClassId().equalsIgnoreCase(relatedClassId))
          {
            List<Field> fields = new ArrayList<Field>();  
            List<List<String>> gridData = new ArrayList<List<String>>();  
            
            HashMap<String, String> relatedClasses = new HashMap<String, String>();
            int propertyTemplateCount = 0;
            boolean firstTemplateObject = true;
            
            for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
            {
              List<String> row = new ArrayList<String>();      
              row.add("<input type=\"image\" src=\"resources/images/info-small.png\" " +
                  "onClick=\"javascript:showIndividualInfo('" + templateObject.getName() + "','" + 
                  templateObject.getTemplateId() + "','" + dto.getIdentifier() + "')\">");
              
              for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
              {
                if (roleObject.getType() == RoleType.PROPERTY ||
                    roleObject.getType() == RoleType.DATA_PROPERTY ||
                    roleObject.getType() == RoleType.OBJECT_PROPERTY)
                {
                  if (firstTemplateObject)
                  {
                    Field field = new Field();
                    field.setName(templateObject.getName() + "." + roleObject.getName());                
                    field.setType(roleObject.getDataType().replace("xsd:", "")); 
                    fields.add(field);
                  }
                  
                  row.add(roleObject.getValue());            
                  propertyTemplateCount++;
                }            
                else if (roleObject.getRelatedClassId() != null && 
                         roleObject.getRelatedClassId().length() > 0)
                {
                  relatedClasses.put(roleObject.getRelatedClassId(), roleObject.getRelatedClassName());
                }    
              }
              
              gridData.add(row);
              
              if (firstTemplateObject) {
                Field infoField = new Field();
                infoField.setName("&nbsp;");                
                infoField.setType("string"); 
                infoField.setWidth(28);
                infoField.setFixed(true);
                fields.add(0, infoField);
                
                firstTemplateObject = false;
              }
            }
            
            relatedClassGrid.setType(classObject.getName()); 
            relatedClassGrid.setFields(fields);
            relatedClassGrid.setData(gridData.subList(start, limit));           
            relatedClassGrid.setTotal(propertyTemplateCount);
            relatedClassGrid.setProperties(relatedClasses);
            
            return relatedClassGrid;
          }
        }
      }
    }
    
    return relatedClassGrid;
  }
}
