package org.iringtools.services.core;

import java.io.File;
import java.io.FileFilter;
import java.util.ArrayList;
import java.util.Collections;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.Map;

import javax.ws.rs.core.MediaType;
import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.commons.codec.digest.DigestUtils;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Response;
import org.iringtools.data.filter.DataFilter;
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
  
  private Map<String, Object> settings;
  private static DatatypeFactory datatypeFactory = null;
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

  public ExchangeProvider(Map<String, Object> settings) throws ServiceProviderException
  {
    this.settings = settings;

    try
    {
      datatypeFactory = DatatypeFactory.newInstance();
    }
    catch (DatatypeConfigurationException e)
    {
      String message = "Error initializing data type factory: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }

    httpClient = new HttpClient();
    HttpUtils.addOAuthHeaders(settings, httpClient);
  }

  public Directory getDirectory() throws ServiceProviderException
  {
    logger.debug("getDirectory()");

    try
    {
      return httpClient.get(Directory.class, settings.get("directoryServiceUri") + "/directory");
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }
  }

  public Manifest getManifest(String scope, String id) throws ServiceProviderException
  {
    logger.debug("getManifest(" + scope + "," + id + ")");
    initExchangeDefinition(scope, id);
    return createCrossedManifest();
  }

  // get dxi without filter
  public DataTransferIndices getDataTransferIndices(String scope, String id, Manifest manifest)
      throws ServiceProviderException
  {
    DataTransferIndices dxis = null;

    logger.debug("getDataTransferIndices(" + scope + "," + id + ")");

    // init exchange definition
    initExchangeDefinition(scope, id);

    // get source dti
    String sourceDtiUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName
        + "/dxi?hashAlgorithm=" + hashAlgorithm;
    DataTransferIndices sourceDtis;

    try
    {
      manifest.getGraphs().getItems().get(0).setName(sourceGraphName);
      sourceDtis = httpClient.post(DataTransferIndices.class, sourceDtiUrl, manifest);
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

    if (sourceDtis != null)
    {
      sourceDtis.setScopeName(sourceScopeName);
      sourceDtis.setAppName(sourceAppName);
    }

    // get target dti
    String targetDtiUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName
        + "/dxi?hashAlgorithm=" + hashAlgorithm;
    DataTransferIndices targetDtis;

    try
    {
      manifest.getGraphs().getItems().get(0).setName(targetGraphName);
      targetDtis = httpClient.post(DataTransferIndices.class, targetDtiUrl, manifest);
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

    if (targetDtis != null)
    {
      targetDtis.setScopeName(targetScopeName);
      targetDtis.setAppName(targetAppName);
    }

    // create dxi request to diff source and target dti
    DfiRequest dfiRequest = new DfiRequest();
    dfiRequest.setSourceScopeName(sourceScopeName);
    dfiRequest.setSourceAppName(sourceAppName);
    dfiRequest.setTargetScopeName(targetScopeName);
    dfiRequest.setTargetAppName(targetAppName);
    dfiRequest.getDataTransferIndices().add(sourceDtis);
    dfiRequest.getDataTransferIndices().add(targetDtis);

    // request exchange service to diff the dti
    String dxiUrl = settings.get("differencingServiceUri") + "/dxi";

    try
    {
      dxis = httpClient.post(DataTransferIndices.class, dxiUrl, dfiRequest);
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

    return dxis;
  }

  // get dxi with filter
  public DataTransferIndices getDataTransferIndices(String scope, String id, String destination, DxiRequest dxiRequest)
      throws ServiceProviderException
  {
    DataTransferIndices dxis = null;

    logger.debug("getDataTransferIndices(" + scope + "," + id + ")");
    Manifest manifest = dxiRequest.getManifest();
    DataFilter dataFilter = dxiRequest.getDataFilter();

    try
    {
      initExchangeDefinition(scope, id);
    }
    catch (ServiceProviderException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

    DxiRequest adapterDxiRequest = new DxiRequest();
    adapterDxiRequest.setDataFilter(dataFilter);
    adapterDxiRequest.setManifest(manifest);

    if (destination.equalsIgnoreCase("source"))
    {
      // get source dti
      String sourceDtiUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName
          + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;
      try
      {
        manifest.getGraphs().getItems().get(0).setName(sourceGraphName);
        dxis = httpClient.post(DataTransferIndices.class, sourceDtiUrl, adapterDxiRequest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new ServiceProviderException(e.getMessage());
      }
    }
    else
    {
      String targetDtiUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName
          + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;
      try
      {
        manifest.getGraphs().getItems().get(0).setName(targetGraphName);
        dxis = httpClient.post(DataTransferIndices.class, targetDtiUrl, adapterDxiRequest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new ServiceProviderException(e.getMessage());
      }
    }

    return dxis;
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

    try
    {
      initExchangeDefinition(scope, id);
    }
    catch (ServiceProviderException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

    List<DataTransferIndex> sourceDtiListItems = new ArrayList<DataTransferIndex>();
    List<DataTransferIndex> targetDtiListItems = new ArrayList<DataTransferIndex>();

    for (DataTransferIndex dti : dtis.getDataTransferIndexList().getItems())
    {
      switch (dti.getTransferType())
      {
      case ADD:
        sourceDtiListItems.add(dti);
        break;
      case CHANGE:
        sourceDtiListItems.add(dti);
        targetDtiListItems.add(dti);
        break;
      case SYNC:
        sourceDtiListItems.add(dti);
        break;
      case DELETE:
        targetDtiListItems.add(dti);
        break;
      }
    }

    // get source DTOs
    if (sourceDtiListItems.size() > 0)
    {
      DxoRequest sourceDxoRequest = new DxoRequest();
      sourceDxoRequest.setManifest(manifest);
      DataTransferIndices sourceDtis = new DataTransferIndices();
      DataTransferIndexList sourceDtiList = new DataTransferIndexList();
      sourceDtiList.setItems(sourceDtiListItems);
      sourceDtis.setDataTransferIndexList(sourceDtiList);
      sourceDxoRequest.setDataTransferIndices(sourceDtis);

      String sourceDtoUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/dxo";
      try
      {
        manifest.getGraphs().getItems().get(0).setName(sourceGraphName);
        sourceDtos = httpClient.post(DataTransferObjects.class, sourceDtoUrl, sourceDxoRequest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new ServiceProviderException(e.getMessage());
      }

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
            for (DataTransferIndex sourceDti : sourceDtiListItems)
            {
              if (sourceDtoIdentifier.equalsIgnoreCase(sourceDti.getIdentifier()))
              {
                TransferType transferOption = sourceDti.getTransferType();

                if (transferOption == TransferType.ADD)
                {
                  DataTransferObject addDto = sourceDtoListItems.remove(i--);
                  addDto.setTransferType(org.iringtools.dxfr.dto.TransferType.ADD);
                  resultDtoListItems.add(addDto);
                  break;
                }
                else if (transferOption == TransferType.SYNC)
                {
                  DataTransferObject syncDto = sourceDtoListItems.remove(i--);
                  syncDto.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);
                  resultDtoListItems.add(syncDto);
                  break;
                }
              }
            }
          }
        }
      }
    }

    // get target DTOs
    if (targetDtiListItems.size() > 0)
    {
      DxoRequest targetDxoRequest = new DxoRequest();
      targetDxoRequest.setManifest(manifest);
      DataTransferIndices targetDtis = new DataTransferIndices();
      DataTransferIndexList targetDtiList = new DataTransferIndexList();
      targetDtiList.setItems(targetDtiListItems);
      targetDtis.setDataTransferIndexList(targetDtiList);
      targetDxoRequest.setDataTransferIndices(targetDtis);

      String targetDtoUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/dxo";
      try
      {
        manifest.getGraphs().getItems().get(0).setName(targetGraphName);
        targetDtos = httpClient.post(DataTransferObjects.class, targetDtoUrl, targetDxoRequest);
      }
      catch (HttpClientException e)
      {
        logger.error(e.getMessage());
        throw new ServiceProviderException(e.getMessage());
      }

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
            for (DataTransferIndex targetDti : targetDtiListItems)
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
        if (dti.getIdentifier().equals(dto.getIdentifier()))
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
    
    try
    {
      ExchangeResponse exchangeResponse = new ExchangeResponse();    
      exchangeResponse.setLevel(Level.SUCCESS);
      
      XMLGregorianCalendar startTime = datatypeFactory.newXMLGregorianCalendar(new GregorianCalendar());
      exchangeResponse.setStartTime(startTime);
  
      if (exchangeRequest == null)
      {
        exchangeResponse.setLevel(Level.WARNING);
        exchangeResponse.setSummary("Exchange request is empty.");
        return exchangeResponse;
      }
  
      // create exchange log directory
      String path = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + scope + "/" + id;
      File dirPath = new File(path);
  
      if (!dirPath.exists())
      {
        dirPath.mkdirs();
      }
      
      Manifest manifest = exchangeRequest.getManifest();
      DataTransferIndices dtis = exchangeRequest.getDataTransferIndices();
  
      if (dtis == null)
        return null;
  
      String exchangeFile = path + "/" + startTime.toString().replace(":", ".");
      List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
  
      if (dtiList.size() == 0)
        return null;
  
      initExchangeDefinition(scope, id);
  
      // add exchange definition info to exchange response
      exchangeResponse.setSenderUri(sourceUri);
      exchangeResponse.setSenderScope(sourceScopeName);
      exchangeResponse.setSenderApp(sourceAppName);
      exchangeResponse.setSenderGraph(sourceGraphName);
      exchangeResponse.setReceiverUri(targetUri);
      exchangeResponse.setReceiverScope(targetScopeName);
      exchangeResponse.setReceiverApp(targetAppName);
      exchangeResponse.setReceiverGraph(targetGraphName);
  
      // get target application uri
      String targetAppUrl = targetUri + "/" + targetScopeName + "/" + targetAppName;
      String targetGraphUrl = targetAppUrl + "/" + targetGraphName;
      String sourceGraphUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName;
  
      // create a pool DTOs to send to target endpoint
      int dtiSize = dtiList.size();
      int presetPoolSize = Integer.parseInt((String) settings.get("poolSize"));
      int poolSize = Math.min(presetPoolSize, dtiSize);
      int postedItemCount = 0;
            
      exchangeResponse.setPoolSize(poolSize);
      exchangeResponse.setItemCount(dtiSize);
      
      for (int i = 0; i < dtiSize; i += poolSize)
      {
        int actualPoolSize = (dtiSize > (i + poolSize)) ? poolSize : dtiSize - i;
        List<DataTransferIndex> poolDtiListItems = dtiList.subList(i, i + actualPoolSize);
  
        List<DataTransferIndex> sourcePoolDtiList = new ArrayList<DataTransferIndex>();
        List<DataTransferIndex> syncDtiList = new ArrayList<DataTransferIndex>();
        List<DataTransferIndex> deleteDtiList = new ArrayList<DataTransferIndex>();
  
        for (DataTransferIndex poolDti : poolDtiListItems)
        {
          switch (poolDti.getTransferType())
          {
          case SYNC:
            syncDtiList.add(poolDti);
            break;
          case DELETE:
            deleteDtiList.add(poolDti);
            break;
          default:
            sourcePoolDtiList.add(poolDti);
            break;
          }
        }
  
        // only include SYNC DTIs if the DTOs were not reviewed
        if (!exchangeRequest.getReviewed())
          sourcePoolDtiList.addAll(syncDtiList);
  
        DataTransferObjects sourceDtos = null;
  
        if (sourcePoolDtiList.size() > 0)
        {
          // request source DTOs
          DxoRequest poolDxoRequest = new DxoRequest();
          poolDxoRequest.setManifest(manifest);
          DataTransferIndices poolDataTransferIndices = new DataTransferIndices();
          poolDxoRequest.setDataTransferIndices(poolDataTransferIndices);
          DataTransferIndexList poolDtiList = new DataTransferIndexList();
          poolDataTransferIndices.setDataTransferIndexList(poolDtiList);
          poolDtiList.setItems(sourcePoolDtiList);
  
          String sourceDtoUrl = sourceGraphUrl + "/dxo";
          try
          {
            manifest.getGraphs().getItems().get(0).setName(sourceGraphName);
            sourceDtos = httpClient.post(DataTransferObjects.class, sourceDtoUrl, poolDxoRequest);
            poolDxoRequest = null;
          }
          catch (HttpClientException e)
          {
            logger.error(e.getMessage());
            throw new ServiceProviderException(e.getMessage());
          }
          
          List<DataTransferObject> sourceDtoListItems = sourceDtos.getDataTransferObjectList().getItems();
  
          // set transfer type for each DTO and remove SYNC ones if DTO grid is reviewed
          for (int j = 0; j < sourceDtoListItems.size(); j++)
          {
            DataTransferObject sourceDto = sourceDtoListItems.get(j);
            String identifier = sourceDto.getIdentifier();
  
            if (sourceDto.getClassObjects() != null)
            {
              for (DataTransferIndex dti : poolDtiListItems)
              {
                if (dti.getIdentifier().equals(identifier))
                {
                  if (exchangeRequest.getReviewed())
                  {
                    sourcePoolDtiList.remove(dti);
                    
                    if (dti.getTransferType() == TransferType.SYNC)
                    {
                      sourceDtoListItems.remove(j--); // exclude SYNC DTOs
                    }
                    else
                    {
                      sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.valueOf(dti.getTransferType().toString()));
                    }
                  }
                  else
                  {
                    sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.valueOf(dti.getTransferType().toString()));
                  }
  
                  break;
                }
              }
            }
          }
        }
  
        DataTransferObjects poolDtos = new DataTransferObjects();
        DataTransferObjectList poolDtosList = new DataTransferObjectList();
        poolDtos.setDataTransferObjectList(poolDtosList);
  
        List<DataTransferObject> poolDtoListItems = new ArrayList<DataTransferObject>();
        poolDtosList.setItems(poolDtoListItems);
  
        if (sourceDtos != null && sourceDtos.getDataTransferObjectList() != null)
        {
          poolDtoListItems.addAll(sourceDtos.getDataTransferObjectList().getItems());
        }
  
        // create identifiers for deleted DTOs
        for (DataTransferIndex deleteDti : deleteDtiList)
        {
          DataTransferObject deleteDto = new DataTransferObject();
          deleteDto.setIdentifier(deleteDti.getIdentifier());
          deleteDto.setTransferType(org.iringtools.dxfr.dto.TransferType.DELETE);
          poolDtoListItems.add(deleteDto);
        }
  
        // post add/change/delete DTOs to receiver's endpoint
        if (poolDtoListItems.size() > 0)
        {
          Response poolResponse = null;
          
          try
          {
            poolResponse = httpClient.post(Response.class, targetGraphUrl + "?format=stream", poolDtos, MediaType.TEXT_PLAIN);
            postedItemCount += poolDtoListItems.size();
            
            // free up resources
            poolDtos = null;  
            sourceDtos = null;
            poolDtiListItems = null;
          }
          catch (HttpClientException e)
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
  
      if (postedItemCount == 0)
      {
        exchangeResponse.setLevel(Level.WARNING);
        String message = "No Add/Change/Delete items in the exchange.";
        exchangeResponse.setSummary(message);
      }
      else 
      {
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
      }

      return exchangeResponse;
    }
    finally {
      System.gc();
    }
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
    
    // get source manifest
    String sourceManifestUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/manifest";
    Manifest sourceManifest;

    try
    {
      sourceManifest = httpClient.get(Manifest.class, sourceManifestUrl);
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

    if (sourceManifest == null || sourceManifest.getGraphs().getItems().size() == 0)
      return null;

    // get target manifest
    String targetManifestUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/manifest";
    Manifest targetManifest;

    try
    {
      targetManifest = httpClient.get(Manifest.class, targetManifestUrl);
    }
    catch (HttpClientException e)
    {
      logger.error(e.getMessage());
      throw new ServiceProviderException(e.getMessage());
    }

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
    valueListMaps.getItems().addAll(sourceManifest.getValueListMaps().getItems());
    valueListMaps.getItems().addAll(targetManifest.getValueListMaps().getItems());
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
