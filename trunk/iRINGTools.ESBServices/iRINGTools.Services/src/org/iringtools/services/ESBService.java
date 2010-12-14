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
import org.iringtools.dxfr.request.DtoPageRequest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.services.core.DataTransferObjectComparator;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.HttpClient;
import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import java.util.GregorianCalendar;

@Path("/")
@Produces("application/xml")
public class ESBService
{
  private static final Logger logger = Logger.getLogger(ESBService.class);
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
    httpClient = new HttpClient();
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
      directory = httpClient.get(Directory.class, settings.get("directoryServiceUri") + "/directory");
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

      // get target manifest
      String targetManifestUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/manifest";
      Manifest targetManifest = httpClient.get(Manifest.class, targetManifestUrl);
      
      if (targetManifest == null || targetManifest.getGraphs().getItems().size() == 0)
        return null;

      // get source dti
      String sourceDtiUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/dxi?hashAlgorithm=" + hashAlgorithm;
      DataTransferIndices sourceDtis = httpClient.post(DataTransferIndices.class, sourceDtiUrl, targetManifest);

      // get target dti
      String targetDtiUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/dxi?hashAlgorithm=" + hashAlgorithm;
      DataTransferIndices targetDtis = httpClient.post(DataTransferIndices.class, targetDtiUrl, targetManifest);

      // create dxi request to diff source and target dti
      DxiRequest dxiRequest = new DxiRequest();
      dxiRequest.setSourceScopeName(sourceScopeName);
      dxiRequest.setSourceAppName(sourceAppName);
      dxiRequest.setTargetScopeName(targetScopeName);
      dxiRequest.setTargetAppName(targetAppName);
      dxiRequest.getDataTransferIndicies().add(sourceDtis);
      dxiRequest.getDataTransferIndicies().add(targetDtis);

      // request exchange service to diff the dti
      String dxiUrl = settings.get("differencingServiceUri") + "/dxi";
      dxiList = httpClient.post(DataTransferIndices.class, dxiUrl, dxiRequest);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return dxiList;
  }

  @POST
  @Path("/{scope}/exchanges/{id}")
  @Consumes("application/xml")
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
      List<DataTransferObject> resultDtoListItems = resultDtoList.getItems();

      // init exchange definition
      initExchangeDefinition(scope, id);
      
      // get target manifest
      String targetManifestUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/manifest";
      Manifest targetManifest = httpClient.get(Manifest.class, targetManifestUrl);
      
      if (targetManifest == null || targetManifest.getGraphs().getItems().size() == 0)
        return null;

      List<DataTransferIndex> sourceDtiListItems = new ArrayList<DataTransferIndex>();
      List<DataTransferIndex> targetDtiListItems = new ArrayList<DataTransferIndex>();

      for (DataTransferIndex dti : dataTransferIndices.getDataTransferIndexList().getItems())
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
        DataTransferIndexList sourceDtiList = sourceDataTransferIndices.getDataTransferIndexList();
        sourceDtiList.setItems(sourceDtiListItems);
        sourceDtoPageRequest.setDataTransferIndices(sourceDataTransferIndices);

        String sourceDtoUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/dxo";
        sourceDtos = httpClient.post(DataTransferObjects.class, sourceDtoUrl, sourceDtoPageRequest);
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
                TransferType transferType = sourceDti.getTransferType();
                
                if (transferType == TransferType.ADD)
                {
                  DataTransferObject addDto = sourceDtoListItems.remove(i--);
                  addDto.setTransferType(org.iringtools.dxfr.dto.TransferType.ADD);
                  resultDtoListItems.add(addDto);
                  break;
                }
                else if (transferType == TransferType.SYNC)
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

      // get target DTOs
      if (targetDtiListItems.size() > 0)
      {
        DtoPageRequest targetDtoPageRequest = new DtoPageRequest();
        targetDtoPageRequest.setManifest(targetManifest);
        DataTransferIndices targetDataTransferIndices = new DataTransferIndices();
        DataTransferIndexList targetDtiList = targetDataTransferIndices.getDataTransferIndexList();
        targetDtiList.setItems(targetDtiListItems);
        targetDataTransferIndices.setDataTransferIndexList(targetDtiList);
        targetDtoPageRequest.setDataTransferIndices(targetDataTransferIndices);

        String targetDtoUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/dxo";
        targetDtos = httpClient.post(DataTransferObjects.class, targetDtoUrl, targetDtoPageRequest);
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

        String dxoUrl = settings.get("differencingServiceUri") + "/dxo";
        DataTransferObjects dxoList = httpClient.post(DataTransferObjects.class, dxoUrl, dxoRequest);

        // add diff DTOs to add/change/sync list
        if (dxoList != null)
          resultDtoListItems.addAll(dxoList.getDataTransferObjectList().getItems());
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
  @Consumes("application/xml")
  public ExchangeResponse submitExchange(@PathParam("scope") String scope, @PathParam("id") String id,
      ExchangeRequest exchangeRequest) throws IOException, JAXBException
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

      List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
      
      if (dtiList.size() == 0)
        return null;

      init();

      // init exchange definition
      initExchangeDefinition(scope, id);
      
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
      String targetAppUrl = targetUri + "/" + targetScopeName + "/" + targetAppName;
      String targetManifestUrl = targetAppUrl + "/manifest";
      String targetGraphUrl = targetAppUrl + "/" + targetGraphName;
      String sourceGraphUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName;

      // get target manifest
      Manifest targetManifest = httpClient.get(Manifest.class, targetManifestUrl);
      
      if (targetManifest == null || targetManifest.getGraphs().getItems().size() == 0)
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
        DataTransferIndexList poolDtiList = poolDataTransferIndices.getDataTransferIndexList();
        poolDtiList.setItems(sourcePoolDtiList);
        poolDataTransferIndices.setDataTransferIndexList(poolDtiList);
        poolDtosRequest.setDataTransferIndices(poolDataTransferIndices);
        
        String sourceDtoUrl = sourceGraphUrl + "/dxo";
        DataTransferObjects poolDtos = httpClient.post(DataTransferObjects.class, sourceDtoUrl, poolDtosRequest);
        List<DataTransferObject> poolDtoListItems = poolDtos.getDataTransferObjectList().getItems();

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
                    exchangeResponse.getStatusList().getItems().add(status);
                    
                    if (exchangeResponse.getLevel() != Level.ERROR)
                      exchangeResponse.setLevel(Level.WARNING);
                    
                    poolDtoListItems.remove(j--); 
                  }
                  else if (dti.getTransferType() == TransferType.SYNC)
                    poolDtoListItems.remove(j--);  // exclude SYNC DTOs
                  else
                    sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.valueOf(dti.getTransferType().toString()));
                }
                else
                  sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.valueOf(dti.getTransferType().toString()));
  
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
            exchangeResponse.getStatusList().getItems().add(status);
            
            if (exchangeResponse.getLevel() != Level.ERROR)
              exchangeResponse.setLevel(Level.WARNING);
          }
        }

        // create identifiers for deleted DTOs
        for (DataTransferIndex deleteDti : deleteDtiList)
        {
          DataTransferObject deleteDto = new DataTransferObject();
          deleteDto.setIdentifier(deleteDti.getIdentifier());
          deleteDto.setTransferType(org.iringtools.dxfr.dto.TransferType.DELETE);
          poolDtoListItems.add(deleteDto);
        }

        // post add/change/delete DTOs to target endpoint
        if (poolDtoListItems.size() > 0)
        {
          Response poolResponse = httpClient.post(Response.class, targetGraphUrl, poolDtos);
          exchangeResponse.getStatusList().getItems().addAll(poolResponse.getStatusList().getItems());
          
          if (exchangeResponse.getLevel() != Level.ERROR || (exchangeResponse.getLevel() == Level.WARNING && poolResponse.getLevel() == Level.SUCCESS))
            exchangeResponse.setLevel(poolResponse.getLevel());
        }
      }
      
      if (exchangeResponse.getStatusList().getItems().size() == 0)
      {
        Status status = createStatus("Overall", "No Add/Change/Delete DTOs are found!");
        exchangeResponse.getStatusList().getItems().add(status);
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
    
    //Store the exchange response in history	
    String timestamp = 
    	datatypeFactory.newXMLGregorianCalendar(gcal).toString().replace(":", ".");
    String path = 
    	settings.get("baseDirectory") + "/WEB-INF/logs/" + scope + "/exchanges/" + id + "/" + timestamp + ".xml";
    JaxbUtil.write(exchangeResponse, path, true);

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

  private void initExchangeDefinition(String scope, String id) throws JAXBException, IOException 
  {
    String directoryServiceUrl = settings.get("directoryServiceUri") + "/" + scope + "/exchanges/" + id;
    ExchangeDefinition xdef = httpClient.get(ExchangeDefinition.class, directoryServiceUrl);
    
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
    List<String> messageList = messages.getItems();
    messageList.add(message);

    Status status = new Status();
    status.setIdentifier(identifier);
    status.setMessages(messages);

    return status;
  }

  private String md5Hash(DataTransferObject dataTransferObject)
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