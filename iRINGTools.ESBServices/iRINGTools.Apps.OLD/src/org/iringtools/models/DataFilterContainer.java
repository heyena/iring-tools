package org.iringtools.models;

import java.util.ArrayList;
import java.util.List;

import org.codehaus.jettison.json.JSONArray;
import org.codehaus.jettison.json.JSONObject;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expression;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.OrderExpression;
import org.iringtools.data.filter.OrderExpressions;
import org.iringtools.data.filter.RelationalOperator;
import org.iringtools.data.filter.SortOrder;
import org.iringtools.data.filter.Values;

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
				Values valueList = new Values();
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
					valueList.getValues().add(jsonObject.getString("value"));
					expression.setValues(valueList);
					expressionList.add(expression);

				} catch (Exception e) {
					System.out.println("Exception :" + e);
				}

			}
		
				Expressions expressionListObject = new Expressions();
				expressionListObject.setItems(expressionList);
				dataFilter.setExpressions(expressionListObject);
			
			
				OrderExpressions orderExpressionListObject = new OrderExpressions();
				orderExpressionListObject.setItems(orderExpressionList);
				dataFilter.setOrderExpressions(orderExpressionListObject);
			
		
	}
	
	public DataFilter getDataFilter() {
		return dataFilter;
	}

	public RelationalOperator getRelationalOperator (String operator) {
		if (operator.equals("eq"))
			return RelationalOperator.EQUAL_TO;
		else if (operator.equals("gt"))
			return RelationalOperator.LESSER_THAN;
		else if (operator.equals("lt"))
			return RelationalOperator.GREATER_THAN;		
		return null;
	}
	
	public SortOrder getSortOrder (String sortOperator) {
		if(sortOperator.equals("ASC"))
			return SortOrder.ASC;
		else if (sortOperator.equals("DESC"))
			return SortOrder.DESC;
		return null;
	}
}
