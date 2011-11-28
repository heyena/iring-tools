using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using NHibernate;
using Ninject;
using org.iringtools.library;
using org.iringtools.nhibernate;
using org.iringtools.utility;

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer : BaseConfigurableDataLayer, IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer));
    private const string UNAUTHORIZED_ERROR = "User not authorized to access NHibernate data layer of [{0}]";

    private string _dataDictionaryPath = String.Empty;
    private string _databaseDictionaryPath = string.Empty;
    private DataDictionary _dataDictionary;
    private DatabaseDictionary _dbDictionary;
    private IDictionary _keyRing = null;
    private string _authorizationPath = string.Empty;
    private Response _response = null;
    private IKernel _kernel = null;
    private NHibernateSettings _nSettings = null;
    private bool _isScopeInitialized = false;

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings, IDictionary keyRing) : base(settings)
    {
      _kernel = new StandardKernel(new NHibernateModule());
      _nSettings = _kernel.Get<NHibernateSettings>();
      _nSettings.AppendSettings(settings);
      _keyRing = keyRing;
      _response = new Response();
      _kernel.Bind<Response>().ToConstant(_response);

      _dataDictionaryPath = string.Format("{0}DataDictionary.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      string dbDictionaryPath = string.Format("{0}DatabaseDictionary.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      if (File.Exists(dbDictionaryPath))
      {
        _dbDictionary = NHibernateUtility.LoadDatabaseDictionary(dbDictionaryPath);
      }

      _authorizationPath = string.Format("{0}Authorization.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );
    }

    #region public methods
    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        DataObject objectDefinition = _dataDictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectType + ", " + _settings["ExecutingAssemblyName"]);
        IDataObject dataObject = null;

        if (identifiers != null)
        {
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
      finally
      {
        CloseSession(session);
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];

          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select count(*) from " + objectType);

        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          DataFilter clonedFilter = Utility.CloneDataContractObject<DataFilter>(filter);
          clonedFilter.OrderExpressions = null;
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = clonedFilter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        long count = query.List<long>().First();
        return count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        StringBuilder queryString = new StringBuilder();
        queryString.Append("select Id from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = filter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        return query.List<string>();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          DataObject dataObjectDef = (from DataObject o in _dbDictionary.dataObjects
                                   where o.objectName == objectType
                                   select o).FirstOrDefault();

          if (dataObjectDef == null)
            return null;

          if (dataObjectDef.keyProperties.Count == 1)
          {
            queryString.Append(" where Id in ('" + String.Join("','", identifiers.ToArray()) + "')");
          }
          else if (dataObjectDef.keyProperties.Count > 1)
          {
            string[] keyList = null;
            int identifierIndex = 1;
            foreach (string identifier in identifiers)
            {
              string[] idParts = identifier.Split(dataObjectDef.keyDelimeter.ToCharArray()[0]);

              keyList = new string[idParts.Count()];

              int partIndex = 0;
              foreach (string part in idParts)
              {
                if (identifierIndex == identifiers.Count())
                {
                  keyList[partIndex] += part;
                }
                else
                {
                  keyList[partIndex] += part + ", ";
                }

                partIndex++;
              }

              identifierIndex++;
            }

            int propertyIndex = 0;
            foreach (KeyProperty keyProperty in dataObjectDef.keyProperties)
            {
              string propertyValues = keyList[propertyIndex];

              if (propertyIndex == 0)
              {
                queryString.Append(" where " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }
              else
              {
                queryString.Append(" and " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }

              propertyIndex++;
            }
          }
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        IList<IDataObject> dataObjects = query.List<IDataObject>();

        // order data objects as list of identifiers
        if (identifiers != null)
        {
          IList<IDataObject> orderedDataObjects = new List<IDataObject>();

          foreach (string identifier in identifiers)
          {
            if (identifier != null)
            {
              foreach (IDataObject dataObject in dataObjects)
              {
                if (dataObject.GetPropertyValue("Id").ToString().ToLower() == identifier.ToLower())
                {
                  orderedDataObjects.Add(dataObject);
                  //break;  // include dups also
                }
              }
            }
          }

          return orderedDataObjects;
        }

        return dataObjects;
      }      
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      
      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        DataObject objectDefinition = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());

        if (objectDefinition == null)
        {
          throw new Exception("Object type [" + objectType + "] not found.");
        }

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectType + ", " + _settings["ExecutingAssemblyName"]);

        // make an exception for tests
        if (type == null)
        {
          type = Type.GetType(ns + objectType + ", NUnit.Tests");
        }
        
        ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, objectDefinition, filter);            

        if (pageSize == 0 && startIndex == 0)
        {
          List<IDataObject> dataObjects = new List<IDataObject>();
          long totalCount = GetCount(objectType, filter);
          int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 1000;
          int numOfRows = 0;

          while (numOfRows < totalCount)
          {
            criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
            dataObjects.AddRange(criteria.List<IDataObject>());
            numOfRows += internalPageSize;
          }

          return dataObjects;
        }
        else
        {
          criteria.SetFirstResult(startIndex).SetMaxResults(pageSize);
          return criteria.List<IDataObject>();
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    private DataFilter FilterByIdentity(string objectType, DataFilter filter, IdentityProperties identityProperties)
    {
      DataObject dataObject = _dbDictionary.dataObjects.Find(d => d.objectName == objectType);
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
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      
      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
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
                _logger.Error("Error posting data object to data layer: " + ex);
              }
            }
            else
            {
              status.Level = StatusLevel.Error;
              status.Identifier = String.Empty;
              status.Messages.Add("Data object is null or duplicate. See log for details.");
            }

            response.Append(status);
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
      finally
      {
        CloseSession(session);
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = Create(objectType, identifiers);

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
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        response.Append(status);
      }
      finally
      {
        CloseSession(session);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      Response response = new Response();
      response.StatusList = new List<Status>();
      Status status = new Status();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
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
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = filter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);          
          queryString.Append(whereClause);
        }

        session.Delete(queryString.ToString());
        session.Flush();
        status.Messages.Add(string.Format("Records of type [{0}] has been deleted succesfully.", objectType));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        //no need to status, thrown exception will be statused above.
      }
      finally
      {
        CloseSession(session);
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
      if (!IsAuthorized())
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> relatedObjects = null;
        DataDictionary dictionary = GetDictionary();
        DataObject dataObject = dictionary.dataObjects.First(c => c.objectName.ToLower() == sourceDataObject.GetType().Name.ToLower());
        DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relatedObjectName.ToLower() == relatedObjectType.ToLower());

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
        IQuery query = session.CreateQuery(sql.ToString());
        relatedObjects = query.List<IDataObject>();

        if (relatedObjects != null && relatedObjects.Count > 0 && dataRelationship.relationshipType == RelationshipType.OneToOne)
        {
          return new List<IDataObject> { relatedObjects.First() };
        }

        return relatedObjects;
      }
      catch (Exception e)
      {
        string error = "Error getting related objects [" + relatedObjectType + "] " + e;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        CloseSession(session);
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
            _settings["AppDataPath"],
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

    private void CloseSession(ISession session)
    {
      try
      {
        if (session != null)
        {
          session.Close();
          session = null;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error closing NHibernate session: " + ex);
      }
    }

    private bool IsAuthorized()
    {
      try
      {
        if (_keyRing != null && _keyRing["UserName"] != null)
        {
          string userName = _keyRing["UserName"].ToString();
          userName = userName.Substring(userName.IndexOf('\\') + 1).ToLower();

          _logger.Debug("Authorizing user [" + userName + "]");

          if (userName == "anonymous")
          {
            return true;
          }

          AuthorizedUsers authUsers = Utility.Read<AuthorizedUsers>(_authorizationPath, true);

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
        else
        {
          _logger.Error("KeyRing is empty.");
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error during processing authorization: " + e);
      }

      return false;
    }
    #endregion
  }
}
