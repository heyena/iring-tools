// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL + exEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
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

namespace org.iringtools.adapter
{
  public class AdapterProvider : BaseProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
    private static readonly int DEFAULT_PAGE_SIZE = 25;

    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private ScopeProjects _scopes = null;
    private ScopeApplication _application = null;
    private IDataLayer2 _dataLayer = null;
    private IIdentityLayer _identityLayer = null;
    private IDictionary _keyRing = null;
    private ISemanticLayer _semanticEngine = null;
    private IProjectionLayer _projectionEngine = null;
    private DataDictionary _dataDictionary = null;
    private mapping.Mapping _mapping = null;
    private mapping.GraphMap _graphMap = null;
    private DataObject _dataObjDef = null;
    private bool _isResourceGraph = false;
    private bool _isProjectionPart7 = false;
    private bool _isFormatExpected = true;

    //Projection specific stuff
    private IList<IDataObject> _dataObjects = new List<IDataObject>(); // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = new Dictionary<string, List<string>>(); // dictionary of class ids and list of identifiers

    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;
    private string[] arrSpecialcharlist;
    private string[] arrSpecialcharValue;

    private string _dataLayersRegistryPath;
    private string _dataLayerPath;

    [Inject]
    public AdapterProvider(NameValueCollection settings)
    {
      //TODO: Pending on testing, do not delete
      //AppDomain currentDomain = AppDomain.CurrentDomain;
      //currentDomain.AssemblyResolve += new ResolveEventHandler(DataLayerAssemblyResolveEventHandler);
      
      var ninjectSettings = new NinjectSettings { LoadExtensions = false, UseReflectionBasedInjection = true };
      _kernel = new StandardKernel(ninjectSettings, new AdapterModule());

      _kernel.Load(new XmlExtensionModule());
      _settings = _kernel.Get<AdapterSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string rdsUri = _settings["ReferenceDataServiceUri"];

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

        foreach (ScopeProject proj in _scopes)
        {
          foreach (ScopeApplication app in proj.Applications)
          {
            string configPath = String.Format("{0}{1}.{2}.config", _settings["AppDataPath"], proj.Name, app.Name);

            if (File.Exists(configPath))
            {
              Configuration config = Utility.Read<Configuration>(configPath, false);
              app.Configuration = config;
            }
          }
        }
      }
      else
      {
        _scopes = new ScopeProjects();
        Utility.Write<ScopeProjects>(_scopes, scopesPath);
      }

      string relativePath = String.Format("{0}BindingConfiguration.Adapter.xml", _settings["AppDataPath"]);

      //Ninject Extension requires fully qualified path.
      string bindingConfigurationPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );

      _kernel.Load(bindingConfigurationPath);

      _dataLayersRegistryPath = string.Format("{0}DataLayersRegistry.xml", _settings["AppDataPath"]);
      if (!Directory.Exists(_settings["DataLayersPath"]))
      {
        Directory.CreateDirectory(_settings["DataLayersPath"]);
      }

      InitializeIdentity();

      if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
      {
        arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
        arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
      }
    }

    #region application methods
    public ScopeProjects GetScopes()
    {
      try
      {
        return _scopes;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetScopes: {0}", ex));
        throw new Exception(string.Format("Error getting the list of scopes: {0}", ex));
      }
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
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        ScopeProject sc = _scopes.Find(x => x.Name.ToLower() == scope.Name.ToLower());

        if (sc == null)
        {
          _scopes.Add(scope);
          Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
          status.Messages.Add(String.Format("Scope [{0}] updated successfully.", scope.Name));
        }
        else
        {
          status.Level = StatusLevel.Error;
          status.Messages.Add(String.Format("Scope [{0}] already exists.", scope.Name));
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

    public Response UpdateScope(string scopeName, ScopeProject scope)
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
          status.Messages.Add(String.Format("Scope [{0}] does not exist.", scope.Name));
        }
        else
        {
          //
          // add new scope and move applications in the existing scope to the new one
          //
          AddScope(scope);

          if (sc.Applications != null)
          {
            foreach (ScopeApplication app in sc.Applications)
            {
              //
              // copy database dictionary
              //
              string path = _settings["AppDataPath"];
              string currDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, scopeName, app.Name);

              if (File.Exists(currDictionaryPath))
              {
                string updatedDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, scope.Name, app.Name);
                File.Copy(currDictionaryPath, updatedDictionaryPath);
              }

              AddApplication(scope.Name, app);
            }
          }

          // delete old scope
          DeleteScope(scopeName);

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
              }

              authorizationBinding.Save(String.Format("{0}AuthorizationBindingConfiguration.{1}.{2}.xml",
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

          summaryBinding.Save(String.Format("{0}SummaryBindingConfiguration.{1}.{2}.xml",
              _settings["AppDataPath"], scope.Name, application.Name));

          //
          // update data layer binding
          //
          if (!String.IsNullOrEmpty(application.Assembly))
          {
            XElement dataLayerBinding = new XElement("module",
              new XAttribute("name", "DataLayerBinding" + "." + scope.Name + "." + application.Name),
              new XElement("bind",
                new XAttribute("name", "DataLayer"),
                new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
                new XAttribute("to", application.Assembly)
              )
            );

            dataLayerBinding.Save(String.Format("{0}BindingConfiguration.{1}.{2}.xml",
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
            Utility.Write<Configuration>(config,configPath,false);
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

        scope.Applications.Add(application);
        Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);

        response.Append(Generate(scope.Name, application.Name));
        status.Messages.Add("Application [{0}.{1}] updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, application.Name, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, application.Name, ex));
      }

      return response;
    }

    public Response UpdateApplication(string scopeName, string appName, ScopeApplication updatedApp)
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

        ScopeApplication application = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name.ToLower() == appName.ToLower());

        if (application != null)  // application exists, delete and re-create it
        {
          //
          // copy database dictionary
          //
          string path = _settings["AppDataPath"];
          string currDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, scopeName, appName);

          if (File.Exists(currDictionaryPath))
          {
            string updatedDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, scopeName, updatedApp.Name);
            if (currDictionaryPath.ToLower() != updatedDictionaryPath.ToLower())
              File.Copy(currDictionaryPath, updatedDictionaryPath);
          }

          DeleteApplication(scopeName, appName);
          AddApplication(scopeName, updatedApp);
        }
        else  // application does not exist, stop processing
        {
          throw new Exception(String.Format("Application [{0}.{1}] not found.", scopeName, appName));
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

      string authorizationBindingPath = String.Format("{0}AuthorizationBindingConfiguration.{1}.xml", path, context);
      if (File.Exists(authorizationBindingPath))
      {
        File.Delete(authorizationBindingPath);
      }

      string bindingConfigurationPath = String.Format("{0}BindingConfiguration.{1}.xml", path, context);
      if (File.Exists(bindingConfigurationPath))
      {
        File.Delete(bindingConfigurationPath);
      }

      string databaseDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", path, context);
      if (File.Exists(databaseDictionaryPath))
      {
        File.Delete(databaseDictionaryPath);
      }

      string dataDictionaryPath = String.Format("{0}DataDictionary.{1}.xml", path, context);
      if (File.Exists(dataDictionaryPath))
      {
        File.Delete(dataDictionaryPath);
      }

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
        XElement binding = XElement.Load(bindingPath);

        if (binding.Element("bind").Attribute("to").Value.Contains(typeof(NHibernateDataLayer).Name))
        {
          string dbDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", path, context);
          DatabaseDictionary dbDictionary = null;

          if (File.Exists(dbDictionaryPath))
          {
            dbDictionary = NHibernateUtility.LoadDatabaseDictionary(dbDictionaryPath);
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

    public XElement GetBinding(string projectName, string applicationName)
    {
      XElement binding = null;

      try
      {
        InitializeScope(projectName, applicationName);

        binding = XElement.Load(_settings["BindingConfigurationPath"]);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateBindingConfiguration: {0}", ex));
        throw ex;
      }
      return binding;
    }

    public Response UpdateBinding(string projectName, string applicationName, XElement binding)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        XDocument bindingConfiguration = new XDocument();
        bindingConfiguration.Add(binding);

        bindingConfiguration.Save(_settings["BindingConfigurationPath"]);

        status.Messages.Add("BindingConfiguration of [" + projectName + "." + applicationName + "] updated successfully.");
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
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _kernel.TryGet<DataDictionary>();

      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetDictionary: {0}", ex));
        throw new Exception(string.Format("Error getting data dictionary: {0}", ex));
      }
    }

    public Contexts GetContexts(string applicationName)
    {
      try
      {
        Contexts contexts = new Contexts();

        foreach (ScopeProject scope in _scopes)
        {
          if (scope.Name.ToLower() != "all")
          {
            var app = scope.Applications.Find(a => a.Name.ToUpper() == applicationName.ToUpper());

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
        _logger.Error(string.Format("Error in GetContexts for {0}: {1}", applicationName, ex));
        throw new Exception(string.Format("Error in GetContexts for {0}: {1}", applicationName, ex));
      }
    }

    public WADLApplication GetWADL(string projectName, string applicationName)
    {
      WADLApplication wadl = new WADLApplication();

      try
      {
        bool isAll = projectName == "all";
        bool isApp = projectName == "app";
        if (isApp)
        {
          //get thie first context and initialize everything
          Context context = GetContexts(applicationName).FirstOrDefault();

          if (context == null)
            throw new WebFaultException(HttpStatusCode.NotFound);

          projectName = context.Name;
        }
        InitializeScope(projectName, applicationName);
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

        string appBaseUri = Utility.FormAppBaseURI(_uriMaps, baseUri, applicationName);
        string baseResource = String.Empty;

        if (!isApp && !isAll)
        {
          appBaseUri = appBaseUri + "/" + projectName;
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
          title = applicationName;

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

        if (_dataDictionary.enableSummary)
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

        foreach (DataObject dataObject in _dataDictionary.dataObjects)
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

            if (_dataDictionary.enableSearch)
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

    public mapping.Mapping GetMapping(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);

        return _mapping;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetMapping: {0}", ex));
        throw new Exception(string.Format("Error getting mapping: {0}", ex));
      }
    }

    public Response UpdateMapping(string projectName, string applicationName, XElement mappingXml)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings["AppDataPath"], projectName, applicationName);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        mapping.Mapping mapping = LoadMapping(path, mappingXml, ref status);

        Utility.Write<mapping.Mapping>(mapping, path, true);

        status.Messages.Add("Mapping of [" + projectName + "." + applicationName + "] updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateMapping: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error saving mapping file to path [{0}]: {1}", path, ex));
      }

      return response;
    }

    public Response Refresh(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        response.Append(Refresh(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error refreshing graph [{0}]: {1}", graphName, ex));
      }

      return response;
    }

    //DataFilter List
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName,
        DataFilter filter, ref string format, int start, int limit, bool fullIndex)
    {
      try
      {
        DataDictionary dictionary = GetDictionary(projectName, applicationName);
        DataObject dataObject = dictionary.GetDataObject(resourceName);

        if (dataObject != null)
          filter.AppendFilter(dataObject.dataFilter);

        _logger.DebugFormat("Initializing Scope: {0}.{1}", projectName, applicationName);
        InitializeScope(projectName, applicationName);
        _logger.Debug("Initializing DataLayer.");
        InitializeDataLayer();
        _logger.DebugFormat("Initializing Projection: {0} as {1}", resourceName, format);
        InitializeProjection(resourceName, ref format, false);

        _projectionEngine.Start = start;
        _projectionEngine.Limit = limit;

        IList<string> index = new List<string>();

        if (limit == 0)
        {
          limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
        }

        _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
        _dataObjects = _dataLayer.Get(_dataObjDef.objectName, filter, limit, start);
        _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, filter);
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

    /*
     * Sample filter with rollups:
     * 
    <?xml version="1.0" encoding="utf-8"?>
    <dataFilter xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://www.iringtools.org/data/filter">
      <rollupExpressions>
        <rollupExpression>
          <groupBy>ID</groupBy>
          <rollups>
            <rollup>
              <propertyName>NOMDIAMETER</propertyName>
              <type>Max</type>
            </rollup>
            <rollup>
              <propertyName>AREA</propertyName>
              <type>First</type>
            </rollup>
          </rollups>
        </rollupExpression>
        <rollupExpression>
          <groupBy>AREA</groupBy>
          <rollups>
            <rollup>
              <propertyName>NOMDIAMETER</propertyName>
              <type>Sum</type>
            </rollup>
          </rollups>
        </rollupExpression>
      </rollupExpressions>
    </dataFilter>
     */

    public XDocument GetDataProjectionWithRollups(
          string projectName, string applicationName, string resourceName,
            DataFilter filter, ref string format, int start, int limit, bool fullIndex)
    {
      try
      {
        DataDictionary dictionary = GetDictionary(projectName, applicationName);
        DataObject objDef = dictionary.GetDataObject(resourceName);

        if (objDef != null)
          filter.AppendFilter(objDef.dataFilter);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, ref format, false);

        // get all data objects to memory
        _dataObjects = PageDataObjects(_dataObjDef.objectName, filter);

        #region process rollups
        IDataObject[] rollupDataObjects = null;

        foreach (RollupExpression rollupExpr in filter.RollupExpressions)
        {
          // apply group-by
          DataProperty dataProp = objDef.dataProperties.Find(x => x.propertyName.ToLower() == rollupExpr.GroupBy.ToLower());
          List<IDataObject> sortedDataObjects = _dataObjects.ToList<IDataObject>();

          sortedDataObjects.Sort(new DataObjectComparer(dataProp));

          List<List<IDataObject>> dataObjectGroups = new List<List<IDataObject>>();
          List<IDataObject> dataObjectGroup = null;
          string prevPropValue = null;

          foreach (IDataObject dataObject in sortedDataObjects)
          {
            string propValue = Convert.ToString(dataObject.GetPropertyValue(dataProp.propertyName));

            if (propValue != prevPropValue)
            {
              dataObjectGroup = new List<IDataObject>();
              dataObjectGroups.Add(dataObjectGroup);
              prevPropValue = propValue;
            }

            if (dataObjectGroup != null)
            {
              dataObjectGroup.Add(dataObject);
            }
          }

          sortedDataObjects = null;

          // apply rollups
          rollupDataObjects = new IDataObject[dataObjectGroups.Count];

          foreach (Rollup rollup in rollupExpr.Rollups)
          {
            DataProperty rollupProp = objDef.dataProperties.Find(x => x.propertyName.ToLower() == rollup.PropertyName.ToLower());

            for (int j = 0; j < dataObjectGroups.Count; j++)
            {
              if (rollupDataObjects[j] == null)
              {
                rollupDataObjects[j] = _dataLayer.Create(resourceName, null)[0];
              }

              switch (rollup.Type)
              {
                case RollupType.Null:
                  {
                    rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, null);
                    break;
                  }
                case RollupType.Max:
                  {
                    object maxValue = null;

                    if (IsNumeric(rollupProp))
                    {
                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        object value = dataObject.GetPropertyValue(rollupProp.propertyName);

                        if (maxValue == null || Convert.ToDecimal(Convert.ToString(value)) > (Decimal)maxValue)
                        {
                          maxValue = Convert.ToDecimal(Convert.ToString(value));
                        }
                      }
                    }
                    else if (rollupProp.dataType == DataType.DateTime)
                    {
                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        DateTime value = (DateTime)dataObject.GetPropertyValue(rollupProp.propertyName);

                        if (maxValue == null || DateTime.Compare(value, (DateTime)maxValue) > 0)
                        {
                          maxValue = value;
                        }
                      }
                    }
                    else if (rollupProp.dataType == DataType.Boolean)
                    {
                      maxValue = true;
                    }
                    else
                    {
                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        string value = (string)dataObject.GetPropertyValue(rollupProp.propertyName);

                        if (maxValue == null || string.Compare(value, (string)maxValue) > 0)
                        {
                          maxValue = value;
                        }
                      }
                    }

                    rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, maxValue);
                    break;
                  }
                case RollupType.Min:
                  {
                    object minValue = null;

                    if (IsNumeric(rollupProp))
                    {
                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        object value = dataObject.GetPropertyValue(rollupProp.propertyName);

                        if (minValue == null || Convert.ToDecimal(Convert.ToString(value)) < (Decimal)minValue)
                        {
                          minValue = Convert.ToDecimal(Convert.ToString(value));
                        }
                      }
                    }
                    else if (rollupProp.dataType == DataType.DateTime)
                    {
                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        DateTime value = (DateTime)dataObject.GetPropertyValue(rollupProp.propertyName);

                        if (minValue == null || DateTime.Compare(value, (DateTime)minValue) < 0)
                        {
                          minValue = value;
                        }
                      }
                    }
                    else if (rollupProp.dataType == DataType.Boolean)
                    {
                      minValue = false;
                    }
                    else
                    {
                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        string value = (string)dataObject.GetPropertyValue(rollupProp.propertyName);

                        if (minValue == null || string.Compare(value, (string)minValue) < 0)
                        {
                          minValue = value;
                        }
                      }
                    }
                    rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, minValue);
                    break;
                  }
                case RollupType.Sum:
                  {
                    if (IsNumeric(rollupProp))
                    {
                      Decimal sum = 0;

                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        object value = dataObject.GetPropertyValue(rollupProp.propertyName);
                        sum += Convert.ToDecimal(Convert.ToString(value));
                      }

                      rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, sum);
                    }
                    else
                    {
                      rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, null);
                    }

                    break;
                  }
                case RollupType.Average:
                  {
                    if (IsNumeric(rollupProp))
                    {
                      Decimal sum = 0;

                      foreach (IDataObject dataObject in dataObjectGroups[j])
                      {
                        object value = dataObject.GetPropertyValue(rollupProp.propertyName);
                        sum += Convert.ToDecimal(Convert.ToString(value));
                      }

                      rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, sum / dataObjectGroups[j].Count);
                    }
                    else
                    {
                      rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, null);
                    }

                    break;
                  }
                default:  // take the first value
                  {
                    rollupDataObjects[j].SetPropertyValue(rollupProp.propertyName, dataObjectGroups[j][0].GetPropertyValue(rollupProp.propertyName));
                    break;
                  }
              }
            }
          }

          _dataObjects = rollupDataObjects;
        }

        // apply paging
        if (limit <= 0)
          limit = 25;

        if (limit > rollupDataObjects.Length)
          limit = rollupDataObjects.Length;

        _dataObjects = _dataObjects.ToList<IDataObject>().GetRange(start, limit);

        #endregion

        _projectionEngine.Start = start;
        _projectionEngine.Limit = limit;
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

    //Search
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName,
      ref string format, string query, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
      NameValueCollection parameters)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        if (!_dataDictionary.enableSearch)
          throw new WebFaultException(HttpStatusCode.NotFound);

        InitializeProjection(resourceName, ref format, false);

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

          _dataObjects = _dataLayer.Search(_dataObjDef.objectName, query, filter, limit, start);
          _projectionEngine.Count = _dataLayer.GetSearchCount(_dataObjDef.objectName, query, filter);
        }
        else
        {
          _dataObjects = _dataLayer.Search(_dataObjDef.objectName, query, limit, start);
          _projectionEngine.Count = _dataLayer.GetSearchCount(_dataObjDef.objectName, query);
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
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    //List
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName,
      ref string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
      NameValueCollection parameters)
    {
      try
      {
        _logger.DebugFormat("Initializing Scope: {0}.{1}", projectName, applicationName);
        InitializeScope(projectName, applicationName);
        _logger.Debug("Initializing DataLayer.");
        InitializeDataLayer();
        _logger.DebugFormat("Initializing Projection: {0} as {1}", resourceName, format);
        InitializeProjection(resourceName, ref format, false);

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

          _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
          _dataObjects = _dataLayer.Get(_dataObjDef.objectName, dataFilter, limit, start);
          _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, dataFilter);
          _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);
        }
        else
        {
          _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
          _dataObjects = _dataLayer.Get(_dataObjDef.objectName, new DataFilter(), limit, start);
          _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, new DataFilter());
          _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);
        }

        _projectionEngine.FullIndex = fullIndex;
        _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}", applicationName, resourceName)
            : String.Format("/{0}/{1}/{2}", applicationName, projectName, resourceName);

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

    //Individual
    public object GetDataProjection(
      string projectName, string applicationName, string resourceName, string className,
       string classIdentifier, ref string format, bool fullIndex)
    {
      string dataObjectName = String.Empty;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, ref format, true);

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
            _dataObjects = _dataLayer.Get(_dataObjDef.objectName, identifiers);
          }
          _projectionEngine.Count = _dataObjects.Count;

          _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}", applicationName, resourceName)
            : String.Format("/{0}/{1}/{2}", applicationName, projectName, resourceName);

          if (_dataObjects != null && _dataObjects.Count > 0)
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
          List<string> identifiers = new List<string> { classIdentifier };
          _dataObjects = _dataLayer.Get(_dataObjDef.objectName, identifiers);

          if (_dataObjects != null && _dataObjects.Count > 0)
          {
            IContentObject contentObject = (IContentObject)_dataObjects[0];
            return contentObject;
          }

          return null;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public IList<PicklistObject> GetPicklists(string projectName, string applicationName, string format)
    {
      string dataObjectName = String.Empty;
      IList<PicklistObject> objs;
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        //InitializeProjection(resourceName, ref format, true);

        objs = _dataDictionary.picklists;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetPicklist: {0}", ex));
        throw ex;
      }

      return objs;
    }

    public Picklists GetPicklist(string projectName, string applicationName, string picklistName,
          string format, int start, int limit)
    {
      string dataObjectName = String.Empty;
      Picklists obj = new Picklists();
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        //InitializeProjection(resourceName, ref format, true);

        obj = _dataLayer.GetPicklist(picklistName, start, limit);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetPicklist: {0}", ex));
        throw ex;
      }

      return obj;
    }

    //Related
    public XDocument GetDataProjection(
            string projectName, string applicationName, string resourceName, string id, string relatedResourceName,
            ref string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex, NameValueCollection parameters)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, ref format, false);

        id = Utility.ConvertSpecialCharOutbound(id, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
        IDataObject parentDataObject = _dataLayer.Get(_dataObjDef.objectName, new List<string> { id }).FirstOrDefault<IDataObject>();
        if (parentDataObject == null) return new XDocument();

        DataRelationship dataRelationship = _dataObjDef.dataRelationships.First(c => c.relationshipName.ToLower() == relatedResourceName.ToLower());
        string relatedObjectType = dataRelationship.relatedObjectName;

        if (limit == 0)
        {
          limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
        }

        _projectionEngine.Start = start;
        _projectionEngine.Limit = limit;
        _projectionEngine.FullIndex = fullIndex;

        _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}/{2}/{3}", applicationName, resourceName, id, relatedResourceName)
            : String.Format("/{0}/{1}/{2}/{3}/{4}", applicationName, projectName, resourceName, id, relatedResourceName);

        _projectionEngine.Count = _dataLayer.GetRelatedCount(parentDataObject, relatedObjectType);

        //if (parameters == null)
        //{
        //    _dataObjects = _dataLayer.GetRelatedObjects(parentDataObject, relatedObjectType, limit, start);
        //}
        //else
        //{
        DataFilter filter = CreateDataFilter(parameters, sortOrder, sortBy);

        foreach (PropertyMap propMap in dataRelationship.propertyMaps)
        {
          filter.Expressions.Add(new Expression()
          {
            PropertyName = propMap.relatedPropertyName,
            RelationalOperator = RelationalOperator.EqualTo,
            Values = new Values() { Convert.ToString(parentDataObject.GetPropertyValue(propMap.dataPropertyName)) }
          });
        }

        _dataObjects = _dataLayer.Get(relatedObjectType, filter, limit, start);
        //}

        XDocument xdoc = _projectionEngine.ToXml(relatedObjectType, ref _dataObjects);
        return xdoc;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public XDocument GetDataProjection(string projectName, string applicationName, string resourceName, string id,
      string relatedResourceName, string relatedId, ref string format)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, ref format, false);

        id = Utility.ConvertSpecialCharOutbound(id, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
        IDataObject parentDataObject = _dataLayer.Get(_dataObjDef.objectName, new List<string> { id }).FirstOrDefault<IDataObject>();
        if (parentDataObject == null) return new XDocument();

        _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}/{2}/{3}", applicationName, resourceName, id, relatedResourceName)
            : String.Format("/{0}/{1}/{2}/{3}/{4}", applicationName, projectName, resourceName, id, relatedResourceName);

        DataRelationship relationship = _dataObjDef.dataRelationships.First(c => c.relationshipName.ToLower() == relatedResourceName.ToLower());
        DataObject relatedDataObject = _dataDictionary.dataObjects.First(c => c.objectName.ToLower() == relationship.relatedObjectName.ToLower());

        _dataObjects = _dataLayer.Get(relatedDataObject.objectName, new List<string> { relatedId });

        XDocument xdoc = _projectionEngine.ToXml(relatedDataObject.objectName, ref _dataObjects);
        return xdoc;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    private IList<IDataObject> GetDataObject(string className, string classIdentifier)
    {
      DataFilter filter = new DataFilter();

      IList<string> identifiers = new List<string> { classIdentifier };

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
              string relatedObjectType = clsIdentifierParts[clsIdentifierParts.Length - 2];

              // get related object then assign its related properties to top level data object properties
              DataFilter relatedObjectFilter = new DataFilter();

              Expression relatedExpression = new Expression
              {
                PropertyName = clsIdentifierParts.Last(),
                Values = new Values { identifierValue }
              };

              relatedObjectFilter.Expressions.Add(relatedExpression);
              IList<IDataObject> relatedObjects = _dataLayer.Get(relatedObjectType, relatedObjectFilter, 0, 0);

              if (relatedObjects != null && relatedObjects.Count > 0)
              {
                IDataObject relatedObject = relatedObjects.First();
                DataRelationship dataRelationship = _dataObjDef.dataRelationships.Find(c => c.relatedObjectName == relatedObjectType);

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

        identifiers = _dataLayer.GetIdentifiers(_dataObjDef.objectName, filter);
        if (identifiers == null || identifiers.Count == 0)
        {
          throw new Exception("Identifier [" + classIdentifier + "] of class [" + className + "] is not found.");
        }
      }
      #endregion

      IList<IDataObject> dataObjects = _dataLayer.Get(_dataObjDef.objectName, identifiers);
      if (dataObjects != null && dataObjects.Count > 0)
      {
        return dataObjects;
      }

      return null;
    }


    public Response Delete(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}.{2}", projectName, applicationName, graphName);

        InitializeScope(projectName, applicationName);

        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

        response.Append(_semanticEngine.Delete(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting {0} graphs: {1}", graphName, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      return response;
    }

    public Response Post(string projectName, string applicationName, string graphName, string format, XDocument xml)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);

        InitializeDataLayer();

        InitializeProjection(graphName, ref format, false);

        if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }

        IList<IDataObject> dataObjects = null;
        if (_isProjectionPart7)
        {
          dataObjects = _projectionEngine.ToDataObjects(_graphMap.name, ref xml);
        }
        else
        {
          dataObjects = _projectionEngine.ToDataObjects(_dataObjDef.objectName, ref xml);
        }

        //_projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        //IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xml);
        response = _dataLayer.Post(dataObjects);

        response.DateTimeStamp = DateTime.Now;
        //response.Level = StatusLevel.Success;

        string baseUri = _settings["GraphBaseUri"] +
                         _settings["ApplicationName"] + "/" +
                         _settings["ProjectName"] + "/" +
                         graphName + "/";

        response.PrepareResponse(baseUri);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }

    public Response PostRelated(string projectName, string applicationName, string graphName, string id, string relatedResource, string format, XDocument xml)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);

        InitializeDataLayer();

        InitializeProjection(graphName, ref format, false);

        if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }
        IList<IDataObject> parentDataObject = _dataLayer.Get(_dataObjDef.objectName, new List<string> { id });

        IList<IDataObject> childdataObjects = null;
        if (_isProjectionPart7)
        {
          childdataObjects = _projectionEngine.ToDataObjects(_graphMap.name, ref xml);
        }
        else
        {
          childdataObjects = _projectionEngine.ToDataObjects(relatedResource, ref xml);
        }

        //_projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        //IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xml);
        IList<IDataObject> MeregedDataObjects = new List<IDataObject>();
        MeregedDataObjects = parentDataObject;
        foreach (IDataObject obj in childdataObjects)
        {
          MeregedDataObjects.Add(obj);
        }
        //MeregedDataObjects.Concat(parentDataObject);
        // MeregedDataObjects.Concat(childdataObjects);

        response = _dataLayer.Post(MeregedDataObjects);

        response.DateTimeStamp = DateTime.Now;
        //response.Level = StatusLevel.Success;

        string baseUri = _settings["GraphBaseUri"] +
                         _settings["ApplicationName"] + "/" +
                         _settings["ProjectName"] + "/" +
                         graphName + "/";

        response.PrepareResponse(baseUri);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }


    public Response Post(string projectName, string applicationName, string graphName, string format, DataItems dataItems)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);

        InitializeDataLayer();

        InitializeProjection(graphName, ref format, false);

        if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
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

        IList<IDataObject> dataObjects = null;
        if (_isProjectionPart7)
        {
          dataObjects = _projectionEngine.ToDataObjects(_graphMap.name, ref xml);
        }
        else
        {
          dataObjects = _projectionEngine.ToDataObjects(_dataObjDef.objectName, ref xml);
        }

        //_projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        //IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xml);
        response = _dataLayer.Post(dataObjects);

        response.DateTimeStamp = DateTime.Now;
        //response.Level = StatusLevel.Success;

        string baseUri = _settings["GraphBaseUri"] +
                         _settings["ApplicationName"] + "/" +
                         _settings["ProjectName"] + "/" +
                         graphName + "/";

        response.PrepareResponse(baseUri);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }

    public Response PostContent(string projectName, string applicationName, string graphName, string format, string identifier, Stream stream)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);

        InitializeDataLayer();

        if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }

        //_projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());

        IList<IDataObject> dataObjects = new List<IDataObject>();
        IList<string> identifiers = new List<string> { identifier };
        dataObjects = _dataLayer.Create(graphName, identifiers);

        IContentObject contentObject = (IContentObject)dataObjects[0];
        contentObject.content = stream;

        IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        string contentType = request.ContentType;
        contentObject.contentType = contentType;

        dataObjects = new List<IDataObject>();
        dataObjects.Add(contentObject);

        response = _dataLayer.Post(dataObjects);
        response.DateTimeStamp = DateTime.Now;
        //response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }

    public Response DeleteIndividual(string projectName, string applicationName, string graphName, string identifier, string format)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        InitializeProjection(graphName, ref format, false);

        if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform delete on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }

        if (_isProjectionPart7)
        {
          response = _dataLayer.Delete(_graphMap.name, new List<String> { identifier });
        }
        else
        {
          identifier = Utility.ConvertSpecialCharOutbound(identifier, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
          response = _dataLayer.Delete(_dataObjDef.objectName, new List<String> { identifier });
        }

        response.DateTimeStamp = DateTime.Now;
        //response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in DeleteIndividual: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }

    public Response DeleteRelated(string projectName, string applicationName, string graphName, string parentidentifier, string relatedResource, string id, string format)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        InitializeProjection(graphName, ref format, false);

        if (_dataObjDef.isReadOnly || _settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform delete on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }

        if (_isProjectionPart7)
        {//TODO:talk to rob
          response = _dataLayer.Delete(_graphMap.name, new List<String> { id });
        }
        else
        {
          id = Utility.ConvertSpecialCharOutbound(id, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
          response = _dataLayer.Delete(relatedResource, new List<String> { id });
        }

        response.DateTimeStamp = DateTime.Now;
        //response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in DeleteIndividual: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }

    #endregion

    #region private methods
    private void InitializeScope(string projectName, string applicationName, bool loadDataLayer)
    {
      try
      {
        string scope = String.Format("{0}.{1}", projectName, applicationName);

        if (!_isScopeInitialized)
        {
          _settings["ProjectName"] = projectName;
          _settings["ApplicationName"] = applicationName;

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

          //scope stuff

          bool isScopeValid = false;
          foreach (ScopeProject project in _scopes)
          {
            if (project.Name.ToUpper() == projectName.ToUpper())
            {
              foreach (ScopeApplication application in project.Applications)
              {
                if (application.Name.ToUpper() == applicationName.ToUpper())
                {
                  _application = application;
                  isScopeValid = true;
                  break;
                }
              }
            }
          }

          if (!isScopeValid)
            scope = String.Format("all.{0}", applicationName);
          //throw new Exception(String.Format("Invalid scope [{0}].", scope));

          _settings["Scope"] = scope;

          string relativePath = String.Format("{0}BindingConfiguration.{1}.xml", _settings["AppDataPath"], scope);

          //Ninject Extension requires fully qualified path.
          string bindingConfigurationPath = Path.Combine(
            _settings["BaseDirectoryPath"],
            relativePath
          );

          _settings["BindingConfigurationPath"] = bindingConfigurationPath;

          if (File.Exists(bindingConfigurationPath))
          {
            _kernel.Load(bindingConfigurationPath);
          }
          else
          {
            _logger.Error("Binding configuration not found.");
          }

          string dbDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", _settings["AppDataPath"], scope);

          _settings["DBDictionaryPath"] = dbDictionaryPath;

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
          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void InitializeScope(string projectName, string applicationName)
    {
      InitializeScope(projectName, applicationName, true);
    }

    private void InitializeProjection(string resourceName, ref string format, bool isIndividual)
    {
      try
      {
        string[] expectedFormats = { 
              "rdf", 
              "dto",
              "p7xml",
              "xml", 
              "json", 
              "html"
            };

        _graphMap = _mapping.FindGraphMap(resourceName);

        if (_graphMap != null)
        {
          _isResourceGraph = true;
          _dataObjDef = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == _graphMap.dataObjectName.ToUpper());

          if (_dataObjDef == null || _dataObjDef.isRelatedOnly)
          {
            _logger.Warn("Data object [" + _graphMap.dataObjectName + "] not found.");
            throw new WebFaultException(HttpStatusCode.NotFound);
          }
        }
        else
        {
          _dataObjDef = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == resourceName.ToUpper());

          if (_dataObjDef == null || _dataObjDef.isRelatedOnly)
          {
            _logger.Warn("Resource [" + resourceName + "] not found.");
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
              throw new FileNotFoundException("Requested resource [" + resourceName + "] cannot be rendered as Part7.");
            }
          }
        }
        else if (format == _settings["DefaultProjectionFormat"] && _isResourceGraph)
        {
          format = "p7xml";
          _projectionEngine = _kernel.Get<IProjectionLayer>("p7xml");
          _isProjectionPart7 = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void InitializeDataLayer()
    {
      InitializeDataLayer(true);
    }

    private void InitializeDataLayer(bool setDictionary)
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
            Utility.Write<IDictionary>(_keyRing, @"KeyRing.xml");
          }

          _dataLayer = _kernel.TryGet<IDataLayer2>("DataLayer");

          if (_dataLayer == null)
          {
            _dataLayer = (IDataLayer2)_kernel.Get<IDataLayer>("DataLayer");
          }

          _kernel.Rebind<IDataLayer2>().ToConstant(_dataLayer);

          if (setDictionary)
            InitializeDictionary();
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void InitializeDictionary()
    {
      if (!_isDataLayerInitialized)
      {
        _dataDictionary = _dataLayer.GetDictionary();
        _kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);
        _isDataLayerInitialized = true;
      }
    }

    private void InitializeIdentity()
    {
      try
      {
        _identityLayer = _kernel.Get<IIdentityLayer>("IdentityLayer");
        _keyRing = _identityLayer.GetKeyRing();
        _kernel.Bind<IDictionary>().ToConstant(_keyRing).Named("KeyRing");
        _settings.AppendKeyRing(_keyRing);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing identity: {0}", ex));
      }
    }

    private Response Refresh(string graphName)
    {
      _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

      _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

      LoadDataObjectSet(graphName, null);

      XDocument rdf = _projectionEngine.ToXml(graphName, ref _dataObjects);

      return _semanticEngine.Refresh(graphName, rdf);
    }

    private long LoadDataObjectSet(string graphName, IList<string> identifiers)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      _dataObjects.Clear();

      if (identifiers != null)
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);
      else
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, null);

      return _dataObjects.Count;
    }

    private long LoadDataObjectSet(string graphName, DataFilter dataFilter, int start, int limit)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      _dataObjects.Clear();

      if (dataFilter != null)
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, dataFilter, limit, start);
      else
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, null);

      long count = _dataLayer.GetCount(_graphMap.dataObjectName, dataFilter);

      return count;
    }

    private void DeleteScope()
    {
      try
      {
        // clean up ScopeList
        foreach (ScopeProject project in _scopes)
        {
          if (project.Name.ToUpper() == _settings["ProjectName"].ToUpper())
          {
            foreach (ScopeApplication application in project.Applications)
            {
              if (application.Name.ToUpper() == _settings["ApplicationName"].ToUpper())
              {
                project.Applications.Remove(application);
              }
              break;
            }
            break;
          }
        }

        // save ScopeList
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

    private IList<IDataObject> CreateDataObjects(string graphName, string dataObjectsString)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();
      dataObjects = _dataLayer.Create(graphName, null);

      if (dataObjectsString != null && dataObjectsString != String.Empty)
      {
        XmlReader reader = XmlReader.Create(new StringReader(dataObjectsString));
        XDocument file = XDocument.Load(reader);
        file = Utility.RemoveNamespace(file);

        var dtoResults = from c in file.Elements("ArrayOf" + graphName).Elements(graphName) select c;
        int j = 0;
        foreach (var dtoResult in dtoResults)
        {
          var dtoProperties = from c in dtoResult.Elements("Properties").Elements("Property") select c;
          IDataObject dto = dataObjects[j];
          j++;
          foreach (var dtoProperty in dtoProperties)
          {
            dto.SetPropertyValue(dtoProperty.Attribute("name").Value, dtoProperty.Attribute("value").Value);
          }
          dataObjects.Add(dto);
        }
      }
      return dataObjects;
    }
    #endregion

    ///TODO: Pending on testing, do not delete
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

    //            _dataLayerPath = dataLayer.Path;

    //            Type type = dataLayerAssembly.GetType(assembly.Split(',')[0]);
    //            ConstructorInfo[] ctors = type.GetConstructors();

    //            foreach (ConstructorInfo ctor in ctors)
    //            {
    //              ParameterInfo[] paramList = ctor.GetParameters();

    //              if (paramList.Length == 0)  // default constructor
    //              {
    //                _dataLayer = (IDataLayer2)Activator.CreateInstance(type);

    //                break;
    //              }
    //              else if (paramList.Length == 1)  // constructor with 1 parameter
    //              {
    //                if (ctor.GetParameters()[0].ParameterType.FullName == typeof(AdapterSettings).FullName)
    //                {
    //                  _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _settings);
    //                }
    //                else if (ctor.GetParameters()[0].ParameterType.FullName == typeof(IDictionary).FullName)
    //                {
    //                  _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _settings);
    //                }

    //                break;
    //              }
    //              else if (paramList.Length == 2)  // constructor with 2 parameters
    //              {
    //                if (ctor.GetParameters()[0].ParameterType.FullName == typeof(AdapterSettings).FullName &&
    //                  ctor.GetParameters()[1].ParameterType.FullName == typeof(IDictionary).FullName)
    //                {
    //                  _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _settings, _keyRing);
    //                }
    //                else if (ctor.GetParameters()[0].ParameterType.FullName == typeof(IDictionary).FullName &&
    //                  ctor.GetParameters()[1].ParameterType.FullName == typeof(AdapterSettings).FullName)
    //                {
    //                  _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _keyRing, _settings);
    //                }
    //                else
    //                {
    //                  throw new Exception("Data layer does not contain supported constructor.");
    //                }

    //                break;
    //              }
    //            }
    //          }

    //          if (_dataLayer != null)
    //          {
    //            _kernel.Rebind<IDataLayer2>().ToConstant(_dataLayer);
    //          }

    //          break;
    //        }
    //      }

    //      //
    //      // if data layer is internal or not registered (for backward compatibility),
    //      // attempt to load it using binding configuration
    //      //
    //      if (_dataLayer == null)
    //      {
    //        if (File.Exists(_settings["BindingConfigurationPath"]))
    //        {
    //          _kernel.Load(_settings["BindingConfigurationPath"]);
    //        }
    //        else
    //        {
    //          _logger.Error("Binding configuration not found.");
    //        }

    //        _dataLayer = _kernel.TryGet<IDataLayer2>("DataLayer");

    //        if (_dataLayer == null)
    //        {
    //          _dataLayer = (IDataLayer2)_kernel.Get<IDataLayer>("DataLayer");
    //        }

    //        _kernel.Rebind<IDataLayer2>().ToConstant(_dataLayer);
    //      }

    //      if (_dataLayer == null)
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
    //    if (File.Exists(_dataLayersRegistryPath))
    //    {
    //      dataLayers = Utility.Read<DataLayers>(_dataLayersRegistryPath);
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
    //        Utility.Write<DataLayers>(dataLayers, _dataLayersRegistryPath);
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

    //      Utility.Write<DataLayers>(dataLayers, _dataLayersRegistryPath);
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

    //      Utility.Write<DataLayers>(dataLayers, _dataLayersRegistryPath);
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
    //        Utility.Write<DataLayers>(dataLayers, _dataLayersRegistryPath);

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

    //  if (Directory.Exists(_dataLayerPath))
    //  {
    //    string[] files = Directory.GetFiles(_dataLayerPath);

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
      DataLayers dataLayers = new DataLayers();

      // Load NHibernate data layer
      Type nhType = Type.GetType("org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary", true);
      string nhLibrary = nhType.Assembly.GetName().Name;
      string nhAssembly = string.Format("{0}, {1}", nhType.FullName, nhLibrary);
      DataLayer nhDataLayer = new DataLayer { Assembly = nhAssembly, Name = nhLibrary, Configurable = true };
      dataLayers.Add(nhDataLayer);

      // Load Spreadsheet data layer
      Type ssType = Type.GetType("org.iringtools.adapter.datalayer.SpreadsheetDatalayer, SpreadsheetDatalayer", true);
      string ssLibrary = ssType.Assembly.GetName().Name;
      string ssAssembly = string.Format("{0}, {1}", ssType.FullName, ssLibrary);
      DataLayer ssDataLayer = new DataLayer { Assembly = ssAssembly, Name = ssLibrary, Configurable = true };
      dataLayers.Add(ssDataLayer);

      try
      {
        Type type = typeof(IDataLayer);
        Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly asm in domainAssemblies)
        {
          try
          {
            Type[] asmTypes = asm.GetTypes();
            try
            {
              if (asmTypes != null)
              {
                foreach (System.Type asmType in asmTypes)
                {
                  if (type.IsAssignableFrom(asmType) && !(asmType.IsInterface || asmType.IsAbstract))
                  {
                    bool configurable = asmType.BaseType.Equals(typeof(BaseConfigurableDataLayer));
                    string name = asm.FullName.Split(',')[0];

                    if (!dataLayers.Exists(x => x.Name.ToLower() == name.ToLower()))
                    {
                      string assembly = string.Format("{0}, {1}", asmType.FullName, name);
                      DataLayer dataLayer = new DataLayer { Assembly = assembly, Name = name, Configurable = configurable };
                      dataLayers.Add(dataLayer);
                    }
                  }
                }
              }
            }
            catch (Exception e)
            {
              _logger.Error("Error loading data layer (while getting types): " + e);
            }
          }
          catch (Exception e)
          {
            _logger.Error("Error loading data layer (while getting assemblies): " + e);
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error loading data layer: " + e);
      }

      return dataLayers;
    }

    public Response Configure(string projectName, string applicationName, HttpRequest httpRequest)
    {
      Response response = new Response();
      response.Messages = new Messages();
      XElement binding;
      XElement configuration = null;
      try
      {
        string savedFileName = string.Empty;

        foreach (string file in httpRequest.Files)
        {
          HttpPostedFile hpf = httpRequest.Files[file] as HttpPostedFile;
          if (hpf.ContentLength == 0)
            continue;
          hpf.InputStream.Position = 0;

          savedFileName = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, _settings["AppDataPath"],
          Path.GetFileName(hpf.FileName));
          hpf.SaveAs(savedFileName);
          hpf.InputStream.Flush();
        }

        InitializeScope(projectName, applicationName, false);

        string dataLayer = httpRequest.Form["DataLayer"];
        // Check request whether have Configuration in Request or not. SP & ID don't have this ----------------------
        if (httpRequest.Form["Configuration"] != null)
        {
          configuration = Utility.DeserializeXml<XElement>(httpRequest.Form["Configuration"]);

          binding = new XElement("module",
             new XAttribute("name", _settings["Scope"]),
               new XElement("bind",
                 new XAttribute("name", "DataLayer"),
                 new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
                 new XAttribute("to", dataLayer)
               )
             );

          binding.Save(_settings["BindingConfigurationPath"]);
          try
          {
            _kernel.Load(_settings["BindingConfigurationPath"]);
          }
          catch
          {
            ///ignore error if already loaded
            ///this is required for Spreadsheet Datalayer 
            ///when spreadsheet is re-uploaded
          }
        }
        InitializeDataLayer(false);
        if (httpRequest.Form["Configuration"] != null)
        {

          response = ((IDataLayer2)_dataLayer).Configure(configuration);
        }

        InitializeDictionary();

      }
      catch (Exception ex)
      {
        response.Messages.Add(String.Format("Failed to Upload Files[{0}]", _settings["Scope"]));
        response.Messages.Add(ex.Message);
        response.Level = StatusLevel.Error;
      }
      return response;
    }

    public XElement GetConfiguration(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.GetConfiguration();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetConfiguration: {0}", ex));
        throw new Exception(string.Format("Error getting configuration: {0}", ex));
      }
    }

    public Response RefreshDataObjects(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.RefreshAll();
      }
      catch (Exception ex)
      {
        string errMsg = String.Format("Error refreshing data objects: {0}", ex);
        _logger.Error(errMsg);
        throw new Exception(errMsg);
      }
    }

    public Response RefreshDataObject(string projectName, string applicationName, string objectType)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.Refresh(objectType);
      }
      catch (Exception ex)
      {
        string errMsg = String.Format("Error refreshing data object [{0}]: {1}", objectType, ex);
        _logger.Error(errMsg);
        throw new Exception(errMsg);
      }
    }

    public Response RefreshDataObject(string projectName, string applicationName, string objectType, DataFilter dataFilter)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.Refresh(objectType, dataFilter);
      }
      catch (Exception ex)
      {
        string errMsg = String.Format("Error refreshing data object [{0}]: {1}", objectType, ex);
        _logger.Error(errMsg);
        throw new Exception(errMsg);
      }
    }

    public IList<Object> GetSummary(String projectName, String applicationName)
    {
      InitializeScope(projectName, applicationName);
      InitializeDataLayer();

      if (!_dataDictionary.enableSummary)
        throw new WebFaultException(HttpStatusCode.NotFound);

      return _dataLayer.GetSummary();
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
        byte[] json = new byte[0];

        if (xElement != null)
        {
          DataItems dataItems = Utility.DeserializeDataContract<DataItems>(xElement.ToString());
          DataItemSerializer serializer = new DataItemSerializer(
            _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
          MemoryStream ms = serializer.SerializeToMemoryStream<DataItems>(dataItems, false);
          json = ms.ToArray();
          ms.Close();
        }

        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        HttpContext.Current.Response.Write(Encoding.UTF8.GetString(json, 0, json.Length));
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
        HttpContext.Current.Response.ContentType = contentObject.contentType;
        HttpContext.Current.Response.BinaryWrite(contentObject.content.ToMemoryStream().GetBuffer());
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

    private List<IDataObject> PageDataObjects(string objectType, DataFilter filter)
    {
      List<IDataObject> dataObjects = new List<IDataObject>();

      int pageSize = (String.IsNullOrEmpty(_settings["DefaultPageSize"]))
        ? 250 : int.Parse(_settings["DefaultPageSize"]);

      long count = _dataLayer.GetCount(objectType, filter);

      for (int offset = 0; offset < count; offset = offset + pageSize)
      {
        dataObjects.AddRange(_dataLayer.Get(objectType, filter, pageSize, offset));
      }

      return dataObjects;
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
        string leftValue = left.GetPropertyValue(_dataProp.propertyName).ToString();
        string rightValue = right.GetPropertyValue(_dataProp.propertyName).ToString();
        return string.Compare(leftValue, rightValue);
      }
    }
  }
}