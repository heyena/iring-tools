using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using Ninject;
using System.Configuration;
using log4net;

namespace org.iringtools.web.Models
{
  public class FacadeRepository : IFacadeRepository
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FacadeRepository));
    private WebHttpClient _facadeServiceClient = null;        
    private string _facadeServiceURI = string.Empty;
    private string _relativeUri = string.Empty;
    private string _proxyHost = "";
    private string _proxyPort = "";
    private WebProxy _webProxy = null;
    private string _facadeServiceUri = "";

    [Inject]
    public FacadeRepository()
    {
      var settings = ConfigurationManager.AppSettings;
      var _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];
      _facadeServiceUri = _settings["FacadeServiceUri"];

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
        _webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy; 
        _facadeServiceClient = new WebHttpClient(_facadeServiceUri, null, _webProxy);
      }
      else
      {
        _facadeServiceClient = new WebHttpClient(_facadeServiceUri);
      }
      #endregion
    }

    public Response RefreshGraph(string scope, string app, string graph, string baseUrl)
    {
      Response resp = null;
      try
      {
        var newServiceClient = GetFacadeServiceClient(baseUrl);
        _relativeUri = string.Format("/{0}/{1}/{2}/refresh", scope, app, graph);
        resp = newServiceClient.Get<Response>(_relativeUri);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
      return resp;
    }

    private WebHttpClient GetFacadeServiceClient(string baseUrl)
    {
      var baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      var facadeBaseUri = CleanBaseUrl(_facadeServiceUri.ToLower(), '/');

      return !baseUri.Equals(facadeBaseUri) ? GetServiceClinet(baseUrl, "facade/svc") : _facadeServiceClient;
    }

    public WebHttpClient GetServiceClinet(string uri, string serviceName)
    {
      WebHttpClient newServiceClient = null;
      var serviceUri = uri + "/" + serviceName;

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
        newServiceClient = new WebHttpClient(serviceUri, null, _webProxy);
      }
      else
      {
        newServiceClient = new WebHttpClient(serviceUri);
      }
      return newServiceClient;
    }

    private static string CleanBaseUrl(string url, char con)
    {
      var uri = new System.Uri(url);
      return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
    }
  }
}