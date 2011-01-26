package org.iringtools.models;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;

public class ExchangeDataModel extends DataModel
{  
  private static final Logger logger = Logger.getLogger(ExchangeDataModel.class);
  
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String scope, String exchangeId, int start, int limit)
  {
    String dtiUrl = serviceUri + "/" + scope + "/exchanges/" + exchangeId;
    String dtoUrl = dtiUrl;
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.EXCHANGE, pageDtos);
    DataTransferIndices dtis = getDtis(dtiUrl);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());    
    return pageDtoGrid;
  }
  
  public Grid getRelatedItemGrid(String serviceUri, String scope, String exchangeId, String individual, 
      String classId, String classIdentifier, int start, int limit)
  {
    String dtiUrl = serviceUri + "/" + scope + "/exchanges/" + exchangeId;
    String dtoUrl = dtiUrl;
    DataTransferObjects pageDtos = getPageDtos(dtiUrl, dtoUrl, start, limit);  
    return getRelatedItemGrid(DataType.EXCHANGE, pageDtos, individual, classId, classIdentifier, start, limit);
  }
  
  public ExchangeResponse submitExchange(String serviceUri, String scope, String exchangeId, boolean reviewed)
  {
    String dtiUrl = serviceUri + "/" + scope + "/exchanges/" + exchangeId;
    DataTransferIndices dtis = getDtis(dtiUrl);
    
    ExchangeResponse response;    
    String dtiSubmitUrl = dtiUrl + "/submit";
    ExchangeRequest request = new ExchangeRequest();
    request.setDataTransferIndices(dtis);
    request.setReviewed(reviewed);
    
    try
    {
      HttpClient httpClient = new HttpClient(dtiSubmitUrl);
      response = httpClient.post(ExchangeResponse.class, request);
      removeSessionData(dtiUrl);
    }
    catch (HttpClientException ex)
    {
      String error = "Error submitExchange: " + ex;
      logger.error(error);
      
      response = new ExchangeResponse();
      response.setLevel(Level.ERROR);      
      Status status = new Status();      
      List<String> messages = new ArrayList<String>();
      messages.add(error);
      StatusList statusList = new StatusList();
      List<Status> statusItems = statusList.getItems();
      statusItems.add(status);
      response.setStatusList(statusList);
    }
    
    return response;
  }
}
