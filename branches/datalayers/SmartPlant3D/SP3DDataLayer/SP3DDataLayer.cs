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

    //public override Response Refresh(string objectType, DataFilter filter)
    //{
    //  if (sp3dProvider == null)
    //  {
    //    setSP3DProviderSettings();
    //  }

    //  return sp3dProvider.RefreshCachingTable(objectType, filter);
    //}

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

    public DataObject GetObjectDefinition(string objectType)
    {
      DataDictionary dictionary = GetDictionary();
      DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
      return objDef;
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.PostSP3DBusinessObjects(dataObjects);
    }       

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (sp3dProvider._databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = sp3dProvider._databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        StringBuilder queryString = new StringBuilder();
        queryString.Append("select Id from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          DataObject dataObject = sp3dProvider._databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = filter.ToSqlWhereClause(sp3dProvider._databaseDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        return query.List<string>();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        sp3dProvider.CloseSession(session);
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }
      
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          DataObject dataObjectDef = (from DataObject o in sp3dProvider._databaseDictionary.dataObjects
                                      where o.objectName == objectType
                                      select o).FirstOrDefault();

          if (dataObjectDef == null)
            return null;

          if (dataObjectDef.keyProperties.Count == 1)
          {
            queryString.Append(" where Id in ('" + String.Join("','", identifiers.ToArray()) + "')");
          }
          else if (dataObjectDef.keyProperties.Count > 1)
          {
            string[] keyList = null;
            int identifierIndex = 1;
            foreach (string identifier in identifiers)
            {
              string[] idParts = identifier.Split(dataObjectDef.keyDelimeter.ToCharArray()[0]);

              keyList = new string[idParts.Count()];

              int partIndex = 0;
              foreach (string part in idParts)
              {
                if (identifierIndex == identifiers.Count())
                {
                  keyList[partIndex] += part;
                }
                else
                {
                  keyList[partIndex] += part + ", ";
                }

                partIndex++;
              }

              identifierIndex++;
            }

            int propertyIndex = 0;
            foreach (KeyProperty keyProperty in dataObjectDef.keyProperties)
            {
              string propertyValues = keyList[propertyIndex];

              if (propertyIndex == 0)
              {
                queryString.Append(" where " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }
              else
              {
                queryString.Append(" and " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }

              propertyIndex++;
            }
          }
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        IList<IDataObject> dataObjects = query.List<IDataObject>();

        // order data objects as list of identifiers
        if (identifiers != null)
        {
          IList<IDataObject> orderedDataObjects = new List<IDataObject>();

          foreach (string identifier in identifiers)
          {
            if (identifier != null)
            {
              foreach (IDataObject dataObject in dataObjects)
              {
                if (dataObject.GetPropertyValue("Id").ToString().ToLower() == identifier.ToLower())
                {
                  orderedDataObjects.Add(dataObject);
                  //break;  // include dups also
                }
              }
            }
          }

          return orderedDataObjects;
        }

        return dataObjects;
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

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = Create(objectType, identifiers);

        foreach (IDataObject dataObject in dataObjects)
        {
          string identifier = dataObject.GetPropertyValue("Id").ToString();
          session.Delete(dataObject);

          Status status = new Status();
          status.Messages = new Messages();
          status.Identifier = identifier;
          status.Messages.Add(string.Format("Record [{0}] deleted successfully.", identifier));

          response.Append(status);
        }

        session.Flush();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        response.Append(status);
      }
      finally
      {
        sp3dProvider.CloseSession(session);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.DeleteSP3DBusinessObjects(objectType, filter);
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      IList<IDataObject> relatedObjects = null;
      ISession session = null;

      try
      {
        DataObject dataObject = sp3dProvider._dataDictionary.dataObjects.Find(c => c.objectName.ToLower() == parentDataObject.GetType().Name.ToLower());
        if (dataObject == null)
        {
          throw new Exception("Parent data object [" + parentDataObject.GetType().Name + "] not found.");
        }

        DataRelationship dataRelationship = dataObject.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedObjectType.ToLower());
        if (dataRelationship == null)
        {
          throw new Exception("Relationship between data object [" + parentDataObject.GetType().Name +
            "] and related data object [" + relatedObjectType + "] not found.");
        }

        session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

        StringBuilder sql = new StringBuilder();
        sql.Append("from " + dataRelationship.relatedObjectName + " where ");

        foreach (PropertyMap map in dataRelationship.propertyMaps)
        {
          DataProperty propertyMap = dataObject.dataProperties.First(c => c.propertyName == map.dataPropertyName);

          if (propertyMap.dataType == DataType.String)
          {
            sql.Append(map.relatedPropertyName + " = '" + parentDataObject.GetPropertyValue(map.dataPropertyName) + "' and ");
          }
          else
          {
            sql.Append(map.relatedPropertyName + " = " + parentDataObject.GetPropertyValue(map.dataPropertyName) + " and ");
          }
        }

        sql.Remove(sql.Length - 4, 4);  // remove the tail "and "
        IQuery query = session.CreateQuery(sql.ToString());
        relatedObjects = query.List<IDataObject>();

        if (relatedObjects != null && relatedObjects.Count > 0 && dataRelationship.relationshipType == RelationshipType.OneToOne)
        {
          return new List<IDataObject> { relatedObjects.First() };
        }

        return relatedObjects;
      }
      catch (Exception e)
      {
        string error = "Error getting related objects [" + relatedObjectType + "] " + e;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        sp3dProvider.CloseSession(session);
      }
    }

    public override long GetRelatedCount(IDataObject parentDataObject, string relatedObjectType)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return GetCount(relatedObjectType, filter);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related object count for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType, int pageSize, int startIndex)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return Get(relatedObjectType, filter, pageSize, startIndex);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related objects for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    private DataFilter FilterByIdentity(string objectType, DataFilter filter, IdentityProperties identityProperties)
    {
      DataObject dataObject = sp3dProvider._databaseDictionary.dataObjects.Find(d => d.objectName == objectType);
      DataProperty dataProperty = dataObject.dataProperties.Find(p => p.columnName == identityProperties.IdentityProperty);

      if (dataProperty != null)
      {
        if (filter == null)
        {
          filter = new DataFilter();
        }
        
        if (filter.Expressions == null)
        {
          filter.Expressions = new List<org.iringtools.library.Expression>();
        }
        else if (filter.Expressions.Count > 0)
        {
          org.iringtools.library.Expression firstExpression = filter.Expressions.First();
          org.iringtools.library.Expression lastExpression = filter.Expressions.Last();
          firstExpression.OpenGroupCount++;
          lastExpression.CloseGroupCount++;          
        }
      }

      return filter;
    }

    private void setSP3DProviderSettings()
    {
      ServiceSettings servcieSettings = new ServiceSettings();
      _settings.AppendSettings(servcieSettings);
      sp3dProvider = new SP3DProvider(_settings);
    }
  }
}







