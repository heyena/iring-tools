using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using org.ids_adi.qmxf;
using VDS.RDF;
using System.Text;
using log4net;
using System.Web.Script.Serialization;

namespace org.iringtools.web.controllers
{
  public class DatagridController : BaseController
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DatagridController));
    private IGridRepository _repository { get; set; }
    private DataDictionary dataDict;
    private DataItems dataItems;
    private Grid dataGrid;
    private string graph;
    private Grid pageDtoGrid;
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
        _key = string.Format("Data-full-{0}.{1}.{2}", scope, app, graph);
        string filter = form["filter"];
        string sort = form["sort"];
        string dir = form["dir"];
        int start = 0;
        int.TryParse(form["start"], out start);
        int limit = 25;
        int.TryParse(form["limit"], out limit);
        string currFilter = filter + "/" + sort + "/" + dir;
        DataFilter dataFilter = CreateDataFilter(filter, sort, dir);
        
        if (dataDict == null)
          GetDatadictionary(scope, app);

        DataObject dataObject = dataDict.dataObjects.FirstOrDefault(d => d.objectName == graph);
        var fields = from row in dataObject.dataProperties
                     select new
                     {
                       name = row.columnName,
                       header = row.columnName,
                       dataIndex = row.columnName,
                       sortable = true
                     };
        if (Session[_key] == null)
        {
          Session[_key] = GetDataObjects(scope, app, graph, dataFilter);
        }
        
        dataItems = (DataItems)Session[_key];
     
        long total = dataItems.total;
        var paginatedData = dataItems.items.Skip(start)
                                           .Take(limit)
                                           .ToList();

        foreach (DataItem dataItem in paginatedData.ToList())
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

    //public ActionResult Pages(FormCollection form)
    //{
    //  JsonContainer<Grid> container = new JsonContainer<Grid>();
    //  pageDtoGrid = GetGrid(form["scope"], form["app"], form["graph"], form["filter"], form["sort"], form["dir"], form["start"], form["limit"]);

    //  //string response = GetResponse();
    //  if (response != "success")
    //  {
    //    return Json(new { success = false } + response, JsonRequestBehavior.AllowGet);
    //  }
    //  return Json(pageDtoGrid, JsonRequestBehavior.AllowGet);
    //}

    //public ActionResult SetSession()
    //{

    //  return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    //}

    //public ActionResult CleanSession()
    //{
    //  foreach (string key in Session.Keys)
    //  {
    //    if (key.StartsWith("AppGrid"))
    //    {
    //      Session.Remove(key);
    //    }
    //  }
    //  return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    //}

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
      DataFilter dataFilter = null;

      // process filtering
      if (filter != null && filter.Count() > 0)
      {
        try
        {
          List<Dictionary<String, String>> filterExpressions = (List<Dictionary<String, String>>)serializer.Deserialize(filter, typeof(List<Dictionary<String, String>>));

          if (filterExpressions != null && filterExpressions.Count > 0)
          {
            dataFilter = new DataFilter();

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
        if (dataFilter == null)
          dataFilter = new DataFilter();

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

    //private void CreateFields(ref List<Field> fields, ref List<List<string>> gridData)
    //{
    //  string type;
    //  foreach (DataObject dataObj in dataDict.dataObjects)
    //  {
    //    if (dataObj.objectName.ToUpper() != graph.ToUpper())
    //      continue;
    //    else
    //    {
    //      foreach (DataProperty dataProp in dataObj.dataProperties)
    //      {
    //        Field field = new Field();
    //        string fieldName = dataProp.columnName;
    //        field.dataIndex = fieldName;
    //        field.header = fieldName;
    //        field.name = fieldName;

    //        int fieldWidth = fieldName.Count() * 6;

    //        if (fieldWidth > 40)
    //        {
    //          field.width = fieldWidth + 20;
    //        }
    //        else
    //        {
    //          field.width = 50;
    //        }

    //        type = dataProp.dataType.ToString().ToLower();
    //        if (type == "single")
    //          type = "auto";
    //        field.type = type;
    //        fields.Add(field);
    //      }
    //    }
    //  }

    //  int newWid;
    //  foreach (DataItem dataItem in dataItems.items)
    //  {
    //    List<string> rowData = new List<string>();
    //    foreach (Field field in fields)
    //    {
    //      foreach (KeyValuePair<string, string> property in dataItem.properties)
    //      {
    //        if (field.dataIndex.ToLower() == property.Key.ToLower())
    //        {
    //          rowData.Add(property.Value);
    //          newWid = property.Value.Count() * 4 + 40;
    //          if (newWid > 40 && newWid > field.width && newWid < 300)
    //            field.width = newWid;
    //          break;
    //        }
    //      }
    //    }
    //    gridData.Add(rowData);
    //  }
    //}

    //private void GetDataGrid()
    //{
    //  List<List<string>> gridData = new List<List<string>>();
    //  List<Field> fields = new List<Field>();
    //  CreateFields(ref fields, ref gridData);
    //  dataGrid.fields = fields;
    //  dataGrid.data = gridData;
    //}

    //private void GetPageData(DataItems allDataItems, string start, string limit)
    //{
    //  int startNum = int.Parse(start);
    //  int limitNum = int.Parse(limit);
    //  DataItems pageData = new DataItems();
    //  pageData.total = allDataItems.total;
    //  pageData.type = allDataItems.type;
    //  pageData.items = new List<DataItem>();
    //  int indexLimit = Math.Min((int)allDataItems.total, startNum + limitNum);

    //  for (int i = startNum; i < indexLimit; i++)
    //    pageData.items.Add(allDataItems.items.ElementAt(i));

    //  dataItems = pageData;
    //}

    //private void SetGridData(string scope, string app, string graph, string start, string limit, DataFilter dataFilter, string partialKey)
    //{
    //  DataItems allDataItems = null;
    //  string key;

    //  if (dataFilter == null)
    //    key = "AppGrid-full/" + scope + "/" + app + "/" + graph;
    //  else
    //    key = partialKey;

    //  if (Session != null)
    //  {
    //    if (Session[key] == null)
    //    {
    //      allDataItems = GetDataObjects( scope, app, graph, dataFilter);
    //    }
    //    else
    //      allDataItems = (DataItems)Session[key];
    //  }
    //  else
    //    GetDataObjects(scope, app, graph, dataFilter);

    //  dataGrid.total = (int)allDataItems.total;
    //  GetPageData(allDataItems, start, limit);
    //}

    //private void GetDataItems(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
    //{
    //  string currFilter = filter + "/" + sort + "/" + dir;

    //  DataFilter dataFilter = CreateDataFilter(filter, sort, dir);
    //  try
    //  {
    //    string partialKey = "AppGrid-part/" + scope + "/" + app + "/" + graph + "/" + filter + "/" + sort + "/" + dir;
    //    SetGridData(scope, app, graph, start, limit, dataFilter, partialKey);
    //  }
    //  catch (Exception ex)
    //  {
    //    _logger.Error("Error getting DatabaseDictionary." + ex);
    //    if (response == "success")
    //      response = ex.Message.ToString();
    //  }
    //}

    private void GetDatadictionary(String scope, String app)
    {
      try
      {
        string reluri = string.Format("{0}/{1}", app, scope);
        dataDict = _repository.GetDictionary(reluri);
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

    //public Grid GetGrid(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
    //{


    //  this.graph = graph;
    //  GetDatadictionary(scope, app);

    //  if (response != "success")
    //    return null;

    //  GetDataItems(scope, app, graph, filter, sort, dir, start, limit);

    //  if (response != "success")
    //    return null;

    //  GetDataGrid();

    //  if (response != "success")
    //    return null;

    //  return dataGrid;
    //}


    private DataItems GetDataObjects(string scope, string app, string graph, DataFilter dataFilter)
    {
      DataItems allDataItems = null;
      try
      {
        allDataItems = _repository.GetDataItems(app, scope, graph, dataFilter);
      }
      catch (Exception ex)
      {
        if (ex.InnerException != null)
          _logger.Error("Error deserializing filtered data objects: " + ex.InnerException);
        _logger.Error("Error deserializing filtered data objects: " + ex);
        if (response == "success")
          response = ex.Message.ToString() + " " + ex.InnerException.Message.ToString();
      }

      return allDataItems;
    }

  }
}
