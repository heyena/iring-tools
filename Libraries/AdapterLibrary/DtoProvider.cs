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
using org.iringtools.adapter.projection;
using System.ServiceModel;
using System.Security.Principal;
using System.Text;
using org.iringtools.mapping;
using org.iringtools.dxfr.manifest;
using org.iringtools.adapter.identity;
using Microsoft.ServiceModel.Web;
using System.Threading;

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

    [Inject]
    public DtoProvider(NameValueCollection settings)
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

    public Manifest GetManifest(string scope, string app)
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
          Graph manifestGraph = new Graph
          {
            classTemplatesList = new ClassTemplatesList(),
            name = graphMap.name
          };
          manifest.graphs.Add(manifestGraph);

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

          if (dataObject != null)
          {
            foreach (var classTemplateMap in graphMap.classTemplateMaps)
            {
              ClassTemplates manifestClassTemplates = new ClassTemplates()
              {
                templates = new Templates()
              };
              manifestGraph.classTemplatesList.Add(manifestClassTemplates);

              ClassMap classMap = classTemplateMap.classMap;
              TemplateMaps templateMaps = classTemplateMap.templateMaps;
              String templateName, roleName, roleId, templateId;

              Keys keys = new Keys();
              foreach (ClassTemplateMap anyClassTemplateMap in graphMap.classTemplateMaps)
              {
                ClassMap anyClassMap = anyClassTemplateMap.classMap;

                foreach (TemplateMap templateMap in anyClassTemplateMap.templateMaps)
                {
                  templateName = templateMap.name;
                  templateId = templateMap.id;

                  foreach (RoleMap roleMap in templateMap.roleMaps)
                  {
                    roleName = roleMap.name;
                    roleId = roleMap.id;

                    if (!String.IsNullOrEmpty(roleMap.propertyName))
                    {
                      string[] property = roleMap.propertyName.Split('.');
                      string objectName = property[0].Trim();
                      string propertyName = property[1].Trim();


                      foreach (String identifier in classMap.identifiers)
                      {
                        if (identifier.ToLower() == roleMap.propertyName.ToLower())
                        {
                          Key key = new Key();
                          key.templateId = templateId;
                          key.roleId = roleId;
                          key.classId = anyClassMap.id;
                          keys.Add(key);

                        }

                      }
                    }
                  }
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
                      string[] property = roleMap.propertyName.Split('.');
                      string objectName = property[0].Trim();
                      string propertyName = property[1].Trim();

                      DataProperty dataProp = dataObject.dataProperties.Find(x => x.propertyName.ToLower() == propertyName.ToLower());
                      manifestRole.dataLength = dataProp.dataLength;

                      if (dataObject.isKeyProperty(propertyName))
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

    public DataTransferIndices GetDataTransferIndicesByRequest(string scope, string app, string graph, string hashAlgorithm, DxiRequest request)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        BuildCrossGraphMap(request.Manifest, graph);

        DataFilter filter = request.DataFilter;
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
          dataTransferIndices = MultiGetDataTransferIndices(filter);
        }
        else
        {
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

    // get list (page) of data transfer objects per dto page request
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest)
    {
      DataTransferObjects dataTransferObjects = new DataTransferObjects();

      if (dxoRequest != null && dxoRequest.DataTransferIndices != null &&
          dxoRequest.DataTransferIndices.DataTransferIndexList.Count > 0)
      {
        try
        {
          InitializeScope(scope, app);
          InitializeDataLayer();

          BuildCrossGraphMap(dxoRequest.Manifest, graph);

          List<DataTransferIndex> dataTrasferIndexList = dxoRequest.DataTransferIndices.DataTransferIndexList;
          List<string> identifiers = new List<string>();

          foreach (DataTransferIndex dti in dataTrasferIndexList)
          {
            identifiers.Add(dti.InternalIdentifier);
          }

          if (identifiers.Count > 0)
          {
            if (_settings["MultiGetDTOs"] != null && bool.Parse(_settings["MultiGetDTOs"]))
            {
              dataTransferObjects = MultiGetDataTransferObjects(identifiers);
            }
            else
            {
              IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);
              DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
              XDocument dtoDoc = dtoProjectionEngine.ToXml(_graphMap, ref dataObjects);

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
            DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
            IList<IDataObject> dataObjects = dtoProjectionEngine.ToDataObjects(_graphMap, ref dataTransferObjects);
            response.Append(_dataLayer.Post(dataObjects));
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
        string message = "Error initializing scope: " + ex;
        _logger.Error(message);
        throw new Exception(message);
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
        string message = "Error initializing datalayer: " + ex;
        _logger.Error(message);
        throw new Exception(message);
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
        dataObjects.AddRange(_dataLayer.Get(_graphMap.dataObjectName, filter, pageSize, offset));
      }

      return dataObjects;
    }

    private DataTransferIndices MultiGetDataTransferIndices(DataFilter filter)
    {
      DataTransferIndices dataTransferIndices = new DataTransferIndices();

      _logger.Debug("Getting the count...");

      long total = _dataLayer.GetCount(_graphMap.dataObjectName, filter);
      int maxThreads = int.Parse(_settings["MaxThreads"]);

      if (total > 0)
      {
        long numOfThreads = Math.Min(total, maxThreads);
        int itemsPerThread = (int)Math.Ceiling((double)total / numOfThreads);

        _logger.Debug("Number of threads [" + numOfThreads + "].");
        _logger.Debug("Items per thread [" + itemsPerThread + "].");

        ManualResetEvent[] doneEvents = new ManualResetEvent[numOfThreads];
        DataTransferIndicesTask[] dtiTasks = new DataTransferIndicesTask[numOfThreads];

        _logger.Debug("Getting DTI ...");

        for (int i = 0; i < numOfThreads; i++)
        {
          doneEvents[i] = new ManualResetEvent(false);

          int offset = i * itemsPerThread;
          int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;

          DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
          DataTransferIndicesTask dtiTask = new DataTransferIndicesTask(doneEvents[i], projectionLayer, _dataLayer, _graphMap,
            filter, pageSize, offset);
          dtiTasks[i] = dtiTask;
          ThreadPool.QueueUserWorkItem(dtiTask.ThreadPoolCallback, i);
        }

        // wait for all tasks to complete
        WaitHandle.WaitAll(doneEvents);

        _logger.Debug("DTI tasks completed!");

        // collect DTIs from the tasks
        for (int i = 0; i < numOfThreads; i++)
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

      _logger.Debug("Number of threads [" + numOfThreads + "].");
      _logger.Debug("Items per thread [" + itemsPerThread + "].");

      ManualResetEvent[] doneEvents = new ManualResetEvent[numOfThreads];
      OutboundDtoTask[] dtoTasks = new OutboundDtoTask[numOfThreads];

      for (int i = 0; i < numOfThreads; i++)
      {
        doneEvents[i] = new ManualResetEvent(false);

        int offset = i * itemsPerThread;
        int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;

        List<string> pageIdentifiers = identifiers.GetRange(offset, pageSize);
        DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
        OutboundDtoTask dtoTask = new OutboundDtoTask(doneEvents[i], projectionLayer, _dataLayer, _graphMap, pageIdentifiers);
        dtoTasks[i] = dtoTask;
        ThreadPool.QueueUserWorkItem(dtoTask.ThreadPoolCallback, i);
      }

      // wait for all tasks to complete
      WaitHandle.WaitAll(doneEvents);

      // collect DTIs from the tasks
      for (int i = 0; i < numOfThreads; i++)
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

        _logger.Debug("Number of threads [" + numOfThreads + "].");
        _logger.Debug("Items per thread [" + itemsPerThread + "].");

        ManualResetEvent[] doneEvents = new ManualResetEvent[numOfThreads];
        DataTransferObjectsTask[] dtoTasks = new DataTransferObjectsTask[numOfThreads];

        for (int i = 0; i < numOfThreads; i++)
        {
          doneEvents[i] = new ManualResetEvent(false);

          int offset = i * itemsPerThread;
          int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;

          DataTransferObjects dtos = new DataTransferObjects();
          dtos.DataTransferObjectList = dataTransferObjects.DataTransferObjectList.GetRange(offset, pageSize);

          DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
          IDataLayer dataLayer = _kernel.Get<IDataLayer>();
          DataTransferObjectsTask dtoTask = new DataTransferObjectsTask(doneEvents[i], projectionLayer, dataLayer, _graphMap, dtos);
          dtoTasks[i] = dtoTask;
          ThreadPool.QueueUserWorkItem(dtoTask.ThreadPoolCallback, i);
        }

        // wait for all tasks to complete
        WaitHandle.WaitAll(doneEvents);

        // collect responses from the tasks
        for (int i = 0; i < numOfThreads; i++)
        {
          response.Append(dtoTasks[i].Response);
        }
      }

      return response;
    }
  }
}