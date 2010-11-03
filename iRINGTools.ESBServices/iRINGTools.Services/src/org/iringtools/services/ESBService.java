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
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dti.TransferType;
import org.iringtools.adapter.dto.ClassObject;
import org.iringtools.adapter.dto.DataTransferObject;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.adapter.dto.RoleObject;
import org.iringtools.adapter.dto.RoleType;
import org.iringtools.adapter.dto.TemplateObject;
import org.iringtools.common.request.DiffDtiRequest;
import org.iringtools.common.request.DiffDtoRequest;
import org.iringtools.common.request.DtoPageRequest;
import org.iringtools.common.request.ExchangeRequest;
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

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
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
    DataTransferIndices resultDtis = null;

    try
    {
      init();

      // get exchange definition
      ExchangeDefinition xdef = getExchangeDefinition(scope, id);
      WebClient sourceClient = new WebClient(xdef.getSourceUri());
      WebClient targetClient = new WebClient(xdef.getTargetUri());

      // get target manifest
      String targetManifestUrl = "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp() + "/manifest";
      Manifest targetManifest = targetClient.get(Manifest.class, targetManifestUrl);
      if (targetManifest == null || targetManifest.getGraphs().getGraphs().size() == 0)
        return null;

      // get source dti
      String sourceDtiUrl = "/" + xdef.getSourceScope() + "/" + xdef.getSourceApp() + "/"
          + xdef.getSourceGraph() + "/xfr?hashAlgorithm=" + xdef.getHashAlgorithm();
      DataTransferIndices sourceDtis = sourceClient.post(DataTransferIndices.class, sourceDtiUrl, targetManifest);

      // get target dti
      String targetDtiUrl = "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp() + "/"
          + xdef.getTargetGraph() + "/xfr?hashAlgorithm=" + xdef.getHashAlgorithm();
      DataTransferIndices targetDtis = targetClient.post(DataTransferIndices.class, targetDtiUrl, targetManifest);

      // create dxi request to diff source and target dti
      DiffDtiRequest diffDtiRequest = new DiffDtiRequest();
      diffDtiRequest.setSourceDataTransferIndicies(sourceDtis);
      diffDtiRequest.setTargetDataTransferIndicies(targetDtis);

      // request exchange service to diff the dti
      WebClient diffClient = new WebClient(settings.get("differencingServiceUri"));
      resultDtis = diffClient.post(DataTransferIndices.class, "/dti", diffDtiRequest);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return resultDtis;
  }

  @POST
  @Path("/{scope}/exchanges/{id}")
  public DataTransferObjects getDataTransferObjects(@PathParam("scope") String scope, @PathParam("id") String id,
      DataTransferIndices dataTransferIndices)
  {
    DataTransferObjects resultDtos = null;

    try
    {
      logger.info("ExchangeRequest: " + JaxbUtil.toXml(dataTransferIndices, true));
      
      init();

      DataTransferObjects sourceDtos = null;
      DataTransferObjects targetDtos = null;

      resultDtos = new DataTransferObjects();
      List<DataTransferObject> resultDtoList = resultDtos.getDataTransferObjects();

      // get exchange definition
      ExchangeDefinition xdef = getExchangeDefinition(scope, id);
      WebClient sourceClient = new WebClient(xdef.getSourceUri());
      WebClient targetClient = new WebClient(xdef.getTargetUri());

      // get target manifest
      String targetManifestUrl = "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp() + "/manifest";
      Manifest targetManifest = targetClient.get(Manifest.class, targetManifestUrl);
      if (targetManifest == null || targetManifest.getGraphs().getGraphs().size() == 0)
        return null;

      List<DataTransferIndex> sourceDtiList = new ArrayList<DataTransferIndex>();
      List<DataTransferIndex> targetDtiList = new ArrayList<DataTransferIndex>();

      for (DataTransferIndex dti : dataTransferIndices.getDataTransferIndices())
      {
        switch (dti.getTransferType())
        {
        case ADD:
          sourceDtiList.add(dti);
          break;
        case CHANGE:
          sourceDtiList.add(dti);
          targetDtiList.add(dti);
          break;
        case SYNC:
          sourceDtiList.add(dti);
          break;
        case DELETE:
          targetDtiList.add(dti);
          break;
        }
      }

      // get source DTOs
      if (sourceDtiList.size() > 0)
      {
        DtoPageRequest sourceDtoPageRequest = new DtoPageRequest();
        sourceDtoPageRequest.setManifest(targetManifest);
        DataTransferIndices sourceDataTransferIndices = new DataTransferIndices();
        sourceDataTransferIndices.setDataTransferIndices(sourceDtiList);
        sourceDtoPageRequest.setDataTransferIndices(sourceDataTransferIndices);

        String sourceDtoUrl = "/" + xdef.getSourceScope() + "/" + xdef.getSourceApp() + "/" + xdef.getSourceGraph() + "/xfr/page";
        sourceDtos = sourceClient.post(DataTransferObjects.class, sourceDtoUrl, sourceDtoPageRequest);
        List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObjects();

        // append add/sync DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < sourceDtoList.size(); i++)
        {
          DataTransferObject sourceDto = sourceDtoList.get(i);
          String sourceDtoIdentifier = sourceDto.getIdentifier();          
          
          if (sourceDto.getClassObjects() != null)
          {
            for (DataTransferIndex sourceDti : sourceDtiList)
            {
              if (sourceDtoIdentifier.equalsIgnoreCase(sourceDti.getIdentifier()))
              {
                TransferType transferType = sourceDti.getTransferType();
                
                if (transferType == TransferType.ADD)
                {
                  DataTransferObject addDto = sourceDtoList.remove(i--);
                  addDto.setTransferType(org.iringtools.adapter.dto.TransferType.ADD);
                  resultDtoList.add(addDto);
                  break;
                }
                else if (transferType == TransferType.SYNC)
                {
                  DataTransferObject syncDto = sourceDtoList.remove(i--);
                  syncDto.setTransferType(org.iringtools.adapter.dto.TransferType.SYNC);
                  resultDtoList.add(syncDto);
                  break;
                }
              }
            }
          }
        }
      }

      // get target DTOs
      if (targetDtiList.size() > 0)
      {
        DtoPageRequest targetDtoPageRequest = new DtoPageRequest();
        targetDtoPageRequest.setManifest(targetManifest);
        DataTransferIndices targetDataTransferIndices = new DataTransferIndices();
        targetDataTransferIndices.setDataTransferIndices(targetDtiList);
        targetDtoPageRequest.setDataTransferIndices(targetDataTransferIndices);

        String targetDtoUrl = "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp() + "/" + xdef.getTargetGraph() + "/xfr/page";
        targetDtos = targetClient.post(DataTransferObjects.class, targetDtoUrl, targetDtoPageRequest);
        List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObjects();

        // append delete DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < targetDtoList.size(); i++)
        {
          DataTransferObject targetDto = targetDtoList.get(i);
          String targetDtoIdentifier = targetDto.getIdentifier();          
          
          if (targetDto.getClassObjects() != null)
          {
            for (DataTransferIndex targetDti : targetDtiList)
            {
              if (targetDtoIdentifier.equalsIgnoreCase(targetDti.getIdentifier()))
              {
                if (targetDti.getTransferType() == TransferType.DELETE)
                {
                  DataTransferObject deleteDto = targetDtoList.remove(i--);
                  deleteDto.setTransferType(org.iringtools.adapter.dto.TransferType.DELETE);
                  resultDtoList.add(deleteDto);
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
        DiffDtoRequest diffDtoRequest = new DiffDtoRequest();
        diffDtoRequest.setSourceDataTransferObjects(sourceDtos);
        diffDtoRequest.setTargetDataTransferObjects(targetDtos);

        WebClient diffClient = new WebClient(settings.get("differencingServiceUri"));
        DataTransferObjects diffDtos = diffClient.post(DataTransferObjects.class, "/dto", diffDtoRequest);

        // add diff DTOs to add/change/sync list
        if (diffDtos != null)
          resultDtoList.addAll(diffDtos.getDataTransferObjects());
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
  @Path("/{scope}/exchanges/{id}/submit")
  public Response submitExchange(@PathParam("scope") String scope, @PathParam("id") String id,
      ExchangeRequest exchangeRequest)
  {
    Response response = new Response();
    StatusList statusList = new StatusList();
    response.setStatusList(statusList);
    response.setLevel(Level.SUCCESS);

    try
    {
      logger.info("ExchangeRequest: " + JaxbUtil.toXml(exchangeRequest, true));
      
      if (exchangeRequest == null)
        return null;

      DataTransferIndices dtis = exchangeRequest.getDataTransferIndices();
      if (dtis == null)
        return null;

      List<DataTransferIndex> dtiList = dtis.getDataTransferIndices();
      if (dtiList.size() == 0)
        return null;

      init();

      // get exchange definition
      ExchangeDefinition xdef = getExchangeDefinition(scope, id);
      WebClient sourceClient = new WebClient(xdef.getSourceUri());
      WebClient targetClient = new WebClient(xdef.getTargetUri());

      // get target application uri
      String targetAppUrl = "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp();
      String targetManifestUrl = targetAppUrl + "/manifest";
      String targetGraphUrl = targetAppUrl + "/" + xdef.getTargetGraph();
      String sourceGraphUrl = "/" + xdef.getSourceScope() + "/" + xdef.getSourceApp()
          + "/" + xdef.getSourceGraph();

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
        List<DataTransferIndex> poolDtiList = dtiList.subList(i, i + actualPoolSize);

        List<DataTransferIndex> sourcePoolDtiList = new ArrayList<DataTransferIndex>();
        List<DataTransferIndex> syncDtiList = new ArrayList<DataTransferIndex>();
        List<DataTransferIndex> deleteDtiList = new ArrayList<DataTransferIndex>();

        for (DataTransferIndex poolDti : poolDtiList)
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
        poolDataTransferIndices.setDataTransferIndices(sourcePoolDtiList);
        poolDtosRequest.setDataTransferIndices(poolDataTransferIndices);
        
        String sourceDtoUrl = sourceGraphUrl + "/xfr/page";
        DataTransferObjects poolDtos = sourceClient.post(DataTransferObjects.class, sourceDtoUrl, poolDtosRequest);
        List<DataTransferObject> poolDtoList = poolDtos.getDataTransferObjects();

        // set transfer type for each DTO in poolDtoList and remove/report ones that have changed
        // and deleted during review and acceptance period
        for (int j = 0; j < poolDtoList.size(); j++)
        {
          DataTransferObject sourceDto = poolDtoList.get(j);
          String identifier = sourceDto.getIdentifier();          
          
          if (sourceDto.getClassObjects() != null)
          {
            for (DataTransferIndex dti : poolDtiList)
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
                    response.getStatusList().getStatuses().add(status);
                    
                    if (response.getLevel() != Level.ERROR)
                      response.setLevel(Level.WARNING);
                    
                    poolDtoList.remove(j--); 
                  }
                  else if (dti.getTransferType() == TransferType.SYNC)
                    poolDtoList.remove(j--);  // exclude SYNC DTOs
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
            response.getStatusList().getStatuses().add(status);
            
            if (response.getLevel() != Level.ERROR)
              response.setLevel(Level.WARNING);
          }
        }

        // create identifiers for deleted DTOs
        for (DataTransferIndex deleteDti : deleteDtiList)
        {
          DataTransferObject deleteDto = new DataTransferObject();
          deleteDto.setIdentifier(deleteDti.getIdentifier());
          deleteDto.setTransferType(org.iringtools.adapter.dto.TransferType.DELETE);
          poolDtoList.add(deleteDto);
        }

        // post add/change/delete DTOs to target endpoint
        if (poolDtoList.size() > 0)
        {
          Response poolResponse = targetClient.post(Response.class, targetGraphUrl, poolDtos);
          response.getStatusList().getStatuses().addAll(poolResponse.getStatusList().getStatuses());
          
          if (response.getLevel() != Level.ERROR || (response.getLevel() == Level.WARNING && poolResponse.getLevel() == Level.SUCCESS))
            response.setLevel(poolResponse.getLevel());
        }
      }
      
      if (response.getStatusList().getStatuses().size() == 0)
      {
        Status status = createStatus("Overall", "No Add/Change/Delete DTOs are found!");
        response.getStatusList().getStatuses().add(status);
        response.setLevel(Level.WARNING);        
      }
    }
    catch (Exception ex)
    {
      logger.error("Error while posting DTOs: " + ex);
      response.setLevel(Level.ERROR);
    }

    return response;
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

  private ExchangeDefinition getExchangeDefinition(String scope, String id) throws WebClientException
  {
    WebClient directoryClient = new WebClient(settings.get("directoryServiceUri"));
    String directoryServiceUrl = "/" + scope + "/exchanges/" + id;
    return directoryClient.get(ExchangeDefinition.class, directoryServiceUrl);
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