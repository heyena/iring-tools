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
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Ninject;
using org.iringtools.adapter.datalayer;
using org.iringtools.library;
using org.iringtools.utility;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using System.Collections.Specialized;
using Ninject.Parameters;
using System.IO;
using log4net;
using Ninject.Contrib.Dynamic;
using NHibernate;
using org.w3.sparql_results;
using Microsoft.ServiceModel.Web;
using System.Net;
using org.ids_adi.qmxf;
using StaticDust.Configuration;

namespace org.iringtools.adapter
{
  public class AdapterProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private IDataLayer _dataLayer = null;
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

    private bool _isInitialized = false;

    [Inject]
    public AdapterProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(new AdapterModule());
      _settings = _kernel.Get<AdapterSettings>();
      _settings.AppendSettings(settings);

      //TODO: Move me!
      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string rdsUri = _settings["ReferenceDataServiceUri"];
     
      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));
        WebProxyCredentials proxyCrendentials = _settings.GetProxyCredentials();

        if (proxyCrendentials != null)
        {
          webProxy.Credentials = proxyCrendentials.GetNetworkCredential();
        }

        _webHttpClient = new WebHttpClient(rdsUri, null, webProxy);
      }
      else
      {
        _webHttpClient = new WebHttpClient(rdsUri);
      }
      #endregion

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);
    }

    #region public methods
    public List<ScopeProject> GetScopes()
    {
      string path = string.Format("{0}Scopes.xml", _settings["XmlPath"]);

      try
      {
        if (File.Exists(path))
        {
          return Utility.Read<List<ScopeProject>>(path);
        }

        return new List<ScopeProject>();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetScopes: {0}", ex));
        throw new Exception(string.Format("Error getting the list of projects/applications from path [{0}]: {1}", path, ex));
      }
    }

    public Manifest GetManifest(string projectName, string applicationName)
    {
      string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings["XmlPath"], projectName, applicationName);

      try
      {
        Initialize(projectName, applicationName);

        Manifest manifest = new Manifest();
        manifest.Graphs = new List<ManifestGraph>();

        if (File.Exists(path))
        {
          foreach (GraphMap graphMap in _mapping.graphMaps)
          {
            ManifestGraph manifestGraph = new ManifestGraph { GraphName = graphMap.name };
            manifest.Graphs.Add(manifestGraph);
          }
        }

        return manifest;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetManifest: {0}", ex));
        throw new Exception(string.Format("Error getting manifest from path [{0}: {1}", path, ex));
      }
    }

    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      try
      {
        Initialize(projectName, applicationName);
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
        Initialize(projectName, applicationName);
        return _mapping;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetMapping: {0}", ex));
        throw new Exception(string.Format("Error getting mapping: {0}", ex));
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
            newRoleMap.type = RoleType.Property;
            newRoleMap.propertyName = dataObjectMap + "." + propertyName;

            if (!String.IsNullOrEmpty(valueList))
            {
              newRoleMap.valueList = valueList;
            }
            else
            {
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

    public Response UpdateMapping(string projectName, string applicationName, XElement mappingXml)
    {
      Response response = new Response();
      string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings["XmlPath"], projectName, applicationName);

      try
      {
        if (!mappingXml.Name.NamespaceName.Contains("schemas.datacontract.org"))
        {
          response.Add("Detected old mapping. Attempting to convert it...");

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

          response.Add("Old mapping has been converted sucessfully.");
          
          Utility.Write<Mapping>(mapping, path, true);
        }
        else
        {
          Mapping mapping = Utility.DeserializeDataContract<Mapping>(mappingXml.ToString());
          Utility.Write<Mapping>(mapping, path, true);
        }

        response.Add("Mapping file has been updated to path [" + path + "] successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateMapping: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error saving mapping file to path [{0}]: {1}", path, ex));
      }

      return response;
    }

    public Response RefreshAll(string projectName, string applicationName)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);

        DateTime start = DateTime.Now;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          response.Append(Refresh(graphMap.name));
        }

        DateTime end = DateTime.Now;
        TimeSpan duration = end.Subtract(start);

        response.Add(String.Format("RefreshAll() completed in [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in RefreshAll: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error refreshing all graphs: {0}", ex));
      }

      return response;
    }

    public Response Refresh(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);

        response.Append(Refresh(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error refreshing graph [{0}]: {1}", graphName, ex));
      }

      return response;
    }

    public XElement GetProjection(string projectName, string applicationName, string graphName, string identifier, string format)
    {
      try
      {
        Initialize(projectName, applicationName);

        IList<string> identifiers = new List<string>() { identifier };

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
        }

        _graphMap = _mapping.FindGraphMap(graphName);

        LoadDataObjectSet(identifiers);

        return _projectionEngine.GetXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public XElement GetProjection(string projectName, string applicationName, string graphName, string format)
    {
      try
      {
        Initialize(projectName, applicationName);

        if (format != null)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
        }

        _graphMap = _mapping.FindGraphMap(graphName);

        LoadDataObjectSet(null);

        return _projectionEngine.GetXml(graphName, ref _dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public IList<IDataObject> GetDataObjects(string projectName, string applicationName, string graphName, string format, XElement xml)
    {
        Initialize(projectName, applicationName);

        if (format != null)
        {
            _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        }
        else
        {
            _projectionEngine = _kernel.Get<IProjectionLayer>(_settings["DefaultProjectionFormat"]);
        }

        IList<IDataObject> dataObjects = _projectionEngine.GetDataObjects(graphName, ref xml);

        return dataObjects;
    }

    public Response PullDTO(string projectName, string applicationName, Request request)
    {
      String targetUri = String.Empty;
      String targetCredentialsXML = String.Empty;
      String graphName = String.Empty;      
      String filter = String.Empty;
      String projectNameForPull = String.Empty;
      String applicationNameForPull = String.Empty;
      String graphNameForPull = String.Empty;
      String dataObjectsString = String.Empty;
      Response response = new Response();
      try
      {
        Initialize(projectName, applicationName);
        
        _projectionEngine = _kernel.Get<IProjectionLayer>("dto");

        targetUri = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        graphName = request["graphName"];
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

        response.Append(_dataLayer.Post(dataObjects));
        response.Add(String.Format("Pull is successful from " + targetUri + "for Graph " + graphName));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PullDTO: " + ex);

        response.Level = StatusLevel.Error;
        response.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
        response.Add(ex.ToString());
      }
      return response;
    }

    public Response Pull(string projectName, string applicationName, Request request)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);
        DateTime startTime = DateTime.Now;

        #region move this portion to dotNetRdfEngine?
        if (!request.ContainsKey("targetUri"))
          throw new Exception("Target uri is required");

        string targetUri = request["targetUri"];
        if (!targetUri.EndsWith("/")) targetUri += "/";

        if (!request.ContainsKey("graphName"))
          throw new Exception("Graph name is required");

        string graphName = request["graphName"];

        string graphUri = _settings["GraphBaseUri"] + "/" + projectName + "/" + applicationName + "/" + graphName;        
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(targetUri), graphUri);

        if (request.ContainsKey("targetCredentials"))
        {
          string targetCredentialsXML = request["targetCredentials"];
          WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);

          if (targetCredentials.isEncrypted)
            targetCredentials.Decrypt();

          endpoint.SetCredentials(targetCredentials.GetNetworkCredential());
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
          
          endpoint.SetProxy(webProxy);          
        }

        Graph graph = endpoint.QueryWithResultGraph("CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}");
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

        response.Level = StatusLevel.Success;
        response.Add(string.Format("Graph [{0}] has been posted to legacy system successfully.", graphName));

        response.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Pull(): ", ex);

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error pulling graph: {0}", ex));
      }

      return response;
    }

    public Response DeleteAll(string projectName, string applicationName)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          response.Append(_semanticEngine.Delete(graphMap.name));
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting all graphs: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      return response;
    }

    public Response Delete(string projectName, string applicationName, string graphName)
    {
      Initialize(projectName, applicationName);

      return _semanticEngine.Delete(graphName);
    }

    public Response UpdateDatabaseDictionary(string projectName, string applicationName, DatabaseDictionary dbDictionary)
    {
      Response response = new Response();

      try
      {
        if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(applicationName))
        {
          response.Add("Error project name and application name can not be null");
        }
        else if (ValidateDatabaseDictionary(dbDictionary))
        {
          foreach (DataObject dataObject in dbDictionary.dataObjects)
          {
            RemoveDups(dataObject);
          }

          EntityGenerator generator = _kernel.Get<EntityGenerator>();
          response = generator.Generate(dbDictionary, projectName, applicationName);

          // Update binding configuration
          Binding dataLayerBinding = new Binding()
          {
            Name = "DataLayer",
            Interface = "org.iringtools.library.IDataLayer, iRINGLibrary",
            Implementation = "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateDataLayer"
          };
          UpdateBindingConfiguration(projectName, applicationName, dataLayerBinding);

          Binding semanticLayerBinding = new Binding()
          {
            Name = "SemanticLayer",
            Interface = "org.iringtools.adapter.ISemanticLayer, AdapterLibrary",
            Implementation = "org.iringtools.adapter.semantic.dotNetRdfEngine, AdapterLibrary"
          };
          UpdateBindingConfiguration(projectName, applicationName, semanticLayerBinding);

          Binding projectionLayerBinding = new Binding()
          {
            Name = "ProjectionLayer",
            Interface = "org.iringtools.adapter.IProjectionLayer, AdapterLibrary",
            Implementation = "org.iringtools.adapter.projection.RdfProjectionEngine, AdapterLibrary"
          };
          UpdateBindingConfiguration(projectName, applicationName, projectionLayerBinding);

          UpdateScopes(projectName, applicationName);

          response.Add("Database dictionary updated successfully.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateDatabaseDictionary: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error updating database dictionary: {0}", ex));
      }

      return response;
    }
    #endregion

    #region private methods
    private void Initialize(string projectName, string applicationName)
    {
      try
      {
        if (!_isInitialized)
        {
          string scope = string.Format("{0}.{1}", projectName, applicationName);
          
          _settings.Add("ProjectName",              projectName);
          _settings.Add("ApplicationName",          applicationName);
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

          string bindingConfigurationPath = String.Format("{0}BindingConfiguration.{1}.xml", 
            _settings["XmlPath"], 
            scope
          );
          
          BindingConfiguration bindingConfiguration = 
            Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);
          _kernel.Load(new DynamicModule(bindingConfiguration));

          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");
          _dataDictionary = _dataLayer.GetDictionary();
          _kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);

          string mappingPath = String.Format("{0}Mapping.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          _mapping = Utility.Read<Mapping>(mappingPath);
          _kernel.Bind<Mapping>().ToConstant(_mapping);

          _semanticEngine = _kernel.Get<ISemanticLayer>("SemanticLayer");

          _isInitialized = true;
        }
      }
      catch (Exception ex)
      {
        //if (ex.Message.Contains("Mapping"))
        //{
        //  _mapping = new Mapping();
        //  Utility.Write<Mapping>(_mapping, string.Format("{0}Mapping.{1}.{2}.xml", _settings["XmlPath"], projectName, applicationName));
        //}
        //else
        //{
          _logger.Error(string.Format("Error initializing application: {0}", ex));
          throw new Exception(string.Format("Error initializing application: {0})", ex));
        //}
      }
    }

    private Response Refresh(string graphName)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      LoadDataObjectSet(null);

      _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

      XElement rdf = _projectionEngine.GetXml(graphName, ref _dataObjects);

      return _semanticEngine.Refresh(graphName, rdf);
    }

    private void LoadDataObjectSet(IList<string> identifiers)
    {
      _dataObjects.Clear();
            
      if (identifiers != null)
        _dataObjects =_dataLayer.Get(_graphMap.dataObjectMap, identifiers);
      else
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectMap, null);
    }

    private void RemoveDups(DataObject dataObject)
    {
      try
      {
        /* GvR
        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          for (int j = 0; j < dataObject.dataProperties.Count; j++)
          {
            // remove columns that are already in keys
            if (dataObject.dataProperties[j].propertyName.ToLower() == dataObject.keyProperties[i].propertyName.ToLower())
            {
              dataObject.dataProperties.Remove(dataObject.dataProperties[j--]);
              continue;
            }

            // remove duplicate columns
            for (int jj = j + 1; jj < dataObject.dataProperties.Count; jj++)
            {
              if (dataObject.dataProperties[jj].propertyName.ToLower() == dataObject.dataProperties[j].propertyName.ToLower())
              {
                dataObject.dataProperties.Remove(dataObject.dataProperties[jj--]);
              }
            }
          }

          // remove duplicate keys (in order of foreign - assigned - iddataObject/sequence)
          for (int ii = i + 1; ii < dataObject.keyProperties.Count; ii++)
          {
            if (dataObject.keyProperties[ii].columnName.ToLower() == dataObject.keyProperties[i].columnName.ToLower())
            {
              if (dataObject.keyProperties[ii].keyType != KeyType.foreign)
              {
                if (((dataObject.keyProperties[ii].keyType == KeyType.identity || dataObject.keyProperties[ii].keyType == KeyType.sequence) && dataObject.keyProperties[i].keyType == KeyType.assigned) ||
                      dataObject.keyProperties[ii].keyType == KeyType.assigned && dataObject.keyProperties[i].keyType == KeyType.foreign)
                {
                  dataObject.keyProperties[i].keyType = dataObject.keyProperties[ii].keyType;
                }
              }

              dataObject.keyProperties.Remove(dataObject.keyProperties[ii--]);
            }
          }
        } */
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private bool ValidateDatabaseDictionary(DatabaseDictionary dbDictionary)
    {
      ISession session = null;

      try
      {
        // Validate connection string
        string connectionString = dbDictionary.connectionString;
        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        Dictionary<string, string> properties = new Dictionary<string, string>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("connection.connection_string", dbDictionary.connectionString);
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("dialect", "NHibernate.Dialect." + dbDictionary.provider + "Dialect");

        if (dbDictionary.provider.ToString().ToUpper().Contains("MSSQL"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
        }
        else if (dbDictionary.provider.ToString().ToUpper().Contains("ORACLE"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.OracleClientDriver");
        }
        else
        {
          throw new Exception("Database not supported.");
        }

        config.AddProperties(properties);
        ISessionFactory factory = config.BuildSessionFactory();

        session = factory.OpenSession();
      }
      catch (Exception ex)
      {
        throw new Exception("Invalid connection string: " + ex.Message);
      }
      finally
      {
        if (session != null) session.Close();
      }

      // Validate table key
      foreach (DataObject dataObject in dbDictionary.dataObjects)
      {
        if (dataObject.keyProperties == null || dataObject.keyProperties.Count == 0)
        {
          throw new Exception(string.Format("Table \"{0}\" has no key.", dataObject.tableName));
        }
      }

      return true;
    }

    private void UpdateBindingConfiguration(string projectName, string applicationName, Binding binding)
    {
      try
      {
        string bindingConfigurationPath = string.Format("{0}BindingConfiguration.{1}.{2}.xml", _settings["XmlPath"], projectName, applicationName);

        if (File.Exists(bindingConfigurationPath))
        {
          BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);
          bool bindingExists = false;

          // Update binding if exists
          for (int i = 0; i < bindingConfiguration.Bindings.Count; i++)
          {
            if (bindingConfiguration.Bindings[i].Name.ToUpper() == binding.Name.ToUpper())
            {
              bindingConfiguration.Bindings[i] = binding;
              bindingExists = true;
              break;
            }
          }

          // Add binding if not exist
          if (!bindingExists)
          {
            bindingConfiguration.Bindings.Add(binding);
          }

          Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
        }
        else
        {
          BindingConfiguration bindingConfiguration = new BindingConfiguration();
          bindingConfiguration.Bindings = new List<Binding>();
          bindingConfiguration.Bindings.Add(binding);
          Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateBindingConfiguration: {0}", ex));
        throw ex;
      }
    }

    private void UpdateScopes(string projectName, string applicationName)
    {
      try
      {
        string scopesPath = string.Format("{0}Scopes.xml", _settings["XmlPath"]);

        if (File.Exists(scopesPath))
        {
          List<ScopeProject> projects = Utility.Read<List<ScopeProject>>(scopesPath);
          bool projectExists = false;

          foreach (ScopeProject project in projects)
          {
            bool applicationExists = false;

            if (project.Name.ToUpper() == projectName.ToUpper())
            {
              foreach (ScopeApplication application in project.Applications)
              {
                if (application.Name.ToUpper() == applicationName.ToUpper())
                {
                  applicationExists = true;
                  break;
                }
              }

              if (!applicationExists)
              {
                project.Applications.Add(new ScopeApplication() { Name = applicationName });
              }

              projectExists = true;
              break;
            }
          }

          // project does not exist, add it
          if (!projectExists)
          {
            ScopeProject project = new ScopeProject()
            {
              Name = projectName,
              Applications = new List<ScopeApplication>()
               {
                 new ScopeApplication()
                 {
                   Name = applicationName
                 }
               }
            };

            projects.Add(project);
          }

          Utility.Write<List<ScopeProject>>(projects, scopesPath, true);
        }
        else
        {
          List<ScopeProject> projects = new List<ScopeProject>()
          {
            new ScopeProject()
            {
              Name = projectName,
              Applications = new List<ScopeApplication>()
               {
                 new ScopeApplication()
                 {
                   Name = applicationName
                 }
               }
            }
          };

          Utility.Write<List<ScopeProject>>(projects, scopesPath, true);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateScopes: {0}", ex));
        throw ex;
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
  }
}