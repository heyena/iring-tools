﻿using System;
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
    private ApplicationSettings _appSettings = null;
    private string _dataDictionaryPath = String.Empty;
    private ISessionFactory _sessionFactory;

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings, ApplicationSettings appSettings, NameValueCollection webConfig)
    {
      string scope = string.Format("{0}.{1}", appSettings.ProjectName, appSettings.ApplicationName);
      string hibernateConfigPath = string.Format("{0}nh-configuration.{1}.xml", settings.XmlPath, scope);
      string hibernateMappingPath = string.Format("{0}nh-mapping.{1}.xml", settings.XmlPath, scope);

      _appSettings = appSettings;
      _dataDictionaryPath = string.Format("{0}DataDictionary.{1}.xml", settings.XmlPath, scope);      
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
        _logger.Error(string.Format("Error in OpenSession: project[{0}] application[{1}] {2}", _appSettings.ProjectName, _appSettings.ApplicationName, ex));
        throw new Exception("Error while openning nhibernate session " + ex);
      }
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        Type type = Type.GetType(objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

            if (!String.IsNullOrEmpty(identifier))
            {
              dataObject.SetPropertyValue("Id", identifier);
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
          queryString.Append(" where Id in ('" + String.Join("','", identifiers.ToArray())+ "')");
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
              try
              {
                session.SaveOrUpdate(dataObject);
                session.Flush();
                response.Add(string.Format("Record [{0}] have been saved successfully.", dataObject.GetPropertyValue("Id")));
              }
              catch (Exception ex)
              {
                response.Add(string.Format("Error while posting record [{0}]. {1}", dataObject.GetPropertyValue("Id"), ex));
              }
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
        throw new Exception(string.Format("Error while posting data objects of type [{0}]. {1}", objectType,  ex));
      }
    }

    public Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();

      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          queryString.Append(" where Id in " + String.Join(",", identifiers.ToArray()));
        }

        using (ISession session = OpenSession())
        {
          session.Delete(queryString.ToString());
          response.Add(string.Format("Records of type [{0}] has been deleted succesfully.", objectType));
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public Response Delete(string objectType, DataFilter filter)
    {
      Response response = new Response();

      try
      {
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
          response.Add(string.Format("Records of type [{0}] has been deleted succesfully.", objectType));
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public DataDictionary GetDictionary()
    {
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }
  }
}