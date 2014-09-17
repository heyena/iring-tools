﻿using org.iringtools.adapter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using org.iringtools.utility;
using org.iringtools.library;
using org.iringtools.mapping;
using log4net;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Xml;

namespace iRINGAgentService
{
    class AgentProvider : BaseProvider
    {
        private static ILog _logger = LogManager.GetLogger(typeof(AgentProvider));
        private string[] arrSpecialcharlist;
        private string[] arrSpecialcharValue;

        private static string _proxyCredentialToken = string.Empty;
        private static string _proxyHost = string.Empty;
        private static int _proxyPort = 0;

        private static string _ssoURL = string.Empty;
        private static string _clientId = string.Empty;
        private static string _clientSecret = string.Empty;
        private static string _authType = string.Empty;
        private static string _clientToken = string.Empty;

        private static int _requestTimeout = 28800000;  // 8 hours
        

        [Inject]
        public AgentProvider(NameValueCollection settings)
            : base(settings)
        {

            try
            {
                if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
                {
                    arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
                    arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
                }

            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }

        public void ProcessTask(AgentCache config)
        {
            string project = string.Empty; ;
            string app = string.Empty; ;
            AdapterSettings adapterSettings;
            NameValueCollection settings;
           
            try
            {
                project = config.Project;
                app = config.App;
                _ssoURL = config.SsoUrl;
                _clientId = config.ClientId;
                _clientSecret = config.ClientSecret;
                _authType = config.GrantType;

               
                settings = ConfigurationManager.AppSettings;
                settings["AppKey"] = config.AppKey;
                settings["AccessToken"] = config.AccessToken;

                adapterSettings = new AdapterSettings();
                adapterSettings.AppendSettings(settings);

                Initialize();
                string clientToken = GetClientToken();

                 RefreshCache(project, app, false);
               
                 _logger.Info("Caching task finished: ");
              
            }
            catch (Exception e)
            {
                _logger.Error("Error processing caching task: " + config.TaskName + "  " + e.Message);
            }
        }

        public void ProcessTask(AgentExchange config)
        {
            string baseUrl = string.Empty; ;
            string scope = string.Empty; ;
            string exchangeId = string.Empty; ;

            try
            {
         
                baseUrl = config.BaseUrl;
                scope = config.Scope;
                exchangeId = config.ExchangeId;
                _ssoURL = config.SsoUrl;
                _clientId = config.ClientId;
                _clientSecret = config.ClientSecret;
                _authType = config.GrantType;

                Initialize();
                string clientToken = GetClientToken();

                string url = string.Format("{0}?scope={1}&xid={2}", baseUrl, scope, exchangeId);
                WebRequest request = CreateWebRequest(url);

                if (!string.IsNullOrEmpty(clientToken))
                {
                    _logger.Info("Use client token.");
                    request.Headers.Add("AuthType", _authType);
                    request.Headers.Add("ClientToken", clientToken);
                    request.Headers.Add("UserName", _clientId);
                }

                request.Timeout = _requestTimeout;
                string responseText = GetResponseText(request);

                _logger.Info("Exchange task finished: " + responseText);

            }
            catch (Exception e)
            {
                _logger.Error("Error processing exchnage task: " + config.TaskName + "  " + e.Message);
            }
        }

        static bool Initialize()
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
                //_ssoURL = ConfigurationManager.AppSettings["SSO_URL"];
                //_clientId = ConfigurationManager.AppSettings["client_id"];
                //_clientSecret = ConfigurationManager.AppSettings["client_secret"];
                //_authType = ConfigurationManager.AppSettings["grant_type"];

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

                #region init agent sequence and timeout
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
