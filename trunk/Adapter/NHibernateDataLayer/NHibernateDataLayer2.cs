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
    private static string NAMESPACE = "org.iringtools.adapter.datalayer";

    private AdapterSettings _settings = null;
    private ApplicationSettings _appSettings = null;
    private string _dataDictionaryPath = String.Empty;
    private ISessionFactory factory;

    #region Constants
    private List<RelationalOperator> LikeOperators = new List<RelationalOperator>
    {
      RelationalOperator.BeginsWith, 
      RelationalOperator.Contains,
      RelationalOperator.EndsWith,
    };
    #endregion

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
      catch (Exception exception)
      {
        _logger.Error("Error in OpenSession: project \"" + _appSettings.ProjectName + "\" application \"" + _appSettings.ApplicationName + "\"" + exception);
        throw new Exception("Error while openning session " + exception);
      }
    }

    public IDataObject Create(string objectType, string identifier)
    {
      try
      {
        return CreateList(objectType, new List<string>{ identifier }).FirstOrDefault();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Create: " + exception);
        throw new Exception("Error while creating data of type " + objectType + ".", exception);
      }
    }

    public IList<IDataObject> CreateList(string objectType, List<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        foreach (string identifier in identifiers)
        {
          Type type = Type.GetType(NAMESPACE + ".proj_" + _appSettings.ProjectName + "." + _appSettings.ApplicationName + "." + objectType);
          IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

          dataObject.SetPropertyValue("Id", identifier);
          dataObjects.Add(dataObject);
        }

        return dataObjects;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in CreateList: " + exception);
        throw new Exception("Error while creating a list of data of type " + objectType + ".", exception);
      }
    }

    public IDataObject Get(string objectType, string identifier)
    {
      try
      {
        StringBuilder queryString = new StringBuilder();
        using (ISession session = OpenSession())
        {
          queryString.Append(" from " + objectType + " where Id = '" + identifier + "'");

          IQuery query = session.CreateQuery(queryString.ToString());
          return query.List<IDataObject>().FirstOrDefault();
        } 
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Get: " + exception);
        throw new Exception("Error while getting data of type " + objectType + ".", exception);
      }
    }

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        StringBuilder queryString = new StringBuilder();
        using (ISession session = OpenSession())
        {
          queryString.Append("select Id from " + objectType);

          if (filter.Expressions.Count > 0)
          {
            string nhWhereClause = GenerateNHWhereClause(objectType, filter);
            queryString.Append(nhWhereClause);
          }

          IQuery query = session.CreateQuery(queryString.ToString());
          return query.List<string>();
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetIdentifiers: " + exception);
        throw new Exception("Error while getting a list of identifiers of type " + objectType + ".", exception);
      }
    }

    public IList<IDataObject> GetList(string objectType, List<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        foreach (string identifier in identifiers)
        {
          dataObjects.Add(Get(objectType, identifier));
        }

        return dataObjects;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetList: " + exception);
        throw new Exception("Error while getting a list of data of type " + objectType + ".", exception);
      }
    }

    //TODO: need to handle paging
    public IList<IDataObject> GetList(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      try
      {
        StringBuilder queryString = new StringBuilder();
        using (ISession session = OpenSession())
        {
          queryString.Append(" from " + objectType);

          if (filter.Expressions.Count > 0)
          {
            string nhWhereClause = GenerateNHWhereClause(objectType, filter);
            queryString.Append(nhWhereClause);
          }

          IQuery query = session.CreateQuery(queryString.ToString());
          return query.List<IDataObject>();
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetList: " + exception);
        throw new Exception("Error while getting a list of data of type " + objectType + ".", exception);
      }
    }

    private string GenerateNHWhereClause(string objectType, DataFilter filter)
    {
      StringBuilder nhWhereClause = new StringBuilder();
      try
      {
        if (filter.Expressions.Count > 0)
        {
          nhWhereClause.Append(" where ");
        }

        foreach (Expression expression in filter.Expressions)
        {
          if (expression.OpenGroupCount > 0)
          {
            string openParenthesis = String.Empty.PadRight(expression.OpenGroupCount, '(');
            openParenthesis = openParenthesis.PadRight(expression.OpenGroupCount + 1);
            nhWhereClause.Append(openParenthesis);
          }

          string propertyName = expression.PropertyName;
          propertyName = propertyName.PadRight(propertyName.Length + 1);
          nhWhereClause.Append(propertyName);

          string relationalOperator = ResolveNHRelationalOperator(expression.RelationalOperator);
          relationalOperator = relationalOperator.PadRight(propertyName.Length + 1);
          nhWhereClause.Append(relationalOperator);

          Type propertyType = Type.GetType(objectType).GetProperty(propertyName).PropertyType;
          bool isString = propertyType == typeof(string);

          if (expression.RelationalOperator == RelationalOperator.In)
          {
            nhWhereClause.Append("(");

            foreach (string value in expression.Values)
            {
              if (nhWhereClause.ToString() != "(")
              {
                nhWhereClause.Append(", ");
              }

              if (isString)
              {
                nhWhereClause.Append("'" + value + "'");
              }
              else
              {
                nhWhereClause.Append(value);
              }
            }

            nhWhereClause.Append(")");
          }
          else
          {
            if (LikeOperators.Contains(expression.RelationalOperator))
            {
              if (isString)
              {
                switch (expression.RelationalOperator)
                {
                  case RelationalOperator.BeginsWith:
                    string value = "'" + expression.Values.FirstOrDefault() + "'";
                    break;
                }
              }
              else
              {
                _logger.Error("Error in GenerateNHFilter: like operator used with non-string property");
                throw new Exception("Error while generating an NHibernate filter. Like operator used with non-string property");
              }
            }
            else
            {
              if (isString)
              {
                string value = "'" + expression.Values.FirstOrDefault() + "'";
              }
              else
              {
                string value = expression.Values.FirstOrDefault();
              }
            }
            propertyName = propertyName.PadRight(propertyName.Length + 1);
            nhWhereClause.Append(propertyName);
          }
        }

        return nhWhereClause.ToString();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GenerateNHFilter: " + exception);
        throw new Exception("Error while generating an NHibernate filter.", exception);
      }
    }

    private string ResolveNHRelationalOperator(RelationalOperator relationalOperator)
    {
      string nhRelationalOperator = String.Empty;
      try
      {
        switch (relationalOperator)
        {
          case RelationalOperator.BeginsWith:
            nhRelationalOperator = "like";
            break;

          case RelationalOperator.Contains:
            nhRelationalOperator = "like";
            break;

          case RelationalOperator.EndsWith:
            nhRelationalOperator = "like";
            break;

          case RelationalOperator.EqualTo:
            nhRelationalOperator = "=";
            break;

          case RelationalOperator.GreaterThan:
            nhRelationalOperator = ">";
            break;

          case RelationalOperator.GreaterThanOrEqual:
            nhRelationalOperator = ">=";
            break;

          case RelationalOperator.In:
            nhRelationalOperator = "in";
            break;

          case RelationalOperator.LesserThan:
            nhRelationalOperator = "<";
            break;

          case RelationalOperator.LesserThanOrEqual:
            nhRelationalOperator = "<=";
            break;

          case RelationalOperator.NotEqualTo:
            nhRelationalOperator = "<>";
            break;
        }

        return nhRelationalOperator;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in ResolveNHRelationalOperator: " + exception);
        throw new Exception("Error while resolving an NHibernate relational operator.", exception);
      }
    }

    public Response Post(IDataObject dataObject)
    {
      Response response;
      try
      {
        response = new Response();
        using (ISession session = OpenSession())
        {
          session.SaveOrUpdate(dataObject);
          session.Flush();
          response.Add("Records of type " + dataObject.GetType().Name + " have been updated successfully");
        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Post: " + exception);
        throw new Exception("Error while posting data of type " + dataObject.GetType().Name + ".", exception);
      }
    }

    public Response PostList(List<IDataObject> dataObjects)
    {
      Response response;
      
      try
      {
        response = new Response();
        foreach (IDataObject dataObject in dataObjects)
        {
          try
          {
            Response responseGraph = Post(dataObject);
            response.Append(responseGraph);
          }
          catch (Exception exception)
          {
            response.Add(exception.ToString());
          }
        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in PostList: " + exception);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception("Error while posting data of type " + objectType + ".", exception);
      }
    }

    public Response Delete(string objectType, string identifier)
    {
      Response response;
      try
      {
        response = new Response();
        StringBuilder queryString = new StringBuilder();
        
        using (ISession session = OpenSession())
        {
          queryString.Append(" from " + objectType + " where Id = '" + identifier + "'");

          session.Delete(queryString.ToString());

          response.Add("Record (" + objectType + ") has been deleted succesfully");
        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Delete: " + exception);
        throw new Exception("Error while deleting data of type " + objectType + ".", exception);
      }
    }

    public Response DeleteList(string objectType, DataFilter filter)
    {
      Response response;
      try
      {
        response = new Response();
        StringBuilder queryString = new StringBuilder();

        using (ISession session = OpenSession())
        {
          queryString.Append(" from " + objectType);

          if (filter.Expressions.Count > 0)
          {
            string nhWhereClause = GenerateNHWhereClause(objectType, filter);
            queryString.Append(nhWhereClause);
          }

          session.Delete(queryString.ToString());

          response.Add("Record (" + objectType + ") has been deleted succesfully");
        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in DeleteList: " + exception);
        throw new Exception("Error while deleting data of type " + objectType + ".", exception);
      }
    }

    public DataDictionary GetDictionary()
    {
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }
  }
}
