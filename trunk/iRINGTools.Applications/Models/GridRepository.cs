using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

using iRINGTools.Web.Helpers;
using System.Text;
using org.iringtools.dxfr.manifest;

using org.iringtools.adapter;
using System.Web.Script.Serialization;

namespace iRINGTools.Web.Models
{
    public class GridRepository : IGridRepository
    {
      private NameValueCollection _settings = null;
      private WebHttpClient _client = null;

			private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
			private JavaScriptSerializer serializer;


			[Inject]
			public GridRepository()
      {
        _settings = ConfigurationManager.AppSettings;
				_client = new WebHttpClient(_settings["DataServiceURI"]);				
				serializer = new JavaScriptSerializer();
      }

      public DataDictionary GetDictionary(string scope)
      {
        if (_settings["DataServiceURI"] == null)
        {
          return null;
        }
        string relativeUrl = string.Format("/{0}/dictionary?format=xml", scope);
        return _client.Get<DataDictionary>(relativeUrl, true);
      }

      public DataItems GetDataItems(string app, string scope, string graph, DataFilter dataFilter, int start, int limit)
      {
          var format = "json";
        string relurl = string.Format("/{0}/{1}/{2}/filter?format={3}&start={4}&limit={5}", app, scope, graph, format, start, limit);
        string allDataItemsJson = _client.Post<DataFilter, string>(relurl, dataFilter,format, true);
        return (DataItems)serializer.Deserialize(allDataItemsJson, typeof(DataItems));
      }
                













		}
}