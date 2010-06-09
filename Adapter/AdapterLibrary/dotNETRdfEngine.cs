using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using org.iringtools.adapter.semantic;
using org.iringtools.utility;
using org.iringtools.library;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Storage;
using Ninject;
using log4net;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Microsoft.ServiceModel.Web;
using System.Text.RegularExpressions;

namespace org.iringtools.adapter.semantic
{
  public class dotNetRdfEngine : ISemanticLayer
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
    private static readonly XName RDF_DESCRIPTION = RDF_NS + "Description";
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string RDF_PREFIX = "rdf:";    
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

    private static readonly ILog _logger = LogManager.GetLogger(typeof(dotNetRdfEngine));

    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private Graph _graph = null;  // dotNetRdf graph
    private MicrosoftSqlStoreManager _tripleStore = null;
    private TripleStore _memoryStore = null;    
    private XNamespace _graphNs = String.Empty;
    private string _dataObjectsAssemblyName = String.Empty;
    private string _dataObjectNs = String.Empty;

    [Inject]
    public dotNetRdfEngine(AdapterSettings adapterSettings, ApplicationSettings appSettings)
    {
      string scope = string.Format("{0}.{1}",appSettings.ProjectName, appSettings.ApplicationName);
      
      _tripleStore = new MicrosoftSqlStoreManager(adapterSettings.DBServer, adapterSettings.DBname, adapterSettings.DBUser, adapterSettings.DBPassword);
      _mapping = Utility.Read<Mapping>(String.Format("{0}Mapping.{1}.xml",adapterSettings.XmlPath, scope));
      _graph = new Graph();
      _graphNs = String.Format("{0}/{1}/{2}#", adapterSettings.GraphBaseUri, appSettings.ProjectName, appSettings.ApplicationName);
      _dataObjectNs = String.Format("{0}.proj_{1}",DATALAYER_NS, scope);
      _dataObjectsAssemblyName = adapterSettings.ExecutingAssemblyName;
    }

    public Response Refresh(string graphName, XElement rdf)
    {
      Response response = new Response();

      try
      {
        DateTime startTime = DateTime.Now;
        
        FindGraphMap(graphName);
        
        // create xdoc from xelement
        Uri graphUri = new Uri(_graphMap.baseUri);
        XmlDocument xdoc = new XmlDocument();
        xdoc.LoadXml(rdf.ToString());
        rdf.RemoveAll();

        // load xdoc to graph
        RdfXmlParser parser = new RdfXmlParser();
        _graph.Clear();
        _graph.BaseUri = graphUri;
        parser.Load(_graph, xdoc);
        xdoc.RemoveAll();

        // delete old graph and save new one
        DeleteGraph(graphUri);
        _tripleStore.SaveGraph(_graph);

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
        _logger.Error(string.Format("Error refreshing graph [{0}]. {1}", graphName, ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error refreshing graph [{0}]. {1}", graphName, ex));
      }

      return response;
    }

    public Dictionary<string, IList<IDataObject>> Get(string graphName)
    {
      FindGraphMap(graphName);

      // load graph from triple store
      _graph.Clear();
      _graph.BaseUri = new Uri(_graphMap.baseUri);
      _tripleStore.LoadGraph(_graph, _graphMap.baseUri);

      // create in-memory store for querying
      _memoryStore = new TripleStore();
      _memoryStore.Add(_graph);
      _graph.Dispose();

      return FillDataObjectSet(GetClassInstanceCount());
    }

    public Response Delete(string graphName)
    {
      Response response = new Response();

      try
      {
        FindGraphMap(graphName);

        Uri graphUri = new Uri(_graphMap.baseUri);
        string graphId = _tripleStore.GetGraphID(graphUri);

        if (!String.IsNullOrEmpty(graphId))
        {
          _tripleStore.ClearGraph(graphId);
          _tripleStore.RemoveGraph(graphId);
        }

        response.Level = StatusLevel.Success;
        response.Add(string.Format("Graph [{0}] has been deleted successfully.", graphUri));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error delete graph [{0}]: {1}", graphName, ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error delete graph [{0}]: {1}", graphName, ex));
      }

      return response;
    }

    #region helper methods
    private void FindGraphMap(string graphName)
    {
      foreach (GraphMap graphMap in _mapping.graphMaps)
      {
        if (graphMap.name.ToLower() == graphName.ToLower())
        {
          _graphMap = graphMap;

          if (_graphMap.classTemplateListMaps.Count == 0)
            throw new Exception(string.Format("Graph [{0}] is empty.", graphName));

          return;
        }
      }

      throw new Exception(string.Format("Graph [{0}] does not exist.", graphName));
    }

    private string ResolveValueMap(string valueList, string qualifiedUri)
    {
      string uri = qualifiedUri.Replace(RDL_NS.NamespaceName, "rdl:");

      foreach (ValueList valueLst in _mapping.valueLists)
      {
        if (valueLst.name == valueList)
        {
          foreach (ValueMap valueMap in valueLst.valueMaps)
          {
            if (valueMap.uri == uri)
            {
              return valueMap.internalValue;
            }
          }
        }
      }

      return String.Empty;
    }

    private Response DeleteGraph(Uri graphUri)
    {
      Response response = new Response();

      try
      {
        string graphId = _tripleStore.GetGraphID(graphUri);

        if (!String.IsNullOrEmpty(graphId))
        {
          _tripleStore.ClearGraph(graphId);
          _tripleStore.RemoveGraph(graphId);
        }

        response.Level = StatusLevel.Success;
        response.Add(string.Format("Graph [{0}] has been deleted successfully.", graphUri));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error delete graph [{0}]: {1}", graphUri, ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error delete graph [{0}]: {1}", graphUri, ex));

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

      throw new Exception(string.Format("Error querying instances of class [{0}].", classMap.name));
    }

    private Dictionary<string, IList<IDataObject>> FillDataObjectSet(int classInstanceCount)
    {
      Dictionary<string, IList<IDataObject>> dataObjectSet = new Dictionary<string, IList<IDataObject>>();

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

              if (!dataObjectSet.ContainsKey(objectName))
              {
                dataObjectSet.Add(objectName, new List<IDataObject>());
              }

              IList<IDataObject> dataObjects = dataObjectSet[objectName];
              if (dataObjects.Count == 0)
              {
                string objectType = _dataObjectNs + "." + objectName + "," + _dataObjectsAssemblyName;
                Type type = Type.GetType(objectType);
                  
                for (int i = 0; i < classInstanceCount; i++)
                {
                  IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);
                  dataObjects.Add(dataObject);
                }
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

              dataObjectSet[objectName] = dataObjects;
            }
            else
            {
              throw new Exception("Error querying in-memory triple store.");
            }
          }
          #endregion
        }
      }

      return dataObjectSet;
    }
    #endregion
  }
}
