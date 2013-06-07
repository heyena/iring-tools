package org.iringtools.models;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.io.UnsupportedEncodingException;
import java.math.BigInteger;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

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
import org.iringtools.directory.Commodity;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Scope;
import org.iringtools.dxfr.dti.DataTransferIndex;
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
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.mapping.ValueListMap;
import org.iringtools.mapping.ValueMap;
import org.iringtools.refdata.response.Entity;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.grid.RelatedClass;

import sun.security.util.BigInt;

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
  
  protected static ConcurrentMap<String, String> valueMapsCache = 
      new ConcurrentHashMap<String, String>();

  public static final String APP_PREFIX = "xchmgr";
  public static final String DIRECTORY_KEY = APP_PREFIX + ".directory";
  public static final String FIELDS_PREFIX = APP_PREFIX + ".fields";
  public static final String EXCHANGE_PREFIX = APP_PREFIX + ".exchange";
  public static final String MANIFEST_PREFIX = EXCHANGE_PREFIX + ".manifest";
  public static final String DATAFILTER_PREFIX = EXCHANGE_PREFIX + ".datafilter";
  public static final String DTI_PREFIX = EXCHANGE_PREFIX + ".dti";
  public static final String PRE_SUMMARY_PREFIX = EXCHANGE_PREFIX + ".presummary";
  public static final String POST_SUMMARY_PREFIX = EXCHANGE_PREFIX + ".postsummary";

  protected final int MIN_FIELD_WIDTH = 50;
  protected final int MAX_FIELD_WIDTH = 300;
  protected final int INFO_FIELD_WIDTH = 40;
  protected final int CONTENT_FIELD_WIDTH = 60;
  protected final int TRANSFER_FIELD_WIDTH = 80;
  protected final int STATUS_FIELD_WIDTH = 60;
  protected final int FIELD_PADDING = 2;
  protected final int HEADER_PX_PER_CHAR = 6;
  protected final int VALUE_PX_PER_CHAR = 10;

  protected Map<String, Object> settings;
  protected Map<String, Object> session;

  protected DataMode dataMode;
  protected String refDataServiceUri;
  protected FieldFit fieldFit;
  protected boolean isAsync = false;
  protected long asyncTimeout = 1800; // seconds
  protected long asyncPollingInterval = 2; // seconds
  protected String scope;
  protected String app;
  protected String xId;

  public DataModel(DataMode dataMode, Map<String, Object> settings, Map<String, Object> session)
  {
    this.dataMode = dataMode;
    this.settings = settings;
    this.session = session;

    refDataServiceUri = settings.get("refDataServiceUri").toString();
    isAsync = Boolean.parseBoolean(settings.get("async").toString());
    asyncTimeout = Long.parseLong(settings.get("asyncTimeout").toString());
    asyncPollingInterval = Long.parseLong(settings.get("asyncPollingInterval").toString());

    String fieldFitSetting = settings.get("fieldFit").toString();
    fieldFit = IOUtils.isNullOrEmpty(fieldFitSetting) ? FieldFit.VALUE : FieldFit
        .valueOf(fieldFitSetting.toUpperCase());
  }

  protected Exchange getExchangeFromDirectory(String scopeName, String xId)
  {
    Exchange exchange = null;

    Directory directory = (Directory) session.get(DIRECTORY_KEY);

    for (Scope scope : directory.getScope())
    {
      if (scope.getName().equalsIgnoreCase(scopeName))
      {
        if (scope.getDataExchanges() != null)
        {
          for (Commodity commodity : scope.getDataExchanges().getCommodity())
          {
            for (Exchange xchange : commodity.getExchange())
            {
              if (xId.equalsIgnoreCase(xchange.getId()))
              {
                if (xchange.getPoolSize() == null)
                {
                  Integer defaultPoolSize = Integer.parseInt(settings.get("poolSize").toString());
                  xchange.setPoolSize(defaultPoolSize);
                }

                return xchange;
              }
            }
          }
        }

        break;
      }
    }

    return exchange;
  }
  
  protected DataFilter createDataFilter(String filter, String sortBy, String sortOrder) throws DataModelException
  {
    DataFilter dataFilter = new DataFilter();

    try
    {
      @SuppressWarnings("unchecked")
      HashMap<String, String> valueMaps = (HashMap<String, String>) session.get("valueMaps");

      // process filtering
      if (filter != null && filter.length() > 0)
      {
        @SuppressWarnings("unchecked")
        List<Map<String, String>> filterExpressions = (List<Map<String, String>>) JSONUtil.deserialize(filter);

        if (filterExpressions != null && filterExpressions.size() > 0)
        {
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
      else
      {
        dataFilter.setExpressions(null);
      }

      // process sorting
      if (sortBy != null && sortBy.length() > 0 && sortOrder != null && sortOrder.length() > 0)
      {
        if (!sortBy.equalsIgnoreCase("Transfer Type"))
        {
          OrderExpressions orderExpressions = new OrderExpressions();
          dataFilter.setOrderExpressions(orderExpressions);
  
          OrderExpression orderExpression = new OrderExpression();
          orderExpressions.getItems().add(orderExpression);
  
          if (sortBy != null)
            orderExpression.setPropertyName(sortBy);
  
          if (sortOrder != null)
            orderExpression.setSortOrder(SortOrder.valueOf(sortOrder));
        }
      }
      else
      {
        dataFilter.setOrderExpressions(null);
      }
    }
    catch (Exception e)
    {
      String message = "Error creating data filter: " + e;
      logger.error(message);
      throw new DataModelException(message);
    }

    return dataFilter;
  }
  
  protected Grid createDtoGrid(String serviceUri, String relativePath, Manifest manifest, Graph graph,
      DataTransferObjects dtos) throws DataModelException
  {
    Grid dtoGrid = new Grid();

    if (graph != null)
    {
      if (graph.getClassTemplatesList() != null && graph.getClassTemplatesList().getItems().size() > 0
          && graph.getClassTemplatesList().getItems().get(0).getClazz() != null)
      {
        String className = IOUtils.toCamelCase(graph.getClassTemplatesList().getItems().get(0).getClazz().getName());
        dtoGrid.setDescription(className);
      }

      List<Field> fields = getFields(relativePath, graph, null);
      dtoGrid.setFields(fields);

      List<List<String>> gridData = new ArrayList<List<String>>();
      dtoGrid.setData(gridData);

      if (dtos != null && dtos.getDataTransferObjectList() != null
          && dtos.getDataTransferObjectList().getItems().size() > 0)
      {
        List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();

        boolean showContentField = false;

        for (int dtoIndex = 0; dtoIndex < dtoList.size(); dtoIndex++)
        {
          DataTransferObject dto = dtoList.get(dtoIndex);

          List<String> rowData = new ArrayList<String>();
          List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();

          // create a place holder for info field
          rowData.add("");
          String relatedClassesJson;

          try
          {
            relatedClassesJson = JSONUtil.serialize(relatedClasses);
          }
          catch (JSONException e)
          {
            relatedClassesJson = "[]";
          }

          if (dataMode == DataMode.EXCHANGE)
          {
            String transferType = dto.getTransferType().toString();
            rowData.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
            
//            rowData.add("<input type=\"image\" src=\"resources/images/" + transferType.toLowerCase()
//                + ".png\" width=15 heigt=15 " + "  onMouseOver= 'javascript:showMessage(\""
//                + transferType.toLowerCase() + "\")' " + "onClick='javascript:showChangedItemsInfo()'>");
          }

          if (dataMode == DataMode.EXCHANGE)
          {
            //rowData.add((dto.getDuplicateCount().toString()));
            
            if (dto.getDuplicateCount() == null)
            {
              rowData.add("<img rc=\"resources/images/warning.png\" width=16 " + 
                  "title=\"Invalid duplicate count.\">");
            }
            else if (dto.getDuplicateCount() == 1)
            {
              rowData.add("<img src=\"resources/images/success.png\" width=16 " + 
                  "title=\"This row is good to exchange.\">");
            }
            else
            {
              rowData.add("<img src=\"resources/images/error.png\" width=16 " + 
                  "title=\"This row can not be exchanged due to [" + 
                  dto.getDuplicateCount() + "] duplicates.\">");
            }
          }

          if (dto.getClassObjects().getItems().size() > 0)
          {
            ClassObject classObject = dto.getClassObjects().getItems().get(0);
            dtoGrid.setIdentifier(classObject.getClassId());

            processClassObject(manifest, graph, dto, dtoIndex, fields, classObject, dtoGrid, rowData, relatedClasses);
          }

          // setting color icon if the row contain Dup's

          if (dataMode == DataMode.APP)
          {
            if (dto.getHasContent())
            {
              String filter = "";

              try
              {
                filter = URLEncoder.encode("{\"" + dto.getIdentifier() + "\":\"\"}", "utf-8");
              }
              catch (UnsupportedEncodingException e)
              {
                e.printStackTrace();
                filter = "{%22" + dto.getIdentifier() + "%22:%22%22}";
              }

              String target = serviceUri + relativePath + "/content?filter=" + filter;
              String value = String.format(
                  "<a href=\"content?target=%s\" target=\"_blank\"><img src=\"resources/images/content.png\"/></a>",
                  target);
              rowData.set(1, value);

              showContentField = true;
            }
            else
            {
              rowData.set(1, "");
            }
          }

          // update info field
          rowData.set(0, "<input type=\"image\" src=\"resources/images/info-small.png\" "
              + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\",\"" + dto.getIdentifier()
              + "\"," + relatedClassesJson + ")'>");
          // }

          gridData.add(rowData);
        }

        if (showContentField)
        {
          // hide content field?
        }
      }
    }

    return dtoGrid;
  }
  
  private void processClassObject(Manifest manifest, Graph graph, DataTransferObject dto, int dtoIndex,
      List<Field> fields, ClassObject classObject, Grid dtoGrid, List<String> rowData, List<RelatedClass> relatedClasses)
      throws DataModelException
  {
    String className = IOUtils.toCamelCase(classObject.getName());
    int classIndex = classObject.getIndex();

    for (TemplateObject templateObject : classObject.getTemplateObjects().getItems())
    {
      for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
      {
        RoleType roleType = roleObject.getType();
        RoleValues roleValues = roleObject.getValues();
        RoleValues roleOldValues = roleObject.getOldValues();
        String roleValue = roleObject.getValue();
        String roleOldValue = roleObject.getOldValue();
        Cardinality cardinality = getCardinality(graph, className, templateObject.getName(), roleObject.getName(),
            roleObject.getRelatedClassName());

        if (templateObject.getTransferType() == TransferType.CHANGE)
        {
          if (roleOldValue == null)
            roleOldValue = "";
          if (roleValue == null)
            roleValue = "";
        }

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
            }

            if (dataMode == DataMode.EXCHANGE && !IOUtils.isNullOrEmpty(roleOldValue))
            {
              roleOldValue = getValueMap(manifest, roleOldValue);
            }
          }
          else if (roleValues != null && roleValues.getItems().size() > 0)
          {
            roleValue = getMultiRoleValues(manifest, roleObject, roleValues.getItems());

            if (roleOldValues != null && roleOldValues.getItems().size() > 0)
              roleOldValue = getMultiRoleValues(manifest, roleObject, roleOldValues.getItems());
          }

          // find the right column to insert value, fill in blank for
          // any gap
          // (because class/template do not exist, e.g. due to null
          // class identifier)
          
          String dataIndex; 
          if(classIndex != 0)
        	  dataIndex = className+ '-'+classIndex + '.' + templateObject.getName() + '.' + roleObject.getName();
          else
        	  dataIndex = className + '.' + templateObject.getName() + '.' + roleObject.getName();
        	  
          if (rowData.size() == fields.size())
          {
            for (int i = 0; i < fields.size(); i++)
            {
              if (fields.get(i).getDataIndex().equalsIgnoreCase(dataIndex))
              {
                if (dataMode == DataMode.APP || roleOldValue == null || roleOldValue.equals(roleValue))
                {
                  rowData.set(i, roleValue);
                }
                else
                {
                  roleValue = roleOldValue + " -> " + roleValue;
                  rowData.set(i, "<span class=\"change\">" + roleValue + "</span>");
                }

                break;
              }
            }
          }
          else
          {
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

  protected <T> T waitForRequestCompletion(Class<T> clazz, String url) throws Exception
  {
    T obj = null;

    RequestStatus requestStatus = null;
    long timeoutCount = 0;

    HttpClient httpClient = new HttpClient(url);
    HttpUtils.addHttpHeaders(settings, httpClient);

    while (timeoutCount < asyncTimeout)
    {
      requestStatus = httpClient.get(RequestStatus.class);

      if (requestStatus.getState() != State.IN_PROGRESS)
        break;

      Thread.sleep(asyncPollingInterval);
      timeoutCount += asyncPollingInterval;
    }

    if (requestStatus.getState() == State.COMPLETED)
    {
      // Note that the requestStatus object will have been decoded (out of UTF-8) during the httpClient.get(), so if the
      // object embedded within the
      // requestStatus.ResponseText has non UTF-8 characters then we must encode that back into UTF-8 before passing to
      // JaxbUtils.toObject
      InputStream streamUTF8 = new ByteArrayInputStream(requestStatus.getResponseText().getBytes("UTF-8"));
      obj = (T) JaxbUtils.toObject(clazz, streamUTF8);
    }
    else
    {
      throw new Exception(requestStatus.getMessage());
    }

    return obj;
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
  
  @SuppressWarnings("unchecked")
  protected List<Field> getFields(String dtiRelativePath, Graph graph, String startClassId) throws DataModelException
  {
    List<Field> fields = null;
    String fieldsKey = FIELDS_PREFIX + dtiRelativePath + graph.getName();

    if (settings.containsKey(fieldsKey))
    {
      fields = (List<Field>) settings.get(fieldsKey);
    }
    else
    {
      fields = createFields(graph, startClassId);
      settings.put(fieldsKey, fields);
    }

    return fields;
  }

  private List<Field> createFields(Graph graph, String startClassId)
  {
    List<Field> fields = new ArrayList<Field>();

    if (dataMode == DataMode.EXCHANGE)
    {
      // Dups count field
      Field dupField = new Field();
      dupField.setName("Status");
      dupField.setDataIndex("_status");
      dupField.setType("string");
      dupField.setWidth(STATUS_FIELD_WIDTH);
      dupField.setFixed(true);
      dupField.setFilterable(false);
      dupField.setSortable(false);
      fields.add(0, dupField);

      // transfer-type field
      Field transferField = new Field();
      transferField.setName("Transfer Type");
      transferField.setDataIndex("Transfer Type");
      transferField.setType("string");
      transferField.setWidth(TRANSFER_FIELD_WIDTH);
      transferField.setFilterable(true);
      transferField.setSortable(false);
      fields.add(0, transferField);
    }

    if (dataMode == DataMode.APP)
    {
      // content field
      Field contentField = new Field();
      contentField.setName("Content");
      contentField.setDataIndex("_content");
      contentField.setType("string");
      contentField.setWidth(CONTENT_FIELD_WIDTH);
      contentField.setFixed(true);
      contentField.setFilterable(false);
      contentField.setSortable(false);
      fields.add(0, contentField);
    }

    // info field
    Field infoField = new Field();
    infoField.setName("Info");
    infoField.setDataIndex("_info");
    infoField.setType("string");
    infoField.setWidth(INFO_FIELD_WIDTH);
    infoField.setFixed(true);
    infoField.setFilterable(false);
    infoField.setSortable(false);
    fields.add(0, infoField);

    List<ClassTemplates> classTemplatesItems = graph.getClassTemplatesList().getItems();

    if (classTemplatesItems.size() > 0)
    {
      if (startClassId == null || startClassId.length() == 0)
      {
        ClassTemplates classTemplates = classTemplatesItems.get(0);
        createFields(fields, graph, classTemplates,0);
      }
      else
      {
        for (ClassTemplates classTempates : classTemplatesItems)
        {
          if (classTempates.getClazz().getId().equalsIgnoreCase(startClassId))
          {
            createFields(fields, graph, classTempates,classTempates.getClazz().getIndex());
            break;
          }
        }
      }
    }

    return fields;
  }

  private void createFields(List<Field> fields, Graph graph, ClassTemplates classTemplates, int classIndex)
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

            Field field = new Field();
            
            String fieldName;

            if(classIndex != 0)
            	fieldName = className + '-'+ classIndex+ '.' + template.getName() + "." + role.getName();
            else
            	fieldName = className + '.' + template.getName() + "." + role.getName();
            
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
            int clsIndex = role.getClazz().getIndex();
            
            ClassTemplates relatedClassTemplates = getClassTemplates(graph, classId);
            createFields(fields, graph, relatedClassTemplates,clsIndex);
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
  
  private ClassTemplates getClassTemplates(Graph graph, String classId)
  {
    for (ClassTemplates classTemplates : graph.getClassTemplatesList().getItems())
    {
      if (classTemplates.getClazz().getId().equals(classId))
        return classTemplates;
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

  protected String getValueMap(Manifest manifest, String value) throws DataModelException
  {
    if (value == null || value.isEmpty())
    {
      return value;
    }
    
    // find in cache
    if (valueMapsCache.containsKey(value))
    {
      return valueMapsCache.get(value);
    }
    
    //
    // if not found, find in manifest
    //
    if (manifest != null && manifest.getValueListMaps() != null)
    {
      for (ValueListMap vlm : manifest.getValueListMaps().getItems())
      {
        if (vlm.getValueMaps() != null)
        {
          for (ValueMap vm : vlm.getValueMaps().getItems())
          {
            if (vm.getUri() != null && vm.getUri().equalsIgnoreCase(value) && vm.getLabel() != null
                && vm.getLabel().length() > 0)
            {
              String valueMap = vm.getLabel();
              valueMapsCache.put(value, valueMap);
              
              return valueMap;
            }
          }
        }
      }
    }

    // if still not found, query reference data service
    String valueMap = resolveValueMap(value);
    valueMapsCache.put(value, valueMap);

    return valueMap;
  }

  protected String resolveValueMap(String id) throws DataModelException
  {
    String label = id;

    try
    {
      HttpClient httpClient = new HttpClient(refDataServiceUri);
      HttpUtils.addHttpHeaders(settings, httpClient);

      Entity value = httpClient.get(Entity.class, "/classes/" + id.substring(4, id.length()) + "/label");

      if (value != null && value.getLabel() != null)
      {
        label = value.getLabel();
      }
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
    }

    return label;
  }
  
  protected Manifest getManifest(String serviceUri, String manifestRelativePath) throws DataModelException
  {
    Manifest manifest = null;
    String manifestKey = MANIFEST_PREFIX + manifestRelativePath;

    if (settings.containsKey(manifestKey))
    {
      manifest = (Manifest) settings.get(manifestKey);
    }
    else
    {
      HttpClient httpClient = new HttpClient(serviceUri);
      HttpUtils.addHttpHeaders(settings, httpClient);

      try
      {
        manifest = httpClient.get(Manifest.class, manifestRelativePath);
        settings.put(manifestKey, manifest);
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
  
  // NOTE: the order of the indices in the list needs to retain to honor grid sorting
  protected void collapseDuplicates(DataTransferIndices dtis) throws Exception
  {
    try
    {
      if (dtis == null || dtis.getDataTransferIndexList() == null)
        return;

      List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();

      for (int i = 0; i < dtiList.size(); i++)
      {
        DataTransferIndex dti = dtiList.get(i);
        int dupes = 1;
        
        for (int j = i + 1; j < dtiList.size(); j++)
        {
          if (dti.getIdentifier().equalsIgnoreCase(dtiList.get(j).getIdentifier()))
          {
            dupes++;
            dtiList.remove(j--);
          }
        }
        
        dti.setDuplicateCount(dupes);
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
      logger.error(e.getMessage());
      throw e;
    }
  }
}
