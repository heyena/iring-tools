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
    private const string WHERECLAUSE_ALIAS = "_t";
    private DataDictionary _dataDictionary;
    private string _execAssemblyName;
    protected AdapterSettings _settings = null;
    
    #region BaseSQLDataLayer methods
    public BaseSQLDataLayer(AdapterSettings settings)
    {
        _settings = new AdapterSettings();
        _settings = settings;
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

    // get a page of data rows with (optional) filter
    public abstract DataTable GetDataTable(string tableName, string whereClause, int start, int limit);

    // get related data rows of a given data row
    public abstract DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName);

    // post data rows and its related items (data rows)
    public abstract Response PostDataTables(IList<DataTable> dataTables);
    
    // delete data rows with filter
    public abstract Response DeleteDataTable(string tableName, string whereClause);

    // delete data rows by identifiers
    public abstract Response DeleteDataTable(string tableName, IList<string> identifiers);

    public abstract Response RefreshDataTable(string tableName);
    #endregion

    #region IDataLayer pass-thru/implementation methods
    public abstract DataDictionary GetDictionary();
    public abstract Response Configure(XElement configuration);
    public abstract XElement GetConfiguration();

    public virtual long GetCount(string objectTypeName, DataFilter filter)
    {
      string tableName = GetTableName(objectTypeName);
      string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectTypeName, WHERECLAUSE_ALIAS);

      try
      {
        return GetCount(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data count: " + ex);
        throw ex;
      }
    }

    public virtual IList<string> GetIdentifiers(string objectTypeName, DataFilter filter)
    {
      string tableName = GetTableName(objectTypeName);
      string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectTypeName, WHERECLAUSE_ALIAS);

      try
      {
        return GetIdentifiers(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data table: " + ex);
        throw ex;
      }
    }

    public virtual IList<IDataObject> Create(string objectTypeName, IList<string> identifiers)
    {
      string tableName = GetTableName(objectTypeName);

      try
      {
        DataTable dataTable = CreateDataTable(tableName, identifiers);
        return ToDataObjects(dataTable, objectTypeName);
      }
      catch (Exception ex)
      {
        _logger.Error("Error creating data table: " + ex);
        throw ex;
      }
    }

    public virtual IList<IDataObject> Get(string objectTypeName, IList<string> identifiers)
    {
      string tableName = GetTableName(objectTypeName);

      try
      {
        DataTable dataTable = GetDataTable(tableName, identifiers);
        return ToDataObjects(dataTable, objectTypeName);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data table: " + ex);
        throw ex;
      }
    }

    public virtual IList<IDataObject> Get(string objectTypeName, DataFilter filter, int pageSize, int startIndex)
    {
      string tableName = GetTableName(objectTypeName);
      string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectTypeName, WHERECLAUSE_ALIAS);

      try
      {
        DataTable dataTable = GetDataTable(tableName, whereClause, startIndex, pageSize);
        return ToDataObjects(dataTable, objectTypeName);
      }
      catch (Exception ex)
      {
        _logger.Error("Error deleting data table: " + ex);
        throw ex;
      }
    }

    public virtual IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectTypeName)
    {
      IList<IDataObject> relatedDataObjects = null;
      DataObject relatedObjectDefinition = GetObjectDefinition(relatedObjectTypeName);
      DataTable dataTable = new DataTable(relatedObjectDefinition.tableName);
      
      try
      {
        DataRow dataRow = CreateDataRow(dataTable, dataObject, relatedObjectDefinition);
        DataTable relatedDataTable = GetRelatedDataTable(dataRow, relatedObjectDefinition.tableName);
        relatedDataObjects = ToDataObjects(relatedDataTable, relatedObjectTypeName);
      }
      catch (Exception ex)
      {
        _logger.Error("Error deleting data table: " + ex);
        throw ex;
      }

      return relatedDataObjects;
    }

   public virtual Response Post(IList<IDataObject> dataObjects)
    {
      try
      {
        IList<DataTable> dataTables = ToDataTables(dataObjects);
        return PostDataTables(dataTables);
      }
      catch (Exception ex)
      {
        _logger.Error("Error posting data tables: " + ex);
        throw ex;
      }
    }

    public virtual Response Delete(string objectTypeName, DataFilter filter)
    {
      string tableName = GetTableName(objectTypeName);
      string whereClause = filter.ToSqlWhereClause(_dataDictionary, objectTypeName, WHERECLAUSE_ALIAS);

      try
      {
        return DeleteDataTable(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error deleting data table: " + ex);
        throw ex;
      }
    }

    public virtual Response Delete(string objectTypeName, IList<string> identifiers)
    {
      string tableName = GetTableName(objectTypeName);

      try
      {
        return DeleteDataTable(tableName, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error deleting data table: " + ex);
        throw ex;
      }
    }

    public virtual Response Refresh(string objectTypeName)
    {
      try
      {
        string tableName = GetTableName(objectTypeName);
        return RefreshDataTable(tableName);
      }
      catch (Exception ex)
      {
        _logger.Error("Error refreshing object type: [" + objectTypeName + "].");
        throw ex;
      }
    }
    #endregion

    #region helper methods
    public string GetTableName(string objectTypeName)
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

    public DataObject GetObjectDefinition(string objectTypeName)
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

    protected IDataObject ToDataObject(DataRow dataRow, DataObject objectDefinition)
    {
      IDataObject dataObject = null;

      if (dataRow != null)
      {
        try
        {
          Type objectType = GetObjectType(objectDefinition, objectDefinition.objectName);
          dataObject = (IDataObject)Activator.CreateInstance(objectType);
        }
        catch (Exception ex)
        {
          _logger.Error("Error instantiating data object: " + ex);
          throw ex;
        }

        if (dataObject != null && objectDefinition.dataProperties != null)
        {
          foreach (DataProperty objectProperty in objectDefinition.dataProperties)
          {
            try
            {
              String value = Convert.ToString(dataRow[objectProperty.columnName]);

              if (value != null)
              {
                dataObject.SetPropertyValue(objectProperty.propertyName, value);
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

      return dataObject;
    }

    protected IList<IDataObject> ToDataObjects(DataTable dataTable, string objectTypeName)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();
      DataObject objectDefinition = GetObjectDefinition(objectTypeName);
      
      if (objectDefinition != null && dataTable.Rows != null)
      {
        foreach (DataRow dataRow in dataTable.Rows)
        {
          IDataObject dataObject = null;

          try
          {
            dataObject = ToDataObject(dataRow, objectDefinition);
          }
          catch (Exception ex)
          {
            _logger.Error("Error converting data row to data object: " + ex);
            throw ex;
          }

          if (dataObjects != null)
          {
            dataObjects.Add(dataObject);
          }
        }
      }

      return dataObjects;
    }

    protected DataRow CreateDataRow(DataTable dataTable, IDataObject dataObject, DataObject objectDefinition)
    {
      DataRow dataRow = null;

      if (dataObject != null)
      {
        dataRow = dataTable.NewRow();

        foreach (DataProperty objectProperty in objectDefinition.dataProperties)
        {
          try
          {
            object value = dataObject.GetPropertyValue(objectProperty.propertyName);

            if (value != null)
            {
              switch (objectProperty.dataType)
              {
                case DataType.Boolean:
                  dataRow[objectProperty.columnName] = Convert.ToBoolean(value);
                  break;
                case DataType.Byte:
                  dataRow[objectProperty.columnName] = Convert.ToByte(value);
                  break;
                case DataType.Int16:
                  dataRow[objectProperty.columnName] = Convert.ToInt16(value);
                  break;
                case DataType.Int32:
                  dataRow[objectProperty.columnName] = Convert.ToInt32(value);
                  break;
                case DataType.Int64:
                  dataRow[objectProperty.columnName] = Convert.ToInt64(value);
                  break;
                case DataType.Decimal:
                  dataRow[objectProperty.columnName] = Convert.ToDecimal(value);
                  break;
                case DataType.Single:
                  dataRow[objectProperty.columnName] = Convert.ToSingle(value);
                  break;
                case DataType.Double:
                  dataRow[objectProperty.columnName] = Convert.ToDouble(value);
                  break;
                default:
                  dataRow[objectProperty.columnName] = value;
                  break;
              }
            }
          }
          catch (Exception ex)
          {
            _logger.Error("Error getting data row value: " + ex);
            throw ex;
          }
        }
      }

      return dataRow;
    }

    protected IList<DataTable> ToDataTables(IList<IDataObject> dataObjects)
    {
      Dictionary<string, DataTable> dataTableDictionary = new Dictionary<string, DataTable>();

      if (dataObjects != null)
      {
        foreach (IDataObject dataObject in dataObjects)
        {
          string objectTypeName = dataObject.GetType().Name;
          DataObject objectDefinition = GetObjectDefinition(objectTypeName);
          DataTable dataTable = null;

          if (dataTableDictionary.ContainsKey(objectTypeName))
          {
            dataTable = dataTableDictionary[objectTypeName];
          }
          else
          {
            dataTable = new DataTable(objectDefinition.tableName);

            foreach (DataProperty objectProperty in objectDefinition.dataProperties)
            {
              DataColumn dataColumn = new DataColumn()
              {
                ColumnName = objectProperty.columnName,
                DataType = Type.GetType("System." + objectProperty.dataType.ToString())
              };

              if (objectProperty.dataType == DataType.String)
              {
                dataColumn.MaxLength = objectProperty.dataLength;
              }

              dataTable.Columns.Add(dataColumn);
            }

            dataTableDictionary[objectTypeName] = dataTable;
          }

          try
          {
            DataRow dataRow = CreateDataRow(dataTable, dataObject, objectDefinition);
            dataTable.Rows.Add(dataRow);
          }
          catch (Exception ex)
          {
            _logger.Error("Error populating data row: " + ex);
            throw ex;
          }
        }
      }

      return dataTableDictionary.Values.ToList();
    }
    #endregion
  }
}
