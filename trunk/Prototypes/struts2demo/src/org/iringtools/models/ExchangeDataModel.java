package org.iringtools.models;

import java.util.ArrayList;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;

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
  
  public Grid getXlogsGrid(String serviceUri, String scope, String exchangeId, int start, int limit)
  {
    String xlogsUrl = serviceUri + "/" + scope + "/exchanges/" + exchangeId;
    History xlogs;
    
    if (session.containsKey(xlogsUrl))
    {
      xlogs = (History)session.get(xlogsUrl);
    }
    else
    {
      HttpClient httpClient = new HttpClient(xlogsUrl);
      
      try 
      {
        xlogs = httpClient.get(History.class);
        session.put(xlogsUrl, xlogs);
      }
      catch (HttpClientException ex)
      {
        xlogs = new History();
      }
    }
    
    List<ExchangeResponse> xrs = xlogs.getResponses();
    int actualLimit = Math.min(xrs.size(), start + limit);
    
    Grid xlogsGrid = new Grid();
    List<Field> fields = new ArrayList<Field>();
    
    Field dateField = new Field();
    dateField.setName("Timestamp");
    dateField.setType("string");
    fields.add(dateField);
    
    Field identifierField = new Field();
    identifierField.setName("Identifier");
    identifierField.setType("string");
    fields.add(identifierField);
    
    Field resultField = new Field();
    resultField.setName("Result");
    resultField.setType("string");
    fields.add(resultField);
    
    for (int i = start; i < actualLimit; i++)
    {
      List<String> row = new ArrayList<String>();
      ExchangeResponse xr = xrs.get(i);
      
      GregorianCalendar calendar = xr.getStartTimeStamp().toGregorianCalendar();
      String timestamp = String.format("%1$tY/%1$tm/%1$te", calendar);
      row.add(timestamp);
      
      for (Status status : xr.getStatusList().getItems())
      {
        row.add(status.getIdentifier());
        StringBuilder messages = new StringBuilder();
        
        for (String message : status.getMessages().getItems())
        {
          if (messages.length() > 0)
            messages.append(" ");
          
          messages.append(message);
        }
        
        row.add(messages.toString());;
      }
    }
      
    xlogsGrid.setTotal(xlogs.getResponses().size());
    
    return xlogsGrid;
  }
}
