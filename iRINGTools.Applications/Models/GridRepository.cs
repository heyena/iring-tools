using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Script.Serialization;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using System.Net;
using org.iringtools.adapter;

namespace iRINGTools.Web.Models
{
    public class GridRepository : AdapterRepository, IGridRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GridRepository));

        private DataDictionary dataDict;
        private DataItems dataItems;
        private DataItems sessionData;
        private StoreViewModel dataGrid;
        private string graph;
        private string response = "";
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;
        private string keyName = "gridSessionData", keyName1 = "sessionData";
        public GridRepository()
        {
            dataGrid = new StoreViewModel();
        }

        public string GetResponse()
        {
            return response;
        }

        public StoreViewModel GetGrid(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
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
                //response = response + " " + ex.Message.ToString();
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, ex, _logger);
                response = response + " " + throwJsonResponse(_CustomError);

            }

            return dataGrid;
        }

        public StoreViewModel GetGrid(string scope, string app, string graph, DataFilter filter, string start, string limit)
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

                GetDataItems(scope, app, graph, filter, start, limit);

                if (response != "")
                    return null;

                GetDataGrid();

                if (response != "")
                    return null;
            }
            catch (Exception ex)
            {
                //response = response + " " + ex.Message.ToString();
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, ex, _logger);
                response = response + " " + throwJsonResponse(_CustomError);

            }

            return dataGrid;
        }

        private void GetDataDictionary(String scope, String app)
        {
            GetDataDictionary(scope, app, false);
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

                    // sort data objects & properties
                    if (dataDict != null && dataDict.dataObjects.Count > 0)
                    {
                        dataDict.dataObjects.Sort(new DataObjectComparer());

                        foreach (DataObject dataObject in dataDict.dataObjects)
                        {
                            dataObject.dataProperties.Sort(new DataPropertyComparer());

                            // Adding Key elements to TOP of the List.
                            List<String> keyPropertyNames = new List<String>();
                            foreach (KeyProperty keyProperty in dataObject.keyProperties)
                            {
                                keyPropertyNames.Add(keyProperty.keyPropertyName);
                            }
                            var value = "";
                            for (int i = 0; i < keyPropertyNames.Count; i++)
                            {
                                value = keyPropertyNames[i];
                                // removing the property name from the list and adding at TOP
                                List<DataProperty> DataProperties = dataObject.dataProperties;
                                DataProperty prop = null;

                                for (int j = 0; j < DataProperties.Count; j++)
                                {
                                    if (DataProperties[j].propertyName == value)
                                    {
                                        prop = DataProperties[j];
                                        DataProperties.RemoveAt(j);
                                        break;

                                    }
                                }

                                if (prop != null)
                                    DataProperties.Insert(0, prop);
                            }
                        }
                    }

                    if (usesCache)
                    {
                        Session[dictKey] = dataDict;
                    }
                }

                if (dataDict == null || dataDict.dataObjects.Count == 0)
                    response = response + "Data dictionary of [" + app + "] is empty.";
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting dictionary." + ex);
                //response = response + " " + ex.Message.ToString();
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, ex, _logger);
                response = response + " " + throwJsonResponse(_CustomError);

            }
        }

        private void GetDataItems(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
        {
            try
            {
                string format = "json";
                if (sort != null)
                {
                    string[] sortArr = sort.Split('\"');
                    sort = sortArr[3];
                    dir = sortArr[7];

                }

                DataFilter dataFilter = CreateDataFilter(filter, sort, dir);
                string relativeUri = "/" + app + "/" + scope + "/" + graph + "/filter?format=" + format + "&start=" + start + "&limit=" + limit;
                string dataItemsJson = string.Empty;

                WebHttpClient client = CreateWebClient(_dataServiceUri);
                string isAsync = _settings["Async"];

                
                DataItems sessionDataItem = null;
                sessionDataItem = (DataItems)Session[keyName];
                bool item = false;


                if (sessionDataItem != null)
                {
                    

                    if (filter == null)
                    {
                        var data = (from d in sessionDataItem.items
                                    select d).Skip(int.Parse(start)).Take(int.Parse(limit)).AsEnumerable().ToList();
                        if (data.Count == 0)
                        {
                            item = true;
                        }
                        else
                        {
                            dataItems = new DataItems();
                            dataItems.items = data;
                            dataItems.limit = data.Count;
                            dataItems.total = sessionDataItem.total;
                        }
                    }
                    else
                    {
                        item = true; 
                    }
                }
                
                if (sessionDataItem == null || item)
                {

                    if (isAsync != null && isAsync.ToLower() == "true")
                    {
                        client.Async = true;
                        string statusUrl = client.Post<DataFilter, string>(relativeUri, dataFilter, format, true);

                        if (string.IsNullOrEmpty(statusUrl))
                        {
                            throw new Exception("Asynchronous status URL not found.");
                        }

                        dataItemsJson = WaitForRequestCompletion<string>(_dataServiceUri, statusUrl);
                    }
                    else
                    {
                        dataItemsJson = client.Post<DataFilter, string>(relativeUri, dataFilter, format, true);
                    }

                    DataItemSerializer serializer = new DataItemSerializer();
                    dataItems = serializer.Deserialize<DataItems>(dataItemsJson, false);

                    
                    mergeSessionData(dataItems);

                    if (Convert.ToInt32(start) == 0)
                    {
                        Session.Add(keyName, dataItems);
                        
                    }
                }



            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data items." + ex);
                //response = response + " " + ex.Message.ToString();
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, ex, _logger);
                response = response + " " + throwJsonResponse(_CustomError);

            }
        }

        private void GetDataItems(string scope, string app, string graph, DataFilter dataFilter, string start, string limit)
        {
            try
            {

                string format = "json";

                string relativeUri = "/" + app + "/" + scope + "/" + graph + "/filter?format=" + format + "&start=" + start + "&limit=" + limit;
                string dataItemsJson = string.Empty;

                WebHttpClient client = CreateWebClient(_dataServiceUri);
                string isAsync = _settings["Async"];

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Post<DataFilter, string>(relativeUri, dataFilter, format, true);

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    dataItemsJson = WaitForRequestCompletion<string>(_dataServiceUri, statusUrl);
                }
                else
                {
                    dataItemsJson = client.Post<DataFilter, string>(relativeUri, dataFilter, format, true);
                }

                DataItemSerializer serializer = new DataItemSerializer();
                dataItems = serializer.Deserialize<DataItems>(dataItemsJson, false);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data items." + ex);
                //response = response + " " + ex.Message.ToString();
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, ex, _logger);
                response = response + " " + throwJsonResponse(_CustomError);
            }
        }

        private void GetDataGrid()
        {

            //List<List<string>> gridData = new List<List<string>>();
            //Array gridData = new Array[dataItems.total];
            Dictionary<string, string>[] gridData = new Dictionary<string, string>[dataItems.limit];

            List<ColumnViewModel> columns = new List<ColumnViewModel>();
            List<FieldViewModel> fields = new List<FieldViewModel>();

            CreateFieldsAndColumn(gridData, columns, fields);

            dataGrid.data = gridData;
            dataGrid.metaData = new MetaDataViewModel();
            dataGrid.metaData.columns = columns;
            dataGrid.metaData.fields = fields;
            dataGrid.total = dataItems.total;
            dataGrid.success = true;
            dataGrid.message = "";


        }

        private void CreateFieldsAndColumn(Array gridData, List<ColumnViewModel> columns, List<FieldViewModel> fields)
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
                        //Field field = new Field();
                        FieldViewModel field = new FieldViewModel();
                        ColumnViewModel column = new ColumnViewModel();

                        string fieldName = dataProp.propertyName;
                        column.dataIndex = fieldName;
                        column.text = fieldName;
                        column.filterable = true;
                        column.sortable = true;
                        field.name = fieldName;
                        field.type = ToExtJsType(dataProp.dataType);

                        //int fieldWidth = fieldName.Count() * 6;

                        //if (fieldWidth > 40)
                        //{
                        //    field.width = fieldWidth + 23;
                        //}
                        //else
                        //{
                        //    field.width = 50;
                        //}


                        //if (dataProp.keyType == KeyType.assigned || dataProp.keyType == KeyType.foreign)
                        //    field.keytype = "key";


                        fields.Add(field);
                        columns.Add(column);
                        //}
                    }
                }
            }

            //int newWid;
            int index = 0;
            foreach (DataItem dataItem in dataItems.items)
            {
                //List<string> rowData = new List<string>();
                Dictionary<string, string> rowData = new Dictionary<string, string>();


                foreach (FieldViewModel field in fields)
                {
                    bool found = false;

                    foreach (KeyValuePair<string, object> property in dataItem.properties)
                    {
                        if (field.name.ToLower() == property.Key.ToLower())
                        {
                            rowData.Add(field.name, property.Value.ToString());

                            //newWid = property.Value.ToString().Count() * 4 + 40;
                            //if (newWid > 40 && newWid > field.width && newWid < 400)
                            //    field.width = newWid;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        rowData.Add(field.name, "");
                    }
                }
                gridData.SetValue(rowData, index++);
            }

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
                case org.iringtools.library.DataType.Date:
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

                        foreach (Dictionary<String, String> expr in filterExpressions)
                        {
                            Expression expression = new Expression();
                            expressions.Add(expression);

                            if (expr.ContainsKey("conjunction")) // new filter
                            {
                                if (expr["conjunction"] != null)
                                {
                                    if (expr["conjunction"].Length == 0)
                                    {
                                        expression.LogicalOperator = LogicalOperator.None;
                                    }
                                    else
                                    {
                                        expression.LogicalOperator = (LogicalOperator)
                                            Enum.Parse(typeof(LogicalOperator), expr["conjunction"]);
                                    }
                                }

                                expression.RelationalOperator = (RelationalOperator)
                                    Enum.Parse(typeof(RelationalOperator), expr["operator"]);
                            }
                            else // legacy ux filter
                            {
                                if (expressions.Count > 1)
                                {
                                    expression.LogicalOperator = LogicalOperator.And;
                                }

                                if (expr.ContainsKey("comparison") && expr["comparison"] != null)
                                {
                                    //expression.RelationalOperator = (RelationalOperator)
                                    //Enum.Parse(typeof(RelationalOperator), expr["comparison"]);
                                    RelationalOperator optor = GetOpt(expr["comparison"]);
                                    expression.RelationalOperator = optor;
                                }
                                else
                                {
                                    expression.RelationalOperator = RelationalOperator.EqualTo;
                                }
                            }

                            expression.PropertyName = expr["field"];

                            Values values = new Values();
                            expression.Values = values;
                            string value = expr["value"];
                            values.Add(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error deserializing filter: " + ex);
                    //response = response + " " + ex.Message.ToString();
                    CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                    _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, ex, _logger);
                    response = response + " " + throwJsonResponse(_CustomError);
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
                        // response = response + " " + ex.Message.ToString();
                        CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                        _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, ex, _logger);
                        response = response + " " + throwJsonResponse(_CustomError);
                    }
                }
            }

            return dataFilter;
        }


        public string throwJsonResponse(CustomError response)
        {
            string jsonMsg;
            return jsonMsg = Utility.SerializeJson(response, true);
        }

        public void mergeSessionData(DataItems dt)
        {
            DataItems dtRow = (DataItems)Session[keyName];
            //if (Session[keyName] != null)
            if (dtRow != null)
            {

                dtRow.items.AddRange(dt.items);
                Session.Add(keyName, dtRow);
                // Session.Add(keyName1, dataItems);
            }
        }

    }



    public class DataObjectComparer : IComparer<DataObject>
    {
        public int Compare(DataObject left, DataObject right)
        {
            // compare strings
            {
                string leftValue = left.objectName.ToString();
                string rightValue = right.objectName.ToString();
                return string.Compare(leftValue, rightValue);
            }
        }
    }

    public class DataPropertyComparer : IComparer<DataProperty>
    {
        public int Compare(DataProperty left, DataProperty right)
        {
            // compare strings
            {
                string leftValue = left.propertyName.ToString();
                string rightValue = right.propertyName.ToString();
                return string.Compare(leftValue, rightValue);
            }
        }
    }
}