﻿using System;
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

    private static readonly ILog _logger = LogManager.GetLogger(typeof(dotNetRdfEngine));

    private AdapterSettings _settings = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private Graph _graph = null;  // dotNetRdf graph
    private MicrosoftSqlStoreManager _tripleStore = null;
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

    public Response Delete(string graphName)
    {
      Uri graphUri = new Uri(_graphNs.NamespaceName + graphName);
      return DeleteGraph(graphUri);
    }

    #region helper methods
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
        _logger.Error(string.Format("Error deleting graph [{0}]: {1}", graphUri, ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error deleting graph [{0}]: {1}", graphUri, ex));
      }

      return response;
    }
    #endregion
  }
}