using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Data;

namespace org.iringtools.adapter.datalayer 
{
  public class MSSQLObject 
  {
    private DataSet _dataSet = new DataSet();
    private string _primaryObject = string.Empty;
    private string _primaryTable = string.Empty;
    private List<SecondaryObject> _secondaryObjects = null;
    private DataTable _dataTable = null;
    private StringBuilder sql = null;
    private string _sqlTableName = string.Empty;
    private List<string> _properties = new List<string>();
    private SqlObject _sqlObject;
    private IList<string> _identifiers = null;

    public MSSQLObject(SqlObject sqlObject, IList<string> identifiers)
    {
      _identifiers = identifiers;
      _sqlObject = sqlObject;
      _primaryObject = sqlObject.ObjectTypeName;
      _primaryTable = sqlObject.ObjectName;
      _secondaryObjects = sqlObject.SecondaryObjects;
      DataCRUD getData = new DataCRUD(sqlObject.ConnectionString);

      string columns = string.Format("{0}.*", _sqlObject.ObjectName);
      
      sql = new StringBuilder();
      
      if (!string.IsNullOrEmpty(_sqlObject.MinimumProperties))
      {
        columns = _sqlObject.MinimumProperties;
      }
      if (string.IsNullOrEmpty(sqlObject.SelectSqlJoin))
      {
        sql.AppendLine(string.Format("SELECT {0} FROM {1}", columns, sqlObject.ObjectName));
      }
      else
      {
        sql.AppendLine(string.Format("SELECT {0} FROM {1}", 
                                      columns,
                                      sqlObject.ObjectName));
        sql.AppendLine(sqlObject.SelectSqlJoin);
      }
      sql.AppendLine(" WHERE "+sqlObject.IdentifierProperty+ " in ('" + String.Join("','", identifiers.ToArray()) + "')");
      _dataTable = getData.SelectRecords(sql.ToString());
      _dataTable.TableName = sqlObject.ObjectName;
      AddDataTable(_dataTable);
     
      foreach (SecondaryObject so in _secondaryObjects)
      {
        if (!string.IsNullOrEmpty(so.MinimumProperties))
        {
          sql = new StringBuilder();
          _dataTable = new DataTable();
          if (!string.IsNullOrEmpty(so.SelectQuery))
          {
            sql.AppendLine(string.Format("SELECT {0} FROM {1}", 
                                          so.SelectQuery, 
                                          so.ObjectName));
          }
          else
          {
            sql.AppendLine(string.Format("SELECT * FROM {0}", 
                                          so.ObjectName));
          }

          if (sqlObject.IdentifierProperty.Contains(so.IdentifierProperty))
          {
            sql.AppendLine(" WHERE "+so.IdentifierProperty+ " in ('" + String.Join("','", identifiers.ToArray()) + "')");
          }
          else
          {
            sql.AppendLine(" WHERE " + _sqlObject.IdentifierProperty + " in ('" + String.Join("','", identifiers.ToArray()) + "')");
          }
          if (so.SecondaryObjects[0].MinimumProperties != null)
          {
            throw new NotImplementedException("Does not support deep relations yet");
          }
          _dataTable = getData.SelectRecords(sql.ToString());
          _dataTable.TableName = so.ObjectName;
          AddDataTable(_dataTable);
        }
      }
    }

    public string GetObjectName()
    {
      return _primaryObject;
    }

    public string GetPrimaryTableName()
    {
      return _primaryTable;
    }

    public string GetIdentifier()
    {
      return _identifiers[0];
    }
    public DataSet GetDataSet()
    {
      return _dataSet;
    }

    
    public DataTable GetObjectData(string objectName)
    {
      DataTable tab = new DataTable();
      foreach (DataTable dataTable in _dataSet.Tables)
      {
        if (dataTable.TableName == objectName)
        {
          tab = dataTable;
          break;
        }
      }
      return tab;
    }

    private void AddDataTable(DataTable dataTable)
    {
      _dataSet.Tables.Add(dataTable);
    }

    //public object GetPropertyValue(string propertyName)
    //{
    //  object propertyValue = null;
    //  if (_sqlObject.IdentifierProperty.Contains(propertyName))
    //  {
    //    propertyValue = _identifiers[0];
    //  }
    //  else
    //  {
    //    foreach (DataTable dataTable in _dataSet.Tables)
    //    {
    //      if (dataTable.TableName == _primaryTable)
    //      {
    //        foreach (DataRow datafield in dataTable.Rows)
    //        {
    //          foreach (DataColumn property in dataTable.Columns)
    //          {
    //            if (property.ColumnName == propertyName)
    //            {
    //              propertyValue = datafield[property];
    //              break;
    //            }
    //          }
    //        }
    //      }
    //    }
    //  }
    //  return propertyValue;
    //}

    public IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      return new List<IDataObject>();
    }

    public DataTable GetDataTable(string objectName)
    {
      DataTable dt = null;
      foreach (DataTable dataTable in _dataSet.Tables)
      {
        if (dataTable.TableName == objectName)
          dt = dataTable;
      }
      return dt;
    }

    public void SetSecondaryProperty(string propertyName, object value, string tableName)
    {
      foreach (DataTable dataTable in _dataSet.Tables)
      {
        if (dataTable.TableName == tableName)
        {
          if (dataTable.Rows.Count == 0)
          {
            DataRow newRow = dataTable.NewRow();
            newRow[propertyName] = value;
            dataTable.Rows.Add(newRow);
          }
          else
          {
            foreach (DataRow datafield in dataTable.Rows)
            {
              foreach (DataColumn property in dataTable.Columns)
              {
                if (property.ColumnName == propertyName)
                {
                  datafield[property] = value;
                  break;
                }
              }
            }
          }
        }
      }
    }

    //public void SetPropertyValue(string propertyName, object value)
    //{
    //  foreach (DataTable dataTable in _dataSet.Tables)
    //  {
    //    if (dataTable.TableName == _primaryTable)
    //    {
    //      if (dataTable.Rows.Count == 0)
    //      {
    //        DataRow newRow = dataTable.NewRow();
    //        newRow[propertyName] = value;
    //        dataTable.Rows.Add(newRow);
    //      }
    //      else
    //      {
    //        foreach (DataRow datafield in dataTable.Rows)
    //        {
    //          foreach (DataColumn property in dataTable.Columns)
    //          {
    //            if (property.ColumnName == propertyName)
    //            {
    //              datafield[property] = value;
    //              break;
    //            }
    //          }
    //        }
    //      }
    //    }
    //  }
    //}
  }
}
