using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.library;
using org.iringtools.library.manifest;
using org.iringtools.utility;
using System.Collections.Specialized;
using Ninject;
using Ninject.Extensions.Xml;
using System.IO;
using log4net;
using Microsoft.ServiceModel.Web;
using StaticDust.Configuration;
using org.iringtools.adapter;

namespace org.iringtools.exchange
{
  public class ExchangeProvider
  {
    private static readonly XNamespace DTO_NS = "http://iringtools.org/adapter/library/dto";
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private Response _response = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private List<ScopeProject> _scopes = null;
    private IDataLayer _dataLayer = null;
    //private DataDictionary _dataDictionary = null;
    private Mapping _mapping = null;
    private Manifest _manifest = null;
    private GraphMap _mappingGraph = null;
    private Graph _manifestGraph = null;
    private GraphMap _crossedGraph = null;
    private HashAlgorithm _hashAlgorithm;

    private IList<IDataObject> _dataObjects = new List<IDataObject>();
    private Dictionary<string, List<string>> _classIdentifiers = new Dictionary<string, List<string>>();

    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;

    [Inject]
    public ExchangeProvider(NameValueCollection settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false };
      _kernel = new StandardKernel(ninjectSettings, new AdapterModule());

      _kernel.Load(new XmlExtensionModule());
      _settings = _kernel.Get<AdapterSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

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

    public XElement GetDxi(string projectName, string applicationName, string graphName, DXRequest request)
    {
      InitializeScope(projectName, applicationName);
      InitializeDataLayer();

      _manifest = request.Manifest;

      if (request.ContainsKey("hashAlgorithm"))
      {
        string hashAlgorithm = request["hashAlgorithm"];

        if (String.IsNullOrEmpty(hashAlgorithm))
        {
          _hashAlgorithm = HashAlgorithm.MD5;
        }
        else
        {
          _hashAlgorithm = (HashAlgorithm)Enum.Parse(typeof(HashAlgorithm), hashAlgorithm);
        }
      }

      BuildCrossedGraphMap(graphName);
      PopulateClassIdentifiers(null);

      XElement dxiList = new XElement(DTO_NS + "dataTransferIndices");
      for (int i = 0; i < _dataObjects.Count; i++)
      {
        XElement dxi = new XElement(DTO_NS + "dataTransferIndex");
        dxiList.Add(dxi);

        bool firstClassMap = true;
        StringBuilder propertyValues = new StringBuilder();

        foreach (var pair in _crossedGraph.classTemplateListMaps)
        {
          ClassMap classMap = pair.Key;
          List<TemplateMap> templateMaps = pair.Value;

          if (firstClassMap)
          {
            dxi.Add(new XElement(DTO_NS + "identifier", _classIdentifiers[classMap.classId][i]));
            firstClassMap = false;
          }

          foreach (TemplateMap templateMap in templateMaps)
          {
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              if (roleMap.type == RoleType.Property)
              {
                string propertyName = roleMap.propertyName.Substring(_crossedGraph.dataObjectMap.Length + 1);
                string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (!String.IsNullOrEmpty(roleMap.valueList))
                {
                  value = _mapping.ResolveValueList(roleMap.valueList, value);
                }

                propertyValues.Append(value);
              }
            }
          }
        }

        string hashValue = String.Empty;

        // todo: handle/implement more hash algorithms
        switch (_hashAlgorithm)
        {
          default: // MD5
            hashValue = Utility.MD5Hash(propertyValues.ToString());
            break;
        }

        dxi.Add(new XElement(DTO_NS + "hashValue", hashValue));
      }

      return dxiList;
    }

    public XElement GetDto(string projectName, string applicationName, string graphName, DXRequest request)
    {
      InitializeScope(projectName, applicationName);
      InitializeDataLayer();

      _manifest = request.Manifest;
      Identifiers identifiers = request.Identifiers;

      BuildCrossedGraphMap(graphName);
      PopulateClassIdentifiers(identifiers);

      XElement dtoList = new XElement(DTO_NS + "dataTransferObjects");

      for (int i = 0; i < _dataObjects.Count; i++)
      {
        XElement dto = new XElement(DTO_NS + "dataTransferObject");
        dtoList.Add(dto);

        XElement classObjects = new XElement(DTO_NS + "classObjects");
        dto.Add(classObjects);

        foreach (var pair in _crossedGraph.classTemplateListMaps)
        {
          ClassMap classMap = pair.Key;
          List<TemplateMap> templateMaps = pair.Value;

          XElement classObject = new XElement(DTO_NS + "classObject");
          classObjects.Add(classObject);

          classObject.Add(new XElement(DTO_NS + "classId", classMap.classId));
          classObject.Add(new XElement(DTO_NS + "name", classMap.name));
          classObject.Add(new XElement(DTO_NS + "identifier", _classIdentifiers[classMap.classId][i]));

          XElement templateObjects = new XElement(DTO_NS + "templateObjects");
          classObjects.Add(templateObjects);          

          foreach (TemplateMap templateMap in templateMaps)
          {
            XElement templateObject = new XElement(DTO_NS + "templateObject");
            templateObjects.Add(templateObject);

            templateObject.Add(new XElement(DTO_NS + "templateId", templateMap.templateId));
            templateObject.Add(new XElement(DTO_NS + "name", templateMap.name));

            XElement roleObjects = new XElement(DTO_NS + "roleObjects");
            templateObject.Add(roleObjects);

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              XElement roleObject = new XElement(DTO_NS + "roleObject");
              roleObjects.Add(roleObject);

              roleObject.Add(new XElement(DTO_NS + "roleId", roleMap.roleId));
              roleObject.Add(new XElement(DTO_NS + "name", roleMap.name));

              if (roleMap.type == RoleType.Property)
              {
                string propertyName = roleMap.propertyName.Substring(_crossedGraph.dataObjectMap.Length + 1);
                string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (!String.IsNullOrEmpty(roleMap.valueList))
                {
                  value = _mapping.ResolveValueList(roleMap.valueList, value);
                }

                roleObject.Add(new XElement(DTO_NS + "value", value));
              }
              else if (!String.IsNullOrEmpty(roleMap.value))                
              {
                if (roleMap.type == RoleType.Reference)
                {
                  roleObject.Add(new XElement(DTO_NS + "reference", roleMap.value));
                }
                else
                {
                  roleObject.Add(new XElement(DTO_NS + "value", roleMap.value));
                }
              }
            }
          }
        }
      }

      return dtoList;
    }

    #region helper methods
    private void InitializeScope(string projectName, string applicationName)
    {
      try
      {
        if (!_isScopeInitialized)
        {
          bool isScopeValid = false;
          foreach (ScopeProject project in _scopes)
          {
            if (project.Name == projectName)
            {
              foreach (ScopeApplication application in project.Applications)
              {
                if (application.Name == applicationName)
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
                new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateDataLayer")
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
          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");

          //_dataDictionary = _dataLayer.GetDictionary();
          //_kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);

          _isDataLayerInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void PopulateClassIdentifiers(List<string> identifiers)
    {
      _dataObjects = _dataLayer.Get(_crossedGraph.dataObjectMap, identifiers);  
      _classIdentifiers.Clear();

      foreach (ClassMap classMap in _crossedGraph.classTemplateListMaps.Keys)
      {
        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
              if (classIdentifiers.Count == i)
              {
                classIdentifiers.Add(value);
              }
              else
              {
                classIdentifiers[i] += classMap.identifierDelimeter + value;
              }
            }
          }
          else  // identifier comes from a property
          {
            string[] property = identifier.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();

            if (_dataObjects != null)
            {
              for (int i = 0; i < _dataObjects.Count; i++)
              {
                string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (classIdentifiers.Count == i)
                {
                  classIdentifiers.Add(value);
                }
                else
                {
                  classIdentifiers[i] += classMap.identifierDelimeter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.classId] = classIdentifiers;
      }
    }

    private void BuildCrossedGraphMap(string graphName)
    {
      _mappingGraph = _mapping.FindGraphMap(graphName);
      _manifestGraph = _manifest.FindGraph(graphName);      
      
      _crossedGraph = new GraphMap();
      _crossedGraph.dataObjectMap = _mappingGraph.dataObjectMap;

      ClassTemplatesMap manifestClassTemplatesMap = _manifestGraph.ClassTemplatesMaps.First();
      Class manifestClass = manifestClassTemplatesMap.Class;

      if (manifestClassTemplatesMap != null)
      {
        foreach (var mappingClassTemplatesMap in _mappingGraph.classTemplateListMaps)
        {
          ClassMap mappingClass = mappingClassTemplatesMap.Key;

          if (mappingClass.classId == manifestClass.ClassId)
          {
            RecurBuildCrossedGraphMap(manifestClass, mappingClass);
          }
        }
      }
    }

    private void RecurBuildCrossedGraphMap(Class manifestClass, ClassMap mappingClass)
    {
      List<Template> manifestTemplates = null;

      // find manifest templates for the manifest class
      foreach (ClassTemplatesMap manifestClassTemplates in _manifestGraph.ClassTemplatesMaps)
      {
        if (manifestClassTemplates.Class.ClassId == manifestClass.ClassId)
        {
          manifestTemplates = manifestClassTemplates.Templates;
        }
      }

      if (manifestTemplates != null)
      {
        // find mapping templates for the mapping class
        foreach (var pair in _mappingGraph.classTemplateListMaps)
        {
          ClassMap localMappingClass = pair.Key;
          List<TemplateMap> mappingTemplates = pair.Value;

          if (localMappingClass.classId == manifestClass.ClassId)
          {
            ClassMap crossedClass = new ClassMap(localMappingClass);
            List<TemplateMap> crossedTemplates = new List<TemplateMap>();

            _crossedGraph.classTemplateListMaps.Add(crossedClass, crossedTemplates);

            foreach (Template manifestTemplate in manifestTemplates)
            {
              foreach (TemplateMap mappingTemplate in mappingTemplates)
              {
                if (mappingTemplate.templateId == manifestTemplate.TemplateId)
                {
                  TemplateMap crossedTemplateMap = new TemplateMap(mappingTemplate);
                  crossedTemplates.Add(crossedTemplateMap);

                  // assume that all roles within a template are matched, thus only interested in classMap
                  foreach (Role manifestRole in manifestTemplate.Roles)
                  {
                    if (manifestRole.Class != null)
                    {
                      foreach (RoleMap mappingRole in mappingTemplate.roleMaps)
                      {
                        if (mappingRole.classMap != null && mappingRole.classMap.classId == manifestRole.Class.ClassId)
                        {
                          RecurBuildCrossedGraphMap(manifestRole.Class, mappingRole.classMap);
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
    }
    #endregion
  }
}
