package org.iringtools.services.core;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import org.apache.log4j.Logger;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DfiRequest;
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
  private ExchangeDefinition xdef;
  private DxiRequest dxiRequest;
  private boolean dtiOnly;
  private RequestStatus requestStatus;
  private DataTransferIndices dtis;
    
  public DtiTask(Map<String, Object> settings, ExchangeDefinition xdef, DxiRequest dxiRequest, boolean dtiOnly, RequestStatus requestStatus)
  {
    this.settings = settings;
    this.xdef = xdef;
    this.dxiRequest = dxiRequest;
    this.dtiOnly = dtiOnly;
    this.requestStatus = requestStatus;
  }
  
  public void processDxiRequest() throws Exception
  {
    String urlPart = "/dxi/filter?hashAlgorithm=" + xdef.getHashAlgorithm();
    
    String sourceDxiRelativePath = 
        "/" + xdef.getSourceScopeName() + 
        "/" + xdef.getSourceAppName() + 
        "/" + xdef.getSourceGraphName() + urlPart;

    String targetDxiRelativePath = 
        "/" + xdef.getTargetScopeName() + 
        "/" + xdef.getTargetAppName() + 
        "/" + xdef.getTargetGraphName() + urlPart;

    DxiRequest sourceDxiRequest = new DxiRequest();
    DxiRequest targetDxiRequest = new DxiRequest();

    Manifest sourceManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
    sourceManifest.getGraphs().getItems().get(0).setName(xdef.getSourceGraphName());
    sourceDxiRequest.setManifest(sourceManifest);
    sourceDxiRequest.setDataFilter(dxiRequest.getDataFilter());

    Manifest targetManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
    targetManifest.getGraphs().getItems().get(0).setName(xdef.getTargetGraphName());
    targetDxiRequest.setManifest(targetManifest);
    targetDxiRequest.setDataFilter(dxiRequest.getDataFilter());

    ExecutorService executor = Executors.newFixedThreadPool(2);

    DtiSubTask sourceDtiTask = new DtiSubTask(settings, xdef.getSourceUri(), sourceDxiRelativePath, sourceDxiRequest);
    executor.execute(sourceDtiTask);

    DtiSubTask targetDtiTask = new DtiSubTask(settings, xdef.getTargetUri(), targetDxiRelativePath, targetDxiRequest);
    executor.execute(targetDtiTask);

    executor.shutdown();
    executor.awaitTermination(Long.parseLong((String) settings.get("dtiTaskTimeout")), TimeUnit.SECONDS);

    DataTransferIndices sourceDtis = sourceDtiTask.getDataTransferIndices();
    if (sourceDtis == null)
    {
      String error = sourceDtiTask.getError();
      
      if (error != null)
        throw new Exception(error);
      else
        sourceDtis = new DataTransferIndices();
    }

    DataTransferIndices targetDtis = targetDtiTask.getDataTransferIndices();
    if (targetDtis == null)
    {
      String error = targetDtiTask.getError();
      
      if (error != null)
        throw new Exception(error);
      else
        targetDtis = new DataTransferIndices();
    }
    
    if (dtiOnly)
    {
      dtis = sourceDtis;

      if (sourceDtis.getDataTransferIndexList() == null)
      {
        dtis.setDataTransferIndexList(new DataTransferIndexList());
      }

      if (targetDtis.getDataTransferIndexList() != null)
      {
        List<DataTransferIndex> resultDtiList = dtis.getDataTransferIndexList().getItems();

        if (resultDtiList.isEmpty())
        {    // if source is empty then check for dup count only in target
        	 targetDups(targetDtis);
          dtis = targetDtis;
        }
        else
        {    targetDups(targetDtis);
             sourceDups(dtis);
          for (DataTransferIndex targetDti : targetDtis.getDataTransferIndexList().getItems())
          {
            boolean found = false;

            // check for duplicates
            for (DataTransferIndex resultDti : resultDtiList)
            { 
              if (resultDti.getIdentifier().equalsIgnoreCase(targetDti.getIdentifier()))
              {
                found = true;
                break;
              }
            }

            if (!found)
            {
              resultDtiList.add(targetDti);
            }
          }
        }
      }
    }
    else
    {
      sourceDtis.setScopeName(xdef.getSourceScopeName());
      sourceDtis.setAppName(xdef.getSourceAppName());

      targetDtis.setScopeName(xdef.getTargetScopeName());
      targetDtis.setAppName(xdef.getTargetAppName());

      // create dxi request to diff source and target dti
      DfiRequest dfiRequest = new DfiRequest();
      dfiRequest.setSourceScopeName(xdef.getSourceScopeName());
      dfiRequest.setSourceAppName(xdef.getSourceAppName());
      dfiRequest.setTargetScopeName(xdef.getTargetScopeName());
      dfiRequest.setTargetAppName(xdef.getTargetAppName());
      dfiRequest.getDataTransferIndices().add(sourceDtis);
      dfiRequest.getDataTransferIndices().add(targetDtis);

      // request exchange service to diff the dtis
      String dxiUrl = settings.get("differencingServiceUri") + "/dxi";
      HttpClient httpClient = new HttpClient();
      HttpUtils.addHttpHeaders(settings, httpClient);
      
      dtis = httpClient.post(DataTransferIndices.class, dxiUrl, dfiRequest);
    }
  }
  
  public void sourceDups(DataTransferIndices sourceDtis)
  {
	  DataTransferIndex previousDti = null;        
      List<DataTransferIndex> sourceDtiItems = sourceDtis.getDataTransferIndexList().getItems();
      
      for (int i = 0; i < sourceDtiItems.size(); i++)
      {
        DataTransferIndex dti = sourceDtiItems.get(i);          
        dti.setDuplicateCount(1);            
        
        if (previousDti != null && dti.getIdentifier().equalsIgnoreCase(previousDti.getIdentifier()))
        {
          previousDti.setDuplicateCount(previousDti.getDuplicateCount() + 1);          
          sourceDtiItems.remove(i--);
        }
        else
        {
          previousDti = dti;
        }
      }
  }
  
  public void targetDups(DataTransferIndices targetDtis) 
  {     
      List<DataTransferIndex> targetDtiItems = targetDtis.getDataTransferIndexList().getItems();
      DataTransferIndex previousTarDti = null;
      for (int i = 0; i < targetDtiItems.size(); i++)
      {
        DataTransferIndex dti = targetDtiItems.get(i);          
        dti.setDuplicateCount(1);
        
        if (previousTarDti != null && dti.getIdentifier().equalsIgnoreCase(previousTarDti.getIdentifier()))
        {
      	  previousTarDti.setDuplicateCount(previousTarDti.getDuplicateCount() + 1);
          targetDtiItems.remove(i--);
        }
        else
        {
        	previousTarDti = dti;
        }
      }
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
