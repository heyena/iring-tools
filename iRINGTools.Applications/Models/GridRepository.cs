using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Script.Serialization;
using log4net;
using Ninject;
using org.iringtools.library;
using org.iringtools.utility;
using System.Net;
using org.iringtools.adapter;

namespace iRINGTools.Web.Models
{
    public class GridRepository : AdapterRepository, IGridRepository
    {
      private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
      
      private DataDictionary dataDict;
      private DataItems dataItems;
      private Grid dataGrid;
      private string graph;
      private string response = "";

      public GridRepository()
      {
        dataGrid = new Grid();
      }

      public string GetResponse()
      {
        return response;
      }

      public Grid GetGrid(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
      {
        try
        {
          this.graph = graph;

          if (start == "0" || start == "1")
          {
            GetDataDictionary(scope, app, false);
          }
          else
          {
            GetDataDictionary(scope, app);
          }

          if (response != "")
            return null;

          GetDataItems(scope, app, graph, filter, sort, dir, start, limit);

          if (response != "")
            return null;

          GetDataGrid();

          if (response != "")
            return null;
        }
        catch (Exception ex)
        {
          response = response + " " + ex.Message.ToString();
        }

        return dataGrid;
      }

      private void GetDataDictionary(String scope, String app)
      {
        GetDataDictionary(scope, app, true);
      }

      private void GetDataDictionary(String scope, String app, bool usesCache)
      {
        try
        {
          string dictKey = string.Format("Dictionary.{0}.{1}", scope, app);

          if (usesCache)
            dataDict = (DataDictionary)Session[dictKey];
          else
            dataDict = null;

          if (dataDict == null)
          {
            WebHttpClient client = CreateWebClient(_dataServiceUri);
            dataDict = client.Get<DataDictionary>("/" + app + "/" + scope + "/dictionary?format=xml", true);

            Session[dictKey] = dataDict;
          }

          if (dataDict == null || dataDict.dataObjects.Count == 0)
            response = response + "Data dictionary of [" + app + "] is empty.";
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting dictionary." + ex);
          response = response + " " + ex.Message.ToString();
        }
      }

      private void GetDataItems(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
      {
        try
        {
          string format = "json";
          DataFilter dataFilter = CreateDataFilter(filter, sort, dir);
          string relativeUri = "/" + app + "/" + scope + "/" + graph + "/filter?format=" + format + "&start=" + start + "&limit=" + limit;
          string dataItemsJson = string.Empty;

          WebHttpClient client = CreateWebClient(_dataServiceUri);
          string isAsync = _settings["Async"];

          if (isAsync != null && isAsync.ToLower() == "false")
          {
            dataItemsJson = client.Post<DataFilter, string>(relativeUri, dataFilter, format, true);
          }
          else
          {
            client.Async = true;
            string statusUrl = client.Post<DataFilter, string>(relativeUri, dataFilter, format, true);
          
            if (string.IsNullOrEmpty(statusUrl))
            {
              throw new Exception("Asynchronous status URL not found.");
            }

            dataItemsJson = WaitForRequestCompletion<string>(_dataServiceUri, statusUrl);
          }

          DataItemSerializer serializer = new DataItemSerializer();
          dataItems = serializer.Deserialize<DataItems>(dataItemsJson, false); 
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting data items." + ex);
          response = response + " " + ex.Message.ToString();
        }
      }

      private void GetDataGrid()
      {				
        List<List<string>> gridData = new List<List<string>>();
        List<Field> fields = new List<Field>();
        CreateFields(ref fields, ref gridData);
        dataGrid.total = dataItems.total;
        dataGrid.fields = fields;
        dataGrid.data = gridData;
      }

      private void CreateFields(ref List<Field> fields, ref List<List<string>> gridData)
      {
        foreach (DataObject dataObj in dataDict.dataObjects)
        {
          if (dataObj.objectName.ToUpper() != graph.ToUpper())
            continue;
          else
          {
            foreach (DataProperty dataProp in dataObj.dataProperties)
            {
              //if (!dataProp.isHidden)
              //{
                Field field = new Field();
                string fieldName = dataProp.propertyName;
                field.dataIndex = fieldName;
                field.name = fieldName;

                int fieldWidth = fieldName.Count() * 6;

                if (fieldWidth > 40)
                {
                  field.width = fieldWidth + 23;
                }
                else
                {
                  field.width = 50;
                }

                field.type = ToExtJsType(dataProp.dataType);

                if (dataProp.keyType == KeyType.assigned || dataProp.keyType == KeyType.foreign)
                  field.keytype = "key";

                fields.Add(field);
              //}
            }
          }
        }

        int newWid;
        foreach (DataItem dataItem in dataItems.items)
        {
          List<string> rowData = new List<string>();
          foreach (Field field in fields)
          {
            bool found = false;

            foreach (KeyValuePair<string, object> property in dataItem.properties)
            {
              if (field.dataIndex.ToLower() == property.Key.ToLower())
              {
                rowData.Add(property.Value.ToString());
                newWid = property.Value.ToString().Count() * 4 + 40;
                if (newWid > 40 && newWid > field.width && newWid < 400)
                  field.width = newWid;
                found = true;
                break;
              }
            }

            if (!found)
            {
              rowData.Add("");
            }
          }
          gridData.Add(rowData);
        }
      }

      private String ToExtJsType(org.iringtools.library.DataType dataType)
      {
        switch (dataType)
        {
          case org.iringtools.library.DataType.Boolean:
            return "string";

          case org.iringtools.library.DataType.Char:
          case org.iringtools.library.DataType.String:
          case org.iringtools.library.DataType.DateTime:
            return "string";

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

      private RelationalOperator GetOpt(string opt)
      {
        switch(opt.ToLower())
        {
          case "eq":
            return RelationalOperator.EqualTo;
          case "lt":
            return RelationalOperator.LesserThan;
          case "gt":
            return RelationalOperator.GreaterThan;
        }
        return RelationalOperator.EqualTo;
      }

      private DataFilter CreateDataFilter(string filter, string sortBy, string sortOrder)
      {
        DataFilter dataFilter = new DataFilter();
        
        // process filtering
        if (filter != null && filter.Count() > 0)
        {
          try
          {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<String, String>> filterExpressions = 
              (List<Dictionary<String, String>>)serializer.Deserialize(filter, typeof(List<Dictionary<String, String>>));
            
            if (filterExpressions != null && filterExpressions.Count > 0)
            {
              List<Expression> expressions = new List<Expression>();
              dataFilter.Expressions = expressions;

              foreach (Dictionary<String, String> filterExpression in filterExpressions)
              {
                Expression expression = new Expression();
                expressions.Add(expression);

                if (expressions.Count > 1)
                {
                  expression.LogicalOperator = LogicalOperator.And;
                }

                if (filterExpression["comparison"] != null)
                {										
                  RelationalOperator optor = GetOpt(filterExpression["comparison"]);
                  expression.RelationalOperator = optor;
                }
                else
                {
                  expression.RelationalOperator = RelationalOperator.EqualTo;
                }

                expression.PropertyName = filterExpression["field"];

                Values values = new Values();
                expression.Values = values;
                string value = filterExpression["value"];
                values.Add(value);
              }
            }
          }
          catch (Exception ex)
          {
            _logger.Error("Error deserializing filter: " + ex);
            response = response + " " + ex.Message.ToString();
          }
        }

        // process sorting
        if (sortBy != null && sortBy.Count() > 0 && sortOrder != null && sortOrder.Count() > 0)
        {
          List<OrderExpression> orderExpressions = new List<OrderExpression>();
          dataFilter.OrderExpressions = orderExpressions;

          OrderExpression orderExpression = new OrderExpression();
          orderExpressions.Add(orderExpression);

          if (sortBy != null)
            orderExpression.PropertyName = sortBy;

          string sortOrderEnumVal = sortOrder.Substring(0, 1).ToUpper() + sortOrder.Substring(1).ToLower();

          if (sortOrderEnumVal != null)
          {
            try
            {
              orderExpression.SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), sortOrderEnumVal);
            }
            catch (Exception ex)
            {
              _logger.Error(ex.ToString());
              response = response + " " + ex.Message.ToString();
            }
          }
        }

        return dataFilter;
      }
    }
}