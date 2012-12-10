package org.iringtools.utility;

import org.iringtools.data.filter.*;
import java.util.ArrayList;
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
		if ((inoe == null) && (exs == null) && (inex == null) && (oe == null)) {
			resultExpressions = null;
			resultOe = null;
		}

		// If both filters have values

		List<Expression> expressionList = new ArrayList<Expression>();
		int inExcount = 0, uiExcount=0;
		int i = 0, j= 0;
		if ((inex != null) && (exs != null)) {
			for (Expression ex : inex.getItems()) {
				i++;
				if(!(ex.getPropertyName().equalsIgnoreCase("Transfer Type")))
				{
				if (inExcount == 0) {
					ex.setOpenGroupCount(ex.getOpenGroupCount() + 1);
					inExcount++;
				}				
				if (inex.getItems().size() == i) {
					ex.setCloseGroupCount(ex.getCloseGroupCount() + 1);
				}
				}
				expressionList.add(ex);

			}
			int count = 0;
			for (Expression ex : exs.getItems()) {
				if (count == 0) {
					if (ex.getLogicalOperator() == null) {
						ex.setLogicalOperator(LogicalOperator.AND);
						count++;
					}
				}
				j++;
				if(!(ex.getPropertyName().equalsIgnoreCase("Transfer Type")))
				{
				if (uiExcount == 0) {
					ex.setOpenGroupCount(ex.getOpenGroupCount() + 1);
					uiExcount++;
				}				
				if (exs.getItems().size() == j) {
					ex.setCloseGroupCount(ex.getCloseGroupCount() + 1);
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
	}
}
