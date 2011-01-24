package org.iringtools.controllers;

import java.util.Map;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.models.ExchangeDataModel;
import org.iringtools.utility.HttpClientException;
import org.iringtools.widgets.grid.Grid;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class ExchangeDataController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private ExchangeDataModel exchangeDataModel;
  private String esbServiceUri;
  private Grid pageDtoGrid;
  private Grid pageRelatedItemGrid;
  
  private String scope;
  private String xid;
  private String individual;
  private String classId;
  private String classIdentifier;
  private int start;
  private int limit;
  
  public ExchangeDataController() 
  {    
    esbServiceUri = ActionContext.getContext().getApplication().get("ESBServiceUri").toString();
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
  public String getPageDtos() throws HttpClientException
  {
    String dtiUrl = esbServiceUri + "/" + scope + "/exchanges/" + xid;
    String dtoUrl = dtiUrl;    
    pageDtoGrid = exchangeDataModel.getDtoGrid(dtiUrl, dtoUrl, start, limit);    
    return SUCCESS;
  }
  
  public Grid getPageDtoGrid()
  {
    return pageDtoGrid;
  }
  
  //-----------------------------
  // get a page of related items
  // ----------------------------
  public String getPageRelatedItems() throws HttpClientException 
  {
    String dtiUrl = esbServiceUri + "/" + scope + "/exchanges/" + xid;
    String dtoUrl = dtiUrl;    
    pageRelatedItemGrid = exchangeDataModel.getRelatedItemGrid(dtiUrl, dtoUrl, individual, classId, classIdentifier, start, limit);
    return SUCCESS;
  }

  public Grid getPageRelatedItemGrid()
  {
    return pageRelatedItemGrid;
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
}
