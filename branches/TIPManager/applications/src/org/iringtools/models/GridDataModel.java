package org.iringtools.models;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.data.DataItem;
import org.iringtools.data.DataItems;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.common.ext.DataFilterExtension;
import org.iringtools.library.DataDictionary;
import org.iringtools.library.DataObject;
import org.iringtools.library.DataProperty;
import org.iringtools.library.DataType;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;

import com.google.gson.Gson;

public class GridDataModel extends DataModel
{
  private static final Logger logger = Logger.getLogger(GridDataModel.class);

  private HttpClient httpClient;
  private String message = "";
  private DataDictionary dataDict;
  private DataItems dataItems;
  private Grid dataGrid;
  private List<List<String>> gridData;
  private List<Field> fields;
  
  public GridDataModel(Map<String, Object> session)
  {
    super();
    this.session = session;
  }

  public Grid getDtoGrid(String baseUri, String scope, String app, String graph, String filter, String sort,
      String dir, int start, int limit)
  {
    dataGrid = new Grid();

    try
    {
      getDatadictionary(baseUri, scope, app);
      getDataItems(scope, app, graph, filter, sort, dir, start, limit);
      getDataGrid(graph);
    }
    catch (Exception ex)
    {
      logger.error("Error getting data grid." + ex);
      message = message + " " + ex.getMessage().toString();
    }

    dataGrid.setMVCErrorMessage(message);
    return dataGrid;
  }

  private void getDatadictionary(String baseUri, String scope, String app)
  {
    try
    {
      httpClient = new HttpClient(baseUri);
      HttpUtils.addHttpHeaders(session, httpClient);
      dataDict = httpClient.get(DataDictionary.class, "/" + app + "/" + scope + "/dictionary");

      if (dataDict == null || dataDict.getDataObjects().getItems().size() == 0)
        message = message + "Data dictionary of [" + app + "] is empty.";
    }
    catch (Exception ex)
    {
      logger.error("Error getting dictionary." + ex);
      message = message + " " + ex.getMessage().toString();
    }
  }

  private void getDataItems(String scope, String app, String graph, String filter, String sort, String dir, int start,
      int limit)
  {
    try
    {
      Gson gson = new Gson();
      DataFilterExtension dataFilterExt = new DataFilterExtension();
      DataFilter dataFilter = dataFilterExt.getDataFilter(createDataFilter(filter, sort, dir));   
      String dataFilterJson = gson.toJson(dataFilter);
      String dataItemsStr = httpClient.post(String.class, "/" + app + "/" + scope + "/" + graph
          + "/filter?format=json&start=" + start + "&limit=" + limit, dataFilterJson, "application/octet-stream");
      dataItems = gson.fromJson(dataItemsStr, DataItems.class);
    }
    catch (Exception ex)
    {
      logger.error("Error getting data items." + ex);
      message = message + " " + ex.getMessage().toString();
    }
  }

  private void getDataGrid(String graph)
  {
    createFields(graph);
    dataGrid.setTotal(dataItems.getTotal().intValue());
    dataGrid.setFields(fields);
    dataGrid.setData(gridData);
  }

  private void createFields(String graph)
  {
    gridData = new ArrayList<List<String>>();
    fields = new ArrayList<Field>();
    String graphUp = graph.toUpperCase();
    String fieldName;

    for (DataObject dataObj : dataDict.getDataObjects().getItems())
    {
      if (dataObj.getObjectName().toUpperCase().compareTo(graphUp) != 0)
        continue;
      else
      {
        for (DataProperty dataProp : dataObj.getDataProperties().getItems())
        {
          Field field = new Field();
          fieldName = dataProp.getPropertyName();
          field.setDataIndex(fieldName);
          field.setName(fieldName);

          int fieldWidth = fieldName.length() * 6;

          if (fieldWidth > 40)
          {
            field.setWidth(fieldWidth + 23);
          }
          else
          {
            field.setWidth(50);
          }

          field.setType(ToExtJsType(dataProp.getDataType()));
          fields.add(field);
        }
      }
    }

    int newWid;
    String value;
    HashMap<String, String> property = null;
    DataItem dataItem;

    List<DataItem> dataItemList = dataItems.getItems();
    for (int i = 0; i < dataItemList.size(); i++)
    {
      dataItem = dataItemList.get(i);
      property = dataItem.getProperties();
      List<String> rowData = new ArrayList<String>();
      for (Field field : fields)
      {
        for (String key : property.keySet())
        {
          if (field.getDataIndex().toLowerCase().compareTo(key.toLowerCase()) == 0)
          {
            value = property.get(key);
            rowData.add(value);
            newWid = value.length() * 4 + 40;
            if (newWid > 40 && newWid > field.getWidth() && newWid < 300)
              field.setWidth(newWid);
            break;
          }
        }
      }
      gridData.add(rowData);
    }
  }

  private String ToExtJsType(DataType dataType)
  {
    switch (dataType)
    {
    case BOOLEAN:
      return "boolean";

    case CHAR:
    case STRING:
      return "string";

    case DATE_TIME:
      return "date";

    case BYTE:
    case INT_16:
    case INT_32:
    case INT_64:
      return "int";

    case SINGLE:
    case DOUBLE:
    case DECIMAL:
      return "float";

    default:
      return "auto";
    }
  }
}
