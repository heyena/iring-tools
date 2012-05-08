package org.iringtools.controllers;

import org.iringtools.models.GridDataModel;
import org.iringtools.widgets.grid.Grid;

public class GridDataController extends AbstractController
{
  private static final long serialVersionUID = 1L;
  private Grid pageDtoGrid;
  private String scope;
  private String app;
  private String graph;
  private String filter;
  private String sort; // sort by
  private String dir; // sort direction
  private String baseUri;
  private int start;
  private int limit;

  public GridDataController()
  {
    super();
  }

  // -------------------------------------
  // get a page of data transfer objects
  // ------------------------------------
  public String getPageDtos()
  {
    GridDataModel gridDataModel = new GridDataModel(session);
    pageDtoGrid = gridDataModel.getDtoGrid(baseUri, scope, app, graph, filter, sort, dir, start, limit);
    return SUCCESS;
  }

  public Grid getPageDtoGrid()
  {
    return pageDtoGrid;
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

  public void setBaseUri(String uri)
  {
    this.baseUri = uri;
  }

  public String getBaseUri()
  {
    return baseUri;
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
