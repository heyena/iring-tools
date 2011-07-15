using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using Ninject.Planning.Bindings;
using System.Data.SqlClient;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using Ninject.Parameters;
using Ninject.Modules;
using Ciloci.Flee;
using System.Text;

namespace org.iringtools.adapter.datalayer
{
  public class ADONetModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IDbConnection>().ToMethod(c => new SqlConnection());
      Bind<IDbCommand>().ToMethod(c => new SqlCommand());
      Bind<IDbDataParameter>().ToMethod(c => new SqlParameter());
    }
  }

  public class ADONetDataLayer : BaseDataLayer, IDataLayer2
  { 
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ADONetDataLayer));
    private string _configPath = String.Empty;
    private string _bindingPath = String.Empty;
    private string _dictionaryPath = String.Empty;
    private ADONetConfiguration _config = null;
    private DataDictionary _dictionary = null;
    private Dictionary<string, Type> _dynamicTypes = new Dictionary<string, Type>();
    
    [Inject]
    public ADONetDataLayer(AdapterSettings settings)
      : base(settings)
    {
      try
      {
        _settings = settings;

        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings["XmlPath"], "adonet-configuration." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml");
        _config = Utility.Read<ADONetConfiguration>(_configPath, true);

        _dictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings["XmlPath"], "DataDictionary." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml");
        _bindingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings["XmlPath"], "adonet-binding." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml");

      }
      catch (Exception e)
      {
        _logger.Error("Error in processing the Configuration File [" + _configPath + "].", e);
      }
    }

    ~ADONetDataLayer()
    {
      _config = null;
    }

    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        ConfigObject configObject = GetConfigObject(objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          string[] delimiters = { configObject.Identifier.Delimiter };
          string[] ids = configObject.Identifier.Key.Split(delimiters, StringSplitOptions.None);

          foreach (string identifier in identifiers)
          {
            IDataObject dataObject = new GenericDataObject()
            {
              ObjectType = objectType
            };

            if (!String.IsNullOrEmpty(identifier))
            {
              string[] vls = identifier.Split(delimiters, StringSplitOptions.None);

              for (int i = 0; i < vls.Length; i++)
              {
                dataObject.SetPropertyValue(ids[i], vls[i]);
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

    public override Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        IList<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);
        ConfigObject configObject = GetConfigObject(objectType);

        string[] delimiters = { configObject.Identifier.Delimiter };
        string[] ids = configObject.Identifier.Key.Split(delimiters, StringSplitOptions.None);

        foreach (IDataObject dataObject in dataObjects)
        {
          List<string> vls = new List<string>();
          for (int i = 0; i < ids.Length; i++)
          {
            vls.Add((string)dataObject.GetPropertyValue(ids[i]));
          }
          identifiers.Add(string.Join(delimiters[0], vls));
        }

        return Delete(objectType, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
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
        IList<IDataObject> dataObjects = Get(objectType, identifiers);

        ConfigObject configObject = GetConfigObject(objectType);

        IDbCommand deleteCmd = configObject.GetCommand(QueryType.DELETE, _config.GetConnection());
        
        foreach (var dataObject in dataObjects)
        {
          foreach (var configParam in configObject.Delete.Parameters)
          {
            IDbDataParameter param = (IDbDataParameter)deleteCmd.Parameters[configParam.Name];
            param.Value = dataObject.GetPropertyValue(configParam.Property);
          }

          var ret = deleteCmd.ExecuteNonQuery();
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in DeletList: " + ex);
        throw new Exception("Error while delete a list of data objects of type [" + objectType + "].", ex);
      }      
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {      
      try
      {        
        ConfigObject configObject = GetConfigObject(objectType);
        IDbCommand selectCmd = configObject.GetCommand(QueryType.SELECT, _config.GetConnection());
                                
        List<IDataObject> dataObjects = new List<IDataObject>();

        if (selectCmd.Connection.State != ConnectionState.Open)
          selectCmd.Connection.Open();        

        IDataReader reader = selectCmd.ExecuteReader();

        while (reader.Read())
        {
          IDataObject dataObject = new GenericDataObject()
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
            context.Variables.DefineVariable(variable, typeof(GenericDataObject));

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

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        DataObject dictionaryObject = _dictionary.dataObjects.Where(o => o.objectName.Equals(objectType)).SingleOrDefault();
        ConfigObject configObject = GetConfigObject(objectType);
        string[] delimiters = { configObject.Identifier.Delimiter };
        string[] ids = configObject.Identifier.Key.Split(delimiters, StringSplitOptions.None);

        IDbCommand selectCmd = configObject.GetCommand(QueryType.SELECT, _config.GetConnection());

        using (IDbCommand tempCmd = ServiceLocator.Get<IDbCommand>())
        {
          tempCmd.CommandText = selectCmd.CommandText;
          tempCmd.Connection = selectCmd.Connection;

          if (identifiers != null && identifiers.Count > 0)
          {
            #region WhereSql

            //select * from table
            //select * from table where column = value

            //id1 in (v11, v12, v13)
            //id2 in (v21, v22, v23)

            Dictionary<string, List<string>> wheres = new Dictionary<string, List<string>>();
            for (int i = 0; i < ids.Length; i++)
              wheres.Add(ids[i], new List<string>());

            foreach (var identifier in identifiers)
            {
              string[] vls = identifier.Split(delimiters, StringSplitOptions.None);

              for (int i = 0; i < ids.Length; i++)
              {
                wheres[ids[i]].Add(vls[i]);
              }
            }

            string whereSql = "";

            for (int i = 0; i < wheres.Count; i++)
            {
              var where = wheres.ElementAt(i);
              whereSql += where.Key + " in ('" + string.Join("','", where.Value) + "')";
              if (i < wheres.Count - 1)
              {
                whereSql += " and ";
              }
            }

            #endregion WhereSql

            string selectSql = string.Format("select * from ({0}) as tmp where {1}", selectCmd.CommandText, whereSql);
            tempCmd.CommandText = selectSql;
          }

          if (tempCmd.Connection.State != ConnectionState.Open)
            tempCmd.Connection.Open();

          using (IDataReader reader = tempCmd.ExecuteReader())
          {
            while (reader.Read())
            {
              IDataObject dataObject = new GenericDataObject()
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

    public override DataDictionary GetDictionary()
    {
      try
      {
        if (File.Exists(_dictionaryPath))
        {
          _dictionary = Utility.Read<DataDictionary>(_dictionaryPath, true);
        }
        else
        {
          _dictionary = new DataDictionary()
          {
            dataObjects = new List<DataObject>()
          };

          foreach (ConfigObject configObject in _config.ConfigObjects)
          {
            DataObject dataObject = new DataObject()
            {
              tableName = configObject.Name,
              objectName = configObject.Name,
              dataProperties = new List<DataProperty>()
            };

            string[] delimiters = { configObject.Identifier.Delimiter };
            string[] ids = configObject.Identifier.Key.Split(delimiters, StringSplitOptions.None);

            _dictionary.dataObjects.Add(dataObject);

            IDbCommand selectCmd = configObject.GetCommand(QueryType.SELECT, _config.GetConnection());

            if (selectCmd.Connection.State != ConnectionState.Open)
              selectCmd.Connection.Open();

            using (IDataReader reader = selectCmd.ExecuteReader())
            {


              DataTable table = reader.GetSchemaTable();
              foreach (DataRow row in table.Rows)
              {
                string columnName = row["ColumnName"].ToString();
                DataType columnDataType = ADONetConfiguration.GetDataType((Type)row["DataType"]);

                DataProperty dataProperty = new DataProperty()
                {
                  columnName = columnName,
                  propertyName = columnName,
                  dataType = columnDataType,
                  dataLength = Convert.ToInt32(row["ColumnSize"].ToString())
                };

                if (ids.Contains(columnName))
                {
                  dataObject.addKeyProperty(dataProperty);
                }
                else
                {
                  dataObject.dataProperties.Add(dataProperty);
                }

              }
            }

          }

          Utility.Write<DataDictionary>(_dictionary, _dictionaryPath, true);
        }
        
        return _dictionary;
      }
      catch (Exception e)
      {
        throw new Exception("Error while creating dictionary.", e);
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        List<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);
        ConfigObject configObject = GetConfigObject(objectType);

        string[] delimiters = { configObject.Identifier.Delimiter };
        string[] ids = configObject.Identifier.Key.Split(delimiters, StringSplitOptions.None);

        foreach (IDataObject dataObject in dataObjects)
        {
          List<string> vls = new List<string>();
          for (int i = 0; i < ids.Length; i++)
          {
            vls.Add((string)dataObject.GetPropertyValue(ids[i]));
          }
          identifiers.Add(string.Join(delimiters[0], vls));
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

    public override Response Post(IList<IDataObject> dataObjects)
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
        
    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      //var genericObject = (GenericDataObject)dataObject;
      //genericObject.ObjectType

      return new List<IDataObject>();
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        DataObject dictionaryObject = _dictionary.dataObjects.Where(o => o.objectName.Equals(objectType)).SingleOrDefault();
        ConfigObject configObject = GetConfigObject(objectType);

        IDbCommand selectCmd = configObject.GetCommand(QueryType.SELECT, _config.GetConnection());

        string queryText = selectCmd.CommandText;
        int fromIdx = queryText.IndexOf("from", 1);
        string fromText = queryText.Substring(fromIdx);

        queryText = "select count(*) as value " + fromText;
        selectCmd.CommandText = queryText;

        long count = 0;
        long.TryParse(selectCmd.ExecuteScalar().ToString(), out count);

        return count;

      }
      catch (Exception e)
      {
        return 0;
      }
    }
  }
}

