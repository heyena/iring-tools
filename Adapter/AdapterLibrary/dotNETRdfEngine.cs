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
    class dotNETRdfEngine : ISemanticLayer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(dotNETRdfEngine));
        private IKernel _kernel;
        private string _scopeName = String.Empty;
        private string _scopedConnectionString = String.Empty;
        private string _company = string.Empty;
        private bool _trimData;
        private MicrosoftSqlStoreManager _store = null;
        private IDTOLayer _dtoService;
        private AdapterSettings _settings;
        private ApplicationSettings _applicationSettings;

        private Mapping _mapping = null;
        private Dictionary<string, Dictionary<string, string>> _refreshValueLists = null;

        private string _identifierClassName = string.Empty;
        private Dictionary<string, DataTransferObject> _dtoList = null;
        private Dictionary<string, Dictionary<string, string>> _pullValueLists = null;

        private int _instanceCounter = 0;

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

        const string _sqlDropDatabase =
          @"DROP DATABASE [@token]";

        const string _sqlCheckLogin =
          @"SELECT * FROM sys.syslogins WHERE name = N'@token'";

        const string _sqlDropLogin =
          @"DROP LOGIN [@token]";

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

        #region IProjectionEngine Members

        [Inject]
        public dotNETRdfEngine(AdapterSettings settings, ApplicationSettings applicationSettings, IDTOLayer dtoService, IKernel kernel)
        {
            _kernel = kernel;
            _settings = settings;
            _applicationSettings = applicationSettings;
            _dtoService = dtoService;
            _mapping = settings.Mapping;
            _trimData = settings.TrimData;
            _scopeName = _applicationSettings.ProjectName + "_" + _applicationSettings.ApplicationName;
            string dbServer, dbName, dbUser, dbPassword = string.Empty;
            dbServer = settings.DBServer;
            dbName = settings.DBname;
            dbUser = settings.DBUser;
            dbPassword = settings.DBPassword;
            _scopedConnectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", dbServer, dbName, dbUser, dbPassword);
        }

        public List<string> GetIdentifiers(string graphName)
        {          
            try
            {
                Graph g = new Graph();
                SparqlQueryParser parser = new SparqlQueryParser();
                List<string> tripleSet = new List<string>();
                SparqlResultSet sparqlResultSet = new SparqlResultSet();
                Uri interfaceServer = new Uri(_settings.InterfaceServer);
                
                string serverUri = interfaceServer.ToString()
                         .Substring(0,interfaceServer.ToString().IndexOf(interfaceServer.PathAndQuery));
                
                Uri graphUri = new Uri(serverUri + "/" + _applicationSettings.ProjectName + "/" + _applicationSettings.ApplicationName + "/" + graphName);
                _store.LoadGraph(g, graphUri);

                List<string> identifiers = new List<string>();
                GraphMap graphMap = new GraphMap();
                bool isIdentifierMapped = false;
                TemplateMap identifierTemplateMap = null;
                RoleMap identifierRoleMap = null;

                foreach (GraphMap mappingGraphMap in _mapping.graphMaps)
                {
                    if (mappingGraphMap.name == graphName)
                    {
                        graphMap = mappingGraphMap;
                    }
                }

                foreach (TemplateMap templateMap in graphMap.templateMaps)
                {
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        if (roleMap.propertyName == graphMap.identifier)
                        {
                            identifierTemplateMap = templateMap;
                            identifierRoleMap = roleMap;
                            isIdentifierMapped = true;
                            break;
                        }
                    }
                    if (isIdentifierMapped) break;
                }

                if (isIdentifierMapped)
                {
                    string identifier = String.Empty;
                    string identifierUri = String.Empty;
                    
                    UriNode classIdEntity = g.CreateUriNode(new Uri(graphMap.classId.Replace("rdl:", rdlPrefix)));
                    UriNode templateIdEntity = g.CreateUriNode(new Uri(identifierTemplateMap.templateId.Replace("tpl:", tplPrefix)));
                    UriNode templateMapClassRole = g.CreateUriNode(new Uri(identifierTemplateMap.classRole.Replace("tpl:", tplPrefix)));
                    Uri roleMapRoleId = new Uri(identifierRoleMap.roleId.Replace("tpl:", tplPrefix));

                    SparqlVariable relatedClassificationTemplate = new SparqlVariable("i1");
                    SparqlVariable classificationVariable = new SparqlVariable("c1");
                    SparqlVariable templateVariable = new SparqlVariable("t1");
                    
                    SparqlVariable propertyNameVariable = new SparqlVariable(identifierRoleMap.propertyName);
                    
                    string query = "SELECT DISTINCT " + propertyNameVariable + Environment.NewLine + " WHERE { ";
                    
                    tripleSet.Add(classificationVariable + " <" + rdfType + "> <" + owlThingEntity + "> .");
                    tripleSet.Add(classificationVariable + " <" + rdfType + "> <" + classificationTemplateType + "> .");
                    tripleSet.Add(classificationVariable + " <" + classType + "> <" + classIdEntity + "> .");
                    tripleSet.Add(classificationVariable + " <" + instanceType + "> " + relatedClassificationTemplate + " .");
                    tripleSet.Add(templateVariable + " <" + rdfType + "> <" + owlThingEntity + "> .");
                    tripleSet.Add(templateVariable + " <" + rdfType + "> <" + templateIdEntity + "> .");
                    tripleSet.Add(templateVariable + " <" + templateMapClassRole + "> " + relatedClassificationTemplate + " .");
                    tripleSet.Add(templateVariable + " <" + roleMapRoleId + "> " + propertyNameVariable + " .");

                    foreach (RoleMap roleMap in identifierTemplateMap.roleMaps)
                    {
                        if (roleMap != identifierRoleMap)
                        {
                            UriNode roleId = g.CreateUriNode(new Uri(roleMap.roleId.Replace("tpl:", tplPrefix)));

                            if (roleMap.reference != String.Empty && roleMap.reference != null)
                            {
                                UriNode referenceEntity = g.CreateUriNode(new Uri(roleMap.reference.Replace("rdl:", rdlPrefix)));
                                tripleSet.Add(templateVariable + " <" + roleId + "> <" + referenceEntity + "> .");

                            }
                        }
                    }
                    foreach (string t in tripleSet)
                        query += t + Environment.NewLine;
                    query += "}";

                    SparqlQuery sparqlQuery = parser.ParseFromString(query);

                    object result = g.ExecuteQuery(sparqlQuery);

                    if (result is SparqlResultSet)
                    {
                        sparqlResultSet = (SparqlResultSet)result;
                        foreach (SparqlResult sparqlResult in sparqlResultSet.Results)
                        {
                            identifiers.Add(((LiteralNode)sparqlResult.Value(sparqlResult.Variables.First())).Value);
                        }
                    }
                }
                else
                {
                    throw new Exception(String.Format("Identifier is not mapped for graph {0}", graphMap.name));
                }
                return identifiers;
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("GetIdentifiersFromTripleStore[{0}]", graphName), exception);
            }
          
        }

        private Dictionary<string, string> GetRefreshValueMap(string valueListName)
        {
            try
            {
                Dictionary<string, string> valueList = null;

                if (_refreshValueLists == null)
                {
                    _refreshValueLists = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                }

                if (_refreshValueLists.ContainsKey(valueListName))
                {
                    valueList = _refreshValueLists[valueListName];
                }
                else
                {
                    valueList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (ValueMap valueMap in _mapping.valueMaps)
                    {
                        if (valueMap.valueList == valueListName)
                        {
                            string key = valueMap.internalValue;
                            if (!valueList.ContainsKey(key))
                            {
                                valueList.Add(key, valueMap.modelURI); //First one is the default
                            }
                        }
                    }
                    _refreshValueLists.Add(valueListName, valueList);
                }

                return valueList;
            }
            catch (Exception exception)
            {
                throw new Exception("Error while getting or building ValueList " + valueListName + ".", exception);
            }
        }

        private string GetPropertyValue(RoleMap roleMap, string dtoPropertyValue)
        {
            string propertyValue = string.Empty;

            if (!String.IsNullOrEmpty(roleMap.propertyName))
            {
                if (!String.IsNullOrEmpty(roleMap.valueList))
                {
                    Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);

                    if (valueList.ContainsKey(dtoPropertyValue))
                    {
                        propertyValue = valueList[dtoPropertyValue].Replace("rdl:", rdlPrefix);
                    }
                    else
                    {
                        throw new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue));
                    }
                }
                else
                {
                    propertyValue = dtoPropertyValue;
                }
            }
            else if (!String.IsNullOrEmpty(roleMap.reference))
            {
                propertyValue = roleMap.reference.Replace("rdl:", rdlPrefix);
            }
            else if (!String.IsNullOrEmpty(roleMap.value))
            {
                propertyValue = roleMap.value;
            }

            return propertyValue;
        }

        public List<DataTransferObject> Get(string graphName)
        {
            try
            {
                _dtoList = new Dictionary<string, DataTransferObject>();
                NetworkCredential interfaceCred = _settings.InterfaceCredentials.GetNetworkCredential();
                NetworkCredential proxyCred = _settings.ProxyCredentials.GetNetworkCredential();
                Uri endpointAddress = new Uri(_settings.InterfaceServer);
                string remoteServer = endpointAddress.ToString().Substring(0, endpointAddress.ToString().IndexOf(endpointAddress.PathAndQuery));
                String graphToQuery = remoteServer + "/" + 
                    _applicationSettings.ProjectName + "/" + 
                    _applicationSettings.ApplicationName + "/" + 
                    graphName;
                
                //get the specific graph into memory from remote tripleStore
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery sparqlQuery = parser.ParseFromString("CONSTRUCT {?s ?p ?o .} FROM <" + graphToQuery + "> WHERE {?s ?p ?o .}");
                SparqlRemoteEndpoint sparqlEndpoint = new SparqlRemoteEndpoint(endpointAddress);
                sparqlEndpoint.Timeout = 50000;
                WebProxy proxy = new WebProxy(_settings.ProxyHost, Convert.ToInt16(_settings.ProxyPort));
                Graph sourceGraph = sparqlEndpoint.QueryWithResultGraph(sparqlQuery.ToString(), interfaceCred, proxy, proxyCred);

                foreach (GraphMap graphMap in _mapping.graphMaps)
                {
                    if (graphMap.name == graphName)
                    {
                      //  QueryGraphMap(graphMap, sourceGraph);
                    }
                }


                return _dtoList.Values.ToList<DataTransferObject>();
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("GetList[{0}]", graphName), exception);
            }
        }



        private SparqlResultSet GetTemplateValues(TemplateMap templateMap, string parentIdentifierVariable, Graph g)
        {
            try
            {
                String query = string.Empty;
                SparqlQueryParser parser = new SparqlQueryParser();
                List<string> tripleSet = new List<string>();
                UriNode templateIdEntity = g.CreateUriNode(templateMap.templateId.Replace("tpl:", tplPrefix));
                UriNode parentIdentifierVariableEntity = g.CreateUriNode(parentIdentifierVariable.Replace("eg:", egPrefix));
                SparqlVariable templateVariable = new SparqlVariable("t1");
                UriNode templateMapClassRole = g.CreateUriNode(templateMap.classRole.Replace("tpl:", tplPrefix));

                tripleSet.Add(templateVariable +" <"+ g.CreateUriNode(rdfType)+ "> <"+ g.CreateUriNode(owlThingEntity)+"> . ");
                tripleSet.Add(templateVariable +" <"+ g.CreateUriNode(rdfType)+ "> <"+ templateIdEntity+ "> . ");
                tripleSet.Add(templateVariable +" <"+ templateMapClassRole + "> <"+ parentIdentifierVariableEntity+"> . ");


                foreach (RoleMap roleMap in templateMap.roleMaps)
                {
                    if (roleMap.reference != null && roleMap.reference != String.Empty)
                    {
                        UriNode roleMapReference = g.CreateUriNode(roleMap.reference.Replace("rdl:", rdlPrefix));
                        UriNode roleMapRoleId = g.CreateUriNode(roleMap.roleId.Replace("tpl:", tplPrefix));
                        tripleSet.Add(templateVariable + " <"+  roleMapRoleId+ "> <"+ roleMapReference + "> . ");
                    }
                    else if (roleMap.value != null && roleMap.value != String.Empty)
                    {
                        LiteralNode propertyValue = GetPropertyValueType(roleMap.value, roleMap.dataType, g);
                        UriNode roleMapRoleId = g.CreateUriNode(roleMap.roleId.Replace("tpl:", tplPrefix));
                        tripleSet.Add(templateVariable + " <" + roleMapRoleId + "> <" + propertyValue + "> . ");
                    }
                    else
                    {
                        SparqlVariable propertyTemplate = new SparqlVariable(roleMap.propertyName);
                        UriNode roleMapRoleId = g.CreateUriNode(roleMap.roleId.Replace("tpl:", tplPrefix));
                        tripleSet.Add(templateVariable + " <"+ roleMapRoleId + "> <"+ propertyTemplate + "> . ");
                    }
                }

                query = "SELECT * WHERE { " + Environment.NewLine;
                foreach (string triple in tripleSet)
                    query += triple + Environment.NewLine;

                query += " }" + Environment.NewLine;
                SparqlQuery sparqlQuery = parser.ParseFromString(query);

                SparqlResultSet resultBuffer = GetUnterminatedTemplates(sparqlQuery, templateVariable, g);
                return resultBuffer;
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("GetTemplateValues[{0}][{1}]", templateMap.name, parentIdentifierVariable), exception);
            }
        }

        private void RefreshGraphMap(GraphMap graphMap, DataTransferObject dto, Graph g)
        {
            try
            {
                string identifier = dto.Identifier;

                identifier = "eg:id__" + identifier;
                RefreshGraphClassName(graphMap.classId, identifier, g);

                foreach (TemplateMap templateMap in graphMap.templateMaps)
                {
                    try
                    {
                        RefreshTemplateMap(templateMap, (ClassMap)graphMap, dto, identifier, g);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Warning with RefreshTemplateMap: " + ex);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("RefreshGraphMap[{0}][{1}]", graphMap.name, dto.Identifier), exception);
            }
        }

        private void RefreshTemplateMap(TemplateMap templateMap, ClassMap classMap, DataTransferObject dto, string identifier, Graph g)
        {
            throw new NotImplementedException();
        }


        private void RefreshGraphClassName(string classId, string identifier, Graph g)
        {
            try
            {
                if (!TestForClassInstance(classId, identifier, g))
                {
                    UriNode classIdEntity = g.CreateUriNode(new Uri(classId.Replace("rdl:", rdlPrefix)));
                    UriNode instanceValue = g.CreateUriNode(new Uri(identifier.Replace("eg:", egPrefix)));
                    LiteralNode startTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime", g);

                    BlankNode bNodeClassificationTemplate = g.CreateBlankNode("c1");
                    g.Assert(new Triple(bNodeClassificationTemplate, g.CreateUriNode(rdfType), g.CreateUriNode(owlThingEntity)));
                    g.Assert(new Triple(bNodeClassificationTemplate, g.CreateUriNode(rdfType), g.CreateUriNode(classificationTemplateType)));
                    g.Assert(new Triple(bNodeClassificationTemplate, g.CreateUriNode(classType), classIdEntity));
                    g.Assert(new Triple(bNodeClassificationTemplate, g.CreateUriNode(instanceType), instanceValue));
                    g.Assert(new Triple(bNodeClassificationTemplate, g.CreateUriNode(startDateTimeTemplate), startTimeValue));

                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("RefreshGraphClassName[{0}][{1}]", classId, identifier), exception);
            }
        }

        private LiteralNode GetPropertyValueType(Object value, string type, Graph g)
        {
            try
            {
                LiteralNode literal;
                if (String.IsNullOrEmpty(type))
                    type = "String";

                if (!type.Contains(':'))
                    type = "http://www.w3.org/2001/XMLSchema#" + type;

                if (type.Contains("xsd:"))
                    type = type.Replace("xsd:", "http://www.w3.org/2001/XMLSchema#");

                string str = string.Empty;
                if (value == null)
                    str = "";
                else
                    str = value.ToString();
                return literal = g.CreateLiteralNode(str, new Uri(type));
            }
            catch (Exception exception)
            {
                throw new Exception("Error in getting property value and type for " + value.ToString() + " and type " + type + ".", exception);
            }
        }

        private SparqlResultSet GetRelatedClass(TemplateMap templateMap, RoleMap roleMap, string className, string graphName, Graph g)
        {
            try
            {
                string query = "SELECT * WHERE { " + Environment.NewLine;
                List<string> statementSet = new List<string>();
                SparqlQueryParser parser = new SparqlQueryParser();

                SparqlResultSet resultSet = null;
                Uri graph = new Uri("http://iring.hatch.com.au/" + _applicationSettings.ProjectName + "/" + _applicationSettings.ApplicationName + "/" + graphName);
                _store.LoadGraph(g, graph);


                UriNode templateIdEntity = g.CreateUriNode(new Uri(templateMap.templateId.Replace("tpl:", tplPrefix)));
                UriNode classNameEntity = g.CreateUriNode(new Uri(className.Replace("eg:", egPrefix)));
                UriNode templateMapClassRole = g.CreateUriNode(new Uri(templateMap.classRole.Replace("tpl:", tplPrefix)));
                UriNode roleMapRoleId = g.CreateUriNode(new Uri(roleMap.roleId.Replace("tpl:", tplPrefix)));
                SparqlVariable relatedClassificationTemplate = new SparqlVariable("i1");
                SparqlVariable templateVariable = new SparqlVariable("t1");

                statementSet.Add(templateVariable + " <" + g.CreateUriNode(rdfType) + "> <" + g.CreateUriNode(owlThingEntity) + "> .");
                statementSet.Add(templateVariable + " <" + g.CreateUriNode(rdfType) + "> <" + templateIdEntity + "> .");
                statementSet.Add(templateVariable + " <" + templateMapClassRole + "> <" + classNameEntity + "> .");
                statementSet.Add(templateVariable + " <" + roleMapRoleId + "> <" + relatedClassificationTemplate + "> .");
                foreach (string statement in statementSet)
                    query += statement + Environment.NewLine;
                query += "}";

                SparqlQuery sparqlQuery = parser.ParseFromString(query);

                object result = g.ExecuteQuery(sparqlQuery);
                if (result is SparqlResultSet)
                    resultSet = (SparqlResultSet)result;

                return resultSet;
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("GetRelatedClass[{0}][{1}][{2}]", templateMap.name, roleMap.name, className), exception);
            }
        }

        private void RefreshDeleteClassMap(ClassMap classMap, RoleMap currentRoleMap, string parentIdentifierVariable, Graph g)
        {
            try
            {
                List<string> tripleSet = new List<string>();
                SparqlQueryParser parser = new SparqlQueryParser();

                SparqlVariable subjectVariable = new SparqlVariable("subject");
                SparqlVariable predicateVariable = new SparqlVariable("predicate");
                UriNode classIdEntity = g.CreateUriNode(classMap.classId.Replace("rdl:", rdlPrefix));
                UriNode instanceTypeEntity = g.CreateUriNode(instanceType);
                UriNode instanceValue = g.CreateUriNode(new Uri(parentIdentifierVariable.Replace("eg:", egPrefix)));

                tripleSet.Add(subjectVariable + " <"+ predicateVariable +"> <"+ instanceValue + "> . ");
                string query = "SELECT * WHERE { " + Environment.NewLine;
                query += tripleSet + Environment.NewLine;
                query += " }";
                SparqlQuery sparqlQuery = parser.ParseFromString(query);

                SparqlResultSet resultBuffer = GetUnterminatedTemplates(sparqlQuery, subjectVariable, g);

                var results = from SparqlResult b in resultBuffer.Results
                              where (((UriNode)(b["predicate"])).Uri == instanceType)
                              select b;

                int noOfInstancesRelated = resultBuffer.Results.Count - results.Count();

                if (noOfInstancesRelated == 1)
                {
                    tripleSet.Clear();

                    SparqlVariable classificationTemplateVariable = new SparqlVariable("c1");
                    
                    tripleSet.Add(classificationTemplateVariable +" <" + g.CreateUriNode(rdfType)+ "> <" + g.CreateUriNode(owlThingEntity)+"> . ");
                    tripleSet.Add(classificationTemplateVariable + " <" + g.CreateUriNode(rdfType) + "> <" + g.CreateUriNode(classificationTemplateType) + "> . ");
                    tripleSet.Add(classificationTemplateVariable + " <" + g.CreateUriNode(classType) + "> <" + classIdEntity + "> . ");
                    tripleSet.Add(classificationTemplateVariable + " <" + g.CreateUriNode(instanceType) + "> <" + instanceValue + "> . ");
                    
                    query = "SELECT * WHERE { " + Environment.NewLine;

                    foreach (string triple in tripleSet)
                        query += triple + Environment.NewLine;
                    query += " }";

                    sparqlQuery = parser.ParseFromString(query);

                    SparqlResultSet resultBufferUnterminatedClass = GetUnterminatedTemplates(sparqlQuery, classificationTemplateVariable, g);
                    
                    string variableBindingsToBeTerminated = resultBufferUnterminatedClass.Variables.First();
                    BlankNode bNodeToBeTerminated = g.CreateBlankNode(variableBindingsToBeTerminated);
                    LiteralNode endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime", g);
                    g.Assert(new Triple((INode)bNodeToBeTerminated, (INode)endDateTimeTemplate, endTimeValue));
                    _store.SaveGraph(g);
                    foreach (TemplateMap templateMap in classMap.templateMaps)
                    {
                        RefreshDeleteTemplateMap(templateMap, classMap, parentIdentifierVariable, g);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("RefreshDeleteClassMap[{0}][{1}][{2}]", classMap.name, currentRoleMap.name, parentIdentifierVariable), exception);
            }
        }

        private void RefreshDeleteTemplateMap(TemplateMap templateMap, ClassMap classMap, string parentIdentifierVariable, Graph g)
        {
            try
            {
                if (templateMap.type == TemplateType.Property)
                {
                    SparqlResultSet resultBuffer = GetTemplateValues(templateMap, parentIdentifierVariable, g);
                    BlankNode bNodeToBeTerminated = null;
                    foreach (string variableBinding in resultBuffer.Variables)
                    {
                        if (variableBinding == "c1")
                        bNodeToBeTerminated = g.CreateBlankNode(variableBinding);
                    }
                    LiteralNode endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime", g);
                    g.Assert(new Triple((INode)bNodeToBeTerminated, (INode)endDateTimeTemplate, endTimeValue));
                    _store.SaveGraph(g);
                }
                else
                {
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        if (roleMap.classMap != null)
                        {
                        //    string instanceVariable = GetRelatedClassInstance(templateMap, roleMap, parentIdentifierVariable)[0];
                        //    QueryResultBuffer resultBuffer = GetRelatedClass(templateMap, roleMap, parentIdentifierVariable);
                        //    BNode bNodeToBeTerminated = null;

                        //    foreach (VariableBindings variableBinding in resultBuffer.Bindings)
                        //    {
                        //        bNodeToBeTerminated = ((BNode)variableBinding["t1"]);
                        //    }

                        //    Literal endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
                        //    Statement statement1 = new Statement(bNodeToBeTerminated, endDateTimeTemplate, endTimeValue);

                        //    _store.Add(statement1);
                        //    RefreshDeleteClassMap(roleMap.classMap, roleMap, instanceVariable);

                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("RefreshDeleteGraphMap[{0}][{1}][{2}]", templateMap.name, classMap.name, parentIdentifierVariable), exception);
            }
        }


        private bool TestForClassInstance(string classId, string identifier, Graph g)
        {
            try
            {

                List<string> tripleSet = new List<string>();
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlResultSet sparqlResultSet = new SparqlResultSet();
                string query = "SELECT * WHERE { " + Environment.NewLine;

                UriNode classIdEntity = g.CreateUriNode(new Uri(classId.Replace("rdl:", rdlPrefix)));
                UriNode instanceValue = g.CreateUriNode(new Uri(identifier.Replace("eg:", egPrefix)));
                SparqlVariable classificationTemplate = new SparqlVariable("c1");

                tripleSet.Add(classificationTemplate + " <" + g.CreateUriNode(rdfType) + "> <" + g.CreateUriNode(owlThingEntity) + "> .");
                tripleSet.Add(classificationTemplate + " <" + g.CreateUriNode(rdfType) + "> <" + g.CreateUriNode(classificationTemplateType) + "> .");
                tripleSet.Add(classificationTemplate + " <" + g.CreateUriNode(classType) + "> <" + classIdEntity + "> .");
                tripleSet.Add(classificationTemplate + " <" + g.CreateUriNode(instanceType) + "> <" + instanceValue + "> .");

                foreach (string triple in tripleSet)
                    query += triple + Environment.NewLine;

                query += " }";
                SparqlQuery sparqlQuery = parser.ParseFromString(query);

                SparqlResultSet resultBuffer = GetUnterminatedTemplates(sparqlQuery, classificationTemplate, g);
                if (resultBuffer.Results.Count <= 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("TestForClassInstance[{0}][{1}]", classId, identifier), exception);
            }
        }

        private SparqlResultSet GetUnterminatedTemplates(SparqlQuery sparqlInQuery, SparqlVariable classificationTemplate, Graph g)
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                string query = sparqlInQuery.ToString();
                object queryResult = null;
                SparqlResultSet resultBuffer = new SparqlResultSet();
                queryResult = g.ExecuteQuery(query);
                if (queryResult is SparqlResultSet)
                    resultBuffer = (SparqlResultSet)queryResult;

                LiteralNode endDateTimeVariable = g.CreateLiteralNode(endDateTime);
                query.Insert(query.LastIndexOf("}"), classificationTemplate + " <" + g.CreateUriNode(endDateTimeTemplate) + "> <" + endDateTimeVariable + "> . ");
                SparqlQuery sparqlQuery = parser.ParseFromString(query);

                SparqlResultSet resultBufferWithEndDateTime = new SparqlResultSet();
                queryResult = g.ExecuteQuery(sparqlQuery);
                if (queryResult is SparqlResultSet)
                    resultBufferWithEndDateTime = (SparqlResultSet)queryResult;

                foreach (SparqlResult binding in resultBufferWithEndDateTime.Results)
                {
                    var results =
                      from SparqlResult b in resultBuffer.Results
                      where ((b.Variables) == (binding.Variables))
                      select b;

                    SparqlResult vb = results.FirstOrDefault();
                    if (vb != null)
                        resultBuffer.Results.Remove(vb);
                }
                return resultBuffer;
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("GetUnterminatedTemplates"), exception);
            }
        }

        public Response Delete(string graphName, List<string> identifiers)
        {
            Response response = new Response();
            //response.Add("No deletion todo for dotNETRdfEngine");
            return response;
        }

        public Response Clear(String graphName)
        {
            using (_store = new MicrosoftSqlStoreManager(_settings.DBServer, _settings.DBname, _settings.DBUser, _settings.DBPassword))
            {
                try
                {
                    DateTime b = DateTime.Now;
                    Response response = new Response();
                    Graph g = new Graph();
                  

                    foreach (Uri graph in _store.GetGraphUris())
                    {
                        if (graph.OriginalString.Contains(_applicationSettings.ProjectName) &&
                            graph.OriginalString.Contains(_applicationSettings.ApplicationName) &&
                            graph.OriginalString.Contains(graphName))
                        {
                            _store.LoadGraph(g, graph);
                            g.Triples.Dispose();
                            string graphID = _store.GetGraphID(graph);
                            _store.SaveGraph(g);
                        }
                    }
                    DateTime e = DateTime.Now;
                    TimeSpan d = e.Subtract(b);

                    response.Add(String.Format("Clear([{3}]) Execution Time [{0}:{1}.{2}] Minutes", d.Minutes, d.Seconds, d.Milliseconds, graphName));
                    return response;
                }
                catch (Exception exception)
                {
                    throw new Exception("DeleteAll: " + exception);
                }

            }
        }

        public void Initialize()
        {
            _store = new MicrosoftSqlStoreManager(_settings.DBServer, _settings.DBname, _settings.DBUser, _settings.DBPassword);
            _store.Open(true);
        }

        private bool VerifyConnectionString(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Response Post(string graphName, List<DataTransferObject> dtoList)
        {
            Response response = new Response();
            DateTime b = DateTime.Now;
            try
            {

                using (_store = new MicrosoftSqlStoreManager(_settings.DBServer, _settings.DBname, _settings.DBUser, _settings.DBPassword))
                {
                    if (dtoList.FirstOrDefault() != null)
                    {
                        
                        XElement dtoListXml = _dtoService.SerializeDTO(graphName, dtoList);

                        ITransformationLayer transformEngine = _kernel.Get<ITransformationLayer>("rdf");
                        XElement rdf = transformEngine.Transform(graphName, dtoListXml);

                        Stream ms = new MemoryStream();

                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(rdf.ToString());

                        string graphURI = "http://iring.hatch.com.au/" + _applicationSettings.ProjectName + "/" + _applicationSettings.ApplicationName + "/" + graphName ;
                        Graph workGraph = new Graph();
                        RdfXmlParser parser = new RdfXmlParser();
                        workGraph.BaseUri = new Uri(graphURI);
                        _store.LoadGraph(workGraph, graphURI);

                        string graphId = _store.GetGraphID(new Uri(graphURI));
                        workGraph.Triples.Dispose();
                        _store.SaveGraph(workGraph);
                        _store.RemoveGraph(graphId);

                        Graph g = new Graph();

                        parser.Load(g, xdoc);

                        g.BaseUri = new Uri(graphURI);
                        _store.LoadGraph(g, graphURI);
                        _store.SaveGraph(g);

                    }
                }

            }
            catch (Exception exception)
            {
                response.Level = StatusLevel.Error;
                response.Add("Error in Post[].");
                response.Add(exception.ToString());
            }
            DateTime e = DateTime.Now;
            TimeSpan d = e.Subtract(b);
            response.Add(String.Format("Post([{3}]) Execution Time [{0}:{1}.{2}] Minutes", d.Minutes, d.Seconds, d.Milliseconds, graphName));
            return response;
        }

        #endregion

        private RoleMap FindRoleMap(TemplateMap templateMap, string propertyName)
        {
            try
            {
                RoleMap roleMap = null;

                var queryResults = from map in templateMap.roleMaps
                                   where map.propertyName == propertyName
                                   select map;

                if (queryResults.Count<RoleMap>() > 0)
                {
                    roleMap = queryResults.First<RoleMap>();
                }

                return roleMap;
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("FindRoleMap[{0}][{1}]", templateMap.name, propertyName), exception);
            }
        }

    }
}
