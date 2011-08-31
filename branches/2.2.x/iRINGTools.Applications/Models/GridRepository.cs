using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

using iRINGTools.Web.Helpers;
using System.Text;
using org.iringtools.dxfr.manifest;

using org.iringtools.adapter;
using System.Web.Script.Serialization;

namespace iRINGTools.Web.Models
{
    public class GridRepository : IGridRepository
    {
      private NameValueCollection _settings = null;
      private WebHttpClient _client = null;
			private DataDictionary dataDict;
			private DataItems dataItems;
			private Grid dataGrid;
			private string graph;
			private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
			static private HttpSessionStateBase session;
			private JavaScriptSerializer serializer;

			[Inject]
			public GridRepository()
      {
        _settings = ConfigurationManager.AppSettings;
				_client = new WebHttpClient(_settings["DataServiceURI"]);
				serializer = new JavaScriptSerializer();
				dataGrid = new Grid();
      }

			public Grid getGrid(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
			{
				this.graph = graph;
				getDatadictionary(scope, app);
				getDataItems(scope, app, graph, filter, sort, dir, start, limit);
				getDataGrid();								
				return dataGrid;
			}

			public void setSession(HttpSessionStateBase Session) 
			{
				GridRepository.session = Session;
			}

			private void getDatadictionary(String scope, String app)
			{
				try
        {
					dataDict = _client.Get<DataDictionary>("/" + app + "/" + scope + "/dictionary", true);
        }
        catch (Exception ex)
        {
					_logger.Error("Error getting DatabaseDictionary." + ex);
        }
      }

			//[WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
			private void getDataItems(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit)
			{
				string currFilter = filter + "/" + sort + "/" + dir;

				DataFilter dataFilter = createDataFilter(filter, sort, dir);
				try
				{
					string partialKey = "AppGrid-part" + "/" + scope + "/" + app + "/" + graph + "/" + filter + "/" + sort + "/" + dir;
					setGridData(scope, app, graph, start, limit, dataFilter, partialKey);					
				}
				catch (Exception ex) 
				{
					_logger.Error("Error getting DatabaseDictionary." + ex);
				}
			}

			private void setGridData(string scope, string app, string graph, string start, string limit, DataFilter dataFilter, string partialKey)
			{
				DataItems allDataItems = null;
				string key;
				
				if (dataFilter == null)
					key = "AppGrid-full" + "/" + scope + "/" + app + "/" + graph;
				else 
					key = partialKey;
				
				if (session != null)
				{
					if (session[key] == null)
					{
						allDataItems = getDataObjects(key, scope, app, graph, dataFilter);
					}
					else
						allDataItems = (DataItems)session[key];
				}
				else
					getDataObjects(key, scope, app, graph, dataFilter);

				dataGrid.total = (int)allDataItems.total;		
				getPageData(allDataItems, start, limit);
			}

			private DataItems getDataObjects(string key, string scope, string app, string graph, DataFilter dataFilter)
			{
				string allDataItemsJson;
				DataItems allDataItems = null;

				try
				{
					allDataItemsJson = _client.Post<DataFilter, string>("/" + app + "/" + scope + "/" + graph + "/filter?format=json", dataFilter, true);
					allDataItems = (DataItems)serializer.Deserialize(allDataItemsJson, typeof(DataItems));
				}
				catch (Exception ex)
				{
					if (ex.InnerException != null)
						_logger.Error("Error deserializing filtered data objects: " + ex.InnerException);
					_logger.Error("Error deserializing filtered data objects: " + ex);
					if (response == "success")
						response = ex.Message.ToString() + " " + ex.InnerException.Message.ToString();
				}

				if (allDataItems.total > 0)
					session[key] = allDataItems;

				return allDataItems;
			}

			private void getPageData(DataItems allDataItems, string start, string limit) 
			{
				int startNum = int.Parse(start);
				int limitNum = int.Parse(limit);
				DataItems pageData = new DataItems();
				pageData.total = allDataItems.total;
				pageData.type = allDataItems.type;
				pageData.items = new List<DataItem>();
				int indexLimit = Math.Min((int)allDataItems.total, startNum + limitNum);

				for (int i = startNum; i < indexLimit; i++) 				
					pageData.items.Add(allDataItems.items.ElementAt(i));

				dataItems = pageData;
			}

			private void getDataGrid()
			{				
				List<List<string>> gridData = new List<List<string>>();
				List<Field> fields = new List<Field>();
				createFields(ref fields, ref gridData);
				dataGrid.fields = fields;
				dataGrid.data = gridData;
			}

			private void createFields(ref List<Field> fields, ref List<List<string>> gridData)
			{
				string type;
				foreach (DataObject dataObj in dataDict.dataObjects)
				{
					if (dataObj.objectName.ToUpper() != graph.ToUpper())
						continue;
					else
					{
						foreach (DataProperty dataProp in dataObj.dataProperties)
						{
							Field field = new Field();
							string fieldName = dataProp.columnName;
							field.dataIndex = fieldName;
							field.name = fieldName;

							int fieldWidth = fieldName.Count() * 6;

							if (fieldWidth > 40)
							{
								field.width = fieldWidth + 20;
							}
							else
							{
								field.width = 50;
							}

							type = dataProp.dataType.ToString().ToLower();
							if (type == "single")
								type = "auto";
							field.type = type;
							fields.Add(field);
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
						}
					}
				}

				return dataFilter;
			}


		}
}