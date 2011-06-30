using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Data;
using log4net;
using System.Xml.Linq;
using org.iringtools.adapter;

namespace org.iringtools.library
{
  public abstract class BaseSQLDataLayer : IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseSQLDataLayer));
    private string _execAssemblyName;
    private DataDictionary _dataDictionary;

    #region BaseSQLDataLayer methods
    public BaseSQLDataLayer(AdapterSettings settings)
    {
      _execAssemblyName = settings["ExecutingAssemblyName"];
      _dataDictionary = GetDictionary();
    }

    // get number of rows with (optional) filter
    public abstract long GetCount(string tableName, string whereClause);

    // get list of identifiers with (optional) filter
    public abstract IList<string> GetIdentifiers(string tableName, string whereClause);

    // create or fetch data rows of given identifiers
    public abstract DataTable CreateDataTable(string tableName, IList<string> identifiers);

    // fetch data rows of given identifiers
    public abstract DataTable GetDataTable(string tableName, IList<string> identifiers);

    // delete data rows with filter
    public abstract Response DeleteDataTable(string tableName, string whereClause);

    // delete data rows by identifiers
    public abstract Response DeleteDataTable(string tableName, IList<string> identifiers);

    // get a page of data rows with (optional) filter
    public abstract DataTable GetDataTable(string tableName, string whereClause, int start, int limit);

    // get related data rows of a given data row
    public abstract DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName);

    // post data rows and its related items (data rows)
    public abstract Response PostDataTables(IList<DataTable> dataTables);
    #endregion

    #region IDataLayer implementation methods
    public abstract DataDictionary GetDictionary();
    public abstract Response Configure(XElement configuration);
    public abstract XElement GetConfiguration();

    public virtual IList<IDataObject> Create(string objectTypeName, IList<string> identifiers)
    {
      string tableName = GetTableName(objectTypeName);
      DataTable dataTable = CreateDataTable(tableName, identifiers);
      return ToDataObjects(dataTable, objectTypeName);
    }

    public virtual IList<IDataObject> Get(string objectTypeName, IList<string> identifiers)
    {
      string tableName = GetTableName(objectTypeName);
      DataTable dataTable = GetDataTable(tableName, identifiers);
      return ToDataObjects(dataTable, objectTypeName);
    }

    public virtual Response Delete(string objectTypeName, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public virtual Response Delete(string objectTypeName, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public virtual IList<IDataObject> Get(string objectTypeName, DataFilter filter, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public virtual long GetCount(string objectTypeName, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public virtual IList<string> GetIdentifiers(string objectTypeName, DataFilter filter)
    {
      string tableName = GetTableName(objectTypeName);
      string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectTypeName, "_t");
      return GetIdentifiers(tableName, whereClause);
    }

    public virtual IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    public virtual Response Post(IList<IDataObject> dataObjects)
    {
      List<DataTable> dataTables = new List<DataTable>();
      return PostDataTables(dataTables);
    }
    #endregion

    #region helper methods
    protected string GetTableName(string objectTypeName)
    {
      foreach (DataObject dataObject in _dataDictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == objectTypeName.ToLower())
        {
          return dataObject.tableName;
        }
      }

      return null;
    }

    protected DataObject GetObjectDefinition(string objectTypeName)
    {
      foreach (DataObject dataObject in _dataDictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == objectTypeName.ToLower())
        {
          return dataObject;
        }
      }

      return null;
    }

    protected Type GetObjectType(DataObject objectDefinition, string objectTypeName)
    {
      return Type.GetType(objectDefinition.objectNamespace + "." + objectTypeName + ", " + _execAssemblyName);
    }

    protected IList<IDataObject> ToDataObjects(DataTable dataTable, string objectTypeName)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();
      DataObject objectDefinition = GetObjectDefinition(objectTypeName);
      Type objectType = GetObjectType(objectDefinition, objectTypeName);

      if (objectDefinition != null)
      {
        foreach (DataRow row in dataTable.Rows)
        {
          IDataObject dataObject = null;

          try
          {
            dataObject = (IDataObject)Activator.CreateInstance(objectType);
          }
          catch (Exception ex)
          {
            _logger.Error("Error instantiating data object: " + ex);
            throw ex;
          }

          if (dataObject != null)
          {
            foreach (DataProperty dataProp in objectDefinition.dataProperties)
            {
              try
              {
                String value = Convert.ToString(row[dataProp.columnName]);

                if (value != null)
                {
                  dataObject.SetPropertyValue(dataProp.propertyName, value);
                }
              }
              catch (Exception ex)
              {
                _logger.Error("Error getting data row value: " + ex);
                throw ex;
              }
            }
          }
        }
      }

      return dataObjects;
    }

    protected IList<DataTable> ToDataTables(IList<DataObject> dataObjects)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
