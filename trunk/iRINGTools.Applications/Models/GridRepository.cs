using System;
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
using org.iringtools.adapter;
using System.Net;

namespace iRINGTools.Web.Models
{
  public class GridRepository : IGridRepository
  {
    private WebHttpClient _dataServiceClient = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));

    [Inject]
    public GridRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string dataServiceUri = null;

      if (_settings["DataServiceURI"] != null)
      {
        dataServiceUri = _settings["DataServiceURI"];      
     
        if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
        {
          WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));

          webProxy.Credentials = _settings.GetProxyCredential();

          _dataServiceClient = new WebHttpClient(dataServiceUri, null, webProxy);
        }
        else
        {
          _dataServiceClient = new WebHttpClient(dataServiceUri);

        }
      }
      #endregion

    }

    public string DataServiceUri()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);
      string dataServiceUri = _settings["DataServiceURI"];
      string response = "";     

      if (string.IsNullOrEmpty(dataServiceUri)        
      {
        response = "DataServiceURI is not configured.";
        _logger.Error(response);
      }

      return response;
    }

    public DataDictionary GetDictionary(string relUri)
    {      
      string relativeUrl = string.Format("/{0}/dictionary?format=xml", relUri);
      return _dataServiceClient.Get<DataDictionary>(relativeUrl, true);
    }

    public DataItems GetDataItems(string endpoint, string context, string graph, DataFilter dataFilter, int start, int limit)
    {
      string fmt = "json";
      string relUrl = string.Format("/{0}/{1}/{2}/filter?format={3}&start={4}&limit={5}", endpoint, context, graph, fmt, start, limit);
      string json = _dataServiceClient.Post<DataFilter, string>(relUrl, dataFilter, fmt, true);
      
      DataItemSerializer serializer = new DataItemSerializer();
      DataItems dataItems = serializer.Deserialize<DataItems>(json, false); 
      return dataItems;
    }
  }
}