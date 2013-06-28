﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
    private IList<IDataObject> _dataObjects = new List<IDataObject>();  // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers =
      new Dictionary<string, List<string>>();  // dictionary of class ids and list of identifiers

    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;
    private string[] arrSpecialcharlist;
    private string[] arrSpecialcharValue;
    private string _dataLayersRegistryPath;

    private static ConcurrentDictionary<string, RequestStatus> _requests =
      new ConcurrentDictionary<string, RequestStatus>();

    [Inject]
    public AdapterProvider(NameValueCollection settings)
    {
      try
      {
        //TODO: Pending on testing, do not delete
        //AppDomain currentDomain = AppDomain.CurrentDomain;
        //currentDomain.AssemblyResolve += new ResolveEventHandler(DataLayerAssemblyResolveEventHandler);

        var ninjectSettings = new NinjectSettings { LoadExtensions = false, UseReflectionBasedInjection = true };
        _kernel = new StandardKernel(ninjectSettings, new AdapterModule());

        _kernel.Load(new XmlExtensionModule());
        _settings = _kernel.Get<AdapterSettings>();
        _settings.AppendSettings(settings);

        // capture request headers
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
        
        if (_settings["LdapConfiguration"] != null &&_settings["LdapConfiguration"].ToLower() == "true")
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
      return _scopes;
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
          _scopes.Add(scope);   
          _scopes.Sort(new ScopeComparer());

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
          _scopes.Sort(new ScopeComparer());

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

        if (application != null)  // application exists, delete and re-create it
        {
          application.DisplayName = updatedApp.DisplayName;
        }
        else  // application does not exist, stop processing
        {
          throw new Exception(String.Format("Application [{0}.{1}] not found.", scopeName, oldAppName));
        }

        scope.Applications.Sort(new ApplicationComparer());

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

    private static bool IsApplicationDataChanged(ScopeApplication updatedApp, ScopeApplication oldApp)
    {
      bool Ischanged = false;
      try
      {
        if (oldApp.Name != updatedApp.Name || oldApp.Description != updatedApp.Description || oldApp.Assembly != updatedApp.Assembly)
        {
          Ischanged = true;
        }
        else if (oldApp.Configuration.AppSettings.Settings.Count != updatedApp.Configuration.AppSettings.Settings.Count)
        {
          Ischanged = true;
        }
        else
        {
          for (int i = 0; i < updatedApp.Configuration.AppSettings.Settings.Count; i++)
          {
            if (updatedApp.Configuration.AppSettings.Settings[i].Value != oldApp.Configuration.AppSettings.Settings[i].Value)
            {
              Ischanged = true;
              break;
            }
          }
        }
      }
      catch
      {
        Ischanged = true;
      }
      return Ischanged;
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

    public XElement GetBinding(string projectName, string applicationName)
    {
      XElement binding = null;

      try
      {
        InitializeScope(projectName, applicationName);
        binding = utility.Utility.GetxElementObject(_settings["BindingConfigurationPath"]);
        //binding = XElement.Load(_settings["BindingConfigurationPath"]);
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
    private void DoGetDictionary(string projectName, string applicationName, string id)
    {
      try
      {
        DataDictionary dictionary = GetDictionary(projectName, applicationName);

        _requests[id].ResponseText = Utility.Serialize<DataDictionary>(dictionary, true);
        _requests[id].State = State.Completed;
      }
      catch (Exception ex)
      {
        _requests[id].Message = ex.Message;
        _requests[id].State = State.Error;
      }
    }

    public string AsyncGetDictionary(string projectName, string applicationName)
    {
      try
      {
        var id = QueueNewRequest();
        Task task = Task.Factory.StartNew(() => DoGetDictionary(projectName, applicationName, id));
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
        Response response = _dataLayer.RefreshAll();

        _requests[id].ResponseText = Utility.Serialize<Response>(response, true);
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

    public string AsyncRefreshDictionary(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer(false);

        var id = QueueNewRequest();
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
        var id = QueueNewRequest();
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
        XDocument xDocument = GetDataProjection(project, app, resource, filter, ref format, start, limit, fullIndex);
        
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
        string errMsg = String.Format("Error getting request status: {0}", ex);
        _logger.Error(errMsg);
        throw new Exception(errMsg);
      }
    }

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
        _logger.Error(string.Format("Error getting data dictionary: {0}", ex));
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
        throw ex;
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
        throw ex;
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
        _dataObjDef = dictionary.GetDataObject(resourceName);

        AddURIsInSettingCollection(projectName, applicationName, resourceName);

        if (_dataObjDef != null)
          filter.AppendFilter(_dataObjDef.dataFilter);

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

        AddURIsInSettingCollection(projectName, applicationName, resourceName);

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


        AddURIsInSettingCollection(projectName, applicationName, resourceName);


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

    private void AddURIsInSettingCollection(string ProjectName, string applicationName, string resourceName, string resourceIdentifier = null, string relatedResourceName = null, string relatedId = null)
    {
        try
        {
            _logger.Debug("Adding URI in setting Collection.");

            if (_dataObjDef.keyProperties.Count > 0)
            {
                string keyPropertyName = _dataObjDef.keyProperties[0].keyPropertyName;

                string genericURI = "/" + resourceName;
                string specificURI = "/" + resourceName;

                if (resourceIdentifier != null)
                {
                    genericURI = resourceName + "/{" + keyPropertyName + "}";
                    specificURI = resourceName + "/" + resourceIdentifier;
                }

                if (relatedResourceName != null)
                {
                    genericURI = resourceName + "/{" + keyPropertyName + "}/" + relatedResourceName;
                    specificURI = resourceName + "/" + resourceIdentifier + "/" + relatedResourceName;
                }
                if (relatedId != null)
                {
                    DataObject releteddataObject = _dataDictionary.dataObjects.Find(x => x.objectName.ToUpper() == relatedResourceName.ToUpper());

                    // null checked needed!
                    if (releteddataObject != null)
                    {
                        string reletedKeyPropertyName = releteddataObject.keyProperties[0].keyPropertyName;

                        genericURI = resourceName + "/{" + keyPropertyName + "}/" + reletedKeyPropertyName + "/{" + reletedKeyPropertyName + "}";
                        specificURI = resourceName + "/" + resourceIdentifier + "/" + reletedKeyPropertyName + "/" + relatedId;
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

        AddURIsInSettingCollection(projectName, applicationName, resourceName, classIdentifier);

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

          if (_dataObjects != null )
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
          IDictionary<string, string> idFormats = new Dictionary<string, string> {
                    {classIdentifier, format}
          };

          IContentObject contentObject = _dataLayer.GetContents(_dataObjDef.objectName, idFormats).FirstOrDefault();
          return contentObject;
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

        AddURIsInSettingCollection(projectName, applicationName, resourceName, id, relatedResourceName);

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

        //_projectionEngine.Count = _dataLayer.GetRelatedCount(parentDataObject, relatedObjectType);

        DataFilter filter = CreateDataFilter(parameters, sortOrder, sortBy);

        foreach (PropertyMap propMap in dataRelationship.propertyMaps)
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
          _dataObjects = _dataLayer.GetRelatedObjects(parentDataObject, relatedObjectType, filter, limit, start);
        }
        catch (NotImplementedException)
        {
          _dataObjects = _dataLayer.Get(relatedObjectType, filter, limit, start);
        }
        _projectionEngine.Count = _dataObjects.Count;

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

        AddURIsInSettingCollection(projectName, applicationName, resourceName, id, relatedResourceName, relatedId);

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
        
        IList<IDataObject> childdataObjects = null;
        if (_isProjectionPart7)
        {
          childdataObjects = _projectionEngine.ToDataObjects(_graphMap.name, ref xml);
        }
        else
        {
          childdataObjects = _projectionEngine.ToDataObjects(relatedResource, ref xml);
        }

        try
        {
          response = _dataLayer.PostRelatedObjects(graphName, id, relatedResource, childdataObjects);
        }
        catch (NotImplementedException)
        {
          IList<IDataObject> parentDataObject = _dataLayer.Get(_dataObjDef.objectName, new List<string> { id });
          IList<IDataObject> MeregedDataObjects = new List<IDataObject>();
          MeregedDataObjects = parentDataObject;

          foreach (IDataObject obj in childdataObjects)
          {
            MeregedDataObjects.Add(obj);
          }

          response = _dataLayer.Post(MeregedDataObjects);
        }

        response.DateTimeStamp = DateTime.Now;

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
        contentObject.Content = stream;

        IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        string contentType = request.ContentType;
        contentObject.ContentType = contentType;

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
    #endregion

    #region private methods
    private void Initialize(string projectName, string applicationName)
    {
      //
      // load app settings
      //
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

      _settings["ProjectName"] = projectName;
      _settings["ApplicationName"] = applicationName;

      //
      // determine whether scope is real or implied to set
      //
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
    }

    private void InitializeScope(string projectName, string applicationName, bool loadDataLayer)
    {
      try
      {
        string scope = string.Format("{0}.{1}", projectName, applicationName);

        if (!_isScopeInitialized)
        {
          Initialize(projectName, applicationName);

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
          else if (utility.Utility.isLdapConfigured && utility.Utility.FileExistInRepository<XElementClone>(bindingConfigurationPath))
          {
              XElement bindingConfig = Utility.GetxElementObject(bindingConfigurationPath);
              string fileName = Path.GetFileName(bindingConfigurationPath);
              string tempPath = Path.GetTempPath() + fileName;
              bindingConfig.Save(tempPath);
              _kernel.Load(tempPath);
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
        throw ex;
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
        throw ex;
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

          _isDataLayerInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw ex;
      }
    }

    private void InitializeDictionary()
    {
      if (!_isDataLayerInitialized)
      {
          try
          {
              string path = string.Format("{0}DataDictionary.{1}.{2}.xml",
                     _settings["AppDataPath"], _settings["ProjectName"], _settings["ApplicationName"]);

              bool isFileinLDAP = utility.Utility.FileExistInRepository<DataDictionary>(path);
              if (((_settings["UseDictionaryCache"] == null || bool.Parse(_settings["UseDictionaryCache"].ToString()) == true) && File.Exists(path))
                  || isFileinLDAP )
              {
                  _dataDictionary = utility.Utility.Read<DataDictionary>(path, true);
              }
              else
              {
                  _dataDictionary = _dataLayer.GetDictionary();

                  if (_dataDictionary != null)
                  {
                      if (_settings["UseDictionaryCache"] == null || bool.Parse(_settings["UseDictionaryCache"].ToString()) == true) // only create cache if settings indicate we will use it
                      {
                          utility.Utility.Write<DataDictionary>(_dataDictionary, path, true);
                      }
                  }
              }

              _kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);
          }
          catch (Exception ex)
          {
              _logger.Error(string.Format("Error initializing data dictionary: {0}" + ex));
              throw ex;
          }
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
      Type nhType = typeof(NHibernateDataLayer);
      string nhLibrary = nhType.Assembly.GetName().Name;
      string nhAssembly = string.Format("{0}, {1}", nhType.FullName, nhLibrary);
      DataLayer nhDataLayer = new DataLayer { Assembly = nhAssembly, Name = nhLibrary, Configurable = true };
      dataLayers.Add(nhDataLayer);

      // Load Spreadsheet data layer
      Type ssType = typeof(SpreadsheetDatalayer);
      string ssLibrary = ssType.Assembly.GetName().Name;
      string ssAssembly = string.Format("{0}, {1}", ssType.FullName, ssLibrary);
      DataLayer ssDataLayer = new DataLayer { Assembly = ssAssembly, Name = ssLibrary, Configurable = true };
      dataLayers.Add(ssDataLayer);

      try
      {
        Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        Type type = typeof(IDataLayer);
        
        foreach (Assembly asm in domainAssemblies)
        {
          Type[] asmTypes = null;
          try
          {
            asmTypes = asm.GetTypes().Where(a => a != null && (type.IsAssignableFrom(a) && !(a.IsInterface || a.IsAbstract))).ToArray();
          }
          catch (ReflectionTypeLoadException e)
          {
            // if we are running the the iRing site under Anonymous authentication with the DefaultApplicationPool Identity we
            // can run into ReflectionPermission issues but as our datalayer assemblies are in our web site's bin folder we
            // should be able to ignore the exceptions and work with the accessibe types loaded in e.Types.
            asmTypes = e.Types.Where(a => a != null && (type.IsAssignableFrom(a) && !(a.IsInterface || a.IsAbstract))).ToArray();
            _logger.Warn("GetTypes() for " + asm.FullName +" cannot access all types, but datalayer loading is continuing: " + e);
          }

          try
          {
            if (asmTypes.Any()) 
            {
              _logger.Debug("assembly:" + asm.FullName);
              foreach (System.Type asmType in asmTypes)
              {
                _logger.Debug("asmType:" + asmType.ToString());
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
            _logger.Error("Error loading data layer (while getting assemblies): " + e);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error loading data layer: {0}" + ex));
        throw ex;
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
        throw ex;
      }
    }

    public Response RefreshDataObjects(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer(false);

        string path = string.Format("{0}DataDictionary.{1}.{2}.xml",
             _settings["AppDataPath"], projectName, applicationName);

        if (File.Exists(path))
        {
          File.Delete(path);
        }

        return _dataLayer.RefreshAll();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error refreshing data objects: {0}", ex));
        throw ex;
      }
    }

    public Response RefreshDataObject(string projectName, string applicationName, string objectType)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer(false);

        return _dataLayer.Refresh(objectType);
      }
      catch (Exception ex)
      {
        string errMsg = String.Format("Error refreshing data object [{0}]: {1}", objectType, ex);
        _logger.Error(errMsg);
        throw new Exception(errMsg);
      }
    }

    #region cache related methods
    public Response RefreshCache(string scope, string app, bool updateDictionary)
    {
      try
      {
        Initialize(scope, app);
        DataLayerGateway gateway = new DataLayerGateway(_kernel);
        Response response = gateway.RefreshCache(updateDictionary);
        return response;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error refreshing cache for {0}.{1}: {2}", scope, app, ex.Message);
        throw ex;
      }
    }

    public Response RefreshCache(string scope, string app, string objectType, bool updateDictionary)
    {
      try
      {
        Initialize(scope, app);
        DataLayerGateway gateway = new DataLayerGateway(_kernel);
        Response response = gateway.RefreshCache(updateDictionary, objectType);
        return response;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error refreshing cache for {0}.{1}: {2}", scope, app, ex.Message);
        throw ex;
      }
    }

    public Response ImportCache(string scope, string app, string baseUri, bool updateDictionary)
    {
      try
      {
        Initialize(scope, app);
        DataLayerGateway gateway = new DataLayerGateway(_kernel);
        Response response = gateway.ImportCache(baseUri, updateDictionary);
        return response;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error refreshing cache for {0}.{1}: {2}", scope, app, ex.Message);
        throw ex;
      }
    }

    public Response ImportCache(string scope, string app, string objectType, string url, bool updateDictionary)
    {
      try
      {
        Initialize(scope, app);
        DataLayerGateway gateway = new DataLayerGateway(_kernel);
        Response response = gateway.ImportCache(objectType, url, updateDictionary);
        return response;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error refreshing cache for {0}.{1}: {2}", scope, app, ex.Message);
        throw ex;
      }
    }

    public Response DeleteCache(string scope, string app)
    {
      try
      {
        Initialize(scope, app);
        DataLayerGateway gateway = new DataLayerGateway(_kernel);
        Response response = gateway.DeleteCache();
        return response;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error deleting cache for {0}.{1}: {2}", scope, app, ex.Message);
        throw ex;
      }
    }

    public Response DeleteCache(string scope, string app, string objectType)
    {
      try
      {
        Initialize(scope, app);
        DataLayerGateway gateway = new DataLayerGateway(_kernel);
        Response response = gateway.DeleteCache(objectType);
        return response;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error deleting cache for {0}.{1}: {2}", scope, app, ex.Message);
        throw ex;
      }
    }
    #endregion

    public Response RefreshDataObject(string projectName, string applicationName, string objectType, DataFilter dataFilter)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer(false);

        return _dataLayer.Refresh(objectType, dataFilter);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error refreshing data object [{0}]: {1}", objectType, ex));
        throw ex;
      }
    }

    public DocumentBytes GetResourceData(string scope, string app)
    {
      DocumentBytes documentBytes = new DocumentBytes();
      string searchPath = AppDomain.CurrentDomain.BaseDirectory + _settings["AppDataPath"];
      string[] filePaths = Directory.GetFiles(searchPath, "SpreadsheetData." + scope + "." + app + ".xlsx");
      string _FileName = filePaths[0];

      byte[] _Buffer = null;

      if (_FileName.Length > 0)
      {
        System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);
        long _TotalBytes = new System.IO.FileInfo(_FileName).Length;
        _Buffer = _BinaryReader.ReadBytes((Int32)_TotalBytes);
      }
      documentBytes.Content = _Buffer;
      documentBytes.DocumentPath = searchPath;
      return documentBytes;
    }

    public byte[] GetResourceDataBytes(string scope, string app)
    {
      string searchPath = AppDomain.CurrentDomain.BaseDirectory + _settings["AppDataPath"];
      string[] filePaths = Directory.GetFiles(searchPath, scope + "." + app + ".*.mdb");

      if (filePaths.Length > 0)
      {
        string _FileName = filePaths[0];
        return System.IO.File.ReadAllBytes(_FileName);
      }
      else
        return null;
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

    private static string QueueNewRequest()
    {
      var id = Guid.NewGuid().ToString("N");
      _requests[id] = new RequestStatus()
      {
        State = State.InProgress
      };
      return id;
    }
  }

  public class ScopeComparer : IComparer<ScopeProject>
  {
    public int Compare(ScopeProject left, ScopeProject right)
    {
      string leftName = left.Name;
      if (left.DisplayName != null && left.DisplayName.Length > 0)
      {
        leftName = left.DisplayName;
      }

      string rightName = right.Name;
      if (right.DisplayName != null && right.DisplayName.Length > 0)
      {
        rightName = right.DisplayName;
      }

      // compare strings
      return string.Compare(leftName, rightName);
    }
  }

  public class ApplicationComparer : IComparer<ScopeApplication>
  {
    public int Compare(ScopeApplication left, ScopeApplication right)
    {
      string leftName = left.Name;
      if (left.DisplayName != null && left.DisplayName.Length > 0)
      {
        leftName = left.DisplayName;
      }

      string rightName = right.Name;
      if (right.DisplayName != null && right.DisplayName.Length > 0)
      {
        rightName = right.DisplayName;
      }

      // compare strings
      return string.Compare(leftName, rightName);
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