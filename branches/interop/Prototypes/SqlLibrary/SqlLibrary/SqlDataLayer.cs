using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using Ciloci.Flee;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using Ninject.Planning.Bindings;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using org.iringtools.sql;
using Ninject.Parameters;

namespace org.iringtools.adapter.datalayer
{
  public class SqlDataObject : IDataObject
  {

    private Dictionary<string, object> Row { get; set; }

    public SqlDataObject()
    {
      Row = new Dictionary<string, object>();
    }

    public string ObjectType { get; set; }

    public object GetPropertyValue(string propertyName)
    {
      if (Row.ContainsKey(propertyName))
      {
        return Row[propertyName];
      }
      else
      {        
        return null;
      }
    }

    public IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
      }
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      Row[propertyName] = value;
    }
  }

  public class ProviderModule : Ninject.Modules.NinjectModule
  {
    private string _provider = String.Empty;

    public ProviderModule(string provider)
    {
      _provider = provider.ToUpper();
    }

    public override void Load()
    {
      switch (_provider) 
      {
        case "ORACLE":
          Bind<IDbConnection>().ToMethod(c => new OracleConnection());
          Bind<IDbCommand>().ToMethod(c => new OracleCommand());      
          Bind<IDataParameter>().ToMethod(c => new OracleParameter());
          return;
        default:
          Bind<IDbConnection>().ToMethod(c => new SqlConnection());
          Bind<IDbCommand>().ToMethod(c => new SqlCommand());
          Bind<IDataParameter>().ToMethod(c => new SqlParameter());
          return;
      }
    }
  }
  
  public class SqlDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SqlDataLayer));
    private AdapterSettings _settings = null;
    private string _configPath = String.Empty;
    private string _bindingsPath = String.Empty;
    private Configuration _config = null;
    private Dictionary<string, Type> _dynamicTypes = new Dictionary<string, Type>();
    private StandardKernel _kernel = null;

    [Inject]
    public SqlDataLayer(AdapterSettings settings)
    {
      try
      {
        _settings = settings;

        _configPath = _settings["XmlPath"] + "sql-configuration." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";        
        _config = Utility.Read<Configuration>(_configPath, true);

        NinjectSettings nsettings = new NinjectSettings { LoadExtensions = false };                
        _kernel = new StandardKernel(nsettings, new XmlExtensionModule());
        _kernel.Load(new ProviderModule(_config.Provider));

      }
      catch (Exception e)
      {
        _logger.Error("Error in processing the Configuration File [" + _configPath + "].", e);
      }
    }

    ~SqlDataLayer()
    {
      _config = null;
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        ConfigObject configObject = GetConfigObject(objectType);        

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            SqlDataObject dataObject = new SqlDataObject()
            {
              ObjectType = objectType
            };

            if (!String.IsNullOrEmpty(identifier))
            {
              dataObject.SetPropertyValue(configObject.Identifier, identifier);              
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
        IList<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
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

      if (identifiers == null || identifiers.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to delete.");
        response.Append(status);
        return response;
      }
            
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        ConfigObject configObject = GetConfigObject(objectType);

        IDbCommand deleteCmd = configObject.GetCommand(_kernel, QueryType.DELETE, _config.GetConnection(_kernel));

        /*
        if (identifiers != null && identifiers.Count > 0)
        {           
          foreach (string identifier in identifiers)
          {
            Status status = new Status();
            status.Identifier = identifier;
            
            int row = GetRowIndex(xlWorksheet, cfWorksheet, identifier);

            try
            {
              xlWorksheet.Rows[row].Delete(Oracle.XlDeleteShiftDirection.xlShiftUp);
              status.Messages.Add("Data object [" + identifier + "] deleted successfully.");
            }
            catch (Exception ex)
            {
              _logger.Error("Error in Delete: " + ex);
              status.Level = StatusLevel.Error;
              status.Messages.Add("Error while deleting data object [" + identifier + "]." + ex);
            }

            response.Append(status);            
          }
        }

        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
        xlWorksheet = null;        
        */

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in DeletList: " + ex);
        throw new Exception("Error while delete a list of data objects of type [" + objectType + "].", ex);
      }      
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {      
      try
      {        
        ConfigObject configObject = GetConfigObject(objectType);
        IDbCommand selectCmd = configObject.GetCommand(_kernel, QueryType.SELECT, _config.GetConnection(_kernel));
                                
        List<IDataObject> dataObjects = new List<IDataObject>();

        if (selectCmd.Connection.State != ConnectionState.Open)
          selectCmd.Connection.Open();        

        IDataReader reader = selectCmd.ExecuteReader();

        while (reader.Read())
        {
          SqlDataObject dataObject = new SqlDataObject()
          {
            ObjectType = objectType
          };

          for (int i = 0; i < reader.FieldCount; i++)
          {
            dataObject.SetPropertyValue(reader.GetName(i), reader.GetValue(i));
          }

          dataObjects.Add(dataObject);
        }
        
        // Apply filter
        if (filter != null && filter.Expressions.Count > 0)
        {
          string variable = "dataObject";
          string linqExpression = string.Empty;
          switch (objectType)
          {
            default:
              linqExpression = filter.ToLinqExpression<IDataObject>(variable);
              break;
          }

          if (linqExpression != String.Empty)
          {
            ExpressionContext context = new ExpressionContext();
            context.Variables.DefineVariable(variable, typeof(SqlDataObject));

            for (int i = 0; i < dataObjects.Count; i++)
            {
              context.Variables[variable] = dataObjects[i];
              var expression = context.CompileGeneric<bool>(linqExpression);
              if (!expression.Evaluate())
              {
                dataObjects.RemoveAt(i--);
              }
            }
          }
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
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
     {      
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        
        ConfigObject configObject = GetConfigObject(objectType);

        IDbCommand selectCmd = configObject.GetCommand(_kernel, QueryType.SELECT, _config.GetConnection(_kernel));

        if (selectCmd != null)
        {

          if (identifiers != null && identifiers.Count > 0)
          {            
            IDbCommand cmd = _kernel.Get<IDbCommand>();
            cmd.Connection = selectCmd.Connection;
            cmd.CommandText = selectCmd.CommandText;
                        
            if (!cmd.CommandText.Contains("where"))
              cmd.CommandText += String.Format(" where ({0} = :Identifier)", configObject.Identifier);
            else
              cmd.CommandText += String.Format(" and ({0} = :Identifier)", configObject.Identifier);
            
            IDataParameter parameter = _kernel.Get<IDataParameter>();
            parameter.ParameterName = ":Identifier";
            parameter.DbType = DbType.Int32;
            cmd.Parameters.Add(parameter);

            if (cmd.Connection.State != ConnectionState.Open)
              cmd.Connection.Open();

            foreach (string identifier in identifiers)
            {
              parameter.Value = identifier;

              IDataReader reader = cmd.ExecuteReader();

              if (reader.Read())
              {
                SqlDataObject dataObject = new SqlDataObject()
                {
                  ObjectType = objectType
                };

                for (int i = 0; i < reader.FieldCount; i++)
                {
                  dataObject.SetPropertyValue(reader.GetName(i), reader.GetValue(i));
                }

                dataObjects.Add(dataObject);
              }
            }
          }
          else
          {
            if (selectCmd.Connection.State != ConnectionState.Open)
              selectCmd.Connection.Open();

            IDataReader reader = selectCmd.ExecuteReader();

            while (reader.Read())
            {
              SqlDataObject dataObject = new SqlDataObject()
              {
                ObjectType = objectType
              };

              for (int i = 0; i < reader.FieldCount; i++)
              {
                dataObject.SetPropertyValue(reader.GetName(i), reader.GetValue(i));
              }

              dataObjects.Add(dataObject);
            }
          }

        }        

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }    

    public DataDictionary GetDictionary()
    {
      try
      {
        DataDictionary dataDictionary = new DataDictionary()
        {
          dataObjects = new List<DataObject>()
        };
        
        foreach (ConfigObject configObject in _config.ConfigObjects)
        {
          DataObject dataObject = new DataObject()
          {
            objectName = configObject.Name,            
            dataProperties = new List<DataProperty>()            
          };

          dataDictionary.dataObjects.Add(dataObject);

          IDbCommand selectCmd = configObject.GetCommand(_kernel, QueryType.SELECT, _config.GetConnection(_kernel));

          if (selectCmd.Connection.State != ConnectionState.Open)
            selectCmd.Connection.Open();

          IDataReader reader = selectCmd.ExecuteReader();          
          DataTable table = reader.GetSchemaTable();
          foreach (DataRow row in table.Rows)
          {
            string columnName = row["ColumnName"].ToString();
            DataType columnDataType = Configuration.GetDataType((Type)row["DataType"]);

              DataProperty dataProperty = new DataProperty()
              {                
                propertyName = columnName,
                dataType = columnDataType,
                dataLength = Convert.ToInt32(row["ColumnSize"].ToString())
              };

              if (configObject.Identifier == columnName)
              {
                dataObject.addKeyProperty(dataProperty);
              }
              else
              {
                dataObject.dataProperties.Add(dataProperty);
              }
            
          }
          
        }        
        
        return dataDictionary;
      }
      catch (Exception e)
      {
        throw new Exception("Error while creating dictionary.", e);
      }
    }

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        List<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);
                
        ConfigObject configObject = GetConfigObject(objectType);
        
        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue(configObject.Identifier));
        }        

        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    private ConfigObject GetConfigObject(string objectType)
    {
      return _config.ConfigObjects.FirstOrDefault<ConfigObject>(o => o.Name == objectType);
    }

    public Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      //Oracle.Application xlApplication = null;
      //Oracle.Workbook xlWorkBook = null;

      if (dataObjects == null || dataObjects.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to update.");
        response.Append(status);
        return response;
      }

      try
      {
        /*
        xlApplication = new Oracle.Application();
        xlWorkBook = xlApplication.Workbooks.Open(_configuration.Location, 0, false, 5, "", "", true, Microsoft.Office.Interop.Oracle.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
         
        foreach(IDataObject dataObject in dataObjects)
        {
          string objectType = ((OracleDataObject)dataObject).SheetName;
          OracleWorksheet cfWorksheet = GetConfigWorkSheet(objectType);

          Oracle.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);
                    
          string identifier = dataObject.GetPropertyValue(cfWorksheet.Identifier).ToString();

          int row = GetRowIndex(xlWorksheet, cfWorksheet, identifier);

          foreach (OracleColumn column in cfWorksheet.Columns)
          {
            xlWorksheet.Cells[row, column.Index] = dataObject.GetPropertyValue(column.Name);
          }

          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
          xlWorksheet = null;          
        }  
        */

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PostList: " + ex);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception("Error while posting data objects of type [" + objectType + "].", ex);
      }
    }

    #region IDataLayer Members

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      return new List<IDataObject>();
    }

    #endregion
  }
}

