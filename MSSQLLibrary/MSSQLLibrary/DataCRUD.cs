using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data;
using System.Data.SqlClient;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer 
{
  public class DataCRUD
  {
    private SqlConnection sConn = null;
    private SqlCommand command = null;
    private SqlDataAdapter dataAdapter = null;
    public DataCRUD(String connectionString)
    {
      sConn = new SqlConnection(connectionString);
    }

    public virtual DataTable SelectRecords(String selectCommand)
    {

      command = new SqlCommand(selectCommand, sConn);
      command.CommandType = CommandType.Text;
      OpenConnection();

      SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
      DataTable dataTable = new DataTable();
      dataTable.Load(reader);
      return dataTable;

    }

    public virtual void UpdateData(DataTable dataTable)
    {
      DataTable dt = new DataTable();
      dt.TableName = dataTable.TableName;
      using (sConn)
      {
        using (dataAdapter = new SqlDataAdapter(string.Format("SELECT * FROM {0}", dt.TableName) ,sConn))
        {
          SqlCommandBuilder scb = new SqlCommandBuilder(dataAdapter);
          dataAdapter.InsertCommand = scb.GetInsertCommand(true);
          dataAdapter.UpdateCommand = scb.GetUpdateCommand(true);
          dataAdapter.DeleteCommand = scb.GetDeleteCommand(true);
          for (int i = dataTable.Rows.Count-1; i > 0; i--)
          {
            if (dataTable.Rows[i].RowState == DataRowState.Unchanged)
            {
              dataTable.Rows[i].Delete();
            }
          }
          OpenConnection();
          dataAdapter.Fill(dt);
          dt.Merge(dataTable);
          dataAdapter.Update(dt);
        }
      }
    }
    public virtual object ExecuteScalar(string selectCommand)
    {
      object obj = null;
      command = new SqlCommand(selectCommand, sConn);
      command.CommandType = CommandType.Text;
      OpenConnection();
      obj = command.ExecuteScalar();
      return obj;
    }

    public virtual void ExecuteNonQuery(String sqlCommand)
    {
      command = new SqlCommand(sqlCommand, sConn);
      command.CommandType = CommandType.Text;
      OpenConnection();
      int rows = command.ExecuteNonQuery();
    }

    private void OpenConnection()
    {
      if (sConn.State == ConnectionState.Open)
        sConn.Close();

      sConn.Open();
    }

    public DataTable GetSqlTableSchema(SqlObject sqlObject)
    {
      DataTable dataTable = new DataTable();
      StringBuilder sql = new StringBuilder();

      using (sConn)
      {
        if (string.IsNullOrEmpty(sqlObject.SelectSqlJoin))
        {
          sql.AppendLine(string.Format("SELECT TOP 1 * FROM {0}", sqlObject.ObjectName));
        }
        else
        {
          sql.AppendLine(string.Format("SELECT TOP 1 {0}.* FROM {1} ", sqlObject.ObjectName, sqlObject.ObjectName));
          sql.AppendLine(sqlObject.SelectSqlJoin);
        }
        sql.AppendLine(string.Format(" WHERE {0}", sqlObject.ListSqlWhere));
        SqlCommand cmd = new SqlCommand(sql.ToString(), sConn);

        OpenConnection();

        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
        dataTable = reader.GetSchemaTable();
      }
      return dataTable;
    }
  }
}
