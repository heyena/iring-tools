package org.iringtools.models;

import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.UnknownHostException;
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
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.library.exchange.Constants;
import org.iringtools.library.directory.DirectoryProvider;
import org.iringtools.library.exchange.ExchangeProvider;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
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
      manifest = provider.GetCachedCrossedManifest(exchange);
      if (manifest == null)
      {
        manifest = provider.getCrossedManifest(exchange);
        // provider.saveCrossedManifest(manifest, exchange);
      }
      session.put(manifestKey, manifest);
    }

    return manifest;
  }

  public Tree getCrossedManifestTree(String scope, String xId) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = getCrossedManifest(exchange, scope, xId);

    Tree tree = manifestToTree(manifest);
    return tree;
  }

  public void saveCrossedManifest(String scope, String xId) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = getCrossedManifest(exchange, scope, xId);
    provider.saveCrossedManifest(manifest, exchange);
  }

  public void resetCrossedManifest(String scope, String xId) throws Exception
  {
    String manifestKey = MANIFEST_PREFIX + "." + scope + "." + xId;
    session.remove(manifestKey);
    deleteCachedCrossManifest(scope, xId);
  }

  public void reloadCrossedManifest(String scope, String xId) throws Exception
  {
    String manifestKey = MANIFEST_PREFIX + "." + scope + "." + xId;
    session.remove(manifestKey);
  }

  public Manifest getCachedCrossedManifest(String scope, String xId) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = provider.GetCachedCrossedManifest(exchange);
    return manifest;
  }

  public void deleteTemplate(String scope, String xId, String parentClassId, String parentClassIndex,
      String parentClassPath, String templateId, String templateIndex) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    Manifest manifest = getCrossedManifest(exchange, scope, xId);

    Graph graph = manifest.getGraphs().getItems().get(0);
    List<ClassTemplates> classTemplateList = graph.getClassTemplatesList().getItems();

    for (ClassTemplates classTemplate : classTemplateList)
    {
      if (classTemplate.getClazz() != null)
      {
        if ((classTemplate.getClazz().getPath().equals(parentClassPath)))
        {
          List<Template> templates = classTemplate.getTemplates().getItems();

          for (int j = 0; j < templates.size(); j++)
          {
            Template template = templates.get(j);
            if (template.getId().equals(templateId) && String.valueOf(template.getIndex()).equals(templateIndex))
            {
              templates.remove(j--);
            }
          }
        }
      }
    }

  }

  public void deleteCachedCrossManifest(String scope, String xId) throws Exception
  {
    Exchange exchange = getExchange(scope, xId);
    provider.deleteCachedCrossedManifest(exchange);
  }

  protected Tree manifestToTree(Manifest manifest) throws Exception
  {
    Tree tree = new Tree();  
    
    try
    {
      Graph graph = manifest.getGraphs().getItems().get(0);
      List<Template> templateList = graph.getClassTemplatesList().getItems().get(0).getTemplates().getItems();
  
      List<Node> nodes = tree.getNodes();  
      TreeNode rootClassNode = new TreeNode();
  
      nodes.add(rootClassNode);
  
      rootClassNode.setText(graph.getName());
      rootClassNode.setIconCls("commodity");
  
      List<Node> templateNodes = rootClassNode.getChildren();
  
      for (Template template : templateList)
      {
        TreeNode templateNode = new TreeNode();
        templateNode.setText(template.getName());
        templateNode.setIconCls("template");
        templateNode.setType("TemplateNode");
        templateNodes.add(templateNode);
  
        HashMap<String, String> templateProperties = templateNode.getProperties();
        templateProperties.put("Id", template.getId());
        templateProperties.put("Name", template.getName());
        templateProperties.put("TemplateIndex", String.valueOf(template.getIndex()));
  
        List<Node> roleNodes = templateNode.getChildren();
  
        for (Role role : template.getRoles().getItems())
        {
          TreeNode roleNode = new TreeNode();
          roleNode.setText(role.getName());
          roleNode.setIconCls("role");
          roleNode.setType("RoleNode");
          roleNodes.add(roleNode);
  
          HashMap<String, String> roleProperties = roleNode.getProperties();
          roleProperties.put("Id", role.getId());
  
          if (role.getClazz() != null)
          {
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
  
            TemplateToTreeNode(role.getClazz(), classNode, graph);
          }  
        }  
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
    }

    return tree;
  }

  private void TemplateToTreeNode(Class parentClass, TreeNode parentClassNode, Graph graph) throws Exception
  {
    try
    {
      List<ClassTemplates> classTemplateList = graph.getClassTemplatesList().getItems();
      for (ClassTemplates classTemplate : classTemplateList)
      {
        if (classTemplate.getClazz() != null && parentClass != null)
        {
          if (parentClass.getPath().equals(classTemplate.getClazz().getPath()))
          {
            List<Node> templateNodes = parentClassNode.getChildren();
  
            for (Template template : classTemplate.getTemplates().getItems())
            {
              TreeNode templateNode = new TreeNode();
              templateNode.setText(template.getName());
              templateNode.setIconCls("template");
              templateNode.setType("TemplateNode");
              templateNodes.add(templateNode);
  
              HashMap<String, String> templateProperties = templateNode.getProperties();
              templateProperties.put("Id", template.getId());
              templateProperties.put("Name", template.getName());
              templateProperties.put("TemplateIndex", String.valueOf(template.getIndex()));
  
              List<Node> roleNodes = templateNode.getChildren();
  
              for (Role role : template.getRoles().getItems())
              {
                TreeNode roleNode = new TreeNode();
                roleNode.setText(role.getName());
                roleNode.setIconCls("role");
                roleNode.setType("RoleNode");
                roleNodes.add(roleNode);
  
                HashMap<String, String> roleProperties = roleNode.getProperties();
                roleProperties.put("Id", role.getId());
  
                if (role.getClazz() != null)
                {
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
  
                  TemplateToTreeNode(role.getClazz(), classNode, graph);
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

  public String testTargetUri(String targetUri) throws IOException
  {
    URL u = new URL(targetUri);
    HttpURLConnection huc = (HttpURLConnection) u.openConnection();
    huc.setRequestMethod("GET"); // OR huc.setRequestMethod ("HEAD");
    huc.connect();
    int code = huc.getResponseCode();
    System.out.println(code);

    if (code == 200)
    {
      return ("Connected successfully!");
    }
    else
    {
      return ("Connection to URL failed.");
    }
  }

  public String testSourceUri(String sourceUri) throws IOException
  {
    URL u = new URL(sourceUri);
    HttpURLConnection huc = (HttpURLConnection) u.openConnection();
    huc.setRequestMethod("GET"); // OR huc.setRequestMethod ("HEAD");
    huc.connect();
    int code = huc.getResponseCode();
    System.out.println(code);

    if (code == 200)
    {
      return ("Connected successfully!");
    }
    else
    {
      return ("Connection to URL failed.");
    }
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

  public String testUri(String sourceUri) // throws IOException
  {
    try
    {
      URL u = new URL(sourceUri);
      HttpURLConnection huc = (HttpURLConnection) u.openConnection();
      huc.setRequestMethod("GET"); // OR huc.setRequestMethod ("HEAD");
      huc.connect();
      int code = huc.getResponseCode();
      System.out.println(code);
      if (code == 200)
      {
        return ("Connected successfully!");
      }
      else
      {
        return ("Connection to URL failed.");
      }
    }
    catch (UnknownHostException e)
    {
      e.printStackTrace();
      return ("UnknownHostException : Invalid URL.");
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return ("Exception : Invalid URL.");
    }
  }
}