package org.iringtools.models;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.apache.struts2.json.JSONException;
import org.apache.struts2.json.JSONUtil;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.RoleType;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.dxfr.dto.TransferType;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.grid.RelatedClass;

public class DataModel
{
  protected static enum DataType {
    APP, EXCHANGE
  };

  private static final Logger logger = Logger.getLogger(DataModel.class);
  protected Map<String, Object> session;
  
  protected void removeSessionData(String key)
  {
    if (session != null && session.keySet().contains(key))
      session.remove(key);
  }
  
  protected DataTransferIndices getDtis(String serviceUri, String relativePath)
  {
    DataTransferIndices dtis = new DataTransferIndices();
    String dtiKey = "dti" + relativePath;

    try
    {
      if (session.containsKey(dtiKey))
      {
        dtis = (DataTransferIndices) session.get(dtiKey);
      }
      else
      {
        HttpClient httpClient = new HttpClient(serviceUri);
        dtis = httpClient.get(DataTransferIndices.class, relativePath);
        session.put(dtiKey, dtis);
      }
    }
    catch (HttpClientException ex)
    {
      logger.error(ex);
    }
    
    return dtis;
  }

  protected DataTransferObjects getDtos(String serviceUri, String relativePath, List<DataTransferIndex> dtiList)
  {
    DataTransferObjects dtos = new DataTransferObjects();

    try
    {
      DataTransferIndices dti = new DataTransferIndices();
      DataTransferIndexList dtiListObj = new DataTransferIndexList();
      dtiListObj.setItems(dtiList);
      dti.setDataTransferIndexList(dtiListObj);

      HttpClient httpClient = new HttpClient(serviceUri);
      dtos = httpClient.post(DataTransferObjects.class, relativePath, dti);
    }
    catch (HttpClientException ex)
    {
      logger.error(ex);
    }

    return dtos;
  }

  public DataTransferObjects getPageDtos(String serviceUri, String dtiRelativePath, String dtoRelativePath, 
      int start, int limit) 
  {
    DataTransferIndices dtis = getDtis(serviceUri, dtiRelativePath);    
    List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
    int actualLimit = Math.min(start + limit, dtiList.size());
    List<DataTransferIndex> pageDtis = dtiList.subList(start, actualLimit);
    return getDtos(serviceUri, dtoRelativePath, pageDtis);
  }

  // paging is based on number of data transfer objects
  public Grid getDtoGrid(DataType dataType, DataTransferObjects dtos)
  {
    Grid pageDtoGrid = new Grid();
    List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();

    List<List<String>> gridData = new ArrayList<List<String>>();
    pageDtoGrid.setData(gridData);

    List<Field> fields = new ArrayList<Field>();
    boolean firstDto = true;

    for (DataTransferObject dto : dtoList)
    {
      List<String> row = new ArrayList<String>();
      List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();

      if (dataType == DataType.EXCHANGE)
      {
        String transferType = dto.getTransferType().toString();
        row.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
      }

      if (dto.getClassObjects().getItems().size() > 0)
      {
        ClassObject classObject = dto.getClassObjects().getItems().get(0);
        pageDtoGrid.setLabel(classObject.getClassId());
        pageDtoGrid.setDescription(classObject.getName());

        for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
        {
          for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
          {
            if (roleObject.getType() == RoleType.PROPERTY || roleObject.getType() == RoleType.DATA_PROPERTY
                || roleObject.getType() == RoleType.OBJECT_PROPERTY)
            {
              if (firstDto)
              {
                Field field = new Field();
                field.setName(templateObject.getName() + "." + roleObject.getName());
                
                if (dataType == DataType.APP)
                  field.setType(roleObject.getDataType().replace("xsd:", ""));
                else
                  field.setType("string");
                
                fields.add(field);
              }

              if (dataType == DataType.APP || roleObject.getOldValue() == null
                  || roleObject.getOldValue().equals(roleObject.getValue()))
              {
                row.add(roleObject.getValue());
              }
              else
              {
                row.add("<span class=\"change\">" + roleObject.getOldValue() + " -> " + roleObject.getValue()
                    + "</span>");
              }
            }
            else if (roleObject.getRelatedClassName() != null && roleObject.getRelatedClassName().length() > 0
                && roleObject.getValue() != null && roleObject.getValue().length() > 0)
            {
              RelatedClass relatedClass = new RelatedClass();
              relatedClass.setId(roleObject.getRelatedClassId());
              relatedClass.setName(roleObject.getRelatedClassName());
              relatedClass.setIdentifier(roleObject.getValue().substring(1));
              relatedClasses.add(relatedClass);
            }
          }
        }
      }

      String relatedClassesJson;

      try
      {
        relatedClassesJson = JSONUtil.serialize(relatedClasses);
      }
      catch (JSONException ex)
      {
        relatedClassesJson = "[]";
      }

      row.add(0, "<input type=\"image\" src=\"resources/images/info-small.png\" "
          + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\"," + relatedClassesJson + ")'>");

      gridData.add(row);

      if (firstDto)
      {
        if (dataType == DataType.EXCHANGE)
        {
          Field transferTypeField = new Field();
          transferTypeField.setName("Transfer Type");
          transferTypeField.setType("string");
          fields.add(0, transferTypeField);
        }

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
  public Grid getRelatedItemGrid(DataType dataType, DataTransferObjects dtos, String individual, String classId,
      String classIdentifier, int start, int limit)
  {
    Grid relatedItemGrid = new Grid();
    List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();

    for (DataTransferObject dto : dtoList)
    {
      if (dto.getIdentifier().equals(individual))
      {
        for (ClassObject classObject : dto.getClassObjects().getItems())
        {
          if (classObject.getClassId().equals(classId) && classObject.getIdentifier().equals(classIdentifier))
          {
            List<Field> fields = new ArrayList<Field>();
            List<List<String>> gridData = new ArrayList<List<String>>();
            boolean firstTemplateObject = true;

            for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
            {
              List<String> row = new ArrayList<String>();
              List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();

              if (dataType == DataType.EXCHANGE)
              {
                if (dto.getTransferType() == TransferType.CHANGE)
                {
                  String transferType = templateObject.getTransferType().toString();
                  row.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
                }
                else
                {
                  String transferType = dto.getTransferType().toString();
                  row.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
                }
              }

              for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
              {
                if (roleObject.getType() == RoleType.PROPERTY || roleObject.getType() == RoleType.DATA_PROPERTY
                    || roleObject.getType() == RoleType.OBJECT_PROPERTY)
                {
                  if (firstTemplateObject)
                  {
                    Field field = new Field();
                    field.setName(templateObject.getName() + "." + roleObject.getName());
                    
                    if (dataType == DataType.APP)
                      field.setType(roleObject.getDataType().replace("xsd:", ""));
                    else
                      field.setType("string");
                    
                    fields.add(field);
                  }

                  if (dataType == DataType.APP || roleObject.getOldValue() == null
                      || roleObject.getOldValue().equals(roleObject.getValue()))
                  {
                    row.add(roleObject.getValue());
                  }
                  else
                  {
                    row.add("<span class=\"change\">" + roleObject.getOldValue() + " -> " + roleObject.getValue()
                        + "</span>");
                  }
                }
                else if (roleObject.getRelatedClassName() != null && roleObject.getRelatedClassName().length() > 0
                    && roleObject.getValue() != null && roleObject.getValue().length() > 0)
                {
                  RelatedClass relatedClass = new RelatedClass();
                  relatedClass.setId(roleObject.getRelatedClassId());
                  relatedClass.setName(roleObject.getRelatedClassName());
                  relatedClass.setIdentifier(roleObject.getValue().substring(1));
                  relatedClasses.add(relatedClass);
                }
              }

              String relatedClassesJson;

              try
              {
                relatedClassesJson = JSONUtil.serialize(relatedClasses);
              }
              catch (JSONException ex)
              {
                relatedClassesJson = "[]";
              }

              row.add(0, "<input type=\"image\" src=\"resources/images/info-small.png\" "
                  + "onClick='javascript:showIndividualInfo(\"" + classObject.getIdentifier() + "\","
                  + relatedClassesJson + ")'>");

              gridData.add(row);

              if (firstTemplateObject)
              {
                if (dataType == DataType.EXCHANGE)
                {
                  Field transferTypeField = new Field();
                  transferTypeField.setName("Transfer Type");
                  transferTypeField.setType("string");
                  fields.add(0, transferTypeField);
                }

                Field infoField = new Field();
                infoField.setName("&nbsp;");
                infoField.setType("string");
                infoField.setWidth(28);
                infoField.setFixed(true);
                fields.add(0, infoField);

                firstTemplateObject = false;
              }
            }

            int total = classObject.getTemplateObjects().getItems().size();
            int actualLimit = Math.min(start + limit, total);

            relatedItemGrid.setLabel(classObject.getClassId());
            relatedItemGrid.setDescription(classObject.getName());
            relatedItemGrid.setTotal(total);
            relatedItemGrid.setFields(fields);
            relatedItemGrid.setData(gridData.subList(start, actualLimit));

            return relatedItemGrid;
          }
        }
      }
    }

    return relatedItemGrid;
  }
}
