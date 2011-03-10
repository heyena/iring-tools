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
using StaticDust.Configuration;
using VDS.RDF;
using VDS.RDF.Query;
using org.iringtools.adapter.projection;
using System.ServiceModel;
using System.Security.Principal;
using org.iringtools.adapter.identity;

namespace org.iringtools.adapter
{
  public class AdapterProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private Response _response = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private List<ScopeProject> _scopes = null;
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
      string relativePath = String.Format("{0}BindingConfiguration.Adapter.xml",
            _settings["XmlPath"]
          );
      string bindingConfigurationPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );
      _kernel.Load(bindingConfigurationPath);
      InitializeIdentity();
    }
    
    #region application methods
    public List<ScopeProject> GetScopes()
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

    public Response UpdateScopes(List<ScopeProject> scopes)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        //_scopes = scopes;
                
        foreach (ScopeProject project in scopes)
        {
          ScopeProject findProject = _scopes.FirstOrDefault<ScopeProject>(o => o.Name == project.Name);

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
            _scopes.Add(project);
          }

        }

        Utility.Write<List<ScopeProject>>(_scopes, _settings["ScopesPath"], true);

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

        if (!mappingXml.Name.NamespaceName.Contains("schemas.datacontract.org"))
        {
          status.Messages.Add("Detected old mapping. Attempting to convert it...");

          Mapping mapping = new Mapping();
          _qmxfTemplateResultCache = new Dictionary<string, KeyValuePair<string, Dictionary<string, string>>>();

          #region convert graphMaps
          IEnumerable<XElement> graphMaps = mappingXml.Element("GraphMaps").Elements("GraphMap");
          foreach (XElement graphMap in graphMaps)
          {
            string dataObjectMap = graphMap.Element("DataObjectMaps").Element("DataObjectMap").Attribute("name").Value;
            RoleMap roleMap = null;

            GraphMap newGraphMap = new GraphMap();
            newGraphMap.name = graphMap.Attribute("name").Value;
            newGraphMap.dataObjectMap = dataObjectMap;
            mapping.graphMaps.Add(newGraphMap);

            ConvertClassMap(ref newGraphMap, ref roleMap, graphMap, dataObjectMap);
          }
          #endregion

          #region convert valueMaps
          IEnumerable<XElement> valueMaps = mappingXml.Element("ValueMaps").Elements("ValueMap");
          string previousValueList = String.Empty;
          ValueList newValueList = null;
          
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
              newValueList = new ValueList
              {
                name = valueList,
                valueMaps = { newValueMap }
              };
              mapping.valueLists.Add(newValueList);
              
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

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          _response.Append(Refresh(graphMap.name));
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

        string dataObjectName = String.Empty;
        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("data");
        }

        _graphMap = _mapping.FindGraphMap(graphName);
        DataObject dataObject = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == graphName.ToUpper());
        
        if (_graphMap != null)
        {
          graphName = _graphMap.name;
          dataObjectName = _graphMap.dataObjectMap;
        }
        else if (dataObject != null)
        {
          graphName = dataObject.objectName;
          dataObjectName = dataObject.objectName;
        }
        else
        {
          throw new FileNotFoundException("Requested graph or dataObject not found.");
        }

        if (limit == 0)
          limit = 100;

        _dataObjects = _dataLayer.Get(dataObjectName, filter, limit, start);
        _projectionEngine.Count = _dataLayer.GetCount(dataObjectName, filter);
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
      string projectName, string applicationName, string graphName, string className,
      string classIdentifier, string format, bool fullIndex)
    {
      string dataObjectName = String.Empty;
      
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("data");
        }

        _graphMap = _mapping.FindGraphMap(graphName);
        DataObject dataObject = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == graphName.ToUpper());

        if (_graphMap != null)
        {
          graphName = _graphMap.name;
          dataObjectName = _graphMap.dataObjectMap;
        }
        else if (dataObject != null)
        {
          graphName = dataObject.objectName;
          dataObjectName = dataObject.objectName;
        }
        else
        {
          throw new FileNotFoundException("Requested graph or dataObject not found.");
        }

        IDataObject dataObj = GetDataObject(dataObject, className, classIdentifier);

        if (dataObj != null)
        {
          return _projectionEngine.ToXml(graphName, className, classIdentifier, ref dataObj);
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

    private IDataObject GetDataObject(DataObject dataObject, string className, string classIdentifier)
    {
      DataFilter filter = new DataFilter();
        
      #region parse identifier to build data filter
      KeyValuePair<ClassMap, List<TemplateMap>> pair = _graphMap.GetClassTemplateListMapByName(className);
      ClassMap classMap = pair.Key;
      
      if (classMap != null)
      {
        string[] identifierParts = !String.IsNullOrEmpty(classMap.identifierDelimiter)
          ? classIdentifier.Split(new string[] { classMap.identifierDelimiter }, StringSplitOptions.None)
          : new string[] { classIdentifier };

        for (int i = 0; i < identifierParts.Length; i++)
        {
          string identifierPart = identifierParts[i];

          // remove fixed values from identifier
          foreach (string clsIdentifier in classMap.identifiers)
          {
            if (clsIdentifier.StartsWith("#") && clsIdentifier.EndsWith("#"))
            {
              identifierPart = identifierPart.Replace(clsIdentifier.Substring(1, clsIdentifier.Length - 2), "");
            }
          }

          // set identifier value to mapped property
          foreach (string clsIdentifier in classMap.identifiers)
          {
            if (clsIdentifier.Split('.').Length > 2)  // related property
            {
              string[] clsIdentifierParts = clsIdentifier.Split('.');
              string relatedObjectType = clsIdentifierParts[clsIdentifierParts.Length - 2];

              // get related object then assign its related properties to top level data object properties
              DataFilter relatedObjectFilter = new DataFilter();

              Expression relatedExpression = new Expression
              {
                PropertyName = clsIdentifierParts.Last(),
                Values = new Values { identifierPart }
              };

              relatedObjectFilter.Expressions.Add(relatedExpression);
              IList<IDataObject> relatedObjects = _dataLayer.Get(relatedObjectType, relatedObjectFilter, 0, 0);

              if (relatedObjects != null && relatedObjects.Count > 0)
              {
                IDataObject relatedObject = relatedObjects.First();
                DataRelationship dataRelationship = dataObject.dataRelationships.Find(c => c.relatedObjectName == relatedObjectType);

                foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
                {
                  Expression expression = new Expression();

                  if (filter.Expressions.Count > 0)
                    expression.LogicalOperator = LogicalOperator.And;

                  expression.PropertyName = propertyMap.dataPropertyName;
                  expression.Values = new Values { relatedObject.GetPropertyValue(propertyMap.relatedPropertyName).ToString() };
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
              expression.Values = new Values { identifierPart };
              filter.Expressions.Add(expression);
            }
          }
        }
      }
      #endregion

      IList<string> identifiers = _dataLayer.GetIdentifiers(dataObject.objectName, filter);
      IList<IDataObject> dataObjects = _dataLayer.Get(dataObject.objectName, identifiers);

      if (dataObjects != null && dataObjects.Count > 0)
      {
        return dataObjects.First<IDataObject>();
      }

      return null;
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

        string dataObjectName = String.Empty;
        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>("data");
        }

        _graphMap = _mapping.FindGraphMap(graphName);
        DataObject dataObject = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == graphName.ToUpper());
          
        if (_graphMap != null)
        {
          graphName = _graphMap.name;
          dataObjectName = _graphMap.dataObjectMap;
        }
        else if (dataObject != null)
        {
          graphName = dataObject.objectName;
          dataObjectName = dataObject.objectName;
        }
        else
        {
          throw new FileNotFoundException("Requested graph or dataObject not found.");
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

          _dataObjects = _dataLayer.Get(dataObjectName, filter, limit, start);

          _projectionEngine.Count = _dataLayer.GetCount(dataObjectName, filter);
        }
        else
        {
          _dataObjects = _dataLayer.Get(dataObjectName, null, limit, start);
          _projectionEngine.Count = _dataLayer.GetCount(dataObjectName, null);
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

    //public IList<IDataObject> GetDataObjects(
    //string projectName, string applicationName, string graphName, 
    //string format, XDocument xDocument)
    //{
    //  InitializeScope(projectName, applicationName);
    //  InitializeDataLayer();

    //  if (format != null)
    //  {
    //    _projectionEngine = _kernel.Get<IProjectionLayer>(format);
    //  }
    //  else
    //  {
    //    _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
    //  }

    //  IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xDocument);

    //  return dataObjects;
    //}

    public Response DeleteAll(string projectName, string applicationName)
    {
      Status status = new Status();
      status.Messages = new Messages();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          _response.Append(_semanticEngine.Delete(graphMap.name));
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

        string objectType = graphMap.dataObjectMap;
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
            _settings["UserName"] = userName;
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
        _dataObjects =_dataLayer.Get(_graphMap.dataObjectMap, identifiers);
      else
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, null);

      return _dataObjects.Count;
    }

    private long LoadDataObjectSet(string graphName, DataFilter dataFilter, int start, int limit)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      _dataObjects.Clear();

      if (dataFilter != null)
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, dataFilter, limit, start);
      else
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, null);

      long count = _dataLayer.GetCount(_graphMap.dataObjectMap, dataFilter);

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
            Applications = new List<ScopeApplication>()
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

        Utility.Write<List<ScopeProject>>(_scopes, _settings["ScopesPath"], true);
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
        Utility.Write<List<ScopeProject>>(_scopes, _settings["ScopesPath"], true);

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

    private void ConvertClassMap(ref GraphMap newGraphMap, ref RoleMap parentRoleMap, XElement classMap, string dataObjectMap)
    {
      string classId = classMap.Attribute("classId").Value;

      ClassMap newClassMap = new ClassMap();
      newClassMap.classId = classId;
      newClassMap.identifiers.Add(dataObjectMap + "." + classMap.Attribute("identifier").Value);

      if (parentRoleMap == null)
      {
        newClassMap.name = GetClassName(classId);
      }
      else
      {
        newClassMap.name = classMap.Attribute("name").Value;
        parentRoleMap.classMap = newClassMap;
      }

      List<TemplateMap> newTemplateMaps = new List<TemplateMap>();
      newGraphMap.classTemplateListMaps.Add(newClassMap, newTemplateMaps);

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
        newTemplateMap.templateId = templateId;
        newTemplateMaps.Add(newTemplateMap);

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
        newClassRoleMap.roleId = classRoleId;

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
          newRoleMap.roleId = roleMap.Attribute("roleId").Value;
          newRoleMap.name = roles[newRoleMap.roleId];

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
            newRoleMap.propertyName = dataObjectMap + "." + propertyName;

            if (!String.IsNullOrEmpty(valueList))
            {
              newRoleMap.type = RoleType.ObjectProperty;
              newRoleMap.valueList = valueList;
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

            ConvertClassMap(ref newGraphMap, ref newRoleMap, roleMap.Element("ClassMap"), dataObjectMap);
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

    public List<string> GetDataLayers()
    {
      List<string> asemblies = new List<string>();

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
                asemblies.Add(t.FullName + ", " + asm.FullName.Split(',')[0]);
              }
            }
          }
        }        
      }
      catch(Exception ex)
      {
        _logger.Error(string.Format("Error in GetDataLayers: {0}", ex), ex);
      }
      
      return asemblies;      
    }

  }
}