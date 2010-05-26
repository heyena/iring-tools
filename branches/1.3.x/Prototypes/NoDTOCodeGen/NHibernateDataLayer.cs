using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
//using System.Reflection;
using NHibernate.Cfg;
using System.Collections;
using System.Reflection;

namespace AdapterPrototype
{
  public class NHibernateDataLayer : IDataLayer
  {
    private static ISession OpenSession()
    {
      string configPath = @"..\..\nh-configuration.xml";
      string mappingPath = @"..\..\nh-mapping.xml";

      ISessionFactory factory = new Configuration()
        .Configure(configPath)
        .AddFile(mappingPath)
        .BuildSessionFactory();

      return factory.OpenSession();
    }

    public object Create(string dataObjectName)
    {
      try
      {
        Type type = Type.GetType(dataObjectName);
        return (object)Activator.CreateInstance(type);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public IList<object> CreateList(string dataObjectName, int count)
    {
      ISession session = OpenSession();
      ICriteria criteria = session.CreateCriteria(dataObjectName);
      criteria.SetMaxResults(count);
      return criteria.List<object>();
    }

    public object GetPropertyValue(object dataObject, string propertyName)
    {
      PropertyInfo propInfo = dataObject.GetType().GetProperty(propertyName);

      if (propInfo != null)
      {
        return propInfo.GetValue(dataObject, null);
      }

      return null;
    }

    public void SetPropertyValue(object dataObject, string propertyName, object value)
    {
      PropertyInfo propInfo = dataObject.GetType().GetProperty(propertyName);

      if (propInfo != null)
      {
        propInfo.SetValue(dataObject, value, null);
      }
    }

    public object Get(string dataObjectName, string filter)
    {
      IList<object> dataObjects = GetList(dataObjectName, filter);
      return dataObjects.FirstOrDefault<object>();
    }

    public IList<object> GetList(string dataObjectName, string filter)
    {
      ISession session = OpenSession();
      IQuery query = session.CreateQuery("from " + dataObjectName + ((filter != null) ? "" : filter));
      return query.List<object>();
    }

    public void Post(object dataObject)
    {
      try
      {
        ISession session = OpenSession();
        session.SaveOrUpdate(dataObject);
        session.Flush();

        //ISession session = OpenSession();
        //ITransaction transaction = session.BeginTransaction();
        //session.SaveOrUpdate(dataObject);
        //transaction.Commit();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void PostList(List<object> dataObjects)
    {
      //try
      //{
      //  ISession session = OpenSession();
      //  ITransaction transaction = session.BeginTransaction();
      //  foreach (object dataObject in dataObjects)
      //  {
      //    session.SaveOrUpdate(dataObject);
      //  }
      //  transaction.Commit();        
      //}
      //catch (Exception ex)
      //{
      //  throw ex;
      //}

      foreach (object dataObject in dataObjects)
      {
        try
        {
          Post(dataObject);
        }
        catch (Exception ex)
        {
          throw ex; // add to response
        }
      }
    }
  }
}
