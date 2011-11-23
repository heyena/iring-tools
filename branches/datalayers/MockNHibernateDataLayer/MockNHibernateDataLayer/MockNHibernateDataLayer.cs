using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using System.Collections;
using org.iringtools.library;
using System.IO;
using org.iringtools.nhibernate;

namespace org.iringtools.adapter.datalayer
{
  public class MockNHibernateDataLayer : NHibernateDataLayer
  {
    private DatabaseDictionary _dbDictionary;

    [Inject]
    public MockNHibernateDataLayer(AdapterSettings settings, IDictionary keyRing)
      : base(settings, keyRing) 
    {
      string dbDictionaryPath = string.Format("{0}DatabaseDictionary.{1}.xml",
        settings["AppDataPath"],
        settings["Scope"]
      );

      if (File.Exists(dbDictionaryPath))
      {
        _dbDictionary = NHibernateUtility.LoadDatabaseDictionary(dbDictionaryPath);
      }
    }

    public override Response Post(IList<library.IDataObject> dataObjects)
    {
      Response response = new Response();
      
      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          foreach (IDataObject dataObject in dataObjects)
          {
            Status status = new Status();
            status.Messages = new Messages();

            if (dataObject != null)
            {
              string identifier = dataObject.GetPropertyValue("Id").ToString();
              status.Identifier = identifier;
              status.Messages.Add(string.Format("[Mock] Record [{0}] have been saved successfully.", identifier));
            }
            else
            {
              status.Level = StatusLevel.Error;
              status.Identifier = String.Empty;
              status.Messages.Add("[Mock] Data object is null or duplicate. See log for details.");
            }

            response.Append(status);
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception(string.Format("[Mock] Error while posting data objects of type [{0}]. {1}", objectType, ex));
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();
      
      try
      {
        IList<IDataObject> dataObjects = Create(objectType, identifiers);

        foreach (IDataObject dataObject in dataObjects)
        {
          string identifier = dataObject.GetPropertyValue("Id").ToString();

          Status status = new Status();
          status.Messages = new Messages();
          status.Identifier = identifier;
          status.Messages.Add(string.Format("[Mock] Record [{0}] have been deleted successfully.", identifier));

          response.Append(status);
        }
      }
      catch (Exception ex)
      {
        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("[Mock] Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        response.Append(status);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      Response response = new Response();
      response.StatusList = new List<Status>();
      
      try
      {
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);
        
        foreach (IDataObject dataObject in dataObjects)
        {
          string identifier = dataObject.GetPropertyValue("Id").ToString();

          Status status = new Status();
          status.Messages = new Messages();
          status.Identifier = identifier;
          status.Messages.Add(string.Format("[Mock] Record [{0}] have been deleted successfully.", identifier));

          response.Append(status);
        }
      }
      catch (Exception ex)
      {
        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("[Mock] Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        response.Append(status);
      }

      return response;
    }
  }
}
