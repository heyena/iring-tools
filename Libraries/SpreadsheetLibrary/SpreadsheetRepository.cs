using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Ninject;

using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace org.iringtools.adapter.datalayer 
{
  public interface ISpreadsheetRepository  
  {
    SpreadsheetConfiguration GetConfiguration(string scope, string application);
    SpreadsheetConfiguration ProcessConfiguration(SpreadsheetConfiguration configuration);
    List<WorksheetPart> GetWorksheets(SpreadsheetConfiguration configuration);
    List<SpreadsheetColumn> GetColumns(SpreadsheetConfiguration configuration, string worksheetName);
    void Configure(string scope, string application, string datalayer, SpreadsheetConfiguration configuration);
  }

  public class SpreadsheetRepository : ISpreadsheetRepository
  {
    private AdapterSettings _settings { get; set; }
    private SpreadsheetProvider _provider { get; set; }
    private WebHttpClient _client { get; set; }

    [Inject]
    public SpreadsheetRepository()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);
      _client = new WebHttpClient(_settings["AdapterServiceUri"]);
    }

    private SpreadsheetProvider InitializeProvider(SpreadsheetConfiguration configuration)
    {
      if (_provider == null)
      {
        _provider = new SpreadsheetProvider(configuration);
      }

      return _provider;
    }

    public List<WorksheetPart> GetWorksheets(SpreadsheetConfiguration configuration)
    {
      List<WorksheetPart> wp = new List<WorksheetPart>();
      using (InitializeProvider(configuration))
      {
        foreach(SpreadsheetTable st in configuration.Tables)
        {
          wp.Add(_provider.GetWorksheetPart(st));
        }
        return wp;
      }
    }

    public List<SpreadsheetColumn> GetColumns(SpreadsheetConfiguration configuration, string worksheetName)
    {
      using (InitializeProvider(configuration))
      {
      SpreadsheetTable table = configuration.Tables.Find(c => c.Name == worksheetName);
      if (table != null)
        return _provider.GetColumns(table);
      else
        return new List<SpreadsheetColumn>();
      }
    }

    public SpreadsheetConfiguration ProcessConfiguration(SpreadsheetConfiguration configuration)
    {
      using (InitializeProvider(configuration))
      {
        return _provider.ProcessConfiguration(configuration);
      }
    }

    public void Configure(string scope, string application, string datalayer, SpreadsheetConfiguration configuration)
    {
      using (InitializeProvider(configuration))
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
    }

    public SpreadsheetConfiguration GetConfiguration(string scope, string application)
    {
      SpreadsheetConfiguration obj = null;

      try
      {
        XElement element = _client.Get<XElement>(string.Format("/{0}/{1}/configuration", scope, application));
        if (!element.IsEmpty)
        {
          obj = Utility.DeserializeFromXElement<SpreadsheetConfiguration>(element);
        }
      }
      catch (Exception)
      {

      }

      return obj;

    }

  }

}
