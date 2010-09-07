using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
using log4net;
using Ninject;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;

namespace Hatch.iPasXLDataLayer.API
{
  public class iPasXLDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(iPasXLDataLayer));
    private AdapterSettings _settings = null;
    private string _dataDictionaryPath = String.Empty;
    private string _configurationPath = null;
    private iPasXLConfiguration _configuration = null;
    
    [Inject]
    public iPasXLDataLayer(AdapterSettings settings)
    {
      try
      {
        _settings = settings;

        _dataDictionaryPath = _settings["XmlPath"] + "DataDictionary." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";
        _configurationPath = _settings["XmlPath"] + "iPasXL." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";

        _configuration = Utility.Read<iPasXLConfiguration>(_configurationPath, true);

        

      }
      catch (Exception e)
      {
        _logger.Error("boom", e);
      }
    }

    ~iPasXLDataLayer()
    {
      try
      {
        
      }
      catch (Exception e)
      {
        _logger.Error("Failed to release excel com object", e);
      }
    }

    #region IDataLayer Members

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        Worksheet worksheet = _configuration.Worksheets.FirstOrDefault<Worksheet>(o => o.Name == objectType);

        IList<IDataObject> dataObjects = new List<IDataObject>();
        Type type = Type.GetType("Hatch.iPasXLDataLayer.API.Model_" + _settings["ProjectName"] + "_" + _settings["ApplicationName"] + "." + objectType);

        objectType = objectType.Substring(objectType.LastIndexOf('.') + 1);

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

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
      throw new NotImplementedException();
    }

    public Response Delete(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      throw new NotImplementedException();
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;

      try
      {
        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(_configuration.Location, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        Worksheet cfWorksheet = _configuration.Worksheets.FirstOrDefault<Worksheet>(o => o.Name == objectType);
        Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

        IList<IDataObject> dataObjects = new List<IDataObject>();
        Type type = Type.GetType("Hatch.iPasXLDataLayer.API.Model_" + _settings["ProjectName"] + "_" + _settings["ApplicationName"] + "." + objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

            dataObject.SetPropertyValue(cfWorksheet.Identifier, identifier);

            int row = GetRowIndex(xlWorksheet, cfWorksheet, dataObject);

            foreach (Column column in cfWorksheet.Columns)
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
        string location = _configuration.Location;

        DataDictionary dataDictionary = new DataDictionary()
        {
          dataObjects = new List<DataObject>()          
        };
                
        foreach (Worksheet worksheet in _configuration.Worksheets)
        {
          DataObject dataObject = new DataObject()
          {
            objectName = worksheet.Name,            
            dataProperties = new List<DataProperty>()            
          };

          foreach (Column column in worksheet.Columns)
          {
            DataProperty dataProperty = new DataProperty() 
            {
              propertyName = column.Name,              
              dataType = column.DataType           
            };

            dataObject.dataProperties.Add(dataProperty);
          }

          DataProperty keyProperty = dataObject.dataProperties.FirstOrDefault<DataProperty>(o => o.propertyName == worksheet.Identifier);
          dataObject.addKeyProperty(keyProperty);

          dataDictionary.dataObjects.Add(dataObject);
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
      throw new NotImplementedException();
    }

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    private Excel.Worksheet GetWorkSheet(string objectType, Excel.Workbook xlWorkBook, Worksheet cfWorksheet)
    {
      try
      {
        dynamic worksheet = xlWorkBook.Worksheets.get_Item(objectType);

        if (worksheet == null)
        {
          Excel.Worksheet nwWorksheet = xlWorkBook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
          nwWorksheet.Name = objectType;

          foreach (Column column in cfWorksheet.Columns)
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

    private int GetRowIndex(Excel.Worksheet xlWorksheet, Worksheet cfWorksheet, IDataObject dataObject)
    {
      try
      {
        Column key = cfWorksheet.Columns.FirstOrDefault<Column>(o => o.Name == cfWorksheet.Identifier);
        string identifier = dataObject.GetPropertyValue(key.Name).ToString();

        Excel.Range usedRange = xlWorksheet.UsedRange;
        Excel.Range findRange = usedRange.Find(identifier, Type.Missing, Type.Missing, Type.Missing, Excel.XlSearchOrder.xlByRows, Excel.XlSearchDirection.xlNext, true, Type.Missing, Type.Missing);

        int row = findRange.Cells[1, 1].Row;
                
        return row;
      }
      catch (Exception e)
      {
        throw new Exception("Error getting excel row index for object [" + cfWorksheet.Name + "].", e);
      }
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
          Worksheet cfWorksheet = _configuration.Worksheets.FirstOrDefault<Worksheet>(o => o.Name == objectType);

          Excel.Worksheet xlWorksheet = GetWorkSheet(objectType, xlWorkBook, cfWorksheet);

          int row = GetRowIndex(xlWorksheet, cfWorksheet, dataObject);

          foreach (Column column in cfWorksheet.Columns)
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

    #endregion    

  }  
          
}
