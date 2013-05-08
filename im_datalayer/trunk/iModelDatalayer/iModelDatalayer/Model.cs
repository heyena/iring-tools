using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;


namespace org.iringtools.adapter.datalayer
    {
    public class Model 
        {
        public OdbcConnection connection;
        private string fileName;
        public string DBCatagory;

        public Model ()
            {
            }

        public Model (string fileName)
            {
            this.fileName = fileName;
            }
        //
        //Demonstrate how to make a connection to i-model
        //
        private OdbcConnection Connection
            {
            get
                {
                if (connection == null)
                    {
                    string connectionString = "DSN=i-model datasource;imodel=" + fileName; 
                    connection = new OdbcConnection (connectionString);
                    connection.Open ();
                    }
                return connection;
                }
            }
        //
        //Close the connection when we are done
        //
        public void Close ()
            {
            connection.Close ();
            }

        //
        //Setter of i-model file name. Each i-model file is modeled as a database category.
        //The name of the category is a normalized verion of the file name.
        //
        public string FileName
            {
            get
                {
                return fileName;
                }
            set
                {
                if (fileName != value)
                    {
                    if (connection != null && connection.State== ConnectionState.Open)
                        connection.Close ();
                    connection = null;
                    }
                fileName = value;
                }
            }

        //
        //Demonstrate how to get all the schemas of an i-model
        //
        public List<string> Schemas
            {
            get
                {
                 //   return new List<string>() { "bharat" ,"gagan"};
                List<string> schamas = new List<string> ();
                DataTable tables = Connection.GetSchema ("Tables");

                //Get catagory
                if (tables.Rows.Count > 0)
                    {
                    DBCatagory = tables.Rows[0]["TABLE_CAT"] as string;
                    if (!char.IsLetterOrDigit (DBCatagory[0]))
                        {
                        DBCatagory = "\"" + DBCatagory + "\"";
                        }
                    }
                foreach (DataRow r in tables.Rows)
                {
                    if (r["TABLE_SCHEM"].ToString().ToLower().Contains("provenance") == false &&
                        r["TABLE_SCHEM"].ToString().ToLower().Contains("schemaext") == false)
                    {
                        if (!schamas.Contains(r["TABLE_SCHEM"] as string))
                        {
                            schamas.Add(r["TABLE_SCHEM"] as string);
                        }
                    }
                }
                return schamas;
                }
            }


        //
        //Demonstrate how to get all the tables of a given schema
        //
        public List<string> GetTableNames (string schema)
            {
            List<string> retVal = new List<string> ();
            DataTable tables = Connection.GetSchema ("Tables", new string[] { null, schema });
            foreach (DataRow r in tables.Rows)
                retVal.Add (r["TABLE_NAME"] as string);

            return retVal;
            }

        //
        //Get all the fields of a table
        //
        public List<string> GetFields (string schema, string table)
            {
            List<string> retVal = new List<string> ();
            DataTable tables = Connection.GetSchema ("Columns", new string[] { null, schema, table });
            foreach (DataRow r in tables.Rows)
                retVal.Add (r["COLUMN_NAME"] as string);
            return retVal;
            }

        public List<FieldInfo> GetFieldInfo (string schema, string table)
            {
            List<FieldInfo> retVal = new List<FieldInfo> ();
            DataTable tables = Connection.GetSchema ("Columns", new string[] { null, schema, table });
            foreach (DataRow r in tables.Rows)
                {
                FieldInfo info = new FieldInfo ();
                info.Name = r["COLUMN_NAME"] as string;
                info.FiledType = r["TyPE_NAME"] as string ;               
                info.FiledLength = Convert.ToInt32(r["COLUMN_SIZE"]);
                info.isNullable = Convert.ToBoolean(r["NULLABLE"]);
               
                retVal.Add (info );
                }
            return retVal;
            }


        public string GetFieldType(string schema, string table,string colName)
        {
            string retVal = "Unknown";
            DataTable tables = Connection.GetSchema ("Columns", new string[] { null, schema, table });
            foreach (DataRow r in tables.Rows)
                if (r["COLUMN_NAME"] as string == colName)
                {
                    retVal = r[5] as string;
                    break;
                }
            return retVal;
        }

        // Get the business key given the table name
        public string GetBusinessKey(string tableName)
        {
            DataTable dt = GetView("BK", "Select * from " + DBCatagory + ".SchemaExt.BusinessKey");
            foreach (DataRow r in dt.Rows)
            {
                if (r["TableName"].ToString().ToLower() == tableName.ToLower())
                    return r["BusinessKey"] as string;
            }
            return null;
        }

        //
        //Using sql statement to get data. Make sure table name it the command text is in the format of "Category.schema.tableName".
        //
        public DataTable GetView (string viewName, string commandstr)
            {
            
            DataTable dt = new DataTable (viewName);
            try
                {
                OdbcCommand command = new OdbcCommand (commandstr, Connection);
                OdbcDataReader reader = command.ExecuteReader ();     //(command, Connection);
                dt.Load (reader);
                reader.Close ();
                }
            catch (Exception e)
                {
                    throw;
                }
            return dt;
            }
        
        //
        //Retrieve all data from the model
        //
        public DataSet Data
            {
            get 
                {
                string command = "select * from ";
                DataSet retVal = new DataSet ();
                foreach (string schema in Schemas)
                    {
                    List<string> tables = GetTableNames(schema);
                    foreach (string table in tables)
                        {
                        string commandstr = command + DBCatagory + "." + schema + "." + table;
                        OdbcDataAdapter adapter = new OdbcDataAdapter(commandstr, Connection);
                        DataTable dt = new DataTable (table);
                        adapter.Fill (dt);
                        retVal.Tables.Add (dt);
                        }
                    }
                return retVal;
                }
            }
        }
    public class FieldInfo
        {
        public string Name { get; set; }
        public string FiledType { get; set; }
        public int FiledLength { get; set; }
        public bool isNullable { get; set; }
        }
    }


