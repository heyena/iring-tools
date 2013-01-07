package org.iringtools.controllers;

import org.iringtools.models.AppDataModel;
import org.iringtools.widgets.grid.Grid;

public class AppDataController extends AbstractController
{
  private static final long serialVersionUID = 1L;
  private String refDataServiceUri;
  private Grid pageDtoGrid;
  private Grid pageRelatedItemGrid;

  private String baseUri;
  private String scope;
  private String app;
  private String graph;
  private String individual;
  private String classId;
  private String classIdentifier;
  private String filter;
  private String sort; // sort by
  private String dir;  // sort direction
  private int start;
  private int limit;
  
  public AppDataController() 
  {
    super();
    
	  refDataServiceUri = context.getInitParameter("RefDataServiceUri");
	  authorize("exchangeManager", "exchangeAdmins");
  }
  
  //-------------------------------------
  // get a page of data transfer objects 
  // ------------------------------------
  public String getPageDtos()
  {
    try
    {
      AppDataModel appDataModel = new AppDataModel(session, refDataServiceUri, fieldFit, 
          isAsync, asyncTimeout, pollingInterval);
      pageDtoGrid = appDataModel.getDtoGrid(baseUri, scope, app, graph, filter, sort, dir, start, limit);    
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
      AppDataModel appDataModel = new AppDataModel(session, refDataServiceUri, fieldFit, 
          isAsync, asyncTimeout, pollingInterval);
      pageRelatedItemGrid = appDataModel.getRelatedDtoGrid(baseUri, scope, app, graph, individual, 
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
  
  // --------------------------
  // getter and setter methods
  // --------------------------
  public void setBaseUri(String baseUri)
  {
    this.baseUri = baseUri;
  }

  public String getBaseUri()
  {
    return baseUri;
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
}
