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

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer2 : IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer2));
    private AdapterSettings _settings = null;
    private ApplicationSettings _appSettings = null;
    private string _dataDictionaryPath = String.Empty;
    private ISessionFactory factory;

    [Inject]
    public NHibernateDataLayer2(AdapterSettings settings, ApplicationSettings appSettings) //, EntityGenerator generator)
    {
      _dataDictionaryPath = settings.XmlPath + "DataDictionary." + appSettings.ProjectName + "." + appSettings.ApplicationName + ".xml";
      _settings = settings;
      _appSettings = appSettings;
    }

    private ISession OpenSession()
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
      catch (Exception ex)
      {
        _logger.Error("Error in OpenSession: project \"" + _appSettings.ProjectName + "\" application \"" + _appSettings.ApplicationName + "\"" + ex);
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
        throw new Exception("Error while creating a list of data objects of type [" + objectType + "].", ex);
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
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
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
          queryString.Append(" where Id in " + String.Join(",", identifiers.ToArray()));
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
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
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
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
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
                response.Add("Record [" + dataObject.GetPropertyValue("Id") + "] have been saved successfully");
              }
              catch (Exception ex)
              {
                response.Add("Error while posting record [" + dataObject.GetPropertyValue("Id") + "]." + ex);
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
        throw new Exception("Error while posting data objects of type [" + objectType + "].", ex);
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
          response.Add("Records of type [" + objectType + "] has been deleted succesfully.");
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
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
          response.Add("Records of type [" + objectType + "] has been deleted succesfully.");
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
    }

    public DataDictionary GetDictionary()
    {
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }
  }
}
