using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using org.iringtools.library;
using org.iringtools.utility;

namespace org.iringtools.nhibernate
{
  public static class NHibernateUtility
  {
    public static DatabaseDictionary LoadDatabaseDictionary(string path)
    {
      return LoadDatabaseDictionary(path, string.Empty);
    }

    public static DatabaseDictionary LoadDatabaseDictionary(string path, string keyFile)
    {      
      DatabaseDictionary dbDictionary = Utility.Read<DatabaseDictionary>(path);
      string connStr = dbDictionary.ConnectionString;

      if (connStr != null)
      {
        if (!Utility.IsBase64Encoded(connStr))
        {
          //
          // connection string is not encrypted, encrypt and write it back
          //
          dbDictionary.ConnectionString = EncryptionUtility.Encrypt(connStr, keyFile);
          Utility.Write<DatabaseDictionary>(dbDictionary, path);

          dbDictionary.ConnectionString = connStr;
        }
        else
        {
          dbDictionary.ConnectionString = EncryptionUtility.Decrypt(connStr, keyFile);
        }
      }

      return dbDictionary;      
    }

    public static void SaveDatabaseDictionary(DatabaseDictionary dbDictionary, string path)
    {
      SaveDatabaseDictionary(dbDictionary, path, string.Empty);
    }

    public static void SaveDatabaseDictionary(DatabaseDictionary dbDictionary, string path, string keyFile)
    {
      string connStr = dbDictionary.ConnectionString;

      if (connStr != null)
      {
        if (Utility.IsBase64Encoded(connStr))
        {
          // connection string is not encrypted, encrypt it
          dbDictionary.ConnectionString = EncryptionUtility.Encrypt(connStr, keyFile);
        }
      }
      
      Utility.Write<DatabaseDictionary>(dbDictionary, path);
    }

    public static ICriteria CreateCriteria(ISession session, Type objectType, DataObject objectDefinition, DataFilter dataFilter)
    {      
      ICriteria criteria = session.CreateCriteria(objectType);
      AddCriteriaExpressions(criteria, objectDefinition, dataFilter);

      return criteria;
    }

    public static ICriteria CreateCriteria(ISession session, Type objectType, DataObject objectDefinition, DataFilter dataFilter, int start, int limit)
    {
      ICriteria criteria = CreateCriteria(session, objectType, objectDefinition, dataFilter);

      if (start >= 0 && limit > 0)
      {
        criteria.SetFirstResult(start).SetMaxResults(limit);
      }

      return criteria;
    }

    public static void AddCriteriaExpressions(ICriteria criteria, DataObject objectDefinition, DataFilter dataFilter)
    {
      if (dataFilter != null)
      {
        if (dataFilter.Expressions != null)
        {
          List<NHibernate.Criterion.ICriterion> criterions = new List<NHibernate.Criterion.ICriterion>();
          string wildcard = "%";

          foreach (Expression expression in dataFilter.Expressions)
          {
            DataProperty dataProperty = objectDefinition.dataProperties.Find(x => x.propertyName.ToUpper() == expression.PropertyName.ToUpper());
            
            if (dataProperty != null)
            {
              string propertyName = dataProperty.propertyName;
              Values valuesStr = expression.Values;
              string valueStr = valuesStr.First();
              object value = ConvertValue(valueStr, dataProperty.dataType);
              
              NHibernate.Criterion.ICriterion criterion = null;

              switch (expression.RelationalOperator)
              {
                case RelationalOperator.EqualTo:
                  criterion = NHibernate.Criterion.Expression.Eq(propertyName, value);
                  break;

                //case RelationalOperator.NotEqualTo:
                //  criterion = NHibernate.Criterion.Expression.NotEqProperty(propertyName, value);
                //  break;

                case RelationalOperator.LesserThan:
                  criterion = NHibernate.Criterion.Expression.Lt(propertyName, value);
                  break;

                case RelationalOperator.LesserThanOrEqual:
                  criterion = NHibernate.Criterion.Expression.Le(propertyName, value);
                  break;

                case RelationalOperator.GreaterThan:
                  criterion = NHibernate.Criterion.Expression.Gt(propertyName, value);
                  break;

                case RelationalOperator.GreaterThanOrEqual:
                  criterion = NHibernate.Criterion.Expression.Ge(propertyName, value);
                  break;

                case RelationalOperator.StartsWith:
                  criterion = NHibernate.Criterion.Expression.Like(propertyName, value + wildcard);
                  break;

                case RelationalOperator.EndsWith:
                  criterion = NHibernate.Criterion.Expression.Like(propertyName, wildcard + value);
                  break;

                case RelationalOperator.Contains:
                  criterion = NHibernate.Criterion.Expression.Like(propertyName, wildcard + value + wildcard);
                  break;
//337
                  case RelationalOperator.IsNull:
                  criterion = NHibernate.Criterion.Expression.IsNull(propertyName);
                  break;
                  case RelationalOperator.IsNotNull:
                  criterion = NHibernate.Criterion.Expression.IsNotNull(propertyName);
                  break;
//337


                case RelationalOperator.In:
                  if (dataProperty.dataType == DataType.String)
                  {
                    criterion = NHibernate.Criterion.Expression.In(propertyName, valuesStr);
                  }
                  else
                  {
                    List<object> values = new List<object>();

                    foreach (string valStr in valuesStr)
                    {
                      values.Add(ConvertValue(valStr, dataProperty.dataType));
                    }

                    criterion = NHibernate.Criterion.Expression.In(propertyName, values);
                  }
                  break;
              }


              criterions.Add(criterion);
            }
          }

          // connect criterions with logical operators
          //TODO: process grouping
          if (criterions.Count == 1)
          {
            criteria.Add(criterions.First());
          }
          else if (criterions.Count > 1)
          {
            NHibernate.Criterion.ICriterion lhs = criterions.First();

            for (int i = 1; i < dataFilter.Expressions.Count; i++)
            {
              Expression expression = dataFilter.Expressions[i];

              if (expression.LogicalOperator != LogicalOperator.None)
              {
                NHibernate.Criterion.ICriterion rhs = criterions[i];

                switch (expression.LogicalOperator)
                {
                  case LogicalOperator.And:
                    lhs = NHibernate.Criterion.Expression.And(lhs, rhs);
                    break;

                  case LogicalOperator.Or:
                    lhs = NHibernate.Criterion.Expression.Or(lhs, rhs);
                    break;

                  //case LogicalOperator.Not:
                  //  lhs = NHibernate.Criterion.Expression.Not(lhs);
                  //  break;

                  //case LogicalOperator.AndNot:
                  //  lhs = NHibernate.Criterion.Expression.And(lhs, rhs);
                  //  break;

                  //case LogicalOperator.OrNot:
                  //  lhs = NHibernate.Criterion.Expression.Or(lhs, rhs);
                  //  break;
                }
              }
            }

            criteria.Add(lhs);
          }
        }

        if (dataFilter.OrderExpressions != null)
        {
          foreach (OrderExpression expression in dataFilter.OrderExpressions)
          {
            DataProperty dataProperty = objectDefinition.dataProperties.Find(x => x.propertyName.ToUpper() == expression.PropertyName.ToUpper());
            string propertyName = dataProperty.propertyName;

            if (expression.SortOrder == SortOrder.Asc)
            {
              criteria.AddOrder(NHibernate.Criterion.Order.Asc(propertyName));
            }

            if (expression.SortOrder == SortOrder.Desc)
            {
              criteria.AddOrder(NHibernate.Criterion.Order.Desc(propertyName));
            }
          }
        }
      }
    }

    private static bool IsNumeric(DataType dataType)
    {
      return
        dataType == DataType.Int32 ||
        dataType == DataType.Double ||
        dataType == DataType.Int16 ||
        dataType == DataType.Int64 ||
        dataType == DataType.Single ||
        dataType == DataType.Byte ||
        dataType == DataType.Decimal;
    }

    static object ConvertValue(string value, DataType dataType)
    {
      switch (dataType)
      {
        case DataType.String:
          return Convert.ToString(value);
        case DataType.Int32:
          return Convert.ToInt32(value);
        case DataType.Double:
          return Convert.ToDouble(value);
        case DataType.Boolean:
          return Convert.ToBoolean(value);
        case DataType.Int16:
          return Convert.ToInt16(value);
        case DataType.Int64:
          return Convert.ToInt64(value);
        case DataType.Decimal:
          return Convert.ToDecimal(value);
        case DataType.Single:
          return Convert.ToSingle(value);
        case DataType.DateTime:
          return Convert.ToDateTime(value);
        case DataType.Byte:
          return Convert.ToByte(value);
        default:
          throw new Exception("Data type [" + dataType + "] not supported.");
      }
    }
  }
}
