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

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer));
    private AdapterSettings _settings = null;
    private ApplicationSettings _appSettings = null;
    private string _dataDictionaryPath = String.Empty;
    private ISessionFactory factory;
    private EntityGenerator _generator = null;

    #region Constants
    #endregion

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings, ApplicationSettings appSettings, EntityGenerator generator) 
    {
      _dataDictionaryPath = settings.XmlPath + "DataDictionary." + appSettings.ProjectName + "." + appSettings.ApplicationName + ".xml";
      _settings = settings;
      _generator = generator;
      _appSettings = appSettings;
    }

    private ISession OpenSession<T>()
    {
      try
      {
        string hibernateConfigPath = _settings.XmlPath + "nh-configuration." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";
        string hibernateMappingPath = _settings.XmlPath + "nh-mapping." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";

        factory = new Configuration()
          .Configure(hibernateConfigPath)
          .AddFile(hibernateMappingPath)
          .BuildSessionFactory();

        return factory.OpenSession();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in OpenSession: project \"" + _appSettings.ProjectName + "\" application \"" + _appSettings.ApplicationName + "\"" + exception);
        throw new Exception("Error while openning session " + exception);
      }
    }

    public T Get<T>(Dictionary<string, object> queryProperties)
    {
      try
      {
        StringBuilder queryString = new StringBuilder();
        using (ISession session = OpenSession<T>())
        {
          queryString.Append(" from " + typeof(T).Name + " as " + typeof(T).Name);
          if (queryProperties != null && queryProperties.Count > 0)
          {
            queryString.Append(" where ");
            foreach (KeyValuePair<string, object> keyValuePair in queryProperties)
            {
              queryString.Append(typeof(T).Name + "." + keyValuePair.Key + "='" + keyValuePair.Value + "'");
              queryString.Append(" and ");
            }
            queryString.Replace(" and ", "", queryString.Length - 5, 5);
          }

          IQuery query = session.CreateQuery(queryString.ToString());
          return query.List<T>().FirstOrDefault<T>();
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Get<T>: " + exception);
        throw new Exception("Error while getting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public IList<T> GetList<T>()
    {
      return GetList<T>(null);
    }

    public IList<T> GetList<T>(Dictionary<string, object> queryProperties)
    {
      try
      {
        StringBuilder queryString = new StringBuilder();
        using (ISession session = OpenSession<T>())
        {
          queryString.Append(" from " + typeof(T).Name + " as " + typeof(T).Name);
          if (queryProperties != null && queryProperties.Count > 0)
          {
            queryString.Append(" where ");
            foreach (KeyValuePair<string, object> keyValuePair in queryProperties)
            {
              queryString.Append(typeof(T).Name + "." + keyValuePair.Key + "='" + keyValuePair.Value + "'");
              queryString.Append(" and ");
            }
            queryString.Replace(" and ", "", queryString.Length - 5, 5);
          }
          IQuery query = session.CreateQuery(queryString.ToString());
          return query.List<T>();
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetList<T>: " + exception);
        throw new Exception("Error while getting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public Response Post<T>(T graph)
    {
      Response response;
      try
      {
        response = new Response();
        using (ISession session = OpenSession<T>())
        {
          using (ITransaction transaction = session.BeginTransaction())
          {             
            session.SaveOrUpdate(graph);
            transaction.Commit();
            response.Add("Records (" + typeof(T).Name + ") have been updated succesfully");         
          }
        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Post<T>: " + exception);
        throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public Response PostList<T>(List<T> graphList)
    {
      Response response;
      try
      {
        response = new Response();
        foreach (T graph in graphList)
        {
          Response responseGraph = Post<T>(graph);
          response.Append(responseGraph);
        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in PostList<T>: " + exception);
        throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public Response Delete<T>(T graph)
    {
      Response response;
      try
      {
        response = new Response();
        using (ISession session = OpenSession<T>())
        {
          using (ITransaction transaction = session.BeginTransaction())
          {
            session.Delete(graph);
            transaction.Commit();
          }
          response.Add("Records (" + typeof(T).Name + ") have been deleted succesfully");

        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Delete<T>: " + exception);
        throw new Exception("Error while deleting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public DataDictionary GetDictionary()
    {
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }

    public Response RefreshDictionary()
    {
      throw new NotImplementedException();
    }
  }
}