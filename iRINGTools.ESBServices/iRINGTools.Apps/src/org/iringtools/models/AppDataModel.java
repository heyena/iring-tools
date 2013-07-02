package org.iringtools.models;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.dxfr.content.ContentObject;
import org.iringtools.dxfr.content.ContentObjects;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.grid.Grid;

public class AppDataModel extends DataModel
{
  private static final Logger logger = Logger.getLogger(AppDataModel.class);

  public AppDataModel(Map<String, Object> settings, Map<String, Object> session)
  {
    super(DataMode.APP, settings, session);
  }

  public Grid getDtoGrid(String serviceUri, String scopeName, String appName, String graphName, String filter,
      String sortBy, String sortOrder, int start, int limit) throws Exception
  {
    scope = scopeName;
    app = appName;

    String graphPath = "/" + scopeName + "/" + appName + "/" + graphName;

    try
    {
      HttpClient httpClient = new HttpClient(serviceUri);
      HttpUtils.addHttpHeaders(settings, httpClient);

      String manifestPath = graphPath + "/manifest";
      Manifest manifest = getManifest(serviceUri, manifestPath);

      if (manifest != null && manifest.getGraphs() != null)
      {
        Graph graph = getGraph(manifest, graphName);

        if (graph != null)
        {
          DataFilter dataFilter = createDataFilter(filter, sortBy, sortOrder);

          String dtiPath = graphPath + "/dti?start=" + start + "&limit=" + limit;
          DataTransferIndices pageDtis = null;

          if (dataFilter != null && (dataFilter.getExpressions() != null || dataFilter.getOrderExpressions() != null))
          {
            pageDtis = httpClient.post(DataTransferIndices.class, dtiPath, dataFilter);
          }
          else
          {
            pageDtis = httpClient.get(DataTransferIndices.class, dtiPath);
          }

          Grid dtoGrid = null;

          if (pageDtis != null && pageDtis.getDataTransferIndexList() != null
              && pageDtis.getDataTransferIndexList().getItems().size() > 0)
          {
            collapseDuplicates(pageDtis);

            logger.debug("Getting a page of DTOs...");
            String dtoPath = graphPath + "/page";
            DataTransferObjects pageDtos = httpClient.post(DataTransferObjects.class, dtoPath, pageDtis);

            if (pageDtos.getDataTransferObjectList() != null)
            {
              List<DataTransferObject> sortedList = sortDtos(pageDtos.getDataTransferObjectList().getItems(),
                  pageDtis.getDataTransferIndexList().getItems());
              pageDtos.getDataTransferObjectList().setItems(sortedList);
            }

            dtoGrid = createDtoGrid(serviceUri, graphPath, manifest, graph, pageDtos);
            dtoGrid.setTotal(pageDtis.getTotalCount());
          }
          else
          {
            dtoGrid = createDtoGrid(serviceUri, graphPath, manifest, graph, null);
          }

          return dtoGrid;
        }
      }
    }
    catch (HttpClientException e)
    {
      e.printStackTrace();
      logger.error(e.toString());
      throw new DataModelException(e);
    }

    return null;
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

  // TODO: complete implementation
  // public Grid getRelatedDtoGrid(String serviceUri, String scopeName, String appName, String graphName, String
  // dtoIdentifier,
  // String classId, String classIdentifier, String filter, String sortBy, String sortOrder, int start, int limit)
  // throws DataModelException
  // {
  // scope = scopeName;
  // app = appName;
  //
  // String appRelativePath = "/" + scopeName + "/" + appName + "/" + graphName;
  // String dtiRelativePath = appRelativePath + "/" + "/dxi/filter";
  // String dtoRelativePath = appRelativePath + "/" + "/dxo";
  // String manifestRelativePath = appRelativePath + "/manifest";
  //
  // Grid pageDtoGrid = null;
  // Manifest manifest = getManifest(serviceUri, manifestRelativePath);
  // Graph graph = getGraph(manifest, graphName);
  //
  // if (graph != null)
  // {
  // DataTransferObjects dtos = getRelatedItems(serviceUri, manifestRelativePath, dtiRelativePath, dtoRelativePath,
  // dtoIdentifier, filter, sortBy, sortOrder, start, limit);
  //
  // pageDtoGrid = getRelatedItemGrid(appRelativePath, manifest, graph, dtos, classId, classIdentifier);
  // }
  //
  // return pageDtoGrid;
  // }

  public ContentObject getContent(String targetUri) throws HttpClientException
  {
    HttpClient httpClient = new HttpClient(targetUri);
    HttpUtils.addHttpHeaders(settings, httpClient);

    ContentObjects contentObjects = httpClient.get(ContentObjects.class);

    return contentObjects.getContentObject().get(0);
  }
}
