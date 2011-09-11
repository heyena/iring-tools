package org.iringtools.controllers;

import java.util.Map.Entry;

import org.iringtools.models.DataModel;
import org.iringtools.models.DirectoryModel;
import org.iringtools.widgets.tree.Tree;

public class DirectoryController extends AbstractController
{
  private static final long serialVersionUID = 1L;
  
  private String exchangeServiceUri;;
  private Tree directoryTree;

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
      prepareErrorResponse(500, e.toString());
      return ERROR;
    }

    return SUCCESS;
  }

  public Tree getDirectoryTree()
  {
    return directoryTree;
  }
}
