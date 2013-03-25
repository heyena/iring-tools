using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

namespace org.iringtools.web.Models
{
  public class MappingRepository : IMappingRepository
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MappingRepository));
      private string _proxyHost = "";
    private string _proxyPort = "";
    private WebProxy _webProxy = null;
    private string _adapterServiceUri = "";   
    private WebHttpClient _adapterServiceClient = null;

    [Inject]
    public MappingRepository()
    {
      var settings = ConfigurationManager.AppSettings;
      var _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];
      _adapterServiceUri = _settings["AdapterServiceUri"];      

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
        _webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _adapterServiceClient = new WebHttpClient(_adapterServiceUri, null, _webProxy);        
      }
      else
      {       
        _adapterServiceClient = new WebHttpClient(_adapterServiceUri);        
      }
      #endregion
    }

    public Mapping GetMapping(string scopeName, string applicationName)
    {
      Mapping obj = null;

      try
      {
        obj = _adapterServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public void UpdateMapping(Mapping mapping, string scopeName, string applicationName)
    {
      XElement mappingXml = XElement.Parse(Utility.SerializeDataContract<Mapping>(mapping));
      try
      {
        _adapterServiceClient.Post<XElement>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), mappingXml, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
    }

    private WebHttpClient GetAdapterServiceClient(string baseUrl)
    {
      var baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      var adapterBaseUri = CleanBaseUrl(_adapterServiceUri.ToLower(), '/');

      return !baseUri.Equals(adapterBaseUri) ? GetServiceClinet(baseUrl, "adapter") : _adapterServiceClient;
    }

    public WebHttpClient GetServiceClinet(string uri, string serviceName)
    {
      WebHttpClient newServiceClient = null;
      string serviceUri = uri + "/" + serviceName;

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