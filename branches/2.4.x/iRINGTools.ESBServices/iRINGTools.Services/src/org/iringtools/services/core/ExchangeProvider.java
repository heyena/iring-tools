package org.iringtools.services.core;

import java.io.File;
import java.io.FileFilter;
import java.util.ArrayList;
import java.util.Collections;
import java.util.GregorianCalendar;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import javax.ws.rs.core.MediaType;
import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.commons.codec.digest.DigestUtils;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Response;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjectList;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.RoleType;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Graphs;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.manifest.TransferOption;
import org.iringtools.dxfr.request.DfiRequest;
import org.iringtools.dxfr.request.DfoRequest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.mapping.ValueListMaps;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class ExchangeProvider
{
  private static final Logger logger = Logger.getLogger(ExchangeProvider.class);
  public static final String POOL_PREFIX = "_pool_";
  
  private static final String progressFormat = "%d-%d/%d";
  private static final String splitToken = "->";
  
  private static Hashtable<String, String> exchangeProgresses = new Hashtable<String, String>();  
  private static DatatypeFactory datatypeFactory = null;
  
  private Map<String, Object> settings;
  private HttpClient httpClient = null;

  private String sourceUri = null;
  private String sourceScopeName = null;
  private String sourceAppName = null;
  private String sourceGraphName = null;
  private String targetUri = null;
  private String targetScopeName = null;
  private String targetAppName = null;
  private String targetGraphName = null;
  private String hashAlgorithm = null;
  private Integer exchangePoolSize;

  public ExchangeProvider(Map<String, Object> settings) throws ServiceProviderException
  {
    this.settings = settings;

    try
    {
      datatypeFactory = DatatypeFactory.newInstance();
      httpClient = new HttpClient();
      HttpUtils.addHttpHeaders(settings, httpClient);
    }
    catch (DatatypeConfigurationException e)
    {
      String message = "Error initializing exchange provider: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }

  public Directory getDirectory() throws ServiceProviderException
  {
    logger.debug("getDirectory()");

    try
    {
      String url = settings.get("directoryServiceUri") + "/directory";      
      Directory directory = httpClient.get(Directory.class, url);
      
      if (directory == null)
      {
        logger.warn("Directory is empty");
      }
      
      return directory;
    }
    catch (HttpClientException e)
    {
      logger.error("Error getting directory information: " + e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }
  }

  public Manifest getManifest(String scope, String id) throws ServiceProviderException
  {
    logger.debug("getManifest(" + scope + "," + id + ")");
    initExchangeDefinition(scope, id);
    return createCrossedManifest();
  }

  public DataTransferIndices getDataTransferIndices(String scope, String id, DxiRequest dxiRequest, boolean dtiOnly)
      throws ServiceProviderException
  {
    logger.debug("getDataTransferIndices(" + scope + "," + id + ",dxiRequest)");
    
    initExchangeDefinition(scope, id);

    String sourceDtiUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName
        + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;

    String targetDtiUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName
        + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;
    
    DxiRequest sourceDxiRequest = new DxiRequest();
    DxiRequest targetDxiRequest = new DxiRequest();
    
    try
    {
      Manifest sourceManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
      sourceManifest.getGraphs().getItems().get(0).setName(sourceGraphName);
      sourceDxiRequest.setManifest(sourceManifest);
      sourceDxiRequest.setDataFilter(dxiRequest.getDataFilter());
      
      Manifest targetManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
      targetManifest.getGraphs().getItems().get(0).setName(targetGraphName);
      targetDxiRequest.setManifest(targetManifest);
      targetDxiRequest.setDataFilter(dxiRequest.getDataFilter());      
    }
    catch (Exception e)
    {
      String error = "Error cloning crossed graph: " + e.getMessage();
      logger.error(error);
      throw new ServiceProviderException(error);
    }
    
    ExecutorService execSvc = Executors.newFixedThreadPool(2); 
    
    DtiTask sourceDtiTask = new DtiTask(settings, sourceDtiUrl, sourceDxiRequest);    
    execSvc.execute(sourceDtiTask);    
    
    DtiTask targetDtiTask = new DtiTask(settings, targetDtiUrl, targetDxiRequest);    
    execSvc.execute(targetDtiTask);    
    
    execSvc.shutdown();
    
    try {
      execSvc.awaitTermination(Long.parseLong((String) settings.get("dtiTaskTimeout")), TimeUnit.SECONDS);
    } 
    catch (InterruptedException e) {
      logger.error("DTI Task Executor interrupted: " + e.getMessage());
    }
    
    DataTransferIndices sourceDtis = sourceDtiTask.getDataTransferIndices();
    if (sourceDtis == null)
    {
      sourceDtis = new DataTransferIndices();
    }
     
    DataTransferIndices targetDtis = targetDtiTask.getDataTransferIndices();
    if (targetDtis == null)
    {
      targetDtis = new DataTransferIndices();
    }
    
    DataTransferIndices resultDtis = null;
    
    if (dtiOnly)
    {
      resultDtis = sourceDtis;
      
      if (sourceDtis.getDataTransferIndexList() == null)
      {
        resultDtis.setDataTransferIndexList(new DataTransferIndexList());  
      }
      
      if (targetDtis.getDataTransferIndexList() != null)
      {
        List<DataTransferIndex> resultDtiList = resultDtis.getDataTransferIndexList().getItems();
        
        if (resultDtiList.isEmpty())
        {
          resultDtis = targetDtis;
        }
        else
        {
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
      sourceDtis.setScopeName(sourceScopeName);
      sourceDtis.setAppName(sourceAppName);
     
      targetDtis.setScopeName(targetScopeName);
      targetDtis.setAppName(targetAppName);
      
      // create dxi request to diff source and target dti
      DfiRequest dfiRequest = new DfiRequest();
      dfiRequest.setSourceScopeName(sourceScopeName);
      dfiRequest.setSourceAppName(sourceAppName);
      dfiRequest.setTargetScopeName(targetScopeName);
      dfiRequest.setTargetAppName(targetAppName);
      dfiRequest.getDataTransferIndices().add(sourceDtis);
      dfiRequest.getDataTransferIndices().add(targetDtis);
  
      // request exchange service to diff the dtis
      String dxiUrl = settings.get("differencingServiceUri") + "/dxi";
      
      try
      {
        resultDtis = httpClient.post(DataTransferIndices.class, dxiUrl, dfiRequest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new ServiceProviderException(e.getMessage());
      }
    }

    return resultDtis;
  }

  public DataTransferObjects getDataTransferObjects(String scope, String id, DxoRequest dxoRequest)
      throws ServiceProviderException
  {
    DataTransferObjects resultDtos = new DataTransferObjects();
    DataTransferObjects sourceDtos = null;
    DataTransferObjects targetDtos = null;

    logger.debug("getDataTransferObjects(" + scope + ", " + id + ", dxoRequest)");

    Manifest manifest = dxoRequest.getManifest();
    DataTransferIndices dtis = dxoRequest.getDataTransferIndices();

    DataTransferObjectList resultDtoList = new DataTransferObjectList();
    resultDtos.setDataTransferObjectList(resultDtoList);
    List<DataTransferObject> resultDtoListItems = resultDtoList.getItems();
    
    List<DataTransferIndex> sourceDtiItems = new ArrayList<DataTransferIndex>();
    List<DataTransferIndex> targetDtiItems = new ArrayList<DataTransferIndex>();    
    
    try
    {
      initExchangeDefinition(scope, id);

      for (DataTransferIndex dti : dtis.getDataTransferIndexList().getItems())
      {
        DataTransferIndex sourceDti, targetDti;
        int splitIndex;
        
        switch (dti.getTransferType())
        {
        case ADD:
          sourceDtiItems.add(dti);
          break;
        case CHANGE:
          splitIndex = dti.getInternalIdentifier().indexOf(splitToken);
          
          sourceDti = new DataTransferIndex();
          sourceDti.setIdentifier(dti.getIdentifier());
          sourceDti.setHashValue(dti.getHashValue());
          sourceDti.setSortIndex(dti.getSortIndex());
          sourceDti.setInternalIdentifier(dti.getInternalIdentifier().substring(0, splitIndex));    
          sourceDti.setTransferType(dti.getTransferType());
          sourceDtiItems.add(sourceDti);   
          
          targetDti = new DataTransferIndex();
          targetDti.setIdentifier(dti.getIdentifier());
          targetDti.setHashValue(dti.getHashValue());
          targetDti.setSortIndex(dti.getSortIndex());
          targetDti.setInternalIdentifier(dti.getInternalIdentifier().substring(splitIndex+2));   
          targetDti.setTransferType(dti.getTransferType());
          targetDtiItems.add(targetDti);
          break;
        case SYNC:
          splitIndex = dti.getInternalIdentifier().indexOf(splitToken);
          
          sourceDti = new DataTransferIndex();
          sourceDti.setIdentifier(dti.getIdentifier());
          sourceDti.setHashValue(dti.getHashValue());
          sourceDti.setSortIndex(dti.getSortIndex());
          sourceDti.setInternalIdentifier(dti.getInternalIdentifier().substring(0, splitIndex));   
          sourceDti.setTransferType(dti.getTransferType());
          sourceDtiItems.add(sourceDti);
          break;
        case DELETE:
          targetDtiItems.add(dti);
          break;
        }
      }
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }
    
    String sourceDtoUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/dxo";
    String targetDtoUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/dxo";
    
    Manifest sourceManifest = null;
    Manifest targetManifest = null;
    DtoTask sourceDtoTask = null;
    DtoTask targetDtoTask = null;
    
    try
    {
      sourceManifest = JaxbUtils.clone(Manifest.class, manifest);
      sourceManifest.getGraphs().getItems().get(0).setName(sourceGraphName);
      
      targetManifest = JaxbUtils.clone(Manifest.class, manifest);
      targetManifest.getGraphs().getItems().get(0).setName(targetGraphName);
    }
    catch (Exception e)
    {
      String error = "Error cloning crossed graph: " + e.getMessage();
      logger.error(error);
      throw new ServiceProviderException(error);
    }
    
    int numOfDtoTasks = (sourceDtiItems.size() > 0 && targetDtiItems.size() > 0) ? 2 : 1;     
    ExecutorService execSvc = Executors.newFixedThreadPool(numOfDtoTasks); 
    
    if (sourceDtiItems.size() > 0)
    {
      sourceDtoTask = new DtoTask(settings, sourceDtoUrl, sourceManifest, sourceDtiItems);    
      execSvc.execute(sourceDtoTask);    
    }
    
    if (targetDtiItems.size() > 0)
    {
      targetDtoTask = new DtoTask(settings, targetDtoUrl, targetManifest, targetDtiItems);    
      execSvc.execute(targetDtoTask);    
    }
    
    execSvc.shutdown();
    
    try {
      execSvc.awaitTermination(Long.parseLong((String) settings.get("dtoTaskTimeout")), TimeUnit.SECONDS);
    } 
    catch (InterruptedException e) {
      logger.error("DTO Task Executor interrupted: " + e.getMessage());
    }
    
    if (sourceDtoTask != null)
    {
      sourceDtos = sourceDtoTask.getDataTransferObjects();
      
      if (sourceDtos != null)
      {
        sourceDtos.setScopeName(sourceScopeName);
        sourceDtos.setAppName(sourceAppName);
        List<DataTransferObject> sourceDtoListItems = sourceDtos.getDataTransferObjectList().getItems();
  
        // append add/sync DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < sourceDtoListItems.size(); i++)
        {
          DataTransferObject sourceDto = sourceDtoListItems.get(i);
          String sourceDtoIdentifier = sourceDto.getIdentifier();
  
          if (sourceDto.getClassObjects() != null)
          {
            for (DataTransferIndex sourceDti : sourceDtiItems)
            {
              if (sourceDtoIdentifier.equalsIgnoreCase(sourceDti.getIdentifier()))
              {
                TransferType transferOption = sourceDti.getTransferType();
  
                if (transferOption == null || transferOption == TransferType.SYNC)
                {
                  DataTransferObject syncDto = sourceDtoListItems.remove(i--);
                  syncDto.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);
                  resultDtoListItems.add(syncDto);
                  break;
                }
                else if (transferOption == TransferType.ADD)
                {
                  DataTransferObject addDto = sourceDtoListItems.remove(i--);
                  addDto.setTransferType(org.iringtools.dxfr.dto.TransferType.ADD);
                  resultDtoListItems.add(addDto);
                  break;
                }
              }
            }
          }
        }
      }
    }
    
    if (targetDtoTask != null)
    {
      targetDtos = targetDtoTask.getDataTransferObjects();
      
      if (targetDtos != null)
      {
        targetDtos.setScopeName(targetScopeName);
        targetDtos.setAppName(targetAppName);
        List<DataTransferObject> targetDtoListItems = targetDtos.getDataTransferObjectList().getItems();
  
        // append delete DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < targetDtoListItems.size(); i++)
        {
          DataTransferObject targetDto = targetDtoListItems.get(i);
          String targetDtoIdentifier = targetDto.getIdentifier();
  
          if (targetDto.getClassObjects() != null)
          {
            for (DataTransferIndex targetDti : targetDtiItems)
            {
              if (targetDtoIdentifier.equalsIgnoreCase(targetDti.getIdentifier()))
              {
                if (targetDti.getTransferType() == TransferType.DELETE)
                {
                  DataTransferObject deleteDto = targetDtoListItems.remove(i--);
                  deleteDto.setTransferType(org.iringtools.dxfr.dto.TransferType.DELETE);
                  resultDtoListItems.add(deleteDto);
                  break;
                }
              }
            }
          }
        }
      }
    }
    
    if (sourceDtos != null && sourceDtos.getDataTransferObjectList() != null
        && sourceDtos.getDataTransferObjectList().getItems().size() > 0 && targetDtos != null
        && targetDtos.getDataTransferObjectList() != null
        && targetDtos.getDataTransferObjectList().getItems().size() > 0)
    {
      // request exchange service to compare changed DTOs
      DfoRequest dfoRequest = new DfoRequest();
      dfoRequest.setSourceScopeName(sourceScopeName);
      dfoRequest.setSourceAppName(sourceAppName);
      dfoRequest.setTargetScopeName(targetScopeName);
      dfoRequest.setTargetAppName(targetAppName);
      dfoRequest.setManifest(manifest);
      dfoRequest.getDataTransferObjects().add(sourceDtos);
      dfoRequest.getDataTransferObjects().add(targetDtos);

      String dxoUrl = settings.get("differencingServiceUri") + "/dxo";
      DataTransferObjects dxoList;
      try
      {
        dxoList = httpClient.post(DataTransferObjects.class, dxoUrl, dfoRequest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new ServiceProviderException(e.getMessage());
      }

      // add diff DTOs to add/change/sync list
      if (dxoList != null)
        resultDtoListItems.addAll(dxoList.getDataTransferObjectList().getItems());
    }

    // order result data transfer objects as requested data transfer indices
    List<DataTransferObject> orderedDtoListItems = new ArrayList<DataTransferObject>();

    for (DataTransferIndex dti : dtis.getDataTransferIndexList().getItems())
    {
      for (DataTransferObject dto : resultDtoListItems)
      {
        if (dti.getIdentifier().equalsIgnoreCase(dto.getIdentifier()))
        {
          orderedDtoListItems.add(dto);
          break;
        }
      }
    }

    resultDtoList.setItems(orderedDtoListItems);

    return resultDtos;
  }

  public ExchangeResponse submitExchange(String scope, String id, ExchangeRequest exchangeRequest)
      throws ServiceProviderException
  {
    logger.debug("submitExchange(" + scope + ", " + id + ", exchangeRequest)");
    
    // 
    // create exchange response
    //
    ExchangeResponse exchangeResponse = new ExchangeResponse();    
    exchangeResponse.setLevel(Level.SUCCESS);
    
    XMLGregorianCalendar startTime = datatypeFactory.newXMLGregorianCalendar(new GregorianCalendar());
    exchangeResponse.setStartTime(startTime);

    //
    // check exchange request
    //
    if (exchangeRequest == null)
    {
      exchangeResponse.setLevel(Level.WARNING);
      exchangeResponse.setSummary("Exchange request is empty.");
      return exchangeResponse;
    }
        
    Manifest manifest = exchangeRequest.getManifest();
    DataTransferIndices dtis = exchangeRequest.getDataTransferIndices();

    // 
    // check data transfer indices
    //
    if (dtis == null)
    {
      exchangeResponse.setLevel(Level.ERROR);
      exchangeResponse.setSummary("No data transfer indices found.");
      return exchangeResponse;
    }
    
    // start tracking exchange progress
    String exchangeKey = scope + "/" + id;
    exchangeProgresses.put(exchangeKey, String.format(progressFormat, 0, 0, 0));

    //
    // collect ADD/CHANGE/DELETE indices
    //
    List<DataTransferIndex> dxIndices = new ArrayList<DataTransferIndex>();
    
    for (DataTransferIndex dxi : dtis.getDataTransferIndexList().getItems())
    {
      TransferType transferType = dxi.getTransferType();
      
      if (transferType != TransferType.SYNC)
      {
        dxIndices.add(dxi);        
      }
    }    

    //
    // make sure there are items to exchange
    //
    if (dxIndices.size() == 0)
    {
      exchangeResponse.setLevel(Level.ERROR);
      exchangeResponse.setSummary("No updated/deleted items found.");
      return exchangeResponse;
    }

    //
    // create directory for logging the exchange
    //
    String path = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + scope + "/" + id;
    File dirPath = new File(path);

    if (!dirPath.exists())
    {
      dirPath.mkdirs();
    }
    
    String exchangeFile = path + "/" + startTime.toString().replace(":", ".");    
    initExchangeDefinition(scope, id);

    //
    // add exchange definition info to exchange response
    //
    exchangeResponse.setSenderUri(sourceUri);
    exchangeResponse.setSenderScope(sourceScopeName);
    exchangeResponse.setSenderApp(sourceAppName);
    exchangeResponse.setSenderGraph(sourceGraphName);
    exchangeResponse.setReceiverUri(targetUri);
    exchangeResponse.setReceiverScope(targetScopeName);
    exchangeResponse.setReceiverApp(targetAppName);
    exchangeResponse.setReceiverGraph(targetGraphName);

    //
    // prepare source and target endpoint
    //
    String targetAppUrl = targetUri + "/" + targetScopeName + "/" + targetAppName;
    String targetGraphUrl = targetAppUrl + "/" + targetGraphName;
    String sourceGraphUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName;
    String sourceDtoUrl = sourceGraphUrl + "/dxo";
    
    //
    // create pool DTOs
    //
    int dxIndicesSize = dxIndices.size();
    
    // if pool size is not set for specific data exchange, then use the default one
    if (exchangePoolSize == null || exchangePoolSize == 0)
    {
      exchangePoolSize = Integer.parseInt((String) settings.get("poolSize"));
    }
    
    int poolSize = Math.min(exchangePoolSize, dxIndicesSize);
          
    exchangeResponse.setPoolSize(poolSize);
    exchangeResponse.setItemCount(dxIndicesSize);
    
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
            
      //
      // create deleted DTOs and collect add/change DTIs from source
      //
      for (DataTransferIndex poolDtiItem : poolDtiItems)
      {
        if (poolDtiItem.getTransferType() == TransferType.DELETE)
        {
          DataTransferObject deletedDto = new DataTransferObject();
          deletedDto.setIdentifier(poolDtiItem.getIdentifier());
          deletedDto.setTransferType(org.iringtools.dxfr.dto.TransferType.DELETE);
          poolDtoListItems.add(deletedDto);
        }
        else
        {
          if (poolDtiItem.getTransferType() == TransferType.CHANGE)
          {
            int splitIndex = poolDtiItem.getInternalIdentifier().indexOf(splitToken);
            poolDtiItem.setInternalIdentifier(poolDtiItem.getInternalIdentifier().substring(0, splitIndex));
          }
          
          sourceDtiItems.add(poolDtiItem);
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
        manifest.getGraphs().getItems().get(0).setName(sourceGraphName);
        
        logger.debug("Requesting source DTOs from [" + sourceDtoUrl + "]");
        logger.debug(JaxbUtils.toXml(sourceDtosRequest, false));
        
        sourceDtos = httpClient.post(DataTransferObjects.class, sourceDtoUrl, sourceDtosRequest);
        
        logger.debug("Source DTOs response: ");
        logger.debug(JaxbUtils.toXml(sourceDtos, false));
        
        sourceDtosRequest = null;
      }
      catch (Exception e)
      {
        logger.error(e.getMessage());
        throw new ServiceProviderException(e.getMessage());
      }

      //
      // add add/change DTOs to pool
      //
      if (sourceDtos != null && sourceDtos.getDataTransferObjectList() != null)
      {
        poolDtoListItems.addAll(sourceDtos.getDataTransferObjectList().getItems());
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
          
          logger.info("Processing pool [" + poolRange + "] of [" + dxIndices.size() + "]...");
          exchangeProgresses.put(exchangeKey, String.format(progressFormat, i, (i+actualPoolSize), dxIndices.size()));
          
          logger.debug("Sending pool DTOs to [" + targetUrl + "]");
          logger.debug(JaxbUtils.toXml(poolDtos, false));
          
          poolResponse = httpClient.post(Response.class, targetUrl, poolDtos, MediaType.TEXT_PLAIN);
          
          logger.debug("Pool DTOs exchange result:");
          logger.debug(JaxbUtils.toXml(poolResponse, false));
          
          logger.info("Pool [" + poolRange + "] completed.");          
          
          // free up resources
          poolDtos = null;  
          sourceDtos = null;
          poolDtiItems = null;
        }
        catch (Exception e)
        {
          logger.error(e.getMessage());
          throw new ServiceProviderException(e.getMessage());
        }
          
        if (poolResponse != null)
        {
          try 
          {
            // write pool response to disk
            String poolResponseFile = exchangeFile + POOL_PREFIX + (i+1) + "-" + (i+actualPoolSize) + ".xml";
            JaxbUtils.write(poolResponse, poolResponseFile, true);
          }
          catch (Exception e) 
          {
            logger.error("Error writing pool response to disk: " + e);
          }
  
          // update level as necessary
          if (exchangeResponse.getLevel().ordinal() < poolResponse.getLevel().ordinal())
          {
            exchangeResponse.setLevel(poolResponse.getLevel());
          }
          
          poolResponse = null;
        }
      }
    }

    if (exchangeResponse.getLevel() == Level.ERROR)
    {          
      String message = "Exchange completed with error.";
      exchangeResponse.setSummary(message);
    }
    else if (exchangeResponse.getLevel() == Level.WARNING)
    {
      String message = "Exchange completed with warning.";
      exchangeResponse.setSummary(message);
    }
    else if (exchangeResponse.getLevel() == Level.SUCCESS)
    {
      String message = "Exchange completed succesfully.";
      exchangeResponse.setSummary(message);
    }

    XMLGregorianCalendar endTime = datatypeFactory.newXMLGregorianCalendar(new GregorianCalendar());
    exchangeResponse.setEndTime(endTime);

    // write exchange response to file system    
    try
    {
      JaxbUtils.write(exchangeResponse, exchangeFile + ".xml", false);
      List<String> exchangeLogs = IOUtils.getFiles(path);
      
      /* if number of log files exceed the limit, 
       * remove the oldest one and its pools
       */
      
      for (int i=0; i < exchangeLogs.size(); i++)
      {
        if (exchangeLogs.get(i).contains(POOL_PREFIX))
        {
          exchangeLogs.remove(i--);
        }
      }
      
      Collections.sort(exchangeLogs);

      while (exchangeLogs.size() > Integer.valueOf((String) settings.get("numOfExchangeLogFiles")))
      {
        final String filePrefix = (exchangeLogs.get(0).replace(".xml", ""));
        
        FileFilter fileFilter = new FileFilter() 
        {
          public boolean accept(File file) 
          {
            return file.getName().startsWith(filePrefix);
          }
        };
          
        for (File file : new File(path).listFiles(fileFilter))
        {
          file.delete();
        }
        
        exchangeLogs.remove(0);
      }
    }
    catch (Exception e)
    {
      String message = "Error writing exchange response to disk: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }

    return exchangeResponse;
  }
  
  public String getProgress(String scope, String id) throws ServiceProviderException
  {
    String exchangeKey = scope + "/" + id;
    
    if (exchangeProgresses.containsKey(exchangeKey))
    {
      return exchangeProgresses.get(exchangeKey);
    }
    
    throw new ServiceProviderException("Exchange [" + exchangeKey + "] not found or started.");
  }

  private void initExchangeDefinition(String scope, String id) throws ServiceProviderException
  {
    String directoryServiceUrl = settings.get("directoryServiceUri") + "/" + scope + "/exchanges/" + id;
    ExchangeDefinition xdef;

    try
    {
      xdef = httpClient.get(ExchangeDefinition.class, directoryServiceUrl);
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

    sourceUri = xdef.getSourceUri();
    sourceScopeName = xdef.getSourceScopeName();
    sourceAppName = xdef.getSourceAppName();
    sourceGraphName = xdef.getSourceGraphName();

    targetUri = xdef.getTargetUri();
    targetScopeName = xdef.getTargetScopeName();
    targetAppName = xdef.getTargetAppName();
    targetGraphName = xdef.getTargetGraphName();
    
    hashAlgorithm = xdef.getHashAlgorithm();
    exchangePoolSize = xdef.getPoolSize();
  }

  public String md5Hash(DataTransferObject dataTransferObject)
  {
    StringBuilder values = new StringBuilder();

    List<ClassObject> classObjects = dataTransferObject.getClassObjects().getItems();
    for (ClassObject classObject : classObjects)
    {
      List<TemplateObject> templateObjects = classObject.getTemplateObjects().getItems();
      for (TemplateObject templateObject : templateObjects)
      {
        List<RoleObject> roleObjects = templateObject.getRoleObjects().getItems();
        for (RoleObject roleObject : roleObjects)
        {
          RoleType roleType = roleObject.getType();

          if (roleType == null
              || // bug in v2.0 of c# service
              roleType == RoleType.PROPERTY || roleType == RoleType.OBJECT_PROPERTY
              || roleType == RoleType.DATA_PROPERTY || roleType == RoleType.FIXED_VALUE
              || (roleType == RoleType.REFERENCE && roleObject.getRelatedClassId() != null && // self-join
                  roleObject.getValue() != null && !roleObject.getValue().startsWith("#")))
          {
            String value = roleObject.getValue();

            if (value != null)
              values.append(value);
          }
        }
      }
    }

    return DigestUtils.md5Hex(values.toString());
  }

  private Manifest createCrossedManifest() throws ServiceProviderException
  {
    Manifest crossedManifest = new Manifest();
    
    String sourceManifestUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/manifest";
    String targetManifestUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/manifest";
    
    ExecutorService execSvc = Executors.newFixedThreadPool(2); 
    
    ManifestTask sourceManifestTask = new ManifestTask(settings, sourceManifestUrl);    
    execSvc.execute(sourceManifestTask);    
    
    ManifestTask targetManifestTask = new ManifestTask(settings, targetManifestUrl);    
    execSvc.execute(targetManifestTask);    
    
    execSvc.shutdown();
    
    try {
      execSvc.awaitTermination(Long.parseLong((String) settings.get("manifestTaskTimeout")), TimeUnit.SECONDS);
    } 
    catch (InterruptedException e) {
      logger.error("Manifest Task Executor interrupted: " + e.getMessage());
    }

    Manifest sourceManifest = sourceManifestTask.getManifest();
    Manifest targetManifest = targetManifestTask.getManifest();
    
    if (targetManifest == null || targetManifest.getGraphs().getItems().size() == 0)
      return null;

    Graph sourceGraph = getGraph(sourceManifest, sourceGraphName);
    Graph targetGraph = getGraph(targetManifest, targetGraphName);

    if (sourceGraph != null && sourceGraph.getClassTemplatesList() != null && 
        targetGraph != null && targetGraph.getClassTemplatesList() != null)
    {
      Graphs crossGraphs = new Graphs();
      crossGraphs.getItems().add(targetGraph);
      crossedManifest.setGraphs(crossGraphs);
      
      List<ClassTemplates> sourceClassTemplatesList = sourceGraph.getClassTemplatesList().getItems();
      List<ClassTemplates> targetClassTemplatesList = targetGraph.getClassTemplatesList().getItems();

      for (int i = 0; i < targetClassTemplatesList.size(); i++)
      {
        org.iringtools.dxfr.manifest.Class targetClass = targetClassTemplatesList.get(i).getClazz();
        ClassTemplates sourceClassTemplates = getClassTemplates(sourceClassTemplatesList, targetClass.getId());

        if (sourceClassTemplates != null && sourceClassTemplates.getTemplates() != null)
        {
          List<Template> targetTemplates = targetClassTemplatesList.get(i).getTemplates().getItems();
          List<Template> sourceTemplates = sourceClassTemplates.getTemplates().getItems();

          for (int j = 0; j < targetTemplates.size(); j++)
          {
            Template targetTemplate = targetTemplates.get(j);
            Template sourceTemplate = getTemplate(sourceTemplates, targetTemplate.getId());

            if (sourceTemplate == null)
            {
              if (targetTemplate.getTransferOption() == TransferOption.REQUIRED)
              {
                throw new ServiceProviderException("Required template [" + targetTemplate.getId() + "] not found");
              }
              else
              {
                targetTemplates.remove(j--);
              }
            }
            else if (targetTemplate.getRoles() != null && sourceTemplate.getRoles() != null)
            {
              List<Role> targetRoles = targetTemplate.getRoles().getItems();
              List<Role> sourceRoles = sourceTemplate.getRoles().getItems();

              for (int k = 0; k < targetRoles.size(); k++)
              {
                Role sourceRole = getRole(sourceRoles, targetRoles.get(k).getId());

                if (sourceRole == null)
                {
                  targetRoles.remove(k--);
                }
              }
            }
          }
        }
        else
        {
          targetClassTemplatesList.remove(i--);
        }
      }
    }

    // add source and target value-list maps
    ValueListMaps valueListMaps = new ValueListMaps();
    
    if (sourceManifest.getValueListMaps() != null) {
      valueListMaps.getItems().addAll(sourceManifest.getValueListMaps().getItems());
    }
    
    if (targetManifest.getValueListMaps() != null) {
      valueListMaps.getItems().addAll(targetManifest.getValueListMaps().getItems());
    }
    
    crossedManifest.setValueListMaps(valueListMaps);
    
    return crossedManifest;
  }
  
  private Graph getGraph(Manifest manifest, String graphName)
  {
    for (Graph graph : manifest.getGraphs().getItems())
    {
      if (graph.getName().equalsIgnoreCase(graphName))
        return graph;
    }
    
    return null;
  }

  private ClassTemplates getClassTemplates(List<ClassTemplates> classTemplatesList, String classId)
  {
    for (ClassTemplates classTemplates : classTemplatesList)
    {
      org.iringtools.dxfr.manifest.Class clazz = classTemplates.getClazz();

      if (clazz.getId().equalsIgnoreCase(classId))
      {
        return classTemplates;
      }
    }

    return null;
  }

  private Template getTemplate(List<Template> templates, String templateId)
  {
    for (Template template : templates)
    {
      if (template.getId().equals(templateId))
        return template;
    }

    return null;
  }

  private Role getRole(List<Role> roles, String roleId)
  {
    for (Role role : roles)
    {
      if (role.getId().equals(roleId))
        return role;
    }

    return null;
  }
}
