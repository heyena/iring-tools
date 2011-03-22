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

using org.iringtools.library;
using org.iringtools.utility;

using Excel = Microsoft.Office.Interop.Excel;

namespace org.iringtools.datalayer.excel
{
  public interface IExcelProvider
  {
    string Source { get; set; }
    
    void Close();
    //bool Save();

    List<ExcelWorksheet> GetWorksheets();
    List<ExcelWorksheet> GetWorksheets(bool recursive);

    List<ExcelColumn> GetColumns(string worksheetName);
    List<ExcelColumn> GetColumns(string worksheetName, int headerIdx);
  }

  public class ExcelProvider: IExcelProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ExcelProvider));

    private string _sourcePath = null;

    private Excel.Application _xlApplication = null;
    private Excel.Workbook _xlWorkBook = null;

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

    public string Source { 
      get {
        return _sourcePath;
      }
      set
      {
        _sourcePath = value;

        Close();

        _xlApplication = new Excel.Application();
        _xlWorkBook = _xlApplication.Workbooks.Open(_sourcePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
      }
    }

    public List<ExcelWorksheet> GetWorksheets()
    {
      return GetWorksheets(false);
    }

    public List<ExcelWorksheet> GetWorksheets(bool recursive)
    {      
      List<ExcelWorksheet> workSheets = null;

      try
      {        
        workSheets = new List<ExcelWorksheet>();

        foreach (Excel.Worksheet xlWorkSheet in _xlWorkBook.Worksheets)
        {
          ExcelWorksheet cfWorkSheet = new ExcelWorksheet()
          {
            Name = xlWorkSheet.Name,
            Label = xlWorkSheet.Name,
            HeaderIdx = 1,
            DataIdx = 2
          };

          if (recursive)
          {
            cfWorkSheet.Columns = GetColumns(xlWorkSheet, cfWorkSheet.HeaderIdx);

            if (cfWorkSheet.Columns != null && cfWorkSheet.Columns.Count > 0)
            {
              cfWorkSheet.Identifier = cfWorkSheet.Columns[0].Name;
              workSheets.Add(cfWorkSheet);
            }
          }
          else
          {
            workSheets.Add(cfWorkSheet);
          }
            
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error Processing Excel File [" + _sourcePath + "]: " + ex);        
      }

      return workSheets;

    }

    public List<ExcelColumn> GetColumns(string worksheetName)
    {
      return GetColumns(GetWorkSheet(worksheetName));
    }

    public List<ExcelColumn> GetColumns(string worksheetName, int headerIdx)
    {
      return GetColumns(GetWorkSheet(worksheetName), headerIdx);
    }

    private List<ExcelColumn> GetColumns(Excel.Worksheet xlWorkSheet)
    {
      return GetColumns(xlWorkSheet, 1);
    }

    private List<ExcelColumn> GetColumns(Excel.Worksheet xlWorkSheet, int headerIdx)
    {
      List<ExcelColumn> columns = new List<ExcelColumn>();

      Excel.Range usedRange = xlWorkSheet.UsedRange;

      for (int i = 1; i <= usedRange.Columns.Count; i++)
      {
        string header = usedRange.Cells[headerIdx, i].Value2;

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

    private Excel.Worksheet GetWorkSheet(string name)
    {
      return GetWorkSheet(name, false);
    }

    private Excel.Worksheet GetWorkSheet(string name, bool createIsMissing)
    {
      try
      {
        Excel.Worksheet worksheet = _xlWorkBook.Worksheets.get_Item(name);

        if (createIsMissing)
        {
          if (worksheet == null)
          {
            worksheet = _xlWorkBook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            worksheet.Name = name;            
          }
        }

        return worksheet;
      }
      catch (Exception e)
      {
        throw new Exception("Error getting excel worksheet for object of type [" + name + "].", e);
      }
    }
    
  }
}
