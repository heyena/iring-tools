package org.iringtools.library.exchange;

import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import org.apache.log4j.Logger;
import org.iringtools.common.response.Response;
import org.iringtools.directory.Exchange;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Graphs;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.manifest.TransferOption;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.mapping.ValueListMaps;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class ExchangeProvider
{
  private static final Logger logger = Logger.getLogger(ExchangeProvider.class);
  private static ConcurrentMap<String, RequestStatus> requests = new ConcurrentHashMap<String, RequestStatus>();
  private Map<String, Object> settings;
  private String path;
  
  public ExchangeProvider(Map<String, Object> settings)
  {
    this.settings = settings;
    this.path = settings.get("basePath").toString()
			.concat("WEB-INF/data/");
  }

  public Manifest getCrossedManifest(Exchange exchange) throws Exception
  {
				  
	Manifest crossedManifest = new Manifest();

    String sourceManifestUrl = exchange.getSourceUri() + "/" + exchange.getSourceScope() + "/"
        + exchange.getSourceApp() + "/" + exchange.getSourceGraph() + "/manifest";

    String targetManifestUrl = exchange.getTargetUri() + "/" + exchange.getTargetScope() + "/"
        + exchange.getTargetApp() + "/" + exchange.getTargetGraph() + "/manifest";

    ExecutorService executor = Executors.newFixedThreadPool(2);

    ManifestTask sourceManifestTask = new ManifestTask(settings, sourceManifestUrl);
    executor.execute(sourceManifestTask);

    ManifestTask targetManifestTask = new ManifestTask(settings, targetManifestUrl);
    executor.execute(targetManifestTask);

    executor.shutdown();

    try
    {
      executor.awaitTermination(Long.parseLong((String) settings.get("manifestTaskTimeout")), TimeUnit.SECONDS);
    }
    catch (InterruptedException e)
    {
      logger.error("Manifest Task Executor interrupted: " + e.getMessage());
    }

    Manifest sourceManifest = sourceManifestTask.getManifest();
    if (sourceManifest == null || sourceManifest.getGraphs().getItems().size() == 0)
      throw new Exception(sourceManifestTask.getError());

    Manifest targetManifest = targetManifestTask.getManifest();
    if (targetManifest == null || targetManifest.getGraphs().getItems().size() == 0)
      throw new Exception(targetManifestTask.getError());

    Graph sourceGraph = getGraph(sourceManifest, exchange.getSourceGraph());
    Graph targetGraph = getGraph(targetManifest, exchange.getTargetGraph());

    if (sourceGraph != null && sourceGraph.getClassTemplatesList() != null && targetGraph != null
        && targetGraph.getClassTemplatesList() != null)
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
                throw new Exception("Required template [" + targetTemplate.getName() + "] not found");
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
                else
                {
                	if(targetRoles.get(k).getDataType() != null &&
                	    sourceRole.getDataType() != null &&
                	    targetRoles.get(k).getDataType().equalsIgnoreCase("xsd:dateTime") && 
                		  sourceRole.getDataType().equalsIgnoreCase("xsd:date"))
                	{
                		targetRoles.get(k).setDataType("xsd:date");
                	}
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

    if (sourceManifest.getValueListMaps() != null)
    {
      valueListMaps.getItems().addAll(sourceManifest.getValueListMaps().getItems());
    }

    if (targetManifest.getValueListMaps() != null)
    {
      valueListMaps.getItems().addAll(targetManifest.getValueListMaps().getItems());
    }

    crossedManifest.setValueListMaps(valueListMaps);
    
    return crossedManifest;
  }

  public Manifest GetCachedCrossedManifest(Exchange exchange)
  {
	  try
	  {
		  String filePath = buildManifestFilePath(exchange)  ;
		  if (IOUtils.fileExists(filePath)) {
			  return JaxbUtils.read(Manifest.class, filePath);
		  }
	  }catch (Exception e) {
		  String message = "Error saving manifest" + e;
		  logger.error(message);

	  }
	  return null;
  }
  
  
  public void saveCrossedManifest(Manifest manifest,Exchange exchange)throws Exception {
	  try {

		  String filePath = buildManifestFilePath(exchange)  ;

		  JaxbUtils.write(manifest, filePath, false);
	  } catch (Exception e) {
		  String message = "Error saving manifest" + e;
		  logger.error(message);
		  throw e;
	  }
  }
  
  public void deleteCachedCrossedManifest(Exchange exchange)throws Exception
  {
	  try {
		  String filePath = buildManifestFilePath(exchange)  ;
		  if (IOUtils.fileExists(filePath)) {
			  IOUtils.deleteFile(filePath);
		  }
	  } catch (Exception e) {
		  String message = "Error saving manifest" + e.getMessage();
		  logger.error(message);
		  throw e;

	  }

  }

  private String buildManifestFilePath(Exchange exchange)throws Exception
  {
	  try{
		  String filePath = path + "manifest." + exchange.getSourceScope() + "."+exchange.getSourceApp() + "." +exchange.getSourceGraph() ;
		  filePath = filePath +  "." + exchange.getTargetScope() + "."+exchange.getTargetApp() + "." +exchange.getTargetGraph() + ".xml" ; 

		  return filePath;
	  } catch (Exception e) {
		  String message = "Error in creating manifest path" + e.getMessage();
		  logger.error(message);
		  throw e;

	  }

  }
  
  public DataTransferIndices getDataTransferIndices(Exchange exchange, DxiRequest dxiRequest) throws Exception
  {
    DataTransferIndices dtis = null;

    try
    {
      ExecutorService executor = Executors.newSingleThreadExecutor();

      DtiTask dtiTask = new DtiTask(settings, exchange, dxiRequest, null);
      executor.execute(dtiTask);
      executor.shutdown();

      executor.awaitTermination(60, TimeUnit.MINUTES);

      if (dtiTask.getError() == null)
      {
        dtis = dtiTask.getDataTransferIndices();
      }
      else
      {
        throw new Exception(dtiTask.getError());
      }
    }
    catch (Exception e)
    {
      throw new Exception(e.getMessage());
    }

    return dtis;
  }

  public List<String> getIdentifiers(Exchange exchange, DxiRequest dxiRequest) throws Exception
  {
    ExecutorService executor = Executors.newSingleThreadExecutor();

    IdTask idTask = new IdTask(settings, exchange, dxiRequest, null);
    executor.execute(idTask);
    executor.shutdown();

    executor.awaitTermination(60, TimeUnit.MINUTES);

    List<String> ids = idTask.getIdentifiers();

    //
    // remove duplicate ids but retain id orders to honor sorting
    //
    List<String> uniqueIds = new ArrayList<String>();

    for (String id : ids)
    {
      if (!uniqueIds.contains(id))
      {
        uniqueIds.add(id);
      }
    }

    return uniqueIds;
  }

  public DataTransferObjects getDataTransferObjects(Exchange exchange, Manifest manifest,
      List<DataTransferIndex> dtiList) throws Exception
  {
    ExecutorService executor = Executors.newSingleThreadExecutor();

    DtoTask dtoTask = new DtoTask(settings, exchange, manifest, dtiList, null);
    executor.execute(dtoTask);
    executor.shutdown();

    try
    {
      executor.awaitTermination(60, TimeUnit.MINUTES);
    }
    catch (InterruptedException e)
    {
      logger.error(e);
      throw new Exception(e.getMessage());
    }

    DataTransferObjects dtos = dtoTask.getDataTransferObjects();
    return dtos;
  }

  public ExchangeResponse submitExchange(boolean async, String scope, String id, Exchange exchange, ExchangeRequest xReq)
  {
    logger.debug("Processing data exchange [" + scope + "." + id + "]...");

    if (async)
    {
      String requestId = UUID.randomUUID().toString().replace("-", "");
      RequestStatus requestStatus = new RequestStatus();
      requestStatus.setState(State.IN_PROGRESS);
      requests.put(requestId, requestStatus);

      ExecutorService executor = Executors.newSingleThreadExecutor();
      ExchangeTask exchangeTask = new ExchangeTask(settings, scope, id, exchange, xReq, requestStatus);
      executor.execute(exchangeTask);
      executor.shutdown();

      ExchangeResponse exchangeResponse = exchangeTask.getExchangeResponse();
      return exchangeResponse;
    }
    else
    {
      ExecutorService executor = Executors.newSingleThreadExecutor();
      ExchangeTask exchangeTask = new ExchangeTask(settings, scope, id, exchange, xReq, null);
      executor.execute(exchangeTask);
      executor.shutdown();

      try
      {
        executor.awaitTermination(24, TimeUnit.HOURS);
      }
      catch (InterruptedException e)
      {
        logger.error("Exchange Task Executor interrupted: " + e.getMessage());
      }

      ExchangeResponse exchangeResponse = exchangeTask.getExchangeResponse();
      return exchangeResponse;
    }
  }

  public History getExchangeHistory(String scope, String id) throws Exception
  {
    return getExchangeHistory(scope, id, 0);
  }

  public History getExchangeHistory(String scope, String id, int limit) throws Exception
  {
    History history = new History();
    List<ExchangeResponse> responses = new ArrayList<ExchangeResponse>();
    history.setExchangeResponses(responses);

    String path = settings.get("basePath") + "/WEB-INF/exchanges/" + scope + "/" + id;

    try
    {
      File file = new File(path);

      if (file.exists())
      {
        List<String> exchangeLogs = IOUtils.getFiles(path);

        // filter out pool logs
        for (int i = 0; i < exchangeLogs.size(); i++)
        {
          if (exchangeLogs.get(i).contains(Constants.XITEM_PREFIX))
          {
            exchangeLogs.remove(i--);
          }
        }

        // show most recent first
        Collections.sort(exchangeLogs);

        // may just want the x most recent history, where x=limit
        int actualLimit = limit;
        if (actualLimit > 0)
        {
          // note we loop from biggest to smallest so actualLimit is an "inverted" value, also the array is zero based
          // so if there were 10 logs and we only wanted the most recent one we'd want to loop while i >= 9
          actualLimit = (limit > exchangeLogs.size() ? 0 : exchangeLogs.size() - limit);
        }

        for (int i = exchangeLogs.size() - 1; i >= actualLimit; i--)
        {
          ExchangeResponse response = JaxbUtils.read(ExchangeResponse.class, path + "/" + exchangeLogs.get(i));
          responses.add(response);
        }
      }
      else
      {
        throw new Exception("No exchange history found.");
      }
    }
    catch (Exception e)
    {
      String message = "Error getting exchange history: " + e;
      logger.error(message);
      throw new Exception(message);
    }

    return history;
  }

  public Response getExchangeResponse(String scope, String id, String timestamp, int start, int limit) throws Exception
  {
    String path = settings.get("basePath") + "/WEB-INF/exchanges/" + scope + "/" + id;
    String exchangeFile = path + "/" + timestamp + Constants.XITEM_PREFIX + (start + 1) + "-" + (start + limit)
        + ".xml";

    return JaxbUtils.read(Response.class, exchangeFile);
  }

  public RequestStatus getRequestStatus(String id)
  {
    RequestStatus requestStatus = null;

    try
    {
      if (requests.containsKey(id))
      {
        requestStatus = requests.get(id);
      }
      else
      {
        requestStatus = new RequestStatus();
        requestStatus.setState(State.NOT_FOUND);
        requestStatus.setMessage("Request [" + id + "] not found.");
      }

      if (requestStatus.getState() == State.COMPLETED)
      {
        requests.remove(id);
      }
    }
    catch (Exception e)
    {
      requestStatus.setState(State.ERROR);
      requestStatus.setMessage(e.getMessage());
      requests.remove(id);
    }

    return requestStatus;
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
      if (template != null && template.getId() != null && template.getId().equals(templateId))
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
