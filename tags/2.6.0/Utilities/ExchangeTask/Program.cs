using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using log4net;
using Newtonsoft.Json.Linq;
using System.Configuration;
using log4net.Config;
using org.iringtools.library;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace ExchangeTask
{
    class Program
    {
        private static ILog _logger = LogManager.GetLogger(typeof(Program));

        private static string _proxyCredentialToken = string.Empty;
        private static string _proxyHost = string.Empty;
        private static int _proxyPort = 0;

        private static string _ssoURL = string.Empty;
        private static string _clientId = string.Empty;
        private static string _clientSecret = string.Empty;
        private static string _authType = string.Empty;
        private static string _clientToken = string.Empty;

        private static int _requestTimeout = 28800000;  // 8 hours
        private static Sequence _sequence = null;

        static void Main(string[] args)
        {
            try
            {
                XmlConfigurator.Configure();

                string sequenceName = args[0];
                if (Initialize(sequenceName))
                {
                    RunTask();
                }

                _logger.Info("Exchange task completed.");
            }
            catch (Exception ex)
            {
                _logger.Error("Exchange task failed: " + ex.Message, ex);                
            }
        }

        static void RunTask()
        {
            _logger.Debug("RunTask ...");
            string clientToken = GetClientToken();

            foreach (Exchange x in _sequence.Exchanges)
            {
                string url = string.Format("{0}?scope={1}&xid={2}", x.BaseURL, x.Scope, x.ExchangeId);
                WebRequest request = CreateWebRequest(url);
                
                if (!string.IsNullOrEmpty(clientToken))
                {
                    _logger.Info("Use client token.");
                    request.Headers.Add("AuthType", _authType);
                    request.Headers.Add("ClientToken", clientToken);
                }
            
                request.Timeout = _requestTimeout;
                string responseText = GetResponseText(request);

                _logger.Info("Exchange finished: " + responseText);
            }
        }

        static bool Initialize(string sequenceName)
        {
            _logger.Debug("Initialize ...");

            try
            {
                #region init proxy info
                _proxyCredentialToken = ConfigurationManager.AppSettings["ProxyCredentialToken"];
                _proxyHost = ConfigurationManager.AppSettings["ProxyHost"];

                string proxyPort = ConfigurationManager.AppSettings["ProxyPort"];
                int.TryParse(proxyPort, out _proxyPort);
                #endregion

                #region init client credentials
                _ssoURL = ConfigurationManager.AppSettings["SSO_URL"];
                _clientId = ConfigurationManager.AppSettings["client_id"];
                _clientSecret = ConfigurationManager.AppSettings["client_secret"];
                _authType = ConfigurationManager.AppSettings["grant_type"];

                string clientKey = ConfigurationManager.AppSettings["client_key"];
                if (!string.IsNullOrEmpty(clientKey))
                {
                    _clientSecret = EncryptionUtility.Decrypt(_clientSecret, clientKey);
                }
                else
                {
                    _clientSecret = EncryptionUtility.Decrypt(_clientSecret);
                }
                #endregion

                #region init exchange sequence and timeout
                string configPath = ConfigurationManager.AppSettings["ExchangeConfig"];
                
                if (!File.Exists(configPath)) {
                    _logger.Error("Exchange Configuration not found.");
                    return false;
                }

                ExchangeConfig config = Utility.Read<ExchangeConfig>(configPath, true);
                foreach (Sequence sequence in config)
                {
                    if (sequence.Name.ToLower() == sequenceName.ToLower())
                    {
                        _sequence = sequence;
                        break;
                    }
                }

                if (_sequence == null)
                {
                    _logger.Error("Sequence [" + sequenceName + "] does not exist in exchange configuration.");
                    return false;
                }

                string httpRequestTimeout = ConfigurationManager.AppSettings["RequestTimeout"];
                if (!string.IsNullOrWhiteSpace(httpRequestTimeout))
                {
                    _requestTimeout = Convert.ToInt32(httpRequestTimeout);
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.Error("Initialization failed: " + ex.Message, ex);
                return false;
            }

            return true;
        }

        static WebRequest CreateWebRequest(string url)
        {
            WebRequest request = WebRequest.Create(url);

            if (!string.IsNullOrEmpty(_proxyHost))
            {
                WebCredentials proxyCreds = new WebCredentials(_proxyCredentialToken);
                if (proxyCreds.isEncrypted) proxyCreds.Decrypt();

                WebProxy proxy = new WebProxy(_proxyHost, _proxyPort);
                proxy.Credentials = proxyCreds.GetNetworkCredential();
                request.Proxy = proxy;
            }

            return request;
        }

        static string GetResponseText(WebRequest request)
        {
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string responseText = reader.ReadToEnd();
            return responseText;
        }

        static string GetClientToken()
        {
            string clientToken = string.Empty;

            try
            {
                WebRequest request = CreateWebRequest(_ssoURL);
                request.Method = "POST";
                string postData = string.Format("client_id={0}&client_secret={1}&grant_type={2}", _clientId, _clientSecret, _authType);
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(byteArray, 0, byteArray.Length);
                requestStream.Close();

                string responseText = GetResponseText(request);
                Dictionary<string, string> responseObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                clientToken = responseObj["access_token"];
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting access token: ", ex);
            }

            return clientToken;
        }
    }
}
