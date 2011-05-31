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
import org.iringtools.dxfr.dto.RoleValues;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.dxfr.dto.TransferType;
import org.iringtools.dxfr.manifest.Cardinality;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.refdata.response.Entity;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.IOUtils;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.grid.RelatedClass;

public class DataModel
{
  public static enum Mode {
    APP, EXCHANGE
  };

  public static List<String> gridFilterTypes;
  static
  {
    gridFilterTypes = new ArrayList<String>();
    gridFilterTypes.add("short");
    gridFilterTypes.add("int");
    gridFilterTypes.add("long");
    gridFilterTypes.add("double");
    gridFilterTypes.add("float");
    gridFilterTypes.add("boolean");
    gridFilterTypes.add("date");
    gridFilterTypes.add("string");
  }
  
  public static Map<String, RelationalOperator> relationalOperatorMap;
  static
  {
    relationalOperatorMap = new HashMap<String, RelationalOperator>();
    relationalOperatorMap.put("eq", RelationalOperator.EQUAL_TO);
    relationalOperatorMap.put("lt", RelationalOperator.LESSER_THAN);
    relationalOperatorMap.put("gt", RelationalOperator.GREATER_THAN);
  }

  private static final Logger logger = Logger.getLogger(DataModel.class);

  protected final String MANIFEST_PREFIX = "manifest-";
  protected final String DTI_PREFIX = "dti-";
  protected final String XLOGS_PREFIX = "xlogs-";
  protected final String FULL_DTI_KEY_PREFIX = DTI_PREFIX + "full";
  protected final String PART_DTI_KEY_PREFIX = DTI_PREFIX + "part";
  protected final String FILTER_KEY_PREFIX = DTI_PREFIX + "filter-key";
  protected final int PIXELS_PER_CHAR = 8;
  protected final int MIN_COLUMN_WIDTH = 50;
  protected final int MAX_COLUMN_WIDTH = 300;

  protected Map<String, Object> session;

  protected void removeSessionData(String key)
  {
    if (session != null && session.keySet().contains(key))
      session.remove(key);
  }

  private DataTransferIndices getFilteredDtis(DataFilter dataFilter, String manifestRelativePath, String dtiRelativePath, String serviceUri,
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
        String value = transferTypeExpression.getValues().getItems().get(0);

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
        String destinationUri = dtiRelativePath + "?destination=source";
        
        DxiRequest dxiRequest = new DxiRequest();
        dxiRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
        dxiRequest.setDataFilter(dataFilter);
        
        resultDtis = httpClient.post(DataTransferIndices.class, destinationUri, dxiRequest);

        if (resultDtis != null && resultDtis.getDataTransferIndexList() != null)
        {
          partialDtiList = resultDtis.getDataTransferIndexList().getItems();
          matched = parsePartialDtis(partialDtiList, cloneFullDtiList);
        }

        if (!matched)
        {
          destinationUri = dtiRelativePath + "?destination=target";
          dxiRequest.setDataFilter(dataFilter);
          DataTransferIndices targetDtis = httpClient.post(DataTransferIndices.class, destinationUri, dxiRequest);

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
  protected DataTransferIndices getDtis(Mode mode, String serviceUri, String manifestRelativePath, String dtiRelativePath, String filter,
      String sortBy, String sortOrder)
  {
    DataTransferIndices dtis = new DataTransferIndices();
    String fullDtiKey = FULL_DTI_KEY_PREFIX + dtiRelativePath;
    String partDtiKey = PART_DTI_KEY_PREFIX + dtiRelativePath;
    String lastFilterKey = FILTER_KEY_PREFIX + dtiRelativePath;
    String currFilter = filter + "/" + sortBy + "/" + sortOrder;

    try
    {      
      DataFilter dataFilter = createDataFilter(filter, sortBy, sortOrder);

      if (dataFilter == null)
      {
        DxiRequest dxiRequest = new DxiRequest();
        dxiRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
        dxiRequest.setDataFilter(new DataFilter());
        
        HttpClient httpClient = new HttpClient(serviceUri);
        dtis = httpClient.post(DataTransferIndices.class, dtiRelativePath, dxiRequest);
        session.put(fullDtiKey, dtis);

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
          if (mode == Mode.EXCHANGE) // exchange data
          {
            dtis = getFilteredDtis(dataFilter, manifestRelativePath, dtiRelativePath, serviceUri, fullDtiKey);
          }
          else
          {
            DxiRequest dxiRequest = new DxiRequest();
            dxiRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
            dxiRequest.setDataFilter(dataFilter);
            
            HttpClient httpClient = new HttpClient(serviceUri);
            dtis = httpClient.post(DataTransferIndices.class, dtiRelativePath, dxiRequest);
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
        if (mode == Mode.EXCHANGE) // exchange data
        {
          dtis = getFilteredDtis(dataFilter, manifestRelativePath, dtiRelativePath, serviceUri, fullDtiKey);
        }
        else // app data
        {
          DxiRequest dxiRequest = new DxiRequest();
          dxiRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
          dxiRequest.setDataFilter(dataFilter);
          
          HttpClient httpClient = new HttpClient(serviceUri);
          dtis = httpClient.post(DataTransferIndices.class, dtiRelativePath, dxiRequest);
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

  protected DataTransferObjects getDtos(String serviceUri, String manifestRelativePath, String dtoRelativePath, List<DataTransferIndex> dtiList)
  {
    DataTransferObjects dtos = new DataTransferObjects();

    try
    {
      DataTransferIndices dtis = new DataTransferIndices();
      DataTransferIndexList dtiListObj = new DataTransferIndexList();
      dtiListObj.setItems(dtiList);
      dtis.setDataTransferIndexList(dtiListObj);
      
      DxoRequest dxoRequest = new DxoRequest();
      dxoRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
      dxoRequest.setDataTransferIndices(dtis);

      HttpClient httpClient = new HttpClient(serviceUri);
      dtos = httpClient.post(DataTransferObjects.class, dtoRelativePath, dxoRequest);
    }
    catch (HttpClientException ex)
    {
      logger.error(ex);
    }

    return dtos;
  }

  protected DataTransferObjects getPageDtos(Mode mode, String serviceUri, String manifestRelativePath, String dtiRelativePath,
      String dtoRelativePath, String filter, String sortBy, String sortOrder, int start, int limit)
  {
    DataTransferIndices dtis = getDtis(mode, serviceUri, manifestRelativePath, dtiRelativePath, filter, sortBy, sortOrder);
    List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
    int actualLimit = Math.min(start + limit, dtiList.size());
    List<DataTransferIndex> pageDtis = dtiList.subList(start, actualLimit);
    return getDtos(serviceUri, manifestRelativePath, dtoRelativePath, pageDtis);
  }

  protected DataTransferObjects getRelatedDtos(String serviceUri, String manifestRelativePath, String dtiRelativePath, 
      String dtoRelativePath, String dtoIdentifier, String filter, String sortBy, String sortOrder, int start, int limit)
  {
    DataTransferObjects relatedDtos = null;
    DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
    List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();

    for (DataTransferIndex dti : dtiList)
    {
      if (dti.getIdentifier().equals(dtoIdentifier))
      {
        DataTransferIndices requestDtis = new DataTransferIndices();
        DataTransferIndexList dtiRequestList = new DataTransferIndexList();
        requestDtis.setDataTransferIndexList(dtiRequestList);
        dtiRequestList.getItems().add(dti);

        DxoRequest dxoRequest = new DxoRequest();
        dxoRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
        dxoRequest.setDataTransferIndices(requestDtis);
        
        try
        {
          HttpClient httpClient = new HttpClient(serviceUri);
          relatedDtos = httpClient.post(DataTransferObjects.class, dtoRelativePath, dxoRequest);

          // apply filter          
          if (relatedDtos != null)
          {
            List<DataTransferObject> dtoList = relatedDtos.getDataTransferObjectList().getItems();
          
            if (dtoList.size() > 0 && filter != null && filter.length() > 0)
            {
              DataFilter dataFilter = createDataFilter(filter, sortBy, sortOrder);              
              List<Expression> expressions = dataFilter.getExpressions().getItems();
              
              for (Expression expression : expressions)
              {
                for (DataTransferObject dto : dtoList)
                {
                  List<ClassObject> classObjects = dto.getClassObjects().getItems();                  
                  dto.getClassObjects().setItems(getFilteredClasses(expression, classObjects));                  
                }
              }
            }
          }
        }
        catch (HttpClientException ex)
        {
          logger.error("Error in getDto: " + ex);
        }

        break;
      }
    }
    
    if (relatedDtos != null)
    {
      List<DataTransferObject> dtos = relatedDtos.getDataTransferObjectList().getItems();
      int actualLimit = Math.min(start + limit, dtos.size());
      List<DataTransferObject> pageRelatedDtos = dtos.subList(start, actualLimit);
      relatedDtos.getDataTransferObjectList().setItems(pageRelatedDtos);
    }

    return relatedDtos;
  }
  
  private List<ClassObject> getFilteredClasses(Expression expression, List<ClassObject> classObjects)
  {
    List<ClassObject> filteredClassObjects = new ArrayList<ClassObject>();
    String[] propertyParts = expression.getPropertyName().split("\\.");    
    
    for (ClassObject classObject : classObjects)
    {
      if (classObject.getName().equalsIgnoreCase(propertyParts[0]))
      {
        List<TemplateObject> templateObjects = classObject.getTemplateObjects().getItems();
        
        for (TemplateObject templateObject : templateObjects)
        {
          if (templateObject.getName().equalsIgnoreCase(propertyParts[1]))
          {
            List<RoleObject> roleObjects = templateObject.getRoleObjects().getItems();
            
            for (RoleObject roleObject : roleObjects)
            {
              if ((roleObject.getType() == RoleType.PROPERTY ||
                  roleObject.getType() == RoleType.DATA_PROPERTY ||
                  roleObject.getType() == RoleType.OBJECT_PROPERTY ||
                  roleObject.getType() == RoleType.FIXED_VALUE) &&
                  roleObject.getName().equalsIgnoreCase(propertyParts[2]))
              {
                int compareValue = roleObject.getValue().compareToIgnoreCase(expression.getValues().getItems().get(0));
                RelationalOperator relationalOperator = expression.getRelationalOperator();
                
                if ((relationalOperator == RelationalOperator.EQUAL_TO && compareValue == 0) ||
                    (relationalOperator == RelationalOperator.GREATER_THAN && compareValue > 0) ||
                    (relationalOperator == RelationalOperator.LESSER_THAN && compareValue < 0))                    
                {
                  filteredClassObjects.add(classObject);
                }
                  
                break;
              }
            }                          
          }                        
        }
      }
    }
    
    return filteredClassObjects;
  }

  private String resolveValueMap(String refServiceUri, String id)
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
  
  protected Grid getDtoGrid(Mode mode, Graph graph, DataTransferObjects dtos, String refServiceUri)
  {
    Grid dtoGrid = new Grid();
    
    List<Field> fields = createFields(mode, graph);
    dtoGrid.setFields(fields);
    
    List<List<String>> gridData = new ArrayList<List<String>>();
    dtoGrid.setData(gridData);

    List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();

    for (int dtoIndex = 0; dtoIndex < dtoList.size(); dtoIndex ++)
    {
      DataTransferObject dto = dtoList.get(dtoIndex);
      List<String> rowData = new ArrayList<String>();
      List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();

      // create a place holder for info field
      rowData.add("");

      if (mode == Mode.EXCHANGE)
      {
        String transferType = dto.getTransferType().toString();
        rowData.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
      }
      
      if (dto.getClassObjects().getItems().size() > 0)
      {
        ClassObject classObject = dto.getClassObjects().getItems().get(0);
        String className = IOUtils.toCamelCase(classObject.getName());
      
        dtoGrid.setIdentifier(classObject.getClassId());
        dtoGrid.setDescription(className);
        
        processClassObject(dto, dtoIndex, fields, mode, classObject, dtoGrid, rowData, relatedClasses, refServiceUri);
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

      // update info field
      rowData.set(0, "<input type=\"image\" src=\"resources/images/info-small.png\" "
          + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\",\"" + 
          dto.getIdentifier() + "\"," + relatedClassesJson + ")'>");

      gridData.add(rowData);
    }

    return dtoGrid;
  }
  
  // fields are column headers
  private List<Field> createFields(Mode mode, Graph graph)
  {
    List<Field> fields = new ArrayList<Field>();
    List<ClassTemplates> classTemplatesItems = graph.getClassTemplatesList().getItems();
    
    // transfer-type field
    if (mode == Mode.EXCHANGE)
    {
      Field field = new Field();
      field.setName("Status");
      field.setDataIndex("Transfer Type");
      field.setType("string");
      field.setWidth(60);
      field.setFixed(true);
      fields.add(0, field);
    }

    // info field
    Field field = new Field();
    field.setName("&nbsp;");
    field.setDataIndex("&nbsp;");
    field.setType("string");
    field.setWidth(28);
    field.setFixed(true);
    field.setFilterable(false);
    fields.add(0, field);
    
    if (classTemplatesItems.size() > 0)
    {
      ClassTemplates classTemplates = classTemplatesItems.get(0);
      createFields(mode, fields, graph, classTemplates);      
    }
    
    return fields;
  }
  
  private void createFields(Mode mode, List<Field> fields, Graph graph, ClassTemplates classTemplates)
  {
    if (classTemplates != null && classTemplates.getTemplates() != null)
    {
      String className = IOUtils.toCamelCase(classTemplates.getClazz().getName());
      
      for (Template template : classTemplates.getTemplates().getItems())
      {
        for (Role role : template.getRoles().getItems())
        {
          if (role.getType() == org.iringtools.mapping.RoleType.PROPERTY ||
              role.getType() == org.iringtools.mapping.RoleType.DATA_PROPERTY ||
              role.getType() == org.iringtools.mapping.RoleType.OBJECT_PROPERTY ||
              role.getType() == org.iringtools.mapping.RoleType.FIXED_VALUE)
          {
            String dataType = role.getDataType();            
            String fieldName = className + '.' + template.getName() + "." + role.getName();
            Field field = new Field();
            
            field.setWidth(MIN_COLUMN_WIDTH);
            field.setName(fieldName);
            field.setDataIndex(fieldName);

            if (mode == Mode.APP && dataType != null && dataType.startsWith("xsd:"))
            {
              dataType = dataType.replace("xsd:", "").toLowerCase();
              
              if (!gridFilterTypes.contains(dataType))
              {
                dataType = "auto";
              }

              field.setType(dataType);
            }
            else
            {
              field.setType("string");
            }

            fields.add(field);
          }
          else if (role.getClazz() != null && (role.getCardinality() == null || 
              role.getCardinality() == Cardinality.ONE_TO_ONE))
          {
            String classId = role.getClazz().getId();              
            ClassTemplates relatedClassTemplates = getClassTemplates(graph, classId);
            createFields(mode, fields, graph, relatedClassTemplates);
          }
        }
      }
    }
  }
  
  private ClassTemplates getClassTemplates(Graph graph, String classId)
  {
    for (ClassTemplates classTemplates : graph.getClassTemplatesList().getItems())
    {
      if (classTemplates.getClazz().getId().equals(classId))
        return classTemplates;
    }
    
    return null;
  }
  
  private void processClassObject(DataTransferObject dto, int dtoIndex, List<Field> fields, Mode mode, 
      ClassObject classObject, Grid dtoGrid, List<String> rowData, List<RelatedClass> relatedClasses, String refServiceUri)
  {
    String className = IOUtils.toCamelCase(classObject.getName());

    for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
    {
      for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
      {
        RoleValues roleValues = roleObject.getValues();
        RoleValues roleOldValues = roleObject.getOldValues();
        String roleValue = roleObject.getValue();
        String roleOldValue = roleObject.getOldValue();
        RoleType roleType = roleObject.getType();
                
        if (roleType == RoleType.PROPERTY ||
            roleType == RoleType.DATA_PROPERTY ||
            roleType == RoleType.OBJECT_PROPERTY ||
            roleType == RoleType.FIXED_VALUE)
        {
          // compute role value
          if (roleValues != null && roleValues.getItems().size() > 0)
          {
            roleValue = getMultiRoleValues(roleObject, roleValues.getItems(), refServiceUri);
            roleOldValue = getMultiRoleValues(roleObject, roleOldValues.getItems(), refServiceUri);
          }
          else if (roleObject.getHasValueMap() != null && roleObject.getHasValueMap())
          {
            roleValue = getValueMap(refServiceUri, roleValue);
            
            if (mode == Mode.EXCHANGE) 
            {
              roleOldValue = getValueMap(refServiceUri, roleOldValue);
            }
          }
          
          // find the right column to insert value, fill in blank for any gap 
          // (because class/template do not exist, e.g. due to null class identifier)         
          String dataIndex = className + '.' + templateObject.getName() + '.' + roleObject.getName();
          
          while (rowData.size() < fields.size())
          {
            if (!fields.get(rowData.size()).getDataIndex().equalsIgnoreCase(dataIndex))
            {
              rowData.add("");
            }
            else
            {
              break;
            }
          }
          
          // add row value to row data
          if (mode == Mode.APP || roleOldValue == null || roleOldValue.equals(roleValue))
          { 
            rowData.add(roleValue);
          }
          else
          {
            roleValue = roleOldValue + " -> " + roleValue;
            rowData.add("<span class=\"change\">" + roleValue + "</span>");
          }
          
          // adjust field width based on value
          Field field = fields.get(rowData.size() - 1);
          int fieldWidth = field.getWidth();
          int newWidth = roleValue.length() * PIXELS_PER_CHAR;
          
          if (newWidth > MIN_COLUMN_WIDTH && newWidth > fieldWidth && newWidth < MAX_COLUMN_WIDTH)
          {
            field.setWidth(newWidth);
          }
        }
        else if (roleObject.getRelatedClassId() != null)
        {
          if (roleObject.getValues().getItems().size() > 1)
          {
            RelatedClass relatedClass = new RelatedClass();
            relatedClass.setId(roleObject.getRelatedClassId());
            relatedClass.setName(IOUtils.toCamelCase(roleObject.getRelatedClassName()));
            relatedClasses.add(relatedClass);
          }
          else if (roleObject.getValues().getItems().size() == 1)
          {
            String relatedClassIdentifier = roleObject.getValues().getItems().get(0);
            
            // find related class and recur
            for (ClassObject relatedClassObject : dto.getClassObjects().getItems())
            {
              if (relatedClassObject.getClassId().equals(roleObject.getRelatedClassId()) && 
                  relatedClassObject.getIdentifier().equals(relatedClassIdentifier))
              {
                processClassObject(dto, dtoIndex, fields, mode, relatedClassObject, dtoGrid, rowData, 
                    relatedClasses, refServiceUri);
                
                break;
              }
            }
          }
        }
      }
    }
  }
  
  private String getMultiRoleValues(RoleObject roleObject, List<String> roleValues, String refServiceUri)
  {
    StringBuilder roleValueBuilder = new StringBuilder();
    
    for (String value : roleValues)
    {
      if (roleObject.getHasValueMap() != null && roleObject.getHasValueMap())
      {
        value = getValueMap(refServiceUri, value);
      }
      
      if (roleValueBuilder.length() > 0)
      {
        roleValueBuilder.append(",");
      }
      
      roleValueBuilder.append(value);
    }
    
    return roleValueBuilder.toString();
  }
  
  protected Grid getRelatedDtoGrid(Mode mode, DataTransferObjects dtos, String classId, String classIdentifier)
  {
    Grid relatedItemGrid = new Grid();    
    List<Field> fields = new ArrayList<Field>();
    List<List<String>> gridData = new ArrayList<List<String>>();    
    int relatedClassCount = 0;

    for (DataTransferObject dto : dtos.getDataTransferObjectList().getItems())
    {
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
            if (mode == Mode.EXCHANGE)
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
              RoleValues roleValues = roleObject.getValues();
              RoleType roleType = roleObject.getType();
              String dataType = roleObject.getDataType();
              boolean hasRelatedValues = false;
              
              // values take priority over value as value will be deprecated/removed later
              if ((roleType == RoleType.PROPERTY ||
                   roleType == RoleType.DATA_PROPERTY ||
                   roleType == RoleType.OBJECT_PROPERTY) && 
                  roleValues != null && roleValues.getItems().size() > 0)
              {
                StringBuilder tempRoleValue = new StringBuilder();
                
                for (String value : roleValues.getItems())
                {
                  if (tempRoleValue.length() > 0)
                    tempRoleValue.append(",");
                  
                  tempRoleValue.append(value);
                }
                
                roleValue = tempRoleValue.toString();
                hasRelatedValues = true;
              }
              
              if (hasRelatedValues || (roleValue != null && ((dataType != null && dataType.startsWith("xsd:")) ||
                  (roleObject.getRelatedClassName() != null && roleObject.getRelatedClassName().length() > 0))))
              {           
                if (relatedClassCount == 1)
                {
                  String fieldName = templateObject.getName() + "." + roleObject.getName();
  
                  Field field = new Field();
                  field.setName(fieldName);
                  field.setDataIndex(className + '.' + fieldName);
  
                  if (mode == Mode.APP && dataType != null && dataType.startsWith("xsd:"))
                  {
                    dataType = dataType.replace("xsd:", "").toLowerCase();
                    
                    if (!gridFilterTypes.contains(dataType))
                    {
                      dataType = "auto";
                    }
   
                    field.setType(dataType);
                  }
                  else
                  {
                    field.setType("string");
                  }
  
                  fields.add(field);
                }
  
                if (mode == Mode.APP || roleObject.getOldValue() == null
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
            if (mode == Mode.EXCHANGE)
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
              + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\",\"" +
              classObject.getIdentifier() + "\"," + relatedClassesJson + ")'>");
          
          gridData.add(row);
        }
      }
    }
    
    //TODO: pass count value into the function
    relatedItemGrid.setTotal(relatedClassCount);
    relatedItemGrid.setFields(fields);
    relatedItemGrid.setData(gridData);

    return relatedItemGrid;
  }

  protected String getValueMapKey(String value, HashMap<String, String> valueMaps)
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
  
  protected DataFilter createDataFilter(String filter, String sortBy, String sortOrder)
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

            List<String> valueItems = new ArrayList<String>();
            values.setItems(valueItems);

            String value = String.valueOf(filterExpression.get("value"));
            
            if (valueMaps != null)
            {
              String valueMap = getValueMapKey(String.valueOf(filterExpression.get("value")), valueMaps);
              
              if (valueMap != null && !valueMap.isEmpty())
              {
                valueItems.add(valueMap);
                value = valueMap;
              }
            }
            
            valueItems.add(value);
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
  
  protected Manifest getManifest(String serviceUri, String manifestRelativePath)
  {
    Manifest manifest = null;
    String manifestKey = MANIFEST_PREFIX + manifestRelativePath;
    
    if (session.containsKey(manifestKey))
    {
      manifest = (Manifest) session.get(manifestKey);
    }
    else 
    {
      HttpClient httpClient = new HttpClient(serviceUri);
      
      try
      {
        manifest = httpClient.get(Manifest.class, manifestRelativePath);
        session.put(manifestKey, manifest);
      }
      catch (Exception ex)
      {
        logger.error("Error getting manifest from [" + manifestRelativePath + "]: " + ex);      
      }
    }
    
    return manifest;
  }
  
  
  protected Graph getGraph(Manifest manifest, String graphName)
  {
    if (manifest.getGraphs() != null)
    {
      for (Graph graph : manifest.getGraphs().getItems())
      {
        if (graph.getName().equalsIgnoreCase(graphName))
        {
          return graph;
        }
      }
    }
    
    return null;
  }
}
