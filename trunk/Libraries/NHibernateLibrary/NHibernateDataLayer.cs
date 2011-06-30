using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.nhibernate;

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer : BaseConfigurableDataLayer, IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer));
    private string _dataDictionaryPath = String.Empty;
    private string _databaseDictionaryPath = string.Empty;
    private DataDictionary _dataDictionary;
    private DatabaseDictionary _databaseDictionary;
    private IDictionary _keyRing = null;
    private ISessionFactory _sessionFactory;
    private string _hibernateConfigPath = string.Empty;
    private string _authorizationPath = string.Empty;
    private Response _response = null;
    private IKernel _kernel = null;
    private NHibernateSettings _nSettings = null;
    //private WebProxyCredentials _proxyCredentials = null;

    bool _isScopeInitialized = false;

    AdapterProvider _adapterProvider = null;

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings, IDictionary keyRing)
    {
      _kernel = new StandardKernel(new NHibernateModule());
      _nSettings = _kernel.Get<NHibernateSettings>();
      _nSettings.AppendSettings(settings);
      _settings = settings;
      _keyRing = keyRing;
      _response = new Response();
      _kernel.Bind<Response>().ToConstant(_response);

      _adapterProvider = new AdapterProvider(_settings);

      _hibernateConfigPath = string.Format("{0}nh-configuration.{1}.xml",
        _settings["XmlPath"],
        _settings["Scope"]
      );

      string hibernateMappingPath = string.Format("{0}nh-mapping.{1}.xml",
        _settings["XmlPath"],
        _settings["Scope"]
      );

      _dataDictionaryPath = string.Format("{0}DataDictionary.{1}.xml",
        _settings["XmlPath"],
        _settings["Scope"]
      );

      string _databaseDictionaryPath = string.Format("{0}DatabaseDictionary.{1}.xml",
        _settings["XmlPath"],
        _settings["Scope"]
      );

      if (File.Exists(_databaseDictionaryPath))
        _databaseDictionary = Utility.Read<DatabaseDictionary>(_databaseDictionaryPath);

      if (File.Exists(_hibernateConfigPath) && File.Exists(hibernateMappingPath))
        _sessionFactory = new Configuration()
          .Configure(_hibernateConfigPath)
          .AddFile(hibernateMappingPath)
          .BuildSessionFactory();

      _authorizationPath = string.Format("{0}Authorization.{1}.xml",
        _settings["DataPath"],
        _settings["Scope"]
      );
    }

    #region public methods
    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        DataObject dictionaryObject = _dataDictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());
        Type type = Type.GetType(dictionaryObject.objectNamespace + "." + objectType + ", " + _settings["ExecutingAssemblyName"]);
        IDataObject dataObject = null;

        if (identifiers != null)
        {
          ISession session = OpenSession();

          foreach (string identifier in identifiers)
          {
            if (!String.IsNullOrEmpty(identifier))
            {
              IQuery query = session.CreateQuery("from " + objectType + " where Id = ?");
              query.SetString(0, identifier);
              dataObject = query.List<IDataObject>().FirstOrDefault<IDataObject>();

              if (dataObject == null)
              {
                dataObject = (IDataObject)Activator.CreateInstance(type);
                dataObject.SetPropertyValue("Id", identifier);
              }
            }
            else
            {
              dataObject = (IDataObject)Activator.CreateInstance(type);
            }

            dataObjects.Add(dataObject);
          }
        }
        else
        {
          dataObject = (IDataObject)Activator.CreateInstance(type);
          dataObjects.Add(dataObject);
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in CreateList: " + ex);
        throw new Exception(string.Format("Error while creating a list of data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];

          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select count(*) from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          filter.OrderExpressions.Clear();
          string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectType, null);
          queryString.Append(whereClause);
        }

        using (ISession session = OpenSession())
        {
          IQuery query = session.CreateQuery(queryString.ToString());

          long count = query.List<long>().First();

          return count;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        StringBuilder queryString = new StringBuilder();
        queryString.Append("select Id from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectType, null);
          queryString.Append(whereClause);
        }

        using (ISession session = OpenSession())
        {
          IQuery query = session.CreateQuery(queryString.ToString());
          return query.List<string>();
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          queryString.Append(" where Id in ('" + String.Join("','", identifiers.ToArray()) + "')");
        }

        using (ISession session = OpenSession())
        {
          IQuery query = session.CreateQuery(queryString.ToString());
          IList<IDataObject> dataObjects = query.List<IDataObject>();

          // order data objects as list of identifiers
          if (identifiers != null)
          {
            IList<IDataObject> orderedDataObjects = new List<IDataObject>();

            foreach (string identifier in identifiers)
            {
              foreach (IDataObject dataObject in dataObjects)
              {
                if (identifier != null)
                  if (dataObject.GetPropertyValue("Id").ToString().ToLower() == identifier.ToLower())
                  {
                    orderedDataObjects.Add(dataObject);
                    break;
                  }
              }
            }

            return orderedDataObjects;
          }

          return dataObjects;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (filter != null && ((filter.Expressions != null && filter.Expressions.Count > 0) ||
          (filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)))
        {
          string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectType, null);
          queryString.Append(whereClause);
        }

        using (ISession session = OpenSession())
        {
          IQuery query = session.CreateQuery(queryString.ToString());
          IList<IDataObject> dataObjects = query.List<IDataObject>();

          if (pageSize == 0)
          {
            dataObjects = dataObjects.ToList();
          }
          else if (startIndex + pageSize <= dataObjects.Count)
          {
            dataObjects = dataObjects.ToList().GetRange(startIndex, pageSize);
          }
          else if (startIndex <= dataObjects.Count)
          {
            int rowsRemaining = dataObjects.Count - startIndex;

            dataObjects = dataObjects.ToList().GetRange(startIndex, rowsRemaining);
          }
          else
          {
            dataObjects = new List<IDataObject>();
          }

          return dataObjects;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    private DataFilter FilterByIdentity(string objectType, DataFilter filter, IdentityProperties identityProperties)
    {
      DataObject dataObject = _databaseDictionary.dataObjects.Find(d => d.objectName == objectType);
      DataProperty dataProperty = dataObject.dataProperties.Find(p => p.columnName == identityProperties.IdentityProperty);
      if (dataProperty != null)
      {
        if (filter == null)
        {
          filter = new DataFilter();
        }
        bool hasExistingExpression = false;
        if (filter.Expressions == null)
        {
          filter.Expressions = new List<Expression>();
        }
        else if (filter.Expressions.Count > 0)
        {
          Expression firstExpression = filter.Expressions.First();
          Expression lastExpression = filter.Expressions.Last();
          firstExpression.OpenGroupCount++;
          lastExpression.CloseGroupCount++;
          hasExistingExpression = true;
        }
        string identityValue = _keyRing[identityProperties.KeyRingProperty].ToString();
        Expression expression = new Expression
        {
          PropertyName = dataProperty.propertyName,
          RelationalOperator = RelationalOperator.EqualTo,
          Values = new Values
          {
            identityValue,
          },
          IsCaseSensitive = identityProperties.IsCaseSensitive
        };
        if (hasExistingExpression)
          expression.LogicalOperator = LogicalOperator.And;
        filter.Expressions.Add(expression);
      }
      return filter;
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      Response response = new Response();

      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          using (ISession session = OpenSession())
          {
            foreach (IDataObject dataObject in dataObjects)
            {
              Status status = new Status();
              status.Messages = new Messages();

              if (dataObject != null)
              {
                string identifier = String.Empty;

                try
                {
                  // NOTE: Id property is not available if it's not mapped and will cause exception
                  identifier = dataObject.GetPropertyValue("Id").ToString();
                }
                catch (Exception ex)
                {
                  _logger.Error(string.Format("Error in Post: {0}", ex));
                }  // no need to handle exception because identifier is only used for statusing

                status.Identifier = identifier;

                try
                {
                  session.SaveOrUpdate(dataObject);
                  session.Flush();
                  status.Messages.Add(string.Format("Record [{0}] have been saved successfully.", identifier));
                }
                catch (Exception ex)
                {
                  status.Level = StatusLevel.Error;
                  status.Messages.Add(string.Format("Error while posting record [{0}]. {1}", identifier, ex));
                  status.Results.Add("ResultTag", identifier);
                  _logger.Error("Error in Post saving: " + ex);
                }
              }
              else
              {
                status.Level = StatusLevel.Error;
                status.Messages.Add("Data object is null or duplicate.");
              }

              response.Append(status);
            }
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception(string.Format("Error while posting data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      Response response = new Response();

      try
      {
        IList<IDataObject> dataObjects = Create(objectType, identifiers);

        using (ISession session = OpenSession())
        {
          foreach (IDataObject dataObject in dataObjects)
          {
            string identifier = dataObject.GetPropertyValue("Id").ToString();
            session.Delete(dataObject);

            Status status = new Status();
            status.Messages = new Messages();
            status.Identifier = identifier;
            status.Messages.Add(string.Format("Record [{0}] have been deleted successfully.", identifier));

            response.Append(status);
          }

          session.Flush();
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        response.Append(status);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      Response response = new Response();
      response.StatusList = new List<Status>();
      Status status = new Status();

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        status.Identifier = objectType;

        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (filter.Expressions.Count > 0)
        {
          string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectType, null);
          queryString.Append(whereClause);
        }

        using (ISession session = OpenSession())
        {
          session.Delete(queryString.ToString());
          session.Flush();
          status.Messages.Add(string.Format("Records of type [{0}] has been deleted succesfully.", objectType));
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));

        //no need to status, thrown exception will be statused above.
      }

      response.Append(status);
      return response;
    }

    public override DataDictionary GetDictionary()
    {
      if (File.Exists(_dataDictionaryPath))
      {
        _dataDictionary = Utility.Read<DataDictionary>(_dataDictionaryPath);
        return _dataDictionary;
      }
      else
      {
        return new DataDictionary();
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject sourceDataObject, string relatedObjectType)
    {
      if (!isAuthorized())
        throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");

      IList<IDataObject> relatedObjects = null;
      DataDictionary dictionary = GetDictionary();
      DataObject dataObject = dictionary.dataObjects.First(c => c.objectName == sourceDataObject.GetType().Name);
      DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relatedObjectName == relatedObjectType);

      StringBuilder sql = new StringBuilder();
      sql.Append("from " + dataRelationship.relatedObjectName + " where ");

      foreach (PropertyMap map in dataRelationship.propertyMaps)
      {
        DataProperty propertyMap = dataObject.dataProperties.First(c => c.propertyName == map.dataPropertyName);

        if (propertyMap.dataType == DataType.String)
        {
          sql.Append(map.relatedPropertyName + " = '" + sourceDataObject.GetPropertyValue(map.dataPropertyName) + "' and ");
        }
        else
        {
          sql.Append(map.relatedPropertyName + " = " + sourceDataObject.GetPropertyValue(map.dataPropertyName) + " and ");
        }
      }

      sql.Remove(sql.Length - 4, 4);  // remove the tail "and "

      using (ISession session = OpenSession())
      {
        IQuery query = session.CreateQuery(sql.ToString());
        relatedObjects = query.List<IDataObject>();
      }

      if (relatedObjects != null && relatedObjects.Count > 0 && dataRelationship.relationshipType == RelationshipType.OneToOne)
      {
        return new List<IDataObject> { relatedObjects.First() };
      }

      return relatedObjects;
    }

    public XElement GetConfiguration(string connectionInfo)
    {
      DatabaseDictionary databaseDictionary = null;

      try
      {
        databaseDictionary = new DatabaseDictionary
        {
          ConnectionString = connectionInfo
        };
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetConfiguration: " + ex);
      }

      return Utility.SerializeToXElement<DatabaseDictionary>(databaseDictionary);
    }

    public Response SaveConfiguration(XElement configuration)
    {
      Response _response = new Response();
      _response.Messages = new Messages();
      string _projectName = _settings["Scope"].Split('.')[0];
      string _applicationName = _settings["Scope"].Split('.')[1];
      try
      {
        //_databaseDictionary = Utility.DeserializeDataContract<DatabaseDictionary>(configuration.Nodes().First().ToString());
        _databaseDictionary = Utility.DeserializeFromXElement<DatabaseDictionary>(configuration);
        Utility.Write<DatabaseDictionary>(_databaseDictionary, _hibernateConfigPath, true);
        _response = Generate(_projectName, _applicationName);
        _response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _response.Messages.Add("Failed to Save datalayer Configuration");
        _response.Messages.Add(ex.Message);
        _response.Level = StatusLevel.Error;
        _logger.Error("Error in SaveConfiguration: " + ex);
      }
      return _response;
    }

    public Response Generate(string projectName, string applicationName)
    {
      Status status = new Status();

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        DatabaseDictionary dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(applicationName))
        {
          status.Messages.Add("Error project name and application name can not be null");
        }
        else if (ValidateDatabaseDictionary(dbDictionary))
        {
          EntityGenerator generator = _kernel.Get<EntityGenerator>();
          _response.Append(generator.Generate(dbDictionary, projectName, applicationName));

          // Update binding configuration
          XElement binding = new XElement("module",
            new XAttribute("name", _settings["Scope"]),
            new XElement("bind",
              new XAttribute("name", "DataLayer"),
              new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
              new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary")
            )
          );

          Response localResponse = _adapterProvider.UpdateBinding(projectName, applicationName, binding);

          _response.Append(localResponse);

          status.Messages.Add("Database dictionary updated successfully.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateDatabaseDictionary: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error updating database dictionary: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    public DatabaseDictionary GetDictionary(string projectName, string applicationName)
    {
      DatabaseDictionary databaseDictionary = new DatabaseDictionary();
      try
      {
        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
        {
          databaseDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        }
        else
        {
          databaseDictionary = new DatabaseDictionary();
          Utility.Write<DatabaseDictionary>(databaseDictionary, _settings["DBDictionaryPath"], true);
        }

      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetDbDictionary: " + ex);
        return null;
      }
      return databaseDictionary;
    }

    public Response PostDictionary(string projectName, string applicationName, DatabaseDictionary databaseDictionary)
    {
      Status status = new Status();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        Utility.Write<DatabaseDictionary>(databaseDictionary, _settings["DBDictionaryPath"]);

        status.Messages.Add("Database Dictionary saved successfully");
      }
      catch (Exception ex)
      {
        _logger.Error("Error in SaveDatabaseDictionary: " + ex);
        status.Messages.Add("Error in saving database dictionary" + ex.Message);
      }

      _response.Append(status);
      return _response;
    }

    public DatabaseDictionary GetDatabaseSchema(string projectName, string applicationName, string schemaName)
    {
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      try
      {

        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        else
        {
          Utility.Write<DatabaseDictionary>(dbDictionary, _settings["DBDictionaryPath"], true);
          return dbDictionary;
        }
        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString();
        dbProvider = dbProvider.ToUpper();
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        dbDictionary = new DatabaseDictionary();
        Dictionary<string, string> properties = new Dictionary<string, string>();
        string metadataQuery = string.Empty;
        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);

        if (dbProvider.Contains("MSSQL"))
        {
          metadataQuery =
              "select t1.table_name, t1.column_name, t1.data_type, t2.max_length, t2.is_identity, t2.is_nullable, t5.constraint_type " +
              "from information_schema.columns t1 " +
              "inner join sys.columns t2 on t2.name = t1.column_name " +
              "inner join sys.tables t3 on t3.name = t1.table_name and t3.object_id = t2.object_id " +
              "left join information_schema.key_column_usage t4 on t4.table_name = t1.table_name and t4.column_name = t1.column_name " +
              "left join information_schema.table_constraints t5 on t5.constraint_name = t4.constraint_name " +
              "where t1.data_type not in ('image') " +
              "order by t1.table_name, t5.constraint_type, t1.column_name";// +
          properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");

          switch (dbProvider)
          {
            case "MSSQL2008":
              dbDictionary.Provider = Provider.MsSql2008.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MsSql2008Dialect");
              break;

            case "MSSQL2005":
              dbDictionary.Provider = Provider.MsSql2005.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");
              break;

            case "MSSQL2000":
              dbDictionary.Provider = Provider.MsSql2000.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MsSql2000Dialect");
              break;

            default:
              throw new Exception("Database provider not supported.");
          }
        }
        else if (dbProvider.Contains("ORACLE"))
        {
          metadataQuery = string.Format(
            "select t1.object_name, t2.column_name, t2.data_type, t2.data_length, 0 as is_sequence, t2.nullable, t4.constraint_type " +
            "from all_objects t1 " +
            "inner join all_tab_cols t2 on t2.table_name = t1.object_name " +
            "left join all_cons_columns t3 on t3.table_name = t2.table_name and t3.column_name = t2.column_name " +
            "left join all_constraints t4 on t4.constraint_name = t3.constraint_name and (t4.constraint_type = 'P' or t4.constraint_type = 'R') " +
            "where t1.object_type = 'TABLE' and (t1.owner = '{0}') order by t1.object_name, t4.constraint_type, t2.column_name", schemaName);

          properties.Add("connection.driver_class", "NHibernate.Driver.OracleClientDriver");

          switch (dbProvider)
          {
            case "ORACLE10G":
              dbDictionary.Provider = Provider.Oracle10g.ToString();
              properties.Add("dialect", "NHibernate.Dialect.Oracle10gDialect");
              break;

            case "ORACLE9I":
              dbDictionary.Provider = Provider.Oracle9i.ToString();
              properties.Add("dialect", "NHibernate.Dialect.Oracle9iDialect");
              break;

            case "ORACLE8I":
              dbDictionary.Provider = Provider.Oracle8i.ToString();
              properties.Add("dialect", "NHibernate.Dialect.Oracle8iDialect");
              break;

            case "ORACLELITE":
              dbDictionary.Provider = Provider.OracleLite.ToString();
              properties.Add("dialect", "NHibernate.Dialect.OracleLiteDialect");
              break;

            default:
              throw new Exception("Database provider not supported.");
          }
        }
        else if (dbProvider.Contains("MYSQL"))
        {
          metadataQuery = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, COLUMN_KEY, IS_NULLABLE " +
                          "FROM INFORMATION_SCHEMA.COLUMNS " +
                          string.Format("WHERE TABLE_SCHEMA = '{0}'", connString.Split(';')[1].Split('=')[1]);
          properties.Add("connection.driver_class", "NHibernate.Driver.MySqlDataDriver");

          switch (dbProvider)
          {
            case "MYSQL3":
              dbDictionary.Provider = Provider.MySql3.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
              break;
            case "MYSQL4":
              dbDictionary.Provider = Provider.MySql4.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
              break;
            case "MYSQL5":
              dbDictionary.Provider = Provider.MySql5.ToString();
              properties.Add("dialect", "NHibernate.Dialect.MySQL5Dialect");
              break;
          }
        }


        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        config.AddProperties(properties);

        ISessionFactory sessionFactory = config.BuildSessionFactory();
        ISession session = sessionFactory.OpenSession();
        ISQLQuery query = session.CreateSQLQuery(metadataQuery);
        IList<object[]> metadataList = query.List<object[]>();
        session.Close();

        DataObject table = null;
        string prevTableName = String.Empty;
        foreach (object[] metadata in metadataList)
        {
          string tableName = Convert.ToString(metadata[0]);
          string columnName = Convert.ToString(metadata[1]);
          string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[2]));
          int dataLength = Convert.ToInt32(metadata[3]);
          bool isIdentity = Convert.ToBoolean(metadata[4]);
          string nullable = Convert.ToString(metadata[5]).ToUpper();
          bool isNullable = (nullable == "Y" || nullable == "TRUE");
          string constraint = Convert.ToString(metadata[6]);

          if (tableName != prevTableName)
          {
            table = new DataObject()
            {
              tableName = tableName,
              dataProperties = new List<DataProperty>(),
              keyProperties = new List<KeyProperty>(),
              dataRelationships = new List<DataRelationship>(), // to be supported in the future
              objectName = Utility.NameSafe(tableName)
            };

            dbDictionary.dataObjects.Add(table);
            prevTableName = tableName;
          }

          if (String.IsNullOrEmpty(constraint)) // process columns
          {
            DataProperty column = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              propertyName = Utility.NameSafe(columnName)
            };

            table.dataProperties.Add(column);
          }
          else // process keys
          {
            KeyType keyType = KeyType.assigned;

            if (isIdentity)
            {
              keyType = KeyType.identity;
            }
            else if (constraint.ToUpper() == "FOREIGN KEY" || constraint.ToUpper() == "R")
            {
              keyType = KeyType.foreign;
            }

            DataProperty key = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              keyType = keyType,
              propertyName = Utility.NameSafe(columnName),
            };

            table.addKeyProperty(key);
          }
        }
        return dbDictionary;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetDatabaseSchema: " + ex);
        return dbDictionary;
      }
    }

    public DataRelationships GetRelationships()
    {
      try
      {
        return new DataRelationships();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetRelationships: " + ex);
        return null;
      }
    }

    public DataProviders GetProviders()
    {
      try
      {
        return new DataProviders();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetProviders: " + ex);
        return null;
      }
    }

    public DataObjects GetSchemaObjects(string projectName, string applicationName)
    {
      DataObjects tableNames = new DataObjects();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      try
      {
        InitializeScope(projectName, applicationName);
        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);
        else
          return tableNames;

        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString().ToUpper();
        string schemaName = dbDictionary.SchemaName;
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        _logger.DebugFormat("ConnectString: {0} \r\n Provider: {1} \r\n SchemaName: {2} \r\n Parsed: {3}",
            connString,
            dbProvider,
            schemaName,
            parsedConnStr);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);
        properties.Add("connection.driver_class", GetConnectionDriver(dbProvider));
        properties.Add("dialect", GetDatabaseDialect(dbProvider));

        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        config.AddProperties(properties);

        ISessionFactory sessionFactory = config.BuildSessionFactory();
        ISession session = sessionFactory.OpenSession();
        string sql = GetDatabaseMetaquery(dbProvider, parsedConnStr.Split(';')[1].Split('=')[1], schemaName);

        _logger.DebugFormat("SQL: {0}",
            sql);

        ISQLQuery query = session.CreateSQLQuery(sql);

        DataObjects metadataList = new DataObjects();
        foreach (string tableName in query.List<string>())
        {
          metadataList.Add(tableName);
        }
        session.Close();

        tableNames = metadataList;
        return tableNames;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetSchemaObjects: " + ex);
        return tableNames;
      }
    }

    public DataObject GetSchemaObjectSchema(string projectName, string applicationName, string schemaObjectName)
    {
      List<string> tableNames = new List<string>();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      DataObject dataObject = new DataObject
      {
        tableName = schemaObjectName,
        dataProperties = new List<DataProperty>(),
        keyProperties = new List<KeyProperty>(),
        dataRelationships = new List<DataRelationship>(),
        objectName = Utility.NameSafe(schemaObjectName)
      };
      try
      {
        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = Utility.Read<DatabaseDictionary>(_settings["DBDictionaryPath"]);

        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString().ToUpper();
        string schemaName = dbDictionary.SchemaName;
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);
        properties.Add("connection.driver_class", GetConnectionDriver(dbProvider));
        properties.Add("dialect", GetDatabaseDialect(dbProvider));

        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        config.AddProperties(properties);

        ISessionFactory sessionFactory = config.BuildSessionFactory();
        ISession session = sessionFactory.OpenSession();
        ISQLQuery query = session.CreateSQLQuery(GetTableMetaQuery(dbProvider, parsedConnStr.Split(';')[1].Split('=')[1], schemaName, schemaObjectName));
        IList<object[]> metadataList = query.List<object[]>();
        session.Close();

        foreach (object[] metadata in metadataList)
        {
          string columnName = Convert.ToString(metadata[0]);
          string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[1]));
          int dataLength = Convert.ToInt32(metadata[2]);
          bool isIdentity = Convert.ToBoolean(metadata[3]);
          string nullable = Convert.ToString(metadata[4]).ToUpper();
          bool isNullable = (nullable == "Y" || nullable == "TRUE");
          string constraint = Convert.ToString(metadata[5]);

          if (String.IsNullOrEmpty(constraint)) // process columns
          {
            DataProperty column = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              propertyName = Utility.NameSafe(columnName)
            };

            dataObject.dataProperties.Add(column);
          }
          else
          {
            KeyType keyType = KeyType.assigned;

            if (isIdentity)
            {
              keyType = KeyType.identity;
            }
            else if (constraint.ToUpper() == "FOREIGN KEY" || constraint.ToUpper() == "R")
            {
              keyType = KeyType.foreign;
            }

            DataProperty key = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              keyType = keyType,
              propertyName = Utility.NameSafe(columnName),
            };
            dataObject.addKeyProperty(key);
          }
        }
        return dataObject;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetSchemaObjectSchema: " + ex);
        return dataObject;
      }
    }

    public VersionInfo GetVersion()
    {
      Version version = this.GetType().Assembly.GetName().Version;
      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }
    #endregion

    #region private methods
    private ISession OpenSession()
    {
      try
      {
        return _sessionFactory.OpenSession();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in OpenSession: project[{0}] application[{1}] {2}", _settings["ProjectName"], _settings["ApplicationName"], ex));
        throw new Exception("Error while openning nhibernate session " + ex);
      }
    }

    private string GetTableMetaQuery(string dbProvider, string databaseName, string schemaName, string objectName)
    {
      string tableQuery = string.Empty;

      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        tableQuery = string.Format(
          "SELECT t1.COLUMN_NAME, t1.DATA_TYPE, t2.max_length, t2.is_identity, t2.is_nullable, t5.CONSTRAINT_TYPE " +
          "FROM INFORMATION_SCHEMA.COLUMNS AS t1 INNER JOIN sys.columns AS t2 ON t2.name = t1.COLUMN_NAME INNER JOIN  sys.schemas AS ts ON " +
          "ts.name = t1.TABLE_SCHEMA INNER JOIN  sys.tables AS t3 ON t3.schema_id = ts.schema_id AND t3.name = t1.TABLE_NAME AND " +
          "t3.object_id = t2.object_id LEFT OUTER JOIN  INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS t4 ON t4.TABLE_SCHEMA = t1.TABLE_SCHEMA AND " +
          "t4.TABLE_NAME = t1.TABLE_NAME AND t4.COLUMN_NAME = t1.COLUMN_NAME LEFT OUTER JOIN  INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS t5 ON " +
          "t5.CONSTRAINT_NAME = t4.CONSTRAINT_NAME AND t5.CONSTRAINT_SCHEMA = t4.TABLE_SCHEMA WHERE (t1.DATA_TYPE NOT IN ('image')) AND " +
          "(t1.TABLE_CATALOG = '{0}') AND (t1.TABLE_SCHEMA = '{1}') AND (t1.TABLE_NAME = '{2}')  ORDER BY t1.COLUMN_NAME",
          databaseName,
          schemaName,
          objectName
        );
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        tableQuery = string.Format(
          "select t1.COLUMN_NAME, t1.DATA_TYPE, t1.CHARACTER_MAXIMUM_LENGTH, t1.COLUMN_KEY, t1.IS_NULLABLE, c1.CONSTRAINT_TYPE " +
          " from INFORMATION_SCHEMA.COLUMNS t1 join KEY_COLUMN_USAGE u1 on u1.TABLE_NAME = t1.TABLE_NAME and u1.TABLE_SCHEMA = t1.TABLE_SCHEMA and " +
          " t1.COLUMN_NAME = u1.COLUMN_NAME join INFORMATION_SCHEMA.TABLE_CONSTRAINTS c1 on u1.CONSTRAINT_NAME = c1.CONSTRAINT_NAME and u1.TABLE_NAME = c1.TABLE_NAME " +
          " where t1.TABLE_SCHEMA = '{0}' and t1.TABLE_NAME = '{1}' ORDER BY t1.COLUMN_NAME",
          schemaName,
          objectName
        );
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {

        tableQuery = string.Format(" SELECT t2.column_name, t2.data_type, t2.data_length," +
          " 0 AS is_sequence, t2.nullable, t4.constraint_type" +
          " FROM all_objects t1 INNER JOIN all_tab_cols t2" +
          " ON t2.table_name = t1.object_name AND t2.owner = t1.owner" +
          " LEFT JOIN all_cons_columns t3 ON t3.table_name   = t2.table_name" +
          " AND t3.column_name = t2.column_name AND t3.owner = t2.owner" +
          " AND SUBSTR(t3.constraint_name, 0, 3) != 'SYS' LEFT JOIN all_constraints t4" +
          " ON t4.constraint_name = t3.constraint_name AND t4.owner = t3.owner" +
          " AND (t4.constraint_type = 'P' OR t4.constraint_type = 'R')" +
          " WHERE UPPER(t1.owner) = '{0}' AND UPPER(t1.object_name) = '{1}' ORDER BY" +
          " t1.object_name, t4.constraint_type, t2.column_name ORDER BY t2.column_name",
          schemaName.ToUpper(),
          objectName.ToUpper()
          );
      }
      return tableQuery;
    }

    private string GetDatabaseMetaquery(string dbProvider, string database, string schemaName)
    {
      string metaQuery = String.Empty;

      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        metaQuery = String.Format("select table_name from INFORMATION_SCHEMA.TABLES WHERE table_schema = '{0}' order by table_name", schemaName);
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        metaQuery = String.Format("select table_name from INFORMATION_SCHEMA.TABLES where table_schema = '{0}' order by table_name;", schemaName);
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        metaQuery = String.Format("select object_name from all_objects where object_type in ('TABLE', 'VIEW', 'SYNONYM') and UPPER(owner) = '{0}' order by object_name", schemaName.ToUpper());
      }
      else
        throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));

      return metaQuery;
    }

    private string GetDatabaseDialect(string dbProvider)
    {
      switch (dbProvider.ToUpper())
      {
        case "MSSQL2008":
          return "NHibernate.Dialect.MsSql2008Dialect";

        case "MSSQL2005":
          return "NHibernate.Dialect.MsSql2005Dialect";

        case "MSSQL2000":
          return "NHibernate.Dialect.MsSql2000Dialect";

        case "ORACLE10G":
          return "NHibernate.Dialect.Oracle10gDialect";

        case "ORACLE9I":
          return "NHibernate.Dialect.Oracle9iDialect";

        case "ORACLE8I":
          return "NHibernate.Dialect.Oracle8iDialect";

        case "ORACLELITE":
          return "NHibernate.Dialect.OracleLiteDialect";

        case "MYSQL3":
        case "MYSQL4":
        case "MYSQL5":
          return "NHibernate.Dialect.MySQL5Dialect";

        default:
          throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));
      }
    }

    private string GetConnectionDriver(string dbProvider)
    {
      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        return "NHibernate.Driver.SqlClientDriver";
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        return "NHibernate.Driver.MySqlDataDriver";
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        return "NHibernate.Driver.OracleClientDriver";
      }
      else
        throw new Exception(string.Format("Database provider {0} is not supported", dbProvider));
    }

    private void InitializeScope(string projectName, string applicationName)
    {
      try
      {
        if (!_isScopeInitialized)
        {
          string scope = string.Format("{0}.{1}", projectName, applicationName);

          _settings["ProjectName"] = projectName;
          _settings["ApplicationName"] = applicationName;
          _settings["Scope"] = scope;

          _settings["DBDictionaryPath"] = String.Format("{0}DatabaseDictionary.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private bool ValidateDatabaseDictionary(DatabaseDictionary dbDictionary)
    {
      ISession session = null;

      try
      {
        // Validate connection string
        string connectionString = dbDictionary.ConnectionString;
        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        Dictionary<string, string> properties = new Dictionary<string, string>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("connection.connection_string", dbDictionary.ConnectionString);
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("default_schema", dbDictionary.SchemaName);
        properties.Add("dialect", "NHibernate.Dialect." + dbDictionary.Provider + "Dialect");

        if (dbDictionary.Provider.ToString().ToUpper().Contains("MSSQL"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
        }
        else if (dbDictionary.Provider.ToString().ToUpper().Contains("ORACLE"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.OracleClientDriver");
        }
        else
        {
          throw new Exception("Database not supported.");
        }

        config.AddProperties(properties);
        ISessionFactory factory = config.BuildSessionFactory();

        session = factory.OpenSession();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ValidateDatabaseDictionary: " + ex);
        throw new Exception("Invalid connection string: " + ex.Message);
      }
      finally
      {
        if (session != null) session.Close();
      }

      // Validate table key
      foreach (DataObject dataObject in dbDictionary.dataObjects)
      {
        if (dataObject.keyProperties == null || dataObject.keyProperties.Count == 0)
        {
          throw new Exception(string.Format("Table \"{0}\" has no key.", dataObject.tableName));
        }
      }

      return true;
    }

    private static string ParseConnectionString(string connStr, string dbProvider)
    {
      try
      {
        string parsedConnStr = String.Empty;
        char[] ch = { ';' };
        string[] connStrKeyValuePairs = connStr.Split(ch, StringSplitOptions.RemoveEmptyEntries);

        foreach (string connStrKeyValuePair in connStrKeyValuePairs)
        {
          string[] connStrKeyValuePairTemp = connStrKeyValuePair.Split('=');
          string connStrKey = connStrKeyValuePairTemp[0].Trim();
          string connStrValue = connStrKeyValuePairTemp[1].Trim();

          if (connStrKey.ToUpper() == "DATA SOURCE" ||
              connStrKey.ToUpper() == "USER ID" ||
              connStrKey.ToUpper() == "PASSWORD")
          {
            parsedConnStr += connStrKey + "=" + connStrValue + ";";
          }

          if (dbProvider.ToUpper().Contains("MSSQL"))
          {
            if (connStrKey.ToUpper() == "INITIAL CATALOG" ||
                connStrKey.ToUpper() == "INTEGRATED SECURITY")
            {
              parsedConnStr += connStrKey + "=" + connStrValue + ";";
            }
          }
          else if (dbProvider.ToUpper().Contains("MYSQL"))
          {
            parsedConnStr += connStrKey + "=" + connStrValue + ";";
          }
        }

        return parsedConnStr;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ParseConnectionString: " + ex);
        throw ex;
      }
    }

    private bool isAuthorized()
    {
      if (_keyRing != null && _keyRing["Name"] != null)
      {
        string userName = _keyRing["Name"].ToString();
        userName = userName.Substring(userName.IndexOf('\\') + 1).ToLower();

        if (userName == "anonymous")
        {
          return true;
        }

        AuthorizedUsers authUsers = null;

        try
        {
          authUsers = Utility.Read<AuthorizedUsers>(_authorizationPath, true);
        }
        catch (Exception ex)
        {
          _logger.Warn("Error loading authorization file: " + ex);
        }

        if (authUsers != null)
        {
          foreach (string authUser in authUsers)
          {
            if (authUser.ToLower() == userName)
            {
              return true;
            }
          }
        }
      }

      return false;
    }
    #endregion
  }
}
