using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace WCFWithSQL
{
  public class Provider
  {
    public bool ResetDB()
    {
      try
      {
        string connStr = "Server=.\\SQLEXPRESS;database=DEF;User Id=def;Password=def";
        string query = @"DELETE FROM [LINES]
          INSERT INTO [LINES] VALUES ('90010-O','0100000039','90','','AAB3','O','010',80,'IH','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-010','mm','kPag','Deg C')
          INSERT INTO [LINES] VALUES ('90011-O','0100000043','91','','AAB3','O','011',90,'IP','','NEW','5.2','','','','','','','','90-AO5676','150','','','','***','','NEW','90-O-011','INCH','kPag','Deg C')
          INSERT INTO [LINES] VALUES ('90012-O','0100000031','92','','AAB3','O','012',100,'IP','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-012','mm','kPag','Deg C')";

        SqlConnection connection = new SqlConnection(connStr);
        connection.Open();

        SqlCommand command = new SqlCommand(query, connection);
        command.ExecuteScalar();

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}