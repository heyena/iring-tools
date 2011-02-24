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
using System.Xml;
using System.Xml.Linq;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using StaticDust.Configuration;
using VDS.RDF;
using VDS.RDF.Query;
using org.iringtools.adapter.projection;
using System.ServiceModel;
using System.Security.Principal;
using org.iringtools.dxfr.manifest;
using org.iringtools.adapter.identity;

namespace org.iringtools.adapter
{
  public class AdapterProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private Response _response = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private ScopeProjects _scopes = null;
    private IDataLayer _dataLayer = null;
    private IIdentityLayer _identityLayer = null;
	  private IDictionary _keyRing = null;
    private ISemanticLayer _semanticEngine = null;
    private IProjectionLayer _projectionEngine = null;
    private DataDictionary _dataDictionary = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private WebHttpClient _webHttpClient = null;  // for old mapping conversion    
    private Dictionary<string, KeyValuePair<string, Dictionary<string, string>>> _qmxfTemplateResultCache = null;

    //Projection specific stuff
    private IList<IDataObject> _dataObjects = new List<IDataObject>(); // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = new Dictionary<string, List<string>>(); // dictionary of class ids and list of identifiers

    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;

    [Inject]
    public AdapterProvider(NameValueCollection settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false };
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
        _scopes = Utility.Read<ScopeProjects>(scopesPath);
      }
      else
      {
        _scopes = new ScopeProjects();
        Utility.Write<ScopeProjects>(_scopes, scopesPath);
      }

      _response = new Response();
      _response.StatusList = new List<Status>();
      _kernel.Bind<Response>().ToConstant(_response);

      string relativePath = String.Format("{0}BindingConfiguration.Adapter.xml",
            _settings["XmlPath"]
          );

      //Ninject Extension requires fully qualified path.
      string bindingConfigurationPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );

      _kernel.Load(bindingConfigurationPath);

      InitializeIdentity();
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
    public Response UpdateScopes(ScopeProjects scopes)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        // _scopes = scopes;

        foreach (ScopeProject project in scopes)
        {
          ScopeProject findProject = scopes.FirstOrDefault<ScopeProject>(o => o.Name == project.Name);

          if (findProject != null)
          {

            findProject.Name = project.Name;
            findProject.Description = project.Description;

            foreach (ScopeApplication application in project.Applications)
            {

              ScopeApplication findApplication = findProject.Applications.FirstOrDefault<ScopeApplication>(o => o.Name == application.Name);

              if (findApplication != null)
              {
                findApplication.Name = application.Name;
                findApplication.Description = application.Description;
              }
              else
              {
                findProject.Applications.Add(application);
              }

            }

          }
          else
          {
            scopes.Add(project);
          }

        }

        Utility.Write<ScopeProjects>(scopes, _settings["ScopesPath"], true);
        _scopes = scopes; //Rest Global variable.

        status.Messages.Add("Scopes have been updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateScopes: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error saving scopes: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    public Response DeleteScope(string projectName, string applicationName)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        DeleteScope();

        status.Messages.Add(String.Format("Scope {0} has been deleted successfully.", _settings["Scope"]));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in DeleteScope: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting scope: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

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
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        XDocument bindingConfiguration = new XDocument();
        bindingConfiguration.Add(binding);

        bindingConfiguration.Save(_settings["BindingConfigurationPath"]);

        status.Messages.Add("BindingConfiguration was saved.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateBindingConfiguration: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error updating the binding configuration: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }
    #endregion

    #region adapter methods
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.GetDictionary();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetDictionary: {0}", ex));
        throw new Exception(string.Format("Error getting data dictionary: {0}", ex));
      }
    }

    public Mapping GetMapping(string projectName, string applicationName)
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
      Status status = new Status();
      status.Messages = new Messages();
      string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings["XmlPath"], projectName, applicationName);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        if (mappingXml.Name.NamespaceName.Contains("schemas.datacontract.org"))
        {
          status.Messages.Add("Detected old mapping. Attempting to convert it...");

          Mapping mapping = new Mapping();
          _qmxfTemplateResultCache = new Dictionary<string, KeyValuePair<string, Dictionary<string, string>>>();

          #region convert graphMaps
          IEnumerable<XElement> graphMaps = mappingXml.Element("GraphMaps").Elements("GraphMap");
          foreach (XElement graphMap in graphMaps)
          {
            string dataObjectName = graphMap.Element("DataObjectMaps").Element("DataObjectMap").Attribute("name").Value;
            RoleMap roleMap = null;

            GraphMap newGraphMap = new GraphMap();
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
            ValueMap newValueMap = new ValueMap
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

          status.Messages.Add("Old mapping has been converted sucessfully.");

          Utility.Write<Mapping>(mapping, path, true);
        }
        else
        {
          Mapping mapping = Utility.DeserializeDataContract<Mapping>(mappingXml.ToString());
          Utility.Write<Mapping>(mapping, path, true);
        }

        status.Messages.Add("Mapping file has been updated to path [" + path + "] successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateMapping: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error saving mapping file to path [{0}]: {1}", path, ex));
      }

      _response.Append(status);
      return _response;
    }

    //public Response RefreshAll(string projectName, string applicationName)
    //{
    //  Status status = new Status();
    //  status.Messages = new Messages();
    //  try
    //  {
    //    status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

    //    InitializeScope(projectName, applicationName);
    //    InitializeDataLayer();

    //    DateTime start = DateTime.Now;

    //    foreach (GraphMap graphMap in _mapping.graphMaps)
    //    {
    //      _response.Append(Refresh(graphMap.name));
    //    }

    //    DateTime end = DateTime.Now;
    //    TimeSpan duration = end.Subtract(start);

    //    status.Messages.Add(String.Format("RefreshAll() completed in [{0}:{1}.{2}] minutes.",
    //      duration.Minutes, duration.Seconds, duration.Milliseconds));
    //  }
    //  catch (Exception ex)
    //  {
    //    _logger.Error(string.Format("Error in RefreshAll: {0}", ex));

    //    status.Level = StatusLevel.Error;
    //    status.Messages.Add(string.Format("Error refreshing all graphs: {0}", ex));
    //  }

    //  _response.Append(status);
    //  return _response;
    //}

    public Response Refresh(string projectName, string applicationName, string graphName)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        _response.Append(Refresh(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error refreshing graph [{0}]: {1}", graphName, ex));
      }

      _response.Append(status);
      return _response;
    }

    public XDocument GetDataProjection(
		string projectName, string applicationName, string graphName, 
		DataFilter filter, string format, int start, int limit, bool fullIndex)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        IList<string> index = new List<string>();

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("data");
        }

        if (limit == 0)
          limit = 100;

        _dataObjects = _dataLayer.Get(graphName, filter, limit, start);

        _projectionEngine.Count = _dataLayer.GetCount(graphName, filter);

       // _projectionEngine.FullIndex = fullIndex;
        return _projectionEngine.ToXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }
    public XDocument GetDataProjection(
      string projectName, string applicationName, string graphName, 
      string identifier, string format, bool fullIndex)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        IList<string> identifiers = new List<string>() { identifier };

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("data");
        }

        _dataObjects = _dataLayer.Get(graphName, identifiers);

        _projectionEngine.FullIndex = fullIndex;
        return _projectionEngine.ToXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public XDocument GetDataProjection(
      string projectName, string applicationName, string graphName,
      string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex, 
      NameValueCollection parameters)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        IList<string> index = new List<string>();

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("data");
        }

        if (limit == 0)
          limit = 100;

        DataFilter filter = new DataFilter();
        
        if (parameters != null)
        {
          List<Expression> expressions = new List<Expression>();
          foreach (string key in parameters.AllKeys)
          {
            string[] expectedParameters = { 
              "format", 
              "start", 
              "limit", 
              "sortBy", 
              "sortOrder",
              "indexStyle",
            };

            if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
            {
              string value = parameters[key];

              Expression expression = new Expression
              {
                PropertyName = key,
                RelationalOperator = library.RelationalOperator.EqualTo,
                Values = new Values { value },
                IsCaseSensitive = false,
              };

              expressions.Add(expression);
            }
          }
          filter.Expressions = expressions;

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

          _dataObjects = _dataLayer.Get(graphName, filter, limit, start);

          _projectionEngine.Count = _dataLayer.GetCount(graphName, filter);
        }
        else
        {
          _dataObjects = _dataLayer.Get(graphName, null);

          _projectionEngine.Count = _dataLayer.GetCount(graphName, null);
        }

        _projectionEngine.FullIndex = fullIndex; 
        return _projectionEngine.ToXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public XDocument GetProjection(
		string projectName, string applicationName, string graphName, 
    string identifier, string format, bool fullIndex)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        IList<string> identifiers = new List<string>() { identifier };

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
        }

        LoadDataObjectSet(graphName, identifiers);

        _projectionEngine.FullIndex = fullIndex;
        return _projectionEngine.ToXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public XDocument GetProjection(
		string projectName, string applicationName, string graphName, 
		DataFilter filter, 
		string format, int start, int limit, bool fullIndex)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
        }

        if (limit == 0)
          limit = 100;

        if (_projectionEngine.GetType().BaseType == typeof(BasePart7ProjectionEngine))
        {
          ((BasePart7ProjectionEngine)_projectionEngine).ProjectDataFilter(_dataDictionary, ref filter, graphName);
        }

        _projectionEngine.Count = LoadDataObjectSet(graphName, filter, start, limit);

        _projectionEngine.FullIndex = fullIndex;
        return _projectionEngine.ToXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }
    public XDocument GetProjection(
      string projectName, string applicationName, string graphName, 
      string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
      NameValueCollection parameters)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
        }

        if (limit == 0)
          limit = 100;

        DataFilter filter = new DataFilter();
        
        if (parameters != null)
        {
          List<Expression> expressions = new List<Expression>();
          foreach (string key in parameters.AllKeys)
          {
            string[] expectedParameters = { 
              "format", 
              "start", 
              "limit", 
              "sortBy", 
              "sortOrder",
              "indexStyle",
            };

            if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
            {
              string value = parameters[key];

              Expression expression = new Expression
              {
                PropertyName = key,
                RelationalOperator = library.RelationalOperator.EqualTo,
                Values = new Values { value },
              };

              expressions.Add(expression);
            }
          }
          filter.Expressions = expressions;

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

          if (_projectionEngine.GetType().BaseType == typeof(BasePart7ProjectionEngine))
          {
            ((BasePart7ProjectionEngine)_projectionEngine).ProjectDataFilter(_dataDictionary, ref filter, graphName);
          }

          _projectionEngine.Count = LoadDataObjectSet(graphName, filter, start, limit);
        }
        else
        {
          _projectionEngine.Count = LoadDataObjectSet(graphName, null);
        }

        _projectionEngine.FullIndex = fullIndex;
        return _projectionEngine.ToXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public IList<IDataObject> GetDataObjects(
		string projectName, string applicationName, string graphName, 
		string format, XDocument xDocument)
    {
      InitializeScope(projectName, applicationName);
      InitializeDataLayer();

      if (format != null)
      {
        _projectionEngine = _kernel.Get<IProjectionLayer>(format);
      }
      else
      {
        _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
      }

      IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xDocument);

      return dataObjects;
    }

    //public Response DeleteAll(string projectName, string applicationName)
    //{
    //  Status status = new Status();
    //  status.Messages = new Messages();
    //  try
    //  {
    //    status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

    //    InitializeScope(projectName, applicationName);

    //    _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

    //    foreach (GraphMap graphMap in _mapping.graphMaps)
    //    {
    //      _response.Append(_semanticEngine.Delete(graphMap.name));
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    _logger.Error(string.Format("Error deleting all graphs: {0}", ex));

    //    status.Level = StatusLevel.Error;
    //    status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
    //  }

    //  _response.Append(status);
    //  return _response;
    //}

    public Response Delete(string projectName, string applicationName, string graphName)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}.{2}", projectName, applicationName, graphName);

        InitializeScope(projectName, applicationName);

        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

        _response.Append(_semanticEngine.Delete(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting {0} graphs: {1}", graphName, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    public Response Post(string projectName, string applicationName, string graphName, string format, XDocument xml)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xml);
        response = _dataLayer.Post(dataObjects);

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
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


    public Response DeleteIndividual(string projectName, string applicationName, string graphName, string identifier)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        GraphMap graphMap = _mapping.FindGraphMap(graphName);

        string objectType = graphMap.dataObjectName;
        response = _dataLayer.Delete(objectType, new List<String> { identifier });

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
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

    private void InitializeDataLayer()
    {
      try
      {
        if (!_isDataLayerInitialized)
        {
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
          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");
          _dataDictionary = _dataLayer.GetDictionary();
          _kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);
          _isDataLayerInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void InitializeIdentity()
    {
      try
      {
        _identityLayer = _kernel.Get<IIdentityLayer>("IdentityLayer");
        _keyRing = _identityLayer.GetKeyRing();

        _kernel.Bind<IDictionary>().ToConstant(_keyRing).Named("KeyRing");

        if (_keyRing.Count > 0)
        {  
        if (_keyRing["Provider"].ToString() == "WindowsAuthenticationProvider")
        {
          string userName = _keyRing["Name"].ToString();
          _settings.Add("UserName", userName);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing identity: {0}", ex));
        throw new Exception(string.Format("Error initializing identity: {0})", ex));
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

    private void UpdateScopes(string projectName, string projectDescription, string applicationName, string applicationDescription)
    {
      try
      {
        bool projectExists = false;
        bool applicationExists = false;
        ScopeProject existingProject = null;

        foreach (ScopeProject project in _scopes)
        {
          if (project.Name.ToUpper() == projectName.ToUpper())
          {
            foreach (ScopeApplication application in project.Applications)
            {
              if (application.Name.ToUpper() == applicationName.ToUpper())
              {
                applicationExists = true;
                break;
              }

              existingProject = project;
              projectExists = true;
              break;
            }
          }
        }

        // project does not exist, add it
        if (!projectExists)
        {
          ScopeProject newProject = new ScopeProject()
          {
            Name = projectName,
            Description = projectDescription,
            Applications = new ScopeApplications()
            {
              new ScopeApplication()
              {
                Name = applicationName,
                Description = applicationDescription,
              }
            }
          };

          _scopes.Add(newProject);
        }
        else if (!applicationExists)
        {
          existingProject.Applications.Add(
            new ScopeApplication()
            {
              Name = applicationName,
              Description = applicationDescription,
            }
          );
        }

        Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateScopes: {0}", ex));
        throw ex;
      }
    }

    private void DeleteScope()
    {
      try
      {
        //Clean up ScopeList
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

        //Save ScopeList
        Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);

        //BindingConfig
        File.Delete(_settings["BindingConfigurationPath"]);

      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in DeleteScope: {0}", ex));
        throw ex;
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

    private void ConvertClassMap(ref GraphMap newGraphMap, ref RoleMap parentRoleMap, XElement classMap, string dataObjectName)
    {
      string classId = classMap.Attribute("classId").Value;

      ClassMap newClassMap = new ClassMap();
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
          continue; // class role not found, skip this template
        }

        IEnumerable<XElement> roleMaps = templateMap.Element("RoleMaps").Elements("RoleMap");
        string templateId = templateMap.Attribute("templateId").Value;

        TemplateMap newTemplateMap = new TemplateMap();
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

        RoleMap newClassRoleMap = new RoleMap();
        newClassRoleMap.type = RoleType.Possessor;
        newTemplateMap.roleMaps.Add(newClassRoleMap);
        newClassRoleMap.id = classRoleId;

        Dictionary<string, string> roles = templateNameRolesPair.Value;
        newClassRoleMap.name = roles[classRoleId];

        for (int i = 0; i < roleMaps.Count(); i++)
        {
          XElement roleMap = roleMaps.ElementAt(i);

          string value = String.Empty;
          try { value = roleMap.Attribute("value").Value; }
          catch (Exception) { }

          string reference = String.Empty;
          try { reference = roleMap.Attribute("reference").Value; }
          catch (Exception) { }

          string propertyName = String.Empty;
          try { propertyName = roleMap.Attribute("propertyName").Value; }
          catch (Exception) { }

          string valueList = String.Empty;
          try { valueList = roleMap.Attribute("valueList").Value; }
          catch (Exception) { }

          RoleMap newRoleMap = new RoleMap();
          newTemplateMap.roleMaps.Add(newRoleMap);
          newRoleMap.id = roleMap.Attribute("roleId").Value;
          newRoleMap.name = roles[newRoleMap.id];

          if (!String.IsNullOrEmpty(value))
          {
            newRoleMap.type = RoleType.FixedValue;
            newRoleMap.value = value;
          }
          else if (!String.IsNullOrEmpty(reference))
          {
            newRoleMap.type = RoleType.Reference;
            newRoleMap.value = reference;
          }
          else if (!String.IsNullOrEmpty(propertyName))
          {
            newRoleMap.propertyName = dataObjectName + "." + propertyName;

            if (!String.IsNullOrEmpty(valueList))
            {
              newRoleMap.type = RoleType.ObjectProperty;
              newRoleMap.valueListName = valueList;
            }
            else
            {
              newRoleMap.type = RoleType.DataProperty;
              newRoleMap.dataType = roleMap.Attribute("dataType").Value;
            }
          }

          if (roleMap.HasElements)
          {
            newRoleMap.type = RoleType.Reference;
            newRoleMap.value = roleMap.Attribute("dataType").Value;

            ConvertClassMap(ref newGraphMap, ref newRoleMap, roleMap.Element("ClassMap"), dataObjectName);
          }
        }
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

    public DataLayers GetDataLayers()
    {
      DataLayers dataLayerAssemblies = new DataLayers();

      try
      {
        //string binaryPath = @"file:///" + _settings["BaseDirectoryPath"] + "bin";        

        System.Type ti = typeof(IDataLayer);
        foreach (System.Reflection.Assembly asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
          //if (!asm.IsDynamic && Path.GetDirectoryName(asm.CodeBase) == binaryPath)
          {
            foreach (System.Type t in asm.GetTypes())
            {
              if (!t.IsInterface && ti.IsAssignableFrom(t))
              {
                DataLayer dataLayer = new DataLayer { Assembly = t.FullName, Name = asm.FullName.Split(',')[0] };
                dataLayerAssemblies.Add(dataLayer);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetDataLayers: {0}", ex), ex);
      }

      return dataLayerAssemblies;
    }

    public Response SaveDataLayerConfig(string projectName, string applicationName, XElement configuration)
    {
      string dlXml = configuration.Element("datalayerName").Value;
      XElement dlcXml = configuration.Element("datalayerConfiguration");

      string bindConf = string.Empty;
      
      string scope = String.Format("{0}.{1}", projectName, applicationName);
      string appSettingsPath = String.Format("{0}{1}.config",  _settings["XmlPath"],  scope);
      string relativePath = String.Format("{0}BindingConfiguration.{1}.xml",  _settings["XmlPath"],  scope);
      string bindingConfigurationPath = Path.Combine(_settings["BaseDirectoryPath"], relativePath);
      _settings["Scope"] = scope;
      _settings["BindingConfigurationPath"] = bindingConfigurationPath;


      bindingConfigurationPath = _settings["BindingConfigurationPath"];

        XElement binding = new XElement("module",
          new XAttribute("name", _settings["Scope"]),
          new XElement("bind",
            new XAttribute("name", "DataLayer"),
            new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
            new XAttribute("to", dlXml)
          )
        );

        binding.Save(bindingConfigurationPath);
        _kernel.Load(bindingConfigurationPath);
      
      _dataLayer = _kernel.Get<IDataLayer>("DataLayer");
      return _dataLayer.Configure(dlcXml);
    }
  }
}