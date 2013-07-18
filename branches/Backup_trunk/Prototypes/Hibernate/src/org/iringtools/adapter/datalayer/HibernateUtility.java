package org.iringtools.adapter.datalayer;

import java.io.IOException;
import java.math.BigDecimal;
import java.sql.Date;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.JAXBException;
import org.hibernate.Criteria;
import org.hibernate.Session;
import org.hibernate.criterion.Criterion;
import org.hibernate.criterion.Order;
import org.hibernate.criterion.Restrictions;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expression;
import org.iringtools.data.filter.LogicalOperator;
import org.iringtools.data.filter.OrderExpression;
import org.iringtools.data.filter.SortOrder;
import org.iringtools.data.filter.Values;
import org.iringtools.library.DataObject;
import org.iringtools.library.DataProperty;
import org.iringtools.library.DataType;
import org.iringtools.library.DatabaseDictionary;
import org.iringtools.utility.EncryptionException;
import org.iringtools.utility.EncryptionUtils;
import org.iringtools.utility.JaxbUtils;

public class HibernateUtility {

	public static DatabaseDictionary loadDatabaseDictionary(String path, String keyFile)
    {      
		DatabaseDictionary dbDictionary =null;
      
		try{
    	  dbDictionary = JaxbUtils.read(DatabaseDictionary.class, path);
          
          String connStr = dbDictionary.getConnectionString();
          
	      if (connStr != null)
	      {
	    	if (connStr.toUpperCase().contains("DATA SOURCE"))
	        {
	          // connection String is not encrypted, encrypt and write it back
	
	          dbDictionary.setConnectionString((keyFile.isEmpty() || keyFile==null) ? EncryptionUtils.encrypt(connStr) : EncryptionUtils.encrypt(connStr, keyFile));
	          JaxbUtils.write(dbDictionary, path, false);
	          dbDictionary.setConnectionString(connStr);
	        }
	        else
	        {
	          dbDictionary.setConnectionString((keyFile.isEmpty() || keyFile==null)
	          ? EncryptionUtils.encrypt(connStr)
	                  : EncryptionUtils.encrypt(connStr, keyFile));
	        }
	      }
      }catch(EncryptionException ee){
    	  //logger.error(String.format("Error in GetScopes: {0}", ex));
      }catch(IOException ie){
    	  
      }catch(JAXBException je){
    	  
      }catch(Exception e){
    	  
      }

      return dbDictionary;      
    }

	public static void saveDatabaseDictionary(DatabaseDictionary dbDictionary, String path, String keyFile)
    {
      String connStr = dbDictionary.getConnectionString();

      if (connStr != null)
      {
    	if (connStr.toUpperCase().contains("DATA SOURCE"))
        {
          // connection string is not encrypted, encrypt and write it back
          try {
			
        	  dbDictionary.setConnectionString((keyFile.isEmpty() || keyFile==null) ? EncryptionUtils.encrypt(connStr) : EncryptionUtils.encrypt(connStr, keyFile));

          } catch (EncryptionException e) {
			
        	// TODO Auto-generated catch block
			e.printStackTrace();
		} 
        }
      }
      
      try {

    	  JaxbUtils.write(dbDictionary, path,false);
  

      } catch (JAXBException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();

      } catch (IOException e) {
		// TODO Auto-generated catch block
		e.printStackTrace();

      }
    }

    public static Criteria createCriteria(Session session, Class objectType, DataObject objectDefinition, DataFilter dataFilter)
    {      
      Criteria criteria = session.createCriteria(objectType);
      addCriteriaExpressions(criteria, objectDefinition, dataFilter);

      return criteria;
    }

    public static Criteria createCriteria(Session session, Class objectType, DataObject objectDefinition, DataFilter dataFilter, int start, int limit)
    {
      Criteria criteria = createCriteria(session, objectType, objectDefinition, dataFilter);

      if (start >= 0 && limit > 0)
      {
        criteria.setFirstResult(start).setMaxResults(limit);
      }

      return criteria;
    }

    private static void addCriteriaExpressions(Criteria criteria, DataObject objectDefinition, DataFilter dataFilter)
    {
    	
      if (dataFilter != null)
      {
    	  List<Expression> expressionList = dataFilter.getExpressions().getItems();
    	  
        if (expressionList != null)
        {
          List<Criterion> criterions = new ArrayList<Criterion>();
          String wildcard = "%";
          
          List<DataProperty> dataProp = objectDefinition.getDataProperties();
          
          for (Expression expression : expressionList)
          {
        	  DataProperty dataProperty = null;  
          
        	  for(DataProperty dataProps : dataProp){
      		  
      		  if(dataProps.getPropertyName().equalsIgnoreCase(expression.getPropertyName())){
      			
      			  dataProperty = dataProps;
      			  
      			  break;
      		  }
        	 }
      		  
            if (dataProperty != null)
            {
              String propertyName = dataProperty.getPropertyName();
              Values valuesStr = expression.getValues();
              String valueStr = valuesStr.getItems().get(0);
              Object value = convertValue(valueStr, dataProperty.getDataType());
              
              Criterion criterion = null;

              switch (expression.getRelationalOperator())
              {
                case EQUAL_TO:
                  criterion = Restrictions.eq(propertyName, value);
                  break;

                case LESSER_THAN:
                	criterion = Restrictions.lt(propertyName, value);
                  break;

                case LESSER_THAN_OR_EQUAL:
                  criterion = Restrictions.le(propertyName, value);
                  break;

                case GREATER_THAN:
                  criterion = Restrictions.gt(propertyName, value);
                  break;

                case GREATER_THAN_OR_EQUAL:
                  criterion = Restrictions.ge(propertyName, value);
                  break;

                case STARTS_WITH:
                  criterion = Restrictions.like(propertyName, value + wildcard);
                  break;

                case ENDS_WITH:
                  criterion = Restrictions.like(propertyName, wildcard + value);
                  break;

                case CONTAINS:
                  criterion = Restrictions.like(propertyName, wildcard + value + wildcard);
                  break;

                case IN:
                  if (dataProperty.getDataType() == DataType.STRING)
                  {
                    criterion = Restrictions.in(propertyName, valuesStr.getItems());
                  }
                  else
                  {
                    List<Object> values = new ArrayList<Object>();

                    for (String valStr : valuesStr.getItems())
                    {
                      values.add(convertValue(valStr, dataProperty.getDataType()));
                    }

                    criterion = Restrictions.in(propertyName, values);
                  }
                  break;
              }

              criterions.add(criterion);
            }
          }

          // connect criterions with logical operators
          //TODO: process grouping
        	  
          if (criterions.size() == 1)
          {
            criteria.add(criterions.get(0));
          }
          else if (criterions.size() > 1)
          {
            Criterion lhs = criterions.get(0);

            for (int i = 1; i < expressionList.size(); i++)
            {
              Expression expression = expressionList.get(i);

              if (expression.getLogicalOperator() != LogicalOperator.NONE)
              {
                Criterion rhs = criterions.get(i);

                switch (expression.getLogicalOperator())
                {
                  case AND:
                    lhs = Restrictions.and(lhs, rhs);
                    break;

                  case OR:
                    lhs = Restrictions.or(lhs, rhs);
                    break;
                }
              }
            }

            criteria.add(lhs);
          }
        }

        List<OrderExpression> orderExpressionList = dataFilter.getOrderExpressions().getItems();
        
        if (orderExpressionList != null)
        {
          List<DataProperty> dataProp = objectDefinition.getDataProperties();
          
          for (OrderExpression expression : orderExpressionList)
          {
            
        	  DataProperty dataProperty = null;

        	  for(DataProperty dataProps : dataProp){
        		  
        		  if(dataProps.getPropertyName().equalsIgnoreCase(expression.getPropertyName())){
        			  
        			  dataProperty = dataProps;
        			  
        			  break;
        		  }
        	  }

            String propertyName = dataProperty.getPropertyName();

            if (expression.getSortOrder() == SortOrder.ASC)
            {
              criteria.addOrder(Order.asc(propertyName));
            }

            if (expression.getSortOrder() == SortOrder.DESC)
            {
              criteria.addOrder(Order.desc(propertyName));
            }
          }
        }
      }
    }

    private static boolean isNumeric(DataType dataType)
    {
      return
        dataType == DataType.INT_32 ||
        dataType == DataType.DOUBLE ||
        dataType == DataType.INT_16 ||
        dataType == DataType.INT_64 ||
        dataType == DataType.SINGLE ||
        dataType == DataType.BYTE ||
        dataType == DataType.DECIMAL;
    }

    private static Object convertValue(String value, DataType dataType)
	{
	  switch (dataType)
	  {
		case STRING:
		  return String.valueOf(value);
		case INT_32:
		  return Integer.parseInt(value);
		case DOUBLE:
		  return Double.parseDouble(value);
		case BOOLEAN:
		  return Boolean.parseBoolean(value);
		case INT_16:
		  return Short.parseShort(value);
		case INT_64:
		  return Long.parseLong(value);
		case DECIMAL:
		  return BigDecimal.valueOf(Float.parseFloat(value));
		case SINGLE:
		  return Float.parseFloat(value);
		case DATE_TIME:
		  return Date.valueOf(value);
		case BYTE:
		  return Byte.parseByte(value);
		default:
		  throw new RuntimeException("Data type [" + dataType + "] not supported.");
	  }
	}
}
