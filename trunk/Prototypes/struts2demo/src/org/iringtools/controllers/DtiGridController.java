package org.iringtools.controllers;

import java.util.List;

import org.iringtools.adapter.dti.DataTransferIndex;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.utility.WebClient;

import com.opensymphony.xwork2.ActionSupport;

public class DtiGridController extends ActionSupport 
{
  private static final long serialVersionUID = 1L;
  private String serviceURL;
  private List<DataTransferIndex> dtiList;
  
  public String execute() throws Exception 
  {
    WebClient webClient = new WebClient(serviceURL);
    DataTransferIndices dti = webClient.get(DataTransferIndices.class, "");
    setDtiList(dti.getDataTransferIndexList().getDataTransferIndexListItems());
    
    return SUCCESS;
  }

  public void setServiceURL(String serviceURL)
  {
    this.serviceURL = serviceURL;
  }

  public String getServiceURL()
  {
    return serviceURL;
  }

  public void setDtiList(List<DataTransferIndex> dtiList)
  {
    this.dtiList = dtiList;
  }

  public List<DataTransferIndex> getDtiList()
  {
    return dtiList;
  }
}
