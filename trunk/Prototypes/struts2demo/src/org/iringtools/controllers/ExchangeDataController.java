package org.iringtools.controllers;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.models.ExchangeDataModel;
import org.iringtools.widgets.grid.Grid;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class ExchangeDataController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private Map<String, Object> session;
  private ExchangeDataModel exchangeDataModel;
  private Grid pageDtoGrid;
  private String scope;
  private int xid;
  private int start;
  private int limit;
  
  public ExchangeDataController() 
  {
    HashMap<String, String> settings = new HashMap<String, String>();
    settings.put("ESBServiceUri", ActionContext.getContext().getApplication().get("ESBServiceUri").toString());
    exchangeDataModel = new ExchangeDataModel(settings);
  }
  
  public String getPageDto() throws Exception 
  {
    String dtiKey = scope + "-" + xid;    
    String pageDtoKey = dtiKey + "-" + start + "-" + limit;    
    DataTransferIndices dti = null;
    
    if (session.containsKey(dtiKey)) {
      dti = (DataTransferIndices)session.get(dtiKey);
    }
    else {
      dti = exchangeDataModel.getDti(scope, xid);
      session.put(dtiKey, dti);
    }
    
    if (session.containsKey(pageDtoKey)) {
      pageDtoGrid = (Grid)session.get(pageDtoKey);
    }
    else {
      List<DataTransferIndex> dtiList = dti.getDataTransferIndexList().getItems();
      int actualLimit = Math.min(start + limit, dtiList.size());
      List<DataTransferIndex> pageDti = dti.getDataTransferIndexList().getItems().subList(start, actualLimit);
      
      pageDtoGrid = exchangeDataModel.getPageDto(scope, xid, pageDti); 
      session.put(pageDtoKey, pageDtoGrid);      
    }    
    
    pageDtoGrid.setTotal(dti.getDataTransferIndexList().getItems().size());
    
    return SUCCESS;
  }

  public Grid getPageDtoGrid()
  {
    return pageDtoGrid;
  }

  public void setScope(String scope)
  {
    this.scope = scope;
  }

  public String getScope()
  {
    return scope;
  }

  public void setXid(int xid)
  {
    this.xid = xid;
  }

  public int getXid()
  {
    return xid;
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
  
  @Override
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  } 
}
