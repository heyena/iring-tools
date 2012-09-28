using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using log4net;
using Oracle.DataAccess.Client;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer.sppid
{
  public sealed class DBManager
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DBManager));
    private static volatile DBManager _instance;
    private static object _lockObj = new Object();

    private DBManager() { }

    public static DBManager Instance
    {
      get
      {
        if (_instance == null)
        {
          lock (_lockObj)
          {
            if (_instance == null)
            {
              _instance = new DBManager();
            }
          }
        }

        return _instance;
      }
    }

    public DataTable ExecuteQuery(string connStr, string query)
    {
      _logger.Debug(query);

      DbConnection dbConn = null;
      DbCommand dbCmd = null;
      DataAdapter dbAdapter = null;
      
      try
      {
        DataSet dataSet = new DataSet();
        DBType dbType = Utility.GetDBType(connStr);

        dbConn = OpenConnection(connStr);
        
        if (dbType == DBType.ORACLE)
        {
          dbCmd = new OracleCommand(query, (OracleConnection)dbConn);
          dbAdapter = new OracleDataAdapter((OracleCommand)dbCmd);
          dbAdapter.Fill(dataSet);
        }
        else if (dbType == DBType.SQLServer)
        {
          dbCmd = new SqlCommand(query, (SqlConnection)dbConn);
          dbAdapter = new SqlDataAdapter();
          ((SqlDataAdapter)dbAdapter).SelectCommand = (SqlCommand)dbCmd;
          dbAdapter.Fill(dataSet);
        }

        if (dataSet != null && dataSet.Tables != null)
        {
          return dataSet.Tables[0];
        }
        else
        {
          return null;
        }
      }
      catch (Exception ex)
      {
        string error = "Error executing query [" + query + "]: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        CloseConnection(dbConn);
      }
    }

    public bool ExecuteNonQuery(string connStr, string command)
    {
      _logger.Debug(command);

      DbConnection dbConn = null;
      DbCommand dbCmd = null;      
      int affectedRows = 0;

      try
      {
        DBType dbType = Utility.GetDBType(connStr);

        dbConn = OpenConnection(connStr);        
        dbCmd = dbConn.CreateCommand();
        dbCmd.CommandText = command;
        affectedRows = dbCmd.ExecuteNonQuery();

        return (affectedRows > 0);
      }
      catch (Exception ex)
      {
        string error = "Error executing command [" + command + "]: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        CloseConnection(dbConn);
      }
    }

    public Response ExecuteUpdate(string connStr, Dictionary<string, string> idCmdMap)
    {
      if (idCmdMap == null || idCmdMap.Count == 0)
      {
        string error = "Nothing to update.";
        _logger.Error(error);
        throw new Exception(error);
      }

      Response response = new Response();
      DBType dbType = Utility.GetDBType(connStr);
      DbConnection dbConn = null;

      try
      {
        dbConn = OpenConnection(connStr);

        foreach (var pair in idCmdMap)
        {
          DbCommand dbCmd = null;

          if (dbType == DBType.ORACLE)
          {
            dbCmd = new OracleCommand(pair.Value, (OracleConnection)dbConn);
          }
          else
          {
            dbCmd = new SqlCommand(pair.Value, (SqlConnection)dbConn);
          }

          DbTransaction dbTransaction = dbConn.BeginTransaction();
          dbCmd.Transaction = dbTransaction;
          int affectedRows = dbCmd.ExecuteNonQuery();
          dbTransaction.Commit();

          string message = (affectedRows > 0) ? "[" + pair.Key + "] saved." : "Saving [" + pair.Key + "] failed.";

          Status status = new Status()
          {
            Identifier = pair.Key,
            Level = (affectedRows > 0) ? StatusLevel.Success : StatusLevel.Warning,
            Messages = new Messages { message }
          };

          response.Append(status);
        }

        return response;
      }
      catch (Exception ex)
      {
        string error = "Error excuting update: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        CloseConnection(dbConn);
      }
    }

    public string GetVersion(string connStr)
    {
      DbConnection dbConn = null;

      try
      {
        dbConn = OpenConnection(connStr);
        return dbConn.ServerVersion;
      }
      catch (Exception ex)
      {
        string error = "Error getting DB version: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        CloseConnection(dbConn);
      }
    }

    #region helper methods
    private DbConnection OpenConnection(string connStr)
    {
      try
      {
        DbConnection dbConn = null;
        DBType dbType = Utility.GetDBType(connStr);

        if (dbType == DBType.ORACLE)
        {
          dbConn = new OracleConnection(connStr);
        }
        else if (dbType == DBType.SQLServer)
        {
          dbConn = new SqlConnection(connStr);
        }
        else
        {
          throw new Exception("DB type not supported.");
        }

        dbConn.Open();

        return dbConn;
      }
      catch (Exception ex)
      {
        string error = "Error creating DB connection: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    private void CloseConnection(DbConnection dbConn)
    {
      try
      {
        if (dbConn != null && dbConn.State == ConnectionState.Open)
        {
          dbConn.Close();
          dbConn.Dispose();
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error closing DB connection: " + ex.Message);
      }
    }
    #endregion
  }
}
