package org.iringtools.library.exchange;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import org.apache.log4j.Logger;
import org.iringtools.directory.Exchange;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.JaxbUtils;

public class DtiTask implements Runnable
{  
  private static final Logger logger = Logger.getLogger(DtiTask.class);
  
  private Map<String, Object> settings;
  private Exchange exchange;
  private DxiRequest dxiRequest;
  private RequestStatus requestStatus;
  private DataTransferIndices dtis;
  private String error;
    
  public DtiTask(Map<String, Object> settings, Exchange exchange, DxiRequest dxiRequest, 
      RequestStatus requestStatus)
  {
    this.settings = settings;
    this.exchange = exchange;
    this.dxiRequest = dxiRequest;
    this.requestStatus = requestStatus;
    this.error = null;
  }
  
  public void processDxiRequest() throws Exception
  {
    String urlPart = "/dxi/filter";
    
    String sourceDxiRelativePath = 
        "/" + exchange.getSourceScope() + 
        "/" + exchange.getSourceApp() + 
        "/" + exchange.getSourceGraph() + urlPart;

    String targetDxiRelativePath = 
        "/" + exchange.getTargetScope() + 
        "/" + exchange.getTargetApp() + 
        "/" + exchange.getTargetGraph() + urlPart;

    DxiRequest sourceDxiRequest = new DxiRequest();
    DxiRequest targetDxiRequest = new DxiRequest();

    Manifest sourceManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
    sourceManifest.getGraphs().getItems().get(0).setName(exchange.getSourceGraph());
    sourceDxiRequest.setManifest(sourceManifest);
    sourceDxiRequest.setDataFilter(dxiRequest.getDataFilter());

    Manifest targetManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
    targetManifest.getGraphs().getItems().get(0).setName(exchange.getTargetGraph());
    targetDxiRequest.setManifest(targetManifest);
    targetDxiRequest.setDataFilter(dxiRequest.getDataFilter());

    ExecutorService executor = Executors.newFixedThreadPool(2);

    DtiSubTask sourceDtiTask = new DtiSubTask(settings, exchange.getSourceUri(), sourceDxiRelativePath, sourceDxiRequest);
    executor.execute(sourceDtiTask);

    DtiSubTask targetDtiTask = new DtiSubTask(settings, exchange.getTargetUri(), targetDxiRelativePath, targetDxiRequest);
    executor.execute(targetDtiTask);

    executor.shutdown();
    executor.awaitTermination(Long.parseLong((String) settings.get("dtiTaskTimeout")), TimeUnit.SECONDS);

    DataTransferIndices sourceDtis = sourceDtiTask.getDataTransferIndices();
    if (sourceDtis == null)
    {
      String taskError = sourceDtiTask.getError();
      
      if (taskError != null)
      {
        error = taskError;
        return;
      }
      else
        sourceDtis = new DataTransferIndices();
    }

    DataTransferIndices targetDtis = targetDtiTask.getDataTransferIndices();
    if (targetDtis == null)
    {
      String taskError = targetDtiTask.getError();
      
      if (taskError != null)
      {
        error = taskError;
        return;
      }
      else
        targetDtis = new DataTransferIndices();
    }
    
    DifferencingProvider df = new DifferencingProvider();
    dtis =  df.diff(sourceDtis, targetDtis);
  }
  
  @Override
  public void run()
  {
    try
    {
      if (requestStatus != null)
      {
        requestStatus.setState(State.IN_PROGRESS);
      }
      
      processDxiRequest();
      
      if (requestStatus != null)
      {
        requestStatus.setResponseText(JaxbUtils.toXml(dtis, false));
        requestStatus.setState(State.COMPLETED);
      }
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      e.printStackTrace();      
      
      requestStatus.setMessage(e.getMessage());
      requestStatus.setState(State.ERROR);
    }
  }

  public DataTransferIndices getDataTransferIndices()
  {
    return dtis;
  }
  
  public String getError() 
  {
    return error;
  }
}

class DtiSubTask implements Runnable
{
  private static final Logger logger = Logger.getLogger(DtiTask.class);
  private Map<String, Object> settings;
  private String serviceUri;
  private String relativePath;
  private DxiRequest dxiRequest;
  private DataTransferIndices dtis;
  private String error;
  
  public DtiSubTask(Map<String, Object> settings, String serviceUri, String relativePath, DxiRequest dxiRequest)
  {
    this.settings = settings;
    this.serviceUri = serviceUri; 
    this.relativePath = relativePath;    
    this.dxiRequest = dxiRequest;
  }
  
  @Override
  public void run()
  {
    try 
    {
      HttpClient httpClient = new HttpClient(serviceUri + relativePath);
      HttpUtils.addHttpHeaders(settings, httpClient); 
      
      if (isAsync())
      {
        httpClient.setAsync(true);
        String statusURL = httpClient.post(String.class, dxiRequest);
        dtis = waitForRequestCompletion(DataTransferIndices.class, serviceUri + statusURL);
      }
      else
      {
        dtis = httpClient.post(DataTransferIndices.class, dxiRequest);
      }
    }
    catch (Exception e) 
    {
      logger.error("Error getting dti: " + e.getMessage());
      error = "Error getting dti: " + e.getMessage();
    }
  }
  
  public DataTransferIndices getDataTransferIndices()
  {
    return dtis;
  } 

  public String getError()
  {
    return error;
  }  
  
  protected <T> T waitForRequestCompletion(Class<T> clazz, String url)
  {
    T obj = null;

    try
    {
      RequestStatus requestStatus = null;
      long timeout = (Long)settings.get("asyncTimeout") * 1000;  // convert to milliseconds
      long interval = (Long)settings.get("pollingInterval") * 1000;  // convert to milliseconds
      long timeoutCount = 0;
      
      HttpClient httpClient = new HttpClient(url);
      HttpUtils.addHttpHeaders(settings, httpClient);
      
      while (timeoutCount < timeout)
      {
        requestStatus = httpClient.get(RequestStatus.class);

        if (requestStatus.getState() != State.IN_PROGRESS)
          break;

        Thread.sleep(interval);
        timeoutCount += interval;
      }

// Note that the requestStatus object will have been decoded (out of UTF-8) during the httpClient.get(), so if the object embedded within the
// requestStatus.ResponseText has non UTF-8 characters then we must encode that back into UTF-8 before passing to JaxbUtils.toObject
		InputStream streamUTF8 = new ByteArrayInputStream(requestStatus.getResponseText().getBytes("UTF-8"));
		obj = (T) JaxbUtils.toObject(clazz, streamUTF8);
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      e.printStackTrace();      
    }

    return obj;
  }
  
  private boolean isAsync()
  {
    String asyncHeader = "http-header-async";
    boolean async = settings.containsKey(asyncHeader) && Boolean.parseBoolean(settings.get(asyncHeader).toString());
    
    return async;
  }
}
