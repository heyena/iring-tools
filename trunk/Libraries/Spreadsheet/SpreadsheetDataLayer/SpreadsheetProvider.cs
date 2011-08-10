using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using org.iringtools.adapter;
using org.iringtools.utility;
using org.iringtools.library;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using org.iringtools.adapter.datalayer;


namespace org.iringtools.datalayer.spreadsheet
{
  public class SpreadsheetProvider : IDisposable
  {
    private AdapterSettings _settings = null;
    private string _configurationPath = string.Empty;
    private SpreadsheetConfiguration _configuration = null;
    private SpreadsheetDocument _document = null;

    public SpreadsheetProvider(SpreadsheetConfiguration configuration)
    {
      InitializeProvider(configuration);
    }

    public SpreadsheetProvider(AdapterSettings settings)
    {
      _settings = settings;
      _configurationPath = Path.Combine(_settings["XmlPath"], "spreadsheet-configuration." + _settings["Scope"] + ".xml");

      if (File.Exists(_configurationPath))
      {
        InitializeProvider(Utility.Read<SpreadsheetConfiguration>(_configurationPath));

        if (_configuration.Generate)
        {
          _configuration = ProcessConfiguration(_configuration);
          _configuration.Generate = false;
          Utility.Write<SpreadsheetConfiguration>(_configuration, _configurationPath, true);
        }
      }
    }

    public void InitializeProvider(SpreadsheetConfiguration configuration)
    {
      if (configuration != null)
      {
        _configuration = configuration;

        if (File.Exists(_configuration.Location))
        {
          _document = SpreadsheetDocument.Open(_configuration.Location, true);

          if (_configuration.Generate)
          {
            _configuration = ProcessConfiguration(_configuration);
            _configuration.Generate = false;
            Utility.Write<SpreadsheetConfiguration>(_configuration, _configurationPath, true);
          }
        }        
      }
    }

    public static DataType GetDataType(EnumValue<CellValues> type)
    {
      if (type == CellValues.Boolean) 
      {
        return DataType.Boolean;
      }
      else if (type == CellValues.Date)
      {
        return DataType.DateTime;
      }      
      else if (type == CellValues.Number)
      {
        return DataType.Double;
      }      
      else
      {
        return DataType.String;
      }
    }

    public static EnumValue<CellValues> GetCellValue(DataType type)
    {
      if (type == DataType.Boolean)
      {
        return CellValues.Boolean;
      }
      else if (type == DataType.DateTime)
      {
        return CellValues.Date;
      }
      else if (type == DataType.TimeSpan)
      {
        return CellValues.Date;
      }
      else if (type == DataType.Decimal)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Double)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Int16)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Int32)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Int64)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Single)
      {
        return CellValues.Number;
      }
      else
      {
        return CellValues.String;
      }
    }

    public List<SpreadsheetColumn> GetColumns(string table)
    {
        SpreadsheetTable t = new SpreadsheetTable();
        t.Name = table;
        return GetColumns(t);
    }

    public List<SpreadsheetColumn> GetColumns(SpreadsheetTable table)
    {
      if (table.Columns == null)
      {
        WorksheetPart worksheetPart = GetWorksheetPart(table);
               
        List<SpreadsheetColumn> columns = new List<SpreadsheetColumn>();

        Row row = worksheetPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex == 1).First();

        foreach (Cell cell in row.ChildElements)
        {
          string value = GetValue(cell);
                    
          if (table.HeaderRow == 0)
            value = SpreadsheetReference.GetColumnName(cell.CellReference);  
          
          SpreadsheetColumn column = new SpreadsheetColumn
          {
            Name = value,
            Label = value,
            DataType = GetDataType(cell.DataType),
            ColumnIdx = SpreadsheetReference.GetColumnName(cell.CellReference)
          };

          columns.Add(column);
        }        

        return columns;
      }
      else
      {
        return table.Columns;
      }      
    }

    public SpreadsheetConfiguration ProcessConfiguration(SpreadsheetConfiguration configuration)
    {
      List<SpreadsheetTable> tables = new List<SpreadsheetTable>();

      DefinedNames definedNames = _document.WorkbookPart.Workbook.DefinedNames;

      if (definedNames != null)
      {
        foreach (DefinedName definedName in definedNames)
        {
          SpreadsheetTable table = new SpreadsheetTable
          {
            TableType = TableType.DefinedName,
            Name = definedName.Name,
            Label = definedName.Name,
            Reference = definedName.InnerText,
            HeaderRow = 1
          };

          table.Columns = GetColumns(table);
          table.Identifier = table.Columns[0].Name;

          tables.Add(table);
        }
      }

      Sheets sheets = _document.WorkbookPart.Workbook.Sheets;

      if (sheets != null)
      {
        foreach (Sheet sheet in sheets)
        {
          WorksheetPart worksheetPart = (WorksheetPart)_document.WorkbookPart.GetPartById(sheet.Id);

          SpreadsheetTable table = new SpreadsheetTable
          {
            TableType = TableType.Worksheet,
            Name = sheet.Name,
            Label = sheet.Name,
            Reference = string.Format("'{0}!'{1}", sheet.Name, worksheetPart.Worksheet.SheetDimension.Reference),
            HeaderRow = 1
          };

          table.Columns = GetColumns(table);
          table.Identifier = table.Columns[0].Name;
          
          tables.Add(table);
        }
      }

      configuration.Tables = tables;

      return configuration;
    }

    public WorksheetPart GetWorksheetPart(SpreadsheetTable table)
    {
      string sheetName = SpreadsheetReference.GetSheetName(table.Reference);
      return GetWorksheetPart(sheetName);
    }

    public WorksheetPart GetWorksheetPart(string sheetName)
    {
      //get worksheet based on defined name 
      string relId = _document.WorkbookPart.Workbook.Descendants<Sheet>()
                           .Where(s => sheetName.Equals(s.Name))
                           .First()
                           .Id;

      return (WorksheetPart)_document.WorkbookPart.GetPartById(relId);
    }
    
    private string GetCellValue(WorksheetPart worksheetPart, string startCol, int startRow)
    {
      string reference = startCol + startRow;

      //get exact cell based on reference 
      Cell cell = worksheetPart.Worksheet.Descendants<Cell>()
                      .Where(c => reference.Equals(c.CellReference))
                      .First();

      return GetValue(cell);
    }

    public string GetValue(Cell cell)
    {
      if (cell.ChildElements.Count == 0)
        return null;

      //get cell value
      string value = cell.CellValue.InnerText;

      //Look up real value from shared string table 
      if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
        value = _document.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;

      return value;
    }

    public SpreadsheetConfiguration GetConfiguration()
    {
      return _configuration;
    }

    public SpreadsheetTable GetConfigurationTable(string objectType)
    {
      return _configuration.Tables.First<SpreadsheetTable>(t => objectType.Equals(t.Name));
    }

    public void Save()
    {
      _document.WorkbookPart.Workbook.Save();
    }

    public void Dispose()
    {
      //Close();
    }
  }
}
