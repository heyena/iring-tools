using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace QMXFGenerator
{
  static class ExcelLibrary
  {
    public static Workbook OpenWorkbook(string excelFilePath)
    {
      ApplicationClass application = new ApplicationClass();

      Workbook workbook =
        application.Workbooks.Open(excelFilePath, 0, false,
          5, null, null, true, XlPlatform.xlWindows, "\t", false, false, 0,
          false, 1, 0);

      return workbook;
    }

    public static void CloseWorkbook(Workbook workbook)
    {
        CloseWorkbook(true, workbook);
    }

    public static void CloseWorkbook(bool save, Workbook workbook)
    {
        Application application = workbook.Application;
        workbook.Close(save, Type.Missing, Type.Missing);
        application.Quit();
    }

    public static Worksheet GetWorksheet(Workbook workbook, int worksheet)
    {
      return (Worksheet)workbook.Worksheets.get_Item(worksheet);
    }
  }
}
