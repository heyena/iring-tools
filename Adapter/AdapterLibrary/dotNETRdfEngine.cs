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

namespace org.iringtools.adapter.semantic
{
    class dotNETRdfEngine : SPARQLEngine, ISemanticLayer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(dotNETRdfEngine));
        private IKernel _kernel;
        private string _scopeName = String.Empty;
        private string _scopedConnectionString = String.Empty;
        private string _company = string.Empty;
        internal Graph _remoteGraph = null;
        private MicrosoftSqlStoreManager _msStore = null;
        private AdapterSettings _settings;
        private ApplicationSettings _applicationSettings;

        #region Constants
        public const string hash = "#";
        public string propertyType = "Property";
        public string relationshipType = "Relationship";
        public string endDateTime = "endDateTime";

        public const string rdfPrefix = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        public const string dmPrefix = "http://dm.rdlfacade.org/data#";
        public const string rdlPrefix = "http://rdl.rdlfacade.org/data#";
        public const string tplPrefix = "http://tpl.rdlfacade.org/data#";
        public const string egPrefix = "http://www.example.com/data#";
        public const string owlPrefix = "http://www.w3.org/2002/07/owl#";
        public const string p7tplPrefix = "http://tpl.rdlfacade.org/data#";

        const string _sqlCheckDatabase =
          @"SELECT name FROM sys.databases WHERE name = N'@token'";


        const string _sqlCreateDatabase =
          @"CREATE DATABASE [@token]";

        const string _sqlCreateLogin =
          @"USE [@token]
        CREATE LOGIN [@token] WITH PASSWORD = '@token', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
        CREATE USER [@token] FOR LOGIN [@token]
        EXEC sp_addrolemember db_owner, [@token]";

        public static Uri rdfType = new Uri(rdfPrefix + "type");
        public static Uri owlThingEntity = new Uri(owlPrefix + "Thing");
        public static Uri classificationTemplateType = new Uri(p7tplPrefix + "R63638239485");
        public static Uri classType = new Uri(p7tplPrefix + "R55055340393");
        public static Uri instanceType = new Uri(p7tplPrefix + "R99011248051");
        public static Uri startDateTimeTemplate = new Uri(p7tplPrefix + "valStartTime");
        public static Uri endDateTimeTemplate = new Uri(p7tplPrefix + "valEndTime");
        #endregion



        [Inject]
        public dotNETRdfEngine(AdapterSettings settings, ApplicationSettings applicationSettings, IDTOLayer dtoService, IKernel kernel)
            : base(settings, dtoService)
        {
            _kernel = kernel;
            _settings = settings;
            _applicationSettings = applicationSettings;
            _scopeName = _applicationSettings.ProjectName + "_" + _applicationSettings.ApplicationName;
            string dbServer, dbName, dbUser, dbPassword = string.Empty;
            dbServer = settings.DBServer;
            dbName = settings.DBname;
            dbUser = settings.DBUser;
            dbPassword = settings.DBPassword;
            _scopedConnectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", dbServer, dbName, dbUser, dbPassword);
        }

        private SparqlResultSet PostQuery(string sparql)
        {

            Uri endpointAddress = new Uri(this._targetUri);

            SparqlQueryParser parser = new SparqlQueryParser();

            SparqlQuery sparqlQuery = parser.ParseFromString(sparql);

            SparqlRemoteEndpoint sparqlEndpoint = new SparqlRemoteEndpoint(endpointAddress);

            sparqlEndpoint.SetCredentials(_settings.InterfaceCredentials.GetNetworkCredential());

            sparqlEndpoint.Timeout = 50000;

            return sparqlEndpoint.QueryWithResultSet(sparqlQuery.ToString());

        }

        public override List<string> GetIdentifiers(string graphName)
        {
            try
            {

                GraphMap graphMap = new GraphMap();
                List<string> identifiers = new List<string>();
                bool isIdentifierMapped = false;
                TemplateMap identifierTemplateMap = null;
                RoleMap identifierRoleMap = null;
                string classIdentifier = string.Empty;

                foreach (GraphMap mappingGraphMap in _mapping.graphMaps)
                {
                    if (mappingGraphMap.name == graphName)
                    {
                        graphMap = mappingGraphMap;
                    }
                }
                foreach (var keyValuePair in graphMap.classTemplateListMaps)
                {
                    foreach (TemplateMap templateMap in keyValuePair.Value)
                    {
                        foreach (RoleMap roleMap in templateMap.roleMaps)
                        {
                            if (keyValuePair.Key.identifiers.Contains(roleMap.propertyName))
                            {
                                classIdentifier = keyValuePair.Key.classId;
                                identifierTemplateMap = templateMap;
                                identifierRoleMap = roleMap;
                                isIdentifierMapped = true;
                                break;
                            }
                        }
                        if (isIdentifierMapped) break;
                    }
                }

                if (isIdentifierMapped)
                {
                    string identifier = String.Empty;
                    string identifierUri = String.Empty;
                    string identifierVariable = String.Empty;

                    SPARQLQuery identifierQuery = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);

                    identifierQuery.addVariable("?" + identifierRoleMap.propertyName);
                    //identifierQuery.addVariable("?i");

                    SPARQLClassification classification = identifierQuery.addClassification(classIdentifier, "?i");
                    //identifierQuery.addTemplate(identifierTemplateMap.templateId, identifierTemplateMap.classRole, "?i", identifierRoleMap.roleId, "?" + identifierRoleMap.propertyName);

                    SPARQLTemplate identifierTemplate = new SPARQLTemplate();
                    identifierTemplate.TemplateName = identifierTemplateMap.templateId;
                    identifierTemplate.ClassRole = identifierTemplateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString();
                    identifierTemplate.ClassId = "?i";

                    foreach (RoleMap roleMap in identifierTemplateMap.roleMaps)
                    {
                        if (roleMap.type == RoleType.Reference)
                        {
                            identifierTemplate.addRole(roleMap.roleId, roleMap.type);
                        }
                        else if (roleMap.value != null && roleMap.value != String.Empty)
                        {
                            string value = identifierQuery.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                            identifierTemplate.addRole(roleMap.roleId, value);
                        }
                        else
                        {
                            identifierVariable = roleMap.propertyName;
                            identifierQuery.addVariable("?" + identifierVariable);
                            identifierTemplate.addRole(roleMap.roleId, "?" + identifierVariable);
                        }
                    }
                    identifierTemplate.addRole("p7tpl:valEndTime", "?endDateTime");
                    identifierQuery.addTemplate(identifierTemplate);

                    Uri targetUri = new Uri(_settings.GraphBaseUri);
                    string remoteServer = targetUri.ToString().Substring(0, targetUri.ToString().IndexOf(targetUri.PathAndQuery));

                    identifierQuery.addSource(String.Format("{0}/{1}/{2}/{3}", _settings.GraphBaseUri, _applicationSettings.ProjectName, _applicationSettings.ApplicationName, graphName));

                    SparqlResultSet sparqlResults = this.PostQuery(identifierQuery.getSPARQL());

                    foreach (SparqlResult sparqlResult in sparqlResults.Results)
                    {
                        identifiers.Add(((LiteralNode)sparqlResult.Value(sparqlResult.Variables.First())).Value);
                    }

                    return identifiers;
                }
                else
                {
                    throw new Exception(String.Format("Identifier is not mapped for graph {0}", graphMap.name));
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("GetIdentifiersFromTripleStore[{0}]", graphName), exception);
            }
        }



        public override Response Clear(String graphName)
        {
            
            using (_msStore)
            {
                try
                {
                    DateTime b = DateTime.Now;
                    Response response = new Response();
                    Graph graph = new Graph();

                    string graphUri = string.Format("{0}/{1}/{2}/{3}", _settings.GraphBaseUri, _applicationSettings.ProjectName, _applicationSettings.ApplicationName, graphName);

                    _msStore.LoadGraph(graph, graphUri);
                    graph.Clear();

                    _msStore.SaveGraph(graph);

                    DateTime e = DateTime.Now;
                    TimeSpan d = e.Subtract(b);


                    return response;
                }
                catch (Exception exception)
                {
                    throw new Exception("DeleteAll: " + exception);
                }

            }
        }

        public override void Initialize()
        {
            _msStore = new MicrosoftSqlStoreManager(_settings.DBServer, _settings.DBname, _settings.DBUser, _settings.DBPassword);
        }

        public override Response Post(string graphName, List<DataTransferObject> dtoList)
        {
            Response response = new Response();
           // DateTime b = DateTime.Now;
            try
            {
                using (_msStore)
                {
                    _msStore.Open(false);

                    if (dtoList.FirstOrDefault() != null)
                    {
                        ITransformationLayer transformEngine = _kernel.Get<ITransformationLayer>("rdf");
                        string baseURI = string.Format("{0}/{1}/{2}/{3}", _settings.GraphBaseUri, _applicationSettings.ProjectName, _applicationSettings.ApplicationName, graphName);
                        Graph workGraph = new Graph();

                        workGraph.BaseUri = new Uri(baseURI);
                        _msStore.LoadGraph(workGraph, baseURI);
                        workGraph.Clear();
                        _msStore.SaveGraph(workGraph);
                        
                        XElement dtoListXml = _dtoService.SerializeDTO(graphName, dtoList);
                        XElement rdf = transformEngine.Transform(graphName, dtoListXml);

                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(rdf.ToString());

                        RdfXmlParser parser = new RdfXmlParser();
                        Graph graph = new Graph();

                        parser.Load(graph, xdoc);
                        graph.BaseUri = new Uri(baseURI);
                        _msStore.LoadGraph(graph, baseURI);
                        _msStore.SaveGraph(graph);

                        response.Add(string.Format("Graph[{0}] updated", baseURI));
                    }
                }

            }
            catch (Exception exception)
            {
                response.Level = StatusLevel.Error;
                response.Add("Error in Post[].");
                response.Add(exception.ToString());
            }
           // DateTime e = DateTime.Now;
            //TimeSpan d = e.Subtract(b);
           // response.Add(String.Format("Post([{3}]) Execution time [{0}:{1}.{2}] minutes.", d.Minutes, d.Seconds, d.Milliseconds, graphName));
            return response;
        }

        protected override void QueryTemplateMap(TemplateMap templateMap, ClassMap classMap, SPARQLQuery previousQuery)
        {
            try
            {

                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECT);
                query.Merge(previousQuery);

                query.Sources.Add(String.Format("{0}/{1}/{2}/{3}",
                        _settings.GraphBaseUri,
                         _applicationSettings.ProjectName,
                         _applicationSettings.ApplicationName,
                         this._graphName));

                string graphIdentifierVariable = query.Variables.First<string>();
                string graphIdentifierVariableName = graphIdentifierVariable.Replace("?", "");
                string parentIdentifierVariable = query.Variables.Last<string>();
                string identifierVariable = String.Empty;

                RoleMap classRoleMap =
                         (from roleMap in templateMap.roleMaps
                          where roleMap.classMap != null
                          select roleMap).FirstOrDefault();

                if (classRoleMap == null)
                {
                    SPARQLTemplate sparqlTemplate = new SPARQLTemplate();
                    sparqlTemplate.TemplateName = templateMap.templateId;
                    sparqlTemplate.ClassRole = templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString();
                    sparqlTemplate.ClassId = parentIdentifierVariable;

                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {

                        if (roleMap.type == RoleType.Reference)
                        {
                            sparqlTemplate.addRole(roleMap.roleId, roleMap.dataType);
                        }
                        else if (roleMap.value != null && roleMap.value != String.Empty)
                        {
                            string value = query.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                            sparqlTemplate.addRole(roleMap.roleId, value);
                        }
                        else
                        {
                            identifierVariable = roleMap.propertyName;
                            query.addVariable("?" + identifierVariable);
                            sparqlTemplate.addRole(roleMap.roleId, "?" + identifierVariable);
                        }
                    }

                    sparqlTemplate.addRole("p7tpl:valEndTime", "?endDateTime");
                    query.addTemplate(sparqlTemplate);

                    SparqlResultSet sparqlResults = this.PostQuery(query.getSPARQL());

                    foreach (SparqlResult sparqlResult in sparqlResults.Results)
                    {
                        INode identifierNode =
                         (from binding in sparqlResult
                          where binding.Key == graphIdentifierVariableName
                          select binding.Value).FirstOrDefault();

                        DataTransferObject dto = _dtoList[((LiteralNode)identifierNode).Value];

                        foreach (KeyValuePair<string, INode> binding in sparqlResult)
                        {
                            string propertyName = binding.Key;

                            if (propertyName != graphIdentifierVariableName)
                            {
                                object propertyValue = null;

                                RoleMap roleMap = FindRoleMap(templateMap, propertyName);

                                if (binding.Value is LiteralNode)
                                    if (((LiteralNode)binding.Value).Value != null)
                                    {
                                        propertyValue = ((LiteralNode)binding.Value).Value;
                                        dto.SetPropertyValueByInternalName(propertyName, propertyValue);
                                    }
                                    else if (roleMap != null && (roleMap.valueList != "" || roleMap.valueList != null))
                                    {
                                        Dictionary<string, string> valueList = GetPullValueMap(roleMap.valueList);
                                        if (binding.Value is UriNode)
                                        {
                                            string propertyUri = ((UriNode)binding.Value).Uri.ToString();
                                            propertyValue = valueList[propertyUri];
                                            dto.SetPropertyValueByInternalName(propertyName, propertyValue);
                                        }
                                    }

                            }
                        }
                    }
                }
                else
                {
                    _instanceCounter++;

                    SPARQLTemplate sparqlTemplate = new SPARQLTemplate();
                    sparqlTemplate.TemplateName = templateMap.templateId;
                    sparqlTemplate.ClassRole = templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString();
                    sparqlTemplate.ClassId = parentIdentifierVariable;

                    string instanceVariable = "?i" + _instanceCounter.ToString();

                    if (classRoleMap.type == RoleType.Reference)
                    {
                        sparqlTemplate.addRole(classRoleMap.roleId, classRoleMap.dataType);
                    }
                    else if (classRoleMap.value != null && classRoleMap.value != String.Empty)
                    {
                        string value = query.getLITERAL_SPARQL(classRoleMap.value, classRoleMap.dataType);
                        sparqlTemplate.addRole(classRoleMap.roleId, value);
                    }
                    else
                    {
                        sparqlTemplate.addRole(classRoleMap.roleId, instanceVariable);
                    }
                    sparqlTemplate.addRole("p7tpl:valEndTime", "?endDateTime");
                    query.addTemplate(sparqlTemplate);

                   // QueryClassMap(classRoleMap.classMap, classRoleMap, query, instanceVariable);

                    _instanceCounter--;
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("QueryTemplateMap[{0}]", templateMap.name), exception);
            }

        }
    }
}
