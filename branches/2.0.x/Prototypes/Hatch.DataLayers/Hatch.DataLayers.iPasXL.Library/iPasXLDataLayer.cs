using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Web;

namespace Hatch.DataLayers.iPasXL
{
  public class iPasXLRow : IDataObject {

    private Dictionary<string, object> Row { get; set; }

    public iPasXLRow()
    {
      Row = new Dictionary<string, object>();
    }

    public object GetPropertyValue(string propertyName)
    {
      if (Row.ContainsKey(propertyName))
      {
        return Row[propertyName];
      }
      else
      {
        throw new Exception("Property [" + propertyName + "] does not exist.");
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

  public class iPasXLDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(iPasXLDataLayer));
    private AdapterSettings _settings = null;
    private string _configurationPath = String.Empty;    
    private iPasXLConfiguration _configuration = null;
    
    [Inject]
    public iPasXLDataLayer(AdapterSettings settings)
    {
      try
      {
        _settings = settings;
                
        _configurationPath = _settings["XmlPath"] + "iPasXL-Configuration." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";
        _configuration = Utility.Read<iPasXLConfiguration>(_configurationPath, true);

        if (_configuration.Worksheets.Count == 0)
        {
          _configuration = CreateiPasXlConfig(_configuration.Location);
        }       

      }
      catch (Exception e)
      {
        _logger.Error("boom", e);
      }
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        iPasXLWorksheet worksheet = _configuration.Worksheets.FirstOrDefault<iPasXLWorksheet>(o => o.Name == objectType);

        IList<IDataObject> dataObjects = new List<IDataObject>();
        
        objectType = objectType.Substring(objectType.LastIndexOf('.') + 1);

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            iPasXLRow dataObject = new iPasXLRow();

            if (!String.IsNullOrEmpty(identifier))
            {
              dataObject.SetPropertyValue(worksheet.Identifier, identifier);
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

        iPasXLWorksheet cfWorksheet = GetConfigWorkSheet(objectType);
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

        iPasXLWorksheet cfWorksheet = GetConfigWorkSheet(objectType);
        Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

        List<IDataObject> dataObjects = new List<IDataObject>();
        
        Excel.Range usedRange = xlWorksheet.UsedRange;
        
        for(int row = 2; row <= usedRange.Rows.Count; row++)
        {
          iPasXLRow dataObject = new iPasXLRow();
          
          foreach (iPasXLColumn column in cfWorksheet.Columns)
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
            context.Variables.DefineVariable(variable, typeof(iPasXLRow));

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

        iPasXLWorksheet cfWorksheet = GetConfigWorkSheet(objectType);
        Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

        IList<IDataObject> dataObjects = new List<IDataObject>();

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            iPasXLRow dataObject = new iPasXLRow();

            dataObject.SetPropertyValue(cfWorksheet.Identifier, identifier);

            int row = GetRowIndex(xlWorksheet, cfWorksheet, identifier);

            foreach (iPasXLColumn column in cfWorksheet.Columns)
            {
              dataObject.SetPropertyValue(column.Name, xlWorksheet.Cells[row, column.Index].Value2);
            }

            dataObjects.Add(dataObject);
          }
        }
        else
        {
          Excel.Range usedRange = xlWorksheet.UsedRange;

          for (int row = 2; row <= usedRange.Rows.Count; row++)
          {
            iPasXLRow dataObject = new iPasXLRow();
                        
            foreach (iPasXLColumn column in cfWorksheet.Columns)
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
                
        foreach (iPasXLWorksheet worksheet in _configuration.Worksheets)
        {
          DataObject dataObject = new DataObject()
          {
            objectName = worksheet.Name,            
            dataProperties = new List<DataProperty>()            
          };

          dataDictionary.dataObjects.Add(dataObject);

          foreach (iPasXLColumn column in worksheet.Columns)
          {
            DataProperty dataProperty = new DataProperty() 
            {
              propertyName = column.Name,              
              dataType = column.DataType           
            };
           
            dataObject.dataProperties.Add(dataProperty);
          }

          DataProperty keyProperty = dataObject.dataProperties.FirstOrDefault<DataProperty>(o => o.propertyName == worksheet.Identifier);
          dataObject.keyProperties.Add(new KeyProperty { keyPropertyName = keyProperty.columnName });
          
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
        iPasXLWorksheet cfWorkSheet = GetConfigWorkSheet(objectType);

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

    private Excel.Worksheet GetWorkSheet(string objectType, Excel.Workbook xlWorkBook, iPasXLWorksheet cfWorksheet)
    {
      try
      {
        dynamic worksheet = xlWorkBook.Worksheets.get_Item(objectType);

        if (worksheet == null)
        {
          Excel.Worksheet nwWorksheet = xlWorkBook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
          nwWorksheet.Name = objectType;

          foreach (iPasXLColumn column in cfWorksheet.Columns)
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

    private int GetRowIndex(Excel.Worksheet xlWorksheet, iPasXLWorksheet cfWorksheet, string identifier)
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

    private iPasXLWorksheet GetConfigWorkSheet(string objectType)
    {
      return _configuration.Worksheets.FirstOrDefault<iPasXLWorksheet>(o => o.Name == objectType);
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
         
        foreach(IDataObject dataObject in dataObjects)
        {
          string objectType = dataObject.GetType().Name;
          iPasXLWorksheet cfWorksheet = GetConfigWorkSheet(objectType);

          Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);
                    
          string identifier = dataObject.GetPropertyValue(cfWorksheet.Identifier).ToString();

          int row = GetRowIndex(xlWorksheet, cfWorksheet, identifier);

          foreach (iPasXLColumn column in cfWorksheet.Columns)
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

    private iPasXLConfiguration CreateiPasXlConfig(string filePath)
    {
      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;
      iPasXLConfiguration config = new iPasXLConfiguration()
      {
        Worksheets = new List<iPasXLWorksheet>()
      };

      try
      {
        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(filePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        config.Location = filePath;

        foreach (Excel.Worksheet xlWorkSheet in xlWorkBook.Worksheets)
        {
          iPasXLWorksheet cfWorkSheet = new iPasXLWorksheet()
          {
            Name = xlWorkSheet.Name,
            Columns = new List<iPasXLColumn>()
          };

          Excel.Range usedRange = xlWorkSheet.UsedRange;

          for (int i = 1; i <= usedRange.Columns.Count; i++)
          {
            string header = usedRange.Cells[1, i].Value2;
            
            if (header == null)
              break;
            
            header = header.Trim();
            header = header.Replace(" ", String.Empty);
            if (header != null && !header.Equals(String.Empty))
            {
              iPasXLColumn cfColumn = new iPasXLColumn()
              {
                Index = i,
                Name = header,
                DataType = DataType.String
              };

              cfWorkSheet.Columns.Add(cfColumn);
            }
          }

          if (cfWorkSheet.Columns.Count > 0)
          {
            cfWorkSheet.Identifier = cfWorkSheet.Columns[0].Name;
            config.Worksheets.Add(cfWorkSheet);
          }
        }

        return config;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Createing iPasXL Configuration: " + ex);
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

    #region IDataLayer Members
    
    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      return new List<IDataObject>(); 
    }

    #endregion    

  }  
          
}
