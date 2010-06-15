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
using org.w3.sparql_results;

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
      SELECT ?instance
      WHERE {{{{ 
        ?instance rdf:type {{0}} . 
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

    private AdapterSettings _settings = null;
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
      string scope = string.Format("{0}.{1}", appSettings.ProjectName, appSettings.ApplicationName);

      _settings = adapterSettings;
      _tripleStore = new MicrosoftSqlStoreManager(adapterSettings.DBServer, adapterSettings.DBname, adapterSettings.DBUser, adapterSettings.DBPassword);
      _mapping = Utility.Read<Mapping>(String.Format("{0}Mapping.{1}.xml", adapterSettings.XmlPath, scope));
      _graph = new Graph();
      _graphNs = String.Format("{0}/{1}/{2}/", adapterSettings.GraphBaseUri, appSettings.ProjectName, appSettings.ApplicationName);
      _dataObjectNs = String.Format("{0}.proj_{1}", DATALAYER_NS, scope);
      _dataObjectsAssemblyName = adapterSettings.ExecutingAssemblyName;
    }

    public Response Refresh(string graphName, XElement rdf)
    {
      Response response = new Response();

      try
      {
        DateTime startTime = DateTime.Now;

        _graphMap = _mapping.FindGraphMap(graphName);
        
        // create xdoc from rdf xelement
        Uri graphUri = new Uri(_graphNs.NamespaceName + graphName);
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

    public Dictionary<string, SPARQLResults> Get(Request request)
    {
      // get rdf from an uri
      string targetUri = request["targetUri"];
      string targetCredentialsXML = request["targetCredentials"];
      string proxyHost = request["proxyHost"];
      int proxyPort = Int32.Parse(request["proxyPort"]);
      string proxyCredentialsXML = request["proxyCredentials"];

      WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
      if (targetCredentials.isEncrypted) targetCredentials.Decrypt();

      WebCredentials proxyCredentials = Utility.Deserialize<WebCredentials>(proxyCredentialsXML, true);
      if (proxyCredentials.isEncrypted) proxyCredentials.Decrypt();

      WebHttpClient client = new WebHttpClient(
        targetUri, targetCredentials.GetNetworkCredential(), proxyHost, proxyPort, proxyCredentials.GetNetworkCredential());
      XElement xElement = client.Get<XElement>(String.Empty);

      // load rdf to xdoc
      XmlDocument xDoc = new XmlDocument();      
      xDoc.LoadXml(xElement.ToString());
      xElement.RemoveAll();

      // create dotNetRdf graph from xdoc
      _graph.Clear();
      RdfXmlParser parser = new RdfXmlParser();
      parser.Load(_graph, xDoc);
      xDoc.RemoveAll();

      // load dotNetRdf graph to memory store for sparql-querying
      TripleStore _memoryStore = new TripleStore();
      _memoryStore.Add(_graph);
      _graph.Dispose();

      return FillResultSet(GetClassInstanceCount());
    }

    public Response Delete(string graphName)
    {
      Response response = new Response();

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);

        Uri graphUri = new Uri(_graphNs.NamespaceName + graphName);
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

    private Dictionary<string, SPARQLResults> FillResultSet(int classInstanceCount)
    {
      Dictionary<string, SPARQLResults> resultSet = new Dictionary<string, SPARQLResults>();

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        string classId = classMap.classId;
        List<TemplateMap> templateMaps = pair.Value;
        int dupeTemplatePosition = 0;

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
            string query = 
              String.Format(LITERAL_QUERY_TEMPLATE, 
                classId, 
                classRoleId, 
                templateMap.templateId, 
                roleMap.roleId);
            
            object results = _memoryStore.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
              if (!resultSet.ContainsKey(classId))
              {
                resultSet.Add(classId, new SPARQLResults());
              }

              SPARQLResults sparqlResults = resultSet[classId];

              SparqlResultSet sparqlResultsSet = (SparqlResultSet)results;
              if (resultSet.Count > classInstanceCount)
              {
                dupeTemplatePosition++;
              }

              int sparqlResultsSetIndex = (dupeTemplatePosition == 0) ? 0 : dupeTemplatePosition - 1;

              List<SPARQLResult> resultList = new List<SPARQLResult>();

              while (sparqlResultsSetIndex < sparqlResultsSet.Count)
              {
                string variable = sparqlResultsSet.Variables.ElementAt(sparqlResultsSetIndex);
                
                sparqlResults.head.variables.Add(new Variable { name = variable });

                INode node = sparqlResultsSet[sparqlResultsSetIndex].Value(variable);

                SPARQLBinding sparqlBinding = new SPARQLBinding
                {
                  name = variable,
                };

                NodeType nodeType = node.NodeType;

                switch (nodeType)
                {
                  case NodeType.Blank:
                    BlankNode blankNode = (BlankNode)node;

                    sparqlBinding.bnode = blankNode.InternalID;
                    break;

                  case NodeType.GraphLiteral:
                    throw new NotImplementedException("Graph Literals are not supported.");

                  case NodeType.Literal:
                    LiteralNode literalNode = (LiteralNode)node;
                    
                    sparqlBinding.literal = new SPARQLLiteral
                    {
                      dataType = literalNode.DataType.ToString(),
                      lang = literalNode.Language,
                      value = literalNode.Value,
                    };
                    break;

                  case NodeType.Uri:
                    UriNode uriNode = (UriNode)node;

                    sparqlBinding.uri = uriNode.Uri.ToString();
                    break;
                };

                sparqlResults.resultsElement.results.FirstOrDefault().bindings.Add(sparqlBinding);

                if (dupeTemplatePosition == 0)
                  sparqlResultsSetIndex++;
                else if (dupeTemplatePosition < 3)
                  sparqlResultsSetIndex += 2;
                else
                  sparqlResultsSetIndex += dupeTemplatePosition;
              }

              resultSet[classId] = sparqlResults;
            }
            else
            {
              throw new Exception("Error querying in-memory triple store.");
            }
          }
          #endregion
        }
      }

      return resultSet;
    }
    #endregion
  }
}
