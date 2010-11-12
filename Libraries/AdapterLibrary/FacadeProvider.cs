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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Xml.Linq;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;
using StaticDust.Configuration;
using VDS.RDF.Query;
using org.iringtools.common.mapping;
using System.ServiceModel;
using System.Security.Principal;


namespace org.iringtools.facade
{
  public class FacadeProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FacadeProvider));
    private Response _response = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private IDataLayer _dataLayer = null;
    private ISemanticLayer _semanticEngine = null;
    private List<ScopeProject> _scopes = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private WebHttpClient _webHttpClient = null;
    private IList<IDataObject> _dataObjects = new List<IDataObject>();
    private IProjectionLayer _projectionEngine = null;


    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;

    [Inject]
    public FacadeProvider(NameValueCollection settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false };
      _kernel = new StandardKernel(ninjectSettings, new AdapterModule());

      _kernel.Load(new XmlExtensionModule());
      _settings = _kernel.Get<AdapterSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      if (ServiceSecurityContext.Current != null)
      {
        IIdentity identity = ServiceSecurityContext.Current.PrimaryIdentity;
        _settings["UserName"] = identity.Name;
      }

      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string rdsUri = _settings["ReferenceDataServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));

        webProxy.Credentials = _settings.GetProxyCredential();

        _webHttpClient = new WebHttpClient(rdsUri, null, webProxy);
      }
      else
      {
        _webHttpClient = new WebHttpClient(rdsUri);
      }
      #endregion

      string scopesPath = String.Format("{0}Scopes.xml", _settings["XmlPath"]);
      _settings["ScopesPath"] = scopesPath;

      if (File.Exists(scopesPath))
      {
        _scopes = Utility.Read<List<ScopeProject>>(scopesPath);
      }
      else
      {
        _scopes = new List<ScopeProject>();
        Utility.Write<List<ScopeProject>>(_scopes, scopesPath);
      }

      _response = new Response();
      _response.StatusList = new List<Status>();
      _kernel.Bind<Response>().ToConstant(_response);
    }

    public Response Delete(string scope, string app, string graph)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}.{2}", scope, app, graph);

        InitializeScope(scope, app);
        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");
        _response.Append(_semanticEngine.Delete(graph));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting {0} graphs: {1}", graph, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    private void InitializeScope(string projectName, string applicationName)
    {
      try
      {
        if (!_isScopeInitialized)
        {
          bool isScopeValid = false;
          foreach (ScopeProject project in _scopes)
          {
            if (project.Name.ToUpper() == projectName.ToUpper())
            {
              foreach (ScopeApplication application in project.Applications)
              {
                if (application.Name.ToUpper() == applicationName.ToUpper())
                {
                  isScopeValid = true;
                }
              }
            }
          }

          string scope = String.Format("{0}.{1}", projectName, applicationName);

          if (!isScopeValid) throw new Exception(String.Format("Invalid scope [{0}].", scope));

          _settings.Add("ProjectName", projectName);
          _settings.Add("ApplicationName", applicationName);
          _settings.Add("Scope", scope);

          string appSettingsPath = String.Format("{0}{1}.config",
            _settings["XmlPath"],
            scope
          );

          if (File.Exists(appSettingsPath))
          {
            AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
            _settings.AppendSettings(appSettings);
          }
          string relativePath = String.Format("{0}BindingConfiguration.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          //Ninject Extension requires fully qualified path.
          string bindingConfigurationPath = Path.Combine(
            _settings["BaseDirectoryPath"],
            relativePath
          );

          _settings["BindingConfigurationPath"] = bindingConfigurationPath;

          if (!File.Exists(bindingConfigurationPath))
          {
            XElement binding = new XElement("module",
              new XAttribute("name", _settings["Scope"]),
              new XElement("bind",
                new XAttribute("name", "DataLayer"),
                new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
                new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary")
              )
            );

            binding.Save(bindingConfigurationPath);
          }

          _kernel.Load(bindingConfigurationPath);

          string mappingPath = String.Format("{0}Mapping.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          if (File.Exists(mappingPath))
          {
            _mapping = Utility.Read<Mapping>(mappingPath);
          }
          else
          {
            _mapping = new Mapping();
            Utility.Write<Mapping>(_mapping, mappingPath);
          }
          _kernel.Bind<Mapping>().ToConstant(_mapping);

          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    public Response Pull(string scope, string app, string graph, Request request)
    {
      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}", scope, app);

        InitializeScope(scope, app);
        InitializeDataLayer();

        DateTime startTime = DateTime.Now;

        #region move this portion to dotNetRdfEngine?
        if (!request.ContainsKey("targetEndpointUri"))
          throw new Exception("Target Endpoint Uri is required");

        string targetEndpointUri = request["targetEndpointUri"];

        if (!request.ContainsKey("targetGraphBaseUri"))
          throw new Exception("Target graph uri is required");

        string targetGraphBaseUri = request["targetGraphBaseUri"];
        _settings.Add("TargetGraphBaseUri", targetGraphBaseUri);

        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(targetEndpointUri), targetGraphBaseUri);

        if (request.ContainsKey("targetCredentials"))
        {
          string targetCredentialsXML = request["targetCredentials"];
          WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);

          if (targetCredentials.isEncrypted)
            targetCredentials.Decrypt();

          endpoint.SetCredentials(targetCredentials.GetNetworkCredential().UserName, targetCredentials.GetNetworkCredential().Password, targetCredentials.GetNetworkCredential().Domain);
        }

        string proxyHost = _settings["ProxyHost"];
        string proxyPort = _settings["ProxyPort"];
        if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
        {
          WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));

          WebProxyCredentials proxyCrendentials = _settings.GetWebProxyCredentials();
          if (proxyCrendentials != null)
          {
            endpoint.UseCredentialsForProxy = true;
            webProxy.Credentials = _settings.GetProxyCredential();
          }
          endpoint.SetProxy(webProxy.Address);
          endpoint.SetProxyCredentials(proxyCrendentials.userName, proxyCrendentials.password);
        }

        VDS.RDF.Graph resultGraph = endpoint.QueryWithResultGraph("CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}");
        #endregion

        // call RdfProjectionEngine to fill data objects from a given graph
        _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        TextWriter textWriter = new StringWriter(sb);
        VDS.RDF.Writing.FastRdfXmlWriter rdfWriter = new VDS.RDF.Writing.FastRdfXmlWriter();
        rdfWriter.Save(resultGraph, textWriter);
        XDocument xDocument = XDocument.Parse(sb.ToString());

        _dataObjects = _projectionEngine.ToDataObjects(graph, ref xDocument);

        // post data objects to data layer
        _dataLayer.Post(_dataObjects);

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);

        status.Messages.Add(string.Format("Graph [{0}] has been posted to legacy system successfully.", graph));

        status.Messages.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Pull(): ", ex);

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error pulling graph: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    private void InitializeDataLayer()
    {
      try
      {
        if (!_isDataLayerInitialized)
        {
          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");

          _isDataLayerInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    public Response Refresh(string scope, string app, string graph)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}", scope, app);

        InitializeScope(scope, app);
        InitializeDataLayer();

        _response.Append(Refresh(graph));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error refreshing graph [{0}]: {1}", graph, ex));
      }

      _response.Append(status);
      return _response;
    }

    private Response Refresh(string graphName)
    {
      _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

      _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

      LoadDataObjectSet(graphName, null);

      XDocument rdf = _projectionEngine.ToXml(graphName, ref _dataObjects);

      return _semanticEngine.Refresh(graphName, rdf);
    }

    public Response RefreshAll(string projectName, string applicationName)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        DateTime start = DateTime.Now;

        foreach (GraphMap graphMap in _mapping.GraphMaps)
        {
          _response.Append(Refresh(graphMap.Name));
        }

        DateTime end = DateTime.Now;
        TimeSpan duration = end.Subtract(start);

        status.Messages.Add(String.Format("RefreshAll() completed in [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in RefreshAll: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error refreshing all graphs: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    private void LoadDataObjectSet(string graphName, IList<string> identifiers)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      _dataObjects.Clear();

      if (identifiers != null)
        _dataObjects = _dataLayer.Get(_graphMap.DataObjectName, identifiers);
      else
        _dataObjects = _dataLayer.Get(_graphMap.DataObjectName, null);
    }

    public Response DeleteAll(string projectName, string applicationName)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

        foreach (GraphMap graphMap in _mapping.GraphMaps)
        {
          _response.Append(_semanticEngine.Delete(graphMap.Name));
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting all graphs: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    public VersionInfo GetVersion()
    {
      System.Version version = this.GetType().Assembly.GetName().Version;

      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }

  }
}
