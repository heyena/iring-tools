package org.iringtools.library.exchange;

import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.FileFilter;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.Map;

import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Response;
import org.iringtools.directory.Exchange;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjectList;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class ExchangeTask implements Runnable
{
  private static final Logger logger = Logger.getLogger(ExchangeTask.class);
  private DatatypeFactory datatypeFactory = null;

  private Map<String, Object> settings;
  private ExchangeRequest xReq;
  private ExchangeResponse xRes;
  private String scope;
  private String id;
  private Exchange exchange;
  private RequestStatus requestStatus;

  public ExchangeTask(final Map<String, Object> settings, String scope, String id, Exchange exchange, 
      ExchangeRequest xReq, RequestStatus requestStatus)
  {
    this.settings = settings;
    this.scope = scope;
    this.id = id;
    this.exchange = exchange;
    this.xReq = xReq;
    this.requestStatus = requestStatus;

    try
    {
      datatypeFactory = DatatypeFactory.newInstance();
    }
    catch (DatatypeConfigurationException e)
    {
      logger.error(e.getMessage());
      e.printStackTrace();
    }
  }

  public ExchangeResponse getExchangeResponse()
  {
    return xRes;
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
      
      //
      // create exchange response
      //
      xRes = new ExchangeResponse();
      xRes.setExchangeId(id);
      xRes.setLevel(Level.SUCCESS);
      xRes.setSummary("");
  
      long startTime = Calendar.getInstance().getTimeInMillis();
      GregorianCalendar c = new GregorianCalendar();
      c.setTimeInMillis(startTime);
      xRes.setStartTime(DatatypeFactory.newInstance().newXMLGregorianCalendar(c));
  
      //
      // create directory for logging the exchange
      //
      String xpath = settings.get("basePath") + "/WEB-INF/exchanges/" + scope + "/" + id;
      File dirPath = new File(xpath);
  
      if (!dirPath.exists())
      {
        dirPath.mkdirs();
      }
  
      String exchangeFile = xpath + "/" + startTime;
      
      //
      // check exchange request
      //
      if (xReq == null)
      {
        xRes.setLevel(Level.WARNING);
        String message = "Exchange request is empty.";
        StringBuilder summary = new StringBuilder(xRes.getSummary());
        xRes.setSummary(summary.append(message).toString());
        //return can't return here because then there would be no exchange response post against the requestStatus
      }
      else
      {
      	Manifest manifest = xReq.getManifest();
      	DataTransferIndices dtis = xReq.getDataTransferIndices();
  
      	//
      	// check data transfer indices
      	//
      	if (dtis == null)
      	{
      		xRes.setLevel(Level.WARNING);
      		String message = "No data transfer indices found.";
      		StringBuilder summary = new StringBuilder(xRes.getSummary());
      		xRes.setSummary(summary.append(message).toString());
      	    //return can't return here because then there would be no exchange response post against the requestStatus
      	}
      	else
      	{
  		    //
  		    // collect ADD/CHANGE/DELETE indices
  		    //
  		    int iCountSync = 0;
  		    int iCountAdd = 0;
  		    int iCountChange = 0;
  		    int iCountDelete = 0;
  		
  		    List<DataTransferIndex> dxIndices = new ArrayList<DataTransferIndex>();
  		
  		    for (DataTransferIndex dxi : dtis.getDataTransferIndexList().getItems())
  		    {
  		      // exclude DTOs with duplicates
  		      if (dxi.getDuplicateCount() != null && dxi.getDuplicateCount() > 1)
  		      {
  		        String message = "Excluding DTO [" + dxi.getIdentifier() + "] due to [" + dxi.getDuplicateCount() + "] duplicates. ";
  		        logger.warn(message);
  		        
  		        StringBuilder summary = new StringBuilder(xRes.getSummary());
  		        xRes.setSummary(summary.append(message).toString());
  		        continue;
  		      }
  		      
  		      TransferType transferType = dxi.getTransferType();
  		
  		      if (transferType != TransferType.SYNC)
  		      {
  		        if (transferType == TransferType.ADD)
  		        {
  		          iCountAdd++;
  		        }
  		        else if (transferType != TransferType.CHANGE)
  		        {
  		          iCountChange++;
  		        }
  		        else // TransferType.DELETE)
  		        {
  		          iCountDelete++;
  		        }
  		
  		        dxIndices.add(dxi);
  		      }
  		      else
  		      {
  		        iCountSync++;
  		      }
  		    }
  		
  		    //
  		    // add exchange definition info to exchange response
  		    //
  		    xRes.setSenderUri(exchange.getSourceUri());
  		    xRes.setSenderScope(exchange.getSourceScope());
  		    xRes.setSenderApp(exchange.getSourceApp());
  		    xRes.setSenderGraph(exchange.getSourceGraph());
  		    xRes.setReceiverUri(exchange.getTargetUri());
  		    xRes.setReceiverScope(exchange.getTargetScope());
  		    xRes.setReceiverApp(exchange.getTargetApp());
  		    xRes.setReceiverGraph(exchange.getTargetGraph());
  		
  		    //
  		    // prepare source and target endpoint
  		    //
  		    String targetAppUrl = exchange.getTargetUri() + "/" + exchange.getTargetScope() + "/" + exchange.getTargetApp();
  		    String targetGraphUrl = targetAppUrl + "/" + exchange.getTargetGraph();
  		    String sourceGraphUrl = exchange.getSourceUri() + "/" + exchange.getSourceScope() + "/" + exchange.getSourceApp() + "/"
  		        + exchange.getSourceGraph();
  		    String sourceDtoUrl = sourceGraphUrl + "/dxo";
  		
  		    //
  		    // create pool DTOs
  		    //
  		    int dxIndicesSize = dxIndices.size();
  		
  		    // if pool size is not set for specific data exchange, then use the default one
  		    int poolSize = 0;
  		
  		    if (exchange.getPoolSize() == null || exchange.getPoolSize() == 0)
  		    {
  		      poolSize = Integer.parseInt((String) settings.get("poolSize"));
  		    }
  		    else
  		    {
  		      poolSize = exchange.getPoolSize();
  		    }
  		
  		    poolSize = Math.min(poolSize, dxIndicesSize);
  		
  		    if (poolSize == 0)
  		      poolSize = 100;
  		
  		    xRes.setPoolSize(poolSize);
  		    xRes.setItemCount(dxIndicesSize);
  		    xRes.setItemCountSync(iCountSync);
  		    xRes.setItemCountAdd(iCountAdd);
  		    xRes.setItemCountChange(iCountChange);
  		    xRes.setItemCountDelete(iCountDelete);
  		
  		    //
  		    // make sure there are items to exchange
  		    //
  		    if (dxIndices.size() == 0)
  		    {
  		      xRes.setLevel(Level.WARNING);
  		
  		      String message = "No changing/deleting items found. ";
  		      StringBuilder summary = new StringBuilder(xRes.getSummary());
  		      xRes.setSummary(summary.append(message).toString());
  		      
  		      if (requestStatus != null)
	  	      {
  		    	requestStatus.setMessage(message);  
	  	        requestStatus.setResponseText(JaxbUtils.toXml(xRes, false));
	  	        requestStatus.setPercentComplete(100);
	  	        requestStatus.setState(State.COMPLETED);
	  	      }  
  		    }
  		    else
  		    {
  			    for (int i = 0; i < dxIndicesSize; i += poolSize)
  			    {
  			      int actualPoolSize = (dxIndicesSize > (i + poolSize)) ? poolSize : dxIndicesSize - i;
  			      List<DataTransferIndex> poolDtiItems = dxIndices.subList(i, i + actualPoolSize);
  			      List<DataTransferIndex> sourceDtiItems = new ArrayList<DataTransferIndex>();
  			
  			      DataTransferObjects poolDtos = new DataTransferObjects();
  			      DataTransferObjectList poolDtosList = new DataTransferObjectList();
  			      poolDtos.setDataTransferObjectList(poolDtosList);
  			      List<DataTransferObject> poolDtoListItems = new ArrayList<DataTransferObject>();
  			      poolDtosList.setItems(poolDtoListItems);
  			
  			      if (requestStatus != null)
				  {  			    
  			    	int perCompleted = (int)((((float)i) /dxIndicesSize)*100);  	
			  		requestStatus.setMessage("Processing Pool [ "+ (i + 1) +" - "+ (i + actualPoolSize) + " ]");  
		  	        requestStatus.setResponseText(JaxbUtils.toXml(xRes, false));
		  	        requestStatus.setPercentComplete(perCompleted);
		  	        requestStatus.setState(State.IN_PROGRESS);
				  } 
  			      
  			      //
  			      // create deleted DTOs and collect add/change DTIs from source
  			      //
  			      
  			      boolean hasContent = false;
  			      
  			      for (DataTransferIndex poolDtiItem : poolDtiItems)
  			      {
  			        if (poolDtiItem.getTransferType() == TransferType.DELETE)
  			        {
  			          DataTransferObject deletedDto = new DataTransferObject();
  			          deletedDto.setIdentifier(poolDtiItem.getInternalIdentifier());
  			          deletedDto.setTransferType(org.iringtools.dxfr.dto.TransferType.DELETE);
  			          poolDtoListItems.add(deletedDto);
  			        }
  			        else
  			        {
  			          if (poolDtiItem.getTransferType() == TransferType.CHANGE)
  			          {
  			            int splitIndex = poolDtiItem.getInternalIdentifier().indexOf(Constants.CHANGE_TOKEN);
  			            poolDtiItem.setInternalIdentifier(poolDtiItem.getInternalIdentifier().substring(0, splitIndex));
  			          }
  			
  			          sourceDtiItems.add(poolDtiItem);
                  
                  if (poolDtiItem.getHasContent() != null && poolDtiItem.getHasContent())
                  {
                    hasContent = true;                  
                  }
  			        }
  			      }
  			
  			      //
  			      // get add/change DTOs from source endpoint
  			      //
  			      DataTransferObjects sourceDtos = null;
  			      DxoRequest sourceDtosRequest = new DxoRequest();
  			      sourceDtosRequest.setManifest(manifest);
  			      DataTransferIndices sourceDtis = new DataTransferIndices();
  			      sourceDtosRequest.setDataTransferIndices(sourceDtis);
  			      DataTransferIndexList sourceDtiList = new DataTransferIndexList();
  			      sourceDtis.setDataTransferIndexList(sourceDtiList);
  			      sourceDtiList.setItems(sourceDtiItems);
  			
  			      try
  			      {
  			        manifest.getGraphs().getItems().get(0).setName(exchange.getSourceGraph());
  			
  			        HttpClient httpClient = new HttpClient();
                HttpUtils.addHttpHeaders(settings, httpClient);
                      
  		          logger.debug("Requesting source DTOs from [" + sourceDtoUrl + "]: ");
  		          
  		          String dtoUrl = hasContent ?  sourceDtoUrl + "?includeContent=true" : sourceDtoUrl;
  		          
                if (!isAsync())
  			        {
                  httpClient.setAsync(false);
                  sourceDtos = httpClient.post(DataTransferObjects.class, dtoUrl, sourceDtosRequest);
  			        }
  			        else
  			        {
                  httpClient.setAsync(true);
                  String statusURL = httpClient.post(String.class, dtoUrl, sourceDtosRequest);
                  sourceDtos = waitForRequestCompletion(DataTransferObjects.class, exchange.getSourceUri() + statusURL);
  			        }
                			
  			        sourceDtosRequest = null;
  			      }
  			      catch (Exception e)
  			      {
  			        logger.error(e.getMessage());
  			        e.printStackTrace();
  			        
  			        xRes.setLevel(Level.ERROR);
  			        StringBuilder summary = new StringBuilder(xRes.getSummary());
  			        xRes.setSummary(summary.append(e.getMessage()).toString());
  			      }
  			
  			      //
  			      // add and set status for each added/changed DTO in exchange pool
  			      //
  			      if (sourceDtos != null && sourceDtos.getDataTransferObjectList() != null)
  			      {
  			        for (DataTransferIndex dti : sourceDtiItems)
  			        {
  			          for (DataTransferObject dto : sourceDtos.getDataTransferObjectList().getItems())
  			          {
  			            if (dti.getIdentifier().equalsIgnoreCase(dto.getIdentifier()))
  			            {
  			              dto.setTransferType(org.iringtools.dxfr.dto.TransferType.valueOf(dti.getTransferType().toString()));
  			              poolDtoListItems.add(dto);
  			              break;
  			            }
  			          }
  			        }
  			      }
  			
  			      //
  			      // send pool DTOs
  			      //
  			      if (poolDtoListItems.size() > 0)
  			      {
  			        Response poolResponse = null;
  			
  			        try
  			        {
  			          String targetUrl = targetGraphUrl + "?format=stream";
  			
  			          String poolRange = i + " - " + (i + actualPoolSize);
  			          poolDtos.setSenderScopeName(exchange.getSourceScope());
  			          poolDtos.setSenderAppName(exchange.getSourceApp());
  			
  			          logger.debug("Sending pool DTOs to [" + targetUrl + "]: ");
  			
  			          HttpClient httpClient = new HttpClient();
  			          HttpUtils.addHttpHeaders(settings, httpClient);
  			
  			          //TODO: handle asynchronous for content DTO also
  	              if (!isAsync() || hasContent)
  			          {
  	                httpClient.setAsync(false);
  	                poolResponse = httpClient.post(Response.class, targetUrl, poolDtos, "text/plain");
  			          }
  			          else
  			          {
  			            httpClient.setAsync(true);
  			            String statusURL = httpClient.post(String.class, targetUrl, poolDtos, "text/plain");
  			            poolResponse = waitForRequestCompletion(Response.class, exchange.getTargetUri() + statusURL);
  			          }         
  			
  			          logger.info("Pool [" + poolRange + "] completed."); 
  			
  			          // free up resources
  			          poolDtos = null;
  			          sourceDtos = null;
  			          poolDtiItems = null;
  			        }
  			        catch (Exception e)
  			        {
  			          logger.error(e.getMessage());
  			          e.printStackTrace();
  			          
  			          xRes.setLevel(Level.ERROR);          
  			          StringBuilder summary = new StringBuilder(xRes.getSummary());
  			          xRes.setSummary(summary.append(e.getMessage()).toString());
  			        }
  			
  			        if (poolResponse != null)
  			        {
  			          try
  			          {
  			            // write pool response to disk
  			            String poolResponseFile = exchangeFile + Constants.XITEM_PREFIX + (i + 1) + "-" + (i + actualPoolSize) + ".xml";
  			            JaxbUtils.write(poolResponse, poolResponseFile, true);
  			          }
  			          catch (Exception e)
  			          {
  			            logger.error("Error writing pool response to disk: " + e);
  			            e.printStackTrace();            
  			          }	          		       
  			
  			          // update level as necessary
  			          if (xRes.getLevel().ordinal() < poolResponse.getLevel().ordinal())
  			          {
  			            xRes.setLevel(poolResponse.getLevel()); 
  			          }
  			      
  			          if (poolResponse.getLevel().name() != Level.SUCCESS.toString())
  			          {
  			        	  StringBuilder messages = new StringBuilder();
  			        	  if (poolResponse.getMessages() != null)
  			        	  {
  			        		  for (String message : poolResponse.getMessages().getItems())
  			        		  {
  			        			  messages.append(message);
  			        		  }
  			        		  
  			        		xRes.setSummary(xRes.getSummary().concat(messages.toString()));
  			        	  }  			        	
    			      }
  			
  			          poolResponse = null;
  			        }
  			      }
  			    }
  		    } // there were some add, update or delete dti's
      	} // dti's count > 0
      } // a valid xReq
      
      if (xRes.getLevel() == Level.ERROR)
      {
        String message = "Exchange completed with error. ";
        StringBuilder summary = new StringBuilder(xRes.getSummary());
        xRes.setSummary(summary.insert(0, message).toString());
      }
      else if (xRes.getLevel() == Level.WARNING)
      {
        String message = "Exchange completed with warning. ";
        StringBuilder summary = new StringBuilder(xRes.getSummary());
        xRes.setSummary(summary.insert(0, message).toString());
      }
      else if (xRes.getLevel() == Level.SUCCESS)
      {
        String message = "Exchange completed successfully.";
        xRes.setSummary(message);
      }
  
      XMLGregorianCalendar endTime = datatypeFactory.newXMLGregorianCalendar(new GregorianCalendar());
      xRes.setEndTime(endTime);

      // write exchange response to disk
      logger.debug("Writing exchange response [" + exchangeFile + ".xml] to disk...");
      JaxbUtils.write(xRes, exchangeFile + ".xml", false);

      //
      // if number of exchanges exceed the limit, remove the oldest one
      //
      List<String> exchangeLogs = IOUtils.getFiles(xpath);
      
      // exclude pool exchange logs
      for (int i = 0; i < exchangeLogs.size(); i++)
      {
        if (exchangeLogs.get(i).contains(Constants.XITEM_PREFIX))
        {
          exchangeLogs.remove(i--);
        }
      }

      // sort by date
      Collections.sort(exchangeLogs);

      int maxHistoriesToKeep;
      try
      {
    	  maxHistoriesToKeep = Integer.valueOf((String) settings.get("exchangeHistory"));
      }
      catch (Exception e)
      {
    	  maxHistoriesToKeep = 10;
      }
      
      // remove old logs
      while (exchangeLogs.size() > maxHistoriesToKeep)
      {
        final String filePrefix = (exchangeLogs.get(0).replace(".xml", ""));

        FileFilter fileFilter = new FileFilter()
        {
          public boolean accept(File file)
          {
            return file.getName().startsWith(filePrefix);
          }
        };

        for (File file : new File(xpath).listFiles(fileFilter))
        {
          file.delete();
        }

        exchangeLogs.remove(0);
      }
      
      if (requestStatus != null)
      {
        requestStatus.setResponseText(JaxbUtils.toXml(xRes, false));
        requestStatus.setState(State.COMPLETED);
        requestStatus.setPercentComplete(100);
        requestStatus.setMessage("Processed");
      }
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      e.printStackTrace();
      
      xRes.setLevel(Level.ERROR);
      StringBuilder summary = new StringBuilder(xRes.getSummary());
      xRes.setSummary(summary.append(e.getMessage()).toString());
      
      if (requestStatus != null)
      {
        requestStatus.setMessage(e.getMessage());
        requestStatus.setState(State.ERROR);
      }
    }
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
      e.printStackTrace();
      logger.error("Error during exchange: " + e.toString());
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
