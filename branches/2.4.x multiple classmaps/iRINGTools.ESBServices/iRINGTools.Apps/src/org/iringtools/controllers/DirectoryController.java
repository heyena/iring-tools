package org.iringtools.controllers;

import java.util.Iterator;
import java.util.Map;
import java.util.Map.Entry;

import org.iringtools.directory.Directory;
import org.iringtools.models.DataModel;
import org.iringtools.models.DirectoryModel; 
import org.iringtools.utility.IOUtils;
import org.iringtools.widgets.tree.Tree;

public class DirectoryController extends AbstractController
{
  private static final long serialVersionUID = 1L;
  
  private Tree directoryTree;
  private String dtoContext;

  public DirectoryController() throws Exception
  {
    super();
    authorize("exchangeAdmins");
  }

  public String getDirectory() throws Exception
  {
    try
    {      
      Iterator<Entry<String, Object>> iterator = settings.entrySet().iterator();
      
      while (iterator.hasNext()) 
      {
        Entry<String, Object> entry = iterator.next();
        String key = entry.getKey();
        
        if (key.startsWith(DataModel.APP_PREFIX))
        {
          iterator.remove();
        }
      }
  
      DirectoryModel directoryModel = new DirectoryModel(settings);   
      Directory directory = directoryModel.getDirectory();
      
      session.put(DataModel.DIRECTORY_KEY, directory);
      directoryTree = directoryModel.directoryToTree(directory);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      throw new Exception(e.toString());
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
      ? map.get("scope") + "." + map.get("xid")    
      : map.get("scope") + "." + map.get("app") + "." + map.get("graph"); 
    
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
