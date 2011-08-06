package org.iringtools.controllers;

import java.util.Map;
import java.util.Map.Entry;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.models.DataModel;
import org.iringtools.models.DirectoryModel;
import org.iringtools.utility.HttpClientException;
import org.iringtools.widgets.tree.Tree;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class DirectoryController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;

  private Map<String, Object> session;
  private DirectoryModel directoryModel;
  private String exchangeServiceUri;;
  private Tree directoryTree;

  public DirectoryController()
  {
    directoryModel = new DirectoryModel();
    exchangeServiceUri = ActionContext.getContext().getApplication().get("ExchangeServiceUri").toString();
  }

  @Override
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
    directoryModel.setSession(session);
  }

  public String getDirectory() throws HttpClientException
  {
    for (Entry<String, Object> entry : session.entrySet())
    {
      String key = entry.getKey();

      if (key.startsWith(DataModel.APP_PREFIX))
      {
        session.remove(key);
      }
    }

    directoryTree = directoryModel.getDirectoryTree(exchangeServiceUri + "/directory");

    return SUCCESS;
  }

  public Tree getDirectoryTree()
  {
    return directoryTree;
  }
}
