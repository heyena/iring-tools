package org.iringtools.models;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.apache.struts2.json.JSONException;
import org.apache.struts2.json.JSONUtil;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expression;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.LogicalOperator;
import org.iringtools.data.filter.OrderExpression;
import org.iringtools.data.filter.OrderExpressions;
import org.iringtools.data.filter.RelationalOperator;
import org.iringtools.data.filter.SortOrder;
import org.iringtools.data.filter.Values;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.RoleType;
import org.iringtools.dxfr.dto.RoleValues;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.dxfr.dto.TransferType;
import org.iringtools.dxfr.manifest.Cardinality;
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Role;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.request.DxiRequest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.library.RequestStatus;
import org.iringtools.library.State;
import org.iringtools.mapping.ValueListMap;
import org.iringtools.mapping.ValueMap;
import org.iringtools.refdata.response.Entity;
import org.iringtools.utility.DtiComparator;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.widgets.grid.Field;
import org.iringtools.widgets.grid.Grid;
import org.iringtools.widgets.grid.RelatedClass;

public class DataModel {
	private static final Logger logger = Logger.getLogger(DataModel.class);

	public static enum DataMode {
		APP, EXCHANGE
	};

	public static enum FieldFit {
		HEADER, VALUE
	};

	protected static List<String> gridFilterTypes;
	static {
		gridFilterTypes = new ArrayList<String>();
		gridFilterTypes.add("short");
		gridFilterTypes.add("int");
		gridFilterTypes.add("long");
		gridFilterTypes.add("double");
		gridFilterTypes.add("float");
		gridFilterTypes.add("boolean");
		gridFilterTypes.add("date");
		gridFilterTypes.add("string");
	}

	protected static Map<String, RelationalOperator> relationalOperatorMap;
	static {
		relationalOperatorMap = new HashMap<String, RelationalOperator>();
		relationalOperatorMap.put("eq", RelationalOperator.EQUAL_TO);
		relationalOperatorMap.put("lt", RelationalOperator.LESSER_THAN);
		relationalOperatorMap.put("gt", RelationalOperator.GREATER_THAN);
	}

	public static final String APP_PREFIX = "xchmgr-";
	public static final String MANIFEST_PREFIX = APP_PREFIX + "manifest-";
	public static final String DATAFILTER_PREFIX = APP_PREFIX + "datafilter-";
	public static final String FIELDS_PREFIX = APP_PREFIX + "fields-";
	public static final String DTI_PREFIX = APP_PREFIX + "dti-";
	public static final String XLOGS_PREFIX = APP_PREFIX + "xlogs-";
	public static final String FULL_DTI_KEY_PREFIX = DTI_PREFIX + "full";
	public static final String PART_DTI_KEY_PREFIX = DTI_PREFIX + "part";
	public static final String FILTER_KEY_PREFIX = DTI_PREFIX + "filter";

	protected final int MIN_FIELD_WIDTH = 50;
	protected final int MAX_FIELD_WIDTH = 300;
	protected final int INFO_FIELD_WIDTH = 28;
	protected final int STATUS_FIELD_WIDTH = 60;
	protected final int FIELD_PADDING = 2;
	protected final int HEADER_PX_PER_CHAR = 6;
	protected final int VALUE_PX_PER_CHAR = 10;

	protected Map<String, Object> session;

	protected DataMode dataMode;
	protected String refDataServiceUri;
	protected FieldFit fieldFit;
	protected DataFilter dataFilterUI = new DataFilter();
	DataFilter dataFilterFile = null;

	protected boolean isAsync = false;
	protected long timeout = 1800; // seconds
	protected long interval = 2; // seconds

	public DataModel(DataMode dataMode, String refDataServiceUri,
			FieldFit fieldFit) {
		this.dataMode = dataMode;
		this.refDataServiceUri = refDataServiceUri;
		this.fieldFit = fieldFit;
	}

	public DataModel(DataMode dataMode, String refDataServiceUri,
			FieldFit fieldFit, boolean isAsync, long timeout, long interval) {
		this(dataMode, refDataServiceUri, fieldFit);
		this.isAsync = isAsync;
		this.timeout = timeout * 1000; // convert to milliseconds
		this.interval = interval * 1000; // convert to milliseconds
	}

	// only cache full dti and last filtered dti
	protected DataTransferIndices getDtis(String serviceUri,
			String manifestRelativePath, String dtiRelativePath, String filter,
			String sortBy, String sortOrder, String dataFilterRelativePath)
			throws DataModelException {
		DataTransferIndices dtis = new DataTransferIndices();
		String fullDtiKey = FULL_DTI_KEY_PREFIX + dtiRelativePath;
		String partDtiKey = PART_DTI_KEY_PREFIX + dtiRelativePath;
		String lastFilterKey = FILTER_KEY_PREFIX + dtiRelativePath;
		String dataFilterKey = DATAFILTER_PREFIX + dataFilterRelativePath;
		String currFilter = filter + "/" + sortBy + "/" + sortOrder;

		try {
			DataFilter dataFilter = null;

			if (dataFilterRelativePath != null) {
				dataFilterUI = createDataFilter(filter, sortBy, sortOrder);
				HttpClient httpClient = new HttpClient(serviceUri);
				HttpUtils.addHttpHeaders(session, httpClient);

				// Getting Filter from file Filter.xxx.xx.xml
				try {
					dataFilterFile = httpClient.get(DataFilter.class,
							dataFilterRelativePath);
					session.put(dataFilterKey, dataFilterFile);
				} catch (HttpClientException e) {
					logger.error(e.getMessage());
					throw new DataModelException(e.getMessage());
				}

				/*
				 * // merging File Filter and UI Filter DataFilterInitial dFI =
				 * new DataFilterInitial(); dataFilter =
				 * dFI.AppendFilter(dataFilterUI, dataFilterFile);
				 */
				dataFilter = dataFilterUI;
			} else {
				dataFilter = createDataFilter(filter, sortBy, sortOrder);
			}

			dtis = getFullDtis(serviceUri, manifestRelativePath,
					dtiRelativePath, fullDtiKey, partDtiKey, lastFilterKey);

			if (dataFilter != null
					&& (dataFilter.getExpressions() != null || dataFilter
							.getOrderExpressions() != null)) {
				if (session.containsKey(lastFilterKey)) // check if filter has
				// changed
				{
					String lastFilter = (String) session.get(lastFilterKey);

					// filter has changed or cache data not available, fetch
					// filtered data
					if (!lastFilter.equals(currFilter)
							|| !session.containsKey(partDtiKey)) {
						dtis = getFilteredDtis(dataFilter,
								manifestRelativePath, dtiRelativePath,
								serviceUri, fullDtiKey, partDtiKey,
								lastFilterKey, currFilter);
					} else
					// filter has not changed, get data from cache
					{
						dtis = (DataTransferIndices) session.get(partDtiKey);
					}
				} else
				// new filter or same filter after exchange, fetch
				// filtered data
				{
					dtis = getFilteredDtis(dataFilter, manifestRelativePath,
							dtiRelativePath, serviceUri, fullDtiKey,
							partDtiKey, lastFilterKey, currFilter);
				}
				/*
				 * { // validating the UI filter for managing the process time
				 * 
				 * boolean uiTransferType = false; boolean compareEqual = false;
				 * int count = 0; int k = 0; List<Expression> fileFilter =
				 * dataFilterFile.getExpressions().getItems();
				 * 
				 * Expressions exs = dataFilter.getExpressions(); if (exs !=
				 * null) { for (int j = 0; j < exs.getItems().size(); j++) {
				 * Expression ex = exs.getItems().get(j);
				 * if(ex.getPropertyName().equalsIgnoreCase("Transfer Type")) {
				 * uiTransferType = true; Expressions fileexs =
				 * dataFilterFile.getExpressions(); for (int i = 0; i <
				 * fileexs.getItems().size(); i++) { Expression fileex =
				 * fileexs.getItems().get(i);
				 * if(fileex.getPropertyName().equalsIgnoreCase
				 * ("Transfer Type")) { compareEqual = true;
				 * //fileFilter.getItems().set(k++, fileex);
				 * if(fileex.getRelationalOperator
				 * ().equals(RelationalOperator.EQUAL_TO)) {
				 * if(!ex.getValues().getItems
				 * ().get(0).equalsIgnoreCase(fileex.getValues
				 * ().getItems().get(0))) { count++; } } else { dtis =
				 * getFilteredDtis(dataFilter, manifestRelativePath,
				 * dtiRelativePath, serviceUri, fullDtiKey, partDtiKey,
				 * lastFilterKey, currFilter); break; } } else
				 * fileFilter.remove(i); } } } } if(fileFilter.size() == count )
				 * { DataTransferIndexList resultDtiList = new
				 * DataTransferIndexList(); resultDtiList.setItems(null);
				 * dtis.setDataTransferIndexList(resultDtiList); }
				 * if(dataFilterFile.getExpressions().getItems().size() > count
				 * ) { dtis = getFilteredDtis(dataFilter, manifestRelativePath,
				 * dtiRelativePath, serviceUri, fullDtiKey, partDtiKey,
				 * lastFilterKey, currFilter); }
				 * 
				 * if((! uiTransferType) || (! compareEqual)) dtis =
				 * getFilteredDtis(dataFilter, manifestRelativePath,
				 * dtiRelativePath, serviceUri, fullDtiKey, partDtiKey,
				 * lastFilterKey, currFilter); }
				 */

			}
		}

		catch (Exception e) {
			logger.error(e.getMessage());
			throw new DataModelException(e.getMessage());
		}

		return dtis;
	}

	private void collapseDuplicates(DataTransferIndices dtis) {
		try {
			DtiComparator DtiComparator = new DtiComparator();
			Collections.sort(dtis.getDataTransferIndexList().getItems(),
					DtiComparator);

			List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList()
					.getItems();
			DataTransferIndex prevDti = null;

			for (int i = 0; i < dtiList.size(); i++) {
				DataTransferIndex dti = dtiList.get(i);
				dti.setDuplicateCount(1);

				if (prevDti != null
						&& dti.getIdentifier().equalsIgnoreCase(
								prevDti.getIdentifier())) {

					// indices with same identifiers but different hash values
					// are considered different
					int j = i;

					do {
						if (dti.getInternalIdentifier()
								.toLowerCase()
								.compareTo(
										prevDti.getInternalIdentifier()
												.toLowerCase()) == 0) {
							prevDti.setDuplicateCount(prevDti
									.getDuplicateCount() + 1);
							dtiList.remove(j);
						} else {
							j++;
						}

						if (j < dtiList.size())
							dti = dtiList.get(j);
						else
							break;

					} while (dti.getIdentifier().equalsIgnoreCase(
							prevDti.getIdentifier()));
				} else {
					prevDti = dti;
				}
			}
		} catch (Exception e) {
			e.printStackTrace();
			logger.error(e.getMessage());
		}
	}

	private DataTransferIndices getFullDtis(String serviceUri,
			String manifestRelativePath, String dtiRelativePath,
			String fullDtiKey, String partDtiKey, String lastFilterKey)
			throws DataModelException {
		DataTransferIndices dtis = new DataTransferIndices();
		DataTransferIndices fullDtis = null;

		if (session.containsKey(fullDtiKey)) {
			dtis = (DataTransferIndices) session.get(fullDtiKey);
		} else {
			DxiRequest dxiRequest = new DxiRequest();
			dxiRequest
					.setManifest(getManifest(serviceUri, manifestRelativePath));
			// For application data pass empty dataFilter.
			if (dataMode == DataMode.APP)
				dxiRequest.setDataFilter(new DataFilter());

			try {
				HttpClient httpClient = new HttpClient(serviceUri);
				HttpUtils.addHttpHeaders(session, httpClient);

				if (dataMode == DataMode.EXCHANGE) {
					// remove transfer type Expression from dataFilter
					List<Expression> transferTypeExpression = removeTransfertypeExpression(dataFilterFile);

					// pass filter to dxiRequest to get filtered dti data.
					dxiRequest.setDataFilter(dataFilterFile);
					if (isAsync) {
						httpClient.setAsync(true);
						String statusUrl = httpClient.post(String.class,
								dtiRelativePath, dxiRequest);
						fullDtis = waitForRequestCompletion(
								DataTransferIndices.class, serviceUri
										+ statusUrl);
					} else {
						fullDtis = httpClient.post(DataTransferIndices.class,
								dtiRelativePath, dxiRequest);
					}
					// apply filterfile on fullDtis list
					List<DataTransferIndex> FileFilterFullDti = fullDtis
							.getDataTransferIndexList().getItems();
					FileFilterFullDti = getTransferTypeDtis(FileFilterFullDti,
							transferTypeExpression);
					// assinging result to dtis.
					DataTransferIndexList resultDtiList = new DataTransferIndexList();
					resultDtiList.setItems(FileFilterFullDti);
					dtis.setDataTransferIndexList(resultDtiList);
				} else {
					if (isAsync) {
						httpClient.setAsync(true);
						String statusUrl = httpClient.post(String.class,
								dtiRelativePath, dxiRequest);
						fullDtis = waitForRequestCompletion(
								DataTransferIndices.class, serviceUri
										+ statusUrl);
					} else {
						fullDtis = httpClient.post(DataTransferIndices.class,
								dtiRelativePath, dxiRequest);
					}
					dtis = fullDtis;
				}

				if (dtis != null
						&& dtis.getDataTransferIndexList() != null
						&& dtis.getDataTransferIndexList().getItems().size() > 0) {
					// collapse duplications for app data (exchange data already
					// done so in differencing engine)
					if (dataMode == DataMode.APP) {
						collapseDuplicates(dtis);
					}

					session.put(fullDtiKey, dtis);
				}
			} catch (HttpClientException e) {
				logger.error(e.getMessage());
				throw new DataModelException(e.getMessage());
			}
		}

		// log duplicates
		if (dtis != null && dtis.getDataTransferIndexList() != null
				&& dtis.getDataTransferIndexList().getItems().size() > 0) {
			for (DataTransferIndex dti : dtis.getDataTransferIndexList()
					.getItems()) {
				if (dti.getDuplicateCount() != null
						&& dti.getDuplicateCount() > 1)
					logger.warn("DTI [" + dti.getIdentifier() + "] has ["
							+ dti.getDuplicateCount() + "] duplicates.");
			}
		}

		if (session.containsKey(partDtiKey)) {
			session.remove(partDtiKey);
		}

		if (session.containsKey(lastFilterKey)) {
			session.remove(lastFilterKey);
		}

		return dtis;
	}

	private DataTransferIndices getFilteredDtis(DataFilter dataFilter,
			String manifestRelativePath, String dtiRelativePath,
			String serviceUri, String fullDtiKey, String partDtiKey,
			String lastFilterKey, String currFilter) throws DataModelException {
		DataTransferIndices dtis = null;

		if (dataMode == DataMode.EXCHANGE) // exchange data
		{
			dtis = getFilteredDtis(dataFilter, manifestRelativePath,
					dtiRelativePath, serviceUri, fullDtiKey);
		} else
		// app data
		{
			DxiRequest dxiRequest = new DxiRequest();
			dxiRequest
					.setManifest(getManifest(serviceUri, manifestRelativePath));
			dxiRequest.setDataFilter(dataFilter);

			HttpClient httpClient = new HttpClient(serviceUri);
			HttpUtils.addHttpHeaders(session, httpClient);

			try {
				if (isAsync) {
					httpClient.setAsync(true);
					String statusUrl = httpClient.post(String.class,
							dtiRelativePath, dxiRequest);
					dtis = waitForRequestCompletion(DataTransferIndices.class,
							serviceUri + statusUrl);
				} else {
					dtis = httpClient.post(DataTransferIndices.class,
							dtiRelativePath, dxiRequest);
				}

				collapseDuplicates(dtis);
			} catch (HttpClientException e) {
				logger.error(e.getMessage());
				throw new DataModelException(e.getMessage());
			}
		}

		if (dtis != null && dtis.getDataTransferIndexList() != null
				&& dtis.getDataTransferIndexList().getItems().size() > 0) {
			session.put(partDtiKey, dtis);
			session.put(lastFilterKey, currFilter);
		}

		return dtis;
	}

	protected List<Expression> removeTransfertypeExpression(
			DataFilter dataFilter) {
		List<Expression> transferTypeExpression = new ArrayList<Expression>();
		String filterFile = "yes";
		if (dataFilter.getExpressions() != null
				&& dataFilterUI.getExpressions() != null) {
			if (dataFilter.getExpressions().getItems().size() == dataFilterUI
					.getExpressions().getItems().size()) {
				filterFile = "no";
			}
		}
		// extract transfer type from expressions
		if (dataFilter.getExpressions() != null
				&& dataFilter.getExpressions().getItems().size() > 0) {
			List<Expression> expressions = dataFilter.getExpressions()
					.getItems();

			int i = 0;
			String valueUi = null;
			int count = 0;
			for (; i < expressions.size(); i++) {
				Expression expression = expressions.get(i);

				if (expression.getPropertyName().equalsIgnoreCase(
						"transfer type")) {

					String value = expressions.get(i).getValues().getItems()
							.get(0);
					RelationalOperator relationalOper = expressions.get(i)
							.getRelationalOperator();

					// To removing filter with type transfer type.
					if (dataFilterUI.getExpressions() != null) {
						List<Expression> expressionsUi = dataFilterUI
								.getExpressions().getItems();
						int k = 0;
						for (; k < expressionsUi.size(); k++) {
							if (expressionsUi.get(k).getPropertyName()
									.equalsIgnoreCase("transfer type")) {
								// compare UI filter with file filter if they
								// are same remove one.
								valueUi = expressionsUi.get(k).getValues()
										.getItems().get(0);
								if (filterFile != "no") {
									if (value.equalsIgnoreCase(valueUi)) {
										if (relationalOper
												.equals(expressionsUi
														.get(k)
														.getRelationalOperator())) {
											if (count < 1) {
												expressions.remove(i);
												count++;
											} else
												count = 0;
										}
									} else
										valueUi = null;
								}
							}
						}
					}
					if (valueUi == null || count < 1) {
						transferTypeExpression.add(expressions.remove(i));
					}

					if (expressions.size() > 0) {
						// remove logical operator of the next expression
						expression.setLogicalOperator(null);
					}
					i--;
				}
			}
		} else
			transferTypeExpression = null;
		return transferTypeExpression;
	}

	protected OrderExpression removeTransfertypeOrderExpression(
			DataFilter dataFilter) {
		OrderExpression transferTypeOrderExpression = null;

		// extract transfer type from order expressions
		if (dataFilter.getOrderExpressions() != null
				&& dataFilter.getOrderExpressions().getItems().size() > 0) {
			List<OrderExpression> orderExpressions = dataFilter
					.getOrderExpressions().getItems();

			for (int i = 0; i < orderExpressions.size(); i++) {
				OrderExpression orderExpression = orderExpressions.get(i);
				if (orderExpression.getPropertyName().equalsIgnoreCase(
						"transfer type")) {
					transferTypeOrderExpression = orderExpressions.remove(i);

					if (orderExpressions.size() > 0) {
						// remove logical operator of the next expression
						orderExpressions.get(i).setPropertyName(null);
						orderExpressions.get(i).setSortOrder(null);
					}
				}

				if (orderExpressions.size() > 1) {
					dataFilter.getOrderExpressions().getItems()
							.remove(orderExpression);
					i--;
				}
			}
		}
		return transferTypeOrderExpression;
	}

	protected List<DataTransferIndex> getTransferTypeDtis(
			List<DataTransferIndex> tmpFullDtiList,
			List<Expression> transferTypeExpression) {
		List<DataTransferIndex> TranfertmpDtiList = new ArrayList<DataTransferIndex>();
		List<DataTransferIndex> TranferNotEqualDtiList = new ArrayList<DataTransferIndex>();
		TranferNotEqualDtiList.addAll(tmpFullDtiList);
		String has_notequal = null;
		if (transferTypeExpression != null) {
			for (int j = 0; j < transferTypeExpression.size(); j++) {
				if (transferTypeExpression != null && tmpFullDtiList != null) {

					String value = transferTypeExpression.get(j).getValues()
							.getItems().get(0);
					int t = 0;
					for (; t < tmpFullDtiList.size(); t++) {
						if (transferTypeExpression.get(j)
								.getRelationalOperator() == RelationalOperator.EQUAL_TO) {
							if (tmpFullDtiList.get(t).getTransferType()
									.toString().equalsIgnoreCase(value)) {
								TranfertmpDtiList.add(tmpFullDtiList.get(t));
								int index = t--;
								tmpFullDtiList.remove(index); // values
								// already
								// added is
								// removed
							}
						}
					}

					int i = 0;
					for (; i < TranferNotEqualDtiList.size(); i++) {
						if (tmpFullDtiList.size() != TranferNotEqualDtiList
								.size()) {
							TranferNotEqualDtiList
									.removeAll(TranferNotEqualDtiList);
							TranferNotEqualDtiList.addAll(tmpFullDtiList);
						}
						if (transferTypeExpression.get(j)
								.getRelationalOperator() == RelationalOperator.NOT_EQUAL_TO) {
							if (TranferNotEqualDtiList.get(i).getTransferType()
									.toString().equalsIgnoreCase(value)) {
								has_notequal = "yes";
								int removeIndex = i--;
								TranferNotEqualDtiList.remove(removeIndex);
								tmpFullDtiList.remove(removeIndex);
							}
						}
					}
				}
			}
			for (int l = 0; l < TranfertmpDtiList.size(); l++) {

				for (int k = 0; k < TranferNotEqualDtiList.size(); k++) {
					if (TranfertmpDtiList.get(l).equals(
							TranferNotEqualDtiList.get(k))) {
						TranferNotEqualDtiList.remove(k--);
					}
				}
			}
			for (int l = 0; l < TranferNotEqualDtiList.size(); l++) {

				for (int k = 0; k < TranfertmpDtiList.size(); k++) {
					if (TranferNotEqualDtiList.get(l).equals(
							TranfertmpDtiList.get(k))) {
						TranfertmpDtiList.remove(k--);
					}
				}
			}
			if (TranfertmpDtiList != null) {

				tmpFullDtiList.removeAll(tmpFullDtiList);
				tmpFullDtiList.addAll(TranfertmpDtiList);
				if (has_notequal != null) {
					tmpFullDtiList.addAll(TranferNotEqualDtiList);
				}

			}

			if (dataFilterUI.getExpressions() != null) {
				List<Expression> expressions = dataFilterUI.getExpressions()
						.getItems();
				int i = 0;
				for (; i < expressions.size(); i++) {
					if (expressions.get(i).getPropertyName()
							.equalsIgnoreCase("transfer type")) {
						String value = expressions.get(i).getValues()
								.getItems().get(0);
						for (int j = 0; j < tmpFullDtiList.size(); j++) {
							if (expressions.get(i).getRelationalOperator() == RelationalOperator.EQUAL_TO) {
								if (!tmpFullDtiList.get(j).getTransferType()
										.toString().equalsIgnoreCase(value)) {
									tmpFullDtiList.remove(j--);
								}
							} else {
								if (tmpFullDtiList.get(j).getTransferType()
										.toString().equalsIgnoreCase(value)) {
									tmpFullDtiList.remove(j--);
								}
							}
						}
					}
				}
			}
		}
		return tmpFullDtiList;
	}

	protected DataTransferIndices getFilteredDtis(DataFilter dataFilter,
			String manifestRelativePath, String dtiRelativePath,
			String serviceUri, String fullDtiKey) throws DataModelException {

		DataTransferIndices resultDtis = new DataTransferIndices();

		// remove transfer type Expression from dataFilter
		List<Expression> transferTypeExpression = removeTransfertypeExpression(dataFilter);
		// remove transfer type OrderExpressions from dataFilter
		OrderExpression transferTypeOrderExpression = removeTransfertypeOrderExpression(dataFilter);

		try {
			DataTransferIndices fullDtis = (DataTransferIndices) session
					.get(fullDtiKey);
			List<DataTransferIndex> fullDtiList = fullDtis
					.getDataTransferIndexList().getItems();

			List<DataTransferIndex> tmpFullDtiList = new ArrayList<DataTransferIndex>();
			for (DataTransferIndex dti : fullDtiList) {
				tmpFullDtiList.add(dti);
				// code for removing dup's from the fullDTi's
				/*
				 * if (dti.getDuplicateCount() == null ||
				 * dti.getDuplicateCount() == 1) { tmpFullDtiList.add(dti); }
				 * else { logger.warn("DTI [" + dti.getIdentifier() + "] has ["
				 * + dti.getDuplicateCount() + "] duplicates."); }
				 */
			}
			if (transferTypeExpression != null) {
				if (transferTypeExpression.size() > 0) {
					// apply transfer type filter on result DtiList
					tmpFullDtiList = getTransferTypeDtis(tmpFullDtiList,
							transferTypeExpression);
				}
			}

			if ((dataFilter.getExpressions() != null && dataFilter
					.getExpressions().getItems().size() > 0)
					|| dataFilter.getOrderExpressions() != null
					&& dataFilter.getOrderExpressions().getItems().size() > 0) {
				// apply property filter
				HttpClient httpClient = new HttpClient(serviceUri);
				HttpUtils.addHttpHeaders(session, httpClient);

				String requestUrl = dtiRelativePath + "?dtiOnly=true";

				DxiRequest dxiRequest = new DxiRequest();
				dxiRequest.setManifest(getManifest(serviceUri,
						manifestRelativePath));
				dxiRequest.setDataFilter(dataFilter);

				resultDtis = httpClient.post(DataTransferIndices.class,
						requestUrl, dxiRequest);

				List<DataTransferIndex> partialDtiList = resultDtis
						.getDataTransferIndexList().getItems();
				parsePartialDtis(partialDtiList, tmpFullDtiList);
			} else {
				// no property filter, return full DTI list
				DataTransferIndexList resultDtiList = new DataTransferIndexList();
				resultDtis.setDataTransferIndexList(resultDtiList);
				resultDtiList.setItems(tmpFullDtiList);
			}

			// apply sorting
			if (resultDtis.getDataTransferIndexList() != null
					&& resultDtis.getDataTransferIndexList().getItems().size() > 0) {
				if (transferTypeOrderExpression != null) {
					final String sortDir = transferTypeOrderExpression
							.getSortOrder().toString().toLowerCase();

					Comparator<DataTransferIndex> comparator = new Comparator<DataTransferIndex>() {
						public int compare(DataTransferIndex dti1,
								DataTransferIndex dti2) {
							int compareValue = 0;

							if (sortDir.equals("asc")) {
								compareValue = dti1
										.getTransferType()
										.toString()
										.compareTo(
												dti2.getTransferType()
														.toString());
							} else {
								compareValue = dti2
										.getTransferType()
										.toString()
										.compareTo(
												dti1.getTransferType()
														.toString());
							}

							return compareValue;
						}
					};

					Collections.sort(resultDtis.getDataTransferIndexList()
							.getItems(), comparator);
				} else if (dataFilter.getOrderExpressions() != null
						&& dataFilter.getOrderExpressions().getItems().size() > 0) {
					/*
					 * if (!(resultDtis.getSortType() == null)) { final String
					 * sortType = resultDtis.getSortType() .toLowerCase(); }
					 * else { final String sortType = null; }
					 */
					final String sortDir = resultDtis.getSortOrder()
							.toLowerCase();

					Comparator<DataTransferIndex> comparator = new Comparator<DataTransferIndex>() {
						public int compare(DataTransferIndex dti1,
								DataTransferIndex dti2) {
							int compareValue = 0;
							String dti1SortIndex = dti1.getSortIndex();
							String dti2SortIndex = dti2.getSortIndex();

							/*
							 * if (sortType.equals("string") ||
							 * sortType.contains("date") ||
							 * sortType.contains("time")) {
							 */
							if (dti1SortIndex == null) {
								dti1SortIndex = "";
							}

							if (dti2SortIndex == null) {
								dti2SortIndex = "";
							}

							if (sortDir.equals("asc")) {
								compareValue = dti1SortIndex
										.compareTo(dti2SortIndex);
							} else {
								compareValue = dti2SortIndex
										.compareTo(dti1SortIndex);
							}
							/*
							 * } else // sort type is numeric { if
							 * (dti1SortIndex == null) { dti1SortIndex = String
							 * .valueOf(Double.MAX_VALUE); }
							 * 
							 * if (dti2SortIndex == null) { dti2SortIndex =
							 * String .valueOf(Double.MAX_VALUE); }
							 * 
							 * if (sortDir.equals("asc")) { compareValue = (int)
							 * (Double .parseDouble(dti1SortIndex) - Double
							 * .parseDouble(dti2SortIndex)); } else {
							 * compareValue = (int) (Double
							 * .parseDouble(dti2SortIndex) - Double
							 * .parseDouble(dti1SortIndex)); } }
							 */

							return compareValue;
						}
					};

					Collections.sort(resultDtis.getDataTransferIndexList()
							.getItems(), comparator);
				}
			}
		} catch (Exception e) {
			logger.error(e.getMessage());
			throw new DataModelException(e.getMessage());
		}

		return resultDtis;
	}

	protected DataTransferIndices getCachedDtis(String relativePath) {
		String dtiKey = PART_DTI_KEY_PREFIX + relativePath;

		if (!session.containsKey(dtiKey)) {
			dtiKey = FULL_DTI_KEY_PREFIX + relativePath;
		}

		return (DataTransferIndices) session.get(dtiKey);
	}

	protected DataTransferObjects getDtos(String serviceUri,
			String manifestRelativePath, String dtoRelativePath,
			List<DataTransferIndex> dtiList) throws DataModelException {
		DataTransferObjects dtos = new DataTransferObjects();

		try {
			DataTransferIndices dtis = new DataTransferIndices();
			DataTransferIndexList dtiListObj = new DataTransferIndexList();
			dtiListObj.setItems(dtiList);
			dtis.setDataTransferIndexList(dtiListObj);

			DxoRequest dxoRequest = new DxoRequest();
			dxoRequest
					.setManifest(getManifest(serviceUri, manifestRelativePath));
			dxoRequest.setDataTransferIndices(dtis);

			HttpClient httpClient = new HttpClient(serviceUri);
			HttpUtils.addHttpHeaders(session, httpClient);

			if (isAsync) {
				httpClient.setAsync(true);
				String statusUrl = httpClient.post(String.class,
						dtoRelativePath, dxoRequest);
				dtos = waitForRequestCompletion(DataTransferObjects.class,
						serviceUri + statusUrl);
			} else {
				dtos = httpClient.post(DataTransferObjects.class,
						dtoRelativePath, dxoRequest);
			}
		} catch (HttpClientException e) {
			logger.error(e.getMessage());
			throw new DataModelException(e.getMessage());
		}

		return dtos;
	}

	protected DataTransferObjects getPageDtos(String serviceUri,
			String manifestRelativePath, String dtiRelativePath,
			String dtoRelativePath, String filter, String sortBy,
			String sortOrder, int start, int limit,
			String dataFilterRelativePath) throws DataModelException {
		DataTransferIndices dtis = getDtis(serviceUri, manifestRelativePath,
				dtiRelativePath, filter, sortBy, sortOrder,
				dataFilterRelativePath);
		List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList()
				.getItems();
		int actualLimit = Math.min(start + limit, dtiList.size());
		List<DataTransferIndex> pageDtis = dtiList.subList(start, actualLimit);
		return getDtos(serviceUri, manifestRelativePath, dtoRelativePath,
				pageDtis);
	}

	protected Grid getDtoGrid(String fieldsContext, Manifest manifest,
			Graph graph, DataTransferObjects dtos) throws DataModelException {
		Grid dtoGrid = new Grid();

		if (graph != null) {
			if (graph.getClassTemplatesList() != null
					&& graph.getClassTemplatesList().getItems().size() > 0
					&& graph.getClassTemplatesList().getItems().get(0)
							.getClazz() != null) {
				String className = IOUtils.toCamelCase(graph
						.getClassTemplatesList().getItems().get(0).getClazz()
						.getName());
				dtoGrid.setDescription(className);
			}

			List<Field> fields = getFields(fieldsContext, graph, null);
			dtoGrid.setFields(fields);

			List<List<String>> gridData = new ArrayList<List<String>>();
			dtoGrid.setData(gridData);

			List<DataTransferObject> dtoList = dtos.getDataTransferObjectList()
					.getItems();

			for (int dtoIndex = 0; dtoIndex < dtoList.size(); dtoIndex++) {
				DataTransferObject dto = dtoList.get(dtoIndex);
				List<String> rowData = new ArrayList<String>();
				List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();

				// create a place holder for info field
				rowData.add("");
				String relatedClassesJson;

				try {
					relatedClassesJson = JSONUtil.serialize(relatedClasses);
				} catch (JSONException e) {
					relatedClassesJson = "[]";
				}

				if (dataMode == DataMode.EXCHANGE) {
					String transferType = dto.getTransferType().toString();
					/*
					 * rowData.add("<span class=\"" + transferType.toLowerCase()
					 * + "\">" + transferType + "</span>");
					 */
					rowData.add("<input type=\"image\" src=\"resources/images/"
							+ transferType.toLowerCase()
							+ ".png\" width=15 heigt=15 "+ "onClick='javascript:showChangedItemsInfo()'>");
				}

				// Adding dup's count to dup's column
				if (dataMode == DataMode.EXCHANGE) {

					String dups;
					if (dto.getDuplicateCount() == null) {
						dups = "0";
					//	rowData.add(dups);
					} else{
					//	rowData.add((dto.getDuplicateCount().toString()));
						if(dto.getDuplicateCount() == 1)
						{
						rowData.add("<input type=\"image\" src=\"resources/images/success.png\" width=15 heigt=15 >");
					}else
					{
						rowData.add("<input type=\"image\" src=\"resources/images/error.png\" width=15 heigt=15  "+ "onClick='javascript:showStatus(\"" + dto.getDuplicateCount()+ "\")'>");
					}
					}
					
				}

				if (dto.getClassObjects().getItems().size() > 0) {
					ClassObject classObject = dto.getClassObjects().getItems()
							.get(0);
					dtoGrid.setIdentifier(classObject.getClassId());

					processClassObject(manifest, graph, dto, dtoIndex, fields,
							classObject, dtoGrid, rowData, relatedClasses);
				}
				
				// setting color icon if the row contain Dup's

				/*
				 * if(dto.getDuplicateCount() > 1) { String transferType =
				 * dto.getTransferType().toString();
				 * rowData.add("<span class=\"icon\">" + transferType +
				 * "</span>");
				 * 
				 * }else {
				 */
				// update info field

				rowData.set(
						0,
						"<input type=\"image\" src=\"resources/images/info-small.png\" "
								+ "onClick='javascript:showIndividualInfo(\""
								+ dto.getIdentifier() + "\",\""
								+ dto.getIdentifier() + "\","
								+ relatedClassesJson + ")'>");
				// }

				gridData.add(rowData);

			}
		}

		return dtoGrid;
	}

	// TODO: apply start and limit
	protected DataTransferObjects getRelatedItems(String serviceUri,
			String manifestRelativePath, String dtiRelativePath,
			String dtoRelativePath, String dtoIdentifier, String filter,
			String sortBy, String sortOrder, int start, int limit)
			throws DataModelException {
		DataTransferObjects relatedDtos = new DataTransferObjects();
		DataTransferIndices dtis = getCachedDtis(dtiRelativePath);
		List<DataTransferIndex> dtiList = dtis.getDataTransferIndexList()
				.getItems();

		for (DataTransferIndex dti : dtiList) {
			if (dti.getIdentifier().equals(dtoIdentifier)) {
				DataTransferIndices requestDtis = new DataTransferIndices();
				DataTransferIndexList dtiRequestList = new DataTransferIndexList();
				requestDtis.setDataTransferIndexList(dtiRequestList);
				dtiRequestList.getItems().add(dti);

				DxoRequest dxoRequest = new DxoRequest();
				dxoRequest.setManifest(getManifest(serviceUri,
						manifestRelativePath));
				dxoRequest.setDataTransferIndices(requestDtis);

				try {
					HttpClient httpClient = new HttpClient(serviceUri);
					HttpUtils.addHttpHeaders(session, httpClient);

					if (isAsync) {
						httpClient.setAsync(true);
						String statusUrl = httpClient.post(String.class,
								dtoRelativePath, dxoRequest);
						relatedDtos = waitForRequestCompletion(
								DataTransferObjects.class, serviceUri
										+ statusUrl);
					} else {
						relatedDtos = httpClient.post(
								DataTransferObjects.class, dtoRelativePath,
								dxoRequest);
					}

					// apply filter
					if (relatedDtos != null) {
						List<DataTransferObject> dtoList = relatedDtos
								.getDataTransferObjectList().getItems();

						if (dtoList.size() > 0 && filter != null
								&& filter.length() > 0) {
							DataFilter dataFilter = createDataFilter(filter,
									sortBy, sortOrder);
							List<Expression> expressions = dataFilter
									.getExpressions().getItems();

							for (Expression expression : expressions) {
								for (DataTransferObject dto : dtoList) {
									List<ClassObject> classObjects = dto
											.getClassObjects().getItems();
									dto.getClassObjects().setItems(
											getFilteredClasses(expression,
													classObjects));
								}
							}
						}
					}
				} catch (HttpClientException e) {
					logger.error(e.getMessage());
					throw new DataModelException(e.getMessage());
				}
			}
		}

		return relatedDtos;
	}

	protected Grid getRelatedItemGrid(String fieldsContext, Manifest manifest,
			Graph graph, DataTransferObjects dtos, String classId,
			String classIdentifier) throws DataModelException {
		Grid dtoGrid = new Grid();

		List<Field> fields = getFields(fieldsContext, graph, classId);
		dtoGrid.setFields(fields);

		List<List<String>> gridData = new ArrayList<List<String>>();
		dtoGrid.setData(gridData);

		List<DataTransferObject> dtoList = dtos.getDataTransferObjectList()
				.getItems();

		for (int dtoIndex = 0; dtoIndex < dtoList.size(); dtoIndex++) {
			DataTransferObject dto = dtoList.get(dtoIndex);
			List<RelatedClass> relatedClasses = new ArrayList<RelatedClass>();

			if (dto.getClassObjects().getItems().size() > 0) {
				for (ClassObject classObject : dto.getClassObjects().getItems()) {
					if (classObject.getClassId().equalsIgnoreCase(classId)) // &&
					// classObject.getIdentifier().equalsIgnoreCase(classIdentifier))
					{
						dtoGrid.setIdentifier(classObject.getClassId());
						dtoGrid.setDescription(classObject.getName());

						List<String> rowData = new ArrayList<String>();

						// create a place holder for info field
						rowData.add("");

						if (dataMode == DataMode.EXCHANGE) {
							String transferType = dto.getTransferType()
									.toString();
							rowData.add("<span class=\""
									+ transferType.toLowerCase() + "\">"
									+ transferType + "</span>");
						}

						processClassObject(manifest, graph, dto, dtoIndex,
								fields, classObject, dtoGrid, rowData,
								relatedClasses);

						String relatedClassesJson;

						try {
							relatedClassesJson = JSONUtil
									.serialize(relatedClasses);
						} catch (JSONException e) {
							relatedClassesJson = "[]";
						}

						// update info field
						rowData.set(
								0,
								"<input type=\"image\" src=\"resources/images/info-small.png\" "
										+ "onClick='javascript:showIndividualInfo(\""
										+ dto.getIdentifier() + "\",\""
										+ classObject.getIdentifier() + "\","
										+ relatedClassesJson + ")'>");

						gridData.add(rowData);
					}
				}
			}
		}

		return dtoGrid;
	}

	protected String resolveValueMap(String id) throws DataModelException {
		String label = id;

		try {
			HttpClient httpClient = new HttpClient(refDataServiceUri);
			HttpUtils.addHttpHeaders(session, httpClient);

			Entity value = httpClient.get(Entity.class,
					"/classes/" + id.substring(4, id.length()) + "/label");

			if (value != null && value.getLabel() != null) {
				label = value.getLabel();
			}
		} catch (HttpClientException e) {
			logger.error(e.getMessage());
			throw new DataModelException(e.getMessage());
		}

		return label;
	}

	protected Cardinality getCardinality(Graph graph, String className,
			String templateName, String roleName, String relatedClassName) {
		for (ClassTemplates classTemplates : graph.getClassTemplatesList()
				.getItems()) {
			String clsName = IOUtils.toCamelCase(classTemplates.getClazz()
					.getName());

			if (clsName.equalsIgnoreCase(className)) {
				for (Template template : classTemplates.getTemplates()
						.getItems()) {
					if (template.getName().equalsIgnoreCase(templateName)) {
						for (Role role : template.getRoles().getItems()) {
							if (role.getName().equalsIgnoreCase(roleName)) {
								return role.getCardinality();
							}
						}
					}
				}
			}
		}

		return null;
	}

	protected String getValueMapKey(String value,
			HashMap<String, String> valueMaps) {
		for (String key : valueMaps.keySet()) {
			if (valueMaps.get(key) == null)
				continue;

			if (valueMaps.get(key).equalsIgnoreCase(value))
				return key;
		}
		return null;
	}

	@SuppressWarnings("unchecked")
	protected String getValueMap(Manifest manifest, String value)
			throws DataModelException {
		Map<String, String> valueMaps;
		String valueMap = value;

		// find value map in manifest first
		if (manifest != null && manifest.getValueListMaps() != null) {
			for (ValueListMap vlm : manifest.getValueListMaps().getItems()) {
				if (vlm.getValueMaps() != null) {
					for (ValueMap vm : vlm.getValueMaps().getItems()) {
						if (vm.getUri() != null
								&& vm.getUri().equalsIgnoreCase(value)
								&& vm.getLabel() != null
								&& vm.getLabel().length() > 0) {
							return vm.getLabel();
						}
					}
				}
			}
		}

		// if not found, find it in session
		if (session.containsKey("valueMaps")) {
			valueMaps = (Map<String, String>) session.get("valueMaps");
		} else {
			valueMaps = new HashMap<String, String>();
			session.put("valueMaps", valueMaps);
		}

		// if still not found, query reference data service
		if (value != null && !value.isEmpty()) {
			if (!valueMaps.containsKey(value)) {
				valueMap = resolveValueMap(value);
				valueMaps.put(value, valueMap);
			} else {
				valueMap = valueMaps.get(value);
			}
		}

		return valueMap;
	}

	protected DataFilter createDataFilter(String filter, String sortBy,
			String sortOrder) throws DataModelException {
		DataFilter dataFilter = new DataFilter();

		@SuppressWarnings("unchecked")
		HashMap<String, String> valueMaps = (HashMap<String, String>) session
				.get("valueMaps");

		// process filtering
		if (filter != null && filter.length() > 0) {
			try {
				@SuppressWarnings("unchecked")
				List<Map<String, String>> filterExpressions = (List<Map<String, String>>) JSONUtil
						.deserialize(filter);

				if (filterExpressions != null && filterExpressions.size() > 0) {
					// dataFilter = new DataFilter();

					Expressions expressions = new Expressions();
					dataFilter.setExpressions(expressions);

					for (Map<String, String> filterExpression : filterExpressions) {
						Expression expression = new Expression();
						expressions.getItems().add(expression);

						if (expressions.getItems().size() > 1) {
							expression.setLogicalOperator(LogicalOperator.AND);
						}

						if (filterExpression.containsKey("comparison")) {
							String operator = filterExpression
									.get("comparison");
							expression
									.setRelationalOperator(relationalOperatorMap
											.get(operator));
						} else {
							expression
									.setRelationalOperator(relationalOperatorMap
											.get("eq"));
						}

						expression.setPropertyName(filterExpression
								.get("field"));

						Values values = new Values();
						expression.setValues(values);

						List<String> valueItems = new ArrayList<String>();
						values.setItems(valueItems);

						String value = String.valueOf(filterExpression
								.get("value"));

						if (valueMaps != null) {
							String valueMap = getValueMapKey(
									String.valueOf(filterExpression
											.get("value")), valueMaps);

							if (valueMap != null && !valueMap.isEmpty()) {
								valueItems.add(valueMap);
								value = valueMap;
							}
						}

						valueItems.add(value);
					}
				}
			} catch (JSONException e) {
				String message = "Error creating data filter: " + e;
				logger.error(message);
				throw new DataModelException(message);
			}
		} else {
			dataFilter.setExpressions(null);
		}

		// process sorting
		if (sortBy != null && sortBy.length() > 0 && sortOrder != null
				&& sortOrder.length() > 0) {
			// if (dataFilter == null)
			// dataFilter = new DataFilter();

			OrderExpressions orderExpressions = new OrderExpressions();
			dataFilter.setOrderExpressions(orderExpressions);

			OrderExpression orderExpression = new OrderExpression();
			orderExpressions.getItems().add(orderExpression);

			if (sortBy != null)
				orderExpression.setPropertyName(sortBy);

			if (sortOrder != null)
				orderExpression.setSortOrder(SortOrder.valueOf(sortOrder));
		} else {
			dataFilter.setOrderExpressions(null);
		}

		return dataFilter;
	}

	protected Manifest getManifest(String serviceUri,
			String manifestRelativePath) throws DataModelException {
		Manifest manifest = null;
		String manifestKey = MANIFEST_PREFIX + manifestRelativePath;

		if (session.containsKey(manifestKey)) {
			manifest = (Manifest) session.get(manifestKey);
		} else {
			HttpClient httpClient = new HttpClient(serviceUri);
			HttpUtils.addHttpHeaders(session, httpClient);

			try {
				manifest = httpClient.get(Manifest.class, manifestRelativePath);
				session.put(manifestKey, manifest);
			} catch (HttpClientException e) {
				logger.error(e.getMessage());
				throw new DataModelException(e.getMessage());
			}
		}

		return manifest;
	}

	protected Graph getGraph(Manifest manifest, String graphName) {
		if (manifest.getGraphs() != null) {
			for (Graph graph : manifest.getGraphs().getItems()) {
				if (graph.getName().equalsIgnoreCase(graphName)) {
					return graph;
				}
			}
		}

		return null;
	}

	protected void removeSessionData(String key) {
		if (session != null && session.keySet().contains(key)) {
			session.remove(key);
		}
	}

	//
	// remove DTIs from the partial list who are not in the full list; also
	// apply internal identifier and hash value
	//
	private boolean parsePartialDtis(List<DataTransferIndex> partialDtiList,
			List<DataTransferIndex> fullDtiList) {
		int count = 0;

		for (int i = 0; i < partialDtiList.size(); i++) {
			boolean exists = false;

			for (int j = 0; j < fullDtiList.size(); j++) {
				if (partialDtiList.get(i).getIdentifier()
						.equalsIgnoreCase(fullDtiList.get(j).getIdentifier())) {
					partialDtiList.get(i).setInternalIdentifier(
							fullDtiList.get(j).getInternalIdentifier());
					partialDtiList.get(i).setTransferType(
							fullDtiList.get(j).getTransferType());
					partialDtiList.get(i).setDuplicateCount(
							fullDtiList.get(j).getDuplicateCount());

					exists = true;
					count++;

					break;
				}
			}

			if (!exists) {
				partialDtiList.remove(i--);
			}
		}

		return count == fullDtiList.size();
	}

	private List<ClassObject> getFilteredClasses(Expression expression,
			List<ClassObject> classObjects) {
		List<ClassObject> filteredClassObjects = new ArrayList<ClassObject>();
		String[] propertyParts = expression.getPropertyName().split("\\.");

		for (ClassObject classObject : classObjects) {
			if (classObject.getName().equalsIgnoreCase(propertyParts[0])) {
				List<TemplateObject> templateObjects = classObject
						.getTemplateObjects().getItems();

				for (TemplateObject templateObject : templateObjects) {
					if (templateObject.getName().equalsIgnoreCase(
							propertyParts[1])) {
						List<RoleObject> roleObjects = templateObject
								.getRoleObjects().getItems();

						for (RoleObject roleObject : roleObjects) {
							RoleType roleType = roleObject.getType();

							if ((roleType == null
									|| // bug in v2.0 of c# service
									roleType == RoleType.PROPERTY
									|| roleType == RoleType.DATA_PROPERTY
									|| roleType == RoleType.OBJECT_PROPERTY || roleType == RoleType.FIXED_VALUE)
									&& roleObject.getName().equalsIgnoreCase(
											propertyParts[2])) {
								int compareValue = roleObject.getValue()
										.compareToIgnoreCase(
												expression.getValues()
														.getItems().get(0));
								RelationalOperator relationalOperator = expression
										.getRelationalOperator();

								// TODO: handle numeric, date, time comparison
								if ((relationalOperator == RelationalOperator.EQUAL_TO && compareValue == 0)
										|| (relationalOperator == RelationalOperator.GREATER_THAN && compareValue > 0)
										|| (relationalOperator == RelationalOperator.LESSER_THAN && compareValue < 0)) {
									filteredClassObjects.add(classObject);
								}

								break;
							}
						}
					}
				}
			}
		}

		return filteredClassObjects;
	}

	@SuppressWarnings("unchecked")
	protected List<Field> getFields(String fieldsContext, Graph graph,
			String startClassId) throws DataModelException {
		List<Field> fields = null;
		String fieldsKey = FIELDS_PREFIX + fieldsContext + graph.getName();

		if (session.containsKey(fieldsKey)) {
			fields = (List<Field>) session.get(fieldsKey);
		} else {
			fields = createFields(graph, startClassId);
			session.put(fieldsKey, fields);
		}

		return fields;
	}

	private List<Field> createFields(Graph graph, String startClassId) {
		List<Field> fields = new ArrayList<Field>();

		if (dataMode == DataMode.EXCHANGE) {

			// Dups count field
			Field dupField = new Field();
			dupField.setName("Status");
			dupField.setDataIndex("Statua");
			dupField.setType("string");
			dupField.setWidth(STATUS_FIELD_WIDTH);
			dupField.setFixed(true);
			dupField.setFilterable(false);
			dupField.setSortable(false);
			fields.add(0, dupField);

			// transfer-type field
			Field field = new Field();
			field.setName("Transfer Type");
			field.setDataIndex("Transfer Type");
			field.setType("string");
			field.setWidth(100);
			field.setFilterable(true);
			fields.add(0, field);

		/*	// Status field
			Field statusField = new Field();
			field.setName("Status");
			field.setDataIndex("Status");
			field.setType("string");
			field.setWidth(STATUS_FIELD_WIDTH);
			field.setFilterable(true);
			fields.add(0, statusField);*/
		}

		// info field
		Field field = new Field();
		field.setName("&nbsp;");
		field.setDataIndex("&nbsp;");
		field.setType("string");
		field.setWidth(INFO_FIELD_WIDTH);
		field.setFixed(true);
		field.setFilterable(false);
		fields.add(0, field);

		List<ClassTemplates> classTemplatesItems = graph
				.getClassTemplatesList().getItems();

		if (classTemplatesItems.size() > 0) {
			if (startClassId == null || startClassId.length() == 0) {
				ClassTemplates classTemplates = classTemplatesItems.get(0);
				createFields(fields, graph, classTemplates);
			} else {
				for (ClassTemplates classTempates : classTemplatesItems) {
					if (classTempates.getClazz().getId()
							.equalsIgnoreCase(startClassId)) {
						createFields(fields, graph, classTempates);
						break;
					}
				}
			}
		}

		return fields;
	}

	private void createFields(List<Field> fields, Graph graph,
			ClassTemplates classTemplates) {
		if (classTemplates != null && classTemplates.getTemplates() != null) {
			String className = IOUtils.toCamelCase(classTemplates.getClazz()
					.getName());

			for (Template template : classTemplates.getTemplates().getItems()) {
				for (Role role : template.getRoles().getItems()) {
					org.iringtools.mapping.RoleType roleType = role.getType();
					Cardinality cardinality = role.getCardinality();

					if (roleType == null
							|| // bug in v2.0 of c# service
							roleType == org.iringtools.mapping.RoleType.PROPERTY
							|| roleType == org.iringtools.mapping.RoleType.DATA_PROPERTY
							|| roleType == org.iringtools.mapping.RoleType.OBJECT_PROPERTY
							|| roleType == org.iringtools.mapping.RoleType.FIXED_VALUE
							|| (cardinality != null && cardinality == Cardinality.SELF)) {
						String dataType = role.getDataType();
						String fieldName = className + '.' + template.getName()
								+ "." + role.getName();
						Field field = new Field();

						field.setName(fieldName);
						field.setDataIndex(fieldName);
						field.setWidth(MIN_FIELD_WIDTH);

						// adjust field width
						if (fieldFit == FieldFit.HEADER) {
							int fieldWidth = fieldName.length()
									* HEADER_PX_PER_CHAR;

							if (fieldWidth > MIN_FIELD_WIDTH) {
								field.setWidth(fieldWidth + FIELD_PADDING);
							}
						}

						if (dataMode == DataMode.APP && dataType != null
								&& dataType.startsWith("xsd:")) {
							dataType = dataType.replace("xsd:", "")
									.toLowerCase();

							if (!gridFilterTypes.contains(dataType)) {
								dataType = "string";
							}

							field.setType(dataType);
						} else {
							field.setType("string");
						}

						fields.add(field);
					} else if (role.getClazz() != null
							&& (cardinality == null || cardinality == Cardinality.ONE_TO_ONE)) {
						String classId = role.getClazz().getId();
						ClassTemplates relatedClassTemplates = getClassTemplates(
								graph, classId);
						createFields(fields, graph, relatedClassTemplates);
					}
				}
			}
		}
	}

	private ClassTemplates getClassTemplates(Graph graph, String classId) {
		for (ClassTemplates classTemplates : graph.getClassTemplatesList()
				.getItems()) {
			if (classTemplates.getClazz().getId().equals(classId))
				return classTemplates;
		}

		return null;
	}

	private void processClassObject(Manifest manifest, Graph graph,
			DataTransferObject dto, int dtoIndex, List<Field> fields,
			ClassObject classObject, Grid dtoGrid, List<String> rowData,
			List<RelatedClass> relatedClasses) throws DataModelException {
		String className = IOUtils.toCamelCase(classObject.getName());

		for (TemplateObject templateObject : classObject.getTemplateObjects()
				.getItems()) {
			for (RoleObject roleObject : templateObject.getRoleObjects()
					.getItems()) {
				RoleType roleType = roleObject.getType();
				RoleValues roleValues = roleObject.getValues();
				RoleValues roleOldValues = roleObject.getOldValues();
				String roleValue = roleObject.getValue();
				String roleOldValue = roleObject.getOldValue();
				Cardinality cardinality = getCardinality(graph, className,
						templateObject.getName(), roleObject.getName(),
						roleObject.getRelatedClassName());

				if (templateObject.getTransferType() == TransferType.CHANGE) {
					if (roleOldValue == null)
						roleOldValue = "";
					if (roleValue == null)
						roleValue = "";
				}

				if (roleType == null
						|| // bug in v2.0 of c# service
						roleType == RoleType.PROPERTY
						|| roleType == RoleType.DATA_PROPERTY
						|| roleType == RoleType.OBJECT_PROPERTY
						|| roleType == RoleType.FIXED_VALUE
						|| (cardinality != null && cardinality == Cardinality.SELF)) {
					// compute role value
					if (roleObject.getHasValueMap() != null
							&& roleObject.getHasValueMap()) {
						if (!IOUtils.isNullOrEmpty(roleValue)) {
							roleValue = getValueMap(manifest, roleValue);
						}

						if (dataMode == DataMode.EXCHANGE
								&& !IOUtils.isNullOrEmpty(roleOldValue)) {
							roleOldValue = getValueMap(manifest, roleOldValue);
						}
					} else if (roleValues != null
							&& roleValues.getItems().size() > 0) {
						roleValue = getMultiRoleValues(manifest, roleObject,
								roleValues.getItems());

						if (roleOldValues != null
								&& roleOldValues.getItems().size() > 0)
							roleOldValue = getMultiRoleValues(manifest,
									roleObject, roleOldValues.getItems());
					}

					// find the right column to insert value, fill in blank for
					// any gap
					// (because class/template do not exist, e.g. due to null
					// class identifier)
					String dataIndex = className + '.'
							+ templateObject.getName() + '.'
							+ roleObject.getName();

					if (rowData.size() == fields.size()) {
						for (int i = 0; i < fields.size(); i++) {
							if (fields.get(i).getDataIndex()
									.equalsIgnoreCase(dataIndex)) {
								if (dataMode == DataMode.APP
										|| roleOldValue == null
										|| roleOldValue.equals(roleValue)) {
									rowData.set(i, roleValue);
								} else {
									roleValue = roleOldValue + " -> "
											+ roleValue;
									rowData.set(i, "<span class=\"change\">"
											+ roleValue + "</span>");
								}

								break;
							}
						}
					} else {
						while (rowData.size() < fields.size()) {
							if (!fields.get(rowData.size()).getDataIndex()
									.equalsIgnoreCase(dataIndex)) {
								rowData.add("");
							} else {
								break;
							}
						}

						// add row value to row data
						if (rowData.size() < fields.size()) {
							if (dataMode == DataMode.APP
									|| roleOldValue == null
									|| roleOldValue.equals(roleValue)) {
								rowData.add(roleValue);
							} else {
								roleValue = roleOldValue + " -> " + roleValue;
								rowData.add("<span class=\"change\">"
										+ roleValue + "</span>");
							}
						}
					}

					// adjust field width based on value
					if (fieldFit == FieldFit.VALUE) {
						Field field = fields.get(rowData.size() - 1);
						int fieldWidth = field.getWidth();
						int newWidth = roleValue.length() * VALUE_PX_PER_CHAR;

						if (newWidth > MIN_FIELD_WIDTH && newWidth > fieldWidth
								&& newWidth < MAX_FIELD_WIDTH) {
							field.setWidth(newWidth);
						}
					}
				} else if (roleObject.getRelatedClassId() != null
						&& (roleObject.getValue() != null && roleObject
								.getValue().startsWith("#")) || // v2.0
						roleObject.getValues() != null) // v2.1
				{
					if ((roleObject.getValue() != null && roleObject.getValue()
							.startsWith("#")) || // v2.0
							(cardinality == null || cardinality == Cardinality.ONE_TO_ONE)) // v2.1
					{
						String relatedClassIdentifier;

						if (roleObject.getValue() != null
								&& roleObject.getValue().startsWith("#")) // v2.0
						{
							relatedClassIdentifier = roleObject.getValue()
									.substring(1);
						} else
						// v2.1
						{
							relatedClassIdentifier = roleObject.getValues()
									.getItems().get(0);
						}

						// find related class and recur
						for (ClassObject relatedClassObject : dto
								.getClassObjects().getItems()) {
							if (relatedClassObject.getClassId().equals(
									roleObject.getRelatedClassId())
									&& relatedClassObject.getIdentifier()
											.equals(relatedClassIdentifier)) {
								processClassObject(manifest, graph, dto,
										dtoIndex, fields, relatedClassObject,
										dtoGrid, rowData, relatedClasses);

								break;
							}
						}
					} else {
						String relatedClassName = IOUtils
								.toCamelCase(roleObject.getRelatedClassName());

						if (!relatedClassExists(relatedClasses,
								relatedClassName)) {
							RelatedClass relatedClass = new RelatedClass();
							relatedClass.setId(roleObject.getRelatedClassId());
							relatedClass.setName(relatedClassName);
							relatedClasses.add(relatedClass);
						}
					}
				}
			}
		}
	}

	private boolean relatedClassExists(List<RelatedClass> relatedClasses,
			String relatedClassName) {
		for (RelatedClass relatedClass : relatedClasses) {
			if (relatedClass.getName().equalsIgnoreCase(relatedClassName))
				return true;
		}

		return false;
	}

	private String getMultiRoleValues(Manifest manifest, RoleObject roleObject,
			List<String> roleValues) throws DataModelException {
		StringBuilder roleValueBuilder = new StringBuilder();

		for (String value : roleValues) {
			if (roleObject.getHasValueMap() != null
					&& roleObject.getHasValueMap()
					&& !IOUtils.isNullOrEmpty(value)) {
				value = getValueMap(manifest, value);
			}

			if (roleValueBuilder.length() > 0) {
				roleValueBuilder.append(",");
			}

			roleValueBuilder.append(value);
		}

		return roleValueBuilder.toString();
	}

	protected <T> T waitForRequestCompletion(Class<T> clazz, String url) {
		T obj = null;

		try {
			RequestStatus requestStatus = null;
			long timeoutCount = 0;

			HttpClient httpClient = new HttpClient(url);
			HttpUtils.addHttpHeaders(session, httpClient);

			while (timeoutCount < timeout) {
				requestStatus = httpClient.get(RequestStatus.class);

				if (requestStatus.getState() != State.IN_PROGRESS)
					break;

				Thread.sleep(interval);
				timeoutCount += interval;
			}

// Note that the Java iRing services (unlike the C# iRingr services) appear to UTF8 encode twice, once with build and secondly when sending the response
// so the object embedded within the response text is still UTF-8 encoded when we get to this point. 
//			InputStream streamUTF8 = new ByteArrayInputStream(requestStatus.getResponseText().getBytes("UTF-8"));
//			obj = (T) JaxbUtils.toObject(clazz, streamUTF8);
			obj = (T) JaxbUtils.toObject(clazz, requestStatus.getResponseText());
			
		} catch (Exception ex) {
			logger.error(ex.getMessage());
		}

		return obj;
	}
}
