using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Web.Script.Serialization;
using org.iringtools.library;
using org.iringtools.utility;

namespace DynamicGridDemo.Models
{
  public class DynamicGridModel
  {
    private String dataServiceUri;
    private JavaScriptSerializer serializer;
    private Dictionary<String, RelationalOperator> relationalOperatorMap;

    public DynamicGridModel()
    {
      NameValueCollection appSettings = ConfigurationManager.AppSettings;

      dataServiceUri = appSettings["DataServiceUri"];
      serializer = new JavaScriptSerializer();

      relationalOperatorMap = new Dictionary<String, RelationalOperator>()
      {
        { "eq", RelationalOperator.EqualTo },
        { "lt", RelationalOperator.LesserThan },
        { "gt", RelationalOperator.GreaterThan }
      };
    }

    public GridDefinition GetGridDefinition(String dataObject)
    {
      WebHttpClient client = new WebHttpClient(dataServiceUri);
      DataDictionary dictionary = client.Get<DataDictionary>("dictionary");
      return PrepareGridDefinition(dictionary, dataObject);
    }

    public GridData GetGridData(String dataObject, int start, int limit, 
      String sortBy, String sortDir, String filter)
    {
      DataFilter dataFilter = CreateDataFilter(sortBy, sortDir, filter);
      String relativeURI = dataObject + "/filter?format=json&start=" + start + "&limit=" + limit;
      WebHttpClient client = new WebHttpClient(dataServiceUri);
      String dataItemsJson = client.Post<DataFilter, String>(relativeURI, dataFilter);
      DataItems dataItems = (DataItems)serializer.Deserialize(dataItemsJson, typeof(DataItems));
      return PrepareGridData(dataItems);
    }

    private DataFilter CreateDataFilter(String sortBy, String sortDir, String filter)
    {
      DataFilter dataFilter = null;

      #region process filtering
      if (!String.IsNullOrEmpty(filter))
      {
        List<Dictionary<String, String>> filterExpressions = 
          (List<Dictionary<String, String>>) serializer.Deserialize(filter, typeof(List<Dictionary<String, String>>));
        
        if (filterExpressions != null && filterExpressions.Count > 0)
        {
          dataFilter = new DataFilter();

          foreach (Dictionary<String, String> filterExpression in filterExpressions)
          {
            Expression expression = new Expression();

            if (dataFilter.Expressions.Count > 1)
            {
              expression.LogicalOperator = LogicalOperator.And;
            }

            expression.PropertyName = filterExpression["field"];

            if (filterExpression.ContainsKey("comparison"))
            {
              string op = filterExpression["comparison"];
              expression.RelationalOperator = relationalOperatorMap[op];
            }
            else
            {
              expression.RelationalOperator = relationalOperatorMap["eq"];
            }

            expression.Values = new Values();
            expression.Values.Add(filterExpression["value"]);

            dataFilter.Expressions.Add(expression);
          }
        }
      }
      #endregion

      #region process sorting
      if (!String.IsNullOrEmpty(sortBy))
      {
        if (dataFilter == null)
        {
          dataFilter = new DataFilter();
        }

        OrderExpression orderExpression = new OrderExpression()
        {
          PropertyName = sortBy,
          SortOrder = (sortDir.ToUpper() == "DESC") ? SortOrder.Desc : SortOrder.Asc
        };

        dataFilter.OrderExpressions.Add(orderExpression);
      }
      #endregion

      return dataFilter;
    }

    private GridDefinition PrepareGridDefinition(DataDictionary dictionary, String dataObject)
    {
      GridDefinition gridDef = new GridDefinition();
      DataObject dataObj = dictionary.dataObjects.Find(o => o.objectName.ToUpper() == dataObject.ToUpper());

      if (dataObj != null)
      {
        foreach (DataProperty dataProperty in dataObj.dataProperties)
        {
          ColumnDefinition column = new ColumnDefinition()
          {
            header = dataProperty.propertyName.ToUpper(),
            dataIndex = dataProperty.propertyName,
            align = IsNumeric(dataProperty.dataType) ? "right" : "left"
          };

          KeyProperty keyProperty = (dataObj.keyProperties.Find(o => o.keyPropertyName.ToUpper() == dataProperty.propertyName.ToUpper()));
          if (keyProperty != null)
          {
            column.id = dataProperty.propertyName;
            gridDef.idProperty = dataProperty.propertyName;
          }

          gridDef.columns.Add(column);

          Dictionary<string, string> field = new Dictionary<string, string>()
          {
            { "name", dataProperty.propertyName },   
            { "type", ToExtJsType(dataProperty.dataType) }
          };

          gridDef.fields.Add(field);

          Dictionary<string, string> filter = new Dictionary<string, string>()
          {
            { "dataIndex", dataProperty.propertyName },   
            { "type", IsNumeric(dataProperty.dataType) ? "numeric" : ToExtJsType(dataProperty.dataType) }
          };

          gridDef.filters.Add(filter);
        }
      }

      return gridDef;
    }

    private GridData PrepareGridData(DataItems dataItems)
    {
      GridData gridData = new GridData()
      {
        total = Convert.ToString(dataItems.total)
      };

      foreach (DataItem dataItem in dataItems.items)
      {
        gridData.rows.Add(dataItem.properties);
      }

      return gridData;
    }

    private bool IsNumeric(org.iringtools.library.DataType dataType)
    {
      return
        dataType == org.iringtools.library.DataType.Byte ||
        dataType == org.iringtools.library.DataType.Int16 ||
        dataType == org.iringtools.library.DataType.Int32 ||
        dataType == org.iringtools.library.DataType.Int64 ||
        dataType == org.iringtools.library.DataType.Single ||
        dataType == org.iringtools.library.DataType.Double ||
        dataType == org.iringtools.library.DataType.Decimal;
    }

    private String ToExtJsType(org.iringtools.library.DataType dataType)
    {
      switch (dataType)
      {
        case org.iringtools.library.DataType.Boolean:
          return "boolean";

        case org.iringtools.library.DataType.Char:
        case org.iringtools.library.DataType.String:
          return "string";

        case org.iringtools.library.DataType.DateTime:
          return "date";

        case org.iringtools.library.DataType.Byte:
        case org.iringtools.library.DataType.Int16:
        case org.iringtools.library.DataType.Int32:
        case org.iringtools.library.DataType.Int64:
          return "int";

        case org.iringtools.library.DataType.Single:
        case org.iringtools.library.DataType.Double:
        case org.iringtools.library.DataType.Decimal:
          return "float";

        default:
          return "auto";
      }
    }
  }
}
