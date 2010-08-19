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
using VDS.RDF.Query;
using System.Net;

namespace org.iringtools.exchange
{
  public class ExchangeProvider
  {
    private static readonly XNamespace DTO_NS = "http://iringtools.org/adapter/library/dto";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";
    
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private Response _response = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private List<ScopeProject> _scopes = null;
    private IDataLayer _dataLayer = null;
    private IProjectionLayer _projectionEngine = null;
    //private DataDictionary _dataDictionary = null;
    private Mapping _mapping = null;
    private Manifest _manifest = null;
    private GraphMap _mappingGraph = null;
    private Graph _manifestGraph = null;
    private GraphMap _graphMap = null;
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

    public Response PullDTO(string projectName, string applicationName, string graphName, Request request)
    {
      String targetUri = String.Empty;
      String targetCredentialsXML = String.Empty;
      String filter = String.Empty;
      String projectNameForPull = String.Empty;
      String applicationNameForPull = String.Empty;
      String graphNameForPull = String.Empty;
      String dataObjectsString = String.Empty;
      Status status = new Status();
      status.Messages = new List<string>();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        _projectionEngine = _kernel.Get<IProjectionLayer>("dto");

        targetUri = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        graphNameForPull = request["targetGraphName"];
        filter = request["filter"];
        projectNameForPull = request["projectName"];
        applicationNameForPull = request["applicationName"];

        WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
        if (targetCredentials.isEncrypted) targetCredentials.Decrypt();

        WebHttpClient httpClient = new WebHttpClient(targetUri);
        if (filter != String.Empty)
        {
          dataObjectsString = httpClient.GetMessage(@"/" + projectNameForPull + "/" + applicationNameForPull + "/" + graphNameForPull + "/" + filter + "?format=dto");
        }
        else
        {
          dataObjectsString = httpClient.GetMessage(@"/" + projectNameForPull + "/" + applicationNameForPull + "/" + graphNameForPull + "?format=dto");
        }
        XElement xml = XElement.Parse(dataObjectsString);

        IList<IDataObject> dataObjects = _projectionEngine.GetDataObjects(graphName, ref xml);

        _response.Append(_dataLayer.Post(dataObjects));
        status.Messages.Add(String.Format("Pull is successful from " + targetUri + "for Graph " + graphName));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PullDTO: " + ex);

        status.Level = StatusLevel.Error;
        status.Messages.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
        status.Messages.Add(ex.ToString());
      }

      _response.Append(status);
      return _response;
    }

    public Response Pull(string projectName, string applicationName, string graphName, Request request)
    {
      Status status = new Status();
      status.Messages = new List<string>();
      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        DateTime startTime = DateTime.Now;

        #region move this portion to dotNetRdfEngine?
        if (!request.ContainsKey("targetEndpointUri"))
          throw new Exception("Target Endpoint Uri is required");

        string targetEndpointUri = request["targetEndpointUri"];
        //if (!targetEndpointUri.EndsWith("/")) targetEndpointUri += "/";

        if (!request.ContainsKey("targetGraphBaseUri"))
          throw new Exception("Target graph uri is required");

        string targetGraphBaseUri = request["targetGraphBaseUri"];

        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(targetEndpointUri), targetGraphBaseUri);

        if (request.ContainsKey("targetCredentials"))
        {
          string targetCredentialsXML = request["targetCredentials"];
          WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);

          if (targetCredentials.isEncrypted)
            targetCredentials.Decrypt();

          endpoint.SetCredentials(targetCredentials.GetNetworkCredential().UserName,targetCredentials.GetNetworkCredential().Password);
        }

        string proxyHost = _settings["ProxyHost"];
        string proxyPort = _settings["ProxyPort"];
        if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
        {
          WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));

          WebProxyCredentials proxyCrendentials = _settings.GetProxyCredentials();
          if (proxyCrendentials != null)
          {
            endpoint.UseCredentialsForProxy = true;
            webProxy.Credentials = proxyCrendentials.GetNetworkCredential();
          }
          endpoint.SetProxyCredentials(proxyCrendentials.userName,proxyCrendentials.password);
          endpoint.SetProxy(webProxy.Address);
        }

        VDS.RDF.Graph graph = endpoint.QueryWithResultGraph("CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}");
        #endregion

        // call RdfProjectionEngine to fill data objects from a given graph
        _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        TextWriter textWriter = new StringWriter(sb);
        VDS.RDF.Writing.RdfXmlTreeWriter rdfWriter = new VDS.RDF.Writing.RdfXmlTreeWriter();
        rdfWriter.Save(graph, textWriter);
        XElement rdf = XElement.Parse(sb.ToString());

        _dataObjects = _projectionEngine.GetDataObjects(graphName, ref rdf);

        // post data objects to data layer
        _dataLayer.Post(_dataObjects);

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);

        status.Messages.Add(string.Format("Graph [{0}] has been posted to legacy system successfully.", graphName));

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

    //Gets from datalayer and send it to another endpoint
    public Response Push(string projectName, string applicationName, string graphName, PushRequest request)
    {
      Status status = new Status();
      status.Messages = new List<string>();
      try
      {
        String targetUri = String.Empty;
        String targetCredentialsXML = String.Empty;
        String filter = String.Empty;
        String projectNameForPush = String.Empty;
        String applicationNameForPush = String.Empty;
        String graphNameForPush = String.Empty;
        String format = String.Empty;
        targetUri = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        filter = request["filter"];
        projectNameForPush = request["targetProjectName"];
        applicationNameForPush = request["targetApplicationName"];
        graphNameForPush = request["targetGraphName"];
        format = request["format"];

        WebHttpClient httpClient = new WebHttpClient(targetUri);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graphName);

        _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        IList<IDataObject> dataObjectList;
        if (filter != String.Empty)
        {
          IList<string> identifiers = new List<string>();
          identifiers.Add(filter);
          dataObjectList = _dataLayer.Get(_graphMap.dataObjectMap, identifiers);
        }
        else
        {
          dataObjectList = _dataLayer.Get(_graphMap.dataObjectMap, null);
        }

        XElement xml = _projectionEngine.GetXml(graphName, ref dataObjectList);

        _isDataLayerInitialized = false;
        _isScopeInitialized = false;
        Response localResponse = httpClient.Post<XElement, Response>(@"/" + projectNameForPush + "/" + applicationNameForPush + "/" + graphNameForPush + "?format=" + format, xml, true);

        _response.Append(localResponse);

        foreach (Status responseStatus in localResponse.StatusList)
        {
          string dataObjectName = request.ExpectedResults.DataObjectName;

          IList<IDataObject> dataObjects = _dataLayer.Get(
            dataObjectName, new List<string> { responseStatus.Identifier });

          foreach (var resultMap in request.ExpectedResults)
          {
            string propertyValue = responseStatus.Results[resultMap.Key];
            string dataPropertyName = resultMap.Value;

            dataObjects[0].SetPropertyValue(dataPropertyName, propertyValue);
          }

          _response.Append(_dataLayer.Post(dataObjects));
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in pushing data", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error in pushing data: {0}", ex));
      }
      return _response;

    }

    public XElement GetDxi(string projectName, string applicationName, string graphName, DXRequest request)
    {
      _manifest = request.Manifest;

      InitializeScope(projectName, applicationName);
      InitializeDataLayer();

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

        foreach (var pair in _graphMap.classTemplateListMaps)
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
                string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
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
      _manifest = request.Manifest;
      Identifiers identifiers = request.Identifiers;

      InitializeScope(projectName, applicationName);
      InitializeDataLayer();

      BuildCrossedGraphMap(graphName);
      PopulateClassIdentifiers(identifiers);

      XElement dtoList = new XElement(DTO_NS + "dataTransferObjects");

      for (int i = 0; i < _dataObjects.Count; i++)
      {
        XElement dto = new XElement(DTO_NS + "dataTransferObject");
        dtoList.Add(dto);

        XElement classObjects = new XElement(DTO_NS + "classObjects");
        dto.Add(classObjects);

        foreach (var pair in _graphMap.classTemplateListMaps)
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

              roleObject.Add(new XElement(DTO_NS + "type", roleMap.type));
              roleObject.Add(new XElement(DTO_NS + "roleId", roleMap.roleId));
              roleObject.Add(new XElement(DTO_NS + "name", roleMap.name));

              switch (roleMap.type)
              {
                case RoleType.Property:
                  string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
                  string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                  if (!String.IsNullOrEmpty(roleMap.valueList))
                  {
                    value = _mapping.ResolveValueList(roleMap.valueList, value);
                    value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                  }

                  roleObject.Add(new XElement(DTO_NS + "value", value));
                  break;

                case RoleType.FixedValue:
                  roleObject.Add(new XElement(DTO_NS + "value", roleMap.value));
                  break;

                case RoleType.Reference:
                  if (roleMap.classMap != null)
                  {
                    roleObject.Add(new XElement(DTO_NS + "value", "#" + _classIdentifiers[roleMap.classMap.classId][i]));
                  }
                  else
                  {
                    roleObject.Add(new XElement(DTO_NS + "value", roleMap.value));
                  }
                  break;
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
      _dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, identifiers);  
      _classIdentifiers.Clear();

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
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
      
      _graphMap = new GraphMap();
      _graphMap.dataObjectMap = _mappingGraph.dataObjectMap;

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

            _graphMap.classTemplateListMaps.Add(crossedClass, crossedTemplates);

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
