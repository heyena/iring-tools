package org.iringtools.models;

import java.util.ArrayList;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.data.filter.DataFilter;
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
 
  public ExchangeDataModel(Map<String, Object> session, String refServiceUri, FieldFit fieldFit, 
      boolean isAsync, long timeout, long interval)
  {
    super(DataMode.EXCHANGE, refServiceUri, fieldFit, isAsync, timeout, interval); 
    this.session = session;
  }
  
  public Grid getDtoGrid(String serviceUri, String scope, String xid, String filter, String sortBy, String sortOrder, 
      int start, int limit) throws DataModelException
  {
    String dtiRelativePath = "/" + scope + "/exchanges/" + xid;
    String dtoRelativePath = dtiRelativePath + "/page";
    String manifestRelativePath = dtiRelativePath + "/manifest";
    String dataFilterRelativePath = dtiRelativePath + "/datafilter";
    
    Grid pageDtoGrid = null;
    Manifest manifest = getManifest(serviceUri, manifestRelativePath);
    
    if (manifest != null && manifest.getGraphs() != null)
    {
      Graph graph = manifest.getGraphs().getItems().get(0);
      
      if (graph != null)
      {
        DataTransferObjects pageDtos = getPageDtos(serviceUri, manifestRelativePath, dtiRelativePath, 
            dtoRelativePath, filter, sortBy, sortOrder, start, limit, dataFilterRelativePath);
        
        pageDtoGrid = getDtoGrid(dtiRelativePath, manifest, graph, pageDtos);
        DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
        
        if (dtis == null || dtis.getDataTransferIndexList() == null || dtis.getDataTransferIndexList().getItems().size() == 0)
          pageDtoGrid.setTotal(0);
        else
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
      DataTransferObjects dtos = getRelatedItems(serviceUri, manifestRelativePath, dtiRelativePath, 
          dtoRelativePath, dtoIdentifier, filter, sortBy, sortOrder, start, limit);
        
      pageDtoGrid = getRelatedItemGrid(dtiRelativePath, manifest, graph, dtos, classId, classIdentifier);
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

    ExchangeResponse xRes = null;
    ExchangeRequest request = new ExchangeRequest();
    request.setManifest(manifest);
    request.setDataTransferIndices(dtis);
    request.setReviewed(reviewed);

    try
    {
      HttpClient httpClient = new HttpClient(serviceUri + exchangeRelativePath);
      HttpUtils.addHttpHeaders(session, httpClient);
     
      if (isAsync)
      {
        httpClient.setAsync(true);      
        String statusURL = httpClient.post(String.class, "/submit", request);
        xRes = waitForRequestCompletion(ExchangeResponse.class, serviceUri + statusURL);
      }
      else
      {
        xRes = httpClient.post(ExchangeResponse.class, "/submit", request);
      }
    }
    catch (Exception ex)
    {
      String error = "Error in submitExchange: " + ex.getMessage();
      logger.error(error);

      xRes = new ExchangeResponse();
      xRes.setSummary(error);
      xRes.setLevel(Level.ERROR);
    }
    
    if (xRes.getLevel() == Level.SUCCESS)
    {
      // remove cache data related to this exchange including the app data
      String appRelativePath = xRes.getReceiverScope() + "/" + xRes.getReceiverApp() + "/"
        + xRes.getReceiverGraph();
      
      for (String key : session.keySet())
      {
        if (key.contains(exchangeRelativePath) || key.contains(appRelativePath))
          removeSessionData(key);
      }
    }

    return xRes;
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
    HttpUtils.addHttpHeaders(session, httpClient);
    
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

    if (xlogs != null && xlogs.getExchangeResponses().size() > 0)
    {
      List<ExchangeResponse> xrs = xlogs.getExchangeResponses();
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
      field.setWidth(150);
      fields.add(field);

      field = new Field();
      field.setName("End Time");
      field.setDataIndex("End Time");
      field.setType("string");
      field.setWidth(150);
      fields.add(field);

      field = new Field();
      field.setName("Sender");
      field.setDataIndex("Sender");
      field.setType("string");
      field.setWidth(180);
      fields.add(field);

      field = new Field();
      field.setName("Receiver");
      field.setDataIndex("Receiver");
      field.setType("string");
      field.setWidth(180);
      fields.add(field);

      field = new Field();
      field.setName("Result");
      field.setDataIndex("Result");
      field.setType("string");
      field.setWidth(260);
      fields.add(field);

      for (ExchangeResponse xr : xrs)
      {
        List<String> row = new ArrayList<String>();        
        String startTime = format(xr.getStartTime().toGregorianCalendar());
        String endTime = format(xr.getEndTime().toGregorianCalendar());
        String exchangeFile = xr.getStartTime().toString().replace(":", ".");
        
        row.add("<input type=\"image\" src=\"resources/images/info-small.png\" "
            + "onClick='javascript:createPageXlogs(\"" + scope + "\",\"" + xid + "\",\"" + xlabel 
            + "\",\"" + startTime + "\",\"" + exchangeFile + "\"," + xr.getPoolSize() 
            + "," + xr.getItemCount() + ")'>");

        row.add(startTime);
        row.add(endTime);
        row.add(xr.getSenderScope() + "." + xr.getSenderApp() + "." + xr.getSenderGraph());
        row.add(xr.getReceiverScope() + "." + xr.getReceiverApp() + "." + xr.getReceiverGraph());
        row.add(xr.getSummary());
        
        data.add(row);
      }

      xlogsGrid.setTotal(xrs.size());
      xlogsGrid.setFields(fields);
      xlogsGrid.setData(data);
    }

    return xlogsGrid;
  }

  public Grid getPageXlogsGrid(String xlogsServiceUri, String scope, String xid, String xlabel, String xtime, 
      int start, int limit, int itemCount)
  {
    int actualLimit = (start + limit > itemCount) ? (itemCount - start) : limit;
    
    String relativePath = "/" + scope + "/exchanges/" + xid + "/" + xtime + "/" + start + "/" + actualLimit;
    String xlogsKey = XLOGS_PREFIX + relativePath;
    Response response;

    if (session.containsKey(xlogsKey))
    {
      response = (Response) session.get(xlogsKey);
    }
    else
    {
      HttpClient httpClient = new HttpClient(xlogsServiceUri);
      HttpUtils.addHttpHeaders(session, httpClient);
      
      try
      {
        response = httpClient.get(Response.class, relativePath);
        session.put(xlogsKey, response);
      }
      catch (HttpClientException ex)
      {
        logger.error("Error getting pool log: " + ex);
        response = new Response();
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
    field.setWidth(120);
    field.setDataIndex("Result");
    field.setType("string");
    fields.add(field);

    for (int i = 0; i < statuses.size(); i++)
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
    
    pageXlogsGrid.setTotal(itemCount);

    return pageXlogsGrid;
  }
 //Getting summary Grid
  
  public Grid getSummaryGrid(String serviceUri, String scope, String xid, String xlabel) throws DataModelException
  {
    //String relativePath = "/" + scope + "/exchanges/" + xid+"?dtiOnly=false";
    String relativePath = "/" + scope + "/exchanges/" + xid+"/differences/summary";
    ExchangeResponse xlogs = null;

    HttpClient httpClient = new HttpClient(serviceUri);
    HttpUtils.addHttpHeaders(session, httpClient);
    
    try
    {
      xlogs = httpClient.post(ExchangeResponse.class, relativePath,new DataFilter());
      System.out.println(xlogs);
    }
    catch (HttpClientException ex)
    {
      logger.error("Error getting exchange logs: " + ex);
      throw new DataModelException(ex.getMessage());
    }

    Grid summaryGrid = new Grid();

   if (xlogs != null && xlogs.getItemCount() > 0)
    {
      //List<ExchangeResponse> xrs = xlogs.getExchangeResponses();
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
      field.setDataIndex("StartTime");
      field.setType("string");
      field.setWidth(150);
      fields.add(field);

      field = new Field();
      field.setName("End Time");
      field.setDataIndex("EndTime");
      field.setType("string");
      field.setWidth(150);
      fields.add(field);

      field = new Field();
      field.setName("Sender");
      field.setDataIndex("Sender");
      field.setType("string");
      field.setWidth(180);
      fields.add(field);

      field = new Field();
      field.setName("Receiver");
      field.setDataIndex("Receiver");
      field.setType("string");
      field.setWidth(180);
      fields.add(field);

      field = new Field();
      field.setName("Result");
      field.setDataIndex("Result");
      field.setType("string");
      field.setWidth(260);
      fields.add(field);
      
      field = new Field();
      field.setName("Item Count");
      field.setDataIndex("ItemCount");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Add Count");
      field.setDataIndex("AddCount");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Delete Count");
      field.setDataIndex("DeleteCount");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Change Count");
      field.setDataIndex("ChangeCount");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Synchronized Count");
      field.setDataIndex("SynchronizedCount");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Pool Size");
      field.setDataIndex("PoolSize");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Sender App");
      field.setDataIndex("SenderApp");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Sender Graph");
      field.setDataIndex("SenderGraph");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Sender Scope");
      field.setDataIndex("SenderScope");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Sender Uri");
      field.setDataIndex("SenderUri");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      
      field = new Field();
      field.setName("Receiver App");
      field.setDataIndex("ReceiverApp");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Receiver Graph");
      field.setDataIndex("ReceiverGraph");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Receiver Scope");
      field.setDataIndex("ReceiverScope");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      field = new Field();
      field.setName("Receiver Uri");
      field.setDataIndex("ReceiverUri");
      field.setType("string");
      field.setWidth(MIN_FIELD_WIDTH);
      fields.add(field);
      
      List<String> row = new ArrayList<String>();
      //String startTime = format(xlogs.getStartTime().toGregorianCalendar());
      //String endTime = format(xlogs.getEndTime().toGregorianCalendar());
      //String exchangeFile = xr.getStartTime().toString().replace(":", ".");
      row.add("");
      row.add("");
      row.add("");
      row.add(xlogs.getSenderScope() + "." + xlogs.getSenderApp() + "." + xlogs.getSenderGraph());
      row.add(xlogs.getReceiverScope() + "." + xlogs.getReceiverApp() + "." + xlogs.getReceiverGraph());
      row.add(xlogs.getSummary());
      row.add(String.valueOf(xlogs.getItemCount()));
      row.add(String.valueOf(xlogs.getItemCountAdd()));
      row.add(String.valueOf(xlogs.getItemCountDelete()));
      row.add(String.valueOf(xlogs.getItemCountChange()));
      row.add(String.valueOf(xlogs.getItemCountSync()));
      row.add(String.valueOf(xlogs.getPoolSize()));
      row.add(xlogs.getSenderApp());
      row.add(xlogs.getSenderGraph());
      row.add(xlogs.getSenderScope());
      row.add(xlogs.getSenderUri());
      row.add(xlogs.getReceiverApp());
      row.add(xlogs.getReceiverGraph());
      row.add(xlogs.getReceiverScope());
      row.add(xlogs.getReceiverUri());
      
      data.add(row);

    /*  for (ExchangeResponse xr : xrs)
      {
        List<String> row = new ArrayList<String>();        
        String startTime = format(xr.getStartTime().toGregorianCalendar());
        String endTime = format(xr.getEndTime().toGregorianCalendar());
        String exchangeFile = xr.getStartTime().toString().replace(":", ".");
        
        row.add("<input type=\"image\" src=\"resources/images/info-small.png\" "
            + "onClick='javascript:createPageXlogs(\"" + scope + "\",\"" + xid + "\",\"" + xlabel 
            + "\",\"" + startTime + "\",\"" + exchangeFile + "\"," + xr.getPoolSize() 
            + "," + xr.getItemCount() + ")'>");

        row.add(startTime);
        row.add(endTime);
        row.add(xr.getSenderScope() + "." + xr.getSenderApp() + "." + xr.getSenderGraph());
        row.add(xr.getReceiverScope() + "." + xr.getReceiverApp() + "." + xr.getReceiverGraph());
        row.add(xr.getSummary());
        
        data.add(row);
      }*/

      //xlogsGrid.setTotal(xrs.size());
      summaryGrid.setTotal(xlogs.getItemCount());
      summaryGrid.setFields(fields);
      summaryGrid.setData(data);
    }

    return summaryGrid;
  }
}
