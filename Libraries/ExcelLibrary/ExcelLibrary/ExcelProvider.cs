using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Ciloci.Flee;
using log4net;
using Ninject;

using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;

using Excel = Microsoft.Office.Interop.Excel;
using org.iringtools.adapter.datalayer;


namespace org.iringtools.datalayer.excel
{
  
  public class ExcelProvider: IDisposable
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ExcelProvider));
    private AdapterSettings _settings = null;
            
    private Excel.Application _xlApplication = null;
    private Excel.Workbook _xlWorkBook = null;

    public ExcelConfiguration Configuration { get; set; }
        
    public ExcelProvider(AdapterSettings settings)
    {
      _settings = settings;

      if (File.Exists(_configurationPath))
      {
        Configuration = Utility.Read<ExcelConfiguration>(_configurationPath);

        _xlApplication = new Excel.Application();
        _xlWorkBook = _xlApplication.Workbooks.Open(Configuration.Location, 0, true, 5, Type.Missing, Type.Missing, true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, Type.Missing, false, false, Type.Missing, true, 1, Type.Missing);

        if (Configuration.Generate)
        {
          Configuration = ProcessConfiguration(Configuration);
          Configuration.Generate = false;
          Utility.Write<ExcelConfiguration>(Configuration, _configurationPath, true);
        }
      }
    }

    ~ExcelProvider()
    {
      Close();
    }

    public void Close()
    {
      if (_xlWorkBook != null)
      {
        _xlWorkBook.Close(true, Type.Missing, Type.Missing);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_xlWorkBook);
        _xlWorkBook = null;
      }

      if (_xlApplication != null)
      {
        _xlApplication.Quit();
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_xlApplication);
        _xlApplication = null;
      }

      GC.Collect();
    }

    public void Dispose()
    {
      Close();
    }

    private string _configurationPath
    {
      get
      {
        string path = Path.Combine(_settings["XmlPath"], "excel-configuration." + _settings["Scope"] + ".xml");

        if (!Path.IsPathRooted(path))
          path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

        return path;
      }
    }

    private List<ExcelColumn> GetColumns(Excel.Worksheet xlWorksheet, ExcelWorksheet cfWorksheet)
    {
      return GetColumns(GetRange(xlWorksheet, cfWorksheet.Range), cfWorksheet);
    }
        
    private List<ExcelColumn> GetColumns(Excel.Range xlRange, ExcelWorksheet cfWorksheet)
    {
      List<ExcelColumn> columns = new List<ExcelColumn>();
      
      for (int i = 1; i <= xlRange.Columns.Count; i++)
      {
        string header = xlRange.Cells[cfWorksheet.HeaderIdx, i].Value2;

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

    private Excel.Range GetRange(Excel.Worksheet xlWorksheet, string range)
    {
      Excel.Range xlRange = null;

      if (range != null && !range.Equals(string.Empty))
        xlRange = xlWorksheet.Range[range];
      else
        xlRange = xlWorksheet.UsedRange;

      return xlRange;
    }

    private ExcelConfiguration ProcessConfiguration(ExcelConfiguration config)
    {
      try
      {         
        if (config.Worksheets == null || config.Worksheets.Count == 0)
        {
          config.Worksheets = new List<ExcelWorksheet>();

          foreach (Excel.Worksheet xlWorksheet in _xlWorkBook.Worksheets)
          {
            ExcelWorksheet cfWorksheet = new ExcelWorksheet()
            {
              Name = xlWorksheet.Name,
              Label = xlWorksheet.Name,
              HeaderIdx = 1,
              DataIdx = 2
            };

            cfWorksheet.Columns = GetColumns(xlWorksheet, cfWorksheet);

            if (cfWorksheet.Columns != null && cfWorksheet.Columns.Count > 0)
            {
              cfWorksheet.Identifier = cfWorksheet.Columns[0].Name;
              config.Worksheets.Add(cfWorksheet);
            }
          }
        }
        else
        {

          foreach (ExcelWorksheet cfWorksheet in config.Worksheets)
          {
            if (cfWorksheet.Columns == null)
            {
              Excel.Worksheet xlWorksheet = _xlWorkBook.Worksheets[cfWorksheet.Name];
              if (xlWorksheet != null)
              {
                cfWorksheet.Columns = GetColumns(xlWorksheet, cfWorksheet);

                if (cfWorksheet.HeaderIdx <= 0)
                  cfWorksheet.HeaderIdx = 1;

                if (cfWorksheet.DataIdx <= 0)
                  cfWorksheet.DataIdx = 2;

                if (cfWorksheet.Identifier.Equals(String.Empty) && cfWorksheet.Columns.Count > 0)
                  cfWorksheet.Identifier = cfWorksheet.Columns[0].Name;
              }
              else
              {
                _logger.Error("Excel Workbook [" + config.Location + "] doesn't have a Worksheet named " + cfWorksheet.Name + ".");
              }
            }
          }
        }
        return config;
      }
      catch (Exception ex)
      {
        _logger.Error("Error Processing Excel Configuration File [" + config.Location + "]: " + ex);
        return config;
      }

    }
    
    public List<ExcelWorksheet> GetWorksheets()
    {
      return GetWorksheets(false);
    }    

    public List<ExcelWorksheet> GetWorksheets(bool withColumns)
    {      
      List<ExcelWorksheet> Worksheets = null;

      try
      {        
        Worksheets = new List<ExcelWorksheet>();

        if (_xlWorkBook != null)
        {
          foreach (Excel.Worksheet xlWorksheet in _xlWorkBook.Worksheets)
          {
            ExcelWorksheet cfWorksheet = new ExcelWorksheet()
            {
              Name = xlWorksheet.Name,
              Label = xlWorksheet.Name,
              HeaderIdx = 1,
              DataIdx = 2
            };

            if (withColumns)
            {
              cfWorksheet.Columns = GetColumns(GetRange(xlWorksheet, cfWorksheet.Range), cfWorksheet.HeaderIdx);

              if (cfWorksheet.Columns != null && cfWorksheet.Columns.Count > 0)
              {
                cfWorksheet.Identifier = cfWorksheet.Columns[0].Name;
                Worksheets.Add(cfWorksheet);
              }
            }
            else
            {
              Worksheets.Add(cfWorksheet);
            }
          }
        }

      }
      catch (Exception ex)
      {
        _logger.Error("Error Processing Excel File [" + Configuration.Location + "]: " + ex);        
      }

      return Worksheets;

    }

    public List<ExcelColumn> GetColumns(string WorksheetName)
    {
      return GetColumns(GetXLWorksheet(WorksheetName));
    }

    public List<ExcelColumn> GetColumns(string WorksheetName, int headerIdx)
    {
      return GetColumns(GetXLWorksheet(WorksheetName), headerIdx);
    }

    private List<ExcelColumn> GetColumns(Excel.Worksheet xlWorksheet)
    {
      return GetColumns(xlWorksheet, 1);
    }

    private List<ExcelColumn> GetColumns(Excel.Worksheet xlWorksheet, int headerIdx)
    {
      return GetColumns(GetRange(xlWorksheet, null), headerIdx); 
    }

    private List<ExcelColumn> GetColumns(Excel.Range xlRange, int headerIdx)
    {
      List<ExcelColumn> columns = new List<ExcelColumn>();

      for (int i = 1; i <= xlRange.Columns.Count; i++)
      {
        string header = xlRange.Cells[headerIdx, i].Value2;

        if (header != null)
        {
          header = header.Trim().Replace(" ", String.Empty);

          if (header != null && !header.Equals(String.Empty))
          {
            ExcelColumn cfColumn = new ExcelColumn()
            {
              Index = i,
              Name = header,
              Label = header,
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

    private Excel.Worksheet GetXLWorksheet(string label)
    {
      return GetXLWorksheet(label, false);
    }

    private Excel.Worksheet GetXLWorksheet(string label, bool createIsMissing)
    {
      try
      {
        ExcelWorksheet cfWorksheet = GetWorksheet(label);
        Excel.Worksheet Worksheet = _xlWorkBook.Worksheets.get_Item(cfWorksheet.Name);

        if (createIsMissing)
        {
          if (Worksheet == null)
          {
            Worksheet = _xlWorkBook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            Worksheet.Name = cfWorksheet.Name;            
          }
        }

        return Worksheet;
      }
      catch (Exception e)
      {
        throw new Exception("Error getting excel Worksheet for object of type [" + label + "].", e);
      }
    }
        
    public ExcelWorksheet GetWorksheet(string label)
    {
      return Configuration.Worksheets.FirstOrDefault<ExcelWorksheet>(o => o.Label == label);
    }

    public void Delete(string objectType, string identifier)
    {
      ExcelWorksheet cfWorksheet = GetWorksheet(objectType);
      Excel.Worksheet xlWorksheet = GetXLWorksheet(objectType);

      int row = GetRowIndex(xlWorksheet, cfWorksheet.Range, identifier);
      xlWorksheet.Rows[row].Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
    }
        
    private int GetRowIndex(Excel.Worksheet xlWorksheet, string range, string identifier)
    {
      try
      {

        Excel.Range xlRange = GetRange(xlWorksheet, range);
                
        Excel.Range findRange = xlRange.Find(identifier, Type.Missing, Type.Missing, Type.Missing, Excel.XlSearchOrder.xlByRows, Excel.XlSearchDirection.xlNext, true, Type.Missing, Type.Missing);

        int row = 0;
        if (findRange != null)
        {
          row = findRange.Cells[1, 1].Row;
        }
        else
        {
          row = xlRange.Rows.Count + 1;
        }

        return row;
      }
      catch (Exception e)
      {
        throw new Exception("Error getting excel row index for object [" + xlWorksheet.Name + "].", e);
      }
    }


    public void Update(ExcelDataObject dataObject)
    {
      string objectType = dataObject.SheetName;
      ExcelWorksheet cfWorksheet = GetWorksheet(objectType);
      Excel.Worksheet xlWorksheet = GetXLWorksheet(objectType);

      string identifier = dataObject.GetPropertyValue(cfWorksheet.Identifier).ToString();

      int row = GetRowIndex(xlWorksheet, cfWorksheet.Range, identifier);

      foreach (ExcelColumn column in cfWorksheet.Columns)
      {
        xlWorksheet.Cells[row, column.Index] = dataObject.GetPropertyValue(column.Name);
      }
    }

    public List<IDataObject> GetDataObjects(string objectType)
    {      
      return GetDataObjects(objectType, null);
    }

    public List<IDataObject> GetDataObjects(string objectType, IList<string> identifiers)
    {
      ExcelWorksheet cfWorksheet = GetWorksheet(objectType);
      Excel.Worksheet xlWorksheet = GetXLWorksheet(objectType);

      List<IDataObject> dataObjects = new List<IDataObject>();

      if (identifiers != null && identifiers.Count > 0)
      {
        foreach (string identifier in identifiers)
        {          
          ExcelDataObject dataObject = new ExcelDataObject()
          {
            SheetName = objectType
          };

          dataObject.SetPropertyValue(cfWorksheet.Identifier, identifier);

          int row = GetRowIndex(xlWorksheet, cfWorksheet.Range, identifier);

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

      return dataObjects;
    }

    public string GetIdentifier(string objectType)
    {
      ExcelWorksheet cfWorksheet = GetWorksheet(objectType);

      return cfWorksheet.Identifier;
    }

    public void SaveConfiguration(ExcelConfiguration excelConfiguration)
    {      
      Utility.Write<ExcelConfiguration>(excelConfiguration, _configurationPath, true);

      Configuration = excelConfiguration;
    }    
    
  }
}
