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
import org.iringtools.mapping.ValueListMap;
import org.iringtools.mapping.ValueMap;
import org.iringtools.refdata.response.Entity;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.grid.RelatedClass;

public class DataModel
{
  private static final Logger logger = Logger.getLogger(DataModel.class);

  public static enum DataMode {
    APP, EXCHANGE
  };

  public static enum FieldFit {
    HEADER, VALUE
  };

  protected static List<String> gridFilterTypes;
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

  protected static Map<String, RelationalOperator> relationalOperatorMap;
  static
  {
    relationalOperatorMap = new HashMap<String, RelationalOperator>();
    relationalOperatorMap.put("eq", RelationalOperator.EQUAL_TO);
    relationalOperatorMap.put("lt", RelationalOperator.LESSER_THAN);
    relationalOperatorMap.put("gt", RelationalOperator.GREATER_THAN);
  }

  public static final String APP_PREFIX = "xchmgr-";
  public static final String MANIFEST_PREFIX = APP_PREFIX + "manifest-";
  public static final String FIELDS_PREFIX = APP_PREFIX + "fields-";
  public static final String DTI_PREFIX = APP_PREFIX + "dti-";
  public static final String XLOGS_PREFIX = APP_PREFIX + "xlogs-";
  public static final String FULL_DTI_KEY_PREFIX = DTI_PREFIX + "full";
  public static final String PART_DTI_KEY_PREFIX = DTI_PREFIX + "part";
  public static final String FILTER_KEY_PREFIX = DTI_PREFIX + "filter";

  protected final int MIN_FIELD_WIDTH = 50;
  protected final int MAX_FIELD_WIDTH = 300;
  protected final int INFO_FIELD_WIDTH = 28;
  protected final int STATUS_FIELD_WIDTH = 60;
  protected final int FIELD_PADDING = 2;
  protected final int HEADER_PX_PER_CHAR = 6;
  protected final int VALUE_PX_PER_CHAR = 10;

  protected Map<String, Object> session;

  protected DataMode dataMode;
  protected String refDataServiceUri;
  protected FieldFit fieldFit;

  public DataModel(DataMode dataMode, String refDataServiceUri, FieldFit fieldFit)
  {
    this.dataMode = dataMode;
    this.refDataServiceUri = refDataServiceUri;
    this.fieldFit = fieldFit;
  }

  // only cache full dti and last filtered dti
  protected DataTransferIndices getDtis(String serviceUri, String manifestRelativePath, String dtiRelativePath,
      String filter, String sortBy, String sortOrder) throws DataModelException
  {
    DataTransferIndices dtis = new DataTransferIndices();
    String fullDtiKey = FULL_DTI_KEY_PREFIX + dtiRelativePath;
    String partDtiKey = PART_DTI_KEY_PREFIX + dtiRelativePath;
    String lastFilterKey = FILTER_KEY_PREFIX + dtiRelativePath;
    String currFilter = filter + "/" + sortBy + "/" + sortOrder;

    try
    {
      DataFilter dataFilter = createDataFilter(filter, sortBy, sortOrder);
      dtis = getFullDtis(serviceUri, manifestRelativePath, dtiRelativePath, fullDtiKey, partDtiKey, lastFilterKey);

      if (dataFilter != null)
      {
        if (session.containsKey(lastFilterKey))  // check if filter has changed
        {
          String lastFilter = (String) session.get(lastFilterKey);

          // filter has changed or cache data not available, fetch filtered data
          if (!lastFilter.equals(currFilter) || !session.containsKey(partDtiKey))
          {
            dtis = getFilteredDtis(dataFilter, manifestRelativePath, dtiRelativePath, serviceUri, fullDtiKey,
                partDtiKey, lastFilterKey, currFilter);
          }
          else  // filter has not changed, get data from cache
          {
            dtis = (DataTransferIndices) session.get(partDtiKey);
          }
        }
        else  // new filter or same filter after exchange, fetch filtered data
        {
          dtis = getFilteredDtis(dataFilter, manifestRelativePath, dtiRelativePath, serviceUri, fullDtiKey, 
              partDtiKey, lastFilterKey, currFilter);
        }
      }
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      throw new DataModelException(e.getMessage());
    }

    return dtis;
  }

  private DataTransferIndices getFullDtis(String serviceUri, String manifestRelativePath, String dtiRelativePath,
      String fullDtiKey, String partDtiKey, String lastFilterKey) throws DataModelException
  {
    DataTransferIndices dtis = null;
    
    if (session.containsKey(fullDtiKey))
    {
      dtis = (DataTransferIndices)session.get(fullDtiKey);      
    }
    else
    {
      DxiRequest dxiRequest = new DxiRequest();
      dxiRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
      dxiRequest.setDataFilter(new DataFilter());

      HttpClient httpClient = new HttpClient(serviceUri);
      HttpUtils.addHttpHeaders(session, httpClient);

      try
      {
        dtis = httpClient.post(DataTransferIndices.class, dtiRelativePath, dxiRequest);
        session.put(fullDtiKey, dtis);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new DataModelException(e.getMessage());
      }
    }

    if (session.containsKey(partDtiKey))
    {
      session.remove(partDtiKey);
    }

    if (session.containsKey(lastFilterKey))
    {
      session.remove(lastFilterKey);
    }

    return dtis;
  }

  private DataTransferIndices getFilteredDtis(DataFilter dataFilter, String manifestRelativePath,
      String dtiRelativePath, String serviceUri, String fullDtiKey, String partDtiKey, String lastFilterKey,
      String currFilter) throws DataModelException
  {
    DataTransferIndices dtis = null;

    if (dataMode == DataMode.EXCHANGE) // exchange data
    {
      dtis = getFilteredDtis(dataFilter, manifestRelativePath, dtiRelativePath, serviceUri, fullDtiKey);
    }
    else // app data
    {
      DxiRequest dxiRequest = new DxiRequest();
      dxiRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
      dxiRequest.setDataFilter(dataFilter);

      HttpClient httpClient = new HttpClient(serviceUri);
      HttpUtils.addHttpHeaders(session, httpClient);

      try
      {
        dtis = httpClient.post(DataTransferIndices.class, dtiRelativePath, dxiRequest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new DataModelException(e.getMessage());
      }
    }

    if (dtis != null && dtis.getDataTransferIndexList() != null
        && dtis.getDataTransferIndexList().getItems().size() > 0)
    {
      session.put(partDtiKey, dtis);
      session.put(lastFilterKey, currFilter);
    }

    return dtis;
  }

  protected DataTransferIndices getFilteredDtis(DataFilter dataFilter, String manifestRelativePath,
      String dtiRelativePath, String serviceUri, String fullDtiKey) throws DataModelException
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
        HttpUtils.addHttpHeaders(session, httpClient);

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
                if (fullDti.getTransferType() == org.iringtools.dxfr.dti.TransferType.DELETE
                    && fullDti.getIdentifier().equalsIgnoreCase(targetDti.getIdentifier()))
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
              else  // sort type is numeric
              {
                if (sortDir.equals("asc"))
                {
                  compareValue = (int) (Double.parseDouble(dti1.getSortIndex()) - Double.parseDouble(dti2
                      .getSortIndex()));
                }
                else
                {
                  compareValue = (int) (Double.parseDouble(dti2.getSortIndex()) - Double.parseDouble(dti1
                      .getSortIndex()));
                }
              }

              return compareValue;
            }
          };

          Collections.sort(resultDtis.getDataTransferIndexList().getItems(), comparator);
        }
      }
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      throw new DataModelException(e.getMessage());
    }

    return resultDtis;
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

  protected DataTransferObjects getDtos(String serviceUri, String manifestRelativePath, String dtoRelativePath,
      List<DataTransferIndex> dtiList) throws DataModelException
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
      HttpUtils.addHttpHeaders(session, httpClient);

      dtos = httpClient.post(DataTransferObjects.class, dtoRelativePath, dxoRequest);
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new DataModelException(e.getMessage());
    }

    return dtos;
  }

  protected DataTransferObjects getPageDtos(String serviceUri, String manifestRelativePath, String dtiRelativePath,
      String dtoRelativePath, String filter, String sortBy, String sortOrder, int start, int limit) throws DataModelException
  {
    DataTransferIndices dtis = getDtis(serviceUri, manifestRelativePath, dtiRelativePath, filter, sortBy, sortOrder);
    List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
    int actualLimit = Math.min(start + limit, dtiList.size());
    List<DataTransferIndex> pageDtis = dtiList.subList(start, actualLimit);
    return getDtos(serviceUri, manifestRelativePath, dtoRelativePath, pageDtis);
  }

  protected Grid getDtoGrid(String fieldsContext, Manifest manifest, Graph graph, DataTransferObjects dtos) throws DataModelException
  {
    Grid dtoGrid = new Grid();

    if (graph != null)
    {
      if (graph.getClassTemplatesList() != null &&
          graph.getClassTemplatesList().getItems().size() > 0 &&
          graph.getClassTemplatesList().getItems().get(0).getClazz() != null)
      {
        String className = IOUtils.toCamelCase(graph.getClassTemplatesList().getItems().get(0).getClazz().getName());  
        dtoGrid.setDescription(className);
      }
      
      List<Field> fields = getFields(fieldsContext, graph, null);
      dtoGrid.setFields(fields);
  
      List<List<String>> gridData = new ArrayList<List<String>>();
      dtoGrid.setData(gridData);
  
      List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();
  
      for (int dtoIndex = 0; dtoIndex < dtoList.size(); dtoIndex++)
      {
        DataTransferObject dto = dtoList.get(dtoIndex);
        List<String> rowData = new ArrayList<String>();
        List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();
  
        // create a place holder for info field
        rowData.add("");
  
        if (dataMode == DataMode.EXCHANGE)
        {
          String transferType = dto.getTransferType().toString();
          rowData.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
        }
  
        if (dto.getClassObjects().getItems().size() > 0)
        {
          ClassObject classObject = dto.getClassObjects().getItems().get(0);
          dtoGrid.setIdentifier(classObject.getClassId());
          
          processClassObject(manifest, graph, dto, dtoIndex, fields, classObject, dtoGrid, rowData, relatedClasses);
        }
  
        String relatedClassesJson;
  
        try
        {
          relatedClassesJson = JSONUtil.serialize(relatedClasses);
        }
        catch (JSONException e)
        {
          relatedClassesJson = "[]";
        }
  
        // update info field
        rowData.set(0, "<input type=\"image\" src=\"resources/images/info-small.png\" "
            + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\",\"" + dto.getIdentifier() + "\","
            + relatedClassesJson + ")'>");
  
        gridData.add(rowData);
      }
    }

    return dtoGrid;
  }

  // TODO: apply start and limit
  protected DataTransferObjects getRelatedItems(String serviceUri, String manifestRelativePath, String dtiRelativePath,
      String dtoRelativePath, String dtoIdentifier, String filter, String sortBy, String sortOrder, int start, int limit) 
      throws DataModelException
  {
    DataTransferObjects relatedDtos = new DataTransferObjects();
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
          HttpUtils.addHttpHeaders(session, httpClient);

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
        catch (HttpClientException e)
        {
          logger.error(e.getMessage());
          throw new DataModelException(e.getMessage());
        }
      }
    }

    return relatedDtos;
  }

  protected Grid getRelatedItemGrid(String fieldsContext, Manifest manifest, Graph graph, DataTransferObjects dtos, String classId,
      String classIdentifier) throws DataModelException
  {
    Grid dtoGrid = new Grid();

    List<Field> fields = getFields(fieldsContext, graph, classId);
    dtoGrid.setFields(fields);

    List<List<String>> gridData = new ArrayList<List<String>>();
    dtoGrid.setData(gridData);

    List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();

    for (int dtoIndex = 0; dtoIndex < dtoList.size(); dtoIndex++)
    {
      DataTransferObject dto = dtoList.get(dtoIndex);
      List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();

      if (dto.getClassObjects().getItems().size() > 0)
      {
        for (ClassObject classObject : dto.getClassObjects().getItems())
        {
          if (classObject.getClassId().equalsIgnoreCase(classId)) // &&
                                                                  // classObject.getIdentifier().equalsIgnoreCase(classIdentifier))
          {
            dtoGrid.setIdentifier(classObject.getClassId());
            dtoGrid.setDescription(classObject.getName());

            List<String> rowData = new ArrayList<String>();

            // create a place holder for info field
            rowData.add("");

            if (dataMode == DataMode.EXCHANGE)
            {
              String transferType = dto.getTransferType().toString();
              rowData.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
            }

            processClassObject(manifest, graph, dto, dtoIndex, fields, classObject, dtoGrid, rowData, relatedClasses);

            String relatedClassesJson;

            try
            {
              relatedClassesJson = JSONUtil.serialize(relatedClasses);
            }
            catch (JSONException e)
            {
              relatedClassesJson = "[]";
            }

            // update info field
            rowData.set(
                0,
                "<input type=\"image\" src=\"resources/images/info-small.png\" "
                    + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\",\""
                    + classObject.getIdentifier() + "\"," + relatedClassesJson + ")'>");

            gridData.add(rowData);
          }
        }
      }
    }

    return dtoGrid;
  }

  protected String resolveValueMap(String id) throws DataModelException
  {
    String label = id;

    try
    {
      HttpClient httpClient = new HttpClient(refDataServiceUri);
      HttpUtils.addHttpHeaders(session, httpClient);

      Entity value = httpClient.get(Entity.class, "/classes/" + id.substring(4, id.length()) + "/label");

      if (value != null && value.getLabel() != null)
      {
        label = value.getLabel();
      }
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new DataModelException(e.getMessage());
    }

    return label;
  }

  protected Cardinality getCardinality(Graph graph, String className, String templateName, String roleName,
      String relatedClassName)
  {
    for (ClassTemplates classTemplates : graph.getClassTemplatesList().getItems())
    {
      String clsName = IOUtils.toCamelCase(classTemplates.getClazz().getName());

      if (clsName.equalsIgnoreCase(className))
      {
        for (Template template : classTemplates.getTemplates().getItems())
        {
          if (template.getName().equalsIgnoreCase(templateName))
          {
            for (Role role : template.getRoles().getItems())
            {
              if (role.getName().equalsIgnoreCase(roleName))
              {
                return role.getCardinality();
              }
            }
          }
        }
      }
    }

    return null;
  }

  protected String getValueMapKey(String value, HashMap<String, String> valueMaps)
  {
    for (String key : valueMaps.keySet()) 
    {
    	if (valueMaps.get(key) == null)
    		continue;
    	
      if (valueMaps.get(key).equalsIgnoreCase(value))
        return key;
    }
    return null;
  }

  @SuppressWarnings("unchecked")
  protected String getValueMap(Manifest manifest, String value) throws DataModelException
  {
    Map<String, String> valueMaps;
    String valueMap = value;

    // find value map in manifest first
    if (manifest != null && manifest.getValueListMaps() != null)
    {
      for (ValueListMap vlm : manifest.getValueListMaps().getItems())
      {
        if (vlm.getValueMaps() != null)
        {
          for (ValueMap vm : vlm.getValueMaps().getItems())
          {
            if (vm.getUri() != null && vm.getUri().equalsIgnoreCase(value) && 
                vm.getLabel() != null && vm.getLabel().length() > 0)
            {
              return vm.getLabel();
            }
          }
        }
      }
    }

    // if not found, find it in session
    if (session.containsKey("valueMaps"))
    {
      valueMaps = (Map<String, String>) session.get("valueMaps");
    }
    else
    {
      valueMaps = new HashMap<String, String>();
      session.put("valueMaps", valueMaps);
    }

    // if still not found, query reference data service
    if (value != null && !value.isEmpty())
    {
      if (!valueMaps.containsKey(value))
      {
        valueMap = resolveValueMap(value);
        valueMaps.put(value, valueMap);
      }
      else
      {
        valueMap = valueMaps.get(value);
      }
    }

    return valueMap;
  }

  protected DataFilter createDataFilter(String filter, String sortBy, String sortOrder) throws DataModelException
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
      catch (JSONException e)
      {
        String message = "Error creating data filter: " + e;
        logger.error(message);
        throw new DataModelException(message);
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

  protected Manifest getManifest(String serviceUri, String manifestRelativePath) throws DataModelException
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
      HttpUtils.addHttpHeaders(session, httpClient);

      try
      {
        manifest = httpClient.get(Manifest.class, manifestRelativePath);
        session.put(manifestKey, manifest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new DataModelException(e.getMessage());
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

  protected void removeSessionData(String key)
  {
    if (session != null && session.keySet().contains(key))
    {
      session.remove(key);
    }
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
              RoleType roleType = roleObject.getType();

              if ((roleType == null
                  || // bug in v2.0 of c# service
                  roleType == RoleType.PROPERTY || roleType == RoleType.DATA_PROPERTY
                  || roleType == RoleType.OBJECT_PROPERTY || roleType == RoleType.FIXED_VALUE)
                  && roleObject.getName().equalsIgnoreCase(propertyParts[2]))
              {
                int compareValue = roleObject.getValue().compareToIgnoreCase(expression.getValues().getItems().get(0));
                RelationalOperator relationalOperator = expression.getRelationalOperator();

                // TODO: handle numeric, date, time comparison
                if ((relationalOperator == RelationalOperator.EQUAL_TO && compareValue == 0)
                    || (relationalOperator == RelationalOperator.GREATER_THAN && compareValue > 0)
                    || (relationalOperator == RelationalOperator.LESSER_THAN && compareValue < 0))
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
  
  @SuppressWarnings("unchecked")
  protected List<Field> getFields(String fieldsContext, Graph graph, String startClassId) throws DataModelException
  {
    List<Field> fields = null;
    String fieldsKey = FIELDS_PREFIX + fieldsContext + graph.getName();

    if (session.containsKey(fieldsKey))
    {
      fields = (List<Field>) session.get(fieldsKey);
    }
    else
    {
      fields = createFields(graph, startClassId);
      session.put(fieldsKey, fields);
    }

    return fields;
  }

  private List<Field> createFields(Graph graph, String startClassId)
  {
    List<Field> fields = new ArrayList<Field>();

    // transfer-type field
    if (dataMode == DataMode.EXCHANGE)
    {
      Field field = new Field();
      field.setName("Status");
      field.setDataIndex("Transfer Type");
      field.setType("string");
      field.setWidth(STATUS_FIELD_WIDTH);
      field.setFilterable(true);
      fields.add(0, field);
    }

    // info field
    Field field = new Field();
    field.setName("&nbsp;");
    field.setDataIndex("&nbsp;");
    field.setType("string");
    field.setWidth(INFO_FIELD_WIDTH);
    field.setFixed(true);
    field.setFilterable(false);
    fields.add(0, field);

    List<ClassTemplates> classTemplatesItems = graph.getClassTemplatesList().getItems();

    if (classTemplatesItems.size() > 0)
    {
      if (startClassId == null || startClassId.length() == 0)
      {
        ClassTemplates classTemplates = classTemplatesItems.get(0);
        createFields(fields, graph, classTemplates);
      }
      else
      {
        for (ClassTemplates classTempates : classTemplatesItems)
        {
          if (classTempates.getClazz().getId().equalsIgnoreCase(startClassId))
          {
            createFields(fields, graph, classTempates);
            break;
          }
        }
      }
    }

    return fields;
  }

  private void createFields(List<Field> fields, Graph graph, ClassTemplates classTemplates)
  {
    if (classTemplates != null && classTemplates.getTemplates() != null)
    {
      String className = IOUtils.toCamelCase(classTemplates.getClazz().getName());

      for (Template template : classTemplates.getTemplates().getItems())
      {
        for (Role role : template.getRoles().getItems())
        {
          org.iringtools.mapping.RoleType roleType = role.getType();
          Cardinality cardinality = role.getCardinality();

          if (roleType == null
              || // bug in v2.0 of c# service
              roleType == org.iringtools.mapping.RoleType.PROPERTY
              || roleType == org.iringtools.mapping.RoleType.DATA_PROPERTY
              || roleType == org.iringtools.mapping.RoleType.OBJECT_PROPERTY
              || roleType == org.iringtools.mapping.RoleType.FIXED_VALUE
              || (cardinality != null && cardinality == Cardinality.SELF))
          {
            String dataType = role.getDataType();
            String fieldName = className + '.' + template.getName() + "." + role.getName();
            Field field = new Field();

            field.setName(fieldName);
            field.setDataIndex(fieldName);
            field.setWidth(MIN_FIELD_WIDTH);

            // adjust field width
            if (fieldFit == FieldFit.HEADER)
            {
              int fieldWidth = fieldName.length() * HEADER_PX_PER_CHAR;

              if (fieldWidth > MIN_FIELD_WIDTH)
              {
                field.setWidth(fieldWidth + FIELD_PADDING);
              }
            }

            if (dataMode == DataMode.APP && dataType != null && dataType.startsWith("xsd:"))
            {
              dataType = dataType.replace("xsd:", "").toLowerCase();

              if (!gridFilterTypes.contains(dataType))
              {
                dataType = "string";
              }

              field.setType(dataType);
            }
            else
            {
              field.setType("string");
            }

            fields.add(field);
          }
          else if (role.getClazz() != null && (cardinality == null || cardinality == Cardinality.ONE_TO_ONE))
          {
            String classId = role.getClazz().getId();
            ClassTemplates relatedClassTemplates = getClassTemplates(graph, classId);
            createFields(fields, graph, relatedClassTemplates);
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

  private void processClassObject(Manifest manifest, Graph graph, DataTransferObject dto, int dtoIndex,
      List<Field> fields, ClassObject classObject, Grid dtoGrid, List<String> rowData, List<RelatedClass> relatedClasses) 
      throws DataModelException
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
        
        if (roleOldValue == null && dto.getTransferType() == TransferType.CHANGE)
        	roleOldValue = "";
        
        if (roleValue == null && dto.getTransferType() == TransferType.CHANGE)
        	roleValue = "";
        
        RoleType roleType = roleObject.getType();
        Cardinality cardinality = getCardinality(graph, className, templateObject.getName(), roleObject.getName(),
            roleObject.getRelatedClassName());

        if (roleType == null
            || // bug in v2.0 of c# service
            roleType == RoleType.PROPERTY || roleType == RoleType.DATA_PROPERTY || roleType == RoleType.OBJECT_PROPERTY
            || roleType == RoleType.FIXED_VALUE || (cardinality != null && cardinality == Cardinality.SELF))
        {
          // compute role value
          if (roleObject.getHasValueMap() != null && roleObject.getHasValueMap())
          {
            if (!IOUtils.isNullOrEmpty(roleValue))
            {
              roleValue = getValueMap(manifest, roleValue);
  
              if (dataMode == DataMode.EXCHANGE)
              {
                roleOldValue = getValueMap(manifest, roleOldValue);
              }
            }
          }
          else if (roleValues != null && roleValues.getItems().size() > 0)
          {
            roleValue = getMultiRoleValues(manifest, roleObject, roleValues.getItems());

            if (roleOldValues != null && roleOldValues.getItems().size() > 0)
              roleOldValue = getMultiRoleValues(manifest, roleObject, roleOldValues.getItems());
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
          if (rowData.size() < fields.size())
          {
            if (dataMode == DataMode.APP || roleOldValue == null || roleOldValue.equals(roleValue))
            {
              rowData.add(roleValue);
            }
            else
            {
              roleValue = roleOldValue + " -> " + roleValue;
            	rowData.add("<span class=\"change\">" + roleValue + "</span>");
            }
          }

          // adjust field width based on value
          if (fieldFit == FieldFit.VALUE)
          {
            Field field = fields.get(rowData.size() - 1);
            int fieldWidth = field.getWidth();
            int newWidth = roleValue.length() * VALUE_PX_PER_CHAR;

            if (newWidth > MIN_FIELD_WIDTH && newWidth > fieldWidth && newWidth < MAX_FIELD_WIDTH)
            {
              field.setWidth(newWidth);
            }
          }
        }
        else if (roleObject.getRelatedClassId() != null
            && (roleObject.getValue() != null && roleObject.getValue().startsWith("#")) || // v2.0
            roleObject.getValues() != null) // v2.1
        {
          if ((roleObject.getValue() != null && roleObject.getValue().startsWith("#")) || // v2.0
              (cardinality == null || cardinality == Cardinality.ONE_TO_ONE)) // v2.1
          {
            String relatedClassIdentifier;

            if (roleObject.getValue() != null && roleObject.getValue().startsWith("#")) // v2.0
            {
              relatedClassIdentifier = roleObject.getValue().substring(1);
            }
            else
            // v2.1
            {
              relatedClassIdentifier = roleObject.getValues().getItems().get(0);
            }

            // find related class and recur
            for (ClassObject relatedClassObject : dto.getClassObjects().getItems())
            {
              if (relatedClassObject.getClassId().equals(roleObject.getRelatedClassId())
                  && relatedClassObject.getIdentifier().equals(relatedClassIdentifier))
              {
                processClassObject(manifest, graph, dto, dtoIndex, fields, relatedClassObject, dtoGrid, rowData,
                    relatedClasses);

                break;
              }
            }
          }
          else
          {
            String relatedClassName = IOUtils.toCamelCase(roleObject.getRelatedClassName());

            if (!relatedClassExists(relatedClasses, relatedClassName))
            {
              RelatedClass relatedClass = new RelatedClass();
              relatedClass.setId(roleObject.getRelatedClassId());
              relatedClass.setName(relatedClassName);
              relatedClasses.add(relatedClass);
            }
          }
        }
      }
    }
  }

  private boolean relatedClassExists(List<RelatedClass> relatedClasses, String relatedClassName)
  {
    for (RelatedClass relatedClass : relatedClasses)
    {
      if (relatedClass.getName().equalsIgnoreCase(relatedClassName))
        return true;
    }

    return false;
  }

  private String getMultiRoleValues(Manifest manifest, RoleObject roleObject, List<String> roleValues) 
      throws DataModelException
  {
    StringBuilder roleValueBuilder = new StringBuilder();

    for (String value : roleValues)
    {
      if (roleObject.getHasValueMap() != null && roleObject.getHasValueMap() && !IOUtils.isNullOrEmpty(value))
      {
        value = getValueMap(manifest, value);
      }

      if (roleValueBuilder.length() > 0)
      {
        roleValueBuilder.append(",");
      }

      roleValueBuilder.append(value);
    }

    return roleValueBuilder.toString();
  }
}
