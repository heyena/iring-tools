//// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
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
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using log4net;
using Ninject;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using org.w3.sparql_results;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;
using VDS.RDF.Writing.Formatting;
using System.Net;
using org.iringtools.refdata.federation;


namespace org.iringtools.refdata
{
    public class ReferenceDataProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ReferenceDataProvider));

        private Response _response = null;

        private const string FEDERATION_FILE_NAME = "Federation.xml";
        private const string QUERIES_FILE_NAME = "Queries.xml";

        private NamespaceMapper nsMap = new NamespaceMapper();

        NTriplesFormatter formatter = new NTriplesFormatter();
        INode _subj, _pred, _obj;

        //private bool qn = false;
        //private string qName = string.Empty;
        private const string insertData = "INSERT DATA {";
        private const string deleteData = "DELETE DATA {";
        private const string deleteWhere = "DELETE WHERE {";
        private const string rdfssubClassOf = "rdfs:subClassOf";
        private const string rdfType = "rdf:type";
        private int _pageSize = 0;

        private bool _useExampleRegistryBase = false;

        private List<Repository> _repositories = null;
        private Federation _federation = null;
        private List<Namespace> _namespaces = null;

        private Queries _queries = null;
        private static Dictionary<string, RefDataEntities> _searchHistory = new Dictionary<string, RefDataEntities>();
        private IKernel _kernel = null;
        private ReferenceDataSettings _settings = null;

        private StringBuilder prefix = new StringBuilder();
        private StringBuilder sparqlBuilder = new StringBuilder();
        private StringBuilder sparqlStr = new StringBuilder();
        private string defaultLanguage = "en";

        public ReferenceDataProvider(NameValueCollection settings)
        {
            try
            {
                _kernel = new StandardKernel(new ReferenceDataModule());
                _settings = this._kernel.Get<ReferenceDataSettings>();
                _settings.AppendSettings(settings);
                Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

                string path = _settings["AppDataPath"];
                if (!path.EndsWith("\\")) path += "\\";

                if (!path.StartsWith(@"\\") && !path.Contains(@":\"))
                {
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                }

                _settings["AppDataPath"] = path;

                _pageSize = Convert.ToInt32(_settings["PageSize"]);
                _useExampleRegistryBase = Convert.ToBoolean(_settings["UseExampleRegistryBase"]);
                var queriesPath = _settings["AppDataPath"] + QUERIES_FILE_NAME;
                _queries = Utility.Read<Queries>(queriesPath);
                var federationPath = _settings["AppDataPath"] + FEDERATION_FILE_NAME;
                if (File.Exists(federationPath))
                {
                    _federation = Utility.Read<Federation>(federationPath);
                    _repositories = _federation.Repositories;
                    _namespaces = _federation.Namespaces;
                    foreach (var ns in _namespaces)
                    {
                        nsMap.AddNamespace(ns.Prefix, new Uri(ns.Uri));
                    }
                }
                _response = new Response();
                _kernel.Bind<Response>().ToConstant(_response);

            }
            catch (Exception ex)
            {
                _logger.Error("Error in initializing ReferenceDataServiceProvider: " + ex);
            }
        }

        public Federation GetFederation()
        {
            return _federation;
        }

        public List<Repository> GetRepositories()
        {
            try
            {
                List<Repository> repositories;
                repositories = _repositories;
                //Don't Expose Tokens
                foreach (var repository in repositories)
                {
                    repository.EncryptedCredentials = null;
                }
                return repositories;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetRepositories: " + ex);
                return null;
            }
        }

        private Repository GetRepository(string name)
        {
            return _repositories.Find(c => c.Name == name);
        }

        public RefDataEntities Search(string query, string repositoryNamesString)
        {
            try
            {
                return SearchPage(query, 0, 0, repositoryNamesString);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Search: " + ex);
                throw new Exception("Error while Searching " + query + ".\n" + ex.ToString(), ex);
            }
        }

        public RefDataEntities SearchPage(string query, int start, int limit, string repositoryNamesString)
        {
            _logger.Debug("SearchPage");

            RefDataEntities entities = null;
            var counter = 0;
            Entity resultEntity = null;
            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;

                // ********************************* FOR TESTING ONLY
                _searchHistory.Clear();
                // ********************************* REMOVE THIS

                if (_searchHistory.ContainsKey(query))
                {
                    _logger.Debug("SearchPage: Using History");

                    entities = _searchHistory[query];
                }
                else
                {
                    var resultEntities = new RefDataEntities();

                    _logger.Debug("SearchPage: Preparing Queries");

                    var queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "ContainsSearch").Query;

                    _logger.Debug("SearchPage: Got Contains Search");

                    var queryBindings = queryContainsSearch.Bindings;

                    _logger.Debug("SearchPage: Got Bindings");

                    foreach (var q in _queries)
                    {
                        _logger.DebugFormat("SearchPage: Looging for ContainsSearchJORD: {0}", q.Key);
                    }

                    var queryItem = _queries.FirstOrDefault(c => c.Key == "ContainsSearchJORD");

                    if (queryItem != null)
                    {
                        _logger.Debug("SearchPage: Got QueryItem");
                    }

                    Query queryContainsSearchJORD = queryItem.Query;

                    _logger.Debug("SearchPage: Got Contains Search JORD");

                    var queryBindingsJORD = queryContainsSearchJORD.Bindings;

                    _logger.Debug("SearchPage: Got JORD Bindings");

                    List<Repository> repositoryList = null;
                    if (repositoryNamesString != null)
                    {
                        string[] repositoryNames = repositoryNamesString.Split(',');
                        repositoryList = new List<Repository>();
                        foreach (var repository in _repositories)
                        {
                            foreach (var repositoryName in repositoryNames)
                            {
                                if (repository.Name.Equals(repositoryName))
                                    repositoryList.Add(repository);
                            }
                        }
                    }
                    else
                    {
                        repositoryList = _repositories;
                    }

                    if (repositoryList == null || repositoryList.Count() == 0)
                    {
                        _logger.Error("Did not find any repositories to search.");
                        throw new Exception("No repositories found for search.");
                    }

                    foreach (var repository in repositoryList)
                    {
                        if (repository.RepositoryType == RepositoryType.JORD)
                        {
                            _logger.Debug("SearchPage: JORD!");

                            sparql = ReadSparql(queryContainsSearchJORD.FileName);
                            sparql = sparql.Replace("param1", query);
                        }
                        else
                        {
                            _logger.Debug("SearchPage: Other!");

                            sparql = ReadSparql(queryContainsSearch.FileName);
                            sparql = sparql.Replace("param1", query);
                        }

                        _logger.Debug("SearchPage: Query Repo");

                        var sparqlResults = QueryFromRepository(repository, sparql);

                        foreach (var result in sparqlResults)
                        {
                            resultEntity = new Entity();
                            foreach (var v in result.Variables)
                            {
                                var node = result[v];
                                if (node is LiteralNode && v.Equals("label"))
                                {
                                    resultEntity.Label = ((LiteralNode)node).Value;
                                    resultEntity.Lang = ((LiteralNode)node).Language;
                                    if (string.IsNullOrEmpty(resultEntity.Lang))
                                        resultEntity.Lang = defaultLanguage;
                                }
                                else if (node is UriNode && v.Equals("uri"))
                                {
                                    resultEntity.Uri = ((UriNode)node).Uri.ToString();
                                }
                                else if (node is UriNode && v.Equals("rds"))
                                {
                                    resultEntity.RDSUri = ((UriNode)node).Uri.ToString();
                                }
                            }
                            resultEntity.Repository = repository.Name;
                            var key = resultEntity.Label;
                            if (resultEntity.Label.StartsWith("has") || resultEntity.Label.StartsWith("val"))
                            {
                                resultEntity = null;
                                continue;
                            }
                            if (resultEntities.Entities.ContainsKey(key))
                            {
                                key += ++counter;
                            }
                            resultEntities.Entities.Add(key, resultEntity);
                        }
                        //results.Clear();
                    }
                    _searchHistory.Add(query, resultEntities);
                    entities = resultEntities;
                    entities.Total = resultEntities.Entities.Count;
                }

                if (limit > 0)
                {
                    entities = GetRequestedPage(entities, start, limit);
                }

                _logger.Info(string.Format("SearchPage is returning {0} records", entities.Entities.Count));
                return entities;
            }
            catch (Exception e)
            {
                _logger.Error("Error in SearchPage: " + e);
                throw new Exception("Error while Finding " + query + ".\n" + e.ToString(), e);
            }
        }

        public RefDataEntities SearchReset(string query, string repositoryNamesString)
        {
            Reset(query);

            return Search(query, repositoryNamesString);
        }

        public RefDataEntities SearchPageReset(string query, int start, int limit, string repositoryNamesString)
        {
            Reset(query);

            return SearchPage(query, start, limit, repositoryNamesString);
        }

        private Entity GetLabel(string uri)
        {
            var labelEntity = new Entity();
            try
            {
                var label = String.Empty;
                var sparql = String.Empty;
                var relativeUri = String.Empty;

                var query = (Query)_queries.FirstOrDefault(c => c.Key == "GetLabel").Query;
                var queryEquivalent = (Query)_queries.FirstOrDefault(c => c.Key == "GetLabelRdlEquivalent").Query;

                foreach (var repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.JORD && uri.Contains("#"))
                    {
                        sparql = ReadSparql(queryEquivalent.FileName).Replace("param1", uri);
                    }
                    else
                    {
                        sparql = ReadSparql(query.FileName).Replace("param1", uri);
                    }
                    var sparqlResults = QueryFromRepository(repository, sparql);

                    foreach (var result in sparqlResults)
                    {
                        foreach (var v in result.Variables.Where(v => (INode)result[v] is LiteralNode && v.Equals("label")))
                        {
                            labelEntity.Label = ((LiteralNode)result[v]).Value;
                            labelEntity.Lang = ((LiteralNode)result[v]).Language;
                            if (string.IsNullOrEmpty(labelEntity.Lang))
                                labelEntity.Lang = defaultLanguage;
                        }
                        labelEntity.Repository = repository.Name;
                        labelEntity.Uri = repository.Uri;
                        break;
                    }
                }

                return labelEntity;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetLabel: " + e);
                throw new Exception("Error while Getting Label for " + uri + ".\n" + e.ToString(), e);
            }
        }

        private List<Classification> GetClassifications(string id, Repository rep)
        {
            var qmxf = new QMXF();

            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;

                var classifications = new List<Classification>();
                Query getClassification;

                foreach (var repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    switch (rep.RepositoryType)
                    {
                        case RepositoryType.Camelot:
                        case RepositoryType.RDSWIP:
                            getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassification").Query;
                            sparql = ReadSparql(getClassification.FileName).Replace("param1", id);
                            classifications = ProcessClassifications(rep, sparql);
                            break;
                        case RepositoryType.JORD:
                            getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassificationJORD").Query;
                            sparql = ReadSparql(getClassification.FileName).Replace("param1", id);
                            classifications = ProcessClassifications(rep, sparql);
                            break;
                        case RepositoryType.Part8:
                            getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Classification").Query;
                            sparql = ReadSparql(getClassification.FileName).Replace("param1", id);
                            classifications = ProcessClassifications(rep, sparql);
                            break;
                    }
                }

                return classifications;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetClassifications: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        private List<Classification> ProcessClassifications(Repository repository, string sparql)
        {

            var sparqlResults = QueryFromRepository(repository, sparql);

            var classifications = new List<Classification>();
            var names = new List<string>();
            var resultValue = string.Empty;

            foreach (SparqlResult result in sparqlResults)
            {
                var classification = new Classification();
                classification.repository = repository.Name;
                var uri = String.Empty;

                if (result.HasValue("uri") && result["uri"] != null)
                {
                    var pref = nsMap.GetPrefix(new Uri(result["uri"].ToString().Substring(0, result["uri"].ToString().IndexOf("#") + 1)));

                    if (pref.Equals("owl") || pref.Contains("dm")) continue;

                    uri = result["uri"].ToString();
                    classification.reference = uri;
                }
                else if (result.HasValue("rdsuri") && result["rdsuri"] != null)
                {
                    classification.reference = result["rdsuri"].ToString();
                }

                foreach (var v in result.Variables)
                {
                    var node = result[v];
                    if (!(node is LiteralNode) || !v.Equals("label")) continue;
                    classification.label = ((LiteralNode)node).Value;
                    classification.lang = ((LiteralNode)node).Language;
                }
                if (string.IsNullOrEmpty(classification.label))
                {
                    var entity = GetLabel(uri);
                    classification.label = entity.Label;
                    classification.lang = entity.Lang;
                }
                if (string.IsNullOrEmpty(classification.lang))
                    classification.lang = defaultLanguage;

                Utility.SearchAndInsert(classifications, classification, Classification.sortAscending());
            }

            return classifications;
        }

        private List<Specialization> GetSpecializations(string id, Repository rep)
        {
            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;
                SparqlResultSet sparqlResults = null;

                var specializations = new List<Specialization>();

                var queryRdsWip = (Query)_queries.FirstOrDefault(c => c.Key == "GetSuperclass").Query;
                var queryJord = (Query)_queries.FirstOrDefault(c => c.Key == "GetSuperclassJORD").Query;
                var queryPart8 = (Query)_queries.FirstOrDefault(c => c.Key == "GetSuperClassOf").Query;

                foreach (Repository repository in _repositories)
                {
                    switch (repository.RepositoryType)
                    {
                        case RepositoryType.Camelot:
                        case RepositoryType.RDSWIP:

                            sparql = ReadSparql(queryRdsWip.FileName).Replace("param1", id);
                            sparqlResults = QueryFromRepository(repository, sparql);
                            foreach (var result in sparqlResults)
                            {
                                var specialization = new Specialization();
                                specialization.repository = repository.Name;
                                var uri = string.Empty;

                                foreach (var v in result.Variables)
                                {
                                    var node = result[v];

                                    if (node is LiteralNode && v.Equals("label"))
                                    {
                                        specialization.label = ((LiteralNode)node).Value;
                                        specialization.lang = ((LiteralNode)node).Language;
                                    }
                                    else if (node is UriNode && v.Equals("uri"))
                                    {
                                        specialization.reference = ((UriNode)node).Uri.ToString();
                                        uri = specialization.reference;
                                    }
                                }
                                if (string.IsNullOrEmpty(specialization.label))
                                {
                                    var entity = GetLabel(uri);
                                    specialization.label = entity.Label;
                                    specialization.lang = entity.Lang;
                                }
                                if (string.IsNullOrEmpty(specialization.lang))
                                    specialization.lang = defaultLanguage;

                                Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
                            }
                            break;

                        case RepositoryType.Part8:
                            sparql = ReadSparql(queryPart8.FileName).Replace("param1", id);
                            sparqlResults = QueryFromRepository(repository, sparql);
                            foreach (var result in sparqlResults)
                            {
                                var specialization = new Specialization();
                                specialization.repository = repository.Name;
                                var uri = string.Empty;

                                foreach (var v in result.Variables)
                                {
                                    var node = result[v];
                                    if (node is LiteralNode && v.Equals("label"))
                                    {
                                        specialization.label = ((LiteralNode)node).Value;
                                        specialization.lang = ((LiteralNode)node).Language;
                                    }
                                    else if (node is UriNode && v.Equals("uri"))
                                    {
                                        specialization.reference = ((UriNode)node).Uri.ToString();
                                        uri = specialization.reference;
                                    }
                                }
                                if (string.IsNullOrEmpty(specialization.label))
                                {
                                    var entity = GetLabel(uri);
                                    specialization.label = entity.Label;
                                    specialization.lang = entity.Lang;
                                }
                                if (string.IsNullOrEmpty(specialization.lang))
                                    specialization.lang = defaultLanguage;

                                Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
                            }
                            break;
                        case RepositoryType.JORD:
                            sparql = ReadSparql(queryJord.FileName).Replace("param1", id);
                            sparqlResults = QueryFromRepository(repository, sparql);
                            foreach (var result in sparqlResults)
                            {
                                var specialization = new Specialization();
                                specialization.repository = repository.Name;
                                var uri = string.Empty;

                                foreach (var v in result.Variables)
                                {
                                    var node = result[v];
                                    if (node is LiteralNode && v.Equals("label"))
                                    {
                                        specialization.label = ((LiteralNode)node).Value;
                                        specialization.lang = ((LiteralNode)node).Language;
                                    }
                                    else if (node is UriNode && v.Equals("uri"))
                                    {
                                        specialization.reference = ((UriNode)node).Uri.ToString();
                                        uri = specialization.reference;
                                    }
                                    else if (node is UriNode && v.Equals("rdsuri"))
                                    {
                                        specialization.rdsuri = ((UriNode)node).Uri.ToString();
                                        uri = specialization.reference;
                                    }
                                }
                                if (string.IsNullOrEmpty(specialization.label))
                                {
                                    var entity = GetLabel(uri);
                                    specialization.label = entity.Label;
                                    specialization.lang = entity.Lang;
                                }
                                if (string.IsNullOrEmpty(specialization.lang))
                                    specialization.lang = defaultLanguage;

                                Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
                            }
                            break;
                    }
                }

                return specializations;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetSpecializations: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        public Entity GetClassLabel(string id)
        {
            //string rdsnumber;
            var isRDS = id.Substring(0, 3);
            return !isRDS.Equals("RDS") ? GetLabel(_namespaces.Find(ns => ns.Prefix == "rdl").Uri + id) : GetLabel(_namespaces.Find(ns => ns.Prefix == "jordrdl").Uri + id);
        }

        public QMXF GetClass(string id, Repository repository)
        {
            return GetClass(id, String.Empty, repository);
        }

        public QMXF GetClass(string id)
        {
            return GetClass(id, String.Empty, null);
        }

        public QMXF GetClass(string id, string namespaceUrl, Repository rep)
        {
            var qmxf = new QMXF();

            try
            {
                QMXFName name;
                Description description;
                QMXFStatus status;

                var classifications = new List<Classification>();
                var specializations = new List<Specialization>();

                var resultEntities = new RefDataEntities();
                var resultEnt = new List<Entity>();
                var sparql = String.Empty;
                var relativeUri = String.Empty;
                var resultValue = string.Empty;
                var dataType = string.Empty;
                var uri = string.Empty;

                var classQuery = (Query)_queries.FirstOrDefault(c => c.Key == "GetClass").Query;
                var classQueryJord = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassJORD").Query;

                namespaceUrl = this._namespaces.Find(n => n.Prefix == "rdl").Uri;
                uri = namespaceUrl + id;
                foreach (var repository in this._repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    sparql = repository.RepositoryType == RepositoryType.JORD ? ReadSparql(classQueryJord.FileName).Replace("param1", uri) : ReadSparql(classQuery.FileName).Replace("param1", uri);
                    ClassDefinition classDefinition = null;

                    var sparqlResults = QueryFromRepository(repository, sparql);
                    classifications = new List<Classification>();
                    specializations = new List<Specialization>();

                    foreach (var result in sparqlResults)
                    {
                        classDefinition = new ClassDefinition();
                        classDefinition.identifier = uri;
                        classDefinition.repositoryName = repository.Name;
                        name = new QMXFName();
                        description = new Description();
                        status = new QMXFStatus();
                        foreach (var v in result.Variables)
                        {
                            var node = result[v];
                            if (node is LiteralNode && v.Equals("label"))
                            {
                                name.value = ((ILiteralNode)node).Value;
                                name.lang = ((ILiteralNode)node).Language;
                                if (string.IsNullOrEmpty(name.lang))
                                    name.lang = defaultLanguage;
                            }
                            else if (node is LiteralNode && v.Equals("definition"))
                            {
                                description.value = ((ILiteralNode)node).Value;
                                description.lang = ((ILiteralNode)node).Language;
                                if (string.IsNullOrEmpty(description.lang))
                                    description.lang = defaultLanguage;
                            }
                            else if (node is LiteralNode && v.Equals("creator"))
                            {
                                status.authority = ((ILiteralNode)node).Value;
                            }
                            else if (node is LiteralNode && v.Equals("creationDate"))
                            {
                                status.from = ((ILiteralNode)node).Value;
                            }
                            else if (node is LiteralNode && v.Equals("class"))
                            {
                                status.Class = ((ILiteralNode)node).Value;
                            }
                            else if (node is LiteralNode && v.Equals("comment"))
                            {
                                description.value = ((ILiteralNode)node).Value;
                                description.lang = ((ILiteralNode)node).Language;
                                if (string.IsNullOrEmpty(description.lang))
                                    description.lang = defaultLanguage;
                            }
                            else if (node is UriNode && v.Equals("type"))
                            {
                                string entityType = ((UriNode)node).Uri.ToString();

                                if (!string.IsNullOrEmpty(entityType))
                                {
                                    classDefinition.entityType = new EntityType { reference = entityType };
                                }
                            }
                            else if (node is UriNode && v.Equals("authority"))
                            {
                                status.authority = ((UriNode)node).Uri.ToString();
                            }
                            else if (node is UriNode && v.Equals("recorded"))
                            {
                                status.Class = ((UriNode)node).Uri.ToString();
                            }
                            else if (node is LiteralNode && v.Equals("from"))
                            {
                                status.from = ((LiteralNode)node).Value;
                            }
                        }
                        classDefinition.name.Add(name);

                        classDefinition.description.Add(description);
                        classDefinition.status.Add(status);

                        classifications = GetClassifications(id, repository);

                        specializations = GetSpecializations(id, repository);
                        if (classifications.Count > 0)
                            classDefinition.classification = classifications;
                        if (specializations.Count > 0)
                            classDefinition.specialization = specializations;
                    }
                    if (classDefinition != null)
                        qmxf.classDefinitions.Add(classDefinition);
                }
                return qmxf;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetClass: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        public Entities GetSuperClasses(string id, Repository repo)
        {
            var queryResult = new Entities();
            var language = string.Empty;
            var names = new List<string>();
            try
            {
                var specializations = GetSpecializations(id, repo);

                foreach (var specialization in specializations)
                {
                    var uri = specialization.reference;

                    var label = specialization.label ?? GetLabel(uri).Label;

                    names = label.Split('@').ToList();

                    language = names.Count == 1 ? defaultLanguage : names[names.Count - 1];

                    var resultEntity = new Entity
                    {
                        Uri = uri,
                        Label = names[0],
                        Lang = language
                    };
                    Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetSuperClasses: " + e);
                throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
            }
            return queryResult;
        }

        public Entities GetAllSuperClasses(string id)
        {
            var list = new Entities();
            return GetAllSuperClasses(id, list);
        }

        public Entities GetAllSuperClasses(string id, Entities list)
        {
            var names = new List<string>();
            try
            {

                List<Specialization> specializations = GetSpecializations(id, null);
                //base case
                if (specializations.Count == 0)
                {
                    return list;
                }

                foreach (Specialization specialization in specializations)
                {
                    var uri = specialization.reference;
                    var label = specialization.label;
                    var language = string.Empty;

                    if (label == null)
                    {
                        names = GetLabel(uri).Label.Split('@').ToList();
                        label = names[0];
                        language = names.Count == 1 ? defaultLanguage : names[names.Count - 1];
                    }
                    var resultEntity = new Entity
                    {
                        Uri = uri,
                        Label = label,
                        Lang = language
                    };

                    var trimmedUri = string.Empty;
                    var found = false;
                    foreach (var entt in list)
                    {
                        if (resultEntity.Uri.Equals(entt.Uri))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        trimmedUri = uri.Remove(0, uri.LastIndexOf('#') + 1);
                        Utility.SearchAndInsert(list, resultEntity, Entity.sortAscending());
                        GetAllSuperClasses(trimmedUri, list);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetAllSuperClasses: " + e);
                throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
            }

            return list;
        }

        public Entities GetClassMembers(string Id, Repository repo)
        {
            var membersResult = new Entities();
            try
            {
                var sparql = string.Empty;
                Entity resultEntity = null;
                SparqlResultSet sparqlResults;
                var getMembers = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembers").Query;
                sparql = ReadSparql(getMembers.FileName);
                var getMembersP8 = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembersPart8").Query;
                var getMembersJORD = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembersJORD").Query;


                foreach (Repository repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        sparql = ReadSparql(getMembersP8.FileName).Replace("param1", Id);
                    }
                    else if (repository.RepositoryType == RepositoryType.JORD)
                    {
                        sparql = ReadSparql(getMembersJORD.FileName).Replace("param1", Id);
                    }
                    else
                    {
                        sparql = ReadSparql(getMembers.FileName).Replace("param1", Id);
                    }
                    sparqlResults = QueryFromRepository(repository, sparql);
                    foreach (var result in sparqlResults)
                    {
                        resultEntity = new Entity();
                        resultEntity.Repository = repository.Name;
                        foreach (var v in result.Variables)
                        {
                            var node = result[v];

                            if (node is LiteralNode)
                            {
                                resultEntity.Label = ((LiteralNode)node).Value;
                                resultEntity.Lang = string.IsNullOrEmpty(((LiteralNode)node).Language) ? defaultLanguage : ((LiteralNode)node).Language;
                            }
                            else if (node is UriNode && v.Equals("uri"))
                            {
                                resultEntity.Uri = ((UriNode)node).Uri.ToString();
                            }
                            else if (node is UriNode && v.Equals("rds"))
                            {
                                resultEntity.RDSUri = ((UriNode)node).Uri.ToString();
                            }
                        }
                        Utility.SearchAndInsert(membersResult, resultEntity, Entity.sortAscending());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Getmembers: " + ex);
                throw new Exception("Error while Finding " + Id + ".\n" + ex.ToString(), ex);
            }
            return membersResult;
        }

        public Entities GetSubClasses(string id, Repository repo)
        {
            var queryResult = new Entities();

            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;
                Entity resultEntity = null;

                var queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClasses").Query;
                var queryGetSubClassesJORD = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassesJORD").Query;
                var queryGetSubClassesP8 = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOf").Query;

                foreach (Repository repository in _repositories)
                {
                    switch (repository.RepositoryType)
                    {
                        case RepositoryType.Part8:
                            {
                                sparql = ReadSparql(queryGetSubClassesP8.FileName).Replace("param1", id);
                                var sparqlResults = QueryFromRepository(repository, sparql);
                                foreach (var result in sparqlResults)
                                {
                                    resultEntity = new Entity();
                                    foreach (var node in result.Variables.Select(v => result[v]))
                                    {
                                        if (node is LiteralNode)
                                        {
                                            resultEntity.Label = ((LiteralNode)node).Value;
                                            resultEntity.Lang = string.IsNullOrEmpty(((LiteralNode)node).Language) ? defaultLanguage : ((LiteralNode)node).Language;
                                        }
                                        else if (node is UriNode)
                                        {
                                            resultEntity.Uri = ((UriNode)node).Uri.ToString();
                                        }
                                    }
                                    resultEntity.Repository = repository.Name;
                                    Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                                }
                            }
                            break;
                        case RepositoryType.JORD:
                            {
                                sparql = ReadSparql(queryGetSubClassesJORD.FileName).Replace("param1", id);
                                var sparqlResults = QueryFromRepository(repository, sparql);

                                foreach (var result in sparqlResults)
                                {
                                    resultEntity = new Entity();
                                    foreach (var v in result.Variables)
                                    {
                                        var node = result[v];
                                        if (node is LiteralNode)
                                        {
                                            resultEntity.Label = ((LiteralNode)result[v]).Value;
                                            resultEntity.Lang = string.IsNullOrEmpty(((LiteralNode)result[v]).Language) ? defaultLanguage : ((LiteralNode)result[v]).Language;
                                        }
                                        else if (node is UriNode && v.Equals("rdsuri"))
                                        {
                                            resultEntity.RDSUri = ((UriNode)result[v]).Uri.ToString();
                                        }
                                        else if (node is UriNode && v.Equals("uri"))
                                        {
                                            resultEntity.Uri = ((UriNode)result[v]).Uri.ToString();
                                        }
                                    }
                                    resultEntity.Repository = repository.Name;

                                    Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                                }
                            }
                            break;
                        default:
                            {
                                sparql = ReadSparql(queryGetSubClasses.FileName).Replace("param1", id);
                                var sparqlResults = QueryFromRepository(repository, sparql);

                                foreach (var result in sparqlResults)
                                {
                                    resultEntity = new Entity();
                                    foreach (var node in result.Variables.Select(v => result[v]))
                                    {
                                        if (node is LiteralNode)
                                        {
                                            resultEntity.Label = ((LiteralNode)node).Value;
                                            resultEntity.Lang = string.IsNullOrEmpty(((LiteralNode)node).Language) ? defaultLanguage : ((LiteralNode)node).Language;
                                        }
                                        else if (node is UriNode)
                                        {
                                            resultEntity.Uri = ((UriNode)node).Uri.ToString();
                                        }
                                    }
                                    resultEntity.Repository = repository.Name;
                                    Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetSubClasses: " + e);
                throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
            }
            return queryResult;
        }

        public Entities GetSubClassesCount(string id)
        {
            var queryResult = new Entities();
            try
            {
                var sparql = String.Empty;
                var sparqlPart8 = String.Empty;
                var sparqlJord = String.Empty;
                var relativeUri = String.Empty;

                var queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassesCount").Query;
                var queryGetSubClassesJord = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassesCountJORD").Query;
                sparqlJord = ReadSparql(queryGetSubClassesJord.FileName).Replace("param1", id);
                sparql = ReadSparql(queryGetSubClasses.FileName).Replace("param1", id);
                var queryGetSubClassOfInverse = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOfCount").Query;
                sparqlPart8 = ReadSparql(queryGetSubClassOfInverse.FileName).Replace("param1", id);

                var count = 0;
                foreach (var repository in _repositories)
                {
                    switch (repository.RepositoryType)
                    {
                        case RepositoryType.Part8:
                            {
                                var sparqlResults = QueryFromRepository(repository, sparqlPart8);
                                count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode)result[v]).Value)));
                            }
                            break;
                        case RepositoryType.JORD:
                            {
                                var sparqlResults = QueryFromRepository(repository, sparqlJord);
                                count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode)result[v]).Value)));
                            }
                            break;
                        default:
                            {
                                var sparqlResults = QueryFromRepository(repository, sparql);

                                count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode)result[v]).Value)));
                            }
                            break;
                    }
                }
                var resultEntity = new Entity
                {
                    Uri = string.Empty,
                    Label = Convert.ToString(count),
                    Lang = string.Empty,
                };

                Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetSubClasses: " + e);
                throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
            }
            return queryResult;
        }
        public Entities GetEntityTypes()
        {
            var queryResult = new Entities();
            var sparql = string.Empty;
            try
            {
                var getEntities = (Query)_queries.FirstOrDefault(c => c.Key.Equals("GetEntityTypes")).Query;
                sparql = ReadSparql(getEntities.FileName);
                foreach (var rep in _repositories)
                {
                    if (!rep.Name.Equals("EntityTypes")) continue;
                    var sparqlResults = QueryFromRepository(rep, sparql);
                    foreach (var result in sparqlResults)
                    {
                        var entity = new Entity();
                        foreach (var e in from v in result.Variables where ((UriNode)result[v]).Uri != null select ((UriNode)result[v]).Uri)
                        {
                            entity.Uri = e.ToString();
                            entity.Label = e.Fragment.Substring(1);
                        }
                        queryResult.Add(entity);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetSubClasses: " + e);
                throw new Exception("Error getting EntityTypes " + e.ToString(), e);
            }

            return queryResult;
        }

        public Entities GetClassTemplates(string id)
        {
            var queryResult = new Entities();
            Entity resultEntity = null;

            try
            {
                var sparqlGetClassTemplates = String.Empty;
                var sparqlGetRelatedTemplates = String.Empty;
                var relativeUri = String.Empty;

                var queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplates").Query;

                sparqlGetClassTemplates = ReadSparql(queryGetClassTemplates.FileName);
                sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

                var queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplates").Query;

                sparqlGetRelatedTemplates = ReadSparql(queryGetRelatedTemplates.FileName);
                sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        var sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

                        foreach (SparqlResult result in sparqlResults.Results)
                        {
                            resultEntity = new Entity();
                            foreach (var v in result.Variables)
                            {
                                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                                {
                                    resultEntity.Label = ((LiteralNode)result[v]).Value;
                                    resultEntity.Lang = string.IsNullOrEmpty(((LiteralNode)result[v]).Language) ? defaultLanguage : ((LiteralNode)result[v]).Language;
                                }
                                else if ((INode)result[v] is UriNode)
                                {
                                    resultEntity.Uri = ((UriNode)result[v]).Uri.ToString();
                                }
                            }
                            resultEntity.Repository = repository.Name;
                            Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                        }
                    }
                    else
                    {
                        var sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);

                        foreach (var result in sparqlResults)
                        {
                            resultEntity = new Entity();
                            foreach (var v in result.Variables)
                            {
                                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                                {
                                    resultEntity.Label = ((LiteralNode)result[v]).Value;
                                    if (string.IsNullOrEmpty(((LiteralNode)result[v]).Language))
                                    {
                                        resultEntity.Lang = defaultLanguage;
                                    }
                                    else
                                    {
                                        resultEntity.Lang = ((LiteralNode)result[v]).Language;
                                    }
                                }
                                else if ((INode)result[v] is UriNode)
                                {
                                    resultEntity.Uri = ((UriNode)result[v]).Uri.ToString();
                                }
                            }
                            resultEntity.Repository = repository.Name;
                            Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetClassTemplates: " + e);
                throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
            }
            return queryResult;
        }

        public Entities GetClassTemplatesCount(string id)
        {
            var queryResult = new Entities();

            try
            {
                var sparqlGetClassTemplates = String.Empty;
                var sparqlGetRelatedTemplates = String.Empty;
                var relativeUri = String.Empty;

                var queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplatesCount").Query;

                sparqlGetClassTemplates = ReadSparql(queryGetClassTemplates.FileName);
                sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

                var queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplatesCount").Query;

                sparqlGetRelatedTemplates = ReadSparql(queryGetRelatedTemplates.FileName);
                sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

                var count = 0;
                foreach (var repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        var sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);
                        count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode)result[v]).Value)));
                    }
                    else
                    {
                        var sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);
                        count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode)result[v]).Value)));
                    }
                }
                var resultEntity = new Entity
                {
                    Uri = string.Empty,
                    Label = Convert.ToString(count),
                    Lang = string.Empty,
                };

                Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetClassTemplates: " + e);
                throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
            }
            return queryResult;
        }

        private List<RoleDefinition> GetRoleDefinition(string id, Repository repository)
        {
            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;
                var sparqlQuery = string.Empty;

                var description = new Description();
                var status = new QMXFStatus();

                var roleDefinitions = new List<RoleDefinition>();
                var resultEntities = new RefDataEntities();

                switch (repository.RepositoryType)
                {
                    case RepositoryType.Part8:
                        sparqlQuery = "GetPart8Roles";
                        break;
                    case RepositoryType.Camelot:
                    case RepositoryType.RDSWIP:
                        sparqlQuery = "GetRoles";
                        break;
                }


                var queryContainsSearch = _queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

                sparql = ReadSparql(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                var sparqlResults = QueryFromRepository(repository, sparql);

                foreach (var result in sparqlResults)
                {
                    var roleDefinition = new RoleDefinition();
                    var name = new QMXFName();
                    foreach (var v in result.Variables)
                    {
                        if ((INode)result[v] is LiteralNode && v.Equals("label"))
                        {
                            name.value = ((LiteralNode)result[v]).Value;
                            name.lang = ((LiteralNode)result[v]).Language;
                            if (string.IsNullOrEmpty(name.lang))
                                name.lang = defaultLanguage;
                        }
                        else if ((INode)result[v] is LiteralNode && v.Equals("comment"))
                        {
                            roleDefinition.description.value = ((LiteralNode)result[v]).Value;
                            roleDefinition.description.lang = ((LiteralNode)result[v]).Language;
                            if (string.IsNullOrEmpty(roleDefinition.description.lang))
                                roleDefinition.description.lang = defaultLanguage;
                        }
                        else if ((INode)result[v] is LiteralNode && v.Equals("index"))
                        {
                            if (string.IsNullOrEmpty(roleDefinition.description.value))
                            {
                                roleDefinition.description.value = ((LiteralNode)result[v]).Value;
                                roleDefinition.description.lang = ((LiteralNode)result[v]).Language;
                                if (string.IsNullOrEmpty(roleDefinition.description.lang))
                                    roleDefinition.description.lang = defaultLanguage;
                            }
                        }
                        else if ((INode)result[v] is UriNode && v.Equals("type"))
                        {
                            roleDefinition.range = ((UriNode)result[v]).Uri.ToString();
                        }
                        else if ((INode)result[v] is UriNode && v.Equals("role"))
                        {
                            roleDefinition.identifier = ((UriNode)result[v]).Uri.ToString();
                        }
                    }
                    roleDefinition.name.Add(name);

                    Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
                }
                return roleDefinitions;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetRoleDefinition: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        private List<RoleDefinition> GetRoleDefinition(string id)
        {
            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;
                var sparqlQuery = string.Empty;

                var description = new Description();
                var status = new QMXFStatus();

                var roleDefinitions = new List<RoleDefinition>();

                var resultEntities = new RefDataEntities();

                foreach (var repository in _repositories)
                {
                    switch (repository.RepositoryType)
                    {
                        case RepositoryType.Camelot:
                        case RepositoryType.RDSWIP:
                            sparqlQuery = "GetRoles";
                            break;
                        case RepositoryType.Part8:
                            sparqlQuery = "GetPart8Roles";
                            break;
                    }
                    var queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

                    sparql = ReadSparql(queryContainsSearch.FileName);
                    sparql = sparql.Replace("param1", id);
                    var sparqlResults = QueryFromRepository(repository, sparql);

                    foreach (var result in sparqlResults)
                    {
                        var roleDefinition = new RoleDefinition();
                        var name = new QMXFName();

                        if (result["label"] != null)
                        {
                            name.value = ((LiteralNode)result["label"]).Value;
                            name.lang = ((LiteralNode)result["label"]).Language;
                            if (string.IsNullOrEmpty(name.lang))
                                name.lang = defaultLanguage;
                        }
                        if (result["comment"] != null)
                        {
                            description.value = ((LiteralNode)result["comment"]).Value;
                            description.lang = ((LiteralNode)result["comment"]).Language;
                            if (string.IsNullOrEmpty(description.lang))
                                description.lang = defaultLanguage;
                        }
                        if (result["index"] != null)
                        {
                            if (string.IsNullOrEmpty(description.value))
                            {
                                description.value = ((LiteralNode)result["index"]).Value;
                                description.lang = ((LiteralNode)result["index"]).Language;
                                if (string.IsNullOrEmpty(description.lang))
                                    description.lang = defaultLanguage;
                            }
                        }
                        if (result["role"] != null)
                        {
                            roleDefinition.identifier = ((UriNode)result["role"]).Uri.ToString();
                        }
                        if (result["type"] != null)
                        {
                            roleDefinition.range = ((UriNode)result["type"]).Uri.ToString();
                        }

                        roleDefinition.description = description;
                        roleDefinition.name.Add(name);
                        Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
                    }
                }

                return roleDefinitions;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetRoleDefinition: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        private List<RoleQualification> GetRoleQualification(string id, Repository rep)
        {
            try
            {
                var description = new Description();
                var status = new QMXFStatus();
                var uri = String.Empty;
                var nameValue = string.Empty;

                var roleQualifications = new List<RoleQualification>();

                foreach (var repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;
                    switch (rep.RepositoryType)
                    {
                        case RepositoryType.Camelot:
                        case RepositoryType.RDSWIP:

                            var rangeSparql = String.Empty;
                            var relativeUri = String.Empty;
                            var referenceSparql = String.Empty;
                            var relativeUri1 = String.Empty;
                            var valueSparql = String.Empty;
                            var relativeUri2 = String.Empty;

                            var rangeResultEntities = new RefDataEntities();
                            var referenceResultEntities = new RefDataEntities();
                            var valueResultEntities = new RefDataEntities();

                            var getRangeRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetRangeRestriction").Query;
                            var getReferenceRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetReferenceRestriction").Query;
                            var getValueRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetValueRestriction").Query;

                            rangeSparql = ReadSparql(getRangeRestriction.FileName);
                            rangeSparql = rangeSparql.Replace("param1", id);

                            referenceSparql = ReadSparql(getReferenceRestriction.FileName);
                            referenceSparql = referenceSparql.Replace("param1", id);

                            valueSparql = ReadSparql(getValueRestriction.FileName);
                            valueSparql = valueSparql.Replace("param1", id);

                            var rangeSparqlResults = QueryFromRepository(repository, rangeSparql);
                            var referenceSparqlResults = QueryFromRepository(repository, referenceSparql);
                            var valueSparqlResults = QueryFromRepository(repository, valueSparql);

                            var combinedResults = rangeSparqlResults;
                            combinedResults.Results.AddRange(referenceSparqlResults);
                            combinedResults.Results.AddRange(valueSparqlResults);

                            foreach (var result in combinedResults)
                            {
                                var roleQualification = new RoleQualification();
                                var name = new QMXFName();
                                var refvalue = new QMXFValue();
                                var valvalue = new QMXFValue();
                                foreach (var v in result.Variables)
                                {
                                    if (v.Equals("qualifies") && result.HasValue(v))
                                    {
                                        uri = ((UriNode)result[v]).Uri.ToString();
                                        roleQualification.qualifies = uri;
                                        roleQualification.identifier = Utility.GetIdFromURI(uri);
                                    }
                                    else if (v.Equals("name"))
                                    {
                                        if (result[v] == null)
                                        {
                                            Entity entity = GetLabel(uri);
                                            name.value = entity.Label;
                                            name.lang = entity.Lang;
                                        }
                                        else
                                        {
                                            name.value = ((LiteralNode)result[v]).Value;
                                            name.lang = ((LiteralNode)result[v]).Language;
                                        }
                                        if (string.IsNullOrEmpty(name.lang))
                                        {
                                            name.lang = defaultLanguage;
                                        }

                                    }
                                    else if (v.Equals("range") && result.HasValue(v))
                                    {
                                        roleQualification.range = ((UriNode)result[v]).Uri.ToString();
                                    }
                                    else if (v.Equals("reference") && result.HasValue(v))
                                    {
                                        refvalue.reference = ((UriNode)result[v]).Uri.ToString();
                                        roleQualification.value = refvalue;
                                    }
                                    else if (v.Equals("value") && result.HasValue(v))
                                    {
                                        valvalue.text = ((LiteralNode)result[v]).Value;
                                        valvalue.As = ((UriNode)result["value_dataType"]).Uri.ToString();
                                        roleQualification.value = valvalue;
                                    }
                                }
                                roleQualification.name.Add(name);
                                Utility.SearchAndInsert(roleQualifications, roleQualification, RoleQualification.sortAscending());
                            }
                            break;
                        case RepositoryType.Part8:
                            var part8Entities = new RefDataEntities();
                            var getPart8Roles = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Roles").Query;
                            var getPart8RolesBindings = getPart8Roles.Bindings;

                            var part8RolesSparql = ReadSparql(getPart8Roles.FileName);
                            part8RolesSparql = part8RolesSparql.Replace("param1", id);
                            var part8RolesResults = QueryFromRepository(repository, part8RolesSparql);
                            foreach (var result in part8RolesResults)
                            {
                                var roleQualification = new RoleQualification();
                                var name = new QMXFName();
                                var refvalue = new QMXFValue();
                                var valvalue = new QMXFValue();
                                var descr = new Description();

                                if (result["role"] != null)
                                {
                                    uri = ((UriNode)result["role"]).Uri.ToString();
                                    roleQualification.qualifies = uri;
                                    roleQualification.identifier = Utility.GetIdFromURI(uri);
                                }
                                if (result["comment"] != null)
                                {
                                    descr.value = ((LiteralNode)result["comment"]).Value;
                                    descr.lang = string.IsNullOrEmpty(((LiteralNode)result["comment"]).Language) ? defaultLanguage : ((LiteralNode)result["comment"]).Language;
                                }
                                if (result["value"] != null)
                                {
                                    valvalue.reference = ((UriNode)result["value"]).Uri.ToString();
                                    roleQualification.value = valvalue;
                                }
                                if (result["type"] != null)
                                {
                                    roleQualification.range = ((UriNode)result["type"]).Uri.ToString();
                                }

                                if (result["label"] == null)
                                {
                                    var entity = GetLabel(uri);
                                    name.value = entity.Label;
                                    name.lang = entity.Lang;
                                }
                                else
                                {
                                    name.value = ((LiteralNode)result["label"]).Value;
                                    name.lang = ((LiteralNode)result["label"]).Language;
                                }
                                if (string.IsNullOrEmpty(name.lang))
                                    name.lang = defaultLanguage;

                                if (result["index"] != null)
                                {
                                    ///TODO
                                }

                                roleQualification.name.Add(name);
                                Utility.SearchAndInsert(roleQualifications, roleQualification, RoleQualification.sortAscending());
                            }
                            break;
                    }
                }
                return roleQualifications;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetRoleQualification: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        private List<TemplateDefinition> GetTemplateDefinition(string id, Repository rep)
        {
            var templateDefinitionList = new List<TemplateDefinition>();
            TemplateDefinition templateDefinition = null;

            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;
                var qId = string.Empty;
                Query queryContainsSearch = null;
                var description = new Description();
                var status = new QMXFStatus();

                qId = !id.Contains(":") ? string.Format("tpl:{0}", id) : id;

                foreach (var repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetBaseTemplatePart8").Query;
                    }
                    else
                    {
                        queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetTemplate").Query;
                    }

                    sparql = ReadSparql(queryContainsSearch.FileName);
                    sparql = sparql.Replace("param1", id);

                    var sparqlResults = QueryFromRepository(repository, sparql);

                    foreach (var result in sparqlResults.Where(result => result.Count != 0))
                    {
                        templateDefinition = new TemplateDefinition();
                        var name = new QMXFName();
                        templateDefinition.repositoryName = repository.Name;

                        foreach (var v in result.Variables)
                        {
                            if ((INode)result[v] is LiteralNode && v.Equals("label"))
                            {
                                name.value = ((LiteralNode)result[v]).Value;
                                name.lang = ((LiteralNode)result[v]).Language;
                                if (string.IsNullOrEmpty(name.lang))
                                    name.lang = defaultLanguage;
                            }
                            else if ((INode)result[v] is LiteralNode && v.Equals("definition"))
                            {
                                description.value = ((LiteralNode)result[v]).Value;
                                description.lang = ((LiteralNode)result[v]).Language;
                                if (string.IsNullOrEmpty(description.lang))
                                    description.lang = defaultLanguage;
                            }
                            else if ((INode)result[v] is LiteralNode && v.Equals("creationDate"))
                            {
                                status.@from = ((LiteralNode)result[v]).Value;
                            }
                        }
                        templateDefinition.identifier = qId;
                        templateDefinition.name.Add(name);
                        templateDefinition.description.Add(description);
                        templateDefinition.status.Add(status);

                        templateDefinition.roleDefinition = GetRoleDefinition(id, repository);
                        templateDefinitionList.Add(templateDefinition);
                    }
                }

                return templateDefinitionList;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetTemplateDefinition: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        public QMXF GetTemplate(string id, QMXFType templateType, Repository rep)
        {
            var qmxf = new QMXF();
            List<TemplateQualification> templateQualification = null;
            List<TemplateDefinition> templateDefinition = null;
            try
            {
                if (templateType == QMXFType.Qualification)
                {
                    templateQualification = GetTemplateQualification(id, rep);
                }
                else
                {
                    templateDefinition = GetTemplateDefinition(id, rep);
                }
                if (templateQualification != null)
                {
                    qmxf.templateQualifications = templateQualification;
                }
                else
                {
                    qmxf.templateDefinitions = templateDefinition;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetTemplate: " + ex);
            }
            return qmxf;
        }

        public QMXF GetTemplate(string id)
        {
            var qmxf = new QMXF();

            try
            {
                var templateQualification = GetTemplateQualification(id, null);

                if (templateQualification.Count > 0)
                {
                    qmxf.templateQualifications = templateQualification;
                }
                else
                {
                    var templateDefinition = GetTemplateDefinition(id, null);
                    qmxf.templateDefinitions = templateDefinition;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetTemplate: " + ex);
            }

            return qmxf;
        }


        private List<TemplateQualification> GetTemplateQualification(string id, Repository rep)
        {
            TemplateQualification templateQualification = null;
            var templateQualificationList = new List<TemplateQualification>();

            try
            {
                var sparql = String.Empty;
                var relativeUri = String.Empty;
                var sparqlQuery = string.Empty;
                var dataType = string.Empty;
                var qId = string.Empty;
                Query getTemplateQualification = null;

                qId = !id.Contains(":") ? string.Format("tpl:{0}", id) : id;

                {
                    foreach (var repository in _repositories)
                    {
                        if (rep != null)
                            if (rep.Name != repository.Name) continue;

                        switch (repository.RepositoryType)
                        {
                            case RepositoryType.Camelot:
                            case RepositoryType.RDSWIP:
                            case RepositoryType.JORD:
                                sparqlQuery = "GetTemplateQualification";
                                break;
                            case RepositoryType.Part8:
                                sparqlQuery = "GetTemplateQualificationPart8";
                                break;
                        }

                        getTemplateQualification = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

                        sparql = ReadSparql(getTemplateQualification.FileName);
                        sparql = sparql.Replace("param1", id);

                        var sparqlResults = QueryFromRepository(repository, sparql);

                        foreach (var result in sparqlResults.Results)
                        {
                            templateQualification = new TemplateQualification();
                            var description = new Description();
                            var status = new QMXFStatus();
                            var name = new QMXFName();

                            templateQualification.repositoryName = repository.Name;

                            foreach (var v in result.Variables)
                            {
                                if ((INode)result[v] is LiteralNode && v.Equals("name"))
                                {
                                    name.value = ((LiteralNode)result[v]).Value;
                                    name.lang = ((LiteralNode)result[v]).Language;
                                    if (string.IsNullOrEmpty(name.lang))
                                        name.lang = defaultLanguage;
                                }
                                else if ((INode)result[v] is LiteralNode && v.Equals("description"))
                                {
                                    description.value = ((LiteralNode)result[v]).Value;
                                    description.lang = ((LiteralNode)result[v]).Language;
                                    if (string.IsNullOrEmpty(description.lang))
                                        description.lang = defaultLanguage;
                                }
                                else if ((INode)result[v] is UriNode && v.Equals("statusClass"))
                                {
                                    status.Class = ((UriNode)result[v]).Uri.ToString();
                                }
                                else if ((INode)result[v] is UriNode && v.Equals("statusAuthority"))
                                {
                                    status.authority = ((UriNode)result[v]).Uri.ToString();
                                }
                                else if ((INode)result[v] is UriNode && v.Equals("qualifies"))
                                {
                                    templateQualification.qualifies = ((UriNode)result[v]).Uri.ToString();
                                }
                            }

                            templateQualification.identifier = qId;
                            templateQualification.name.Add(name);
                            templateQualification.description.Add(description);
                            templateQualification.status.Add(status);

                            templateQualification.roleQualification = GetRoleQualification(id, repository);
                            templateQualificationList.Add(templateQualification);
                        }
                    }
                }
                return templateQualificationList;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetTemplateQualification: " + e);
                throw new Exception("Error while Getting Template: " + id + ".\n" + e.ToString(), e);
            }
        }

        private int GetIndexFromName(string name)
        {
            try
            {
                var index = 0;
                foreach (var repository in _repositories.Where(repository => repository.Name.Equals(name)))
                {
                    index = _repositories.IndexOf(repository);
                    return index;
                }
                foreach (var repository in _repositories.Where(repository => !repository.IsReadOnly))
                {
                    index = _repositories.IndexOf(repository);
                    return index;
                }

                return index;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///  this will generate an id formatted as R + new Guid replacing '_' with blank '' space
        ///  example = RC2E15CCD8F104DD69188E6A5A23354B1
        /// </summary>
        /// <param name="RegistryBase"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string CreateNewGuidId(string RegistryBase)//, string name)
        {
            if (!string.IsNullOrEmpty(RegistryBase))
                return string.Format("{0}R{1}", RegistryBase, Guid.NewGuid().ToString().Replace("_", "").Replace("-", "").ToUpper());
            else
            {
                _logger.Error("Failed to create id:");
                throw new Exception("CreateIdsAdiId: Failed to create id ");
            }
        }

        private List<Dictionary<string, string>> MergeLists(List<Dictionary<string, string>> a, List<Dictionary<string, string>> b)
        {
            try
            {
                a.AddRange(b);
                return a;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Read query from file
        /// </summary>
        private string ReadSparql(string queryName)
        {
            try
            {
                return Utility.ReadString(_settings["SparqlPath"] + queryName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private SparqlResultSet QueryFromRepository(Repository repository, string sparql)
        {
            try
            {
                var endpoint = new SparqlRemoteEndpoint(new Uri(repository.Uri));
                if (string.IsNullOrEmpty(repository.AcceptHeader))
                {
                    endpoint.ResultsAcceptHeader = repository.AcceptHeader;
                }

                var encryptedCredentials = repository.EncryptedCredentials;
                var cred = new WebCredentials(encryptedCredentials);
                if (cred.isEncrypted) cred.Decrypt();

                if (!string.IsNullOrEmpty(_settings["ProxyHost"])
                  && !string.IsNullOrEmpty(_settings["ProxyPort"])
                  && !string.IsNullOrEmpty(_settings["ProxyCredentialToken"])) /// need to use proxy
                {
                    var pcred = _settings.GetWebProxyCredentials();
                    endpoint.Proxy = pcred.GetWebProxy() as WebProxy;
                    endpoint.ProxyCredentials = pcred.GetNetworkCredential();
                }
                endpoint.Credentials = cred.GetNetworkCredential();

                var resultSet = endpoint.QueryWithResultSet(sparql);
                return resultSet;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to read repository['{0}']", repository.Uri), ex);
                return new SparqlResultSet();
            }
        }

        private Response PostToRepository(Repository repository, string sparql)
        {
            try
            {
                var response = new Response();

                var endpoint = new SparqlRemoteUpdateEndpoint(repository.UpdateUri);
                var encryptedCredentials = repository.EncryptedCredentials;
                var cred = new WebCredentials(encryptedCredentials);
                if (cred.isEncrypted) cred.Decrypt();

                if (!string.IsNullOrEmpty(_settings["ProxyHost"])
                  && !string.IsNullOrEmpty(_settings["ProxyPort"])
                  && !string.IsNullOrEmpty(_settings["ProxyCredentialToken"])) /// need to use proxy
                {
                    var pcred = _settings.GetWebProxyCredentials();
                    endpoint.Proxy = pcred.GetWebProxy() as WebProxy;
                    endpoint.ProxyCredentials = pcred.GetNetworkCredential();
                }

                endpoint.Credentials = cred.GetNetworkCredential();
                endpoint.Update(sparql);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string MakeUniqueKey(IDictionary<string, string> dictionary, string duplicateKey)
        {
            try
            {
                var newKey = String.Empty;

                for (var i = 2; i < Int32.MaxValue; i++)
                {
                    var postfix = " (" + i.ToString(CultureInfo.InvariantCulture) + ")";
                    if (dictionary.ContainsKey(duplicateKey + postfix)) continue;
                    newKey += postfix;
                    break;
                }

                return newKey;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private RefDataEntities GetRequestedPage(RefDataEntities rde, int startIdx, int pageSize)
        {
            try
            {
                var page = new RefDataEntities();
                page.Total = rde.Entities.Count;

                for (var i = startIdx; i < startIdx + pageSize; i++)
                {
                    if (rde.Entities.Count == i) break;

                    var key = rde.Entities.Keys[i];
                    var entity = rde.Entities[key];
                    page.Entities.Add(key, entity);
                }

                return page;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Reset(string query)
        {
            try
            {
                if (_searchHistory.ContainsKey(query))
                {
                    _searchHistory.Remove(query);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Response PostTemplate(QMXF qmxf)
        {
            var response = new Response();
            try
            {

                var repository = GetRepository(qmxf.targetRepository);
                if (repository == null || repository.IsReadOnly)
                {
                    var status = new Status { Level = StatusLevel.Error };
                    if (repository == null)
                        status.Messages.Add("Repository not found!");
                    else
                        status.Messages.Add("Repository [" + qmxf.targetRepository + "] is read-only!");

                    _response.Append(status);
                }
                else if (repository.RepositoryType != RepositoryType.Part8)
                {
                    throw new Exception("Cannnot write to [" + repository.Name + " repository type " + repository.RepositoryType + "]");
                }
                else
                {
                    var registry = _useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["ClassRegistryBase"];

                    if (qmxf.templateDefinitions.Count > 0)
                    {
                        response = ProcessTemplateDefinitions(qmxf.templateDefinitions, repository);
                    }
                    if (qmxf.templateQualifications.Count > 0)
                    {
                        response = ProcessTemplateQualifications(qmxf.templateQualifications, repository);
                    }

                }
            }

            catch (Exception ex)
            {
                var errMsg = "Error in PostTemplate: " + ex;
                var status = new Status();

                response.Level = StatusLevel.Error;
                status.Messages.Add(errMsg);
                response.Append(status);
                _logger.Error(errMsg);
            }

            return response;
        }

        private Response ProcessTemplateQualifications(List<TemplateQualification> list, Repository repository)
        {
            var response = new Response();
            var delete = new Graph();
            var insert = new Graph();
            delete.NamespaceMap.Import(nsMap);
            insert.NamespaceMap.Import(nsMap);

            foreach (var newTQ in list)
            {
                var roleCount = 0;
                var templateName = string.Empty;
                var templateID = string.Empty;
                var generatedId = string.Empty;
                var roleQualification = string.Empty;
                //int index = 1;
                if (!string.IsNullOrEmpty(newTQ.identifier))
                    templateID = newTQ.identifier;

                templateName = newTQ.name[0].value;
                var oldQmxf = new QMXF();
                if (!String.IsNullOrEmpty(templateID))
                {
                    oldQmxf = GetTemplate(templateID, QMXFType.Qualification, repository);
                }
                else
                {
                    generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                    templateID = generatedId;
                }

                if (oldQmxf.templateQualifications.Count > 0)
                {
                    foreach (var oldTq in oldQmxf.templateQualifications)
                    {
                        foreach (var nn in newTQ.name)
                        {
                            templateName = nn.value;
                            var on = oldTq.name.Find(n => n.lang == nn.lang);
                            if (@on == null) continue;
                            if (String.Compare(@on.value, nn.value, true) == 0) continue;
                            GenerateName(ref delete, @on, templateID, oldTq);
                            GenerateName(ref insert, nn, templateID, newTQ);
                        }
                        foreach (var nd in newTQ.description)
                        {
                            if (nd.lang == null) nd.lang = defaultLanguage;
                            Description od = null;
                            od = oldTq.description.Find(d => d.lang == nd.lang);

                            if (od != null && od.value != null)
                            {
                                if (string.Compare(od.value, nd.value, true) != 0)
                                {
                                    GenerateDescription(ref delete, od, templateID);
                                    GenerateDescription(ref insert, nd, templateID);
                                }
                            }
                            else if (od == null && nd.value != null)
                            {
                                GenerateDescription(ref insert, nd, templateID);
                            }
                        }
                        if (oldTq.roleQualification.Count != newTQ.roleQualification.Count)
                        {
                            GenerateRoleCountPart8(ref delete, oldTq.roleQualification.Count, templateID, oldTq);
                            GenerateRoleCountPart8(ref insert, newTQ.roleQualification.Count, templateID, newTQ);
                        }

                        foreach (var ns in newTQ.specialization)
                        {
                            var os = oldTq.specialization.FirstOrDefault();

                            if (os != null && os.reference != ns.reference)
                            {
                                //TODO
                            }
                        }
                        if (oldTq.roleQualification.Count < newTQ.roleQualification.Count)
                        {
                            var count = 0;
                            foreach (var nrq in newTQ.roleQualification)
                            {
                                var roleName = nrq.name[0].value;
                                var newRoleId = nrq.identifier;

                                if (string.IsNullOrEmpty(newRoleId))
                                {
                                    generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                                    newRoleId = generatedId;
                                }
                                var orq = oldTq.roleQualification.Find(r => r.identifier == newRoleId);
                                if (orq != null) continue;
                                GenerateTypesPart8(ref insert, newRoleId, templateID, nrq);
                                foreach (var nn in nrq.name)
                                {
                                    GenerateName(ref insert, nn, newRoleId, nrq);
                                }
                                GenerateRoleIndexPart8(ref insert, newRoleId, ++count, nrq);
                                GenerateHasTemplate(ref insert, newRoleId, templateID, nrq);
                                GenerateHasRole(ref insert, templateID, newRoleId, newTQ);
                                if (nrq.value != null && !string.IsNullOrEmpty(nrq.value.reference))
                                {
                                    GenerateRoleFillerType(ref insert, newRoleId, nrq.value.reference);
                                }
                                else if (nrq.range != null)
                                {
                                    GenerateRoleFillerType(ref insert, newRoleId, nrq.range);
                                }
                            }
                        }
                        else if (oldTq.roleQualification.Count > newTQ.roleQualification.Count)
                        {
                            var count = 0;
                            foreach (var orq in oldTq.roleQualification)
                            {
                                var roleName = orq.name[0].value;
                                var newRoleId = orq.identifier;

                                if (string.IsNullOrEmpty(newRoleId))
                                {
                                    generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                                    newRoleId = generatedId;
                                }
                                var nrq = newTQ.roleQualification.Find(r => r.identifier == newRoleId);
                                if (nrq != null) continue;
                                GenerateTypesPart8(ref delete, newRoleId, templateID, orq);
                                foreach (var nn in orq.name)
                                {
                                    GenerateName(ref delete, nn, newRoleId, orq);
                                }
                                GenerateRoleIndexPart8(ref delete, newRoleId, ++count, orq);
                                GenerateHasTemplate(ref delete, newRoleId, templateID, orq);
                                GenerateHasRole(ref delete, templateID, newRoleId, oldTq);
                                if (orq.value != null && !string.IsNullOrEmpty(orq.value.reference))
                                {
                                    GenerateRoleFillerType(ref delete, newRoleId, orq.value.reference);
                                }
                                else if (orq.range != null)
                                {
                                    GenerateRoleFillerType(ref delete, newRoleId, nrq.range);
                                }
                            }
                        }
                    }
                    if (delete.IsEmpty && insert.IsEmpty)
                    {
                        var errMsg = "No changes made to template [" + templateName + "]";
                        var status = new Status();
                        response.Level = StatusLevel.Warning;
                        status.Messages.Add(errMsg);
                        response.Append(status);
                        continue;
                    }
                }

                if (delete.IsEmpty)
                {
                    var templateLabel = String.Empty;
                    var labelSparql = String.Empty;

                    foreach (var newName in newTQ.name)
                    {
                        GenerateName(ref insert, newName, templateID, newTQ);
                    }
                    foreach (var newDescr in newTQ.description.Where(newDescr => !string.IsNullOrEmpty(newDescr.value)))
                    {
                        GenerateDescription(ref insert, newDescr, templateID);
                    }
                    GenerateRoleCountPart8(ref insert, newTQ.roleQualification.Count, templateID, newTQ);
                    GenerateTypesPart8(ref insert, templateID, newTQ.qualifies, newTQ);

                    foreach (string specialization in newTQ.specialization.Select(spec => spec.reference))
                    {
                        /// TODO
                    }

                    foreach (var newRole in newTQ.roleQualification)
                    {
                        var roleLabel = newRole.name.FirstOrDefault().value;
                        var roleID = string.Empty;
                        generatedId = string.Empty;
                        var genName = string.Empty;
                        var range = newRole.range;

                        genName = "Role Qualification " + roleLabel;
                        if (string.IsNullOrEmpty(newRole.identifier))
                        {
                            generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                            roleID = generatedId;
                        }
                        else
                        {
                            roleID = newRole.identifier;
                        }

                        GenerateTypesPart8(ref insert, roleID, templateID, newRole);
                        foreach (var newName in newRole.name)
                        {
                            GenerateName(ref insert, newName, roleID, newRole);
                        }
                        GenerateRoleIndexPart8(ref insert, roleID, ++roleCount, newRole);
                        GenerateHasTemplate(ref insert, roleID, templateID, newRole);
                        GenerateHasRole(ref insert, templateID, roleID, newTQ);

                        if (newRole.value != null && !string.IsNullOrEmpty(newRole.value.reference))
                        {
                            GenerateValRoleFiller(ref insert, roleID, newRole.value.reference);
                            //GenerateRoleFillerType(ref insert, roleID, newRole.value.reference);

                        }
                        else if (newRole.range != null)
                        {
                            GenerateRoleFillerType(ref insert, roleID, newRole.range);
                        }
                    }
                }

                if (!delete.IsEmpty)
                {
                    sparqlBuilder.Append(deleteData);
                    foreach (var t in delete.Triples)
                    {
                        sparqlBuilder.AppendLine(t.ToString(formatter));
                    }
                    sparqlBuilder.AppendLine(insert.IsEmpty ? "}" : "};");
                }
                if (!insert.IsEmpty)
                {
                    sparqlBuilder.AppendLine(insertData);
                    foreach (var t in insert.Triples)
                    {
                        sparqlBuilder.AppendLine(t.ToString(formatter));
                    }
                    sparqlBuilder.AppendLine("}");
                }

                var sparql = sparqlBuilder.ToString();
                var postResponse = PostToRepository(repository, sparql);
                response.Append(postResponse);
            }
            return response;
        }

        private Response ProcessTemplateDefinitions(List<TemplateDefinition> list, Repository repository)
        {
            var response = new Response();
            var delete = new Graph();
            var insert = new Graph();
            delete.NamespaceMap.Import(nsMap);
            insert.NamespaceMap.Import(nsMap);

            foreach (var newTDef in list)
            {
                var language = string.Empty;
                var roleCount = 0;
                var templateName = string.Empty;
                var templateId = string.Empty;
                var generatedId = string.Empty;
                var roleDefinition = string.Empty;
                var index = 1;
                if (!string.IsNullOrEmpty(newTDef.identifier))
                    templateId = newTDef.identifier;

                templateName = newTDef.name[0].value;

                var oldQmxf = new QMXF();
                if (!String.IsNullOrEmpty(templateId))
                {
                    oldQmxf = GetTemplate(templateId, QMXFType.Definition, repository);
                }
                else
                {
                    generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                    templateId = generatedId;
                }
                if (oldQmxf.templateDefinitions.Count > 0)
                {
                    foreach (var oldTDef in oldQmxf.templateDefinitions)
                    {
                        foreach (var newName in newTDef.name)
                        {
                            templateName = newName.value;
                            var oldName = oldTDef.name.Find(n => n.lang == newName.lang);
                            if (System.String.Compare(oldName.value, newName.value, System.StringComparison.OrdinalIgnoreCase) == 0) continue;
                            GenerateName(ref delete, oldName, templateId, oldTDef);
                            GenerateName(ref insert, newName, templateId, newTDef);
                        }
                        foreach (var newDescr in newTDef.description)
                        {
                            var oldDescr = oldTDef.description.Find(d => d.lang == newDescr.lang);
                            if (oldDescr != null && newDescr != null)
                            {
                                if (System.String.Compare(oldDescr.value, newDescr.value, System.StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    GenerateDescription(ref delete, oldDescr, templateId);
                                    GenerateDescription(ref insert, newDescr, templateId);
                                }
                            }
                            else if (newDescr != null && oldDescr == null)
                            {
                                GenerateDescription(ref insert, newDescr, templateId);
                            }
                        }

                        index = 1;

                        if (oldTDef.roleDefinition.Count < newTDef.roleDefinition.Count) ///Role(s) added
                        {
                            foreach (var nrd in newTDef.roleDefinition)
                            {
                                var roleName = nrd.name[0].value;
                                var newRoleID = nrd.identifier;
                                if (string.IsNullOrEmpty(newRoleID))
                                {
                                    generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                                    newRoleID = generatedId;
                                }
                                var ord = oldTDef.roleDefinition.Find(r => r.identifier == newRoleID);
                                if (ord == null)
                                {
                                    foreach (var name in nrd.name)
                                    {
                                        GenerateName(ref insert, name, newRoleID, nrd);
                                    }
                                    if (nrd.description != null)
                                    {
                                        GenerateDescription(ref insert, nrd.description, newRoleID);
                                    }
                                    GenerateTypesPart8(ref insert, newRoleID, templateId, nrd);
                                    GenerateRoleIndexPart8(ref insert, newRoleID, index, nrd);
                                }
                                if (nrd.range != null)
                                {
                                    GenerateRoleFillerType(ref insert, newRoleID, nrd.range);
                                }
                            }
                        }
                        else if (oldTDef.roleDefinition.Count > newTDef.roleDefinition.Count)
                        {
                            foreach (var ord in oldTDef.roleDefinition)
                            {
                                var nrd = newTDef.roleDefinition.Find(r => r.identifier == ord.identifier);
                                if (nrd == null)
                                {
                                    foreach (var name in ord.name)
                                    {
                                        GenerateName(ref delete, name, ord.identifier, ord);
                                    }
                                    if (ord.description != null)
                                    {
                                        GenerateDescription(ref delete, ord.description, ord.identifier);
                                    }
                                    GenerateTypesPart8(ref delete, ord.identifier, templateId, ord);
                                    GenerateRoleIndexPart8(ref delete, ord.identifier, index, ord);
                                }

                                if (ord.range != null)
                                {
                                    GenerateRoleFillerType(ref delete, ord.identifier, ord.range);
                                }
                            }
                        }
                    }
                    if (delete.IsEmpty && insert.IsEmpty)
                    {
                        var errMsg = "No changes made to template [" + templateName + "]";
                        var status = new Status();
                        response.Level = StatusLevel.Warning;
                        status.Messages.Add(errMsg);
                        response.Append(status);
                        continue;
                    }
                }

                if (insert.IsEmpty && delete.IsEmpty)
                {

                    GenerateTypesPart8(ref insert, templateId, null, newTDef);
                    GenerateRoleCountPart8(ref insert, newTDef.roleDefinition.Count, templateId, newTDef);

                    foreach (var name in newTDef.name)
                    {
                        GenerateName(ref insert, name, templateId, newTDef);
                    }

                    foreach (var descr in newTDef.description)
                    {
                        GenerateDescription(ref insert, descr, templateId);
                    }
                    foreach (var newRole in newTDef.roleDefinition)
                    {

                        string roleLabel = newRole.name.FirstOrDefault().value;
                        string newRoleID = string.Empty;
                        generatedId = string.Empty;
                        string genName = string.Empty;
                        string range = newRole.range;

                        genName = "Role definition " + roleLabel;
                        if (string.IsNullOrEmpty(newRole.identifier))
                        {
                            generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                            newRoleID = generatedId;
                        }
                        else
                        {
                            newRoleID = newRole.identifier;
                        }
                        foreach (var newName in newRole.name)
                        {
                            GenerateName(ref insert, newName, newRoleID, newRole);
                        }

                        if (newRole.description != null && newRole.description.value != null)
                        {
                            GenerateDescription(ref insert, newRole.description, newRoleID);
                        }
                        GenerateTypesPart8(ref insert, newRoleID, null, newRole);
                        GenerateRoleIndexPart8(ref insert, newRoleID, ++roleCount, newRole);
                        GenerateHasTemplate(ref insert, newRoleID, templateId, newRole);
                        GenerateHasRole(ref insert, templateId, newRoleID, newRole);

                        if (!string.IsNullOrEmpty(newRole.range))
                        {
                            GenerateRoleFillerType(ref insert, newRoleID, newRole.range);
                        }
                    }
                }

                if (!delete.IsEmpty)
                {
                    sparqlBuilder.AppendLine(deleteData);
                    foreach (Triple t in delete.Triples)
                    {
                        sparqlBuilder.AppendLine(t.ToString(formatter));
                    }
                    sparqlBuilder.AppendLine(insert.IsEmpty ? "}" : "};");
                }
                if (!insert.IsEmpty)
                {
                    sparqlBuilder.AppendLine(insertData);
                    foreach (var t in insert.Triples)
                    {
                        sparqlBuilder.AppendLine(t.ToString(formatter));
                    }
                    sparqlBuilder.AppendLine("}");
                }
                var sparql = sparqlBuilder.ToString();
                var postResponse = PostToRepository(repository, sparql);
                response.Append(postResponse);
            }
            return response;
        }


        public Response PostClass(QMXF qmxf)
        {
            var delete = new Graph();
            var insert = new Graph();
            delete.NamespaceMap.Import(nsMap);
            insert.NamespaceMap.Import(nsMap);
            var response = new Response();
            response.Level = StatusLevel.Success;
            try
            {
                var repository = GetRepository(qmxf.targetRepository);
                if (repository == null || repository.IsReadOnly)
                {
                    var status = new Status();
                    status.Level = StatusLevel.Error;
                    if (repository == null)
                        status.Messages.Add("Repository not found!");
                    else
                        status.Messages.Add("Repository [" + qmxf.targetRepository + "] is read-only!");
                    _response.Append(status);
                }
                else
                {
                    var registry = _useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["ClassRegistryBase"];
                    foreach (var newClsDef in qmxf.classDefinitions)
                    {
                        var language = string.Empty;
                        var clsId = newClsDef.identifier;
                        var oldQmxf = new QMXF();

                        if (!String.IsNullOrEmpty(clsId))
                        {
                            oldQmxf = GetClass(clsId, repository);
                        }
                        if (oldQmxf.classDefinitions.Count > 0)
                        {
                            foreach (var oldClsDef in oldQmxf.classDefinitions)
                            {
                                foreach (var nn in newClsDef.name)
                                {
                                    var on = oldClsDef.name.Find(n => n.lang == nn.lang);
                                    if (on != null)
                                    {
                                        if (System.String.Compare(@on.value, nn.value, System.StringComparison.Ordinal) != 0)
                                        {
                                            GenerateClassName(ref delete, on, clsId, oldClsDef);
                                            GenerateClassName(ref insert, nn, clsId, newClsDef);
                                        }
                                    }
                                    foreach (var nd in newClsDef.description)
                                    {
                                        var od = oldClsDef.description.Find(d => d.lang == nd.lang);
                                        if (od == null) continue;
                                        if (System.String.Compare(od.value, nd.value, System.StringComparison.Ordinal) == 0) continue;
                                        GenerateClassDescription(ref delete, od, clsId);
                                        GenerateClassDescription(ref insert, nd, clsId);
                                    }
                                    if (newClsDef.specialization.Count == oldClsDef.specialization.Count)
                                    {
                                        continue;
                                    }
                                    else if (newClsDef.specialization.Count < oldClsDef.specialization.Count)
                                    {
                                        foreach (var os in from os in oldClsDef.specialization let ns = newClsDef.specialization.Find(s => s.reference == os.reference) where ns == null select os)
                                        {
                                            GenerateRdfSubClass(ref delete, clsId, os.reference);
                                        }
                                    }
                                    else if (newClsDef.specialization.Count > oldClsDef.specialization.Count)
                                    {
                                        foreach (var ns in from ns in newClsDef.specialization let os = oldClsDef.specialization.Find(s => s.reference == ns.reference) where os == null select ns)
                                        {
                                            GenerateRdfSubClass(ref insert, clsId, ns.reference);
                                        }
                                    }
                                    if (newClsDef.classification.Count == oldClsDef.classification.Count)
                                    {
                                        continue;
                                    }
                                    else if (newClsDef.classification.Count < oldClsDef.classification.Count)
                                    {
                                        foreach (var oc in from oc in oldClsDef.classification let nc = newClsDef.classification.Find(c => c.reference == oc.reference) where nc == null select oc)
                                        {
                                            GenerateClassMember(ref delete, oc.reference, clsId);
                                            // GenerateSuperClass(ref delete, oc.reference, clsId); ///delete from old
                                        }
                                    }
                                    else if (newClsDef.classification.Count > oldClsDef.classification.Count)//some is added ... find added classifications
                                    {
                                        foreach (var nc in from nc in newClsDef.classification let oc = oldClsDef.classification.Find(c => c.reference == nc.reference) where oc == null select nc)
                                        {
                                            GenerateClassMember(ref insert, nc.reference, clsId);
                                            //GenerateSuperClass(ref insert, nc.reference, clsId); ///insert from new
                                        }
                                    }
                                }
                            }
                            if (delete.IsEmpty && insert.IsEmpty)
                            {
                                var errMsg = "No changes made to class [" + qmxf.classDefinitions[0].name[0].value + "]";
                                var status = new Status();
                                response.Level = StatusLevel.Warning;
                                status.Messages.Add(errMsg);
                                response.Append(status);
                                continue;
                            }
                        }
                        if (delete.IsEmpty && insert.IsEmpty)
                        {
                            var clsLabel = newClsDef.name[0].value;
                            if (string.IsNullOrEmpty(clsId))
                            {
                                var newClsName = "Class definition " + clsLabel;
                                clsId = CreateNewGuidId(registry);
                            }
                            if (newClsDef.entityType != null && !String.IsNullOrEmpty(newClsDef.entityType.reference))
                            {
                                GenerateTypesPart8(ref insert, clsId, newClsDef.entityType.reference, newClsDef);
                            }
                            foreach (var ns in newClsDef.specialization)
                            {
                                if (String.IsNullOrEmpty(ns.reference)) continue;
                                if (repository.RepositoryType == RepositoryType.Part8)
                                {
                                    GenerateRdfSubClass(ref insert, clsId, ns.reference);
                                }
                                else
                                {
                                    throw new Exception("Repository updates not supported for repository type[" + repository.RepositoryType + "]");
                                }
                            }
                            foreach (var nd in newClsDef.description.Where(nd => !String.IsNullOrEmpty(nd.value)))
                            {
                                GenerateClassDescription(ref insert, nd, clsId);
                            }
                            foreach (var nn in newClsDef.name)
                            {
                                GenerateClassName(ref insert, nn, clsId, newClsDef);
                            }
                            foreach (var nc in newClsDef.classification.Where(nc => !string.IsNullOrEmpty(nc.reference)))
                            {
                                if (repository.RepositoryType == RepositoryType.Part8)
                                {
                                    GenerateClassMember(ref insert, nc.reference, clsId);
                                    //GenerateSuperClass(ref insert, nc.reference, clsId);
                                }
                                else
                                {
                                    throw new Exception("Repository updates not supported for repository type[" + repository.RepositoryType + "]");
                                }
                            }
                        }
                        if (!delete.IsEmpty)
                        {
                            sparqlBuilder.AppendLine(deleteData);
                            foreach (var t in delete.Triples)
                            {
                                sparqlBuilder.AppendLine(t.ToString(formatter));
                            }
                            if (insert.IsEmpty)
                                sparqlBuilder.AppendLine("}");
                            else
                                sparqlBuilder.AppendLine("};");
                        }
                        if (!insert.IsEmpty)
                        {
                            sparqlBuilder.AppendLine(insertData);
                            foreach (var t in insert.Triples)
                            {
                                sparqlBuilder.AppendLine(t.ToString(formatter));
                            }
                            sparqlBuilder.AppendLine("}");
                        }
                        var sparql = sparqlBuilder.ToString();
                        var postResponse = PostToRepository(repository, sparql);
                        response.Append(postResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                var errMsg = "Error in PostClass: " + ex;
                var status = new Status();
                response.Level = StatusLevel.Error;
                status.Messages.Add(errMsg);
                response.Append(status);
                _logger.Error(errMsg);
            }
            return response;
        }

        public Response ClearAll(QMXF qmxf)
        {
            var response = new Response();
            response.Level = StatusLevel.Success;
            try
            {
                var repository = GetRepository(qmxf.targetRepository);
                if (repository == null || repository.IsReadOnly)
                {
                    var status = new Status();
                    status.Level = StatusLevel.Error;
                    if (repository == null)
                        status.Messages.Add("Repository not found!");
                    else
                        status.Messages.Add("Repository [" + qmxf.targetRepository + "] is read-only!");
                    response.Append(status);
                }
                else
                {
                    sparqlBuilder.AppendLine("CLEAR  ALL");
                    var sparql = sparqlBuilder.ToString();
                    var postResponse = PostToRepository(repository, sparql);
                    response.Append(postResponse);
                }
            }
            catch (Exception ex)
            {
                var errMsg = "Error in PostClass: " + ex;
                var status = new Status();
                response.Level = StatusLevel.Error;
                status.Messages.Add(errMsg);
                response.Append(status);
                _logger.Error(errMsg);
            }
            return response;
        }


        public List<Entity> Find(string queryString)
        {
            List<Entity> queryResult = new List<Entity>();
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Query queryExactSearch = (Query)_queries.FirstOrDefault(c => c.Key == "ExactSearch").Query;

                sparql = ReadSparql(queryExactSearch.FileName);

                sparql = sparql.Replace("param1", queryString);

                foreach (Repository repository in _repositories)
                {
                    SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

                    foreach (SparqlResult result in sparqlResults)
                    {
                        Entity resultEntity = new Entity();
                        foreach (var v in result.Variables)
                        {
                            if (v.Equals("uri"))
                            {
                                resultEntity.Uri = result[v].ToString();
                            }
                            else if (v.Equals("label") && result.HasValue("label"))
                            {
                                resultEntity.Label = ((ILiteralNode)result[v]).Value;
                                resultEntity.Lang = ((ILiteralNode)result[v]).Language;
                            }
                        }

                        resultEntity.Repository = repository.Name;


                        queryResult.Add(resultEntity);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in Find: " + e);
                throw new Exception("Error while Finding " + queryString + ".\n" + e.ToString(), e);
            }
            return queryResult;
        }

        public VersionInfo GetVersion()
        {
            Version version = this.GetType().Assembly.GetName().Version;

            return new VersionInfo()
            {
                Major = version.Major,
                Minor = version.Minor,
                Build = version.Build,
                Revision = version.Revision
            };
        }

        private void GenerateValue(ref Graph work, string subjId, string objId, object gobj)
        {
            RoleQualification role = (RoleQualification)gobj;
            _pred = work.CreateUriNode("tpl:R56456315674");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
            _pred = work.CreateUriNode("tpl:R89867215482");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
            _pred = work.CreateUriNode("tpl:R29577887690");
            _obj = work.CreateLiteralNode(role.value.text, string.IsNullOrEmpty(role.value.lang) ? defaultLanguage : role.value.lang);
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateReferenceQual(ref Graph work, string subjId, string objId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("tpl:R30741601855");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateReferenceType(ref Graph work, string subjId, string objId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode(rdfType);
            _obj = work.CreateUriNode("tpl:R40103148466");
            work.Assert(new Triple(_subj, _pred, _obj));
            _pred = work.CreateUriNode("tpl:R49267603385");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateReferenceTpl(ref Graph work, string subjId, string objId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("tpl:R21129944603");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateQualifies(ref Graph work, string subjId, string objId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("tpl:R91125890543");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateRange(ref Graph work, string subjId, string objId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("rdfs:range");
            _obj = work.CreateUriNode(objId);
            work.Assert(new Triple(_subj, _pred, _obj));
            _pred = work.CreateUriNode("tpl:R98983340497");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateHasRole(ref Graph work, string subjId, string objId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("p8:hasRole");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateHasTemplate(ref Graph work, string subjId, string objId, object gobj)
        {
            if (gobj is RoleDefinition || gobj is RoleQualification)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode("p8:hasTemplate");
                var uri = new Uri(objId);
                _obj = work.CreateUriNode(uri);
                var triple = new Triple(_subj, _pred, _obj);
                Console.WriteLine(triple.ToString());
                work.Assert(triple);
            }
        }

        private void GenerateRoleIndex(ref Graph work, string subjId, int index)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("tpl:R97483568938");
            _obj = work.CreateLiteralNode(index.ToString(), new Uri("xsd:integer"));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateRoleIndexPart8(ref Graph work, string subjId, int index, object gobj)
        {
            if (gobj is RoleDefinition || gobj is RoleQualification)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode("p8:valRoleIndex");
                _obj = work.CreateLiteralNode(index.ToString(), new Uri("xsd:integer"));
                work.Assert(new Triple(_subj, _pred, _obj));
            }
        }

        private void GenerateRoleDomain(ref Graph work, string subjId, string objId)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("rdfs:domain");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateRoleFillerType(ref Graph work, string subjId, string range)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("p8:hasRoleFillerType");
            _obj = work.CreateUriNode(new Uri(range));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateValRoleFiller(ref Graph work, string subjId, string value)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("p8:valRoleFiller");
            _obj = work.CreateUriNode(new Uri(value));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateRoleCount(ref Graph work, int rolecount, string subjId, object gobj)
        {
            if (gobj is TemplateDefinition || gobj is TemplateQualification)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode("tpl:R35529169909");
                _obj = work.CreateLiteralNode(Convert.ToString(rolecount), new Uri("xsd:integer"));
                work.Assert(new Triple(_subj, _pred, _obj));
            }
        }

        private void GenerateRoleCountPart8(ref Graph work, int rolecount, string subjId, object gobj)
        {
            if (gobj is TemplateDefinition || gobj is TemplateQualification)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode("p8:valNumberOfRoles");
                _obj = work.CreateLiteralNode(Convert.ToString(rolecount), new Uri("xsd:integer"));
                work.Assert(new Triple(_subj, _pred, _obj));
            }

        }

        private void GenerateTypesPart8(ref Graph work, string subjId, string objectId, object gobj)
        {
            if (gobj is TemplateDefinition)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode("owl:Thing");
                //obj = work.CreateUriNode("p8:TemplateDescription");
                work.Assert(new Triple(_subj, _pred, _obj));
                _pred = work.CreateUriNode(rdfssubClassOf);
                _obj = work.CreateUriNode("p8:BaseTemplateStatement");
                work.Assert(new Triple(_subj, _pred, _obj));
            }
            else if (gobj is RoleQualification)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode("owl:Thing");
                work.Assert(new Triple(_subj, _pred, _obj));
                _obj = work.CreateUriNode("p8:TemplateRoleDescription");
                work.Assert(new Triple(_subj, _pred, _obj));
                //_pred = work.CreateUriNode("p8:hasRoleFillerType");
                //_obj = work.CreateUriNode(new Uri(((RoleQualification)gobj).range));
                //work.Assert(new Triple(_subj, _pred, _obj));
            }
            else if (gobj is RoleDefinition)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode("owl:Thing");
                work.Assert(new Triple(_subj, _pred, _obj));
                _obj = work.CreateUriNode("p8:TemplateRoleDescription");
                work.Assert(new Triple(_subj, _pred, _obj));
                _pred = work.CreateUriNode("p8:hasRoleFillerType");
                _obj = work.CreateUriNode(new Uri(((RoleDefinition)gobj).range));
                work.Assert(new Triple(_subj, _pred, _obj));
            }
            else if (gobj is TemplateQualification)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                //obj = work.CreateUriNode("p8:TemplateDescription");
                //work.Assert(new Triple(subj, pred, obj));
                _obj = work.CreateUriNode("owl:Thing");
                work.Assert(new Triple(_subj, _pred, _obj));
                _pred = work.CreateUriNode(rdfssubClassOf);
                _obj = work.CreateUriNode("p8:SpecializedTemplateStatement");
                work.Assert(new Triple(_subj, _pred, _obj));
                _pred = work.CreateUriNode(rdfssubClassOf);
                _obj = work.CreateUriNode(new Uri(objectId));
                work.Assert(new Triple(_subj, _pred, _obj));
            }
            else if (gobj is ClassDefinition)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode(new Uri(objectId));
                work.Assert(new Triple(_subj, _pred, _obj));
                //_pred = work.CreateUriNode(rdfType);
                //_obj = work.CreateUriNode("owl:Class");
                //work.Assert(new Triple(_subj, _pred, _obj));
            }
        }

        private void GenerateTypes(ref Graph work, string subjId, string objId, object gobj)
        {
            if (gobj is TemplateDefinition)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode("tpl:R16376066707");
                work.Assert(new Triple(_subj, _pred, _obj));
            }
            else if (gobj is RoleDefinition)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode("tpl:R74478971040");
                work.Assert(new Triple(_subj, _pred, _obj));
            }
            else if (gobj is TemplateQualification)
            {
                _subj = work.CreateUriNode(new Uri(objId));
                _pred = work.CreateUriNode("dm:hasSubclass");
                _obj = work.CreateUriNode(new Uri(subjId));
                work.Assert(new Triple(_subj, _pred, _obj));
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode("dm:hasSuperclass");
                _obj = work.CreateUriNode(new Uri(objId));
                work.Assert(new Triple(_subj, _pred, _obj));
            }
            else if (gobj is RoleQualification)
            {
                _subj = work.CreateUriNode(new Uri(subjId));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode("tpl:R76288246068");
                work.Assert(new Triple(_subj, _pred, _obj));
                _pred = work.CreateUriNode("tpl:R99672026745");
                _obj = work.CreateUriNode(new Uri(objId));
                work.Assert(new Triple(_subj, _pred, _obj));
                _pred = work.CreateUriNode(rdfType);
                _obj = work.CreateUriNode("tpl:R67036823327");
                work.Assert(new Triple(_subj, _pred, _obj));
            }
        }

        private void GenerateName(ref Graph work, QMXFName name, string subjId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("rdfs:label");
            _obj = work.CreateLiteralNode(name.value, string.IsNullOrEmpty(name.lang) ? defaultLanguage : name.lang);
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateClassName(ref Graph work, QMXFName name, string subjId, object gobj)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("rdfs:label");
            _obj = work.CreateLiteralNode(name.value, string.IsNullOrEmpty(name.lang) ? defaultLanguage : name.lang);
            work.Assert(new Triple(_subj, _pred, _obj));
        }
        private void GenerateDescription(ref Graph work, Description descr, string subjectId)
        {
            _subj = work.CreateUriNode(new Uri(subjectId));
            _pred = work.CreateUriNode("rdfs:comment");
            _obj = work.CreateLiteralNode(descr.value, string.IsNullOrEmpty(descr.lang) ? defaultLanguage : descr.lang);
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateClassDescription(ref Graph work, Description descr, string subjectId)
        {
            _subj = work.CreateUriNode(new Uri(subjectId));
            _pred = work.CreateUriNode("rdfs:comment");
            _obj = work.CreateLiteralNode(descr.value, string.IsNullOrEmpty(descr.lang) ? defaultLanguage : descr.lang);
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateClassMember(ref Graph work, string subjId, string clsId)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode(rdfType);
            _obj = work.CreateUriNode(new Uri(clsId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateSuperClass(ref Graph work, string subjId, string objId)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("rdfs:subClassOf");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateRdfSubClass(ref Graph work, string subjId, string objId)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("rdfs:subClassOf");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateDmClassification(ref Graph work, string subjId, string objId)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("dm:hasClassified");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
            _pred = work.CreateUriNode("dm:hasClassifier");
            work.Assert(new Triple(_subj, _pred, _obj));
        }

        private void GenerateDmSubClass(ref Graph work, string subjId, string objId)
        {
            _subj = work.CreateUriNode(new Uri(subjId));
            _pred = work.CreateUriNode("dm:hasSubclass");
            _obj = work.CreateUriNode(new Uri(objId));
            work.Assert(new Triple(_subj, _pred, _obj));
        }

    }
}