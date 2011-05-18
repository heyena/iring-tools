package org.iringtools.controllers;

import java.util.Map;

import org.apache.log4j.Logger;
import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.models.DirectoryModel;
import org.iringtools.utility.HttpClientException;
import org.iringtools.widgets.tree.Tree;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class DirectoryController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private static final Logger logger = Logger.getLogger(DirectoryController.class);

  private Map<String, Object> session;
  private DirectoryModel directoryModel;
  private String exchangeServiceUri;;
  private Tree directoryTree;
  
  @Override
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  } 
  
  public DirectoryController()
  {
    directoryModel = new DirectoryModel();
    exchangeServiceUri = ActionContext.getContext().getApplication().get("ExchangeServiceUri").toString();
  }

  public String getDirectory()
  {
    try
    {
      session.clear();
      directoryTree = directoryModel.getDirectoryTree(exchangeServiceUri + "/directory");  
    }
    catch (HttpClientException ex)
    {
      logger.error("Error in getDirectory: " + ex);
      directoryTree = new Tree();
    } 
    
    return SUCCESS;
  }

  public Tree getDirectoryTree()
  {
    return directoryTree;
  }
}
