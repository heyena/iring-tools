package org.iringtools.controllers;

import java.util.HashMap;
import java.util.Map;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.models.DirectoryModel;
import org.iringtools.widgets.tree.Tree;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class DirectoryController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private Map<String, Object> session;
  private DirectoryModel directoryModel;
  private Tree directoryTree;
  
  public DirectoryController()
  {
    HashMap<String, String> settings = new HashMap<String, String>();
    settings.put("ESBServiceUri", ActionContext.getContext().getApplication().get("ESBServiceUri").toString());
    directoryModel = new DirectoryModel(settings);
  }

  public String getDirectory() throws Exception
  {
    try
    {
      session.clear();
      directoryTree = directoryModel.createDirectoryTree();
      
      return SUCCESS;
    }
    catch (Exception ex)
    {
      throw ex;
    }
  }

  public Tree getDirectoryTree()
  {
    return directoryTree;
  }
  
  @Override
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  } 
}
