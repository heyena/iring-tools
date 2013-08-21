package org.iringtools.library.exchange;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import org.apache.log4j.Logger;
import org.iringtools.directory.Exchange;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjectList;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.JaxbUtils;

public class DtoTask implements Runnable
{
  private static final Logger logger = Logger.getLogger(DtoTask.class);

  private Map<String, Object> settings;
  private Exchange exchange;
  private Manifest manifest;
  private List<DataTransferIndex> dtiList;
  private DataTransferObjects dtos;
  private RequestStatus requestStatus;

  public DtoTask(Map<String, Object> settings, Exchange exchange, Manifest manifest, List<DataTransferIndex> dtiList,
      RequestStatus requestStatus)
  {
    this.settings = settings;
    this.exchange = exchange;
    this.manifest = manifest;
    this.dtiList = dtiList;
    this.requestStatus = requestStatus;
  }

  public void processDxoRequest() throws Exception
  {
    DataTransferObjects sourceDtos = null;
    DataTransferObjects targetDtos = null;

    dtos = new DataTransferObjects();

    DataTransferObjectList resultDtoList = new DataTransferObjectList();
    dtos.setDataTransferObjectList(resultDtoList);
    List<DataTransferObject> resultDtoListItems = resultDtoList.getItems();

    List<DataTransferIndex> sourceDtiItems = new ArrayList<DataTransferIndex>();
    List<DataTransferIndex> targetDtiItems = new ArrayList<DataTransferIndex>();

    for (DataTransferIndex dti : dtiList)
    {
      if (dti.getDuplicateCount() != null && dti.getDuplicateCount() > 1)
      {
        logger.warn("DTI [" + dti.getIdentifier() + "] has [" + dti.getDuplicateCount() + "] duplicates.");
        // continue;
      }

      DataTransferIndex sourceDti, targetDti;
      int splitIndex;

      switch (dti.getTransferType())
      {
      case ADD:
        sourceDtiItems.add(dti);
        break;
      case CHANGE:
        splitIndex = dti.getInternalIdentifier().indexOf(Constants.CHANGE_TOKEN);

        sourceDti = new DataTransferIndex();
        sourceDti.setIdentifier(dti.getIdentifier());
        sourceDti.setHashValue(dti.getHashValue());
        sourceDti.setSortIndex(dti.getSortIndex());
        sourceDti.setInternalIdentifier(dti.getInternalIdentifier().substring(0, splitIndex));
        sourceDti.setTransferType(dti.getTransferType());
        sourceDti.setDuplicateCount(dti.getDuplicateCount());
        sourceDtiItems.add(sourceDti);

        targetDti = new DataTransferIndex();
        targetDti.setIdentifier(dti.getIdentifier());
        targetDti.setHashValue(dti.getHashValue());
        targetDti.setSortIndex(dti.getSortIndex());
        targetDti.setInternalIdentifier(dti.getInternalIdentifier().substring(splitIndex + 2));
        targetDti.setTransferType(dti.getTransferType());
        targetDti.setDuplicateCount(dti.getDuplicateCount());
        targetDtiItems.add(targetDti);
        break;
      case SYNC:
        splitIndex = dti.getInternalIdentifier().indexOf(Constants.CHANGE_TOKEN);

        sourceDti = new DataTransferIndex();
        sourceDti.setIdentifier(dti.getIdentifier());
        sourceDti.setHashValue(dti.getHashValue());
        sourceDti.setSortIndex(dti.getSortIndex());
        sourceDti.setInternalIdentifier(dti.getInternalIdentifier().substring(0, splitIndex));
        sourceDti.setTransferType(dti.getTransferType());
        sourceDti.setDuplicateCount(dti.getDuplicateCount());
        sourceDtiItems.add(sourceDti);
        break;
      case DELETE:
        targetDtiItems.add(dti);
        break;
      }
    }

    String sourceRelativePath = "/" + exchange.getSourceScope() + "/" + exchange.getSourceApp() + "/"
        + exchange.getSourceGraph() + "/dxo";

    String targetRelativePath = "/" + exchange.getTargetScope() + "/" + exchange.getTargetApp() + "/"
        + exchange.getTargetGraph() + "/dxo";

    Manifest sourceManifest = null;
    Manifest targetManifest = null;
    DtoSubTask sourceDtoTask = null;
    DtoSubTask targetDtoTask = null;

    sourceManifest = JaxbUtils.clone(Manifest.class, manifest);
    sourceManifest.getGraphs().getItems().get(0).setName(exchange.getSourceGraph());

    targetManifest = JaxbUtils.clone(Manifest.class, manifest);
    targetManifest.getGraphs().getItems().get(0).setName(exchange.getTargetGraph());

    int numOfDtoTasks = (sourceDtiItems.size() > 0 && targetDtiItems.size() > 0) ? 2 : 1;
    ExecutorService executor = Executors.newFixedThreadPool(numOfDtoTasks);

    if (sourceDtiItems.size() > 0)
    {
      sourceDtoTask = new DtoSubTask(settings, exchange.getSourceUri(), sourceRelativePath, sourceManifest,
          sourceDtiItems);
      executor.execute(sourceDtoTask);
    }

    if (targetDtiItems.size() > 0)
    {
      targetDtoTask = new DtoSubTask(settings, exchange.getTargetUri(), targetRelativePath, targetManifest,
          targetDtiItems);
      executor.execute(targetDtoTask);
    }

    executor.shutdown();

    executor.awaitTermination(Long.parseLong((String) settings.get("dtoTaskTimeout")), TimeUnit.SECONDS);

    if (sourceDtoTask != null)
    {
      sourceDtos = sourceDtoTask.getDataTransferObjects();

      Graph graph = sourceManifest.getGraphs().getItems().get(0);

      for (ClassTemplates classTemplates : graph.getClassTemplatesList().getItems())
      {
        String path = classTemplates.getClazz().getPath();
        String id = classTemplates.getClazz().getId();
        int index = classTemplates.getClazz().getIndex();
        
        for (DataTransferObject dataTransferObject : sourceDtos.getDataTransferObjectList().getItems())
        {
          for (ClassObject classObject : dataTransferObject.getClassObjects().getItems())
            if (classObject.getClassId().equals(id)
                && (classObject.getPath() == null ? path == null : classObject.getPath().equals(path)))
            {
              classObject.setIndex(index);
            }
        }
      }

      if (sourceDtos != null)
      {
        sourceDtos.setScopeName(exchange.getSourceScope());
        sourceDtos.setAppName(exchange.getSourceApp());
        List<DataTransferObject> sourceDtoListItems = sourceDtos.getDataTransferObjectList().getItems();

        // append add/sync DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < sourceDtoListItems.size(); i++)
        {
          DataTransferObject sourceDto = sourceDtoListItems.get(i);
          String sourceDtoIdentifier = sourceDto.getInternalIdentifier();

          if (sourceDto.getClassObjects() != null)
          {
            for (DataTransferIndex sourceDti : sourceDtiItems)
            {
              if (sourceDtoIdentifier.equalsIgnoreCase(sourceDti.getInternalIdentifier()))
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
        targetDtos.setScopeName(exchange.getTargetScope());
        targetDtos.setAppName(exchange.getTargetApp());
        List<DataTransferObject> targetDtoListItems = targetDtos.getDataTransferObjectList().getItems();

        // append delete DTOs to resultDtoList, leave change DTOs to send to differencing engine
        for (int i = 0; i < targetDtoListItems.size(); i++)
        {
          DataTransferObject targetDto = targetDtoListItems.get(i);
          String targetDtoIdentifier = targetDto.getInternalIdentifier();

          if (targetDto.getClassObjects() != null)
          {
            for (DataTransferIndex targetDti : targetDtiItems)
            {
              if (targetDtoIdentifier.equalsIgnoreCase(targetDti.getInternalIdentifier()))
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

    DifferencingProvider df = new DifferencingProvider();
    DataTransferObjects dxoList = df.diff(manifest, sourceDtos, targetDtos);

    if (dxoList != null)
    {
      resultDtoListItems.addAll(dxoList.getDataTransferObjectList().getItems());
    }
      
    List<DataTransferObject> orderedDtoListItems = sortDtos(resultDtoListItems, dtiList);
    resultDtoList.setItems(orderedDtoListItems);
  }

  // sort data transfer objects as data transfer indices
  private List<DataTransferObject> sortDtos(List<DataTransferObject> dtos, List<DataTransferIndex> dtis)
  {
    List<DataTransferObject> sortedList = new ArrayList<DataTransferObject>();

    for (DataTransferIndex dti : dtis)
    {
      for (DataTransferObject dto : dtos)
      {
        if (dti.getIdentifier().equalsIgnoreCase(dto.getIdentifier()))
        {
          if (dti.getHasContent() != null)
          {
            dto.setHasContent(dti.getHasContent());
          }

          dto.setDuplicateCount(dti.getDuplicateCount());
          sortedList.add(dto);
          break;
        }
      }
    }

    return sortedList;
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

      processDxoRequest();

      if (requestStatus != null)
      {
        requestStatus.setResponseText(JaxbUtils.toXml(dtos, false));
        requestStatus.setState(State.COMPLETED);
      }
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      e.printStackTrace();

      requestStatus.setMessage(e.getMessage());
      requestStatus.setState(State.ERROR);
    }
  }

  public DataTransferObjects getDataTransferObjects()
  {
    return dtos;
  }
}

class DtoSubTask implements Runnable
{
  private static final Logger logger = Logger.getLogger(DtoTask.class);
  private Map<String, Object> settings;
  private String serviceUri;
  private String relativePath;
  private Manifest manifest;
  private List<DataTransferIndex> dtiItems;
  private DataTransferObjects dtos;

  public DtoSubTask(final Map<String, Object> settings, String serviceUri, String relativePath, Manifest manifest,
      List<DataTransferIndex> dtiItems)
  {
    this.settings = settings;
    this.serviceUri = serviceUri;
    this.relativePath = relativePath;
    this.manifest = manifest;
    this.dtiItems = dtiItems;
  }

  @Override
  public void run()
  {
    try
    {
      DataTransferIndices indices = new DataTransferIndices();
      DataTransferIndexList dtiList = new DataTransferIndexList();
      dtiList.setItems(dtiItems);
      indices.setDataTransferIndexList(dtiList);

      DxoRequest dxoRequest = new DxoRequest();
      dxoRequest.setManifest(manifest);
      dxoRequest.setDataTransferIndices(indices);

      HttpClient httpClient = new HttpClient(serviceUri + relativePath);
      HttpUtils.addHttpHeaders(settings, httpClient);

      if (isAsync())
      {
        httpClient.setAsync(true);
        String statusURL = httpClient.post(String.class, dxoRequest);
        dtos = waitForRequestCompletion(DataTransferObjects.class, serviceUri + statusURL);
      }
      else
      {
        dtos = httpClient.post(DataTransferObjects.class, dxoRequest);
      }
    }
    catch (Exception e)
    {
      logger.error("Error getting dxo: " + e.getMessage());
      e.printStackTrace();
    }
  }

  public DataTransferObjects getDataTransferObjects()
  {
    return dtos;
  }

  protected <T> T waitForRequestCompletion(Class<T> clazz, String url)
  {
    T obj = null;

    try
    {
      RequestStatus requestStatus = null;
      long timeout = (Long) settings.get("asyncTimeout") * 1000; // convert to milliseconds
      long interval = (Long) settings.get("pollingInterval") * 1000; // convert to milliseconds
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

      // Note that the requestStatus object will have been decoded (out of UTF-8) during the httpClient.get(), so if the
      // object embedded within the
      // requestStatus.ResponseText has non UTF-8 characters then we must encode that back into UTF-8 before passing to
      // JaxbUtils.toObject
      InputStream streamUTF8 = new ByteArrayInputStream(requestStatus.getResponseText().getBytes("UTF-8"));
      obj = (T) JaxbUtils.toObject(clazz, streamUTF8);
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
      e.printStackTrace();
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
