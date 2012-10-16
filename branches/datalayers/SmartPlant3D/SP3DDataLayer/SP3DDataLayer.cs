using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using Ingr.SP3D.Common.Middle.Services;
using Ingr.SP3D.Common.Middle;
using Ingr.SP3D.Structure.Middle;
using Ingr.SP3D.ReferenceData.Middle;
using Ingr.SP3D.Systems.Middle;
using Ingr.SP3D.ReferenceData.Middle.Services;
using NHibernate;
using Ninject.Extensions.Xml;

namespace iringtools.sdk.sp3ddatalayer
{
  public class SP3DDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SP3DDataLayer));
    private SP3DProvider sp3dProvider = null;
    
    [Inject]
    public SP3DDataLayer(AdapterSettings settings)
      : base(settings)
    {
      ServiceSettings servcieSettings = new ServiceSettings();
      _settings.AppendSettings(servcieSettings);
      _settings.AppendSettings(settings);
      sp3dProvider = new SP3DProvider(_settings);
    }

    public override DataDictionary GetDictionary()
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      DataDictionary dataDictionary = sp3dProvider.GetDictionary();
      return dataDictionary;
    }

    public override Response Refresh(string objectType, DataFilter filter)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.RefreshCachingTable(objectType, filter);
    }

    public override Response Refresh(string objectType)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.RefreshCachingTables(objectType);
    }

    public override Response RefreshAll()
    {
      return Refresh(string.Empty);
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (sp3dProvider._databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = sp3dProvider._databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = sp3dProvider.FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        DataObject objectDefinition = sp3dProvider._databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());

        if (objectDefinition == null)
        {
          throw new Exception("Object type [" + objectType + "] not found.");
        }

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectType + ", " + _settings["ExecutingAssemblyName"]);

        // make an exception for tests
        if (type == null)
        {
          type = Type.GetType(ns + objectType + ", NUnit.Tests");
        }

        ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, objectDefinition, filter);

        if (pageSize == 0 && startIndex == 0)
        {
          List<IDataObject> dataObjects = new List<IDataObject>();
          long totalCount = GetCount(objectType, filter);
          int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 1000;
          int numOfRows = 0;

          while (numOfRows < totalCount)
          {
            criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
            dataObjects.AddRange(criteria.List<IDataObject>());
            numOfRows += internalPageSize;
          }

          return dataObjects;
        }
        else
        {
          criteria.SetFirstResult(startIndex).SetMaxResults(pageSize);
          IList<IDataObject> dataObjects = criteria.List<IDataObject>();
          return dataObjects;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        sp3dProvider.CloseSession(session);
      }
    }

    

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      throw new Exception("Error while getting a count of type ");
      //try
      //{
      //    List<string> identifiers = new List<string>();

      //    //NOTE: pageSize of 0 indicates that all rows should be returned.
      //    IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

      //    foreach (IDataObject dataObject in dataObjects)
      //    {
      //        identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
      //    }

      //    return identifiers;
      //}
      //catch (Exception ex)
      //{
      //    _logger.Error("Error in GetIdentifiers: " + ex);

      //    throw new Exception(
      //      "Error while getting a list of identifiers of type [" + objectType + "].",
      //      ex
      //    );
      //}
    }

    public DataObject GetObjectDefinition(string objectType)
    {
      DataDictionary dictionary = GetDictionary();
      DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
      return objDef;
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          string objectType = dataObjects[0].GetType().Name; 
          
          foreach (IDataObject dataObject in dataObjects)
          {
            Status status = new Status();
            status.Messages = new Messages();

            if (dataObject != null)
            {
              string identifier = String.Empty;

              try
              {
                // NOTE: Id property is not available if it's not mapped and will cause exception
                identifier = dataObject.GetPropertyValue("Id").ToString();
              }
              catch (Exception ex)
              {
                _logger.Error(string.Format("Error in Post: {0}", ex));
              }  // no need to handle exception because identifier is only used for statusing

              status.Identifier = identifier;

              try
              {
                session.SaveOrUpdate(dataObject);
                session.Flush();
                status.Messages.Add(string.Format("Record [{0}] saved successfully.", identifier));
              }
              catch (Exception ex)
              {
                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error while posting record [{0}]. {1}", identifier, ex));
                status.Results.Add("ResultTag", identifier);
                _logger.Error("Error posting data object to data layer: " + ex);
              }
            }
            else
            {
              status.Level = StatusLevel.Error;
              status.Identifier = String.Empty;
              status.Messages.Add("Data object is null or duplicate. See log for details.");
            }

            response.Append(status);
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
      finally
      {
        sp3dProvider.CloseSession(session);
      }
    }

    

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      throw new Exception("Error while getting a count of type ");
      //// Not gonna do it. Wouldn't be prudent.
      //Response response = new Response();
      //Status status = new Status();
      //status.Level = StatusLevel.Error;
      //status.Messages.Add("Delete not supported by the SP3D DataLayer.");
      //response.Append(status);
      //return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      throw new Exception("Error while getting a count of type ");
      //// Not gonna do it. Wouldn't be prudent with a filter either.
      //Response response = new Response();
      //Status status = new Status();
      //status.Level = StatusLevel.Error;
      //status.Messages.Add("Delete not supported by the SP3D DataLayer.");
      //response.Append(status);
      //return response;
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      throw new Exception("Error while getting a count of type ");
      //try
      //{
      //    LoadDataDictionary(objectType);

      //    IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

      //    var expressions = FormMultipleKeysPredicate(identifiers);

      //    if (expressions != null)
      //    {
      //        _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList();
      //    }

      //    return _dataObjects;
      //}
      //catch (Exception ex)
      //{
      //    _logger.Error("Error in GetList: " + ex);
      //    throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      //}
    }

    private void setSP3DProviderSettings()
    {
      ServiceSettings servcieSettings = new ServiceSettings();
      _settings.AppendSettings(servcieSettings);
      sp3dProvider = new SP3DProvider(_settings);
    }
  }
}







