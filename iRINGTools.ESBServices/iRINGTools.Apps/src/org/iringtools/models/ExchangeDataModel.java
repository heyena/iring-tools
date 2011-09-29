package org.iringtools.models;

import java.util.ArrayList;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.xwork.StringUtils;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Status;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;

public class ExchangeDataModel extends DataModel
{
  private static final Logger logger = Logger.getLogger(ExchangeDataModel.class);
 
  public ExchangeDataModel(Map<String, Object> session, String refServiceUri, FieldFit fieldFit)
  {
    super(DataMode.EXCHANGE, refServiceUri, fieldFit); 
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String scope, String xid, String filter, String sortBy, String sortOrder, 
      int start, int limit) throws DataModelException
  {
    String dtiRelativePath = "/" + scope + "/exchanges/" + xid;
    String dtoRelativePath = dtiRelativePath + "/page";
    String manifestRelativePath = dtiRelativePath + "/manifest";
    
    Grid pageDtoGrid = null;
    Manifest manifest = getManifest(serviceUri, manifestRelativePath);
    
    if (manifest != null && manifest.getGraphs() != null)
    {
      Graph graph = manifest.getGraphs().getItems().get(0);
      
      if (graph != null)
      {
        DataTransferObjects pageDtos = getPageDtos(serviceUri, manifestRelativePath, dtiRelativePath, 
            dtoRelativePath, filter, sortBy, sortOrder, start, limit);
        
        pageDtoGrid = getDtoGrid(manifest, graph, pageDtos);
        DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
        pageDtoGrid.setTotal(dtis.getDataTransferIndexList().getItems().size());
      }      
    }
    
    return pageDtoGrid;
  }

  public Grid getRelatedDtoGrid(String serviceUri, String scope, String xid, String dtoIdentifier, String classId, 
      String classIdentifier, String filter, String sortBy, String sortOrder, int start, int limit) throws DataModelException
  {
    String dtiRelativePath = "/" + scope + "/exchanges/" + xid;
    String dtoRelativePath = dtiRelativePath + "/page";
    String manifestRelativePath = dtiRelativePath + "/manifest";
    
    Grid pageDtoGrid = null;
    Manifest manifest = getManifest(serviceUri, manifestRelativePath);    
    Graph graph = manifest.getGraphs().getItems().get(0);
    
    if (graph != null)
    {
      DataTransferObjects dtos = getRelatedItems(serviceUri, manifestRelativePath, dtiRelativePath, dtoRelativePath, 
            dtoIdentifier, filter, sortBy, sortOrder, start, limit);
        
      pageDtoGrid = getRelatedItemGrid(manifest, graph, dtos, classId, classIdentifier);
    }
    
    return pageDtoGrid;
  }

  public ExchangeResponse submitExchange(String serviceUri, String scope, String xid, boolean reviewed) 
      throws DataModelException
  {
    String exchangeRelativePath = "/" + scope + "/exchanges/" + xid;
    String manifestRelativePath = exchangeRelativePath + "/manifest";
    
    Manifest manifest = getManifest(serviceUri, manifestRelativePath);
    DataTransferIndices dtis = getCachedDtis(exchangeRelativePath);    

    ExchangeResponse response;
    ExchangeRequest request = new ExchangeRequest();
    request.setManifest(manifest);
    request.setDataTransferIndices(dtis);
    request.setReviewed(reviewed);

    try
    {
      HttpClient httpClient = new HttpClient(serviceUri + exchangeRelativePath);
      HttpUtils.addOAuthHeaders(session, httpClient);
      
      response = httpClient.post(ExchangeResponse.class, "/submit", request);

      if (response.getLevel() == Level.SUCCESS)
      {
        // remove cache data related to this exchange including the app data
        String appRelativePath = response.getReceiverScopeName() + "/" + response.getReceiverAppName() + "/"
          + response.getReceiverGraphName();
        
        for (String key : session.keySet())
        {
          if (key.contains(exchangeRelativePath) || key.contains(appRelativePath))
            removeSessionData(key);
        }
      }
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

  public Grid getXlogsGrid(String serviceUri, String scope, String xid, String xlabel) throws DataModelException
  {
    String relativePath = "/" + scope + "/exchanges/" + xid;
    History xlogs = null;

    HttpClient httpClient = new HttpClient(serviceUri);
    HttpUtils.addOAuthHeaders(session, httpClient);
    
    try
    {
      xlogs = httpClient.get(History.class, relativePath);
    }
    catch (HttpClientException ex)
    {
      logger.error("Error getting exchange logs: " + ex);
      throw new DataModelException(ex.getMessage());
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
        String exchangeTime = xr.getEndTimeStamp().toString().replace(":", ".");
        String startTime = format(xr.getStartTimeStamp().toGregorianCalendar());
        String endTime = format(xr.getEndTimeStamp().toGregorianCalendar());
        
        row.add("<input type=\"image\" src=\"resources/images/info-small.png\" "
            + "onClick='javascript:createPageXlogs(\"" + scope + "\",\"" + xid + "\",\"" + xlabel 
            + "\",\"" + startTime + "\",\"" + exchangeTime + "\")'>");

        row.add(startTime);
        row.add(endTime);
        row.add(xr.getSenderScopeName() + "." + xr.getSenderAppName() + "." + xr.getSenderGraphName());
        row.add(xr.getReceiverScopeName() + "." + xr.getReceiverAppName() + "." + xr.getReceiverGraphName());
        
        if (xr.getMessages() != null)
          row.add(StringUtils.join(xr.getMessages().getItems(), "<br/>"));
        else
          row.add("<br/>");
          
        data.add(row);
      }

      xlogsGrid.setTotal(xrs.size());
      xlogsGrid.setFields(fields);
      xlogsGrid.setData(data);
    }

    return xlogsGrid;
  }

  public Grid getPageXlogsGrid(String xlogsServiceUri, String scope, String xid, String xlabel, String xtime, 
      int start, int limit)
  {
    String relativePath = "/" + scope + "/exchanges/" + xid + "/" + xtime;
    String xlogsKey = XLOGS_PREFIX + relativePath;
    ExchangeResponse response;

    if (session.containsKey(xlogsKey))
    {
      response = (ExchangeResponse) session.get(xlogsKey);
    }
    else
    {
      HttpClient httpClient = new HttpClient(xlogsServiceUri);
      HttpUtils.addOAuthHeaders(session, httpClient);
      
      try
      {
        if (session.containsKey(xlogsKey))
        {
          response = (ExchangeResponse) session.get(xlogsKey);
        }
        else
        {
          response = httpClient.get(ExchangeResponse.class, relativePath);
          session.put(xlogsKey, response);
        }
      }
      catch (HttpClientException ex)
      {
        response = new ExchangeResponse();
      }
    }

    Grid pageXlogsGrid = new Grid();
    List<Status> statuses = response.getStatusList().getItems();
    
    pageXlogsGrid.setIdentifier(xlabel);
    pageXlogsGrid.setDescription(xtime);
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
    field.setWidth(100);
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
    
    pageXlogsGrid.setTotal(statuses.size());

    return pageXlogsGrid;
  }
}
