using System.Collections.Specialized;
using System.Configuration;
using Ninject;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;


using System.Web.Script.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace iRINGTools.Web.Models
{
    public class GridRepository : IGridRepository
    {
      private NameValueCollection _settings = null;
      private WebHttpClient _client = null;

			private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
            private DataContractJsonSerializer serializer;


			[Inject]
			public GridRepository()
      {
        _settings = ConfigurationManager.AppSettings;
				_client = new WebHttpClient(_settings["DataServiceURI"]);
              
      }

      public DataDictionary GetDictionary(string scope)
      {
        if (_settings["DataServiceURI"] == null)
        {
          return null;
        }
        string relativeUrl = string.Format("/{0}/dictionary", scope);
        return _client.Get<DataDictionary>(relativeUrl, true);
      }

      public DataItems GetDataItems(string app, string scope, string graph, DataFilter dataFilter, int start, int limit)
      {

        serializer = new DataContractJsonSerializer(typeof(DataItems));
        string relurl = string.Format("/{0}/{1}/{2}/filter?format=json&start={3}&limit={4}", app, scope, graph, start, limit);
        string allDataItemsJson = _client.Post<DataFilter, string>(relurl, dataFilter, true);
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(allDataItemsJson));
        DataItems dataItems = (DataItems)serializer.ReadObject(ms);
        ms.Close();
        return dataItems;
      }
                













		}
}