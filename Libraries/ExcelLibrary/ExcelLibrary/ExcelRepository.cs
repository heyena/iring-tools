using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using org.iringtools.library;

namespace org.iringtools.datalayer.excel
{
  public interface IExcelRepository
  {
    List<ExcelWorksheet> GetWorksheets(string sourcePath);
  }

  public class ExcelRepository : IExcelRepository
  {    
    public ExcelRepository()
    { 
    }
    
    public List<ExcelWorksheet> GetWorksheets(string sourcePath)
    {
      ExcelProvider provider = new ExcelProvider(sourcePath);
      return provider.GetWorksheets();
    }



  }
}
