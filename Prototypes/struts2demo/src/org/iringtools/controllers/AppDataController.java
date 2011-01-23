package org.iringtools.controllers;

import java.util.HashMap; 
import java.util.List;
import java.util.Map;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices; 
import org.iringtools.models.AppDataModel;
import org.iringtools.widgets.grid.Grid;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class AppDataController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private Map<String, Object> session;
  private AppDataModel appDataModel;
  private Grid pageDtoGrid;
  private String scope;
  private String app;
  private String graph;
  private int start;
  private int limit;
  
  public AppDataController() 
  {
    HashMap<String, String> settings = new HashMap<String, String>();
    settings.put("ESBServiceUri", ActionContext.getContext().getApplication().get("ESBServiceUri").toString());
    settings.put("DXFRServiceUri", ActionContext.getContext().getApplication().get("DXFRServiceUri").toString());
    appDataModel = new AppDataModel(settings);
  }
  
  public String getPageDto() throws Exception 
  {
    String dtiKey = scope + "-" + app + "-" + graph;    
    String pageDtoKey = dtiKey + "-" + start + "-" + limit;    
    DataTransferIndices dti = null;
    
    if (session.containsKey(dtiKey)) {
      dti = (DataTransferIndices)session.get(dtiKey);
    }
    else {
      dti = appDataModel.getDti(scope, app, graph);
      session.put(dtiKey, dti);
    }
    
    if (session.containsKey(pageDtoKey)) {
      pageDtoGrid = (Grid)session.get(pageDtoKey);
    }
    else {
      List<DataTransferIndex> dtiList = dti.getDataTransferIndexList().getItems();
      int actualLimit = Math.min(start + limit, dtiList.size());
    
      List<DataTransferIndex> pageDti = dti.getDataTransferIndexList().getItems().subList(start, actualLimit);
      pageDtoGrid = appDataModel.getPageDto(scope, app, graph, pageDti); 
      session.put(pageDtoKey, pageDtoGrid);      
    }
    
    pageDtoGrid.setTotal(dti.getDataTransferIndexList().getItems().size());
    
    return SUCCESS;
  }

  public String getRelatedIndividual() throws Exception 
  {
    String dtiKey = scope + "-" + app + "-" + graph;    
    String pageDtoKey = dtiKey + "-" + start + "-" + limit;    
    DataTransferIndices dti = null;
    
    if (session.containsKey(dtiKey)) {
      dti = (DataTransferIndices)session.get(dtiKey);
    }
    else {
      dti = appDataModel.getDti(scope, app, graph);
      session.put(dtiKey, dti);
    }
    
    if (session.containsKey(pageDtoKey)) {
      pageDtoGrid = (Grid)session.get(pageDtoKey);
    }
    else {
      List<DataTransferIndex> dtiList = dti.getDataTransferIndexList().getItems();
      int actualLimit = Math.min(start + limit, dtiList.size());
    
      List<DataTransferIndex> pageDti = dti.getDataTransferIndexList().getItems().subList(start, actualLimit);
      pageDtoGrid = appDataModel.getPageDto(scope, app, graph, pageDti); 
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

  public void setApp(String app)
  {
    this.app = app;
  }

  public String getApp()
  {
    return app;
  }

  public void setGraph(String graph)
  {
    this.graph = graph;
  }

  public String getGraph()
  {
    return graph;
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
