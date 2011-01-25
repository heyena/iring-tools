using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Xml.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using log4net;
using Ninject;

namespace org.iringtools.library.mssql
{
  public class MSSQLDataLayer : IDataLayer
  {

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MSSQLDataLayer));
    private AdapterSettings _settings = null;

    private string _dataDictionaryPath = String.Empty;
    private DataDictionary _dataDictionary = null;
    private string applicationName = string.Empty;
    private string projectName = string.Empty;
    private MSSQLConfiguration _configration = null;

    [Inject]
    public MSSQLDataLayer(AdapterSettings settings)
    {
      _dataDictionaryPath = string.Format("{0}DataDictionary.{1}.xml", settings["XmlPath"], settings["Scope"]);
      _settings = settings;
      projectName = _settings["Scope"].Split('.')[0];
      applicationName = _settings["Scope"].Split('.')[1];
      _configration = GetConfiguration(projectName, applicationName);
    }

    public DataDictionary GetDictionary()
    {
      DataTable dataTable;
      _dataDictionary = new DataDictionary();
      MSSQLConfiguration sqlObjects = GetConfiguration(projectName, applicationName);
      foreach (SqlObject sqlObject in sqlObjects)
      {
        dataTable = new DataTable();
        DataCRUD getData = new DataCRUD(sqlObject.ConnectionString);
        dataTable = getData.GetSqlTableSchema(sqlObject);
        dataTable.TableName = sqlObject.ObjectName;
        _dataDictionary.DataObjects.Add(CreateDataObject(dataTable, sqlObject));
      }
      return _dataDictionary;
    }

    private DataObject CreateDataObject(DataTable dataTable, SqlObject sqlObject)
    {
      DataProperty dataProperty = null;
      string colName = string.Empty;
      DataObject dataObject = new DataObject { TableName = dataTable.TableName, ObjectName = sqlObject.ObjectTypeName };
      if (sqlObject.IdentifierType == IdType.Foreign)
      {
        dataProperty = new DataProperty
        {
          ColumnName = sqlObject.IdentifierProperty.Split('.')[1],
          DataLength = 50,
          DataType = DataType.String,
          IsNullable = false,
          KeyType = KeyType.assigned,
          PropertyName = sqlObject.IdentifierProperty.Split('.')[1]
        };
        dataObject.AddKeyProperty(dataProperty);
      }
      foreach (DataRow dataField in dataTable.Rows)
      {
        dataProperty = new DataProperty();
        foreach (DataColumn property in dataTable.Columns)
        {
          switch (property.ColumnName)
          {
            case "ProviderType":
              break;
            case "ColumnName":
              colName = dataField[property].ToString();
              dataProperty.ColumnName = colName;
              dataProperty.PropertyName = colName;
              break;
            case "ColumnSize":
              dataProperty.DataLength = Convert.ToInt32(dataField[property].ToString());
              break;
            case "IsUnique":
              break;
            case "IsKey":
              if (Convert.ToBoolean(dataField[property]) == true)
              {
                var key = dataObject.KeyProperties.Where(c => c.KeyPropertyName == colName).FirstOrDefault();
                if (key == null)
                {
                  dataProperty.KeyType = KeyType.assigned;
                }
              }
              break;
            case "DataType":
              dataProperty.DataType = (DataType)Enum.Parse(typeof(DataType), dataField[property].ToString().Split('.')[1]);
              break;
            case "IsIdentity":
              break;
            case "DataTypeName":
              break;
            case "AllowDBNull":
              dataProperty.IsNullable = Convert.ToBoolean(dataField[property]);

              break;
          }
        }
        var exists = dataObject.DataProperties.Select(c => c.ColumnName == dataProperty.ColumnName).FirstOrDefault();
        if (!exists && dataProperty.KeyType == KeyType.unassigned)
        {
          dataObject.DataProperties.Add(dataProperty);
        }
        else if (dataProperty.KeyType != KeyType.unassigned)
        {
          if (sqlObject.IdentifierProperty.Contains(dataProperty.ColumnName) &&
            !sqlObject.KeyProperties.Contains(dataProperty.ColumnName))
          {
            dataObject.AddKeyProperty(dataProperty);
          }
        }
      }
      return dataObject;
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        StringBuilder sql = null;
        List<string> propertyMap = new List<string>();
        List<string> identifierMap = new List<string>();
        List<SecondaryObject> secondaryObjects = new List<SecondaryObject>();
        SqlObject objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == objectType);
        secondaryObjects.AddRange(objectSql.SecondaryObjects);

        DataCRUD getData = new DataCRUD(objectSql.ConnectionString);
        object keyValue = null;
        List<IDataObject> dataObjects = new List<IDataObject>();

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            MSSQLObject dataObject = new MSSQLObject(objectSql, identifier);
            keyValue = dataObject.GetPropertyValue(objectSql.KeyProperties);
            if (!string.IsNullOrEmpty(identifier))
            {
              if (!string.IsNullOrEmpty(objectSql.KeyReferenceObject.ReferenceObjectName) && keyValue == null)
              {
                sql = new StringBuilder();
                sql.AppendLine(string.Format("SELECT {0}", objectSql.KeyReferenceObject.ReturnProperty));
                sql.AppendLine(string.Format(" FROM {0}", objectSql.KeyReferenceObject.ReferenceObjectName));
                sql.AppendLine(string.Format(" WHERE {0}", objectSql.KeyReferenceObject.WhereClause));
                keyValue = getData.ExecuteScalar(sql.ToString());
                keyValue = Convert.ToInt32(keyValue) + 1;
                if (keyValue != null)
                {
                  sql = new StringBuilder();
                  sql.AppendLine(string.Format("UPDATE {0}", objectSql.KeyReferenceObject.ReferenceObjectName));
                  sql.AppendLine(string.Format(" SET {0} = '{1}'", objectSql.KeyReferenceObject.ReturnProperty, keyValue));
                  sql.AppendLine(string.Format(" WHERE {0}", objectSql.KeyReferenceObject.WhereClause));
                  getData.ExecuteNonQuery(sql.ToString());
                }
              }
              if (objectSql.IdentifierMapSeperator != null)
              {
                string delimStr = objectSql.IdentifierMapSeperator;
                char[] delimiters = delimStr.ToArray();
                propertyMap.AddRange(objectSql.IdentifierMap.Split(delimiters, StringSplitOptions.RemoveEmptyEntries));
                identifierMap.AddRange(identifier.Split(delimiters, StringSplitOptions.RemoveEmptyEntries));
              }
              else
              {
                propertyMap.Add(objectSql.IdentifierMap);
                identifierMap.Add(identifier);
              }
              //First do key column
              if (keyValue != null)
              {
                dataObject.SetPropertyValue(objectSql.KeyProperties, keyValue);
              }
              // then idetifier
              if (objectSql.IdentifierType != IdType.Foreign)
              {
                dataObject.SetPropertyValue(objectSql.IdentifierProperty, identifier);
              }

              if (propertyMap.Count > 0)
              {
                for (int i = 0; i <= propertyMap.Count - 1; i++)
                {
                  dataObject.SetPropertyValue(propertyMap[i], identifierMap[i]);
                }
              }
              //then status if any
              if (!string.IsNullOrEmpty(objectSql.StatusProperty) && !string.IsNullOrEmpty(objectSql.CreateStatus))
              {
                dataObject.SetPropertyValue(objectSql.StatusProperty, objectSql.CreateStatus);
              }
              //process any secondaty table values
              DataTable primaryTable = null;

              SecondaryObject dso = null;
              List<string> selectProperties = new List<string>();
              primaryTable = ((MSSQLObject)dataObject).GetDataTable(objectSql.ObjectName);
              foreach (SecondaryObject so in secondaryObjects)
              {

                if (so.MinimumProperties != null)
                {
                  if (so.SecondaryObjects.Count == 1)
                  {
                    dso = so.SecondaryObjects[0];
                    string delimStr = ",";
                    char[] delimiters = delimStr.ToArray();
                    selectProperties.AddRange(dso.SelectQuery.Split(delimiters, StringSplitOptions.RemoveEmptyEntries));
                  }
                  //process secondary table key
                  if (objectSql.KeyProperties.Contains(so.KeyProperty))
                  {
                    ((MSSQLObject)dataObject).SetSecondaryProperty(so.KeyProperty, keyValue, so.ObjectName);
                  }
                  if (objectSql.IdentifierProperty.Contains(so.IdentifierProperty))
                  {
                    ((MSSQLObject)dataObject).SetSecondaryProperty(so.IdentifierProperty, identifier, so.ObjectName);
                  }
                  if (!string.IsNullOrEmpty(so.StatusProperty) && !string.IsNullOrEmpty(so.CreateStatus))
                  {
                    ((MSSQLObject)dataObject).SetSecondaryProperty(so.StatusProperty, so.CreateStatus, so.ObjectName);
                  }
                  //process other dependent properties
                  foreach (string select in selectProperties)
                  {
                    string value = select;
                    StringBuilder query = new StringBuilder();
                    query.AppendLine(string.Format(" SELECT {0}", select));
                    query.AppendLine(string.Format(" FROM {0}", dso.ObjectName));
                    query.AppendLine(string.Format(" WHERE {0}", dso.WhereClause));
                    object result = getData.ExecuteScalar(query.ToString());
                    if (select.Contains(" AS "))
                    {
                      string[] str = select.Split(' ');
                      value = str[str.Length - 1];
                    }
                    ((MSSQLObject)dataObject).SetSecondaryProperty(value, result, so.ObjectName);
                  }
                }
              }
            }
            dataObjects.Add(dataObject);
          }
        }
        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in CreateList: " + ex);
        throw new Exception("Error while creating a list of data objects of type [" + objectType + "].", ex);
      }
    }



    public Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        Response response = new Response();
        Status status = new Status();
        StringBuilder sql = new StringBuilder();
        IList<string> identifiers = new List<string>();
        SqlObject objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == objectType);
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue(objectSql.IdentifierProperty));
        }

        return Delete(objectType, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
    }


    public Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();
      Status status = new Status();
      IList<IDataObject> dataObjects = new List<IDataObject>();
      if (identifiers == null || identifiers.Count == 0)
      {
        status.Messages.Add("Nothing to delete");
        response.StatusList.Add(status);
        return response;
      }
      try
      {
        StringBuilder sql; ;
        SqlObject objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == objectType);
        DataCRUD deleteData = new DataCRUD(objectSql.ConnectionString);

        foreach (string identifier in identifiers)
        {
          sql = new StringBuilder();
          //delete / flag primary record
          if (!string.IsNullOrEmpty(objectSql.DeleteQuery))
          {
            sql.AppendLine(string.Format("UPDATE {0}", objectSql.ObjectName));
            sql.AppendLine(string.Format(" SET {0}", objectSql.DeleteQuery));
          }
          else
          {
            sql.AppendLine(string.Format("DELETE FROM {0}", objectSql.ObjectName));
          }
          sql.AppendLine(string.Format(" WHERE {0} = '{1}'", objectSql.IdentifierProperty, identifier));
          deleteData.ExecuteNonQuery(sql.ToString());

          // delete / flag secondary object
          foreach (SecondaryObject so in objectSql.SecondaryObjects)
          {
            sql = new StringBuilder();
            if (!string.IsNullOrEmpty(so.DeleteQuery))
            {
              sql.AppendLine(so.DeleteQuery);
            }
            else
            {
              sql.AppendLine(string.Format("DELETE FROM {0}", objectSql.ObjectName));
            }
            sql.AppendLine(string.Format(" WHERE {0} = '{1}'", so.WhereClause, identifier));

            if (sql.Length > 0)
            {
              deleteData.ExecuteNonQuery(sql.ToString());
            }
          }
          status.Messages.Add("Success: Object with identifier " + identifier + " deleted or flagged as deleted");
        }
        response.StatusList.Add(status);
        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      StringBuilder sql = new StringBuilder();

      SqlObject objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == objectType);
      IList<string> identifiers = GetIdentifiers(objectType, null);

      List<IDataObject> dataObjects = Get(objectType, identifiers).ToList();

      //// Apply filter
      if (filter != null && filter.Expressions.Count > 0)
      {
        string variable = objectSql.ObjectName;
        string whereExpression = string.Empty;

        whereExpression = filter.ToSqlWhereClause(GetDictionary(), variable, null);

      }
      // Apply paging
      if (pageSize > 0 && pageNumber > 0)
      {
        if (dataObjects.Count > (pageSize * (pageNumber - 1) + pageSize))
        {
          dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), pageSize);
        }
        else if (pageSize * (pageNumber - 1) > dataObjects.Count)
        {
          dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), dataObjects.Count);
        }
        else
        {
          return null;
        }
      }
      return dataObjects;
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      try
      {
        List<IDataObject> dataObjects = new List<IDataObject>();
        SqlObject objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == objectType);
        if (identifiers == null)
        {
          identifiers = new List<string>();
          identifiers = GetIdentifiers(objectType, null);
        }
        foreach (String identifier in identifiers)
        {
          MSSQLObject dataObject = new MSSQLObject(objectSql, identifier);
          dataObjects.Add(dataObject);
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public IList<String> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {

        List<String> identifiers = new List<String>();
        SqlObject objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == objectType);

        StringBuilder sql = new StringBuilder();
        if (!string.IsNullOrEmpty(objectSql.SelectSqlJoin))
        {
          sql.AppendLine("SELECT " + objectSql.IdentifierProperty);
          sql.AppendLine(" FROM " + objectSql.ObjectName);
          sql.AppendLine(" " + objectSql.SelectSqlJoin);
          if (filter != null)
          {
            sql.AppendLine(" " + filter.ToSqlWhereClause(GetDictionary(), objectSql.ObjectName, objectSql.ObjectName));
          }
          else if (!string.IsNullOrEmpty(objectSql.ListSqlWhere))
          {
            sql.AppendLine(" WHERE " + objectSql.ListSqlWhere);
          }
        }
        else if (!string.IsNullOrEmpty(objectSql.IdentifierProperty))
        {
          sql.AppendLine("SELECT " + objectSql.IdentifierProperty);
          sql.AppendLine(" FROM " + objectSql.ObjectName);
          if (filter != null)
          {
            sql.Append(filter.ToSqlWhereClause(GetDictionary(), objectSql.ObjectName, null));
          }
          else if (!string.IsNullOrEmpty(objectSql.ListSqlWhere))
          {
            sql.AppendLine(" WHERE " + objectSql.ListSqlWhere);
          }

        }
        DataCRUD getData = new DataCRUD(objectSql.ConnectionString);
        DataTable identifiersList = getData.SelectRecords(sql.ToString());
        foreach (DataRow dataField in identifiersList.Rows)
        {
          foreach (DataColumn property in identifiersList.Columns)
          {
            identifiers.Add(dataField[property].ToString());
          }
        }
        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    public long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        List<String> identifiers = new List<String>();
        SqlObject objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == objectType);

        StringBuilder sql = new StringBuilder();
        if (!string.IsNullOrEmpty(objectSql.SelectSqlJoin))
        {
          sql.AppendLine("SELECT " + objectSql.IdentifierProperty);
          sql.AppendLine(" FROM " + objectSql.ObjectName);
          sql.AppendLine(" " + objectSql.SelectSqlJoin);
          if (filter != null)
          {
            sql.AppendLine(" " + filter.ToSqlWhereClause(GetDictionary(), objectSql.ObjectName, objectSql.ObjectName));
          }
          else if (!string.IsNullOrEmpty(objectSql.ListSqlWhere))
          {
            sql.AppendLine(" WHERE " + objectSql.ListSqlWhere);
          }
        }
        else if (!string.IsNullOrEmpty(objectSql.IdentifierProperty))
        {
          sql.AppendLine("SELECT " + objectSql.IdentifierProperty);
          sql.AppendLine(" FROM " + objectSql.ObjectName);
          if (filter != null)
          {
            sql.Append(filter.ToSqlWhereClause(GetDictionary(), objectSql.ObjectName, null));
          }
          else if (!string.IsNullOrEmpty(objectSql.ListSqlWhere))
          {
            sql.AppendLine(" WHERE " + objectSql.ListSqlWhere);
          }

        }
        DataCRUD getData = new DataCRUD(objectSql.ConnectionString);
        DataTable identifiersList = getData.SelectRecords(sql.ToString());

        return identifiersList.Rows.Count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      return new List<IDataObject>();//no related objects expected
    }

    public Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      Status status = new Status();
      DataTable _dataTable = null;
      SqlObject objectSql = null;
      List<string> secondaryObjects = new List<string>();
      string primaryTable = string.Empty;
      string primaryObject = string.Empty;
      DataCRUD saveData = null;
      List<string> success = new List<string>();

      if (dataObjects == null || dataObjects.Count == 0)
      {
        status.Messages.Add("Nothing to update");
        response.StatusList.Add(status);
        return response;
      }
      try
      {
        foreach (IDataObject dataObject in dataObjects)
        {
          primaryObject = ((MSSQLObject)dataObject).GetObjectName();
          primaryTable = ((MSSQLObject)dataObject).GetPrimaryTableName();
          if (objectSql == null)
          {
            objectSql = _configration.FirstOrDefault(c => c.ObjectTypeName == primaryObject);
            saveData = new DataCRUD(objectSql.ConnectionString);
            foreach (SecondaryObject so in objectSql.SecondaryObjects)
            {
              if (!string.IsNullOrEmpty(so.MinimumProperties))
              {
                secondaryObjects.Add(so.ObjectName);
              }
            }
          }
          DataSet dataSet = ((MSSQLObject)dataObject).GetDataSet();
          foreach (DataTable dataTable in dataSet.Tables)
          {
            _dataTable = new DataTable { TableName = dataTable.TableName };
            if (dataTable.TableName == primaryTable)
            {
              _dataTable.Merge(dataTable);
              saveData = new DataCRUD(objectSql.ConnectionString);
              saveData.UpdateData(_dataTable);
            }
            else
            {
              if (secondaryObjects.Count > 0)
              {
                _dataTable = new DataTable { TableName = dataTable.TableName };
                _dataTable.Merge(dataTable);
                saveData = new DataCRUD(objectSql.ConnectionString);
                saveData.UpdateData(dataTable);
              }
            }
          }
          status.Messages.Add("Data Object [" + ((MSSQLObject)dataObject).GetIdentifier() + "] has been saved successfully.");
        }
        response.StatusList.Add(status);
        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PostList: " + ex);
        throw new Exception("Error while posting data objects of type [" + objectSql.ObjectName + "].", ex);
      }
    }

    public MSSQLConfiguration GetConfiguration(string projectName, string applicationName)
    {
      MSSQLConfiguration msSQLCofiguration = new MSSQLConfiguration();

      string scope = string.Format("mssql-configuration.{0}.{1}.xml", projectName, applicationName);
      string path = Path.Combine(_settings["XmlPath"], scope);
      if (File.Exists(path))
      {
        msSQLCofiguration = Utility.Read<MSSQLConfiguration>(path, true);
      }
      else
      {
        throw new Exception("File " + scope + " not found at " + path);
      }
      return msSQLCofiguration;
    }

    public SqlObject GetObjectSql(string objectTypeName)
    {
      return _configration.FirstOrDefault(c => c.ObjectTypeName == objectTypeName);
    }

  }
}
