package org.iringtools.models;

import java.util.ArrayList;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.Map;

import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.commons.lang.xwork.StringUtils;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Status;
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
  
  public Grid getDtoGrid(String serviceUri, String scope, String exchangeId, String filter, String sortOrder,
      String sortBy, int start, int limit)
  {
    String dtiRelativePath = "/" + scope + "/exchanges/" + exchangeId;
    String dtoRelativePath = dtiRelativePath + "/page";
    DataTransferObjects pageDtos = getPageDtos(serviceUri, dtiRelativePath, dtoRelativePath, filter,
        sortOrder, sortBy, start, limit);
    Grid pageDtoGrid = getDtoGrid(DataType.EXCHANGE, pageDtos);
    DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
    pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());    
    return pageDtoGrid;
  }

  public Grid getRelatedItemGrid(String serviceUri, String scope, String exchangeId, String dtoIdentifier, 
      String classId, String classIdentifier, String filter, String sortOrder, String sortBy, 
      int start, int limit)
  {
    String dtiRelativePath = "/" + scope + "/exchanges/" + exchangeId;
    String dtoRelativePath = dtiRelativePath + "/page";
    DataTransferObject dto = getDto(serviceUri, dtiRelativePath, dtoRelativePath, dtoIdentifier, filter, 
        sortOrder, sortBy, start, limit);  
    return getRelatedItemGrid(DataType.EXCHANGE, dto, classId, classIdentifier, start, limit);
  }
  
  public ExchangeResponse submitExchange(String serviceUri, String scope, String exchangeId, boolean reviewed)
  {
    String relativePath = "/" + scope + "/exchanges/" + exchangeId;
    DataTransferIndices dtis = getDtis(serviceUri, relativePath, null, null, null);
    
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
      String error = "Error in submitExchange: " + ex;
      logger.error(error);
      
      response = new ExchangeResponse();
      response.setLevel(Level.ERROR);
      
      Messages messages = new Messages();
      response.setMessages(messages);
      List<String> messageList = messages.getItems();
      messageList.add(error);
    }
    
    return response;
  }
  
  private String format(GregorianCalendar gcal)
  {
    return String.format("%1$tY/%1$tm/%1$td-%1$tH:%1$tM:%1$tS.%1$tL", gcal);
  }  

  public Grid getXlogsGrid(String serviceUri, String scope, String exchangeId)
  {
    String relativePath = "/" + scope + "/exchanges/" + exchangeId;
    History xlogs = null;
    
    HttpClient httpClient = new HttpClient(serviceUri);
      
    try 
    {
      xlogs = httpClient.get(History.class, relativePath);
    }
    catch (HttpClientException ex)
    {
      logger.error("Error getting exchange logs: " + ex);
    }
    
    Grid xlogsGrid = new Grid();
    
    if (xlogs != null && xlogs.getResponses().size() > 0)
    {
      List<ExchangeResponse> xrs = xlogs.getResponses(); 
      List<Field> fields = new ArrayList<Field>();
      List<List<String>> data = new ArrayList<List<String>>();
      
      Field field = new Field();
      field.setName("&nbsp;");
      field.setDataIndex("&nbsp;");
      field.setType("string");
      field.setWidth(28);
      field.setFixed(true);
      field.setFilterable(false);
      fields.add(field);
      
      field = new Field();
      field.setName("Start Time");
      field.setDataIndex("Start Time");
      field.setType("string");
      fields.add(field);
      
      field = new Field();
      field.setName("End Time");
      field.setDataIndex("End Time");
      field.setType("string");
      fields.add(field);
      
      field = new Field();
      field.setName("Sender");
      field.setDataIndex("Sender");
      field.setType("string");
      fields.add(field);
      
      field = new Field();
      field.setName("Receiver");
      field.setDataIndex("Receiver");
      field.setType("string");
      fields.add(field);
      
      field = new Field();
      field.setName("Result");
      field.setDataIndex("Result");
      field.setType("string");
      fields.add(field);
      
      for (ExchangeResponse xr : xrs)
      { 
        List<String> row = new ArrayList<String>(); 
        String exchangeTime = xr.getEndTimeStamp().toString().replace(":", ".");;
        
        row.add("<input type=\"image\" src=\"resources/images/info-small.png\" "
                + "onClick='javascript:getPageXlogs(\"" + scope + "\",\"" + exchangeId 
                + "\",\"" + exchangeTime + "\")'>");
        
        String startTime = format(xr.getStartTimeStamp().toGregorianCalendar());
        String endTime = format(xr.getEndTimeStamp().toGregorianCalendar());
        
        row.add(startTime);
        row.add(endTime);      
        row.add(xr.getSenderScopeName() + "." + xr.getSenderAppName() + "." + xr.getSenderGraphName());
        row.add(xr.getReceiverScopeName() + "." + xr.getReceiverAppName() + "." + xr.getReceiverGraphName());      
        row.add(StringUtils.join(xr.getMessages().getItems(), "<br/>"));      
        data.add(row);
      }
      
      xlogsGrid.setTotal(xrs.size());
      xlogsGrid.setFields(fields);
      xlogsGrid.setData(data);
    }
    
    return xlogsGrid;
  }
  
  
  
  public Grid getPageXlogsGrid(String xlogsServiceUri, String scope, String xid, String exchangeTime, int start, int limit){

	  String relativePath = "/" + scope + "/exchanges/" + xid + "/" + exchangeTime;
	  String xlogsKey = "xlogs" + relativePath;
	  ExchangeResponse response;
	    
	    if (session.containsKey(xlogsKey))
	    {
	      response = (ExchangeResponse)session.get(xlogsKey);
	    }
	    else
	    {
	      HttpClient httpClient = new HttpClient(xlogsServiceUri);
	      
	      try 
	      {
	    	response = httpClient.get(ExchangeResponse.class, relativePath);
	        session.put(xlogsKey, response);
	      }
	      catch (HttpClientException ex)
	      {
	    	response = new ExchangeResponse();
	      }
	    }
	    
	    
	    List<Status> statuses = response.getStatusList().getItems();	   
	    
	    Grid pageXlogsGrid = new Grid();  
	    pageXlogsGrid.setTotal(statuses.size());
	    
	    List<Field> fields = new ArrayList<Field>();
	    pageXlogsGrid.setFields(fields);
	    
	    List<List<String>> data = new ArrayList<List<String>>();
	    pageXlogsGrid.setData(data);
	    
	    Field field = new Field();
	    field.setName("Identifier");
	    field.setDataIndex("Identifier");
	    field.setType("string");
	    fields.add(field);
	    
	    field = new Field();
	    field.setName("Result");
	    field.setDataIndex("Result");
	    field.setType("string");
	    fields.add(field);  	    
	   
	    int actualLimit = Math.min(statuses.size(), start + limit);    

	    for (int i = start; i < actualLimit; i++)
	    {      
	      List<String> row = new ArrayList<String>();
	      StringBuilder messages = new StringBuilder();      

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
	    
	    return pageXlogsGrid;

  }
  
  
  
  
  
//TODO: this code is obsolete and need to be rewritten to just get a page out of an exchange log
  /* public Grid getPageXLogGrid(String serviceUri, String scope, String exchangeId, int start, int limit)
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
     
     Field field = new Field();
     field.setName("Timestamp");
     field.setDataIndex("Timestamp");
     field.setType("string");
     fields.add(field);
     
     field = new Field();
     field.setName("Identifier");
     field.setDataIndex("Identifier");
     field.setType("string");
     fields.add(field);
     
     field = new Field();
     field.setName("Result");
     field.setDataIndex("Result");
     field.setType("string");
     fields.add(field);
     
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
   }*/
}
