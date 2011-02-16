// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "dataFilter")]
  public class DataFilter
  {
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

    //public string ToLinqOrderBy()
    //{
    //  if (this == null || this.OrderExpressions.Count == 0)
    //    return String.Empty;

    //  try
    //  {
    //    StringBuilder linqOrderBy = new StringBuilder();

    //    foreach (OrderExpression orderExpression in this.OrderExpressions)
    //    {
    //      if (linqOrderBy.Length != 0)
    //        linqOrderBy.Append(", ");

    //      string orderBy = ResolveLinqOrderExpression(orderExpression);
    //      linqOrderBy.Append(orderBy);
    //    }

    //    return linqOrderBy.ToString();
    //  }
    //  catch (Exception ex)
    //  {
    //    throw new Exception("Error while generating LINQ orderBy.", ex);
    //  }
    //}

    public string ToLinqExpression(Type type, string objectVariable)
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
          string exp = ResolveLinqExpression(type, expression, objectVariable);
          linqExpression.Append(exp);
        }

        return linqExpression.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating LINQ expression.", ex);
      }
    }

    public string ToLinqExpression<T>(string objectVariable)
    {
      return ToLinqExpression(typeof(T), objectVariable);
    }

    public string ToLinqExpression(string objectType, string objectVariable)
    {
      return ToLinqExpression(Type.GetType(objectType), objectVariable);
    }

    private string ResolveSqlExpression(Type type, Expression expression, string objectAlias)
    {
      string propertyName = expression.PropertyName;

      string qualifiedPropertyName = String.Empty;
      if (expression.IsCaseSensitive)
      {
        qualifiedPropertyName = objectAlias + propertyName;
      }
      else
      {
        qualifiedPropertyName = "UPPER(" + objectAlias + propertyName + ")";
      }

      Type propertyType = type.GetProperty(propertyName).PropertyType;
      bool isString = propertyType == typeof(string);
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

    private string ResolveSqlExpression(DataDictionary dataDictionary, string objectType, Expression expression, string objectAlias)
    {
      string propertyName = expression.PropertyName;

      string qualifiedPropertyName = String.Empty;
      if (expression.IsCaseSensitive)
      {
        qualifiedPropertyName = objectAlias + propertyName;
      }
      else
      {
        qualifiedPropertyName = "UPPER(" + objectAlias + propertyName + ")";
      }

      //TODO: Is it better to default to string or non-string?
      DataType propertyType = DataType.String;
      foreach (DataObject dataObject in dataDictionary.dataObjects)
      {
        if (dataObject.ObjectName.ToUpper() == objectType.ToUpper())
        {
          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {
            if (dataProperty.PropertyName.ToUpper() == PropertyName.ToUpper())
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
      string propertyName = orderExpression.PropertyName;

      string  qualifiedPropertyName = objectAlias + propertyName;

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

    //private string ResolveLinqOrderExpression(OrderExpression orderExpression)
    //{
    //  string propertyName = orderExpression.PropertyName;

    //  string qualifiedPropertyName = propertyName;

    //  StringBuilder linqOrderBy = new StringBuilder();

    //  switch (orderExpression.SortOrder)
    //  {
    //    case SortOrder.Asc:
    //      linqOrderBy.Append(qualifiedPropertyName);
    //      break;

    //    case SortOrder.Desc:
    //      linqOrderBy.Append(qualifiedPropertyName + " DESC");
    //      break;

    //    default:
    //      throw new Exception("Sort order is not specified.");
    //  }

    //  return linqOrderBy.ToString();
    //}

    private string ResolveLinqExpression(Type type, Expression expression, string objectVariable)
    {
      string propertyName = expression.PropertyName;
      string qualifiedPropertyName = objectVariable + propertyName;
      Type propertyType = type.GetProperty(propertyName).PropertyType;
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
