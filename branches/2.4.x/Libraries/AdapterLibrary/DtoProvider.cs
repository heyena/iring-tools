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
using System.Xml;
using System.Xml.Linq;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using org.iringtools.adapter.projection;
using System.ServiceModel;
using System.Security.Principal;
using System.Text;
using org.iringtools.mapping;
using org.iringtools.dxfr.manifest;
using org.iringtools.adapter.identity;
using Microsoft.ServiceModel.Web;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.ServiceModel.Web;
using Newtonsoft.Json;

namespace org.iringtools.adapter
{
  public class DtoProvider : BaseProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProvider));

    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private ScopeProjects _scopes = null;
    private IDataLayer2 _dataLayer = null;
    private DataDictionary _dataDictionary = null;
    private Mapping _mapping = null;
    private IIdentityLayer _identityLayer = null;
    private IDictionary _keyRing = null;
    private GraphMap _graphMap = null;
    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;
    protected string _fixedIdentifierBoundary = "#";
    private static ConcurrentDictionary<string, RequestStatus> _requests = 
      new ConcurrentDictionary<string, RequestStatus>();

    private static string QueueNewRequest()
    {
      var id = Guid.NewGuid().ToString("N");
      _requests[id] = new RequestStatus()
      {
        State = State.InProgress
      };
      return id;
    }

    [Inject]
    public DtoProvider(NameValueCollection settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false };
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

      if (!String.IsNullOrEmpty(_settings["fixedIdentifierBoundary"]))
      {
        _fixedIdentifierBoundary = _settings["fixedIdentifierBoundary"];
      }

      if (ServiceSecurityContext.Current != null)
      {
        IIdentity identity = ServiceSecurityContext.Current.PrimaryIdentity;
        _settings["UserName"] = identity.Name;
      }

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

      string bindingConfigurationPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );

      _kernel.Load(bindingConfigurationPath);
      InitializeIdentity();
    }

    public VersionInfo GetVersion()
    {
      System.Version version = this.GetType().Assembly.GetName().Version;

      return new org.iringtools.library.VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }

    public Key GetKey(GraphMap graphMap, ClassMap classMap, string identifier)
    {
      foreach (var classTemplateMap in graphMap.classTemplateMaps)
      {
        foreach (var templateMap in classTemplateMap.templateMaps)
        {
          foreach (var roleMap in templateMap.roleMaps)
          {
            if ((roleMap.type == RoleType.Property ||
                 roleMap.type == RoleType.DataProperty ||
                 roleMap.type == RoleType.ObjectProperty) &&
                 identifier.ToLower() == roleMap.propertyName.ToLower())
            {
              Key key = new Key()
              {
                classId = classMap.id,
                templateId = templateMap.id,
                roleId = roleMap.id
              };

              return key;
            }
          }
        }
      }

      string error = string.Format(
        "Property [{0}], which is class identifier of class [{1}], is not mapped to any role in graph [{2}].",
        identifier, classMap.name, graphMap.name);

      throw new Exception(error);
    }
    
    public Manifest GetManifest(string scope, string app, string graph)
    {
      Manifest manifest = new Manifest()
      {
        graphs = new Graphs(),
        version = "2.3.1",
        valueListMaps = new ValueListMaps()
      };

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        DataDictionary dataDictionary = _dataLayer.GetDictionary();

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          Graph manifestGraph = null;

          if (string.IsNullOrEmpty(graph) || graph.ToLower() == graphMap.name.ToLower())
          {
            manifestGraph = new Graph
            {
              classTemplatesList = new ClassTemplatesList(),
              name = graphMap.name
            };

            manifest.graphs.Add(manifestGraph);
          }
          else 
          {
            continue;
          }

          if (manifestGraph == null)
          {
            throw new Exception("Graph [" + graph + "] does not exist in mapping.");
          }

          string dataObjectName = graphMap.dataObjectName;
          DataObject dataObject = null;

          foreach (DataObject dataObj in dataDictionary.dataObjects)
          {
            if (dataObj.objectName.ToLower() == dataObjectName.ToLower())
            {
              dataObject = dataObj;
              break;
            }
          }

          if (dataObject == null)
          {
            throw new Exception("Data Object [" + dataObjectName + "] does not exist in data dictionary.");
          }

          foreach (var classTemplateMap in graphMap.classTemplateMaps)
          {
            ClassTemplates manifestClassTemplates = new ClassTemplates();
            manifestGraph.classTemplatesList.Add(manifestClassTemplates);

            ClassMap classMap = classTemplateMap.classMap;
            List<TemplateMap> templateMaps = classTemplateMap.templateMaps;
            
            Keys keys = new Keys();

            if (templateMaps.Count > 0)
            {
              foreach (string identifier in classMap.identifiers)
              {
                Key key = GetKey(graphMap, classMap, identifier);
                keys.Add(key);
              }
            }

            Class manifestClass = new Class
            {
              id = classMap.id,
              name = classMap.name,
              keys = keys,
            };
            manifestClassTemplates.@class = manifestClass;

            foreach (TemplateMap templateMap in templateMaps)
            {
              Template manifestTemplate = new Template
              {
                roles = new Roles(),
                id = templateMap.id,
                name = templateMap.name,
                transferOption = TransferOption.Desired,
              };
              manifestClassTemplates.templates.Add(manifestTemplate);

              foreach (RoleMap roleMap in templateMap.roleMaps)
              {
                Role manifestRole = new Role
                {
                  type = roleMap.type,
                  id = roleMap.id,
                  name = roleMap.name,
                  dataType = roleMap.dataType,
                  value = roleMap.value,
                };
                manifestTemplate.roles.Add(manifestRole);

                if (roleMap.type == RoleType.Property ||
                    roleMap.type == RoleType.DataProperty ||
                    roleMap.type == RoleType.ObjectProperty)
                {
                  if (!String.IsNullOrEmpty(roleMap.propertyName))
                  {
                    string[] propertyParts = roleMap.propertyName.Split('.');
                    string objectName = propertyParts[propertyParts.Length - 2].Trim();
                    string propertyName = propertyParts[propertyParts.Length - 1].Trim();
                    DataObject dataObj = dataObject;

                    if (propertyParts.Length < 2)
                    {
                      throw new Exception("Property [" + roleMap.propertyName + "] is invalid.");
                    }
                    else if (propertyParts.Length > 2) // related property
                    {
                      // find related object
                      for (int i = 1; i < propertyParts.Length - 1; i++)
                      {
                        DataRelationship rel = dataObj.dataRelationships.Find(x => x.relationshipName.ToLower() == propertyParts[i].ToLower());
                        if (rel == null)
                        {
                          throw new Exception("Relationship [" + rel.relationshipName + "] does not exist.");
                        }

                        dataObj = dataDictionary.dataObjects.Find(x => x.objectName.ToLower() == rel.relatedObjectName.ToLower());
                        if (dataObj == null)
                        {
                          throw new Exception("Related object [" + rel.relatedObjectName + "] is not found.");
                        }
                      }
                    }

                    DataProperty dataProp = dataObj.dataProperties.Find(x => x.propertyName.ToLower() == propertyName.ToLower());
                    if (dataProp == null)
                    {
                      throw new Exception("Property [" + roleMap.propertyName + "] does not exist in data dictionary.");
                    }

                    manifestRole.dataLength = dataProp.dataLength;

                    if (dataObj.isKeyProperty(propertyName))
                    {
                      manifestTemplate.transferOption = TransferOption.Required;
                    }
                  }
                }

                if (roleMap.classMap != null)
                {
                  Cardinality cardinality = graphMap.GetCardinality(roleMap, _dataDictionary, _fixedIdentifierBoundary);
                  manifestRole.cardinality = cardinality;

                  manifestRole.@class = new Class
                  {
                    id = roleMap.classMap.id,
                    name = roleMap.classMap.name,
                  };
                }
              }
            }
          }
        }

        manifest.valueListMaps = Utility.CloneDataContractObject<ValueListMaps>(_mapping.valueListMaps);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting manifest: " + ex);
        throw ex;
      }

      return manifest;
    }

    public Manifest GetManifest(string scope, string app)
    {
      return GetManifest(scope, app, null);
    }

    public DataTransferIndices GetDataTransferIndicesWithManifest(string scope, string app, string graph, string hashAlgorithm, Manifest manifest)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        BuildCrossGraphMap(manifest, graph);
        DataFilter filter = GetPredeterminedFilter();

        if (_settings["MultiGetDTIs"] == null || bool.Parse(_settings["MultiGetDTIs"]))
        {
          _logger.Debug("Multi-threading enabled!");
          dataTransferIndices = MultiGetDataTransferIndices(filter);
        }
        else
        {
          _logger.Debug("Single threading...");
          DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");

          _logger.Debug("Fetching Data...");
          List<IDataObject> dataObjects = PageDataObjects(_graphMap.dataObjectName, filter);

          _logger.Debug("Transforming into DTI");
          dataTransferIndices = projectionLayer.GetDataTransferIndices(_graphMap, dataObjects, String.Empty);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer indices: " + ex);
        throw ex;
      }

      return dataTransferIndices;
    }
    
    public string AsyncGetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DxiRequest dxiRequest)
    {
      try
      {
        var id = QueueNewRequest();
        Task task = Task.Factory.StartNew(() => DoGetDataTransferIndicesWithFilter(scope, app, graph, hashAlgorithm, dxiRequest, id));
        return "/requests/" + id;
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data transfer indices: " + e.Message);
        throw e;
      }
    }

    private void DoGetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DxiRequest dxiRequest, string id)
    {
      try
      {
        DataTransferIndices dataTransferIndices = GetDataTransferIndicesWithFilter(scope, app, graph, hashAlgorithm, dxiRequest);

        _requests[id].ResponseText = Utility.Serialize<DataTransferIndices>(dataTransferIndices, true);
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

    public DataTransferIndices GetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DxiRequest dxiRequest)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        BuildCrossGraphMap(dxiRequest.Manifest, graph);

        DataFilter filter = dxiRequest.DataFilter;
        DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
        dtoProjectionEngine.ProjectDataFilter(_dataDictionary, ref filter, graph);
        filter.AppendFilter(GetPredeterminedFilter());

        // get sort index
        string sortIndex = String.Empty;
        string sortOrder = String.Empty;

        if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
        {
          sortIndex = filter.OrderExpressions.First().PropertyName;
          sortOrder = filter.OrderExpressions.First().SortOrder.ToString();
        }

        if (_settings["MultiGetDTIs"] == null || bool.Parse(_settings["MultiGetDTIs"]))
        {
          _logger.Debug("Running muti-threaded DTIs.");
          dataTransferIndices = MultiGetDataTransferIndices(filter);
        }
        else
        {
          _logger.Debug("Running single-threaded DTIs.");            
          List<IDataObject> dataObjects = PageDataObjects(_graphMap.dataObjectName, filter);
          dataTransferIndices = dtoProjectionEngine.GetDataTransferIndices(_graphMap, dataObjects, sortIndex);
        }

        if (sortOrder != String.Empty)
          dataTransferIndices.SortOrder = sortOrder;
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer indices: " + ex);
        throw ex;
      }

      return dataTransferIndices;
    }

    // get single data transfer object (but wrap it in a list!)
    public DataTransferObjects GetDataTransferObject(string scope, string app, string graph, string id)
    {
      DataTransferObjects dataTransferObjects = new DataTransferObjects();

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        IList<string> identifiers = new List<string> { id };
        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);

        DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
        XDocument dtoDoc = dtoProjectionEngine.ToXml(_graphMap.name, ref dataObjects);

        if (dtoDoc != null && dtoDoc.Root != null)
        {
          dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer objects: " + ex);
        throw ex;
      }

      return dataTransferObjects;
    }

    // get list (page) of data transfer objects per data transfer indicies
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DataTransferIndices dataTransferIndices)
    {
      DataTransferObjects dataTransferObjects = new DataTransferObjects();

      if (dataTransferIndices != null && dataTransferIndices.DataTransferIndexList.Count > 0)
      {
        try
        {
          InitializeScope(scope, app);
          InitializeDataLayer();

          _graphMap = _mapping.FindGraphMap(graph);

          List<DataTransferIndex> dataTrasferIndexList = dataTransferIndices.DataTransferIndexList;
          List<string> identifiers = new List<string>();

          foreach (DataTransferIndex dti in dataTrasferIndexList)
          {
            identifiers.Add(dti.InternalIdentifier);
          }

          if (identifiers.Count > 0)
          {
            if (_settings["MultiGetDTOs"] == null || bool.Parse(_settings["MultiGetDTOs"]))
            {
              dataTransferObjects = MultiGetDataTransferObjects(identifiers);
            }
            else
            {
              IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);
              DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
              XDocument dtoDoc = dtoProjectionEngine.ToXml(_graphMap.name, ref dataObjects);

              if (dtoDoc != null && dtoDoc.Root != null)
              {
                dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
              }
            }
          }
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting data transfer objects: " + ex);
          throw ex;
        }
      }

      return dataTransferObjects;
    }
    
    public string AsyncGetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest, bool includeContent)
    {
      try
      {
        var id = QueueNewRequest();
        Task task = Task.Factory.StartNew(() => DoGetDataTransferObjects(scope, app, graph, dxoRequest, id, includeContent));
        return "/requests/" + id;
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data transfer objects: " + e.Message);
        throw e;
      }
    }

    private void DoGetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest, string id, bool includeContent)
    {
      try
      {
        DataTransferObjects dtos = GetDataTransferObjects(scope, app, graph, dxoRequest, includeContent);

        _requests[id].ResponseText = Utility.Serialize<DataTransferObjects>(dtos, true);
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

    // get list (page) of data transfer objects per dto page request
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest, bool includeContent)
    {
      DataTransferObjects dtos = new DataTransferObjects();

      if (dxoRequest != null && dxoRequest.DataTransferIndices != null &&
          dxoRequest.DataTransferIndices.DataTransferIndexList.Count > 0)
      {
        try
        {
          InitializeScope(scope, app);
          InitializeDataLayer();

          BuildCrossGraphMap(dxoRequest.Manifest, graph);

          List<DataTransferIndex> dataTrasferIndexList = dxoRequest.DataTransferIndices.DataTransferIndexList;
          IDictionary<string, string> idFormats = new Dictionary<string, string>();
          bool hasContent = false;

          foreach (DataTransferIndex dti in dataTrasferIndexList)
          {
            if (dti.HasContent)
            {
              hasContent = true;
            }

            idFormats[dti.InternalIdentifier] = string.Empty;
          }

          if (idFormats.Count > 0)
          {
            if (_settings["MultiGetDTOs"] != null && bool.Parse(_settings["MultiGetDTOs"]))
            {
              //TODO: handle content in multithreaded mode
              dtos = MultiGetDataTransferObjects(idFormats.Keys.ToList<string>());
            }
            else
            {
              _logger.Debug("Single threaded get DTOs.");

              if (hasContent)
              {
                _settings["IncludeContent"] = includeContent.ToString();              
                

              IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, idFormats.Keys.ToList<string>());
              DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
              
              dtos = dtoProjectionEngine.BuildDataTransferObjects(_graphMap, ref dataObjects);
            }
          }
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting data transfer objects: " + ex);
          throw ex;
        }
      }

      return dtos;
    }

    public string AsyncPostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dtos)
    {
      try
      {
        var id = QueueNewRequest();
        Task task = Task.Factory.StartNew(() => DoPostDataTransferObjects(scope, app, graph, dtos, id));
        return "/requests/" + id;
      }
      catch (Exception e)
      {
        _logger.Error("Error posting data transfer objects: " + e.Message);
        throw e;
      }
    }

    private void DoPostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dtos, string id)
    {
      try
      {
        Response response = PostDataTransferObjects(scope, app, graph, dtos);

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

    public Response PostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dataTransferObjects)
    {
      Response response = new Response();
      response.DateTimeStamp = DateTime.Now;

      try
      {
        _settings["SenderProjectName"] = dataTransferObjects.SenderScopeName;
        _settings["SenderApplicationName"] = dataTransferObjects.SenderAppName;

        InitializeScope(scope, app);

        if (_settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + scope + "." + app + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }

        response.Level = StatusLevel.Success;

        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        // extract deleted identifiers from data transfer objects
        List<string> deletedIdentifiers = new List<string>();
        List<DataTransferObject> dataTransferObjectList = dataTransferObjects.DataTransferObjectList;

        for (int i = 0; i < dataTransferObjectList.Count; i++)
        {
          if (dataTransferObjectList[i].transferType == TransferType.Delete)
          {
            deletedIdentifiers.Add(dataTransferObjectList[i].identifier);
            dataTransferObjectList.RemoveAt(i--);
          }
        }

        // call data layer to delete data objects
        if (deletedIdentifiers.Count > 0)
        {
          response.Append(_dataLayer.Delete(_graphMap.dataObjectName, deletedIdentifiers));
        }

        if (dataTransferObjectList.Count > 0)
        {
          if (_settings["MultiPostDTOs"] != null && bool.Parse(_settings["MultiPostDTOs"]))
          {
            response.Append(MultiPostDataTransferObjects(dataTransferObjects));
          }
          else
          {
            _logger.Debug("Single threaded post DTOs.");
            DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
            IList<IDataObject> dataObjects = dtoProjectionEngine.ToDataObjects(_graphMap, ref dataTransferObjects);
            Response postResponse = _dataLayer.Post(dataObjects);
            response.Append(postResponse);
          }
        }

        if (response.Level != StatusLevel.Success)
        {
          string dtoFilename = String.Format(_settings["BaseDirectoryPath"] + "/Logs/DTO_{0}.{1}.{2}.xml", scope, app, graph);
          Utility.Write<DataTransferObjects>(dataTransferObjects, dtoFilename, true);
        }
      }
      catch (Exception ex)
      {
        string message = "Error posting data transfer objects: " + ex;

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { message },
        };

        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);

        _logger.Error(message);
      }

      return response;
    }

    public Response DeleteDataTransferObject(string scope, string app, string graph, string id)
    {
      Response response = new Response();

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        IList<string> identifiers = new List<string> { id };
        response.Append(_dataLayer.Delete(_graphMap.dataObjectName, identifiers));
      }
      catch (Exception ex)
      {
        string message = "Error deleting data transfer object: " + ex;

        response.Level = StatusLevel.Error;
        response.StatusList.Add(
          new Status()
          {
            Messages = new Messages { message }
          }
        );

        _logger.Error(message);
      }

      return response;
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

          if (!isScopeValid) throw new Exception(string.Format("Scope [{0}] not found.", scope));

          _settings["ProjectName"] = projectName;
          _settings["ApplicationName"] = applicationName;
          _settings["Scope"] = scope;

          string appSettingsPath = String.Format("{0}{1}.config", _settings["AppDataPath"], scope);

          if (File.Exists(appSettingsPath))
          {
            AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
            _settings.AppendSettings(appSettings);
          }

          string relativePath = String.Format("{0}BindingConfiguration.{1}.xml", _settings["AppDataPath"], scope);

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
                new XAttribute("service", "org.iringtools.library.IDataLayer2, iRINGLibrary"),
                new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary")
              )
            );

            binding.Save(bindingConfigurationPath);
          }

          _kernel.Load(bindingConfigurationPath);

          string mappingPath = String.Format("{0}Mapping.{1}.xml", _settings["AppDataPath"], scope);

          if (File.Exists(mappingPath))
          {
            try
            {
              _mapping = Utility.Read<Mapping>(mappingPath);
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

          _kernel.Bind<Mapping>().ToConstant(_mapping);
          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing scope: {0}", ex));
        throw ex;
      }
    }

    private void InitializeDataLayer()
    {
      try
      {
        if (!_isDataLayerInitialized)
        {
          _dataLayer = _kernel.TryGet<IDataLayer2>("DataLayer");

          if (_dataLayer == null)
          {
            _dataLayer = (IDataLayer2)_kernel.Get<IDataLayer>("DataLayer");
          }

          _kernel.Rebind<IDataLayer2>().ToConstant(_dataLayer).InThreadScope();

          _dataDictionary = _dataLayer.GetDictionary();
          _kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);

          _isDataLayerInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing datalayer: {0}",  ex));
        throw ex;
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
        throw ex;
      }
    }

    public ContentObjects GetContents(string scope, string app, string graph, string filter)
    {
      try
      {
        ContentObjects contentObjects = new ContentObjects();

        IDictionary<string, string> idFormats = (IDictionary<string, string>)
          JsonConvert.DeserializeObject<Dictionary<string, string>>(filter);

        InitializeScope(scope, app);
        InitializeDataLayer();

        GraphMap graphMap = _mapping.FindGraphMap(graph);
        if (graph == null)
        {
          throw new Exception("Graph [" + graph + "] not found.");
        }

        DataObject objDef = _dataDictionary.dataObjects.Find(x => x.objectName.ToLower() == graphMap.dataObjectName.ToLower());
        if (objDef == null)
        {
          throw new Exception("Data object [" + graphMap.dataObjectName + "] not found.");
        }
        
        IList<IContentObject> iContentObjects = _dataLayer.GetContents(graphMap.dataObjectName, idFormats);
        
        #region marshall iContentObjects into contentObjects
        foreach (IContentObject iContentObject in iContentObjects)
        {
          ContentObject contentObject = new ContentObject()
          {
            Identifier = iContentObject.Identifier,
            MimeType = iContentObject.ContentType,
            Content = iContentObject.Content.ToMemoryStream().ToArray(),
            HashType = iContentObject.HashType,
            HashValue = iContentObject.HashValue,
            URL = iContentObject.URL
          };
           
	        foreach (DataProperty prop in objDef.dataProperties)
	        {
	          object value = iContentObject.GetPropertyValue(prop.propertyName);
	          if (value != null)
	          {
	            string valueStr = Convert.ToString(value);

	            if (prop.dataType == DataType.DateTime)
	              valueStr = Utility.ToXsdDateTime(valueStr);

	            Attribute attr = new Attribute()
	            {
	              Name = prop.propertyName,
	              Value = valueStr
	            };

	            contentObject.Attributes.Add(attr);
	          }
	        }

          contentObjects.Add(contentObject);
        }
        #endregion

        return contentObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting content objects: " + ex.ToString());
        throw ex;
      }
    }

    public IContentObject GetContent(string scope, string app, string graph, string id, string format)
    {
      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        GraphMap graphMap = _mapping.FindGraphMap(graph);
        if (graph == null)
        {
          throw new Exception("Graph [" + graph + "] not found.");
        }

        IDictionary<string, string> idFormats = new Dictionary<string, string>() { {id, format} };
        IList<IContentObject> iContentObjects = _dataLayer.GetContents(graphMap.dataObjectName, idFormats);

        if (iContentObjects == null || iContentObjects.Count == 0)
          throw new Exception("Content object [" + id + "] not found.");

        return iContentObjects[0];
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting content object: " + ex.ToString());
        throw ex;
      }
    }

    public Response PostContents(string scope, string app, string graph, ContentObjects contentObjects)
    {
      try
      {
        IList<IDataObject> iDataObjects = new List<IDataObject>();

        InitializeScope(scope, app);
        InitializeDataLayer();

        GraphMap graphMap = _mapping.FindGraphMap(graph);
        if (graph == null)
        {
          throw new Exception("Graph [" + graph + "] not found.");
        } 
        
        #region marshall contentObjects into iContentObjects
        foreach (ContentObject contentObject in contentObjects)
        {
          IContentObject iContentObject = new GenericContentObject();
          iContentObject.Identifier = contentObject.Identifier;
          iContentObject.ContentType = contentObject.MimeType;
          iContentObject.Content = iContentObject.Content.ToMemoryStream();

          IContentObject dataObject = new GenericContentObject()
          {
            ObjectType = graphMap.dataObjectName
          };

          foreach (Attribute attr in contentObject.Attributes)
          {
            dataObject.SetPropertyValue(attr.Name, attr.Value);
          }
          contentObjects.Add(contentObject);
        }
        #endregion

        Response response = _dataLayer.Post(iDataObjects);
        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error posting content objects: " + ex.ToString());
        throw ex;
      }
    }

    // build cross _graphmap from manifest graph and mapping graph
    private void BuildCrossGraphMap(Manifest manifest, string graph)
    {
      if (manifest == null || manifest.graphs == null || manifest.graphs.Count == 0)
        throw new Exception("Manifest of graph [" + graph + "] is empty.");

      Graph manifestGraph = manifest.FindGraph(graph);

      if (manifestGraph.classTemplatesList == null || manifestGraph.classTemplatesList.Count == 0)
        throw new Exception("Manifest of graph [" + graph + "] does not contain any class-template-maps.");

      GraphMap mappingGraph = _mapping.FindGraphMap(graph);
      ClassTemplates manifestClassTemplatesMap = manifestGraph.classTemplatesList.First();
      Class manifestClass = manifestClassTemplatesMap.@class;

      _graphMap = new GraphMap()
      {
        name = mappingGraph.name,
        dataObjectName = mappingGraph.dataObjectName,
        dataFilter = mappingGraph.dataFilter
      };

      if (manifestClassTemplatesMap != null)
      {
        foreach (var mappingClassTemplatesMap in mappingGraph.classTemplateMaps)
        {
          ClassMap mappingClass = mappingClassTemplatesMap.classMap;

          if (mappingClass.id == manifestClass.id)
          {
            RecurBuildCrossGraphMap(ref manifestGraph, manifestClass, mappingGraph, mappingClass);
            break;
          }
        }
      }
    }

    private DataFilter GetPredeterminedFilter()
    {
      if (_dataDictionary == null)
      {
        _dataDictionary = _dataLayer.GetDictionary();
      }

      DataObject dataObject = _dataDictionary.GetDataObject(_graphMap.dataObjectName);
      DataFilter dataFilter = new DataFilter();
      dataFilter.AppendFilter(dataObject.dataFilter);
      dataFilter.AppendFilter(_graphMap.dataFilter);

      return dataFilter;
    }

    private void RecurBuildCrossGraphMap(ref Graph manifestGraph, Class manifestClass, GraphMap mappingGraph, ClassMap mappingClass)
    {
      List<Template> manifestTemplates = null;

      // get manifest templates from the manifest class
      foreach (ClassTemplates manifestClassTemplates in manifestGraph.classTemplatesList)
      {
        if (manifestClassTemplates.@class.id == manifestClass.id)
        {
          manifestTemplates = manifestClassTemplates.templates;
          break;
        }
      }

      if (manifestTemplates != null)
      {
        // find mapping templates for the mapping class
        foreach (var pair in mappingGraph.classTemplateMaps)
        {
          ClassMap localMappingClass = pair.classMap;
          List<TemplateMap> mappingTemplates = pair.templateMaps;

          if (localMappingClass.id == manifestClass.id)
          {
            ClassMap crossedClass = localMappingClass.CrossClassMap(mappingGraph, manifestClass);
            TemplateMaps crossedTemplates = new TemplateMaps();

            _graphMap.classTemplateMaps.Add(new ClassTemplateMap { classMap = crossedClass, templateMaps = crossedTemplates });

            foreach (Template manifestTemplate in manifestTemplates)
            {
              TemplateMap crossedTemplate = null;

              foreach (TemplateMap mappingTemplate in mappingTemplates)
              {
                if (mappingTemplate.id == manifestTemplate.id)
                {
                  List<string> unmatchedRoleIds = new List<string>();
                  int rolesMatchedCount = 0;

                  for (int i = 0; i < mappingTemplate.roleMaps.Count; i++)
                  {
                    RoleMap roleMap = mappingTemplate.roleMaps[i];
                    bool found = false;

                    foreach (Role manifestRole in manifestTemplate.roles)
                    {
                      if (manifestRole.id == roleMap.id)
                      {
                        found = true;

                        if (roleMap.type != RoleType.Reference || roleMap.value == manifestRole.value)
                        {
                          rolesMatchedCount++;
                        }

                        if (manifestRole.type == RoleType.Property ||
                            manifestRole.type == RoleType.DataProperty ||
                            manifestRole.type == RoleType.ObjectProperty)
                        {
                          roleMap.dataLength = manifestRole.dataLength;
                        }

                        break;
                      }
                    }

                    if (!found)
                    {
                      unmatchedRoleIds.Add(roleMap.id);
                    }
                  }

                  if (rolesMatchedCount == manifestTemplate.roles.Count)
                  {
                    crossedTemplate = CloneTemplateMap(mappingTemplate);

                    if (unmatchedRoleIds.Count > 0)
                    {
                      // remove unmatched roles                      
                      for (int i = 0; i < crossedTemplate.roleMaps.Count; i++)
                      {
                        if (unmatchedRoleIds.Contains(crossedTemplate.roleMaps[i].id))
                        {
                          crossedTemplate.roleMaps.RemoveAt(i--);
                        }
                      }
                    }
                  }
                }
              }

              if (crossedTemplate != null)
              {
                crossedTemplates.Add(crossedTemplate);

                // set cardinality for crossed role map that references to class map
                foreach (Role manifestRole in manifestTemplate.roles)
                {
                  if (manifestRole.@class != null)
                  {
                    foreach (RoleMap mappingRole in crossedTemplate.roleMaps)
                    {
                      if (mappingRole.classMap != null && mappingRole.classMap.id == manifestRole.@class.id)
                      {
                        Cardinality cardinality = mappingGraph.GetCardinality(mappingRole, _dataDictionary, _fixedIdentifierBoundary);

                        // get crossed role map and set its cardinality
                        foreach (RoleMap crossedRoleMap in crossedTemplate.roleMaps)
                        {
                          if (crossedRoleMap.id == mappingRole.id)
                          {
                            manifestRole.cardinality = cardinality;
                            break;
                          }
                        }

                        Class childManifestClass = manifestRole.@class;
                        foreach (ClassTemplates anyClassTemplates in manifestGraph.classTemplatesList)
                        {
                          if (manifestRole.@class.id == anyClassTemplates.@class.id)
                          {
                            childManifestClass = anyClassTemplates.@class;
                          }
                        }

                        RecurBuildCrossGraphMap(ref manifestGraph, childManifestClass, mappingGraph, mappingRole.classMap);
                      }
                    }
                  }
                }
              }
            }

            break;
          }
        }
      }
    }

    private TemplateMap CloneTemplateMap(TemplateMap templateMap)
    {
      TemplateMap clonedTemplateMap = new TemplateMap()
      {
        id = templateMap.id,
        name = templateMap.name,
        type = templateMap.type
      };

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        clonedTemplateMap.roleMaps.Add(roleMap);
      }

      return clonedTemplateMap;
    }

    private List<IDataObject> PageDataObjects(string objectType, DataFilter filter)
    {
      List<IDataObject> dataObjects = new List<IDataObject>();

      int pageSize = (String.IsNullOrEmpty(_settings["DefaultPageSize"]))
        ? 250 : int.Parse(_settings["DefaultPageSize"]);

      long count = _dataLayer.GetCount(_graphMap.dataObjectName, filter);

      for (int offset = 0; offset < count; offset = offset + pageSize)
      {
        _logger.Debug(string.Format("Getting paged data {0}-{1}.", offset, offset + pageSize));

        dataObjects.AddRange(_dataLayer.Get(_graphMap.dataObjectName, filter, pageSize, offset));

        _logger.Debug(string.Format("Paged data {0}-{1} completed.", offset, offset + pageSize));
      }

      return dataObjects;
    }

    private DataTransferIndices MultiGetDataTransferIndices(DataFilter filter)
    {
      DataTransferIndices dataTransferIndices = new DataTransferIndices();
      long total = _dataLayer.GetCount(_graphMap.dataObjectName, filter);
      int maxThreads = int.Parse(_settings["MaxThreads"]);

      if (total > 0)
      {
        long numOfThreads = Math.Min(total, maxThreads);
        int itemsPerThread = (int)Math.Ceiling((double)total / numOfThreads);

        List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
        List<DataTransferIndicesTask> dtiTasks = new List<DataTransferIndicesTask>();
        int threadCount;

        for (threadCount = 0; threadCount < numOfThreads; threadCount++)
        {
          int offset = threadCount * itemsPerThread;
          if (offset >= total)
            break;
          
          int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;
          DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
          ManualResetEvent doneEvent = new ManualResetEvent(false);
          DataTransferIndicesTask dtiTask = new DataTransferIndicesTask(doneEvent, projectionLayer, _dataLayer, _graphMap,
            filter, pageSize, offset);

          doneEvents.Add(doneEvent);
          dtiTasks.Add(dtiTask);

          ThreadPool.QueueUserWorkItem(dtiTask.ThreadPoolCallback, threadCount);
        }

        _logger.Debug("Number of threads [" + threadCount + "].");
        _logger.Debug("Items per thread [" + itemsPerThread + "].");
        _logger.Debug("DTI tasks started!");

        // wait for all tasks to complete
        WaitHandle.WaitAll(doneEvents.ToArray());

        _logger.Debug("DTI tasks completed!");

        // collect DTIs from the tasks
        for (int i = 0; i < threadCount; i++)
        {
          DataTransferIndices dtis = dtiTasks[i].DataTransferIndices;

          if (dtis != null)
          {
            dataTransferIndices.DataTransferIndexList.AddRange(dtis.DataTransferIndexList);
          }
        }

        _logger.Debug("DTIs assembled count: " + dataTransferIndices.DataTransferIndexList.Count);
      }

      return dataTransferIndices;
    }

    private DataTransferObjects MultiGetDataTransferObjects(List<string> identifiers)
    {
      DataTransferObjects dataTransferObjects = new DataTransferObjects();

      int total = identifiers.Count;
      int maxThreads = int.Parse(_settings["MaxThreads"]);

      int numOfThreads = Math.Min(total, maxThreads);
      int itemsPerThread = (int)Math.Ceiling((double)total / numOfThreads);

      List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
      List<OutboundDtoTask> dtoTasks = new List<OutboundDtoTask>();
      int threadCount;

      for (threadCount = 0; threadCount < numOfThreads; threadCount++)
      {
        int offset = threadCount * itemsPerThread;
        if (offset >= total)
          break;

        int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;
        List<string> pageIdentifiers = identifiers.GetRange(offset, pageSize);
        DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
        ManualResetEvent doneEvent = new ManualResetEvent(false);
        OutboundDtoTask dtoTask = new OutboundDtoTask(doneEvent, projectionLayer, _dataLayer, _graphMap, pageIdentifiers);
        ThreadPool.QueueUserWorkItem(dtoTask.ThreadPoolCallback, threadCount);

        doneEvents.Add(doneEvent);
        dtoTasks.Add(dtoTask);
      }

      _logger.Debug("Number of threads [" + threadCount + "].");
      _logger.Debug("Items per thread [" + itemsPerThread + "].");

      // wait for all tasks to complete
      WaitHandle.WaitAll(doneEvents.ToArray());

      // collect DTIs from the tasks
      for (int i = 0; i < threadCount; i++)
      {
        DataTransferObjects dtos = dtoTasks[i].DataTransferObjects;

        if (dtos != null)
        {
          dataTransferObjects.DataTransferObjectList.AddRange(dtos.DataTransferObjectList);
        }
      }

      return dataTransferObjects;
    }

    private Response MultiPostDataTransferObjects(DataTransferObjects dataTransferObjects)
    {
      Response response = new Response();

      {
        int total = dataTransferObjects.DataTransferObjectList.Count;
        int maxThreads = int.Parse(_settings["MaxThreads"]);

        int numOfThreads = Math.Min(total, maxThreads);
        int itemsPerThread = (int)Math.Ceiling((double)total / numOfThreads);

        List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
        List<DataTransferObjectsTask> dtoTasks = new List<DataTransferObjectsTask>();      
        int threadCount;

        for (threadCount = 0; threadCount < numOfThreads; threadCount++)
        {
          int offset = threadCount * itemsPerThread;
          if (offset >= total)
            break;
          
          int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;
          DataTransferObjects dtos = new DataTransferObjects();
          dtos.DataTransferObjectList = dataTransferObjects.DataTransferObjectList.GetRange(offset, pageSize);

          DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
          IDataLayer dataLayer = _kernel.Get<IDataLayer>();
          ManualResetEvent doneEvent = new ManualResetEvent(false);
          DataTransferObjectsTask dtoTask = new DataTransferObjectsTask(doneEvent, projectionLayer, dataLayer, _graphMap, dtos);
          ThreadPool.QueueUserWorkItem(dtoTask.ThreadPoolCallback, threadCount);
          
          doneEvents.Add(doneEvent);
          dtoTasks.Add(dtoTask);        
        }

        _logger.Debug("Number of threads [" + threadCount + "].");
        _logger.Debug("Items per thread [" + itemsPerThread + "].");
        
        // wait for all tasks to complete
        WaitHandle.WaitAll(doneEvents.ToArray());

        // collect responses from the tasks
        for (int i = 0; i < threadCount; i++)
        {
          response.Append(dtoTasks[i].Response);
        }
      }

      return response;
    }
  }
}