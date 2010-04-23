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

    private readonly List<RelationalOperator> LikeOperators = new List<RelationalOperator>
    {
      RelationalOperator.StartsWith, 
      RelationalOperator.Contains,
      RelationalOperator.EndsWith,
    };

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
          string nhWhereClause = GenerateNHWhereClause(objectType, filter);
          queryString.Append(nhWhereClause);
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
          string nhWhereClause = GenerateNHWhereClause(objectType, filter);
          queryString.Append(nhWhereClause);
        }

        using (ISession session = OpenSession())
        {
          IQuery query = session.CreateQuery(queryString.ToString());
          IList<IDataObject> dataObjects = query.List<IDataObject>();
          
          if (pageSize > 0 && pageNumber > 0 && dataObjects.Count > (pageSize * (pageNumber - 1) + pageSize))
          {
            dataObjects = dataObjects.ToList().GetRange(pageSize * (pageNumber - 1), pageSize);
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
          string nhWhereClause = GenerateNHWhereClause(objectType, filter);
          queryString.Append(nhWhereClause);
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

    private string GenerateNHWhereClause(string objectType, DataFilter filter)
    {
      if (filter == null || filter.Expressions.Count == 0)
      {
        return String.Empty;
      }

      try
      {
        StringBuilder whereClause = new StringBuilder();
        whereClause.Append(" WHERE");

        foreach (Expression expression in filter.Expressions)
        {
          if (expression.LogicalOperator != LogicalOperator.None)
          {
            whereClause.Append(" " + ResolveNHLogicalOperator(expression.LogicalOperator));
          }

          for (int i = 0; i < expression.OpenGroupCount; i++)
          {
            whereClause.Append("(");
          }

          string propertyName = expression.PropertyName;
          whereClause.Append(" " + propertyName);

          string relationalOperator = ResolveNHRelationalOperator(expression.RelationalOperator);
          whereClause.Append(" " + relationalOperator);

          Type propertyType = Type.GetType(objectType).GetProperty(propertyName).PropertyType;
          bool isString = propertyType == typeof(string);

          if (expression.RelationalOperator == RelationalOperator.In)
          {
            whereClause.Append("(");

            foreach (string value in expression.Values)
            {
              if (whereClause.ToString() != "(")
              {
                whereClause.Append(", ");
              }

              if (isString)
              {
                whereClause.Append("'" + value + "'");
              }
              else
              {
                whereClause.Append(value);
              }
            }

            whereClause.Append(")");
          }
          else
          {
            string value = String.Empty;

            if (LikeOperators.Contains(expression.RelationalOperator))
            {
              if (isString)
              {
                value = "\"(" + expression.Values.FirstOrDefault() + "\")";
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
                value = "'" + expression.Values.FirstOrDefault() + "'";
              }
              else
              {
                value = expression.Values.FirstOrDefault();
              }
            }

            whereClause.Append(value);
          }

          for (int i = 0; i < expression.CloseGroupCount; i++)
          {
            whereClause.Append(")");
          }
        }

        return whereClause.ToString();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GenerateNHFilter: " + ex);
        throw new Exception("Error while generating an NHibernate filter.", ex);
      }
    }

    private string ResolveNHRelationalOperator(RelationalOperator relationalOperator)
    {
      string nhRelationalOperator = String.Empty;

      switch (relationalOperator)
      {
        case RelationalOperator.StartsWith:
          nhRelationalOperator = "LIKE";
          break;

        case RelationalOperator.Contains:
          nhRelationalOperator = "LIKE";
          break;

        case RelationalOperator.EndsWith:
          nhRelationalOperator = "LIKE";
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
          nhRelationalOperator = "IN";
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

    private string ResolveNHLogicalOperator(LogicalOperator logicalOperator)
    {
      string nhLogicalOperator = String.Empty;

      switch (logicalOperator)
      {
        case LogicalOperator.And:
          nhLogicalOperator = "AND";
          break;

        case LogicalOperator.AndNot:
          nhLogicalOperator = "AND NOT";
          break;

        case LogicalOperator.Not:
          nhLogicalOperator = "NOT";
          break;

        case LogicalOperator.Or:
          nhLogicalOperator = "OR";
          break;

        case LogicalOperator.OrNot:
          nhLogicalOperator = "OR NOT";
          break;
      }

      return nhLogicalOperator;
    }
  }
}
