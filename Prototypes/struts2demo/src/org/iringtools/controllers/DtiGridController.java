package org.iringtools.controllers;

import java.util.List;

import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.utility.WebClient;

import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class DtiGridController extends ActionSupport 
{
  private static final long serialVersionUID = 1L;
  private String scopeName;
  private String appName;
  private String graphName;  
  private List<DataTransferIndex> dtiList;
  
  public String execute() throws Exception 
  {
    String serviceURL = ActionContext.getContext().getApplication().get("AdapterServiceUri").toString();
    WebClient webClient = new WebClient(serviceURL);
    DataTransferIndices dti = webClient.get(DataTransferIndices.class, "/" + scopeName + "/" + appName + "/" + graphName);
    setDtiList(dti.getDataTransferIndexList().getDataTransferIndexListItems());
    
    return SUCCESS;
  }

  public void setDtiList(List<DataTransferIndex> dtiList)
  {
    this.dtiList = dtiList;
  }

  public List<DataTransferIndex> getDtiList()
  {
    return dtiList;
  }

  public void setScopeName(String scopeName)
  {
    this.scopeName = scopeName;
  }

  public String getScopeName()
  {
    return scopeName;
  }

  public void setAppName(String appName)
  {
    this.appName = appName;
  }

  public String getAppName()
  {
    return appName;
  }

  public void setGraphName(String graphName)
  {
    this.graphName = graphName;
  }

  public String getGraphName()
  {
    return graphName;
  }
}
