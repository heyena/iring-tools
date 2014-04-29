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
using Ninject.Extensions.Xml;
using System.Xml.Linq;

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer : BaseConfigurableDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer));
    private IKernel _kernel = null;
    private org.iringtools.nhibernate.IAuthorization _authorization;
    protected const string UNAUTHORIZED_ERROR = "User not authorized to access NHibernate data layer of [{0}]";
    
    protected string _dataDictionaryPath = String.Empty;
    protected string _dbDictionaryPath = String.Empty;
    
    protected string _authorizationBindingPath = String.Empty;
    protected string _summaryBindingPath = String.Empty;

    protected DataDictionary _dataDictionary;
    protected DatabaseDictionary _dbDictionary;
    protected IDictionary _keyRing = null;
    protected NHibernateSettings _nSettings = null;

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings, IDictionary keyRing) : base(settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false, UseReflectionBasedInjection = true };
      _kernel = new StandardKernel(ninjectSettings, new NHibernateModule());
      _kernel.Load(new XmlExtensionModule());
      _nSettings = _kernel.Get<NHibernateSettings>();
      _nSettings.AppendSettings(settings);
      _keyRing = keyRing;

      _kernel.Rebind<AdapterSettings>().ToConstant(_settings);
      _kernel.Bind<IDictionary>().ToConstant(_keyRing).Named("KeyRing");

      _dataDictionaryPath = string.Format("{0}DataDictionary.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      _dbDictionaryPath = string.Format("{0}DatabaseDictionary.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      string keyFile = string.Format("{0}{1}.{2}.key",
        _settings["AppDataPath"], _settings["ProjectName"], _settings["ApplicationName"]);

      if (File.Exists(_dbDictionaryPath))
      {
        _dbDictionary = NHibernateUtility.LoadDatabaseDictionary(_dbDictionaryPath, keyFile);
      }
      else if (utility.Utility.isLdapConfigured && utility.Utility.FileExistInRepository<DatabaseDictionary>(_dbDictionaryPath))
      {
        _dbDictionary = NHibernateUtility.LoadDatabaseDictionary(_dbDictionaryPath, keyFile);
      }

      _dataDictionary = new DataDictionary();
      _dataDictionary.dataObjects = _dbDictionary.dataObjects;
      
      string relativePath = String.Format("{0}AuthorizationBindingConfiguration.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      _authorizationBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );
      
      _summaryBindingPath = String.Format("{0}SummaryBindingConfiguration.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      //_kernel.Load(_authorizationBindingPath);
      if (File.Exists(_authorizationBindingPath))
      {
          _kernel.Load(_authorizationBindingPath);
      }
      else if (utility.Utility.isLdapConfigured && utility.Utility.FileExistInRepository<XElementClone>(_authorizationBindingPath))
      {
          XElement bindingConfig = Utility.GetxElementObject(_authorizationBindingPath);
          string fileName = Path.GetFileName(_authorizationBindingPath);
          string tempPath = Path.GetTempPath() + fileName;
          bindingConfig.Save(tempPath);
          _kernel.Load(tempPath);
      }
      else
      {
          _logger.Error("Authourization Binding configuration not found.");
      }
    }

    #region public methods
    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      AccessLevel accessLevel = Authorize(objectType);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        DataObject objDef = _dataDictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());

        string ns = String.IsNullOrEmpty(objDef.objectNamespace)
          ? String.Empty : (objDef.objectNamespace + ".");

        Type type = Type.GetType(ns + objDef.objectName + ", " + _settings["ExecutingAssemblyName"]);
        IDataObject dataObject = null;

        if (identifiers != null)
        {
          _logger.Debug("Preparing to create [" + identifiers.Count + "] data objects...");
          foreach (string identifier in identifiers)
          {
            if (!String.IsNullOrEmpty(identifier))
            {
              _logger.Debug("Creating data object with identifier [" + identifier + "] from [" +
                objDef.keyProperties.Count + "] key properties...");
              IQuery query = null;
              
              if (objDef.keyProperties.Count == 1)
              {
                query = session.CreateQuery("from " + objectType + " where Id = ?");
                query.SetString(0, identifier);
              }
              else if (String.IsNullOrEmpty(objDef.keyDelimeter))
              {
                throw new Exception("Object type [" + objDef.objectName + 
                  "] uses composite key but has no key delimiter.");
              }
              else
              {
                string conjunction = " and ";
                string[] idParts = identifier.Split(objDef.keyDelimeter.ToCharArray());

                if (idParts.Length != objDef.keyProperties.Count)
                {
                  throw new Exception("Inequality number of identifier parts [" + idParts.Length + 
                    "] with number of key properties [" + objDef.keyProperties.Count + 
                    "]. This is most likely due to one or more key properties not being mapped.");
                }

                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < objDef.keyProperties.Count; i++)
                {
                  string propName = objDef.keyProperties[i].keyPropertyName;
                  builder.Append(conjunction + propName + "='" + idParts[i] + "'");
                }

                builder.Remove(0, conjunction.Length);

                query = session.CreateQuery("from " + objectType + " where " + builder.ToString());
                _logger.Debug("Create query [" + query + "].");
              }
              
              dataObject = query.List<IDataObject>().FirstOrDefault<IDataObject>();

              if (dataObject == null)
              {
                dataObject = (IDataObject)Activator.CreateInstance(type);
                dataObject.SetPropertyValue("Id", identifier);
              }
            }
            else
            {
              _logger.Debug("Creating empty data object...");
              dataObject = NewDataObject(objDef, type);
            }

            dataObjects.Add(dataObject);
          }
        }
        else
        {
          dataObject = NewDataObject(objDef, type);
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
      DataFilter newFilter = null;

      if (filter != null)
      {
        newFilter = Utility.CloneDataContractObject<DataFilter>(filter);
        newFilter.OrderExpressions = null;
      }

      AccessLevel accessLevel = Authorize(objectType, ref newFilter);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];

          if (identityProperties.UseIdentityFilter)
          {
            newFilter = FilterByIdentity(objectType, newFilter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select count(*) from " + objectType);

        if (newFilter != null && newFilter.Expressions != null && newFilter.Expressions.Count > 0)
        {
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = newFilter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);
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
      DataFilter newFilter = Utility.CloneDataContractObject<DataFilter>(filter);
      AccessLevel accessLevel = Authorize(objectType, ref newFilter);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            newFilter = FilterByIdentity(objectType, newFilter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select Id from " + objectType);

        if (newFilter != null && (newFilter.Expressions.Count > 0 || newFilter.OrderExpressions.Count > 0))
        {
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = newFilter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);
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

    private bool IsNumeric(DataType dataType)
    {
      return (dataType == DataType.Byte ||
        dataType == DataType.Decimal ||
        dataType == DataType.Double ||
        dataType == DataType.Int16 ||
        dataType == DataType.Int32 ||
        dataType == DataType.Int64 ||
        dataType == DataType.Single);
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      AccessLevel accessLevel = Authorize(objectType);

      if (accessLevel < AccessLevel.Read)
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
            List<DataProperty> dataProps = new List<DataProperty>();

            foreach (KeyProperty keyProp in dataObjectDef.keyProperties)
            {
              DataProperty dataProp = dataObjectDef.dataProperties.Find(
                x => x.propertyName.ToLower() == keyProp.keyPropertyName.ToLower());

              dataProps.Add(dataProp);
            }

            for (int i = 0; i < identifiers.Count; i++)
            {
              string[] idParts = identifiers[i].Split(dataObjectDef.keyDelimeter.ToCharArray()[0]);

              if (i == 0)
              {
                queryString.Append(" WHERE ");
              }
              else
              {
                queryString.Append(" OR ");
              }

              queryString.Append("(");

              for (int j = 0; j < dataObjectDef.keyProperties.Count; j++)
              {
                string propName = dataObjectDef.keyProperties[j].keyPropertyName;

                if (j > 0)
                {
                  queryString.Append(" AND ");
                }

                if (!IsNumeric(dataProps[j].dataType))
                  idParts[j] = "'" + idParts[j] + "'";

                queryString.Append(propName + " = " + idParts[j]);
              }

              queryString.Append(")");
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
      DataFilter newFilter = Utility.CloneDataContractObject<DataFilter>(filter);
      AccessLevel accessLevel = Authorize(objectType, ref newFilter);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      
      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            newFilter = FilterByIdentity(objectType, newFilter, identityProperties);
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
        
        ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, objectDefinition, newFilter);            

        if (pageSize == 0 && startIndex == 0)
        {
          List<IDataObject> dataObjects = new List<IDataObject>();
          long totalCount = GetCount(objectType, filter);
          int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 1000;
          int numOfRows = 0;

          while (numOfRows < totalCount)
          {
            if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
            {
              criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
            }
            else
            {
              NHibernate.Criterion.Order order = new NHibernate.Criterion.Order(objectDefinition.keyProperties.First().keyPropertyName, true);
              criteria.AddOrder(order).SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
            }

            dataObjects.AddRange(criteria.List<IDataObject>());
            numOfRows += internalPageSize;
          }

          return dataObjects;
        }
        else
        {
          if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
          {
            criteria.SetFirstResult(startIndex).SetMaxResults(pageSize);
          }
          else
          {
            NHibernate.Criterion.Order order = new NHibernate.Criterion.Order(objectDefinition.keyProperties.First().keyPropertyName, true);
            criteria.AddOrder(order).SetFirstResult(startIndex).SetMaxResults(pageSize);
          }
          
          IList<IDataObject> dataObjects = criteria.List<IDataObject>();
          return dataObjects;
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
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      
      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          string objectType = dataObjects[0].GetType().Name;

          AccessLevel accessLevel = Authorize(objectType);

          if (accessLevel < AccessLevel.Write)
            throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

          foreach (IDataObject dataObject in dataObjects)
          {
            Status status = new Status();
            status.Messages = new Messages();

            if (dataObject != null)
            {
              string identifier = Convert.ToString(dataObject.GetPropertyValue("Id"));

              if (string.IsNullOrWhiteSpace(identifier))
              {
                response.Messages.Add("Identifier can not be blank.");
                continue;
              }

              status.Identifier = identifier;

              try
              {
                session.SaveOrUpdate(dataObject);
                session.Flush();
                status.Messages.Add(string.Format("Record [{0}] saved successfully.", identifier));
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
      AccessLevel accessLevel = Authorize(objectType);

      if (accessLevel < AccessLevel.Delete)
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
          status.Messages.Add(string.Format("Record [{0}] deleted successfully.", identifier));

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
      DataFilter newFilter = Utility.CloneDataContractObject<DataFilter>(filter);
      AccessLevel accessLevel = Authorize(objectType, ref newFilter);

      if (accessLevel < AccessLevel.Delete)
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
            newFilter = FilterByIdentity(objectType, newFilter, identityProperties);
          }
        }
        status.Identifier = objectType;

        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (newFilter.Expressions.Count > 0)
        {
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = newFilter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);          
          queryString.Append(whereClause);
        }

        session.Delete(queryString.ToString());
        session.Flush();
        status.Messages.Add(string.Format("Records of type [{0}] deleted succesfully.", objectType));
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
      if (_dataDictionary != null)
      {
        return _dataDictionary;
      }
      else
      {
        return new DataDictionary();
      }
    }

    public override Response Refresh(string objectType)
    {

      string keyFile = string.Format("{0}{1}.{2}.key",
        _settings["AppDataPath"], _settings["ProjectName"], _settings["ApplicationName"]);

      if (File.Exists(_settings["DBDictionaryPath"]))
        _dbDictionary = NHibernateUtility.LoadDatabaseDictionary(_settings["DBDictionaryPath"], keyFile);

      if (_dbDictionary == null || _dbDictionary.dataObjects == null)
      {
        Response response = new Response()
        {
          Level = StatusLevel.Error,
          Messages = new Messages() { "Dictionary is empty." },
        };

        return response;
      }

      return Generate(_settings["projectName"], _settings["applicationName"]);
    }

    public override Response RefreshAll()
    {
      return Refresh(null);
    }

    protected Response Generate(string scope, string app)
    {
      Response response = new Response();

      try
      {
        EntityGenerator generator = _kernel.Get<EntityGenerator>();

        string compilerVersion = "v4.0";
        if (!string.IsNullOrEmpty(_settings["CompilerVersion"]))
        {
          compilerVersion = _settings["CompilerVersion"];
        }

        Response genRes = generator.Generate(compilerVersion, _dbDictionary, scope, app);

        response.Append(genRes);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error refreshing dictionary {0}:", ex));

        response.Level = StatusLevel.Error;
        response.Messages.Add(ex.Message);
      }

      return response;
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType)
    {
      IList<IDataObject> relatedObjects = null;
      ISession session = null;
       
      try
      {
        DataObject dataObject = _dataDictionary.dataObjects.Find(c => c.objectName.ToLower() == parentDataObject.GetType().Name.ToLower());
        if (dataObject == null)
        {
          throw new Exception("Parent data object [" + parentDataObject.GetType().Name + "] not found.");
        }

        DataRelationship dataRelationship = dataObject.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedObjectType.ToLower());
        if (dataRelationship == null)
        {
          throw new Exception("Relationship between data object [" + parentDataObject.GetType().Name +
            "] and related data object [" + relatedObjectType + "] not found.");
        }

        session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
        
        StringBuilder sql = new StringBuilder();
        sql.Append("from " + dataRelationship.relatedObjectName + " where ");

        foreach (PropertyMap map in dataRelationship.propertyMaps)
        {
          DataProperty propertyMap = dataObject.dataProperties.First(c => c.propertyName == map.dataPropertyName);

          if (propertyMap.dataType == DataType.String)
          {
            sql.Append(map.relatedPropertyName + " = '" + parentDataObject.GetPropertyValue(map.dataPropertyName) + "' and ");
          }
          else
          {
            sql.Append(map.relatedPropertyName + " = " + parentDataObject.GetPropertyValue(map.dataPropertyName) + " and ");
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

    public override long GetRelatedCount(IDataObject parentDataObject, string relatedObjectType)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return GetCount(relatedObjectType, filter);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related object count for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType, int pageSize, int startIndex)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return Get(relatedObjectType, filter, pageSize, startIndex);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related objects for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override IList<Object> GetSummary()
    {
      try
      {
        AccessLevel accessLevel = Authorize("summary");

        if (accessLevel < AccessLevel.Read)
          throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

        _kernel.Load(_summaryBindingPath);
        ISummary summary = _kernel.Get<ISummary>();
        return summary.GetSummary();
      }
      catch (Exception e)
      {
        _logger.Error("Error getting summary: " + e);
        throw e;
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

    private AccessLevel Authorize(string objectType)
    {
      DataFilter dataFilter = new DataFilter();
      return Authorize(objectType, ref dataFilter);
    }

    private AccessLevel Authorize(string objectType, ref DataFilter dataFilter)
    {
      try
      {
        _authorization = _kernel.Get<org.iringtools.nhibernate.IAuthorization>();
        return _authorization.Authorize(objectType, ref dataFilter);
      }
      catch (Exception e)
      {
        _logger.Error("Error authorizing: " + e);
        throw e;
      }
    }

    private IDataObject NewDataObject(DataObject objDef, Type type)
    {
      IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

      // generate key property(properties)
      foreach (KeyProperty keyProp in objDef.keyProperties)
      {
        string newId = Guid.NewGuid().ToString("N");
        dataObject.SetPropertyValue(keyProp.keyPropertyName, newId);
      }

      return dataObject;
    }
    #endregion
  }
}
