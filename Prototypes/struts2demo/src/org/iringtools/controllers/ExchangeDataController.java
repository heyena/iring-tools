package org.iringtools.controllers;

import java.util.List;
import java.util.Map;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Status;
import org.iringtools.dxfr.response.ExchangeResponse;
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
  private boolean reviewed;
  private String exchangeResult;
  
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
    pageDtoGrid = exchangeDataModel.getDtoGrid(esbServiceUri, scope, xid, start, limit);    
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
    pageRelatedItemGrid = exchangeDataModel.getRelatedItemGrid(esbServiceUri, scope, xid, 
        individual, classId, classIdentifier, start, limit);
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
    List<Status> statusItems = response.getStatusList().getItems();    
    int duration = response.getEndTimeStamp().compare(response.getStartTimeStamp());    
    StringBuilder result = new StringBuilder();
         
    if (response.getLevel() == Level.SUCCESS)
    {
      result.append("Exchange completed successfully.\r");
    }
    else
    {
      if (response.getLevel() == Level.ERROR)
      {
        result.append("Exchange failed.\r\r");
      }
      else if (response.getLevel() == Level.WARNING)
      {
        result.append("Exchange completed with warning(s).\r\r");
      }
      
      if (statusItems != null && statusItems.size() > 0)
      {
        for (String message : statusItems.get(0).getMessages().getItems())
        {
          result.append(message + "\r");
        }
      }    
      
      result.append("\r");
    }
    
    result.append("Execution time [" + duration + "] second(s).");   
    exchangeResult = result.toString();
    
    return SUCCESS;
  }
  
  public String getExchangeResult()
  {
    return exchangeResult;
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

  public void setReviewed(boolean reviewed)
  {
    this.reviewed = reviewed;
  }

  public boolean getReviewed()
  {
    return reviewed;
  } 
}
