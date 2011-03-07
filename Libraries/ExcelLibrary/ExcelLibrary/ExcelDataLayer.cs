using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.excel;
using org.iringtools.library;
using org.iringtools.utility;
using Excel = Microsoft.Office.Interop.Excel;

namespace org.iringtools.adapter.datalayer
{
  public class ExcelDataObject : IDataObject
  {

    private Dictionary<string, object> Row { get; set; }

    public ExcelDataObject()
    {
      Row = new Dictionary<string, object>();
    }

    public string SheetName { get; set; }

    public object GetPropertyValue(string propertyName)
    {
      if (Row.ContainsKey(propertyName))
      {
        return Row[propertyName];
      }
      else
      {
        //throw new Exception("Property [" + propertyName + "] does not exist.");
        return null;
      }
    }

    public IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
      }
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      Row[propertyName] = value;
    }
  }

  public class ExcelDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ExcelDataLayer));
    private AdapterSettings _settings = null;
    private string _configurationPath = String.Empty;
    private ExcelConfiguration _configuration = null;
    private Dictionary<string, Type> _dynamicTypes = new Dictionary<string, Type>();
   
    [Inject]
    public ExcelDataLayer(AdapterSettings settings)
    {
      try
      {
        _settings = settings;

        _configurationPath = _settings["XmlPath"] + "excel-configuration." + _settings["Scope"] + ".xml";
        _configuration = ProcessExcelConfig(_configurationPath);

      }
      catch (Exception e)
      {
        _logger.Error("Error in processing the Configuration File [" + _configurationPath + "].", e);
      }
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        ExcelWorksheet cfWorksheet = _configuration.Worksheets.FirstOrDefault<ExcelWorksheet>(o => o.Name == objectType);

        IList<IDataObject> dataObjects = new List<IDataObject>();

        objectType = objectType.Substring(objectType.LastIndexOf('.') + 1);
        //Type type = this.GetType(_settings["projectName"], _settings["applicationName"], objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            ExcelDataObject dataObject = new ExcelDataObject()
            {
              SheetName = objectType
            };

            if (!String.IsNullOrEmpty(identifier))
            {
              dataObject.SetPropertyValue(cfWorksheet.Identifier, identifier);
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

    public Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        IList<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
        }

        return Delete(objectType, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
    }

    public Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();

      if (identifiers == null || identifiers.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to delete.");
        response.Append(status);
        return response;
      }

      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;

      try
      {

        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(_configuration.Location, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        ExcelWorksheet cfWorksheet = GetConfigWorkSheet(objectType);
        Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

        IList<IDataObject> dataObjects = new List<IDataObject>();

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            Status status = new Status();
            status.Identifier = identifier;

            int row = GetRowIndex(xlWorksheet, cfWorksheet, identifier);

            try
            {
              xlWorksheet.Rows[row].Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
              status.Messages.Add("Data object [" + identifier + "] deleted successfully.");
            }
            catch (Exception ex)
            {
              _logger.Error("Error in Delete: " + ex);
              status.Level = StatusLevel.Error;
              status.Messages.Add("Error while deleting data object [" + identifier + "]." + ex);
            }

            response.Append(status);
          }
        }

        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
        xlWorksheet = null;

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in DeletList: " + ex);
        throw new Exception("Error while delete a list of data objects of type [" + objectType + "].", ex);
      }
      finally
      {
        if (xlWorkBook != null)
        {
          xlWorkBook.Close(true, Type.Missing, Type.Missing);
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
          xlWorkBook = null;
        }

        if (xlApplication != null)
        {
          xlApplication.Quit();
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApplication);
          xlApplication = null;
        }

        GC.Collect();
      }
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;

      try
      {
        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(_configuration.Location, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        ExcelWorksheet cfWorksheet = GetConfigWorkSheet(objectType);
        Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

        List<IDataObject> dataObjects = new List<IDataObject>();
        //Type type = this.GetType(_settings["projectName"], _settings["applicationName"], objectType);

        Excel.Range usedRange = xlWorksheet.UsedRange;

        for (int row = 2; row <= usedRange.Rows.Count; row++)
        {
          //IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);
          ExcelDataObject dataObject = new ExcelDataObject()
          {
            SheetName = objectType
          };

          foreach (ExcelColumn column in cfWorksheet.Columns)
          {
            dataObject.SetPropertyValue(column.Name, xlWorksheet.Cells[row, column.Index].Value2);
          }

          dataObjects.Add(dataObject);
        }

        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
        xlWorksheet = null;

        // Apply filter
        if (filter != null && filter.Expressions.Count > 0)
        {
          string variable = "dataObject";
          string linqExpression = string.Empty;
          switch (objectType)
          {
            default:
              linqExpression = filter.ToLinqExpression<IDataObject>(variable);
              break;
          }

          if (linqExpression != String.Empty)
          {
            ExpressionContext context = new ExpressionContext();
            context.Variables.DefineVariable(variable, typeof(ExcelDataObject));

            for (int i = 0; i < dataObjects.Count; i++)
            {
              context.Variables[variable] = dataObjects[i];
              var expression = context.CompileGeneric<bool>(linqExpression);
              if (!expression.Evaluate())
              {
                dataObjects.RemoveAt(i--);
              }
            }
          }
        }

        // Apply paging
        if (pageSize > 0 && pageNumber > 0)
        {
          if (dataObjects.Count > (pageSize * (pageNumber - 1) + pageSize))
          {
            dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), pageSize);
          }
          else if (pageSize * (pageNumber - 1) > dataObjects.Count)
          {
            dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), dataObjects.Count);
          }
          else
          {
            return null;
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
      finally
      {
        if (xlWorkBook != null)
        {
          xlWorkBook.Close(true, Type.Missing, Type.Missing);
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
          xlWorkBook = null;
        }

        if (xlApplication != null)
        {
          xlApplication.Quit();
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApplication);
          xlApplication = null;
        }

        GC.Collect();
      }
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;

      try
      {
        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(_configuration.Location, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        ExcelWorksheet cfWorksheet = GetConfigWorkSheet(objectType);
        Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

        IList<IDataObject> dataObjects = new List<IDataObject>();
        //Type type = this.GetType(_settings["projectName"], _settings["applicationName"], objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            //IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);
            ExcelDataObject dataObject = new ExcelDataObject()
            {
              SheetName = objectType
            };

            dataObject.SetPropertyValue(cfWorksheet.Identifier, identifier);

            int row = GetRowIndex(xlWorksheet, cfWorksheet, identifier);

            foreach (ExcelColumn column in cfWorksheet.Columns)
            {
              dataObject.SetPropertyValue(column.Name, xlWorksheet.Cells[row, column.Index].Value2);
            }

            dataObjects.Add(dataObject);
          }
        }
        else
        {
          Excel.Range usedRange = xlWorksheet.UsedRange;

          for (int row = cfWorksheet.DataIdx; row <= usedRange.Rows.Count; row++)
          {
            //IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);
            ExcelDataObject dataObject = new ExcelDataObject()
            {
              SheetName = objectType
            };

            foreach (ExcelColumn column in cfWorksheet.Columns)
            {
              dataObject.SetPropertyValue(column.Name, xlWorksheet.Cells[row, column.Index].Value2);
            }

            dataObjects.Add(dataObject);
          }
        }

        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
        xlWorksheet = null;

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
      finally
      {
        if (xlWorkBook != null)
        {
          xlWorkBook.Close(true, Type.Missing, Type.Missing);
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
          xlWorkBook = null;
        }

        if (xlApplication != null)
        {
          xlApplication.Quit();
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApplication);
          xlApplication = null;
        }

        GC.Collect();
      }
    }

    public DataDictionary GetDictionary()
    {
      try
      {
        DataDictionary dataDictionary = new DataDictionary()
        {
          dataObjects = new List<DataObject>()
        };

        foreach (ExcelWorksheet worksheet in _configuration.Worksheets)
        {
          DataObject dataObject = new DataObject()
          {
            objectName = worksheet.Name,
            dataProperties = new List<DataProperty>()
          };

          dataDictionary.dataObjects.Add(dataObject);

          foreach (ExcelColumn column in worksheet.Columns)
          {
            DataProperty dataProperty = new DataProperty()
            {
              propertyName = column.Name,
              dataType = column.DataType
            };

            if (worksheet.Identifier == column.Name)
            {
              dataObject.addKeyProperty(dataProperty);
            }
            else
            {
              dataObject.dataProperties.Add(dataProperty);
            }
          }
        }

        return dataDictionary;
      }
      catch (Exception e)
      {
        throw new Exception("Error while creating dictionary.", e);
      }
    }

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        ExcelWorksheet cfWorkSheet = GetConfigWorkSheet(objectType);

        List<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue(cfWorkSheet.Identifier));
        }

        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    public long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        ExcelWorksheet cfWorkSheet = GetConfigWorkSheet(objectType);

        List<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        return dataObjects.Count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    private Excel.Worksheet GetWorkSheet(string objectType, Excel.Workbook xlWorkBook, ExcelWorksheet cfWorksheet)
    {
      try
      {
        dynamic worksheet = xlWorkBook.Worksheets.get_Item(objectType);

        if (worksheet == null)
        {
          Excel.Worksheet nwWorksheet = xlWorkBook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
          nwWorksheet.Name = objectType;

          foreach (ExcelColumn column in cfWorksheet.Columns)
          {
            nwWorksheet.Cells[1, column.Index] = column.Name;
          }

          worksheet = nwWorksheet;
        }

        return (Excel.Worksheet)worksheet;
      }
      catch (Exception e)
      {
        throw new Exception("Error getting excel worksheet for object of type [" + objectType + "].", e);
      }
    }

    private int GetRowIndex(Excel.Worksheet xlWorksheet, ExcelWorksheet cfWorksheet, string identifier)
    {
      try
      {
        Excel.Range usedRange = xlWorksheet.UsedRange;
        Excel.Range findRange = usedRange.Find(identifier, Type.Missing, Type.Missing, Type.Missing, Excel.XlSearchOrder.xlByRows, Excel.XlSearchDirection.xlNext, true, Type.Missing, Type.Missing);

        int row = 0;
        if (findRange != null)
        {
          row = findRange.Cells[1, 1].Row;
        }
        else
        {
          row = usedRange.Rows.Count + 1;
        }

        return row;
      }
      catch (Exception e)
      {
        throw new Exception("Error getting excel row index for object [" + cfWorksheet.Name + "].", e);
      }
    }

    private ExcelWorksheet GetConfigWorkSheet(string objectType)
    {
      return _configuration.Worksheets.FirstOrDefault<ExcelWorksheet>(o => o.Name == objectType);
    }

    public Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;

      if (dataObjects == null || dataObjects.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to update.");
        response.Append(status);
        return response;
      }

      try
      {
        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(_configuration.Location, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          string objectType = ((ExcelDataObject)dataObject).SheetName;
          ExcelWorksheet cfWorksheet = GetConfigWorkSheet(objectType);

          Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

          string identifier = dataObject.GetPropertyValue(cfWorksheet.Identifier).ToString();

          int row = GetRowIndex(xlWorksheet, cfWorksheet, identifier);

          foreach (ExcelColumn column in cfWorksheet.Columns)
          {
            xlWorksheet.Cells[row, column.Index] = dataObject.GetPropertyValue(column.Name);
          }

          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
          xlWorksheet = null;
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PostList: " + ex);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception("Error while posting data objects of type [" + objectType + "].", ex);
      }
      finally
      {
        if (xlWorkBook != null)
        {
          xlWorkBook.Close(true, Type.Missing, Type.Missing);
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
          xlWorkBook = null;
        }

        if (xlApplication != null)
        {
          xlApplication.Quit();
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApplication);
          xlApplication = null;
        }

        GC.Collect();
      }
    }

    private List<ExcelColumn> CreateConfigColumns(Excel.Worksheet xlWorkSheet, ExcelWorksheet cfWorkSheet)
    {
      List<ExcelColumn> columns = new List<ExcelColumn>();

      Excel.Range usedRange = xlWorkSheet.UsedRange;

      for (int i = 1; i <= usedRange.Columns.Count; i++)
      {
        string header = usedRange.Cells[cfWorkSheet.HeaderIdx, i].Value2;

        if (header != null)
        {
          header = header.Trim().Replace(" ", String.Empty);

          if (header != null && !header.Equals(String.Empty))
          {
            ExcelColumn cfColumn = new ExcelColumn()
            {
              Index = i,
              Name = header,
              DataType = DataType.String
            };

            columns.Add(cfColumn);
          }
        }
      }

      if (columns.Count > 0)
      {
        return columns;
      }
      else
      {
        return null;
      }
    }

    private ExcelConfiguration ProcessExcelConfig(string filePath)
    {
      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;
      ExcelConfiguration config = null;

      try
      {
        config = Utility.Read<ExcelConfiguration>(_configurationPath, true);

        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(config.Location, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        if (config.Worksheets == null)
        {
          config.Worksheets = new List<ExcelWorksheet>();

          foreach (Excel.Worksheet xlWorkSheet in xlWorkBook.Worksheets)
          {
            ExcelWorksheet cfWorkSheet = new ExcelWorksheet()
            {
              Name = xlWorkSheet.Name,
              HeaderIdx = 1,
              DataIdx = 2
            };

            cfWorkSheet.Columns = CreateConfigColumns(xlWorkSheet, cfWorkSheet);

            if (cfWorkSheet.Columns != null && cfWorkSheet.Columns.Count > 0)
            {
              cfWorkSheet.Identifier = cfWorkSheet.Columns[0].Name;
              config.Worksheets.Add(cfWorkSheet);
            }
          }
        }
        else
        {

          foreach (ExcelWorksheet cfWorkSheet in config.Worksheets)
          {
            if (cfWorkSheet.Columns == null)
            {
              Excel.Worksheet xlWorkSheet = xlWorkBook.Worksheets[cfWorkSheet.Name];
              if (xlWorkSheet != null)
              {
                cfWorkSheet.Columns = CreateConfigColumns(xlWorkSheet, cfWorkSheet);

                if (cfWorkSheet.HeaderIdx <= 0)
                  cfWorkSheet.HeaderIdx = 1;

                if (cfWorkSheet.DataIdx <= 0)
                  cfWorkSheet.DataIdx = 2;

                if (cfWorkSheet.Identifier.Equals(String.Empty) && cfWorkSheet.Columns.Count > 0)
                  cfWorkSheet.Identifier = cfWorkSheet.Columns[0].Name;
              }
              else
              {
                _logger.Error("Excel Workbook [" + filePath + "] doesn't have a Worksheet named " + cfWorkSheet.Name + ".");
              }
            }
          }
        }
        return config;
      }
      catch (Exception ex)
      {
        _logger.Error("Error Processing Excel Configuration File [" + filePath + "]: " + ex);
        return config;
      }
      finally
      {
        if (xlWorkBook != null)
        {
          xlWorkBook.Close(true, Type.Missing, Type.Missing);
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
          xlWorkBook = null;
        }

        if (xlApplication != null)
        {
          xlApplication.Quit();
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApplication);
          xlApplication = null;
        }

        GC.Collect();
      }

    }

    //private Type GetType(string projectName, string applicationName, string className)
    //{
    //  string keyType = String.Format("Model_{0}_{1}.{2}", projectName, applicationName, className);

    //  if (_dynamicTypes.ContainsKey(keyType))
    //  {
    //    return _dynamicTypes[keyType];
    //  }
    //  else
    //  {         
    //    // Create a new Assembly for Methods
    //    AssemblyName assemName = new AssemblyName();
    //    assemName.Name = "ExcelLibrary";
    //    AssemblyBuilder assemBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemName, AssemblyBuilderAccess.RunAndCollect);

    //    // Create a new module within this assembly
    //    ModuleBuilder moduleBuilder = assemBuilder.DefineDynamicModule("Model_" + projectName + "_" + applicationName);

    //    // Create a new type within the module
    //    TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(ExcelDataObject));

    //    // Create a type.
    //    return typeBuilder.CreateType();
    //  }
    //}  

    #region IDataLayer Members

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      return new List<IDataObject>();
    }

    #endregion


    public Response Configure(XElement configuration)
    {
      Response _response = new Response();
      _response.Messages = new Messages();
      try
      {

        _configuration = Utility.DeserializeDataContract<ExcelConfiguration>(configuration.ToString());
        Utility.Write<ExcelConfiguration>(_configuration, _configurationPath, true);
        _response.Messages.Add("DataLayer configuration Saved successfully");
        _response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _response.Messages.Add("Failed to Save datalayer Configuration");
        _response.Messages.Add(ex.Message);
        _response.Level = StatusLevel.Error;
      }
      return _response;
    }
  }

}
