using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


using org.iringtools.library;

using iRINGTools.Web.Models;
using log4net;
using System.Web.Script.Serialization;
using org.iringtools.web.Helpers;
using org.iringtools.web.Models;

namespace org.iringtools.web.controllers
{
    public class DatagridController : BaseController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DatagridController));
        private IGridRepository Repository { get; set; }
        private DataDictionary _dataDict = null;

        private JavaScriptSerializer _serializer;
        private string _response = "";
        private string _key = null;
        private string _context = string.Empty;
        private string _endpoint = string.Empty;
        private string _baseUrl = string.Empty;

        public DatagridController() : this(new GridRepository()) { }

        public DatagridController(IGridRepository repository)
        {
            Repository = repository;
            _serializer = new JavaScriptSerializer();
        }

        public JsonResult GetData(FormCollection form)
        {
            try
            {
                SetContextEndpoint(form);
                _response = Repository.DataServiceUri();
                if (_response != "")
                  return Json(new { success = false } + _response, JsonRequestBehavior.AllowGet);

                StoreViewModel model = null;
                var columns = new List<ColumnViewModel>();
                var fields = new List<FieldViewModel>();
                var rows = new List<Dictionary<string, object>>();

                var graph = form["graph"];
                _key = adapter_PREFIX + string.Format("Datadictionary-{0}.{1}", _context, _endpoint, _baseUrl);
                var filter = form["filter"];
                var sort = form["sort"];
                var dir = form["dir"];
                var start = 0;
                int.TryParse(form["start"], out start);
                var limit = 25;
                int.TryParse(form["limit"], out limit);
                var currFilter = filter + "/" + sort + "/" + dir;
                var dataFilter = CreateDataFilter(filter, sort, dir);

                if (((DataDictionary)Session[_key]) == null)
                    GetDatadictionary(_context, _endpoint, _baseUrl);

                var dataObject = ((DataDictionary)Session[_key]).dataObjects.FirstOrDefault(d => d.objectName == graph);
                 columns = (from dataProperty in dataObject.dataProperties
                              where !dataProperty.isHidden
                              select new ColumnViewModel
                                  {
                                      Text = dataProperty.propertyName,
                                      DataIndex = dataProperty.propertyName,
                                      Flex = 2
                                  }).ToList();

              fields.AddRange(columns.Select(o => new FieldViewModel {Name = o.DataIndex}).ToList());

                var dataItems = GetDataObjects(_context, _endpoint, graph, dataFilter, start, limit, _baseUrl);

                var total = dataItems.total;

                foreach (var dataItem in dataItems.items)
                {
                    var rowData = new Dictionary<string, object>();

                    foreach (var column in columns)
                    {
                        var found = false;
                        foreach (var property in dataItem.properties.Where(property => column.DataIndex.ToLower() == property.Key.ToLower()))
                        {
                            rowData.Add(property.Key, property.Value);
                            found = true;
                            break;
                        }

                        if (!found)
                        {
                            rowData.Add(column.DataIndex, "");
                        }
                    }
                    rows.Add(rowData);
                }
              model = new StoreViewModel
                {
                  Data = rows.ToArray(),
                  MetaData = new MetaDataViewModel {Columns = columns, Fields = fields},
                  Total = total,
                  Success = true
                };

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _response = _response + " " + ex.Message.ToString();
                _logger.Error(ex + " " + _response);
                return Json(new { success = false } + _response, JsonRequestBehavior.AllowGet);
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
            if (!string.IsNullOrEmpty(filter))
            {
                try
                {
                    var filterExpressions = (List<Dictionary<String, String>>)_serializer.Deserialize(filter, typeof(List<Dictionary<String, String>>));

                    if (filterExpressions != null && filterExpressions.Count > 0)
                    {

                        var expressions = new List<Expression>();
                        dataFilter.Expressions = expressions;

                        foreach (var filterExpression in filterExpressions)
                        {
                            var expression = new Expression();
                            expressions.Add(expression);

                            if (expressions.Count > 1)
                            {
                                expression.LogicalOperator = LogicalOperator.And;
                            }

                            if (filterExpression["comparison"] != null)
                            {
                                var optor = GetOpt(filterExpression["comparison"]);
                                expression.RelationalOperator = optor;
                            }
                            else
                            {
                                expression.RelationalOperator = RelationalOperator.EqualTo;
                            }

                            expression.PropertyName = filterExpression["field"];

                            var values = new Values();
                            expression.Values = values;
                            var value = filterExpression["value"];
                            values.Add(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error deserializing filter: " + ex);
                    _response = _response + " " + ex.Message.ToString();
                }
            }

            // process sorting
            if (sortBy != null && sortBy.Count() > 0 && sortOrder != null && sortOrder.Count() > 0)
            {

                var orderExpressions = new List<OrderExpression>();
                dataFilter.OrderExpressions = orderExpressions;

                var orderExpression = new OrderExpression();
                orderExpressions.Add(orderExpression);

                if (sortBy != null)
                    orderExpression.PropertyName = sortBy;

                var Sortorder = sortOrder.Substring(0, 1).ToUpper() + sortOrder.Substring(1);

                if (Sortorder != null)
                {
                    try
                    {
                        orderExpression.SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), Sortorder);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.ToString());
                        _response = _response + " " + ex.Message.ToString();
                    }
                }
            }

            return dataFilter;
        }

        private void GetDatadictionary(string context, string endpoint, string baseurl)
        {
            try
            {
                if (Session[_key] == null)
                {
                    Session[_key] = Repository.GetDictionary(context, endpoint, baseurl);
                }
                _dataDict = (DataDictionary)Session[_key];
                if (_dataDict.dataObjects.Count == 0)
                    _response = "There is no records in the database for data object \"" + endpoint + "\"";
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting DatabaseDictionary." + ex);
                _response = _response + " " + ex.Message.ToString();
            }
        }


        private DataItems GetDataObjects(string context, string endpoint, string graph, DataFilter dataFilter, int start, int limit, string baseurl)
        {
            DataItems dataItems = null;
            try
            {
                dataItems = Repository.GetDataItems(endpoint, context, graph, dataFilter, start, limit, baseurl);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    _logger.Error("Error deserializing filtered data objects: " + ex);
                if (_response != "success")
                {
                    _response = ex.Message.ToString();
                    if (ex.InnerException.Message != null)
                        _response = _response + " " + ex.InnerException.Message.ToString();
                }
            }

            return dataItems;
        }

        private void SetContextEndpoint(FormCollection form)
        {
            _context = form["context"];
            _endpoint = form["endpoint"];
            _baseUrl = form["baseUrl"];
        }

        private static String ToExtJsType(org.iringtools.library.DataType dataType)
        {
            switch (dataType)
            {
                case org.iringtools.library.DataType.Boolean:
                    return "boolean";

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

    }
}