using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer));
    private string _dataDictionaryPath = String.Empty;
    private DataDictionary _dataDictionary;
    private DatabaseDictionary _databaseDictionary;
    private AdapterSettings _settings = null;
    private IDictionary _keyRing = null;
    private ISessionFactory _sessionFactory;

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings, IDictionary keyRing)
    {
      _settings = settings;

      _keyRing = keyRing;

      string hibernateConfigPath = string.Format("{0}nh-configuration.{1}.xml",
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

      string databaseDictionaryPath = string.Format("{0}DatabaseDictionary.{1}.xml",
        _settings["XmlPath"],
        _settings["Scope"]
      );

      _databaseDictionary = Utility.Read<DatabaseDictionary>(databaseDictionaryPath);

      _sessionFactory = new Configuration()
        .Configure(hibernateConfigPath)
        .AddFile(hibernateMappingPath)
        .BuildSessionFactory();
    }

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

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        Type type = Type.GetType("org.iringtools.adapter.datalayer.proj_" + _settings["Scope"] + "." + objectType + ", " + _settings["ExecutingAssemblyName"]);

        if (identifiers != null)
        {
          ISession session = OpenSession();

          foreach (string identifier in identifiers)
          {
            IDataObject dataObject = null;

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

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in CreateList: " + ex);
        throw new Exception(string.Format("Error while creating a list of data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public long GetCount(string objectType, DataFilter filter)
    {
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

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
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

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
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
          return query.List<IDataObject>();
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
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

        if (filter != null && filter.Expressions != null && (filter.Expressions.Count > 0 || filter.OrderExpressions.Count > 0))
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

    public Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();

      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          using (ISession session = OpenSession())
          {
            foreach (IDataObject dataObject in dataObjects)
            {
              string identifier = dataObject.GetPropertyValue("Id").ToString();

              Status status = new Status();
              status.Messages = new Messages();
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

    public Response Delete(string objectType, IList<string> identifiers)
    {
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

    public Response Delete(string objectType, DataFilter filter)
    {
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

    public DataDictionary GetDictionary()
    {
      _dataDictionary = Utility.Read<DataDictionary>(_dataDictionaryPath);
      return _dataDictionary;
    }

    public IList<IDataObject> GetRelatedObjects(IDataObject sourceDataObject, string relatedObjectType)
    {
      IList<IDataObject> relatedObjects;
      DataDictionary dictionary = GetDictionary();
      DataObject dataObject = dictionary.dataObjects.First(c => c.objectName == sourceDataObject.GetType().Name);
      DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relatedObjectName == relatedObjectType);

      StringBuilder sql = new StringBuilder();
      sql.Append("from " + dataRelationship.relatedObjectName + " where ");
      foreach (PropertyMap map in dataRelationship.propertyMaps)
      {
        sql.Append(map.relatedPropertyName + " = '" + sourceDataObject.GetPropertyValue(map.dataPropertyName) + "' and ");
      }
      sql.Remove(sql.Length - 4, 4);

      using (ISession session = OpenSession())
      {
        IQuery query = session.CreateQuery(sql.ToString());
        relatedObjects = query.List<IDataObject>();
      }

      return relatedObjects;
    }
  }
}
