﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using LINQ = System.Linq.Expressions;
using System.Collections;
using org.iringtools.utility;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "dataFilter")]
  public class DataFilter
  {
    private List<Expression> _filterBuffer = null;
    private DataObject _dataObjectDefinition = null;

    public DataFilter()
    {
      Expressions = new List<Expression>();
      OrderExpressions = new List<OrderExpression>();
    }

    [DataMember(Name = "expressions", Order = 0, EmitDefaultValue = false)]
    public List<Expression> Expressions { get; set; }

    [DataMember(Name = "orderExpressions", Order = 1, EmitDefaultValue = false)]
    public List<OrderExpression> OrderExpressions { get; set; }

    //public string ToSqlWhereClause(Type type, string objectAlias)
    //{
    //  if (this == null || this.Expressions.Count == 0)
    //    return String.Empty;

    //  if (!String.IsNullOrEmpty(objectAlias)) objectAlias += ".";
    //  else objectAlias = String.Empty;

    //  try
    //  {
    //    StringBuilder whereClause = new StringBuilder();
    //    whereClause.Append(" WHERE ");

    //    foreach (Expression expression in this.Expressions)
    //    {

    //      string sqlExpression = ResolveSqlExpression(type, expression, objectAlias);
    //      whereClause.Append(sqlExpression);
    //    }

    //    return whereClause.ToString();
    //  }
    //  catch (Exception ex)
    //  {
    //    throw new Exception("Error while generating SQLWhereClause.", ex);
    //  }
    //}

    //public string ToSqlWhereClause<T>(string objectAlias)
    //{
    //  return ToSqlWhereClause(typeof(T), objectAlias);
    //}

    //public string ToSqlWhereClause(string objectType, string objectAlias)
    //{
    //  return ToSqlWhereClause(Type.GetType(objectType), objectAlias);
    //}

    public string ToSqlWhereClause(DataDictionary dataDictionary, string objectType, string objectAlias)
    {
      if (!String.IsNullOrEmpty(objectAlias)) objectAlias += ".";
      else objectAlias = String.Empty;

      try
      {
        StringBuilder whereClause = new StringBuilder();
        if (Expressions != null && Expressions.Count > 0)
        {
          whereClause.Append(" WHERE ");

          foreach (Expression expression in this.Expressions)
          {
            string sqlExpression = ResolveSqlExpression(dataDictionary, objectType, expression, objectAlias);
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

    public LINQ.Expression<Func<IDataObject, bool>> ToPredicate(DataObject dataObjectDefinition)
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
            _filterBuffer = Expressions.ToList();

          List<Expression> localBuffer = _filterBuffer.ToList();
          foreach (Expression expression in localBuffer)
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

        foreach (Expression expression in this.Expressions)
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

    //public string ToLinqExpression(string objectType, string objectVariable)
    //{
    //  return ToLinqExpression(Type.GetType(objectType), objectVariable);
    //}

    //private string ResolveSqlExpression(Type type, Expression expression, string objectAlias)
    //{
    //  string PropertyName = expression.PropertyName;

    //  string qualifiedPropertyName = String.Empty;
    //  if (expression.IsCaseSensitive)
    //  {
    //    qualifiedPropertyName = objectAlias + PropertyName;
    //  }
    //  else
    //  {
    //    qualifiedPropertyName = "UPPER(" + objectAlias + PropertyName + ")";
    //  }

    //  Type propertyType = type.GetProperty(PropertyName).PropertyType;
    //  bool isString = propertyType == typeof(string);
    //  StringBuilder sqlExpression = new StringBuilder();

    //  if (expression.LogicalOperator != LogicalOperator.None)
    //  {
    //    string logicalOperator = ResolveLogicalOperator(expression.LogicalOperator);
    //    sqlExpression.Append(" " + logicalOperator + " ");
    //  }

    //  for (int i = 0; i < expression.OpenGroupCount; i++)
    //    sqlExpression.Append("(");

    //  string value = String.Empty;
    //  switch (expression.RelationalOperator)
    //  {
    //    case RelationalOperator.StartsWith:
    //      if (!isString) throw new Exception("StartsWith operator used with non-string property");

    //      if (expression.IsCaseSensitive)
    //      {
    //        value = expression.Values.FirstOrDefault();
    //      }
    //      else
    //      {
    //        value = expression.Values.FirstOrDefault().ToUpper();
    //      }

    //      sqlExpression.Append(qualifiedPropertyName + " LIKE '" + value + "%'");

    //      break;

    //    case RelationalOperator.Contains:
    //      if (!isString) throw new Exception("Contains operator used with non-string property");

    //      if (expression.IsCaseSensitive)
    //      {
    //        value = expression.Values.FirstOrDefault();
    //      }
    //      else
    //      {
    //        value = expression.Values.FirstOrDefault().ToUpper();
    //      }

    //      sqlExpression.Append(qualifiedPropertyName + " LIKE '%" + value + "%'");

    //      break;

    //    case RelationalOperator.EndsWith:
    //      if (!isString) throw new Exception("EndsWith operator used with non-string property");

    //      if (expression.IsCaseSensitive)
    //      {
    //        value = expression.Values.FirstOrDefault();
    //      }
    //      else
    //      {
    //        value = expression.Values.FirstOrDefault().ToUpper();
    //      }

    //      sqlExpression.Append(qualifiedPropertyName + " LIKE '%" + value + "'");
    //      break;

    //    case RelationalOperator.In:
    //      if (isString)
    //      {
    //        if (expression.IsCaseSensitive)
    //        {
    //          value = String.Join("','", expression.Values.ToArray());
    //        }
    //        else
    //        {
    //          value = String.Join("','", expression.Values.ToArray()).ToUpper();
    //        }

    //        sqlExpression.Append(qualifiedPropertyName + " IN ('" + value + "')");
    //      }
    //      else
    //      {
    //        sqlExpression.Append(qualifiedPropertyName + " IN (" + String.Join(",", expression.Values.ToArray()) + ")");
    //      }
    //      break;

    //    case RelationalOperator.EqualTo:
    //      if (isString)
    //      {
    //        if (expression.IsCaseSensitive)
    //        {
    //          value = expression.Values.FirstOrDefault();
    //        }
    //        else
    //        {
    //          value = expression.Values.FirstOrDefault().ToUpper();
    //        }

    //        sqlExpression.Append(qualifiedPropertyName + "='" + value + "'");
    //      }
    //      else
    //      {
    //        sqlExpression.Append(qualifiedPropertyName + "=" + expression.Values.FirstOrDefault() + "");
    //      }
    //      break;

    //    case RelationalOperator.NotEqualTo:
    //      if (isString)
    //      {
    //        if (expression.IsCaseSensitive)
    //        {
    //          value = expression.Values.FirstOrDefault();
    //        }
    //        else
    //        {
    //          value = expression.Values.FirstOrDefault().ToUpper();
    //        }

    //        sqlExpression.Append(qualifiedPropertyName + "<>'" + value + "'");
    //      }
    //      else
    //      {
    //        sqlExpression.Append(qualifiedPropertyName + "<>" + expression.Values.FirstOrDefault() + "");
    //      }
    //      break;

    //    case RelationalOperator.GreaterThan:
    //      if (isString)
    //      {
    //        if (expression.IsCaseSensitive)
    //        {
    //          value = expression.Values.FirstOrDefault();
    //        }
    //        else
    //        {
    //          value = expression.Values.FirstOrDefault().ToUpper();
    //        }

    //        sqlExpression.Append(qualifiedPropertyName + ">'" + value + "'");
    //      }
    //      else
    //      {
    //        sqlExpression.Append(qualifiedPropertyName + ">" + expression.Values.FirstOrDefault());
    //      }
    //      break;

    //    case RelationalOperator.GreaterThanOrEqual:
    //      if (isString)
    //      {
    //        if (expression.IsCaseSensitive)
    //        {
    //          value = expression.Values.FirstOrDefault();
    //        }
    //        else
    //        {
    //          value = expression.Values.FirstOrDefault().ToUpper();
    //        }

    //        sqlExpression.Append(qualifiedPropertyName + ">='" + value + "'");
    //      }
    //      else
    //      {
    //        sqlExpression.Append(qualifiedPropertyName + ">=" + expression.Values.FirstOrDefault());
    //      }
    //      break;

    //    case RelationalOperator.LesserThan:
    //      if (isString)
    //      {
    //        if (expression.IsCaseSensitive)
    //        {
    //          value = expression.Values.FirstOrDefault();
    //        }
    //        else
    //        {
    //          value = expression.Values.FirstOrDefault().ToUpper();
    //        }

    //        sqlExpression.Append(qualifiedPropertyName + "<'" + value + "'");
    //      }
    //      else
    //      {
    //        sqlExpression.Append(qualifiedPropertyName + "<" + expression.Values.FirstOrDefault());
    //      }
    //      break;

    //    case RelationalOperator.LesserThanOrEqual:
    //      if (isString)
    //      {
    //        if (expression.IsCaseSensitive)
    //        {
    //          value = expression.Values.FirstOrDefault();
    //        }
    //        else
    //        {
    //          value = expression.Values.FirstOrDefault().ToUpper();
    //        }

    //        sqlExpression.Append(qualifiedPropertyName + "<='" + value + "'");
    //      }
    //      else
    //      {
    //        sqlExpression.Append(qualifiedPropertyName + "<=" + expression.Values.FirstOrDefault());
    //      }
    //      break;

    //    default:
    //      throw new Exception("Relational operator does not exist.");
    //  }

    //  for (int i = 0; i < expression.CloseGroupCount; i++)
    //    sqlExpression.Append(")");

    //  return sqlExpression.ToString();
    //}

    private string ResolveSqlExpression(DataDictionary dataDictionary, string objectType, Expression expression, string objectAlias)
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
      foreach (DataObject dataObject in dataDictionary.dataObjects)
      {
        if (dataObject.objectName.ToUpper() == objectType.ToUpper())
        {
          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {
            if (dataProperty.propertyName.ToUpper() == PropertyName.ToUpper())
            {
              propertyType = dataProperty.dataType;
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

      string  qualifiedPropertyName = objectAlias + PropertyName;

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

    private string ResolveLinqExpression<T>(Expression expression, string objectVariable)
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

    private LINQ.Expression<Func<IDataObject, bool>> ResolvePredicate(Expression expression)
    {
      string propertyName = expression.PropertyName;

      if (_dataObjectDefinition == null)
        throw new Exception("");

#if !SILVERLIGHT
      DataProperty dataProperty =
        _dataObjectDefinition.dataProperties.Find(
          o => o.propertyName.ToUpper() == propertyName.ToUpper()
        );
#else
      DataProperty dataProperty = null;
      foreach (DataProperty o in _dataObjectDefinition.dataProperties)
      {
        if (o.propertyName.ToUpper() == propertyName.ToUpper())
        {
          dataProperty = o;
          break;
        }
      }
#endif
      

      if (dataProperty == null)
        throw new Exception("");

      DataType propertyType = dataProperty.dataType;

      bool isString = propertyType == DataType.String;
      bool isBoolean = propertyType == DataType.Boolean;

      switch (expression.RelationalOperator)
      {
        case RelationalOperator.StartsWith:
          if (!isString) throw new Exception("StartsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().StartsWith(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().StartsWith(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.Contains:
          if (!isString) throw new Exception("Contains operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().Contains(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().Contains(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.EndsWith:
          if (!isString) throw new Exception("EndsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().EndsWith(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().EndsWith(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.In:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.propertyName).ToString());
          }
          else
          {
            return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.propertyName).ToString(), new GenericDataComparer(propertyType));
          }

        case RelationalOperator.EqualTo:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
          }
          else
          {
            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
          }

        case RelationalOperator.NotEqualTo:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => !o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
          }
          else
          {
            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => !comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
          }

        case RelationalOperator.GreaterThan:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1;
          }

        case RelationalOperator.GreaterThanOrEqual:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1 ||
                        o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1 ||
                        comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
          }

        case RelationalOperator.LesserThan:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1;
          }
          else
          {
            if (!isBoolean) throw new Exception("LesserThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1;
          }

        case RelationalOperator.LesserThanOrEqual:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1 ||
                        o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1 ||
                        comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
          }

        default:
          throw new Exception("Relational operator does not exist.");
      }
    }
  }

  public class GenericDataComparer : IEqualityComparer<string>, IComparer<string>
  {
    private DataType _dataType { get; set; }

    public GenericDataComparer(DataType dataType)
    {
      _dataType = dataType;
    }

    // Implement the IComparable interface. 
    public bool Equals(string str1, string str2)
    {
      return Compare(str1, str2) == 0;
    }

    public int Compare(string str1, string str2)
    {
      switch (_dataType)
      {
        case DataType.Boolean:
          bool bool1 = false;
          Boolean.TryParse(str1, out bool1);

          bool bool2 = false;
          Boolean.TryParse(str2, out bool1);

          if (Boolean.Equals(bool1, bool2))
          {
            return 0;
          }
          else if (bool1)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Byte:
          byte byte1 = 0;
          Byte.TryParse(str1, out byte1);

          byte byte2 = 0;
          Byte.TryParse(str2, out byte2);

          if (byte1 == byte2)
          {
            return 0;
          }
          else if (byte1 > byte2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Char:
          char char1 = Char.MinValue;
          Char.TryParse(str1, out char1);

          char char2 = Char.MinValue;
          Char.TryParse(str2, out char2);

          if (char1 == char2)
          {
            return 0;
          }
          else if (char1 > char2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.DateTime:
          DateTime dateTime1 = DateTime.MinValue;
          DateTime.TryParse(str1, out dateTime1);

          DateTime dateTime2 = DateTime.MinValue;
          DateTime.TryParse(str2, out dateTime2);

          return DateTime.Compare(dateTime1, dateTime2);

        case DataType.Decimal:
          decimal decimal1 = 0;
          Decimal.TryParse(str1, out decimal1);

          decimal decimal2 = 0;
          Decimal.TryParse(str2, out decimal2);

          return Decimal.Compare(decimal1, decimal2);

        case DataType.Double:
          double double1 = 0;
          Double.TryParse(str1, out double1);

          double double2 = 0;
          Double.TryParse(str2, out double2);

          if (Double.Equals(double1, double2))
          {
            return 0;
          }
          else if (double1 > double2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int16:
          Int16 int161 = 0;
          Int16.TryParse(str1, out int161);

          Int16 int162 = 0;
          Int16.TryParse(str2, out int162);

          if (Int16.Equals(int161, int162))
          {
            return 0;
          }
          else if (int161 > int162)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int32:
          int int1 = 0;
          Int32.TryParse(str1, out int1);

          int int2 = 0;
          Int32.TryParse(str2, out int2);

          if (Int32.Equals(int1, int2))
          {
            return 0;
          }
          else if (int1 > int2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int64:
          Int64 int641 = 0;
          Int64.TryParse(str1, out int641);

          Int64 int642 = 0;
          Int64.TryParse(str2, out int642);

          if (Int16.Equals(int641, int642))
          {
            return 0;
          }
          else if (int641 > int642)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Single:
          Single single1 = 0;
          Single.TryParse(str1, out single1);

          Single single2 = 0;
          Single.TryParse(str2, out single2);

          if (Single.Equals(single1, single2))
          {
            return 0;
          }
          else if (single1 > single2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        //Case Insensitive!
        case DataType.String:
#if !SILVERLIGHT
          return String.Compare(str1, str2, true);
#else
          return String.Compare(str1, str2, StringComparison.InvariantCultureIgnoreCase);
#endif

        case DataType.TimeSpan:
          TimeSpan span1 = TimeSpan.MinValue;
          TimeSpan.TryParse(str1, out span1);

          TimeSpan span2 = TimeSpan.MinValue;
          TimeSpan.TryParse(str2, out span2);

          return TimeSpan.Compare(span1, span2);

        default:
          throw new Exception("Invalid property datatype.");
      }
    }

    public int GetHashCode(string obj)
    {
      throw new NotImplementedException();
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "expression")]
  public class Expression
  {
    [DataMember(Name = "openGroupCount", Order = 0, EmitDefaultValue = false)]
    public int OpenGroupCount { get; set; }

    [DataMember(Name = "propertyName", Order = 1, IsRequired = true)]
    public string PropertyName { get; set; }

    [DataMember(Name = "relationalOperator", Order = 2, IsRequired = true)]
    public RelationalOperator RelationalOperator { get; set; }

    [DataMember(Name = "values", Order = 3, IsRequired = true)]
    public Values Values { get; set; }

    [DataMember(Name = "logicalOperator", Order = 4, EmitDefaultValue = false)]
    public LogicalOperator LogicalOperator { get; set; }

    [DataMember(Name = "closeGroupCount", Order = 5, EmitDefaultValue = false)]
    public int CloseGroupCount { get; set; }

    [DataMember(Name = "isCaseSensitive", Order = 6, EmitDefaultValue = false)]
    public bool IsCaseSensitive { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "values", ItemName = "value")]
  public class Values : List<string> { }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "orderExpression")]
  public class OrderExpression
  {
    [DataMember(Name = "propertyName", Order = 0, IsRequired = true)]
    public string PropertyName { get; set; }

    [DataMember(Name = "sortOrder", Order = 1, EmitDefaultValue = false)]
    public SortOrder SortOrder { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "logicalOperator")]
  public enum LogicalOperator
  {
    [EnumMember]
    None,
    [EnumMember]
    And,
    [EnumMember]
    Or,
    [EnumMember]
    Not,
    [EnumMember]
    AndNot,
    [EnumMember]
    OrNot,
  };

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "relationalOperator")]
  public enum RelationalOperator
  {
    [EnumMember]
    EqualTo,
    [EnumMember]
    NotEqualTo,
    [EnumMember]
    StartsWith,
    [EnumMember]
    EndsWith,
    [EnumMember]
    Contains,
    [EnumMember]
    In,
    [EnumMember]
    GreaterThan,
    [EnumMember]
    GreaterThanOrEqual,
    [EnumMember]
    LesserThan,
    [EnumMember]
    LesserThanOrEqual,
  };

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "sortOrder")]
  public enum SortOrder
  {
    [EnumMember]
    Asc,
    [EnumMember]
    Desc,
  };
}
