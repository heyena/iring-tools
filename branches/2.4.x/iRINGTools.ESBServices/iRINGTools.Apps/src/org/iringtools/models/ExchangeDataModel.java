package org.iringtools.models;

import java.io.IOException;
import java.util.ArrayList;
import java.util.GregorianCalendar;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expression;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.LogicalOperator;
import org.iringtools.data.filter.OrderExpression;
import org.iringtools.data.filter.OrderExpressions;
import org.iringtools.data.filter.RelationalOperator;
import org.iringtools.directory.Application;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Scope;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Class;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.ClassTemplatesList;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Graphs;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.manifest.Templates;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.library.Applications;
import org.iringtools.library.RequestStatus;
import org.iringtools.library.Scopes;
import org.iringtools.library.directory.DirectoryProvider;
import org.iringtools.library.exchange.Constants;
import org.iringtools.library.exchange.ExchangeProvider;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;

public class ExchangeDataModel extends DataModel
{
  private static final Logger logger = Logger.getLogger(ExchangeDataModel.class);
  private ExchangeProvider provider;
  private DirectoryProvider dprovider;

  public ExchangeDataModel(Map<String, Object> settings, Map<String, Object> session)
  {
    super(DataMode.EXCHANGE, settings, session);
    provider = new ExchangeProvider(settings);
    dprovider = new DirectoryProvider(settings);
  }

  public Grid getDtoGrid(String serviceUri, String scope, String xId, String filter, String sortBy, String sortOrder,
      int start, int limit) throws Exception
  {
    this.scope = scope;
    this.xId = xId;

    Grid dtoGrid = null;

    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = getCrossedManifest(exchange, scope, xId);

    //
    // store the last filter being applied DTIs so exchange submission will not need to provide
    //
    String exchangeKey = EXCHANGE_PREFIX + "." + scope + "." + xId;
    session.put(exchangeKey + ".filter", filter);
    session.put(exchangeKey + ".sortOrder", sortOrder);
    session.put(exchangeKey + ".sortBy", sortBy);

    DataTransferIndices dtis = getDtis(exchange, manifest, filter, sortOrder, sortBy);

    if (dtis != null && dtis.getDataTransferIndexList() != null)
    {
      Graph graph = manifest.getGraphs().getItems().get(0);

      //
      // get a page of DTIs
      //
      List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
      int actualLimit = Math.min(start + limit, dtiList.size());
      List<DataTransferIndex> pageDtis = dtiList.subList(start, actualLimit);

      //
      // send a page of DTIs to get DTOs
      //
      DataTransferObjects dtos = provider.getDataTransferObjects(exchange, manifest, pageDtis);

      //
      // remove duplicate DTOs if any to show only one representative DTO
      //
      if (dtos != null && dtos.getDataTransferObjectList() != null)
      {
        if (dtiList.size() < dtos.getDataTransferObjectList().getItems().size())
        {
          List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();

          for (DataTransferIndex dti : dtiList)
          {
            if (dti.getDuplicateCount() != null && dti.getDuplicateCount() > 1)
            {
              for (int i = 0; i < dtoList.size(); i++)
              {
                if (dti.getInternalIdentifier().equalsIgnoreCase(dtoList.get(i).getInternalIdentifier()))
                {
                  // found one, search and remove the rest
                  for (int j = i; j < dtoList.size(); j++)
                  {
                    if (dti.getInternalIdentifier().equalsIgnoreCase(dtoList.get(j).getInternalIdentifier()))
                    {
                      dtoList.remove(j--);
                    }
                  }

                  break;
                }
              }
            }
          }
        }

        String dtiRelativePath = "/" + scope + "/" + xId;
        dtoGrid = createDtoGrid(serviceUri, dtiRelativePath, manifest, graph, dtos);
        dtoGrid.setTotal(dtiList.size());
      }
    }

    return dtoGrid;
  }

  //
  // get pre-exchange summary
  //
  @SuppressWarnings("unchecked")
  public Map<String, String> getPreSummaryGrid(String serviceUri, String scope, String xId) throws Exception
  {
    this.scope = scope;
    this.xId = xId;

    // retrieve last filter
    String exchangeKey = EXCHANGE_PREFIX + "." + scope + "." + xId;
    String filter = (String) session.get(exchangeKey + ".filter");
    String sortOrder = (String) session.get(exchangeKey + ".sortOrder");
    String sortBy = (String) session.get(exchangeKey + ".sortBy");

    String preSummaryKey = PRE_SUMMARY_PREFIX + "." + scope + "." + xId + "." + filter;
    Map<String, String> data = null;

    if (session.containsKey(preSummaryKey))
    {
      data = (Map<String, String>) session.get(preSummaryKey);
    }
    else
    {
      data = new HashMap<String, String>();

      Exchange exchange = getExchange(scope, xId);
      Manifest manifest = getCrossedManifest(exchange, scope, xId);

      DataTransferIndices dtis = getDtis(exchange, manifest, filter, sortOrder, sortBy);

      if (dtis != null && dtis.getDataTransferIndexList() != null)
      {
        int adds = 0, changes = 0, deletes = 0, syncs = 0;

        for (DataTransferIndex dti : dtis.getDataTransferIndexList().getItems())
        {
          switch (dti.getTransferType())
          {
          case ADD:
            adds++;
            break;

          case CHANGE:
            changes++;
            break;

          case DELETE:
            deletes++;
            break;

          case SYNC:
            syncs++;
            break;
          }
        }

        data.put("SenderApplication", exchange.getSourceScope() + "." + exchange.getSourceApp());
        data.put("SenderGraph", exchange.getSourceGraph());
        data.put("SenderURI", exchange.getSourceUri());
        data.put("ReceiverApplication", exchange.getTargetScope() + "." + exchange.getTargetApp());
        data.put("ReceiverGraph", exchange.getTargetGraph());
        data.put("ReceiverURI", exchange.getTargetUri());
        data.put("TotalCount", String.valueOf(dtis.getDataTransferIndexList().getItems().size()));
        data.put("AddingCount", String.valueOf(adds));
        data.put("ChangingCount", String.valueOf(changes));
        data.put("DeletingCount", String.valueOf(deletes));
        data.put("SynchronizingCount", String.valueOf(syncs));
        data.put("PoolSize", String.valueOf(exchange.getPoolSize()));

        session.put(preSummaryKey, data);
      }
    }

    return data;
  }

  public ExchangeResponse submitExchange(String serviceUri, String scope, String xId, boolean reviewed)
      throws Exception
  {
    this.scope = scope;
    this.xId = xId;

    ExchangeResponse xRes = new ExchangeResponse();

    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = getCrossedManifest(exchange, scope, xId);

    //
    // retrieve last filter from cache
    //
    String exchangeKey = EXCHANGE_PREFIX + "." + scope + "." + xId;
    String filter = (String) session.get(exchangeKey + ".filter");
    String sortOrder = (String) session.get(exchangeKey + ".sortOrder");
    String sortBy = (String) session.get(exchangeKey + ".sortBy");

    DataTransferIndices dtis = getDtis(exchange, manifest, filter, sortOrder, sortBy);

    ExchangeRequest xReq = new ExchangeRequest();
    xReq.setManifest(manifest);
    xReq.setDataTransferIndices(dtis);
    xReq.setReviewed(reviewed);

    try
    {
      xRes = provider.submitExchange(isAsync, scope, xId, exchange, xReq);
    }
    catch (Exception ex)
    {
      String error = "Error during exchange: " + ex.getMessage();
      logger.error(error);

      String noHtmlError = error.replaceAll("(<html)[^&]*(</html>)", "");
      String noDocTypeError = noHtmlError.replaceAll("(<!DOCTYPE)[^&]*(>)", "");
      xRes.setSummary(noDocTypeError.replaceAll("\\r\\n", ""));

      xRes.setLevel(Level.ERROR);
    }

    //
    // TODO: refresh only updated DTI cache entries
    //
    if (xRes.getLevel() == Level.SUCCESS)
    {
      String dtiKeyPrefix = DTI_PREFIX + "." + scope + "." + xId;

      for (String key : session.keySet())
        if (key.startsWith(dtiKeyPrefix))
          session.remove(key);
    }

    return xRes;
  }

  public RequestStatus getExchangeRequestStatus(String scope, String xId) throws Exception
  {
    // TODO get exchange status from provider and return;
    RequestStatus requestStatus = provider.getExchangeRequestStatus(scope, xId);
    return requestStatus;
  }

  public Grid getXHistoryGrid(String scope, String xId, String xlabel) throws Exception
  {
    Grid xHistoryGrid = new Grid();

    History xHistory = provider.getExchangeHistory(scope, xId);

    if (xHistory != null && xHistory.getExchangeResponses().size() > 0)
    {
      List<ExchangeResponse> xrs = xHistory.getExchangeResponses();
      List<Field> fields = new ArrayList<Field>();
      List<List<String>> data = new ArrayList<List<String>>();

      Field field = new Field();
      field.setName("&nbsp;");
      field.setDataIndex("&nbsp;");
      field.setType("string");
      field.setWidth(28);
      field.setFixed(true);
      field.setFilterable(false);
      fields.add(field);

      field = new Field();
      field.setName("Start Time");
      field.setDataIndex("Start Time");
      field.setType("string");
      field.setWidth(150);
      fields.add(field);

      field = new Field();
      field.setName("End Time");
      field.setDataIndex("End Time");
      field.setType("string");
      field.setWidth(150);
      fields.add(field);

      field = new Field();
      field.setName("Sender");
      field.setDataIndex("Sender");
      field.setType("string");
      field.setWidth(180);
      fields.add(field);

      field = new Field();
      field.setName("Receiver");
      field.setDataIndex("Receiver");
      field.setType("string");
      field.setWidth(180);
      fields.add(field);

      field = new Field();
      field.setName("Result");
      field.setDataIndex("Result");
      field.setType("string");
      field.setWidth(260);
      fields.add(field);

      for (ExchangeResponse xr : xrs)
      {
        List<String> row = new ArrayList<String>();
        GregorianCalendar startTime = xr.getStartTime().toGregorianCalendar();
        String formattedStartTime = format(startTime);
        long xTime = startTime.getTimeInMillis();
        GregorianCalendar endTime = xr.getEndTime().toGregorianCalendar();

        row.add("<input type=\"image\" src=\"resources/images/info-small.png\" "
            + "onClick='javascript:createPageXlogs(\"" + scope + "\",\"" + xId + "\",\"" + xlabel + "\",\""
            + formattedStartTime + "\",\"" + xTime + "\"," + xr.getPoolSize() + "," + xr.getItemCount() + ")'>");

        row.add(formattedStartTime);
        row.add(format(endTime));
        row.add(xr.getSenderScope() + "." + xr.getSenderApp() + "." + xr.getSenderGraph());
        row.add(xr.getReceiverScope() + "." + xr.getReceiverApp() + "." + xr.getReceiverGraph());
        String summary = xr.getSummary();
        // String resultSummary = summary.replaceAll("\\<.*?\\>", "");
        String resultSummary = summary.replaceAll("(<html)[^&]*(</html>)", "");
        String romoveDoc = resultSummary.replaceAll("(<!DOCTYPE)[^&]*(>)", "");
        row.add(romoveDoc.replaceAll("\\r\\n", ""));

        data.add(row);
      }

      xHistoryGrid.setTotal(xrs.size());
      xHistoryGrid.setFields(fields);
      xHistoryGrid.setData(data);
    }

    return xHistoryGrid;
  }

  public Grid getPageXHistoryGrid(String scope, String xId, String xtime, int start, int limit, int itemCount)
      throws Exception
  {
    Grid pageXHistoryGrid = new Grid();

    int actualLimit = (start + limit > itemCount) ? (itemCount - start) : limit;
    Response response = provider.getExchangeResponse(scope, xId, xtime, start, actualLimit);
    List<Status> statuses = response.getStatusList().getItems();

    pageXHistoryGrid.setIdentifier(xtime);
    pageXHistoryGrid.setDescription(xtime);
    List<Field> fields = new ArrayList<Field>();
    pageXHistoryGrid.setFields(fields);

    List<List<String>> data = new ArrayList<List<String>>();
    pageXHistoryGrid.setData(data);

    Field field = new Field();
    field.setName("Identifier");
    field.setDataIndex("Identifier");
    field.setType("string");
    fields.add(field);

    field = new Field();
    field.setName("Result");
    field.setWidth(120);
    field.setDataIndex("Result");
    field.setType("string");
    fields.add(field);

    for (int i = 0; i < statuses.size(); i++)
    {
      List<String> row = new ArrayList<String>();
      StringBuilder messages = new StringBuilder();

      Status status = statuses.get(i);
      row.add(status.getIdentifier());

      for (String message : status.getMessages().getItems())
      {
        if (messages.length() > 0)
          messages.append(" ");

        messages.append(message);
      }

      row.add(messages.toString());
      data.add(row);
    }

    pageXHistoryGrid.setTotal(itemCount);

    return pageXHistoryGrid;
  }

  // merge data filter expressions, remove duplicate expressions and exclude transfer type expressions
  protected DataFilter mergeDataFilters(DataFilter filter1, DataFilter filter2, List<Expression> transferTypeExpressions)
  {
    DataFilter dataFilter = new DataFilter();
    Expressions expressions = new Expressions();
    dataFilter.setExpressions(expressions);
    OrderExpressions orderExpressions = new OrderExpressions();
    dataFilter.setOrderExpressions(orderExpressions);

    if (filter1 != null)
    {
      if (filter1.getExpressions() != null)
      {
        for (Expression expression : filter1.getExpressions().getItems())
        {
          if (expression.getPropertyName().equalsIgnoreCase("transfer type"))
          {
            transferTypeExpressions.add(expression);
          }
          else
          {
            if (expressions.getItems().size() > 0 && expression.getLogicalOperator() == null)
              expression.setLogicalOperator(LogicalOperator.AND);

            expressions.getItems().add(expression);
          }
        }
      }

      if (filter1.getOrderExpressions() != null)
      {
        for (OrderExpression orderExpression : filter1.getOrderExpressions().getItems())
        {
          orderExpressions.getItems().add(orderExpression);
        }
      }
    }

    if (filter2 != null)
    {
      if (filter2.getExpressions() != null)
      {
        for (Expression expression : filter2.getExpressions().getItems())
        {
          if (expression.getPropertyName().equalsIgnoreCase("transfer type"))
          {
            if (!transferTypeExpressions.contains(expression))
            {
              transferTypeExpressions.add(expression);
            }
          }
          else if (!expressions.getItems().contains(expression))
          {
            if (expressions.getItems().size() > 0 && expression.getLogicalOperator() == null)
              expression.setLogicalOperator(LogicalOperator.AND);

            expressions.getItems().add(expression);
          }
        }
      }

      if (filter2.getOrderExpressions() != null)
      {
        for (OrderExpression orderExpression : filter2.getOrderExpressions().getItems())
        {
          if (!orderExpressions.getItems().contains(orderExpression))
          {
            orderExpressions.getItems().add(orderExpression);
          }
        }
      }
    }

    return dataFilter;
  }

  protected void applyTransferTypeFilterToDtis(DataTransferIndices dtis, List<Expression> transferTypeExpressions)
  {
    if (transferTypeExpressions != null && transferTypeExpressions.size() > 0 && dtis != null
        && dtis.getDataTransferIndexList() != null && dtis.getDataTransferIndexList().getItems().size() > 0)
    {
      List<DataTransferIndex> indices = dtis.getDataTransferIndexList().getItems();

      for (int i = 0; i < indices.size(); i++)
      {
        TransferType transferType = indices.get(i).getTransferType();
        boolean found = false;

        // NOTE: only accept OR logical operators
        for (Expression expression : transferTypeExpressions)
        {
          String value = expression.getValues().getItems().get(0);
          if (expression.getRelationalOperator() == RelationalOperator.EQUAL_TO)
          {
            if (value.equalsIgnoreCase(transferType.toString()))
            {
              found = true;
              break;
            }
          }
          if (expression.getRelationalOperator() == RelationalOperator.NOT_EQUAL_TO)
          {
            if (!value.equalsIgnoreCase(transferType.toString()))
            {
              found = true;
              break;
            }
          }
        }

        if (!found)
        {
          indices.remove(i--);
        }
      }
    }
  }

  protected DataFilter separateTransferTypeExpressions(DataFilter inFilter, List<Expression> transferTypeExpressions)
  {
    DataFilter outFilter = new DataFilter();

    if (inFilter != null)
    {
      Expressions expressions = new Expressions();
      outFilter.setExpressions(expressions);

      if (inFilter.getExpressions() != null)
      {
        for (Expression expression : inFilter.getExpressions().getItems())
        {
          if (expression.getPropertyName().equalsIgnoreCase("transfer type"))
          {
            transferTypeExpressions.add(expression);
          }
          else
          {
            expressions.getItems().add(expression);
          }
        }
      }

      if (inFilter.getOrderExpressions() != null)
      {
        outFilter.setOrderExpressions(inFilter.getOrderExpressions());
      }
    }

    return outFilter;
  }

  protected DataTransferIndices getFullDtis(Exchange exchange, Manifest manifest) throws Exception
  {
    DataTransferIndices dtis = null;

    try
    {
      List<Expression> transferTypeExpressions = new ArrayList<Expression>();

      DataFilter preFilter = exchange.getDataFilter();
      DataFilter filter = separateTransferTypeExpressions(preFilter, transferTypeExpressions);

      DxiRequest dxiRequest = new DxiRequest();
      dxiRequest.setManifest(manifest);
      dxiRequest.setDataFilter(filter);

      dtis = provider.getDataTransferIndices(exchange, dxiRequest);

      if (transferTypeExpressions.size() > 0)
      {
        applyTransferTypeFilterToDtis(dtis, transferTypeExpressions);
      }
    }
    catch (Exception ex)
    {
      logger.error("Error getting data transfer indices: " + ex.toString());
      throw new Exception(ex.getMessage());
    }

    return dtis;
  }

  protected DataTransferIndices getFilteredDtis(Exchange exchange, DataTransferIndices fullDtis, Manifest manifest,
      DataFilter uiFilter)
  {
    DataTransferIndices dtis = new DataTransferIndices();

    try
    {
      List<Expression> transferTypeExpressions = new ArrayList<Expression>();

      DataFilter preFilter = exchange.getDataFilter();
      DataFilter filter = mergeDataFilters(preFilter, uiFilter, transferTypeExpressions);

      //
      // if filter contains only transfer type expression(s), then no need to fetch data but use the full DTIs
      //
      if (filter == null
          || (filter.getExpressions().getItems().size() == 0 && filter.getOrderExpressions().getItems().size() == 0))
      {
        dtis = getFullDtis(exchange, manifest);
      }
      else
      {
        DxiRequest dxiRequest = new DxiRequest();
        dxiRequest.setManifest(manifest);
        dxiRequest.setDataFilter(filter);

        List<String> ids = provider.getIdentifiers(exchange, dxiRequest);

        if (ids != null)
        {
          DataTransferIndexList dtiList = new DataTransferIndexList();
          dtis.setDataTransferIndexList(dtiList);

          for (String id : ids)
          {
            for (DataTransferIndex dti : fullDtis.getDataTransferIndexList().getItems())
            {
              if (dti.getInternalIdentifier().equalsIgnoreCase(id)) // handle add, delete, sync
              {
                //
                // make sure no duplicates in dtiList
                //
                // boolean found = false;
                //
                // for (DataTransferIndex existingDti : dtiList.getItems())
                // {
                // if (existingDti.getHashValue().equals(dti.getHashValue()))
                // {
                // found = true;
                // break;
                // }
                // }
                //
                // if (!found)
                // {
                // dtiList.getItems().add(dti);
                // }

                dtiList.getItems().add(dti);
                break;
              }
              else if (dti.getInternalIdentifier().contains(Constants.CHANGE_TOKEN)) // handle change
              {
                String[] internalIds = dti.getInternalIdentifier().split(Constants.CHANGE_TOKEN);

                if (id.equals(internalIds[0]) || id.equals(internalIds[1]))
                {
                  //
                  // make sure no duplicates in dtiList
                  //
                  // boolean found = false;
                  //
                  // for (DataTransferIndex existingDti : dtiList.getItems())
                  // {
                  // if (existingDti.getHashValue().equals(dti.getHashValue()))
                  // {
                  // found = true;
                  // break;
                  // }
                  // }
                  //
                  // if (!found)
                  // {
                  // dtiList.getItems().add(dti);
                  // }

                  dtiList.getItems().add(dti);
                  break;
                }
              }
            }
          }
        }
      }

      List<DataTransferIndex> indices = dtis.getDataTransferIndexList().getItems();
      if (uiFilter != null)
      {
        if (uiFilter.getExpressions() != null)
        {
          for (int i = 0; i < indices.size(); i++)
          {
            String transferType = indices.get(i).getTransferType().toString();

            for (Expression expression : uiFilter.getExpressions().getItems())
            {
              if (expression.getPropertyName().equalsIgnoreCase("Transfer Type"))
              {
                String value = expression.getValues().getItems().get(0);
                if (!value.equalsIgnoreCase(transferType))
                {
                  indices.remove(i--);
                  break;
                }
              }
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      logger.error("Error getting data transfer indices: " + ex.toString());
    }

    return dtis;
  }

  protected DataTransferIndices getDtis(Exchange exchange, Manifest manifest, String filter, String sortOrder,
      String sortBy) throws Exception
  {
    DataTransferIndices dtis = null;

    if (manifest != null && manifest.getGraphs() != null)
    {
      Graph graph = manifest.getGraphs().getItems().get(0);

      if (graph != null)
      {
        String fullDtiKey = DTI_PREFIX + "." + scope + "." + xId;

        if (!session.containsKey(fullDtiKey))
        {
          String preSummaryKey = PRE_SUMMARY_PREFIX + "." + scope + "." + xId + "." + filter;
          if (session.containsKey(preSummaryKey))
            session.remove(preSummaryKey);
          
          dtis = getFullDtis(exchange, manifest);
          session.put(fullDtiKey, dtis);
        }

        if (filter == null && sortOrder == null && sortBy == null)
        {
          dtis = (DataTransferIndices) session.get(fullDtiKey);
        }
        else
        {
          String dtiKey = DTI_PREFIX + "." + scope + "." + xId + "." + filter + "." + sortOrder + "." + sortBy;

          //
          // get filtered DTIs from cache or query if not exist
          //
          if (session.containsKey(dtiKey))
          {
            dtis = (DataTransferIndices) session.get(dtiKey);
          }
          else
          {
            DataFilter uiFilter = createDataFilter(filter, sortBy, sortOrder);
            DataTransferIndices fullDtis = (DataTransferIndices) session.get(fullDtiKey);
            dtis = getFilteredDtis(exchange, fullDtis, manifest, uiFilter);
            session.put(dtiKey, dtis);
          }
        }
      }
    }

    return dtis;
  }

  // get exchange definition
  protected Exchange getExchange(String scope, String xId)
  {
    Exchange exchange = null;
    String exchangeKey = EXCHANGE_PREFIX + "." + scope + "." + xId;

    if (session.containsKey(exchangeKey))
    {
      exchange = (Exchange) session.get(exchangeKey);
    }
    else
    {
      exchange = getExchangeFromDirectory(scope, xId);
      session.put(exchangeKey, exchange);
    }

    return exchange;
  }

  protected Manifest getLatestCrossedManifest(Exchange exchange) throws Exception
  {
    return provider.getCrossedManifest(exchange);
  }

  protected Manifest getCrossedManifest(Exchange exchange, String scope, String xId) throws Exception
  {
    Manifest manifest = null;
    String manifestKey = MANIFEST_PREFIX + "." + scope + "." + xId;

    if (session.containsKey(manifestKey))
    {
      manifest = (Manifest) session.get(manifestKey);
    }
    else
    {
      manifest = getLatestCrossedManifest(exchange);
      session.put(manifestKey, manifest);
    }
    ClassTemplatesList deletedTemplateList = getExcludedTemplateList(scope, xId);
    filterCrossedManifest(manifest, deletedTemplateList);
    return manifest;
  }

  public void saveExcludedTemplateList(String scope, String xId) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    ClassTemplatesList deletedTemplateList = getExcludedTemplateList(scope, xId);
    provider.saveExcludedTemplateList(deletedTemplateList, exchange);
  }

  public void resetCrossedManifest(String scope, String xId) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    String filterKey = MANIFEST_FILTER_PREFIX + "." + scope + "." + xId;
    String manifestKey = MANIFEST_PREFIX + "." + scope + "." + xId;
    session.remove(filterKey);
    session.remove(manifestKey);
    provider.deleteExcludedTemplateList(exchange);
  }

  public void reloadCrossedManifest(String scope, String xId) throws Exception
  {
    String filterKey = MANIFEST_FILTER_PREFIX + "." + scope + "." + xId;
    String manifestKey = MANIFEST_PREFIX + "." + scope + "." + xId;
    session.remove(filterKey);
    session.remove(manifestKey);
  }

  public void addTemplateInDeletedList(String scope, String xId, String parentClassId, String parentClassIndex,
      String parentClassPath, String templateId, String templateIndex) throws Exception
  {
    List<ClassTemplates> removedTemplateList = getExcludedTemplateList(scope, xId).getItems();

    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = getCrossedManifest(exchange, scope, xId);

    Graph graph = manifest.getGraphs().getItems().get(0);
    List<ClassTemplates> classTemplateList = graph.getClassTemplatesList().getItems();

    for (ClassTemplates classTemplate : classTemplateList)
    {
      if (classTemplate.getClazz() != null)
      {
        // if ((classTemplate.getClazz().getPath().equals(parentClassPath)))
        if (classTemplate.getClazz().getId().equals(parentClassId)
            && (classTemplate.getClazz().getPath() == null ? parentClassPath == null : classTemplate.getClazz()
                .getPath().equals(parentClassPath)))

        {
          List<Template> templates = classTemplate.getTemplates().getItems();

          for (int j = 0; j < templates.size(); j++)
          {
            Template template = templates.get(j);
            if (template.getId().equals(templateId) && String.valueOf(template.getIndex()).equals(templateIndex))
            {
              boolean isClassFound = false;
              // templates.remove(j--);
              for (int k = 0; k < removedTemplateList.size(); k++)
              {
                Class cls = removedTemplateList.get(k).getClazz();
                if (cls != null && cls.getId().equals(parentClassId)
                    && (cls.getPath() == null ? parentClassPath == null : cls.getPath().equals(parentClassPath)))
                {
                  removedTemplateList.get(k).getTemplates().getItems().add(templates.get(j));
                  isClassFound = true;
                }
              }

              if (!isClassFound)
              {
                ClassTemplates cts = new ClassTemplates();
                cts.setClazz(classTemplate.getClazz());
                Templates temps = new Templates();
                cts.setTemplates(temps);
                temps.getItems().add(template);
                removedTemplateList.add(cts);
              }
            }
          }
        }
      }
    }

  }

  public void removeTemplateFromDeletedList(String scope, String xId, String parentClassId, String parentClassIndex,
      String parentClassPath, String templateId, String templateIndex) throws Exception
  {
    List<ClassTemplates> deletedTemplateList = getExcludedTemplateList(scope, xId).getItems();

    for (int i = 0; i < deletedTemplateList.size(); i++)
    {
      Class cls = deletedTemplateList.get(i).getClazz();
      if (cls != null && cls.getId().equals(parentClassId)
          && (cls.getPath() == null ? parentClassPath == null : cls.getPath().equals(parentClassPath)))
      {
        Templates templates = deletedTemplateList.get(i).getTemplates();
        if (templates != null)
        {
          for (int j = 0; j < templates.getItems().size(); j++)
          {
            Template tmp = templates.getItems().get(j);
            if (tmp.getId().equals(templateId) && templateIndex.endsWith(String.valueOf(tmp.getIndex())))
              templates.getItems().remove(j--);
          }
        }
      }
    }
  }

  public Manifest filterCrossedManifest(Manifest manifest, ClassTemplatesList deletedTemplateList) throws Exception
  {
    if (deletedTemplateList == null || deletedTemplateList.getItems().isEmpty())
      return manifest;

    for (int i = 0; i < deletedTemplateList.getItems().size(); i++)
    {
      Class cls = deletedTemplateList.getItems().get(i).getClazz();
      Templates templatesList = deletedTemplateList.getItems().get(i).getTemplates();

      if (cls == null || templatesList == null)
        continue;

      for (Template template : templatesList.getItems())
      {
        // Map<String,String> templateIdentifier = filter.get(i);
        String parentClassId = cls.getId();
        String parentClassPath = cls.getPath();
        String templateId = template.getId();
        String templateIndex = String.valueOf(template.getIndex());

        Graph graph = manifest.getGraphs().getItems().get(0);
        List<ClassTemplates> classTemplateList = graph.getClassTemplatesList().getItems();

        for (ClassTemplates classTemplate : classTemplateList)
        {
          if (classTemplate.getClazz() != null)
          {
            // if ((classTemplate.getClazz().getPath().equals(parentClassPath)))
            if (classTemplate.getClazz().getId().equals(parentClassId)
                && (classTemplate.getClazz().getPath() == null ? parentClassPath == null : classTemplate.getClazz()
                    .getPath().equals(parentClassPath)))
            {
              List<Template> templates = classTemplate.getTemplates().getItems();
              for (int j = 0; j < templates.size(); j++)
              {
                Template temp = templates.get(j);
                if (temp.getId().equals(templateId) && String.valueOf(temp.getIndex()).equals(templateIndex))
                {
                  templates.remove(j--);
                }
              }
            }
          }
        }
      }
    }

    return manifest;
  }

  private ClassTemplatesList getExcludedTemplateList(String scope, String xId) throws Exception
  {
    ClassTemplatesList filter;
    String filterKey = MANIFEST_FILTER_PREFIX + "." + scope + "." + xId;

    if (session.containsKey(filterKey))
    {
      filter = (ClassTemplatesList) session.get(filterKey);
    }
    else
    {
      Exchange exchange = getExchange(scope, xId);
      filter = provider.getExcludedTemplateList(exchange);
      session.put(filterKey, filter);
    }
    return filter;
  }

  public Tree getCrossedManifestTree(String scope, String xId) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    // Manifest manifest = getCrossedManifest(exchange, scope, xId);
    Manifest manifest = getLatestCrossedManifest(exchange);

    ClassTemplatesList deletedTemplateList = getExcludedTemplateList(scope, xId);

    Tree tree = manifestToTree(manifest, deletedTemplateList);
    return tree;
  }

  protected Tree manifestToTree(Manifest manifest, ClassTemplatesList deletedTemplateList) throws Exception
  {
    Tree tree = new Tree();

    try
    {
      Graph graph = manifest.getGraphs().getItems().get(0);
      Class rootClass = graph.getClassTemplatesList().getItems().get(0).getClazz();

      List<Node> nodes = tree.getNodes();
      TreeNode rootClassNode = new TreeNode();

      nodes.add(rootClassNode);

      rootClassNode.setText(graph.getName());
      rootClassNode.setIconCls("commodity");
      HashMap<String, String> graphProperties = rootClassNode.getProperties();
      graphProperties.put("Id", rootClass.getId());
      graphProperties.put("Name", rootClass.getName());
      graphProperties.put("Path", rootClass.getPath());
      graphProperties.put("ClassIndex", String.valueOf(rootClass.getIndex()));
      graphProperties.put("IsDeleted", String.valueOf(false));

      TemplateToTreeNode(rootClass, rootClassNode, graph, deletedTemplateList, false);
    }
    catch (Exception e)
    {
      e.printStackTrace();
    }

    return tree;
  }

  private void TemplateToTreeNode(Class parentClass, TreeNode parentClassNode, Graph graph,
      ClassTemplatesList deletedclsTemplateList, boolean isDeleted) throws Exception
  {
    try
    {
      List<ClassTemplates> classTemplateList = graph.getClassTemplatesList().getItems();
      for (ClassTemplates classTemplate : classTemplateList)
      {
        if (classTemplate.getClazz() != null && parentClass != null)
        {
          // if (parentClass.getPath().equals(classTemplate.getClazz().getPath()))
          if (parentClass.getId().equals(classTemplate.getClazz().getId())
              && (parentClass.getPath() == null ? classTemplate.getClazz().getPath() == null : parentClass.getPath()
                  .equals(classTemplate.getClazz().getPath())))
          {
            List<Node> templateNodes = parentClassNode.getChildren();

            for (Template template : classTemplate.getTemplates().getItems())
            {
              // check is template deleted or not
              boolean isTemplateDeleted = false;
              if (isDeleted)
              {
                isTemplateDeleted = true;
              }
              else
              {
                for (ClassTemplates deletedClassTemplates : deletedclsTemplateList.getItems())
                {
                  Class cls = deletedClassTemplates.getClazz();
                  if (cls != null && deletedClassTemplates.getTemplates() != null)
                  {
                    if (cls.getId().equals(classTemplate.getClazz().getId())
                        && (cls.getPath() == null ? classTemplate.getClazz().getPath() == null : cls.getPath().equals(
                            classTemplate.getClazz().getPath())))
                    {
                      List<Template> deletedTmpList = deletedClassTemplates.getTemplates().getItems();
                      for (Template tmp : deletedTmpList)
                      {
                        if (tmp.getId().equals(template.getId()) && tmp.getIndex() == template.getIndex())
                        {
                          isTemplateDeleted = true;
                          break;
                        }
                      }
                    }
                  }
                  if (isTemplateDeleted)
                    break;
                }
              }

              TreeNode templateNode = new TreeNode();
              templateNode.setText(template.getName());
              templateNode.setType("TemplateNode");
              if (!isTemplateDeleted)
                templateNode.setIconCls("template");
              else
                templateNode.setIconCls("deletedTemplate");
              templateNodes.add(templateNode);

              HashMap<String, String> templateProperties = templateNode.getProperties();
              templateProperties.put("Id", template.getId());
              templateProperties.put("Name", template.getName());
              templateProperties.put("TemplateIndex", String.valueOf(template.getIndex()));
              templateProperties.put("IsDeleted", String.valueOf(isTemplateDeleted));

              List<Node> roleNodes = templateNode.getChildren();

              for (Role role : template.getRoles().getItems())
              {
                if (role.getClazz() != null)
                {
                  // add tree node
                  TreeNode roleNode = new TreeNode();
                  roleNode.setText(role.getName());
                  roleNode.setIconCls("role");
                  roleNode.setType("RoleNode");
                  roleNodes.add(roleNode);

                  HashMap<String, String> roleProperties = roleNode.getProperties();
                  roleProperties.put("Id", role.getId());
                  roleProperties.put("IsDeleted", String.valueOf(isTemplateDeleted));

                  // add class node
                  List<Node> classNodes = roleNode.getChildren();
                  TreeNode classNode = new TreeNode();
                  classNode.setText(role.getClazz().getName());
                  classNode.setIconCls("class");
                  classNode.setType("ClassNode");
                  classNodes.add(classNode);

                  HashMap<String, String> classProperties = classNode.getProperties();
                  classProperties.put("Id", role.getClazz().getId());
                  classProperties.put("Name", role.getClazz().getName());
                  classProperties.put("Path", role.getClazz().getPath());
                  classProperties.put("ClassIndex", String.valueOf(role.getClazz().getIndex()));
                  classProperties.put("IsDeleted", String.valueOf(isTemplateDeleted));

                  TemplateToTreeNode(role.getClazz(), classNode, graph, deletedclsTemplateList, isTemplateDeleted);
                }
                else
                {
                  // add leafnode
                  LeafNode roleNode = new LeafNode();
                  roleNode.setText(role.getName());
                  roleNode.setIconCls("role");
                  roleNode.setType("RoleNode");
                  roleNodes.add(roleNode);

                  HashMap<String, String> roleProperties = roleNode.getProperties();
                  roleProperties.put("Id", role.getId());
                }
              }

            }

          }
        }
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
    }
  }

  protected String format(GregorianCalendar gcal)
  {
    return String.format("%1$tY/%1$tm/%1$td-%1$tH:%1$tM:%1$tS.%1$tL", gcal);
  }
  // TODO: complete implementation
  // public Grid getRelatedDtoGrid(String serviceUri, String scope, String xId, String dtoIdentifier, String classId,
  // String classIdentifier, String filter, String sortBy, String sortOrder, int start, int limit)
  // throws DataModelException
  // {
  // this.scope = scope;
  // this.xId = xId;
  //
  // String dtiRelativePath = "/" + scope + "/exchanges/" + xId;
  // String dtoRelativePath = dtiRelativePath + "/page";
  // String manifestRelativePath = dtiRelativePath + "/manifest";
  //
  // Grid pageDtoGrid = null;
  // Manifest manifest = getManifest(serviceUri, manifestRelativePath);
  // Graph graph = manifest.getGraphs().getItems().get(0);
  //
  // if (graph != null)
  // {
  // DataTransferObjects dtos = getRelatedItems(serviceUri, manifestRelativePath, dtiRelativePath, dtoRelativePath,
  // dtoIdentifier, filter, sortBy, sortOrder, start, limit);
  //
  // pageDtoGrid = getRelatedItemGrid(dtiRelativePath, manifest, graph, dtos, classId, classIdentifier);
  // }
  //
  // return pageDtoGrid;
  // }

  // TODO: apply start and limit and complete code
  // protected DataTransferObjects getRelatedItems(String serviceUri, String manifestRelativePath, String
  // dtiRelativePath,
  // String dtoRelativePath, String dtoIdentifier, String filter, String sortBy, String sortOrder, int start, int limit)
  // throws DataModelException
  // {
  // DataTransferObjects relatedDtos = new DataTransferObjects();
  // DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
  // List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList().getItems();
  //
  // for (DataTransferIndex dti : dtiList)
  // {
  // if (dti.getIdentifier().equals(dtoIdentifier))
  // {
  // DataTransferIndices requestDtis = new DataTransferIndices();
  // DataTransferIndexList dtiRequestList = new DataTransferIndexList();
  // requestDtis.setDataTransferIndexList(dtiRequestList);
  // dtiRequestList.getItems().add(dti);
  //
  // DxoRequest dxoRequest = new DxoRequest();
  // dxoRequest.setManifest(getManifest(serviceUri, manifestRelativePath));
  // dxoRequest.setDataTransferIndices(requestDtis);
  //
  // try
  // {
  // HttpClient httpClient = new HttpClient(serviceUri);
  // HttpUtils.addHttpHeaders(settings, httpClient);
  //
  // if (isAsync)
  // {
  // httpClient.setAsync(true);
  // String statusUrl = httpClient.post(String.class, dtoRelativePath, dxoRequest);
  //
  // try
  // {
  // relatedDtos = waitForRequestCompletion(DataTransferObjects.class, serviceUri + statusUrl);
  // }
  // catch (Exception ex)
  // {
  // throw new DataModelException(ex.getMessage());
  // }
  // }
  // else
  // {
  // relatedDtos = httpClient.post(DataTransferObjects.class, dtoRelativePath, dxoRequest);
  // }
  //
  // // apply filter
  // if (relatedDtos != null)
  // {
  // List<DataTransferObject> dtoList = relatedDtos.getDataTransferObjectList().getItems();
  //
  // if (dtoList.size() > 0 && filter != null && filter.length() > 0)
  // {
  // DataFilter dataFilter = createDataFilter(filter, sortBy, sortOrder);
  // List<Expression> expressions = dataFilter.getExpressions().getItems();
  //
  // for (Expression expression : expressions)
  // {
  // for (DataTransferObject dto : dtoList)
  // {
  // List<ClassObject> classObjects = dto.getClassObjects().getItems();
  // dto.getClassObjects().setItems(getFilteredClasses(expression, classObjects));
  // }
  // }
  // }
  // }
  // }
  // catch (HttpClientException e)
  // {
  // logger.error(e.getMessage());
  // throw new DataModelException(e.getMessage());
  // }
  // }
  // }
  //
  // return relatedDtos;
  // }
  //
  // //TODO: complete code
  // protected Grid getRelatedItemGrid(String fieldsContext, Manifest manifest, Graph graph, DataTransferObjects dtos,
  // String classId, String classIdentifier) throws DataModelException
  // {
  // Grid dtoGrid = new Grid();
  //
  // List<Field> fields = getFields(fieldsContext, graph, classId);
  // dtoGrid.setFields(fields);
  //
  // List<List<String>> gridData = new ArrayList<List<String>>();
  // dtoGrid.setData(gridData);
  //
  // List<DataTransferObject> dtoList = dtos.getDataTransferObjectList().getItems();
  //
  // for (int dtoIndex = 0; dtoIndex < dtoList.size(); dtoIndex++)
  // {
  // DataTransferObject dto = dtoList.get(dtoIndex);
  // List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();
  //
  // if (dto.getClassObjects().getItems().size() > 0)
  // {
  // for (ClassObject classObject : dto.getClassObjects().getItems())
  // {
  // if (classObject.getClassId().equalsIgnoreCase(classId)) // &&
  // // classObject.getIdentifier().equalsIgnoreCase(classIdentifier))
  // {
  // dtoGrid.setIdentifier(classObject.getClassId());
  // dtoGrid.setDescription(classObject.getName());
  //
  // List<String> rowData = new ArrayList<String>();
  //
  // // create a place holder for info field
  // rowData.add("");
  //
  // if (dataMode == DataMode.EXCHANGE)
  // {
  // String transferType = dto.getTransferType().toString();
  // rowData.add("<span class=\"" + transferType.toLowerCase() + "\">" + transferType + "</span>");
  // }
  //
  // processClassObject(manifest, graph, dto, dtoIndex, fields, classObject, dtoGrid, rowData, relatedClasses);
  //
  // String relatedClassesJson;
  //
  // try
  // {
  // relatedClassesJson = JSONUtil.serialize(relatedClasses);
  // }
  // catch (JSONException e)
  // {
  // relatedClassesJson = "[]";
  // }
  //
  // // update info field
  // rowData.set(
  // 0,
  // "<input type=\"image\" src=\"resources/images/info-small.png\" "
  // + "onClick='javascript:showIndividualInfo(\"" + dto.getIdentifier() + "\",\""
  // + classObject.getIdentifier() + "\"," + relatedClassesJson + ")'>");
  //
  // gridData.add(rowData);
  // }
  // }
  // }
  // }
  //
  // return dtoGrid;
  // }

  public void newExchangeConfig(Exchange form, String scope, String commodity, String serviceUri)
  {
    dprovider.postExchangeDefinition(form, scope, commodity);
  }

  public void editExchangeConfig(Exchange form, String scope, String commodity, String oldConfName, String serviceUri)
  {

    dprovider.editExchangeDefinition(form, scope, commodity, oldConfName);
  }

  public Exchange getCommodityConfigInfo(String com, String scope, String name, String serviceUri)
  {

    return dprovider.getCommodityConfigInfo(com, scope, name);
  }

  public void deleteExchangeConfig(String com, String scope, String name, String serviceUri)
  {

    dprovider.deleteExchangeConfig(com, scope, name);
  }

  public ExchangeResponse newScope(Scope scope, String serviceUri)
  {
    ExchangeResponse exres = dprovider.postNewScope(scope);
    return exres;
  }

  public ExchangeResponse editScope(String scope, String oldScope, String serviceUri)
  {
    ExchangeResponse exres = dprovider.postEditedScope(scope, oldScope);
    return exres;
  }

  public Scope getScopeInfo(String scope, String serviceUri)
  {
    return dprovider.getScopeInfo(scope);
  }

  public void deleteScope(String scope, String serviceUri)
  {
    dprovider.deleteScope(scope);
  }

  public ExchangeResponse newApplication(Application app, String scope, String serviceUri)
  {
    ExchangeResponse exres = dprovider.postApplication(app, scope);
    return exres;
  }

  public ExchangeResponse editApplication(Application app, String oldAppName, String scope)
  {
    ExchangeResponse exres = dprovider.editApplication(app, oldAppName, scope);
    return exres;
  }

  public void deleteApplication(String app, String scope, String serviceUri)
  {
    dprovider.deleteApplication(app, scope);
  }

  public Application getApplicationInfo(String app, String scope, String serviceUri)
  {

    return dprovider.getApplicationInfo(app, scope);
  }

  public ExchangeResponse newGraph(org.iringtools.directory.Graph graph, String scope, String appname, String serviceUri)
  {
    ExchangeResponse exres = dprovider.postGraph(graph, scope, appname);
    return exres;
  }

  public ExchangeResponse editGraph(org.iringtools.directory.Graph graph, String scope, String appname,
      String oldGraphName, String serviceUri)
  {
    ExchangeResponse exres = dprovider.editGraph(graph, scope, appname, oldGraphName);
    return exres;
  }

  public void deleteGraph(String graph, String scope, String appname, String serviceUri)
  {

    dprovider.deleteGraph(graph, scope, appname);
  }

  public org.iringtools.directory.Graph getGraphInfo(String graph, String scope, String appname, String serviceUri)
  {
    return dprovider.getGraphInfo(graph, scope, appname);
  }

  public ExchangeResponse newCommodity(Commodity com, String scope, String serviceUri)
  {
    ExchangeResponse exRes = dprovider.postCommodity(com, scope);
    return exRes;
  }

  public ExchangeResponse editCommodity(Commodity com, String scope, String oldCommName, String serviceUri)
  {
    ExchangeResponse exRes = dprovider.editCommodity(com, scope, oldCommName);
    return exRes;
  }

  public void deleteCommodity(String com, String scope, String serviceUri)
  {
    dprovider.deleteCommodity(com, scope);
  }

  public Commodity getCommodityInfo(String com, String scope, String serviceUri)
  {
    return dprovider.getCommodityInfo(com, scope);
  }

  public Result testUri(String Uri, String graphName) throws IOException
  {
    Result result = new Result();
    Manifest manifest = null;
    try
    {
      HttpClient httpClient = new HttpClient(Uri);
      httpClient.setAsync(false);
      manifest = httpClient.get(Manifest.class);
      Graphs graph = manifest.getGraphs();
      for(int i = 0; i < graph.getItems().size(); i++)
      {
    	  String name = graph.getItems().get(i).getName();
    	  if(graphName.equalsIgnoreCase(name))
    	  {
    		  result.setSuccess(true);
    	      result.setMessage("Connected successfully!");
    	      return result;
    	  }    	 
      }
      
      result.setSuccess(false);
      result.setMessage("Graph name doesn't exist in manifest");
    }
    catch (HttpClientException e)
    {
      result.setSuccess(false);
      result.setMessage("Exception : " + e.getErrorMessage());
    }
    catch (Exception e)
    {
      result.setSuccess(false);
      result.setMessage("Exception : Invalid URL.");
    }
    return result;
  }

  public void saveDataFilterExpression(Expressions ex, String commName, String scope, String xid, OrderExpressions Oe)
  {
    dprovider.postDataFilterExpressions(scope, xid, commName, ex, Oe);
    Exchange exchange = null;
    String exchangeKey = EXCHANGE_PREFIX + "." + scope + "." + xid;
    String fullDtiKey = DTI_PREFIX + "." + scope + "." + xid;
    for (String key : session.keySet())
      if (key.startsWith(fullDtiKey))
        session.remove(key);

    System.out.println(session);
    exchange = getExchangeFromDirectory(scope, xid);
    session.put(exchangeKey, exchange);
  }

  public DataFilter getDataFilter(String commName, String scope, String xid)
  {
    return dprovider.getDataFilter(commName, scope, xid);
  }

  public List<List<String>> getColumns(String exchangeServiceUri, String scope, String xid, String sort)
      throws Exception
  {
    this.scope = scope;
    this.xId = xid;

    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = getCrossedManifest(exchange, scope, xId);

    Graph graph = manifest.getGraphs().getItems().get(0);
    String relativePath = "/" + scope + "/" + xId;
    List<Field> fields = getFields(relativePath, graph, null);
    List<List<String>> columnnameList = new ArrayList<List<String>>();

    for (Field field : fields)
    {
      List<String> names = new ArrayList<String>();
      if (sort.equalsIgnoreCase("true"))
      {
        if (field.getName() != "Status" && field.getName() != "Content" && field.getName() != "Info"
            && field.getName() != "Transfer Type")
        {
          names.add(field.getName());
          names.add(field.getName());
          columnnameList.add(names);
        }
      }
      else
      {
        if (field.getName() != "Status" && field.getName() != "Content" && field.getName() != "Info")
        {
          names.add(field.getName());
          names.add(field.getName());
          columnnameList.add(names);
        }
      }
    }
    return columnnameList;
  }

  public Result testBaseUri(String Uri) throws IOException
  {
    Result result = new Result();
    try
    {
      HttpClient httpClient = new HttpClient(Uri);
      httpClient.setAsync(false);
      httpClient.get(String.class);
      result.setSuccess(true);
      result.setMessage("Connected successfully!");
    }
    catch (HttpClientException e)
    {
      result.setSuccess(false);
      result.setMessage("Exception : " + e.getErrorMessage());
    }
    catch (Exception e)
    {
      result.setSuccess(false);
      result.setMessage("Exception : Invalid URL.");
    }
    return result;
  }

  public List<List<String>> getInternalScopeNameFromAM(String Uri) throws IOException
  {
    // List<String> scopelist = new ArrayList<String>();
    List<List<String>> scopelist = new ArrayList<List<String>>();
    try
    {
      HttpClient httpClient = new HttpClient(Uri + "/adapter/scopes");
      httpClient.setAsync(false);
      Scopes adapterMangerScopes = httpClient.get(Scopes.class);
      List<org.iringtools.library.Scope> scopes = adapterMangerScopes.getItems();
      for (org.iringtools.library.Scope s : scopes)
      {
        List<String> scopeName = new ArrayList<String>();
        scopeName.add(s.getName());
        scopeName.add(s.getName());
        scopelist.add(scopeName);
        // scopelist.add(s.getName());
      }

    }
    catch (Exception e)
    {
      String error = "Error getting scopes :" + e;
      logger.error(error);
    }
    return scopelist;
  }

  public List<List<String>> getAppNames(String appScope, String Uri)
  {
    List<List<String>> applist = new ArrayList<List<String>>();
    try
    {
      HttpClient httpClient = new HttpClient(Uri + "/adapter/scopes");
      httpClient.setAsync(false);
      Scopes adapterMangerScopes = httpClient.get(Scopes.class);
      List<org.iringtools.library.Scope> scopes = adapterMangerScopes.getItems();
      for (org.iringtools.library.Scope s : scopes)
      {
        if (s.getName().equalsIgnoreCase(appScope))
        {
          Applications apps = s.getApplications();
          List<org.iringtools.library.Application> appNamelist = apps.getItems();
          for (org.iringtools.library.Application app : appNamelist)
          {
            List<String> appName = new ArrayList<String>();
            appName.add(app.getName());
            appName.add(app.getName());
            applist.add(appName);
            // scopelist.add(s.getName());
          }
        }
      }

    }
    catch (Exception e)
    {
      String error = "Error getting scopes :" + e;
      logger.error(error);
    }
    return applist;
  }
}
