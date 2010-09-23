package org.iringtools.services;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Hashtable;
import java.util.List;
import javax.servlet.ServletContext;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.xml.bind.JAXBException;
import org.apache.log4j.Logger;
import org.iringtools.adapter.dto.DataTransferObjectComparator;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dti.DataTransferIndices.DataTransferIndex;
import org.iringtools.adapter.dto.DataTransferObject;
import org.iringtools.adapter.dto.ClassObject;
import org.iringtools.adapter.dto.DataTransferObject.ClassObjects;
import org.iringtools.adapter.dto.TransferType;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.adapter.manifest.Manifest;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.exchange.DtiSubmission;
import org.iringtools.exchange.DxiRequest;
import org.iringtools.exchange.DxoRequest;
import org.iringtools.exchange.DxRequest;
import org.iringtools.exchange.Identifiers;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusLevel;
import org.iringtools.utility.NetUtil;

@Path("/")
@Produces("application/xml")
@Consumes("application/xml")
public class ESBService
{
  private static final Logger logger = Logger.getLogger(ESBService.class);
  
  @Context
  private ServletContext context;
  private Hashtable<String, String> settings;

  public ESBService()
  {
    settings = new Hashtable<String, String>();
  }

  @GET
  @Path("/exchanges")
  public Directory getExchanges() throws JAXBException, IOException
  {
    Directory directory = null;
    
    try 
    {
      init();
  
      // request directory service to get exchange definitions
      String diffServiceUrl = settings.get("directoryServiceUri") + "/exchanges";
      directory = NetUtil.get(Directory.class, diffServiceUrl);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }
    
    return directory;
  }

  @GET
  @Path("/dti/{exchangeId}")
  public DataTransferIndices getDtiList(@PathParam("exchangeId") String exchangeId) 
  {
    DataTransferIndices resultDtis = null;
    
    try
    {
      init();
  
      // get exchange definition
      ExchangeDefinition xdef = getExchangeDefinition(exchangeId);
  
      // get target manifest
      String targetManifestUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName()
          + "/manifest";
      Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);    
      if (targetManifest == null || targetManifest.getGraphs().getGraph().size() == 0) return null;
  
      // create data exchange request
      DxRequest dxRequest = new DxRequest();
      dxRequest.setManifest(targetManifest);
      dxRequest.setHashAlgorithm(xdef.getHashAlgorithm());
  
      // get source dti
      String sourceUrl = xdef.getSourceUri() + "/" + xdef.getSourceAppScope() + "/" + xdef.getSourceAppName() + "/"
          + xdef.getSourceGraphName() + "/dti";
      DataTransferIndices sourceDtis = NetUtil.post(DataTransferIndices.class, sourceUrl, dxRequest);
  
      // get target dti
      String targetUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName() + "/"
          + xdef.getTargetGraphName() + "/dti";
      DataTransferIndices targetDtis = NetUtil.post(DataTransferIndices.class, targetUrl, dxRequest);
  
      // create dxi request to diff source and target dti
      DxiRequest dxiRequest = new DxiRequest();
      dxiRequest.setSourceDataTransferIndicies(sourceDtis);
      dxiRequest.setTargetDataTransferIndicies(targetDtis);
  
      // request exchange service to diff the dti
      String diffServiceUrl = settings.get("differencingServiceUri") + "/dxi";
      resultDtis = NetUtil.post(DataTransferIndices.class, diffServiceUrl, dxiRequest);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }
    
    return resultDtis;
  }

  @POST
  @Path("/dto/{exchangeId}")
  public DataTransferObjects getDtoList(@PathParam("exchangeId") String exchangeId, DataTransferIndices dtis)
  {
    DataTransferObjects resultDtos = null;
    
    try
    {
      init();
  
      DataTransferObjects sourceDtos = null;
      DataTransferObjects targetDtos = null;
      
      resultDtos = new DataTransferObjects();
      List<DataTransferObject> resultDtoList = resultDtos.getDataTransferObject();
      
      // get exchange definition
      ExchangeDefinition xdef = getExchangeDefinition(exchangeId);
  
      // get target manifest
      String targetManifestUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName() + "/manifest";
      Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);    
      if (targetManifest == null || targetManifest.getGraphs().getGraph().size() == 0) return null;
  
      // store add/change-sync/delete identifiers in different lists
      List<String> addIdentifierList = new ArrayList<String>();
      List<String> changeIdentifierList = new ArrayList<String>();
      List<String> syncIdentifierList = new ArrayList<String>();
      List<String> deleteIdentifierList = new ArrayList<String>();
  
      for (DataTransferIndex dti : dtis.getDataTransferIndex())
      {
        String identifier = dti.getIdentifier();
  
        switch (dti.getTransferType())
        {
        case ADD:
          addIdentifierList.add(identifier);
          break;
        case CHANGE:
          changeIdentifierList.add(identifier);
          break;
        case SYNC:
          syncIdentifierList.add(identifier);
          break;
        case DELETE:
          deleteIdentifierList.add(identifier);
          break;
        }
      }
  
      // get add/change/sync DTOs from source endpoint
      Identifiers sourceIdentifiers = new Identifiers();
      sourceIdentifiers.getIdentifier().addAll(addIdentifierList);
      sourceIdentifiers.getIdentifier().addAll(changeIdentifierList);
      sourceIdentifiers.getIdentifier().addAll(syncIdentifierList);
  
      if (sourceIdentifiers.getIdentifier().size() > 0)
      {
        DxRequest sourceDtosRequest = new DxRequest();
        sourceDtosRequest.setManifest(targetManifest);
        sourceDtosRequest.setIdentifiers(sourceIdentifiers);
  
        String sourceUrl = xdef.getSourceUri() + "/" + xdef.getSourceAppScope() + "/" + xdef.getSourceAppName() + "/"
            + xdef.getSourceGraphName() + "/dto";
        sourceDtos = NetUtil.post(DataTransferObjects.class, sourceUrl, sourceDtosRequest);
        List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObject();
  
        // append add/sync DTOs to resultDtoList and remove them from sourceDtoList
        for (int i = 0; i < sourceDtoList.size(); i++)
        {
          DataTransferObject sourceDto = sourceDtoList.get(i);
          List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getClassObject();
  
          if (sourceClassObjectList.size() > 0)
          {
            for (int j = 0; j < addIdentifierList.size(); j++)
            {
              if (sourceClassObjectList.get(0).getIdentifier().equalsIgnoreCase(addIdentifierList.get(j)))
              {
                DataTransferObject addDto = sourceDtoList.remove(i--);
                addDto.setTransferType(TransferType.ADD);
                resultDtoList.add(addDto);
  
                addIdentifierList.remove(j--);
                break;
              }
            }
  
            for (int j = 0; j < syncIdentifierList.size(); j++)
            {
              if (sourceClassObjectList.get(0).getIdentifier().equalsIgnoreCase(syncIdentifierList.get(j)))
              {
                DataTransferObject syncDto = sourceDtoList.remove(i--);
                syncDto.setTransferType(TransferType.SYNC);
                resultDtoList.add(syncDto);
  
                syncIdentifierList.remove(j--);
                break;
              }
            }
          }
        }
      }
  
      // get delete/change DTOs from target endpoint
      Identifiers targetIdentifiers = new Identifiers();
      targetIdentifiers.getIdentifier().addAll(changeIdentifierList);
      targetIdentifiers.getIdentifier().addAll(deleteIdentifierList);
  
      if (targetIdentifiers.getIdentifier().size() > 0)
      {
        DxRequest targetDtosRequest = new DxRequest();
        targetDtosRequest.setManifest(targetManifest);
        targetDtosRequest.setIdentifiers(targetIdentifiers);
  
        String targetUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName() + "/"
            + xdef.getTargetGraphName() + "/dto";
        targetDtos = NetUtil.post(DataTransferObjects.class, targetUrl, targetDtosRequest);
        List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObject();
  
        // add deleted DTOs to resultDtoList and remove them from targetDtoList
        for (int i = 0; i < targetDtoList.size(); i++)
        {
          DataTransferObject targetDto = targetDtoList.get(i);
          List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getClassObject();
  
          if (targetClassObjectList.size() > 0)
          {
            for (int j = 0; j < deleteIdentifierList.size(); j++)
            {
              if (targetClassObjectList.get(0).getIdentifier().equalsIgnoreCase(deleteIdentifierList.get(j)))
              {
                DataTransferObject deleteDto = targetDtoList.remove(i--);
                deleteDto.setTransferType(TransferType.DELETE);
                resultDtoList.add(deleteDto);
  
                deleteIdentifierList.remove(j--);
                break;
              }
            }
          }
        }
      }
  
      if (sourceDtos != null && targetDtos != null)
      {
        // request exchange service to compare changed DTOs
        DxoRequest dxoRequest = new DxoRequest();
        dxoRequest.setSourceDataTransferObjects(sourceDtos);
        dxoRequest.setTargetDataTransferObjects(targetDtos);
        String diffServiceUrl = settings.get("differencingServiceUri") + "/dxo";
        DataTransferObjects diffDtos = NetUtil.post(DataTransferObjects.class, diffServiceUrl, dxoRequest);
    
        // add diff DTOs to add/change/sync list
        if (diffDtos != null)
          resultDtoList.addAll(diffDtos.getDataTransferObject());
      }
      
      DataTransferObjectComparator dtoc = new DataTransferObjectComparator();
      Collections.sort(resultDtoList, dtoc);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return resultDtos;
  }

  @POST
  @Path("/dti/{exchangeId}")
  public Response postDtiList(@PathParam("exchangeId") String exchangeId, DtiSubmission dtiSubmission)
  {
    Response response = new Response();
    Response.StatusList statusList = new Response.StatusList();
    response.setStatusList(statusList);
    
    try
    {
      if (dtiSubmission == null)
        return null;
  
      DataTransferIndices dtis = dtiSubmission.getDti();
      if (dtis == null)
        return null;
  
      List<DataTransferIndex> dtiList = dtis.getDataTransferIndex();
      if (dtiList.size() == 0)
        return null;
  
      init();
  
      // get exchange definition
      ExchangeDefinition xdef = getExchangeDefinition(exchangeId);
  
      // get target application uri
      String targetAppUri = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName();
      String targetManifestUrl = targetAppUri + "/manifest";
      String targetGraphUri = targetAppUri + "/" + xdef.getTargetGraphName();
      String sourceGraphUri = xdef.getSourceUri() + "/" + xdef.getSourceAppScope() + "/" + xdef.getSourceAppName() + "/"
        + xdef.getSourceGraphName();
  
      // get target manifest
      Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);    
      if (targetManifest == null || targetManifest.getGraphs().getGraph().size() == 0) return null;
  
      // submit DTOs to target adapter service by configured pool size per submission
      int dtiSize = dtiList.size();
      int poolSize = Integer.parseInt(settings.get("poolSize")) - 1; // zero-based
  
      for (int i = 0; i < dtiSize; i += poolSize)
      {
        int actualPoolSize = (dtiSize > (i + poolSize)) ? poolSize : dtiSize - i;
        List<DataTransferIndex> poolDtiList = dtiList.subList(i, actualPoolSize);
        List<DataTransferIndex> sourceDtiList = new ArrayList<DataTransferIndex>();
  
        List<String> addIdentifierList = new ArrayList<String>();
        List<String> changeIdentifierList = new ArrayList<String>();
        List<String> syncIdentifierList = new ArrayList<String>();
        List<String> deleteIdentifierList = new ArrayList<String>();
  
        for (DataTransferIndex dti : poolDtiList)
        {
          String identifier = dti.getIdentifier();
  
          switch (dti.getTransferType())
          {
          case ADD:
            addIdentifierList.add(identifier);
            sourceDtiList.add(dti);
            break;
          case CHANGE:
            changeIdentifierList.add(identifier);
            sourceDtiList.add(dti);
            break;
          case SYNC:
            syncIdentifierList.add(identifier);
            sourceDtiList.add(dti);
            break;
          case DELETE:
            deleteIdentifierList.add(identifier);
            break;
          }
        }
  
        // get add/change/sync DTOs from source endpoint
        Identifiers sourceIdentifiers = new Identifiers();
        sourceIdentifiers.getIdentifier().addAll(addIdentifierList);
        sourceIdentifiers.getIdentifier().addAll(changeIdentifierList);
  
        if (dtiSubmission.isReviewed())
          sourceIdentifiers.getIdentifier().addAll(syncIdentifierList);
  
        if (sourceIdentifiers.getIdentifier().size() > 0)
        {   
          // request for current dti list and compare with previous set; also remove sync ones
          if (dtiSubmission.isReviewed())
          {            
            // create data exchange request
            DxRequest dxRequest = new DxRequest();
            dxRequest.setManifest(targetManifest);
            dxRequest.setHashAlgorithm(xdef.getHashAlgorithm());
        
            // get source dti
            String sourceDtiUrl = sourceGraphUri + "/dti";
            DataTransferIndices currentSourceDtis = NetUtil.post(DataTransferIndices.class, sourceDtiUrl, dxRequest);
            
            if (currentSourceDtis != null)
            {
              List<DataTransferIndex> currentSourceDtiList = currentSourceDtis.getDataTransferIndex();
              List<String> sourceIdentifierList = sourceIdentifiers.getIdentifier();
              
              for (DataTransferIndex sourceDti : sourceDtiList)
              {
                // case 1: if current dti found in original source dti list but hash value different, 
                // remove it from sourceIdentifers and log message
                
                // case 2: if current dti found in original source dti list with same hash value and has sync status, 
                // remove it from sourceIdentifers
                
                // case 3: if current dti not found in original source dti list (item no longer exists), 
                // remove it from sourceIdentifiers and log message
                
                String sourceIdentifier = sourceDti.getIdentifier();                
                boolean found = false;    
                
                for (DataTransferIndex currentSourceDti : currentSourceDtiList)
                {
                  if (sourceIdentifier.equals(currentSourceDti.getIdentifier()))
                  {
                    if (!sourceDti.getHashValue().equals(currentSourceDti.getHashValue()))
                    {
                      Status status = createStatus(sourceIdentifier, "DTO has changed.");
                      response.getStatusList().getStatus().add(status);
                      
                      sourceIdentifierList.remove(sourceIdentifier);
                    }
                    else if (sourceDti.getTransferType() == org.iringtools.adapter.dti.TransferType.SYNC)
                    {
                      sourceIdentifierList.remove(sourceIdentifier);
                    }
                    
                    found = true;
                    break;
                  }
                }
                
                if (!found)
                {
                  Status status = createStatus(sourceIdentifier, "DTO no longer exists.");
                  response.getStatusList().getStatus().add(status);
                  
                  sourceIdentifierList.remove(sourceIdentifier);
                }
              }
            }
          }
          
          // get source DTOs
          DxRequest poolDtosRequest = new DxRequest();
          poolDtosRequest.setManifest(targetManifest);        
          poolDtosRequest.setIdentifiers(sourceIdentifiers);
    
          String sourceDtoUrl = sourceGraphUri + "/dto";
          DataTransferObjects poolDtos = NetUtil.post(DataTransferObjects.class, sourceDtoUrl, poolDtosRequest);
          List<DataTransferObject> poolDtoList = poolDtos.getDataTransferObject();
              
          for (DataTransferObject dto : poolDtoList)
          {
            if (addIdentifierList.contains(dto.getClassObjects().getClassObject().get(0).getIdentifier()))
            {
              dto.setTransferType(TransferType.ADD);
            }
            else if (changeIdentifierList.contains(dto.getClassObjects().getClassObject().get(0).getIdentifier()))
            {
              dto.setTransferType(TransferType.CHANGE);
            }
          }
    
          // append delete DTOs
          for (String identifier : deleteIdentifierList)
          {
            DataTransferObject deleteDto = new DataTransferObject();
            deleteDto.setTransferType(TransferType.DELETE);
    
            ClassObjects classObjects = new ClassObjects();
            deleteDto.setClassObjects(classObjects);
    
            List<ClassObject> classObjectList = classObjects.getClassObject();
            ClassObject classObject = new ClassObject();
            classObject.setIdentifier(identifier);
            classObjectList.add(classObject);
    
            poolDtoList.add(deleteDto);
          }
    
          Response poolResponse = NetUtil.post(Response.class, targetGraphUri + "?format=dxo", poolDtos);
          response.getStatusList().getStatus().addAll(poolResponse.getStatusList().getStatus());
        }
      }
  
      response.setLevel(StatusLevel.SUCCESS);
    }
    catch (Exception ex)
    {
      logger.error("Error while posting DTOs: " + ex);
      response.setLevel(StatusLevel.ERROR);
    }
    
    return response;
  }

  // TODO: set default values if not provided
  private void init()
  {
    settings.put("baseDirectory", context.getRealPath("/"));
    settings.put("directoryServiceUri", context.getInitParameter("directoryServiceUri"));
    settings.put("differencingServiceUri", context.getInitParameter("differencingServiceUri"));
    settings.put("poolSize", context.getInitParameter("poolSize"));
  }

  private ExchangeDefinition getExchangeDefinition(String exchangeId) throws JAXBException, IOException
  {
    String directoryServiceUrl = settings.get("directoryServiceUri") + "/exchanges/" + exchangeId;
    return NetUtil.get(ExchangeDefinition.class, directoryServiceUrl);
  }
  
  private Status createStatus(String identifier, String message)
  {
    Status.Messages messages = new Status.Messages();
    List<String> messageList = messages.getMessage();
    messageList.add(message);
    
    Status status = new Status();
    status.setIdentifier(identifier);                      
    status.setMessages(messages);
    
    return status;
  }
}