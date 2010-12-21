using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using NHibernate;
using NHibernate.Cfg;
using System.IO;
using org.iringtools.utility;
using org.iringtools.adapter;
using System.Text;
using Ninject;
using log4net;
using System.Reflection;
using System.Collections.Specialized;

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer));
    private string _dataDictionaryPath = String.Empty;
    private AdapterSettings _settings = null;
    private ISessionFactory _sessionFactory;

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings)
    {
      _settings = settings;

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

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("select Id from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          string whereClause = filter.ToSqlWhereClause(objectType, null);
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

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          string whereClause = filter.ToSqlWhereClause(objectType, null);
          queryString.Append(whereClause);
        }

        using (ISession session = OpenSession())
        {
          IQuery query = session.CreateQuery(queryString.ToString());
          IList<IDataObject> dataObjects = query.List<IDataObject>();

          if (pageSize > 0 && pageNumber > 0)
          {
            if (dataObjects.Count > (pageSize * (pageNumber - 1) + pageSize))
            {
              dataObjects = dataObjects.ToList().GetRange(pageSize * (pageNumber - 1), pageSize);
            }
            else if (pageSize * (pageNumber - 1) > dataObjects.Count)
            {
              dataObjects = dataObjects.ToList().GetRange(pageSize * (pageNumber - 1), dataObjects.Count);
            }
            else
            {
              return null;
            }
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

    //TODO: Status should be assigned to the appropriate identifier
    public Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();
      Status status = new Status();

      try
      {
        status.Identifier = objectType;
        IList<IDataObject> dataObjects = Create(objectType, identifiers);

        using (ISession session = OpenSession())
        {
          foreach (IDataObject dataObject in dataObjects)
            session.Delete(dataObject);
          session.Flush();
          status.Messages.Add(string.Format("Records of type [{0}] has been deleted succesfully.", objectType));
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
      }

      response.Append(status);
      return response;
    }

    public Response Delete(string objectType, DataFilter filter)
    {
      Response response = new Response();
      response.StatusList = new List<Status>();
      Status status = new Status();

      try
      {
        status.Identifier = objectType;

        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (filter.Expressions.Count > 0)
        {
          string whereClause = filter.ToSqlWhereClause(objectType, null);
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
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }

    public IList<IDataObject> GetRelatedObjects(IDataObject sourceDataObject, string relatedObjectType)
    {
      IList<IDataObject> relatedObjects;
      DataDictionary dictionary = GetDictionary();
      DataObject dataObject = dictionary.DataObjects.First(c => c.ObjectName == sourceDataObject.GetType().Name);
      DataRelationship dataRelationship = dataObject.DataRelationships.First(c => c.RelationshipName == relatedObjectType);

      StringBuilder sql = new StringBuilder();
      sql.Append("from " + dataRelationship.RelatedObjectName + " where ");
      foreach (PropertyMap map in dataRelationship.PropertyMaps)
      {
        sql.Append(map.RelatedPropertyName + " = '" + sourceDataObject.GetPropertyValue(map.DataPropertyName) + "' and ");
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
