using System;
using System.Data;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;

namespace org.iringtools.adapter.datalayer
{
  public enum QueryType 
  {
    SELECT,
    UPDATE,
    DELETE,
    INSERT
  }
    
  [DataContract(Name = "configuration")]
  public class ADONetConfiguration : IDisposable
  {
    private IDbConnection _connection = null;

    ~ADONetConfiguration()
    {
      if (_connection != null)
      {
        if (_connection.State != ConnectionState.Closed)
        {
          _connection.Close();
          _connection.Dispose();
        }
      }
    }
     
    [DataMember(Name = "connection", Order = 0)]
    public string Connection { get; set; }

    [DataMember(Name = "objects", Order = 1)]
    public List<ConfigObject> ConfigObjects { get; set; }
        
    public IDbConnection GetConnection()
    {
      if (_connection == null)
      {
        if (this.Connection != null && !this.Connection.Equals(String.Empty))
        {
          _connection = ServiceLocator.Get<IDbConnection>();
          _connection.ConnectionString = this.Connection;
        }
      }

      return _connection;
    }

    public static DataType GetDataType(Type type)
    {      
      if (type == typeof(System.Boolean))
      {
        return DataType.Boolean;
      }
      else if (type == typeof(System.Byte))
      {
        return DataType.Byte;
      }
      else if (type == typeof(System.Char))
      {
        return DataType.Char;
      }
      else if (type == typeof(System.DateTime))
      {
        return DataType.DateTime;
      }
      else if (type == typeof(System.Decimal))
      {
        return DataType.Decimal;
      }
      else if (type == typeof(System.Double))
      {
        return DataType.Double;
      }
      else if (type == typeof(System.Int16))
      {
        return DataType.Int16;
      }
      else if (type == typeof(System.Int32))
      {
        return DataType.Int32;
      }
      else if (type == typeof(System.Int64))
      {
        return DataType.Int64;
      }
      else if (type == typeof(System.Single))
      {
        return DataType.Single;
      }
      else if (type == typeof(System.String))
      {
        return DataType.String;
      }
      else if (type == typeof(System.TimeSpan))
      {
        return DataType.TimeSpan;
      }
      else
      {
        return DataType.String;
      }
    }

    public static DbType GetDBType(DataType type)
    {
      switch (type)
      {
        case DataType.Boolean:        
          return DbType.Boolean;     
        case DataType.Byte:       
          return DbType.Byte;      
        case DataType.Char:      
          return DbType.Byte;      
        case DataType.DateTime:
          return DbType.DateTime;      
        case DataType.Decimal:
          return DbType.Decimal;
        case DataType.Double:
          return DbType.Double;
        case DataType.Int16:       
          return DbType.Int16;
        case DataType.Int32:
          return DbType.Int32;
        case DataType.Int64:
          return DbType.Int64;      
        case DataType.Single:
          return DbType.Single;
        case DataType.String: 
          return DbType.String;
        case DataType.TimeSpan:
          return DbType.DateTime2;
        default:
          return DbType.String;
      }
    }

    public void Dispose()
    {
      if (_connection != null)
      {
        if (_connection.State != ConnectionState.Closed)
        {
          _connection.Close();
          _connection.Dispose();
        }
      }
    }
  }

  [DataContract(Name = "column")]
  public class RelatedColumn
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "related", Order = 1)]
    public string Related { get; set; }
  }

  [DataContract(Name = "relatedObject")]
  public class RelatedObject
  {
    [DataMember(Name = "relatedObjectName", Order = 0, IsRequired = true)]
    public string Name { get; set; }

    [DataMember(Name = "columns", Order = 1)]
    public List<RelatedColumn> Columns { get; set; }
  }

  [DataContract(Name = "identifier")]
  public class Identifier
  {
    [DataMember(Name = "key", Order = 0, IsRequired = true)]
    public string Key { get; set; }

    [DataMember(Name = "delimiter", Order = 1)]
    public string Delimiter { get; set; }
  }

  [DataContract(Name = "object")]
  public class ConfigObject : IDisposable
  {
    private IDbConnection _connection = null;

    ~ConfigObject() 
    {
      if (_connection != null)
      {
        if (_connection.State != ConnectionState.Closed)
        {
          _connection.Close();
          _connection.Dispose();
        }
      }
    }

    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "identifier", Order = 1, IsRequired = true)]
    public Identifier Identifier { get; set; }

    [DataMember(Name = "connection", Order = 2, EmitDefaultValue = false)]
    public string Connection { get; set; }
        
    [DataMember(Name = "select", Order = 3, EmitDefaultValue = false)]
    public ConfigCommand Select { get; set; }

    [DataMember(Name = "update", Order = 4, EmitDefaultValue = false)]
    public ConfigCommand Update { get; set; }

    [DataMember(Name = "delete", Order = 5, EmitDefaultValue = false)]
    public ConfigCommand Delete { get; set; }

    [DataMember(Name = "insert", Order = 6, EmitDefaultValue = false)]
    public ConfigCommand Insert { get; set; }

    [DataMember(Name = "relatedObjects", Order = 7, EmitDefaultValue = false)]
    public List<RelatedObject> RelatedObjects { get; set; }

    public IDbConnection GetConnection(IDbConnection connection)
    {
      if (_connection == null)
      {
        if (!string.IsNullOrEmpty(this.Connection))
        {
          _connection = ServiceLocator.Get<IDbConnection>();
          _connection.ConnectionString = this.Connection;
        }
        else
        {
          _connection = connection;
        }
      }

      return _connection;
    }

    public IDbCommand GetCommand(QueryType type, IDbConnection connection)
    {
      IDbCommand command = null;
      
      switch (type)
      {
        case QueryType.SELECT:
          {
            command = this.Select.GetCommand(this.GetConnection(connection));
            break;
          }
        case QueryType.UPDATE:
          {
            command = this.Update.GetCommand(this.GetConnection(connection));
            break;
          }
        case QueryType.DELETE:
          {
            command = this.Delete.GetCommand(this.GetConnection(connection));
            break;
          }
        case QueryType.INSERT:
          {
            command = this.Insert.GetCommand(this.GetConnection(connection));
            break;
          }
      }

      return command;
    }

    public void Dispose()
    {
      if (_connection != null)
      {
        if (_connection.State != ConnectionState.Closed)
        {
          _connection.Close();
          _connection.Dispose();
        }
      }
    }
  }

  [DataContract(Name = "command")]
  public class ConfigCommand : IDisposable
  {
    private IDbConnection _connection = null;
    private IDbCommand _command = null;
    
    ~ConfigCommand()
    {
      if (_connection != null)
      {
        if (_connection.State != ConnectionState.Closed)
        {
          _connection.Close();
          _connection.Dispose();
        }
      }
    }

    [DataMember(Name = "connection", Order = 0, EmitDefaultValue = false)]
    public string Connection { get; set; }

    [DataMember(Name = "query", Order = 1)]
    public string Query { get; set; }

    [DataMember(Name = "parameters", Order = 2, EmitDefaultValue = false)]
    public List<ConfigParameter> Parameters { get; set; }

    public ADONetConfiguration Configuration { get; set; }

    public IDbConnection GetConnection(IDbConnection connection)
    {
      if (_connection == null)
      {
        if (this.Connection != null && !this.Connection.Equals(String.Empty))
        {
          _connection = ServiceLocator.Get<IDbConnection>();
          _connection.ConnectionString = this.Connection;
        } else 
        {
          _connection = connection;
        }
      }

      return _connection;
    }

    public IDbCommand GetCommand(IDbConnection connection)
    {
      if (_command == null)
      {
        if (!string.IsNullOrEmpty(this.Query))
        {
          _command = ServiceLocator.Get<IDbCommand>();
          _command.Connection = this.GetConnection(connection);
          _command.CommandText = this.Query;

          if (this.Parameters != null)
          {
            foreach (ConfigParameter parameter in this.Parameters)
            {
              IDataParameter dataParameter = ServiceLocator.Get<IDataParameter>();
              dataParameter.ParameterName = parameter.Name;
              dataParameter.DbType = ADONetConfiguration.GetDBType(parameter.DataType);
              _command.Parameters.Add(parameter);
            }
          }
        }
      }

      return _command;
    }

    public void Dispose()
    {
      if (_command != null)
      {
        _command.Dispose();
      }

      if (_connection != null)
      {
        if (_connection.State != ConnectionState.Closed)
        {
          _connection.Close();
          _connection.Dispose();
        }
      }
    }
  }

  [DataContract(Name = "parameter")]
  public class ConfigParameter
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "type", Order = 1)]
    public DataType DataType { get; set; }

    [DataMember(Name = "property", Order = 2)]
    public string Property { get; set; }
  }

}
