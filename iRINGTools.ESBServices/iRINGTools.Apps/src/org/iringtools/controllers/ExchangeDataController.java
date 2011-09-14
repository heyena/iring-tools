package org.iringtools.controllers;

import org.apache.commons.lang.xwork.StringUtils;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.models.DataModel.FieldFit;
import org.iringtools.models.ExchangeDataModel;
import org.iringtools.utility.IOUtils;
import org.iringtools.widgets.grid.Grid;

public class ExchangeDataController extends AbstractController
{
  private static final long serialVersionUID = 1L;
  
  private String refDataServiceUri;
  private String exchangeServiceUri;
  private String historyServiceUri;
  private FieldFit fieldFit;
  private Grid pageDtoGrid;
  private Grid pageRelatedItemGrid;
  
  private Grid xLogsGrid;
  private String xResultsGrid;
  private Grid pageXLogsGrid;
  
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
  
  public ExchangeDataController() 
  {    
    refDataServiceUri = context.getInitParameter("RefDataServiceUri");   
    
    String fieldFitSetting = context.getInitParameter("FieldFit");    
    fieldFit = IOUtils.isNullOrEmpty(fieldFitSetting) 
      ? FieldFit.VALUE : FieldFit.valueOf(fieldFitSetting.toUpperCase());
    
    exchangeServiceUri = context.getInitParameter("ExchangeServiceUri");
    historyServiceUri = context.getInitParameter("HistoryServiceUri"); 
    
    authorize("exchangeManager", "exchangeAdmins");
  }
  
  //-------------------------------------
  // get a page of data transfer objects 
  // ------------------------------------
  public String getPageDtos()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(session, refDataServiceUri, fieldFit);    
      pageDtoGrid = exchangeDataModel.getDtoGrid(exchangeServiceUri, scope, xid, filter, sort, dir, start, limit);  
    }
    catch (Exception e)
    {
      prepareErrorResponse(500, e.getMessage());
      return ERROR;
    }
    
    return SUCCESS;
  }
  
  public Grid getPageDtoGrid()
  {
    return pageDtoGrid;
  }
  
  //-----------------------------
  // get a page of related items
  // ----------------------------
  public String getPageRelatedItems() 
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(session, refDataServiceUri, fieldFit);    
      pageRelatedItemGrid = exchangeDataModel.getRelatedDtoGrid(exchangeServiceUri, scope, xid, individual, 
          classId, classIdentifier, filter, sort, dir, start, limit);  
    }
    catch (Exception e)
    {
      prepareErrorResponse(500, e.getMessage());
      return ERROR;
    }
    
    return SUCCESS;
  }

  public Grid getPageRelatedItemGrid()
  {
    return pageRelatedItemGrid;
  }
  
  //--------------------
  // submit an exchange
  // -------------------
  public String submitExchange() 
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(session, refDataServiceUri, fieldFit);    
      ExchangeResponse response = exchangeDataModel.submitExchange(exchangeServiceUri, scope, xid, reviewed);  
      xResultsGrid = StringUtils.join(response.getMessages().getItems(), "\r");  
    }
    catch (Exception e)
    {
      prepareErrorResponse(500, e.getMessage());
      return ERROR;
    }
      
    return SUCCESS;
  }
  
  public String getXResultsGrid()
  {
    return xResultsGrid;
  }
  
  //----------------------------
  // get all exchange responses 
  // ---------------------------
  public String getXLogs()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(session, refDataServiceUri, fieldFit);    
      xLogsGrid = exchangeDataModel.getXlogsGrid(historyServiceUri, scope, xid, xlabel);    
    }
    catch (Exception e)
    {
      prepareErrorResponse(500, e.getMessage());
      return ERROR;
    }
    
    return SUCCESS;
  }
  
  public Grid getXLogsGrid()
  {
    return xLogsGrid;
  }
  
  //----------------------------------
  // get a page of exchange responses 
  // ---------------------------------
  public String getPageXLogs()
  {
    try
    {
      ExchangeDataModel exchangeDataModel = new ExchangeDataModel(session, refDataServiceUri, fieldFit);    
      pageXLogsGrid = exchangeDataModel.getPageXlogsGrid(historyServiceUri, scope, xid, xlabel, xtime, start, limit); 
    }
    catch (Exception e)
    {
      prepareErrorResponse(500, e.getMessage());
      return ERROR;
    }
       
    return SUCCESS;
  }
  
  public Grid getPageXLogsGrid()
  {
    return pageXLogsGrid;
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
}
