using System;
using System.Data;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using Ninject.Parameters;
using Ninject.Extensions.Xml;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;

namespace org.iringtools.sql
{
  public enum QueryType 
  {
    SELECT,
    UPDATE,
    DELETE,
    INSERT
  }
    
  [DataContract(Name = "configration")]
  public class Configuration
  {
    private IDbConnection _connetion = null;    

    ~Configuration()
    {
      if (_connetion != null)
        _connetion.Close();
    }

    [DataMember(Name = "provider", Order = 0)]
    public string Provider { get; set; }

    [DataMember(Name = "connection", Order = 1)]
    public string Connection { get; set; }

    [DataMember(Name = "objects", Order = 2)]
    public List<ConfigObject> ConfigObjects { get; set; }
        
    public IDbConnection GetConnection(StandardKernel kernel)
    {
      if (_connetion == null)      
      {
        if (this.Connection != null && !this.Connection.Equals(String.Empty))
        {
          _connetion = kernel.Get<IDbConnection>();        
          _connetion.ConnectionString = this.Connection;
        }
      }

      return _connetion;
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
      switch (type) {
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

  }

  [DataContract(Name = "object")]
  public class ConfigObject
  {
    private IDbConnection _connetion = null;

    ~ConfigObject() 
    {
      if (_connetion != null)
        _connetion.Close();
    }

    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "identifier", Order = 1)]
    public string Identifier { get; set; }

    [DataMember(Name = "connection", Order = 2)]
    public string Connection { get; set; }
        
    [DataMember(Name = "select", Order = 3)]
    public ConfigCommand Select { get; set; }

    [DataMember(Name = "update", Order = 4)]
    public ConfigCommand Update { get; set; }

    [DataMember(Name = "delete", Order = 5)]
    public ConfigCommand Delete { get; set; }

    [DataMember(Name = "insert", Order = 6)]
    public ConfigCommand Insert { get; set; }

    public IDbConnection GetConnection(StandardKernel kernel)
    {
      if (_connetion == null)
      {
        if (this.Connection != null && !this.Connection.Equals(String.Empty))
        {
          _connetion = kernel.Get<IDbConnection>();
          _connetion.ConnectionString = this.Connection;
        }
      }

      return _connetion;
    }

    public IDbCommand GetCommand(StandardKernel kernel, QueryType type, IDbConnection connection)
    {
      IDbCommand command = null;

      if (this.GetConnection(kernel) != null)
        connection = this.GetConnection(kernel);

      switch (type)
      {
        case QueryType.SELECT:
          {
            command = this.Select.GetCommand(kernel, connection);
            break;
          }
        case QueryType.UPDATE:
          {
            command = this.Update.GetCommand(kernel, connection);
            break;
          }
        case QueryType.DELETE:
          {
            command = this.Delete.GetCommand(kernel, connection);
            break;
          }
        case QueryType.INSERT:
          {
            command = this.Insert.GetCommand(kernel, connection);
            break;
          }
        
      }

      return command;
    }
  }

  [DataContract(Name = "command")]
  public class ConfigCommand
  {
    private IDbConnection _connetion = null;
    private IDbCommand _command = null;
    
    ~ConfigCommand()
    {
      if (_connetion != null)
        _connetion.Close();
    }

    [DataMember(Name = "connection", Order = 0)]
    public string Connection { get; set; }

    [DataMember(Name = "query", Order = 1)]
    public string Query { get; set; }

    [DataMember(Name = "parameters", Order = 2)]
    public List<ConfigParameter> Parameters { get; set; }

    public Configuration Configuration { get; set; }

    public IDbConnection GetConnection(StandardKernel kernel)
    {
      if (_connetion == null)
      {
        if (this.Connection != null && !this.Connection.Equals(String.Empty))
          _connetion = kernel.Get<IDbConnection>(new Ninject.Parameters.ConstructorArgument("connectionString", this.Connection));
      }

      return _connetion;
    }

    public IDbCommand GetCommand(StandardKernel kernel, IDbConnection connection)
    {
      if (_command == null)
      {
        if (this.GetConnection(kernel) != null)
          connection = this.GetConnection(kernel);

        if (this.Query != null && !this.Query.Equals(String.Empty))
        {           
          _command = kernel.Get<IDbCommand>();
          _command.Connection = connection;
          _command.CommandText = this.Query;          
          
          if (this.Parameters != null)
          {
            foreach (ConfigParameter parameter in this.Parameters)
            {
              IDataParameter dataParameter = kernel.Get<IDataParameter>();
              dataParameter.ParameterName = parameter.Name;
              dataParameter.DbType = Configuration.GetDBType(parameter.DataType);
              _command.Parameters.Add(parameter);                            
            }
          }
        }

      }

      return _command;
    }
  }

  [DataContract(Name = "parameter")]
  public class ConfigParameter
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "type", Order = 1)]
    public DataType DataType { get; set; }
  }

}
