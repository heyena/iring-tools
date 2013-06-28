package org.iringtools.controllers;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expression;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.LogicalOperator;
import org.iringtools.data.filter.OrderExpression;
import org.iringtools.data.filter.OrderExpressions;
import org.iringtools.data.filter.RelationalOperator;
import org.iringtools.data.filter.SortOrder;
import org.iringtools.data.filter.Values;
import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Graph;
import org.iringtools.directory.Scope;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.models.ExchangeDataModel;
import org.iringtools.widgets.grid.Grid;

public class ExchangeDataController extends BaseController
{
  private static final long serialVersionUID = 1L;
  
	private String xResultsGrid;
	private Grid xHistoryGrid;
    private Grid pageXHistoryGrid;

	private String scope, oldScope, resultScope, commodity;
	private Grid pageDtoGrid;
	private Grid pageRelatedItemGrid;
	private Map<String, String> summaryGrid;
	private String exchangeServiceUri;
	private Exchange form;
	private Scope newScopeAdd;

	private String id, name, description, sourceUri, sourceScopeName, result,
			sourceAppName, sourceGraphName, targetUri, targetScopeName,
			targetAppName, commName, targetGraphName, hasAlgorithm, appName,
			oldAppName, oldConfigName, oldGraphName, oldCommName, appDesc,
			baseUri;
	// private String openGroup, propertyName, relationalOper, value,
	// logicalOper, closeGroup, sortOrder;
	private String openGroup[], propertyName[], propertyNameOE[],
			relationalOper[], value[], logicalOper[], closeGroup[],
			sortOrder[];

	private String individual, expressCount, expressCountOE;

	private Application application;
	private Commodity comDetails;
	private Graph graph;
	// private List<String[]> dfList ;
	// private DataFilter dataFilter;
	private List<List<String>> dfList;
	private String classId;
	private String classIdentifier;
	private String filter;
	private String sort; // sort by
	private String dir; // sort direction
	private int start;
	private int limit;
	private boolean reviewed;
	private String xid;
	private String xlabel;
	private String xtime;
	private int itemCount;
	private HttpServletRequest httpRequest = null;  
  
  public ExchangeDataController() throws Exception
  {   
    super();    
    authorize("exchangeAdmins");
  }
  
  //-------------------------------------
  // get a page of data transfer objects 
  // ------------------------------------
  public String getPageDtos() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);    
      pageDtoGrid = exchangeDataModel.getDtoGrid(exchangeServiceUri, scope, xid, filter, sort, dir, start, limit);  
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    
    return SUCCESS;
  }
  
  //TODO: implement this method
  //-----------------------------
  // get a page of related items
  // ----------------------------
//  public String getPageRelatedItems() throws Exception 
//  {
//    try
//    {
//      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
//      pageRelatedItemGrid = exchangeDataModel.getRelatedDtoGrid(exchangeServiceUri, scope, xid, individual, 
//          classId, classIdentifier, filter, sort, dir, start, limit);  
//    }
//    catch (Exception e)
//    {
//      throw new Exception(e.getMessage());
//    }
//    
//    return SUCCESS;
//  }
  
  //--------------------
  // submit an exchange
  // -------------------
  public String submitExchange() throws Exception 
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);    
      ExchangeResponse response = exchangeDataModel.submitExchange(exchangeServiceUri, scope, xid, reviewed);  
      xResultsGrid = response.getSummary();
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
      
    return SUCCESS;
  }
  
  //----------------------------
  // get all exchange responses 
  // ---------------------------
  public String getXHistory() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);    
      xHistoryGrid = exchangeDataModel.getXHistoryGrid(scope, xid, xlabel);    
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    
    return SUCCESS;
  }
  
  //----------------------------
  // get exchange summary responses 
  // ---------------------------
  public String getSummary() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);    
      summaryGrid = exchangeDataModel.getPreSummaryGrid(exchangeServiceUri, scope, xid);    
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    
    return SUCCESS;
  }
  
  //----------------------------------
  // get a page of exchange responses 
  // ---------------------------------
  public String getPageXHistory() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      pageXHistoryGrid = exchangeDataModel.getPageXHistoryGrid(scope, xid, xtime, start, limit, itemCount); 
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
       
    return SUCCESS;
  }

	// Adding new Scope
	public String newScope() {
		try {
			 ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);

			if ((oldScope.equalsIgnoreCase("null")) || (oldScope.equals(""))) {
				newScopeAdd = new Scope();
				newScopeAdd.setName(scope);
				ApplicationData appData = new ApplicationData();
				DataExchanges exchangeData = new DataExchanges();
				newScopeAdd.setApplicationData(appData);
				newScopeAdd.setDataExchanges(exchangeData);
				exchangeDataModel.newScope(newScopeAdd, exchangeServiceUri);
			} else {
				exchangeDataModel
						.editScope(scope, oldScope, exchangeServiceUri);
			}
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// deleting scope
	public String deleteScope() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			  exchangeDataModel.deleteScope(scope, exchangeServiceUri);	
			  } catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// get Scope
	public String getScopeInfo() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);

			newScopeAdd = exchangeDataModel.getScopeInfo(scope,
					exchangeServiceUri);
			// newScopeAdd = scope;
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// Adding new Application
	public String newApplication() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			Application appNew = new Application();
			appNew.setName(appName);
			appNew.setContext(null);
			appNew.setDescription(appDesc);
			appNew.setBaseUri(baseUri);
			if ((oldAppName == null) || (oldAppName.equals(""))) {
				//Graph graph = new Graph();
				appNew.setGraph(null);
				exchangeDataModel.newApplication(appNew, scope,
						exchangeServiceUri);
			} else {
				exchangeDataModel.editApplication(appNew, oldAppName, oldScope);
			}
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// deleting Application
	public String deleteApplication() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			exchangeDataModel.deleteApplication(appName, scope,
					exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// get Application
	public String getApplicationInfo() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			application = exchangeDataModel.getApplicationInfo(appName, scope,
					exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// Adding new Graph
	public String newGraph() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			Graph graph = new Graph();
			graph.setName(name);
			graph.setDescription(description);
		//	graph.setCommodity(commName);
			if ((oldGraphName == null) || (oldGraphName.equals(""))) {
				exchangeDataModel.newGraph(graph, scope, appName,
						exchangeServiceUri);
			} else {
				exchangeDataModel.editGraph(graph, oldScope, oldAppName,
						oldGraphName, exchangeServiceUri);
			}
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// deleting Graph
	public String deleteGraph() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);

			exchangeDataModel.deleteGraph(name, scope, appName,
					exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String getGraphInfo() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			graph = exchangeDataModel.getGraphInfo(name, scope, appName,
					exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// Adding new Commodity
	public String newCommodity() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			Commodity newCommo = new Commodity();
			//Graph graph = new Graph();
			newCommo.setName(commName);

			if ((oldCommName == null) || (oldCommName.equals(""))) {
				newCommo.setExchange(null);
				exchangeDataModel.newCommodity(newCommo, scope,
						exchangeServiceUri);
			} else {
				exchangeDataModel.editCommodity(newCommo, scope, oldCommName,
						exchangeServiceUri);
			}

		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// deleting Commodity
	public String deleteCommodity() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			exchangeDataModel.deleteCommodity(commName, scope,
					exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String getCommodityInfo() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			comDetails = exchangeDataModel.getCommodityInfo(commName, scope,
					exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	// Adding New Exchange Configuration
	public String newExchangeConfig() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			// ExchangeDefinition
			form = new Exchange();
			// form.setId();
			form.setName(name);
			form.setDescription(description);
			form.setSourceUri(sourceUri);
			form.setSourceScope(sourceScopeName);
			form.setSourceApp(sourceAppName);
			form.setSourceGraph(sourceGraphName);
			form.setTargetUri(targetUri);
			form.setTargetScope(targetScopeName);
			form.setTargetApp(targetAppName);
			form.setTargetGraph(targetGraphName);
			if ((oldConfigName == null) || (oldConfigName.equals(""))) {
				exchangeDataModel.newExchangeConfig(form, scope, commodity,
						exchangeServiceUri);
			} else {
				exchangeDataModel.editExchangeConfig(form, oldScope,
						oldCommName, oldConfigName, exchangeServiceUri);
			}
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String getCommodityConfigInfo() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			form = exchangeDataModel.getCommodityConfigInfo(commName, scope,
					name, exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String deleteExchangeConfig() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			exchangeDataModel.deleteExchangeConfig(commName, scope, name,
					exchangeServiceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String testSourceUri() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			result = exchangeDataModel.testSourceUri(sourceUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}
	
	public String testTargetUri() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			result = exchangeDataModel.testTargetUri(targetUri);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String dataFilterExpressions() {
		try {
			  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
			// DataFilter df = new DataFilter();
			Expressions mexp = new Expressions();
			int count = Integer.parseInt(expressCount.toString());
			for (int i = 1; i < count; i++) {
				Expression ex = new Expression();
				LogicalOperator logvalue = null;
				RelationalOperator reValue = null;
				Values val = new Values();
				List<String> valitems = new ArrayList<String>();
				int valsize = value.length;
				if (i < valsize) {
					valitems.add(value[i]);
				}
				int logsize = logicalOper.length;
				if (i < logsize) {
					if ((logicalOper[i] != null) || (logicalOper[i] != "")) {
						if (logicalOper[i].equalsIgnoreCase("0")) {
							logvalue = LogicalOperator.AND;
						} else if (logicalOper[i].equalsIgnoreCase("1")) {
							logvalue = LogicalOperator.OR;
						}
					}
				}
				int relsize = relationalOper.length;
				if (i < relsize) {
					if ((relationalOper[i] != null)
							|| (relationalOper[i] != "")) {
						if (relationalOper[i].equalsIgnoreCase("0")) {
							reValue = RelationalOperator.EQUAL_TO;
						} else if (relationalOper[i].equalsIgnoreCase("1")) {
							reValue = RelationalOperator.NOT_EQUAL_TO;
						} else if (relationalOper[i].equalsIgnoreCase("2")) {
							reValue = RelationalOperator.GREATER_THAN;
						}
						if (relationalOper[i].equalsIgnoreCase("3")) {
							reValue = RelationalOperator.LESSER_THAN;
						}
					}
				}
				int opensize = openGroup.length;
				int openInt = 0;
				if (i < opensize) {
					if ((openGroup[i].equalsIgnoreCase(""))
							|| (openGroup[i] == null)) {
						openInt = 0;
					} else {
						openInt = Integer.parseInt(openGroup[i].toString());
					}
				}
				int closesize = closeGroup.length;
				int closeInt = 0;
				if (i < closesize) {
					if ((closeGroup[i].equalsIgnoreCase(""))
							|| (closeGroup[i] == null)) {
						closeInt = 0;
					} else {
						closeInt = Integer.parseInt(closeGroup[i].toString());
					}
				}
				int propSize = propertyName.length;
				String propName = null;
				if (i < propSize) {
					propName = propertyName[i];
				}
				
				// if ((propName != null) || (propName.equalsIgnoreCase(""))) { <-- this logic is faulty, presumably meant to avoid missing propName ? 
				if ((propName != null) && (!propName.equalsIgnoreCase(""))) {
					ex.setOpenGroupCount(openInt);
					ex.setCloseGroupCount(closeInt);
					ex.setIsCaseSensitive(false);
					ex.setLogicalOperator(logvalue);
					ex.setPropertyName(propName);
					ex.setRelationalOperator(reValue);
					val.setItems(valitems);
					ex.setValues(val);
					mexp.getItems().add(ex);
				}

			}
			OrderExpressions mOex = new OrderExpressions();
			int oECount = Integer.parseInt(expressCountOE.toString());
			for (int i = 1; i < oECount; i++) {
				OrderExpression oex = new OrderExpression();
				int sortSize = sortOrder.length;
				SortOrder order = null;
				if (i < sortSize) {
					if ((sortOrder[i].equalsIgnoreCase("0"))
							|| (sortOrder[i].equalsIgnoreCase("Asc"))) {
						order = SortOrder.ASC;
					} else if ((sortOrder[i].equalsIgnoreCase("1"))
							|| (sortOrder[i].equalsIgnoreCase("Desc"))) {
						order = SortOrder.DESC;
					}
				}
				int propSize = propertyNameOE.length;
				String propNameOE = null;
				if (i < propSize) {
					propNameOE = propertyNameOE[i];
				}

				oex.setPropertyName(propNameOE);
				oex.setSortOrder(order);
				if(propNameOE  != "")
				{
				mOex.getItems().add(oex);
				}
			}
			System.out.println("---");
			exchangeDataModel.saveDataFilterExpression(mexp, commName, scope,
					name, mOex);
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String getDataFilterEdit() {
		try {
			 ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session); 
			DataFilter dataFilter = exchangeDataModel.getDataFilter(
					commName, scope, name);
			if (dataFilter != null) {
				Expressions ex = dataFilter.getExpressions();
				List<Expression> exs = ex.getItems();
				int size = exs.size();
				dfList = new ArrayList<List<String>>();

				for (int j = 0; j < size; j++) {
					Expression e = exs.get(j);
					List<String> dataF = new ArrayList<String>();
					String rel = null;
					if (e.getRelationalOperator() != null) {
						if (e.getRelationalOperator().equals(
								RelationalOperator.EQUAL_TO)) {
							rel = "0";
						} else if (e.getRelationalOperator().equals(
								RelationalOperator.NOT_EQUAL_TO)) {
							rel = "1";
						} else if (e.getRelationalOperator().equals(
								RelationalOperator.GREATER_THAN)) {
							rel = "2";
						} else if (e.getRelationalOperator().equals(
								RelationalOperator.LESSER_THAN)) {
							rel = "3";
						}
					}
					String log = null;
					if (e.getLogicalOperator() != null) {
						if (e.getLogicalOperator().equals(LogicalOperator.AND)) {
							log = "0";
						} else if (e.getLogicalOperator().equals(
								LogicalOperator.OR)) {
							log = "1";
						}
					}

					String val = null;
					Values vals = e.getValues();
					List<String> valitems = vals.getItems();
					val = valitems.get(0);

					dataF.add(new Integer(e.getOpenGroupCount()).toString());
					dataF.add(log);
					dataF.add(rel);
					dataF.add(e.getPropertyName());
					dataF.add(val);
					dataF.add(new Integer(e.getCloseGroupCount()).toString());
					dfList.add(dataF);
				}
			}
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	public String getDataFilterEditOE()  {
		try {
			 ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session); 
			DataFilter dataFilter = exchangeDataModel.getDataFilter(
					commName, scope, name);
			if (dataFilter != null) {
				OrderExpressions oex = dataFilter.getOrderExpressions();
				List<OrderExpression> oexs = oex.getItems();
				int size = oexs.size();
				dfList = new ArrayList<List<String>>();

				for (int j = 0; j < size; j++) {
					OrderExpression oe = oexs.get(j);
					List<String> dataF = new ArrayList<String>();
					String so = null;
					if (oe.getSortOrder() != null) {
						if (oe.getSortOrder().equals(SortOrder.ASC)) {
							so = "Asc";
						} else
							so = "Desc";
					}

					dataF.add(oe.getPropertyName());
					dataF.add(so);
					dfList.add(dataF);
				}
			}
		} catch (Exception e) {
			 e.printStackTrace();
			return ERROR;
		}

		return SUCCESS;
	}

	/*
	 * public String dataFilterOrderExpression() { try{ ExchangeDataModel
	 * exchangeDataModel = new ExchangeDataModel( session, refDataServiceUri,
	 * fieldFit, isAsync, asyncTimeout, pollingInterval); OrderExpressions mOex
	 * = new OrderExpressions(); int count =
	 * Integer.parseInt(expressCountOE.toString()); for(int i= 1; i < count;
	 * i++) { OrderExpression oex = new OrderExpression(); int sortSize =
	 * sortOrder.length; SortOrder order = null; if( i < sortSize) { if
	 * ((sortOrder[i].equalsIgnoreCase("0"))
	 * ||(sortOrder[i].equalsIgnoreCase("Asc"))) { order = SortOrder.ASC; }else
	 * if((sortOrder[i].equalsIgnoreCase("1"))
	 * ||(sortOrder[i].equalsIgnoreCase("Desc"))) { order = SortOrder.DESC; } }
	 * int propSize = propertyNameOE.length; String propNameOE= null; if(i <
	 * propSize) { propNameOE = propertyNameOE[i]; }
	 * 
	 * oex.setPropertyName(propNameOE); oex.setSortOrder(order);
	 * mOex.getItems().add(oex); }
	 * exchangeDataModel.saveDataFilterExpression(null , commName, scope, name,
	 * mOex); } catch (Exception e) { prepareErrorResponse(500, e.getMessage());
	 * return ERROR; }
	 * 
	 * return SUCCESS; }
	 */
	
	
	// --------------------------
	  // getter and setter methods
	  // --------------------------

	public Exchange getForm() {
		return form;
	}

	public void setForm(Exchange form) {
		this.form = form;
	}

	public HttpServletRequest getHttpRequest() {
		return httpRequest;
	}

	public void setServletRequest(HttpServletRequest request) {
		this.httpRequest = request;
	}

	public String getXResultsGrid() {
		return xResultsGrid;
	}

	public void setScope(String scope) {
		this.scope = scope;
	}

	public String getScope() {
		return scope;
	}

	public void setIndividual(String individual) {
		this.individual = individual;
	}

	public String getIndividual() {
		return individual;
	}

	public void setClassId(String classId) {
		this.classId = classId;
	}

	public String getClassId() {
		return classId;
	}

	public void setClassIdentifier(String classIdentifier) {
		this.classIdentifier = classIdentifier;
	}

	public String getClassIdentifier() {
		return classIdentifier;
	}

	public void setFilter(String filter) {
		this.filter = filter;
	}

	public String getFilter() {
		return filter;
	}

	public void setSort(String sort) {
		this.sort = sort;
	}

	public String getSort() {
		return sort;
	}

	public void setDir(String dir) {
		this.dir = dir;
	}

	public String getDir() {
		return dir;
	}

	public String getId() {
		return id;
	}

	public void setId(String id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public void setStart(int start) {
		this.start = start;
	}

	public int getStart() {
		return start;
	}

	public void setLimit(int limit) {
		this.limit = limit;
	}

	public int getLimit() {
		return limit;
	}

	public void setReviewed(boolean reviewed) {
		this.reviewed = reviewed;
	}

	public boolean getReviewed() {
		return reviewed;
	}

	public void setXid(String xid) {
		this.xid = xid;
	}

	public String getXid() {
		return xid;
	}

	public void setXlabel(String xlabel) {
		this.xlabel = xlabel;
	}

	public String getXlabel() {
		return xlabel;
	}

	public void setXtime(String xtime) {
		this.xtime = xtime;
	}

	public String getXtime() {
		return xtime;
	}

	public void setItemCount(int itemCount) {
		this.itemCount = itemCount;
	}

	public int getItemCount() {
		return itemCount;
	}

	public String getDescription() {
		return description;
	}

	public void setDescription(String description) {
		this.description = description;
	}

	public String getSourceUri() {
		return sourceUri;
	}

	public void setSourceUri(String sourceUri) {
		this.sourceUri = sourceUri;
	}

	public String getSourceScopeName() {
		return sourceScopeName;
	}

	public void setSourceScopeName(String sourceScopeName) {
		this.sourceScopeName = sourceScopeName;
	}

	public String getSourceAppName() {
		return sourceAppName;
	}

	public void setSourceAppName(String sourceAppName) {
		this.sourceAppName = sourceAppName;
	}

	public String getSourceGraphName() {
		return sourceGraphName;
	}

	public void setSourceGraphName(String sourceGraphName) {
		this.sourceGraphName = sourceGraphName;
	}

	public String getTargetUri() {
		return targetUri;
	}

	public void setTargetUri(String targetUri) {
		this.targetUri = targetUri;
	}

	public String getTargetScopeName() {
		return targetScopeName;
	}

	public void setTargetScopeName(String targetScopeName) {
		this.targetScopeName = targetScopeName;
	}

	public String getTargetAppName() {
		return targetAppName;
	}

	public void setTargetAppName(String targetAppName) {
		this.targetAppName = targetAppName;
	}

	public String getTargetGraphName() {
		return targetGraphName;
	}

	public void setTargetGraphName(String targetGraphName) {
		this.targetGraphName = targetGraphName;
	}

	public String getHasAlgorithm() {
		return hasAlgorithm;
	}

	public void setHasAlgorithm(String hasAlgorithm) {
		this.hasAlgorithm = hasAlgorithm;
	}

	public String getCommodity() {
		return commodity;
	}

	public void setCommodity(String commodity) {
		this.commodity = commodity;
	}

	public Scope getNewScopeAdd() {
		return newScopeAdd;
	}

	public void setNewScopeAdd(Scope newScopeAdd) {
		this.newScopeAdd = newScopeAdd;
	}

	public String getAppName() {
		return appName;
	}

	public void setAppName(String appName) {
		this.appName = appName;
	}

	public String getAppDesc() {
		return appDesc;
	}

	public void setAppDesc(String appDesc) {
		this.appDesc = appDesc;
	}

	public String getBaseUri() {
		return baseUri;
	}

	public void setBaseUri(String baseUri) {
		this.baseUri = baseUri;
	}

	public String getCommName() {
		return commName;
	}

	public void setCommName(String commName) {
		this.commName = commName;
	}

	public String getOldScope() {
		return oldScope;
	}

	public void setOldScope(String oldScope) {
		this.oldScope = oldScope;
	}

	public String getResultscope() {
		return resultScope;
	}

	public void setResultscope(String resultScope) {
		this.resultScope = resultScope;
	}

	public Application getApplication() {
		return application;
	}

	public void setApplication(Application application) {
		this.application = application;
	}

	public String getOldAppName() {
		return oldAppName;
	}

	public void setOldAppName(String oldAppName) {
		this.oldAppName = oldAppName;
	}

	public String getOldGraphName() {
		return oldGraphName;
	}

	public void setOldGraphName(String oldGraphName) {
		this.oldGraphName = oldGraphName;
	}

	public Graph getGraph() {
		return graph;
	}

	public void setGraph(Graph graph) {
		this.graph = graph;
	}

	public String getOldCommName() {
		return oldCommName;
	}

	public void setOldCommName(String oldCommName) {
		this.oldCommName = oldCommName;
	}

	public Commodity getComDetails() {
		return comDetails;
	}

	public void setComDetails(Commodity comDetails) {
		this.comDetails = comDetails;
	}

	public String getOldConfigName() {
		return oldConfigName;
	}

	public void setOldConfigName(String oldConfigName) {
		this.oldConfigName = oldConfigName;
	}

	public String getResult() {
		return result;
	}

	public void setResult(String result) {
		this.result = result;
	}
	
	public String[] getOpenGroup() {
		return openGroup;
	}

	public void setOpenGroup(String[] openGroup) {
		this.openGroup = openGroup;
	}

	public String[] getPropertyName() {
		return propertyName;
	}

	public void setPropertyName(String[] propertyName) {
		this.propertyName = propertyName;
	}

	public String[] getRelationalOper() {
		return relationalOper;
	}

	public void setRelationalOper(String[] relationalOper) {
		this.relationalOper = relationalOper;
	}

	public String[] getValue() {
		return value;
	}

	public void setValue(String[] value) {
		this.value = value;
	}

	public String[] getLogicalOper() {
		return logicalOper;
	}

	public void setLogicalOper(String[] logicalOper) {
		this.logicalOper = logicalOper;
	}

	public String[] getCloseGroup() {
		return closeGroup;
	}

	public void setCloseGroup(String[] closeGroup) {
		this.closeGroup = closeGroup;
	}

	public String[] getSortOrder() {
		return sortOrder;
	}

	public void setSortOrder(String[] sortOrder) {
		this.sortOrder = sortOrder;
	}

	public String getExpressCount() {
		return expressCount;
	}

	public void setExpressCount(String expressCount) {
		this.expressCount = expressCount;
	}

	public List<List<String>> getDfList() {
		return dfList;
	}

	public void setDfList(List<List<String>> dfList) {
		this.dfList = dfList;
	}

	public String[] getPropertyNameOE() {
		return propertyNameOE;
	}

	public void setPropertyNameOE(String[] propertyNameOE) {
		this.propertyNameOE = propertyNameOE;
	}

	public String getExpressCountOE() {
		return expressCountOE;
	}

	public void setExpressCountOE(String expressCountOE) {
		this.expressCountOE = expressCountOE;
	}

	public Grid getPageDtoGrid() {
		return pageDtoGrid;
	}

	public Grid getPageRelatedItemGrid() {
		return pageRelatedItemGrid;
	}

	public String getxResultsGrid() {
		return xResultsGrid;
	}

	public Grid getXHistoryGrid() {
		return xHistoryGrid;
	}

	public Grid getPageXHistoryGrid() {
		return pageXHistoryGrid;
	}

	public Map<String, String> getSummaryGrid() {
    return summaryGrid;
  }
}
