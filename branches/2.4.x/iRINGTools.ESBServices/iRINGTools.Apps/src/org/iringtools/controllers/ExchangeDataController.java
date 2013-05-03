package org.iringtools.controllers;

import java.util.Map;

import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.models.ExchangeDataModel;
import org.iringtools.widgets.grid.Grid;

public class ExchangeDataController extends BaseController
{
  private static final long serialVersionUID = 1L;
  
  //TODO remove
  private String exchangeServiceUri;
  
  private Grid pageDtoGrid;
  private Grid pageRelatedItemGrid;
  private String xResultsGrid;
  private Map<String, String> summaryGrid;

  private Grid xHistoryGrid;
  private Grid pageXHistoryGrid;
  
  private String scope;
  private String individual;
  private String classId;
  private String classIdentifier;
  private String filter;
  private String sort; // sort by
  private String dir;  // sort direction
  private int start;
  private int limit;
  private boolean reviewed;
  private String xid;
  private String xlabel;
  private String xtime;
  private int itemCount;  
  
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
      throw new Exception(e.toString());
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
//      throw new Exception(e.toString());
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
      throw new Exception(e.toString());
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
      throw new Exception(e.toString());
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
      throw new Exception(e.toString());
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
      throw new Exception(e.toString());
    }
       
    return SUCCESS;
  }
  
  // --------------------------
  // getter and setter methods
  // --------------------------
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
  
  public Grid getPageDtoGrid()
  {
    return pageDtoGrid;
  }
  
  public Grid getPageRelatedItemGrid()
  {
    return pageRelatedItemGrid;
  }
  
  public String getXResultsGrid()
  {
    return xResultsGrid;
  }
  
  public Map<String, String> getSummaryGrid() {
    return summaryGrid;
  }
  
  public Grid getXHistoryGrid()
  {
    return xHistoryGrid;
  }
  
  public Grid getPageXHistoryGrid()
  {
    return pageXHistoryGrid;
  }
}
