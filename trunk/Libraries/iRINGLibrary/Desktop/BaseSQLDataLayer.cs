using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Data;
using log4net;
using System.Xml.Linq;
using org.iringtools.adapter;
using System.Globalization;

namespace org.iringtools.library
{
  public abstract class BaseSQLDataLayer : BaseDataLayer, IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseSQLDataLayer));
    protected DatabaseDictionary _dbDictionary = null;
    protected string _whereClauseAlias = String.Empty;
     
    #region BaseSQLDataLayer methods
    public BaseSQLDataLayer(AdapterSettings settings) : base(settings)
    {
      if (!String.IsNullOrEmpty(settings["WhereClauseAlias"]))
      {
        _whereClauseAlias = settings["WhereClauseAlias"];
      }
    }

    // get number of rows with (optional) filter
    public abstract DatabaseDictionary GetDatabaseDictionary();

    public abstract long GetCount(string tableName, string whereClause);

    // get list of identifiers with (optional) filter
    public abstract IList<string> GetIdentifiers(string tableName, string whereClause);

    // create or fetch data rows of given identifiers
    public abstract DataTable CreateDataTable(string tableName, IList<string> identifiers);

    // fetch data rows of given identifiers
    public abstract DataTable GetDataTable(string tableName, IList<string> identifiers);

    // get a page of data rows with (optional) filter
    public abstract DataTable GetDataTable(string tableName, string whereClause, long start, long limit);

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

    #region IDataLayer implementation methods
    public override DataDictionary GetDictionary()
    {
      throw new NotImplementedException();
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        InitializeDataDictionary();

        string tableName = GetTableName(objectType);
        string whereClause = filter.ToSqlWhereClause(_dbDictionary, objectType, null);

        return GetCount(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data count: " + ex);
        throw ex;
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        InitializeDataDictionary();
        
        string tableName = GetTableName(objectType);
        string whereClause = filter.ToSqlWhereClause(_dbDictionary, objectType, _whereClauseAlias);

        return GetIdentifiers(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      string tableName = GetTableName(objectType);

      try
      {
        DataTable dataTable = CreateDataTable(tableName, identifiers);
        return ToDataObjects(dataTable, objectType);
      }
      catch (Exception ex)
      {
        _logger.Error("Error creating data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      string tableName = GetTableName(objectType);

      try
      {
        DataTable dataTable = GetDataTable(tableName, identifiers);
        return ToDataObjects(dataTable, objectType);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      try
      {
        InitializeDataDictionary();

        string tableName = GetTableName(objectType);
        string whereClause = filter.ToSqlWhereClause(_dbDictionary, objectType, _whereClauseAlias);

        DataTable dataTable = GetDataTable(tableName, whereClause, startIndex, pageSize);
        return ToDataObjects(dataTable, objectType);
      }
      catch (Exception ex)
      {
        _logger.Error("Error get data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      IList<IDataObject> relatedDataObjects = null;
      DataObject relatedObjectDefinition = GetObjectDefinition(relatedObjectType);
      
      try
      {
        DataTable dataTable = NewDataTable(relatedObjectDefinition);
        DataRow dataRow = CreateDataRow(dataTable, dataObject, relatedObjectDefinition);
        DataTable relatedDataTable = GetRelatedDataTable(dataRow, relatedObjectDefinition.tableName);
        relatedDataObjects = ToDataObjects(relatedDataTable, relatedObjectType);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting related objects: " + ex);
        throw ex;
      }

      return relatedDataObjects;
    }

    public override Response Post(IList<IDataObject> dataObjects)
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

    public override Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        InitializeDataDictionary();

        string tableName = GetTableName(objectType);
        string whereClause = filter.ToSqlWhereClause(_dbDictionary, objectType, _whereClauseAlias);

        return DeleteDataTable(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error deleting data table: " + ex);
        throw ex;
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      string tableName = GetTableName(objectType);

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

    public virtual Response Refresh(string objectType)
    {
      try
      {
        string tableName = GetTableName(objectType);
        return RefreshDataTable(tableName);
      }
      catch (Exception ex)
      {
        _logger.Error("Error refreshing data table: [" + objectType + "].");
        throw ex;
      }
    }
    #endregion

    #region helper methods
    public string GetTableName(string objectType)
    {
      InitializeDataDictionary();

      foreach (DataObject dataObject in _dbDictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == objectType.ToLower())
        {
          return dataObject.tableName;
        }
      }

      return null;
    }

    public DataObject GetObjectDefinition(string objectType)
    {
      InitializeDataDictionary();

      foreach (DataObject dataObject in _dbDictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == objectType.ToLower())
        {
          return dataObject;
        }
      }

      return null;
    }

    protected IDataObject ToDataObject(DataRow dataRow, DataObject objectDefinition)
    {
      IDataObject dataObject = null;

      if (dataRow != null)
      {
        try
        {
          dataObject = new GenericDataObject();
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

    protected IList<IDataObject> ToDataObjects(DataTable dataTable, string objectType)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();
      DataObject objectDefinition = GetObjectDefinition(objectType);
      
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

            if (value != null && value.ToString().Trim().Length > 0)
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
                  CultureInfo culture = new CultureInfo("en-US");
                  String format = "YYYY-MM-DD HH24:MI:SS.FF TZH:TZM";
                  DateTime dtValue = 
                    DateTime.ParseExact((String)value, format, culture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);
                  dataRow[objectProperty.columnName] = dtValue;
                  break;
                case DataType.Single:
                  dataRow[objectProperty.columnName] = Convert.ToSingle(value);
                  break;
                case DataType.Double:
                  dataRow[objectProperty.columnName] = Convert.ToDouble(value);
                  break;
                case DataType.DateTime:
                  dataRow[objectProperty.columnName] = Convert.ToDateTime(value);
                  break;
                default:
                  dataRow[objectProperty.columnName] = value;
                  break;
              }
            }
            else if (objectProperty.dataType == DataType.String || objectProperty.isNullable)
            {
              dataRow[objectProperty.columnName] = DBNull.Value;
            }
            else
            {
              _logger.Error("Object property is set to not nullable but received a null value.");
              return null;
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

    protected DataTable NewDataTable(DataObject objectDefinition)
    {
      DataTable dataTable = new DataTable(objectDefinition.tableName);

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

      return dataTable;
    }

    protected IList<DataTable> ToDataTables(IList<IDataObject> dataObjects)
    {
      Dictionary<string, DataTable> dataTableDictionary = new Dictionary<string, DataTable>();

      if (dataObjects != null)
      {
        foreach (IDataObject dataObject in dataObjects)
        {
          string objectType = dataObject.GetType().Name;

          if (objectType == typeof(GenericDataObject).Name)
          {
            objectType = ((GenericDataObject)dataObject).ObjectType;
          }

          DataObject objectDefinition = GetObjectDefinition(objectType);
          DataTable dataTable = null;

          if (dataTableDictionary.ContainsKey(objectType))
          {
            dataTable = dataTableDictionary[objectType];
          }
          else
          {
            dataTable = NewDataTable(objectDefinition);
            dataTableDictionary[objectType] = dataTable;
          }

          try
          {
            DataRow dataRow = CreateDataRow(dataTable, dataObject, objectDefinition);

            if (dataRow != null)
            {
              dataTable.Rows.Add(dataRow);
            }
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

    private void InitializeDataDictionary()
    {
      if (_dbDictionary == null)
      {
        _dbDictionary = GetDatabaseDictionary();
      }
    }
    #endregion
  }
}
