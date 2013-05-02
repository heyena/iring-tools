package org.iringtools.controllers;

import java.io.ByteArrayInputStream;
import java.io.InputStream;

import org.iringtools.dxfr.content.ContentObject;
import org.iringtools.models.AppDataModel;
import org.iringtools.widgets.grid.Grid;

public class AppDataController extends AbstractController
{
  private static final long serialVersionUID = 1L;

  private Grid pageDtoGrid;
  private Grid pageRelatedItemGrid;
  private InputStream contentStream;
  private String contentType;
    
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
  private String target;
  
  public AppDataController() throws Exception
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
      AppDataModel appDataModel = new AppDataModel(settings, session);      
      pageDtoGrid = appDataModel.getDtoGrid(baseUri, scope, app, graph, filter, sort, dir, start, limit);    
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.toString());
    }
    
    return SUCCESS;
  }
  
  public String getContent() throws Exception
  {
    try
    {
      AppDataModel appDataModel = new AppDataModel(settings, session);
      
      ContentObject contenObject = appDataModel.getContent(target);
      contentType = contenObject.getMimeType();
      contentStream = new ByteArrayInputStream(contenObject.getContent());
      return SUCCESS;
    }
    catch (Exception e)
    {
      throw new Exception(e.toString());
    }
  }

  // --------------------------
  // getter and setter methods
  // --------------------------
  
  public Grid getPageDtoGrid()
  {
    return pageDtoGrid;
  }
  
  //TODO: complete implementation
  //-----------------------------
  // get a page of related items
  // ----------------------------
//  public String getPageRelatedItems() throws Exception
//  {
//    try
//    {
//      AppDataModel appDataModel = new AppDataModel(settings, session);
//      pageRelatedItemGrid = appDataModel.getRelatedDtoGrid(baseUri, scope, app, graph, individual, 
//          classId, classIdentifier, filter, sort, dir, start, limit);
//    }
//    catch (Exception e)
//    {
//      throw new Exception(e.toString());
//    }
//    
//    return SUCCESS;
//  }

  public Grid getPageRelatedItemGrid()
  {
    return pageRelatedItemGrid;
  }

  public InputStream getContentStream() {
    return contentStream;
  }
  
  public String getContentType() {
    return contentType;
  }
  
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

  public String getTarget()
  {
    return target;
  }

  public void setTarget(String target)
  {
    this.target = target;
  }
}
