package org.iringtools.controllers;

import java.util.List;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.utility.HttpClient;
import com.opensymphony.xwork2.ActionContext;
import com.opensymphony.xwork2.ActionSupport;

public class DtiGridController extends ActionSupport 
{
  private static final long serialVersionUID = 1L;
  private String scopeName;
  private String exchangeId;
  private List<DataTransferIndex> dtiList;
  
  public String execute() throws Exception 
  {
    String serviceUri = ActionContext.getContext().getApplication().get("ExchangeServiceUri").toString();
    HttpClient httpClient = new HttpClient(serviceUri);
    DataTransferIndices dti = httpClient.get(DataTransferIndices.class, "/" + scopeName + "/exchanges/" + exchangeId);
    setDtiList(dti.getDataTransferIndexList().getItems());
    
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

  public void setExchangeId(String exchangeId)
  {
    this.exchangeId = exchangeId;
  }

  public String getExchangeId()
  {
    return exchangeId;
  }
}
