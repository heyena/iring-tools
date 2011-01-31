package org.iringtools.models;

import java.util.ArrayList;
import java.util.List;

import org.codehaus.jettison.json.JSONArray;
import org.codehaus.jettison.json.JSONObject;
import org.iringtools.ui.widgets.grid.Grid;
import org.iringtools.ui.widgets.grid.GridDefinition;
import org.iringtools.ui.widgets.grid.Rows;
import org.iringtools.utility.HttpClient;
import org.iringtools.dxfr.datafilter.DataFilter;
import org.iringtools.dxfr.datafilter.Expression;
import org.iringtools.dxfr.datafilter.ExpressionList;
import org.iringtools.dxfr.datafilter.OrderExpression;
import org.iringtools.dxfr.datafilter.OrderExpressionList;
import org.iringtools.dxfr.datafilter.RelationalOperator;
import org.iringtools.dxfr.datafilter.SortOrder;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.history.History;

import com.opensymphony.xwork2.ActionContext;

public class DataFilterContainer {
	
	private DataFilter dataFilter = null;
	private List<Expression> expressionList = null;	
	private List<OrderExpression> orderExpressionList = null;

	public DataFilterContainer(String filter, String sortOrder, String sortBy) {
		expressionList = new ArrayList<Expression>();	
		orderExpressionList = new ArrayList<OrderExpression>();
		dataFilter = new DataFilter();
		
		try {
			if (filter != null) {
				JSONArray jsonFilter = new JSONArray(filter);
				createDataFilter(jsonFilter, sortOrder, sortBy);
			}
			else {
				createDataFilter(null, sortOrder, sortBy);
			}
					
		}
		catch (Exception e) {
			System.out.println("Exception :" + e);
		}		
	}
	
	
	
	public void createDataFilter(JSONArray jsonFilter, String sortOrder, String sortBy) {
		JSONObject jsonObject;
		String operator = null;			
		
		OrderExpression orderExpression = new OrderExpression();
		
		if (sortBy != null)
			orderExpression.setPropertyName(sortBy);
		if (sortOrder != null)
			orderExpression.setSortOrder(getSortOrder(sortOrder));
		if (sortBy != null || sortOrder != null)
			orderExpressionList.add(orderExpression);
		
		
		if (jsonFilter != null)
			for (int i = 0; i < jsonFilter.length(); i++) {
				Expression expression = new Expression();
				List<String> valueList = new ArrayList<String>();
				try {
					jsonObject = jsonFilter.getJSONObject(i);
					expression.setOpenGroupCount(jsonFilter.length());
					expression.setCloseGroupCount(jsonFilter.length());

					if (jsonObject.has("comparison")) {
						operator = jsonObject.getString("comparison");
						expression
								.setRelationalOperator(getRelationalOperator(operator));
					} else {
						expression
								.setRelationalOperator(RelationalOperator.EQUAL_TO);
					}

					expression.setPropertyName(jsonObject.getString("field"));
					valueList.add(jsonObject.getString("value"));
					expression.setValues(valueList);
					expressionList.add(expression);

				} catch (Exception e) {
					System.out.println("Exception :" + e);
				}

			}
		
		if (expressionList.isEmpty() && orderExpressionList.isEmpty()) {
			dataFilter.setExpressions(null);
			dataFilter.setOrderExpressions(null);
		} else {
			if (!expressionList.isEmpty()) {
				ExpressionList expressionListObject = new ExpressionList();
				expressionListObject.setItems(expressionList);
				dataFilter.setExpressions(expressionListObject);
			}
			if (!orderExpressionList.isEmpty()) {
				OrderExpressionList orderExpressionListObject = new OrderExpressionList();
				orderExpressionListObject.setItems(orderExpressionList);
				dataFilter.setOrderExpressions(orderExpressionListObject);
			}
		}
	}
	
	public DataFilter getDataFilter() {
		return dataFilter;
	}

	public RelationalOperator getRelationalOperator (String operator) {
		if (operator == "eq")
			return RelationalOperator.EQUAL_TO;
		else if (operator == "gt" )
			return RelationalOperator.LESSER_THAN;
		else if (operator == "lt")
			return RelationalOperator.GREATER_THAN;		
		return null;
	}
	
	public SortOrder getSortOrder (String sortOperator) {
		if(sortOperator == "ASC")
			return SortOrder.ASC;
		else if (sortOperator == "DESC")
			return SortOrder.DESC;
		return null;
	}
}
