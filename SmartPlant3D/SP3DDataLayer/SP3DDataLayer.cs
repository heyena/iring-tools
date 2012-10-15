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
    private DataDictionary _dataDictionary = null;
    Dictionary<string, IList<IDataObject>> _sourceDataObjects = null;

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
      return sp3dProvider.GetDictionary();
    }

    private void getSourceDataObjects(BusinessCommodity bco)
    {
      DataObject loopDobj = null;
      string propertyName = string.Empty;
      List<IDataObject> dataObjects = null;
      string commodityName = bco.commodityName, objectName = string.Empty;
      long totalCount = GetCount(commodityName, null);

      if (_sourceDataObjects == null)
        _sourceDataObjects = new Dictionary<string, IList<IDataObject>>();

      BusinessObject mainBObj = bco.GetBusinessObject(_dataDictionary.dataObjects.First().objectName);
      
      try
      {
        ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

        for (int i = 1; i < _dataDictionary.dataObjects.Count; i++)
        {
          loopDobj = _dataDictionary.dataObjects[i];

          if (mainBObj.GetRelation(loopDobj.objectName) == null)
          {
            if (loopDobj == null)
            {
              throw new Exception("Object type [" + commodityName + "." + objectName + "] is not found.");
            }

            objectName = loopDobj.objectName;
            string ns = String.IsNullOrEmpty(loopDobj.objectNamespace)
              ? String.Empty : (loopDobj.objectNamespace + ".");

            Type type = Type.GetType(ns + objectName + ", " + _settings["ExecutingAssemblyName"]);

            // make an exception for tests
            if (type == null)
            {
              type = Type.GetType(ns + objectName + ", NUnit.Tests");
            }

            ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, loopDobj, null);

            dataObjects = new List<IDataObject>();
            int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 2000;
            int numOfRows = 0;

            while (numOfRows < totalCount)
            {
              criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
              dataObjects.AddRange(criteria.List<IDataObject>());
              numOfRows += internalPageSize;
            }
            _sourceDataObjects.Add(objectName, dataObjects);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}.{1}]. {2}", commodityName, objectName, ex));
      }
    }

    private IList<IDataObject> getDataObjectRows(BusinessCommodity bo, string objectName, string commodityName, DataFilter filter, int startIndex, int pageSize)
    {
      long totalCount = 0;
      DataObject objectDefinition = null;
      string propertyName = string.Empty;
      List<IDataObject> dataObjects = new List<IDataObject>();
      string rowPropertyName = string.Empty;
      string oidOrigin = string.Empty;        

      BusinessObject bObj = bo.GetBusinessObject(objectName);
      DataObject objectDef = _dataDictionary.GetDataObject(objectName);

      try
      {
        ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

        if (sp3dProvider._databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = sp3dProvider._databaseDictionary.IdentityConfiguration[commodityName];
          if (identityProperties.UseIdentityFilter)
          {
            filter = sp3dProvider.FilterByIdentity(objectName, filter, identityProperties);
          }
        }

        objectDefinition = _dataDictionary.GetDataObject(objectName);

        if (objectDefinition == null)
        {
          throw new Exception("Object type [" + commodityName + "." + objectName + "] is not found.");
        }

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectName + ", " + _settings["ExecutingAssemblyName"]);

        // make an exception for tests
        if (type == null)
        {
          type = Type.GetType(ns + objectName + ", NUnit.Tests");
        }

        ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, objectDefinition, filter);

        if (pageSize == 0 && startIndex == 0)
        {
          totalCount = GetCount(commodityName, filter);
          int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 1000;
          int numOfRows = 0;

          while (numOfRows < totalCount)
          {
            criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
            dataObjects.AddRange(criteria.List<IDataObject>());
            numOfRows += internalPageSize;
          }
        }
        else
        {
          criteria.SetFirstResult(startIndex).SetMaxResults(pageSize);
          IList<IDataObject> IdataObjects = criteria.List<IDataObject>();

          foreach (IDataObject addItem in IdataObjects)
            dataObjects.Add(addItem);
        }

        foreach (IDataObject row in dataObjects)
        {          
          foreach (string connectedEntityName in bObj.rightClassNames)
          {
            BusinessRelation relation = bObj.GetRelation(connectedEntityName);
            oidOrigin = row.GetPropertyValue(relation.relationName + "_" + relation.businessKeyProperties.First().columnName).ToString();

            foreach (string con in relation.rightClassNames)
            {
              RelatedObject relatedObject = bObj.GetRelatedObject(con);
              setRow(row, oidOrigin, relatedObject, bObj);
            }
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}.{1}]. {2}", commodityName, objectName, ex));
      }
    }

    private void setRow(IDataObject targetRow, string oidOrigin, RelatedObject relatedObject, BusinessObject bObj)
    {
      string newOidOrigin = string.Empty;
      string relationKey = bObj.GetRelation(relatedObject.relationName).businessKeyProperties.First().columnName;

      foreach (IDataObject row1 in _sourceDataObjects[relatedObject.objectName])
      {
        if (row1.GetPropertyValue(relatedObject.businessKeyProperties.First().columnName).ToString().ToLower() == oidOrigin.ToLower())
        {
          if (row1.GetPropertyValue(relationKey) != null)
            newOidOrigin = row1.GetPropertyValue(relationKey).ToString();

          foreach (BusinessProperty bp in relatedObject.businessProperties)
          {
            targetRow.SetPropertyValue(bp.propertyName, row1.GetPropertyValue(bp.propertyName));
          }                
          break;
        }
      }

      if (relatedObject.relatedObjects != null && !string.IsNullOrEmpty(newOidOrigin))
      {
        if (relatedObject.relatedObjects.Count > 0)
        {
          foreach (RelatedObject newRelatedObject in relatedObject.relatedObjects)
          {
            setRow(targetRow, newOidOrigin, newRelatedObject, bObj);
          }
        }
      }
      else
        return;      
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      ISession session = null;
      int startObjectIndex = 0, endObjectIndex = 0, endPartialPageRowNumber = 0, startPartialPageRowNumber = 0, rowsAccount = 0, minusValue = 0;
      string commodityName = string.Empty;
      int minNumberOfRows = 0;
      bool dictionaryInMemory = true, readConfig = true;
      int numberOfObjects = 0, objectPageSize = 0;

      try
      {
        if (sp3dProvider == null)
        {
          setSP3DProviderSettings();
        }

        if (sp3dProvider._sp3dDataBaseDictionary != null)
        {
          if (sp3dProvider._sp3dDataBaseDictionary.businessCommodities != null)
            readConfig = false;
        }

        if (readConfig)
        {
          sp3dProvider._sp3dDataBaseDictionary = Utility.Read<BusinessObjectConfiguration>(string.Format("{0}BusinessObjectConfiguration.{1}.xml", _settings["AppDataPath"], _settings["Scope"]));
        }

        if (sp3dProvider._sp3dDataBaseDictionary != null)
          if (sp3dProvider._sp3dDataBaseDictionary.businessCommodities != null)
          {
            BusinessCommodity businessCommodity = sp3dProvider._sp3dDataBaseDictionary.GetBusinessCommoditiy(objectType);
            numberOfObjects = businessCommodity.businessObjects.Count;
            commodityName = objectType.ToLower();

            if (_dataDictionary != null)
            {
              if (_dataDictionary.dataObjects != null)
              {
                if (_dataDictionary.dataObjects.First().objectNamespace.Split('.').Last() != commodityName)
                {
                  dictionaryInMemory = false;
                }
              }
              else
                dictionaryInMemory = false;
            }
            else
              dictionaryInMemory = false;

            if (!dictionaryInMemory)
              _dataDictionary = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _settings["AppDataPath"], _settings["Scope"], commodityName));

            List<IDataObject> dataObjects = new List<IDataObject>();

            if (businessCommodity.hasMinusOrZeroRowNumbers())
              GetCount(objectType, filter);

            minusValue = startIndex - rowsAccount;

            while (minusValue >= 0 && startObjectIndex < numberOfObjects)
            {
              rowsAccount += (int)businessCommodity.businessObjects[startObjectIndex].rowNumber;
              startObjectIndex++;
              minusValue = startIndex - rowsAccount;
            }

            getSourceDataObjects(businessCommodity);

            startPartialPageRowNumber = (int)businessCommodity.businessObjects[startObjectIndex - 1].rowNumber + minusValue;
            minNumberOfRows = Math.Min(0 - minusValue, pageSize);
            dataObjects.AddRange(getDataObjectRows(businessCommodity, businessCommodity.businessObjects[startObjectIndex - 1].objectName, commodityName, filter, startPartialPageRowNumber, minNumberOfRows));

            if (minNumberOfRows == pageSize || startObjectIndex >= numberOfObjects)
              return dataObjects;

            endObjectIndex = startObjectIndex;
            rowsAccount = 0;
            objectPageSize = pageSize + minusValue;
            minusValue = objectPageSize - rowsAccount;

            if (_sourceDataObjects == null)
              getSourceDataObjects(businessCommodity);

            while (minusValue >= 0 && endObjectIndex < numberOfObjects)
            {
              rowsAccount += (int)businessCommodity.businessObjects[endObjectIndex].rowNumber;
              endObjectIndex++;
              minusValue = objectPageSize - rowsAccount;
            }

            for (int i = startObjectIndex; i < endObjectIndex - 1; i++)
            {
              dataObjects.AddRange(getDataObjectRows(businessCommodity, businessCommodity.businessObjects[i].objectName, commodityName, filter, 0, (int)businessCommodity.businessObjects[i].rowNumber));
            }

            minNumberOfRows = Math.Min(0 - minusValue, objectPageSize);

            endPartialPageRowNumber = (int)businessCommodity.businessObjects[endObjectIndex - 1].rowNumber + minusValue;
            dataObjects.AddRange(getDataObjectRows(businessCommodity, businessCommodity.businessObjects[endObjectIndex - 1].objectName, commodityName, filter, 0, objectPageSize));
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
      return null;
    }


    public long GetCountSP3D(string objectType, DataFilter filter, string commodityName, DataDictionary dataDictionary)
    {
      ISession session = NHibernateSessionManager.Instance.GetSessionSP3D(_settings["AppDataPath"], _settings["Scope"], commodityName);

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

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select count(*) from " + objectType);

        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          DataFilter clonedFilter = Utility.CloneDataContractObject<DataFilter>(filter);
          clonedFilter.OrderExpressions = null;
          DataObject dataObject = dataDictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
          string whereClause = clonedFilter.ToSqlWhereClause(dataDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        long count = query.List<long>().First();
        return count;
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

    public override long GetCount(string objectType, DataFilter filter)
    {
      long totalCount = 0, objectRowNumber = 0;
      string objectName = string.Empty;
      BusinessObject businessObject = null;

      try
      {
        if (sp3dProvider == null)
        {
          setSP3DProviderSettings();
        }

        if (sp3dProvider._sp3dDataBaseDictionary != null)
          if (sp3dProvider._sp3dDataBaseDictionary.businessCommodities != null)
          {
            BusinessCommodity businessCommodity = sp3dProvider._sp3dDataBaseDictionary.GetBusinessCommoditiy(objectType);
            string commodityName = objectType.ToLower();
            if (_dataDictionary == null)
              _dataDictionary = Utility.Read<DataDictionary>(string.Format("{0}DataDictionary.{1}.{2}.xml", _settings["AppDataPath"], _settings["Scope"], commodityName));

            foreach (DataObject dataObject in _dataDictionary.dataObjects)
            {
              objectName = dataObject.objectName;

              if (businessCommodity.GetBusinessObject(objectName) != null)
              {
                businessObject = businessCommodity.GetBusinessObject(objectName);
                objectRowNumber = GetCountSP3D(dataObject.objectName, filter, commodityName, _dataDictionary);
                businessObject.rowNumber = objectRowNumber;
                totalCount += objectRowNumber;
              }
            }
            return totalCount;
          }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);

        throw new Exception(
          "Error while getting a count of type [" + objectType + "].",
          ex
        );
      }
      return -1;
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
      return response;
    }

    public override Response Refresh(string objectType)
    {
      return RefreshAll();
    }

    public override Response RefreshAll()
    {
      Response response = new Response();

      try
      {
        sp3dProvider._databaseDictionary = null;
        System.IO.File.Delete(sp3dProvider._dictionaryPath);
        GetDictionary();
        response.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages = new Messages() { e.Message };
      }

      return response;
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







