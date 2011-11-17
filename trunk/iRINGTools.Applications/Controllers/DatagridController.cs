using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


using org.iringtools.library;

using iRINGTools.Web.Models;
using log4net;
using System.Web.Script.Serialization;

namespace org.iringtools.web.controllers
{
  public class DatagridController : BaseController
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DatagridController));
    private IGridRepository _repository { get; set; }
    private DataDictionary dataDict;

    private JavaScriptSerializer serializer;
    private string response = "success";
    private string _key = null;

    public DatagridController() : this(new GridRepository()) { }

    public DatagridController(IGridRepository repository)
    {
      _repository = repository;

      serializer = new JavaScriptSerializer();
    }

    public JsonResult GetData(FormCollection form)
    {
      try
      {

        var metaData = new Dictionary<string, object>();
        var gridData = new List<Dictionary<string, object>>();
        var encode = new Dictionary<string, object>();
        DataItems dataItems = new DataItems();
        string scope = form["scope"];
        string app = form["app"];
        string graph = form["graph"];
        _key = string.Format("Dictionary-{0}.{1}", scope, app);
        string filter = form["filter"];
        string sort = form["sort"];
        string dir = form["dir"];
        int start = 0;
        int.TryParse(form["start"], out start);
        int limit = 25;
        int.TryParse(form["limit"], out limit);
        string currFilter = filter + "/" + sort + "/" + dir;
        DataFilter dataFilter = CreateDataFilter(filter, sort, dir);

        if (((DataDictionary)Session[_key]) == null)
            GetDatadictionary(scope, app);

        DataObject dataObject = ((DataDictionary)Session[_key]).dataObjects.FirstOrDefault(d => d.objectName == graph);
        var fields = from row in dataObject.dataProperties
                     select new
                     {
                       name = row.columnName,
                       header = row.columnName,
                       dataIndex = row.columnName,
                       sortable = true
                     };


          dataItems = GetDataObjects(scope, app, graph, dataFilter,start, limit);
     
        long total = dataItems.total;

        foreach (DataItem dataItem in dataItems.items)
        {
          var rowData = new Dictionary<string, object>();
          foreach (var field in fields)
          {
            foreach (KeyValuePair<string, string> property in dataItem.properties)
            {
              if (field.dataIndex.ToLower() == property.Key.ToLower())
              {
                rowData.Add(property.Key, property.Value);
                break;
              }
            }
          }
          gridData.Add(rowData);
        }

        metaData.Add("root", "data");
        metaData.Add("fields", fields);
        encode.Add("metaData", metaData);
        encode.Add("success", "true");
        encode.Add("data", gridData);
        encode.Add("totalCount", total);
        return Json(encode, JsonRequestBehavior.AllowGet);
      }
      catch (Exception ex)
      {
        _logger.Error(ex);
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
    }

    private RelationalOperator GetOpt(string opt)
    {
      switch (opt.ToLower())
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
        var dataFilter = new DataFilter();

      // process filtering
      if (filter != null && filter.Count() > 0)
      {
        try
        {
          List<Dictionary<String, String>> filterExpressions = (List<Dictionary<String, String>>)serializer.Deserialize(filter, typeof(List<Dictionary<String, String>>));

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
          if (response == "success")
            response = ex.Message.ToString();
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

        string Sortorder = sortOrder.Substring(0, 1).ToUpper() + sortOrder.Substring(1);

        if (Sortorder != null)
        {
          try
          {
            orderExpression.SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), Sortorder);
          }
          catch (Exception ex)
          {
            _logger.Error(ex.ToString());
            response = ex.Message.ToString();
          }
        }
      }

      return dataFilter;
    }

    private void GetDatadictionary(String scope, String app)
    {
      try
      {
        string reluri = string.Format("{0}/{1}", app, scope);
        Session[_key] = _repository.GetDictionary(reluri);
        if (dataDict.dataObjects.Count == 0)
          response = "There is no records in the database for data object \"" + app + "\"";
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting DatabaseDictionary." + ex);
        if (response == "success")
          response = ex.Message.ToString();
      }
    }


    private DataItems GetDataObjects(string scope, string app, string graph, DataFilter dataFilter, int start, int limit)
    {
      DataItems dataItems = null;
      try
      {
        dataItems = _repository.GetDataItems(app, scope, graph, dataFilter, start, limit);
      }
      catch (Exception ex)
      {
        if (ex.InnerException != null)
        _logger.Error("Error deserializing filtered data objects: " + ex);
        if (response == "success")
          response = ex.Message.ToString() + " " + ex.InnerException.Message.ToString();
      }

      return dataItems;
    }

  }
}
