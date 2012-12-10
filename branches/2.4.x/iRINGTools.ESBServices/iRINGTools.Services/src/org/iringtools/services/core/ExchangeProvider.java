package org.iringtools.services.core;

import java.io.File;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;

import org.apache.commons.codec.digest.DigestUtils;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
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
  private static final String splitToken = "->";
  
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
  //private Integer exchangePoolSize;

  public ExchangeProvider(Map<String, Object> settings) throws ServiceProviderException
  {
    this.settings = settings;
    this.httpClient = new HttpClient();
    HttpUtils.addHttpHeaders(settings, httpClient);
  }

  public Directory getDirectory() throws ServiceProviderException
  {
    logger.debug("getDirectory()");

    String path = settings.get("baseDirectory") + "/WEB-INF/data/directory.xml";
    
    try
    {
      if (IOUtils.fileExists(path))
      {
        return JaxbUtils.read(Directory.class, path);
      }
      
      Directory directory = new Directory();
      JaxbUtils.write(directory, path, false);      
      return directory;
    }
    catch (Exception e)
    {
      String message = "Error getting exchange definitions: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }
  
  public ExchangeDefinition getExchangeDefinition(String scope, String id) throws ServiceProviderException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-" + scope + "-" + id + ".xml";
    try
    {
      return JaxbUtils.read(ExchangeDefinition.class, path);
    }
    catch (Exception e)
    {
      String message = "Error getting exchange definition of [" + scope + "." + id + "]: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }

  public DataFilter getDataFilter(String scope, String id) throws ServiceProviderException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/Filter-" + scope + "-" + id + ".xml";
    File file = new File(path);
    
    if (file.exists())
    {
      try
      {
        return JaxbUtils.read(DataFilter.class, path);
      }
      catch (Exception e)
      {
        String message = "Error getting Data Filter of [" + scope + "." + id + "]: " + e;
        logger.error(message);
        throw new ServiceProviderException(message);
      }
    }
    else
    {
      return new DataFilter();
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
 //   DataFilter initialFilter = getDataFilter(scope, id);

    String sourceDtiUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName
        + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;

    String targetDtiUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName
        + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;
    
    DxiRequest sourceDxiRequest = new DxiRequest();
    DxiRequest targetDxiRequest = new DxiRequest();
    
    try
    {   /*DataFilterInitial dFI = new DataFilterInitial();
       DataFilter df = dFI.AppendFilter(dxiRequest.getDataFilter(), initialFilter);*/
      Manifest sourceManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
      sourceManifest.getGraphs().getItems().get(0).setName(sourceGraphName);
      sourceDxiRequest.setManifest(sourceManifest);
      sourceDxiRequest.setDataFilter(dxiRequest.getDataFilter());
      
      Manifest targetManifest = JaxbUtils.clone(Manifest.class, dxiRequest.getManifest());
      targetManifest.getGraphs().getItems().get(0).setName(targetGraphName);
      targetDxiRequest.setManifest(targetManifest);
      targetDxiRequest.setDataFilter(dxiRequest.getDataFilter());
   /*   String path = "C:/Bug-iring-2.4/iRINGTools.ESBServices/iRINGTools.Services/WebContent/WEB-INF/data/Filter-25509-3.xml";

		JaxbUtils.write(dxiRequest.getDataFilter(),path, false);*/
    }
    catch (Exception e)
    {
      String error = "Error cloning crossed graph: " + e.getMessage();
      logger.error(error);
      throw new ServiceProviderException(error);
    }
    
    ExecutorService executor = Executors.newFixedThreadPool(2); 
    
    DtiTask sourceDtiTask = new DtiTask(settings, sourceDtiUrl, sourceDxiRequest);    
    executor.execute(sourceDtiTask);    
    
    DtiTask targetDtiTask = new DtiTask(settings, targetDtiUrl, targetDxiRequest);    
    executor.execute(targetDtiTask);    
    
    executor.shutdown();
    
    try {
      executor.awaitTermination(Long.parseLong((String) settings.get("dtiTaskTimeout")), TimeUnit.SECONDS);
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
        if (dti.getDuplicateCount() != null && dti.getDuplicateCount() > 0)
        {
          logger.warn("DTI [" + dti.getIdentifier() + "] contains [" + dti.getDuplicateCount() + "] duplicates.");
          continue;
        }
        
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
    ExecutorService executor = Executors.newFixedThreadPool(numOfDtoTasks); 
    
    if (sourceDtiItems.size() > 0)
    {
      sourceDtoTask = new DtoTask(settings, sourceDtoUrl, sourceManifest, sourceDtiItems);    
      executor.execute(sourceDtoTask);    
    }
    
    if (targetDtiItems.size() > 0)
    {
      targetDtoTask = new DtoTask(settings, targetDtoUrl, targetManifest, targetDtiItems);    
      executor.execute(targetDtoTask);    
    }
    
    executor.shutdown();
    
    try {
      executor.awaitTermination(Long.parseLong((String) settings.get("dtoTaskTimeout")), TimeUnit.SECONDS);
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
  
  public DataTransferObjects getDataTransferObjectsFiltered(String scope, String id, int start, int limit, boolean sync, boolean add, boolean change, boolean delete, DataFilter filter)
		  throws ServiceProviderException
  {
	  logger.debug("getDataTransferObjectsFiltered(" + scope + ", " + id + ", " + start + ", " + limit + ", " + sync + ", " + add + ", " + change + ", " + delete + ", " + ", filter)");
	  
	  Manifest manifest = null;
	  DataTransferIndices dtis = null;
	  DxiRequest dxiRequest = new DxiRequest();
      ExchangeResponse xr = new ExchangeResponse();
	  try
	  {
	      manifest = getManifest(scope, id);
	      
	      dxiRequest.setManifest(manifest);
	      dxiRequest.setDataFilter(filter);
	      dtis = getDataTransferIndices(scope, id, dxiRequest, false);
	   
	      int itemCount = 0;
	      int itemCountSync = 0;
	      int itemCountAdd = 0;
	      int itemCountChange = 0;
	      int itemCountDelete = 0;
		  DataTransferIndices actionDtis = new DataTransferIndices();
		  DataTransferIndexList actionDtiList = new DataTransferIndexList();
		  actionDtis.setDataTransferIndexList(actionDtiList);
		  List<DataTransferIndex> actionDtiListItems = actionDtiList.getItems();
		    
	      // Depending on the 'actions' we'll limit the dti's we send to the exchange request
	      for (DataTransferIndex dxi : dtis.getDataTransferIndexList().getItems())
	      {
	        TransferType transferType = dxi.getTransferType();

	        // gather counts for the exchange response
	        if (transferType == TransferType.SYNC) itemCountSync++;
	        if (transferType == TransferType.ADD) itemCountAdd++;
	        if (transferType == TransferType.CHANGE) itemCountChange++;
	        if (transferType == TransferType.DELETE) itemCountDelete++;
	        
	        if ( (transferType == TransferType.SYNC && sync) ||
	        	 (transferType == TransferType.ADD && add) ||
	        	 (transferType == TransferType.CHANGE && change) ||
	        	 (transferType == TransferType.DELETE && delete) ) 
	        {
          		itemCount++;
	        	if ((itemCount > start) && (itemCount <= start+limit) )// get just the requested page of changes
	        	{
		        	actionDtiListItems.add(dxi);        
	        	}
	        }
	      }

		  DxoRequest dxoRequest = new DxoRequest();
	      dxoRequest.setManifest(manifest);
	      dxoRequest.setDataTransferIndices(actionDtis);

	      DataTransferObjects dtos = getDataTransferObjects(scope, id, dxoRequest);
	      
	      xr.setExchangeId(id);
	      xr.setSenderUri(sourceUri);
	      xr.setSenderScope(sourceScopeName);
	      xr.setSenderApp(sourceAppName);
	      xr.setSenderGraph(sourceGraphName);
	      xr.setReceiverUri(targetUri);
	      xr.setReceiverScope(targetScopeName);
	      xr.setReceiverApp(targetAppName);
	      xr.setReceiverGraph(targetGraphName);
	      //<xs:element name="startTime" type="xs:dateTime" />
	      //<xs:element name="endTime" type="xs:dateTime" />
	      // NB the local itemCount is only the items on the page that are different, where as the xr's itemCount should reflect everything
	      xr.setItemCount(itemCountSync + itemCountDelete + itemCountChange + itemCountDelete);
	      xr.setItemCountSync(itemCountSync);
	      xr.setItemCountAdd(itemCountAdd);
	      xr.setItemCountChange(itemCountChange);
	      xr.setItemCountDelete(itemCountDelete);
	 	  xr.setSummary("Page of differences.");

	 	  dtos.setSummary(xr);
	      dtos.setVersion(id);
	      dtos.setSenderAppName(sourceAppName);
	      dtos.setSenderScopeName(sourceScopeName);
	      dtos.setAppName(targetAppName);
	      dtos.setScopeName(targetScopeName);
	      
		  return dtos;
	  }
	  catch (Exception e)
	  {
		  throw new ServiceProviderException(e.getMessage());
	  }
  }

  public Response getDifferencesSummary(String scope, String id, DataFilter filter)
  {
	  logger.debug("getDifferencesSummary(" + scope + ", " + id + ", filter)");

	  Manifest manifest = null;
	  DataTransferIndices dtis = null;
	  DxiRequest dxiRequest = new DxiRequest();
	    
	  try
	  {
	      manifest = getManifest(scope, id);
	      
	      dxiRequest.setManifest(manifest);
	      dxiRequest.setDataFilter(filter);
	      dtis = getDataTransferIndices(scope, id, dxiRequest, false);

	      int iCountSync = 0;
	      int iCountAdd = 0;
	      int iCountChange = 0;
	      int iCountDelete = 0;
	      for (DataTransferIndex dxi : dtis.getDataTransferIndexList().getItems())
	      {
	    	  TransferType transferType = dxi.getTransferType();
	    	  if (transferType == TransferType.ADD)
	    	  {
	    		  iCountAdd++;
	          }
	          else if (transferType == TransferType.CHANGE) 
	          {
	        	  iCountChange++;
	          }
	          else if (transferType == TransferType.DELETE) 
	          {
	        	  iCountDelete++;
	          }
	          else
	          {
	        	  iCountSync++;
	          }
	      }    

	      ExchangeResponse xRes = new ExchangeResponse();
	      xRes.setExchangeId(id);
	      xRes.setSenderUri(sourceUri);
	      xRes.setSenderScope(sourceScopeName);
	      xRes.setSenderApp(sourceAppName);
	      xRes.setSenderGraph(sourceGraphName);
	      xRes.setReceiverUri(targetUri);
	      xRes.setReceiverScope(targetScopeName);
	      xRes.setReceiverApp(targetAppName);
	      xRes.setReceiverGraph(targetGraphName);
	      //<xs:element name="startTime" type="xs:dateTime" />
	      //<xs:element name="endTime" type="xs:dateTime" />
	      xRes.setLevel(Level.WARNING);
	      xRes.setItemCount(iCountSync + iCountAdd + iCountChange + iCountDelete);
	      xRes.setItemCountSync(iCountSync);
	      xRes.setItemCountAdd(iCountAdd);
	      xRes.setItemCountChange(iCountChange);
	      xRes.setItemCountDelete(iCountDelete);
	 	  xRes.setSummary("Difference Summary only, this was not a data exchange request.");
	      return Response.ok().entity(xRes).build();
	  }
	  catch (Exception e)
	  {
		  return Response.serverError().entity(e.getMessage()).build();
	  }
  }
  
  public Response submitExchange(String scope, String id, ExchangeRequest xReq)
  {
    logger.debug("submitExchange(" + scope + ", " + id + ", exchangeRequest)");
    
    String directoryServiceUrl = settings.get("directoryServiceUri") + "/" + scope + "/exchanges/" + id;
    ExchangeDefinition xDef;
    
    try
    {
      xDef = httpClient.get(ExchangeDefinition.class, directoryServiceUrl);
    }
    catch (HttpClientException e)
    {
      ExchangeResponse exchangeResponse = new ExchangeResponse();
      logger.error(e.getMessage());
      exchangeResponse.setLevel(Level.ERROR);
      exchangeResponse.setSummary(e.getMessage());
      return Response.ok().entity(exchangeResponse).build();
    }
    
    String asyncHeader = "http-header-async";
    boolean isAsync = settings.containsKey(asyncHeader) && Boolean.parseBoolean(settings.get(asyncHeader).toString());
       
    if (isAsync)
    {
      String xtoken = UUID.randomUUID().toString();
      String resultPath = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + xtoken + ".xml";
      
      ExecutorService executor = Executors.newSingleThreadExecutor();   
      ExchangeTask exchangeTask = new ExchangeTask(settings, scope, id, xReq, xDef, resultPath);    
      executor.execute(exchangeTask);
      executor.shutdown();
      
      return Response.status(Status.ACCEPTED).entity(xtoken).type(MediaType.TEXT_PLAIN).build();
    }
    else
    {
      ExecutorService executor = Executors.newSingleThreadExecutor();    
      ExchangeTask exchangeTask = new ExchangeTask(settings, scope, id, xReq, xDef, null);    
      executor.execute(exchangeTask);
      executor.shutdown();
           
      try {
        executor.awaitTermination(24, TimeUnit.HOURS);
      } 
      catch (InterruptedException e) {
        logger.error("Exchange Task Executor interrupted: " + e.getMessage());
        return Response.serverError().entity(e.getMessage()).build();
      }
      
      ExchangeResponse exchangeResponse = exchangeTask.getExchangeResponse();
      return Response.ok().entity(exchangeResponse).build();
    }
  }

  public ExchangeResponse getExchangeResult(String xtoken) throws HttpClientException
  {
    
    String transPath = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + xtoken + ".xml";
    File file = new File(transPath);
    
    if (file.exists())
    {
      try
      {
        ExchangeResponse res = JaxbUtils.read(ExchangeResponse.class, transPath);        
        file.delete();
        return res;
      }
      catch (Exception e)
      {
        throw new HttpClientException(e.getMessage());
      }
    }
    
    return null;
  }
  
  private void initExchangeDefinition(String scope, String id) throws ServiceProviderException
  {
    ExchangeDefinition xdef = getExchangeDefinition(scope, id);

    sourceUri = xdef.getSourceUri();
    sourceScopeName = xdef.getSourceScopeName();
    sourceAppName = xdef.getSourceAppName();
    sourceGraphName = xdef.getSourceGraphName();

    targetUri = xdef.getTargetUri();
    targetScopeName = xdef.getTargetScopeName();
    targetAppName = xdef.getTargetAppName();
    targetGraphName = xdef.getTargetGraphName();
    
    hashAlgorithm = xdef.getHashAlgorithm();
    //exchangePoolSize = xdef.getPoolSize();
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
    
    ExecutorService executor = Executors.newFixedThreadPool(2); 
    
    ManifestTask sourceManifestTask = new ManifestTask(settings, sourceManifestUrl);    
    executor.execute(sourceManifestTask);    
    
    ManifestTask targetManifestTask = new ManifestTask(settings, targetManifestUrl);    
    executor.execute(targetManifestTask);    
    
    executor.shutdown();
    
    try {
      executor.awaitTermination(Long.parseLong((String) settings.get("manifestTaskTimeout")), TimeUnit.SECONDS);
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
