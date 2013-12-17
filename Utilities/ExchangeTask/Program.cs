using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using log4net;
using Newtonsoft.Json.Linq;
using System.Configuration;
using log4net.Config;

namespace ExchangeTask
{
    class Program
    {
        private static ILog _logger = LogManager.GetLogger(typeof(Program));

        private static string _proxyHost = String.Empty;
        private static int _proxyPort = 0;
        private static string _proxyCredentialToken = String.Empty;

        private static string _ssoUrl = String.Empty;
        private static string _clientId = String.Empty;
        private static string _clientSecret = String.Empty;
        private static string _grantType = String.Empty;
        private static string _clientToken = String.Empty;

        private static string _configurationFiles = String.Empty;
        private static List<Properties> _exchangeConfigurationList = new List<Properties>();
        private static WebHttpClient _httpClient = null;
        private static int _httpRequestTimeout;

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            RunExchange(args);
        }

        private static void RunExchange(string[] args)
        {
            if (Initialize(args))
            {

                _logger.Debug("Requesting client token ...");

                if (String.IsNullOrWhiteSpace(_ssoUrl))
                {
                    _httpClient = new WebHttpClient(_ssoUrl);
                }
                else
                {
                    WebCredentials credential = new WebCredentials(_proxyCredentialToken);
                    _httpClient = new WebHttpClient(_ssoUrl, credential.userName, credential.password, credential.domain, _proxyHost, _proxyPort);
                }

                string requestBody = String.Format("client_id={0}&client_secret={1}&grant_type={2}", _clientId, _clientSecret, _grantType);
                string responseBody = _httpClient.PostMessage("", requestBody, true);

                dynamic responseObject = JObject.Parse(responseBody);
                _clientToken = responseObject.access_token;

                foreach (var exchangeConfiguration in _exchangeConfigurationList)
                {
                    string baseUrl = exchangeConfiguration["BaseUrl"];
                    string scope = exchangeConfiguration["Scope"]; ;
                    string exchangeId = exchangeConfiguration["ExchangeId"];

                    try
                    {
                        string relativeUrl = String.Format("?scope={0}&xid={1}", scope, exchangeId);

                        if (String.IsNullOrWhiteSpace(_proxyCredentialToken))
                        {
                            _httpClient = new WebHttpClient(baseUrl);
                        }
                        else
                        {
                            WebCredentials credential = new WebCredentials(_proxyCredentialToken);
                            _httpClient = new WebHttpClient(baseUrl, credential.userName, credential.password, credential.domain, _proxyHost, _proxyPort);
                        }

                        if (_httpRequestTimeout != 0)
                            _httpClient.Timeout = _httpRequestTimeout;

                        if (_httpClient.Headers == null)
                            _httpClient.Headers = new Dictionary<string, string>();

                        _httpClient.Headers.Add("AuthType", _grantType);
                        _httpClient.Headers.Add("ClientToken", _clientToken);

                        _logger.Debug(String.Format("Sending exchange request  for scope: {0} and exchange Id: {1}", scope, exchangeId));
                        
                        string responseMessage = _httpClient.GetMessage(relativeUrl);
                        _logger.Info("Response message: " + responseMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(String.Format("Error in exchange aginst for scope : {0} and exchange Id: {1}", scope, exchangeId), ex);
                    }
                }
            }
        }

        private static bool Initialize(string[] args)
        {
            _logger.Debug("Initialize ...");

            try
            {
                _proxyHost = ConfigurationManager.AppSettings["ProxyHost"];
                _proxyCredentialToken = ConfigurationManager.AppSettings["ProxyCredentialToken"];
                _ssoUrl = ConfigurationManager.AppSettings["SSO_URL"];
                _clientId = ConfigurationManager.AppSettings["client_id"];
                _clientSecret = ConfigurationManager.AppSettings["client_secret"];                
                _grantType = ConfigurationManager.AppSettings["grant_type"];
                _configurationFiles = ConfigurationManager.AppSettings["ConfigurationFiles"];

                string clientKey = ConfigurationManager.AppSettings["client_key"];
                if (!string.IsNullOrEmpty(clientKey))
                {
                    _clientSecret = EncryptionUtility.Decrypt(_clientSecret, clientKey);
                }
                else
                {
                    _clientSecret = EncryptionUtility.Decrypt(_clientSecret);
                }

                string httpRequestTimeout = ConfigurationManager.AppSettings["HttpRequestTimeout"];
                if (!String.IsNullOrWhiteSpace(httpRequestTimeout))
                    _httpRequestTimeout = Convert.ToInt32(httpRequestTimeout);

                string proxyPortString = ConfigurationManager.AppSettings["ProxyPort"];
                int.TryParse(proxyPortString, out _proxyPort);

                string[] confArray = _configurationFiles.Split(',');
                foreach (var path in confArray)
                {
                    _logger.Debug("Reading configuration files...");

                    try
                    {
                        if (!String.IsNullOrWhiteSpace(path))
                        {
                            Properties props = new Properties();
                            props.Load(path);
                            _exchangeConfigurationList.Add(props);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(String.Format("Error in reading file: {0}", path), ex);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
