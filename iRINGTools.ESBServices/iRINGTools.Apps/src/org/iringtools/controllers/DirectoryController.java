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
  
  private String exchangeServiceUri;;
  private Tree directoryTree;
  private String dtoContext;

  public DirectoryController()
  {
    super();    
    exchangeServiceUri = context.getInitParameter("ExchangeServiceUri");
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
  
      DirectoryModel directoryModel = new DirectoryModel(session);      
      directoryTree = directoryModel.getDirectoryTree(exchangeServiceUri + "/directory");
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
}
