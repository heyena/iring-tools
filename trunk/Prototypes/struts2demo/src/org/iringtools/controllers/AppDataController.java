package org.iringtools.controllers;

import java.util.Map;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.models.AppDataModel;
import org.iringtools.widgets.grid.Grid;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class AppDataController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private AppDataModel appDataModel;
  private String dxfrServiceUri;
  private Grid pageDtoGrid;
  private Grid pageRelatedItemGrid;
  
  private String scope;
  private String app;
  private String graph;
  private String individual;
  private String classId;
  private String classIdentifier;
  private int start;
  private int limit;
  
  public AppDataController() 
  {
    dxfrServiceUri = ActionContext.getContext().getApplication().get("DXFRServiceUri").toString();    
    appDataModel = new AppDataModel();
  }
  
  @Override
  public void setSession(Map<String, Object> session)
  {
    appDataModel.setSession(session);
  } 
  
  //-------------------------------------
  // get a page of data transfer objects 
  // ------------------------------------
  public String getPageDtos()
  {
    pageDtoGrid = appDataModel.getDtoGrid(dxfrServiceUri, scope, app, graph, start, limit);    
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
    pageRelatedItemGrid = appDataModel.getRelatedItemGrid(dxfrServiceUri, scope, app, graph, 
        individual, classId, classIdentifier, start, limit);
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
