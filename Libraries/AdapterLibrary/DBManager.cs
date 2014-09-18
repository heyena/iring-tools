using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using log4net;
using org.iringtools.library;
using System.Xml;

namespace org.iringtools.adapter
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

        dbConn = OpenConnection(connStr);
        dbCmd = new SqlCommand(query, (SqlConnection)dbConn);
        dbAdapter = new SqlDataAdapter();
        ((SqlDataAdapter)dbAdapter).SelectCommand = (SqlCommand)dbCmd;
        dbAdapter.Fill(dataSet);

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
        _logger.Error("Error executing query [" + query + "]: " + ex.Message);
        throw ex;
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
        dbConn = OpenConnection(connStr);
        dbCmd = dbConn.CreateCommand();
        dbCmd.CommandText = command;
        affectedRows = dbCmd.ExecuteNonQuery();

        return (affectedRows > 0);
      }
      catch (Exception ex)
      {
        _logger.Error("Error executing command [" + command + "]: " + ex.Message);
        throw ex;
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
      DbConnection dbConn = null;

      try
      {
        dbConn = OpenConnection(connStr);

        foreach (var pair in idCmdMap)
        {
          DbCommand dbCmd = new SqlCommand(pair.Value, (SqlConnection)dbConn);
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
        _logger.Error("Error excuting update: " + ex.Message);
        throw ex;
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
        _logger.Error("Error getting database version: " + ex.Message);
        throw ex;
      }
      finally
      {
        CloseConnection(dbConn);
      }
    }

    public bool ExecuteNonQueryStoredProcedure(string connStr, string spName)
    {
        _logger.Debug(spName);

        DbConnection dbConn = null;
        DbCommand dbCmd = null;
        int affectedRows = 0;

        try
        {
            dbConn = OpenConnection(connStr);
            dbCmd = new SqlCommand();
            dbCmd.CommandType = System.Data.CommandType.StoredProcedure;
            dbCmd.CommandText = spName;
            dbCmd.Connection = dbConn;

            dbCmd.CommandTimeout = 0;
            affectedRows = dbCmd.ExecuteNonQuery();

            return (affectedRows > 0);
        }
        catch (Exception ex)
        {
            _logger.Error("Error executing stored procedure [" + spName + "]: " + ex.Message);
            throw ex;
        }
        finally
        {
            CloseConnection(dbConn);
        }
    }

    public bool ExecuteNonQueryStoredProcedure(string connStr, string spName, NameValueList spParamters)
    {
        _logger.Debug(spName);

        DbConnection dbConn = null;
        DbCommand dbCmd = null;
        int affectedRows = 0;

        try
        {
            dbConn = OpenConnection(connStr);   
            dbCmd = new SqlCommand();
            dbCmd.CommandType = System.Data.CommandType.StoredProcedure;
            dbCmd.CommandText = spName;
            dbCmd.Connection = dbConn;


            foreach (ListItem item in spParamters)
            {
                dbCmd.Parameters.Add(new SqlParameter(item.Name, item.Value));
            }
            
            dbCmd.CommandTimeout = 0;
            affectedRows = dbCmd.ExecuteNonQuery();

            return (affectedRows > 0);
        }
        catch (Exception ex)
        {
            _logger.Error("Error executing stored procedure [" + spName + "]: " + ex.Message);
            throw ex;
        }
        finally
        {
            CloseConnection(dbConn);
        }
    }

    public string ExecuteXmlQuery(string connStr, string spName, NameValueList spParamters)
    {
        _logger.Debug(spName);

        SqlConnection conn = null;
        SqlCommand dbCmd = null;
        string xmlString = string.Empty;

        try
        {
            conn = new SqlConnection(connStr);
            conn.Open();

            dbCmd = new SqlCommand();
            dbCmd.CommandType = System.Data.CommandType.StoredProcedure;
            dbCmd.CommandText = spName;
            dbCmd.Connection = conn;


            foreach (ListItem item in spParamters)
            {
                dbCmd.Parameters.Add(new SqlParameter(item.Name, item.Value));
            }

            dbCmd.CommandTimeout = 0;
            using (XmlReader reader = dbCmd.ExecuteXmlReader())
            {
                while (reader.Read())
                {
                    xmlString = reader.ReadOuterXml(); // XML returned from SP.                    
                }
            }


            return xmlString;
        }
        catch (Exception ex)
        {
            _logger.Error("Error executing stored procedure [" + spName + "]: " + ex.Message);
            throw ex;
        }
        finally
        {
            CloseConnection(conn);
        }
    }

    public byte[] ExecuteBytesQuery(string connStr, string spName, NameValueList spParamters)
    {
        _logger.Debug(spName);

        SqlConnection conn = null;
        SqlCommand dbCmd = null;
        string xmlString = string.Empty;
        byte[] byteArray;

        try
        {
            conn = new SqlConnection(connStr);
            conn.Open();

            dbCmd = new SqlCommand();
            dbCmd.CommandType = System.Data.CommandType.StoredProcedure;
            dbCmd.CommandText = spName;
            dbCmd.Connection = conn;


            foreach (ListItem item in spParamters)
            {
                dbCmd.Parameters.Add(new SqlParameter(item.Name, item.Value));
            }

            dbCmd.CommandTimeout = 0;
            SqlDataAdapter myAdapter1 = new SqlDataAdapter(dbCmd);
            DataTable dt = new DataTable();
            myAdapter1.Fill(dt);

            foreach (DataRow row in dt.Rows)
            {
                byteArray = (byte[])row["graphObject"];
                return byteArray;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error("Error executing stored procedure [" + spName + "]: " + ex.Message);
            throw ex;
        }
        finally
        {
            CloseConnection(conn);
        }
    }

    public DataTable ExecuteStoredProcedure(string connStr, string spName, NameValueList spParamters)
    {
        _logger.Debug(spName);
        SqlConnection _conn = new SqlConnection(connStr);

        try
        {
            _conn.Open();

            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable datatable = new DataTable();
            cmd = new SqlCommand(spName, _conn);

            foreach (ListItem item in spParamters)
            {
                cmd.Parameters.Add(new SqlParameter(item.Name, item.Value));
            }

            cmd.CommandType = CommandType.StoredProcedure;
            adapter.SelectCommand = cmd;
            adapter.Fill(datatable);

            return datatable;
        }
        catch (Exception ex)
        {
            _logger.Error("Error executing stored procedure [" + spName + "]: " + ex.Message);
            throw ex;
        }
        finally
        {
            _conn.Close();
        }
    }

    public DataTable ExecuteStoredProcedure(string connStr, string spName)
    {
        _logger.Debug(spName);
        SqlConnection _conn = new SqlConnection(connStr);

        try
        {
            _conn.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(spName, _conn);
            DataTable datatable = new DataTable();
            adapter.Fill(datatable);
            return datatable;
        }
        catch (Exception ex)
        {
            _logger.Error("Error executing stored procedure [" + spName + "]: " + ex.Message);
            throw ex;
        }
        finally
        {
            _conn.Close();
        }
    }
    #region helper methods
    private DbConnection OpenConnection(string connStr)
    {
      try
      {
        DbConnection dbConn = new SqlConnection(connStr);
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
