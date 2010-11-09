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
import org.apache.commons.codec.digest.DigestUtils;
import org.apache.log4j.Logger;
import org.iringtools.adapter.dti.DataTransferIndex;
import org.iringtools.adapter.dti.DataTransferIndexList;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dti.TransferType;
import org.iringtools.adapter.dto.ClassObject;
import org.iringtools.adapter.dto.DataTransferObject;
import org.iringtools.adapter.dto.DataTransferObjectList;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.adapter.dto.RoleObject;
import org.iringtools.adapter.dto.RoleType;
import org.iringtools.adapter.dto.TemplateObject;
import org.iringtools.common.request.DtoPageRequest;
import org.iringtools.common.request.DxiRequest;
import org.iringtools.common.request.DxoRequest;
import org.iringtools.common.request.ExchangeRequest;
import org.iringtools.common.response.ExchangeResponse;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.protocol.manifest.Manifest;
import org.iringtools.services.core.DataTransferObjectComparator;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.WebClient;
import org.iringtools.utility.WebClientException;
import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import java.util.GregorianCalendar;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class ESBService
{
  private static final Logger logger = Logger.getLogger(ESBService.class);
  private static DatatypeFactory datatypeFactory = null;  
  
  private String sourceUri = null;
  private String sourceScopeName = null;
  private String sourceAppName = null;
  private String sourceGraphName = null;
  private String targetUri = null;
  private String targetScopeName = null;
  private String targetAppName = null;
  private String targetGraphName = null;
  private String hashAlgorithm = null;

  @Context
  private ServletContext context;
  private Hashtable<String, String> settings;
  
//  @Context
//  private javax.ws.rs.core.SecurityContext securityContext;
//  @Context 
//  private org.apache.cxf.jaxrs.ext.MessageContext messageContext;

  public ESBService() throws DatatypeConfigurationException
  {
    settings = new Hashtable<String, String>();
    datatypeFactory = DatatypeFactory.newInstance();
  }

  @GET
  @Path("/directory")
  public Directory getExchanges() throws JAXBException, IOException
  {
    Directory directory = null;

    try
    {
      init();

      // request directory service to get exchange definitions
      WebClient webClient = new WebClient(settings.get("directoryServiceUri"));
      directory = webClient.get(Directory.class, "/directory");
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return directory;
  }

  @GET
  @Path("/{scope}/exchanges/{id}")
  public DataTransferIndices getDataTransferIndices(@PathParam("scope") String scope, @PathParam("id") String id)
  {
    DataTransferIndices dxiList = null;

    try
    {
      init();

      // init exchange definition
      initExchangeDefinition(scope, id);
      WebClient sourceClient = new WebClient(sourceUri);
      WebClient targetClient = new WebClient(targetUri);

      // get target manifest
      String targetManifestUrl = "/" + targetScopeName + "/" + targetAppName + "/manifest";
      Manifest targetManifest = targetClient.get(Manifest.class, targetManifestUrl);
      
      if (targetManifest == null || targetManifest.getGraphs().getGraphs().size() == 0)
        return null;

      // get source dti
      String sourceDtiUrl = "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/xfr?hashAlgorithm=" + hashAlgorithm;
      DataTransferIndices sourceDtis = sourceClient.post(DataTransferIndices.class, sourceDtiUrl, targetManifest);

      // get target dti
      String targetDtiUrl = "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/xfr?hashAlgorithm=" + hashAlgorithm;
      DataTransferIndices targetDtis = targetClient.post(DataTransferIndices.class, targetDtiUrl, targetManifest);

      // create dxi request to diff source and target dti
      DxiRequest dxiRequest = new DxiRequest();
      dxiRequest.setSourceScopeName(sourceScopeName);
      dxiRequest.setSourceAppName(sourceAppName);
      dxiRequest.setTargetScopeName(targetScopeName);
      dxiRequest.setTargetAppName(targetAppName);
      dxiRequest.getDataTransferIndicies().add(sourceDtis);
      dxiRequest.getDataTransferIndicies().add(targetDtis);

      // request exchange service to diff the dti
      WebClient dxiClient = new WebClient(settings.get("differencingServiceUri"));
      dxiList = dxiClient.post(DataTransferIndices.class, "/dxi", dxiRequest);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return dxiList;
  }

  @POST
  @Path("/{scope}/exchanges/{id}")
  public DataTransferObjects getDataTransferObjects(@PathParam("scope") String scope, @PathParam("id") String id,
      DataTransferIndices dataTransferIndices)
  {
    DataTransferObjects resultDtos = new DataTransferObjects();
    DataTransferObjects sourceDtos = null;
    DataTransferObjects targetDtos = null;

    try
    {
      logger.info("ExchangeRequest: " + JaxbUtil.toXml(dataTransferIndices, true));
      
      init();

      DataTransferObjectList resultDtoList = new DataTransferObjectList();
      resultDtos.setDataTransferObjectList(resultDtoList);
      List<DataTransferObject> resultDtoListItems = resultDtoList.getDataTransferObjectListItems();

      // init exchange definition
      initExchangeDefinition(scope, id);
      WebClient sourceClient = new WebClient(sourceUri);
      WebClient targetClient = new WebClient(targetUri);
      
      // get target manifest
      String targetManifestUrl = "/" + targetScopeName + "/" + targetAppName + "/manifest";
      Manifest targetManifest = targetClient.get(Manifest.class, targetManifestUrl);
      
      if (targetManifest == null || targetManifest.getGraphs().getGraphs().size() == 0)
        return null;

      List<DataTransferIndex> sourceDtiListItems = new ArrayList<DataTransferIndex>();
      List<DataTransferIndex> targetDtiListItems = new ArrayList<DataTransferIndex>();

      for (DataTransferIndex dti : dataTransferIndices.getDataTransferIndexList().getDataTransferIndexListItems())
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
        DtoPageRequest sourceDtoPageRequest = new DtoPageRequest();
        sourceDtoPageRequest.setManifest(targetManifest);
        DataTransferIndices sourceDataTransferIndices = new DataTransferIndices();
        DataTransferIndexList sourceDtiList = new DataTransferIndexList();
        sourceDtiList.setDataTransferIndexListItems(sourceDtiListItems);
        sourceDataTransferIndices.setDataTransferIndexList(sourceDtiList);
        sourceDtoPageRequest.setDataTransferIndices(sourceDataTransferIndices);

        String sourceDtoUrl = "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/xfr/page";
        sourceDtos = sourceClient.post(DataTransferObjects.class, sourceDtoUrl, sourceDtoPageRequest);
        List<DataTransferObject> sourceDtoListItems = sourceDtos.getDataTransferObjectList().getDataTransferObjectListItems();

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
                TransferType transferType = sourceDti.getTransferType();
                
                if (transferType == TransferType.ADD)
                {
                  DataTransferObject addDto = sourceDtoListItems.remove(i--);
                  addDto.setTransferType(org.iringtools.adapter.dto.TransferType.ADD);
                  resultDtoListItems.add(addDto);
                  break;
                }
                else if (transferType == TransferType.SYNC)
                {
                  DataTransferObject syncDto = sourceDtoListItems.remove(i--);
                  syncDto.setTransferType(org.iringtools.adapter.dto.TransferType.SYNC);
                  resultDtoListItems.add(syncDto);
                  break;
                }
              }
            }
          }
        }
      }

      // get target DTOs
      if (targetDtiListItems.size() > 0)
      {
        DtoPageRequest targetDtoPageRequest = new DtoPageRequest();
        targetDtoPageRequest.setManifest(targetManifest);
        DataTransferIndices targetDataTransferIndices = new DataTransferIndices();
        DataTransferIndexList targetDtiList = new DataTransferIndexList();
        targetDtiList.setDataTransferIndexListItems(targetDtiListItems);
        targetDataTransferIndices.setDataTransferIndexList(targetDtiList);
        targetDtoPageRequest.setDataTransferIndices(targetDataTransferIndices);

        String targetDtoUrl = "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/xfr/page";
        targetDtos = targetClient.post(DataTransferObjects.class, targetDtoUrl, targetDtoPageRequest);
        List<DataTransferObject> targetDtoListItems = targetDtos.getDataTransferObjectList().getDataTransferObjectListItems();

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
                  deleteDto.setTransferType(org.iringtools.adapter.dto.TransferType.DELETE);
                  resultDtoListItems.add(deleteDto);
                  break;
                }
              }
            }
          }
        }
      }

      if (sourceDtos != null && targetDtos != null)
      {
        // request exchange service to compare changed DTOs
        DxoRequest dxoRequest = new DxoRequest();
        dxoRequest.setSourceScopeName(sourceScopeName);
        dxoRequest.setSourceAppName(sourceAppName);
        dxoRequest.setTargetScopeName(targetScopeName);
        dxoRequest.setTargetAppName(targetAppName);
        dxoRequest.getDataTransferObjects().add(sourceDtos);
        dxoRequest.getDataTransferObjects().add(targetDtos);

        WebClient dxoClient = new WebClient(settings.get("differencingServiceUri"));
        DataTransferObjects dxoList = dxoClient.post(DataTransferObjects.class, "/dxo", dxoRequest);

        // add diff DTOs to add/change/sync list
        if (dxoList != null)
          resultDtoListItems.addAll(dxoList.getDataTransferObjectList().getDataTransferObjectListItems());
      }

      DataTransferObjectComparator dtoc = new DataTransferObjectComparator();
      Collections.sort(resultDtoListItems, dtoc);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return resultDtos;
  }

  @POST
  @Path("/{scope}/exchanges/{id}/submit")
  public ExchangeResponse submitExchange(@PathParam("scope") String scope, @PathParam("id") String id,
      ExchangeRequest exchangeRequest)
  {
    ExchangeResponse exchangeResponse = new ExchangeResponse();
    GregorianCalendar gcal = new GregorianCalendar();
    exchangeResponse.setStartTimeStamp(datatypeFactory.newXMLGregorianCalendar(gcal));
    StatusList statusList = new StatusList();
    exchangeResponse.setStatusList(statusList);
    exchangeResponse.setLevel(Level.SUCCESS);

    try
    {
      logger.info("ExchangeRequest: " + JaxbUtil.toXml(exchangeRequest, true));
      
      if (exchangeRequest == null)
        return null;

      DataTransferIndices dtis = exchangeRequest.getDataTransferIndices();
      
      if (dtis == null)
        return null;

      List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getDataTransferIndexListItems();
      
      if (dtiList.size() == 0)
        return null;

      init();

      // init exchange definition
      initExchangeDefinition(scope, id);
      WebClient sourceClient = new WebClient(sourceUri);
      WebClient targetClient = new WebClient(targetUri);
      
      // add exchange definition info to exchange response
      exchangeResponse.setSenderUri(sourceUri);
      exchangeResponse.setSenderScopeName(sourceScopeName);
      exchangeResponse.setSenderAppName(sourceAppName);
      exchangeResponse.setSenderGraphName(sourceGraphName);
      exchangeResponse.setReceiverUri(targetUri);
      exchangeResponse.setReceiverScopeName(targetScopeName);
      exchangeResponse.setReceiverAppName(targetAppName);
      exchangeResponse.setReceiverGraphName(targetGraphName);

      // get target application uri
      String targetAppUrl = "/" + targetScopeName + "/" + targetAppName;
      String targetManifestUrl = targetAppUrl + "/manifest";
      String targetGraphUrl = targetAppUrl + "/" + targetGraphName;
      String sourceGraphUrl = "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName;

      // get target manifest
      Manifest targetManifest = targetClient.get(Manifest.class, targetManifestUrl);
      
      if (targetManifest == null || targetManifest.getGraphs().getGraphs().size() == 0)
        return null;

      // create a pool (page) DTOs to send to target endpoint
      int dtiSize = dtiList.size();
      int poolSize = Integer.parseInt(settings.get("poolSize"));

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

        // only include SYNC DTIs if the DTOs have been reviewed         
        if (exchangeRequest.isReviewed())
          sourcePoolDtiList.addAll(syncDtiList);

        // request source DTOs
        DtoPageRequest poolDtosRequest = new DtoPageRequest();
        poolDtosRequest.setManifest(targetManifest);
        DataTransferIndices poolDataTransferIndices = new DataTransferIndices();
        DataTransferIndexList poolDtiList = new DataTransferIndexList();
        poolDtiList.setDataTransferIndexListItems(sourcePoolDtiList);
        poolDataTransferIndices.setDataTransferIndexList(poolDtiList);
        poolDtosRequest.setDataTransferIndices(poolDataTransferIndices);
        
        String sourceDtoUrl = sourceGraphUrl + "/xfr/page";
        DataTransferObjects poolDtos = sourceClient.post(DataTransferObjects.class, sourceDtoUrl, poolDtosRequest);
        List<DataTransferObject> poolDtoListItems = poolDtos.getDataTransferObjectList().getDataTransferObjectListItems();

        // set transfer type for each DTO in poolDtoList and remove/report ones that have changed
        // and deleted during review and acceptance period
        for (int j = 0; j < poolDtoListItems.size(); j++)
        {
          DataTransferObject sourceDto = poolDtoListItems.get(j);
          String identifier = sourceDto.getIdentifier();          
          
          if (sourceDto.getClassObjects() != null)
          {
            for (DataTransferIndex dti : poolDtiListItems)
            {
              if (dti.getIdentifier().equals(identifier))
              {
                if (exchangeRequest.isReviewed())
                {
                  sourcePoolDtiList.remove(dti);
                  String hashValue = md5Hash(sourceDto);
  
                  if (!hashValue.equalsIgnoreCase(dti.getHashValue()))
                  {
                    Status status = createStatus(identifier, "DTO has changed.");
                    exchangeResponse.getStatusList().getStatuses().add(status);
                    
                    if (exchangeResponse.getLevel() != Level.ERROR)
                      exchangeResponse.setLevel(Level.WARNING);
                    
                    poolDtoListItems.remove(j--); 
                  }
                  else if (dti.getTransferType() == TransferType.SYNC)
                    poolDtoListItems.remove(j--);  // exclude SYNC DTOs
                  else
                    sourceDto.setTransferType(org.iringtools.adapter.dto.TransferType.valueOf(dti.getTransferType().toString()));
                }
                else
                  sourceDto.setTransferType(org.iringtools.adapter.dto.TransferType.valueOf(dti.getTransferType().toString()));
  
                break;
              }
            }
          }
        }

        // report DTOs that were deleted during review and acceptance
        if (exchangeRequest.isReviewed() && sourcePoolDtiList.size() > 0)
        {
          for (DataTransferIndex sourceDti : sourcePoolDtiList)
          {
            Status status = createStatus(sourceDti.getIdentifier(), "DTO no longer exists.");
            exchangeResponse.getStatusList().getStatuses().add(status);
            
            if (exchangeResponse.getLevel() != Level.ERROR)
              exchangeResponse.setLevel(Level.WARNING);
          }
        }

        // create identifiers for deleted DTOs
        for (DataTransferIndex deleteDti : deleteDtiList)
        {
          DataTransferObject deleteDto = new DataTransferObject();
          deleteDto.setIdentifier(deleteDti.getIdentifier());
          deleteDto.setTransferType(org.iringtools.adapter.dto.TransferType.DELETE);
          poolDtoListItems.add(deleteDto);
        }

        // post add/change/delete DTOs to target endpoint
        if (poolDtoListItems.size() > 0)
        {
          Response poolResponse = targetClient.post(Response.class, targetGraphUrl, poolDtos);
          exchangeResponse.getStatusList().getStatuses().addAll(poolResponse.getStatusList().getStatuses());
          
          if (exchangeResponse.getLevel() != Level.ERROR || (exchangeResponse.getLevel() == Level.WARNING && poolResponse.getLevel() == Level.SUCCESS))
            exchangeResponse.setLevel(poolResponse.getLevel());
        }
      }
      
      if (exchangeResponse.getStatusList().getStatuses().size() == 0)
      {
        Status status = createStatus("Overall", "No Add/Change/Delete DTOs are found!");
        exchangeResponse.getStatusList().getStatuses().add(status);
        exchangeResponse.setLevel(Level.WARNING);        
      }
    }
    catch (Exception ex)
    {
      logger.error("Error while posting DTOs: " + ex);
      exchangeResponse.setLevel(Level.ERROR);
    }
    
    gcal = new GregorianCalendar();
    exchangeResponse.setEndTimeStamp(datatypeFactory.newXMLGregorianCalendar(gcal));

    return exchangeResponse;
  }

  private void init()
  {
    settings.put("baseDirectory", context.getRealPath("/"));

    String directoryServiceUri = context.getInitParameter("directoryServiceUri");
    if (directoryServiceUri == null || directoryServiceUri.equals(""))
      directoryServiceUri = "http://localhost:8080/iringtools/services/dirsvc";
    settings.put("directoryServiceUri", directoryServiceUri);

    String differencingServiceUri = context.getInitParameter("differencingServiceUri");
    if (differencingServiceUri == null || differencingServiceUri.equals(""))
      differencingServiceUri = "http://localhost:8080/iringtools/services/diffsvc";
    settings.put("differencingServiceUri", differencingServiceUri);

    String poolSize = context.getInitParameter("poolSize");
    if (poolSize == null || poolSize.equals(""))
      poolSize = "50";
    settings.put("poolSize", poolSize);
  }

  private void initExchangeDefinition(String scope, String id) throws WebClientException
  {
    WebClient directoryClient = new WebClient(settings.get("directoryServiceUri"));
    String directoryServiceUrl = "/" + scope + "/exchanges/" + id;
    ExchangeDefinition xdef = directoryClient.get(ExchangeDefinition.class, directoryServiceUrl);
    
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

  private Status createStatus(String identifier, String message)
  {
    Messages messages = new Messages();
    List<String> messageList = messages.getMessages();
    messageList.add(message);

    Status status = new Status();
    status.setIdentifier(identifier);
    status.setMessages(messages);

    return status;
  }

  private String md5Hash(DataTransferObject dataTransferObject)
  {
    StringBuilder values = new StringBuilder();

    List<ClassObject> classObjects = dataTransferObject.getClassObjects().getClassObjects();
    for (ClassObject classObject : classObjects)
    {
      List<TemplateObject> templateObjects = classObject.getTemplateObjects().getTemplateObjects();
      for (TemplateObject templateObject : templateObjects)
      {
        List<RoleObject> roleObjects = templateObject.getRoleObjects().getRoleObjects();
        for (RoleObject roleObject : roleObjects)
        {
          if (roleObject.getType() == RoleType.PROPERTY || roleObject.getType() == RoleType.OBJECT_PROPERTY || roleObject.getType() == RoleType.DATA_PROPERTY)
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
}