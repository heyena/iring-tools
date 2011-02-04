package org.iringtools.models;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.apache.struts2.json.JSONException;
import org.apache.struts2.json.JSONUtil;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expression;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.LogicalOperator;
import org.iringtools.data.filter.OrderExpression;
import org.iringtools.data.filter.OrderExpressions;
import org.iringtools.data.filter.RelationalOperator;
import org.iringtools.data.filter.SortOrder;
import org.iringtools.data.filter.Values;
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
import org.iringtools.utility.IOUtil;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.grid.RelatedClass;

public class DataModel
{
  public static enum DataType {
    APP, EXCHANGE
  };

  public static Map<String, RelationalOperator> relationalOperatorMap;
  static
  {
    relationalOperatorMap = new HashMap<String, RelationalOperator>();
    relationalOperatorMap.put("eq", RelationalOperator.EQUAL_TO);
    relationalOperatorMap.put("lt", RelationalOperator.LESSER_THAN);
    relationalOperatorMap.put("gt", RelationalOperator.GREATER_THAN);
  }

  private static final Logger logger = Logger.getLogger(DataModel.class);
  protected Map<String, Object> session;

  protected void removeSessionData(String key)
  {
    if (session != null && session.keySet().contains(key))
      session.remove(key);
  }

  protected DataTransferIndices getDtis(String serviceUri, String relativePath, String filter, String sortOrder,
      String sortBy)
  {
    DataTransferIndices dtis = new DataTransferIndices();
    String fullDtiKey = "dti-full" + relativePath;
    String partDtiKey = "dti-part" + relativePath;

    try
    {
      DataFilter dataFilter = createDataFilter(filter, sortOrder, sortBy);

      if (dataFilter == null)
      {
        if (session.containsKey(partDtiKey))
        {
          session.remove(partDtiKey);
        }

        HttpClient httpClient = new HttpClient(serviceUri);
        dtis = httpClient.post(DataTransferIndices.class, relativePath, new DataFilter());
        session.put(fullDtiKey, dtis);
      }
      else if (session.containsKey(partDtiKey))
      {
        dtis = (DataTransferIndices) session.get(partDtiKey);
      }
      else
      {
        HttpClient httpClient = new HttpClient(serviceUri);
        dtis = httpClient.post(DataTransferIndices.class, relativePath, dataFilter);
        session.put(partDtiKey, dtis);
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return dtis;
  }

  protected DataTransferIndices getCachedDtis(String relativePath)
  {
    String dtiKey = "dti-part" + relativePath;

    if (!session.containsKey(dtiKey))
    {
      dtiKey = "dti-full" + relativePath;
    }

    return (DataTransferIndices) session.get(dtiKey);
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
      String filter, String sortOrder, String sortBy, int start, int limit)
  {
    DataTransferIndices dtis = getDtis(serviceUri, dtiRelativePath, filter, sortOrder, sortBy);
    List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
    int actualLimit = Math.min(start + limit, dtiList.size());
    List<DataTransferIndex> pageDtis = dtiList.subList(start, actualLimit);
    return getDtos(serviceUri, dtoRelativePath, pageDtis);
  }

  // TODO: use filter, sort, and start/limit for related individual
  public DataTransferObject getDto(String serviceUri, String dtiRelativePath, String dtoRelativePath,
      String dtoIdentifier, String filter, String sortOrder, String sortBy, int start, int limit)
  {
    DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
    List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();

    for (DataTransferIndex dti : dtiList)
    {
      if (dti.getIdentifier().equals(dtoIdentifier))
      {
        DataTransferIndices dtiRequest = new DataTransferIndices();
        DataTransferIndexList dtiRequestList = new DataTransferIndexList();
        dtiRequest.setDataTransferIndexList(dtiRequestList);
        dtiRequestList.getItems().add(dti);

        try
        {
          HttpClient httpClient = new HttpClient(serviceUri);
          DataTransferObjects dtos = httpClient.post(DataTransferObjects.class, dtoRelativePath, dtiRequest);

          if (dtos != null && dtos.getDataTransferObjectList().getItems().size() > 0)
            return dtos.getDataTransferObjectList().getItems().get(0);
        }
        catch (HttpClientException ex)
        {
          logger.error("Error in getDto: " + ex);
        }

        break;
      }
    }

    return null;
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
        pageDtoGrid.setIdentifier(classObject.getClassId());
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
                String name = templateObject.getName() + "." + roleObject.getName();

                Field field = new Field();
                field.setName(name);
                field.setDataIndex(IOUtil.toCamelCase(classObject.getName()) + '.' + name);

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
          transferTypeField.setDataIndex("Transfer Type");
          transferTypeField.setType("string");
          fields.add(0, transferTypeField);
        }

        Field infoField = new Field();
        infoField.setName("&nbsp;");
        infoField.setDataIndex("&nbsp;");
        infoField.setType("string");
        infoField.setWidth(28);
        infoField.setFixed(true);
        infoField.setFilterable(false);
        fields.add(0, infoField);

        pageDtoGrid.setFields(fields);
        firstDto = false;
      }
    }

    return pageDtoGrid;
  }

  // paging is based on number of templates of the related class
  public Grid getRelatedItemGrid(DataType dataType, DataTransferObject dto, String classId, String classIdentifier,
      int start, int limit)
  {
    Grid relatedItemGrid = new Grid();

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
                String name = templateObject.getName() + "." + roleObject.getName();

                Field field = new Field();
                field.setName(name);
                field.setDataIndex(IOUtil.toCamelCase(classObject.getName()) + '.' + name);

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
              + "onClick='javascript:showIndividualInfo(\"" + classObject.getIdentifier() + "\"," + relatedClassesJson
              + ")'>");

          gridData.add(row);

          if (firstTemplateObject)
          {
            if (dataType == DataType.EXCHANGE)
            {
              Field transferTypeField = new Field();
              transferTypeField.setName("Transfer Type");
              transferTypeField.setDataIndex("Transfer Type");
              transferTypeField.setType("string");
              fields.add(0, transferTypeField);
            }

            Field infoField = new Field();
            infoField.setName("&nbsp;");
            infoField.setDataIndex("&nbsp;");
            infoField.setType("string");
            infoField.setWidth(28);
            infoField.setFixed(true);
            infoField.setFilterable(false);
            fields.add(0, infoField);

            firstTemplateObject = false;
          }
        }

        int total = classObject.getTemplateObjects().getItems().size();
        int actualLimit = Math.min(start + limit, total);

        relatedItemGrid.setIdentifier(classObject.getClassId());
        relatedItemGrid.setDescription(classObject.getName());
        relatedItemGrid.setTotal(total);
        relatedItemGrid.setFields(fields);
        relatedItemGrid.setData(gridData.subList(start, actualLimit));

        return relatedItemGrid;
      }
    }

    return relatedItemGrid;
  }

  private DataFilter createDataFilter(String filter, String sortOrder, String sortBy)
  {
    DataFilter dataFilter = null;

    // process filtering
    if (filter != null && filter.length() > 0)
    {
      try
      {
        @SuppressWarnings("unchecked")
        List<Map<String, String>> filterExpressions = (List<Map<String, String>>) JSONUtil.deserialize(filter);

        if (filterExpressions != null && filterExpressions.size() > 0)
        {
          dataFilter = new DataFilter();

          Expressions expressions = new Expressions();
          dataFilter.setExpressions(expressions);

          for (Map<String, String> filterExpression : filterExpressions)
          {
            Expression expression = new Expression();
            expressions.getItems().add(expression);

            if (expressions.getItems().size() > 1)
            {
              expression.setLogicalOperator(LogicalOperator.AND);
            }
            
            if (filterExpression.containsKey("comparison"))
            {
              String operator = filterExpression.get("comparison");
              expression.setRelationalOperator(relationalOperatorMap.get(operator));
            }
            else
            {
              expression.setRelationalOperator(relationalOperatorMap.get("eq"));
            }

            expression.setPropertyName(filterExpression.get("field"));

            Values values = new Values();
            expression.setValues(values);

            List<String> valueList = new ArrayList<String>();
            values.setValues(valueList);

            valueList.add(String.valueOf(filterExpression.get("value")));
          }
        }
      }
      catch (JSONException ex)
      {
        logger.error("Error deserializing filter: " + ex);
      }
    }

    // process sorting
    if (sortOrder != null || sortBy != null)
    {
      if (dataFilter == null)
        dataFilter = new DataFilter();

      OrderExpressions orderExpressions = new OrderExpressions();
      dataFilter.setOrderExpressions(orderExpressions);

      OrderExpression orderExpression = new OrderExpression();
      orderExpressions.getItems().add(orderExpression);

      if (sortOrder != null)
        orderExpression.setSortOrder(SortOrder.valueOf(sortOrder));

      if (sortBy != null)
        orderExpression.setPropertyName(sortBy);
    }

    return dataFilter;
  }
}
