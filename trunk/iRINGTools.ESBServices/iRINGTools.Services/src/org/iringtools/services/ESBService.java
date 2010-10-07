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
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dti.DataTransferIndex;
import org.iringtools.adapter.dto.DataTransferObject;
import org.iringtools.adapter.dto.ClassObject;
import org.iringtools.adapter.dto.ClassObjects;
import org.iringtools.adapter.dto.RoleObject;
import org.iringtools.adapter.dto.RoleType;
import org.iringtools.adapter.dto.TemplateObject;
import org.iringtools.adapter.dti.TransferType;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.protocol.manifest.Manifest;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.common.request.ExchangeRequest;
import org.iringtools.common.request.DtoPageRequest;
import org.iringtools.common.request.DiffDtiRequest;
import org.iringtools.common.request.DiffDtoRequest;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.StatusList;
import org.iringtools.common.response.Messages;
import org.iringtools.services.core.DataTransferObjectComparator;
import org.iringtools.utility.NetUtil;
import org.iringtools.utility.JaxbUtil;

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
      String diffServiceUrl = settings.get("directoryServiceUri") + "/directory";
      directory = NetUtil.get(Directory.class, diffServiceUrl);
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

      // get target manifest
      String targetManifestUrl = xdef.getTargetUri() + "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp()
          + "/manifest";
      Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);
      if (targetManifest == null || targetManifest.getGraphs().getGraphs().size() == 0)
        return null;

      // get source dti
      String sourceUrl = xdef.getSourceUri() + "/" + xdef.getSourceScope() + "/" + xdef.getSourceApp() + "/"
          + xdef.getSourceGraph() + "/xfr?hashAlgorithm=" + xdef.getHashAlgorithm();
      DataTransferIndices sourceDtis = NetUtil.post(DataTransferIndices.class, sourceUrl, targetManifest);

      // get target dti
      String targetUrl = xdef.getTargetUri() + "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp() + "/"
          + xdef.getTargetGraph() + "/xfr?hashAlgorithm=" + xdef.getHashAlgorithm();
      DataTransferIndices targetDtis = NetUtil.post(DataTransferIndices.class, targetUrl, targetManifest);

      // create dxi request to diff source and target dti
      DiffDtiRequest diffDtiRequest = new DiffDtiRequest();
      diffDtiRequest.setSourceDataTransferIndicies(sourceDtis);
      diffDtiRequest.setTargetDataTransferIndicies(targetDtis);

      // request exchange service to diff the dti
      String diffServiceUrl = settings.get("differencingServiceUri") + "/dti";
      resultDtis = NetUtil.post(DataTransferIndices.class, diffServiceUrl, diffDtiRequest);
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

      // get target manifest
      String targetManifestUrl = xdef.getTargetUri() + "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp()
          + "/manifest";
      Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);
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

        String sourceUrl = xdef.getSourceUri() + "/" + xdef.getSourceScope() + "/" + xdef.getSourceApp() + "/"
            + xdef.getSourceGraph() + "/xfr/page";
        sourceDtos = NetUtil.post(DataTransferObjects.class, sourceUrl, sourceDtoPageRequest);
        List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObjects();

        // append add/sync DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < sourceDtoList.size(); i++)
        {
          DataTransferObject sourceDto = sourceDtoList.get(i);
          
          if (sourceDto.getClassObjects() != null && sourceDto.getClassObjects().getClassObjects().size() > 0)
          {
            String sourceDtoIdentifier = sourceDto.getClassObjects().getClassObjects().get(0).getIdentifier();
  
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

        String targetUrl = xdef.getTargetUri() + "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp() + "/"
            + xdef.getTargetGraph() + "/xfr/page";
        targetDtos = NetUtil.post(DataTransferObjects.class, targetUrl, targetDtoPageRequest);
        List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObjects();

        // append delete DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < targetDtoList.size(); i++)
        {
          DataTransferObject targetDto = targetDtoList.get(i);
          
          if (targetDto.getClassObjects() != null && targetDto.getClassObjects().getClassObjects().size() > 0)
          {
            String targetDtoIdentifier = targetDto.getClassObjects().getClassObjects().get(0).getIdentifier();
  
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
        String diffServiceUrl = settings.get("differencingServiceUri") + "/dto";
        DataTransferObjects diffDtos = NetUtil.post(DataTransferObjects.class, diffServiceUrl, diffDtoRequest);

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

      // get target application uri
      String targetAppUri = xdef.getTargetUri() + "/" + xdef.getTargetScope() + "/" + xdef.getTargetApp();
      String targetManifestUrl = targetAppUri + "/manifest";
      String targetGraphUri = targetAppUri + "/" + xdef.getTargetGraph();
      String sourceGraphUri = xdef.getSourceUri() + "/" + xdef.getSourceScope() + "/" + xdef.getSourceApp()
          + "/" + xdef.getSourceGraph();

      // get target manifest
      Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);
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
        
        String sourceDtoUrl = sourceGraphUri + "/xfr/page";
        DataTransferObjects poolDtos = NetUtil.post(DataTransferObjects.class, sourceDtoUrl, poolDtosRequest);
        List<DataTransferObject> poolDtoList = poolDtos.getDataTransferObjects();

        // set transfer type for each DTO in poolDtoList and remove/report ones that have changed
        // and deleted during review and acceptance period
        for (int j = 0; j < poolDtoList.size(); j++)
        {
          DataTransferObject sourceDto = poolDtoList.get(j);
          
          if (sourceDto.getClassObjects() != null && sourceDto.getClassObjects().getClassObjects().size() > 0)
          {
            String identifier = sourceDto.getClassObjects().getClassObjects().get(0).getIdentifier();
  
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

        // report DTOs that were deleted during review and acceptance period
        if (exchangeRequest.isReviewed() && sourcePoolDtiList.size() > 0)
        {
          for (DataTransferIndex sourceDti : sourcePoolDtiList)
          {
            Status status = createStatus(sourceDti.getIdentifier(), "DTO no longer exists.");
            response.getStatusList().getStatuses().add(status);
          }
        }

        // create identifiers for deleted DTOs
        for (DataTransferIndex deleteDti : deleteDtiList)
        {
          DataTransferObject deleteDto = new DataTransferObject();
          deleteDto.setTransferType(org.iringtools.adapter.dto.TransferType.DELETE);

          ClassObjects classObjects = new ClassObjects();
          deleteDto.setClassObjects(classObjects);

          List<ClassObject> classObjectList = classObjects.getClassObjects();
          ClassObject classObject = new ClassObject();
          classObject.setIdentifier(deleteDti.getIdentifier());
          classObjectList.add(classObject);

          poolDtoList.add(deleteDto);
        }

        // post add/change/delete DTOs to target endpoint
        Response poolResponse = NetUtil.post(Response.class, targetGraphUri, poolDtos);
        response.getStatusList().getStatuses().addAll(poolResponse.getStatusList().getStatuses());
      }

      response.setLevel(Level.SUCCESS);
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

  private ExchangeDefinition getExchangeDefinition(String scope, String id) throws JAXBException, IOException
  {
    String directoryServiceUrl = settings.get("directoryServiceUri") + "/" + scope + "/exchanges/" + id;
    return NetUtil.get(ExchangeDefinition.class, directoryServiceUrl);
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
          if (roleObject.getType() == RoleType.PROPERTY)
          {
            String value = roleObject.getValue();
            if (value == null)
              value = "";
            values.append(value);
          }
        }
      }
    }

    return DigestUtils.md5Hex(values.toString());
  }
}