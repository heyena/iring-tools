package org.iringtools.models;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
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
import org.iringtools.refdata.response.Entity;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.IOUtils;
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

  protected String DTI_PREFIX = "dti-";
  protected String XLOGS_PREFIX = "xlogs-";
  protected String FULL_DTI_KEY_PREFIX = DTI_PREFIX + "full";
  protected String PART_DTI_KEY_PREFIX = DTI_PREFIX + "part";
  protected String FILTER_KEY_PREFIX = DTI_PREFIX + "filter-key";

  protected Map<String, Object> session;

  protected void removeSessionData(String key)
  {
    if (session != null && session.keySet().contains(key))
      session.remove(key);
  }

  private DataTransferIndices getFilteredDtis(DataFilter dataFilter, String relativePath, String serviceUri,
      String fullDtiKey)
  {
    DataTransferIndices resultDtis = new DataTransferIndices();
    Expression transferTypeExpression = null;
    OrderExpression transferTypeOrderExpression = null;

    // extract transfer type from expressions
    if (dataFilter.getExpressions() != null && dataFilter.getExpressions().getItems().size() > 0)
    {
      List<Expression> expressions = dataFilter.getExpressions().getItems();

      for (int i = 0; i < expressions.size(); i++)
      {
        Expression expression = expressions.get(i);

        if (expression.getPropertyName().equalsIgnoreCase("transfer type"))
        {
          transferTypeExpression = expressions.remove(i);

          if (expressions.size() > 0)
          {
            // remove logical operator of the next expression
            expressions.get(i).setLogicalOperator(null);
          }

          break;
        }
      }
    }

    // extract transfer type from order expressions
    if (dataFilter.getOrderExpressions() != null && dataFilter.getOrderExpressions().getItems().size() > 0)
    {
      List<OrderExpression> orderExpressions = dataFilter.getOrderExpressions().getItems();

      for (int i = 0; i < orderExpressions.size(); i++)
      {
        OrderExpression orderExpression = orderExpressions.get(i);

        if (orderExpression.getPropertyName().equalsIgnoreCase("transfer type"))
        {
          transferTypeOrderExpression = orderExpressions.remove(i);

          if (orderExpressions.size() > 0)
          {
            // remove logical operator of the next expression
            orderExpressions.get(i).setPropertyName(null);
            orderExpressions.get(i).setSortOrder(null);
          }

          break;
        }
      }
    }

    try
    {
      DataTransferIndices fullDtis = (DataTransferIndices) session.get(fullDtiKey);
      List<DataTransferIndex> fullDtiList = fullDtis.getDataTransferIndexList().getItems();

      List<DataTransferIndex> cloneFullDtiList = new ArrayList<DataTransferIndex>();
      cloneFullDtiList.addAll(fullDtiList);

      // apply transfer type filtering
      if (transferTypeExpression != null && cloneFullDtiList != null)
      {
        String value = transferTypeExpression.getValues().getValues().get(0);

        for (int i = 0; i < cloneFullDtiList.size(); i++)
        {
          if (!cloneFullDtiList.get(i).getTransferType().toString().equalsIgnoreCase(value))
          {
            cloneFullDtiList.remove(i--);
          }
        }
      }

      if ((dataFilter.getExpressions() != null && dataFilter.getExpressions().getItems().size() > 0)
          || dataFilter.getOrderExpressions() != null && dataFilter.getOrderExpressions().getItems().size() > 0)
      {
        boolean matched = false;
        List<DataTransferIndex> partialDtiList = null;
        
        HttpClient httpClient = new HttpClient(serviceUri);
        String destinationUri = relativePath + "?destination=source";
        resultDtis = httpClient.post(DataTransferIndices.class, destinationUri, dataFilter);

        if (resultDtis != null && resultDtis.getDataTransferIndexList() != null)
        {
          partialDtiList = resultDtis.getDataTransferIndexList().getItems();
          matched = parsePartialDtis(partialDtiList, cloneFullDtiList);
        }

        if (!matched)
        {
          destinationUri = relativePath + "?destination=target";
          DataTransferIndices targetDtis = httpClient.post(DataTransferIndices.class, destinationUri, dataFilter);

          if (targetDtis != null && targetDtis.getDataTransferIndexList() != null)
          {
            List<DataTransferIndex> targetDtiList = targetDtis.getDataTransferIndexList().getItems();

            for (DataTransferIndex targetDti : targetDtiList)
            {
              for (DataTransferIndex fullDti : cloneFullDtiList)
              {
                if (fullDti.getTransferType() == org.iringtools.dxfr.dti.TransferType.DELETE &&
                    fullDti.getIdentifier().equalsIgnoreCase(targetDti.getIdentifier()))
                {
                  partialDtiList.add(targetDti);
                  break;
                }                
              }
            }
            
            parsePartialDtis(partialDtiList, cloneFullDtiList);
          }
        }
      }
      else
      {        
        DataTransferIndexList resultDtiList = new DataTransferIndexList();
        resultDtis.setDataTransferIndexList(resultDtiList);
        resultDtiList.setItems(cloneFullDtiList);        
      }

      // apply sorting
      if (resultDtis.getDataTransferIndexList() != null && resultDtis.getDataTransferIndexList().getItems().size() > 0)
      {
        if (transferTypeOrderExpression != null)
        {
          final String sortDir = transferTypeOrderExpression.getSortOrder().toString().toLowerCase();
          
          Comparator<DataTransferIndex> comparator = new Comparator<DataTransferIndex>()
          {
            public int compare(DataTransferIndex dti1, DataTransferIndex dti2)
            {
              int compareValue = 0;
         
              if (sortDir.equals("asc"))
              {
                compareValue = dti1.getTransferType().toString().compareTo(dti2.getTransferType().toString());
              }
              else
              {
                compareValue = dti2.getTransferType().toString().compareTo(dti1.getTransferType().toString());
              }            
  
              return compareValue;
            }
          };
  
          Collections.sort(resultDtis.getDataTransferIndexList().getItems(), comparator);
        }
        else if (dataFilter.getOrderExpressions() != null && dataFilter.getOrderExpressions().getItems().size() > 0)
        {
          final String sortType = resultDtis.getSortType().toLowerCase();
          final String sortDir = resultDtis.getSortOrder().toLowerCase();
  
          Comparator<DataTransferIndex> comparator = new Comparator<DataTransferIndex>()
          {
            public int compare(DataTransferIndex dti1, DataTransferIndex dti2)
            {
              int compareValue = 0;
  
              if (sortType.equals("string") || sortType.contains("date") || sortType.contains("time"))
              {
                if (sortDir.equals("asc"))
                {
                  compareValue = dti1.getSortIndex().compareTo(dti2.getSortIndex());
                }
                else
                {
                  compareValue = dti2.getSortIndex().compareTo(dti1.getSortIndex());
                }
              }
              else // sort type is numeric
              {
                if (sortDir.equals("asc"))
                {
                  compareValue = (int) (Double.parseDouble(dti1.getSortIndex()) - Double.parseDouble(dti2.getSortIndex()));
                }
                else
                {
                  compareValue = (int) (Double.parseDouble(dti2.getSortIndex()) - Double.parseDouble(dti1.getSortIndex()));
                }
              }
  
              return compareValue;
            }
          };
  
          Collections.sort(resultDtis.getDataTransferIndexList().getItems(), comparator);
        }
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }
    
    return resultDtis;
  }

  private boolean parsePartialDtis(List<DataTransferIndex> partialDtiList, List<DataTransferIndex> fullDtiList)
  {
    int count = 0;

    for (int i = 0; i < partialDtiList.size(); i++)
    {
      boolean exists = false;
      
      for (int j = 0; j < fullDtiList.size(); j++)
      {
        if (partialDtiList.get(i).getIdentifier().equalsIgnoreCase(fullDtiList.get(j).getIdentifier()))
        {
          partialDtiList.get(i).setTransferType(fullDtiList.get(j).getTransferType());

          if (!partialDtiList.get(i).getHashValue().equalsIgnoreCase(fullDtiList.get(j).getHashValue()))
            fullDtiList.get(j).setHashValue(partialDtiList.get(i).getHashValue());

          exists = true;
          count++;
          break;
        }
      }
      
      if (!exists)
      {
        partialDtiList.remove(i--);
      }
    }
    
    return count == fullDtiList.size();
  }

  // only cache full dti and one filtered dti
  protected DataTransferIndices getDtis(DataType dataType, String serviceUri, String relativePath, String filter,
      String sortBy, String sortOrder)
  {
    DataTransferIndices dtis = new DataTransferIndices();
    String fullDtiKey = FULL_DTI_KEY_PREFIX + relativePath;
    String partDtiKey = PART_DTI_KEY_PREFIX + relativePath;
    String lastFilterKey = FILTER_KEY_PREFIX + relativePath;
    String currFilter = filter + "/" + sortBy + "/" + sortOrder;

    try
    {      
      DataFilter dataFilter = createDataFilter(filter, sortBy, sortOrder);

      if (dataFilter == null)
      {
        if (dataType == DataType.EXCHANGE)
        {
          HttpClient httpClient = new HttpClient(serviceUri);
          dtis = httpClient.get(DataTransferIndices.class, relativePath);
          session.put(fullDtiKey, dtis);
        }
        else
        {
          HttpClient httpClient = new HttpClient(serviceUri);
          dtis = httpClient.post(DataTransferIndices.class, relativePath, new DataFilter());
          session.put(fullDtiKey, dtis);
        }

        if (session.containsKey(partDtiKey))
        {
          session.remove(partDtiKey);
        }

        if (session.containsKey(lastFilterKey))
        {
          session.remove(lastFilterKey);
        }
      }
      else if (session.containsKey(lastFilterKey))
      {
        String lastFilter = (String) session.get(lastFilterKey);

        // filter has not changed
        if (lastFilter.equals(currFilter))
        {
          dtis = (DataTransferIndices) session.get(partDtiKey);
        }
        else  // filter does not exist or has changed
        {
          if (dataType == DataType.EXCHANGE) // exchange data
          {
            dtis = getFilteredDtis(dataFilter, relativePath, serviceUri, fullDtiKey);
          }
          else
          {
            HttpClient httpClient = new HttpClient(serviceUri);
            dtis = httpClient.post(DataTransferIndices.class, relativePath, dataFilter);
          }
          if (dtis != null && dtis.getDataTransferIndexList() != null
              && dtis.getDataTransferIndexList().getItems().size() > 0)
          {
            session.put(partDtiKey, dtis);
            session.put(lastFilterKey, currFilter);
          }
        }
      }
      else
      {
        if (dataType == DataType.EXCHANGE) // exchange data
        {
          dtis = getFilteredDtis(dataFilter, relativePath, serviceUri, fullDtiKey);
        }
        else // app data
        {
          HttpClient httpClient = new HttpClient(serviceUri);
          dtis = httpClient.post(DataTransferIndices.class, relativePath, dataFilter);
        }

        if (dtis != null && dtis.getDataTransferIndexList() != null
            && dtis.getDataTransferIndexList().getItems().size() > 0)
        {
          session.put(partDtiKey, dtis);
          session.put(lastFilterKey, currFilter);
        }
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
    String dtiKey = PART_DTI_KEY_PREFIX + relativePath;

    if (!session.containsKey(dtiKey))
    {
      dtiKey = FULL_DTI_KEY_PREFIX + relativePath;
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

  public DataTransferObjects getPageDtos(DataType dataType, String serviceUri, String dtiRelativePath,
      String dtoRelativePath, String filter, String sortBy, String sortOrder, int start, int limit)
  {
    DataTransferIndices dtis = getDtis(dataType, serviceUri, dtiRelativePath, filter, sortBy, sortOrder);
    List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
    int actualLimit = Math.min(start + limit, dtiList.size());
    List<DataTransferIndex> pageDtis = dtiList.subList(start, actualLimit);
    return getDtos(serviceUri, dtoRelativePath, pageDtis);
  }

  // TODO: use filter, sort, and start/limit for related individual
  public DataTransferObject getDto(String serviceUri, String dtiRelativePath, String dtoRelativePath,
      String dtoIdentifier, String filter, String sortBy, String sortOrder, int start, int limit)
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

  public String resolveValueMap(String refServiceUri, String id)
  {
    String label = id;

    try
    {
      HttpClient httpClient = new HttpClient(refServiceUri);
      Entity value = httpClient.get(Entity.class, "/classes/" + id.substring(4, id.length()) + "/label");
      
      if (value != null)
      {
        label = value.getLabel();
      }
    }
    catch (Exception e)
    {
      logger.error("Error in resolveValueMap:" + e);
    }

    return label;
  }

  // paging is based on number of data transfer objects
  public Grid getDtoGrid(DataType dataType, DataTransferObjects dtos, String refServiceUri)
  {
    Grid pageDtoGrid = new Grid();
    List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();

    List<List<String>> gridData = new ArrayList<List<String>>();
    pageDtoGrid.setData(gridData);

    List<Field> fields = new ArrayList<Field>();
    boolean firstDto = true;
    ClassObject classObject = null;
    
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
        classObject = dto.getClassObjects().getItems().get(0);
        String className = IOUtils.toCamelCase(classObject.getName());
        
        pageDtoGrid.setIdentifier(classObject.getClassId());
        pageDtoGrid.setDescription(className);

        for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
        {
          for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
          {
            if (roleObject.getType() == RoleType.PROPERTY || roleObject.getType() == RoleType.DATA_PROPERTY
                || roleObject.getType() == RoleType.OBJECT_PROPERTY)
            {
              if (firstDto)
              {
                String fieldName = templateObject.getName() + "." + roleObject.getName();

                Field field = new Field();
                field.setName(fieldName);
                field.setDataIndex(className + '.' + fieldName);

                if (dataType == DataType.APP)
                  field.setType(roleObject.getDataType().replace("xsd:", ""));
                else
                  field.setType("string");

                fields.add(field);
              }

              if (roleObject.getHasValueMap() != null && roleObject.getHasValueMap())
              {
                String roleObjectValue = roleObject.getValue();
                String newValue = getValueMap(refServiceUri, roleObjectValue);
                roleObject.setValue(newValue);
                logger.debug("roleObjectValue: " + roleObjectValue);
                logger.debug("refServiceUri: " + refServiceUri);
                logger.debug("newValue: " + newValue);
                
                if (dataType == DataType.EXCHANGE) 
                {
                  String roleObjectOldValue = roleObject.getOldValue();
                  roleObject.setOldValue(getValueMap(refServiceUri, roleObjectOldValue));
                }
              }

              if (dataType == DataType.APP || roleObject.getOldValue() == null
                  || roleObject.getOldValue().equals(roleObject.getValue()))
              {
                row.add(roleObject.getValue());
              }
              else
              {
                row.add("<span class=\"change\">" + roleObject.getOldValue() + " -> " + roleObject.getValue() + "</span>");
              }
            }
            else if (roleObject.getRelatedClassName() != null && roleObject.getRelatedClassName().length() > 0)
            {
              RelatedClass relatedClass = new RelatedClass();
              relatedClass.setId(roleObject.getRelatedClassId());
              relatedClass.setName(IOUtils.toCamelCase(roleObject.getRelatedClassName()));
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
          + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\"," + 
          relatedClassesJson + ")'>");

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
    List<Field> fields = new ArrayList<Field>();
    List<List<String>> gridData = new ArrayList<List<String>>();    
    int relatedClassCount = 0;

    for (ClassObject classObject : dto.getClassObjects().getItems())
    {
      if (classObject.getClassId().equals(classId))
      {
        List<String> row = new ArrayList<String>();
        String className = IOUtils.toCamelCase(classObject.getName());
        List<RelatedClass> nextRelatedClasses = new ArrayList<RelatedClass>();

        relatedClassCount++;
        
        if (relatedClassCount == 1)
        {
          relatedItemGrid.setIdentifier(classObject.getClassId());
          relatedItemGrid.setDescription(className);
        }
        
        for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
        {
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
            String roleValue = roleObject.getValue();
            
            if (roleValue != null && !roleValue.startsWith("rdl:"))
            {              
              if (relatedClassCount == 1)
              {
                String fieldName = templateObject.getName() + "." + roleObject.getName();

                Field field = new Field();
                field.setName(fieldName);
                field.setDataIndex(className + '.' + fieldName);

                String fieldType = roleObject.getDataType();
                
                if (dataType == DataType.APP && fieldType != null && fieldType.startsWith("xsd:"))
                  field.setType(fieldType.replace("xsd:", ""));
                else
                  field.setType("string");

                fields.add(field);
              }

              if (dataType == DataType.APP || roleObject.getOldValue() == null
                  || roleObject.getOldValue().equals(roleValue))
              {
                row.add(roleValue);
              }
              else
              {
                row.add("<span class=\"change\">" + roleObject.getOldValue() + " -> " + roleValue + "</span>");
              }
            }
            else if (roleObject.getRelatedClassName() != null && roleObject.getRelatedClassName().length() > 0)
            {              
              RelatedClass nextRelatedClass = new RelatedClass();
              nextRelatedClass.setId(roleObject.getRelatedClassId());
              nextRelatedClass.setName(IOUtils.toCamelCase(roleObject.getRelatedClassName()));
              nextRelatedClasses.add(nextRelatedClass);
            }
          }
        }
        
        if (relatedClassCount == 1)
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
        }
        
        String relatedClassesJson;

        try
        {
          relatedClassesJson = JSONUtil.serialize(nextRelatedClasses);
        }
        catch (JSONException ex)
        {
          relatedClassesJson = "[]";
        }

        row.add(0, "<input type=\"image\" src=\"resources/images/info-small.png\" "
            + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\"," +
            relatedClassesJson + ")'>");
        
        gridData.add(row);
      }
    }
    
    int actualLimit = Math.min(start + limit, relatedClassCount);

    relatedItemGrid.setTotal(relatedClassCount);
    relatedItemGrid.setFields(fields);    
    relatedItemGrid.setData(gridData.subList(start, actualLimit));

    return relatedItemGrid;
  }

  private String getValueMapKey(String value, HashMap<String, String> valueMaps)
  {
    for (String key : valueMaps.keySet())
      if (valueMaps.get(key).equalsIgnoreCase(value))
        return key;
    
    return null;
  }

  @SuppressWarnings("unchecked")
  private String getValueMap(String refServiceUri, String value)
  {
    Map<String, String> valueMaps;
    String valueMap = value;
    
    if (session.containsKey("valueMaps"))
    {
      valueMaps = (Map<String, String>)session.get("valueMaps");
    }
    else
    {
      valueMaps = new HashMap<String, String>();
      session.put("valueMaps", valueMaps);
    }

    if (value != null && !value.isEmpty())
    {
      if (!valueMaps.containsKey(value))
      {
        valueMap = resolveValueMap(refServiceUri, value);
        valueMaps.put(value, valueMap);
      }
      else
      {
        valueMap = valueMaps.get(value);
      }
    }
    
    return valueMap;
  }
  
  private DataFilter createDataFilter(String filter, String sortBy, String sortOrder)
  {
    DataFilter dataFilter = null;

    @SuppressWarnings("unchecked")
    HashMap<String, String> valueMaps = (HashMap<String, String>) session.get("valueMaps");
    
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

            List<String> valMaps = new ArrayList<String>();
            values.setValues(valMaps);

            String valueMap = getValueMapKey(String.valueOf(filterExpression.get("value")), valueMaps);
            if (valueMap != null && !valueMap.isEmpty())
              valMaps.add(valueMap);
            else
              valMaps.add(String.valueOf(filterExpression.get("value")));
          }
        }
      }
      catch (JSONException ex)
      {
        logger.error("Error deserializing filter: " + ex);
      }
    }

    // process sorting
    if (sortBy != null && sortBy.length() > 0 && sortOrder != null && sortOrder.length() > 0)
    {
      if (dataFilter == null)
        dataFilter = new DataFilter();

      OrderExpressions orderExpressions = new OrderExpressions();
      dataFilter.setOrderExpressions(orderExpressions);

      OrderExpression orderExpression = new OrderExpression();
      orderExpressions.getItems().add(orderExpression);

      if (sortBy != null)
        orderExpression.setPropertyName(sortBy);

      if (sortOrder != null)
        orderExpression.setSortOrder(SortOrder.valueOf(sortOrder));
    }

    return dataFilter;
  }
}
