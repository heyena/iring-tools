﻿using System;
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
    public class GridRepository : IGridRepository
    {
      //private NameValueCollection _settings = null;
      private WebHttpClient _dataServiceClient = null;
			private DataDictionary dataDict;
			private DataItems dataItems;
			private Grid dataGrid;
			private string graph;
			private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
			private string response = "";

			[Inject]
			public GridRepository()
      {
        dataGrid = new Grid();

        NameValueCollection settings = ConfigurationManager.AppSettings;
        ServiceSettings _settings = new ServiceSettings();
        _settings.AppendSettings(settings);      

        #region initialize webHttpClient for converting old mapping
        string proxyHost = _settings["ProxyHost"];
        string proxyPort = _settings["ProxyPort"];
        string dataServiceUri = _settings["DataServiceURI"];

        response = "";

        if (dataServiceUri == null)
        {
          response = response + "DataServiceURI is not configured.";
          _logger.Error(response);         
        }

        if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
        {
          WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));

          webProxy.Credentials = _settings.GetProxyCredential();

          _dataServiceClient = new WebHttpClient(dataServiceUri, null, webProxy);
        }
        else
        {
          _dataServiceClient = new WebHttpClient(dataServiceUri);

        }
        #endregion
      }

			public string getResponse()
			{
				return response;
			}

			public Grid getGrid(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
			{
        if (response != "")
          return null;

				this.graph = graph;
				getDatadictionary(scope, app);

				if (response != "")
					return null;

				getDataItems(scope, app, graph, filter, sort, dir, start, limit);

				if (response != "")
					return null;

        getDataGrid();

        if (response != "")
          return null;

				return dataGrid;
			}			

			private void getDatadictionary(String scope, String app)
			{
				try
        {
					dataDict = _dataServiceClient.Get<DataDictionary>("/" + app + "/" + scope + "/dictionary?format=xml", true);

					if (dataDict == null || dataDict.dataObjects.Count == 0)
						response = response + "Data dictionary of [" + app + "] is empty.";
        }
        catch (Exception ex)
        {
					_logger.Error("Error getting dictionary." + ex);
					response = response + " " + ex.Message.ToString();
        }
      }

      private void getDataItems(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
      {
        try
        {
          string format = "json";
          DataFilter dataFilter = createDataFilter(filter, sort, dir);
          string relativeUri = "/" + app + "/" + scope + "/" + graph + "/filter?format=" + format + "&start=" + start + "&limit=" + limit;
          string dataItemsJson = _dataServiceClient.Post<DataFilter, string>(relativeUri, dataFilter, format, true);

          DataItemSerializer serializer = new DataItemSerializer();
          dataItems = serializer.Deserialize<DataItems>(dataItemsJson, false); 
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting data items." + ex);
					response = response + " " + ex.Message.ToString();
        }
      }

			private void getDataGrid()
			{				
				List<List<string>> gridData = new List<List<string>>();
				List<Field> fields = new List<Field>();
				createFields(ref fields, ref gridData);
        dataGrid.total = dataItems.total;
				dataGrid.fields = fields;
				dataGrid.data = gridData;
			}

			private void createFields(ref List<Field> fields, ref List<List<string>> gridData)
			{
				foreach (DataObject dataObj in dataDict.dataObjects)
				{
					if (dataObj.objectName.ToUpper() != graph.ToUpper())
						continue;
					else
					{
						foreach (DataProperty dataProp in dataObj.dataProperties)
						{
              if (!dataProp.isHidden)
              {
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
                fields.Add(field);
              }
						}
					}
				}

				int newWid;
				foreach (DataItem dataItem in dataItems.items)
				{
					List<string> rowData = new List<string>();
					foreach (Field field in fields)
					{
						foreach (KeyValuePair<string, string> property in dataItem.properties)
						{
							if (field.dataIndex.ToLower() == property.Key.ToLower())
							{
								rowData.Add(property.Value);
								newWid = property.Value.Count() * 4 + 40;
								if (newWid > 40 && newWid > field.width && newWid < 300)
									field.width = newWid;
								break;
							}
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

			private RelationalOperator getOpt(string opt)
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

			private DataFilter createDataFilter(string filter, string sortBy, string sortOrder)
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
									RelationalOperator optor = getOpt(filterExpression["comparison"]);
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