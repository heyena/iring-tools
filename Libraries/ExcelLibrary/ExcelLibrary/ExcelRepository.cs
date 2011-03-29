using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Ninject;

using org.iringtools.library;
using org.iringtools.adapter;
using System.Configuration;

namespace org.iringtools.datalayer.excel
{
  public interface IExcelRepository
  {
    ExcelConfiguration GetConfiguration(string scope, string application);

    List<ExcelWorksheet> GetWorksheets(string scope, string application);

    List<ExcelColumn> GetColumns(string scope, string application, string worksheetName);
  }

  public class ExcelRepository : IExcelRepository
  {
    private AdapterSettings settings { get; set; }

    [Inject]
    public ExcelRepository()
    {
      settings = new AdapterSettings();
      settings.AppendSettings(ConfigurationManager.AppSettings);

      settings["XmlPath"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\App_Data\\");
    }

    public ExcelConfiguration GetConfiguration(string scope, string application)
    {
      settings["Scope"] = string.Format("{0}.{1}", scope, application);
      
      ExcelProvider provider = new ExcelProvider(settings);
      return provider.Configuration;
    }

    public List<ExcelWorksheet> GetWorksheets(string scope, string application)
    {
      settings["Scope"] = string.Format("{0}.{1}", scope, application);

      ExcelProvider provider = new ExcelProvider(settings);
      return provider.GetWorksheets();
    }

    public List<ExcelColumn> GetColumns(string scope, string application, string worksheetName)
    {
      settings["Scope"] = string.Format("{0}.{1}", scope, application);

      ExcelProvider provider = new ExcelProvider(settings);
      return provider.GetColumns(worksheetName);
    }
    
  }
}
