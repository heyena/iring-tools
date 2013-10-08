using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using LINQ = System.Linq.Expressions;
using System.Collections;

namespace iRINGTools.Data
{
  public class DataFilter
  {
    private IList<DataExpression> _filterBuffer = null;
    private DictionaryObject _dataObjectDefinition = null;

    public DataFilter()
    {
      Expressions = new List<DataExpression>();
      OrderExpressions = new List<OrderExpression>();
    }

    public List<DataExpression> Expressions { get; set; }

    public List<OrderExpression> OrderExpressions { get; set; }

    public string ToSqlWhereClause(Dictionary dictionary, string objectType, string objectAlias)
    {
      if (!String.IsNullOrEmpty(objectAlias)) objectAlias += ".";
      else objectAlias = String.Empty;

      try
      {
        StringBuilder whereClause = new StringBuilder();
        if (Expressions != null && Expressions.Count > 0)
        {
          whereClause.Append(" WHERE ");

          foreach (DataExpression expression in this.Expressions)
          {
            string sqlExpression = ResolveSqlExpression(dictionary, objectType, expression, objectAlias);
            whereClause.Append(sqlExpression);
          }
        }

        if (OrderExpressions != null && OrderExpressions.Count > 0)
        {
          whereClause.Append(" ORDER BY ");

          foreach (OrderExpression orderExpression in this.OrderExpressions)
          {
            string orderStatement = ResolveOrderExpression(orderExpression, objectAlias);
            whereClause.Append(orderStatement);
          }
        }

        return whereClause.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error while geerating SQLWhereClause.", ex);
      }
    }

    public LINQ.Expression<Func<IDataObject, bool>> ToPredicate(DictionaryObject dataObjectDefinition)
    {
      _dataObjectDefinition = dataObjectDefinition;

      return ToPredicate(0);
    }

    public LINQ.Expression<Func<IDataObject, bool>> ToPredicate(int groupLevel)
    {
      LINQ.Expression<Func<IDataObject, bool>> predicate = null;

      try
      {
        if (Expressions != null && Expressions.Count > 0)
        {
          if (_filterBuffer == null)
            _filterBuffer = Expressions;

          List<DataExpression> localBuffer = _filterBuffer.ToList();
          foreach (DataExpression expression in localBuffer)
          {
            _filterBuffer.Remove(expression);

            groupLevel += (expression.OpenGroupCount - expression.CloseGroupCount);

            switch (expression.LogicalOperator)
            {
              case LogicalOperator.And:
              case LogicalOperator.None:
                if (predicate == null)
                  predicate = PredicateBuilder.True<IDataObject>();
                predicate = predicate.And(ResolvePredicate(expression));
                break;

              case LogicalOperator.AndNot:
              case LogicalOperator.Not:
                if (predicate == null)
                  predicate = PredicateBuilder.True<IDataObject>();
                predicate = predicate.And(ResolvePredicate(expression));
                predicate = LINQ.Expression.Lambda<Func<IDataObject, bool>>(LINQ.Expression.Not(predicate.Body), predicate.Parameters[0]);
                break;

              case LogicalOperator.Or:
                if (predicate == null)
                  predicate = PredicateBuilder.False<IDataObject>();
                predicate = predicate.Or(ResolvePredicate(expression));
                break;

              case LogicalOperator.OrNot:
                if (predicate == null)
                  predicate = PredicateBuilder.False<IDataObject>();
                predicate = predicate.Or(ResolvePredicate(expression));
                predicate = LINQ.Expression.Lambda<Func<IDataObject, bool>>(LINQ.Expression.Not(predicate.Body), predicate.Parameters[0]);
                break;
            }

            if (groupLevel > 0)
            {
              predicate = predicate.And(ToPredicate(groupLevel));
            }
          }
        }

        if (predicate == null)
          predicate = PredicateBuilder.True<IDataObject>();

        return predicate;
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating Predicate.", ex);
      }
    }

    public string ToLinqExpression<T>(string objectVariable)
    {
      if (this == null || this.Expressions.Count == 0)
        return String.Empty;

      if (!String.IsNullOrEmpty(objectVariable)) objectVariable += ".";
      else throw new Exception("Object variable can not be null or empty.");

      try
      {
        StringBuilder linqExpression = new StringBuilder();

        foreach (DataExpression expression in this.Expressions)
        {
          string exp = ResolveLinqExpression<T>(expression, objectVariable);
          linqExpression.Append(exp);
        }

        return linqExpression.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating LINQ expression.", ex);
      }
    }

    private string ResolveSqlExpression(Dictionary dictionary, string objectType, DataExpression expression, string objectAlias)
    {
      string PropertyName = expression.PropertyName;

      string qualifiedPropertyName = String.Empty;
      if (expression.IsCaseSensitive)
      {
        qualifiedPropertyName = objectAlias + PropertyName;
      }
      else
      {
        qualifiedPropertyName = "UPPER(" + objectAlias + PropertyName + ")";
      }

      //TODO: Is it better to default to string or non-string?
      DataType propertyType = DataType.String;
      foreach (DictionaryObject dataObject in dictionary.DictionaryObjects)
      {
        if (dataObject.ObjectName.ToUpper() == objectType.ToUpper())
        {
          foreach (DataProperty dataProperty in dataObject.DataProperties)
          {
            if (dataProperty.PropertyName.ToUpper() == PropertyName.ToUpper())
            {
              propertyType = dataProperty.DataType;
              break;
            }
          }
        }
      }

      bool isString = propertyType == DataType.String;
      StringBuilder sqlExpression = new StringBuilder();

      if (expression.LogicalOperator != LogicalOperator.None)
      {
        string logicalOperator = ResolveLogicalOperator(expression.LogicalOperator);
        sqlExpression.Append(" " + logicalOperator + " ");
      }

      for (int i = 0; i < expression.OpenGroupCount; i++)
        sqlExpression.Append("(");

      string value = String.Empty;
      switch (expression.RelationalOperator)
      {
        case RelationalOperator.StartsWith:
          if (!isString) throw new Exception("StartsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            value = expression.Values.FirstOrDefault();
          }
          else
          {
            value = expression.Values.FirstOrDefault().ToUpper();
          }

          sqlExpression.Append(qualifiedPropertyName + " LIKE '" + value + "%'");

          break;

        case RelationalOperator.Contains:
          if (!isString) throw new Exception("Contains operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            value = expression.Values.FirstOrDefault();
          }
          else
          {
            value = expression.Values.FirstOrDefault().ToUpper();
          }

          sqlExpression.Append(qualifiedPropertyName + " LIKE '%" + value + "%'");

          break;

        case RelationalOperator.EndsWith:
          if (!isString) throw new Exception("EndsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            value = expression.Values.FirstOrDefault();
          }
          else
          {
            value = expression.Values.FirstOrDefault().ToUpper();
          }

          sqlExpression.Append(qualifiedPropertyName + " LIKE '%" + value + "'");
          break;

        case RelationalOperator.In:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = String.Join("','", expression.Values.ToArray());
            }
            else
            {
              value = String.Join("','", expression.Values.ToArray()).ToUpper();
            }

            sqlExpression.Append(qualifiedPropertyName + " IN ('" + value + "')");
          }
          else
          {
            sqlExpression.Append(qualifiedPropertyName + " IN (" + String.Join(",", expression.Values.ToArray()) + ")");
          }
          break;

        case RelationalOperator.EqualTo:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualifiedPropertyName + "='" + value + "'");
          }
          else
          {
            sqlExpression.Append(qualifiedPropertyName + "=" + expression.Values.FirstOrDefault() + "");
          }
          break;

        case RelationalOperator.NotEqualTo:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualifiedPropertyName + "<>'" + value + "'");
          }
          else
          {
            sqlExpression.Append(qualifiedPropertyName + "<>" + expression.Values.FirstOrDefault() + "");
          }
          break;

        case RelationalOperator.GreaterThan:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualifiedPropertyName + ">'" + value + "'");
          }
          else
          {
            sqlExpression.Append(qualifiedPropertyName + ">" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.GreaterThanOrEqual:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualifiedPropertyName + ">='" + value + "'");
          }
          else
          {
            sqlExpression.Append(qualifiedPropertyName + ">=" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.LesserThan:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualifiedPropertyName + "<'" + value + "'");
          }
          else
          {
            sqlExpression.Append(qualifiedPropertyName + "<" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.LesserThanOrEqual:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualifiedPropertyName + "<='" + value + "'");
          }
          else
          {
            sqlExpression.Append(qualifiedPropertyName + "<=" + expression.Values.FirstOrDefault());
          }
          break;

        default:
          throw new Exception("Relational operator does not exist.");
      }

      for (int i = 0; i < expression.CloseGroupCount; i++)
        sqlExpression.Append(")");

      return sqlExpression.ToString();
    }

    private string ResolveOrderExpression(OrderExpression orderExpression, string objectAlias)
    {
      string PropertyName = orderExpression.PropertyName;

      string qualifiedPropertyName = objectAlias + PropertyName;

      StringBuilder sqlExpression = new StringBuilder();

      switch (orderExpression.SortOrder)
      {
        case SortOrder.Asc:
          sqlExpression.Append(qualifiedPropertyName + " ASC");
          break;

        case SortOrder.Desc:
          sqlExpression.Append(qualifiedPropertyName + " DESC");
          break;

        default:
          throw new Exception("Sort order is not specified.");
      }

      return sqlExpression.ToString();
    }

    private string ResolveLinqExpression<T>(DataExpression expression, string objectVariable)
    {
      string PropertyName = expression.PropertyName;
      string qualifiedPropertyName = objectVariable + PropertyName;
      Type propertyType = typeof(T).GetProperty(PropertyName).PropertyType;
      bool isString = (propertyType == typeof(string));
      StringBuilder linqExpression = new StringBuilder();

      if (expression.LogicalOperator != LogicalOperator.None)
      {
        string logicalOperator = ResolveLogicalOperator(expression.LogicalOperator);
        linqExpression.Append(" " + logicalOperator + " ");
      }

      for (int i = 0; i < expression.OpenGroupCount; i++)
        linqExpression.Append("(");

      switch (expression.RelationalOperator)
      {
        case RelationalOperator.StartsWith:
          if (!isString) throw new Exception("StartsWith operator used with non-string property");
          linqExpression.Append(qualifiedPropertyName + ".StartsWith(\"" + expression.Values.FirstOrDefault() + "\")");
          break;

        case RelationalOperator.Contains:
          if (!isString) throw new Exception("Contains operator used with non-string property");
          linqExpression.Append(qualifiedPropertyName + ".Contains(\"" + expression.Values.FirstOrDefault() + "\")");
          break;

        case RelationalOperator.EndsWith:
          if (!isString) throw new Exception("EndsWith operator used with non-string property");
          linqExpression.Append(qualifiedPropertyName + ".EndsWith(\"" + expression.Values.FirstOrDefault() + "\")");
          break;

        case RelationalOperator.In:
          if (isString)
            linqExpression.Append("(" + qualifiedPropertyName + "=\"" + String.Join("\" OR " + qualifiedPropertyName + "=\"", expression.Values.ToArray()) + "\")");
          else
            linqExpression.Append("(" + qualifiedPropertyName + "=" + String.Join(" OR " + qualifiedPropertyName + "=", expression.Values.ToArray()) + ")");
          break;

        case RelationalOperator.EqualTo:
          if (isString)
            linqExpression.Append(qualifiedPropertyName + "=\"" + expression.Values.FirstOrDefault() + "\"");
          else
            linqExpression.Append(qualifiedPropertyName + "=" + expression.Values.FirstOrDefault() + "");
          break;

        case RelationalOperator.NotEqualTo:
          if (isString)
            linqExpression.Append(qualifiedPropertyName + "<>\"" + expression.Values.FirstOrDefault() + "\"");
          else
            linqExpression.Append(qualifiedPropertyName + "<>" + expression.Values.FirstOrDefault() + "");
          break;

        case RelationalOperator.GreaterThan:
          if (isString)
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")>0");
          else
            linqExpression.Append(qualifiedPropertyName + ">" + expression.Values.FirstOrDefault());
          break;

        case RelationalOperator.GreaterThanOrEqual:
          if (isString)
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")>=0");
          else
            linqExpression.Append(qualifiedPropertyName + ">=" + expression.Values.FirstOrDefault());
          break;

        case RelationalOperator.LesserThan:
          if (isString)
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")<0");
          else
            linqExpression.Append(qualifiedPropertyName + "<" + expression.Values.FirstOrDefault());
          break;

        case RelationalOperator.LesserThanOrEqual:
          if (isString)
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")<=0");
          else
            linqExpression.Append(qualifiedPropertyName + "<=" + expression.Values.FirstOrDefault());
          break;

        default:
          throw new Exception("Relational operator does not exist.");
      }

      for (int i = 0; i < expression.CloseGroupCount; i++)
        linqExpression.Append(")");

      return linqExpression.ToString();
    }

    private string ResolveLogicalOperator(LogicalOperator logicalOperator)
    {
      switch (logicalOperator)
      {
        case LogicalOperator.And:
          return "AND";

        case LogicalOperator.AndNot:
          return "AND NOT";

        case LogicalOperator.Not:
          return "NOT";

        case LogicalOperator.Or:
          return "OR";

        case LogicalOperator.OrNot:
          return "OR NOT";

        default:
          throw new Exception("Logical operator [" + logicalOperator + "] not supported.");
      }
    }

    private LINQ.Expression<Func<IDataObject, bool>> ResolvePredicate(DataExpression expression)
    {
      string propertyName = expression.PropertyName;

      if (_dataObjectDefinition == null)
        throw new Exception("");

      DataProperty dataProperty = _dataObjectDefinition.DataProperties
        .Where(o => o.PropertyName.ToUpper() == propertyName.ToUpper())
        .SingleOrDefault();


      if (dataProperty == null)
        throw new Exception("");

      DataType propertyType = dataProperty.DataType;

      bool isString = propertyType == DataType.String;
      bool isBoolean = propertyType == DataType.Boolean;

      switch (expression.RelationalOperator)
      {
        case RelationalOperator.StartsWith:
          if (!isString) throw new Exception("StartsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().StartsWith(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().ToUpper().StartsWith(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.Contains:
          if (!isString) throw new Exception("Contains operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().Contains(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().ToUpper().Contains(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.EndsWith:
          if (!isString) throw new Exception("EndsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().EndsWith(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().ToUpper().EndsWith(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.In:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.PropertyName).ToString());
          }
          else
          {
            return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.PropertyName).ToString(), new GenericDataComparer(propertyType));
          }

        case RelationalOperator.EqualTo:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().Equals(expression.Values.FirstOrDefault());
          }
          else
          {
            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Equals(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault());
          }

        case RelationalOperator.NotEqualTo:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => !o.GetPropertyValue(dataProperty.PropertyName).ToString().Equals(expression.Values.FirstOrDefault());
          }
          else
          {
            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => !comparer.Equals(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault());
          }

        case RelationalOperator.GreaterThan:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault()) == 1;
          }

        case RelationalOperator.GreaterThanOrEqual:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1 ||
                        o.GetPropertyValue(dataProperty.PropertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault()) == 1 ||
                        comparer.Compare(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
          }

        case RelationalOperator.LesserThan:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1;
          }
          else
          {
            if (!isBoolean) throw new Exception("LesserThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault()) == -1;
          }

        case RelationalOperator.LesserThanOrEqual:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.PropertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1 ||
                        o.GetPropertyValue(dataProperty.PropertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault()) == -1 ||
                        comparer.Compare(o.GetPropertyValue(dataProperty.PropertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
          }

        default:
          throw new Exception("Relational operator does not exist.");
      }
    }
  }
}
