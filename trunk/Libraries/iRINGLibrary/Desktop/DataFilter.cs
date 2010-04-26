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
  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class DataFilter
  {
    [DataMember]
    public List<Expression> Expressions { get; set; }

    private readonly List<RelationalOperator> LikeOperators = new List<RelationalOperator>
    {
      RelationalOperator.StartsWith, 
      RelationalOperator.Contains,
      RelationalOperator.EndsWith,
    };

    public string ToSqlWhereClause(string objectType, string objectAlias)
    {
      if (this == null || this.Expressions.Count == 0)
      {
        return String.Empty;
      }

      if (!String.IsNullOrEmpty(objectAlias)) objectAlias += ".";
      else objectAlias = String.Empty;

      try
      {
        StringBuilder whereClause = new StringBuilder();
        whereClause.Append(" WHERE ");

        foreach (Expression expression in this.Expressions)
        {
          if (expression.LogicalOperator != LogicalOperator.None)
          {
            whereClause.Append(" " + ResolveLogicalOperator(expression.LogicalOperator));
          }

          for (int i = 0; i < expression.OpenGroupCount; i++)
          {
            whereClause.Append("(");
          }

          string propertyName = expression.PropertyName;
          whereClause.Append(objectAlias + propertyName);

          string relationalOperator = ResolveSqlRelationalOperator(expression.RelationalOperator);
          whereClause.Append(relationalOperator);

          Type propertyType = Type.GetType(objectType).GetProperty(propertyName).PropertyType;
          bool isString = propertyType == typeof(string);

          if (expression.RelationalOperator == RelationalOperator.In)
          {
            whereClause.Append("(");

            foreach (string value in expression.Values)
            {
              if (whereClause.ToString() != "(")
              {
                whereClause.Append(",");
              }

              if (isString)
              {
                whereClause.Append("'" + value + "'");
              }
              else
              {
                whereClause.Append(value);
              }
            }

            whereClause.Append(")");
          }
          else
          {
            string value = String.Empty;

            if (LikeOperators.Contains(expression.RelationalOperator))
            {
              if (isString)
              {
                value = "\"(" + expression.Values.FirstOrDefault() + "\")";
              }
              else
              {
                throw new Exception("Error while generating SQLWhereClause. Like operator used with non-string property");
              }
            }
            else
            {
              if (isString)
              {
                value = "'" + expression.Values.FirstOrDefault() + "'";
              }
              else
              {
                value = expression.Values.FirstOrDefault();
              }
            }

            whereClause.Append(value);
          }

          for (int i = 0; i < expression.CloseGroupCount; i++)
          {
            whereClause.Append(")");
          }
        }

        return whereClause.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating SQLWhereClause.", ex);
      }
    }

    private string ResolveSqlRelationalOperator(RelationalOperator relationalOperator)
    {
      string sqlRelationalOperator = String.Empty;

      switch (relationalOperator)
      {
        case RelationalOperator.StartsWith:
          sqlRelationalOperator = "LIKE";
          break;

        case RelationalOperator.Contains:
          sqlRelationalOperator = "LIKE";
          break;

        case RelationalOperator.EndsWith:
          sqlRelationalOperator = "LIKE";
          break;

        case RelationalOperator.EqualTo:
          sqlRelationalOperator = "=";
          break;

        case RelationalOperator.GreaterThan:
          sqlRelationalOperator = ">";
          break;

        case RelationalOperator.GreaterThanOrEqual:
          sqlRelationalOperator = ">=";
          break;

        case RelationalOperator.In:
          sqlRelationalOperator = "IN";
          break;

        case RelationalOperator.LesserThan:
          sqlRelationalOperator = "<";
          break;

        case RelationalOperator.LesserThanOrEqual:
          sqlRelationalOperator = "<=";
          break;

        case RelationalOperator.NotEqualTo:
          sqlRelationalOperator = "<>";
          break;
      }

      return sqlRelationalOperator;
    }

    private string ResolveLogicalOperator(LogicalOperator logicalOperator)
    {
      string sqlLogicalOperator = String.Empty;

      switch (logicalOperator)
      {
        case LogicalOperator.And:
          sqlLogicalOperator = " AND ";
          break;

        case LogicalOperator.AndNot:
          sqlLogicalOperator = " AND NOT ";
          break;

        case LogicalOperator.Not:
          sqlLogicalOperator = " NOT ";
          break;

        case LogicalOperator.Or:
          sqlLogicalOperator = " OR ";
          break;

        case LogicalOperator.OrNot:
          sqlLogicalOperator = " OR NOT ";
          break;
      }

      return sqlLogicalOperator;
    }


    public string ToLinqExpression(string objectType, string objectVariable)
    {
      if (this == null || this.Expressions.Count == 0)
      {
        return String.Empty;
      }

      if (!String.IsNullOrEmpty(objectVariable)) objectVariable += ".";
      else throw new Exception("Variable can not be null or empty.");

      try
      {
        StringBuilder whereClause = new StringBuilder();

        foreach (Expression expression in this.Expressions)
        {
          whereClause.Append(ResolveLinqExpression(expression, objectType, objectVariable));
        }

        return whereClause.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating LINQ expression.", ex);
      }
    }

    private string ResolveLinqExpression(Expression expression, string objectType, string objectVariable)
    {
      string propertyName = expression.PropertyName;
      string qualifiedPropertyName = objectVariable + propertyName;
      Type propertyType = Type.GetType(objectType).GetProperty(propertyName).PropertyType;
      bool isString = (propertyType == typeof(string));
      StringBuilder linqExpression = new StringBuilder();

      if (expression.LogicalOperator != LogicalOperator.None)
      {
        linqExpression.Append(ResolveLogicalOperator(expression.LogicalOperator));
      }

      for (int i = 0; i < expression.OpenGroupCount; i++)
      {
        linqExpression.Append("(");
      }

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
          if (!isString) throw new Exception("In operator used with non-string property");
          string groupExpression = "(";
          foreach (string value in expression.Values)
          {
            if (groupExpression != "(") groupExpression += " OR ";
            groupExpression += qualifiedPropertyName + "=\"" + value + "\"";
          }
          linqExpression.Append(groupExpression + ")");
          break;

        case RelationalOperator.EqualTo:
          if (isString)
          {
            linqExpression.Append(qualifiedPropertyName + "=\"" + expression.Values.FirstOrDefault() + "\"");
          }
          else
          {
            linqExpression.Append(qualifiedPropertyName + "=" + expression.Values.FirstOrDefault() + "");
          }
          break;

        case RelationalOperator.NotEqualTo:
          if (isString)
          {
            linqExpression.Append(qualifiedPropertyName + "<>\"" + expression.Values.FirstOrDefault() + "\"");
          }
          else
          {
            linqExpression.Append(qualifiedPropertyName + "<>" + expression.Values.FirstOrDefault() + "");
          }
          break;

        case RelationalOperator.GreaterThan:
          if (isString)
          {
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")>0");
          }
          else
          {
            linqExpression.Append(qualifiedPropertyName + ">" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.GreaterThanOrEqual:
          if (isString)
          {
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")>=0");
          }
          else
          {
            linqExpression.Append(qualifiedPropertyName + ">=" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.LesserThan:
          if (isString)
          {
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")<0");
          }
          else
          {
            linqExpression.Append(qualifiedPropertyName + "<" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.LesserThanOrEqual:
          if (isString)
          {
            linqExpression.Append(qualifiedPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")<=0");
          }
          else
          {
            linqExpression.Append(qualifiedPropertyName + "<=" + expression.Values.FirstOrDefault());
          }
          break;

        default:
          throw new Exception("Relational operator does not exist.");
      }

      for (int i = 0; i < expression.CloseGroupCount; i++)
      {
        linqExpression.Append(")");
      }

      return linqExpression.ToString();
    }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class Expression
  {
    [DataMember(Order = 0, EmitDefaultValue = false)]
    public int OpenGroupCount { get; set; }

    [DataMember(Order = 1)]
    public string PropertyName { get; set; }

    [DataMember(Order = 2)]
    public RelationalOperator RelationalOperator { get; set; }

    [DataMember(Order = 3)]
    public List<string> Values { get; set; }

    [DataMember(Order = 4, EmitDefaultValue = false)]
    public LogicalOperator LogicalOperator { get; set; }

    [DataMember(Order = 5, EmitDefaultValue = false)]
    public int CloseGroupCount { get; set; }

  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
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

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
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
}
