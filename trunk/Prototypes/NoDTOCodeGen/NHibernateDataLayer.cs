using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using System.Reflection;
using NHibernate.Cfg;
using System.Collections;

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
        return Activator.CreateInstance(type);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public T GetPropertyValue<T>(object dataObject, string propertyName)
    {
      PropertyInfo propInfo = dataObject.GetType().GetProperty(propertyName);

      if (propInfo != null)
      {
        return (T)propInfo.GetValue(dataObject, null);
      }

      return default(T);
    }

    public void SetPropertyValue<T>(object dataObject, string propertyName, T value)
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
        ITransaction transaction = session.BeginTransaction();
        session.SaveOrUpdate(dataObject);
        transaction.Commit();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void PostList(List<object> dataObjects)
    {
      try
      {
        foreach (object dataObject in dataObjects)
        {
          Post(dataObject);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }
}
