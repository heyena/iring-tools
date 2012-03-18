using System.Collections.Specialized;
using System.Configuration;
using Ninject;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace iRINGTools.Web.Models
{
  public class GridRepository : IGridRepository
  {
    private NameValueCollection _settings = null;
    private WebHttpClient _client = null;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));

    [Inject]
    public GridRepository()
    {
      _settings = ConfigurationManager.AppSettings;
      _client = new WebHttpClient(_settings["DataServiceURI"]);

    }

    public DataDictionary GetDictionary(string relUri)
    {
      if (_settings["DataServiceURI"] == null)
      {
        return null;
      }
      string relativeUrl = string.Format("/{0}/dictionary", relUri);
      return _client.Get<DataDictionary>(relativeUrl, true);
    }

    public DataItems GetDataItems(string endpoint, string context, string graph, DataFilter dataFilter, int start, int limit)
    {
      string fmt = "json";
      string relurl = string.Format("/{0}/{1}/{2}/filter?format={3}&start={4}&limit={5}", endpoint, context, graph, fmt, start, limit);
      string allDataItemsJson = _client.Post<DataFilter, string>(relurl, dataFilter, fmt, true);
      JavaScriptSerializer serializer = new JavaScriptSerializer();
      DataItems dataItems = (DataItems)serializer.Deserialize(allDataItemsJson, typeof(DataItems));

      return dataItems;
    }
  }
}