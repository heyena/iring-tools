using System;
using System.Collections.Specialized;
using System.Configuration;
using Ninject;
using iRINGTools.Web.Models;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;
using System.Net;

namespace org.iringtools.web.Models
{
  public class GridRepository : IGridRepository
  {
    private WebHttpClient _dataServiceClient = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));   
    ServiceSettings _settings = null;
    string _proxyHost = "";
    string _proxyPort = "";
    string dataServiceUri = null;

    [Inject]
    public GridRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);
      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];

      #region initialize webHttpClient for converting old mapping      

        if (_settings["DataServiceURI"] == null) return;
        dataServiceUri = _settings["DataServiceURI"];      
     
        if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
        {
            var webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
            _dataServiceClient = new WebHttpClient(dataServiceUri, null, webProxy);
        }
        else
        {
            _dataServiceClient = new WebHttpClient(dataServiceUri);

        }
        #endregion
    }   

    public string DataServiceUri()
    {
      GetSetting();
      var dataServiceUri = _settings["DataServiceURI"];
      var response = "";     

      if (string.IsNullOrEmpty(dataServiceUri))        
      {
        response = "DataServiceURI is not configured.";
        _logger.Error(response);
      }

      return response;
    }

    public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
    {
      DataDictionary obj = null;

      try
      {
        var newServiceClient = PrepareServiceClient(baseUrl, "data");
        obj = newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary?format=xml", endpoint, contextName), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public DataItems GetDataItems(string endpoint, string context, string graph, DataFilter dataFilter, int start, int limit, string baseUrl)
    {
      var newServiceClient = PrepareServiceClient(baseUrl, "data");      
      var fmt = "json";
      var relUrl = string.Format("/{0}/{1}/{2}/filter?format={3}&start={4}&limit={5}", endpoint, context, graph, fmt, start, limit);
      var json = newServiceClient.Post<DataFilter, string>(relUrl, dataFilter, fmt, true);
      
      var serializer = new DataItemSerializer();
      var dataItems = serializer.Deserialize<DataItems>(json, false); 
      return dataItems;
    }

    private void GetSetting()
    {
      if (_settings == null)
        _settings = new ServiceSettings();     
    }

    private void GetAllSetting()
    {
      if (_settings == null)
        _settings = new ServiceSettings();
      GetProxy();
    }

    private void GetProxy()
    {     
      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];
    }

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      GetSetting();
      if (string.IsNullOrEmpty(baseUrl))
        return _dataServiceClient;

      var baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      var adapterBaseUri = CleanBaseUrl(dataServiceUri.ToLower(), '/');

      return !baseUri.Equals(adapterBaseUri) ? GetServiceClient(baseUrl, serviceName) : _dataServiceClient;
    }

    private static string CleanBaseUrl(string url, char con)
    {
      try
      {
        var uri = new System.Uri(url);
        return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
      }
      catch (Exception) { }
      return null;
    }

    private WebHttpClient GetServiceClient(string uri, string serviceName)
    {
      GetProxy();
      WebHttpClient newServiceClient = null;
        var serviceUri = uri + "/" + serviceName;

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
          var webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
          newServiceClient = new WebHttpClient(serviceUri, null, webProxy);
      }
      else
      {
        newServiceClient = new WebHttpClient(serviceUri);
      }
      return newServiceClient;
    }
  }
}