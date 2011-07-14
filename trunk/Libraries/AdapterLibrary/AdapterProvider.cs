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
using System.Web;
using System.Xml;
using System.Xml.Linq;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using org.ids_adi.qmxf;
using org.iringtools.adapter.identity;
using org.iringtools.library;
using org.iringtools.legacy;
using org.iringtools.mapping;
using org.iringtools.utility;
using StaticDust.Configuration;
using org.iringtools.adapter.projection;

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
    private mapping.Mapping _mapping = null;
    private mapping.GraphMap _graphMap = null;
    private DataObject _dataObjDef = null;
    private bool _isResourceGraph = false;
    private bool _isProjectionPart7 = false;
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
        _scopes = scopes; //Update global variable

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

    private mapping.Mapping LoadMapping(string path, ref Status status)
    {
      XElement mappingXml = Utility.ReadXml(path);

      return LoadMapping(mappingXml, ref status);
    }

    private mapping.Mapping LoadMapping(XElement mappingXml, ref Status status)
    {
      mapping.Mapping mapping = new mapping.Mapping();

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
          try
          {
            mapping = ConvertMapping(mappingXml);
          }
          catch (Exception oldEx)
          {
            status.Messages.Add("Legacy mapping could not be converted.");
          }
        }

        status.Messages.Add("Legacy mapping has been converted sucessfully.");
      }
      else
      {
        mapping = Utility.DeserializeDataContract<mapping.Mapping>(mappingXml.ToString());
      }

      return mapping;
    }

    public Response UpdateMapping(string projectName, string applicationName, XElement mappingXml)
    {
      Status status = new Status();
      status.Messages = new Messages();
      string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings["XmlPath"], projectName, applicationName);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        mapping.Mapping mapping = LoadMapping(mappingXml, ref status);

        Utility.Write<mapping.Mapping>(mapping, path, true);

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

    //DataFilter List
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName, 
        DataFilter filter, string format, int start, int limit, bool fullIndex)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, format);

        IList<string> index = new List<string>();

        if (limit == 0)
          limit = 100;

        _dataObjects = _dataLayer.Get(_dataObjDef.objectName, filter, limit, start);
        _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, filter);
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
      string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
      NameValueCollection parameters)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, format);

        IList<string> index = new List<string>();

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
                RelationalOperator = RelationalOperator.EqualTo,
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

          _dataObjects = _dataLayer.Get(_dataObjDef.objectName, filter, limit, start);
          _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, filter);
        }
        else
        {
          _dataObjects = _dataLayer.Get(_dataObjDef.objectName, new DataFilter(), limit, start);
          _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, null);
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

    //Individual
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName, string className,
       string classIdentifier, string format, bool fullIndex)
    {
      string dataObjectName = String.Empty;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, format);

        if (_isResourceGraph)
        {
          _dataObjects = GetDataObject(className, classIdentifier);
        }
        else
        {
          List<string> identifiers = new List<string> { classIdentifier };
          _dataObjects = _dataLayer.Get(_dataObjDef.objectName, identifiers);
        }
        _projectionEngine.Count = _dataObjects.Count;

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
          throw new Exception("Data object with identifier [" + classIdentifier + "] not found.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    //public IList<IDataObject> GetDataObjects(
    //    string projectName, string applicationName, string graphName,
    //    string format, XDocument xDocument)
    //{
    //  InitializeScope(projectName, applicationName);
    //  InitializeDataLayer();

    //  if (format != null)
    //  {
    //    _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
    //  }
    //  else
    //  {
    //    _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
    //  }

    //  IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xDocument);

    //  return dataObjects;
    //}

    private IList<IDataObject> GetDataObject(string className, string classIdentifier)
    {
      DataFilter filter = new DataFilter();

      IList<string> identifiers = new List<string>{ classIdentifier };

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

        _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xml);
        response = _dataLayer.Post(dataObjects);

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Success;
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

    public Response DeleteIndividual(string projectName, string applicationName, string graphName, string identifier)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        mapping.GraphMap graphMap = _mapping.FindGraphMap(graphName);

        string objectType = graphMap.dataObjectName;
        response = _dataLayer.Delete(objectType, new List<String> { identifier });

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Success;
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

          _settings["ProjectName"] = projectName;
          _settings["ApplicationName"] = applicationName;
          _settings["Scope"] = scope;

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

          if (loadDataLayer)
          {

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
          }

          _settings["DBDictionaryPath"] = String.Format("{0}DatabaseDictionary.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          string mappingPath = String.Format("{0}Mapping.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          if (File.Exists(mappingPath))
          {
            try
            {
              _mapping = Utility.Read<mapping.Mapping>(mappingPath);
            }
            catch (Exception legacyEx)
            {
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

    private void InitializeProjection(string resourceName, string format)
    {
      try
      {
        _graphMap = _mapping.FindGraphMap(resourceName);
        _dataObjDef = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == resourceName.ToUpper());

        if (_graphMap != null)
        {
          _isResourceGraph = true;
          _dataObjDef = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == _graphMap.dataObjectName.ToUpper());
        }
        
        if (_dataObjDef == null)
        {
          throw new FileNotFoundException("Requested graph or dataObject not found.");
        }

        if (format != null)
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
        else if (_isResourceGraph)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("xml");
          _isProjectionPart7 = true;
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("data");
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

          try
          {
            _dataLayer = (IDataLayer)_kernel.Get<IDataLayer2>("DataLayer");
          }
          catch (Exception ex)
          {
            //_logger.Debug(ex.ToString());
            _dataLayer = _kernel.Get<IDataLayer>("DataLayer");
          }

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

          foreach(legacy.RoleMap roleMap in templateMap.roleMaps)
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
          continue; // class role not found, skip this template
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
          catch (Exception ex) {
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
              if (!t.IsInterface && ti.IsAssignableFrom(t) && t.IsAbstract.Equals(false))
              {
                bool configurable = t.BaseType.Equals(typeof(BaseConfigurableDataLayer));
                string name = asm.FullName.Split(',')[0];
                string assembly = string.Format("{0}, {1}", t.FullName, name);
                DataLayer dataLayer = new DataLayer { Assembly = assembly, Name = name, Configurable = configurable };
                dataLayerAssemblies.Add(dataLayer);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetDataLayers: {0}", ex), ex);

        DataLayer dataLayer = new DataLayer { 
          Assembly = "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary", 
          Name = "NHibernateLibrary", 
          Configurable = true
        };
        dataLayerAssemblies.Add(dataLayer);
      }

      return dataLayerAssemblies;
    }

    public Response Configure(string projectName, string applicationName, HttpRequest httpRequest)
    {
      Response response = new Response();
      response.Messages = new Messages();

      try
      {
        string savedFileName = string.Empty;

        foreach (string file in httpRequest.Files)
        {
          HttpPostedFile hpf = httpRequest.Files[file] as HttpPostedFile;
          if (hpf.ContentLength == 0)
            continue;

          savedFileName = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          _settings["XmlPath"],
          Path.GetFileName(hpf.FileName));
          hpf.SaveAs(savedFileName);
        }

        InitializeScope(projectName, applicationName, false);

        string dataLayer = httpRequest.Form["DataLayer"];
        XElement configuration = Utility.DeserializeXml<XElement>(httpRequest.Form["Configuration"]);

        XElement binding = new XElement("module",
        new XAttribute("name", _settings["Scope"]),
          new XElement("bind",
            new XAttribute("name", "DataLayer"),
            new XAttribute("service", "org.iringtools.legacy.IDataLayer2, iRINGLibrary"),
            new XAttribute("to", dataLayer)
          )
        );

        binding.Save(_settings["BindingConfigurationPath"]);
        _kernel.Load(_settings["BindingConfigurationPath"]);

        InitializeDataLayer(false);

        ((IDataLayer2)_dataLayer).Configure(configuration);

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

        return ((IDataLayer2)_dataLayer).GetConfiguration();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetConfiguration: {0}", ex));
        throw new Exception(string.Format("Error getting configuration: {0}", ex));
      }
    }
  }

}