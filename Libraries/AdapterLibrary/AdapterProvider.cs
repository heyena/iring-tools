﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Xml;
using System.Xml.Linq;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using org.iringtools.adapter.identity;
using org.iringtools.adapter.projection;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.utility;
using StaticDust.Configuration;
using System.Reflection;
using System.ServiceModel.Web;
using net.java.dev.wadl;
using System.Globalization;
using org.iringtools.nhibernate;
using org.iringtools.adapter.datalayer;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace org.iringtools.adapter
{
    public class AdapterProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
        private static readonly int DEFAULT_PAGE_SIZE = 25;

        private ISemanticLayer _semanticEngine = null;
        private IProjectionLayer _projectionEngine = null;
        private mapping.GraphMap _graphMap = null;
        private DataObject _dataObjDef = null;
        private bool _isResourceGraph = false;
        private bool _isProjectionPart7 = false;
        private bool _isFormatExpected = true;

        private List<IDataObject> _dataObjects = new List<IDataObject>();  // dictionary of object names and list of data objects
        private Dictionary<string, List<string>> _classIdentifiers =
          new Dictionary<string, List<string>>();  // dictionary of class ids and list of identifiers

        private string[] arrSpecialcharlist;
        private string[] arrSpecialcharValue;

        [Inject]
        public AdapterProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
                {
                    arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
                    arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
                }

                if (_settings["LdapConfiguration"] != null && _settings["LdapConfiguration"].ToLower() == "true")
                {
                    utility.Utility.isLdapConfigured = true;
                    utility.Utility.InitializeConfigurationRepository(new Type[] { 
            typeof(DataDictionary), 
            typeof(DatabaseDictionary),
            typeof(XElementClone),
            typeof(AuthorizedUsers),
            typeof(Mapping)
          });
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }

        #region application methods
        public ScopeProjects GetScopes()
        {
            if (_enableUISecurity == false)
                return _scopes;
            else
                return _authorizedscopes;
        }

        public VersionInfo GetVersion()
        {
            Version version = this.GetType().Assembly.GetName().Version;

            return new VersionInfo()
            {
                Major = version.Major,
                Minor = version.Minor,
                Build = version.Build,
                Revision = version.Revision
            };
        }

        public Response AddScope(ScopeProject scope)
        {
            Response response = new Response();

            try
            {
                ScopeProject sc = _scopes.Find(x => x.Name.ToLower() == scope.Name.ToLower());

                if (sc == null)
                {
                    if (string.IsNullOrEmpty(scope.DisplayName))
                        scope.DisplayName = scope.Name;

                    if (scope.Configuration.AppSettings.Settings != null)
                    {
                        var connectionSetting = (from setting in scope.Configuration.AppSettings.Settings
                                                 where setting.Key == CACHE_CONNSTR
                                                 select setting).First();

                        if (connectionSetting != null)
                        {
                            string keyFile = string.Format("{0}{1}.key", _settings["AppDataPath"], scope.Name);
                            connectionSetting.Value = EncryptionUtility.Encrypt(connectionSetting.Value, keyFile);
                        }
                    }

                    _scopes.Add(scope);
                    _scopes.Sort(new ScopeComparer());

                    foreach (ScopeProject proj in _scopes)
                    {
                        proj.Applications.Sort(new ApplicationComparer());
                    }

                    //save configration
                    string scopeConfigPath = String.Format("{0}{1}.config", _settings["AppDataPath"], scope.Name);
                    Utility.Write<Configuration>(scope.Configuration, scopeConfigPath, false);

                    Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
                    response.Messages.Add(String.Format("Scope [{0}] updated successfully.", scope.Name));
                }
                else
                {
                    response.Level = StatusLevel.Error;
                    response.Messages.Add(String.Format("Scope [{0}] already exists.", scope.Name));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Error updating scope [{0}]: {1}", scope.Name, ex));

                response.Level = StatusLevel.Error;
                response.Messages.Add(String.Format("Error updating scope [{0}]: {1}", scope.Name, ex));
            }

            return response;
        }

        public Response UpdateScope(string oldScopeName, ScopeProject scope)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            try
            {
                ScopeProject sc = _scopes.Find(x => x.Name.ToLower() == oldScopeName.ToLower());

                if (sc == null)
                {
                    status.Level = StatusLevel.Error;
                    status.Messages.Add(String.Format("Scope [{0}] does not exist.", scope.Name));
                }
                else
                {
                    sc.DisplayName = scope.DisplayName;
                    sc.Description = scope.Description;
                    sc.PermissionGroup = scope.PermissionGroup;

                    _scopes.Sort(new ScopeComparer());

                    if (scope.Configuration.AppSettings.Settings != null)
                    {
                        var connectionSetting = (from setting in scope.Configuration.AppSettings.Settings
                                                 where setting.Key == CACHE_CONNSTR
                                                 select setting).First();

                        if (connectionSetting != null)
                        {
                            string keyFile = string.Format("{0}{1}.key", _settings["AppDataPath"], scope.Name);
                            connectionSetting.Value = EncryptionUtility.Encrypt(connectionSetting.Value, keyFile);
                        }
                    }

                    string scopeConfigPath = string.Format("{0}{1}.config", _settings["AppDataPath"], scope.Name);
                    Utility.Write<Configuration>(scope.Configuration, scopeConfigPath, false);

                    Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
                    status.Messages.Add(String.Format("Scope [{0}] updated successfully.", scope.Name));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Error updating scope [{0}]: {1}", scope.Name, ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(String.Format("Error updating scope [{0}]: {1}", scope.Name, ex));
            }

            return response;
        }

        public Response DeleteScope(string scopeName)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            try
            {
                ScopeProject sc = _scopes.Find(x => x.Name.ToLower() == scopeName.ToLower());

                if (sc == null)
                {
                    status.Level = StatusLevel.Error;
                    status.Messages.Add(String.Format("Scope [{0}] not found.", scopeName));
                }
                else
                {
                    //
                    // delete all applications under scope
                    //
                    if (sc.Applications != null)
                    {
                        for (int i = 0; i < sc.Applications.Count; i++)
                        {
                            ScopeApplication app = sc.Applications[i];
                            DeleteApplicationArtifacts(sc.Name, app.Name);
                            sc.Applications.RemoveAt(i--);
                        }
                    }

                    // remove scope from scope list
                    _scopes.Remove(sc);

                    string scopeConfigPath = string.Format("{0}{1}.config", _settings["AppDataPath"], sc.Name);
                    if (File.Exists(scopeConfigPath))
                    {
                        File.Delete(scopeConfigPath);
                    }

                    Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
                    status.Messages.Add(String.Format("Scope [{0}] deleted successfully.", scopeName));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Error deleting scope [{0}]: {1}", scopeName, ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(String.Format("Error deleting scope [{0}]: {1}", scopeName, ex));
            }

            return response;
        }

        public Response AddApplication(string scopeName, ScopeApplication application)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            try
            {
                ScopeProject scope = _scopes.FirstOrDefault<ScopeProject>(o => o.Name.ToLower() == scopeName.ToLower());

                if (scope == null)
                {
                    throw new Exception(String.Format("Scope [{0}] not found.", scopeName));
                }

                //
                // update binding configurations
                //
                string adapterBindingConfigPath = String.Format("{0}BindingConfiguration.Adapter.xml",
                  _settings["AppDataPath"], scope.Name, application.Name);

                if (File.Exists(adapterBindingConfigPath))
                {
                    XElement adapterBindingConfig = XElement.Load(adapterBindingConfigPath);

                    //
                    // update authorization binding
                    //
                    foreach (XElement bindElement in adapterBindingConfig.Elements("bind"))
                    {
                        if (bindElement.Attribute("name").Value == "IdentityLayer")
                        {
                            XAttribute toAttribute = bindElement.Attribute("to");
                            XElement authorizationBinding = null;

                            if (toAttribute.Value.ToString().Contains(typeof(AnonymousIdentityProvider).FullName))
                            {
                                authorizationBinding = new XElement("module",
                                  new XAttribute("name", "AuthorizationBinding" + "." + scope.Name + "." + application.Name),
                                  new XElement("bind",
                                    new XAttribute("name", "AuthorizationBinding"),
                                    new XAttribute("service", "org.iringtools.nhibernate.IAuthorization, NHibernateLibrary"),
                                    new XAttribute("to", "org.iringtools.nhibernate.ext.EveryoneAuthorization, NHibernateExtension")
                                  )
                                );
                            }
                            else  // default to NHibernate Authorization
                            {
                                authorizationBinding = new XElement("module",
                                  new XAttribute("name", "AuthorizationBinding" + "." + scope.Name + "." + application.Name),
                                  new XElement("bind",
                                    new XAttribute("name", "AuthorizationBinding"),
                                    new XAttribute("service", "org.iringtools.nhibernate.IAuthorization, NHibernateLibrary"),
                                    new XAttribute("to", "org.iringtools.nhibernate.ext.NHibernateAuthorization, NHibernateExtension")
                                  )
                                );

                                // create white-list authorization file
                                string authorizationPath = string.Format("{0}Authorization.{1}.{2}.xml", _settings["AppDataPath"], scope.Name, application.Name);
                                AuthorizedUsers authorizedUsers = new AuthorizedUsers();
                                authorizedUsers.Add(_settings["UserName"]);
                                Utility.Write<AuthorizedUsers>(authorizedUsers, authorizationPath, true);
                            }

                            //authorizationBinding.Save(String.Format("{0}AuthorizationBindingConfiguration.{1}.{2}.xml",
                            //   _settings["AppDataPath"], scope.Name, application.Name));
                            utility.Utility.SavexElementObject(authorizationBinding, String.Format("{0}AuthorizationBindingConfiguration.{1}.{2}.xml",
                                  _settings["AppDataPath"], scope.Name, application.Name));
                        }
                        break;
                    }

                    //
                    // update summary binding
                    //
                    XElement summaryBinding = new XElement("module",
                      new XAttribute("name", "SummaryBinding" + "." + scope.Name + "." + application.Name),
                      new XElement("bind",
                        new XAttribute("name", "SummaryBinding"),
                        new XAttribute("service", "org.iringtools.nhibernate.ISummary, NHibernateLibrary"),
                        new XAttribute("to", "org.iringtools.nhibernate.ext.NHibernateSummary, NHibernateExtension")
                      )
                    );

                    //summaryBinding.Save(String.Format("{0}SummaryBindingConfiguration.{1}.{2}.xml",
                    //   _settings["AppDataPath"], scope.Name, application.Name));
                    utility.Utility.SavexElementObject(summaryBinding, String.Format("{0}SummaryBindingConfiguration.{1}.{2}.xml",
                     _settings["AppDataPath"], scope.Name, application.Name));

                    //
                    // update data layer binding
                    //
                    if (!string.IsNullOrEmpty(application.Assembly))
                    {
                        string service = "org.iringtools.library.IDataLayer, iRINGLibrary";
                        if (typeof(ILightweightDataLayer).IsAssignableFrom(Type.GetType(application.Assembly)))
                        {
                            service = "org.iringtools.library.ILightweightDataLayer, iRINGLibrary";
                        }

                        XElement dataLayerBinding = new XElement("module",
                          new XAttribute("name", "DataLayerBinding" + "." + scope.Name + "." + application.Name),
                          new XElement("bind",
                            new XAttribute("name", "DataLayer"),
                            new XAttribute("service", service),
                            new XAttribute("to", application.Assembly)
                          )
                        );

                        //dataLayerBinding.Save(String.Format("{0}BindingConfiguration.{1}.{2}.xml",
                        //    _settings["AppDataPath"], scope.Name, application.Name));
                        utility.Utility.SavexElementObject(dataLayerBinding, String.Format("{0}BindingConfiguration.{1}.{2}.xml",
                         _settings["AppDataPath"], scope.Name, application.Name));
                    }

                    //
                    // save off configuration
                    //
                    Configuration config = application.Configuration;
                    string configPath = String.Format("{0}{1}.{2}.config", _settings["AppDataPath"], scope.Name, application.Name);

                    //if (!File.Exists(configPath))
                    //{
                    //  File.Create(configPath);
                    //}
                    if (config.AppSettings.Settings.Count > 0)
                        Utility.Write<Configuration>(config, configPath, false);
                }
                else
                {
                    throw new Exception("Adapter binding configuration not found.");
                }

                //
                // now add scope to scopes.xml
                //
                if (scope.Applications == null)
                {
                    scope.Applications = new ScopeApplications();
                }

                ScopeApplication sa = scope.Applications.Find(x => x.Name.ToLower() == application.Name.ToLower());

                if (sa == null)
                {
                    if (string.IsNullOrEmpty(application.DisplayName))
                        application.DisplayName = application.Name;

                    scope.Applications.Add(application);
                    scope.Applications.Sort(new ApplicationComparer());

                    Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);

                    response.Append(Generate(scope.Name, application.Name));
                    status.Messages.Add("Application [{0}.{1}] Added successfully.");
                }
                else
                {
                    status.Level = StatusLevel.Error;
                    status.Messages.Add(String.Format("Application [{0}.{1}] already exists.", application));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, application.Name, ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, application.Name, ex));
            }

            return response;
        }

        public Response UpdateApplication(string scopeName, string oldAppName, ScopeApplication updatedApp)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            try
            {
                // check if this scope exists in the current scope list
                ScopeProject scope = _scopes.FirstOrDefault<ScopeProject>(o => o.Name.ToLower() == scopeName.ToLower());

                if (scope == null)
                {
                    throw new Exception(String.Format("Scope [{0}] not found.", scopeName));
                }

                ScopeApplication application = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name.ToLower() == oldAppName.ToLower());

                if (application != null)
                {
                    application.DisplayName = updatedApp.DisplayName;
                    application.Description = updatedApp.Description;
                    application.DataMode = updatedApp.DataMode;
                    application.PermissionGroup = updatedApp.PermissionGroup;

                    if (application.CacheInfo == null)
                        application.CacheInfo = new CacheInfo();

                    application.CacheInfo.ImportURI = updatedApp.CacheInfo.ImportURI;
                    application.CacheInfo.Timeout = updatedApp.CacheInfo.Timeout;

                    scope.Applications.Sort(new ApplicationComparer());

                    string appConfigPath = string.Format("{0}{1}.{2}.config", _settings["AppDataPath"], scope.Name, application.Name);
                    Utility.Write<Configuration>(updatedApp.Configuration, appConfigPath, false);
                }
                else  // application does not exist, stop processing
                {
                    throw new Exception(String.Format("Application [{0}.{1}] not found.", scopeName, oldAppName));
                }

                Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
                status.Messages.Add("Application [{0}.{1}] updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error updating application [{0}.{1}]: {2}", scopeName, updatedApp.Name, ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error updating application [{0}.{1}]: {2}", scopeName, updatedApp.Name, ex));
            }

            return response;
        }

        public Response DeleteApplication(string scopeName, string appName)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            try
            {
                bool found = false;

                foreach (ScopeProject sc in _scopes)
                {
                    if (sc.Name.ToLower() == scopeName.ToLower() && sc.Applications != null)
                    {
                        for (int i = 0; i < sc.Applications.Count; i++)
                        {
                            ScopeApplication app = sc.Applications[i];

                            if (app.Name.ToLower() == appName.ToLower())
                            {
                                DeleteApplicationArtifacts(sc.Name, app.Name);
                                sc.Applications.RemoveAt(i--);
                                found = true;
                                break;
                            }
                        }

                        break;
                    }
                }

                if (!found)
                {
                    status.Level = StatusLevel.Error;
                    status.Messages.Add(String.Format("Application [{0}.{1}] not found.", scopeName, appName));
                }
                else
                {
                    Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
                    status.Messages.Add(String.Format("Application [{0}.{1}] deleted successfully.", scopeName, appName));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Error deleting application [{0}.{1}]: {2}", scopeName, appName, ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(String.Format("Error deleting application [{0}.{1}]: {2}", scopeName, appName, ex));
            }

            return response;
        }

        //private static bool IsApplicationDataChanged(ScopeApplication updatedApp, ScopeApplication oldApp)
        //{
        //  bool Ischanged = false;
        //  try
        //  {
        //    if (oldApp.Name != updatedApp.Name || oldApp.Description != updatedApp.Description || oldApp.Assembly != updatedApp.Assembly)
        //    {
        //      Ischanged = true;
        //    }
        //    else if (oldApp.Configuration.AppSettings.Settings.Count != updatedApp.Configuration.AppSettings.Settings.Count)
        //    {
        //      Ischanged = true;
        //    }
        //    else
        //    {
        //      for (int i = 0; i < updatedApp.Configuration.AppSettings.Settings.Count; i++)
        //      {
        //        if (updatedApp.Configuration.AppSettings.Settings[i].Value != oldApp.Configuration.AppSettings.Settings[i].Value)
        //        {
        //          Ischanged = true;
        //          break;
        //        }
        //      }
        //    }
        //  }
        //  catch
        //  {
        //    Ischanged = true;
        //  }
        //  return Ischanged;
        //}

        // delete all application artifacts except for its mapping

        private void DeleteApplicationArtifacts(string scopeName, string appName)
        {
            string path = _settings["AppDataPath"];
            string context = scopeName + "." + appName;

            string authorizationPath = String.Format("{0}Authorization.{1}.xml", path, context);
            if (File.Exists(authorizationPath))
            {
                File.Delete(authorizationPath);
            }
            utility.Utility.DeleteFileFromRepository<AuthorizedUsers>(authorizationPath);

            string authorizationBindingPath = String.Format("{0}AuthorizationBindingConfiguration.{1}.xml", path, context);
            if (File.Exists(authorizationBindingPath))
            {
                File.Delete(authorizationBindingPath);
            }
            utility.Utility.DeleteFileFromRepository<XElementClone>(authorizationBindingPath);

            string bindingConfigurationPath = String.Format("{0}BindingConfiguration.{1}.xml", path, context);
            if (File.Exists(bindingConfigurationPath))
            {
                File.Delete(bindingConfigurationPath);
            }
            utility.Utility.DeleteFileFromRepository<XElementClone>(bindingConfigurationPath);

            string databaseDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", path, context);
            if (File.Exists(databaseDictionaryPath))
            {
                File.Delete(databaseDictionaryPath);
            }
            utility.Utility.DeleteFileFromRepository<DatabaseDictionary>(databaseDictionaryPath);

            string dataDictionaryPath = String.Format("{0}DataDictionary.{1}.xml", path, context);
            if (File.Exists(dataDictionaryPath))
            {
                File.Delete(dataDictionaryPath);
            }
            utility.Utility.DeleteFileFromRepository<DataDictionary>(dataDictionaryPath);

            string nhConfigPath = String.Format("{0}nh-configuration.{1}.xml", path, context);
            if (File.Exists(nhConfigPath))
            {
                File.Delete(nhConfigPath);
            }

            string nhMappingPath = String.Format("{0}nh-mapping.{1}.xml", path, context);
            if (File.Exists(nhMappingPath))
            {
                File.Delete(nhMappingPath);
            }

            string summaryBindingConfigurationPath = String.Format("{0}SummaryBindingConfiguration.{1}.xml", path, context);
            if (File.Exists(summaryBindingConfigurationPath))
            {
                File.Delete(summaryBindingConfigurationPath);
            }
            utility.Utility.DeleteFileFromRepository<XElementClone>(summaryBindingConfigurationPath);

            string summaryConfigPath = String.Format("{0}SummaryConfig.{1}.xml", path, context);
            if (File.Exists(summaryConfigPath))
            {
                File.Delete(summaryConfigPath);
            }

            string appCodePath = String.Format("{0}Model.{1}.cs", _settings["AppCodePath"], context);
            if (File.Exists(appCodePath))
            {
                File.Delete(appCodePath);
            }

            string SpreadSheetConfigPath = String.Format("{0}spreadsheet-configuration.{1}.xml", path, context);
            if (File.Exists(SpreadSheetConfigPath))
            {
                File.Delete(SpreadSheetConfigPath);
            }

            string SpreadSheetDataPath = String.Format("{0}SpreadsheetData.{1}.xlsx", path, context);
            if (File.Exists(SpreadSheetDataPath))
            {
                File.Delete(SpreadSheetDataPath);
            }
        }

        public CacheInfo GetCacheInfo(string scope, string app)
        {
            CacheInfo cacheInfo = null;

            try
            {
                InitializeScope(scope, app);

                ScopeApplication application = GetApplication(scope, app);

                if (application == null)
                {
                    throw new Exception("Application [" + scope + "." + app + "] not found.");
                }

                if (application.CacheInfo == null)
                {
                    cacheInfo = new CacheInfo();
                }
                else
                {
                    cacheInfo = application.CacheInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting application cache information: " + ex);
                throw ex;
            }

            return cacheInfo;
        }

        #region Generate methods
        public Response Generate()
        {
            Response response = new Response();
            Status status = new Status()
            {
                Identifier = "Scopes"
            };

            response.StatusList.Add(status);

            try
            {
                foreach (ScopeProject scope in _scopes)
                {
                    response.Append(Generate(scope));
                }

                status.Messages.Add("Artifacts are generated successfully.");
            }
            catch (Exception ex)
            {
                string error = String.Format("Error generating application artifacts, {0}", ex);
                _logger.Error(error);

                status.Level = StatusLevel.Error;
                status.Messages.Add(error);
            }

            return response;
        }

        public Response Generate(string scope)
        {
            foreach (ScopeProject sc in _scopes)
            {
                if (sc.Name.ToLower() == scope.ToLower())
                {
                    return Generate(sc);
                }
            }

            Response response = new Response()
            {
                Level = StatusLevel.Warning,
                Messages = new Messages()
        {
          "Scope [" + scope + "] not found."
        }
            };

            return response;
        }

        private Response Generate(ScopeProject scope)
        {
            Response response = new Response();
            Status status = new Status()
            {
                Identifier = scope.Name
            };

            response.StatusList.Add(status);

            try
            {
                foreach (ScopeApplication app in scope.Applications)
                {
                    response.Append(Generate(scope.Name, app.Name));
                }

                status.Messages.Add("Artifacts are generated successfully.");
            }
            catch (Exception ex)
            {
                string error = String.Format("Error generating application artifacts, {0}", ex);
                _logger.Error(error);

                status.Level = StatusLevel.Error;
                status.Messages.Add(error);
            }

            return response;
        }

        public Response Generate(string scopeName, string appName)
        {
            Response response = new Response();
            Status status = new Status()
            {
                Identifier = scopeName + "." + appName
            };

            response.StatusList.Add(status);

            try
            {
                InitializeScope(scopeName, appName);

                ScopeProject scope = _scopes.FirstOrDefault<ScopeProject>(o => o.Name.ToLower() == scopeName.ToLower());

                if (scope == null)
                {
                    throw new Exception(String.Format("Scope [{0}] not found.", scopeName));
                }

                if (scope.Applications == null)
                {
                    throw new Exception(String.Format("No applications found in scope [{0}].", scopeName));
                }

                ScopeApplication application = scope.Applications.Find(o => o.Name.ToLower() == appName.ToLower());
                if (scope.Applications == null)
                {
                    throw new Exception(String.Format("Application [{0}.{1}] not found.", scopeName, appName));
                }

                string path = _settings["AppDataPath"];
                string context = scope.Name + "." + application.Name;
                string bindingPath = String.Format("{0}BindingConfiguration.{1}.xml", path, context);
                //XElement binding = XElement.Load(bindingPath);
                XElement binding = utility.Utility.GetxElementObject(bindingPath);

                if (binding.Element("bind").Attribute("to").Value.Contains(typeof(NHibernateDataLayer).Name))
                {
                    string dbDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", path, context);
                    DatabaseDictionary dbDictionary = null;

                    if (File.Exists(dbDictionaryPath))
                    {
                        dbDictionary = NHibernateUtility.LoadDatabaseDictionary(dbDictionaryPath, _settings["KeyFile"]);
                    }

                    if (dbDictionary != null && dbDictionary.dataObjects != null)
                    {
                        EntityGenerator generator = _kernel.Get<EntityGenerator>();

                        string compilerVersion = "v4.0";
                        if (!String.IsNullOrEmpty(_settings["CompilerVersion"]))
                        {
                            compilerVersion = _settings["CompilerVersion"];
                        }

                        response.Append(generator.Generate(compilerVersion, dbDictionary, scope.Name, application.Name));
                    }
                    else
                    {
                        status.Level = StatusLevel.Warning;
                        status.Messages.Add(string.Format("Database dictionary [{0}.{1}] does not exist.", scopeName, application.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, appName, ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, appName, ex));
            }

            return response;
        }
        #endregion Generate methods

        public XElement GetConfig(string project, string application)
        {
            Configuration config = null;

            try
            {
                string configPath = (project.ToLower() != "all")
                  ? string.Format("{0}{1}.{2}.config", _settings["AppDataPath"], project, application)
                  : string.Format("{0}All.{1}.config", _settings["AppDataPath"], application);

                if (File.Exists(configPath))
                {
                    config = Utility.Read<Configuration>(configPath, false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error getting application configuration: {0}", ex));
                throw ex;
            }

            return config.ToXElement();
        }

        public XElement GetBinding(string project, string application)
        {
            XElement binding = null;

            try
            {
                InitializeScope(project, application);
                binding = utility.Utility.GetxElementObject(_settings["BindingConfigurationPath"]);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error getting application binding configuration: {0}", ex));
                throw ex;
            }

            return binding;
        }

        public Response UpdateBinding(string project, string application, XElement binding)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            try
            {
                status.Identifier = String.Format("{0}.{1}", project, application);

                InitializeScope(project, application);

                XDocument bindingConfiguration = new XDocument();
                bindingConfiguration.Add(binding);

                bindingConfiguration.Save(_settings["BindingConfigurationPath"]);

                status.Messages.Add("BindingConfiguration of [" + project + "." + application + "] updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in UpdateBindingConfiguration: {0}", ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error updating the binding configuration: {0}", ex));
            }

            return response;
        }
        #endregion

        #region adapter methods
        private void DoGetDictionary(string project, string application, string id)
        {
            try
            {
                DataDictionary dictionary = GetDictionary(project, application);

                _requests[id].ResponseText = Utility.Serialize<DataDictionary>(dictionary, true);
                _requests[id].State = State.Completed;
            }
            catch (Exception ex)
            {
                _requests[id].Message = ex.Message;
                _requests[id].State = State.Error;
            }
        }

        public string AsyncGetDictionary(string project, string application)
        {
            try
            {
                var id = NewQueueRequest();
                Task task = Task.Factory.StartNew(() => DoGetDictionary(project, application, id));
                return "/requests/" + id;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error refreshing data objects: {0}", ex));
                throw ex;
            }
        }

        private void DoRefreshDictionary(string id)
        {
            try
            {
                DataDictionary dictionary = _dataLayerGateway.GetDictionary(true);
                _requests[id].ResponseText = "Dictionary refreshed successfully.";
                _requests[id].State = State.Completed;
            }
            catch (Exception ex)
            {
                if (ex is WebFaultException)
                {
                    _requests[id].Message = Convert.ToString(((WebFaultException)ex).Data["StatusText"]);
                }
                else
                {
                    _requests[id].Message = ex.Message;
                }

                _requests[id].State = State.Error;
            }
        }

        public string AsyncRefreshDictionary(string project, string application)
        {
            try
            {
                InitializeScope(project, application);
                Impersonate();
                InitializeDataLayer(false);

                var id = NewQueueRequest();
                Task task = Task.Factory.StartNew(() => DoRefreshDictionary(id));
                return "/requests/" + id;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error refreshing data objects: {0}", ex));
                throw ex;
            }
        }

        public string AsyncGetWithFilter(string project, string app, string resource,
          string format, int start, int limit, bool fullIndex, DataFilter filter)
        {
            try
            {
                var id = NewQueueRequest();
                Task task = Task.Factory.StartNew(() =>
                  DoGetWithFilter(project, app, resource, format, start, limit, fullIndex, filter, id));
                return "/requests/" + id;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error refreshing data objects: {0}", ex));
                throw ex;
            }
        }

        private string ToJson(XElement xElement)
        {
            try
            {
                DataItems dataItems = Utility.DeserializeDataContract<DataItems>(xElement.ToString());
                DataItemSerializer serializer = new DataItemSerializer(
                  _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
                MemoryStream ms = serializer.SerializeToMemoryStream<DataItems>(dataItems, false);
                byte[] json = ms.ToArray();
                ms.Close();

                return Encoding.UTF8.GetString(json, 0, json.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error converting XElement to JSON: {0}", ex));
                throw ex;
            }
        }

        private void DoGetWithFilter(string project, string app, string resource, string format,
          int start, int limit, bool fullIndex, DataFilter filter, string id)
        {
            try
            {
                XDocument xDocument = GetWithFilter(project, app, resource, filter, ref format, start, limit, fullIndex);

                string responseText = string.Empty;
                State responseState = State.Completed;

                if (xDocument != null && xDocument.Root != null)
                {
                    if (format.ToUpper() == "JSON")
                    {
                        responseText = ToJson(xDocument.Root);
                    }
                    else
                    {
                        responseText = xDocument.Root.ToString();
                    }
                }
                else
                {
                    responseState = State.Error;
                    responseText = "No data objects found.";
                }

                _requests[id].ResponseText = responseText;
                _requests[id].State = responseState;
            }
            catch (Exception ex)
            {
                if (ex is WebFaultException)
                {
                    _requests[id].Message = Convert.ToString(((WebFaultException)ex).Data["StatusText"]);
                }
                else
                {
                    _requests[id].Message = ex.Message;
                }

                _requests[id].State = State.Error;
            }
        }

        public DataDictionary GetDictionary(string project, string application)
        {
            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();

                return _kernel.TryGet<DataDictionary>();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error getting data dictionary: {0}", ex));
                throw new Exception(string.Format("Error getting data dictionary: {0}", ex));
            }
        }

        public Contexts GetContexts(string application)
        {
            try
            {
                Contexts contexts = new Contexts();

                foreach (ScopeProject scope in _scopes)
                {
                    if (scope.Name.ToLower() != "all")
                    {
                        var app = scope.Applications.Find(a => a.Name.ToUpper() == application.ToUpper());

                        if (app != null)
                        {
                            Context context = new Context
                            {
                                Name = scope.Name,
                                Description = scope.Description,
                            };

                            contexts.Add(context);
                        }
                    }
                }

                return contexts;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetContexts for {0}: {1}", application, ex));
                throw ex;
            }
        }

        public WADLApplication GetWADL(string project, string application)
        {
            WADLApplication wadl = new WADLApplication();

            try
            {
                bool isAll = project == "all";
                bool isApp = project == "app";
                if (isApp)
                {
                    //get thie first context and initialize everything
                    Context context = GetContexts(application).FirstOrDefault();

                    if (context == null)
                        throw new WebFaultException(HttpStatusCode.NotFound);

                    project = context.Name;
                }
                InitializeScope(project, application);
                InitializeDataLayer();

                bool isReadOnly = (_settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true");

                // load uri maps config
                Properties _uriMaps = new Properties();

                string uriMapsFilePath = _settings["AppDataPath"] + "UriMaps.conf";

                if (File.Exists(uriMapsFilePath))
                {
                    try
                    {
                        _uriMaps.Load(uriMapsFilePath);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("Error loading [UriMaps.config]: " + e);
                    }
                }

                string baseUri = _settings["GraphBaseUri"];
                if (isAll)
                    baseUri += "all/";

                string appBaseUri = Utility.FormAppBaseURI(_uriMaps, baseUri, application);
                string baseResource = String.Empty;

                if (!isApp && !isAll)
                {
                    appBaseUri = appBaseUri + "/" + project;
                }
                else if (!isAll)
                {
                    baseResource = "/{contextName}";
                }

                WADLResources resources = new WADLResources
                {
                    @base = appBaseUri,
                };

                string title = _application.Name;
                if (title == String.Empty)
                    title = application;

                string appDescription = "This is an iRINGTools endpoint.";
                if (_application.Description != null && _application.Description != String.Empty)
                    appDescription = _application.Description;

                string header = "<div id=\"wadlDescription\">" +
                    "  <p class=\"wadlDescText\">" +
                    "    " + appDescription +
                    "  </p>" +
                    "  <ul class=\"wadlList\">" +
                    "    <li>API access is restricted to Authorized myPSN Users only.</li>" +
                    "    <li>The attributes available for each context may be different.</li>" +
                    "  </ul>" +
                    "</div>";

                XmlDocument dummy = new XmlDocument();
                XmlNode[] headerDocText = new XmlNode[] { dummy.CreateCDataSection(header) };

                WADLHeaderDocumentation doc = new WADLHeaderDocumentation
                {
                    title = title,
                    CData = headerDocText,
                };

                resources.Items.Add(doc);

                if (isApp)
                {
                    #region Build Contexts Resource
                    WADLResource contexts = new WADLResource
                    {
                        path = "/contexts",
                        Items = new List<object>
            {
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = "Gets the list of contexts. A context could be a Bechtel project, GBU, or other name that identifies a set of data."
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "xsd:int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "xsd:string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
                    };

                    resources.Items.Add(contexts);
                    #endregion
                }

                if (_dictionary.enableSummary)
                {
                    #region Build Summary Resource
                    WADLResource summary = new WADLResource
                    {
                        path = baseResource + "/summary",
                        Items = new List<object>
            {
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = "Gets a customizable summary of the data on the endpoint. Only JSON is returned at this time."
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
                    };

                    resources.Items.Add(summary);
                    #endregion
                }

                foreach (DataObject dataObject in _dictionary.dataObjects)
                {
                    if (!dataObject.isRelatedOnly)
                    {
                        #region Build DataObject List Resource
                        WADLResource list = new WADLResource
                        {
                            path = baseResource + "/" + dataObject.objectName.ToLower(),
                            Items = new List<object>
            {
              #region Build GetList Method
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = String.Format(
                      "Gets a list of {0} data. {1} Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                      dataObject.description
                    )
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      },                      
                      new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "xsd:int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "xsd:string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              },
              #endregion
            }
                        };

                        if (!dataObject.isReadOnly && !isReadOnly)
                        {
                            #region Build PutList Method
                            WADLMethod put = new WADLMethod
                            {
                                name = "PUT",
                                Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Updates a list of {0} data in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
                            };
                            #endregion

                            list.Items.Add(put);

                            #region Build PostList Method
                            WADLMethod post = new WADLMethod
                            {
                                name = "POST",
                                Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Creates a single {0} item in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
                            };
                            #endregion

                            list.Items.Add(post);
                        }

                        resources.Items.Add(list);
                        #endregion

                        if (_dictionary.enableSearch)
                        {
                            #region Build DataObject Search Resource
                            WADLResource search = new WADLResource
                            {
                                path = baseResource + "/" + dataObject.objectName.ToLower() + "/search?q={query}",
                                Items = new List<object>
            {
              #region Build GetList Method
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = String.Format(
                      "Searches the  {0} data for the specified context.  The specific properties searched, and whether content is searched, will depend on the context configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                    )
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "q",
                        type = "string",
                        style = "query",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "Enter full or partial text to search for (minimum 2 characters). The specific properties searched, and whether content is searched, will depend on the repository configuration."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              },
              #endregion
            }
                            };

                            resources.Items.Add(search);
                            #endregion
                        }

                        if (!dataObject.isListOnly)
                        {
                            #region Build DataObject Item Resource
                            WADLResource item = new WADLResource
                            {
                                path = baseResource + "/" + dataObject.objectName.ToLower() + "/{identifier}",
                                Items = new List<object>
              {
                #region Build GetItem Method
                new WADLMethod
                {
                  name = "GET",
                  Items = new List<object>
                  {
                    new WADLDocumentation
                    {
                      Value = String.Format(
                        "Gets a list containing the specified {0} data. {1}. Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                         CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                        "This is a dynamic data object"
                      )
                    },
                    new WADLRequest
                    {
                      Items = new List<object>
                      {
                        new WADLParameter
                        {
                          name = "contextName",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch.", 
                                 CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "start",
                          type = "integer",
                          style = "query",
                          required = false,
                          @default = "0",
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "limit",
                          type = "xsd:int",
                          style = "query",
                          required = false,
                          @default = "25",
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "format",
                          type = "xsd:string",
                          style = "query",
                          required = false,
                          @default = "json",
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                            },
                            new WADLOption
                            {
                              value = "xml",
                              mediaType = "application/xml",
                            },
                            new WADLOption
                            {
                              value = "json",
                              mediaType = "application/json",
                            },
                            new WADLOption
                            {
                              value = "html",
                              mediaType = "application/html",
                            }
                          }
                        }
                      }
                    }
                  }
                },
                #endregion
              }
                            };

                            if (!dataObject.isReadOnly && !isReadOnly)
                            {
                                #region Build PutItem Method
                                WADLMethod put = new WADLMethod
                                {
                                    name = "PUT",
                                    Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Updates the specified {0} in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch.", 
                                 CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
                                };
                                #endregion

                                item.Items.Add(put);

                                #region Build DeleteItem Method
                                WADLMethod delete = new WADLMethod
                                {
                                    name = "DELETE",
                                    Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Deletes the specified {0} item in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch.", 
                                 CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
                                };
                                #endregion

                                item.Items.Add(delete);
                            }

                            resources.Items.Add(item);
                            #endregion
                        }

                        foreach (DataRelationship relationship in dataObject.dataRelationships)
                        {
                            #region Build DataObject List Resource
                            WADLResource relatedList = new WADLResource
                            {
                                path = baseResource + "/" + dataObject.objectName.ToLower() + "/{identifier}/" + relationship.relationshipName.ToLower(),
                                Items = new List<object>
            {
              #region Build GetList Method
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = String.Format(
                      "Gets a list containing the {0} data related to the specified {1}. Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(relationship.relationshipName.ToLower()),
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())  
                    )
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      },                      
                      new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch related items.", 
                                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                        new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "xsd:int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "xsd:string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              },
              #endregion
            }
                            };

                            resources.Items.Add(relatedList);
                            #endregion

                            if (relationship.relationshipType == RelationshipType.OneToMany)
                            {
                                #region Build DataObject Item Resource
                                WADLResource relatedItem = new WADLResource
                                {
                                    path = baseResource + "/" + dataObject.objectName.ToLower() + "/{identifier}/" + relationship.relationshipName.ToLower() + "/{relatedIdentifier}",
                                    Items = new List<object>
                {
                  #region Build GetItem Method
                  new WADLMethod
                  {
                    name = "GET",
                    Items = new List<object>
                    {
                      new WADLDocumentation
                      {
                        Value = String.Format(
                          "Gets a list containing the specified {0} data related to the specified {1}. Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                          CultureInfo.CurrentCulture.TextInfo.ToTitleCase(relationship.relationshipName.ToLower()),
                          CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                        )
                      },
                      new WADLRequest
                      {
                        Items = new List<object>
                        {
                          new WADLParameter
                          {
                            name = "contextName",
                            type = "string",
                            style = "template",
                            required = true,
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "identifier",
                            type = "string",
                            style = "template",
                            required = true,
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = String.Format(
                                  "The identifier of the {0} that you would like to fetch related items.",  
                                   CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                                )
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "relatedIdentifier",
                            type = "string",
                            style = "template",
                            required = true,
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = String.Format(
                                  "The identifier of the {0} that you would like to fetch.", 
                                   CultureInfo.CurrentCulture.TextInfo.ToTitleCase(relationship.relationshipName.ToLower())
                                )
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "start",
                            type = "integer",
                            style = "query",
                            required = false,
                            @default = "0",
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "limit",
                            type = "xsd:int",
                            style = "query",
                            required = false,
                            @default = "25",
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "format",
                            type = "xsd:string",
                            style = "query",
                            required = false,
                            @default = "json",
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                              },
                              new WADLOption
                              {
                                value = "xml",
                                mediaType = "application/xml",
                              },
                              new WADLOption
                              {
                                value = "json",
                                mediaType = "application/json",
                              },
                              new WADLOption
                              {
                                value = "html",
                                mediaType = "application/html",
                              }
                            }
                          }
                        }
                      }
                    }
                  },
                  #endregion
                }
                                };

                                resources.Items.Add(relatedItem);
                                #endregion
                            }
                        }
                    }
                }

                wadl.Items.Add(resources);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetDictionary: {0}", ex));
                throw new Exception(string.Format("Error getting data dictionary: {0}", ex));
            }

            return wadl;
        }

        public mapping.Mapping GetMapping(string project, string application)
        {
            try
            {
                InitializeScope(project, application);

                return _mapping;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetMapping: {0}", ex));
                throw ex;
            }
        }

        public Response UpdateMapping(string project, string application, XElement mappingXml)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings["AppDataPath"], project, application);

            try
            {
                status.Identifier = String.Format("{0}.{1}", project, application);

                mapping.Mapping mapping = LoadMapping(path, mappingXml, ref status);

                Utility.Write<mapping.Mapping>(mapping, path, true);

                status.Messages.Add("Mapping of [" + project + "." + application + "] updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in UpdateMapping: {0}", ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error saving mapping file to path [{0}]: {1}", path, ex));
            }

            return response;
        }

        public XDocument GetWithFilter(
          string project, string application, string resource,
            DataFilter filter, ref string format, int start, int limit, bool fullIndex)
        {
            try
            {
                DataDictionary dictionary = GetDictionary(project, application);
                _dataObjDef = dictionary.GetDataObject(resource);

                AddURIsInSettingCollection(project, application, resource);

                if (_dataObjDef != null)
                    filter.AppendFilter(_dataObjDef.dataFilter);

                _logger.DebugFormat("Initializing Scope: {0}.{1}", project, application);
                InitializeScope(project, application);
                _logger.Debug("Initializing DataLayer.");
                InitializeDataLayer();
                _logger.DebugFormat("Initializing Projection: {0} as {1}", resource, format);
                InitializeProjection(resource, ref format, false);

                _projectionEngine.Start = start;
                _projectionEngine.Limit = limit;

                IList<string> index = new List<string>();

                if (limit == 0)
                {
                    limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
                }

                _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
                _dataObjects = _dataLayerGateway.Get(_dataObjDef, filter, start, limit);

                _projectionEngine.Count = _dataLayerGateway.GetCount(_dataObjDef, filter);
                _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);

                _projectionEngine.FullIndex = fullIndex;

                if (_isProjectionPart7)
                {
                    return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects);
                }
                else
                {
                    return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetProjection: {0}", ex));
                throw ex;
            }
        }

        private bool IsNumeric(DataProperty dataProperty)
        {
            return (dataProperty.dataType == DataType.Byte ||
                dataProperty.dataType == DataType.Decimal ||
                dataProperty.dataType == DataType.Double ||
                dataProperty.dataType == DataType.Int16 ||
                dataProperty.dataType == DataType.Int32 ||
                dataProperty.dataType == DataType.Int64 ||
                dataProperty.dataType == DataType.Single);
        }

        public XDocument Search(
          string project, string application, string resource,
          ref string format, string query, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
          NameValueCollection parameters)
        {
            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                _dataObjDef = _dictionary.GetDataObject(resource);

                AddURIsInSettingCollection(project, application, resource);

                if (!_dictionary.enableSearch)
                    throw new WebFaultException(HttpStatusCode.NotFound);

                InitializeProjection(resource, ref format, false);

                IList<string> index = new List<string>();

                if (limit == 0)
                {
                    limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
                }

                _projectionEngine.Start = start;
                _projectionEngine.Limit = limit;

                DataFilter filter = new DataFilter();
                if (parameters != null)
                {
                    foreach (string key in parameters.AllKeys)
                    {
                        string[] expectedParameters = { 
                          "project",
                          "app",
                          "format", 
                          "start", 
                          "limit", 
                          "sortBy", 
                          "sortOrder",
                          "indexStyle",
                          "_dc",
                          "page",
                          "callback",
                          "q",
                        };

                        if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
                        {
                            string value = parameters[key];

                            Expression expression = new Expression
                            {
                                PropertyName = key,
                                RelationalOperator = RelationalOperator.EqualTo,
                                Values = new Values { value },
                                IsCaseSensitive = false,
                            };

                            if (filter.Expressions.Count > 0)
                            {
                                expression.LogicalOperator = LogicalOperator.And;
                            }

                            filter.Expressions.Add(expression);
                        }
                    }

                    if (!String.IsNullOrEmpty(sortBy))
                    {
                        OrderExpression orderBy = new OrderExpression
                        {
                            PropertyName = sortBy,
                        };

                        if (String.Compare(SortOrder.Desc.ToString(), sortOrder, true) == 0)
                        {
                            orderBy.SortOrder = SortOrder.Desc;
                        }
                        else
                        {
                            orderBy.SortOrder = SortOrder.Asc;
                        }

                        filter.OrderExpressions.Add(orderBy);
                    }

                    _dataObjects = _dataLayerGateway.Search(_dataObjDef.objectName, query, filter, start, limit);
                    _projectionEngine.Count = _dataLayerGateway.SearchCount(_dataObjDef.objectName, query, filter);
                }
                else
                {
                    _dataObjects = _dataLayerGateway.Search(_dataObjDef.objectName, query, null, start, limit);
                    _projectionEngine.Count = _dataLayerGateway.SearchCount(_dataObjDef.objectName, query, null);
                }

                _projectionEngine.FullIndex = fullIndex;

                if (_isProjectionPart7)
                {
                    return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects);
                }
                else
                {
                    return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error while searching: {0}", ex));
                throw ex;
            }
        }

        public XDocument GetList(
          string project, string application, string resource,
          ref string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
          NameValueCollection parameters)
        {
            try
            {
                _logger.DebugFormat("Initializing Scope: {0}.{1}", project, application);
                InitializeScope(project, application);
                _logger.Debug("Initializing DataLayer.");
                InitializeDataLayer();
                _logger.DebugFormat("Initializing Projection: {0} as {1}", resource, format);
                InitializeProjection(resource, ref format, false);

                AddURIsInSettingCollection(project, application, resource);

                IList<string> index = new List<string>();

                if (limit == 0)
                {
                    limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
                }

                _projectionEngine.Start = start;
                _projectionEngine.Limit = limit;

                DataFilter dataFilter = new DataFilter();

                if (parameters != null)
                {
                    string filter = parameters["filter"];

                    if (filter != null)
                    {
                        dataFilter = Utility.DeserializeJson<DataFilter>(filter, true);
                    }
                    else
                    {
                        _logger.Debug("Preparing Filter from parameters.");

                        foreach (string key in parameters.AllKeys)
                        {
                            string[] expectedParameters = { 
                          "project",
                          "app",
                          "format", 
                          "start", 
                          "limit", 
                          "sortBy", 
                          "sortOrder",
                          "indexStyle",
                          "_dc",
                          "page",
                          "callback",
                        };

                            if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
                            {
                                string value = parameters[key];

                                Expression expression = new Expression
                                {
                                    PropertyName = key,
                                    RelationalOperator = RelationalOperator.EqualTo,
                                    Values = new Values { value },
                                    IsCaseSensitive = false,
                                };

                                if (dataFilter.Expressions.Count > 0)
                                {
                                    expression.LogicalOperator = LogicalOperator.And;
                                }

                                dataFilter.Expressions.Add(expression);
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(sortBy))
                    {
                        OrderExpression orderBy = new OrderExpression
                        {
                            PropertyName = sortBy,
                        };

                        if (String.Compare(SortOrder.Desc.ToString(), sortOrder, true) == 0)
                        {
                            orderBy.SortOrder = SortOrder.Desc;
                        }
                        else
                        {
                            orderBy.SortOrder = SortOrder.Asc;
                        }

                        dataFilter.OrderExpressions.Add(orderBy);
                    }
                }

                _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
                _dataObjects = _dataLayerGateway.Get(_dataObjDef, dataFilter, start, limit);

                _projectionEngine.Count = _dataLayerGateway.GetCount(_dataObjDef, dataFilter);
                _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);

                _projectionEngine.FullIndex = fullIndex;
                _projectionEngine.BaseURI = (project.ToLower() == "all")
                    ? String.Format("/{0}/{1}", application, resource)
                    : String.Format("/{0}/{1}/{2}", application, project, resource);

                if (_isProjectionPart7)
                {
                    return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects);
                }
                else
                {
                    return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetProjection: {0}", ex));
                throw ex;
            }
        }

        private void AddURIsInSettingCollection(string project, string application, string resource, string resourceIdentifier = null, string relatedresource = null, string relatedId = null)
        {
            try
            {
                _logger.Debug("Adding URI in setting Collection.");

                if (_dataObjDef.keyProperties.Count > 0)
                {
                    string keyPropertyName = _dataObjDef.keyProperties[0].keyPropertyName;

                    string genericURI = "/" + resource;
                    string specificURI = "/" + resource;

                    if (resourceIdentifier != null)
                    {
                        genericURI = resource + "/{" + keyPropertyName + "}";
                        specificURI = resource + "/" + resourceIdentifier;
                    }

                    if (relatedresource != null)
                    {
                        genericURI = resource + "/{" + keyPropertyName + "}/" + relatedresource;
                        specificURI = resource + "/" + resourceIdentifier + "/" + relatedresource;
                    }
                    if (relatedId != null)
                    {
                        DataObject releteddataObject = _dictionary.dataObjects.Find(x => x.objectName.ToUpper() == relatedresource.ToUpper());

                        if (releteddataObject != null)
                        {
                            string reletedKeyPropertyName = releteddataObject.keyProperties[0].keyPropertyName;

                            genericURI = resource + "/{" + keyPropertyName + "}/" + reletedKeyPropertyName + "/{" + reletedKeyPropertyName + "}";
                            specificURI = resource + "/" + resourceIdentifier + "/" + reletedKeyPropertyName + "/" + relatedId;
                        }
                    }

                    _settings["GenericURI"] = genericURI;
                    _settings["SpecificURI"] = specificURI;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Exception in Adding URI in setting Collection: {0}", ex));
                throw ex;
            }
        }

        public object GetItem(
          string project, string application, string resource, string className,
           string classIdentifier, ref string format, bool fullIndex)
        {
            string dataObjectName = string.Empty;

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, true);

                AddURIsInSettingCollection(project, application, resource, classIdentifier);

                if (_isFormatExpected)
                {
                    if (_isResourceGraph)
                    {
                        _dataObjects = GetDataObject(className, classIdentifier);
                    }
                    else
                    {
                        classIdentifier = Utility.ConvertSpecialCharInbound(classIdentifier, arrSpecialcharlist, arrSpecialcharValue);    //Handling special Characters here.
                        List<string> identifiers = new List<string> { classIdentifier };
                        _dataObjects = _dataLayerGateway.Get(_dataObjDef, identifiers);
                    }

                    _projectionEngine.Count = _dataObjects.Count;

                    _projectionEngine.BaseURI = (project.ToLower() == "all")
                      ? String.Format("/{0}/{1}", application, resource)
                      : String.Format("/{0}/{1}/{2}", application, project, resource);

                    if (_dataObjects != null)
                    {
                        if (_isProjectionPart7)
                        {
                            return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects, className, classIdentifier);
                        }
                        else
                        {
                            return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
                        }
                    }
                    else
                    {
                        _logger.Warn("Data object with identifier [" + classIdentifier + "] not found.");
                        throw new WebFaultException(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    Dictionary<string, string> idFormats = new Dictionary<string, string> {
                    {classIdentifier, format}
          };

                    IContentObject contentObject = _dataLayerGateway.GetContents(_dataObjDef, idFormats).FirstOrDefault();
                    return contentObject;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetProjection: {0}", ex));
                throw ex;
            }
        }

        public IList<PicklistObject> GetPicklists(string project, string application, string format)
        {
            IList<PicklistObject> picklists;

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();

                picklists = _dictionary.picklists;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetPicklist: {0}", ex));
                throw ex;
            }

            return picklists;
        }

        public Picklists GetPicklist(string project, string application, string picklistName,
              string format, int start, int limit)
        {
            string dataObjectName = String.Empty;
            Picklists picklist = new Picklists();

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();

                picklist = _dataLayerGateway.GetPicklist(picklistName, start, limit);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetPicklist: {0}", ex));
                throw ex;
            }

            return picklist;
        }

        public XDocument GetRelatedList(
                string project, string application, string resource, string id, string relatedResource,
                ref string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex, NameValueCollection parameters)
        {
            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_projectionEngine == null)
                {
                    throw new Exception("Initializing projection failed. This is most likely due to invalid format/content-type.");
                }

                AddURIsInSettingCollection(project, application, resource, id, relatedResource);

                id = Utility.ConvertSpecialCharOutbound(id, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
                IDataObject parentDataObject = _dataLayerGateway.Get(_dataObjDef, new List<string> { id }).FirstOrDefault<IDataObject>();
                if (parentDataObject == null) return new XDocument();

                DataObject objectType = _dictionary.dataObjects.Find(c => c.objectName.ToLower() == resource.ToLower());

                if (objectType == null)
                {
                    throw new Exception("Primary resource [" + resource + "] not found.");
                }

                DataRelationship relationship = objectType.dataRelationships.Find(c => c.relationshipName.ToLower() == relatedResource.ToLower());

                //
                // if relationship is null, check if provided related resource by name
                //
                if (relationship == null)
                {
                    relationship = objectType.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedResource.ToLower());
                }

                DataObject relatedType = _dictionary.dataObjects.Find(c => c.objectName == relationship.relatedObjectName);

                if (relatedType == null)
                {
                    throw new Exception("Relationship between parent and child resource not found.");
                }

                if (limit == 0)
                {
                    limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
                }

                _projectionEngine.Start = start;
                _projectionEngine.Limit = limit;
                _projectionEngine.FullIndex = fullIndex;

                _projectionEngine.BaseURI = (project.ToLower() == "all")
                    ? String.Format("/{0}/{1}/{2}/{3}", application, resource, id, relatedResource)
                    : String.Format("/{0}/{1}/{2}/{3}/{4}", application, project, resource, id, relatedResource);

                DataFilter filter = CreateDataFilter(parameters, sortOrder, sortBy);

                if (relatedType.isRelatedOnly)
                {
                    // NOTE: this path strictly supports legacy data layers with related only support.
                    _dataObjects = _dataLayerGateway.GetRelatedObjects(parentDataObject, relatedType, filter, limit, start);
                    _projectionEngine.Count = _dataLayerGateway.GetRelatedCount(parentDataObject, relatedType, filter);
                }
                else
                {
                    foreach (PropertyMap propMap in relationship.propertyMaps)
                    {
                        filter.Expressions.Add(new Expression()
                        {
                            PropertyName = propMap.relatedPropertyName,
                            RelationalOperator = RelationalOperator.EqualTo,
                            LogicalOperator = LogicalOperator.And,
                            Values = new Values() { Convert.ToString(parentDataObject.GetPropertyValue(propMap.dataPropertyName)) }
                        });
                    }

                    try
                    {
                        _dataObjects = _dataLayerGateway.Get(relatedType, filter, start, limit);
                    }
                    catch (NotImplementedException nie)
                    {
                        throw nie;
                    }

                    _projectionEngine.Count = _dataObjects.Count;
                }

                XDocument xdoc = _projectionEngine.ToXml(relatedType.objectName, ref _dataObjects);
                return xdoc;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetProjection: {0}", ex));
                throw ex;
            }
        }

        public XDocument GetRelatedItem(string project, string application, string resource, string id,
          string relatedResource, string relatedId, ref string format)
        {
            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_projectionEngine == null)
                {
                    throw new Exception("Initializing projection failed. This is most likely due to invalid format/content-type.");
                }

                AddURIsInSettingCollection(project, application, resource, id, relatedResource, relatedId);

                id = Utility.ConvertSpecialCharOutbound(id, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
                IDataObject parentDataObject = _dataLayerGateway.Get(_dataObjDef, new List<string> { id }).FirstOrDefault<IDataObject>();
                if (parentDataObject == null) return new XDocument();

                _projectionEngine.BaseURI = (project.ToLower() == "all")
                    ? String.Format("/{0}/{1}/{2}/{3}", application, resource, id, relatedResource)
                    : String.Format("/{0}/{1}/{2}/{3}/{4}", application, project, resource, id, relatedResource);

                DataObject objectType = _dictionary.dataObjects.Find(c => c.objectName.ToLower() == resource.ToLower());

                if (objectType == null)
                {
                    throw new Exception("Primary resource [" + resource + "] not found.");
                }

                DataRelationship relationship = objectType.dataRelationships.Find(c => c.relationshipName.ToLower() == relatedResource.ToLower());

                //
                // if relationship is null, check if provided related resource by name
                //
                if (relationship == null)
                {
                    relationship = objectType.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedResource.ToLower());
                }

                DataObject relatedType = _dictionary.dataObjects.Find(c => c.objectName == relationship.relatedObjectName);

                if (relatedType == null)
                {
                    throw new Exception("Relationship between parent and child resource not found.");
                }
                _dataObjects = _dataLayerGateway.Get(relatedType, new List<string> { relatedId });

                if (_dataObjects != null)
                {
                    _projectionEngine.Count = _dataObjects.Count;
                }

                XDocument xdoc = _projectionEngine.ToXml(relatedType.objectName, ref _dataObjects);
                return xdoc;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetProjection: {0}", ex));
                throw ex;
            }
        }

        private List<IDataObject> GetDataObject(string className, string classIdentifier)
        {
            DataFilter filter = new DataFilter();
            List<string> identifiers = new List<string> { classIdentifier };

            string fixedIdentifierBoundary = (_settings["fixedIdentifierBoundary"] == null)
              ? "#" : _settings["fixedIdentifierBoundary"];

            #region parse identifier to build data filter
            ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMapByName(className);

            if (classTemplateMap != null && classTemplateMap.classMap != null)
            {
                mapping.ClassMap classMap = classTemplateMap.classMap;

                string[] identifierValues = !String.IsNullOrEmpty(classMap.identifierDelimiter)
                  ? classIdentifier.Split(new string[] { classMap.identifierDelimiter }, StringSplitOptions.None)
                  : new string[] { classIdentifier };

                for (int i = 0; i < classMap.identifiers.Count; i++)
                {
                    if (!(classMap.identifiers[i].StartsWith(fixedIdentifierBoundary) && classMap.identifiers[i].EndsWith(fixedIdentifierBoundary)))
                    {
                        string clsIdentifier = classMap.identifiers[i];
                        string identifierValue = identifierValues[i];

                        if (clsIdentifier.Split('.').Length > 2)  // related property
                        {
                            string[] clsIdentifierParts = clsIdentifier.Split('.');
                            string relatedObjectName = clsIdentifierParts[clsIdentifierParts.Length - 2];
                            DataObject relatedObjectType = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == relatedObjectName.ToLower());

                            // get related object then assign its related properties to top level data object properties
                            DataFilter relatedObjectFilter = new DataFilter();

                            Expression relatedExpression = new Expression
                            {
                                PropertyName = clsIdentifierParts.Last(),
                                Values = new Values { identifierValue }
                            };

                            relatedObjectFilter.Expressions.Add(relatedExpression);
                            IList<IDataObject> relatedObjects = _dataLayerGateway.Get(relatedObjectType, relatedObjectFilter, 0, 0);

                            if (relatedObjects != null && relatedObjects.Count > 0)
                            {
                                IDataObject relatedObject = relatedObjects.First();
                                DataRelationship dataRelationship = _dataObjDef.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedObjectType.objectName.ToLower());

                                foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
                                {
                                    Expression expression = new Expression();

                                    if (filter.Expressions.Count > 0)
                                        expression.LogicalOperator = LogicalOperator.And;

                                    expression.PropertyName = propertyMap.dataPropertyName;
                                    expression.Values = new Values { 
                    relatedObject.GetPropertyValue(propertyMap.relatedPropertyName).ToString() 
                  };
                                    filter.Expressions.Add(expression);
                                }
                            }
                        }
                        else  // direct property
                        {
                            Expression expression = new Expression();

                            if (filter.Expressions.Count > 0)
                                expression.LogicalOperator = LogicalOperator.And;

                            expression.PropertyName = clsIdentifier.Substring(clsIdentifier.LastIndexOf('.') + 1);
                            expression.Values = new Values { identifierValue };
                            filter.Expressions.Add(expression);
                        }
                    }
                }

                identifiers = _dataLayerGateway.GetIdentifiers(_dataObjDef, filter);

                if (identifiers == null || identifiers.Count == 0)
                {
                    throw new Exception("Identifier [" + classIdentifier + "] of class [" + className + "] is not found.");
                }
            }
            #endregion

            List<IDataObject> dataObjects = _dataLayerGateway.Get(_dataObjDef, identifiers);
            if (dataObjects != null && dataObjects.Count > 0)
            {
                return dataObjects;
            }

            return null;
        }


        public Response Delete(string project, string application, string resource)
        {
            Response response = new Response();
            Status status = new Status();

            response.StatusList.Add(status);

            try
            {
                status.Identifier = String.Format("{0}.{1}.{2}", project, application, resource);

                InitializeScope(project, application);

                _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

                response.Append(_semanticEngine.Delete(resource));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error deleting {0} graphs: {1}", resource, ex));

                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
            }

            return response;
        }

        public Response Create(string project, string application, string resource, string format, XDocument xml)
        {
            return Post(PostAction.Create, project, application, resource, format, xml);
        }

        public Response Update(string project, string application, string resource, string format, XDocument xml)
        {
            return Post(PostAction.Update, project, application, resource, format, xml);
        }

        private void SetObjectState(PostAction action, List<IDataObject> dataObjects)
        {
            if (action == PostAction.Create)
            {
                foreach (IDataObject dataObject in dataObjects)
                {
                    ((SerializableDataObject)dataObject).State = ObjectState.Create;
                }
            }
            else if (action == PostAction.Update)
            {
                foreach (IDataObject dataObject in dataObjects)
                {
                    ((SerializableDataObject)dataObject).State = ObjectState.Update;
                }
            }
            else
            {
                throw new Exception("Invalid operation.");
            }
        }

        private Response Post(PostAction action, string project, string application, string resource, string format, XDocument xml)
        {
            Response response = new Response(); ;

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
                {
                    string message = "Can not perform post on read-only data layer of [" + project + "." + application + "].";
                    _logger.Error(message);

                    response.DateTimeStamp = DateTime.Now;
                    response.Level = StatusLevel.Error;
                    response.Messages = new Messages() { message };

                    return response;
                }

                _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
                List<IDataObject> dataObjects = null;

                if (_isProjectionPart7)
                {
                    dataObjects = _projectionEngine.ToDataObjects(_graphMap.name, ref xml);
                }
                else
                {
                    dataObjects = _projectionEngine.ToDataObjects(_dataObjDef.objectName, ref xml);
                    SetObjectState(action, dataObjects);

                    // throw exception if all key properties are not provided
                    if (action != PostAction.Create)
                    {
                        for (int i = 0; i < dataObjects.Count; i++)
                        {
                            IDataObject dataObject = dataObjects[i];

                            if (_dataObjDef.keyProperties.Count == 1)
                            {
                                string keyProp = _dataObjDef.keyProperties[0].keyPropertyName;
                                object propValue = dataObject.GetPropertyValue(keyProp);
                                if (propValue == null)
                                {
                                    //TODO: remove object from list and add error in payload
                                    throw new Exception("Value of key property: " + keyProp + " cannot be null.");
                                }
                            }
                            else if (_dataObjDef.keyProperties.Count > 1)
                            {
                                bool isKeyPropertiesHaveValues = false;
                                foreach (KeyProperty keyProp in _dataObjDef.keyProperties)
                                {
                                    object propValue = dataObject.GetPropertyValue(keyProp.keyPropertyName);

                                    // it is acceptable to have some key property values to be null but not all
                                    if (propValue != null)
                                    {
                                        isKeyPropertiesHaveValues = true;
                                        break;
                                    }
                                }

                                if (!isKeyPropertiesHaveValues)
                                {
                                    //TODO: remove object from list and add error in payload
                                    throw new Exception("At leat one key property is required.");
                                }

                            }
                        }
                    }
                }

                response = _dataLayerGateway.Update(_dataObjDef, dataObjects);
                response.DateTimeStamp = DateTime.Now;

                string baseUri = _settings["GraphBaseUri"] +
                                 _settings["applicationName"] + "/" +
                                 _settings["projectName"] + "/" +
                                 resource + "/";

                response.PrepareResponse(baseUri);
            }
            catch (Exception ex)
            {
                _logger.Error("Error posting item: " + ex);

                Status status = new Status { Level = StatusLevel.Error };

                if (ex is WebFaultException)
                {
                    status.Messages = new Messages() { Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                    response.Messages = new Messages() { ((WebFaultException)ex).Message, Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                }
                else
                    status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateRelated(string project, string application, string resource, string id, string relatedResource, string format, XDocument xml)
        {
            return PostRelated(PostAction.Update, project, application, resource, id, relatedResource, format, xml);
        }

        private Response PostRelated(PostAction action, string project, string application, string resource, string id, string relatedResource, string format, XDocument xml)
        {
            Response response = new Response();

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_projectionEngine == null)
                {
                    throw new Exception("Initializing projection failed. This is most likely due to invalid format/content-type.");
                }

                if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
                {
                    string message = "Can not perform post on read-only data layer of [" + project + "." + application + "].";
                    _logger.Error(message);

                    response.DateTimeStamp = DateTime.Now;
                    response.Level = StatusLevel.Error;
                    response.Messages = new Messages() { message };

                    return response;
                }

                if (_isProjectionPart7)
                {
                    throw new Exception("Operation not supported.");
                }

                DataObject objectType = _dictionary.dataObjects.Find(c => c.objectName.ToLower() == resource.ToLower());

                if (objectType == null)
                {
                    throw new Exception("Primary resource [" + resource + "] not found.");
                }

                DataRelationship relationship = objectType.dataRelationships.Find(c => c.relationshipName.ToLower() == relatedResource.ToLower());

                //
                // if relationship is null, check if provided related resource by name
                //
                if (relationship == null)
                {
                    relationship = objectType.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedResource.ToLower());
                }

                DataObject relatedType = _dictionary.dataObjects.Find(c => c.objectName == relationship.relatedObjectName);

                if (relatedType == null)
                {
                    throw new Exception("Relationship between parent and child resource not found.");
                }

                //
                // get parent object and set property maps from parent resource in the posted xml
                //
                IDataObject parentObject = _dataLayerGateway.Get(objectType, new List<string> { id }).FirstOrDefault();

                if (parentObject == null)
                {
                    throw new Exception("Parent object with id [" + id + "] not found.");
                }

                List<IDataObject> childObjects = _projectionEngine.ToDataObjects(relatedType.objectName, ref xml);
                SetObjectState(action, childObjects);

                foreach (IDataObject childObject in childObjects)
                {
                    foreach (PropertyMap propMap in relationship.propertyMaps)
                    {
                        object value = parentObject.GetPropertyValue(propMap.dataPropertyName);
                        childObject.SetPropertyValue(propMap.relatedPropertyName, value);
                    }
                }

                response = _dataLayerGateway.Update(relatedType, childObjects);
                response.DateTimeStamp = DateTime.Now;

                string baseUri = _settings["GraphBaseUri"] +
                                 _settings["applicationName"] + "/" +
                                 _settings["projectName"] + "/" +
                                 resource + "/";

                response.PrepareResponse(baseUri);
            }
            catch (Exception ex)
            {
                _logger.Error("Error posting related item: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                if (ex is WebFaultException)
                {
                    status.Messages = new Messages() { Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                    response.Messages = new Messages() { ((WebFaultException)ex).Message, Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                }
                else
                {
                    status.Messages = new Messages { ex.Message };
                }

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response Update(string project, string application, string resource, string format, DataItems dataItems)
        {
            return Post(PostAction.Update, project, application, resource, format, dataItems);
        }

        private Response Post(PostAction action, string project, string application, string resource, string format, DataItems dataItems)
        {
            Response response = new Response();

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
                {
                    string message = "Can not perform post on read-only data layer of [" + project + "." + application + "].";
                    _logger.Error(message);

                    response.DateTimeStamp = DateTime.Now;
                    response.Level = StatusLevel.Error;
                    response.Messages = new Messages() { message };

                    return response;
                }

                foreach (DataItem dataItem in dataItems.items)
                {
                    if (dataItem.id != null && dataItem.id.ToString() != String.Empty)
                    {
                        string[] keyValues = !String.IsNullOrEmpty(_dataObjDef.keyDelimeter)
                                ? dataItem.id.Split(new string[] { _dataObjDef.keyDelimeter }, StringSplitOptions.None)
                                : new string[] { dataItem.id };

                        int i = 0;
                        foreach (KeyProperty key in _dataObjDef.keyProperties)
                        {
                            dataItem.properties[key.keyPropertyName] = keyValues[i];
                            i++;
                        }
                    }
                }

                XDocument xml = new XDocument(dataItems.ToXElement<DataItems>());

                List<IDataObject> dataObjects = null;
                if (_isProjectionPart7)
                {
                    dataObjects = _projectionEngine.ToDataObjects(_graphMap.name, ref xml);
                }
                else
                {
                    dataObjects = _projectionEngine.ToDataObjects(_dataObjDef.objectName, ref xml);
                    SetObjectState(action, dataObjects);
                }

                response = _dataLayerGateway.Update(_dataObjDef, dataObjects);
                response.DateTimeStamp = DateTime.Now;

                string baseUri = _settings["GraphBaseUri"] +
                                 _settings["applicationName"] + "/" +
                                 _settings["projectName"] + "/" +
                                 resource + "/";

                response.PrepareResponse(baseUri);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Post: " + ex);

                Status status = new Status { Level = StatusLevel.Error };

                if (ex is WebFaultException)
                {
                    status.Messages = new Messages() { Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                    response.Messages = new Messages() { ((WebFaultException)ex).Message, Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                }
                else
                {
                    status.Messages = new Messages { ex.Message };
                }

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response PostContent(string project, string application, string resource, string format, string identifier, Stream stream)
        {
            Response response = new Response();
            response.DateTimeStamp = DateTime.Now;

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
                {
                    string message = "Can not perform post on read-only data layer of [" + project + "." + application + "].";
                    _logger.Error(message);

                    response.DateTimeStamp = DateTime.Now;
                    response.Level = StatusLevel.Error;
                    response.Messages = new Messages() { message };

                    return response;
                }

                IContentObject contentObject = new GenericContentObject();
                contentObject.Content = stream;
                contentObject.Identifier = identifier;
                contentObject.ObjectType = _dataObjDef.objectName;

                IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                string contentType = request.ContentType;
                contentObject.ContentType = contentType;

                List<IDataObject> dataObjects = new List<IDataObject>();
                dataObjects.Add(contentObject);

                ///TODO: handle content separately?
                //response = _dataLayerGateway.Update(_dataObjDef, dataObjects);
            }
            catch (Exception ex)
            {
                _logger.Error("Error posting content: " + ex);

                Status status = new Status { Level = StatusLevel.Error };

                if (ex is WebFaultException)
                {
                    status.Messages = new Messages() { Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                    response.Messages = new Messages() { ((WebFaultException)ex).Message, Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                }
                else
                {
                    status.Messages = new Messages { ex.Message };
                }

                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteItem(string project, string application, string resource, string identifier, string format)
        {
            Response response = new Response();

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
                {
                    string message = "Can not perform delete on read-only data layer of [" + project + "." + application + "].";
                    _logger.Error(message);

                    response.DateTimeStamp = DateTime.Now;
                    response.Level = StatusLevel.Error;
                    response.Messages = new Messages() { message };

                    return response;
                }

                if (!_isProjectionPart7)
                {
                    identifier = Utility.ConvertSpecialCharOutbound(identifier, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.

                    List<IDataObject> dataObjects = new List<IDataObject>
          { 
            new SerializableDataObject()
            {
              Id = identifier,
              Type = _dataObjDef.objectName,
              State = ObjectState.Delete
            }
          };

                    response = _dataLayerGateway.Update(_dataObjDef, dataObjects);
                }
                else
                {
                    throw new Exception("The operation is not supported in this service.");
                }

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Success;
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting item: " + ex);

                Status status = new Status { Level = StatusLevel.Error };

                if (ex is WebFaultException)
                {
                    status.Messages = new Messages() { Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                    response.Messages = new Messages() { ((WebFaultException)ex).Message, Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                }
                else
                {
                    status.Messages = new Messages { ex.Message };
                }

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteRelated(string project, string application, string resource, string parentId, string relatedResource, string id, string format)
        {
            Response response = new Response();

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer();
                InitializeProjection(resource, ref format, false);

                if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
                {
                    string message = "Can not perform delete on read-only data layer of [" + project + "." + application + "].";
                    _logger.Error(message);

                    response.DateTimeStamp = DateTime.Now;
                    response.Level = StatusLevel.Error;
                    response.Messages = new Messages() { message };

                    return response;
                }

                if (_isProjectionPart7)
                {
                    throw new Exception("Operation not supported.");
                }
                else
                {
                    id = Utility.ConvertSpecialCharOutbound(id, arrSpecialcharlist, arrSpecialcharValue);

                    DataObject relatedObjectType = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == relatedResource);

                    if (relatedObjectType == null)
                    {
                        throw new Exception("Related resource [" + relatedResource + "] not found.");
                    }

                    List<IDataObject> dataObjects = new List<IDataObject>
          { 
            new SerializableDataObject()
            {
              Id = id,
              Type = relatedResource,
              State = ObjectState.Delete
            }
          };

                    response = _dataLayerGateway.Update(relatedObjectType, dataObjects);
                }

                response.DateTimeStamp = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting item: " + ex);
                Status status = new Status { Level = StatusLevel.Error };

                if (ex is WebFaultException)
                {
                    status.Messages = new Messages() { Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                    response.Messages = new Messages() { ((WebFaultException)ex).Message, Convert.ToString(((WebFaultException)ex).Data["StatusText"]) };
                }
                else
                {
                    status.Messages = new Messages { ex.Message };
                }

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }
        #endregion

        #region private methods
        private void InitializeProjection(string resource, ref string format, bool isIndividual)
        {
            try
            {
                string[] expectedFormats = { 
              "rdf", 
              "dto",
              "xml",
              "json"
            };

                _graphMap = _mapping.FindGraphMap(resource);

                if (_graphMap != null)
                {
                    _isResourceGraph = true;
                    _dataObjDef = _dictionary.dataObjects.Find(o => o.objectName.ToUpper() == _graphMap.dataObjectName.ToUpper());

                    if (_dataObjDef == null || _dataObjDef.isRelatedOnly)
                    {
                        _logger.Warn("Data object [" + _graphMap.dataObjectName + "] not found.");
                        throw new WebFaultException(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    _dataObjDef = _dictionary.dataObjects.Find(o => o.objectName.ToUpper() == resource.ToUpper());

                    if (_dataObjDef == null || _dataObjDef.isRelatedOnly)
                    {
                        _logger.Warn("Resource [" + resource + "] not found.");
                        throw new WebFaultException(HttpStatusCode.NotFound);
                    }
                }

                if (format == null)
                {
                    if (isIndividual && !String.IsNullOrEmpty(_dataObjDef.defaultProjectionFormat))
                    {
                        format = _dataObjDef.defaultProjectionFormat;
                    }
                    else if (!String.IsNullOrEmpty(_dataObjDef.defaultListProjectionFormat))
                    {
                        format = _dataObjDef.defaultListProjectionFormat;
                    }
                    else if (!String.IsNullOrEmpty(_settings["DefaultProjectionFormat"]))
                    {
                        format = _settings["DefaultProjectionFormat"];
                    }
                    else
                    {
                        format = "json";
                    }
                }
                _isFormatExpected = expectedFormats.Contains(format.ToLower());

                if (format != null && _isFormatExpected)
                {
                    _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());

                    if (_projectionEngine.GetType().BaseType == typeof(BasePart7ProjectionEngine))
                    {
                        _isProjectionPart7 = true;
                        if (_graphMap == null)
                        {
                            throw new FileNotFoundException("Requested resource [" + resource + "] cannot be rendered as Part7.");
                        }
                    }
                }
                else if (format == _settings["DefaultProjectionFormat"] && _isResourceGraph)
                {
                    format = "dto";
                    _projectionEngine = _kernel.Get<IProjectionLayer>("dto");
                    _isProjectionPart7 = true;
                }

                if (_projectionEngine != null && typeof(BasePart7ProjectionEngine).IsAssignableFrom(_projectionEngine.GetType()))
                    ((BasePart7ProjectionEngine)_projectionEngine).dataLayerGateway = _dataLayerGateway;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error initializing application: {0}", ex));
                throw ex;
            }
        }

        private void DeleteScope()
        {
            try
            {
                // remove scope
                foreach (ScopeProject project in _scopes)
                {
                    if (project.Name.ToUpper() == _settings["projectName"].ToUpper())
                    {
                        foreach (ScopeApplication application in project.Applications)
                        {
                            if (application.Name.ToUpper() == _settings["applicationName"].ToUpper())
                            {
                                project.Applications.Remove(application);
                            }
                            break;
                        }
                        break;
                    }
                }

                // save scopes
                Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);

                // delete its bindings
                File.Delete(_settings["BindingConfigurationPath"]);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in DeleteScope: {0}", ex));
                throw ex;
            }
        }
        #endregion

        ///TODO
        #region data layer management methods
        //private void InitializeDataLayer(bool setDictionary)
        //{
        //  try
        //  {
        //    if (!_isDataLayerInitialized)
        //    {
        //      _logger.Debug("Initializing data layer...");

        //      if (_settings["DumpSettings"] == "True")
        //      {
        //        Dictionary<string, string> settingsDictionary = new Dictionary<string, string>();
        //        foreach (string key in _settings.AllKeys)
        //        {
        //          settingsDictionary.Add(key, _settings[key]);
        //        }
        //        Utility.Write<Dictionary<string, string>>(settingsDictionary, @"AdapterSettings.xml");
        //        Utility.Write<IDictionary>(_keyRing, @"KeyRing.xml");
        //      }

        //      XElement bindingConfig = Utility.ReadXml(_settings["BindingConfigurationPath"]);
        //      string assembly = bindingConfig.Element("bind").Attribute("to").Value;
        //      DataLayers dataLayers = GetDataLayers();

        //      foreach (DataLayer dataLayer in dataLayers)
        //      {
        //        if (dataLayer.Assembly.ToLower() == assembly.ToLower())
        //        {
        //          if (dataLayer.External)
        //          {
        //            Assembly dataLayerAssembly = GetDataLayerAssembly(dataLayer);

        //            if (dataLayerAssembly == null)
        //            {
        //              throw new Exception("Unable to load data layer assembly.");
        //            }

        //            _dataLayerGatewayPath = dataLayer.Path;

        //            Type type = dataLayerAssembly.GetType(assembly.Split(',')[0]);
        //            ConstructorInfo[] ctors = type.GetConstructors();

        //            foreach (ConstructorInfo ctor in ctors)
        //            {
        //              ParameterInfo[] paramList = ctor.GetParameters();

        //              if (paramList.Length == 0)  // default constructor
        //              {
        //                _dataLayerGateway = (IDataLayer2)Activator.CreateInstance(type);

        //                break;
        //              }
        //              else if (paramList.Length == 1)  // constructor with 1 parameter
        //              {
        //                if (ctor.GetParameters()[0].ParameterType.FullName == typeof(AdapterSettings).FullName)
        //                {
        //                  _dataLayerGateway = (IDataLayer2)Activator.CreateInstance(type, _settings);
        //                }
        //                else if (ctor.GetParameters()[0].ParameterType.FullName == typeof(IDictionary).FullName)
        //                {
        //                  _dataLayerGateway = (IDataLayer2)Activator.CreateInstance(type, _settings);
        //                }

        //                break;
        //              }
        //              else if (paramList.Length == 2)  // constructor with 2 parameters
        //              {
        //                if (ctor.GetParameters()[0].ParameterType.FullName == typeof(AdapterSettings).FullName &&
        //                  ctor.GetParameters()[1].ParameterType.FullName == typeof(IDictionary).FullName)
        //                {
        //                  _dataLayerGateway = (IDataLayer2)Activator.CreateInstance(type, _settings, _keyRing);
        //                }
        //                else if (ctor.GetParameters()[0].ParameterType.FullName == typeof(IDictionary).FullName &&
        //                  ctor.GetParameters()[1].ParameterType.FullName == typeof(AdapterSettings).FullName)
        //                {
        //                  _dataLayerGateway = (IDataLayer2)Activator.CreateInstance(type, _keyRing, _settings);
        //                }
        //                else
        //                {
        //                  throw new Exception("Data layer does not contain supported constructor.");
        //                }

        //                break;
        //              }
        //            }
        //          }

        //          if (_dataLayerGateway != null)
        //          {
        //            _kernel.Rebind<IDataLayer2>().ToConstant(_dataLayerGateway);
        //          }

        //          break;
        //        }
        //      }

        //      //
        //      // if data layer is internal or not registered (for backward compatibility),
        //      // attempt to load it using binding configuration
        //      //
        //      if (_dataLayerGateway == null)
        //      {
        //        if (File.Exists(_settings["BindingConfigurationPath"]))
        //        {
        //          _kernel.Load(_settings["BindingConfigurationPath"]);
        //        }
        //        else
        //        {
        //          _logger.Error("Binding configuration not found.");
        //        }

        //        _dataLayerGateway = _kernel.TryGet<IDataLayer2>("DataLayer");

        //        if (_dataLayerGateway == null)
        //        {
        //          _dataLayerGateway = (IDataLayer2)_kernel.Get<IDataLayer>("DataLayer");
        //        }

        //        _kernel.Rebind<IDataLayer2>().ToConstant(_dataLayerGateway);
        //      }

        //      if (_dataLayerGateway == null)
        //      {
        //        throw new Exception("Error initializing data layer.");
        //      }

        //      if (setDictionary)
        //        InitializeDictionary();
        //    }
        //  }
        //  catch (Exception ex)
        //  {
        //    _logger.Error(string.Format("Error initializing application: {0}", ex));
        //    throw new Exception(string.Format("Error initializing application: {0})", ex));
        //  }
        //}

        //public DataLayers GetDataLayers()
        //{
        //  DataLayers dataLayers = new DataLayers();

        //  try
        //  {
        //    if (File.Exists(_dataLayerGatewaysRegistryPath))
        //    {
        //      dataLayers = Utility.Read<DataLayers>(_dataLayerGatewaysRegistryPath);
        //      int dataLayersCount = dataLayers.Count;

        //      //
        //      // validate external data layers, remove from list if no longer exists
        //      //
        //      for (int i = 0; i < dataLayers.Count; i++)
        //      {
        //        DataLayer dataLayer = dataLayers[i];

        //        if (dataLayer.External)
        //        {
        //          string qualPath = dataLayer.Path + "\\" + dataLayer.MainDLL;

        //          if (!File.Exists(qualPath))
        //          {
        //            dataLayers.RemoveAt(i--);
        //          }
        //        }
        //      }

        //      if (dataLayersCount > dataLayers.Count)
        //      {
        //        Utility.Write<DataLayers>(dataLayers, _dataLayerGatewaysRegistryPath);
        //      }
        //    }
        //    else
        //    {
        //      //
        //      // get internal data layers
        //      //
        //      DataLayers internalDataLayers = GetInternalDataLayers();

        //      if (internalDataLayers != null && internalDataLayers.Count > 0)
        //      {
        //        dataLayers.AddRange(internalDataLayers);
        //      }

        //      // 
        //      // register existing data layers from manual deployment
        //      //
        //      try
        //      {
        //        Type type = typeof(IDataLayer);
        //        Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        //        foreach (Assembly asm in domainAssemblies)
        //        {
        //          Type[] asmTypes = null;

        //          try
        //          {
        //            asmTypes = asm.GetTypes();
        //          }
        //          catch (Exception) { }

        //          if (asmTypes != null)
        //          {
        //            foreach (System.Type asmType in asmTypes)
        //            {
        //              if (type.IsAssignableFrom(asmType) && !(asmType.IsInterface || asmType.IsAbstract))
        //              {
        //                bool configurable = asmType.BaseType.Equals(typeof(BaseConfigurableDataLayer));
        //                string name = asm.FullName.Split(',')[0];

        //                if (!dataLayers.Exists(x => x.Name.ToLower() == name.ToLower()))
        //                {
        //                  string assembly = string.Format("{0}, {1}", asmType.FullName, name);

        //                  DataLayer dataLayer = new DataLayer { 
        //                    Assembly = assembly, 
        //                    Name = name,
        //                    External = true,
        //                    Path = _settings["BaseDirectoryPath"] + @"bin\",
        //                    MainDLL = asm.ManifestModule.Name,
        //                    Configurable = configurable 
        //                  };

        //                  dataLayers.Add(dataLayer);
        //                }
        //              }
        //            }
        //          }
        //        }
        //      }
        //      catch (Exception e)
        //      {
        //        _logger.Error("Error loading assembly: " + e);
        //      }

        //      Utility.Write<DataLayers>(dataLayers, _dataLayerGatewaysRegistryPath);
        //    }
        //  }
        //  catch (Exception e)
        //  {
        //    _logger.Error("Error getting data layers: " + e);
        //    throw e;
        //  }

        //  return dataLayers;
        //}

        //public Response PostDataLayer(DataLayer dataLayer)
        //{
        //  Response response = new Response();
        //  response.Level = StatusLevel.Success;

        //  try
        //  {
        //    DataLayers dataLayers = GetDataLayers();
        //    DataLayer dl = dataLayers.Find(x => x.Name.ToLower() == dataLayer.Name.ToLower());
        //    dataLayer.Path = _settings["DataLayersPath"] + dataLayer.Name + "\\";

        //    // extract package file
        //    if (dataLayer.Package != null)
        //    {
        //      try
        //      {
        //        //
        //        // remove DLLs and EXEs from previous upload
        //        //
        //        if (Directory.Exists(dataLayer.Path))
        //        {
        //          string[] files = Directory.GetFiles(dataLayer.Path);
        //          foreach (string file in files)
        //          {
        //            string lcFile = file.ToLower();
        //            if (lcFile.EndsWith(".exe") || lcFile.EndsWith(".dll"))
        //            {
        //              File.Delete(file);
        //            }
        //          }
        //        }

        //        Utility.Unzip(dataLayer.Package, dataLayer.Path);
        //        dataLayer.Package = null;
        //      }
        //      catch (UnauthorizedAccessException e)
        //      {
        //        _logger.Warn("Error extracting DataLayer package: " + e);
        //        response.Level = StatusLevel.Warning;
        //      }
        //    }

        //    //
        //    // validate data layer
        //    //
        //    Assembly dataLayerAssembly = GetDataLayerAssembly(dataLayer);
        //    if (dataLayerAssembly == null)
        //    {
        //      throw new Exception("Unable to load DataLayer assembly.");
        //    }

        //    dataLayer.Assembly = GetDataLayerAssemblyName(dataLayerAssembly);
        //    dataLayer.MainDLL = dataLayerAssembly.ManifestModule.ScopeName;
        //    dataLayer.External = true;

        //    if (!string.IsNullOrEmpty(dataLayer.Assembly))
        //    {
        //      //
        //      // move configuration to app data folder
        //      //
        //      string[] files = Directory.GetFiles(dataLayer.Path);
        //      foreach (string file in files)
        //      {
        //        string lcFile = file.ToLower();
        //        if (!(lcFile.EndsWith(".exe") || lcFile.EndsWith(".dll") || lcFile.EndsWith(".resources")))
        //        {
        //          File.Copy(file, _settings["AppDataPath"] + Path.GetFileName(file), true);
        //        }
        //      }

        //      // remove data layer if exists
        //      if (dl != null)
        //      {
        //        if (dl.Path == _settings["BaseDirectoryPath"] + "bin\\")
        //        {
        //          File.Delete(dl.Path + dl.MainDLL);
        //        }

        //        dataLayers.Remove(dl);
        //      }

        //      dataLayers.Add(dataLayer);

        //      Utility.Write<DataLayers>(dataLayers, _dataLayerGatewaysRegistryPath);
        //      response.Messages.Add("DataLayer [" + dataLayer.Name + "] saved successfully.");
        //    }
        //    else
        //    {
        //      if (Directory.Exists(dataLayer.Path))
        //      {
        //        Directory.Delete(dataLayer.Path, true);
        //      }

        //      response.Level = StatusLevel.Error;
        //      response.Messages.Add("DataLayer [" + dataLayer.Name + "] is not compatible.");
        //    }
        //  }
        //  catch (Exception e)
        //  {
        //    _logger.Error("Error saving DataLayer: " + e);

        //    if (Directory.Exists(dataLayer.Path))
        //    {
        //      Directory.Delete(dataLayer.Path, true);
        //    }

        //    response.Level = StatusLevel.Error;
        //    response.Messages.Add("Error adding DataLayer [" + dataLayer.Name + "]. " + e);
        //  }

        //  return response;
        //}

        //public Response DeleteDataLayer(string dataLayerName)
        //{
        //  Response response = new Response();

        //  try
        //  {
        //    DataLayers dataLayers = GetDataLayers();
        //    DataLayer dl = dataLayers.Find(x => x.Name.ToLower() == dataLayerName.ToLower());

        //    if (dl == null)
        //    {
        //      response.Level = StatusLevel.Error;
        //      response.Messages.Add("DataLayer [" + dataLayerName + "] not found.");
        //    }
        //    else
        //    {
        //      if (dl.External)
        //      {
        //        dataLayers.Remove(dl);
        //        Utility.Write<DataLayers>(dataLayers, _dataLayerGatewaysRegistryPath);

        //        string dlPath = dl.Path;
        //        Directory.Delete(dlPath, true);

        //        response.Level = StatusLevel.Success;
        //        response.Messages.Add("DataLayer [" + dataLayerName + "] deleted successfully.");
        //      }
        //      else
        //      {
        //        response.Level = StatusLevel.Error;
        //        response.Messages.Add("Deleting internal DataLayer [" + dataLayerName + "] is not allowed.");
        //      }
        //    }
        //  }
        //  catch (Exception e)
        //  {
        //    _logger.Error("Error getting DataLayer: " + e);

        //    response.Level = StatusLevel.Success;
        //    response.Messages.Add("Error deleting DataLayer [" + dataLayerName + "]." + e);
        //  }

        //  return response;
        //}

        //private DataLayers GetInternalDataLayers()
        //{
        //  DataLayers dataLayers = new DataLayers();

        //  // load NHibernate data layer
        //  Type type = typeof(NHibernateDataLayer);
        //  string library = type.Assembly.GetName().Name;
        //  string assembly = string.Format("{0}, {1}", type.FullName, library);
        //  DataLayer dataLayer = new DataLayer { Assembly = assembly, Name = library, Configurable = true };
        //  dataLayers.Add(dataLayer);

        //  // load Spreadsheet data layer
        //  type = typeof(SpreadsheetDatalayer);
        //  library = type.Assembly.GetName().Name;
        //  assembly = string.Format("{0}, {1}", type.FullName, library);
        //  dataLayer = new DataLayer { Assembly = assembly, Name = library, Configurable = true };
        //  dataLayers.Add(dataLayer);

        //  return dataLayers;
        //}

        //private Assembly GetDataLayerAssembly(DataLayer dataLayer)
        //{
        //  string mainDLL = dataLayer.MainDLL;

        //  if (string.IsNullOrEmpty(mainDLL))
        //  {
        //    Type type = typeof(IDataLayer);
        //    string path = Path.Combine(_settings["BaseDirectoryPath"], dataLayer.Path);
        //    string[] files = Directory.GetFiles(path);

        //    foreach (string file in files)
        //    {
        //      string lcFile = file.ToLower();

        //      if (!(lcFile.EndsWith(".dll") || lcFile.EndsWith(".exe")))
        //        continue;

        //      Assembly asm = null;
        //      Type[] asmTypes = null;

        //      try
        //      {
        //        byte[] bytes = Utility.GetBytes(file);
        //        asm = Assembly.Load(bytes);
        //        asmTypes = asm.GetTypes();
        //      }
        //      catch (Exception e) 
        //      {
        //        _logger.Error("Error getting types from assembly [" + file + "]: " + e);
        //      }

        //      if (asmTypes != null)
        //      {
        //        foreach (System.Type asmType in asmTypes)
        //        {
        //          if (type.IsAssignableFrom(asmType) && !(asmType.IsInterface || asmType.IsAbstract))
        //          {
        //            return asm;
        //          }
        //        }
        //      }
        //    }
        //  }
        //  else
        //  {
        //    byte[] bytes = Utility.GetBytes(dataLayer.Path + mainDLL);
        //    return Assembly.Load(bytes);
        //  }

        //  return null;
        //}

        //private string GetDataLayerAssemblyName(Assembly assembly)
        //{
        //  Type dlType = typeof(IDataLayer);
        //  Type[] asmTypes = assembly.GetTypes();

        //  if (asmTypes != null)
        //  {
        //    foreach (System.Type asmType in asmTypes)
        //    {
        //      if (dlType.IsAssignableFrom(asmType) && !(asmType.IsInterface || asmType.IsAbstract))
        //      {
        //        bool configurable = asmType.BaseType.Equals(typeof(BaseConfigurableDataLayer));
        //        string name = assembly.FullName.Split(',')[0];

        //        return string.Format("{0}, {1}", asmType.FullName, name);
        //      }
        //    }
        //  }

        //  return string.Empty;
        //}

        //private Assembly DataLayerAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        //{
        //  if (args.Name.Contains(".resources,"))
        //  {
        //    return null;
        //  }

        //  if (Directory.Exists(_dataLayerGatewayPath))
        //  {
        //    string[] files = Directory.GetFiles(_dataLayerGatewayPath);

        //    foreach (string file in files)
        //    {
        //      if (file.ToLower().EndsWith(".dll") || file.ToLower().EndsWith(".exe"))
        //      {
        //        byte[] bytes = Utility.GetBytes(file);
        //        Assembly assembly = Assembly.Load(bytes);

        //        if (args.Name == assembly.FullName)
        //          return assembly;
        //      }
        //    }

        //    _logger.Error("Unable to resolve assembly [" + args.Name + "].");
        //  }

        //  return null;
        //}
        #endregion data layer management methods

        public DataLayers GetDataLayers()
        {
            try
            {
                //string path = AppDomain.CurrentDomain.BaseDirectory + "\\App_Data\\DataLayers.xml";
                string path = _settings["AppDataPath"] + "DataLayers.xml";
                DataLayers dataLayers = Utility.Read<DataLayers>(path);
                return dataLayers;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error getting data layer: {0}" + ex));
                throw ex;
            }
        }

        public Response RefreshAll(string project, string application)
        {
            Response response = new Response();

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer(false);

                _dataLayerGateway.GetDictionary(true);

                response.Level = StatusLevel.Success;
                response.Messages.Add("Refresh successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error refreshing data objects: {0}", ex));

                response.Level = StatusLevel.Success;
                response.Messages.Add("Error refreshing data objects: " + ex.Message);
            }

            return response;
        }

        public Response RefreshDataObject(string project, string application, string objectType)
        {
            Response response = new Response();

            try
            {
                InitializeScope(project, application);
                InitializeDataLayer(false);

                _dataLayerGateway.GetDictionary(true, objectType);

                response.Level = StatusLevel.Success;
                response.Messages.Add("Refresh successfully.");
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("Error refreshing data object [{0}]: {1}", objectType, ex);
                _logger.Error(errMsg);

                response.Level = StatusLevel.Success;
                response.Messages.Add("Error refreshing data objects: " + ex.Message);
            }

            return response;
        }

        public DocumentBytes GetResourceData(string scope, string app)
        {
            DocumentBytes documentBytes = new DocumentBytes();

            string path = _settings["AppDataPath"];
            if (!path.StartsWith(@"\\") && !path.Contains(@":\"))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            string[] filePaths = Directory.GetFiles(path, "SpreadsheetData." + scope + "." + app + ".xlsx");
            string fileName = filePaths[0];

            byte[] _Buffer = null;

            if (fileName.Length > 0)
            {
                System.IO.FileStream _FileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);
                long _TotalBytes = new System.IO.FileInfo(fileName).Length;
                _Buffer = _BinaryReader.ReadBytes((Int32)_TotalBytes);
            }

            documentBytes.Content = _Buffer;
            documentBytes.DocumentPath = path;

            return documentBytes;
        }

        public byte[] GetResourceDataBytes(string scope, string app)
        {
            string path = _settings["AppDataPath"];
            if (!path.StartsWith(@"\\") && !path.Contains(@":\"))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            string[] filePaths = Directory.GetFiles(path, scope + "." + app + ".*.mdb");

            if (filePaths.Length > 0)
            {
                string _FileName = filePaths[0];
                return System.IO.File.ReadAllBytes(_FileName);
            }
            else
                return null;
        }

        /// <summary>
        /// upload a file to the app folder
        /// </summary>
        /// <param name="filecontent">stream of fie</param>
        /// <param name="fileName">name of the file</param>
        /// <returns></returns>
        public Response UploadFile(Stream filecontent, string fileName)
        {

            Response response = new Response();
            response.Messages = new Messages();

            string path = _settings["AppDataPath"];
            if (!path.StartsWith(@"\\") && !path.Contains(@":\"))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            string savedFileName = Path.Combine(path, Path.GetFileName(fileName));

            try
            {
                const int bufferSize = 65536; // 64K

                using (FileStream outfile = new FileStream(savedFileName, FileMode.Create))
                {
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead = filecontent.Read(buffer, 0, bufferSize);

                    while (bytesRead > 0)
                    {
                        outfile.Write(buffer, 0, bytesRead);
                        bytesRead = filecontent.Read(buffer, 0, bufferSize);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Messages.Add(String.Format("Failed to Upload Files[{0}]", _settings["Scope"]));
                response.Messages.Add(ex.Message);
                response.Level = StatusLevel.Error;
            }
            finally
            {
                // fileToupload.Close();
                //fileToupload.Dispose();
                filecontent.Close();
            }

            return response;
        }


        /// <summary>
        /// It will be used for downloading a single file
        /// </summary>
        /// <param name="scope">scope name</param>
        /// <param name="app">appication name</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="extension">extension of the file</param>
        /// <returns></returns>
        public DocumentBytes DownLoadFile(string scope, string app, string fileName, string extension)
        {
            DocumentBytes documentBytes = new DocumentBytes();

            string path = _settings["AppDataPath"];
            if (!path.StartsWith(@"\\") && !path.Contains(@":\"))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            string[] filePaths = Directory.GetFiles(path, scope + "." + app + "." + fileName + "." + extension);
            string file = filePaths[0];

            if (file.Length > 0)
            {
                System.IO.FileStream fileStream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                byte[] buffer = new byte[32768];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read = 1;
                    while (read > 0)
                    {
                        read = fileStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                            documentBytes.Content = ms.ToArray();
                        ms.Write(buffer, 0, read);
                    }
                }
            }

            documentBytes.DocumentPath = path;
            return documentBytes;
        }

        /// <summary>
        /// list of the files to be downloaded
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        public List<Files> GetDownloadedList(string scope, string app)
        {
            List<Files> files = new List<Files>();
            string pattern = scope + "." + app + ".*";
            string patternToRemove = scope + "." + app + ".";

            string path = _settings["AppDataPath"];
            if (!path.StartsWith(@"\\") && !path.Contains(@":\"))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            string[] filePaths = Directory.GetFiles(path, pattern);
            foreach (string name in filePaths)
            {
                Files file = new Files();

                if (name.Contains("\\"))
                {
                    file.File = name.Substring(name.LastIndexOf("\\") + 1).Remove(0, patternToRemove.Length);
                }

                files.Add(file);
            }

            return files;
        }

        public void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        {
            if (format.ToUpper() == "JSON")
            {
                string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
        }

        public void FormatOutgoingMessage(XElement xElement, string format)
        {
            if (format == null)
            {
                format = String.Empty;
            }

            if (format.ToUpper() == "HTML")
            {
                HttpContext.Current.Response.ContentType = "text/html";
                HttpContext.Current.Response.Write(xElement.ToString());
            }
            else if (format.ToUpper() == "JSON")
            {
                string json = ToJson(xElement);
                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xElement.ToString());
            }
        }

        public void FormatOutgoingMessage(object content, string format)
        {
            if (typeof(IContentObject).IsInstanceOfType(content))
            {
                IContentObject contentObject = (IContentObject)content;

                if (!string.IsNullOrEmpty(contentObject.ContentType))
                {
                    HttpContext.Current.Response.ContentType = contentObject.ContentType;
                }
                else
                {
                    if (_isFormatExpected)
                    {
                        format = _settings["DefaultContentFormat"];

                        if (string.IsNullOrEmpty(format))
                        {
                            format = "pdf";
                        }
                    }

                    string contentType = Registry.ClassesRoot.OpenSubKey("." + format).GetValue("Content Type").ToString();
                    HttpContext.Current.Response.ContentType = contentType;
                }

                HttpContext.Current.Response.BinaryWrite(contentObject.Content.ToMemoryStream().GetBuffer());
            }
            else if (content.GetType() == typeof(XDocument))
            {
                XDocument xDoc = (XDocument)content;
                FormatOutgoingMessage(xDoc.Root, format);
            }
            else
            {
                throw new Exception("Invalid response type from DataLayer.");
            }
        }

        public XElement FormatIncomingMessage(Stream stream, string format)
        {
            XElement xElement = null;

            if (format != null && (format.ToLower().Contains("xml") || format.ToLower().Contains("rdf") ||
              format.ToLower().Contains("dto")))
            {
                xElement = XElement.Load(stream);
            }
            else
            {
                DataItems dataItems = FormatIncomingMessage(stream);

                xElement = dataItems.ToXElement<DataItems>();
            }

            return xElement;
        }

        public DataItems FormatIncomingMessage(Stream stream)
        {
            DataItems dataItems = null;

            DataItemSerializer serializer = new DataItemSerializer(
                _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
            string json = Utility.ReadString(stream);
            dataItems = serializer.Deserialize<DataItems>(json, false);
            stream.Close();

            return dataItems;
        }

        public T FormatIncomingMessage<T>(Stream stream, string format, bool useDataContractSerializer)
        {
            T graph = default(T);

            if (format != null && format.ToLower().Contains("xml"))
            {
                graph = Utility.DeserializeFromStream<T>(stream, useDataContractSerializer);
            }
            else
            {
                DataItemSerializer serializer = new DataItemSerializer(
                    _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
                string json = Utility.ReadString(stream);
                graph = serializer.Deserialize<T>(json, false);
                stream.Close();
            }

            return graph;
        }

        private DataFilter CreateDataFilter(NameValueCollection parameters, string sortOrder, string sortBy)
        {
            DataFilter filter = new DataFilter();

            if (parameters != null)
            {
                _logger.Debug("Preparing Filter from parameters.");

                foreach (string key in parameters.AllKeys)
                {
                    string[] expectedParameters = { 
                          "project",
                          "app",
                          "format", 
                          "start", 
                          "limit", 
                          "sortBy", 
                          "sortOrder",
                          "indexStyle",
                          "_dc",
                          "page",
                          "callback",
                        };

                    if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
                    {
                        string value = parameters[key];

                        Expression expression = new Expression
                        {
                            PropertyName = key,
                            RelationalOperator = RelationalOperator.EqualTo,
                            Values = new Values { value },
                            IsCaseSensitive = false,
                        };

                        if (filter.Expressions.Count > 0)
                        {
                            expression.LogicalOperator = LogicalOperator.And;
                        }

                        filter.Expressions.Add(expression);
                    }
                }

                if (!String.IsNullOrEmpty(sortBy))
                {
                    OrderExpression orderBy = new OrderExpression
                    {
                        PropertyName = sortBy,
                    };

                    if (String.Compare(SortOrder.Desc.ToString(), sortOrder, true) == 0)
                    {
                        orderBy.SortOrder = SortOrder.Desc;
                    }
                    else
                    {
                        orderBy.SortOrder = SortOrder.Asc;
                    }

                    filter.OrderExpressions.Add(orderBy);
                }
            }

            return filter;
        }

        private List<IDataObject> PageDataObjects(DataObject objectType, DataFilter filter)
        {
            List<IDataObject> dataObjects = new List<IDataObject>();

            int pageSize = (String.IsNullOrEmpty(_settings["DefaultPageSize"]))
              ? 250 : int.Parse(_settings["DefaultPageSize"]);

            long count = _dataLayerGateway.GetCount(objectType, filter);

            for (int offset = 0; offset < count; offset = offset + pageSize)
            {
                dataObjects.AddRange(_dataLayerGateway.Get(objectType, filter, offset, pageSize));
            }

            return dataObjects;
        }

        public Locator Publish(string project, string application)
        {
            string path = _settings["AppDataPath"] + "publishingTemplate.xml";
            var xmlDoc = new XmlDocument { XmlResolver = null };
            if (File.Exists(path))
            {
                xmlDoc.Load(path);
            }
            else
                throw new Exception("Publishing template not found.");

            DataDictionary dictionary = GetDictionary(project, application);

            Locator locator = (Locator)Utility.Deserialize<Locator>(xmlDoc.InnerXml, true);

            if (dictionary != null)
            {
                locator.updated = DateTime.Now;
                if (!string.IsNullOrEmpty(dictionary.description))
                    locator.description = dictionary.description;

                foreach (Instance Inst in locator.instances)
                {
                    Inst.updated = DateTime.Now;
                    List<Endpoint> newEndpoints = new List<Endpoint>();
                    bool IsContextsEndpoint = false;
                    foreach (DataObject objects in dictionary.dataObjects)
                    {
                        foreach (Endpoint epoint in Inst.endpoints)
                        {
                            Endpoint newepoint = Utility.CloneDataContractObject<Endpoint>(epoint);
                            if (!newepoint.path.Contains("0") && IsContextsEndpoint)
                                continue;
                            if (!newepoint.path.Contains("0"))
                                IsContextsEndpoint = true;


                            newepoint.path = string.Format(newepoint.path, objects.objectName);
                            if (!string.IsNullOrEmpty(objects.description))
                            {
                                newepoint.description = objects.description;
                            }
                            else
                            {
                                newepoint.description = string.Format(newepoint.description, objects.objectName);
                            }

                            foreach (Operation opers in newepoint.operations)
                            {
                                opers.summary = string.Format(opers.summary, objects.objectName);
                                opers.updated = DateTime.Now;

                                foreach (Parameter param in opers.parameters)
                                {
                                    param.description = string.Format(param.description, objects.objectName);
                                }
                            }
                            newEndpoints.Add(newepoint);
                        }
                    }

                    Inst.endpoints.Clear();
                    Inst.endpoints.AddRange(newEndpoints);
                }
            }
            return locator;
        }
    }
}