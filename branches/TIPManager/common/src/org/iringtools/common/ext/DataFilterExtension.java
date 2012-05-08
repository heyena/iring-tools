package org.iringtools.common.ext;

import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.OrderExpressions;

public class DataFilterExtension extends DataFilter
{	
	public DataFilterExtension()
	{
		this.expressions = new Expressions();
		this.orderExpressions = new OrderExpressions();
	}	
	
	public DataFilter getDataFilter(final DataFilter dataFilter)
	{
		DataFilter dataFilter1 = new DataFilter();
		
		if (dataFilter != null)
		{
			if (dataFilter.getExpressions() == null && dataFilter.getOrderExpressions() != null)
			{
				Expressions expressions = new Expressions();
				dataFilter1.setExpressions(expressions);
				dataFilter1.setOrderExpressions(dataFilter.getOrderExpressions());
			}
			else if (dataFilter.getOrderExpressions() == null && dataFilter.getExpressions() != null)
			{			
				OrderExpressions orderExpressions = new OrderExpressions();
				dataFilter1.setOrderExpressions(orderExpressions);
				dataFilter1.setExpressions(dataFilter.getExpressions());
			}
			else
			{
				dataFilter1.setExpressions(dataFilter.getExpressions());
				dataFilter1.setOrderExpressions(dataFilter.getOrderExpressions());
			}
		}		
		else
			dataFilter1 = dataFilter;
		
		return dataFilter1;	
	}	
	
}
