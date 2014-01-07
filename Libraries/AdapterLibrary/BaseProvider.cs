using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Xml.Linq;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using org.ids_adi.qmxf;
using org.iringtools.adapter.identity;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.utility;
using StaticDust.Configuration;
using org.iringtools.library.tip;

namespace org.iringtools.adapter
{
  public abstract class BaseProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseProvider));
    public const string CACHE_CONNSTR = "iRINGCacheConnStr";
    public const string CACHE_CONNSTR_LEVEL = "Adapter";

    protected IKernel _kernel = null;
    protected AdapterSettings _settings = null;
    protected ScopeProjects _scopes = null;
    protected ScopeProjects _authorizedscopes = null;
    protected ScopeApplication _application = null;
    protected DataLayerGateway _dataLayerGateway = null;
    protected DataDictionary _dictionary = null;
    protected mapping.Mapping _mapping = null;
    protected bool _isScopeInitialized = false;
    protected bool _isDataLayerInitialized = false;
    protected static ConcurrentDictionary<string, RequestStatus> _requests =
     new ConcurrentDictionary<string, RequestStatus>();

    protected bool _enableUISecurity = false;
    protected bool _isAdministrator = false;
    protected List<string> _lstSecurityGroups = new List<string>();

    protected Dictionary<string, KeyValuePair<string, Dictionary<string, string>>> _qmxfTemplateResultCache = null;
    protected WebHttpClient _webHttpClient = null;  // for old mapping conversion

    //FKM
    protected TipMapping _tipMapping = null;
    protected string format = "";

    public BaseProvider(NameValueCollection settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false, UseReflectionBasedInjection = true };
      _kernel = new StandardKernel(ninjectSettings, new AdapterModule());

      _kernel.Load(new XmlExtensionModule());
      _settings = _kernel.Get<AdapterSettings>();
      _settings.AppendSettings(settings);

      // Capture request headers
      if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest != null &&
        WebOperationContext.Current.IncomingRequest.Headers != null)
      {
        foreach (string headerName in WebOperationContext.Current.IncomingRequest.Headers.AllKeys)
        {
          _settings["http-header-" + headerName] = WebOperationContext.Current.IncomingRequest.Headers[headerName];
        }
      }

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      _settings[CACHE_CONNSTR_LEVEL] = "Adapter";

      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string rdsUri = _settings["RefDataServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _webHttpClient = new WebHttpClient(rdsUri, null, webProxy);
      }
      else
      {
        _webHttpClient = new WebHttpClient(rdsUri);
      }
      #endregion

      string scopesPath = String.Format("{0}Scopes.xml", _settings["AppDataPath"]);
      _settings["ScopesPath"] = scopesPath;

      if (File.Exists(scopesPath))
      {
        _scopes = Utility.Read<ScopeProjects>(scopesPath);
        bool needToUpdate = false;

        foreach (ScopeProject scope in _scopes)
        {
          if (string.IsNullOrEmpty(scope.DisplayName))
          {
            scope.DisplayName = scope.Name;
            needToUpdate = true;
          }

          foreach (ScopeApplication app in scope.Applications)
          {
            if (string.IsNullOrEmpty(app.DisplayName))
            {
              app.DisplayName = app.Name;
              needToUpdate = true;
            }
          }
        }

        if (needToUpdate)
        {
          Utility.Write<ScopeProjects>(_scopes, scopesPath);
        }
      }
      else
      {
        _scopes = new ScopeProjects();
        Utility.Write<ScopeProjects>(_scopes, scopesPath);
      }

      // read scope configration file
      
      foreach (var scope in _scopes)
      {
        string scopeConfigPath = String.Format("{0}{1}.config", _settings["AppDataPath"], scope.Name);
        if (File.Exists(scopeConfigPath))
          scope.Configuration = Utility.Read<Configuration>(scopeConfigPath,false);
        else
          scope.Configuration = new Configuration() { AppSettings = new AppSettings()};

        if (scope.Configuration != null && scope.Configuration.AppSettings != null && scope.Configuration.AppSettings.Settings != null)
        {
          var connectionSetting = (from setting in scope.Configuration.AppSettings.Settings
                                   where setting.Key == CACHE_CONNSTR
                                   select setting).SingleOrDefault();
          if (connectionSetting != null)
          {
            if (Utility.IsBase64Encoded(connectionSetting.Value))
            {
              _settings[CACHE_CONNSTR_LEVEL] = "Scope";
              string keyFile = string.Format("{0}{1}.key", _settings["AppDataPath"], scope.Name);
              connectionSetting.Value = EncryptionUtility.Decrypt(connectionSetting.Value, keyFile);
            }
          }
        }
      }
      
      string relativePath = String.Format("{0}BindingConfiguration.Adapter.xml", _settings["AppDataPath"]);

      // Ninject Extension requires fully qualified path.
      string adapterBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );

      _kernel.Load(adapterBindingPath);
      
      InitializeIdentity();
      InitializeAuthorizedScopes();
    }

    protected void InitializeAuthorizedScopes()
    {
        _enableUISecurity = Convert.ToBoolean(_settings["EnableUISecurity"]);
        _authorizedscopes = GetAuthorizedScope(_settings["UserName"]);
    }

    protected void InitializeDataLayer()
    {
      InitializeDataLayer(true);
    }

    protected void InitializeDataLayer(bool setDictionary)
    {
      try
      {
        if (!_isDataLayerInitialized)
        {
          _logger.Debug("Initializing data layer...");

          if (_settings["DumpSettings"] == "True")
          {
            Dictionary<string, string> settingsDictionary = new Dictionary<string, string>();

            foreach (string key in _settings.AllKeys)
            {
              settingsDictionary.Add(key, _settings[key]);
            }

            Utility.Write<Dictionary<string, string>>(settingsDictionary, @"AdapterSettings.xml");
          }

          _dataLayerGateway = new DataLayerGateway(_kernel);

          if (setDictionary)
          {
            _dictionary = _dataLayerGateway.GetDictionary();
            _kernel.Rebind<DataDictionary>().ToConstant(_dictionary);
          }

          _isDataLayerInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw ex;
      }
    }

    protected void InitializeIdentity()
    {
      try
      {
        IIdentityLayer identityLayer = _kernel.Get<IIdentityLayer>("IdentityLayer");

        IDictionary keyRing = identityLayer.GetKeyRing();
        _kernel.Bind<IDictionary>().ToConstant(keyRing).Named("KeyRing");

        if (keyRing != null)
        {
          _logger.Debug("Identity attributes:");
          foreach (var key in keyRing.Keys)
          {
            _logger.Debug(key.ToString() + ": " + keyRing[key]);
          }
        }

        _settings.AppendKeyRing(keyRing);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing identity: {0}", ex));
        throw ex;
      }
    }

    public ScopeProject GetScope(string scopeName)
    {
        ScopeProjects scopeProjects;
        if (_enableUISecurity == false)
        {
            scopeProjects = _scopes;
        }
        else
        {
            scopeProjects = _authorizedscopes;
        }

        foreach (ScopeProject scope in scopeProjects)
        {
            if (scope.Name.ToLower() == scopeName.ToLower())
            {
                foreach (ScopeApplication app in scope.Applications)
                {
                    string bindingConfigPath =
                      string.Format("{0}BindingConfiguration.{1}.{2}.xml",
                      _settings["AppDataPath"], scope.Name, app.Name);

                    XElement binding = Utility.GetxElementObject(bindingConfigPath);

                    if (binding.Element("bind").Attribute("service").Value.ToString().Contains(typeof(ILightweightDataLayer).Name))
                        app.DataMode = DataMode.Cache;
                }

                return scope;
            }
        }

        throw new Exception("Scope [" + scopeName + "] not found.");
    }

    public ScopeProjects GetAuthorizedScope(string userName)
    {
        List<string> lstGroups = new List<string>();
        int count = 0; int appcount = 0; bool exist;
        ScopeProjects authorizedScopes = null;

        try
        {
            if (_enableUISecurity == false)
                return null;

            authorizedScopes = Utility.CloneDataContractObject<ScopeProjects>(_scopes);
            lstGroups = GetUserGroups(userName);  //Get all groups from LDAP to which user belongs.
            if (lstGroups.Contains("administrator"))
            {
                _isAdministrator = true;
            }
            else
            {
                foreach (ScopeProject scope in _scopes)
                {
                    exist = false;
                    if (scope.PermissionGroup != null && scope.PermissionGroup.Count != 0)
                        exist = lstGroups.Any(s => scope.PermissionGroup.Contains(s));

                    if (!exist)    // If no access on scope or access not given, check on apps level.
                    {
                        appcount = 0;
                        foreach (ScopeApplication app in scope.Applications)
                        {
                            if (app.PermissionGroup == null || app.PermissionGroup.Count == 0)
                            {
                                //If no access on scope and rights not defined on app, app will not be shown.
                                authorizedScopes[count].Applications.RemoveAt(appcount);
                                continue;
                            }
                            
                            exist = lstGroups.Any(s => app.PermissionGroup.Contains(s));
                            if (!exist)  // If no access on app, remove it
                            {
                                authorizedScopes[count].Applications.RemoveAt(appcount);
                                appcount--;
                            }
                            appcount++;
                        }
                       
                        if (authorizedScopes[count].Applications.Count == 0) // If no apps exist, remove scopes too.
                        {
                            authorizedScopes.RemoveAt(count);
                            count--;
                        }
                    }
                    count++;
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(string.Format("Error initializing authorized scopes: {0}", e));
        }
        return authorizedScopes;
    }

    public PermissionGroups GetUserGroups(string userName)
    {
        List<string> lstGroups = new List<string>();

        if (_enableUISecurity == true)
            lstGroups = ConfigurationRepository.GetUserGroups(userName, _settings["AppDataPath"]);

        PermissionGroups groups = new PermissionGroups();
        groups.AddRange(lstGroups);
        return groups;
    }

    public PermissionGroups GetAllSecurityGroups()
    {
        List<string> lstGroups = new List<string>();
        if (_enableUISecurity == true)
            lstGroups = ConfigurationRepository.GetAllGroups(_settings["AppDataPath"]);

        PermissionGroups groups = new PermissionGroups();
        groups.AddRange(lstGroups);
        return groups;
    }

    public NameValueList GetGlobalUISettings()
    {
        NameValueList nvc = new NameValueList();
        nvc.Add(new ListItem() { Name = "isUISecurityEnabled", Value = _enableUISecurity.ToString() });
        nvc.Add(new ListItem() { Name = "isAdmin", Value = _isAdministrator.ToString() });

        return nvc;
    }

    public ScopeApplication GetApplication(string scopeName, string appName)
    {
      foreach (ScopeProject scope in _scopes)
      {
        if (scope.Name.ToLower() == scopeName.ToLower())
        {
          foreach (ScopeApplication app in scope.Applications)
          {
            if (app.Name.ToLower() == appName.ToLower())
            {
              string bindingConfigPath =
                string.Format("{0}BindingConfiguration.{1}.{2}.xml",
                _settings["AppDataPath"], scope.Name, app.Name);

              XElement binding = Utility.GetxElementObject(bindingConfigPath);

              if (binding.Element("bind").Attribute("service").Value.ToString().Contains(typeof(ILightweightDataLayer).Name))
                app.DataMode = DataMode.Cache;

              return app;
            }
          }

          break;
        }
      }

      throw new Exception("Application [" + scopeName + "." + appName + "] not found.");
    }
    
    protected void Impersonate()
    {
      if (_settings["AllowImpersonation"] != null &&
        bool.Parse(_settings["AllowImpersonation"].ToString()))
      {
        if (_settings["ImpersonatedUser"] != null)
        {
          _settings["UserName"] = _settings["ImpersonatedUser"];
        }

        if (_settings["ImpersonatedUserDomain"] != null)
        {
          _settings["DomainName"] = _settings["ImpersonatedUserDomain"];
        }
      }
    }

    private void ProcessSettings(string projectName, string applicationName)
    {
      // Load scope settings
      string scopeSettingsPath = String.Format("{0}{1}.config", _settings["AppDataPath"], projectName);

      if (File.Exists(scopeSettingsPath))
      {
        AppSettingsReader scopeSettings = new AppSettingsReader(scopeSettingsPath);

        if (scopeSettings.Contains(CACHE_CONNSTR))
        {
          _settings[CACHE_CONNSTR_LEVEL] = "Scope";
          _settings[CACHE_CONNSTR] = scopeSettings[CACHE_CONNSTR].ToString();
        }

        _settings.AppendSettings(scopeSettings);
      }

      // Load app settings
      string appSettingsPath = (projectName.ToLower() != "all")
        ? string.Format("{0}{1}.{2}.config", _settings["AppDataPath"], projectName, applicationName)
        : string.Format("{0}All.{1}.config", _settings["AppDataPath"], applicationName);
      
      if (File.Exists(appSettingsPath))
      {
        AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
        _settings.AppendSettings(appSettings);
      }

      // Determine whether scope is real or implied (ALL).
      // Also set data mode 
      string scope = string.Format("{0}.{1}", projectName, applicationName);
      bool scopeFound = false;

      foreach (ScopeProject project in _scopes)
      {
        if (project.Name.ToUpper() == projectName.ToUpper())
        {
          foreach (ScopeApplication application in project.Applications)
          {
            if (application.Name.ToUpper() == applicationName.ToUpper())
            {
              _application = application;
              scopeFound = true;
              _settings["DataMode"] = application.DataMode.ToString();
              break;
            }
          }

          break;
        }
      }

      if (!scopeFound)
      {
        scope = String.Format("all.{0}", applicationName);
      }

      _settings["Scope"] = scope;

      string relativePath = String.Format("{0}BindingConfiguration.{1}.xml", _settings["AppDataPath"], scope);

      // Ninject Extension requires fully qualified path.
      string dataLayerBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );

      _settings["BindingConfigurationPath"] = dataLayerBindingPath;

      string dbDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", _settings["AppDataPath"], scope);
      _settings["DBDictionaryPath"] = dbDictionaryPath;
    }

    protected void InitializeScope(string projectName, string applicationName)
    {
        InitializeScope(projectName, applicationName, true);
    }

    protected void InitializeScope(string projectName, string applicationName, bool loadMapping)
    {
      try
      {
        _settings["ProjectName"] = projectName;
        _settings["ApplicationName"] = applicationName;

        string scope = string.Format("{0}.{1}", projectName, applicationName);

        if (!_isScopeInitialized)
        {
          ProcessSettings(projectName, applicationName);

            if (loadMapping)
            {
                string mappingPath;

                //FKM
                if (format.Equals("jsonld"))
                {
                    mappingPath = String.Format("{0}TipMapping.{1}.xml", _settings["AppDataPath"], scope);
                }
                else
                {
                    mappingPath = String.Format("{0}Mapping.{1}.xml", _settings["AppDataPath"], scope);
                }

                if (File.Exists(mappingPath))
                {
                    try
                    {
                        //FKM
                        if (format.Equals("jsonld"))
                        {
                            _tipMapping = Utility.Read<TipMapping>(mappingPath);
                        }
                        else
                        {
                            _mapping = Utility.Read<mapping.Mapping>(mappingPath);
                        }
                    }
                    catch (Exception legacyEx)
                    {
                    _logger.Warn("Error loading mapping file [" + mappingPath + "]:" + legacyEx);
                    Status status = new Status();

                    _mapping = LoadMapping(mappingPath, ref status);
                    _logger.Info(status.ToString());
                    }
                }
                else
                {
                    _mapping = new mapping.Mapping();
                    Utility.Write<mapping.Mapping>(_mapping, mappingPath);
                }

                //FKM
                if (format.Equals("jsonld"))
                {
                    _kernel.Bind<TipMapping>().ToConstant(_tipMapping).InThreadScope();
                }
                else
                {
                    _kernel.Bind<mapping.Mapping>().ToConstant(_mapping).InThreadScope();
                }
                
          }

          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing scope: {0}", ex));
        throw ex;
      }
    }

    #region cache related methods
    public Response SwitchDataMode(string scope, string app, string mode)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app, false);

        ScopeProject proj = _scopes.Find(x => x.Name.ToLower() == scope.ToLower());
        ScopeApplication application = proj.Applications.Find(x => x.Name.ToLower() == app.ToLower());

        application.DataMode = (DataMode)Enum.Parse(typeof(DataMode), mode);

        string scopesPath = String.Format("{0}Scopes.xml", _settings["AppDataPath"]);
        Utility.Write<ScopeProjects>(_scopes, scopesPath);

        response.Level = StatusLevel.Success;
        response.Messages.Add("Data Mode switched successfully.");
      }
      catch (Exception ex)
      {
        _logger.Debug("Error switching data mode: ", ex);
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error switching data mode: " + ex.Message);
      }

      return response;
    }

    public Response RefreshCache(string scope, string app, bool updateDictionary)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app, false);
        Impersonate();
        InitializeDataLayer(false);

        response = _dataLayerGateway.RefreshCache(updateDictionary);

        if (response.Level == StatusLevel.Success)
        {
          UpdateCacheInfo(scope, app, null);
        }
      }
      catch (Exception ex)
      {
        _logger.Debug("Error refreshing cache: ", ex);
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error refreshing cache: " + ex.Message);
      }

      return response;
    }

    public Response RefreshCache(string scope, string app, string objectType, bool updateDictionary)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app, false);
        Impersonate();
        InitializeDataLayer(false);

        response = _dataLayerGateway.RefreshCache(updateDictionary, objectType, true);

        if (response.Level == StatusLevel.Success)
        {
          UpdateCacheInfo(scope, app, objectType);
        }
      }
      catch (Exception ex)
      {
        _logger.Debug("Error refreshing cache: ", ex);
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error refreshing cache: " + ex.Message);
      }

      return response;
    }

    public Response ImportCache(string scope, string app, string baseUri, bool updateDictionary)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app, false);
        Impersonate();
        InitializeDataLayer(false);

        response = _dataLayerGateway.ImportCache(baseUri, updateDictionary);

        if (response.Level == StatusLevel.Success)
        {
          UpdateCacheInfo(scope, app, null);
        }
      }
      catch (Exception ex)
      {
        _logger.Debug("Error importing cache: ", ex);
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error importing cache: " + ex.Message);
      }

      return response;
    }

    public Response ImportCache(string scope, string app, string objectType, string url, bool updateDictionary)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app, false);
        Impersonate();
        InitializeDataLayer(false);

        response = _dataLayerGateway.ImportCache(objectType, url, updateDictionary, true);

        if (response.Level == StatusLevel.Success)
        {
          UpdateCacheInfo(scope, app, objectType);
        }
      }
      catch (Exception ex)
      {
        _logger.Debug("Error importing cache: ", ex);
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error importing cache: " + ex.Message);
      }

      return response;
    }

    public Response DeleteCache(string scope, string app)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app, false);
        InitializeDataLayer(false);

        response = _dataLayerGateway.DeleteCache();
      }
      catch (Exception ex)
      {
        _logger.Debug("Error deleting cache: ", ex);
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error deleting cache: " + ex.Message);
      }

      return response;
    }

    public Response DeleteCache(string scope, string app, string objectType)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app, false);
        InitializeDataLayer(false);

        response = _dataLayerGateway.DeleteCache(objectType);
      }
      catch (Exception ex)
      {
        _logger.Debug("Error deleting cache: ", ex);
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error deleting cache: " + ex.Message);
      }

      return response;
    }

    protected void UpdateCacheInfo(ScopeApplication application, DataObject dataObject)
    {
      if (dataObject == null)
      {
        throw new Exception("Object type [" + dataObject.objectName + "] not known.");
      }

      if (application.CacheInfo == null)
      {
        application.CacheInfo = new CacheInfo();
      }

      if (application.CacheInfo.CacheEntries == null)
      {
        application.CacheInfo.CacheEntries = new CacheEntries();
      }

      CacheEntry cacheEntry = application.CacheInfo.CacheEntries.Find(x => x.ObjectName.ToLower() == dataObject.objectName.ToLower());

      if (cacheEntry == null)
      {
        cacheEntry = new CacheEntry()
        {
          ObjectName = dataObject.objectName,
          LastUpdate = DateTime.Now
        };

        application.CacheInfo.CacheEntries.Add(cacheEntry);
      }
      else
      {
        cacheEntry.LastUpdate = DateTime.Now;
      }
    }

    public void UpdateCacheInfo(string scope, string app, string objectType)
    {
      try
      {
        ScopeProject project = _scopes.Find(x => x.Name.ToLower() == scope.ToLower());
        ScopeApplication application = project.Applications.Find(x => x.Name.ToLower() == app.ToLower());
        DataDictionary dictionary = _dataLayerGateway.GetDictionary();

        if (string.IsNullOrEmpty(objectType))
        {
          foreach (DataObject dataObject in dictionary.dataObjects)
          {
            UpdateCacheInfo(application, dataObject);
          }
        }
        else
        {
          DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
          UpdateCacheInfo(application, dataObject);
        }

        Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
      }
      catch (Exception e)
      {
        _logger.Debug("Error updating cache information: ", e);
        throw e;
      }
    }
    #endregion

    #region Convert old mapping
    protected mapping.Mapping LoadMapping(string path, ref Status status)
    {
      XElement mappingXml = Utility.ReadXml(path);

      return LoadMapping(path, mappingXml, ref status);
    }

    protected mapping.Mapping LoadMapping(string path, XElement mappingXml, ref Status status)
    {
      mapping.Mapping mapping = null;

      if (mappingXml.Name.NamespaceName.Contains("schemas.datacontract.org"))
      {
        status.Messages.Add("Detected legacy mapping. Attempting to convert it...");

        try
        {
          org.iringtools.legacy.Mapping legacyMapping = null;

          legacyMapping = Utility.DeserializeDataContract<legacy.Mapping>(mappingXml.ToString());

          mapping = ConvertMapping(legacyMapping);
        }
        catch (Exception legacyEx)
        {
          status.Messages.Add("Error loading legacy mapping file: " + legacyEx + ". Attempting to convert...");

          try
          {
            mapping = ConvertMapping(mappingXml);
          }
          catch (Exception oldEx)
          {
            status.Messages.Add("Legacy mapping could not be converted." + oldEx);
          }
        }

        if (mapping != null)
        {
          //
          // Write new mapping to disk
          //
          Utility.Write<mapping.Mapping>(mapping, path, true);
          status.Messages.Add("Legacy mapping has been converted sucessfully.");
        }
      }
      else
      {
        mapping = Utility.DeserializeDataContract<mapping.Mapping>(mappingXml.ToString());
      }
      
      return mapping;
    }

    private mapping.Mapping ConvertMapping(XElement mappingXml)
    {
      mapping.Mapping mapping = new mapping.Mapping();
      _qmxfTemplateResultCache = new Dictionary<string, KeyValuePair<string, Dictionary<string, string>>>();

      #region convert graphMaps
      IEnumerable<XElement> graphMaps = mappingXml.Element("GraphMaps").Elements("GraphMap");
      foreach (XElement graphMap in graphMaps)
      {
        string dataObjectName = graphMap.Element("DataObjectMaps").Element("DataObjectMap").Attribute("name").Value;
        mapping.RoleMap roleMap = null;

        mapping.GraphMap newGraphMap = new mapping.GraphMap();
        newGraphMap.name = graphMap.Attribute("Name").Value;
        newGraphMap.dataObjectName = dataObjectName;
        mapping.graphMaps.Add(newGraphMap);

        ConvertClassMap(ref newGraphMap, ref roleMap, graphMap, dataObjectName);
      }
      #endregion

      #region convert valueMaps
      IEnumerable<XElement> valueMaps = mappingXml.Element("ValueMaps").Elements("ValueMap");
      string previousValueList = String.Empty;
      ValueListMap newValueList = null;

      foreach (XElement valueMap in valueMaps)
      {
        string valueList = valueMap.Attribute("valueList").Value;
        mapping.ValueMap newValueMap = new mapping.ValueMap
        {
          internalValue = valueMap.Attribute("internalValue").Value,
          uri = valueMap.Attribute("modelURI").Value
        };

        if (valueList != previousValueList)
        {
          newValueList = new ValueListMap
          {
            name = valueList,
            valueMaps = { newValueMap }
          };
          mapping.valueListMaps.Add(newValueList);

          previousValueList = valueList;
        }
        else
        {
          newValueList.valueMaps.Add(newValueMap);
        }
      }
      #endregion

      return mapping;
    }

    private void ConvertClassMap(ref mapping.GraphMap newGraphMap, ref mapping.RoleMap parentRoleMap, XElement classMap, string dataObjectName)
    {
      string classId = classMap.Attribute("classId").Value;

      mapping.ClassMap newClassMap = new mapping.ClassMap();
      newClassMap.id = classId;
      newClassMap.identifiers.Add(dataObjectName + "." + classMap.Attribute("identifier").Value);

      if (parentRoleMap == null)
      {
        newClassMap.name = GetClassName(classId);
      }
      else
      {
        newClassMap.name = classMap.Attribute("name").Value;
        parentRoleMap.classMap = newClassMap;
      }

      ClassTemplateMap newTemplateMaps = new ClassTemplateMap();
      newGraphMap.classTemplateMaps.Add(newTemplateMaps);

      IEnumerable<XElement> templateMaps = classMap.Element("TemplateMaps").Elements("TemplateMap");
      KeyValuePair<string, Dictionary<string, string>> templateNameRolesPair;

      foreach (XElement templateMap in templateMaps)
      {
        string classRoleId = String.Empty;

        try
        {
          classRoleId = templateMap.Attribute("classRole").Value;
        }
        catch (Exception)
        {
          continue;  // Class role not found, skip this template
        }

        IEnumerable<XElement> roleMaps = templateMap.Element("RoleMaps").Elements("RoleMap");
        string templateId = templateMap.Attribute("templateId").Value;

        mapping.TemplateMap newTemplateMap = new mapping.TemplateMap();
        newTemplateMap.id = templateId;
        newTemplateMaps.templateMaps.Add(newTemplateMap);

        if (_qmxfTemplateResultCache.ContainsKey(templateId))
        {
          templateNameRolesPair = _qmxfTemplateResultCache[templateId];
        }
        else
        {
          templateNameRolesPair = GetQmxfTemplateRolesPair(templateId);
          _qmxfTemplateResultCache[templateId] = templateNameRolesPair;
        }

        newTemplateMap.name = templateNameRolesPair.Key;

        mapping.RoleMap newClassRoleMap = new mapping.RoleMap();
        newClassRoleMap.type = mapping.RoleType.Possessor;
        newTemplateMap.roleMaps.Add(newClassRoleMap);
        newClassRoleMap.id = classRoleId;

        Dictionary<string, string> roles = templateNameRolesPair.Value;
        newClassRoleMap.name = roles[classRoleId];

        for (int i = 0; i < roleMaps.Count(); i++)
        {
          XElement roleMap = roleMaps.ElementAt(i);

          string value = String.Empty;
          try { value = roleMap.Attribute("value").Value; }
          catch (Exception ex)
          {
            _logger.Error("Error in ConvertClassMap: " + ex);
          }

          string reference = String.Empty;
          try { reference = roleMap.Attribute("reference").Value; }
          catch (Exception ex) { _logger.Error("Error in GetSection: " + ex); }

          string propertyName = String.Empty;
          try { propertyName = roleMap.Attribute("propertyName").Value; }
          catch (Exception ex) { _logger.Error("Error in ConvertClassMap: " + ex); }

          string valueList = String.Empty;
          try { valueList = roleMap.Attribute("valueList").Value; }
          catch (Exception ex) { _logger.Error("Error in ConvertClassMap: " + ex); }

          mapping.RoleMap newRoleMap = new mapping.RoleMap();
          newTemplateMap.roleMaps.Add(newRoleMap);
          newRoleMap.id = roleMap.Attribute("roleId").Value;
          newRoleMap.name = roles[newRoleMap.id];

          if (!String.IsNullOrEmpty(value))
          {
            newRoleMap.type = mapping.RoleType.FixedValue;
            newRoleMap.value = value;
          }
          else if (!String.IsNullOrEmpty(reference))
          {
            newRoleMap.type = mapping.RoleType.Reference;
            newRoleMap.value = reference;
          }
          else if (!String.IsNullOrEmpty(propertyName))
          {
            newRoleMap.propertyName = dataObjectName + "." + propertyName;

            if (!String.IsNullOrEmpty(valueList))
            {
              newRoleMap.type = mapping.RoleType.ObjectProperty;
              newRoleMap.valueListName = valueList;
            }
            else
            {
              newRoleMap.type = mapping.RoleType.DataProperty;
              newRoleMap.dataType = roleMap.Attribute("dataType").Value;
            }
          }

          if (roleMap.HasElements)
          {
            newRoleMap.type = mapping.RoleType.Reference;
            newRoleMap.value = roleMap.Attribute("dataType").Value;

            ConvertClassMap(ref newGraphMap, ref newRoleMap, roleMap.Element("ClassMap"), dataObjectName);
          }
        }
      }
    }

    private string GetClassName(string classId)
    {
      QMXF qmxf = _webHttpClient.Get<QMXF>("/classes/" + classId.Substring(classId.IndexOf(":") + 1), false);
      return qmxf.classDefinitions.First().name.First().value;
    }

    private KeyValuePair<string, Dictionary<string, string>> GetQmxfTemplateRolesPair(string templateId)
    {
      string templateName = String.Empty;
      Dictionary<string, string> roleIdNames = new Dictionary<string, string>();

      QMXF qmxf = _webHttpClient.Get<QMXF>("/templates/" + templateId.Substring(templateId.IndexOf(":") + 1), false);

      if (qmxf.templateDefinitions.Count > 0)
      {
        TemplateDefinition tplDef = qmxf.templateDefinitions.First();
        templateName = tplDef.name.First().value;

        foreach (RoleDefinition roleDef in tplDef.roleDefinition)
        {
          roleIdNames.Add(roleDef.identifier.Replace("http://tpl.rdlfacade.org/data#", "tpl:"), roleDef.name.First().value);
        }
      }
      else if (qmxf.templateQualifications.Count > 0)
      {
        TemplateQualification tplQual = qmxf.templateQualifications.First();
        templateName = tplQual.name.First().value;

        foreach (RoleQualification roleQual in tplQual.roleQualification)
        {
          roleIdNames.Add(roleQual.qualifies.Replace("http://tpl.rdlfacade.org/data#", "tpl:"), roleQual.name.First().value);
        }
      }

      return new KeyValuePair<string, Dictionary<string, string>>(templateName, roleIdNames);
    }

    private mapping.Mapping ConvertMapping(legacy.Mapping legacyMapping)
    {
      mapping.Mapping mapping = new mapping.Mapping();
      _qmxfTemplateResultCache = new Dictionary<string, KeyValuePair<string, Dictionary<string, string>>>();

      #region convert graphMaps
      IList<legacy.GraphMap> graphMaps = legacyMapping.graphMaps;
      foreach (legacy.GraphMap graphMap in graphMaps)
      {
        string dataObjectName = graphMap.dataObjectMap;
        mapping.RoleMap roleMap = null;

        mapping.GraphMap newGraphMap = new mapping.GraphMap();
        newGraphMap.name = graphMap.name;
        newGraphMap.dataObjectName = dataObjectName;
        mapping.graphMaps.Add(newGraphMap);

        ConvertGraphMap(ref newGraphMap, ref roleMap, graphMap, dataObjectName);
      }
      #endregion

      #region convert valueMaps
      IList<legacy.ValueList> valueLists = legacyMapping.valueLists;

      foreach (legacy.ValueList valueList in valueLists)
      {
        string valueListName = valueList.name;

        ValueListMap newValueList = new ValueListMap
        {
          name = valueList.name,
          valueMaps = new ValueMaps()
        };
        mapping.valueListMaps.Add(newValueList);

        foreach (legacy.ValueMap valueMap in valueList.valueMaps)
        {
          mapping.ValueMap newValueMap = new mapping.ValueMap
          {
            internalValue = valueMap.internalValue,
            uri = valueMap.uri
          };

          newValueList.valueMaps.Add(newValueMap);
        }
      }
      #endregion

      return mapping;
    }

    private void ConvertGraphMap(ref mapping.GraphMap newGraphMap, ref mapping.RoleMap parentRoleMap, legacy.GraphMap graphMap, string dataObjectName)
    {
      foreach (var classTemplateListMap in graphMap.classTemplateListMaps)
      {
        ClassTemplateMap classTemplateMap = new ClassTemplateMap();

        legacy.ClassMap legacyClassMap = classTemplateListMap.Key;

        org.iringtools.mapping.Identifiers identifiers = new org.iringtools.mapping.Identifiers();

        foreach (string identifier in legacyClassMap.identifiers)
        {
          identifiers.Add(identifier);
        }

        mapping.ClassMap newClassMap = new mapping.ClassMap
        {
          id = legacyClassMap.classId,
          identifierDelimiter = legacyClassMap.identifierDelimiter,
          identifiers = identifiers,
          identifierValue = legacyClassMap.identifierValue,
          name = legacyClassMap.name
        };

        classTemplateMap.classMap = newClassMap;

        TemplateMaps templateMaps = new TemplateMaps();
        foreach (legacy.TemplateMap templateMap in classTemplateListMap.Value)
        {
          mapping.TemplateType templateType = mapping.TemplateType.Definition;
          Enum.TryParse<mapping.TemplateType>(templateMap.templateType.ToString(), out templateType);

          mapping.TemplateMap newTemplateMap = new mapping.TemplateMap
          {
            id = templateMap.templateId,
            name = templateMap.name,
            type = templateType,
            roleMaps = new RoleMaps(),
          };

          foreach (legacy.RoleMap roleMap in templateMap.roleMaps)
          {
            mapping.RoleType roleType = mapping.RoleType.DataProperty;
            Enum.TryParse<mapping.RoleType>(roleMap.type.ToString(), out roleType);

            newClassMap = null;
            if (roleMap.classMap != null)
            {
              identifiers = new mapping.Identifiers();

              foreach (string identifier in roleMap.classMap.identifiers)
              {
                identifiers.Add(identifier);
              }

              newClassMap = new mapping.ClassMap
              {
                id = roleMap.classMap.classId,
                identifierDelimiter = roleMap.classMap.identifierDelimiter,
                identifiers = identifiers,
                identifierValue = roleMap.classMap.identifierValue,
                name = roleMap.classMap.name
              };
            }

            mapping.RoleMap newRoleMap = new mapping.RoleMap
            {
              id = roleMap.roleId,
              name = roleMap.name,
              type = roleType,
              classMap = newClassMap,
              dataType = roleMap.dataType,
              propertyName = roleMap.propertyName,
              value = roleMap.value,
              valueListName = roleMap.valueList
            };

            newTemplateMap.roleMaps.Add(newRoleMap);
          }

          templateMaps.Add(newTemplateMap);
        }

        classTemplateMap.templateMaps = templateMaps;

        if (classTemplateMap.classMap != null)
        {
          newGraphMap.classTemplateMaps.Add(classTemplateMap);
        }
      }
    }
    #endregion Convert old mapping

    protected static string NewQueueRequest()
    {
      var id = Guid.NewGuid().ToString("N");
      _requests[id] = new RequestStatus()
      {
        State = State.InProgress
      };
      return id;
    }

    public RequestStatus GetRequestStatus(string id)
    {
      try
      {
        RequestStatus status = null;

        if (_requests.ContainsKey(id))
        {
          status = _requests[id];
        }
        else
        {
          status = new RequestStatus()
          {
            State = State.NotFound,
            Message = "Request [" + id + "] not found."
          };
        }

        if (status.State == State.Completed)
        {
          _requests.TryRemove(id, out status);
        }

        return status;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error getting request status: {0}", ex));
        throw ex;
      }
    }
  }

  public enum PostAction { Create, Update }

  public class ScopeComparer : IComparer<ScopeProject>
  {
      public int Compare(ScopeProject left, ScopeProject right)
      {
          // compare strings
          {
              string leftValue = left.DisplayName.ToLower();
              string rightValue = right.DisplayName.ToLower();
              return string.Compare(leftValue, rightValue);
          }
      }
  }

  public class ApplicationComparer : IComparer<ScopeApplication>
  {
      public int Compare(ScopeApplication left, ScopeApplication right)
      {
          // compare strings
          {
              string leftValue = left.DisplayName.ToLower();
              string rightValue = right.DisplayName.ToLower();
              return string.Compare(leftValue, rightValue);
          }
      }
  }

  public class DataObjectComparer : IComparer<IDataObject>
  {
      private DataProperty _dataProp;

      public DataObjectComparer(DataProperty dataProp)
      {
          _dataProp = dataProp;
      }

      public int Compare(IDataObject left, IDataObject right)
      {
          // compare booleans
          if (_dataProp.dataType == DataType.Boolean)
          {
              int leftValue = (int)left.GetPropertyValue(_dataProp.propertyName);
              int rightValue = (int)right.GetPropertyValue(_dataProp.propertyName);

              if (leftValue > rightValue)
                  return 1;

              if (rightValue > leftValue)
                  return -1;

              return 0;
          }

          // compare numerics
          if (_dataProp.dataType == DataType.Byte ||
            _dataProp.dataType == DataType.Decimal ||
            _dataProp.dataType == DataType.Double ||
            _dataProp.dataType == DataType.Int16 ||
            _dataProp.dataType == DataType.Int32 ||
            _dataProp.dataType == DataType.Int64 ||
            _dataProp.dataType == DataType.Single)
          {
              decimal leftValue = (decimal)left.GetPropertyValue(_dataProp.propertyName);
              decimal rightValue = (decimal)right.GetPropertyValue(_dataProp.propertyName);

              if (leftValue > rightValue)
                  return 1;

              if (rightValue > leftValue)
                  return -1;

              return 0;
          }

          // compare date times
          if (_dataProp.dataType == DataType.DateTime)
          {
              DateTime leftValue = (DateTime)left.GetPropertyValue(_dataProp.propertyName);
              DateTime rightValue = (DateTime)right.GetPropertyValue(_dataProp.propertyName);

              return DateTime.Compare(leftValue, rightValue);
          }

          // compare strings
          {
              string leftValue = Convert.ToString(left.GetPropertyValue(_dataProp.propertyName));
              string rightValue = Convert.ToString(right.GetPropertyValue(_dataProp.propertyName));
              return string.Compare(leftValue, rightValue);
          }
      }
  }
}
