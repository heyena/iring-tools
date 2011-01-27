// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
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


namespace org.iringtools.refdata
{
    // NOTE: If you change the class name "Service" here, you must also update the reference to "Service" in Web.config and in the associated .svc file.
    public class ReferenceDataProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ReferenceDataProvider));

        private Response _response = null;

        private const string REPOSITORIES_FILE_NAME = "Repositories.xml";
        private const string QUERIES_FILE_NAME = "Queries.xml";

        //private const string rdlUri = "http://rdl.rdlfacade.org/data#";
        //private const string tplUri = "http://tpl.rdlfacade.org/data#";

        //private const string egPrefix = "PREFIX eg: <http://example.org/data#>";
        //private const string rdlPrefix = "PREFIX rdl: <http://rdl.rdlfacade.org/data#>";
        //private const string tplPrefix = "PREFIX tpl: <http://tpl.rdlfacade.org/data#>";
        //private const string rdfPrefix = "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>";
        //private const string rdfsPrefix = "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>";
        //private const string xsdPrefix = "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>";
        //private const string dmPrefix = "PREFIX dm: <http://dm.rdlfacade.org/data#>";
        //private const string p8dm = "PREFIX p8dm: <http://standards.tc184-sc4.org/iso/15926/-8/data-model#>";
        //private const string owlPrefix = "PREFIX owl: <http://www.w3.org/2002/07/owl#>";
        //private const string owl2xmlPrefix = "PREFIX owl2xml: <http://www.w3.org/2006/12/owl2-xml#>";
        //private const string p8Prefix = "PREFIX p8: <http://standards.tc184-sc4.org/iso/15926/-8/template-model#>";
        //private const string templatesPrefix = "PREFIX templates: <http://standards.tc184-sc4.org/iso/15926/-8/templates#>";
        private NamespaceMapper nsMap = new NamespaceMapper();
        private const string insertData = "INSERT DATA {";
        private const string deleteData = "DELETE DATA {";
        private const string deleteWhere = "DELETE WHERE {";

        private int _pageSize = 0;

        private bool _useExampleRegistryBase = false;
        private WebCredentials _registryCredentials = null;
        private WebProxyCredentials _proxyCredentials = null;
        private Repositories _repositories = null;
        private Queries _queries = null;
        private static Dictionary<string, RefDataEntities> _searchHistory = new Dictionary<string, RefDataEntities>();
        private IKernel _kernel = null;
        private ReferenceDataSettings _settings = null;

        private StringBuilder prefix = new StringBuilder();
        private StringBuilder sparqlBuilder = new StringBuilder();
        private StringBuilder sparqlStr = null;
        private string defaultLanguage = "en";

        public ReferenceDataProvider(NameValueCollection settings)
        {
            try
            {
                _kernel = new StandardKernel(new ReferenceDataModule());
                _settings = _kernel.Get<ReferenceDataSettings>();
                _settings.AppendSettings(settings);
                Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);
                _pageSize = Convert.ToInt32(_settings["PageSize"]);
                _useExampleRegistryBase = Convert.ToBoolean(_settings["UseExampleRegistryBase"]);
                string registryCredentialToken = _settings["RegistryCredentialToken"];
                bool tokenIsEmpty = registryCredentialToken == String.Empty;
                if (tokenIsEmpty)
                {
                    _registryCredentials = new WebCredentials();
                }
                else
                {
                    _registryCredentials = new WebCredentials(registryCredentialToken);
                    _registryCredentials.Decrypt();
                }
                _proxyCredentials = _settings.GetWebProxyCredentials();
                string queriesPath = _settings["XmlPath"] + QUERIES_FILE_NAME;
                _queries = Utility.Read<Queries>(queriesPath);
                string repositoriesPath = _settings["XmlPath"] + REPOSITORIES_FILE_NAME;
                _repositories = Utility.Read<Repositories>(repositoriesPath);
                _response = new Response();
                _kernel.Bind<Response>().ToConstant(_response);

                prefix.Append(nsMap.PrefixString()); 
                
            }
            catch (Exception ex)
            {
                _logger.Error("Error in initializing ReferenceDataServiceProvider: " + ex);
            }
        }

        public Repositories GetRepositories()
        {
            try
            {
                Repositories repositories;

                repositories = _repositories;

                //Don't Expose Tokens
                foreach (Repository repository in repositories)
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
            foreach (Repository repository in _repositories)
            {
                if (repository.Name.Equals(name))
                {
                    return repository;
                }
            }

            return null;
        }

        #region Prototype Part8

        public RefDataEntities Search(string query)
        {
            try
            {
                return SearchPage(query, 0, 0);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Search: " + ex);
                throw new Exception("Error while Searching " + query + ".\n" + ex.ToString(), ex);
            }
        }

        public RefDataEntities SearchPage(string query, int start, int limit)
        {
            RefDataEntities entities = null;
            int counter = 0;

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                //Check the search History for Optimization
                if (_searchHistory.ContainsKey(query))
                {
                    entities = _searchHistory[query];
                }
                else
                {
                    RefDataEntities resultEntities = new RefDataEntities();

                    Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "ContainsSearch").Query;
                    QueryBindings queryBindings = queryContainsSearch.Bindings;

                    sparql = ReadSPARQL(queryContainsSearch.FileName);
                    sparql = sparql.Replace("param1", query);

                    foreach (Repository repository in _repositories)
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
                        foreach (Dictionary<string, string> result in results)
                        {
                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = result["label"],
                                Repository = repository.Name
                            };

                            string key = resultEntity.Label;

                            if (resultEntities.Entities.ContainsKey(key))
                            {
                                key += ++counter;
                            }

                            resultEntities.Entities.Add(key, resultEntity);
                        }
                        results.Clear();
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

        public RefDataEntities SearchReset(string query)
        {
            Reset(query);

            return Search(query);
        }

        public RefDataEntities SearchPageReset(string query, int start, int limit)
        {
            Reset(query);

            return SearchPage(query, start, limit);
        }

        //private string GetLabel(string uri)
        //{
        //  try
        //  {
        //    string label = String.Empty;
        //    string sparql = String.Empty;
        //    string relativeUri = String.Empty;

        //    Query query = (Query)_queries.FirstOrDefault(c => c.Key == "GetLabel").Query;
        //    QueryBindings queryBindings = query.Bindings;

        //    sparql = ReadSPARQL(query.FileName);
        //    sparql = sparql.Replace("param1", uri);

        //    foreach (Repository repository in _repositories)
        //    {
        //      SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

        //      List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

        //      foreach (Dictionary<string, string> result in results)
        //      {
        //        if (result.ContainsKey("label"))
        //        {
        //          label = result["label"];
        //        }
        //      }
        //    }

        //    return label;
        //  }
        //  catch (Exception e)
        //  {
        //    _logger.Error("Error in GetLabel: " + e);
        //    throw new Exception("Error while Getting Label for " + uri + ".\n" + e.ToString(), e);
        //  }
        //}

        private Entity GetLabel(string uri)
        {
            Entity labelEntity = new Entity();

            try
            {
                string label = String.Empty;
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Query query = (Query)_queries.FirstOrDefault(c => c.Key == "GetLabel").Query;
                QueryBindings queryBindings = query.Bindings;

                sparql = ReadSPARQL(query.FileName);
                sparql = sparql.Replace("param1", uri);

                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {
                        if (result.ContainsKey("label"))
                        {
                            labelEntity.Label = result["label"];
                            labelEntity.Repository = repository.Name;
                            labelEntity.Uri = repository.Uri;
                            break;
                        }
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
            QMXF qmxf = new QMXF();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                List<Classification> classifications = new List<Classification>();


                Query getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassification").Query;
                QueryBindings queryBindings = getClassification.Bindings;

                sparql = ReadSPARQL(getClassification.FileName);
                sparql = sparql.Replace("param1", id);

                if (rep == null)
                {
                    foreach (Repository repository in _repositories)
                    {
                        classifications = ProcessClassifications(repository, sparql, queryBindings);
                    }
                }
                else
                {
                    classifications = ProcessClassifications(rep, sparql, queryBindings);

                }

                return classifications;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetClassifications: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        private List<Classification> ProcessClassifications(Repository repository, string sparql, QueryBindings queryBindings)
        {
            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);
            List<Classification> classifications = new List<Classification>();
            List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

            foreach (Dictionary<string, string> result in results)
            {

                Classification classification = new Classification();
                string uri = String.Empty;
                string label = String.Empty;

                if (result.ContainsKey("uri"))
                {
                    uri = result["uri"];
                    classification.reference = uri;
                }

                if (result.ContainsKey("label"))
                    label = result["label"];
                else
                    label = GetLabel(uri).Label;

                classification.label = label;
                Utility.SearchAndInsert(classifications, classification, Classification.sortAscending());
            }

            return classifications;
        }

        private List<Specialization> GetSpecializations(string id, Repository rep)
        {
            try
            {
                string sparql = String.Empty;
                string sparqlPart8 = String.Empty;
                string relativeUri = String.Empty;

                List<Specialization> specializations = new List<Specialization>();

                Query queryGetSpecialization = (Query)_queries.FirstOrDefault(c => c.Key == "GetSpecialization").Query;
                QueryBindings queryBindings = queryGetSpecialization.Bindings;

                sparql = ReadSPARQL(queryGetSpecialization.FileName);
                sparql = sparql.Replace("param1", id);

                Query queryGetSubClassOf = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOf").Query;
                QueryBindings queryBindingsPart8 = queryGetSubClassOf.Bindings;

                sparqlPart8 = ReadSPARQL(queryGetSubClassOf.FileName);
                sparqlPart8 = sparqlPart8.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlPart8);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindingsPart8, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            Specialization specialization = new Specialization();

                            string uri = String.Empty;
                            string label = String.Empty;
                            if (result.ContainsKey("uri"))
                            {
                                uri = result["uri"];
                                specialization.reference = uri;
                            }
                            if (result.ContainsKey("label"))
                            {
                                label = result["label"];
                            }
                            else
                            {
                                label = GetLabel(uri).Label;
                            }

                            specialization.label = label;
                            Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
                            //specializations.Add(specialization); 
                        }
                    }
                    else
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            Specialization specialization = new Specialization();

                            string uri = String.Empty;
                            string label = String.Empty;
                            if (result.ContainsKey("uri"))
                            {
                                uri = result["uri"];
                                specialization.reference = uri;
                            }
                            if (result.ContainsKey("label"))
                            {
                                label = result["label"];
                            }
                            else
                            {
                                label = GetLabel(uri).Label;
                            }

                            specialization.label = label;
                            Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
                            //specializations.Add(specialization); 
                        }
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
            return GetLabel(nsMap.GetNamespaceUri("rdl").ToString() + id);
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
            QMXF qmxf = new QMXF();

            try
            {
                ClassDefinition classDefinition = null;
                QMXFName name;
                Description description;
                QMXFStatus status;
                List<string> names = new List<string>();

                List<Classification> classifications = new List<Classification>();
                List<Specialization> specializations = new List<Specialization>();

                RefDataEntities resultEntities = new RefDataEntities();
                List<Entity> resultEnt = new List<Entity>();
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetClass").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);

                if (namespaceUrl == String.Empty || namespaceUrl == null)
                    namespaceUrl = nsMap.GetNamespaceUri("rdl").ToString();

                string uri = namespaceUrl + id;

                sparql = sparql.Replace("param1", uri);
                foreach (Repository repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
                    classifications = new List<Classification>();
                    specializations = new List<Specialization>();

                    foreach (Dictionary<string, string> result in results)
                    {
                        classDefinition = new ClassDefinition();


                        classDefinition.identifier = uri;
                        classDefinition.repositoryName = repository.Name;
                        name = new QMXFName();
                        description = new Description();
                        status = new QMXFStatus();

                        if (result.ContainsKey("label"))
                        {
                            names = result["label"].Split('@').ToList();
                            name.value = names[0];
                            if (names.Count == 1)
                            {
                                name.lang = defaultLanguage;
                            }
                            else if (names.Count == 2)
                                name.lang = names[1];
                        }
                        if (result.ContainsKey("type"))
                            classDefinition.entityType = new EntityType { reference = result["type"] };

                        //legacy properties
                        if (result.ContainsKey("definition"))
                        {
                            names = result["definition"].Split('@').ToList();
                            description.value = names[0];
                            if (names.Count == 1)
                            {
                                description.lang = defaultLanguage;
                            }
                            else if (names.Count == 2)
                                description.lang = names[1];
                        }
                           // description.value = result["definition"];

                        if (result.ContainsKey("creator"))
                            status.authority = result["creator"];

                        if (result.ContainsKey("creationDate"))
                            status.from = result["creationDate"];

                        if (result.ContainsKey("class"))
                            status.Class = result["class"];

                        //camelot properties
                        if (result.ContainsKey("comment"))
                        {
                            names = result["comment"].Split('@').ToList();
                            description.value = names[0];
                            if (names.Count == 1)
                            {
                                description.lang = defaultLanguage;
                            }
                            else if (names.Count == 2)
                                description.lang = names[1];
                        }
                        if (result.ContainsKey("authority"))
                            status.authority = result["authority"];

                        if (result.ContainsKey("recorded"))
                            status.Class = result["recorded"];

                        if (result.ContainsKey("from"))
                            status.from = result["from"];
                        
                        classDefinition.name.Add(name);
                        
                        classDefinition.description.Add(description);
                        classDefinition.status.Add(status);

                        classifications = GetClassifications(id, repository);
                        specializations = GetSpecializations(id, repository);
                        classDefinition.classification = classifications;
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

        public Entities GetSuperClasses(string id)
        {
            Entities queryResult = new Entities();

            try
            {
                List<Specialization> specializations = GetSpecializations(id, null);

                foreach (Specialization specialization in specializations)
                {
                    string uri = specialization.reference;
                    string label = specialization.label;

                    if (label == null)
                        label = GetLabel(uri).Label;

                    Entity resultEntity = new Entity
                    {
                        Uri = uri,
                        Label = label
                    };
                    Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                    //queryResult.Add(resultEntity);
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
            Entities list = new Entities();
            return GetAllSuperClasses(id, list);
        }

        public Entities GetAllSuperClasses(string id, Entities list)
        {
            //List<Entity> queryResult = new List<Entity>();

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
                    string uri = specialization.reference;
                    string label = specialization.label;

                    if (label == null)
                        label = GetLabel(uri).Label;

                    Entity resultEntity = new Entity
                    {
                        Uri = uri,
                        Label = label
                    };

                    string trimmedUri = string.Empty;
                    bool found = false;
                    foreach (Entity entt in list)
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

        public Entities GetSubClasses(string id)
        {
            Entities queryResult = new Entities();

            try
            {
                string sparql = String.Empty;
                string sparqlPart8 = String.Empty;
                string relativeUri = String.Empty;

                Query queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClasses").Query;
                QueryBindings queryBindings = queryGetSubClasses.Bindings;

                sparql = ReadSPARQL(queryGetSubClasses.FileName);
                sparql = sparql.Replace("param1", id);

                Query queryGetSubClassOfInverse = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOfInverse").Query;
                QueryBindings queryBindingsPart8 = queryGetSubClassOfInverse.Bindings;

                sparqlPart8 = ReadSPARQL(queryGetSubClassOfInverse.FileName);
                sparqlPart8 = sparqlPart8.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlPart8);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindingsPart8, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = result["label"],
                            };
                            Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                            //queryResult.Add(resultEntity);
                        }
                    }
                    else
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = result["label"],
                            };
                            Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                            //queryResult.Add(resultEntity);
                        }
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

        public Entities GetClassTemplates(string id)
        {
            Entities queryResult = new Entities();
            try
            {
                string sparqlGetClassTemplates = String.Empty;
                string sparqlGetRelatedTemplates = String.Empty;
                string relativeUri = String.Empty;

                Query queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplates").Query;
                QueryBindings queryBindingsGetClassTemplates = queryGetClassTemplates.Bindings;

                sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.FileName);
                sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

                Query queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplates").Query;
                QueryBindings queryBindingsGetRelatedTemplates = queryGetRelatedTemplates.Bindings;

                sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.FileName);
                sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindingsGetRelatedTemplates, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = result["label"],
                                Repository = repository.Name,
                            };
                            Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                            //queryResult.Add(resultEntity);                        
                        }
                    }
                    else
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindingsGetClassTemplates, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = result["label"],
                                Repository = repository.Name,
                            };
                            Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                            //queryResult.Add(resultEntity);
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

        private List<RoleDefinition> GetRoleDefinition(string id, Repository repository)
        {
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;
                string sparqlQuery = string.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();

                List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();
                RefDataEntities resultEntities = new RefDataEntities();

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


                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                foreach (Dictionary<string, string> result in results)
                {

                    RoleDefinition roleDefinition = new RoleDefinition();
                    QMXFName name = new QMXFName();

                    if (result.ContainsKey("label"))
                    {
                        name.value = result["label"];
                    }
                    if (result.ContainsKey("role"))
                    {
                        roleDefinition.identifier = result["role"];
                    }
                    if (result.ContainsKey("comment"))
                    {
                        roleDefinition.description.value = result["comment"];
                    }
                    if (result.ContainsKey("index"))
                    {
                        roleDefinition.description.value = result["index"].ToString();
                    }
                    if (result.ContainsKey("type"))
                    {
                        roleDefinition.range = result["type"];
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
                string sparql = String.Empty;
                string relativeUri = String.Empty;
                string sparqlQuery = string.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();
                //List<Classification> classifications = new List<Classification>();

                List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();

                RefDataEntities resultEntities = new RefDataEntities();

                //Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetRoles").Query;
                //QueryBindings queryBindings = queryContainsSearch.Bindings;

                //sparql = ReadSPARQL(queryContainsSearch.FileName);
                //sparql = sparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
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
                    Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;
                    QueryBindings queryBindings = queryContainsSearch.Bindings;

                    sparql = ReadSPARQL(queryContainsSearch.FileName);
                    sparql = sparql.Replace("param1", id);
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {

                        RoleDefinition roleDefinition = new RoleDefinition();
                        QMXFName name = new QMXFName();

                        if (result.ContainsKey("label"))
                        {
                            name.value = result["label"];
                        }
                        if (result.ContainsKey("role"))
                        {
                            roleDefinition.identifier = result["role"];
                        }
                        if (result.ContainsKey("comment"))
                        {
                            roleDefinition.description.value = result["comment"];
                        }
                        if (result.ContainsKey("index"))
                        {
                            roleDefinition.description.value = result["index"].ToString();
                        }
                        if (result.ContainsKey("type"))
                        {
                            roleDefinition.range = result["type"];
                        }
                        roleDefinition.name.Add(name);
                        Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
                        //roleDefinitions.Add(roleDefinition);
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
                string rangeSparql = String.Empty;
                string relativeUri = String.Empty;

                string referenceSparql = String.Empty;
                string relativeUri1 = String.Empty;

                string valueSparql = String.Empty;
                string relativeUri2 = String.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();
                //List<Classification> classifications = new List<Classification>();

                List<RoleQualification> roleQualifications = new List<RoleQualification>();

                RefDataEntities rangeResultEntities = new RefDataEntities();
                RefDataEntities referenceResultEntities = new RefDataEntities();
                RefDataEntities valueResultEntities = new RefDataEntities();

                Query getRangeRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetRangeRestriction").Query;
                QueryBindings rangeRestrictionBindings = getRangeRestriction.Bindings;

                Query getReferenceRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetReferenceRestriction").Query;
                QueryBindings referenceRestrictionBindings = getReferenceRestriction.Bindings;

                Query getValueRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetValueRestriction").Query;
                QueryBindings valueRestrictionBindings = getValueRestriction.Bindings;

                rangeSparql = ReadSPARQL(getRangeRestriction.FileName);
                rangeSparql = rangeSparql.Replace("param1", id);

                referenceSparql = ReadSPARQL(getReferenceRestriction.FileName);
                referenceSparql = referenceSparql.Replace("param1", id);

                valueSparql = ReadSPARQL(getValueRestriction.FileName);
                valueSparql = valueSparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    SPARQLResults rangeSparqlResults = QueryFromRepository(repository, rangeSparql);
                    SPARQLResults referenceSparqlResults = QueryFromRepository(repository, referenceSparql);
                    SPARQLResults valueSparqlResults = QueryFromRepository(repository, valueSparql);

                    List<Dictionary<string, string>> rangeBindingResults = BindQueryResults(rangeRestrictionBindings, rangeSparqlResults);
                    List<Dictionary<string, string>> referenceBindingResults = BindQueryResults(referenceRestrictionBindings, referenceSparqlResults);
                    List<Dictionary<string, string>> valueBindingResults = BindQueryResults(valueRestrictionBindings, valueSparqlResults);

                    List<Dictionary<string, string>> combinedResults = MergeLists(MergeLists(rangeBindingResults, referenceBindingResults), valueBindingResults);

                    foreach (Dictionary<string, string> combinedResult in combinedResults)
                    {

                        RoleQualification roleQualification = new RoleQualification();



                        string uri = String.Empty;
                        if (combinedResult.ContainsKey("qualifies"))
                        {
                            uri = combinedResult["qualifies"];
                            roleQualification.qualifies = uri;
                            roleQualification.identifier = Utility.GetIdFromURI(uri);
                        }
                        if (combinedResult.ContainsKey("name"))
                        {
                            string nameValue = combinedResult["name"];

                            if (nameValue == null)
                            {
                                nameValue = GetLabel(uri).Label;
                            }

                            QMXFName name = new QMXFName
                            {
                                value = nameValue
                            };

                            roleQualification.name.Add(name);
                        }
                        else
                        {
                            string nameValue = GetLabel(uri).Label;

                            if (nameValue == String.Empty)
                                nameValue = "tpl:" + Utility.GetIdFromURI(uri);

                            QMXFName name = new QMXFName
                            {
                                value = nameValue
                            };

                            roleQualification.name.Add(name);
                        }
                        if (combinedResult.ContainsKey("range"))
                        {
                            roleQualification.range = combinedResult["range"];
                        }
                        if (combinedResult.ContainsKey("reference"))
                        {
                            QMXFValue value = new QMXFValue
                            {
                                reference = combinedResult["reference"]
                            };

                            roleQualification.value = value;
                        }
                        if (combinedResult.ContainsKey("value"))
                        {
                            QMXFValue value = new QMXFValue
                            {
                                text = combinedResult["value"],
                                As = combinedResult["value_dataType"]
                            };

                            roleQualification.value = value;
                        }
                        Utility.SearchAndInsert(roleQualifications, roleQualification, RoleQualification.sortAscending());
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
            List<TemplateDefinition> templateDefinitionList = new List<TemplateDefinition>();
            TemplateDefinition templateDefinition = null;

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();

                RefDataEntities resultEntities = new RefDataEntities();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetTemplate").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);


                foreach (Repository repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {
                        templateDefinition = new TemplateDefinition();
                        QMXFName name = new QMXFName();

                        templateDefinition.repositoryName = repository.Name;

                        if (result.ContainsKey("label"))
                        {
                            name.value = result["label"];
                        }

                        if (result.ContainsKey("definition"))
                        {
                            description.value = result["definition"];
                        }

                        if (result.ContainsKey("creationDate"))
                        {
                            status.from = result["creationDate"];
                        }
                        templateDefinition.identifier = "tpl:" + id;
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
            QMXF qmxf = new QMXF();
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
            QMXF qmxf = new QMXF();

            try
            {
                List<TemplateQualification> templateQualification = GetTemplateQualification(id, null);

                if (templateQualification.Count > 0)
                {
                    qmxf.templateQualifications = templateQualification;
                }
                else
                {
                    List<TemplateDefinition> templateDefinition = GetTemplateDefinition(id, null);
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
            List<TemplateQualification> templateQualificationList = new List<TemplateQualification>();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;
                string sparqlQuery = string.Empty;

                RefDataEntities resultEntities = new RefDataEntities();

                Query getTemplateQualification = null;
                QueryBindings queryBindings = null;

                {
                    foreach (Repository repository in _repositories)
                    {
                        if (rep != null)
                            if (rep.Name != repository.Name) continue;

                        switch (repository.RepositoryType)
                        {
                            case RepositoryType.Camelot:
                            case RepositoryType.RDSWIP:
                            case RepositoryType.Part8:
                                sparqlQuery = "GetTemplateQualification";
                                break;
                        }

                        getTemplateQualification = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;
                        queryBindings = getTemplateQualification.Bindings;

                        sparql = ReadSPARQL(getTemplateQualification.FileName);
                        sparql = sparql.Replace("param1", id);

                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            templateQualification = new TemplateQualification();
                            Description description = new Description();
                            QMXFStatus status = new QMXFStatus();
                            QMXFName name = new QMXFName();

                            templateQualification.repositoryName = repository.Name;

                            if (result.ContainsKey("name"))
                            {
                                name.value = result["name"];
                            }
                            if (result.ContainsKey("description"))
                            {
                                description.value = result["description"];
                            }
                            if (result.ContainsKey("statusClass"))
                            {
                                status.Class = result["statusClass"];
                            }
                            if (result.ContainsKey("statusAuthority"))
                            {
                                status.authority = result["statusAuthority"];
                            }
                            if (result.ContainsKey("qualifies"))
                            {
                                templateQualification.qualifies = result["qualifies"];
                            }


                            templateQualification.identifier = id;
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

        private int getIndexFromName(string name)
        {
            try
            {
                int index = 0;
                foreach (Repository repository in _repositories)
                {
                    if (repository.Name.Equals(name))
                    {
                        index = _repositories.IndexOf(repository);
                        return index;
                    }
                }
                foreach (Repository repository in _repositories)
                {
                    if (!repository.IsReadOnly)
                    {
                        index = _repositories.IndexOf(repository);
                        return index;
                    }
                }

                return index;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string CreateIdsAdiId(string RegistryBase, string name)
        {
            string baseServiceUrl = "https://secure.ids-adi.org/registry?registry-op=acquire&registry-base=" +
                                    HttpUtility.UrlEncode(RegistryBase) + "&registry-comment=";
            string serviceUrl = baseServiceUrl + HttpUtility.UrlEncode(name);
            string idsAdiId = "";

            try
            {
                string responseText = QueryIdGenerator(serviceUrl);

                if (responseText != null && responseText.Contains("<registry-id>"))
                {
                    int startIndex = responseText.IndexOf("<registry-id>");
                    int endIndex = responseText.IndexOf("</registry-id>");
                    idsAdiId = responseText.Substring(startIndex + 13, endIndex - startIndex - 13);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in CreateIdsAdiId: " + e);
                throw new Exception("CreateIdsAdiId: " + e.ToString() + " registrybase: " + RegistryBase);
            }

            return idsAdiId;
        }

        private List<Dictionary<string, string>> MergeLists(List<Dictionary<string, string>> a, List<Dictionary<string, string>> b)
        {
            try
            {
                foreach (Dictionary<string, string> dictionary in b)
                {
                    a.Add(dictionary);
                }
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
        private string ReadSPARQL(string queryName)
        {
            try
            {
                string query;

                query = Utility.ReadString(_settings["SparqlPath"] + queryName);

                return query;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private SPARQLResults QueryFromRepository(Repository repository, string sparql)
        {
            try
            {
                SPARQLResults sparqlResults;

                string encryptedCredentials = repository.EncryptedCredentials;

                WebCredentials credentials = new WebCredentials(encryptedCredentials);
                if (credentials.isEncrypted) credentials.Decrypt();

                sparqlResults = SPARQLClient.Query(repository.Uri, sparql, credentials, _proxyCredentials);

                return sparqlResults;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to read repository['{0}']", repository.Uri), ex);
                return new SPARQLResults();
            }
        }

        private string QueryIdGenerator(string serviceUrl)
        {
            try
            {
                string result;

                WebHttpClient webClient = new WebHttpClient(serviceUrl, _registryCredentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                result = webClient.GetMessage(serviceUrl);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Response PostToRepository(Repository repository, string sparql)
        {
            try
            {
                Response response = new Response();

                string encryptedCredentials = repository.EncryptedCredentials;
                string uri = string.IsNullOrEmpty(repository.UpdateUri) ? repository.Uri : repository.UpdateUri;

                WebCredentials credentials = new WebCredentials(encryptedCredentials);
                if (credentials.isEncrypted) credentials.Decrypt();

                SPARQLClient.PostQueryAsMultipartMessage(uri, sparql, credentials, _proxyCredentials);

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<Dictionary<string, string>> BindQueryResults(QueryBindings queryBindings, SPARQLResults sparqlResults)
        {
            try
            {
                List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

                foreach (SPARQLResult sparqlResult in sparqlResults.resultsElement.results)
                {
                    Dictionary<string, string> result = new Dictionary<string, string>();

                    string sortKey = string.Empty;

                    foreach (SPARQLBinding sparqlBinding in sparqlResult.bindings)
                    {
                        foreach (QueryBinding queryBinding in queryBindings)
                        {
                            if (queryBinding.Name == sparqlBinding.name)
                            {
                                string key = queryBinding.Name;

                                string value = String.Empty;
                                string dataType = String.Empty;
                                if (queryBinding.Type == SPARQLBindingType.Uri)
                                {
                                    value = sparqlBinding.uri;
                                }
                                else if (queryBinding.Type == SPARQLBindingType.Literal)
                                {
                                    value = sparqlBinding.literal.value;
                                    dataType = sparqlBinding.literal.datatype;
                                    sortKey = value;
                                }

                                if (result.ContainsKey(key))
                                {
                                    key = MakeUniqueKey(result, key);
                                }

                                result.Add(key, value);

                                if (dataType != String.Empty && dataType != null)
                                {
                                    result.Add(key + "_dataType", dataType);
                                }
                            }
                        }
                    }
                    results.Add(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string MakeUniqueKey(Dictionary<string, string> dictionary, string duplicateKey)
        {
            try
            {
                string newKey = String.Empty;

                for (int i = 2; i < Int32.MaxValue; i++)
                {
                    string postfix = " (" + i.ToString() + ")";
                    if (!dictionary.ContainsKey(duplicateKey + postfix))
                    {
                        newKey += postfix;
                        break;
                    }
                }

                return newKey;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private RefDataEntities GetRequestedPage(RefDataEntities entities, int startIdx, int pageSize)
        {
            try
            {
                RefDataEntities page = new RefDataEntities();
                page.Total = entities.Entities.Count;

                for (int i = startIdx; i < startIdx + pageSize; i++)
                {
                    if (entities.Entities.Count == i) break;

                    string key = entities.Entities.Keys[i];
                    Entity entity = entities.Entities[key];
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

        #endregion Protoype Part8

        #region Part8

        private QMXF GetTemplate(string id, Repository repository)
        {
            QMXF qmxf = new QMXF();

            try
            {
                TemplateQualification templateQualification = GetTemplateQualification(id, repository);

                if (templateQualification != null)
                {
                    qmxf.templateQualifications.Add(templateQualification);
                }
                else
                {
                    TemplateDefinition templateDefinition = GetTemplateDefinition(id, repository);
                    qmxf.templateDefinitions.Add(templateDefinition);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetTemplate: " + ex);
            }

            return qmxf;
        }

        public List<Classification> GetPart8TemplateClassif(string id)
        {
            QMXF qmxf = new QMXF();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                List<Classification> classifications = new List<Classification>();


                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetTemplateClassification").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);


                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {

                        Classification classification = new Classification();
                        string uri = String.Empty;
                        string label = String.Empty;

                        if (result.ContainsKey("uri"))
                        {
                            uri = result["uri"];
                            classification.reference = uri;
                        }

                        //if (result.ContainsKey("label"))
                        //    label = result["label"];

                        classification.label = label;
                        Utility.SearchAndInsert(classifications, classification, Classification.sortAscending());
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

        public List<Specialization> GetPart8TemplateSpec(string id)
        {
            QMXF qmxf = new QMXF();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                List<Specialization> specializations = new List<Specialization>();


                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetTemplateSpecialization").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);


                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {

                        Specialization specialization = new Specialization();
                        string uri = String.Empty;
                        string label = String.Empty;

                        if (result.ContainsKey("uri"))
                        {
                            uri = result["uri"];
                            specialization.reference = uri;
                        }

                        //if (result.ContainsKey("label"))
                        //    label = result["label"];

                        specialization.label = label;
                        Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
                    }

                }

                return specializations;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetClassifications: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        //public QMXF GetPart8Template(string id)
        //{
        //    QMXF qmxf = new QMXF();

        //    try
        //    {
        //        TemplateDefinition templateDefinition = GetPart8TemplateDefinition(id);
        //        qmxf.templateDefinitions.Add(templateDefinition);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error in GetPart8Template: " + ex);
        //    }

        //    return qmxf;
        //}

        //private TemplateDefinition GetPart8TemplateDefinition(string id)
        //{
        //    TemplateDefinition templateDefinition = null;

        //    try
        //    {
        //        string sparql = String.Empty;
        //        string relativeUri = String.Empty;

        //        Description description = new Description();

        //        RefDataEntities resultEntities = new RefDataEntities();

        //        Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Template").Query;
        //        QueryBindings queryBindings = queryContainsSearch.Bindings;

        //        sparql = ReadSPARQL(queryContainsSearch.FileName);
        //        sparql = sparql.Replace("param1", id);


        //        foreach (Repository repository in _repositories)
        //        {
        //            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

        //            List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

        //            foreach (Dictionary<string, string> result in results)
        //            {
        //                templateDefinition = new TemplateDefinition();
        //                QMXFName name = new QMXFName();

        //                if (result.ContainsKey("label"))
        //                {
        //                    name.value = result["label"];
        //                }

        //                if (result.ContainsKey("definition"))
        //                {
        //                    description.value = result["definition"];
        //                }

        //                templateDefinition.identifier = @"http://standards.tc184-sc4.org/iso/15926/-8/templates#" + id;
        //                templateDefinition.name.Add(name);
        //                templateDefinition.description.Add(description);

        //                templateDefinition.roleDefinition = GetPart8RoleDefintion(id);
        //            }
        //        }

        //        return templateDefinition;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("Error in GetTemplateDefinition: " + e);
        //        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
        //    }
        //}

        //private List<RoleDefinition> GetPart8RoleDefintion(string id)
        //{
        //    try
        //    {
        //        string sparql = String.Empty;
        //        string relativeUri = String.Empty;

        //        Description description = new Description();
        //        QMXFStatus status = new QMXFStatus();
        //        //List<Classification> classifications = new List<Classification>();

        //        List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();

        //        RefDataEntities resultEntities = new RefDataEntities();

        //        Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Roles").Query;
        //        QueryBindings queryBindings = queryContainsSearch.Bindings;

        //        sparql = ReadSPARQL(queryContainsSearch.FileName);
        //        sparql = sparql.Replace("param1", id);

        //        foreach (Repository repository in _repositories)
        //        {
        //            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

        //            List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

        //            foreach (Dictionary<string, string> result in results)
        //            {

        //                RoleDefinition roleDefinition = new RoleDefinition();
        //                QMXFName name = new QMXFName();

        //                if (result.ContainsKey("label"))
        //                {
        //                    name.value = result["label"];
        //                }
        //                if (result.ContainsKey("role"))
        //                {
        //                    roleDefinition.identifier = result["role"];
        //                }
        //                if (result.ContainsKey("comment"))
        //                {
        //                    roleDefinition.description.value = result["comment"];
        //                }
        //                if (result.ContainsKey("index"))
        //                {
        //                    roleDefinition.description.value = result["index"].ToString();
        //                }
        //                if (result.ContainsKey("type"))
        //                {
        //                    roleDefinition.range = result["type"];
        //                }
        //                if (string.IsNullOrEmpty(name.value))
        //                    name.value = Utility.GetIdFromURI(roleDefinition.identifier);//.Replace(roleDefinition.identifier.Substring(0, roleDefinition.identifier.LastIndexOf("#") + 1), "");
        //                roleDefinition.name.Add(name);
        //                //Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending()); //problem with search an insert - skips some roles
        //                roleDefinitions.Add(roleDefinition);
        //                roleDefinition.restrictions = GetPart8RoleRestrictions(roleDefinition.identifier.Replace(roleDefinition.identifier.Substring(0, roleDefinition.identifier.LastIndexOf("#") + 1), ""));
        //                //roleDefinitions.Add(roleDefinition);
        //            }
        //        }

        //        return roleDefinitions;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("Error in GetRoleDefinition: " + e);
        //        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
        //    }
        //}

        //private List<PropertyRestriction> GetPart8RoleRestrictions(string id)
        //{
        //    try
        //    {
        //        string sparql = String.Empty;
        //        string relativeUri = String.Empty;

        //        Description description = new Description();
        //        QMXFStatus status = new QMXFStatus();
        //        //List<Classification> classifications = new List<Classification>();

        //        List<PropertyRestriction> propertyRestrictions = new List<PropertyRestriction>();

        //        RefDataEntities resultEntities = new RefDataEntities();

        //        Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8RoleRestrictions").Query;
        //        QueryBindings queryBindings = queryContainsSearch.Bindings;

        //        sparql = ReadSPARQL(queryContainsSearch.FileName);
        //        sparql = sparql.Replace("param1", id);

        //        foreach (Repository repository in _repositories)
        //        {
        //            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

        //            List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

        //            foreach (Dictionary<string, string> result in results)
        //            {

        //                PropertyRestriction propertyRestriction = new PropertyRestriction();

        //                if (result.ContainsKey("valuesFrom"))
        //                {
        //                    propertyRestriction.valuesFrom = result["valuesFrom"];
        //                    propertyRestriction.type = "allValuesFrom";
        //                }
        //                if (result.ContainsKey("cardinality"))
        //                {
        //                    propertyRestriction.cardiniality = result["cardinality"];
        //                    propertyRestriction.type = "minCardinality";
        //                }
        //                //roleDefinition.name.Add(name);
        //                //Utility.SearchAndInsert(propertyRestrictions, propertyRestriction, PropertyRestriction.sortAscending());
        //                propertyRestrictions.Add(propertyRestriction);
        //            }
        //        }

        //        return propertyRestrictions;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("Error in GetRoleDefinition: " + e);
        //        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
        //    }
        //}

        public Response oldPostTemplate(QMXF qmxf)
        {
            Status status = new Status();
            bool existInTarget = false;
            try
            {
                Response response = null;
                sparqlStr = new StringBuilder();
                sparqlStr.Append(prefix);

                int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
                Repository target = _repositories[repository];

                if (target.IsReadOnly)
                {
                    status.Level = StatusLevel.Error;
                    status.Messages.Add("Repository is Read Only");
                    _response.Append(status);
                    return _response;
                }

                #region Template Definitions
                if (qmxf.templateDefinitions.Count > 0) //Template Definitions
                {
                    foreach (TemplateDefinition template in qmxf.templateDefinitions)
                    {
                        string ID = string.Empty;
                        string id = string.Empty;
                        string language = string.Empty;
                        string label = string.Empty;
                        string generatedTempId = string.Empty;
                        string templateName = string.Empty;
                        string roleDefinition = string.Empty;

                        int templateIndex = -1;
                        int roleCount = 0;

                        ID = template.identifier;

                        QMXF q = new QMXF();
                        if (ID != null)
                        {
                            ID = Utility.ExtractId(ID);

                            q = GetTemplate(ID, QMXFType.Definition, target);

                            foreach (TemplateDefinition templateFound in q.templateDefinitions)
                            {
                                templateIndex++;
                                if (templateFound.repositoryName.Equals(_repositories[repository].Name))
                                {
                                    existInTarget = true;
                                    Utility.WriteString("Template found: " + q.templateDefinitions[templateIndex].name[0].value, "stats.log", true);
                                    break;
                                }
                            }
                        }

                        if (!existInTarget)
                        {
                            sparqlStr.AppendLine(insertData);

                            foreach (QMXFName name in template.name)
                            {
                                label = name.value.Split('@')[0];
                                
                                if (string.IsNullOrEmpty(name.lang))
                                    language = "@" + defaultLanguage;
                                else
                                    language = "@" + name.lang;

                                //ID generator
                                templateName = "Template definition " + label;
                                if (ID == null || ID == string.Empty)
                                {
                                    if (_useExampleRegistryBase)
                                        generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                    else
                                        generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                                    ID = Utility.GetIdFromURI(generatedTempId);

                                    Utility.WriteString("\n" + ID + "\t" + label, "TempDef IDs.log", true);
                                }

                                sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type owl:class ;", ID));

                                //int descrCount = template.description.Count;

                                sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine("  rdfs:subClassOf p8:BaseTemplateStatement ;");
                                sparqlStr.AppendLine("  rdf:type p8:Template ;");
                                foreach (Description descr in template.description)
                                {
                                    if (string.IsNullOrEmpty(descr.value))
                                        continue;
                                    else
                                    {
                                        if (string.IsNullOrEmpty(descr.lang))
                                           language = "@" + defaultLanguage;
                                        else 
                                            language = "@" + descr.lang;

                                            sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", descr.value.Split('@')[0], language));
                                    }
                                }

                                sparqlStr.AppendLine(string.Format("  tpl:TemplateDescription_of_{0} rdf:type p8:TemplateDescription ;", label));
                                // sparql.AppendLine(" rdf:type owl:thing ;");
                                sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateDescription_of_{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine(string.Format("  p8:valNumberOfRoles {0} ;", template.roleDefinition.Count));
                                sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .", ID));

                                sparqlStr.AppendLine("}");

                                response = PostToRepository(target, sparqlStr.ToString());

                                foreach (RoleDefinition role in template.roleDefinition)
                                {
                                    string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                    string roleID = string.Empty;
                                    string generatedId = string.Empty;
                                    string genName = string.Empty;
                                    string range = role.range;

                                    if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = role.name.FirstOrDefault().lang;

                                    //if (!string.IsNullOrEmpty(role.range))
                                    //{
                                    //    range = "<" + range + ">";
                                    //}
                                    //else if (!string.IsNullOrEmpty(role.range))
                                    //{
                                    //    range = " xsd:" + range;
                                    //}

                                    genName = "Role definition " + roleLabel;
                                    if (string.IsNullOrEmpty(role.identifier))
                                    {
                                        if (_useExampleRegistryBase)
                                            generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                        else
                                            generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                        roleID = Utility.GetIdFromURI(generatedId);
                                    }
                                    else
                                    {
                                        roleID = Utility.GetIdFromURI(role.identifier);
                                    }

                                    sparqlStr = new StringBuilder();
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);
                                    sparqlStr.AppendLine(string.Format("  tpl:TemplateRoleDescription_of_{0}_{1} rdf:type p8:TemplateRoleDescription ;", label, roleLabel));// could get roleIndex from QMXF?
                                    sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateRoleDescription_of_{0}_{1}{2}\"^^xsd:string ;", label, roleLabel, language));
                                    sparqlStr.AppendLine(string.Format("  p8:valRoleIndex {0} ;", ++roleCount));
                                    sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} ;", ID));
                                    //  sparqlStr.AppendLine("  p8:hasRoleFillerType " + range + " ;");  //need to work out how to get id for this
                                    sparqlStr.AppendLine(string.Format("  p8:hasRole tpl:{0} .", roleID ));
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }
                            }
                        }
                        //else edit template
                        else
                        {
                            string identifier = string.Empty;
                            string description = string.Empty;
                            roleCount = 0;

                            TemplateDefinition td = q.templateDefinitions[templateIndex];
                            string rName = string.Empty;

                            sparqlStr = new StringBuilder();
                            sparqlStr.Append(prefix);
                            sparqlStr.AppendLine(deleteWhere);

                            identifier = td.identifier;

                            sparqlStr.AppendLine(string.Format("{0} rdf:type owl:class ;", identifier));
                            sparqlStr.AppendLine(" ?property ?value . ");


                            label = td.name[0].value;

                            sparqlStr.AppendLine(string.Format(" tpl:TemplateDescription_of_{0} rdf:type p8:TemplateDescription ;", label));
                            sparqlStr.AppendLine(" ?property ?value . ");

                            foreach (RoleDefinition role in td.roleDefinition)
                            {
                                string roleID = role.identifier;
                                string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                sparqlStr = new StringBuilder();
                                sparqlStr.Append(prefix);
                                sparqlStr.AppendLine(deleteWhere);
                                sparqlStr.AppendLine(string.Format(" tpl:TemplateRoleDescription_of_{0}_{1} p8:hasRole {2} ;", label, roleLabel, roleID));
                                sparqlStr.AppendLine(" ?property ?value . ");
                            }

                            sparqlStr = sparqlStr.AppendLine("}");
                            response = PostToRepository(target, sparqlStr.ToString());

                            sparqlStr = new StringBuilder();
                            sparqlStr.Append(prefix);
                            sparqlStr.AppendLine(insertData);

                            foreach (QMXFName name in template.name)
                            {
                                label = name.value.Split('@')[0];

                                if (string.IsNullOrEmpty(name.lang))
                                    language = "@" + defaultLanguage;
                                else
                                    language = "@" + name.lang;

                                ID = template.identifier;
                                ID = Utility.GetIdFromURI(ID);
                                //ID generator
                                templateName = "Template definition " + label;
                                if (string.IsNullOrEmpty(ID))
                                {
                                    if (_useExampleRegistryBase)
                                        generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                    else
                                        generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                                    ID = Utility.GetIdFromURI(generatedTempId);

                                    Utility.WriteString("\n" + ID + "\t" + label, "TempDef IDs.log", true);
                                }

                                sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type owl:class ; ", ID));

                                int descrCount = template.description.Count;

                                sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine("  rdfs:subClassOf p8:BaseTemplateStatement ;");
                                sparqlStr.AppendLine("  rdf:type p8:Template ;");
                                foreach (Description descr in template.description)
                                {
                                    

                                    if (string.IsNullOrEmpty(descr.value))
                                        continue;
                                    else
                                    {
                                        description = descr.value.Split('@')[0];

                                        if (string.IsNullOrEmpty(descr.lang))
                                            language = "@" + defaultLanguage;
                                        else
                                            language = "@" + descr.lang;

                                        sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", description, language));
                                    }
                                }

                                sparqlStr.AppendLine(string.Format("  tpl:TemplateDescription_of_{0} rdf:type p8:TemplateDescription ;", label));
                                // sparql.AppendLine(" rdf:type owl:thing ;");
                                sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateDescription_of_{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine(string.Format("  p8:valNumberOfRoles {0} ;", template.roleDefinition.Count));
                                sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .", ID));

                                sparqlStr.AppendLine("}");


                                response = PostToRepository(target, sparqlStr.ToString());

                                foreach (Specialization spec in template.specialization)
                                {
                                    string specialization = String.Empty;

                                    sparqlStr = new StringBuilder();
                                    specialization = spec.reference;
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);

                                    sparqlStr.AppendLine(string.Format("  tpl:TemplateSpecialization_of_{0}_to_{1} rdf:type p8:TemplateSpecialization ;", specialization, label));

                                    sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateSpecialization_of_{0}_to_{1}{2}\"^^xds:string ;",specialization, label, language));
                                                sparqlStr.AppendLine(string.Format("  p8:hasSubTemplate tpl:{0} .", ID));
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }

                                foreach (RoleDefinition role in template.roleDefinition)
                                {
                                    string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                    string roleID = string.Empty;
                                    string generatedId = string.Empty;
                                    string genName = string.Empty;

                                    if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = "@" + role.name.FirstOrDefault().lang;

                                    genName = "Role definition " + roleLabel;
                                    if (string.IsNullOrEmpty(role.identifier))
                                    {
                                        if (_useExampleRegistryBase)
                                            generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                        else
                                            generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                        roleID = Utility.GetIdFromURI(generatedId);
                                    }
                                    else
                                    {
                                        roleID = Utility.GetIdFromURI(role.identifier);
                                    }

                                    sparqlStr = new StringBuilder();
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);

                                    sparqlStr.AppendLine(string.Format("  tpl:TemplateRoleDescription_of_{0}_{1} rdf:type p8:TemplateRoleDescription ;", label, roleLabel));// could get roleIndex from QMXF?
                                    sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateRoleDescription_of_{0}_{1}{2}\"^^xsd:string ;", label, roleLabel, language));
                                    sparqlStr.AppendLine(string.Format("  p8:valRoleIndex {0} ;", roleCount));
                                    sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} ;", ID));
                                    //sparqlStr.AppendLine("  p8:hasRoleFillerType " + range + " ;");
                                    sparqlStr.AppendLine(string.Format("  p8:hasRole rdl:{0} .", roleID));
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }

                                response = PostToRepository(target, sparqlStr.ToString());
                            }
                        }
                    }
                }
                #endregion template Definitions
                #region Template Qualification
                else if (qmxf.templateQualifications.Count > 0)
                {
                    foreach (TemplateQualification template in qmxf.templateQualifications)
                    {
                        string ID = string.Empty;
                        string id = string.Empty;
                        string label = string.Empty;
                        string language = string.Empty;
                        string qName = string.Empty;
                        //string description = string.Empty;
                        string generatedTempId = string.Empty;
                        string templateName = string.Empty;
                        string roleDefinition = string.Empty;
                        string specialization = string.Empty;
                        string nameSparql = string.Empty;
                        string specSparql = string.Empty;
                        string classSparql = string.Empty;
                        int templateIndex = -1;
                        int roleCount = 0;
                        ID = template.identifier;
                        QMXF q = new QMXF();

                        if (ID != null)
                        {
                            id = Utility.GetIdFromURI(ID);
                            ID = id;

                            q = GetTemplate(id, QMXFType.Qualification, target);
                            foreach (TemplateQualification templateFound in q.templateQualifications)
                            {
                                templateIndex++;
                                if (templateFound.repositoryName.Equals(_repositories[repository]))
                                {
                                    existInTarget = true;
                                    Utility.WriteString("Template found: " + q.templateDefinitions[templateIndex].name[0].value, "stats.log", true);
                                    break;
                                }
                            }
                        }

                        if (!existInTarget)
                        {
                            foreach (QMXFName name in template.name)
                            {
                                label = name.value.Split('@')[0];

                                if (string.IsNullOrEmpty(name.lang))
                                    language = "@" + defaultLanguage;
                                else
                                    language = "@" + name.lang;

                                templateName = "Template qualification " + label;

                                if (ID == null || ID == string.Empty)
                                {
                                    if (_useExampleRegistryBase)
                                        generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                    else
                                        generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                                    ID = Utility.GetIdFromURI(generatedTempId);

                                    Utility.WriteString("\n" + ID + "\t" + label, "TempQual IDs.log", true);
                                }

                                sparqlStr.AppendLine(insertData);
                                sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type owl:class ;", ID));

                                int descrCount = template.description.Count;

                                sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine("  rdf:type p8:Template ;");

                                foreach (Description descr in template.description)
                                {

                                    if (string.IsNullOrEmpty(descr.value))
                                        continue;
                                    else
                                    {
                                        if (string.IsNullOrEmpty(descr.lang))
                                            language = "@" + defaultLanguage;
                                        else
                                            language = "@" + descr.lang;

                                        if (--descrCount > 0)
                                            sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", descr.value.Split('@')[0], language));
                                        else
                                            sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string .", descr.value.Split('@')[0], language));
                                    }
                                    
                                }

                                sparqlStr.AppendLine(string.Format("  tpl:TemplateDescription_of_{0} rdf:type p8:TemplateDescription ;", label));
                                // sparql.AppendLine(" rdf:type owl:thing ;");
                                sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateDescription_of_{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine(string.Format("  p8:valNumberOfRoles {0} ;", template.roleQualification.Count));
                                sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .",ID));

                                sparqlStr.AppendLine("}");

                                response = PostToRepository(target, sparqlStr.ToString());

                                foreach (Specialization spec in template.specialization)
                                {
                                    sparqlStr = new StringBuilder();
                                    specialization = spec.reference;
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);

                                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:subClassOf {1} ;", ID, specialization));

                                    sparqlStr.AppendLine(string.Format("  tpl:TemplateSpecialization_of_{0}_to_{1} rdf:type p8:TemplateSpecialization ;", spec.label.Split('@')[0], label));
                                    sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateSpecialization_of_{0}_to_{1}{2}\"^^xds:string ;",spec.label.Split('@')[0], label, language));
                                    sparqlStr.AppendLine(string.Format("  p8:hasSuperTemplate {0} ;", specialization));
                                    sparqlStr.AppendLine(string.Format("  p8:hasSubTemplate tpl:{0} .", ID));
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }

                                foreach (RoleQualification role in template.roleQualification)
                                {
                                    string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                    string roleID = string.Empty;
                                    string generatedId = string.Empty;
                                    string genName = string.Empty;

                                    if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = role.name.FirstOrDefault().lang;

                                    genName = "Role qualification " + roleLabel;
                                    if (string.IsNullOrEmpty(role.identifier))
                                    {
                                        if (_useExampleRegistryBase)
                                            generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                        else
                                            generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                        roleID = Utility.GetIdFromURI(generatedId);
                                    }
                                    else
                                    {
                                        roleID = Utility.GetIdFromURI(role.identifier);
                                    }

                                    sparqlStr = new StringBuilder();
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);
                                    sparqlStr.AppendLine(string.Format("  tpl:TemplateRoleDescription_of_{0}_{1} rdf:type p8:TemplateRoleDescription ;", label, roleLabel));// could get roleIndex from QMXF?
                                    sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateRoleDescription_of_{0}_{1}{2}\"^^xsd:string ;", label, roleLabel, language));
                                    sparqlStr.AppendLine(string.Format("  p8:valRoleIndex {0} ;", ++roleCount));
                                    sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} ;", ID));
                                    //sparqlStr.AppendLine("  p8:hasRoleFillerType " + range + " ;");
                                    sparqlStr.AppendLine(string.Format("  p8:hasRole tpl:{0} .", roleID));
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }
                            }
                        }
                        //else edit template
                        else
                        {
                            string identifier = string.Empty;
                            roleCount = 0;

                            TemplateDefinition td = q.templateDefinitions[templateIndex];
                            string rName = string.Empty;

                            sparqlStr = new StringBuilder();
                            sparqlStr.Append(prefix);
                            sparqlStr.AppendLine(deleteWhere);

                            identifier = td.identifier;

                            sparqlStr.AppendLine(string.Format("{0} rdf:type owl:class ;", identifier));
                            sparqlStr.AppendLine(" ?property ?value . ");


                            label = td.name[0].value.Split('@')[0];

                            sparqlStr.AppendLine(string.Format(" tpl:TemplateDescription_of_{0} rdf:type p8:TemplateDescription ;", label));
                            sparqlStr.AppendLine(" ?property ?value . ");

                            foreach (RoleDefinition role in td.roleDefinition)
                            {
                                string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                string roleID = role.identifier;


                                if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                                    language = "@" + defaultLanguage;
                                else
                                    language = role.name.FirstOrDefault().lang;

                                sparqlStr = new StringBuilder();
                                sparqlStr.Append(prefix);
                                sparqlStr.AppendLine(deleteData);
                                sparqlStr.AppendLine(string.Format("  tpl:TemplateRoleDescription_of_{0}_{1}{2} p8:hasRole {3} ;", label, roleLabel, roleID));
                                sparqlStr.AppendLine(" ?property ?value . ");
                            }

                            foreach (Specialization spec in template.specialization)
                            {
                                specialization = String.Empty;

                                sparqlStr = new StringBuilder();
                                specialization = spec.reference;
                                sparqlStr.Append(prefix);
                                sparqlStr.AppendLine(deleteData);

                                sparqlStr.AppendLine(string.Format("  p8:hasSubTemplate tpl:{0}", identifier));
                                sparqlStr.AppendLine(" ?property ?value . ");
                            }

                            sparqlStr = sparqlStr.AppendLine("}");
                            response = PostToRepository(target, sparqlStr.ToString());

                            sparqlStr = new StringBuilder();
                            sparqlStr.Append(prefix);
                            sparqlStr.AppendLine(insertData);

                            foreach (QMXFName name in template.name)
                            {
                                label = name.value.Split('@')[0];
                                
                                if (string.IsNullOrEmpty(name.lang))
                                    language = "@" + defaultLanguage;
                                else
                                    language = "@" + name.lang;

                                ID = template.identifier;
                                ID = Utility.GetIdFromURI(ID);
                                //ID generator
                                templateName = "Template definition " + label;
                                if (ID == null || ID == string.Empty)
                                {
                                    if (_useExampleRegistryBase)
                                        generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                    else
                                        generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                                    ID = Utility.GetIdFromURI(generatedTempId);

                                    Utility.WriteString("\n" + ID + "\t" + label, "TempDef IDs.log", true);
                                }

                                sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type owl:class ;", ID));

                                int descrCount = template.description.Count;

                                sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine(string.Format("  rdfs:subClassOf {0} ;", template.qualifies));
                                // sparql.AppendLine(" rdf:type owl:Thing ;");
                                sparqlStr.AppendLine("  rdf:type p8:Template ;");
                                foreach (Description descr in template.description)
                                {
                                    if (string.IsNullOrEmpty(descr.value))
                                        continue;
                                    else
                                    {
                                        if (string.IsNullOrEmpty(descr.lang))
                                            language = "@" + defaultLanguage;
                                        else
                                            language = "@" + descr.lang;

                                        if (--descrCount > 0)
                                            sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", descr.value.Split('@')[0], language));
                                        else
                                            sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string .", descr.value.Split('@')[0], language));
                                    }
                                }

                                sparqlStr.AppendLine(string.Format("  tpl:TemplateDescription_of_{0} rdf:type p8:TemplateDescription ;", label));
                                // sparql.AppendLine(" rdf:type owl:thing ;");
                                sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateDescription_of_{0}{1}\"^^xsd:string ;", label, language));
                                sparqlStr.AppendLine(string.Format("  p8:valNumberOfRoles {0} ;", template.roleQualification.Count ));
                                sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .", ID));

                                sparqlStr.AppendLine("}");
                                response = PostToRepository(target, sparqlStr.ToString());

                                foreach (Specialization spec in template.specialization)
                                {
                                    sparqlStr = new StringBuilder();
                                    specialization = spec.reference;
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);

                                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:subClassOf {1} ;", ID, specialization));

                                    sparqlStr.AppendLine(string.Format("  tpl:TemplateSpecialization_of_{0}_to_{1} rdf:type p8:TemplateSpecialization ;", spec.label, label ));
                                    sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateSpecialization_of_{0}_to_{1} \"^^xds:string ;",spec.label.Split('@')[0], label, language));
                                    sparqlStr.AppendLine(string.Format("  p8:hasSuperTemplate {0} ;", specialization));
                                    sparqlStr.AppendLine(string.Format("  p8:hasSubTemplate tpl:{0} .", ID));
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }

                                foreach (RoleQualification role in template.roleQualification)
                                {
                                    string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];

                                    if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = role.name.FirstOrDefault().lang;

                                    string roleID = string.Empty;
                                    string generatedId = string.Empty;
                                    string genName = string.Empty;

                                    genName = "Role definition " + roleLabel;
                                    if (string.IsNullOrEmpty(role.identifier))
                                    {
                                        if (_useExampleRegistryBase)
                                            generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                        else
                                            generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                        roleID = Utility.GetIdFromURI(generatedId);
                                    }
                                    else
                                    {
                                        roleID = Utility.GetIdFromURI(role.identifier);
                                    }

                                    sparqlStr = new StringBuilder();
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);
                                    sparqlStr.AppendLine(string.Format("  tpl:TemplateRoleDescription_of_{0}_{1} rdf:type p8:TemplateRoleDescription ;", label, roleLabel));// could get roleIndex from QMXF?
                                    sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateRoleDescription_of_{0}_{1}{2}\"^^xsd:string ;", label, roleLabel, language));
                                    sparqlStr.AppendLine(string.Format("  p8:valRoleIndex {0} ;", roleCount));
                                    sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} ;", ID));
                                    //sparqlStr.AppendLine("  p8:hasRoleFillerType " + range + " ;");
                                    sparqlStr.AppendLine(string.Format("  p8:hasRole rdl:{0}", roleID));
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }
                            }
                        }
                    }
                }
                #endregion
                _response.Append(status);
                return _response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in PostTemplate: " + ex);
                throw ex;
            }
        }

        public Response PostTemplate(QMXF qmxf)
        {
            Response response = new Response();
            response.Level = StatusLevel.Success;

            try
            {
                Repository repository = GetRepository(qmxf.targetRepository);

                if (repository == null || repository.IsReadOnly)
                {
                    Status status = new Status();
                    status.Level = StatusLevel.Error;

                    if (repository == null)
                        status.Messages.Add("Repository not found!");
                    else
                        status.Messages.Add("Repository [" + qmxf.targetRepository + "] is read-only!");

                    _response.Append(status);
                }
                else
                {
                    string registry = _useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["ClassRegistryBase"];
                    StringBuilder sparqlDelete = new StringBuilder();

                    #region Template Definitions
                    if (qmxf.templateDefinitions.Count > 0) //Template Definitions
                    {
                        foreach (TemplateDefinition template in qmxf.templateDefinitions)
                        {
                            string identifier = String.Empty;
                            string generatedTempId = string.Empty;
                            string templateName = string.Empty;
                            string roleDefinition = string.Empty;
                            int index = 1;
                            string deleteSparql = String.Empty;
                            string insertSparql = String.Empty;
                            string whereSparql = String.Empty;
                            string nameSparql = string.Empty;
                            string specSparql = string.Empty;
                            string classSparql = string.Empty;

                            identifier = Utility.GetIdFromURI(template.identifier);

                            //check for exisitng template
                            QMXF existingQmxf = null;
                            if (!String.IsNullOrEmpty(identifier))
                            {
                                existingQmxf = GetTemplate(identifier, repository);
                            }

                            if (existingQmxf == null || existingQmxf.templateDefinitions.Count == 0)
                            {
                                #region Form Insert SPARQL

                                string label = String.Empty;
                                string labelSparql = String.Empty;

                                //start with prefix
                                string sparql = prefixSparql;

                                //adding a new template, INSERT only
                                sparql += "INSERT DATA {";

                                //form labels
                                foreach (QMXFName name in template.name)
                                {
                                    label = name.value; //the last label will be used.
                                    labelSparql += "rdfs:label \"" + label + "\"^^xsd:string ;";
                                }

                                //ID generator
                                templateName = "Template definition " + label;

                                string id = String.Empty;
                                if (_useExampleRegistryBase)
                                    id = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                else
                                    id = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                                identifier = "<" + id + ">";

                                //append labels to sparql
                                sparql += identifier + " " + labelSparql;

                                //add descriptions to sparql
                                foreach (Description description in template.description)
                                {
                                    sparql += " rdfs:comment \"" + description.value + "\"^^xsd:string ; ";
                                }

                                //add role count to sparql
                                sparql += " tpl:R35529169909 \"" + template.roleDefinition.Count + "\"^^xsd:int . ";

                                foreach (RoleDefinition role in template.roleDefinition)
                                {
                                    #region Process New Roles

                                    string roleIdentifier = string.Empty;
                                    string roleLabel = string.Empty;
                                    string roleDescription = string.Empty;
                                    string generatedId = string.Empty;

                                    //form labels and comments (meta data)
                                    string metaSparql = String.Empty;
                                    foreach (QMXFName roleName in role.name)
                                    {
                                        roleLabel = roleName.value;
                                        metaSparql += " rdfs:label \"" + roleLabel + "\"^^xsd:string ; ";
                                    }

                                    if (role.description != null)
                                    {
                                        roleDescription = role.description.value;
                                        metaSparql += " rdfs:comment \"" + roleDescription + "\"^^xsd:string ; ";
                                    }

                                    //ID generator
                                    string genName = "Role definition " + roleLabel;

                                    if (_useExampleRegistryBase)
                                        generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                    else
                                        generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                    roleIdentifier = "<" + generatedId + ">";

                                    //add role to block
                                    sparql += roleIdentifier + " ";

                                    //role template
                                    sparql += " rdf:type tpl:R74478971040 ; ";

                                    //role type
                                    if (role.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                        || role.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                    {
                                        sparql += " rdf:type owl:DataTypeProperty ; ";
                                    }
                                    else
                                    {
                                        sparql += " rdf:type owl:ObjectProperty ; "
                                                + " rdf:type owl:FunctionalProperty ; ";
                                    }

                                    //add metaData to block
                                    sparql += metaSparql;

                                    //add template possessor role to block
                                    sparql += " rdfs:domain " + identifier + " ; ";

                                    //add range to block
                                    sparql += "rdfs:range <" + role.range + "> ; ";

                                    //add role index to block
                                    sparql += "tpl:R97483568938 \"" + index + "\"^^xsd:int ; ";

                                    //terminate role statement
                                    sparql = sparql.Insert(sparql.LastIndexOf(";"), ". ");
                                    sparql = sparql.Remove(sparql.LastIndexOf(";"));

                                    index++;
                                    #endregion
                                }

                                //template status
                                sparql += "_:status rdf:type tpl:R20247551573 ; "
                                          + "tpl:R64574858717 " + identifier + " ; "
                                          + "tpl:R56599656536 rdl:R6569332477 ; "
                                          + "tpl:R61794465713 rdl:R3732211754 . ";

                                sparql += "}";

                                #endregion

                                //post sparql
                                response = PostToRepository(repository, sparql);

                                _response.Append(response);
                                if (response.Level == StatusLevel.Error)
                                {
                                    throw new Exception("Error while Inserting a new template.");
                                }

                                status = new Status
                                {
                                    Level = StatusLevel.Success,
                                };

                                status.Messages.Add("Successfully added template to repository, " + repository.name + ".");
                            }
                            else
                            {
                                #region Form Delete/Insert SPARQL
                                bool hasInserts = false;
                                bool hasDeletes = false;

                                TemplateDefinition existingTemplate = existingQmxf.templateDefinitions[0];

                                //append prefixes
                                string sparql = prefixSparql;

                                //start each block
                                deleteSparql += "DELETE { ";
                                insertSparql += "INSERT { ";
                                whereSparql += "WHERE { ";

                                //start template statement in each block
                                string deleteTemplateSparql = identifier + " ";
                                string insertTemplateSparql = identifier + " ";
                                string whereTemplateSparql = identifier + " ?p ?o . ";

                                //append changing labels to each block
                                foreach (QMXFName name in template.name)
                                {
                                    QMXFName existingName = existingTemplate.name.Find(n => n.lang == name.lang);

                                    if (existingName != null)
                                    {
                                        if (String.Compare(existingName.value, name.value, true) != 0)
                                        {
                                            hasDeletes = hasInserts = true;

                                            deleteTemplateSparql += "rdfs:label \"" + existingName.value + "\"^^xsd:string ; ";
                                            insertTemplateSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                        }
                                    }
                                    else
                                    {
                                        hasInserts = true;

                                        insertTemplateSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                    }
                                }

                                //append changing descriptions to each block
                                foreach (Description description in template.description)
                                {
                                    Description existingDescription = existingTemplate.description.Find(d => d.lang == description.lang);

                                    if (existingDescription != null)
                                    {
                                        if (String.Compare(existingDescription.value, description.value, true) != 0)
                                        {
                                            hasDeletes = hasInserts = true;

                                            deleteTemplateSparql += "rdfs:comment \"" + existingDescription.value + "\"^^xsd:string ; ";
                                            insertTemplateSparql += "rdfs:comment \"" + description.value + "\"^^xsd:string ; ";
                                        }
                                    }
                                    else
                                    {
                                        hasInserts = true;

                                        insertTemplateSparql += "rdfs:comment \"" + description.value + "\"^^xsd:string ; ";
                                    }
                                }

                                //append changing role count to each block
                                if (existingTemplate.roleDefinition.Count != template.roleDefinition.Count)
                                {
                                    hasDeletes = hasInserts = true;

                                    deleteTemplateSparql += "tpl:R35529169909 \"" + existingTemplate.roleDefinition.Count + "\"^^xsd:int ; ";
                                    insertTemplateSparql += "tpl:R35529169909 \"" + template.roleDefinition.Count + "\"^^xsd:int ; ";
                                }

                                //close template statements and append to each block if needed.
                                if (hasDeletes)
                                {
                                    deleteTemplateSparql = deleteTemplateSparql.Insert(deleteTemplateSparql.LastIndexOf(";"), ". ");
                                    deleteTemplateSparql = deleteTemplateSparql.Remove(deleteTemplateSparql.LastIndexOf(";"));
                                    deleteSparql += deleteTemplateSparql;
                                    whereSparql += whereTemplateSparql;
                                }

                                if (hasInserts)
                                {
                                    insertTemplateSparql = insertTemplateSparql.Insert(insertTemplateSparql.LastIndexOf(";"), ". ");
                                    insertTemplateSparql = insertTemplateSparql.Remove(insertTemplateSparql.LastIndexOf(";"));
                                    insertSparql += insertTemplateSparql;
                                }

                                index = 1;
                                foreach (RoleDefinition role in template.roleDefinition)
                                {
                                    string roleIdentifier = "<" + role.identifier + ">";

                                    hasDeletes = hasInserts = false;

                                    //get existing role if it exists
                                    RoleDefinition existingRole =
                                      existingTemplate.roleDefinition.Find(r => r.identifier == role.identifier);

                                    //remove existing role from existing template, leftovers will be deleted later
                                    existingTemplate.roleDefinition.Remove(existingRole);

                                    if (existingRole != null)
                                    {
                                        #region Process Changing Role

                                        //add the role to each block
                                        string deleteRoleSparql = roleIdentifier + " ";
                                        string insertRoleSparql = roleIdentifier + " ";
                                        string whereRoleSparql = roleIdentifier + " ?p" + index + " ?o" + index + " . ";

                                        //append changing labels to each block
                                        string label = String.Empty;
                                        foreach (QMXFName name in role.name)
                                        {
                                            QMXFName existingName = existingRole.name.Find(n => n.lang == name.lang);

                                            if (existingName != null)
                                            {
                                                if (String.Compare(existingName.value, name.value, true) != 0)
                                                {
                                                    hasDeletes = hasInserts = true;

                                                    deleteRoleSparql += "rdfs:label \"" + existingName.value + "\"^^xsd:string ; ";
                                                    insertRoleSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                                }
                                            }
                                            else
                                            {
                                                hasInserts = true;

                                                insertRoleSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                            }
                                        }

                                        //append changing description to each block
                                        Description existingDescription = existingRole.description;

                                        if (String.Compare(existingDescription.value, role.description.value, true) != 0)
                                        {
                                            hasDeletes = hasInserts = true;

                                            deleteRoleSparql += "rdfs:comment \"" + existingDescription.value + "\"^^xsd:string ; ";
                                            insertRoleSparql += "rdfs:comment \"" + role.description.value + "\"^^xsd:string ; ";
                                        }

                                        //append changing range to each block
                                        if (existingRole.range != role.range)
                                        {
                                            hasDeletes = hasInserts = true;

                                            if (existingRole.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                            || existingRole.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                            {
                                                deleteRoleSparql += " rdf:type owl:DataTypeProperty ; ";
                                            }
                                            else
                                            {
                                                deleteRoleSparql += " rdf:type owl:ObjectProperty ; rdf:type owl:FunctionalProperty ; ";
                                            }

                                            deleteRoleSparql += "rdfs:range <" + existingRole.range + "> ; ";

                                            if (!String.IsNullOrEmpty(role.range))
                                            {
                                                if (role.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                                || role.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                                {
                                                    insertRoleSparql += " rdf:type owl:DataTypeProperty ; ";
                                                }
                                                else
                                                {
                                                    insertRoleSparql += " rdf:type owl:ObjectProperty ; "
                                                            + " rdf:type owl:FunctionalProperty ; ";
                                                }

                                                insertRoleSparql += "rdfs:range <" + role.range + "> ; ";
                                            }
                                        }

                                        //append changing index to each block
                                        if (existingRole.index != index.ToString())
                                        {
                                            deleteRoleSparql += " tpl:R97483568938 \"" + existingRole.index + "\"^^xsd:int ; ";
                                            insertRoleSparql += " tpl:R97483568938 \"" + index + "\"^^xsd:int ; ";
                                        }

                                        //close role statements and append to each block if needed.
                                        if (hasDeletes)
                                        {
                                            deleteRoleSparql = deleteRoleSparql.Insert(deleteRoleSparql.LastIndexOf(";"), ". ");
                                            deleteRoleSparql = deleteRoleSparql.Remove(deleteRoleSparql.LastIndexOf(";"));
                                            deleteSparql += deleteRoleSparql;
                                            whereSparql += whereRoleSparql;
                                        }

                                        if (hasInserts)
                                        {
                                            insertRoleSparql = insertRoleSparql.Insert(insertRoleSparql.LastIndexOf(";"), ". ");
                                            insertRoleSparql = insertRoleSparql.Remove(insertRoleSparql.LastIndexOf(";"));
                                            insertSparql += insertRoleSparql;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Insert New Role

                                        string roleLabel = String.Empty;
                                        string roleDescription = String.Empty;

                                        //form labels and comments (meta data)
                                        string metaSparql = String.Empty;
                                        foreach (QMXFName roleName in role.name)
                                        {
                                            roleLabel = roleName.value;
                                            metaSparql += " rdfs:label \"" + roleLabel + "\"^^xsd:string ; ";
                                        }

                                        if (role.description != null)
                                        {
                                            roleDescription = role.description.value;
                                            metaSparql += " rdfs:comment \"" + roleDescription + "\"^^xsd:string ; ";
                                        }

                                        //genertate new role id if needed.
                                        if (role.identifier == null || role.identifier == String.Empty)
                                        {
                                            string generatedId = string.Empty;

                                            string genName = "Role definition " + roleLabel;

                                            if (_useExampleRegistryBase)
                                                generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                            else
                                                generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                            role.identifier = generatedId;
                                        }

                                        roleIdentifier = "<" + role.identifier + ">";

                                        //add role to block
                                        insertSparql += roleIdentifier + " ";

                                        //role template
                                        insertSparql += " rdf:type tpl:R74478971040 ; ";

                                        //add type to block
                                        if (role.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                          || role.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                        {
                                            insertSparql += " rdf:type owl:DataTypeProperty ; ";
                                        }
                                        else
                                        {
                                            insertSparql += " rdf:type owl:ObjectProperty ; "
                                                    + " rdf:type owl:FunctionalProperty ; ";
                                        }

                                        //add metaData to block
                                        insertSparql += metaSparql;

                                        //add template possessor role to block
                                        insertSparql += " rdfs:domain " + identifier + " ; ";

                                        //add range to block
                                        insertSparql += "rdfs:range <" + role.range + "> ; ";

                                        //add role index to block
                                        insertSparql += "tpl:R97483568938 \"" + index + "\"^^xsd:int ; ";

                                        //close role statement.
                                        insertSparql = insertSparql.Insert(insertSparql.LastIndexOf(";"), ".");
                                        insertSparql = insertSparql.Remove(insertSparql.LastIndexOf(";"));
                                        #endregion
                                    }

                                    index++;
                                }

                                foreach (RoleDefinition role in existingTemplate.roleDefinition)
                                {
                                    #region Delete Leftover Roles
                                    string roleIdentifier = "<" + role.identifier + ">";

                                    string deleteRoleSparql = roleIdentifier + " ";
                                    string whereRoleSparql = roleIdentifier + " ?px" + role.index + " ?ox" + role.index + " . ";

                                    string label = String.Empty;
                                    foreach (QMXFName name in role.name)
                                    {
                                        label = name.value; //the last label will be used.

                                        deleteRoleSparql += "rdfs:label \"" + label + "\"^^xsd:string ; ";
                                    }

                                    if (role.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                      || role.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                    {
                                        deleteRoleSparql += " rdf:type owl:DataTypeProperty ; ";
                                    }
                                    else
                                    {
                                        deleteRoleSparql += " rdf:type owl:ObjectProperty ; "
                                                + " rdf:type owl:FunctionalProperty ; ";
                                    }

                                    deleteRoleSparql += " rdfs:domain " + identifier + " ; ";

                                    deleteRoleSparql += "rdfs:range <" + role.range + "> ; "
                                      + " tpl:R97483568938 \"" + role.index + "\"^^xsd:int ; ";

                                    deleteRoleSparql = deleteRoleSparql.Insert(deleteRoleSparql.LastIndexOf(";"), ".");
                                    deleteRoleSparql = deleteRoleSparql.Remove(deleteRoleSparql.LastIndexOf(";"));

                                    deleteSparql += deleteRoleSparql;
                                    whereSparql += whereRoleSparql;

                                    #endregion
                                }

                                deleteSparql += "}";
                                insertSparql += "}";
                                whereSparql += "}";

                                if (deleteSparql != "DELETE { }")
                                {
                                    sparql += deleteSparql;
                                    sparql += insertSparql;
                                    sparql += whereSparql;
                                }
                                else
                                {
                                    sparql += insertSparql;
                                    sparql = sparql.Replace("INSERT {", "INSERT DATA {");
                                }

                                #endregion

                                //post sparql
                                response = PostToRepository(repository, sparql);

                                _response.Append(response);
                                if (response.Level == StatusLevel.Error)
                                {
                                    throw new Exception("Error while Updating an existing template.");
                                }

                                status = new Status
                                {
                                    Level = StatusLevel.Success,
                                };

                                status.Messages.Add("Successfully updated template in repository, " + repository.name + ".");
                            }
                        }
                    }
                    #endregion Template Definitions

                    #region Template Qualifications
                    else if (qmxf.templateQualifications.Count > 0)
                    {
                        foreach (TemplateQualification template in qmxf.templateQualifications)
                        {
                            string ID = string.Empty;
                            string id = string.Empty;
                            string label = string.Empty;
                            string generatedTempId = string.Empty;
                            string templateName = string.Empty;
                            string roleDefinition = string.Empty;
                            string specialization = string.Empty;
                            string deleteSparql = string.Empty;
                            string insertSparql = string.Empty;
                            string whereSparql = string.Empty;
                            int index;

                            ID = template.identifier;

                            string sparql = prefixSparql;
                            QMXF existingQmxf = new QMXF();

                            if (ID != null)
                            {
                                id = ID.Substring(ID.LastIndexOf("#") + 1);
                                existingQmxf = GetTemplate(id, repository);
                                ID = "tpl:" + template.identifier;
                            }

                            String identifier = ID;
                            if (existingQmxf.templateQualifications.Count == 0)
                            {
                                #region Form Insert SPARQL
                                foreach (QMXFName name in template.name)
                                {
                                    label = name.value;

                                    //ID generator
                                    templateName = "Template qualification " + label;
                                    /// TODO: change to class registry base
                                    if (_useExampleRegistryBase)
                                        generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                    else
                                        generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);
                                    ID = "<" + generatedTempId + ">";
                                    Utility.WriteString("\n" + ID + "\t" + label, "TempQual IDs.log", true);
                                    specialization = Utility.GetQNameFromUri(template.qualifies);
                                    sparql += "_:spec rdf:type dm:Specialization ;\n"
                                            + "dm:hasSuperclass " + specialization + " ;\n"
                                            + "dm:hasSubclass " + ID + " .\n";

                                    //sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
                                    if (template.description.Count == 0)
                                    {
                                        sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string .\n";
                                    }
                                    else
                                    {
                                        sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ;\n";
                                    }
                                    foreach (Description descr in template.description)
                                    {
                                        if (string.IsNullOrEmpty(descr.value)) continue;
                                        sparql += " rdfs:comment \"" + descr.value + "\"^^xsd:string .\n";
                                    }

                                    int i = 0;
                                    foreach (RoleQualification role in template.roleQualification)
                                    {
                                        string roleID = Utility.GetQNameFromUri(role.qualifies);
                                        string roleLabel = string.Empty;
                                        string roleDescription = string.Empty;
                                        string generatedId = string.Empty;
                                        string genName = string.Empty;

                                        //ID generator
                                        genName = "Role definition " + roleLabel;
                                        /// TODO: change to template registry base
                                        if (_useExampleRegistryBase)
                                            generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                        else
                                            generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);
                                        roleID = Utility.GetQNameFromUri(generatedId);

                                        //roleID = role.identifier;
                                        foreach (QMXFName roleName in role.name)
                                        {
                                            roleLabel = roleName.value;
                                            Utility.WriteString("\n" + roleID + "\t" + roleLabel, "RoleQual IDs.log", true);

                                            //roleDescription = role.description.value;
                                            roleDescription = String.Join(" ", role.description);
                                        }
                                        //append role to sparql query
                                        //value restriction
                                        if (role.value != null && !String.IsNullOrEmpty(role.value.text))
                                        {
                                            string roleValueAs = role.value.As;
                                            if (role.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
                                            {
                                                roleValueAs = role.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", string.Empty);
                                            }

                                            sparql += "_:role" + i + " rdf:type tpl:R67036823327 ; "
                                                  + " tpl:R56456315674 " + ID + " ;\n"
                                                  + " tpl:R89867215482 " + roleID + " ;\n"
                                                  + " tpl:R29577887690 '" + role.value.text + "'^^xsd:" + roleValueAs + " .\n";

                                            i++;
                                        }

                                        else if (role.value != null && !String.IsNullOrEmpty(role.value.reference))
                                        {
                                            //reference restriction
                                            sparql += roleID + " rdf:type tpl:R40103148466 ;\n"
                                                  + " tpl:R49267603385 " + ID + " ;\n"
                                                  + " tpl:R30741601855 " + roleID + " ;\n"
                                                  + " tpl:R21129944603 " + Utility.GetQNameFromUri(role.value.reference) + " .\n";
                                        }
                                        else if (!String.IsNullOrEmpty(role.range))
                                        {
                                            //range restriction
                                            sparql += roleID + " rdf:type tpl:R76288246068 ;\n"
                                                    + " tpl:R99672026745 " + ID + " ;\n"
                                                    + " tpl:R91125890543 " + roleID + "> ; "
                                                    + " tpl:R98983340497 " + Utility.GetQNameFromUri(role.range) + " .\n";
                                        }

                                    }
                                    //status
                                    sparql += "_:status rdf:type tpl:R20247551573 ;\n"
                                              + "tpl:R64574858717 " + ID + " ;\n"
                                              + "tpl:R56599656536 rdl:R6569332477 ;\n"
                                              + "tpl:R61794465713 rdl:R3732211754 .\n";
                                    sparql += "}";
                                    //prefixSparql = prefixSparql.Insert(prefixSparql.LastIndexOf("."), "}").Remove(prefixSparql.Length - 1);
                                    response = PostToRepository(repository, prefixSparql);
                                }
                            }
                                #endregion Form Insert SPARQL
                            else
                            {
                                #region Form Update SPARQL
                                bool hasInserts = false;
                                bool hasDeletes = false;

                                TemplateQualification existingTemplate = existingQmxf.templateQualifications[0];

                                //start each block
                                deleteSparql += "DELETE { ";
                                insertSparql += "INSERT { ";
                                whereSparql += "WHERE { ";

                                //start template statement in each block
                                string deleteTemplateSparql = identifier + " ";
                                string insertTemplateSparql = identifier + " ";
                                string whereTemplateSparql = identifier + " ?p ?o . ";

                                //append changing labels to each block
                                foreach (QMXFName name in template.name)
                                {
                                    QMXFName existingName = existingTemplate.name.Find(n => n.lang == name.lang);

                                    if (existingName != null)
                                    {
                                        if (String.Compare(existingName.value, name.value, true) != 0)
                                        {
                                            hasDeletes = hasInserts = true;

                                            deleteTemplateSparql += "rdfs:label \"" + existingName.value + "\"^^xsd:string ; ";
                                            insertTemplateSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                        }
                                    }
                                    else
                                    {
                                        hasInserts = true;

                                        insertTemplateSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                    }
                                }

                                //append changing descriptions to each block
                                foreach (Description description in template.description)
                                {
                                    Description existingDescription = existingTemplate.description.Find(d => d.lang == description.lang);

                                    if (existingDescription != null)
                                    {
                                        if (String.Compare(existingDescription.value, description.value, true) != 0)
                                        {
                                            hasDeletes = hasInserts = true;

                                            deleteTemplateSparql += "rdfs:comment \"" + existingDescription.value + "\"^^xsd:string ; ";
                                            insertTemplateSparql += "rdfs:comment \"" + description.value + "\"^^xsd:string ; ";
                                        }
                                    }
                                    else
                                    {
                                        hasInserts = true;

                                        insertTemplateSparql += "rdfs:comment \"" + description.value + "\"^^xsd:string ; ";
                                    }
                                }

                                //append changing role count to each block
                                if (existingTemplate.roleQualification.Count != template.roleQualification.Count)
                                {
                                    hasDeletes = hasInserts = true;

                                    deleteTemplateSparql += "tpl:R35529169909 \"" + existingTemplate.roleQualification.Count + "\"^^xsd:int ; ";
                                    insertTemplateSparql += "tpl:R35529169909 \"" + template.roleQualification.Count + "\"^^xsd:int ; ";
                                }

                                //close template statements and append to each block if needed.
                                if (hasDeletes)
                                {
                                    deleteTemplateSparql = deleteTemplateSparql.Insert(deleteTemplateSparql.LastIndexOf(";"), ". ");
                                    deleteTemplateSparql = deleteTemplateSparql.Remove(deleteTemplateSparql.LastIndexOf(";"));
                                    deleteSparql += deleteTemplateSparql;
                                    whereSparql += whereTemplateSparql;
                                }

                                if (hasInserts)
                                {
                                    insertTemplateSparql = insertTemplateSparql.Insert(insertTemplateSparql.LastIndexOf(";"), ". ");
                                    insertTemplateSparql = insertTemplateSparql.Remove(insertTemplateSparql.LastIndexOf(";"));
                                    insertSparql += insertTemplateSparql;
                                }

                                index = 1;
                                foreach (RoleQualification role in template.roleQualification)
                                {
                                    string roleIdentifier = "<" + role.qualifies + ">";

                                    hasDeletes = hasInserts = false;

                                    //get existing role if it exists
                                    RoleQualification existingRole =
                                      existingTemplate.roleQualification.Find(r => r.qualifies == role.qualifies);

                                    //remove existing role from existing template, leftovers will be deleted later
                                    existingTemplate.roleQualification.Remove(existingRole);

                                    if (existingRole != null)
                                    {
                                        #region Process Changing Role

                                        //add the role to each block
                                        string deleteRoleSparql = roleIdentifier + " ";
                                        string insertRoleSparql = roleIdentifier + " ";
                                        string whereRoleSparql = roleIdentifier + " ?p" + index + " ?o" + index + " . ";

                                        //append changing labels to each block
                                        label = String.Empty;
                                        foreach (QMXFName name in role.name)
                                        {
                                            QMXFName existingName = existingRole.name.Find(n => n.lang == name.lang);

                                            if (existingName != null)
                                            {
                                                if (String.Compare(existingName.value, name.value, true) != 0)
                                                {
                                                    hasDeletes = hasInserts = true;

                                                    deleteRoleSparql += "rdfs:label \"" + existingName.value + "\"^^xsd:string ; ";
                                                    insertRoleSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                                }
                                            }
                                            else
                                            {
                                                hasInserts = true;

                                                insertRoleSparql += "rdfs:label \"" + name.value + "\"^^xsd:string ; ";
                                            }
                                        }

                                        //append changing description to each block
                                        for (int i = 0; i < existingRole.description.Count; i++)
                                        {
                                            if (String.Compare(existingRole.description[i].value, role.description[i].value, true) != 0)
                                            {
                                                hasDeletes = hasInserts = true;

                                                deleteRoleSparql += "rdfs:comment \"" + existingRole.description[i].value + "\"^^xsd:string ; ";
                                                insertRoleSparql += "rdfs:comment \"" + role.description[i].value + "\"^^xsd:string ; ";
                                            }
                                        }

                                        //append changing range to each block
                                        if (existingRole.range != role.range)
                                        {
                                            hasDeletes = hasInserts = true;

                                            if (existingRole.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                            || existingRole.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                            {
                                                deleteRoleSparql += " rdf:type owl:DataTypeProperty ; ";
                                            }
                                            else
                                            {
                                                deleteRoleSparql += " rdf:type owl:ObjectProperty ; rdf:type owl:FunctionalProperty ; ";
                                            }

                                            deleteRoleSparql += "rdfs:range <" + existingRole.range + "> ; ";

                                            if (!String.IsNullOrEmpty(role.range))
                                            {
                                                if (role.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                                || role.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                                {
                                                    insertRoleSparql += " rdf:type owl:DataTypeProperty ; ";
                                                }
                                                else
                                                {
                                                    insertRoleSparql += " rdf:type owl:ObjectProperty ; rdf:type owl:FunctionalProperty ; ";
                                                }

                                                insertRoleSparql += "rdfs:range <" + role.range + "> ; ";
                                            }
                                        }

                                        //append changing index to each block
                                        //if (existingRole.index != index.ToString())
                                        //{
                                        //  deleteRoleSparql += " tpl:R97483568938 \"" + existingRole.index + "\"^^xsd:int ; ";
                                        //  insertRoleSparql += " tpl:R97483568938 \"" + index + "\"^^xsd:int ; ";
                                        //}

                                        //close role statements and append to each block if needed.
                                        if (hasDeletes)
                                        {
                                            deleteRoleSparql = deleteRoleSparql.Insert(deleteRoleSparql.LastIndexOf(";"), ". ");
                                            deleteRoleSparql = deleteRoleSparql.Remove(deleteRoleSparql.LastIndexOf(";"));
                                            deleteSparql += deleteRoleSparql;
                                            whereSparql += whereRoleSparql;
                                        }

                                        if (hasInserts)
                                        {
                                            insertRoleSparql = insertRoleSparql.Insert(insertRoleSparql.LastIndexOf(";"), ". ");
                                            insertRoleSparql = insertRoleSparql.Remove(insertRoleSparql.LastIndexOf(";"));
                                            insertSparql += insertRoleSparql;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Insert New Role

                                        string roleLabel = String.Empty;
                                        string roleDescription = String.Empty;

                                        //form labels and comments (meta data)
                                        string metaSparql = String.Empty;
                                        foreach (QMXFName roleName in role.name)
                                        {
                                            roleLabel = roleName.value;
                                            metaSparql += " rdfs:label \"" + roleLabel + "\"^^xsd:string ; ";
                                        }

                                        if (role.description != null && role.description.Count > 0)
                                        {
                                            roleDescription = role.description[0].value;
                                            metaSparql += " rdfs:comment \"" + roleDescription + "\"^^xsd:string ; ";
                                        }

                                        //genertate new role id if needed.
                                        if (String.IsNullOrEmpty(role.qualifies))
                                        {
                                            string generatedId = string.Empty;

                                            string genName = "Role definition " + roleLabel;

                                            if (_useExampleRegistryBase)
                                                generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                            else
                                                generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                            role.qualifies = generatedId;
                                        }

                                        roleIdentifier = "<" + role.qualifies + ">";

                                        //add role to block
                                        insertSparql += roleIdentifier + " ";

                                        //role template
                                        insertSparql += " rdf:type tpl:R74478971040 ; ";

                                        //add type to block
                                        if (role.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                          || role.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                        {
                                            insertSparql += " rdf:type owl:DataTypeProperty ; ";
                                        }
                                        else
                                        {
                                            insertSparql += " rdf:type owl:ObjectProperty ; "
                                                    + " rdf:type owl:FunctionalProperty ; ";
                                        }

                                        //add metaData to block
                                        insertSparql += metaSparql;

                                        //add template possessor role to block
                                        insertSparql += " rdfs:domain " + identifier + " ; ";

                                        //add range to block
                                        insertSparql += "rdfs:range <" + role.range + "> ; ";

                                        //add role index to block
                                        insertSparql += "tpl:R97483568938 \"" + index + "\"^^xsd:int ; ";

                                        //close role statement.
                                        insertSparql = insertSparql.Insert(insertSparql.LastIndexOf(";"), ".");
                                        insertSparql = insertSparql.Remove(insertSparql.LastIndexOf(";"));
                                        #endregion
                                    }

                                    index++;
                                }

                                foreach (RoleQualification role in existingTemplate.roleQualification)
                                {
                                    #region Delete Leftover Roles
                                    string roleIdentifier = "<" + role.qualifies + ">";

                                    string deleteRoleSparql = roleIdentifier + " ";
                                    //string whereRoleSparql = roleIdentifier + " ?px" + role.index + " ?ox" + role.index + " . ";

                                    label = String.Empty;
                                    foreach (QMXFName name in role.name)
                                    {
                                        label = name.value; //the last label will be used.

                                        deleteRoleSparql += "rdfs:label \"" + label + "\"^^xsd:string ; ";
                                    }

                                    if (role.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                      || role.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                    {
                                        deleteRoleSparql += " rdf:type owl:DataTypeProperty ; ";
                                    }
                                    else
                                    {
                                        deleteRoleSparql += " rdf:type owl:ObjectProperty ; rdf:type owl:FunctionalProperty ; ";
                                    }

                                    deleteRoleSparql += " rdfs:domain " + identifier + " ; ";

                                    //deleteRoleSparql += "rdfs:range <" + role.range + "> ; " + " tpl:R97483568938 \"" + role.index + "\"^^xsd:int ; ";

                                    deleteRoleSparql = deleteRoleSparql.Insert(deleteRoleSparql.LastIndexOf(";"), ".");
                                    deleteRoleSparql = deleteRoleSparql.Remove(deleteRoleSparql.LastIndexOf(";"));

                                    deleteSparql += deleteRoleSparql;
                                    //whereSparql += whereRoleSparql;

                                    #endregion
                                }

                                deleteSparql += "}";
                                insertSparql += "}";
                                whereSparql += "}";

                                if (deleteSparql != "DELETE { }")
                                {
                                    sparql += deleteSparql;
                                    sparql += insertSparql;
                                    sparql += whereSparql;
                                }
                                else
                                {
                                    sparql += insertSparql;
                                    sparql = sparql.Replace("INSERT {", "INSERT DATA {");
                                }

                                //post sparql
                                response = PostToRepository(repository, sparql);

                                _response.Append(response);
                                if (response.Level == StatusLevel.Error)
                                {
                                    throw new Exception("Error while Updating an existing template.");
                                }

                                status = new Status
                                {
                                    Level = StatusLevel.Success,
                                };

                                status.Messages.Add("Successfully updated template in repository, " + repository.name + ".");

                                #endregion
                            }
                        }
                    }
                    #endregion Template Qualifications
                }
            }
            catch (Exception ex)
            {
                string errMsg = "Error in PostTemplate: " + ex;
                Status status = new Status();

                response.Level = StatusLevel.Error;
                status.Messages.Add(errMsg);
                response.Append(status);

                _logger.Error(errMsg);
            }
            return _response;
        }

        public QMXF GetPart8Class(string id)
        {
            QMXF qmxf = new QMXF();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                ClassDefinition classDefinition;
                QMXFName name;
                Description description;

                List<Specialization> specializations = new List<Specialization>();

                RefDataEntities resultEntities = new RefDataEntities();
                List<Entity> resultEnt = new List<Entity>();

                Query getPart8Class = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Class").Query;
                QueryBindings queryBindings = getPart8Class.Bindings;

                sparql = ReadSPARQL(getPart8Class.FileName);
                sparql = sparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
                    specializations = new List<Specialization>();

                    classDefinition = new ClassDefinition();

                    classDefinition.identifier = nsMap.GetNamespaceUri("tpl").ToString() + id;
                    classDefinition.repositoryName = repository.Name;
                    name = new QMXFName();

                    foreach (Dictionary<string, string> result in results)
                    {

                        description = new Description();

                        if (result.ContainsKey("label"))
                            name.value = result["label"];

                        if (result.ContainsKey("comment"))
                            description.value = result["comment"];

                        classDefinition.name.Add(name);
                        classDefinition.description.Add(description);

                    }
                    specializations = GetPart8Specializations(id, repository);
                    classDefinition.specialization = specializations;

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

        private List<Specialization> GetPart8Specializations(string id, Repository rep)
        {
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                List<Specialization> specializations = new List<Specialization>();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Specialization").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {
                        Specialization specialization = new Specialization();

                        string uri = String.Empty;
                        string label = String.Empty;
                        if (result.ContainsKey("uri"))
                        {
                            uri = result["uri"];
                            specialization.reference = uri;
                        }
                        if (result.ContainsKey("label"))
                        {
                            label = result["label"];
                        }
                        else
                        {
                            label = GetLabel(uri).Label;
                        }

                        specialization.label = label;
                        Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
                        //specializations.Add(specialization);
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

        //public RefDataEntities Part8Search(string query)
        //{
        //    try
        //    {
        //        return Part8SearchPage(query, 0, 0);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error in Search: " + ex);
        //        throw new Exception("Error while Searching " + query + ".\n" + ex.ToString(), ex);
        //    }
        //}

        //public RefDataEntities Part8SearchPage(string query, int start, int limit)
        //{
        //    RefDataEntities entities = null;
        //    int counter = 0;

        //    try
        //    {
        //        string sparql = String.Empty;
        //        string relativeUri = String.Empty;

        //        //Check the search History for Optimization
        //        if (_searchHistory.ContainsKey(query))
        //        {
        //            entities = _searchHistory[query];
        //        }
        //        else
        //        {
        //            RefDataEntities resultEntities = new RefDataEntities();

        //            Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "Part8ContainsSearch").Query;
        //            QueryBindings queryBindings = queryContainsSearch.Bindings;

        //            sparql = ReadSPARQL(queryContainsSearch.FileName);
        //            sparql = sparql.Replace("param1", query);

        //            foreach (Repository repository in _repositories)
        //            {
        //                SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);


        //                List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
        //                foreach (Dictionary<string, string> result in results)
        //                {
        //                    Entity resultEntity = new Entity
        //                    {
        //                        Uri = result["uri"],
        //                        Label = result["label"],
        //                        Repository = repository.Name
        //                    };

        //                    string key = resultEntity.Label;

        //                    if (resultEntities.Entities.ContainsKey(key))
        //                    {
        //                        key += ++counter;
        //                    }

        //                    resultEntities.Entities.Add(key, resultEntity);
        //                }
        //                results.Clear();
        //            }

        //            _searchHistory.Add(query, resultEntities);
        //            entities = resultEntities;
        //            entities.Total = resultEntities.Entities.Count;
        //        }

        //        if (limit > 0)
        //        {
        //            entities = GetRequestedPage(entities, start, limit);
        //        }

        //        _logger.Info(string.Format("SearchPage is returning {0} records", entities.Entities.Count));
        //        return entities;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("Error in SearchPage: " + e);
        //        throw new Exception("Error while Finding " + query + ".\n" + e.ToString(), e);
        //    }
        //}

        public Response oldPostClass(QMXF qmxf)
        {
            Status status = new Status();

            try
            {
                int count = 0;
                sparqlStr = new StringBuilder();
                sparqlStr.Append(prefix);

                string nameSparql = string.Empty;
                string specSparql = string.Empty;
                string classSparql = string.Empty;

                int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
                Repository target = _repositories[repository];

                Utility.WriteString("Number of classes to insert: " + qmxf.classDefinitions.Count.ToString(), "stats.log", true);
                foreach (ClassDefinition Class in qmxf.classDefinitions)
                {
                    bool existInTarget = false;
                    string ID = string.Empty;
                    string id = string.Empty;
                    string label = string.Empty;
                    string description = string.Empty;
                    string specialization = string.Empty;
                    string classification = string.Empty;
                    string generatedId = string.Empty;
                    string serviceUrl = string.Empty;
                    string className = string.Empty;
                    int classIndex = -1;

                    string language = string.Empty;
                    bool qn = false;
                    string qName = string.Empty;

                    if (target.IsReadOnly)
                    {
                        status.Level = StatusLevel.Error;
                        status.Messages.Add("Repository is Read Only");
                        _response.Append(status);
                        return _response;
                    }

                    ID = Utility.GetIdFromURI(Class.identifier);

                    QMXF q = new QMXF();
                    if (ID != null)
                    {
                        id = Utility.GetIdFromURI(ID);

                        q = GetClass(id, null, target);

                        foreach (ClassDefinition classFound in q.classDefinitions)
                        {
                            classIndex++;
                            if (classFound.repositoryName.Equals(_repositories[repository].Name))
                            {
                                existInTarget = true;
                                Utility.WriteString("Class found: " + q.classDefinitions[classIndex].name[0].value, "stats.log", true);
                                break;
                            }
                        }
                    }
                    #region !existInTarget
                    if (!existInTarget)
                    {
                        sparqlStr.AppendLine(insertData);
                        //add the class
                        foreach (QMXFName name in Class.name)
                        {
                            label = name.value.Split('@')[0];

                            if (string.IsNullOrEmpty(name.lang))
                                language = "@" + defaultLanguage;
                            else
                                language = "@" + name.lang;

                            count++;
                            Utility.WriteString("Inserting : " + label, "stats.log", true);

                            className = "Class definition " + label;
                            if (string.IsNullOrEmpty(ID))
                            {
                                if (_useExampleRegistryBase)
                                    generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], className);
                                else
                                    generatedId = CreateIdsAdiId(_settings["ClassRegistryBase"], className);

                                ID = Utility.GetIdFromURI(generatedId);

                                Utility.WriteString("\n" + ID + "\t" + label, "Class IDs.log", true);
                            }
                            sparqlStr.AppendLine(string.Format("  rdl:{0} rdf:type owl:Class ;", ID));

                            sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;",label , language));

                            foreach (Description descr in Class.description)
                            {
                                if (string.IsNullOrEmpty(descr.value))
                                    continue;
                                else
                                {
                                    if (string.IsNullOrEmpty(descr.lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = "@" + descr.lang;

                                    description = descr.value.Split('@')[0];

                                    sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;",description, language));
                                }
                            }

                           qn = nsMap.ReduceToQName(Class.entityType.reference, out qName);
                           if (qn)
                               sparqlStr.AppendLine(string.Format("  rdf:type {0} ;", qName));

                          

                            int specCount = Class.specialization.Count;
                            //append Specialization to sparql query
                            foreach (Specialization spec in Class.specialization)
                            {
                                if (string.IsNullOrEmpty(spec.reference))
                                    continue;

                                qn = nsMap.ReduceToQName(spec.reference, out qName);
                                if (--specCount > 0)
                                    sparqlStr.AppendLine(string.Format("  rdfs:subClassOf {0} ;", qName));
                                else
                                    sparqlStr.AppendLine(string.Format("  rdfs:subClassOf {0} .",qName));
                            }

                            foreach (Classification classif in Class.classification)
                            {
                                qn = nsMap.ReduceToQName(classif.reference, out qName);
                                if (qn)
                                {
                                    sparqlStr.AppendLine(string.Format("{0} rdfs:subClassOf rdl:{1} .", qName, ID));
                                }
                            }
                            sparqlStr.AppendLine("}");

                            if (!label.Equals(String.Empty))
                            {
                                Reset(label);
                            }
                            _response = PostToRepository(target, sparqlStr.ToString());
                        }
                    }
                    #endregion !existInTarget
                    #region existInTarget
                    else if (existInTarget)
                    {
                        ClassDefinition cd = q.classDefinitions[classIndex];

                        foreach (QMXFName name in cd.name)
                        {
                            sparqlStr = new StringBuilder();
                            sparqlStr.Append(prefix);
                            sparqlStr.AppendLine(deleteWhere);

                            if (string.IsNullOrEmpty(name.lang))
                                language = "@" + defaultLanguage;
                            else
                                language = "@" + name.lang;

                            label = name.value.Split('@')[0];

                            sparqlStr.AppendLine(string.Format("  rdl:{0} rdfs:label \"{1}{2}\"^^xsd:string ; ", ID, label, language));
                            sparqlStr.AppendLine("  ?property ?value . ");
                            sparqlStr = sparqlStr.AppendLine("}");

                            _response = PostToRepository(target, sparqlStr.ToString());

                            foreach (Classification classif in cd.classification)
                            {
                                sparqlStr = new StringBuilder();
                                sparqlStr.Append(prefix);
                                sparqlStr.AppendLine(deleteWhere);
                                
                                qn = nsMap.ReduceToQName(classif.reference, out qName);
                                if (qn)
                                {
                                    sparqlStr.AppendLine("  ?a rdf:type dm:Classification .");
                                    sparqlStr.AppendLine(string.Format("  ?a dm:hasClassifier {0} .", qName));
                                    sparqlStr.AppendLine(string.Format("  ?a dm:hasClassified  rdl:{0} .", ID));
                                    sparqlStr.AppendLine("}");

                                    _response = PostToRepository(target, sparqlStr.ToString());
                                }
                            }

                            foreach (Specialization spec in cd.specialization)
                            {
                                sparqlStr = new StringBuilder();
                                sparqlStr.Append(prefix);
                                sparqlStr.AppendLine(deleteWhere);

                                qn = nsMap.ReduceToQName(spec.reference, out qName);
                                if (qn)
                                {
                                    sparqlStr.AppendLine("  ?a rdf:type dm:Specialization .");
                                    sparqlStr.AppendLine(string.Format("  ?a dm:hasSuperclass {0} .", qName));
                                    sparqlStr.AppendLine(string.Format("  ?a dm:hasSubclass rdl:{0} .", ID));
                                    sparqlStr.AppendLine("}");

                                    _response = PostToRepository(target, sparqlStr.ToString());
                                }
                            }
                        }

                        sparqlStr = new StringBuilder();
                        sparqlStr.Append(prefix);
                        sparqlStr.AppendLine(insertData);

                        foreach (QMXFName name in Class.name)
                        {
                            if (string.IsNullOrEmpty(name.lang))
                                language = "@" + defaultLanguage;
                            else
                                language = "@" + name.lang;

                            label = name.value.Split('@')[0];
                            count++;
                            Utility.WriteString("Inserting : " + label, "stats.log", true);

                            className = "Class definition " + label;
                            if (ID == null || ID == string.Empty)
                            {
                                if (_useExampleRegistryBase)
                                    generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], className);
                                else
                                    generatedId = CreateIdsAdiId(_settings["ClassRegistryBase"], className);

                                ID = Utility.GetIdFromURI(generatedId);

                                Utility.WriteString("\n" + ID + "\t" + label, "Class IDs.log", true);
                            }
                            sparqlStr.AppendLine(string.Format("  rdl:{0} rdf:type owl:Class ;", ID));

                            sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));

                            foreach (Description descr in Class.description)
                            {
                                if (string.IsNullOrEmpty(descr.value))
                                    continue;

                                if (string.IsNullOrEmpty(descr.lang))
                                    language = "@" + defaultLanguage;
                                else
                                    language = "@" + descr.lang;

                                description = descr.value.Split('@')[0];

                                sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", description, language));
                            }

                            qn = nsMap.ReduceToQName(Class.entityType.reference, out qName);
                            if (qn)
                                sparqlStr.AppendLine(string.Format("  rdf:type {0} ;", qName));

                            int specCount = Class.specialization.Count;
                            int classCount = Class.classification.Count;
                            //append Specialization to sparql query
                            foreach (Specialization spec in Class.specialization)
                            {
                                if (string.IsNullOrEmpty(spec.reference))
                                    continue;

                                /// Note: QMXF contains only superclass info while qxf and rdf contain superclass and subclass info
                                qn = nsMap.ReduceToQName(spec.reference, out qName);

                                if (--specCount > 0)
                                    sparqlStr.AppendLine(string.Format("  rdfs:subClassOf {0} ;",qName));
                                else
                                    sparqlStr.AppendLine(string.Format("  rdfs:subClassOf {0} .", qName));
                            }

                            foreach (Classification classif in Class.classification)
                            {
                                qn = nsMap.ReduceToQName(classif.reference, out qName);
                                if (qn)
                                {
                                    sparqlStr.AppendLine(string.Format("{0} rdfs:subClassOf rdl:{1} .", qName, ID));
                                }
                            }

                            sparqlStr.AppendLine("}");

                            if (!label.Equals(String.Empty))
                            {
                                Reset(label);
                            }
                            _response = PostToRepository(target, sparqlStr.ToString());
                        }
                    }
                    #endregion existInTarget
                }

                Utility.WriteString("Insertion Done", "stats.log", true);
                Utility.WriteString("Total classes inserted: " + count, "stats.log", true);

                return _response;
            }

            catch (Exception e)
            {
                Utility.WriteString(e.ToString(), "error.log");
                _logger.Error("Error in PostClass: " + e);
                throw e;
            }
        }

        public Response PostClass(QMXF qmxf)
        {
            Response response = new Response();
            response.Level = StatusLevel.Success;

            try
            {
                Repository repository = GetRepository(qmxf.targetRepository);

                if (repository == null || repository.IsReadOnly)
                {
                    Status status = new Status();
                    status.Level = StatusLevel.Error;

                    if (repository == null)
                        status.Messages.Add("Repository not found!");
                    else
                        status.Messages.Add("Repository [" + qmxf.targetRepository + "] is read-only!");

                    _response.Append(status);
                }
                else
                {
                    string registry = _useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["ClassRegistryBase"];
                    StringBuilder sparqlDelete = new StringBuilder();

                    foreach (ClassDefinition clsDef in qmxf.classDefinitions)
                    {
                        //string clsId = clsDef.identifier;
                        ///koos
                        string language = string.Empty;
                        bool qn = false;
                        string qName = string.Empty;
                        string clsId = Utility.GetIdFromURI(clsDef.identifier);
                        QMXF existingQmxf = new QMXF();

                        if (!String.IsNullOrEmpty(clsId))
                        {
                            //existingQmxf = GetClass(clsId.Substring(clsId.LastIndexOf("#") + 1));
                            existingQmxf = GetClass(clsId);
                        }

                        // delete class
                        if (existingQmxf.classDefinitions.Count > 0)
                        {
                            StringBuilder sparqlStmts = new StringBuilder();
                            //int count = 0;

                            //clsId = "<" + clsId + ">";

                            foreach (ClassDefinition existingClsDef in existingQmxf.classDefinitions)
                            {
                                foreach (QMXFName clsName in existingClsDef.name)
                                {
                                    //string clsLabel = clsName.value.Split('@')[0];

                                    if (string.IsNullOrEmpty(clsName.lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = "@" + clsName.lang;

                                    // delete label, entity type, and description
                                    //sparqlStmts.Append(string.Format("  rdl:{0} rdfs:label \"{1}{2}\"^^xsd:string ; ", clsId, clsLabel, language));
                                    //sparqlStmts.Append("  ?property ?value . ");

                                    QMXFName existingName = existingClsDef.name.Find(n => n.lang == clsName.lang);

                                    if (existingName != null)
                                    {
                                        if (String.Compare(existingName.value, clsName.value, true) != 0)
                                        {
                                            sparqlStmts.Append(string.Format("  rdl:{0} rdfs:label \"{1}{2}\"^^xsd:string . ", clsId, existingName.value, clsName.lang));
                                        }
                                    }

                                    foreach (Description description in existingClsDef.description)
                                    {
                                        Description existingDescription = existingClsDef.description.Find(d => d.lang == description.lang);

                                        if (existingDescription != null)
                                        {
                                            if (String.Compare(existingDescription.value, description.value, true) != 0)
                                            {
                                                sparqlStmts.Append(string.Format("  rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string . ", clsId, existingDescription.value, description.lang));
                                            }
                                        }
                                    }

                                    // delete specialization
                                    foreach (Specialization spec in existingClsDef.specialization)
                                    {
                                        //string specVariable = "?v" + count++;
                                        //sparqlStmts.Append(specVariable + " rdf:type dm:Specialization . ");

                                        Specialization existingSpec = existingClsDef.specialization.Find(s => s.reference == spec.reference);

                                        if (existingSpec != null && existingSpec.reference != null)
                                        {
                                            if (String.Compare(existingSpec.reference, spec.reference, true) != 0)
                                            {
                                                qn = nsMap.ReduceToQName(existingSpec.reference, out qName);
                                                sparqlStmts.Append(string.Format("  ?a dm:hasSubclass rdl:{0} . ", qName));
                                            }
                                        }   
                                    }

                                    ///TODO
                                    // delete classification
                                    /*foreach (Classification clsif in existingClsDef.classification)
                                    {
                                        
                                    }*/
                                }
                            }

                            sparqlDelete.Append(" DELETE { ");
                            sparqlDelete.Append(sparqlStmts);
                            sparqlDelete.Append(" } ");

                            sparqlDelete.Append(" WHERE { ");
                            sparqlDelete.Append(sparqlStmts);
                            sparqlDelete.Append(" }; ");
                        }

                        // add class
                        StringBuilder sparqlAdd = new StringBuilder();
                        sparqlAdd.Append("INSERT DATA {");

                        foreach (QMXFName clsName in clsDef.name)
                        {
                            string clsLabel = clsName.value.Split('@')[0];
                            ///hanh
                            //string clsLabel = clsName.value;

                            if (string.IsNullOrEmpty(clsName.lang))
                                language = "@" + defaultLanguage;
                            else
                                language = "@" + clsName.lang;

                            if (String.IsNullOrEmpty(clsId) || !(clsId.StartsWith("<") && clsId.EndsWith(">")))
                            {
                                string newClsName = "Class definition " + clsLabel;
                                ///koos
                                clsId = CreateIdsAdiId(registry, newClsName);
                                clsId = Utility.GetIdFromURI(clsId);
                                ///hanh
                                //clsId = "<" + CreateIdsAdiId(registry, newClsName) + ">";  
                            }

                            // append label
                            sparqlAdd.Append(clsId + " rdfs:label \"" + clsLabel + "\"^^xsd:string . ");
                            sparqlAdd.Append(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", clsLabel, language));

                            // append entity type
                            if (clsDef.entityType != null && !String.IsNullOrEmpty(clsDef.entityType.reference))
                            {
                                qn = nsMap.ReduceToQName(clsDef.entityType.reference, out qName);
                                if (qn)
                                    sparqlAdd.Append(string.Format("  rdf:type {0} ;", qName));
                                ///hanh
                                //sparqlAdd.Append(clsId + " rdf:type <" + clsDef.entityType.reference + "> . ");
                            }

                            // append description
                            foreach (Description desc in clsDef.description)
                            {
                                if (!String.IsNullOrEmpty(desc.value))
                                {
                                    if (string.IsNullOrEmpty(desc.lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = "@" + desc.lang;

                                    string description = desc.value.Split('@')[0];
                                    sparqlAdd.Append(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", description, language));
                                    ///hanh
                                    //sparqlAdd.Append(clsId + " rdfs:comment \"" + desc.value + "\"^^xsd:string . ");
                                }
                            }

                            // append specialization
                            int specCount = clsDef.specialization.Count;
                            foreach (Specialization spec in clsDef.specialization)
                            {
                                if (!String.IsNullOrEmpty(spec.reference))
                                {
                                    qn = nsMap.ReduceToQName(spec.reference, out qName);
                                    if (--specCount > 0)
                                        sparqlAdd.Append(string.Format("  rdfs:subClassOf {0} ;", qName));
                                    else
                                        sparqlAdd.Append(string.Format("  rdfs:subClassOf {0} .", qName));
                                }
                            }

                            ///TODO
                            /* append classification
                            foreach (Classification clsif in clsDef.classification)
                            {
                                
                            }*/

                            sparqlAdd.Append("}");
                        }

                        sparqlBuilder.Append(sparqlDelete);
                        sparqlBuilder.Append(sparqlAdd);

                        string sparql = sparqlBuilder.ToString();
                        Response postResponse = PostToRepository(repository, sparql);
                        response.Append(postResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = "Error in PostClass: " + ex;
                Status status = new Status();

                response.Level = StatusLevel.Error;
                status.Messages.Add(errMsg);
                response.Append(status);

                _logger.Error(errMsg);
            }

            return response;
        }

        #endregion Part8

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
    }
}