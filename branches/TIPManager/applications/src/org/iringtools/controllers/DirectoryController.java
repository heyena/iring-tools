package org.iringtools.controllers;

import java.util.Map;
import java.util.Map.Entry;

import org.iringtools.models.DataModel;
import org.iringtools.models.DirectoryModel; 
import org.iringtools.utility.IOUtils;
import org.iringtools.widgets.tree.Tree;

public class DirectoryController extends AbstractController
{
  private static final long serialVersionUID = 1L;
  
  private Tree directoryTree;
  private String dtoContext;
  private String directoryServiceUri;
  private String type;
  private String baseUri;
  private String contextName;
  private String name;

  public DirectoryController()
  {
    super();    
    directoryServiceUri = context.getInitParameter("DirectoryServiceUri");
    authorize("exchangeManager", "exchangeAdmins");
  }

  public String getDirectory()
  {
    try
    {      
      for (Entry<String, Object> entry : session.entrySet())
      {
        String key = entry.getKey();
  
        if (key.startsWith(DataModel.APP_PREFIX))
        {
          session.remove(key);
        }
      }
  
      DirectoryModel directoryModel = new DirectoryModel(session, directoryServiceUri);      
      directoryTree = directoryModel.getDirectoryTree(directoryServiceUri + "/directory", type, baseUri, contextName, name);
    }
    catch (Exception e)
    {
      prepareErrorResponse(500, e.getMessage());
      return ERROR;
    }

    return SUCCESS;
  }

  public Tree getDirectoryTree()
  {
    return directoryTree;
  }

  public String resetDtoContext()
  {
    Map<String, String> map = IOUtils.splitQueryParams(dtoContext);    
    
    String dtoContextKey = (map.containsKey("xid"))    
      ? map.get("scope") + "/exchanges/" + map.get("xid")    
      : map.get("scope") + "/" + map.get("app") + "/" + map.get("graph"); 
    
    for (String key : session.keySet())
    {
      if (key.contains(dtoContextKey))
        session.remove(key);
    }
            
    return SUCCESS;
  }
  
  public void setDtoContext(String dtoContext)
  {
    this.dtoContext = dtoContext;
  }
  
  public String getType()
  {
    return type;
  }

  public void setType(String type)
  {
    this.type = type;
  }

  public void setBaseUri(String baseUri)
  {
    this.baseUri = baseUri;
  }

  public String getBaseUri()
  {
    return this.baseUri;
  }
  
  public void setName(String name)
  {
    this.name = name;
  }

  public String getName()
  {
    return this.name;
  }
  
  public void setContextName(String contextName)
  {
    this.contextName = contextName;
  }

  public String getContextName()
  {
    return this.contextName;
  }
}
