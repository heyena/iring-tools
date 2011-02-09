package org.iringtools.controllers;

import java.util.Map;

import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.commons.lang.xwork.StringUtils;
import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.models.ExchangeDataModel;
import org.iringtools.widgets.grid.Grid;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class ExchangeDataController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private ExchangeDataModel exchangeDataModel;
  private String esbServiceUri;
  private String xlogsServiceUri;
  private Grid pageDtoGrid;
  private Grid pageRelatedItemGrid;
  private Grid xlogsGrid;
  private Grid pageXlogsGrid;
  
  private String scope;
  private String xid;
  private String individual;
  private String classId;
  private String classIdentifier;
  private String filter;
  private String sort; // sort by
  private String dir;  // sort direction
  private int start;
  private int limit;
  private boolean reviewed;
  private String exchangeResult;
  private String xTime;
  private String label;
  
  public ExchangeDataController() 
  {    
    Map<String, Object> appContext = ActionContext.getContext().getApplication();
    
    esbServiceUri = appContext.get("ESBServiceUri").toString();
    xlogsServiceUri = appContext.get("XlogsServiceUri").toString();
    exchangeDataModel = new ExchangeDataModel();
  }
  
  @Override
  public void setSession(Map<String, Object> session)
  {
    exchangeDataModel.setSession(session);
  } 
  
  //-------------------------------------
  // get a page of data transfer objects 
  // ------------------------------------
  public String getPageDtos()
  {
    pageDtoGrid = exchangeDataModel.getDtoGrid(esbServiceUri, scope, xid, filter, dir, sort, start, limit);    
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
    pageRelatedItemGrid = exchangeDataModel.getRelatedItemGrid(esbServiceUri, scope, xid, 
        individual, classId, classIdentifier, filter, dir, sort, start, limit);
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
    ExchangeResponse response = exchangeDataModel.submitExchange(esbServiceUri, scope, xid, reviewed);  
    exchangeResult = StringUtils.join(response.getMessages().getItems(), "\r");    
    return SUCCESS;
  }
  
  public String getExchangeResult()
  {
    return exchangeResult;
  }
  
  //----------------------------------
  // get exchange responses 
  // ---------------------------------
  public String getXlogs()
  {
    xlogsGrid = exchangeDataModel.getXlogsGrid(xlogsServiceUri, scope, xid, label);    
    return SUCCESS;
  }
  
  public Grid getXlogsGrid()
  {
    return xlogsGrid;
  }
  
  //----------------------------------
  // get a page of exchange responses 
  // ---------------------------------
  public String getPageXlogs()
  {
	pageXlogsGrid = exchangeDataModel.getPageXlogsGrid(xlogsServiceUri, scope, xid, xTime, start, limit, label);    
    return SUCCESS;
  }
  
  public Grid getPageXlogsGrid()
  {
    return pageXlogsGrid;
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

  public void setXid(String xid)
  {
    this.xid = xid;
  }

  public String getXid()
  {
    return xid;
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
  
  public void setXTime(String xTime)
  {
    this.xTime = xTime;
  }

  public String getXTime()
  {
    return xTime;
  }
  
  public void setLabel(String label)
  {
    this.label = label;
  }

  public String getLabel()
  {
    return label;
  }
  
}
