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
        private const string egPrefix = "PREFIX eg: <http://example.org/data#>";
        private const string rdlPrefix = "PREFIX rdl: <http://rdl.rdlfacade.org/data#>";
        private const string tplPrefix = "PREFIX tpl: <http://tpl.rdlfacade.org/data#>";
        private const string rdfPrefix = "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>";
        private const string rdfsPrefix = "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>";
        private const string xsdPrefix = "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>";
        private const string dmPrefix = "PREFIX dm: <http://dm.rdlfacade.org/data#>";
        private const string owlPrefix = "PREFIX owl: <http://www.w3.org/2002/07/owl#>";
        private const string owl2xmlPrefix = "PREFIX owl2xml: <http://www.w3.org/2006/12/owl2-xml#>";
        private const string p8Prefix = "PREFIX p8: <http://standards.tc184-sc4.org/iso/15926/-8/template-model#>";
        private const string templatesPrefix = "PREFIX templates: <http://standards.tc184-sc4.org/iso/15926/-8/templates#>";
        private const string insertData = "INSERT DATA {";
        private const string delWhere = "DELETE WHERE {";

        private int _pageSize = 0;
        private bool _useExampleRegistryBase = false;
        private WebCredentials _registryCredentials = null;
        private WebProxyCredentials _proxyCredentials = null;
        private Repositories _repositories = null;
        private Queries _queries = null;
        private static Dictionary<string, RefDataEntities> _searchHistory = new Dictionary<string, RefDataEntities>();
        private IKernel _kernel = null;
        private ReferenceDataSettings _settings = null;

        private StringBuilder prefix = null;
        private StringBuilder sparqlStr = null;

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
                prefix = new StringBuilder();
                prefix.AppendLine(egPrefix);
                prefix.AppendLine(rdlPrefix);
                prefix.AppendLine(tplPrefix);
                prefix.AppendLine(rdfPrefix);
                prefix.AppendLine(rdfsPrefix);
                prefix.AppendLine(xsdPrefix);
                prefix.AppendLine(dmPrefix);
                prefix.AppendLine(owlPrefix);
                prefix.AppendLine(owl2xmlPrefix);
                prefix.AppendLine(p8Prefix);
                prefix.AppendLine(templatesPrefix);
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

        #region Prototype Part8

        public List<Entity> Find(string query)
        {
            List<Entity> queryResult = new List<Entity>();
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Query queryExactSearch = (Query)_queries.FirstOrDefault(c => c.Key == "ExactSearch").Query;
                QueryBindings queryBindings = queryExactSearch.Bindings;

                sparql = ReadSPARQL(queryExactSearch.FileName);

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

                        queryResult.Add(resultEntity);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error in Find: " + e);
                throw new Exception("Error while Finding " + query + ".\n" + e.ToString(), e);
            }
            return queryResult;
        }

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
                string sparql1 = String.Empty;
                string sparql2 = String.Empty;
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

                    Query queryTemplateContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "TemplateContainsSearch").Query;
                    QueryBindings queryBindings2 = queryTemplateContainsSearch.Bindings;

                    sparql1 = ReadSPARQL(queryContainsSearch.FileName);
                    sparql1 = sparql1.Replace("param1", query);

                    sparql2 = ReadSPARQL(queryTemplateContainsSearch.FileName);
                    sparql2 = sparql2.Replace("param1", query);
                    sparql2 = sparql2.Replace("param2", "0"); //TODO:Remove hard-coded OFFSET parameter

                    foreach (Repository repository in _repositories)
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparql1);

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

                        //for Camelot repositories, search using "TemplateContainsSearch" too
                        if (repository.RepositoryType == RepositoryType.Camelot)
                        {
                            sparqlResults = QueryFromRepository(repository, sparql2);

                            results = BindQueryResults(queryBindings, sparqlResults);
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

        private string GetLabel(string uri)
        {
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
                            label = result["label"];
                        }
                    }
                }

                return label;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetLabel: " + e);
                throw new Exception("Error while Getting Label for " + uri + ".\n" + e.ToString(), e);
            }
        }

        private List<Classification> GetClassifications(string id)
        {
            QMXF qmxf = new QMXF();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                List<Classification> classifications = new List<Classification>();


                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassification").Query;
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

                        if (result.ContainsKey("label"))
                            label = result["label"];
                        else
                            label = GetLabel(uri);

                        classification.label = label;
                        Utility.SearchAndInsert(classifications, classification, Classification.sortAscending());
                        //classifications.Add(classification);
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

        private List<Specialization> GetSpecializations(string id)
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
                                label = GetLabel(uri);
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
                                label = GetLabel(uri);
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

        public string GetClassLabel(string id)
        {
            return GetLabel("http://rdl.rdlfacade.org/data#" + id);
        }

        public QMXF GetClass(string id)
        {
            return GetClass(id, String.Empty);
        }

        public QMXF GetClass(string id, string namespaceUrl)
        {
            QMXF qmxf = new QMXF();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                ClassDefinition classDefinition;
                QMXFName name;
                Description description;
                QMXFStatus status;
                List<Classification> classifications = new List<Classification>();
                List<Specialization> specializations = new List<Specialization>();

                RefDataEntities resultEntities = new RefDataEntities();
                List<Entity> resultEnt = new List<Entity>();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetClass").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);

                if (namespaceUrl == String.Empty || namespaceUrl == null)
                    namespaceUrl = @"http://rdl.rdlfacade.org/data";

                string uri = namespaceUrl + "#" + id;

                sparql = sparql.Replace("param1", uri);

                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
                    classifications = new List<Classification>();
                    specializations = new List<Specialization>();

                    foreach (Dictionary<string, string> result in results)
                    {
                        classDefinition = new ClassDefinition();


                        classDefinition.identifier = "http://rdl.rdlfacade.org/data#" + id;
                        classDefinition.repositoryName = repository.Name;
                        name = new QMXFName();
                        description = new Description();
                        status = new QMXFStatus();

                        if (result.ContainsKey("label"))
                            name.value = result["label"];

                        if (result.ContainsKey("type"))
                            classDefinition.entityType = new EntityType { reference = result["type"] };

                        //legacy properties
                        if (result.ContainsKey("definition"))
                            description.value = result["definition"];

                        if (result.ContainsKey("creator"))
                            status.authority = result["creator"];

                        if (result.ContainsKey("creationDate"))
                            status.from = result["creationDate"];

                        if (result.ContainsKey("class"))
                            status.Class = result["class"];

                        //camelot properties
                        if (result.ContainsKey("comment"))
                            description.value = result["comment"];

                        if (result.ContainsKey("authority"))
                            status.authority = result["authority"];

                        if (result.ContainsKey("recorded"))
                            status.Class = result["recorded"];

                        if (result.ContainsKey("from"))
                            status.from = result["from"];

                        classDefinition.name.Add(name);
                        classDefinition.description.Add(description);
                        classDefinition.status.Add(status);

                        classifications = GetClassifications(id);
                        specializations = GetSpecializations(id);
                        classDefinition.classification = classifications;
                        classDefinition.specialization = specializations;

                        qmxf.classDefinitions.Add(classDefinition);
                    }

                }

                return qmxf;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetClass: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        public List<Entity> GetSuperClasses(string id)
        {
            List<Entity> queryResult = new List<Entity>();

            try
            {
                List<Specialization> specializations = GetSpecializations(id);

                foreach (Specialization specialization in specializations)
                {
                    string uri = specialization.reference;
                    string label = specialization.label;

                    if (label == null)
                        label = GetLabel(uri);

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

        public List<Entity> GetAllSuperClasses(string id)
        {
            List<Entity> list = new List<Entity>();
            return GetAllSuperClasses(id, list);
        }

        public List<Entity> GetAllSuperClasses(string id, List<Entity> list)
        {
            //List<Entity> queryResult = new List<Entity>();

            try
            {

                List<Specialization> specializations = GetSpecializations(id);
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
                        label = GetLabel(uri);

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
                        //list.Add(resultEntity);
                        GetAllSuperClasses(trimmedUri, list);
                    }
                }

                //list.Sort(Entity.sortAscending());
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetAllSuperClasses: " + e);
                throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
            }

            return list;
        }

        public List<Entity> GetSubClasses(string id)
        {
            List<Entity> queryResult = new List<Entity>();

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

        public List<Entity> GetClassTemplates(string id)
        {
            List<Entity> queryResult = new List<Entity>();
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

        private List<RoleDefinition> GetRoleDefintion(string id)
        {
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();
                //List<Classification> classifications = new List<Classification>();

                List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();

                RefDataEntities resultEntities = new RefDataEntities();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetRoles").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
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

        private List<RoleQualification> GetRoleQualification(string id)
        {
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                string sparql1 = String.Empty;
                string relativeUri1 = String.Empty;

                string sparql2 = String.Empty;
                string relativeUri2 = String.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();
                //List<Classification> classifications = new List<Classification>();

                List<RoleQualification> roleQualifications = new List<RoleQualification>();

                RefDataEntities resultEntities = new RefDataEntities();
                RefDataEntities resultEntities1 = new RefDataEntities();
                RefDataEntities resultEntities2 = new RefDataEntities();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetRangeRestriction").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                Query queryContainsSearch1 = (Query)_queries.FirstOrDefault(c => c.Key == "GetReferenceRestriction").Query;
                QueryBindings queryBindings1 = queryContainsSearch1.Bindings;

                Query queryContainsSearch2 = (Query)_queries.FirstOrDefault(c => c.Key == "GetValueRestriction").Query;
                QueryBindings queryBindings2 = queryContainsSearch2.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                sparql1 = ReadSPARQL(queryContainsSearch1.FileName);
                sparql1 = sparql1.Replace("param1", id);

                sparql2 = ReadSPARQL(queryContainsSearch2.FileName);
                sparql2 = sparql2.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);
                    SPARQLResults sparqlResults1 = QueryFromRepository(repository, sparql1);
                    SPARQLResults sparqlResults2 = QueryFromRepository(repository, sparql2);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
                    List<Dictionary<string, string>> results1 = BindQueryResults(queryBindings1, sparqlResults1);
                    List<Dictionary<string, string>> results2 = BindQueryResults(queryBindings2, sparqlResults2);

                    List<Dictionary<string, string>> combinedResults = MergeLists(MergeLists(results, results1), results2);

                    foreach (Dictionary<string, string> result in combinedResults)
                    {

                        RoleQualification roleQualification = new RoleQualification();

                        string uri = String.Empty;
                        if (result.ContainsKey("qualifies"))
                        {
                            uri = result["qualifies"];
                            roleQualification.qualifies = uri;
                        }
                        if (result.ContainsKey("name"))
                        {
                            string nameValue = result["name"];

                            if (nameValue == null)
                            {
                                nameValue = GetLabel(uri);
                            }

                            QMXFName name = new QMXFName
                            {
                                value = nameValue
                            };

                            roleQualification.name.Add(name);
                        }
                        else
                        {
                            string nameValue = GetLabel(uri);

                            if (nameValue == String.Empty)
                                nameValue = "tpl:" + Utility.GetIdFromURI(uri);

                            QMXFName name = new QMXFName
                            {
                                value = nameValue
                            };

                            roleQualification.name.Add(name);
                        }
                        if (result.ContainsKey("range"))
                        {
                            roleQualification.range = result["range"];
                        }
                        if (result.ContainsKey("reference"))
                        {
                            QMXFValue value = new QMXFValue
                            {
                                reference = result["reference"]
                            };

                            roleQualification.value = value;
                        }
                        if (result.ContainsKey("value"))
                        {
                            QMXFValue value = new QMXFValue
                            {
                                text = result["value"],
                                As = result["value_dataType"]
                            };

                            roleQualification.value = value;
                        }
                        Utility.SearchAndInsert(roleQualifications, roleQualification, RoleQualification.sortAscending());
                        //roleQualifications.Add(roleQualification);
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

        private List<TemplateDefinition> GetTemplateDefinition(string id)
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

                        templateDefinition.roleDefinition = GetRoleDefintion(id);
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

        public QMXF GetTemplate(string id, string templateType)
        {
            QMXF qmxf = new QMXF();
            List<TemplateQualification> templateQualification = null;
            List<TemplateDefinition> templateDefinition = null;
            try
            {
                if (templateType == "Qualification")
                {
                    templateQualification = GetTemplateQualification(id);
                }
                else
                {
                    templateDefinition = GetTemplateDefinition(id);
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
                List<TemplateQualification> templateQualification = GetTemplateQualification(id);

                if (templateQualification.Count > 0)
                {
                    qmxf.templateQualifications = templateQualification;
                }
                else
                {
                    List<TemplateDefinition> templateDefinition = GetTemplateDefinition(id);
                    qmxf.templateDefinitions = templateDefinition;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetTemplate: " + ex);
            }

            return qmxf;
        }


        private List<TemplateQualification> GetTemplateQualification(string id)
        {
            TemplateQualification templateQualification = null;
            List<TemplateQualification> templateQualificationList = new List<TemplateQualification>();

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                RefDataEntities resultEntities = new RefDataEntities();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetTemplateQualification").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);


                foreach (Repository repository in _repositories)
                {
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

                        templateQualification.roleQualification = GetRoleQualification(id);
                        templateQualificationList.Add(templateQualification);
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

        public Response PostTemplate(QMXF qmxf)
        {
            Status status = new Status();

            try
            {
                Response response = null;
                string sparql = "PREFIX eg: <http://example.org/data#> "
                                + "PREFIX rdl: <http://rdl.rdlfacade.org/data#> "
                                + "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> "
                                + "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> "
                                + "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#> "
                                + "PREFIX dm: <http://dm.rdlfacade.org/data#> "
                                + "PREFIX tpl: <http://tpl.rdlfacade.org/data#> "
                                + "PREFIX owl: <http://www.w3.org/2002/07/owl#> "
                                + "INSERT DATA { ";

                int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
                Repository source = _repositories[repository];

                if (source.IsReadOnly)
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
                        string label = string.Empty;
                        string description = string.Empty;
                        string generatedTempId = string.Empty;
                        string templateName = string.Empty;
                        string roleDefinition = string.Empty;
                        int index = 1;
                        string nameSparql = string.Empty;
                        string specSparql = string.Empty;
                        string classSparql = string.Empty;
                        int templateIndex = -1;

                        ID = template.identifier;

                        QMXF q = new QMXF();
                        if (ID != null)
                        {
                            if (!ID.StartsWith("tpl:"))
                            {
                                id = ID.Substring((ID.LastIndexOf("#") + 1), ID.Length - (ID.LastIndexOf("#") + 1));
                            }
                            else
                            {
                                id = ID.Substring(4, (ID.Length - 4));
                                ID = "http://tpl.rdlfacade.org/data#" + ID;
                            }
                            q = GetTemplate(id, "Definition");
                            foreach (TemplateDefinition templateFound in q.templateDefinitions)
                            {
                                templateIndex++;
                                if (templateFound.repositoryName.Equals(_repositories[repository]))
                                {
                                    ID = "<" + ID + ">";
                                    Utility.WriteString("Template found: " + q.templateDefinitions[templateIndex].name[0].value, "stats.log", true);
                                    break;
                                }
                            }
                        }

                        if (q.templateDefinitions.Count == 0)
                        {
                            foreach (QMXFName name in template.name)
                            {
                                label = name.value;

                                //ID generator
                                templateName = "Template definition " + label;
                                /// TODO: change to class registry base
                                if (_useExampleRegistryBase)
                                    generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                else
                                    generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);
                                ID = "<" + generatedTempId + ">";
                                Utility.WriteString("\n" + ID + "\t" + label, "TempDef IDs.log", true);
                                //append description to sparql query
                                if (template.description.Count == 0)
                                {
                                    sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
                                }
                                else
                                {
                                    sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                                }
                                foreach (Description descr in template.description)
                                {
                                    description = descr.value;
                                    sparql += " rdfs:comment \"" + description + "\"^^xsd:string ; "
                                            + " tpl:R35529169909 \"" + template.roleDefinition.Count + "\"^^xsd:int . ";
                                }

                                foreach (RoleDefinition role in template.roleDefinition)
                                {

                                    string roleID = string.Empty;
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

                                    roleID = "<" + generatedId + ">";

                                    //roleID = role.identifier;
                                    foreach (QMXFName roleName in role.name)
                                    {
                                        roleLabel = roleName.value;
                                        roleDescription = role.description.value;
                                        Utility.WriteString("\n" + roleID + "\t" + roleLabel, "RoleDef IDs.log", true);
                                    }
                                    //append role to sparql query
                                    sparql += roleID + " rdf:type tpl:R74478971040 ; ";

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

                                    sparql += " rdfs:domain " + ID + " ; "
                                            + " rdfs:range <" + role.range + "> ; "
                                            + " tpl:R97483568938 \"" + index + "\"^^xsd:int ; "
                                            + " rdfs:label \"" + roleLabel + "\"@en ; "
                                            + " rdfs:comment \"" + roleDescription + "\"@en . ";

                                    index++;
                                }
                                //status
                                sparql += "_:status rdf:type tpl:R20247551573 ; "
                                          + "tpl:R64574858717 " + ID + " ; "
                                          + "tpl:R56599656536 rdl:R6569332477 ; "
                                          + "tpl:R61794465713 rdl:R3732211754 . ";

                                sparql = sparql.Insert(sparql.LastIndexOf("."), "}").Remove(sparql.Length - 1);
                                response = PostToRepository(source, sparql);
                            }
                        }
                        else
                        {
                            TemplateDefinition td = q.templateDefinitions[templateIndex];
                            string rName = string.Empty;

                            sparql = sparql.Replace("INSERT DATA { ", "DELETE DATA { ");
                            foreach (QMXFName name in td.name)
                            {
                                ID = td.identifier;
                                label = name.value;
                                nameSparql = sparql + ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                                foreach (Description descr in td.description)
                                {
                                    description = descr.value;
                                    nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string ; "
                                                + "tpl:R35529169909 \"" + td.roleDefinition.Count + "\"^^xsd:int . ";
                                }
                                index = 0;
                                foreach (RoleDefinition rd in td.roleDefinition)
                                {
                                    foreach (QMXFName rn in rd.name)
                                    {
                                        rName = rn.value;
                                        nameSparql += "<" + rd.identifier + "> rdfs:label \"" + rName + "\"@en ; ";
                                        nameSparql += "rdfs:comment \"" + rd.description.value + "\"@en ; ";
                                        if (rd.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                          || rd.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                        {
                                            nameSparql += " rdf:type owl:DataTypeProperty ; ";
                                        }
                                        else
                                        {
                                            nameSparql += " rdf:type owl:ObjectProperty ; "
                                                    + " rdf:type owl:FunctionalProperty ; ";
                                        }
                                        nameSparql += "rdf:type tpl:R74478971040 ; "
                                              + " rdfs:domain " + ID + " ; "
                                              + " rdfs:range <" + rd.range + "> ; "
                                              + " tpl:R97483568938 \"" + rd.description.value + "\"^^xsd:int . ";
                                    }
                                }
                                nameSparql = nameSparql.Insert(nameSparql.LastIndexOf("."), "}").Remove(nameSparql.Length - 1);
                                response = PostToRepository(source, nameSparql);
                                //}
                            }
                            foreach (QMXFName name in template.name)
                            {
                                string gen = String.Empty;
                                string generatedId = string.Empty;
                                string roleID = string.Empty;

                                label = name.value;
                                sparql = sparql.Replace("DELETE DATA { ", "INSERT DATA { ");
                                nameSparql = sparql + ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                                foreach (Description descr in template.description)
                                {
                                    description = descr.value;
                                    nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string ; "
                                               + "tpl:R35529169909 \"" + template.roleDefinition.Count + "\"^^xsd:int . ";
                                }
                                index = 0;
                                foreach (RoleDefinition def in template.roleDefinition)
                                {

                                    foreach (QMXFName defName in def.name)
                                    {
                                        if (def.identifier != null)
                                        {
                                            roleID = def.identifier;
                                        }
                                        else
                                        {
                                            gen = "Role definition " + label;
                                            /// TODO: change to template registry base
                                            if (_useExampleRegistryBase)
                                                generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], gen);
                                            else
                                                generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], gen);

                                            roleID = generatedId;
                                        }
                                        nameSparql += "<" + roleID + "> rdfs:label \"" + defName.value + "\"@en ; ";
                                        nameSparql += "rdfs:comment \"" + def.description.value + "\"@en ; ";

                                        if (def.range.StartsWith("http://www.w3.org/2000/01/rdf-schema#")
                                        || def.range.StartsWith("http://www.w3.org/2001/XMLSchema"))
                                        {
                                            nameSparql += " rdf:type owl:DataTypeProperty ; ";
                                        }
                                        else
                                        {
                                            nameSparql += " rdf:type owl:ObjectProperty ; "
                                                    + " rdf:type owl:FunctionalProperty ; ";
                                        }
                                        nameSparql += "rdf:type tpl:R74478971040 ; "
                                                + " rdfs:domain " + ID + " ; "
                                                + " rdfs:range <" + def.range + "> ; "
                                                + " tpl:R97483568938 \"" + ++index + "\"^^xsd:int . ";
                                    }
                                }
                                nameSparql = nameSparql.Insert(nameSparql.LastIndexOf("."), "}").Remove(nameSparql.Length - 1);
                            }
                            response = PostToRepository(source, nameSparql);
                        }
                    }
                }
                #endregion
                #region Template Qualifications
                else//template qualification
                {
                    if (qmxf.templateQualifications.Count > 0)
                    {
                        foreach (TemplateQualification template in qmxf.templateQualifications)
                        {
                            string ID = string.Empty;
                            string id = string.Empty;
                            string label = string.Empty;
                            string description = string.Empty;
                            //ID = template.identifier.Remove(0, 1);
                            string generatedTempId = string.Empty;
                            string templateName = string.Empty;
                            string roleDefinition = string.Empty;
                            string specialization = string.Empty;
                            string nameSparql = string.Empty;
                            string specSparql = string.Empty;
                            string classSparql = string.Empty;

                            ID = template.identifier;

                            QMXF q = new QMXF();
                            if (ID != null)
                            {
                                id = ID.Substring((ID.LastIndexOf("#") + 1), ID.Length - (ID.LastIndexOf("#") + 1));
                                q = GetTemplate(id, "Qualification");
                                ID = "tpl:" + template.identifier;
                            }

                            if (q.templateQualifications.Count == 0)
                            {
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
                                    specialization = template.qualifies;
                                    sparql += "_:spec rdf:type dm:Specialization ; "
                                            + "dm:hasSuperclass <" + specialization + "> ; "
                                            + "dm:hasSubclass " + ID + " . ";

                                    //sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
                                    if (template.description.Count == 0)
                                    {
                                        sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
                                    }
                                    else
                                    {
                                        sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                                    }
                                    foreach (Description descr in template.description)
                                    {
                                        description = descr.value;
                                        sparql += " rdfs:comment \"" + description + "\"^^xsd:string . ";
                                    }

                                    int i = 0;
                                    foreach (RoleQualification role in template.roleQualification)
                                    {

                                        string roleID = string.Empty;
                                        string roleLabel = string.Empty;
                                        string roleDescription = string.Empty;
                                        string generatedId = string.Empty;
                                        string genName = string.Empty;

                                        //ID generator
                                        genName = "Role definition " + roleLabel;
                                        /// TODO: change to template registry base
                                        //if (_useExampleRegistryBase)
                                        //    generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                        //else
                                        //    generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);
                                        roleID = "<" + generatedId + ">";

                                        //roleID = role.identifier;
                                        foreach (QMXFName roleName in role.name)
                                        {
                                            roleLabel = roleName.value;
                                            Utility.WriteString("\n" + roleID + "\t" + roleLabel, "RoleQual IDs.log", true);
                                            foreach (Description desc in role.description)
                                            {
                                                roleDescription = desc.value;
                                            }
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
                                                  + " tpl:R56456315674 " + ID + " ; "
                                                  + " tpl:R89867215482 <" + role.qualifies + "> ; "
                                                  + " tpl:R29577887690 '" + role.value.text + "'^^xsd:" + roleValueAs + " . ";

                                            i++;
                                        }

                                        else if (role.value != null && !String.IsNullOrEmpty(role.value.reference))
                                        {
                                            //reference restriction
                                            sparql += "<" + role.qualifies + "> rdf:type tpl:R40103148466 ; "
                                                  + " tpl:R49267603385 " + ID + " ; "
                                                  + " tpl:R30741601855 <" + role.qualifies + "> ; "
                                                  + " tpl:R21129944603 <" + role.value.reference + "> . ";
                                        }
                                        else if (!String.IsNullOrEmpty(role.range))
                                        {
                                            //range restriction
                                            sparql += "<" + role.qualifies + "> rdf:type tpl:R76288246068 ; "
                                                    + " tpl:R99672026745 " + ID + " ; "
                                                    + " tpl:R91125890543 <" + role.qualifies + "> ; "
                                                    + " tpl:R98983340497 <" + role.range + "> . ";
                                        }

                                    }
                                    //status
                                    sparql += "_:status rdf:type tpl:R20247551573 ; "
                                              + "tpl:R64574858717 " + ID + " ; "
                                              + "tpl:R56599656536 rdl:R6569332477 ; "
                                              + "tpl:R61794465713 rdl:R3732211754 . ";

                                    sparql = sparql.Insert(sparql.LastIndexOf("."), "}").Remove(sparql.Length - 1);
                                    response = PostToRepository(source, sparql);
                                }
                            }
                            else
                            {
                                string roleID = string.Empty;
                                string roleLabel = string.Empty;
                                string roleDescription = string.Empty;
                                string generatedId = string.Empty;
                                string genName = string.Empty;
                                int i = 0;

                                TemplateQualification td = q.templateQualifications[0];
                                string rName = string.Empty;

                                sparql = sparql.Replace("INSERT DATA { ", "MODIFY DELETE { ");
                                foreach (QMXFName name in td.name)
                                {
                                    specialization = td.qualifies;
                                    nameSparql = sparql + "?a rdf:type dm:Specialization . "
                                          + " ?b dm:hasSuperclass <" + specialization + "> . "
                                          + " ?c dm:hasSubclass " + ID + " . ";

                                    label = name.value;
                                    if (td.description.Count == 0)
                                    {
                                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
                                    }
                                    else
                                    {
                                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                                    }

                                    foreach (Description descr in td.description)
                                    {
                                        description = descr.value;
                                        nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string . ";
                                    }

                                    foreach (RoleQualification rd in td.roleQualification)
                                    {

                                        foreach (QMXFName roleName in rd.name)
                                        {
                                            roleLabel = roleName.value;
                                            foreach (Description desc in rd.description)
                                            {
                                                roleDescription = desc.value;
                                            }
                                        }
                                        //append role to sparql query
                                        //value restriction
                                        if (rd.value != null)
                                        {
                                            if (!String.IsNullOrEmpty(rd.value.text))
                                            {
                                                string roleValueAs = rd.value.As;
                                                if (rd.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
                                                {
                                                    roleValueAs = rd.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", string.Empty);
                                                }

                                                nameSparql += "_:role" + i + " rdf:type tpl:R67036823327 ; "
                                                      + " tpl:R56456315674 " + ID + " ; "
                                                      + " tpl:R89867215482 <" + rd.qualifies + "> ; "
                                                      + " tpl:R29577887690 '" + rd.value.text + "'^^xsd:" + roleValueAs + " . ";

                                                i++;
                                            }
                                            else if (!String.IsNullOrEmpty(rd.value.reference))
                                            {
                                                //reference restriction                                                
                                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R40103148466 ; "
                                                        + " tpl:R49267603385 " + ID + " ; "
                                                        + " tpl:R30741601855 <" + rd.qualifies + "> ; "
                                                        + " tpl:R21129944603 <" + rd.value.reference + "> . ";
                                            }
                                            else if (!String.IsNullOrEmpty(rd.range))
                                            {
                                                //range restriction
                                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
                                                        + " tpl:R99672026745 " + ID + " ; "
                                                        + " tpl:R91125890543 <" + rd.qualifies + "> ; "
                                                        + " tpl:R98983340497 <" + rd.range + "> . ";
                                            }
                                        }
                                        else if (!String.IsNullOrEmpty(rd.range))
                                        {
                                            //range restriction
                                            nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
                                                    + " tpl:R99672026745 " + ID + " ; "
                                                    + " tpl:R91125890543 <" + rd.qualifies + "> ; "
                                                    + " tpl:R98983340497 <" + rd.range + "> . ";
                                        }
                                    }
                                    nameSparql = nameSparql.Insert(nameSparql.LastIndexOf("."), "}").Remove(nameSparql.Length - 1);
                                }
                                foreach (QMXFName name in template.name)
                                {
                                    specialization = template.qualifies;
                                    nameSparql += " INSERT { _:spec rdf:type dm:Specialization ; "
                                          + " dm:hasSuperclass <" + specialization + "> ; "
                                          + " dm:hasSubclass " + ID + " . ";

                                    label = name.value;


                                    if (template.description.Count == 0)
                                    {
                                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
                                    }
                                    else
                                    {
                                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                                    }

                                    foreach (Description descr in template.description)
                                    {
                                        description = descr.value;
                                        nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string . ";
                                    }

                                    i = 0;
                                    foreach (RoleQualification rd in template.roleQualification)
                                    {
                                        foreach (QMXFName roleName in rd.name)
                                        {
                                            roleLabel = roleName.value;
                                            foreach (Description desc in rd.description)
                                            {
                                                roleDescription = desc.value;
                                            }
                                        }
                                        //append role to sparql query
                                        //value restriction
                                        if (rd.value != null)
                                        {
                                            if (!String.IsNullOrEmpty(rd.value.text))
                                            {
                                                string roleValueAs = rd.value.As;
                                                if (rd.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
                                                {
                                                    roleValueAs = rd.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", string.Empty);
                                                }

                                                nameSparql += "_:role" + i + " rdf:type tpl:R67036823327 ; "
                                                      + " tpl:R56456315674 " + ID + " ; "
                                                      + " tpl:R89867215482 <" + rd.qualifies + "> ; "
                                                      + " tpl:R29577887690 '" + rd.value.text + "'^^xsd:" + roleValueAs + " . ";

                                                i++;
                                            }
                                            else if (!String.IsNullOrEmpty(rd.value.reference))
                                            {
                                                //reference restriction
                                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R40103148466 ; "
                                                        + " tpl:R49267603385 " + ID + " ; "
                                                        + " tpl:R30741601855 <" + rd.qualifies + "> ; "
                                                        + " tpl:R21129944603 <" + rd.value.reference + "> . ";
                                            }
                                            else if (!String.IsNullOrEmpty(rd.range))
                                            {
                                                //range restriction
                                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
                                                        + " tpl:R99672026745 " + ID + " ; "
                                                        + " tpl:R91125890543 <" + rd.qualifies + "> ; "
                                                        + " tpl:R98983340497 <" + rd.range + "> . ";
                                            }
                                        }
                                        else if (!String.IsNullOrEmpty(rd.range))
                                        {
                                            //range restriction
                                            nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
                                                    + " tpl:R99672026745 " + ID + " ; "
                                                    + " tpl:R91125890543 <" + rd.qualifies + "> ; "
                                                    + " tpl:R98983340497 <" + rd.range + "> . ";
                                        }
                                    }
                                    nameSparql = nameSparql.Insert(nameSparql.LastIndexOf("."), "}").Remove(nameSparql.Length - 1);
                                }

                                response = PostToRepository(source, nameSparql);
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

        public Response PostClass(QMXF qmxf)
        {
            Status status = new Status();

            try
            {
                int count = 0;
                /*SPARQL sparqlQuery = new SPARQL();

                Clause insertClause = new Clause();
                insertClause.SetSPARQLType(Clause.SPARQLType.Insert);

                sparqlQuery.AddClause(insertClause);*/

                string sparql = "PREFIX eg: <http://example.org/data#> "
                                + "PREFIX rdl: <http://rdl.rdlfacade.org/data#> "
                                + "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> "
                                + "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> "
                                + "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#> "
                                + "PREFIX dm: <http://dm.rdlfacade.org/data#> "
                                + "PREFIX tpl: <http://tpl.rdlfacade.org/data#> "
                                + "INSERT DATA { ";
                string nameSparql = string.Empty;
                string specSparql = string.Empty;
                string classSparql = string.Empty;

                int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
                Repository source = _repositories[repository];

                Utility.WriteString("Number of classes to insert: " + qmxf.classDefinitions.Count.ToString(), "stats.log", true);
                foreach (ClassDefinition Class in qmxf.classDefinitions)
                {

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

                    if (source.IsReadOnly)
                    {
                        status.Level = StatusLevel.Error;
                        status.Messages.Add("Repository is Read Only");
                        _response.Append(status);
                        return _response;
                    }

                    ID = Class.identifier;

                    QMXF q = new QMXF();
                    if (ID != null)
                    {
                        id = ID.Substring((ID.LastIndexOf("#") + 1), ID.Length - (ID.LastIndexOf("#") + 1));

                        q = GetClass(id);

                        foreach (ClassDefinition classFound in q.classDefinitions)
                        {
                            classIndex++;
                            if (classFound.repositoryName.Equals(_repositories[repository].Name))
                            {
                                ID = "<" + ID + ">";
                                Utility.WriteString("Class found: " + q.classDefinitions[classIndex].name[0].value, "stats.log", true);
                                break;
                            }
                        }
                    }

                    if (q.classDefinitions.Count == 0)
                    {
                        //add the class
                        foreach (QMXFName name in Class.name)
                        {
                            label = name.value;
                            count++;
                            Utility.WriteString("Inserting : " + label, "stats.log", true);

                            className = "Class definition " + label;

                            if (_useExampleRegistryBase)
                                generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], className);
                            else
                                generatedId = CreateIdsAdiId(_settings["ClassRegistryBase"], className);

                            ID = "<" + generatedId + ">";

                            Utility.WriteString("\n" + ID + "\t" + label, "Class IDs.log", true);
                            //ID = Class.identifier.Remove(0, 1);

                            if (Class.entityType != null)
                                sparql += ID + " rdf:type <" + Class.entityType.reference + "> . ";

                            sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";

                            foreach (Description descr in Class.description)
                            {
                                description = descr.value;
                                sparql += ID + "rdfs:comment \"" + description + "\"^^xsd:string . ";
                            }


                            /// TODO: fix the type

                            //append Specialization to sparql query
                            foreach (Specialization spec in Class.specialization)
                            {
                                /// Note: QMXF contains only superclass info while qxf and rdf contain superclass and subclass info
                                specialization = spec.reference;
                                sparql += "_:spec rdf:type dm:Specialization ; "
                                        + "dm:hasSuperclass <" + specialization + "> ; "
                                        + "dm:hasSubclass " + ID + " . ";
                            }

                            //append Classification to sparql query
                            foreach (Classification classif in Class.classification)
                            {
                                /// Note: QMXF contains only classifier info while rdf contain classifier and classified info
                                classification = classif.reference;
                                sparql += "_:classif rdf:type dm:Classification ; "
                                        + "dm:hasClassifier <" + classification + "> ; "
                                        + "dm:hasClassified  " + ID + "  . ";
                            }

                            //append Status to sparql query
                            //foreach (Status stat in Class.status)
                            //{
                            sparql += "_:status rdf:type tpl:R20247551573 ; "
                                      + "tpl:R64574858717 " + ID + " ; "
                                      + "tpl:R56599656536 rdl:R6569332477 ; "
                                      + "tpl:R61794465713 rdl:R3732211754 . ";
                            //}

                            //remove last semi-colon
                            sparql = sparql.Insert(sparql.LastIndexOf("."), "}").Remove(sparql.Length - 1);
                            if (!label.Equals(String.Empty))
                            {
                                Reset(label);
                            }
                            _response = PostToRepository(source, sparql);
                        }
                    }
                    else
                    {
                        ClassDefinition cd = q.classDefinitions[classIndex];

                        sparql = sparql.Replace("INSERT DATA { ", "MODIFY DELETE { ");
                        foreach (QMXFName name in cd.name)
                        {
                            label = name.value;
                            nameSparql = sparql + ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                            foreach (Description descr in cd.description)
                            {
                                description = descr.value;
                                nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string . } ";
                            }
                            classSparql = sparql;
                            foreach (Classification classif in cd.classification)
                            {
                                classification = classif.reference;
                                classSparql += " ?a rdf:type dm:Classification . "
                                      + " ?a dm:hasClassifier <" + classification + "> . "
                                      + " ?a dm:hasClassified  " + ID + "  . } ";
                            }
                            specSparql = sparql;
                            foreach (Specialization spec in cd.specialization)
                            {
                                specialization = spec.reference;
                                specSparql += " ?a rdf:type dm:Specialization . "
                                      + " ?a dm:hasSuperclass <" + specialization + "> . "
                                      + " ?a dm:hasSubclass " + ID + " . } ";
                            }

                        }
                        foreach (QMXFName name in Class.name)
                        {
                            label = name.value;

                            nameSparql += "INSERT { " + ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
                            foreach (Description descr in Class.description)
                            {
                                description = descr.value;
                                nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string . } ";
                            }

                            string oldClass = classification;
                            if (Class.classification.Count == 0 && classSparql.Length > 0 && oldClass.Length > 0)
                            {
                                classSparql = classSparql.Replace("MODIFY DELETE { ", "DELETE {");
                                classSparql += "WHERE { ?a rdf:type dm:Classification . "
                                        + " ?a dm:hasClassifier <" + oldClass + "> . "
                                        + " ?a dm:hasClassified  " + ID + "  . } ";
                            }
                            if (Class.classification.Count == 0 && oldClass.Length == 0)
                            {
                                classSparql = "";
                            }
                            foreach (Classification classif in Class.classification)
                            {
                                classification = classif.reference;
                                if (oldClass.Length > 0)
                                {
                                    classSparql += "INSERT { _:classif rdf:type dm:Classification ; "
                                            + "dm:hasClassifier <" + classification + "> ; "
                                            + "dm:hasClassified  " + ID + "  . } "
                                        /*+ "WHERE { ?c dm:hasClassified " + ID + " . "
                                        + " ?b dm:hasClassifier ?o . "
                                        + " ?a rdf:type dm:Classification . } ";*/
                                            + "WHERE { ?a rdf:type dm:Classification . "
                                            + " ?a dm:hasClassifier <" + oldClass + "> . "
                                            + " ?a dm:hasClassified  " + ID + "  . } ";
                                }
                                else
                                {
                                    classSparql = classSparql.Replace("MODIFY DELETE { ", "INSERT { ");
                                    classSparql += "_:classif rdf:type dm:Classification ; "
                                            + "dm:hasClassifier <" + classification + "> ; "
                                            + "dm:hasClassified  " + ID + "  . } ";
                                }
                            }
                            string oldSpec = specialization;
                            if (Class.specialization.Count == 0 && specSparql.Length > 0 && oldSpec.Length > 0)
                            {
                                specSparql = specSparql.Replace("MODIFY DELETE { ", "DELETE {");
                                specSparql += "WHERE { ?a rdf:type dm:Specialization . "
                                        + " ?a dm:hasSuperclass <" + oldSpec + "> . "
                                        + " ?a dm:hasSubclass " + ID + " . } ";
                            }
                            if (Class.specialization.Count == 0 && oldSpec.Length == 0)
                            {
                                specSparql = "";
                            }
                            foreach (Specialization spec in Class.specialization)
                            {
                                specialization = spec.reference;
                                if (oldSpec.Length > 0)
                                {
                                    specSparql += "INSERT { _:spec rdf:type dm:Specialization ; "
                                            + "dm:hasSuperclass <" + specialization + "> ; "
                                            + "dm:hasSubclass " + ID + " . } "
                                        /*+ "WHERE { ?c dm:hasSubclass " + ID + " . "
                                        + " ?b dm:hasSuperclass ?o . "
                                        + " ?a rdf:type dm:Specialization . } ";*/
                                            + "WHERE { ?a rdf:type dm:Specialization . "
                                            + " ?a dm:hasSuperclass <" + oldSpec + "> . "
                                            + " ?a dm:hasSubclass " + ID + " . } ";
                                }
                                else
                                {
                                    specSparql = specSparql.Replace("MODIFY DELETE { ", "INSERT { ");
                                    specSparql += "_:spec rdf:type dm:Specialization ; "
                                            + "dm:hasSuperclass <" + specialization + "> ; "
                                            + "dm:hasSubclass " + ID + " . } ";
                                }
                            }
                        }
                        if (!label.Equals(String.Empty))
                        {
                            Reset(label);
                        }
                        _response = PostToRepository(source, nameSparql);
                        _response = PostToRepository(source, classSparql);
                        _response = PostToRepository(source, specSparql);
                    }
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

                //sparqlResults = SPARQLClient.PostQuery(repository.uri, sparql, credentials, _proxyCredentials);
                sparqlResults = SPARQLClient.Query(repository.Uri, sparql, credentials, _proxyCredentials);

                return sparqlResults;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to read repository['{0}']", repository.Uri), ex);
                return new SPARQLResults();
                //                throw ex;
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

        public QMXF GetPart8Template(string id)
        {
            QMXF qmxf = new QMXF();

            try
            {
                TemplateDefinition templateDefinition = GetPart8TemplateDefinition(id);
                qmxf.templateDefinitions.Add(templateDefinition);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetPart8Template: " + ex);
            }

            return qmxf;
        }

        private TemplateDefinition GetPart8TemplateDefinition(string id)
        {
            TemplateDefinition templateDefinition = null;

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Description description = new Description();

                RefDataEntities resultEntities = new RefDataEntities();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Template").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);


                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {
                        templateDefinition = new TemplateDefinition();
                        QMXFName name = new QMXFName();

                        if (result.ContainsKey("label"))
                        {
                            name.value = result["label"];
                        }

                        if (result.ContainsKey("definition"))
                        {
                            description.value = result["definition"];
                        }

                        templateDefinition.identifier = @"http://standards.tc184-sc4.org/iso/15926/-8/templates#" + id;
                        templateDefinition.name.Add(name);
                        templateDefinition.description.Add(description);

                        templateDefinition.roleDefinition = GetPart8RoleDefintion(id);
                    }
                }

                return templateDefinition;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetTemplateDefinition: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        private List<RoleDefinition> GetPart8RoleDefintion(string id)
        {
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();
                //List<Classification> classifications = new List<Classification>();

                List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();

                RefDataEntities resultEntities = new RefDataEntities();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Roles").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
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
                        if (string.IsNullOrEmpty(name.value))
                            name.value = roleDefinition.identifier.Replace(roleDefinition.identifier.Substring(0, roleDefinition.identifier.LastIndexOf("#") + 1), "");
                        roleDefinition.name.Add(name);
                        //Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending()); //problem with search an insert - skips some roles
                        roleDefinitions.Add(roleDefinition);
                        roleDefinition.restrictions = GetPart8RoleRestrictions(roleDefinition.identifier.Replace(roleDefinition.identifier.Substring(0, roleDefinition.identifier.LastIndexOf("#") + 1), ""));
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

        private List<PropertyRestriction> GetPart8RoleRestrictions(string id)
        {
            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();
                //List<Classification> classifications = new List<Classification>();

                List<PropertyRestriction> propertyRestrictions = new List<PropertyRestriction>();

                RefDataEntities resultEntities = new RefDataEntities();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8RoleRestrictions").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);

                    foreach (Dictionary<string, string> result in results)
                    {

                        PropertyRestriction propertyRestriction = new PropertyRestriction();

                        if (result.ContainsKey("valuesFrom"))
                        {
                            propertyRestriction.valuesFrom = result["valuesFrom"];
                            propertyRestriction.type = "allValuesFrom";
                        }
                        if (result.ContainsKey("cardinality"))
                        {
                            propertyRestriction.cardiniality = result["cardinality"];
                            propertyRestriction.type = "minCardinality";
                        }
                        //roleDefinition.name.Add(name);
                        //Utility.SearchAndInsert(propertyRestrictions, propertyRestriction, PropertyRestriction.sortAscending());
                        propertyRestrictions.Add(propertyRestriction);
                    }
                }

                return propertyRestrictions;
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetRoleDefinition: " + e);
                throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
            }
        }

        public Response PostPart8Template(QMXF qmxf)
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
                        string label = string.Empty;
                        string description = string.Empty;
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

                            q = GetTemplate(ID, "Definition");

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
                                label = name.value;

                                //ID generator
                                templateName = "Template definition " + label;
                                if (ID == null || ID == string.Empty)
                                {
                                    if (_useExampleRegistryBase)
                                        generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                    else
                                        generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);
                                    ID = "<" + generatedTempId + ">";
                                    Utility.WriteString("\n" + ID + "\t" + label, "TempDef IDs.log", true);
                                }

                                sparqlStr.AppendLine("  tpl:" + ID + " rdf:type owl:class ;");

                                int descrCount = template.description.Count;

                                sparqlStr.AppendLine("  rdfs:label \"" + label + "\"^^xsd:string ;");
                                sparqlStr.AppendLine("  rdfs:subClassOf p8:BaseTemplateStatement ;");
                                // sparql.AppendLine(" rdf:type owl:Thing ;");
                                sparqlStr.AppendLine("  rdf:type p8:Template ;");
                                foreach (Description descr in template.description)
                                {
                                    description = descr.value;

                                    if (--descrCount > 0)
                                        sparqlStr.AppendLine("  rdfs:comment \"" + description + "\"^^xsd:string ;");
                                    else
                                        sparqlStr.AppendLine("  rdfs:comment \"" + description + "\"^^xsd:string .");
                                }

                                sparqlStr.AppendLine("  tpl:TemplateDescription_of_" + label + " rdf:type p8:TemplateDescription ;");
                                // sparql.AppendLine(" rdf:type owl:thing ;");
                                sparqlStr.AppendLine("  rdfs:label \"TemplateDescription_of_" + label + "\"^^xsd:string ;");
                                sparqlStr.AppendLine("  p8:valNumberOfRoles " + template.roleDefinition.Count + " ;");
                                sparqlStr.AppendLine("  p8:hasTemplate tpl:" + ID + " .");

                                sparqlStr.AppendLine("}");


                                response = PostToRepository(target, sparqlStr.ToString());

                                foreach (Specialization spec in template.specialization)
                                {
                                    string specialization = String.Empty;

                                    sparqlStr = new StringBuilder();
                                    specialization = spec.reference;
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);

                                    sparqlStr.AppendLine("  tpl:TemplateSpecialization_of_" + specialization + "_to_" + label + " rdf:type p8:TemplateSpecialization ;");

                                    sparqlStr.AppendLine("  rdfs:label \"TemplateSpecialization_of_" + specialization + "_to_" + label + "\"^^xds:string ;");
                                    sparqlStr.AppendLine("  p8:hasSuperTemplate " + specialization + " ;");
                                    sparqlStr.AppendLine("  p8:hasSubTemplate tpl:" + ID + " .");
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }

                                foreach (RoleDefinition role in template.roleDefinition)
                                {
                                    string roleLabel = string.Empty;
                                    string roleID = string.Empty;
                                    string generatedId = string.Empty;
                                    string genName = string.Empty;

                                    genName = "Role definition " + roleLabel;
                                    if (role.identifier == null || role.identifier == string.Empty)
                                    {
                                        if (_useExampleRegistryBase)
                                            generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                        else
                                            generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                        roleID = "<" + generatedId + ">";
                                    }
                                    else
                                    {
                                        roleID = "<" + role.identifier + ">";
                                    }

                                    sparqlStr = new StringBuilder();
                                    sparqlStr.Append(prefix);
                                    sparqlStr.AppendLine(insertData);
                                    sparqlStr.AppendLine("  tpl:TemplateRoleDescription_of_" + label + "_" + ++roleCount + " rdf:type p8:TemplateRoleDescription ;");// could get roleIndex from QMXF?
                                    sparqlStr.AppendLine("  rdfs:label \"TemplateRoleDescription_of_" + label + "_" + roleCount + "\"^^xsd:string ;");
                                    sparqlStr.AppendLine("  p8:valRoleIndex " + roleCount + " ;");
                                    sparqlStr.AppendLine("  p8:hasTemplate tpl:" + ID + " ;");
                                    //  sparql.AppendLine("  rdf:type owl:Thing ;");
                                    sparqlStr.AppendLine("  p8:hasRole " + roleID + " .");
                                    sparqlStr.AppendLine("}");

                                    response = PostToRepository(target, sparqlStr.ToString());
                                }

                                response = PostToRepository(target, sparqlStr.ToString());
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
                            sparqlStr.AppendLine(delWhere);

                            identifier = td.identifier;

                            sparqlStr.AppendLine(identifier + " rdf:type owl:class ;");
                            sparqlStr.AppendLine(" ?property ?value }");

                            response = PostToRepository(target, sparqlStr.ToString());

                            sparqlStr = new StringBuilder();
                            sparqlStr.Append(prefix);
                            sparqlStr.AppendLine(delWhere);
                            sparqlStr.AppendLine(" tpl:TemplateDescription_of_" + label + " rdf:type p8:TemplateDescription ;");
                            sparqlStr.AppendLine(" ?property ?value }");

                            response = PostToRepository(target, sparqlStr.ToString());

                            foreach (RoleDefinition role in template.roleDefinition)
                            {
                                string roleID = role.identifier;
                                sparqlStr = new StringBuilder();
                                sparqlStr.Append(prefix);
                                sparqlStr.AppendLine(delWhere);
                                sparqlStr.AppendLine(" tpl:TemplateRoleDescription_of_" + label + "_" + ++roleCount + " p8:hasRole " + roleID + " ;");
                                sparqlStr.AppendLine(" ?property ?value }");

                                response = PostToRepository(target, sparqlStr.ToString());
                            }

                            foreach (Specialization spec in template.specialization)
                            {
                                string specialization = String.Empty;
                                sparqlStr = new StringBuilder();
                                sparqlStr.Append(prefix);
                                sparqlStr.AppendLine(delWhere);
                                specialization = spec.reference;
                                sparqlStr.AppendLine("  tpl:TemplateSpecialization_of_" + specialization + "_to_" + label + " rdf:type p8:TemplateSpecialization ;");
                                sparqlStr.AppendLine("  rdfs:label TemplateSpecialization_of_" + specialization + "_to_" + label + " ;");
                                sparqlStr.AppendLine("  p8:hasSuperTemplate " + specialization + " ;");
                                sparqlStr.AppendLine("  p8:hasSubTemplate tpl:" + ID + " .");
                                sparqlStr.AppendLine("}");

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
                        string description = string.Empty;
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
                            q = GetTemplate(id, "Qualification");
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

        public Response PostPart8Template2(QMXF qmxf)
        {
            Status status = new Status();

            try
            {
                //Response response = null;
                string sparql = string.Empty;
                string tempSparql = string.Empty;

                Query query1 = (Query)_queries.FirstOrDefault(c => c.Key == "PostPart8Template_1").Query;
                string sparql1 = ReadSPARQL(query1.FileName);

                Query query2 = (Query)_queries.FirstOrDefault(c => c.Key == "PostPart8Template_2").Query;
                string sparql2 = ReadSPARQL(query2.FileName);

                Query query3 = (Query)_queries.FirstOrDefault(c => c.Key == "PostPart8Template_3").Query;
                string sparql3 = ReadSPARQL(query3.FileName);

                Query query4 = (Query)_queries.FirstOrDefault(c => c.Key == "PostPart8Template_4").Query;
                string sparql4 = ReadSPARQL(query4.FileName);

                Query query5_1 = (Query)_queries.FirstOrDefault(c => c.Key == "PostPart8Template_5_1").Query;
                string sparql5_1 = ReadSPARQL(query5_1.FileName);

                Query query5_2 = (Query)_queries.FirstOrDefault(c => c.Key == "PostPart8Template_5_2").Query;
                string sparql5_2 = ReadSPARQL(query5_2.FileName);

                int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
                Repository source = _repositories[repository];

                if (source.IsReadOnly)
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
                        string label = string.Empty;
                        string description = string.Empty;
                        string generatedTempId = string.Empty;
                        string templateName = string.Empty;
                        string roleDefinition = string.Empty;
                        string nameSparql = string.Empty;
                        string specSparql = string.Empty;
                        string classSparql = string.Empty;
                        int templateIndex = -1;

                        ID = template.identifier;

                        QMXF q = new QMXF();
                        if (ID != null)
                        {
                            id = ID.Replace(ID.Substring(0, ID.LastIndexOf("#")), "");
                            q = GetTemplate(id, "Definition");
                            foreach (TemplateDefinition templateFound in q.templateDefinitions)
                            {
                                templateIndex++;
                                if (templateFound.repositoryName.Equals(_repositories[repository]))
                                {
                                    ID = "<" + ID + ">";
                                    Utility.WriteString("Template found: " + q.templateDefinitions[templateIndex].name[0].value, "stats.log", true);
                                    break;
                                }
                            }
                        }

                        if (q.templateDefinitions.Count == 0)
                        {
                            foreach (QMXFName name in template.name)
                            {
                                label = name.value;

                                //ID generator
                                templateName = "Template definition " + label;

                                if (_useExampleRegistryBase)
                                    generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                else
                                    generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);
                                ID = "<" + generatedTempId + ">";
                                Utility.WriteString("\n" + ID + "\t" + label, "TempDef IDs.log", true);

                                sparql1 = sparql1.Replace("param1", ID);
                                sparql = sparql1;

                                sparql2 = sparql2.Replace("param1", label);

                                //append description to sparql query
                                int descrCount = template.description.Count;
                                if (descrCount == 0)
                                {
                                    sparql2 += " . ";
                                }
                                else
                                {
                                    sparql2 += " ; ";
                                }
                                sparql += sparql2;

                                foreach (Description descr in template.description)
                                {
                                    description = descr.value;
                                    tempSparql = sparql3.Replace("param1", description);

                                    if (--descrCount > 0)
                                        tempSparql += "  ; ";
                                    else
                                        tempSparql += "  . ";

                                    sparql += tempSparql;
                                }


                                #region roles
                                foreach (RoleDefinition role in template.roleDefinition)
                                {
                                    string roleID = string.Empty;
                                    string roleLabel = string.Empty;
                                    string roleDescription = string.Empty;
                                    string generatedId = string.Empty;
                                    string genName = string.Empty;
                                    int blankNodeCount = 0;

                                    //ID generator
                                    genName = "Role definition " + roleLabel;

                                    if (_useExampleRegistryBase)
                                        generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
                                    else
                                        generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);

                                    roleID = "<" + generatedId + ">";

                                    //roleID = role.identifier;
                                    foreach (QMXFName roleName in role.name)
                                    {
                                        roleLabel = roleName.value;
                                        //roleDescription = role.description.value;
                                        Utility.WriteString("\n" + roleID + "\t" + roleLabel, "RoleDef IDs.log", true);
                                    }
                                    //append role to sparql query
                                    tempSparql = sparql4.Replace("param1", roleID);
                                    tempSparql = tempSparql.Replace("param2", blankNodeCount.ToString());
                                    tempSparql = tempSparql.Replace("param3", blankNodeCount++.ToString());
                                    tempSparql = tempSparql.Replace("param4", blankNodeCount.ToString());

                                    sparql += tempSparql;

                                    int restrictionCount = role.restrictions.Count;
                                    foreach (PropertyRestriction restriction in role.restrictions)
                                    {
                                        if (--restrictionCount > 0)
                                        {
                                            tempSparql = sparql5_1.Replace("param1", blankNodeCount++.ToString());
                                            tempSparql = tempSparql.Replace("param2", blankNodeCount.ToString());
                                            tempSparql = tempSparql.Replace("param3", (++blankNodeCount).ToString());
                                            tempSparql = tempSparql.Replace("param4", blankNodeCount.ToString());
                                            tempSparql = tempSparql.Replace("param5", roleID);
                                            tempSparql = tempSparql.Replace("param6", restriction.type);
                                            tempSparql = tempSparql.Replace("param7", restriction.value);
                                        }
                                        else
                                        {
                                            tempSparql = sparql5_1.Replace("param1", blankNodeCount++.ToString());
                                            tempSparql = tempSparql.Replace("param2", blankNodeCount.ToString());
                                            tempSparql = tempSparql.Replace("param3", blankNodeCount.ToString());
                                            tempSparql = tempSparql.Replace("param4", roleID);
                                            tempSparql = tempSparql.Replace("param5", restriction.type);
                                            tempSparql = tempSparql.Replace("param6", restriction.value);
                                        }

                                        sparql += tempSparql;
                                    }
                                }
                                #endregion roles

                                sparql = sparql.Insert(sparql.LastIndexOf("."), "}").Remove(sparql.Length - 1);
                                //response = PostToRepository(source, sparql);
                                Utility.WriteString(sparql, @"C:\XMLs\PostPart8Template.txt");
                            }
                        }
                    }
                }
                #endregion template Definitions

                _response.Append(status);
                return _response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in PostTemplate: " + ex);
                throw ex;
            }
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
                //QMXFStatus status;
                //List<Classification> classifications = new List<Classification>();
                List<Specialization> specializations = new List<Specialization>();

                RefDataEntities resultEntities = new RefDataEntities();
                List<Entity> resultEnt = new List<Entity>();

                Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Class").Query;
                QueryBindings queryBindings = queryContainsSearch.Bindings;

                sparql = ReadSPARQL(queryContainsSearch.FileName);
                sparql = sparql.Replace("param1", id);

                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
                    // classifications = new List<Classification>();
                    specializations = new List<Specialization>();

                    classDefinition = new ClassDefinition();

                    classDefinition.identifier = "http://rdl.rdlfacade.org/data#" + id;
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
                    specializations = GetPart8Specializations(id);
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

        private List<Specialization> GetPart8Specializations(string id)
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
                            label = GetLabel(uri);
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

        public RefDataEntities Part8Search(string query)
        {
            try
            {
                return Part8SearchPage(query, 0, 0);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Search: " + ex);
                throw new Exception("Error while Searching " + query + ".\n" + ex.ToString(), ex);
            }
        }

        public RefDataEntities Part8SearchPage(string query, int start, int limit)
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

                    Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "Part8ContainsSearch").Query;
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

        public Response PostPart8Class(QMXF qmxf)
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

                    if (target.IsReadOnly)
                    {
                        status.Level = StatusLevel.Error;
                        status.Messages.Add("Repository is Read Only");
                        _response.Append(status);
                        return _response;
                    }

                    ID = Class.identifier;

                    QMXF q = new QMXF();
                    if (ID != null)
                    {
                        id = Utility.GetIdFromURI(ID);

                        q = GetClass(id);
                        ID = "<" + ID + ">";
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

                    if (!existInTarget)
                    {
                        sparqlStr.AppendLine(insertData);
                        //add the class
                        foreach (QMXFName name in Class.name)
                        {
                            label = name.value;
                            count++;
                            Utility.WriteString("Inserting : " + label, "stats.log", true);

                            className = "Class definition " + label;
                            if (ID == null || ID == string.Empty)
                            {
                                if (_useExampleRegistryBase)
                                    generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], className);
                                else
                                    generatedId = CreateIdsAdiId(_settings["ClassRegistryBase"], className);

                                ID = "<" + generatedId + ">";

                                Utility.WriteString("\n" + ID + "\t" + label, "Class IDs.log", true);
                            }
                            sparqlStr.AppendLine("  " + ID + " rdf:type owl:Class ;");

                            foreach (Description descr in Class.description)
                            {
                                description = descr.value;
                                sparqlStr.AppendLine("  rdfs:comment \"" + description + "\"^^xsd:string ;");
                            }
                            int specCount = Class.specialization.Count;
                            //append Specialization to sparql query
                            foreach (Specialization spec in Class.specialization)
                            {
                                /// Note: QMXF contains only superclass info while qxf and rdf contain superclass and subclass info
                                specialization = spec.reference;
                                if (--specCount > 0)
                                    sparqlStr.AppendLine("  rdfs:subClassOf " + specialization + " ;");
                                else
                                    sparqlStr.AppendLine("  rdfs:subClassOf " + specialization + " .");
                            }

                            sparqlStr.AppendLine("}");

                            if (!label.Equals(String.Empty))
                            {
                                Reset(label);
                            }
                            _response = PostToRepository(target, sparqlStr.ToString());
                        }
                    }
                    //else edit the class
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

        #endregion Part8
    }
}


//   foreach (TemplateQualification template in qmxf.templateQualifications)
//        {
//            string ID = string.Empty;
//            string id = string.Empty;
//            string label = string.Empty;
//            string description = string.Empty;
//            //ID = template.identifier.Remove(0, 1);
//            string generatedTempId = string.Empty;
//            string templateName = string.Empty;
//            string roleDefinition = string.Empty;
//            string specialization = string.Empty;
//            string nameSparql = string.Empty;
//            string specSparql = string.Empty;
//            string classSparql = string.Empty;
//            int templateIndex = -1;
//            int roleCount = 0;
//            ID = template.identifier;

//            QMXF q = new QMXF();
//            if (ID != null)
//            {
//                id = ID.Substring((ID.LastIndexOf("#") + 1), ID.Length - (ID.LastIndexOf("#") + 1));
//                q = GetTemplate(id);
//                ID = "tpl:" + template.identifier;
//            }

//            if (q.templateQualifications.Count == 0)
//            {
//                foreach (QMXFName name in template.name)
//                {
//                    label = name.value;

//                    //ID generator
//                    templateName = "Template qualification " + label;
//                    /// TODO: change to class registry base
//                    if (id == null || id == string.Empty)
//                    {
//                    if (_useExampleRegistryBase)
//                        generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
//                    else
//                        generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);
//                    ID = "<" + generatedTempId + ">";

//                    Utility.WriteString("\n" + ID + "\t" + label, "TempQual IDs.log", true);
//                    }
//                    specialization = template.qualifies;
//                    sparql = new StringBuilder();
//                    sparql.Append(prefix);
//                    sparql.AppendLine(" _:spec rdf:type dm:Specialization ;")
//                            + "dm:hasSuperclass <" + specialization + "> ; "
//                            + "dm:hasSubclass " + ID + " . ";

//                    //sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
//                    if (template.description.Count == 0)
//                    {
//                        sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
//                    }
//                    else
//                    {
//                        sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
//                    }
//                    foreach (Description descr in template.description)
//                    {
//                        description = descr.value;
//                        sparql += " rdfs:comment \"" + description + "\"^^xsd:string . ";
//                    }

//                    int i = 0;
//                    foreach (RoleQualification role in template.roleQualification)
//                    {

//                        string roleID = string.Empty;
//                        string roleLabel = string.Empty;
//                        string roleDescription = string.Empty;
//                        string generatedId = string.Empty;
//                        string genName = string.Empty;

//                        //ID generator
//                        genName = "Role definition " + roleLabel;
//                        /// TODO: change to template registry base
//                        //if (_useExampleRegistryBase)
//                        //    generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], genName);
//                        //else
//                        //    generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], genName);
//                        roleID = "<" + generatedId + ">";

//                        //roleID = role.identifier;
//                        foreach (QMXFName roleName in role.name)
//                        {
//                            roleLabel = roleName.value;
//                            Utility.WriteString("\n" + roleID + "\t" + roleLabel, "RoleQual IDs.log", true);
//                            foreach (Description desc in role.description)
//                            {
//                                roleDescription = desc.value;
//                            }
//                        }
//                        //append role to sparql query
//                        //value restriction
//                        if (role.value != null && !String.IsNullOrEmpty(role.value.text))
//                        {
//                            string roleValueAs = role.value.As;
//                            if (role.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
//                            {
//                                roleValueAs = role.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", string.Empty);
//                            }

//                            sparql += "_:role" + i + " rdf:type tpl:R67036823327 ; "
//                                  + " tpl:R56456315674 " + ID + " ; "
//                                  + " tpl:R89867215482 <" + role.qualifies + "> ; "
//                                  + " tpl:R29577887690 '" + role.value.text + "'^^xsd:" + roleValueAs + " . ";

//                            i++;
//                        }

//                        else if (role.value != null && !String.IsNullOrEmpty(role.value.reference))
//                        {
//                            //reference restriction
//                            sparql += "<" + role.qualifies + "> rdf:type tpl:R40103148466 ; "
//                                  + " tpl:R49267603385 " + ID + " ; "
//                                  + " tpl:R30741601855 <" + role.qualifies + "> ; "
//                                  + " tpl:R21129944603 <" + role.value.reference + "> . ";
//                        }
//                        else if (!String.IsNullOrEmpty(role.range))
//                        {
//                            //range restriction
//                            sparql += "<" + role.qualifies + "> rdf:type tpl:R76288246068 ; "
//                                    + " tpl:R99672026745 " + ID + " ; "
//                                    + " tpl:R91125890543 <" + role.qualifies + "> ; "
//                                    + " tpl:R98983340497 <" + role.range + "> . ";
//                        }

//                    }
//                    //status
//                    sparql += "_:status rdf:type tpl:R20247551573 ; "
//                              + "tpl:R64574858717 " + ID + " ; "
//                              + "tpl:R56599656536 rdl:R6569332477 ; "
//                              + "tpl:R61794465713 rdl:R3732211754 . ";

//                    sparql = sparql.Insert(sparql.LastIndexOf("."), "}").Remove(sparql.Length - 1);
//                    response = PostToRepository(source, sparql);
//                }
//            }
//            else
//            {
//                string roleID = string.Empty;
//                string roleLabel = string.Empty;
//                string roleDescription = string.Empty;
//                string generatedId = string.Empty;
//                string genName = string.Empty;
//                int i = 0;

//                TemplateQualification td = q.templateQualifications[0];
//                string rName = string.Empty;

//                sparql = sparql.Replace("INSERT DATA { ", "MODIFY DELETE { ");
//                foreach (QMXFName name in td.name)
//                {
//                    specialization = td.qualifies;
//                    nameSparql = sparql + "?a rdf:type dm:Specialization . "
//                          + " ?b dm:hasSuperclass <" + specialization + "> . "
//                          + " ?c dm:hasSubclass " + ID + " . ";

//                    label = name.value;
//                    if (td.description.Count == 0)
//                    {
//                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
//                    }
//                    else
//                    {
//                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
//                    }

//                    foreach (Description descr in td.description)
//                    {
//                        description = descr.value;
//                        nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string . ";
//                    }

//                    foreach (RoleQualification rd in td.roleQualification)
//                    {

//                        foreach (QMXFName roleName in rd.name)
//                        {
//                            roleLabel = roleName.value;
//                            foreach (Description desc in rd.description)
//                            {
//                                roleDescription = desc.value;
//                            }
//                        }
//                        //append role to sparql query
//                        //value restriction
//                        if (rd.value != null)
//                        {
//                            if (!String.IsNullOrEmpty(rd.value.text))
//                            {
//                                string roleValueAs = rd.value.As;
//                                if (rd.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
//                                {
//                                    roleValueAs = rd.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", string.Empty);
//                                }

//                                nameSparql += "_:role" + i + " rdf:type tpl:R67036823327 ; "
//                                      + " tpl:R56456315674 " + ID + " ; "
//                                      + " tpl:R89867215482 <" + rd.qualifies + "> ; "
//                                      + " tpl:R29577887690 '" + rd.value.text + "'^^xsd:" + roleValueAs + " . ";

//                                i++;
//                            }
//                            else if (!String.IsNullOrEmpty(rd.value.reference))
//                            {
//                                //reference restriction                                                
//                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R40103148466 ; "
//                                        + " tpl:R49267603385 " + ID + " ; "
//                                        + " tpl:R30741601855 <" + rd.qualifies + "> ; "
//                                        + " tpl:R21129944603 <" + rd.value.reference + "> . ";
//                            }
//                            else if (!String.IsNullOrEmpty(rd.range))
//                            {
//                                //range restriction
//                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
//                                        + " tpl:R99672026745 " + ID + " ; "
//                                        + " tpl:R91125890543 <" + rd.qualifies + "> ; "
//                                        + " tpl:R98983340497 <" + rd.range + "> . ";
//                            }
//                        }
//                        else if (!String.IsNullOrEmpty(rd.range))
//                        {
//                            //range restriction
//                            nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
//                                    + " tpl:R99672026745 " + ID + " ; "
//                                    + " tpl:R91125890543 <" + rd.qualifies + "> ; "
//                                    + " tpl:R98983340497 <" + rd.range + "> . ";
//                        }
//                    }
//                    nameSparql = nameSparql.Insert(nameSparql.LastIndexOf("."), "}").Remove(nameSparql.Length - 1);
//                }
//                foreach (QMXFName name in template.name)
//                {
//                    specialization = template.qualifies;
//                    nameSparql += " INSERT { _:spec rdf:type dm:Specialization ; "
//                          + " dm:hasSuperclass <" + specialization + "> ; "
//                          + " dm:hasSubclass " + ID + " . ";

//                    label = name.value;


//                    if (template.description.Count == 0)
//                    {
//                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
//                    }
//                    else
//                    {
//                        nameSparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
//                    }

//                    foreach (Description descr in template.description)
//                    {
//                        description = descr.value;
//                        nameSparql += "rdfs:comment \"" + description + "\"^^xsd:string . ";
//                    }

//                    i = 0;
//                    foreach (RoleQualification rd in template.roleQualification)
//                    {
//                        foreach (QMXFName roleName in rd.name)
//                        {
//                            roleLabel = roleName.value;
//                            foreach (Description desc in rd.description)
//                            {
//                                roleDescription = desc.value;
//                            }
//                        }
//                        //append role to sparql query
//                        //value restriction
//                        if (rd.value != null)
//                        {
//                            if (!String.IsNullOrEmpty(rd.value.text))
//                            {
//                                string roleValueAs = rd.value.As;
//                                if (rd.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
//                                {
//                                    roleValueAs = rd.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", string.Empty);
//                                }

//                                nameSparql += "_:role" + i + " rdf:type tpl:R67036823327 ; "
//                                      + " tpl:R56456315674 " + ID + " ; "
//                                      + " tpl:R89867215482 <" + rd.qualifies + "> ; "
//                                      + " tpl:R29577887690 '" + rd.value.text + "'^^xsd:" + roleValueAs + " . ";

//                                i++;
//                            }
//                            else if (!String.IsNullOrEmpty(rd.value.reference))
//                            {
//                                //reference restriction
//                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R40103148466 ; "
//                                        + " tpl:R49267603385 " + ID + " ; "
//                                        + " tpl:R30741601855 <" + rd.qualifies + "> ; "
//                                        + " tpl:R21129944603 <" + rd.value.reference + "> . ";
//                            }
//                            else if (!String.IsNullOrEmpty(rd.range))
//                            {
//                                //range restriction
//                                nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
//                                        + " tpl:R99672026745 " + ID + " ; "
//                                        + " tpl:R91125890543 <" + rd.qualifies + "> ; "
//                                        + " tpl:R98983340497 <" + rd.range + "> . ";
//                            }
//                        }
//                        else if (!String.IsNullOrEmpty(rd.range))
//                        {
//                            //range restriction
//                            nameSparql += "<" + rd.qualifies + "> rdf:type tpl:R76288246068 ; "
//                                    + " tpl:R99672026745 " + ID + " ; "
//                                    + " tpl:R91125890543 <" + rd.qualifies + "> ; "
//                                    + " tpl:R98983340497 <" + rd.range + "> . ";
//                        }
//                    }
//                    nameSparql = nameSparql.Insert(nameSparql.LastIndexOf("."), "}").Remove(nameSparql.Length - 1);
//                }