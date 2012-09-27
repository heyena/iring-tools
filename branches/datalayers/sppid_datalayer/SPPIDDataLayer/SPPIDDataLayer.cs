using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using log4net;
using Ninject;
using org.iringtools.library;
using IU = org.iringtools.utility;

namespace org.iringtools.adapter.datalayer.sppid
{
  //TODO: commit updates from staging table to SPPID table(s)
  public class SPPIDDataLayer : BaseSQLDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SPPIDDataLayer));
    private string _dataPath;
    private string _project;
    private string _application;
    private WorkingSet _workingSet;
    private string _stagingConnStr;
    private DataDictionary _dataDictionary;

    [Inject]
    public SPPIDDataLayer(AdapterSettings settings)
      : base(settings)
    {
      try
      {
        _dataPath = _settings["DataLayerPath"] ?? _settings[Constants.DATA_PATH];
        _project = _settings[Constants.PROJECT];
        _application = _settings[Constants.APPLICATION];
        _stagingConnStr = _settings[Constants.SPPID_STAGING];
      }
      catch (Exception ex)
      {
        string error = "Error initializing SPPID data layer: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    #region override methods
    public override DatabaseDictionary GetDatabaseDictionary()
    {
      try
      {
        string dictionaryPath = string.Format("{0}DataDictionary.{1}.{2}.xml", _dataPath, _project, _application);

        if (!File.Exists(dictionaryPath))
        {
          RefreshDataTable(string.Empty);
        }

        DataDictionary dataDictionary = IU.Utility.Read<DataDictionary>(dictionaryPath);

        _dbDictionary = new DatabaseDictionary()
        {
          Provider = Utility.GetDBType(_settings[Constants.SPPID_STAGING]).ToString(),
          dataObjects = dataDictionary.dataObjects
        };
      }
      catch (Exception ex)
      {
        string error = "Error getting data dictionary: " + ex.Message;
        _logger.Error(error);
        throw ex;
      }

      return _dbDictionary;
    }

    /// <summary>
    ///   This method does the following: 
    ///     Rebuild staging tables
    ///     Refetch data 
    ///     Recreate data dictionary
    /// </summary>
    public override Response RefreshDataTable(string tableName)
    {
      Response response = new Response();

      try
      {
        string configPath = string.Format("{0}{1}.{2}.configuration.xml", _dataPath, _project, _application);
        if (!File.Exists(configPath))
        {
          throw new Exception("Configuration file [" + configPath + "] not found.");
        }

        //
        // Rebuild staging table and refetch data
        //
        XDocument configDoc = XDocument.Load(configPath);
        Response stagingRefresh = Refresh(configDoc, tableName);

        //
        // Cache data dictionary
        //
        if (stagingRefresh.Level != StatusLevel.Error)
        {
          string path = string.Format(@"{0}DataDictionary.{1}.{2}.xml", _dataPath, _project, _application);
          IU.Utility.Write<DataDictionary>(_dataDictionary, path);

          response.StatusList.Add(new Status()
          {
            Messages = new Messages()
          {
            "Data dictionary refreshed successfully."
          }
          });
        }
      }
      catch (Exception ex)
      {
        string error = "Error refreshing [" + tableName + "]: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }

      return response;
    }

    public override long GetCount(string tableName, string whereClause)
    {
      string query = string.Empty;

      try
      {
        query = "SELECT COUNT(*) FROM " + tableName.ToUpper();

        if (whereClause != null && whereClause.TrimStart().ToUpper().StartsWith("WHERE"))
        {
          int orderByIndex = whereClause.ToUpper().IndexOf("ORDER BY");

          if (orderByIndex != -1)
          {
            whereClause = whereClause.Remove(orderByIndex);
          }

          query += " " + whereClause;
        }

        DataTable result = DBManager.Instance.ExecuteQuery(_stagingConnStr, query);
        Int64 count = Convert.ToInt64(result.Rows[0][0]);

        return count;
      }
      catch (Exception ex)
      {
        string error = "Error getting count [" + query + "]: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override DataTable GetDataTable(string tableName, string whereClause, long start, long limit)
    {
      DataTable result = null;
      string query = string.Empty;

      try
      {
        DBType dbType = Utility.GetDBType(_stagingConnStr);

        if (limit > 0)
        {
          if (dbType == DBType.ORACLE)
          {
            string subQuery = "SELECT * FROM " + tableName + " " + whereClause;
            query = string.Format("SELECT * FROM ({0}) WHERE rownum between {1} and {2}", subQuery, start + 1, start + limit);
          }
          else if (dbType == DBType.SQLServer)
          {            
            int orderByIndex = whereClause.ToUpper().IndexOf("ORDER BY");
            string orderByClause = string.Empty;

            if (orderByIndex != -1)
            {
              orderByClause = whereClause.Substring(orderByIndex);
              whereClause = whereClause.Remove(orderByIndex);
            }

            query = string.Format(@"
                SELECT * FROM (SELECT row_number() over (order by current_timestamp) as __rn, * 
                FROM {0} {1}) as __query WHERE __rn between {2} and {3} {4}", 
                tableName, whereClause, start + 1, start + limit, orderByClause);
          }
          else
          {
            throw new Exception("Database type not supported.");
          }
        }
        else if (!string.IsNullOrEmpty(whereClause))
        {
          query = string.Format("SELECT * FROM {0} {1}", tableName, whereClause);
        }
        else
        {
          throw new Exception("Invalid request.");
        }

        result = DBManager.Instance.ExecuteQuery(_stagingConnStr, query);
      }
      catch (Exception ex)
      {
        string error = "Error executing [" + query + "]: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }

      return result;
    }

    public override DataTable GetDataTable(string tableName, IList<string> identifiers)
    {
      try
      {
        DatabaseDictionary dbDictionary = GetDatabaseDictionary();
        DataObject objDef = _dbDictionary.dataObjects.Find(p => p.tableName.ToLower() == tableName.ToLower());
        string whereClause = FormWhereClause(objDef, identifiers);
        return GetDataTable(tableName, whereClause, 0, 0);
      }
      catch (Exception ex)
      {
        string error = "Error getting [" + string.Join(",", identifiers.ToArray() + "] from [" + tableName + "]: " + ex.Message);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override IList<string> GetIdentifiers(string tableName, string whereClause)
    {
      try
      {
        DataTable result = GetDataTable(tableName, whereClause, 0, 0);
        DataObject objDef = _dbDictionary.dataObjects.Find(x => x.tableName.ToLower() == tableName.ToLower());
      
        List<string> identifiers = FormIdentifiers(objDef, result);
        return identifiers;
      }
      catch (Exception ex)
      {
        string error = "Error getting identifiers from [" + tableName + "] with [" + whereClause + "]: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override DataTable CreateDataTable(string tableName, IList<string> identifiers)
    {
      try
      {
        DataTable result = null;
        
        if (identifiers == null || identifiers.Count == 0)
        {
          //
          // create an empty row
          //
          result = GetDataTable(tableName, " WHERE 1=0", 0, 1);
          result.Rows.Add(result.NewRow());

          return result;
        }

        result = GetDataTable(tableName, identifiers);

        //
        // if number of rows returned does not match number of identifiers passed in, it means some identifiers exist
        // and some don't. Create empty data objects with only key properties set for the ones that don't.
        //
        if (result.Rows.Count < identifiers.Count)
        {
          DataObject objDef = _dbDictionary.dataObjects.Find(x => x.tableName.ToLower() == tableName.ToLower());
          List<string> existingIdentifiers = FormIdentifiers(objDef, result);

          foreach (string identifier in identifiers)
          {
            if (!string.IsNullOrEmpty(identifier))
            {
              if (!existingIdentifiers.Contains(identifier))
              {
                DataRow row = result.NewRow();
                SetKeyProperties(row, objDef, identifier);
                result.Rows.Add(row);
              }
            }
            else
            {
              _logger.Error("Identifier cannot be blank.");
            }
          }
        }

        return result;
      }
      catch (Exception ex)
      {
        string error = "Error creating [" + string.Join(",", identifiers.ToArray() + "] in [" + tableName + "]: " + ex.Message);

        _logger.Error(error);
        throw new Exception(error);
      }
    }
    
    public override Response DeleteDataTable(string tableName, string whereClause)
    {
      Response response = new Response();

      try
      {
        if (string.IsNullOrEmpty(whereClause))
        {
          response.Level = StatusLevel.Error;
          response.Messages.Add("Filter is required.");
        }
        else
        {
          Dictionary<string, string> idCmdMap = new Dictionary<string, string>();
          idCmdMap[tableName] = "DELETE FROM " + tableName + whereClause;

          Response deleteResponse = DBManager.Instance.ExecuteUpdate(_stagingConnStr, idCmdMap);
          response.Append(deleteResponse);
        }
      }
      catch (Exception ex)
      {
        string error = "Error deleting data from [" + tableName + "] " + whereClause + ": " + ex.Message;
        _logger.Error(error);
        response.Level = StatusLevel.Error;
        response.Messages = new Messages { error };
      }

      return response;
    }

    public override Response DeleteDataTable(string tableName, IList<string> identifiers)
    {
      Response response = new Response();
            
      if (identifiers == null || identifiers.Count == 0)
      {
        response.Level = StatusLevel.Error;
        response.Messages = new Messages { "Identifiers not found." };
        return response;
      }

      DataObject objDef = _dbDictionary.dataObjects.Find(x => x.tableName.ToLower() == tableName.ToLower());
      Dictionary<string, string> idCmdMap = new Dictionary<string, string>();

      try
      {
        foreach (string identifier in identifiers)
        {
          string whereClause = FormWhereClause(objDef, identifier);

          if (!string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(whereClause))
          {
            idCmdMap[identifier] = "DELETE FROM " + tableName + whereClause;
          }
        }

        Response deleteResponse = DBManager.Instance.ExecuteUpdate(_stagingConnStr, idCmdMap);
        response.Append(deleteResponse);
      }
      catch (Exception ex)
      {
        string error = "Error deleting data rows from [" + tableName + "]: " + ex.Message;
        _logger.Error(error);
        response.Level = StatusLevel.Error;
        response.Messages = new Messages { error };
      }

      return response;
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();

      try
      {
        IList<DataTable> dataTables = new List<DataTable>();

        Dictionary<string, DataObject> objectTypesObjectDefinitions = new Dictionary<string, DataObject>();
        Dictionary<string, IList<string>> objectTypesIdentifiers = new Dictionary<string, IList<string>>();
        Dictionary<string, IList<IDataObject>> objectTypesDataObjects = new Dictionary<string, IList<IDataObject>>();

        if (dataObjects != null)
        {
          foreach (IDataObject dataObject in dataObjects)
          {
            string objectType = dataObject.GetType().Name;

            if (objectType == typeof(GenericDataObject).Name)
            {
              objectType = ((GenericDataObject)dataObject).ObjectType;
            }

            if (objectTypesIdentifiers.ContainsKey(objectType))
            {
              DataObject objectDefinition = objectTypesObjectDefinitions[objectType];
              string identifier = GetIdentifier(objectDefinition, dataObject);
              objectTypesIdentifiers[objectType].Add(identifier);
              objectTypesDataObjects[objectType].Add(dataObject);
            }
            else
            {
              DataObject objectDefinition = GetObjectDefinition(objectType);
              string identifier = GetIdentifier(objectDefinition, dataObject);
              objectTypesObjectDefinitions[objectType] = objectDefinition;
              objectTypesIdentifiers[objectType] = new List<string>() { identifier };
              objectTypesDataObjects[objectType] = new List<IDataObject>() { dataObject };
            }
          }
        }

        foreach (var pair in objectTypesIdentifiers)
        {
          DataObject objectDefinition = objectTypesObjectDefinitions[pair.Key];

          if (!objectDefinition.isReadOnly)
          {
            IList<string> identifiers = objectTypesIdentifiers[pair.Key];
            DataTable dataTable = CreateDataTable(objectDefinition.tableName, pair.Value);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
              dataTable.TableName = objectDefinition.tableName;
              dataTables.Add(dataTable);
            }
          }
        }

        if (dataTables.Count == 0)
        {
          response.Level = StatusLevel.Warning;
          response.Messages = new Messages() { "No data to post." };

          return response;
        }

        foreach (DataTable dataTable in dataTables)
        {
          Response res = PostDataTable(dataTable);
          response.Append(res);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error posting data objects: " + ex);
        throw ex;
      }

      return response;
    }

    // this method should not be called since Post(IList<IDataObject>) has been overriden
    public override Response PostDataTables(IList<DataTable> dataTables)
    {
      return new Response();
    }

    protected Response PostDataTable(DataTable dataTable)
    {
      Response response = new Response();
            
      try
      {
        if (dataTable == null || dataTable.Rows.Count == 0)
        {
          Status status = new Status()
          {
            Level = StatusLevel.Error,
            Messages = new Messages() { "No rows in [" + dataTable.TableName + "] to update." }
          };

          response.StatusList.Add(status);
        }
        else
        {
          string tableName = dataTable.TableName;
          DataObject objDef = _dbDictionary.dataObjects.Find(x => x.tableName.ToLower() == tableName.ToLower());
          Dictionary<string, string> idCmdMap = new Dictionary<string, string>();
            
          foreach (DataRow row in dataTable.Rows)
          {
            string identifier = FormIdentifier(objDef, row);
            string updateCmd = string.Empty;
            int columnCount = dataTable.Columns.Count;

            if (row.RowState == DataRowState.Added)
            {
              StringBuilder colsBuilder = new StringBuilder();
              StringBuilder valsBuilder = new StringBuilder();

              foreach (DataColumn col in dataTable.Columns)
              {
                if (row[col.ColumnName] != DBNull.Value && row[col.ColumnName] != null)
                {
                  colsBuilder.Append("," + col.ColumnName);

                  if (Utility.IsNumeric(col.DataType))
                  {
                    valsBuilder.Append("," + row[col.ColumnName].ToString());
                  }
                  else
                  {
                    valsBuilder.Append("," + "'" + row[col.ColumnName].ToString() + "'");
                  }
                }
              }

              updateCmd = string.Format(Constants.SQL_INSERT_TEMPLATE, tableName, colsBuilder.Remove(0, 1), valsBuilder.Remove(0, 1));
            }
            else if (row.RowState == DataRowState.Modified) // || row.RowState == DataRowState.Deleted)
            {
              StringBuilder builder = new StringBuilder();
              string whereClause = FormWhereClause(objDef, row);

              foreach (DataColumn col in dataTable.Columns)
              {
                if (row[col.ColumnName] != DBNull.Value && row[col.ColumnName] != null)
                {
                  if (Utility.IsNumeric(col.DataType))
                  {
                    builder.Append("," + col.ColumnName + "=" + row[col.ColumnName].ToString());
                  }
                  else
                  {
                    builder.Append("," + col.ColumnName + "='" + row[col.ColumnName].ToString() + "'");
                  }
                }
              }

              if (row.RowState == DataRowState.Modified)
              {
                updateCmd = string.Format(Constants.SQL_UPDATE_TEMPLATE, tableName, builder.Remove(0, 1), whereClause);
              }
              //else
              //{
              //  updateCmd = string.Format(Constants.SQL_DELETE_TEMPLATE, tableName, whereClause);
              //}
            }

            if (!string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(updateCmd))
            {
              idCmdMap[identifier] = updateCmd;
            }
            else
            {
              _logger.Error("[" + identifier + "] not found or unchanged.");
            }
          }

          Response updateResponse = DBManager.Instance.ExecuteUpdate(_stagingConnStr, idCmdMap);
          response.Append(updateResponse);
        }
      }
      catch (Exception ex)
      {
        string error = "Error updating [" + dataTable.TableName + "]: " + ex.Message;

        response.Level = StatusLevel.Error;
        response.Messages.Add(error);

        _logger.Error(error);
      }

      return response;
    }

    public override long GetRelatedCount(DataRow parentRow, string relatedTableName)
    {
      try
      {
        string whereClause = FormWhereClause(parentRow, relatedTableName);
        long count = GetCount(relatedTableName, whereClause);
        return count;
      }
      catch (Exception ex)
      {
        string error = "Error getting related count: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override DataTable GetRelatedDataTable(DataRow parentRow, string relatedTableName, long start, long limit)
    {
      try
      {
        string whereClause = FormWhereClause(parentRow, relatedTableName);
        DataTable result = GetDataTable(relatedTableName, whereClause, start, limit);
        return result;
      }
      catch (Exception ex)
      {
        string error = "Error getting related rows: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override DataTable GetRelatedDataTable(DataRow parentRow, string relatedTableName)
    {
      try
      {
        string whereClause = FormWhereClause(parentRow, relatedTableName);
        DataTable result = GetDataTable(relatedTableName, whereClause, 1, int.MaxValue);
        return result;
      }
      catch (Exception ex)
      {
        string error = "Error getting related rows: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }
    #endregion

    #region helper methods
    private Response Refresh(XDocument configDoc, string tableName)
    {
      Response response = new Response();

      try
      {
        _dataDictionary = new library.DataDictionary();

        XElement config = configDoc.Element("configuration");
        SQLBuilder sqlBuilder;

        //
        // Parse project assignments
        //
        Dictionary<string, string> projectAssignments = new Dictionary<string, string>();
        IEnumerable<XElement> projectAssignmentElts = config.Element("assignments").Elements("assignment");

        if (projectAssignmentElts != null && projectAssignmentElts.Count() > 0)
        {
          foreach (XElement assignment in projectAssignmentElts)
          {
            string name = assignment.Attribute("name").Value;
            string value = assignment.Attribute("value").Value;

            if (name.StartsWith("@") && value.Length > 0)
            {
              projectAssignments[name] = value;
            }
          }
        }

        // 
        // Parse project text replacements
        //
        Dictionary<string, string> projectReplacements = new Dictionary<string, string>();
        IEnumerable<XElement> projectReplacementElts = config.Element("replacements").Elements("replacement");

        if (projectReplacementElts != null && projectReplacementElts.Count() > 0)
        {
          foreach (XElement replacement in projectReplacementElts)
          {
            string placeHolder = replacement.Attribute("placeHolder").Value;
            string name = replacement.Attribute("name").Value;
            string value = replacement.Attribute("value").Value;

            if (placeHolder == string.Empty || name == string.Empty || value == string.Empty)
            {
              continue;
            }

            projectReplacements[placeHolder[0] + name + placeHolder[1]] = value;
          }
        }

        //
        // Get query elements
        //
        IEnumerable<XElement> queryElts = config.Elements("query");

        string siteConnStr = _settings[Constants.SPPID_SITE_SCHEMA];
        DBType siteDbType = Utility.GetDBType(siteConnStr);
        DataTable siteSchemaResult = DBManager.Instance.ExecuteQuery(siteConnStr, Constants.ORACLE_GET_CURRENT_SCHEMA);
        string siteSchema = siteSchemaResult.Rows[0][0].ToString();

        //
        // Process !SiteData query
        //
        XElement siteQueryElt = (from query in queryElts
                                 where query.Attribute("name").Value == Constants.SITE_DATA_QUERY
                                 select query).First();

        Dictionary<string, string> siteSchemaMap = new Dictionary<string, string>();
        siteSchemaMap["SITE"] = siteSchema;

        sqlBuilder = new SQLBuilder(siteDbType, siteQueryElt, siteSchemaMap, projectAssignments, projectReplacements);
        string siteSelectQuery = sqlBuilder.Build(SQLCommand.SELECT);
        DataTable siteInfo = DBManager.Instance.ExecuteQuery(siteConnStr, siteSelectQuery);

        // 
        // Get actual schemas from !SiteData query
        //
        Dictionary<string, string> schemaMap = new Dictionary<string, string>();

        if (siteInfo != null && siteInfo.Rows.Count > 0)
        {
          foreach (DataRow row in siteInfo.Rows)
          {
            schemaMap[row["SP_SCHEMA_TYPE"].ToString()] = row["USERNAME"].ToString();
          }
        }

        //
        // Process other queries
        //
        if (string.IsNullOrEmpty(tableName))
        {
          queryElts = from query in queryElts
                      where query.Attribute("name").Value != Constants.TEMPLATE_QUERY && query.Attribute("name").Value != Constants.SITE_DATA_QUERY
                      select query;
        }
        else
        {
          queryElts = from query in queryElts
                      where query.Attribute("name").Value != Constants.TEMPLATE_QUERY && query.Attribute("name").Value != Constants.SITE_DATA_QUERY && query.Attribute("destination").Value.ToUpper() == tableName.ToUpper()
                      select query;
        }

        DBType stagingDbType = Utility.GetDBType(_stagingConnStr);

        //   NOTE - although it is possible to make use of an INTO clause to create a selection query that will 
        //   also automatically create the destination table, this has limitations, the most serious of which is
        //   it is not safe to assume that the Source DB and Staging DB have the same security requirements. Instead,
        //   we will always assume that security is separate for these two databases and that the connection strings for the 
        //   Source and Staging connections provide this information for each individual location. We also cannot assume that
        //   the specified credentials have the power to create a Linked Server connection or that both SQL Server instances
        //   allow ad hoc (OpenDataSource) queries. Instead, the provided credentials are used to copy the data to the 
        //   local machine and then bulk copied out to the staging server, bypassing the need for a more sophisticated security
        //   check/edit)

        DBType plantDbType = Utility.GetDBType(_settings[Constants.SPPID_PLANT_SCHEMA]);

        if (plantDbType == DBType.ORACLE)
        {
          _workingSet = new WorkingSet(
            _settings[Constants.SPPID_PLANT_SCHEMA],
            _settings[Constants.SPPID_PLANT_DICTIONARY],
            _settings[Constants.SPPID_PID_SCHEMA],
            _settings[Constants.SPPID_PID_DICTIONARY]);
        }
        else if (plantDbType == DBType.SQLServer)
        {
          _workingSet = new WorkingSet(_settings[Constants.SPPID_PLANT_SCHEMA]);
        }
        else
        {
          throw new Exception("SPPID DB type not supported.");
        }

        _workingSet.GrantPrivilege("SELECT");

        foreach (XElement queryElt in queryElts)
        {
          sqlBuilder = new SQLBuilder(stagingDbType, queryElt, schemaMap, projectAssignments, projectReplacements, true);

          response.StatusList.Add(new Status()
          {
            Messages = new Messages()
            {
              "Query [" + queryElt.Attribute("name").Value + "] processed."
            }
          });

          //
          // Delete existing staging table
          //
          string stagingTableName = queryElt.Attribute("destination").Value;
          string deleteQuery = string.Format(Constants.SQLSERVER_DELETE_TEMPLATE, stagingTableName);
          DBManager.Instance.ExecuteNonQuery(_stagingConnStr, deleteQuery);

          //
          // Create new staging table
          //
          string createQuery = sqlBuilder.Build(SQLCommand.CREATE);
          DBManager.Instance.ExecuteNonQuery(_stagingConnStr, createQuery);

          response.StatusList.Add(new Status()
          {
            Messages = new Messages()
            {
              "Staging table [" + stagingTableName + "] created."
            }
          });

          //
          // Fetch data
          //
          string selectQuery = sqlBuilder.Build(SQLCommand.SELECT);
          DataTable result = DBManager.Instance.ExecuteQuery(_settings[Constants.SPPID_PLANT_SCHEMA], selectQuery);

          response.StatusList.Add(new Status()
          {
            Messages = new Messages()
            {
              "New data fetched."
            }
          });

          //
          // Bulk copy data to staging table
          //
          SqlBulkCopy bulkCopy = new SqlBulkCopy(_stagingConnStr);
          bulkCopy.DestinationTableName = stagingTableName;
          bulkCopy.WriteToServer(result);

          response.StatusList.Add(new Status()
          {
            Messages = new Messages()
            {
              "Data copied to staging table."
            }
          });

          // 
          // Add to data dictionary
          //
          DataObject objDef = new DataObject()
          {
            tableName = stagingTableName,
            objectNamespace = "SPPID",
            objectName = stagingTableName
          };

          foreach (var pair in sqlBuilder.Keys)
          {
            objDef.keyProperties.Add(new KeyProperty()
            {
              keyPropertyName = pair.Key
            });
          }

          foreach (DBField field in sqlBuilder.Fields)
          {
            DataProperty dataProperty = new DataProperty()
            {
              propertyName = field.Name,
              columnName = field.Name,
              dataType = Utility.ResolveDataType(field.DataType),
              isNullable = field.Nullable,
            };

            if (sqlBuilder.Keys.ContainsKey(field.Name))
            {
              dataProperty.keyType = (sqlBuilder.Keys[field.Name] == KeyType.AUTO)
                ? library.KeyType.unassigned : library.KeyType.assigned;
            }

            objDef.dataProperties.Add(dataProperty);
          }

          _dataDictionary.dataObjects.Add(objDef);
        }

        _workingSet.RevokePrivilege("SELECT");
      }
      catch (Exception ex)
      {
        string error = "Error refreshing [" + tableName + "]: " + ex.Message;

        response.Level = StatusLevel.Error;
        response.Messages = new Messages() { error };
        _logger.Error(error);
      }

      return response;
    }

    private string FormWhereClause(DataObject objDef, string identifier)
    {
      return FormWhereClause(objDef, new List<string> { identifier });
    }

    private string FormWhereClause(DataObject objDef, IList<string> identifiers)
    {
      try
      {
        StringBuilder clauseBuilder = new StringBuilder();
        string[] delim = new string[] { objDef.keyDelimeter };

        foreach (string id in identifiers)
        {
          StringBuilder exprBuilder = new StringBuilder();
          string[] idParts = id.Split(delim, StringSplitOptions.None);

          for (int i = 0; i < objDef.keyProperties.Count; i++)
          {
            string key = objDef.keyProperties[i].keyPropertyName;
            DataProperty prop = objDef.dataProperties.Find(x => x.propertyName.ToLower() == key.ToLower());

            string expr = (Utility.IsNumeric(prop.dataType))
              ? string.Format("{0} = {1}", key, idParts[i])
              : string.Format("{0} = '{1}'", key, idParts[i]);

            if (exprBuilder.Length > 0)
            {
              exprBuilder.Append(" and ");
            }

            exprBuilder.Append(expr);
          }

          if (clauseBuilder.Length > 0)
          {
            clauseBuilder.Append(" or ");
          }

          clauseBuilder.Append("(" + exprBuilder.ToString() + ")");
        }

        if (clauseBuilder.Length > 0)
        {
          clauseBuilder.Insert(0, " WHERE ");
        }

        return clauseBuilder.ToString();
      }
      catch (Exception ex)
      {
        string error = "Error forming WHERE clause: " + ex;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    private string FormWhereClause(DataObject objDef, DataRow dataRow)
    {
      try
      {
        StringBuilder clauseBuilder = new StringBuilder();
        IDataObject dataObject = ToDataObject(dataRow, objDef);
        string op = " AND ";
                
        foreach (KeyProperty keyProp in objDef.keyProperties)
        {
          string key = keyProp.keyPropertyName;
          DataProperty prop = objDef.dataProperties.Find(x => x.propertyName.ToLower() == key.ToLower());

          string expr = (Utility.IsNumeric(prop.dataType))
            ? string.Format("{0} = {1}", key, dataRow[key].ToString())
            : string.Format("{0} = '{1}'", key, dataRow[key].ToString());

          clauseBuilder.Append(op + expr);
        }

        if (clauseBuilder.Length > 0)
        {
          clauseBuilder.Remove(0, op.Length);
          clauseBuilder.Insert(0, " WHERE ");
        }

        return clauseBuilder.ToString();
      }
      catch (Exception ex)
      {
        string error = "Error forming WHERE clause from data row: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    private string FormWhereClause(DataRow parentRow, string relatedTableName)
    {
      //
      // validate relationship
      //
      if (parentRow == null)
        throw new Exception("Parent data row is empty.");

      DataObject parentObjDef = _dbDictionary.dataObjects.Find(x => x.tableName.ToUpper() == parentRow.Table.TableName.ToUpper());

      if (parentObjDef == null)
        throw new Exception("Parent object [" + parentRow.Table.TableName + " not found.");

      DataObject childObjDef = _dbDictionary.dataObjects.Find(x => x.tableName.ToUpper() == relatedTableName.ToUpper());

      if (childObjDef == null)
        throw new Exception("Child object [" + relatedTableName + " not found.");

      DataRelationship relationship = parentObjDef.dataRelationships.Find(x => x.relatedObjectName.ToUpper() == childObjDef.objectName.ToUpper());

      if (relationship == null)
        throw new Exception("Relationship between [" + parentRow.Table.TableName + "] and [" + relatedTableName + " not found.");

      //
      // build WHERE clause 
      //
      StringBuilder builder = new StringBuilder();

      foreach (PropertyMap propMap in relationship.propertyMaps)
      {
        DataProperty relatedProp = childObjDef.dataProperties.Find(x => x.propertyName.ToUpper() == propMap.relatedPropertyName.ToUpper());

        if (relatedProp == null)
          throw new Exception("Related property [" + propMap.relatedPropertyName + "] not found.");

        string value = parentRow[propMap.dataPropertyName].ToString();

        if (!Utility.IsNumeric(relatedProp.dataType))
        {
          value = "'" + value + "'";
        }

        if (builder.Length > 0)
          builder.Append(" and ");

        builder.Append(propMap.dataPropertyName + " = " + value);
      }

      if (builder.Length > 0)
        builder.Insert(0, " WHERE ");

      return builder.ToString();
    }

    private List<string> FormIdentifiers(DataObject objDef, DataTable dataTable)
    {
      List<string> identifiers = new List<string>();

      try
      {
        if (objDef != null && dataTable != null)
        {
          foreach (DataRow row in dataTable.Rows)
          {
            identifiers.Add(FormIdentifier(objDef, row));
          }
        }
      }
      catch (Exception ex)
      {
        string error = "Error forming identifiers from data rows: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }

      return identifiers;
    }

    private string FormIdentifier(DataObject objDef, DataRow dataRow)
    {
      try
      {
        string[] identifierParts = new string[objDef.keyProperties.Count];
        IDataObject dataObject = ToDataObject(dataRow, objDef);

        for (int i = 0; i < objDef.keyProperties.Count; i++)
        {
          KeyProperty keyProp = objDef.keyProperties[i];
          object value = dataObject.GetPropertyValue(keyProp.keyPropertyName);

          if (value != null)
          {
            identifierParts[i] = value.ToString();
          }
          else
          {
            identifierParts[i] = String.Empty;
          }
        }

        return string.Join(objDef.keyDelimeter, identifierParts);
      }
      catch (Exception ex)
      {
        string error = "Error forming identifier from data object: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    private void SetKeyProperties(DataRow dataRow, DataObject objDef, string identifier)
    {
      try
      {
        List<KeyProperty> keyProps = objDef.keyProperties;

        if (keyProps.Count == 1)
        {
          dataRow[keyProps.First().keyPropertyName] = identifier;
        }
        else
        {
          string delimiter = objDef.keyDelimeter;
          string[] identifierParts = identifier.Split(delimiter.ToCharArray());

          for (int i = 0; i < identifierParts.Length; i++)
          {
            dataRow[keyProps[i].keyPropertyName] = identifierParts[i];
          }
        }
      }
      catch (Exception ex)
      {
        string error = "Error setting key properties from [" + identifier + "]: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }
    #endregion
  }
}
