package org.iringtools.utility;

import org.iringtools.data.filter.*;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class DataFilterInitial {

	public DataFilter AppendFilter(DataFilter filter, DataFilter initialFilter) {
		DataFilter resultFilter = new DataFilter();
		Expressions resultExpressions = new Expressions();
		OrderExpressions resultOe = new OrderExpressions();
		Expressions inex = initialFilter.getExpressions();
		OrderExpressions inoe = initialFilter.getOrderExpressions();
		Expressions exs = filter.getExpressions();
		OrderExpressions oe = filter.getOrderExpressions();

		// if only one filter object have values.

		if ((exs == null) && (inex != null))
			resultExpressions.setItems(inex.getItems());

		if ((exs != null) && (inex == null))
			resultExpressions.setItems(exs.getItems());

		if ((oe == null) && (inoe != null))
			resultOe.setItems(inoe.getItems());

		if ((oe != null) && (inoe == null))
			resultOe.setItems(oe.getItems());
		
		// if both the filter are null
		if ((inoe == null) && (exs == null) && (inex== null) && (oe == null)) {
			resultExpressions = null;
			resultOe = null;
		}

		// If both filters have values

		List<Expression> expressionList = new ArrayList<Expression>();

		if ((inex != null) && (exs != null)) {
			for (Expression ex : inex.getItems()) {
				expressionList.add(ex);

			}  
			int count = 0;
			for (Expression ex : exs.getItems()) {
				if(count == 0)
				{
				  if( ex.getLogicalOperator() == null)
				  {
					  ex.setLogicalOperator(LogicalOperator.AND);
					  count++;
				  }
				}
				expressionList.add(ex);
			}
			// Collections.sort(resultExpressions, valueComparator);

			resultExpressions.setItems(expressionList);
		}

		List<OrderExpression> OrderexpressList = new ArrayList<OrderExpression>();
		if ((oe != null) && (inoe != null)) {
			for (OrderExpression ox : oe.getItems()) {
				OrderexpressList.add(ox);
			}
			for (OrderExpression ox : inoe.getItems()) {
				OrderexpressList.add(ox);
			}
			// Collections.sort(OrderexpressList, valueComparator);
			resultOe.setItems(OrderexpressList);
		}

		resultFilter.setExpressions(resultExpressions);
		resultFilter.setOrderExpressions(resultOe);

		return resultFilter;
		/*
		 * if (oe == null) oe = new OrderExpressions();
		 * 
		 * if (filter != null) { DataFilter clonedFilter =
		 * Utility.CloneDataContractObject<DataFilter>(filter);
		 * 
		 * if (clonedFilter.Expressions != null &&
		 * clonedFilter.Expressions.Count > 0) { int maxIndex =
		 * clonedFilter.Expressions.Count - 1;
		 * clonedFilter.Expressions[0].LogicalOperator = LogicalOperator.And;
		 * clonedFilter.Expressions[0].OpenGroupCount++;
		 * clonedFilter.Expressions[maxIndex].CloseGroupCount++;
		 * Expressions.AddRange(clonedFilter.Expressions); }
		 * 
		 * 
		 * 
		 * if (clonedFilter.OrderExpressions != null &&
		 * clonedFilter.OrderExpressions.Count > 0) { foreach (OrderExpression
		 * orderExpression in clonedFilter.OrderExpressions) { if
		 * (!DuplicateOrderExpression(orderExpression))
		 * OrderExpressions.Add(orderExpression); } } } }
		 * 
		 * private bool DuplicateOrderExpression(OrderExpression
		 * orderExpression)
		 * 
		 * {
		 * 
		 * foreach (OrderExpression item in OrderExpressions) { if
		 * (item.PropertyName.ToLower() ==
		 * orderExpression.PropertyName.ToLower()) { if(item.SortOrder ==
		 * orderExpression.SortOrder) return true;
		 * 
		 * //else
		 * 
		 * //{
		 * 
		 * // item.SortOrder = orderExpression.SortOrder;
		 * 
		 * // return true;
		 * 
		 * //}
		 * 
		 * }
		 * 
		 * }
		 * 
		 * return false;
		 */
	}
}
