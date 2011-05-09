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

namespace org.iringtools.adapter
{
  public class DataTranferProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataTranferProvider));

    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private ScopeProjects _scopes = null;
    private IDataLayer _dataLayer = null;
    private DataDictionary _dataDictionary = null;
    private Mapping _mapping = null;
    private IIdentityLayer _identityLayer = null;
    private IDictionary _keyRing = null;
    private GraphMap _graphMap = null;
    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;

    [Inject]
    public DataTranferProvider(NameValueCollection settings)
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
        version = ""
      };      

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        DataDictionary dataDictionary = _dataLayer.GetDictionary();

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          Graph manifestGraph = new Graph { 
            classTemplatesList = new ClassTemplatesList(),
            name = graphMap.name             
          };
          manifest.graphs.Add(manifestGraph);

          string dataObjectName = graphMap.dataObjectName;
          DataObject dataObject = null;

          foreach (DataObject dataObj in dataDictionary.dataObjects)
          {
            if (dataObj.objectName == dataObjectName)
            {
              dataObject = dataObj;
              break;
            }
          }

          if (dataObject != null)
          {
            foreach (var classTemplateListMap in graphMap.classTemplateMaps)
            {
              ClassTemplates manifestClassTemplatesMap = new ClassTemplates()
              {
                templates = new Templates()
              };
              manifestGraph.classTemplatesList.Add(manifestClassTemplatesMap);

              ClassMap classMap = classTemplateListMap.classMap;
              List<TemplateMap> templateMaps = classTemplateListMap.templateMaps;

              Class manifestClass = new Class
              {
                id = classMap.id,
                name = classMap.name,
              };
              manifestClassTemplatesMap.@class = manifestClass;

              foreach (TemplateMap templateMap in templateMaps)
              {
                Template manifestTemplate = new Template
                {
                  roles = new Roles(),
                  id = templateMap.id,
                  name = templateMap.name,
                  transferOption = TransferOption.Desired,
                };
                manifestClassTemplatesMap.templates.Add(manifestTemplate);

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
                    string[] property = roleMap.propertyName.Split('.');
                    string objectName = property[0].Trim();
                    string propertyName = property[1].Trim();

                    if (dataObject.isKeyProperty(propertyName))
                    {
                      manifestTemplate.transferOption = TransferOption.Required;
                    }
                  }

                  if (roleMap.classMap != null)
                  {
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
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting manifest: " + ex);
      }

      return manifest;
    }

    public DataTransferIndices GetDataTransferIndices(string scope, string app, string graph, string hashAlogrithm)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, null);
        Dictionary<string, List<string>> classIdentifiers = GetClassIdentifiers(ref dataObjects);

        dataTransferIndices = BuildDataTransferIndices(ref dataObjects, ref classIdentifiers, hashAlogrithm, String.Empty);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer indices: " + ex);
      }

      return dataTransferIndices;
    }

    public DataTransferIndices GetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlogrithm, DataFilter filter)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        BasePart7ProjectionEngine dtoProjectionEngine = (_settings["dtoProjectionEngine"] == null || _settings["dtoProjectionEngine"] == "dto")
          ? (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto") : (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto2");

        dtoProjectionEngine.ProjectDataFilter(_dataDictionary, ref filter, graph);

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, filter, 0, 0);
        Dictionary<string, List<string>> classIdentifiers = GetClassIdentifiers(ref dataObjects);

        dataTransferIndices = BuildDataTransferIndices(ref dataObjects, ref classIdentifiers, hashAlogrithm, String.Empty);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer indices: " + ex);
      }

      return dataTransferIndices;
    }

    public DataTransferIndices GetDataTransferIndicesWithManifest(string scope, string app, string graph, string hashAlgorithm, Manifest manifest)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        BuildCrossGraphMap(manifest, graph);

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, null, 0, 0);
        Dictionary<string, List<string>> classIdentifiers = GetClassIdentifiers(ref dataObjects);

        dataTransferIndices = BuildDataTransferIndices(ref dataObjects, ref classIdentifiers, hashAlgorithm, String.Empty);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer indices: " + ex);
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

        BasePart7ProjectionEngine dtoProjectionEngine = (_settings["dtoProjectionEngine"] == null || _settings["dtoProjectionEngine"] == "dto")
          ? (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto") : (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto2");

        DataFilter filter = request.DataFilter;
        dtoProjectionEngine.ProjectDataFilter(_dataDictionary, ref filter, graph);

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, filter, 0, 0);
        Dictionary<string, List<string>> classIdentifiers = GetClassIdentifiers(ref dataObjects);

        // get sort index
        string sortIndex = String.Empty; 
       
        if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
        {
          sortIndex = filter.OrderExpressions.First().PropertyName;
        }
        
        dataTransferIndices = BuildDataTransferIndices(ref dataObjects, ref classIdentifiers, hashAlgorithm, sortIndex);

        // set sort order and type 
        if (dataTransferIndices != null && filter != null && filter.OrderExpressions != null && 
          filter.OrderExpressions.Count > 0)
        {
          dataTransferIndices.SortOrder = filter.OrderExpressions.First().SortOrder.ToString();
          
          // find data type of the sort index
          DataObject dataObject = _dataDictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);
          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {
            if (dataProperty.propertyName.ToUpper() == sortIndex.ToUpper())
            {
              dataTransferIndices.SortType = dataProperty.dataType.ToString();
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer indices: " + ex);
      }

      return dataTransferIndices;
    }

    // get list (page) of data transfer objects per data transfer indicies
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DataTransferIndices dataTransferIndices)
    {
      DataTransferObjects dataTransferObjects = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        List<DataTransferIndex> dataTrasferIndexList = dataTransferIndices.DataTransferIndexList;

        IList<string> identifiers = new List<string>();
        foreach (DataTransferIndex dti in dataTrasferIndexList)
        {
          identifiers.Add(dti.Identifier);
        }

        BasePart7ProjectionEngine dtoProjectionEngine = (_settings["dtoProjectionEngine"] == null || _settings["dtoProjectionEngine"] == "dto")
          ? (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto") : (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto2");
        
        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);
        XDocument dtoDoc = dtoProjectionEngine.ToXml(_graphMap.dataObjectName, ref dataObjects);

        dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer objects: " + ex);
      }

      return dataTransferObjects;
    }

    public Response PostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dataTransferObjects)
    {
      Response response = new Response();
      response.DateTimeStamp = DateTime.Now;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        // extract delete identifiers from data transfer objects
        List<string> deleteIdentifiers = new List<string>();
        List<DataTransferObject> dataTransferObjectList = dataTransferObjects.DataTransferObjectList;

        for (int i = 0; i < dataTransferObjectList.Count; i++)
        {
          if (dataTransferObjectList[i].transferType == TransferType.Delete)
          {
            deleteIdentifiers.Add(dataTransferObjectList[i].identifier);
            dataTransferObjectList.RemoveAt(i--);
          }
        }

        BasePart7ProjectionEngine dtoProjectionEngine = (_settings["dtoProjectionEngine"] == null || _settings["dtoProjectionEngine"] == "dto")
          ? (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto") : (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto2");

        string xml = Utility.SerializeDataContract<DataTransferObjects>(dataTransferObjects);
        XElement xElement = XElement.Parse(xml);
        XDocument dtoDoc = new XDocument(xElement);
        IList<IDataObject> dataObjects = dtoProjectionEngine.ToDataObjects(_graphMap.name, ref dtoDoc);

        if (dataObjects != null && dataObjects.Count > 0)
          response.Append(_dataLayer.Post(dataObjects));  // add/change/sync data objects

        if (deleteIdentifiers.Count > 0)
          response.Append(_dataLayer.Delete(_graphMap.dataObjectName, deleteIdentifiers));

        response.Level = StatusLevel.Success;
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

    // get single data transfer object (but wrap it in a list!)
    public DataTransferObjects GetDataTransferObject(string scope, string app, string graph, string id)
    {
      DataTransferObjects dataTransferObjects = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        IList<string> identifiers = new List<string> { id };
        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);

        BasePart7ProjectionEngine dtoProjectionEngine = (_settings["dtoProjectionEngine"] == null || _settings["dtoProjectionEngine"] == "dto")
          ? (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto") : (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto2");

        XDocument dtoDoc = dtoProjectionEngine.ToXml(_graphMap.name, ref dataObjects);
        dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer objects: " + ex);
      }

      return dataTransferObjects;
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

    // get list (page) of data transfer objects per dto page request
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest)
    {
      DataTransferObjects dataTransferObjects = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        BuildCrossGraphMap(dxoRequest.Manifest, graph);

        List<DataTransferIndex> dataTrasferIndexList = dxoRequest.DataTransferIndices.DataTransferIndexList;

        IList<string> identifiers = new List<string>();
        foreach (DataTransferIndex dti in dataTrasferIndexList)
        {
          identifiers.Add(dti.Identifier);
        }

        BasePart7ProjectionEngine dtoProjectionEngine = (_settings["dtoProjectionEngine"] == null || _settings["dtoProjectionEngine"] == "dto")
          ? (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto") : (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto2");

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);
        XDocument dtoDoc = dtoProjectionEngine.ToXml(_graphMap.name, ref dataObjects);
        dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer objects: " + ex);
      }

      return dataTransferObjects;

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

          _settings["ProjectName"] =  projectName;
          _settings["ApplicationName"] = applicationName;
          _settings["Scope"] =  scope;

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
          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");

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
    private Dictionary<string, List<string>> GetClassIdentifiers(ref IList<IDataObject> dataObjects)
    {
      Dictionary<string, List<string>> classIdentifiers = new Dictionary<string, List<string>>();

      foreach (ClassTemplateMap classTemplateMap in _graphMap.classTemplateMaps)
      {
        ClassMap classMap = classTemplateMap.classMap;

        List<string> identifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < dataObjects.Count; i++)
            {
              if (identifiers.Count == i)
              {
                identifiers.Add(value);
              }
              else
              {
                identifiers[i] += classMap.identifierDelimiter + value;
              }
            }
          }
          else  // identifier comes from a property
          {
            string[] property = identifier.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();

            if (dataObjects != null)
            {
              for (int i = 0; i < dataObjects.Count; i++)
              {
                string value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

                if (identifiers.Count == i)
                {
                  identifiers.Add(value);
                }
                else
                {
                  identifiers[i] += classMap.identifierDelimiter + value;
                }
              }
            }
          }
        }

        classIdentifiers[classMap.id] = identifiers;
      }

      return classIdentifiers;
    }

    // build cross graph map from manifest graph and mapping graph and save it in _graphMap
    private void BuildCrossGraphMap(Manifest manifest, string graph)
    {
      GraphMap mappingGraph = _mapping.FindGraphMap(graph);
      Graph manifestGraph = manifest.FindGraph(graph);

      _graphMap = new GraphMap();
      _graphMap.name = mappingGraph.name;
      _graphMap.dataObjectName = mappingGraph.dataObjectName;

      ClassTemplates manifestClassTemplatesMap = manifestGraph.classTemplatesList.First();
      Class manifestClass = manifestClassTemplatesMap.@class;

      if (manifestClassTemplatesMap != null)
      {
        foreach (var mappingClassTemplatesMap in mappingGraph.classTemplateMaps)
        {
          ClassMap mappingClass = mappingClassTemplatesMap.classMap;

          if (mappingClass.id == manifestClass.id)
          {
            RecurBuildCrossGraphMap(ref manifestGraph, manifestClass, mappingGraph, mappingClass);
          }
        }
      }
    }

    private void RecurBuildCrossGraphMap(ref Graph manifestGraph, Class manifestClass, GraphMap mappingGraph, ClassMap mappingClass)
    {
      List<Template> manifestTemplates = null;

      // find manifest templates for the manifest class
      foreach (ClassTemplates manifestClassTemplates in manifestGraph.classTemplatesList)
      {
        if (manifestClassTemplates.@class.id == manifestClass.id)
        {
          manifestTemplates = manifestClassTemplates.templates;
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
            ClassMap crossedClass = localMappingClass.Clone();
            TemplateMaps crossedTemplates = new TemplateMaps();

            _graphMap.classTemplateMaps.Add(new ClassTemplateMap { classMap = crossedClass, templateMaps = crossedTemplates });

            foreach (Template manifestTemplate in manifestTemplates)
            {
              TemplateMap theMappingTemplate = null;
              bool found = false;

              foreach (TemplateMap mappingTemplate in mappingTemplates)
              {
                if (found) break;

                if (mappingTemplate.id == manifestTemplate.id)
                {
                  int rolesMatchedCount = 0;

                  foreach (RoleMap roleMap in mappingTemplate.roleMaps)
                  {
                    if (found) break;

                    foreach (Role manifestRole in manifestTemplate.roles)
                    {
                      if (manifestRole.id == roleMap.id)
                      {
                        if (roleMap.type == RoleType.Reference && roleMap.classMap == null && roleMap.value == manifestRole.value)
                        {
                          theMappingTemplate = mappingTemplate;
                          found = true;
                        }

                        rolesMatchedCount++;
                        break;
                      }
                    }
                  }

                  if (rolesMatchedCount == manifestTemplate.roles.Count)
                  {
                    theMappingTemplate = mappingTemplate;
                  }
                }
              }

              if (theMappingTemplate != null)
              {
                TemplateMap crossedTemplateMap = theMappingTemplate.Clone();
                crossedTemplates.Add(crossedTemplateMap);

                // assume that all roles within a template are matched, thus only interested in classMap
                foreach (Role manifestRole in manifestTemplate.roles)
                {
                  if (manifestRole.@class != null)
                  {
                    foreach (RoleMap mappingRole in theMappingTemplate.roleMaps)
                    {
                      if (mappingRole.classMap != null && mappingRole.classMap.id == manifestRole.@class.id)
                      {
                        RecurBuildCrossGraphMap(ref manifestGraph, manifestRole.@class, mappingGraph, mappingRole.classMap);
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    //NOTE: only MD5 hash algorithm is supported at current
    private DataTransferIndices BuildDataTransferIndices(ref IList<IDataObject> dataObjects, ref Dictionary<string, List<string>> classIdentifiers, 
      string hashAlgorithm, string sortIndex)
    {
      DataTransferIndices dataTransferIndices = new DataTransferIndices();

      for (int i = 0; i < dataObjects.Count; i++)
      {
        DataTransferIndex dti = new DataTransferIndex();
        dataTransferIndices.DataTransferIndexList.Add(dti);

        bool firstClassMap = true;
        StringBuilder propertyValues = new StringBuilder();

        foreach (var pair in _graphMap.classTemplateMaps)
        {
          ClassMap classMap = pair.classMap;
          List<TemplateMap> templateMaps = pair.templateMaps;

          if (firstClassMap)
          {
            dti.Identifier = classIdentifiers[classMap.id][i];
            firstClassMap = false;
          }

          foreach (TemplateMap templateMap in templateMaps)
          {
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              if (roleMap.type == RoleType.Property ||
                  roleMap.type == RoleType.DataProperty ||
                  roleMap.type == RoleType.ObjectProperty)
              {
                string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectName.Length + 1);
                string value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

                if (!String.IsNullOrEmpty(roleMap.valueListName))
                {
                  value = _mapping.ResolveValueList(roleMap.valueListName, value);

                  if (value == "rdf:nil")
                    value = String.Empty;
                }

                if (propertyName == sortIndex)
                {
                  dti.SortIndex = value;
                }

                propertyValues.Append(value);
              }
            }
          }
        }

        if (String.IsNullOrEmpty(hashAlgorithm) || hashAlgorithm.ToUpper() == "MD5")
          dti.HashValue = Utility.MD5Hash(propertyValues.ToString());
      }

      return dataTransferIndices;
    }
  }
}