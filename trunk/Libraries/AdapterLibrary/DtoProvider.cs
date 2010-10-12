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
using org.iringtools.library.manifest;
using System.Text;

namespace org.iringtools.adapter
{
  public class DtoProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProvider));

    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private List<ScopeProject> _scopes = null;
    private IDataLayer _dataLayer = null;
    private DataDictionary _dataDictionary = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;

    [Inject]
    public DtoProvider(NameValueCollection settings)
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
        _scopes = Utility.Read<List<ScopeProject>>(scopesPath);
      }
      else
      {
        _scopes = new List<ScopeProject>();
        Utility.Write<List<ScopeProject>>(_scopes, scopesPath);
      }
    }

    public DataTransferIndices GetDataTransferIndices(string scope, string app, string graph, string hashAlogrithm)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graph);

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, null);
        Dictionary<string, List<string>> classIdentifiers = GetClassIdentifiers(ref dataObjects);

        dataTransferIndices = BuildDataTransferIndices(ref dataObjects, ref classIdentifiers, hashAlogrithm);
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

        IList<string> identifiers = new List<string>();
        foreach (DataTransferIndex dti in dataTransferIndices)
        {
          identifiers.Add(dti.Identifier);
        }

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, identifiers);
        DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");

        dataTransferObjects = dtoProjectionEngine.ToDataTransferObjects(_graphMap, ref dataObjects);
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
        for (int i = 0; i < dataTransferObjects.Count; i++)
        {
          if (dataTransferObjects[i].transferType == TransferType.Delete)
          {
            deleteIdentifiers.Add(dataTransferObjects[i].Identifier);
            dataTransferObjects.RemoveAt(i--);
          }
        }

        DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
        IList<IDataObject> dataObjects = dtoProjectionEngine.ToDataObjects(_graphMap, ref dataTransferObjects);

        if (dataObjects != null && dataObjects.Count > 0)
          response.Append(_dataLayer.Post(dataObjects));  // add/change/sync data objects

        if (deleteIdentifiers.Count > 0)
          response.Append(_dataLayer.Delete(_graphMap.dataObjectMap, deleteIdentifiers));
        
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
        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, identifiers);

        DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
        dataTransferObjects = dtoProjectionEngine.ToDataTransferObjects(_graphMap, ref dataObjects);
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
        response.Append(_dataLayer.Delete(_graphMap.dataObjectMap, identifiers));
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

    public DataTransferIndices GetDataTransferIndicesWithManifest(string scope, string app, string graph, string hashAlgorithm, Manifest manifest)
    {
      DataTransferIndices dataTransferIndices = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        BuildCrossGraphMap(manifest, graph);

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, null);
        Dictionary<string, List<string>> classIdentifiers = GetClassIdentifiers(ref dataObjects);

        dataTransferIndices = BuildDataTransferIndices(ref dataObjects, ref classIdentifiers, hashAlgorithm);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer indices: " + ex);
      }

      return dataTransferIndices;
    }

    // get list (page) of data transfer objects per dto page request
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DtoPageRequest dtoPageRequest)
    {
      DataTransferObjects dataTransferObjects = null;

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();

        BuildCrossGraphMap(dtoPageRequest.Manifest, graph);
        
        IList<string> identifiers = new List<string>();
        foreach (DataTransferIndex dti in dtoPageRequest.DataTransferIndices)
        {
          identifiers.Add(dti.Identifier);
        }

        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, identifiers);
        Dictionary<string, List<string>> classIdentifiers = GetClassIdentifiers(ref dataObjects);

        DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
        dataTransferObjects = dtoProjectionEngine.ToDataTransferObjects(_graphMap, ref dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data transfer objects: " + ex);
      }

      return dataTransferObjects;
    }

    public Manifest GetManifest(string scope, string app)
    {
      Manifest manifest = new Manifest();

      try
      {
        InitializeScope(scope, app);
        InitializeDataLayer();
        
        DataDictionary dataDictionary = _dataLayer.GetDictionary();

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          Graph manifestGraph = new Graph { Name = graphMap.name };
          manifest.Graphs.Add(manifestGraph);

          string dataObjectMap = graphMap.dataObjectMap;
          DataObject dataObject = null;

          foreach (DataObject dataObj in dataDictionary.dataObjects)
          {
            if (dataObj.objectName == dataObjectMap)
            {
              dataObject = dataObj;
              break;
            }
          }

          if (dataObject != null)
          {
            foreach (var classTemplateListMap in graphMap.classTemplateListMaps)
            {
              ClassTemplates manifestClassTemplatesMap = new ClassTemplates();
              manifestGraph.ClassTemplatesList.Add(manifestClassTemplatesMap);

              ClassMap classMap = classTemplateListMap.Key;
              List<TemplateMap> templateMaps = classTemplateListMap.Value;

              Class manifestClass = new Class
              {
                ClassId = classMap.classId,
                Name = classMap.name,
              };
              manifestClassTemplatesMap.Class = manifestClass;

              foreach (TemplateMap templateMap in templateMaps)
              {
                Template manifestTemplate = new Template 
                {
                  TemplateId = templateMap.templateId,
                  Name = templateMap.name,
                  TransferOption = TransferOption.Desired,
                };
                manifestClassTemplatesMap.Templates.Add(manifestTemplate);

                foreach (RoleMap roleMap in templateMap.roleMaps)
                {
                  Role manifestRole = new Role
                  {
                    Type = roleMap.type,
                    RoleId = roleMap.roleId,
                    Name = roleMap.name,
                    DataType = roleMap.dataType,
                    Value = roleMap.value,
                  };
                  manifestTemplate.Roles.Add(manifestRole);

                  if (roleMap.type == RoleType.Property)
                  {
                    string[] property = roleMap.propertyName.Split('.');
                    string objectName = property[0].Trim();
                    string propertyName = property[1].Trim();

                    if (dataObject.isKeyProperty(propertyName))
                    {
                      manifestTemplate.TransferOption = TransferOption.Required;
                    }
                  }

                  if (roleMap.classMap != null)
                  {
                    manifestRole.Class = new Class
                    {
                      ClassId = roleMap.classMap.classId,
                      Name = roleMap.classMap.name,
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

    public org.iringtools.library.Version GetVersion()
    {
      System.Version version = this.GetType().Assembly.GetName().Version;

      return new org.iringtools.library.Version()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
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

    private Dictionary<string, List<string>> GetClassIdentifiers(ref IList<IDataObject> dataObjects)
    {
      Dictionary<string, List<string>> classIdentifiers = new Dictionary<string, List<string>>();

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
      {
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

        classIdentifiers[classMap.classId] = identifiers;
      }

      return classIdentifiers;
    }

    // build cross graph map from manifest graph and mapping graph and save it in _graphMap
    private void BuildCrossGraphMap(Manifest manifest, string graph)
    {
      GraphMap mappingGraph = _mapping.FindGraphMap(graph);
      Graph manifestGraph = manifest.FindGraph(graph);

      _graphMap = new GraphMap();
      _graphMap.dataObjectMap = mappingGraph.dataObjectMap;

      ClassTemplates manifestClassTemplatesMap = manifestGraph.ClassTemplatesList.First();
      Class manifestClass = manifestClassTemplatesMap.Class;

      if (manifestClassTemplatesMap != null)
      {
        foreach (var mappingClassTemplatesMap in mappingGraph.classTemplateListMaps)
        {
          ClassMap mappingClass = mappingClassTemplatesMap.Key;

          if (mappingClass.classId == manifestClass.ClassId)
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
      foreach (ClassTemplates manifestClassTemplates in manifestGraph.ClassTemplatesList)
      {
        if (manifestClassTemplates.Class.ClassId == manifestClass.ClassId)
        {
          manifestTemplates = manifestClassTemplates.Templates;
        }
      }

      if (manifestTemplates != null)
      {
        // find mapping templates for the mapping class
        foreach (var pair in mappingGraph.classTemplateListMaps)
        {
          ClassMap localMappingClass = pair.Key;
          List<TemplateMap> mappingTemplates = pair.Value;

          if (localMappingClass.classId == manifestClass.ClassId)
          {
            ClassMap crossedClass = new ClassMap(localMappingClass);
            List<TemplateMap> crossedTemplates = new List<TemplateMap>();

            _graphMap.classTemplateListMaps.Add(crossedClass, crossedTemplates);

            foreach (Template manifestTemplate in manifestTemplates)
            {
              TemplateMap theMappingTemplate = null;
              bool found = false;

              foreach (TemplateMap mappingTemplate in mappingTemplates)
              {
                if (found) break;

                if (mappingTemplate.templateId == manifestTemplate.TemplateId)
                {
                  int rolesMatchedCount = 0;

                  foreach (RoleMap roleMap in mappingTemplate.roleMaps)
                  {
                    if (found) break;

                    foreach (Role manifestRole in manifestTemplate.Roles)
                    {
                      if (manifestRole.RoleId == roleMap.roleId)
                      {
                        if (roleMap.type == RoleType.Reference && roleMap.classMap == null && roleMap.value == manifestRole.Value)
                        {
                          theMappingTemplate = mappingTemplate;
                          found = true;
                        }

                        rolesMatchedCount++;
                        break;
                      }
                    }
                  }

                  if (rolesMatchedCount == manifestTemplate.Roles.Count)
                  {
                    theMappingTemplate = mappingTemplate;
                  }
                }
              }

              if (theMappingTemplate != null)
              {
                TemplateMap crossedTemplateMap = new TemplateMap(theMappingTemplate);
                crossedTemplates.Add(crossedTemplateMap);

                // assume that all roles within a template are matched, thus only interested in classMap
                foreach (Role manifestRole in manifestTemplate.Roles)
                {
                  if (manifestRole.Class != null)
                  {
                    foreach (RoleMap mappingRole in theMappingTemplate.roleMaps)
                    {
                      if (mappingRole.classMap != null && mappingRole.classMap.classId == manifestRole.Class.ClassId)
                      {
                        RecurBuildCrossGraphMap(ref manifestGraph, manifestRole.Class, mappingGraph, mappingRole.classMap);
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
    private DataTransferIndices BuildDataTransferIndices(ref IList<IDataObject> dataObjects, ref Dictionary<string, List<string>> classIdentifiers, string hashAlgorithm)
    {
      DataTransferIndices dataTransferIndices = new DataTransferIndices();

      for (int i = 0; i < dataObjects.Count; i++)
      {
        DataTransferIndex dti = new DataTransferIndex();
        dataTransferIndices.Add(dti);

        bool firstClassMap = true;
        StringBuilder propertyValues = new StringBuilder();

        foreach (var pair in _graphMap.classTemplateListMaps)
        {
          ClassMap classMap = pair.Key;
          List<TemplateMap> templateMaps = pair.Value;

          if (firstClassMap)
          {
            dti.Identifier = classIdentifiers[classMap.classId][i];
            firstClassMap = false;
          }

          foreach (TemplateMap templateMap in templateMaps)
          {
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              if (roleMap.type == RoleType.Property)
              {
                string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
                string value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

                if (!String.IsNullOrEmpty(roleMap.valueList))
                {
                  value = _mapping.ResolveValueList(roleMap.valueList, value);
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