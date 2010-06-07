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

namespace org.iringtools.adapter
{
  public class AdapterProvider
  {
    private static readonly string DATALAYER_NS = "org.iringtools.adapter.datalayer";

    private static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    private static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    private static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    private static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly XName OWL_THING = OWL_NS + "Thing";
    private static readonly XName RDF_ABOUT = RDF_NS + "about";
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string XSD_PREFIX = "xsd:";
    private static readonly string RDF_PREFIX = "rdf:";
    private static readonly string RDL_PREFIX = "rdl:";
    private static readonly string TPL_PREFIX = "tpl:";

    private static readonly string RDF_TYPE_ID = "tpl:R63638239485";
    private static readonly string CLASSIFICATION_INSTANCE_ID = "tpl:R55055340393";
    private static readonly string CLASS_INSTANCE_ID = "tpl:R99011248051";   
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    private static readonly string CLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      SELECT ?_instance
      WHERE {{{{ 
        ?_instance rdf:type {{0}} . 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName);

    private static readonly string LITERAL_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}> 
      SELECT ?_values 
      WHERE {{{{
	      ?_instance rdf:type {{0}} . 
	      ?_bnode {{1}} ?_instance . 
	      ?_bnode rdf:type {{2}} . 
	      ?_bnode {{3}} ?_values 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private AdapterSettings _settings = null;
    private IKernel _kernel = null;
    private ISemanticLayer _semanticEngine = null;
    private IDataLayer _dataLayer = null;
    private bool _isInitialized = false;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private Graph _graph = null;  // dotNetRdf graph
    private Dictionary<string, IList<IDataObject>> _dataObjectSet = null; // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private List<Dictionary<string, string>> _dtoList = null;  // dictionary of property xpath and value pairs
    private Dictionary<string, List<string>> _hierachicalDTOClasses = null;  // dictionary of class rdlUri and identifiers
    private MicrosoftSqlStoreManager _tripleStore = null;
    private TripleStore _memoryStore = null;    
    private XNamespace _graphNs = String.Empty;
    private string _dataObjectsAssemblyName = String.Empty;
    private string _dataObjectNs = String.Empty;

    [Inject]
    public AdapterProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(new AdapterModule());
      _settings = _kernel.Get<AdapterSettings>(new ConstructorArgument("AppSettings", settings));
    }

    #region public methods
    public List<ScopeProject> GetScopes()
    {
      string path = _settings.XmlPath + "Scopes.xml";

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
        _logger.Error("Error in GetScopes: " + ex);
        throw new Exception("Error getting the list of projects/applications from path [" + path + "]: " + ex);
      }
    }

    public Manifest GetManifest(string projectName, string applicationName)
    {
      string path = _settings.XmlPath + "Mapping." + projectName + "." + applicationName + ".xml";

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
        _logger.Error("Error in GetManifest: " + ex);
        throw new Exception("Error getting manifest from path [" + path + "]: " + ex);
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
        _logger.Error("Error in GetDictionary: " + ex);
        throw new Exception("Error getting data dictionary: " + ex);
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
        _logger.Error("Error in GetMapping: " + ex);
        throw new Exception("Error getting mapping: " + ex);
      }
    }

    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      Response response = new Response();
      string path = _settings.XmlPath + "Mapping." + projectName + "." + applicationName + ".xml";

      try
      {
        Utility.Write<Mapping>(mapping, path, true);
        response.Add("Mapping file updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error("Error in UpdateMapping: " + ex);
        response.Level = StatusLevel.Error;
        response.Add("Error saving mapping file to path [" + path + "]: " + ex);
      }

      return response;
    }

    //todo: move to dotNetEngine and fix sparqlEngine
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
        _logger.Error("Error in RefreshAll: " + ex);

        response.Level = StatusLevel.Error;
        response.Add("Error refreshing all graphs. " + ex);
      }

      return response;
    }

    //todo: move to dotNetEngine and fix sparqlEngine
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
        _logger.Error("Error in Refresh: " + ex);

        response.Level = StatusLevel.Error;
        response.Add("Error refreshing graph [" + graphName + "]." + ex);
      }

      return response;
    }

    public XElement GetRdf(string projectName, string applicationName, string graphName)
    {
      try
      {
        Initialize(projectName, applicationName);
        return GetRdf(graphName);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Refresh: " + ex);
        throw ex;
      }
    }
    
    public List<Dictionary<string, string>> GetDTOList(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();

        int maxDataObjectsCount = MaxDataObjectsCount();
        _dtoList.Clear();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          _dtoList.Add(new Dictionary<string, string>());
        }

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        FillDTOList(classMap.classId, "rdl:" + classMap.name);

        return _dtoList;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetHierarchicalDTOList(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();

        XElement graphElement = new XElement(_graphNs + graphName,
          new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
          new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
          new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          XElement classElement = new XElement(_graphNs + TitleCase(classMap.name));
          graphElement.Add(classElement);
          FillHierarchicalDTOList(classElement, classMap.classId, i);
        }

        return graphElement;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetHierarchicalDTOListWithReference(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();

        _hierachicalDTOClasses.Clear();

        XElement graphElement = new XElement(_graphNs + graphName,
          new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
          new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
          new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          XElement classElement = new XElement(_graphNs + TitleCase(classMap.name));
          graphElement.Add(classElement);
          FillHierarchicalDTOListWithReference(classElement, classMap.classId, i);
        }

        return graphElement;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetRdf(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();
        return GetRdf();
      }
      catch (Exception ex)
      {
        throw ex;
      }      
    }

    public XElement GetQtxf(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();

        XElement graphElement = new XElement(_graphNs + graphName, new XAttribute(XNamespace.Xmlns + "i", XSI_NS));
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          foreach (var pair in _graphMap.classTemplateListMaps)
          {
            ClassMap classMap = pair.Key;
            List<TemplateMap> templateMaps = pair.Value;
            string classInstance = _classIdentifiers[classMap.classId][i];

            XElement typeOfThingElement = CreateQtxfClassElement(classMap, classInstance);
            graphElement.Add(typeOfThingElement);

            foreach (TemplateMap templateMap in templateMaps)
            {
              XElement templateElement = CreateQtxfTemplateElement(templateMap, classInstance, i);
              graphElement.Add(templateElement);
            }
          }
        }

        return graphElement;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }    

    //todo: move to dotNetEngine and fix sparqlEngine
    public Response Pull(string projectName, string applicationName, Request request)
    {
      Response response = new Response();

      try
      {
        DateTime startTime = DateTime.Now;

        string targetUri = request["targetUri"];
        string targetCredentialsXML = request["targetCredentials"];
        string graphName = request["graphName"];

        WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
        if (targetCredentials.isEncrypted) targetCredentials.Decrypt();
        _settings.TargetCredentials = targetCredentials;        
        
        Initialize(projectName, applicationName);        
        FindGraphMap(graphName);

        _graphMap.baseUri = targetUri + "/" + graphName;
        _graph.BaseUri = new Uri(_graphMap.baseUri);

        // load graph from triple store
        _graph.Clear();
        _graph.BaseUri = new Uri(_graphMap.baseUri);
        _tripleStore.LoadGraph(_graph, _graphMap.baseUri);

        // create in-memory store for querying
        _memoryStore = new TripleStore();
        _memoryStore.Add(_graph);
        _graph.Dispose();
        
        // query in-memory store to fill dataObjectSet
        FillDataObjectSet();

        // post data objects to data layer
        foreach (var pair in _dataObjectSet)
          response.Append(_dataLayer.Post(pair.Value));

        #region report status
        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);
        
        response.Level = StatusLevel.Success;
        response.Add("Graph [" + graphName + "] has been posted to legacy system successfully.");

        response.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
        #endregion
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add("Error pulling graph. " + ex);
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
          response.Append(DeleteGraph(new Uri(graphMap.baseUri)));
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error clearing all graphs: " + ex);

        response.Level = StatusLevel.Error;
        response.Add("Error clearing all graphs. " + ex);
      }

      return response;
    }

    public Response DeleteGraph(string projectName, string applicationName, string graphName)
    {
      Initialize(projectName, applicationName);
      FindGraphMap(graphName);
      return DeleteGraph(new Uri(_graphMap.baseUri));
    }
    
    public Response UpdateDatabaseDictionary(DatabaseDictionary dbDictionary, string projectName, string applicationName)
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
          Binding semanticLayerBinding = new Binding()
          {
            Name = "SemanticLayer",
            Interface = "org.iringtools.adapter.ISemanticLayer, AdapterLibrary",
            Implementation = "org.iringtools.adapter.semantic.dotNETRdfEngine, AdapterLibrary"
          };
          UpdateBindingConfiguration(projectName, applicationName, semanticLayerBinding);

          Binding dataLayerBinding = new Binding()
          {
            Name = "DataLayer",
            Interface = "org.iringtools.library.IDataLayer, iRINGLibrary",
            Implementation = "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateDataLayer"
          };
          UpdateBindingConfiguration(projectName, applicationName, dataLayerBinding);

          UpdateScopes(projectName, applicationName);

          response.Add("Database dictionary updated successfully.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in UpdateDatabaseDictionary: " + ex);

        response.Level = StatusLevel.Error;
        response.Add("Error updating database dictionary: " + ex);
      }

      return response;
    }
    #endregion

    #region utility methods
    private string ExtractId(string qualifiedId)
    {
      if (String.IsNullOrEmpty(qualifiedId) || !qualifiedId.Contains(":"))
        return qualifiedId;

      return qualifiedId.Substring(qualifiedId.IndexOf(":") + 1);
    }

    private string TitleCase(string value)
    {
      string returnValue = String.Empty;
      string[] words = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string word in words)
      {
        returnValue += word.Substring(0, 1).ToUpper();

        if (word.Length > 1)
          returnValue += word.Substring(1).ToLower();
      }

      return returnValue;
    }

    public void SaveRdf(XElement rdf, string fileName)
    {
      XDocument doc = new XDocument(rdf);
      doc.Save(fileName);
    }
    #endregion

    #region private methods
    private void Initialize(string projectName, string applicationName)
    {
      try
      {
        if (!_isInitialized)
        {
          string scope = projectName + "." + applicationName;

          ApplicationSettings applicationSettings = _kernel.Get<ApplicationSettings>(
            new ConstructorArgument("projectName", projectName),
            new ConstructorArgument("applicationName", applicationName)
          );

          string bindingConfigurationPath = _settings.XmlPath + applicationSettings.BindingConfigurationPath;
          BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);

          _kernel.Load(new DynamicModule(bindingConfiguration));
          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");
          _semanticEngine = _kernel.Get<ISemanticLayer>("SemanticLayer");
          //todo: move this to dotNetEngine constructor
          _tripleStore = new MicrosoftSqlStoreManager(_settings.DBServer, _settings.DBname, _settings.DBUser, _settings.DBPassword);

          _dataObjectSet = new Dictionary<string, IList<IDataObject>>();
          _classIdentifiers = new Dictionary<string, List<string>>();
          _dtoList = new List<Dictionary<string, string>>();
          _hierachicalDTOClasses = new Dictionary<string, List<string>>();
          _graph = new Graph();

          _mapping = Utility.Read<Mapping>(_settings.XmlPath + "Mapping." + scope + ".xml");
          _graphNs = _settings.GraphBaseUri + projectName + "/" + applicationName + "#";
          _dataObjectNs = DATALAYER_NS + ".proj_" + scope;
          _dataObjectsAssemblyName = _settings.ExecutingAssemblyName;

          Directory.SetCurrentDirectory(_settings.BaseDirectoryPath);
          _isInitialized = true;
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error initializing application: " + exception);
        throw new Exception("Error initializing application: " + exception.ToString() + exception);
      }
    }

    private void FindGraphMap(string graphName)
    {
      foreach (GraphMap graphMap in _mapping.graphMaps)
      {
        if (graphMap.name.ToLower() == graphName.ToLower())
        {
          _graphMap = graphMap;

          if (_graphMap.classTemplateListMaps.Count == 0)
            throw new Exception("Graph [" + graphName + "] is empty.");

          return;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }

    private void LoadDataObjectSet()
    {
      _dataObjectSet.Clear();

      foreach (DataObjectMap dataObjectMap in _graphMap.dataObjectMaps)
      {
        _dataObjectSet.Add(dataObjectMap.name, _dataLayer.Get(dataObjectMap.name, null));
      }

      PopulateClassIdentifiers();
    }

    private string ResolveValueList(string valueList, string value)
    {
      if (_mapping != null && _mapping.valueMaps.Count > 0)
      {
        foreach (ValueMap valueMap in _mapping.valueMaps)
        {
          if (valueMap.valueList == valueList && valueMap.internalValue == value)
          {
            return valueMap.uri;
          }
        }
      }

      return RDF_NIL;
    }

    private string ResolveValueMap(string valueList, string uri)
    {
      if (_mapping != null && _mapping.valueMaps.Count > 0)
      {
        foreach (ValueMap valueMap in _mapping.valueMaps)
        {
          if (valueMap.valueList == valueList && valueMap.uri == uri)
          {
            return valueMap.internalValue;
          }
        }
      }

      return String.Empty;
    }

    private void PopulateClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
      {
        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          string[] property = identifier.Split('.');
          string objectName = property[0].Trim();
          string propertyName = property[1].Trim();

          IList<IDataObject> dataObjects = _dataObjectSet[objectName];
          if (dataObjects != null)
          {
            for (int i = 0; i < dataObjects.Count; i++)
            {
              string value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

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

        _classIdentifiers[classMap.classId] = classIdentifiers;
      }
    }

    // get max # of data records from all data objects
    private int MaxDataObjectsCount()
    {
      int maxCount = 0;

      foreach (var pair in _dataObjectSet)
      {
        if (pair.Value.Count > maxCount)
        {
          maxCount = pair.Value.Count;
        }
      }

      return maxCount;
    }

    private Response Refresh(string graphName)
    {
      Response response = new Response();

      try
      {
        DateTime startTime = DateTime.Now;

        // create RDF from graphName
        XElement rdf = GetRdf(graphName);

        #region load RDF to graph then save it to triple store
        Uri graphUri = new Uri(_graphMap.baseUri);
        XmlDocument xdoc = new XmlDocument();
        xdoc.LoadXml(rdf.ToString());
        rdf.RemoveAll();

        RdfXmlParser parser = new RdfXmlParser();
        _graph.Clear();
        _graph.BaseUri = graphUri;
        parser.Load(_graph, xdoc);
        xdoc.RemoveAll();

        DeleteGraph(graphUri);
        _tripleStore.SaveGraph(_graph);
        #endregion

        #region report status
        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);

        response.Level = StatusLevel.Success;
        response.Add("Graph [" + graphName + "] has been refreshed in triple store successfully.");

        response.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
        #endregion
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add("Error refreshing graph [" + graphName + "]. " + ex);
      }

      return response;
    }

    private XElement GetRdf()
    {
      XElement graphElement = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          string classInstance = _graphNs.NamespaceName + _classIdentifiers[classMap.classId][i];

          graphElement.Add(CreateRdfClassElement(classId, classInstance));

          foreach (TemplateMap templateMap in pair.Value)
          {
            graphElement.Add(CreateRdfTemplateElement(templateMap, classInstance, i));
          }
        }
      }

      return graphElement;
    }

    private XElement CreateRdfClassElement(string classId, string classInstance)
    {
      return new XElement(OWL_THING, new XAttribute(RDF_ABOUT, classInstance),
        new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));
    }

    private XElement CreateRdfTemplateElement(TemplateMap templateMap, string classInstance, int dataObjectIndex)
    {
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);

      XElement templateElement = new XElement(OWL_THING);
      templateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        switch (roleMap.type)
        {
          case RoleType.ClassRole:
          {
            roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
            break;
          }
          case RoleType.Reference:
          {
            if (roleMap.classMap != null)
            {
              string identifierValue = String.Empty;

              foreach (string identifier in roleMap.classMap.identifiers)
              {
                string[] property = identifier.Split('.');
                string objectName = property[0].Trim();
                string propertyName = property[1].Trim();

                IDataObject dataObject = _dataObjectSet[objectName].ElementAt(dataObjectIndex);

                if (dataObject != null)
                {
                  string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                  if (identifierValue != String.Empty)
                    identifierValue += roleMap.classMap.identifierDelimeter;

                  identifierValue += value;
                }
              }

              roleElement.Add(new XAttribute(RDF_RESOURCE, _graphNs.NamespaceName + identifierValue));
            }
            else
            {
              roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS.NamespaceName)));
            }
            break;
          }
          case RoleType.FixedValue:
          {
            dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
            roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
            roleElement.Add(new XText(roleMap.value));
            break;
          }
          case RoleType.Property:
          {
            string[] property = roleMap.propertyName.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();

            IDataObject dataObject = _dataObjectSet[objectName].ElementAt(dataObjectIndex);
            string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

            if (String.IsNullOrEmpty(roleMap.valueList))
            {
              if (String.IsNullOrEmpty(value))
              {
                roleElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
              }
              else
              {
                dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
                roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
                roleElement.Add(new XText(value));
              }
            }
            else // resolve value list to uri
            {
              string valueListUri = ResolveValueList(roleMap.valueList, value);
              roleElement.Add(new XAttribute(RDF_RESOURCE, valueListUri));
            }

            break;
          }
        }

        templateElement.Add(roleElement);
      }

      return templateElement;
    }

    private XElement CreateQtxfClassElement(ClassMap classMap, string classInstance)
    {
      XElement typeOfThingElement = new XElement(_graphNs + "TypeOfThing");
      typeOfThingElement.Add(new XAttribute("rdlUri", RDF_TYPE_ID));

      XElement hasClassElement = new XElement(_graphNs + "hasClass");
      hasClassElement.Add(new XAttribute("rdlUri", CLASSIFICATION_INSTANCE_ID));
      hasClassElement.Add(new XAttribute("reference", classMap.classId));
      typeOfThingElement.Add(hasClassElement);

      XElement hasIndividualElement = new XElement(_graphNs + "hasIndividual");
      hasIndividualElement.Add(new XAttribute("rdlUri", CLASS_INSTANCE_ID));
      hasIndividualElement.Add(new XAttribute("reference", classInstance));
      typeOfThingElement.Add(hasIndividualElement);

      return typeOfThingElement;
    }

    private XElement CreateQtxfTemplateElement(TemplateMap templateMap, string classInstance, int objectIndex)
    {
      XElement templateElement = new XElement(_graphNs + templateMap.name);
      templateElement.Add(new XAttribute("rdlUri", templateMap.templateId));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        XElement roleElement = new XElement(_graphNs + roleMap.name);
        roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
        templateElement.Add(roleElement);

        switch (roleMap.type)
        {
          case RoleType.ClassRole:
            roleElement.Add(new XAttribute("reference", classInstance));
            break;

          case RoleType.Reference:
            {
              if (roleMap.classMap != null)
              {
                string identifierValue = String.Empty;

                foreach (string identifier in roleMap.classMap.identifiers)
                {
                  string[] property = identifier.Split('.');
                  string objectName = property[0].Trim();
                  string propertyName = property[1].Trim();

                  IDataObject dataObject = _dataObjectSet[objectName].ElementAt(objectIndex);

                  if (dataObject != null)
                  {
                    string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                    if (identifierValue != String.Empty)
                      identifierValue += roleMap.classMap.identifierDelimeter;

                    identifierValue += value;
                  }
                }

                roleElement.Add(new XAttribute("reference", identifierValue));
              }
              else
              {
                roleElement.Add(new XAttribute("reference", roleMap.value));
              }
              break;
            }

          case RoleType.FixedValue:
            roleElement.Add(new XAttribute("reference", roleMap.value));
            break;

          case RoleType.Property:
            {
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();

              IDataObject dataObject = _dataObjectSet[objectName].ElementAt(objectIndex);
              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

              if (String.IsNullOrEmpty(roleMap.valueList))
              {
                if (String.IsNullOrEmpty(value))
                {
                  roleElement.Add(new XAttribute("reference", RDF_NIL));
                }
                else
                {
                  roleElement.Add(new XText(value));
                }
              }
              else // resolve value list to uri
              {
                string valueListUri = ResolveValueList(roleMap.valueList, value);
                roleElement.Add(new XAttribute("reference", Regex.Replace(valueListUri, ".*#", "rdl:")));
              }

              break;
            }
        }
      }

      return templateElement;
    }

    private void FillDTOList(string classId, string xPath)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      string classPath = xPath;

      foreach (TemplateMap templateMap in classTemplateListMap.Value)
      {
        xPath = classPath + "/tpl:" + templateMap.name;
        string templatePath = xPath;

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          if (roleMap.type == RoleType.Property)
          {
            xPath += "/tpl:" + roleMap.name;

            string[] property = roleMap.propertyName.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();
            string value = String.Empty;

            IList<IDataObject> dataObjects = _dataObjectSet[objectName];
            for (int i = 0; i < dataObjects.Count; i++)
            {              
              value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
              }

              Dictionary<string, string> propertyValuePair = _dtoList[i];
              propertyValuePair[xPath] = value;
            }

            xPath = templatePath;
          }

          if (roleMap.classMap != null)
          {
            FillDTOList(roleMap.classMap.classId, xPath + "/rdl:" + roleMap.classMap.name);
          }
        }
      }
    }

    private void FillHierarchicalDTOList(XElement classElement, string classId, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      ClassMap classMap = classTemplateListMap.Key;
      List<TemplateMap> templateMaps = classTemplateListMap.Value;

      classElement.Add(new XAttribute("rdlUri", classMap.classId));
      classElement.Add(new XAttribute("id", _classIdentifiers[classMap.classId][dataObjectIndex]));

      foreach (TemplateMap templateMap in templateMaps)
      {
        XElement templateElement = new XElement(_graphNs + templateMap.name);
        templateElement.Add(new XAttribute("rdlUri", templateMap.templateId));
        classElement.Add(templateElement);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          XElement roleElement = new XElement(_graphNs + roleMap.name);

          switch (roleMap.type)
          {
            case RoleType.ClassRole:
              templateElement.Add(new XAttribute("classRole", roleMap.roleId));
              break;

            case RoleType.Reference:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              templateElement.Add(roleElement);

              if (roleMap.classMap != null)
              {
                XElement element = new XElement(_graphNs + TitleCase(roleMap.classMap.name));
                roleElement.Add(element);
                FillHierarchicalDTOList(element, roleMap.classMap.classId, dataObjectIndex);
              }
              else
              {
                roleElement.Add(new XAttribute("reference", roleMap.value));
              }

              break;

            case RoleType.FixedValue:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              roleElement.Add(new XText(roleMap.value));
              templateElement.Add(roleElement);
              break;

            case RoleType.Property:
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();
              IDataObject dataObject = _dataObjectSet[objectName][dataObjectIndex];
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));

              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));
              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
                value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                roleElement.Add(new XAttribute("reference", value));
              }
              else
              {
                roleElement.Add(new XText(value));
              }

              templateElement.Add(roleElement);
              break;
          }
        }
      }
    }

    private void FillHierarchicalDTOListWithReference(XElement classElement, string classId, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      ClassMap classMap = classTemplateListMap.Key;
      List<TemplateMap> templateMaps = classTemplateListMap.Value;
      string classIdentifier = _classIdentifiers[classMap.classId][dataObjectIndex];

      classElement.Add(new XAttribute("rdlUri", classMap.classId));
      classElement.Add(new XAttribute("id", classIdentifier));

      if (_hierachicalDTOClasses.ContainsKey(classId))
      {
        List<string> classIdentifiers = _hierachicalDTOClasses[classId];
        classIdentifiers.Add(classIdentifier);
      }
      else
      {
        _hierachicalDTOClasses[classId] = new List<string> { classIdentifier };
      }

      foreach (TemplateMap templateMap in templateMaps)
      {
        XElement templateElement = new XElement(_graphNs + templateMap.name);
        templateElement.Add(new XAttribute("rdlUri", templateMap.templateId));
        classElement.Add(templateElement);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          XElement roleElement = new XElement(_graphNs + roleMap.name);

          switch (roleMap.type)
          {
            case RoleType.ClassRole:
              templateElement.Add(new XAttribute("classRole", roleMap.roleId));
              break;

            case RoleType.Reference:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              templateElement.Add(roleElement);                

              if (roleMap.classMap != null)
              {
                bool classExists = false;
                
                // check if the class instance has been created
                if (_hierachicalDTOClasses.ContainsKey(roleMap.classMap.classId))
                {
                  List<string> identifiers = _hierachicalDTOClasses[roleMap.classMap.classId];
                  string identifier = _classIdentifiers[roleMap.classMap.classId][dataObjectIndex];

                  if (identifiers.Contains(identifier))
                  {
                    roleElement.Add(new XAttribute("reference", identifier));
                    classExists = true;
                  }
                }

                if (!classExists)
                {
                  XElement element = new XElement(_graphNs + TitleCase(roleMap.classMap.name));
                  roleElement.Add(element);

                  FillHierarchicalDTOListWithReference(element, roleMap.classMap.classId, dataObjectIndex);
                }
              }
              else
              {
                roleElement.Add(new XAttribute("reference", roleMap.value));
              }

              break;

            case RoleType.FixedValue:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              roleElement.Add(new XText(roleMap.value));
              templateElement.Add(roleElement);
              break;

            case RoleType.Property:
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();
              IDataObject dataObject = _dataObjectSet[objectName][dataObjectIndex];

              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              templateElement.Add(roleElement);

              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));
              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
                value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                roleElement.Add(new XAttribute("reference", value));
              }
              else
              {
                roleElement.Add(new XText(value));
              }

              break;
          }
        }
      }
    }

    private Response DeleteGraph(Uri graphUri)
    {
      Response response = new Response();

      try
      {
        string graphId = _tripleStore.GetGraphID(graphUri);

        if (!String.IsNullOrEmpty(graphId))
        {
          _tripleStore.RemoveGraph(graphId);
        }

        response.Level = StatusLevel.Success;
        response.Add("Graph [" + graphUri + "] has been deleted successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error("Error delete graph [" + graphUri + "]: " + ex);

        response.Level = StatusLevel.Error;
        response.Add("Error deleting graph [" + graphUri + "]. " + ex);
      }

      return response;
    }

    private int GetClassInstanceCount()
    {
      ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
      string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classMap.classId);
      object results = _memoryStore.ExecuteQuery(query);

      if (results is SparqlResultSet)
      {                
        SparqlResultSet resultSet = (SparqlResultSet)results;
        return resultSet.Count;
      }

      throw new Exception("Error querying instances of class [" + classMap.name + "].");
    }

    private void FillDataObjectSet()
    {
      int classInstanceCount = GetClassInstanceCount();
      _dataObjectSet = new Dictionary<string, IList<IDataObject>>();
        
      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        List<TemplateMap> templateMaps = pair.Value;
        int dupTemplatePos = 0;

        foreach (TemplateMap templateMap in templateMaps)
        {
          List<RoleMap> propertyMapRoles = new List<RoleMap>();
          string classRoleId = String.Empty;

          #region find propertyMapRoles and classRoleId
          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            if (roleMap.type == RoleType.ClassRole)
            {
              classRoleId = roleMap.roleId;
            }
            else if (roleMap.type == RoleType.Property)
            {
              propertyMapRoles.Add(roleMap);
            }
          }
          #endregion

          #region query for property values and save them into dataObjects
          foreach (RoleMap roleMap in propertyMapRoles)
          {
            string query = String.Format(LITERAL_QUERY_TEMPLATE, classMap.classId, classRoleId, templateMap.templateId, roleMap.roleId);
            object results = _memoryStore.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();

              if (!_dataObjectSet.ContainsKey(objectName))
              {
                _dataObjectSet.Add(objectName, new List<IDataObject>());
              }

              IList<IDataObject> dataObjects = _dataObjectSet[objectName];
              if (dataObjects.Count == 0)
              {
                string objectType = _dataObjectNs + "." + objectName + "," + _dataObjectsAssemblyName;
                dataObjects = _dataLayer.Create(objectType, new string[classInstanceCount]);
              }

              SparqlResultSet resultSet = (SparqlResultSet)results;
              if (resultSet.Count > classInstanceCount)
              {
                dupTemplatePos++;
              }

              int objectIndex = 0;
              int resultSetIndex = (dupTemplatePos == 0) ? 0 : dupTemplatePos - 1;

              while (resultSetIndex < resultSet.Count)
              {
                string value = Regex.Replace(resultSet[resultSetIndex].ToString(), @".*= ", String.Empty);

                if (value == RDF_NIL)
                  value = String.Empty;
                else if (value.Contains("^^"))
                  value = value.Substring(0, value.IndexOf("^^"));
                else if (!String.IsNullOrEmpty(roleMap.valueList))
                  value = ResolveValueMap(roleMap.valueList, value);

                dataObjects[objectIndex++].SetPropertyValue(propertyName, value);

                if (dupTemplatePos == 0)
                  resultSetIndex++;
                else if (dupTemplatePos < 3)
                  resultSetIndex += 2;
                else
                  resultSetIndex += dupTemplatePos;
              }

              _dataObjectSet[objectName] = dataObjects;
            }
            else
            {
              throw new Exception("Error querying in-memory triple store.");
            }
          }
          #endregion
        }
      }
    }

    private void RemoveDups(DataObject dataObject)
    {
      try
      {
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
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void UpdateScopes(string projectName, string applicationName)
    {
      try
      {
        string scopesPath = _settings.XmlPath + "Scopes.xml";

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
                project.Applications.Add(new ScopeApplication() { Name = applicationName});
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
        _logger.Error("Error in UpdateScopes: " + ex);
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
          throw new Exception("Table \"" + dataObject.tableName + "\" has no key.");
        }
      }

      return true;
    }
    
    private void UpdateBindingConfiguration(string projectName, string applicationName, Binding binding)
    {
      try
      {
        string bindingConfigurationPath = _settings.XmlPath + "BindingConfiguration." + projectName + "." + applicationName + ".xml";

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
        _logger.Error("Error in UpdateBindingConfiguration: " + ex);
        throw ex;
      }
    }
    #endregion
  }
}
