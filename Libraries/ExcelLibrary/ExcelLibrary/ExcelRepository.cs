using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Ninject;

using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.Xml.Linq;

namespace org.iringtools.datalayer.excel
{
  public interface IExcelRepository
  {
    ExcelConfiguration GetConfiguration(string scope, string application);

    List<ExcelWorksheet> GetWorksheets(string scope, string application);

    List<ExcelColumn> GetColumns(string scope, string application, string worksheetName);

    void Configure(string scope, string application, string datalayer, ExcelConfiguration configuration);
  }

  public class ExcelRepository : IExcelRepository
  {
    private AdapterSettings _settings { get; set; }
    private ExcelProvider _provider { get; set; }
    private WebHttpClient _client { get; set; }

    [Inject]
    public ExcelRepository()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);
      _client = new WebHttpClient(_settings["AdapterServiceUri"]);
    }

    private ExcelProvider InitializeProvider(string scope, string application)
    {
      _settings["Scope"] = string.Format("{0}.{1}", scope, application);

      if (_provider == null)
      {
        _provider = new ExcelProvider(_settings);
      }

      return _provider;
    }
    
    public List<ExcelWorksheet> GetWorksheets(string scope, string application)
    {
      using (InitializeProvider(scope, application))
      {
        return _provider.GetWorksheets();
      }
    }

    public List<ExcelColumn> GetColumns(string scope, string application, string worksheetName)
    {      
      using (InitializeProvider(scope, application))
      {
        return _provider.GetColumns(worksheetName);
      }
    }

    public void Configure(string scope, string application, string datalayer, ExcelConfiguration configuration)
    {
      List<MultiPartMessage> requestMessages = new List<MultiPartMessage>();
      //string sourceFile = configuration.Location;
      //configuration.Location = Path.GetFileName(sourceFile);

      /*
      requestMessages.Add(new MultiPartMessage
      {
        name = "SourceFile",
        fileName = Path.GetFileName(sourceFile),
        message = Utility.ReadStream(sourceFile),
        mimeType = Utility.GetMimeType(sourceFile),        
        type = MultipartMessageType.File
      });
      */
            
      requestMessages.Add(new MultiPartMessage
      {
        name = "DataLayer",
        message = datalayer,
        type = MultipartMessageType.FormData
      });

      requestMessages.Add(new MultiPartMessage
      {
        name = "Configuration",
        message = Utility.Serialize<XElement>(Utility.SerializeToXElement(configuration), true),
        type = MultipartMessageType.FormData
      });

      _client.PostMultipartMessage(string.Format("/{0}/{1}/configure", scope, application), requestMessages);
    }

    public ExcelConfiguration GetConfiguration(string scope, string application)
    {       
      using (InitializeProvider(scope, application))
      {
        //XElement element = _client.Get<XElement>("{0}/{1}/configuration");
        //if (!element.IsEmpty)
        //{
        //   ExcelConfiguration config = Utility.DeserializeFromXElement<ExcelConfiguration>(element);
        //    _provider.SaveConfiguration(config);          
        //}
        return _provider.Configuration;
      }
    }
  }
}
