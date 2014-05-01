using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using log4net;
using Newtonsoft.Json.Linq;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using org.iringtools.agent;


namespace org.iringtools.agent
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
        private static TaskSequence _sequence = null;
        

        static void Main(string[] args)
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                //GenerateSampleAgentSequence();
                string sequenceName = "InfoWorks"; //args[0];
                if (Initialize(sequenceName))
                {
                    RunTask();
                }

                _logger.Info("Agent task completed.");
            }
            catch (Exception ex)
            {
                _logger.Error("Agent task failed: " + ex.Message, ex);
            }
        }

        static void RunTask()
        {
            _logger.Debug("RunTask ...");
            AdapterSettings adapterSettings;
            NameValueCollection settings;

            string clientToken = GetClientToken();

            foreach (Task task in _sequence.Tasks)
            {
                string assembly = task.Assembly;
                string project = task.Project;
                string app = task.App;

                settings = ConfigurationManager.AppSettings;

                adapterSettings = new AdapterSettings();
                adapterSettings.AppendSettings(settings);
                adapterSettings["ProjectName"] = project; 
                adapterSettings["ApplicationName"] = app; 
                adapterSettings["Scope"] = project + "." + app;

                if (task.TaskType.ToLower().Equals("caching"))
                {
                    //load the data layer dll using agent provider

                    AgentProvider agentProvider = new AgentProvider(adapterSettings);
                    agentProvider.ProcessTask(task);
                    

                    _logger.Info("Caching Task finished: " );

                }
                else if (task.TaskType.ToLower().Equals("exchange"))
                {
                    string url = string.Format("{0}?scope={1}&xid={2}", task.BaseURL, task.Scope, task.ExchangeId);
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

                #region init agent sequence and timeout
                string configPath = ConfigurationManager.AppSettings["AgentConfig"];

                if (!File.Exists(configPath))
                {
                    _logger.Error("Agent Configuration not found.");
                    return false;
                }

                AgentConfig config = Utility.Read<AgentConfig>(configPath, true);
                foreach (TaskSequence sequence in config)
                {
                    if (sequence.Name.ToLower() == sequenceName.ToLower())
                    {
                        _sequence = sequence;
                        break;
                    }
                }

                if (_sequence == null)
                {
                    _logger.Error("Sequence [" + sequenceName + "] does not exist in Agent configuration.");
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

        static void GenerateSampleAgentSequence()
        {
            TaskSequence sequence = new TaskSequence
            {
                Name = "Test",
                Tasks = new List<Task>
                {
                    new Task
                    {
                        BaseURL = "",
                        Assembly = "",
                        Scope = "",
                        App = ""
                    }
                }
            };

            //Stream stream = Utility.SerializeToBinaryStream<AgentSequence>(sequence);

            //Utility.WriteStream(stream, "SampleSequence.dat");

            Stream stream = Utility.ReadStream("SampleAgentSequence.dat");

            sequence = Utility.DeserializeBinaryStream<TaskSequence>(stream);

            Utility.Write<TaskSequence>(sequence, "SampleAgentSequence.xml", true);
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
