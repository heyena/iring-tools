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
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.models.ExchangeDataModel;
import org.iringtools.models.Result;

import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.tree.Tree;

public class ExchangeDataController extends BaseController
{
  private static final long serialVersionUID = 1L;

  private Tree crossedManifestTree;
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
  private Result result;
  private String id, name, description, sourceUri, sourceScopeName, sourceAppName, sourceGraphName, targetUri,
      targetScopeName, targetAppName, commName, targetGraphName, hasAlgorithm, appName, appScope, oldAppName,
      oldConfigName, oldGraphName, oldCommName, appDesc, baseUri, response, displayName, scopeDisplayName, graphName;

  private String parentClassId, parentClassIndex, parentClassPath, templateId, templateIndex,requestId;
 
  private RequestStatus requestStatus; 
 

// private String openGroup, propertyName, relationalOper, value,
  // logicalOper, closeGroup, sortOrder;
  private String openGroup[], propertyName[], propertyNameOE[], relationalOper[], value[], logicalOper[], closeGroup[],
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
  private List<List<String>> appNames;
  private List<List<String>> adapterScopeList;
  private List<List<String>> columnNames;
  private List<List<String>> relationOperator;
  private List<List<String>> logicalOperator;
  private List<List<String>> sortOrderList;
  private HttpServletRequest httpRequest = null;

  public ExchangeDataController() throws Exception
  {
    super();
    authorize("exchangeAdmins");
  }

  // -------------------------------------
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

  // TODO: implement this method
  // -----------------------------
  // get a page of related items
  // ----------------------------
  // public String getPageRelatedItems() throws Exception
  // {
  // try
  // {
  // ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings,
  // session);
  // pageRelatedItemGrid =
  // exchangeDataModel.getRelatedDtoGrid(exchangeServiceUri, scope, xid,
  // individual,
  // classId, classIdentifier, filter, sort, dir, start, limit);
  // }
  // catch (Exception e)
  // {
  // throw new Exception(e.getMessage());
  // }
  //
  // return SUCCESS;
  // }

  // --------------------
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

  // ----------------------------
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

  // ----------------------------
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

  // ----------------------------------
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

  public String getColumns() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      columnNames = exchangeDataModel.getColumns(exchangeServiceUri, scope, xid, sort);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }

    return SUCCESS;
  }

  public String getRelationalOperator() throws Exception
  {
    try
    {
      if (name.equalsIgnoreCase("Transfer Type"))
      {
        relationOperator = new ArrayList<List<String>>();
        List<String> equal = new ArrayList<String>();
        equal.add("0");
        equal.add("EqualTo");
        relationOperator.add(equal);
        List<String> notEqual = new ArrayList<String>();
        notEqual.add("1");
        notEqual.add("NotEqualTo");
        relationOperator.add(notEqual);

      }
      else
      {
        relationOperator = new ArrayList<List<String>>();
        List<String> equal = new ArrayList<String>();
        equal.add("0");
        equal.add("EqualTo");
        relationOperator.add(equal);
        List<String> notEqual = new ArrayList<String>();
        notEqual.add("1");
        notEqual.add("NotEqualTo");
        relationOperator.add(notEqual);
        List<String> greaterthen = new ArrayList<String>();
        greaterthen.add("2");
        greaterthen.add("GreaterThan");
        relationOperator.add(greaterthen);
        List<String> lessthen = new ArrayList<String>();
        lessthen.add("3");
        lessthen.add("LessThan");
        relationOperator.add(lessthen);
        List<String> contains = new ArrayList<String>();
        contains.add("4");
        contains.add("Contains");
        relationOperator.add(contains);
        List<String> startsWith = new ArrayList<String>();
        startsWith.add("5");
        startsWith.add("StartsWith");
        relationOperator.add(startsWith);
        List<String> endsWith = new ArrayList<String>();
        endsWith.add("6");
        endsWith.add("EndsWith");
        relationOperator.add(endsWith);
        List<String> graterOrEqual = new ArrayList<String>();
        graterOrEqual.add("7");
        graterOrEqual.add("GreaterThanOrEqual");
        relationOperator.add(graterOrEqual);
        List<String> lessOrEqual = new ArrayList<String>();
        lessOrEqual.add("8");
        lessOrEqual.add("LesserThanOrEqual");
        relationOperator.add(lessOrEqual);
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }

    return SUCCESS;
  }

  public String getLogicalOpr() throws Exception
  {
    try
    {
      logicalOperator = new ArrayList<List<String>>();
      List<String> andopr = new ArrayList<String>();
      andopr.add("0");
      andopr.add("AND");
      logicalOperator.add(andopr);
      List<String> orOpr = new ArrayList<String>();
      orOpr.add("1");
      orOpr.add("OR");
      logicalOperator.add(orOpr);

    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }

    return SUCCESS;
  }

  public String getSortOrd() throws Exception
  {
    try
    {
      sortOrderList = new ArrayList<List<String>>();
      List<String> asc = new ArrayList<String>();
      asc.add("0");
      asc.add("Asc");
      sortOrderList.add(asc);
      List<String> desc = new ArrayList<String>();
      desc.add("1");
      desc.add("Desc");
      sortOrderList.add(desc);

    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }

    return SUCCESS;
  }

  public String newScope()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);

      if ((oldScope.equalsIgnoreCase("null")) || (oldScope.equals("")))
      {
        newScopeAdd = new Scope();
        newScopeAdd.setName(scope);
        ApplicationData appData = new ApplicationData();
        DataExchanges exchangeData = new DataExchanges();
        newScopeAdd.setApplicationData(appData);
        newScopeAdd.setDataExchanges(exchangeData);
        ExchangeResponse exres = exchangeDataModel.newScope(newScopeAdd, exchangeServiceUri);
        response = exres.getSummary();
      }
      else
      {
        ExchangeResponse exres = exchangeDataModel.editScope(scope, oldScope, exchangeServiceUri);
        response = exres.getSummary();
      }

    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String deleteScope()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      exchangeDataModel.deleteScope(scope, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getScopeInfo()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      newScopeAdd = exchangeDataModel.getScopeInfo(scope, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String newApplication()
	  {
		    try
		    {
		      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
		      Application newApp = new Application();
		      newApp.setName(appName); 
		      newApp.setDisplayName(displayName);
		      newApp.setContext(appScope);
		     // newApp.setScopeDisplayName(scope);
		      newApp.setDescription(appDesc);
		      newApp.setBaseUri(baseUri);

		      if ((oldAppName == null) || (oldAppName.equals("")))
		      {
		        newApp.setGraph(null);
		        ExchangeResponse exres = exchangeDataModel.newApplication(newApp, scope, exchangeServiceUri);
		        response = exres.getSummary();
		      }
		      else
		      {
		        ExchangeResponse exres = exchangeDataModel.editApplication(newApp, oldAppName, oldScope);
		        response = exres.getSummary();
		      }
		    }
		    catch (Exception e)
		    {
		      e.printStackTrace();
		      return ERROR;
		    }

		    return SUCCESS;
		  }

  public String deleteApplication()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      exchangeDataModel.deleteApplication(appName, scope, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getApplicationInfo()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      application = exchangeDataModel.getApplicationInfo(appName, scope, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String newGraph()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      Graph graph = new Graph();
      graph.setName(name);
      graph.setDescription(description);

      if ((oldGraphName == null) || (oldGraphName.equals("")))
      {
        ExchangeResponse exres = exchangeDataModel.newGraph(graph, scope, appName, exchangeServiceUri);
        response = exres.getSummary();
      }
      else
      {
        ExchangeResponse exres = exchangeDataModel.editGraph(graph, oldScope, oldAppName, oldGraphName,
            exchangeServiceUri);
        response = exres.getSummary();
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String deleteGraph()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      exchangeDataModel.deleteGraph(name, scope, appName, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getGraphInfo()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      graph = exchangeDataModel.getGraphInfo(name, scope, appName, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String newCommodity()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      Commodity newCommo = new Commodity();
      newCommo.setName(commName);

      if ((oldCommName == null) || (oldCommName.equals("")))
      {
        newCommo.setExchange(null);
        ExchangeResponse exres = exchangeDataModel.newCommodity(newCommo, scope, exchangeServiceUri);
        response = exres.getSummary();

      }
      else
      {
        ExchangeResponse exres = exchangeDataModel.editCommodity(newCommo, oldScope, oldCommName, exchangeServiceUri);
        response = exres.getSummary();
      }

    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String deleteCommodity()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      exchangeDataModel.deleteCommodity(commName, scope, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getCommodityInfo()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      comDetails = exchangeDataModel.getCommodityInfo(commName, scope, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  // Adding New Exchange Configuration
  public String newExchangeConfig()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      form = new Exchange();
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

      if ((oldConfigName == null) || (oldConfigName.equals("")))
      {
        exchangeDataModel.newExchangeConfig(form, scope, commodity, exchangeServiceUri);
      }
      else
      {
        exchangeDataModel.editExchangeConfig(form, oldScope, oldCommName, oldConfigName, exchangeServiceUri);
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getCommodityConfigInfo()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      form = exchangeDataModel.getCommodityConfigInfo(commName, scope, name, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String deleteExchangeConfig()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      exchangeDataModel.deleteExchangeConfig(commName, scope, name, exchangeServiceUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }
  
  public String getAMScope()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      boolean checkUrl = sourceUri.contains("/dxfr");
      if(checkUrl)
      {
      int len = sourceUri.length();
      int corlen = len - 5;
       baseUri = sourceUri.substring(0, corlen);
      }else{
    	baseUri = sourceUri;
      }
      adapterScopeList = exchangeDataModel.getInternalScopeNameFromAM(baseUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }
  
  public String getAppNamesList()
  {
    try
    {
    	
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      String baseUri;
      boolean checkUrl = sourceUri.contains("/dxfr");
      if(checkUrl)
      {
      int len = sourceUri.length();
      int corlen = len - 5;
      baseUri = sourceUri.substring(0, corlen);
      }else{
      	baseUri = sourceUri;
        }
      appNames = exchangeDataModel.getAppNames(appScope, baseUri );
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }


  public String testBaseUri()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);

      String baseUri;
      boolean checkUrl = sourceUri.contains("/dxfr");
      if(checkUrl)
      {
      int len = sourceUri.length();
      int corlen = len - 5;
      baseUri = sourceUri.substring(0, corlen);
      }else{
    	baseUri = sourceUri;
      }
      result = exchangeDataModel.testBaseUri(baseUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String testSourceUri()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      result = exchangeDataModel.testUri(sourceUri, graphName);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String testTargetUri()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      result = exchangeDataModel.testUri(targetUri, graphName);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String dataFilterExpressions()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      Expressions mexp = new Expressions();
      int count = Integer.parseInt(expressCount.toString());
      for (int i = 1; i < count; i++)
      {
        Expression ex = new Expression();
        LogicalOperator logvalue = null;
        RelationalOperator reValue = null;
        Values val = new Values();
        List<String> valitems = new ArrayList<String>();
        int valsize = value.length;
        if (i < valsize)
        {
          valitems.add(value[i]);
        }
        int logsize = logicalOper.length;
        if (i < logsize)
        {
          if ((logicalOper[i] != null) || (logicalOper[i] != ""))
          {
            if (logicalOper[i].equalsIgnoreCase("AND"))
            {
              logvalue = LogicalOperator.AND;
            }
            else if (logicalOper[i].equalsIgnoreCase("OR"))
            {
              logvalue = LogicalOperator.OR;
            }
            if (logicalOper[i].equalsIgnoreCase("0"))
            {
              logvalue = LogicalOperator.AND;
            }
            else if (logicalOper[i].equalsIgnoreCase("1"))
            {
              logvalue = LogicalOperator.OR;
            }
          }
        }
        int relsize = relationalOper.length;
        if (i < relsize)
        {
          if ((relationalOper[i] != null) || (relationalOper[i] != ""))
          {
            if (relationalOper[i].equalsIgnoreCase("EqualTo"))
            {
              reValue = RelationalOperator.EQUAL_TO;
            }
            else if (relationalOper[i].equalsIgnoreCase("NotEqualTo"))
            {
              reValue = RelationalOperator.NOT_EQUAL_TO;
            }
            else if (relationalOper[i].equalsIgnoreCase("GreaterThan"))
            {
              reValue = RelationalOperator.GREATER_THAN;
            }
            if (relationalOper[i].equalsIgnoreCase("LessThan"))
            {
              reValue = RelationalOperator.LESSER_THAN;
            }
            if (relationalOper[i].equalsIgnoreCase("0"))
            {
              reValue = RelationalOperator.EQUAL_TO;
            }
            else if (relationalOper[i].equalsIgnoreCase("1"))
            {
              reValue = RelationalOperator.NOT_EQUAL_TO;
            }
            else if (relationalOper[i].equalsIgnoreCase("2"))
            {
              reValue = RelationalOperator.GREATER_THAN;
            }
            if (relationalOper[i].equalsIgnoreCase("3"))
            {
              reValue = RelationalOperator.LESSER_THAN;
            }
            if (relationalOper[i].equalsIgnoreCase("4"))
            {
              reValue = RelationalOperator.CONTAINS;
            }
            if (relationalOper[i].equalsIgnoreCase("5"))
            {
              reValue = RelationalOperator.STARTS_WITH;
            }
            if (relationalOper[i].equalsIgnoreCase("6"))
            {
              reValue = RelationalOperator.ENDS_WITH;
            }
            if (relationalOper[i].equalsIgnoreCase("7"))
            {
              reValue = RelationalOperator.GREATER_THAN_OR_EQUAL;
            }
            if (relationalOper[i].equalsIgnoreCase("8"))
            {
              reValue = RelationalOperator.LESSER_THAN_OR_EQUAL;
            }
          }
        }
        int opensize = openGroup.length;
        int openInt = 0;
        if (i < opensize)
        {
          if ((openGroup[i].equalsIgnoreCase("")) || (openGroup[i] == null))
          {
            openInt = 0;
          }
          else
          {
            openInt = Integer.parseInt(openGroup[i].toString());
          }
        }
        int closesize = closeGroup.length;
        int closeInt = 0;
        if (i < closesize)
        {
          if ((closeGroup[i].equalsIgnoreCase("")) || (closeGroup[i] == null))
          {
            closeInt = 0;
          }
          else
          {
            closeInt = Integer.parseInt(closeGroup[i].toString());
          }
        }
        int propSize = propertyName.length;
        String propName = null;
        if (i < propSize)
        {
          propName = propertyName[i];
        }

        // if ((propName != null) || (propName.equalsIgnoreCase(""))) {
        // <-- this logic is faulty, presumably meant to
        // avoid missing propName ?
        if ((propName != null) && (!propName.equalsIgnoreCase("")))
        {
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
      for (int i = 1; i < oECount; i++)
      {
        OrderExpression oex = new OrderExpression();
        int sortSize = sortOrder.length;
        SortOrder order = null;
        if (i < sortSize)
        {
          if ((sortOrder[i].equalsIgnoreCase("Asc")) || (sortOrder[i].equalsIgnoreCase("Asc")))
          {
            order = SortOrder.ASC;
          }
          else if ((sortOrder[i].equalsIgnoreCase("Desc")) || (sortOrder[i].equalsIgnoreCase("Desc")))
          {
            order = SortOrder.DESC;
          }
          if ((sortOrder[i].equalsIgnoreCase("0")) || (sortOrder[i].equalsIgnoreCase("Asc")))
          {
            order = SortOrder.ASC;
          }
          else if ((sortOrder[i].equalsIgnoreCase("1")) || (sortOrder[i].equalsIgnoreCase("Desc")))
          {
            order = SortOrder.DESC;
          }
        }
        int propSize = propertyNameOE.length;
        String propNameOE = null;
        if (i < propSize)
        {
          propNameOE = propertyNameOE[i];
        }

        oex.setPropertyName(propNameOE);
        oex.setSortOrder(order);
        if (propNameOE != "")
        {
          mOex.getItems().add(oex);
        }
      }

      exchangeDataModel.saveDataFilterExpression(mexp, commName, scope, xid, mOex);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getDataFilterEdit()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      DataFilter dataFilter = exchangeDataModel.getDataFilter(commName, scope, xid);
      if (dataFilter != null)
      {
        Expressions ex = dataFilter.getExpressions();
        if (ex != null)
        {
          List<Expression> exs = ex.getItems();
          int size = exs.size();
          dfList = new ArrayList<List<String>>();

          for (int j = 0; j < size; j++)
          {
            Expression e = exs.get(j);
            List<String> dataF = new ArrayList<String>();
            String rel = null;
            if (e.getRelationalOperator() != null)
            {
              if (e.getRelationalOperator().equals(RelationalOperator.EQUAL_TO))
              {
                rel = "0";
                // rel = "EqualTo";
              }
              else if (e.getRelationalOperator().equals(RelationalOperator.NOT_EQUAL_TO))
              {
                rel = "1";
                // rel = "NotEqualTo";
              }
              else if (e.getRelationalOperator().equals(RelationalOperator.GREATER_THAN))
              {
                rel = "2";
                // rel = "GreaterThan";
              }
              else if (e.getRelationalOperator().equals(RelationalOperator.LESSER_THAN))
              {
                rel = "3";
                // rel = "LessThan";
              }
              if (e.getRelationalOperator().equals(RelationalOperator.CONTAINS))
              {
                rel = "4";
              }
              if (e.getRelationalOperator().equals(RelationalOperator.STARTS_WITH))
              {
                rel = "5";
              }
              if (e.getRelationalOperator().equals(RelationalOperator.ENDS_WITH))
              {
                rel = "6";
              }
              if (e.getRelationalOperator().equals(RelationalOperator.GREATER_THAN_OR_EQUAL))
              {
                rel = "7";
              }
              if (e.getRelationalOperator().equals(RelationalOperator.LESSER_THAN_OR_EQUAL))
              {
                rel = "8";
              }
            }
            String log = null;
            if (e.getLogicalOperator() != null)
            {
              if (e.getLogicalOperator().equals(LogicalOperator.AND))
              {
                log = "0";
                // log="AND";
              }
              else if (e.getLogicalOperator().equals(LogicalOperator.OR))
              {
                log = "1";
                // log = "OR";
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
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getDataFilterEditOE()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      DataFilter dataFilter = exchangeDataModel.getDataFilter(commName, scope, xid);
      if (dataFilter != null)
      {
        OrderExpressions oex = dataFilter.getOrderExpressions();
        if (oex != null)
        {
          List<OrderExpression> oexs = oex.getItems();
          int size = oexs.size();
          dfList = new ArrayList<List<String>>();

          for (int j = 0; j < size; j++)
          {
            OrderExpression oe = oexs.get(j);
            List<String> dataF = new ArrayList<String>();
            String so = null;
            if (oe.getSortOrder() != null)
            {
              if (oe.getSortOrder().equals(SortOrder.ASC))
              {
                so = "Asc";
              }
              else
                so = "Desc";
            }

            dataF.add(oe.getPropertyName());
            dataF.add(so);
            dfList.add(dataF);
          }
        }
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ERROR;
    }

    return SUCCESS;
  }

  public String getCrossedManifest() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      crossedManifestTree = exchangeDataModel.getCrossedManifestTree(scope, xid);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }

    return SUCCESS;
  }

  public String resetCrossedManifest() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      //exchangeDataModel.resetCrossedManifest(scope, xid);
      exchangeDataModel.resetCrossedManifest(scope, xid);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    return SUCCESS;
  }

  public String reloadCrossedManifest() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      //exchangeDataModel.reloadCrossedManifest(scope, xid);
      exchangeDataModel.reloadCrossedManifest(scope, xid);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    return SUCCESS;
  }

  public String saveCrossedManifest() throws Exception
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      //exchangeDataModel.saveCrossedManifest(scope, xid);
      exchangeDataModel.saveExcludedTemplateList(scope, xid);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    return SUCCESS;
  }

  public String deleteTemplateFromManifest() throws Exception
  {
    try
    { // String abc = httpRequest.getParameter(arg0)
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);

      exchangeDataModel.addTemplateInDeletedList(scope, xid, parentClassId, parentClassIndex, parentClassPath, templateId,
          templateIndex);

      // parentClassId,parentClassIndex,parentClassPath,templateId,templateIndex;

    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    return SUCCESS;
  }
  
  public String includeTemplateInManifest() throws Exception
  {
    try
    { 
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
      exchangeDataModel.removeTemplateFromDeletedList(scope, xid, parentClassId, parentClassIndex, parentClassPath, templateId,
          templateIndex);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.getMessage());
    }
    return SUCCESS;
  }

  public String getExchangeRequestStatus() throws Exception
  {
	requestStatus = null;
	try
	{
	  ExchangeDataModel exchangeDataModel = new ExchangeDataModel(settings, session);
	  requestStatus = exchangeDataModel.getExchangeRequestStatus(scope, xid);
	//  requestStatus = exchangeDataModel.getStatusOfRequest(requestId);
      if (requestStatus.getState() == State.NOT_FOUND )
       {
         throw new Exception("request not found");
       }
    }
    catch (Exception ex)
    {
   	  requestStatus = new RequestStatus();
      requestStatus.setState(State.ERROR);
      requestStatus.setMessage(ex.getMessage());
    }
	
	return SUCCESS;
  }
  // --------------------------
  // getter and setter methods
  // --------------------------

  public Tree getCrossedManifestTree()
  {
    return crossedManifestTree;
  }

  public Exchange getForm()
  {
    return form;
  }

  public void setForm(Exchange form)
  {
    this.form = form;
  }

  public HttpServletRequest getHttpRequest()
  {
    return httpRequest;
  }

  public void setServletRequest(HttpServletRequest request)
  {
    this.httpRequest = request;
  }

  public String getXResultsGrid()
  {
    return xResultsGrid;
  }

  public void setScope(String scope)
  {
    this.scope = scope;
  }

  public String getScope()
  {
    return scope;
  }

  public void setIndividual(String individual)
  {
    this.individual = individual;
  }

  public String getIndividual()
  {
    return individual;
  }

  public void setClassId(String classId)
  {
    this.classId = classId;
  }

  public String getClassId()
  {
    return classId;
  }

  public void setClassIdentifier(String classIdentifier)
  {
    this.classIdentifier = classIdentifier;
  }

  public String getClassIdentifier()
  {
    return classIdentifier;
  }

  public void setFilter(String filter)
  {
    this.filter = filter;
  }

  public String getFilter()
  {
    return filter;
  }

  public void setSort(String sort)
  {
    this.sort = sort;
  }

  public String getSort()
  {
    return sort;
  }

  public void setDir(String dir)
  {
    this.dir = dir;
  }

  public String getDir()
  {
    return dir;
  }

  public String getId()
  {
    return id;
  }

  public void setId(String id)
  {
    this.id = id;
  }

  public String getName()
  {
    return name;
  }

  public void setName(String name)
  {
    this.name = name;
  }

  public void setStart(int start)
  {
    this.start = start;
  }

  public int getStart()
  {
    return start;
  }

  public void setLimit(int limit)
  {
    this.limit = limit;
  }

  public int getLimit()
  {
    return limit;
  }

  public void setReviewed(boolean reviewed)
  {
    this.reviewed = reviewed;
  }

  public boolean getReviewed()
  {
    return reviewed;
  }

  public void setXid(String xid)
  {
    this.xid = xid;
  }

  public String getXid()
  {
    return xid;
  }

  public void setXlabel(String xlabel)
  {
    this.xlabel = xlabel;
  }

  public String getXlabel()
  {
    return xlabel;
  }

  public void setXtime(String xtime)
  {
    this.xtime = xtime;
  }

  public String getXtime()
  {
    return xtime;
  }

  public void setItemCount(int itemCount)
  {
    this.itemCount = itemCount;
  }

  public int getItemCount()
  {
    return itemCount;
  }

  public String getDescription()
  {
    return description;
  }

  public void setDescription(String description)
  {
    this.description = description;
  }

  public String getSourceUri()
  {
    return sourceUri;
  }

  public void setSourceUri(String sourceUri)
  {
    this.sourceUri = sourceUri;
  }

  public String getSourceScopeName()
  {
    return sourceScopeName;
  }

  public void setSourceScopeName(String sourceScopeName)
  {
    this.sourceScopeName = sourceScopeName;
  }

  public String getSourceAppName()
  {
    return sourceAppName;
  }

  public void setSourceAppName(String sourceAppName)
  {
    this.sourceAppName = sourceAppName;
  }

  public String getSourceGraphName()
  {
    return sourceGraphName;
  }

  public void setSourceGraphName(String sourceGraphName)
  {
    this.sourceGraphName = sourceGraphName;
  }

  public String getTargetUri()
  {
    return targetUri;
  }

  public void setTargetUri(String targetUri)
  {
    this.targetUri = targetUri;
  }

  public String getTargetScopeName()
  {
    return targetScopeName;
  }

  public void setTargetScopeName(String targetScopeName)
  {
    this.targetScopeName = targetScopeName;
  }

  public String getTargetAppName()
  {
    return targetAppName;
  }

  public void setTargetAppName(String targetAppName)
  {
    this.targetAppName = targetAppName;
  }

  public String getTargetGraphName()
  {
    return targetGraphName;
  }

  public void setTargetGraphName(String targetGraphName)
  {
    this.targetGraphName = targetGraphName;
  }

  public String getHasAlgorithm()
  {
    return hasAlgorithm;
  }

  public void setHasAlgorithm(String hasAlgorithm)
  {
    this.hasAlgorithm = hasAlgorithm;
  }

  public String getCommodity()
  {
    return commodity;
  }

  public void setCommodity(String commodity)
  {
    this.commodity = commodity;
  }

  public Scope getNewScopeAdd()
  {
    return newScopeAdd;
  }

  public void setNewScopeAdd(Scope newScopeAdd)
  {
    this.newScopeAdd = newScopeAdd;
  }

  public String getAppName()
  {
    return appName;
  }

  public void setAppName(String appName)
  {
    this.appName = appName;
  }

  public String getAppScope()
  {
    return appScope;
  }

  public void setAppScope(String appScope)
  {
    this.appScope = appScope;
  }

  public String getAppDesc()
  {
    return appDesc;
  }

  public void setAppDesc(String appDesc)
  {
    this.appDesc = appDesc;
  }

  public String getBaseUri()
  {
    return baseUri;
  }

  public void setBaseUri(String baseUri)
  {
    this.baseUri = baseUri;
  }

  public String getCommName()
  {
    return commName;
  }

  public void setCommName(String commName)
  {
    this.commName = commName;
  }

  public String getOldScope()
  {
    return oldScope;
  }

  public void setOldScope(String oldScope)
  {
    this.oldScope = oldScope;
  }

  public String getResultscope()
  {
    return resultScope;
  }

  public void setResultscope(String resultScope)
  {
    this.resultScope = resultScope;
  }

  public Application getApplication()
  {
    return application;
  }

  public void setApplication(Application application)
  {
    this.application = application;
  }

  public String getOldAppName()
  {
    return oldAppName;
  }

  public void setOldAppName(String oldAppName)
  {
    this.oldAppName = oldAppName;
  }

  public String getOldGraphName()
  {
    return oldGraphName;
  }

  public void setOldGraphName(String oldGraphName)
  {
    this.oldGraphName = oldGraphName;
  }

  public Graph getGraph()
  {
    return graph;
  }

  public void setGraph(Graph graph)
  {
    this.graph = graph;
  }

  public String getOldCommName()
  {
    return oldCommName;
  }

  public void setOldCommName(String oldCommName)
  {
    this.oldCommName = oldCommName;
  }

  public Commodity getComDetails()
  {
    return comDetails;
  }

  public void setComDetails(Commodity comDetails)
  {
    this.comDetails = comDetails;
  }

  public String getOldConfigName()
  {
    return oldConfigName;
  }

  public void setOldConfigName(String oldConfigName)
  {
    this.oldConfigName = oldConfigName;
  }

  public Result getResult()
  {
    return result;
  }

  public void setResult(Result result)
  {
    this.result = result;
  }

  public String[] getOpenGroup()
  {
    return openGroup;
  }

  public void setOpenGroup(String[] openGroup)
  {
    this.openGroup = openGroup;
  }

  public String[] getPropertyName()
  {
    return propertyName;
  }

  public void setPropertyName(String[] propertyName)
  {
    this.propertyName = propertyName;
  }

  public String[] getRelationalOper()
  {
    return relationalOper;
  }

  public void setRelationalOper(String[] relationalOper)
  {
    this.relationalOper = relationalOper;
  }

  public String[] getValue()
  {
    return value;
  }

  public void setValue(String[] value)
  {
    this.value = value;
  }

  public String[] getLogicalOper()
  {
    return logicalOper;
  }

  public void setLogicalOper(String[] logicalOper)
  {
    this.logicalOper = logicalOper;
  }

  public String[] getCloseGroup()
  {
    return closeGroup;
  }

  public void setCloseGroup(String[] closeGroup)
  {
    this.closeGroup = closeGroup;
  }

  public String[] getSortOrder()
  {
    return sortOrder;
  }

  public void setSortOrder(String[] sortOrder)
  {
    this.sortOrder = sortOrder;
  }

  public String getExpressCount()
  {
    return expressCount;
  }

  public void setExpressCount(String expressCount)
  {
    this.expressCount = expressCount;
  }

  public List<List<String>> getDfList()
  {
    return dfList;
  }

  public void setDfList(List<List<String>> dfList)
  {
    this.dfList = dfList;
  }

  public String[] getPropertyNameOE()
  {
    return propertyNameOE;
  }

  public void setPropertyNameOE(String[] propertyNameOE)
  {
    this.propertyNameOE = propertyNameOE;
  }

  public String getExpressCountOE()
  {
    return expressCountOE;
  }

  public void setExpressCountOE(String expressCountOE)
  {
    this.expressCountOE = expressCountOE;
  }

  public Grid getPageDtoGrid()
  {
    return pageDtoGrid;
  }

  public Grid getPageRelatedItemGrid()
  {
    return pageRelatedItemGrid;
  }

  public String getxResultsGrid()
  {
    return xResultsGrid;
  }

  public Grid getXHistoryGrid()
  {
    return xHistoryGrid;
  }

  public Grid getPageXHistoryGrid()
  {
    return pageXHistoryGrid;
  }

  public Map<String, String> getSummaryGrid()
  {
    return summaryGrid;
  }

  public List<List<String>> getColumnNames()
  {
    return columnNames;
  }

  public void setColumnNames(List<List<String>> columnNames)
  {
    this.columnNames = columnNames;
  }

  public List<List<String>> getRelationOperator()
  {
    return relationOperator;
  }

  public void setRelationOperator(List<List<String>> relationOperator)
  {
    this.relationOperator = relationOperator;
  }

  public List<List<String>> getLogicalOperator()
  {
    return logicalOperator;
  }

  public void setLogicalOperator(List<List<String>> logicalOperator)
  {
    this.logicalOperator = logicalOperator;
  }

  public List<List<String>> getSortOrderList()
  {
    return sortOrderList;
  }

  public void setSortOrderList(List<List<String>> sortOrderList)
  {
    this.sortOrderList = sortOrderList;
  }

  public String getResponse()
  {
    return response;
  }

  public void setResponse(String response)
  {
    this.response = response;
  }

  public String getTemplateIndex()
  {
    return templateIndex;
  }

  public void setTemplateIndex(String templateIndex)
  {
    this.templateIndex = templateIndex;
  }

  public String getParentClassPath()
  {
    return parentClassPath;
  }

  public void setParentClassPath(String parentClassPath)
  {
    this.parentClassPath = parentClassPath;
  }

  public String getParentClassIndex()
  {
    return parentClassIndex;
  }

  public void setParentClassIndex(String parentClassIndex)
  {
    this.parentClassIndex = parentClassIndex;
  }

  public String getParentClassId()
  {
    return parentClassId;
  }

  public void setParentClassId(String parentClassId)
  {
    this.parentClassId = parentClassId;
  }

  public String getTemplateId()
  {
    return templateId;
  }

  public void setTemplateId(String templateId)
  {
    this.templateId = templateId;
  }
  
  public void setRequestStatus(RequestStatus requestStatus) 
  {
	this.requestStatus = requestStatus;
  }
  
  public RequestStatus getRequestStatus() 
  {
	return requestStatus;
  }
  
  public String getRequestId() 
  {
	return requestId;
  }

  public void setRequestId(String requestId) 
  {
	this.requestId = requestId;
  }
  public List<List<String>> getAdapterScopeList() {
		return adapterScopeList;

	}


	public void setAdapterScopeList(List<List<String>> adapterScopeList) {
		this.adapterScopeList = adapterScopeList;
	}

	public void setAppNames(List<List<String>> appNames) {
		this.appNames = appNames;
	}

	public List<List<String>> getAppNames() {
		return appNames;
	}

	public String getDisplayName() {
		return displayName;
	}

	public void setDisplayName(String displayName) {
		this.displayName = displayName;
	}

	public String getScopeDisplayName() {
		return scopeDisplayName;
	}

	public void setScopeDisplayName(String scopeDisplayName) {
		this.scopeDisplayName = scopeDisplayName;
	}

	public String getGraphName() {
		return graphName;
	}

	public void setGraphName(String graphName) {
		this.graphName = graphName;
	}
	  
}

