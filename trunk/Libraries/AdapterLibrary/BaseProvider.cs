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

namespace org.iringtools.adapter
{
  public abstract class BaseProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseProvider));

    protected IKernel _kernel = null;
    protected AdapterSettings _settings = null;
    protected ScopeProjects _scopes = null;
    protected ScopeApplication _application = null;
    protected DataLayerGateway _dataLayerGateway = null;
    protected DataDictionary _dictionary = null;
    protected mapping.Mapping _mapping = null;
    protected bool _isScopeInitialized = false;
    protected bool _isDataLayerInitialized = false;
    protected static ConcurrentDictionary<string, RequestStatus> _requests =
     new ConcurrentDictionary<string, RequestStatus>();

    protected Dictionary<string, KeyValuePair<string, Dictionary<string, string>>> _qmxfTemplateResultCache = null;
    protected WebHttpClient _webHttpClient = null;  // for old mapping conversion

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
      }
      else
      {
        _scopes = new ScopeProjects();
        Utility.Write<ScopeProjects>(_scopes, scopesPath);
      }

      string relativePath = String.Format("{0}BindingConfiguration.Adapter.xml", _settings["AppDataPath"]);

      // Ninject Extension requires fully qualified path.
      string adapterBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );

      _kernel.Load(adapterBindingPath);
      
      InitializeIdentity();
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

        _settings.AppendKeyRing(keyRing);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing identity: {0}", ex));
        throw ex;
      }
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
      // Load app settings
      string scopeSettingsPath = String.Format("{0}{1}.{2}.config", _settings["AppDataPath"], projectName, applicationName);

      if (File.Exists(scopeSettingsPath))
      {
        AppSettingsReader scopeSettings = new AppSettingsReader(scopeSettingsPath);
        _settings.AppendSettings(scopeSettings);
      }

      if (projectName.ToLower() != "all")
      {
        string appSettingsPath = String.Format("{0}All.{1}.config", _settings["AppDataPath"], applicationName);

        if (File.Exists(appSettingsPath))
        {
          AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
          _settings.AppendSettings(appSettings);
        }
      }

      // Determine whether scope is real or implied (ALL)
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
            string mappingPath = String.Format("{0}Mapping.{1}.xml", _settings["AppDataPath"], scope);

            if (File.Exists(mappingPath))
            {
              try
              {
                _mapping = Utility.Read<mapping.Mapping>(mappingPath);
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

            _kernel.Bind<mapping.Mapping>().ToConstant(_mapping);
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

        Identifiers identifiers = new Identifiers();

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
              identifiers = new Identifiers();

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
}
