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
import org.iringtools.dxfr.dto.DataTransferObject;
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
    String relativePath = "/" + scope + "/exchanges/" + exchangeId;
    DataTransferObjects pageDtos = getPageDtos(serviceUri, relativePath, relativePath, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.EXCHANGE, pageDtos);
    DataTransferIndices dtis = getDtis(serviceUri, relativePath);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());    
    return pageDtoGrid;
  }

  public Grid getRelatedItemGrid(String serviceUri, String scope, String exchangeId,
      String dtoIdentifier, String classId, String classIdentifier, int start, int limit)
  {
    String relativePath = "/" + scope + "/exchanges/" + exchangeId;
    DataTransferObject dto = getDto(serviceUri, relativePath, relativePath, dtoIdentifier);  
    return getRelatedItemGrid(DataType.EXCHANGE, dto, classId, classIdentifier, start, limit);
  }
  
  public ExchangeResponse submitExchange(String serviceUri, String scope, String exchangeId, boolean reviewed)
  {
    String relativePath = "/" + scope + "/exchanges/" + exchangeId;
    DataTransferIndices dtis = getDtis(serviceUri, relativePath);
    
    ExchangeResponse response;
    ExchangeRequest request = new ExchangeRequest();
    request.setDataTransferIndices(dtis);
    request.setReviewed(reviewed);
    
    try
    {
      HttpClient httpClient = new HttpClient(serviceUri + relativePath);
      response = httpClient.post(ExchangeResponse.class, "/submit", request);
      
      // remove exchange dti cache
      removeSessionData("dti" + relativePath);
      
      // remove exchange logs cache
      removeSessionData("xlogs" + relativePath);
      
      // remove app dti cache
      removeSessionData("dti/" + response.getReceiverScopeName() + "/" + 
          response.getReceiverAppName() + "/" + response.getReceiverGraphName());
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
    String relativePath = "/" + scope + "/exchanges/" + exchangeId;
    String xlogsKey = "xlogs" + relativePath;
    History xlogs;
    
    if (session.containsKey(xlogsKey))
    {
      xlogs = (History)session.get(xlogsKey);
    }
    else
    {
      HttpClient httpClient = new HttpClient(serviceUri);
      
      try 
      {
        xlogs = httpClient.get(History.class, relativePath);
        session.put(xlogsKey, xlogs);
      }
      catch (HttpClientException ex)
      {
        xlogs = new History();
      }
    }
    
    List<ExchangeResponse> xrs = xlogs.getResponses();   
    List<Status> statuses = new ArrayList<Status>();
    List<Integer> responseIndices = new ArrayList<Integer>();
    
    for (ExchangeResponse xr : xrs)
    {
      statuses.addAll(xr.getStatusList().getItems());
      responseIndices.add(statuses.size());
    }
    
    Grid xlogsGrid = new Grid();
    xlogsGrid.setTotal(statuses.size());
    
    List<Field> fields = new ArrayList<Field>();
    xlogsGrid.setFields(fields);
    
    List<List<String>> data = new ArrayList<List<String>>();
    xlogsGrid.setData(data);
    
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
    
    int responseIndex = 0;
    int actualLimit = Math.min(statuses.size(), start + limit);    
    
    // find start response index in the response list
    for (int i = 0; i < responseIndices.size(); i++)
    {
      if (start <= responseIndices.get(i))
      {
        responseIndex = i;
        break;
      }
    }
    
    for (int i = start; i < actualLimit; i++)
    {      
      List<String> row = new ArrayList<String>();
      StringBuilder messages = new StringBuilder();      
      
      // check see if the status has moved in the next response in the response list
      if (responseIndex < responseIndices.size() - 1 && i > responseIndices.get(responseIndex+1))
      {        
        responseIndex++;        
      }
      
      ExchangeResponse xr = xrs.get(responseIndex);        
      GregorianCalendar calendar = xr.getStartTimeStamp().toGregorianCalendar();
      String timestamp = String.format("%1$tY/%1$tm/%1$te", calendar);
      row.add(timestamp);        
      
      Status status = statuses.get(i);
      row.add(status.getIdentifier());
      
      for (String message : status.getMessages().getItems())
      {
        if (messages.length() > 0)
          messages.append(" ");
        
        messages.append(message);
      }
      
      row.add(messages.toString());
      data.add(row);
    }
    
    return xlogsGrid;
  }
}
