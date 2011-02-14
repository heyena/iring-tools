package org.iringtools.services.core;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.GregorianCalendar;
import java.util.Hashtable;
import java.util.List;

import javax.xml.bind.JAXBException;
import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.commons.codec.digest.DigestUtils;
import org.apache.commons.lang.StringUtils;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;
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
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.manifest.TransferOption;
import org.iringtools.dxfr.request.DfiRequest;
import org.iringtools.dxfr.request.DfoRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.IOUtil;
import org.iringtools.utility.JaxbUtil;

public class ESBServiceProvider
{
  private static final Logger logger = Logger.getLogger(ESBServiceProvider.class);
  private Hashtable<String, String> settings;
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
  
  public ESBServiceProvider(Hashtable<String, String> settings) throws DatatypeConfigurationException
  {
    this.settings = settings;
    datatypeFactory = DatatypeFactory.newInstance();
    httpClient = new HttpClient();
  }
  
  public Directory getDirectory() throws JAXBException, IOException
  {
    Directory directory = null;

    try
    {
      logger.debug("getDirectory()");
      directory = httpClient.get(Directory.class, settings.get("directoryServiceUri") + "/directory");
    }
    catch (Exception ex)
    {
      logger.error("Error : getDirectory(): " + ex);
    }

    return directory;
  }
  
  public DataTransferIndices getDataTransferIndices(String scope, String id, DataFilter dataFilter)
  {
    DataTransferIndices dxiList = null;

    try
    {
      logger.debug("getDataTransferIndices(" + scope + "," + id + ")");

      // init exchange definition
      initExchangeDefinition(scope, id);

      Manifest crossedManifest = getCrossedManifest();
      DxiRequest dxiRequest = new DxiRequest();
       
      dxiRequest.setDataFilter(dataFilter);
      dxiRequest.setManifest(crossedManifest);
      
      // get source dti
      String sourceDtiUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;
      DataTransferIndices sourceDtis = httpClient.post(DataTransferIndices.class, sourceDtiUrl, dxiRequest);
      
      if (sourceDtis != null)
      {
        sourceDtis.setScopeName(sourceScopeName);
        sourceDtis.setAppName(sourceAppName);
      }      

      // get target dti
      String targetDtiUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/dxi/filter?hashAlgorithm=" + hashAlgorithm;
      DataTransferIndices targetDtis = httpClient.post(DataTransferIndices.class, targetDtiUrl, dxiRequest);
      
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
      dfiRequest.getDataTransferIndicies().add(sourceDtis);
      dfiRequest.getDataTransferIndicies().add(targetDtis);

      // request exchange service to diff the dti
      String dxiUrl = settings.get("differencingServiceUri") + "/dxi";
      dxiList = httpClient.post(DataTransferIndices.class, dxiUrl, dfiRequest);
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return dxiList;
  }

  public DataTransferObjects getDataTransferObjects(String scope, String id, DataTransferIndices dtis)
  {
    DataTransferObjects resultDtos = new DataTransferObjects();
    DataTransferObjects sourceDtos = null;
    DataTransferObjects targetDtos = null;

    try
    {
      logger.debug("getDataTransferObjects(" + scope + "," + id + "," + JaxbUtil.toXml(dtis, true) + ")");

      DataTransferObjectList resultDtoList = new DataTransferObjectList();
      resultDtos.setDataTransferObjectList(resultDtoList);
      List<DataTransferObject> resultDtoListItems = resultDtoList.getItems();

      // init exchange definition
      initExchangeDefinition(scope, id);
      
      Manifest crossedManifest = getCrossedManifest();
      
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
        sourceDxoRequest.setManifest(crossedManifest);
        DataTransferIndices sourceDtis = new DataTransferIndices();
        DataTransferIndexList sourceDtiList = new DataTransferIndexList();        
        sourceDtiList.setItems(sourceDtiListItems);
        sourceDtis.setDataTransferIndexList(sourceDtiList);
        sourceDxoRequest.setDataTransferIndices(sourceDtis);

        String sourceDtoUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName + "/dxo";
        sourceDtos = httpClient.post(DataTransferObjects.class, sourceDtoUrl, sourceDxoRequest);
        
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
        targetDxoRequest.setManifest(crossedManifest);
        DataTransferIndices targetDtis = new DataTransferIndices();
        DataTransferIndexList targetDtiList = new DataTransferIndexList();
        targetDtiList.setItems(targetDtiListItems);
        targetDtis.setDataTransferIndexList(targetDtiList);
        targetDxoRequest.setDataTransferIndices(targetDtis);

        String targetDtoUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/" + targetGraphName + "/dxo";
        targetDtos = httpClient.post(DataTransferObjects.class, targetDtoUrl, targetDxoRequest);
        
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

      if (sourceDtos != null && targetDtos != null)
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
        DataTransferObjects dxoList = httpClient.post(DataTransferObjects.class, dxoUrl, dfoRequest);

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
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }

    return resultDtos;
  }

  public ExchangeResponse submitExchange(String scope, String id, ExchangeRequest exchangeRequest) throws IOException, JAXBException
  {
    ExchangeResponse exchangeResponse = new ExchangeResponse();
    GregorianCalendar gcal = new GregorianCalendar();
    exchangeResponse.setStartTimeStamp(datatypeFactory.newXMLGregorianCalendar(gcal));
    StatusList statusList = new StatusList();
    exchangeResponse.setStatusList(statusList);
    exchangeResponse.setLevel(Level.SUCCESS);

    try
    {
      logger.debug("submitExchange(" + scope + "," + id + "," + JaxbUtil.toXml(exchangeRequest, true) + ")");
      
      if (exchangeRequest == null)
        return null;

      DataTransferIndices dtis = exchangeRequest.getDataTransferIndices();
      
      if (dtis == null)
        return null;

      List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
      
      if (dtiList.size() == 0)
        return null;

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
      String targetGraphUrl = targetAppUrl + "/" + targetGraphName;
      String sourceGraphUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/" + sourceGraphName;

      Manifest crossedManifest = getCrossedManifest();
      
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

        if (sourcePoolDtiList.size() > 0)
        {
          // request source DTOs
          DxoRequest poolDxoRequest = new DxoRequest();
          poolDxoRequest.setManifest(crossedManifest);
          DataTransferIndices poolDataTransferIndices = new DataTransferIndices();
          poolDxoRequest.setDataTransferIndices(poolDataTransferIndices);
          DataTransferIndexList poolDtiList = new DataTransferIndexList();
          poolDataTransferIndices.setDataTransferIndexList(poolDtiList);
          poolDtiList.setItems(sourcePoolDtiList);
          
          String sourceDtoUrl = sourceGraphUrl + "/dxo";
          DataTransferObjects poolDtos = httpClient.post(DataTransferObjects.class, sourceDtoUrl, poolDxoRequest);
          List<DataTransferObject> poolDtoListItems = poolDtos.getDataTransferObjectList().getItems();
  
          // set transfer type for each DTO : poolDtoList and remove/report ones that have changed
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
                      {
                        exchangeResponse.setLevel(Level.WARNING);
                      }
                      
                      poolDtoListItems.remove(j--); 
                    }
                    else if (dti.getTransferType() == TransferType.SYNC)
                    {
                      poolDtoListItems.remove(j--);  // exclude SYNC DTOs
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
            
            if (exchangeResponse.getLevel() != Level.ERROR || 
               (exchangeResponse.getLevel() == Level.WARNING && poolResponse.getLevel() == Level.SUCCESS))
            {
              exchangeResponse.setLevel(poolResponse.getLevel());
              
              // find the errors and warnings to put in the summary
              for (Status status : poolResponse.getStatusList().getItems())
              {
                String messages = StringUtils.join(status.getMessages().getItems(), " ");                
                if (!messages.contains("successful"))
                {
                  exchangeResponse.getMessages().getItems().addAll(status.getMessages().getItems());
                }
              }
            }
          }
        }
      }
      
      List<Status> statusItems = exchangeResponse.getStatusList().getItems();
      
      if (statusItems != null && statusItems.size() == 0)
      {
        exchangeResponse.setLevel(Level.WARNING);
        
        Messages messages = new Messages();
        messages.getItems().add("No Add/Change/Delete DTOs are found!");
        exchangeResponse.setMessages(messages);
      }
      else if (exchangeResponse.getMessages() == null)
      {
        if (exchangeResponse.getLevel() == Level.WARNING)
        {
          Messages messages = new Messages();
          messages.getItems().add("Exchange completed with some warnings!");
          exchangeResponse.setMessages(messages);
        }
        else
        {
          Messages messages = new Messages();
          messages.getItems().add("Exchange completed succesfully!");
          exchangeResponse.setMessages(messages);
        }
      }
    }
    catch (Exception ex)
    {
      String error = "Error while posting DTOs: " + ex;      
      logger.error(error);
      
      exchangeResponse.setLevel(Level.ERROR);      
      Status status = new Status();      
      List<String> messages = new ArrayList<String>();
      messages.add(error);
      StatusList statuses = new StatusList();
      List<Status> statusItems = statusList.getItems();
      statusItems.add(status);
      exchangeResponse.setStatusList(statuses);
    }
    
    XMLGregorianCalendar timestamp = datatypeFactory.newXMLGregorianCalendar(new GregorianCalendar());
    exchangeResponse.setEndTimeStamp(timestamp);
    
    // save exchange response to file system
    String path = settings.get("baseDirectory") + "/WEB-INF/logs/exchanges/" + scope + "/" + id;
    File dirPath = new File(path);
    
    if (!dirPath.exists())
    {
      dirPath.mkdirs();
    }
    
    String file = path + "/" + timestamp.toString().replace(":", ".") + ".xml";
    JaxbUtil.write(exchangeResponse, file, true);
    
    List<String> filesInFolder = IOUtil.getFiles(path);
    Collections.sort(filesInFolder);
    
    // if number of log files exceed the limit, remove the oldest ones
    while (filesInFolder.size() > Integer.valueOf(settings.get("numOfExchangeLogFiles")))
    {
      File oldestFile = new File(path + "/" + filesInFolder.get(0));      
      oldestFile.delete();
      filesInFolder.remove(0);
    }
    
    // only interest in summary
    exchangeResponse.setStatusList(null);

    return exchangeResponse;
  }

  private void initExchangeDefinition(String scope, String id) throws HttpClientException 
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
  
  private Manifest getCrossedManifest() throws Exception
  {
    // get source manifest
    String sourceManifestUrl = sourceUri + "/" + sourceScopeName + "/" + sourceAppName + "/manifest";
    Manifest sourceManifest = httpClient.get(Manifest.class, sourceManifestUrl);
    
    if (sourceManifest == null || sourceManifest.getGraphs().getItems().size() == 0)
      return null;

    // get target manifest
    String targetManifestUrl = targetUri + "/" + targetScopeName + "/" + targetAppName + "/manifest";
    Manifest targetManifest = httpClient.get(Manifest.class, targetManifestUrl);
    
    if (targetManifest == null || targetManifest.getGraphs().getItems().size() == 0)
      return null;
    
    Graph sourceGraph = sourceManifest.getGraphs().getItems().get(0);
    Graph targetGraph = targetManifest.getGraphs().getItems().get(0);
    List<ClassTemplates> sourceClassTemplatesList = sourceGraph.getClassTemplatesList().getItems();
    List<ClassTemplates> targetClassTemplatesList = targetGraph.getClassTemplatesList().getItems();
            
    for (int i = 0; i < targetClassTemplatesList.size(); i++)
    {
      org.iringtools.dxfr.manifest.Class targetClass = targetClassTemplatesList.get(i).getClazz();
      ClassTemplates sourceClassTemplates = getClassTemplates(sourceClassTemplatesList, targetClass.getId());
      
      if (sourceClassTemplates != null)
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
              throw new Exception("Required template [" + targetTemplate.getId() + "] not found");
            }
            else
            {
              targetTemplates.remove(j--);
            }
          }
          else
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
    
    return targetManifest;
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
