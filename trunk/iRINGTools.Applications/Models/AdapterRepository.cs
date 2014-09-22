using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.ServiceModel.Web;


namespace iRINGTools.Web.Models
{
    public class AdapterRepository : IAdapterRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
        private CustomError _CustomError = null;
        protected ServiceSettings _settings;
        protected string _proxyHost;
        protected string _proxyPort;
        protected string _adapterServiceUri = null;
        protected string _dataServiceUri = null;
        protected string _hibernateServiceUri = null;
        protected string _referenceDataServiceUri = null;
        protected string _servicesBasePath = string.Empty;

        public IDictionary<string, string> AuthHeaders { get; set; }

        public AdapterRepository()
        {
            NameValueCollection settings = ConfigurationManager.AppSettings;

            _settings = new ServiceSettings();
            _settings.AppendSettings(settings);

            _proxyHost = _settings["ProxyHost"];
            _proxyPort = _settings["ProxyPort"];

            _adapterServiceUri = _settings["AdapterServiceUri"];
            if (_adapterServiceUri.EndsWith("/"))
                _adapterServiceUri = _adapterServiceUri.Remove(_adapterServiceUri.Length - 1);

            _dataServiceUri = _settings["DataServiceUri"];
            if (_dataServiceUri.EndsWith("/"))
                _dataServiceUri = _dataServiceUri.Remove(_dataServiceUri.Length - 1);

            _hibernateServiceUri = _settings["HibernateServiceUri"];
            if (_hibernateServiceUri.EndsWith("/"))
                _hibernateServiceUri = _hibernateServiceUri.Remove(_hibernateServiceUri.Length - 1);

            _referenceDataServiceUri = _settings["RefDataServiceUri"];
            if (_referenceDataServiceUri.EndsWith("/"))
                _referenceDataServiceUri = _referenceDataServiceUri.Remove(_referenceDataServiceUri.Length - 1);

            if (!string.IsNullOrEmpty(_settings["BaseDirectoryPath"]) && _settings["BaseDirectoryPath"].Contains("Applications"))
            {
                _servicesBasePath = _settings["BaseDirectoryPath"].Replace("Applications", "Services");
            }
        }

        public HttpSessionStateBase Session { get; set; }

        protected WebHttpClient CreateWebClient(string baseUri)
        {
            WebHttpClient client = null;

            if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
            {
                WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
                client = new WebHttpClient(baseUri, null, webProxy);
            }
            else
            {
                client = new WebHttpClient(baseUri);
            }

            if (AuthHeaders != null && AuthHeaders.Count > 0)
            {
                _logger.Debug("Injecting authorization [" + AuthHeaders.Count + "] headers.");
                client.Headers = AuthHeaders;
            }
            else
            {
                _logger.Debug("No authorization headers.");
            }

            return client;
        }

        protected T WaitForRequestCompletion<T>(string baseUri, string statusUrl)
        {
            T obj;

            try
            {
                long timeoutCount = 0;

                long asyncTimeout = 1800;  // seconds
                if (_settings["AsyncTimeout"] != null)
                {
                    long.TryParse(_settings["AsyncTimeout"], out asyncTimeout);
                }
                asyncTimeout *= 1000;  // convert to milliseconds

                int asyncPollingInterval = 2;  // seconds
                if (_settings["AsyncPollingInterval"] != null)
                {
                    int.TryParse(_settings["AsyncPollingInterval"], out asyncPollingInterval);
                }
                asyncPollingInterval *= 1000;  // convert to milliseconds

                WebHttpClient client = CreateWebClient(baseUri);
                RequestStatus requestStatus = null;

                while (timeoutCount < asyncTimeout)
                {
                    requestStatus = client.Get<RequestStatus>(statusUrl);

                    if (requestStatus.State != State.InProgress)
                        break;

                    Thread.Sleep(asyncPollingInterval);
                    timeoutCount += asyncPollingInterval;
                }

                if (requestStatus.State != State.Completed)
                {
                    throw new Exception(requestStatus.Message);
                }

                if (typeof(T) == typeof(string))
                {
                    obj = (T)Convert.ChangeType(requestStatus.ResponseText, typeof(T));
                }
                else
                {
                    obj = Utility.Deserialize<T>(requestStatus.ResponseText, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw ex;
            }

            return obj;
        }

        public ScopeProjects GetScopes()
        {
            _logger.Debug("In AdapterRepository GetScopes");

            ScopeProjects obj = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Get<ScopeProjects>("/scopes");

                _logger.Debug("Successfully called Adapter.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
                // throw ex;
            }

            return obj;
        }

        public DataLayers GetDataLayers()
        {
            DataLayers obj = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Get<DataLayers>("/datalayers");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public List<Dictionary<string, string>> GetSecurityGroups()
        {
            List<Dictionary<string, string>> dicSecuritygroups = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                PermissionGroups lstgroup = client.Get<PermissionGroups>("/groups");

                dicSecuritygroups = new List<Dictionary<string, string>>();
                Dictionary<string, string> dicobj = null;
                foreach (string group in lstgroup)
                {
                    dicobj = new Dictionary<string, string>();
                    dicobj.Add("permission", group);
                    dicSecuritygroups.Add(dicobj);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return dicSecuritygroups;
        }

        public NameValueList GetGlobalVariables()
        {
            NameValueList nvlobj = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                nvlobj = client.Get<NameValueList>("/settings");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return nvlobj;
        }

        public void SaveFilterFile(DataFilter filter, string fileName)
        {
            string filterPath = String.Format("{0}filter.{1}.xml", _servicesBasePath + _settings["AppDataPath"], fileName);
            Utility.Write<DataFilter>(filter, filterPath, true);
        }

        public void GetFilterFile(ref DataFilter filter, string fileName)
        {
            string filterPath = String.Format("{0}filter.{1}.xml", _servicesBasePath + _settings["AppDataPath"], fileName);
            if (File.Exists(filterPath))
            {
                filter = Utility.Read<DataFilter>(filterPath, true);
            }
        }

        public void DeleteFilterFile(string fileName)
        {
            string filterPath = String.Format("{0}filter.{1}.xml", _servicesBasePath + _settings["AppDataPath"], fileName);
            if (File.Exists(filterPath))
            {
                File.Delete(filterPath);
            }
        }

        public Entity GetClassLabel(string classId)
        {
            Entity entity = new Entity();
            try
            {
                WebHttpClient client = CreateWebClient(_referenceDataServiceUri);
                entity = client.Get<Entity>(String.Format("/classes/{0}/label", classId), true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }
            return entity;
        }

        public ScopeProject GetScope(string scopeName)
        {
            ScopeProject scope = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                scope = client.Get<ScopeProject>("/scopes/" + scopeName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }



            return scope;
        }

        public DataDictionary GetDictionary(string scopeName, string applicationName)
        {
            string dictKey = string.Format("Dictionary.{0}.{1}", scopeName, applicationName);
            DataDictionary dictionary = (DataDictionary)Session[dictKey];

            if (dictionary != null)
                return dictionary;

            try
            {
                WebHttpClient client = CreateWebClient(_dataServiceUri);
                string isAsync = _settings["Async"];

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Get<string>(String.Format("/{0}/{1}/dictionary?format=xml", applicationName, scopeName));

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    dictionary = WaitForRequestCompletion<DataDictionary>(_dataServiceUri, statusUrl);
                }
                else
                {
                    dictionary = client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary?format=xml", applicationName, scopeName), true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw ex;
            }

            // sort data objects and properties
            if (dictionary != null && dictionary.dataObjects != null)
            {
                dictionary.dataObjects.Sort(new DataObjectComparer());

                foreach (DataObject dataObject in dictionary.dataObjects)
                {
                    dataObject.dataProperties.Sort(new DataPropertyComparer());

                    // move key elements to top of the List.
                    List<String> keyPropertyNames = new List<String>();
                    foreach (KeyProperty keyProperty in dataObject.keyProperties)
                    {
                        keyPropertyNames.Add(keyProperty.keyPropertyName);
                    }
                    var value = "";
                    for (int i = 0; i < keyPropertyNames.Count; i++)
                    {
                        value = keyPropertyNames[i];
                        List<DataProperty> DataProperties = dataObject.dataProperties;
                        DataProperty prop = null;

                        for (int j = 0; j < DataProperties.Count; j++)
                        {
                            if (DataProperties[j].propertyName == value)
                            {
                                prop = DataProperties[j];
                                DataProperties.RemoveAt(j);
                                break;
                            }
                        }

                        if (prop != null)
                            DataProperties.Insert(0, prop);
                    }
                }
            }

            Session[dictKey] = dictionary;

            return dictionary;
        }

        public Mapping GetMapping(string scopeName, string applicationName)
        {
            Mapping obj = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public org.iringtools.library.Configuration GetConfig(string scope, string application)
        {
            org.iringtools.library.Configuration config = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                XElement element = client.Get<XElement>(string.Format("/{0}/{1}/config", scope, application), true);
                config = SerializationExtensions.ToObject<org.iringtools.library.Configuration>(element);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw ex;
            }

            return config;
        }

        public XElement GetBinding(string scope, string application)
        {
            XElement binding = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                binding = client.Get<XElement>(String.Format("/{0}/{1}/binding", scope, application), true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw ex;
            }

            return binding;
        }

        public string UpdateBinding(string scope, string application, string dataLayer)
        {
            string obj = null;

            try
            {
                XElement binding = new XElement("module",
                  new XAttribute("name", string.Format("{0}.{1}", scope, application)),
                  new XElement("bind",
                    new XAttribute("name", "DataLayer"),
                    new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
                    new XAttribute("to", dataLayer)
                  )
                );

                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            return obj;
        }

        public DataLayer GetDataLayer(string scopeName, string applicationName)
        {
            DataLayer dataLayer = null;
            XElement binding = GetBinding(scopeName, applicationName);

            if (binding != null)
            {
                _logger.Debug("Received data layer binding: " + binding.ToString());

                XElement bindElt = binding.Element("bind");
                string service = bindElt.Attribute("service").Value;
                string impl = bindElt.Attribute("to").Value;

                dataLayer = new DataLayer();
                dataLayer.Assembly = impl;
                dataLayer.Name = impl.Split(',')[1].Trim();
                dataLayer.IsLightweight = service.Contains(typeof(ILightweightDataLayer).Name);
            }
            else
            {
                throw new Exception("Error getting data layer binding configuration for [" + scopeName + "." + applicationName + "]");
            }

            return dataLayer;
        }

        public string AddScope(string name, string description, string cacheDBConnStr, string permissions)
        {
            string obj = null;

            try
            {
                List<string> groups = new List<string>();
                if (permissions.Contains(","))
                {
                    string[] arrstring = permissions.Split(',');
                    groups = new List<string>(arrstring);
                }
                else
                {
                    groups.Add(permissions);
                }
                ScopeProject scope = new ScopeProject()
                {
                    Name = name,
                    Description = description,
                    Configuration = new org.iringtools.library.Configuration() { AppSettings = new AppSettings() },
                    PermissionGroup = new PermissionGroups()
                };
                if (!string.IsNullOrEmpty(permissions))
                    scope.PermissionGroup.AddRange(groups);

                if (!String.IsNullOrWhiteSpace(cacheDBConnStr))
                {
                    scope.Configuration.AppSettings.Settings = new List<Setting>(){
            new Setting(){
                  Key = "iRINGCacheConnStr",
                  Value = cacheDBConnStr
              }
          };
                }

                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Post<ScopeProject>("/scopes", scope, true);
            }

            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string UpdateScope(string oldName, string displayName, string newDescription, string cacheDBConnStr, string permissions)
        {
            string obj = null;

            try
            {
                List<string> groups = new List<string>();
                if (permissions.Contains(","))
                {
                    string[] arrstring = permissions.Split(',');
                    groups = new List<string>(arrstring);
                }
                else
                {
                    groups.Add(permissions);
                }
                ScopeProject scope = new ScopeProject()
                {
                    Name = oldName,
                    DisplayName = displayName,
                    Description = newDescription,
                    Configuration = new org.iringtools.library.Configuration() { AppSettings = new AppSettings() },
                    PermissionGroup = new PermissionGroups()
                };
                if (!string.IsNullOrEmpty(permissions))
                    scope.PermissionGroup.AddRange(groups);

                if (!String.IsNullOrWhiteSpace(cacheDBConnStr))
                {
                    scope.Configuration.AppSettings.Settings = new List<Setting>(){
            new Setting(){
                  Key = "iRINGCacheConnStr",
                  Value = cacheDBConnStr
              }
          };
                }

                string uri = string.Format("/scopes/{0}", oldName);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Post<ScopeProject>(uri, scope, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string DeleteScope(string name)
        {
            string obj = null;

            try
            {
                string uri = String.Format("/scopes/{0}/delete", name);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Get<string>(uri, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string AddApplication(string scopeName, ScopeApplication application)
        {
            string obj = null;

            try
            {
                string uri = String.Format("/scopes/{0}/apps", scopeName);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Post<ScopeApplication>(uri, application, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string UpdateApplication(string scopeName, string applicationName, ScopeApplication application)
        {
            string obj = null;

            try
            {
                string uri = String.Format("/scopes/{0}/apps/{1}", scopeName, applicationName);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Post<ScopeApplication>(uri, application, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string DeleteApplication(string scopeName, string applicationName)
        {
            string obj = null;

            try
            {
                string uri = String.Format("/scopes/{0}/apps/{1}/delete", scopeName, applicationName);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Get<string>(uri, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            return obj;
        }

        public Response Refresh(string scope, string application)
        {
            Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                client.Timeout = 600000;

                string isAsync = _settings["Async"];

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Get<string>(String.Format("/{0}/{1}/refresh?format=xml", scope, application));

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    response = WaitForRequestCompletion<Response>(_adapterServiceUri, statusUrl);
                }
                else
                {
                    response = client.Get<Response>(String.Format("/{0}/{1}/refresh?format=xml", scope, application), true);
                }

                string dictKey = string.Format("Dictionary.{0}.{1}", scope, application);
                Session.Remove(dictKey);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                //response = new Response()
                //{
                //    Level = StatusLevel.Error,
                //    Messages = new Messages { ex.Message }
                //};
                return PrepareErrorResponse(ex, ErrorMessages.errUIRefresh);
            }

            return response;
        }

        public Response Refresh(string scope, string application, string dataObjectName)
        {
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                client.Timeout = 600000;

                Response response = client.Get<Response>(String.Format("/{0}/{1}/{2}/refresh", scope, application, dataObjectName));
                return response;
            }
            catch (Exception e)
            {
                return PrepareErrorResponse(e, ErrorMessages.errUIRefresh);
            }

        }

        public Response UpdateDataLayer(MemoryStream dataLayerStream)
        {
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                Response response = Utility.Deserialize<Response>(client.PostStream("/datalayers", dataLayerStream), true);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                //Response response = new Response()
                //{
                //    Level = StatusLevel.Error,
                //    Messages = new Messages { ex.Message }
                //};
                return PrepareErrorResponse(ex, ErrorMessages.errUdateDataLayer);
                //return response;
            }
        }

        public Response SwitchDataMode(string scope, string application, string mode)
        {
            Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                client.Timeout = 3600000;

                string isAsync = _settings["Async"];
                string url = string.Format("/{0}/{1}/data/{2}", scope, application, mode);

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Get<string>(url);

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    response = WaitForRequestCompletion<Response>(_adapterServiceUri, statusUrl);
                }
                else
                {
                    response = client.Get<Response>(url, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                //response = new Response()
                //{
                //    Level = StatusLevel.Error,
                //    Messages = new Messages { ex.Message }
                //};
                return PrepareErrorResponse(ex, ErrorMessages.errSwitchDataMode);
            }

            return response;
        }

        public Response RefreshCache(string scope, string app, int timeout)
        {
            Response response = null;

            try
            {

                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                client.Timeout = timeout;

                string isAsync = _settings["Async"];
                string url = string.Format("/{0}/{1}/cache/refresh", scope, app);

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Get<string>(url);

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    response = WaitForRequestCompletion<Response>(_adapterServiceUri, statusUrl);
                }
                else
                {
                    response = client.Get<Response>(url, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                //response = new Response()
                //{
                //    Level = StatusLevel.Error,
                //    Messages = new Messages { ex.Message }
                //};
                return PrepareErrorResponse(ex, ErrorMessages.errRefreshCache);
            }

            return response;
        }

        public Response RefreshCache(string scope, string app, string dataObjectName)
        {
            Response response = null;

            try
            {

                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                client.Timeout = 3600000;

                string isAsync = _settings["Async"];
                string url = string.Format("/{0}/{1}/{2}/cache/refresh", scope, app, dataObjectName);

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Get<string>(url);

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    response = WaitForRequestCompletion<Response>(_adapterServiceUri, statusUrl);
                }
                else
                {
                    response = client.Get<Response>(url, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                response = new Response()
                {
                    Level = StatusLevel.Error,
                    Messages = new Messages { ex.Message }
                };
            }

            return response;
        }

        public Response ImportCache(string scope, string app, string importURI, int timeout)
        {
            Response response = null;

            try
            {

                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                client.Timeout = timeout;

                string isAsync = _settings["Async"];
                string url = string.Format("/{0}/{1}/cache/import?baseUri={2}", scope, app, importURI);

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Get<string>(url);

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    response = WaitForRequestCompletion<Response>(_adapterServiceUri, statusUrl);
                }
                else
                {
                    response = client.Get<Response>(url, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                //response = new Response()
                //{
                //    Level = StatusLevel.Error,
                //    Messages = new Messages { ex.Message }
                //};

                return PrepareErrorResponse(ex, ErrorMessages.errUIImportCache);

            }

            return response;
        }

        public Response DeleteCache(string scope, string application)
        {
            Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                string isAsync = _settings["Async"];
                string url = string.Format("/{0}/{1}/cache/delete", scope, application);

                if (isAsync != null && isAsync.ToLower() == "true")
                {
                    client.Async = true;
                    string statusUrl = client.Get<string>(url);

                    if (string.IsNullOrEmpty(statusUrl))
                    {
                        throw new Exception("Asynchronous status URL not found.");
                    }

                    response = WaitForRequestCompletion<Response>(_adapterServiceUri, statusUrl);
                }
                else
                {
                    response = client.Get<Response>(url, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                response = new Response()
                {
                    Level = StatusLevel.Error,
                    Messages = new Messages { ex.Message }
                };
            }

            return response;
        }

        public CacheInfo GetCacheInfo(string scope, string app)
        {
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                string relativePath = string.Format("/{0}/{1}/cacheinfo", scope, app);
                return client.Get<CacheInfo>(relativePath);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw;

            }
        }

        #region NHibernate Configuration Wizard support methods
        public DataProviders GetDBProviders()
        {
            DataProviders providers = new DataProviders();

            foreach (Provider provider in System.Enum.GetValues(typeof(Provider)))
            {
                providers.Add(provider);
            }

            return providers;
        }

        public DatabaseDictionary GetDBDictionary(string scope, string application)
        {
            try
            {
                WebHttpClient client = CreateWebClient(_hibernateServiceUri);
                DatabaseDictionary dbDictionary = client.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", scope, application));

                string connStr = dbDictionary.ConnectionString;
                if (!String.IsNullOrEmpty(connStr))
                {
                    dbDictionary.ConnectionString = Utility.EncodeTo64(connStr);
                }

                return dbDictionary;
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw;

            }
        }

        public Response SaveDBDictionary(string scope, string app, DatabaseDictionary dictionary)
        {
            Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(_hibernateServiceUri);
                response = client.Post<DatabaseDictionary, Response>("/" + scope + "/" + app + "/dictionary", dictionary);
            }
            catch (Exception ex)
            {
                //response = new Response()
                //{
                //    Level = StatusLevel.Error,
                //    Messages = new Messages() { ex.ToString() }
                //};

                return PrepareErrorResponse(ex, ErrorMessages.errUISaveDBDirectory);

            }

            return response;
        }

        public List<string> GetTableNames(string scope, string app, Dictionary<string, string> conElts)
        {
            var uri = String.Format("/{0}/{1}/tables", scope, app);

            Request request = new Request();
            foreach (var pair in conElts)
            {
                request.Add(pair.Key, pair.Value);
            }

            WebHttpClient client = CreateWebClient(_hibernateServiceUri);
            return client.Post<Request, List<string>>(uri, request, true);
        }

        public List<DataObject> GetDBObjects(string scope, string app,
            Dictionary<string, string> conElts, List<string> tableNames)
        {
            string uri = String.Format("/{0}/{1}/objects", scope, app);

            Request request = new Request();
            foreach (var pair in conElts)
            {
                request.Add(pair.Key, pair.Value);
            }

            request.Add("tableNames", string.Join(",", tableNames));

            WebHttpClient client = CreateWebClient(_hibernateServiceUri);
            List<DataObject> dbObjects = client.Post<Request, List<DataObject>>(uri, request, true);
            return dbObjects;
        }

        // use appropriate icons especially node with children
        public List<JsonTreeNode> GetDBObjects(string scope, string application, string dbProvider, string dbServer,
          string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames, string portNumber, string serName)
        {
            List<JsonTreeNode> dbObjectNodes = new List<JsonTreeNode>();
            //bool hasDBDictionary = false;
            //bool hasDataObjectinDBDictionary = false;
            //DatabaseDictionary dbDictionary = null;
            string uri = String.Format("/{0}/{1}/objects", scope, application);

            Request request = new Request();
            request.Add("dbProvider", dbProvider);
            request.Add("dbServer", dbServer);
            request.Add("portNumber", portNumber);
            request.Add("dbInstance", dbInstance);
            request.Add("dbName", dbName);
            request.Add("dbSchema", dbSchema);
            request.Add("dbUserName", dbUserName);
            request.Add("dbPassword", dbPassword);
            request.Add("tableNames", tableNames);
            request.Add("serName", serName);

            WebHttpClient client = CreateWebClient(_hibernateServiceUri);
            List<DataObject> dataObjects = client.Post<Request, List<DataObject>>(uri, request, true);

            //try
            //{
            //    dbDictionary = GetDBDictionary(scope, application);

            //    if (dbDictionary != null)
            //        if (dbDictionary.dataObjects.Count > 0)
            //            hasDBDictionary = true;
            //}
            //catch (Exception)
            //{
            //    hasDBDictionary = false;
            //}

            foreach (DataObject dataObject in dataObjects)
            {
                //hasDataObjectinDBDictionary = false;

                //if (hasDBDictionary)
                //    if (dbDictionary.dataObjects.FirstOrDefault<DataObject>(o => o.tableName.ToLower() == dataObject.tableName.ToLower()) != null)
                //        hasDataObjectinDBDictionary = true;

                JsonTreeNode keyPropertiesNode = new JsonTreeNode()
                {
                    text = "Keys",
                    type = "keys",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                JsonTreeNode dataPropertiesNode = new JsonTreeNode()
                {
                    text = "Properties",
                    type = "properties",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                JsonTreeNode relationshipsNode = new JsonTreeNode()
                {
                    text = "Relationships",
                    type = "relationships",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                // create data object node
                JsonTreeNode dataObjectNode = new JsonTreeNode()
                {
                    text = dataObject.tableName,
                    type = "dataObject",
                    iconCls = "treeObject",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                    {
                        keyPropertiesNode, dataPropertiesNode, relationshipsNode
                    },
                    property = new Dictionary<string, string>
                    {
                        {"objectNamespace", "org.iringtools.adapter.datalayer.proj_" + scope + "." + application},
                        {"objectName", dataObject.objectName},
                        {"keyDelimiter", dataObject.keyDelimeter}
                    }
                };

                // add key/data property nodes
                foreach (DataProperty dataProperty in dataObject.dataProperties)
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>()
                    {
                        {"columnName", dataProperty.columnName},
                        {"propertyName", dataProperty.propertyName},
                        {"dataType", dataProperty.dataType.ToString()},
                        {"dataLength", dataProperty.dataLength.ToString()},
                        {"nullable", dataProperty.isNullable.ToString()},
                        {"showOnIndex", dataProperty.showOnIndex.ToString()},
                        {"numberOfDecimals", dataProperty.numberOfDecimals.ToString()},
                        {"isHidden", dataProperty.isHidden.ToString()},
                        {"precision", dataProperty.precision.ToString()},
                        {"scale", dataProperty.scale.ToString()}
                    };

                    if (dataObject.isKeyProperty(dataProperty.propertyName))
                    {
                        properties.Add("keyType", dataProperty.keyType.ToString());

                        JsonTreeNode keyPropertyNode = new JsonTreeNode()
                        {
                            text = dataProperty.columnName,
                            type = "keyProperty",
                            iconCls = "treeKey",
                            leaf = true,
                            property = properties
                        };

                        keyPropertiesNode.children.Add(keyPropertyNode);
                    }
                    else
                    {
                        JsonTreeNode dataPropertyNode = new JsonTreeNode()
                        {
                            text = dataProperty.columnName,
                            type = "dataProperty",
                            iconCls = "treeProperty",
                            leaf = true,
                            property = properties
                        };

                        dataPropertiesNode.children.Add(dataPropertyNode);
                    }
                }

                dbObjectNodes.Add(dataObjectNode);
            }

            return dbObjectNodes;
        }

        public Response RegenAll()
        {
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                Response response = client.Get<Response>("/generate");
                return response;
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                return PrepareErrorResponse(e, ErrorMessages.errUIRegenAll);
                throw;
            }
        }

        private Response PrepareErrorResponse(Exception ex, string errMsg)
        {
            _logger.Error(ex.ToString());
            CustomErrorLog objCustomErrorLog = new CustomErrorLog();
            _CustomError = objCustomErrorLog.customErrorLogger(errMsg, ex, _logger);
            Response response = new Response
            {
                Level = StatusLevel.Error,
                Messages = new Messages
          {
            //ex.Message
             "[ " + _CustomError.msgId + "] " +errMsg 
          },
                StatusText = _CustomError.stackTraceDescription,
                StatusCode = HttpStatusCode.InternalServerError,
                StatusList = null
            };
            return response;
        }
        #endregion

        public string AddScope(string name, string description, string cacheDBConnStr, string permissions, string displayName)
        {
            string obj = null;

            try
            {
                List<string> groups = new List<string>();
                if (permissions.Contains(","))
                {
                    string[] arrstring = permissions.Split(',');
                    groups = new List<string>(arrstring);
                }
                else
                {
                    groups.Add(permissions);
                }
                ScopeProject scope = new ScopeProject()
                {
                    Name = name,
                    Description = description,
                    Configuration = new org.iringtools.library.Configuration() { AppSettings = new AppSettings() },
                    PermissionGroup = new PermissionGroups(),
                    DisplayName = displayName //   displayName
                };
                if (!string.IsNullOrEmpty(permissions))
                    scope.PermissionGroup.AddRange(groups);

                if (!String.IsNullOrWhiteSpace(cacheDBConnStr))
                {
                    scope.Configuration.AppSettings.Settings = new List<Setting>(){
            new Setting(){
                  Key = "iRINGCacheConnStr",
                  Value = cacheDBConnStr
              }
          };
                }

                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                obj = client.Post<ScopeProject>("/scopes", scope, true);
            }

            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

    }
}