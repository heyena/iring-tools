package org.iringtools.controllers;

import org.iringtools.directory.Directory;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.WidgetsUtil;
import org.iringtools.ui.widgets.tree.Tree;

import com.opensymphony.xwork2.ActionSupport;

public class DirectoryTreeController extends ActionSupport
{
  private static final long serialVersionUID = 1L;

  private String directoryURL;
  private Tree directoryTree;

  public String execute() throws Exception
  {
    try
    {
      // Directory directory = NetUtil.get(Directory.class, directoryURL));
      Directory directory = JaxbUtil.read(Directory.class, "C:\\Users\\rpdecarl\\iring-tools\\Prototypes\\struts2demo\\WebContent\\WEB-INF\\data\\directory.xml");
      directoryTree = WidgetsUtil.toTree(directory);
    }
    catch (Exception ex)
    {
      throw ex;
    }

    return SUCCESS;
  }
  
  public void setDirectoryURL(String directoryURL)
  {
    this.directoryURL = directoryURL;
  }

  public String getDirectoryURL()
  {
    return directoryURL;
  }

  public void setDirectoryTree(Tree directoryTree)
  {
    this.directoryTree = directoryTree;
  }

  public Tree getDirectoryTree()
  {
    return directoryTree;
  }
}
