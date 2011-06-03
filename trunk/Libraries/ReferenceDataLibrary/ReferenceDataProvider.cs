﻿// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
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
    public class ReferenceDataProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ReferenceDataProvider));

        private Response _response = null;

        private const string REPOSITORIES_FILE_NAME = "Repositories.xml";
        private const string QUERIES_FILE_NAME = "Queries.xml";

        private NamespaceMapper nsMap = new NamespaceMapper();
        private bool qn = false;
        private string qName = string.Empty;
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
        private StringBuilder sparqlStr = new StringBuilder();
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

                prefix.AppendLine(nsMap.PrefixString());
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
            return _repositories.Find(c => c.Name == name);
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
                List<string> names = new List<string>();
                string language = string.Empty;

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
                            names = result["label"].Split('@').ToList();
                            if (names.Count == 1)
                                language = defaultLanguage;
                            else
                                language = names[names.Count - 1];

                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = names[0],
                                Lang = language,
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
                List<string> names = new List<string>();

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
                            names = result["label"].Split('@').ToList();
                            if (names.Count == 1)
                                labelEntity.Lang = defaultLanguage;
                            else
                                labelEntity.Lang = names[names.Count - 1];

                            labelEntity.Label = names[0];
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
                Query getClassification;
                QueryBindings queryBindings;

                foreach (Repository repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;

                    switch (rep.RepositoryType)
                    {
                        case RepositoryType.Camelot:
                        case RepositoryType.RDSWIP:
                            getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassification").Query;
                            queryBindings = getClassification.Bindings;

                            sparql = ReadSPARQL(getClassification.FileName);
                            sparql = sparql.Replace("param1", id);
                            classifications = ProcessClassifications(rep, sparql, queryBindings);
                            break;
                        case RepositoryType.Part8:
                            getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Classification").Query;
                            queryBindings = getClassification.Bindings;

                            sparql = ReadSPARQL(getClassification.FileName);
                            sparql = sparql.Replace("param1", id);
                            classifications = ProcessClassifications(rep, sparql, queryBindings);
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

        private List<Classification> ProcessClassifications(Repository repository, string sparql, QueryBindings queryBindings)
        {
            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);
            List<Classification> classifications = new List<Classification>();
            List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
            List<string> names = new List<string>();

            foreach (Dictionary<string, string> result in results)
            {

                Classification classification = new Classification();
                string uri = String.Empty;
                string label = String.Empty;
                string lang = string.Empty;

                if (result.ContainsKey("uri"))
                {
                    string pref = nsMap
                        .GetPrefix(new Uri(result["uri"]
                        .Substring(0, result["uri"].IndexOf("#") + 1)));
                    if (pref.Equals("owl") || pref.Equals("dm")) continue;
                    uri = result["uri"];
                    classification.reference = uri;
                }

                if (result.ContainsKey("label"))
                {
                  names = result["label"].Split('@').ToList();
                  label = names[0];
                  if (names.Count == 1)
                    lang = defaultLanguage;
                  else
                    lang = names[names.Count - 1];
                }
                else
                {
                  names = GetLabel(uri).Label.Split('@').ToList();
                  label = names[0];
                  if (names.Count == 1)
                    lang = defaultLanguage;
                  else
                    lang = names[names.Count - 1];
                }
                classification.label = label;
                classification.lang = lang;
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
                List<string> names = new List<string>();

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
                            string lang = string.Empty;

                            if (result.ContainsKey("uri"))
                            {
                                uri = result["uri"];
                                specialization.reference = uri;
                            }
                            if (result.ContainsKey("label"))
                            {
                                names = result["label"].Split('@').ToList();
                                label = names[0];
                                if (names.Count == 1)
                                    lang = defaultLanguage;
                                else
                                    lang = names[names.Count - 1];
                            }
                            else
                            {
                                names = GetLabel(uri).Label.Split('@').ToList();
                                label = names[0];
                                if (names.Count == 1)
                                    lang = defaultLanguage;
                                else
                                    lang = names[names.Count - 1];
                            }


                            specialization.label = label;
                            specialization.lang = lang;
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
                            string lang = string.Empty;

                            if (result.ContainsKey("uri"))
                            {
                                uri = result["uri"];
                                specialization.reference = uri;
                            }
                            if (result.ContainsKey("label"))
                            {
                                names = result["label"].Split('@').ToList();
                                label = names[0];
                                if (names.Count == 1)
                                    lang = defaultLanguage;
                                else
                                    lang = names[names.Count - 1];
                            }
                            else
                            {
                                label = GetLabel(uri).Label;
                            }

                            specialization.label = label;
                            specialization.lang = lang;
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
                    ClassDefinition classDefinition = null;

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

                        if (result.ContainsKey("type"))
                        {
                            Uri typeName = new Uri(result["type"].Substring(0, result["type"].IndexOf("#") + 1));
                            string pref = nsMap.GetPrefix(typeName);
                            if (pref == "dm")
                                classDefinition.entityType = new EntityType { reference = result["type"] };
                            //  else if(repository.RepositoryType == RepositoryType.Part8)
                            //    continue;
                        }

                        if (result.ContainsKey("label"))
                        {
                            names = result["label"].Split('@').ToList();
                            name.value = names[0];
                            if (names.Count == 1)
                                name.lang = defaultLanguage;
                            else
                                name.lang = names[names.Count - 1];
                        }

                        //legacy properties
                        if (result.ContainsKey("definition"))
                        {
                            names = result["definition"].Split('@').ToList();
                            description.value = names[0];
                            if (names.Count == 1)
                                description.lang = defaultLanguage;
                            else
                                description.lang = names[names.Count - 1];
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
                                description.lang = defaultLanguage;
                            else
                                description.lang = names[names.Count - 1];
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

        public Entities GetSuperClasses(string id)
        {
            Entities queryResult = new Entities();
            string language = string.Empty;
            List<string> names = new List<string>();
            try
            {
                List<Specialization> specializations = GetSpecializations(id, null);

                foreach (Specialization specialization in specializations)
                {
                    string uri = specialization.reference;
                     
                    string label = specialization.label;

                    if (label == null)
                        label = GetLabel(uri).Label;
                    names = label.Split('@').ToList();

                    if (names.Count == 1)
                      language = defaultLanguage;
                    else
                      language = names[names.Count - 1];

                    
                    Entity resultEntity = new Entity
                    {
                        Uri = uri,
                        Label = names[0],
                        Lang = language
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
            List<string> names = new List<string>();
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
                    string language = string.Empty;

                    if (label == null)
                    {
                        names = GetLabel(uri).Label.Split('@').ToList();
                        label = names[0];
                        if (names.Count == 1)
                            language = defaultLanguage;
                        else
                            language = names[names.Count - 1];
                    }
                    Entity resultEntity = new Entity
                    {
                        Uri = uri,
                        Label = label,
                        Lang = language
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

        public Entities GetClassMembers(string Id)
        {
          Entities membersResult = new Entities();
          try
          {
            string sparql = string.Empty;
            string language = string.Empty;
            List<string> names = new List<string>();

            Query getMembers = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembers").Query;
            QueryBindings memberBindings = getMembers.Bindings;
            sparql = ReadSPARQL(getMembers.FileName);
            sparql = sparql.Replace("param1", Id);

            foreach (Repository repository in _repositories)
            {
              SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

              List<Dictionary<string, string>> results = BindQueryResults(memberBindings, sparqlResults);

              foreach (Dictionary<string, string> result in results)
              {
                names = result["label"].Split('@').ToList();
                if (names.Count == 1)
                  language = defaultLanguage;
                else
                  language = names[names.Count - 1];
                Entity resultEntity = new Entity
                {
                  Uri = result["uri"],
                  Label = names[0],
                  Lang = language,
                  Repository = repository.Name
                };

                Utility.SearchAndInsert(membersResult, resultEntity, Entity.sortAscending());
                //queryResult.Add(resultEntity);
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

        public Entities GetSubClasses(string id)
        {
            Entities queryResult = new Entities();
           
            try
            {
                string sparql = String.Empty;
                string sparqlPart8 = String.Empty;
                string relativeUri = String.Empty;
                string language = string.Empty;
                List<string> names = new List<string>();

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
                            names = result["label"].Split('@').ToList();

                            if (names.Count == 1)
                                language = defaultLanguage;
                            else
                                language = names[names.Count - 1];
                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = names[0],
                                Lang = language,
                                Repository = repository.Name
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
                            names = result["label"].Split('@').ToList();
                            if (names.Count == 1)
                                language = defaultLanguage;
                            else
                                language = names[names.Count - 1];
                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = names[0],
                                Lang = language,
                                Repository = repository.Name
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

        public Entities GetSubClassesCount(string id)
        {
            Entities queryResult = new Entities();

            try
            {
                string sparql = String.Empty;
                string sparqlPart8 = String.Empty;
                string relativeUri = String.Empty;
                string language = string.Empty;
                List<string> names = new List<string>();

                Query queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassesCount").Query;
                QueryBindings queryBindings = queryGetSubClasses.Bindings;

                sparql = ReadSPARQL(queryGetSubClasses.FileName);
                sparql = sparql.Replace("param1", id);

                Query queryGetSubClassOfInverse = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOfInverseCount").Query;
                QueryBindings queryBindingsPart8 = queryGetSubClassOfInverse.Bindings;

                sparqlPart8 = ReadSPARQL(queryGetSubClassOfInverse.FileName);
                sparqlPart8 = sparqlPart8.Replace("param1", id);

                int count = 0;
                foreach (Repository repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlPart8);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindingsPart8, sparqlResults);

                       
                        foreach (Dictionary<string, string> result in results)
                        {
                          if(result.Count > 0)
                            count = count + Convert.ToInt32(result["label"]);
                            
                            //queryResult.Add(resultEntity);
                        }
                    }
                    else
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
                        
                        foreach (Dictionary<string, string> result in results)
                        {
                          if (result.Count > 0)
                            count = count + Convert.ToInt32(result["label"]);
                        }
                    }
                }
                Entity resultEntity = new Entity
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

        public Entities GetClassTemplates(string id)
        {
            Entities queryResult = new Entities();
            List<string> names = new List<string>();
            string language = string.Empty;
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

                            names = result["label"].Split('@').ToList();
                            if (names.Count == 1)
                                language = defaultLanguage;
                            else
                                language = names[names.Count - 1];

                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = names[0],
                                Lang = language,
                                Repository = repository.Name
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
                            names = result["label"].Split('@').ToList();
                            if (names.Count == 1)
                                language = defaultLanguage;
                            else
                                language = names[names.Count - 1];

                            Entity resultEntity = new Entity
                            {
                                Uri = result["uri"],
                                Label = names[0],
                                Lang = language,
                                Repository = repository.Name
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

        public Entities GetClassTemplatesCount(string id)
        {
            Entities queryResult = new Entities();
            List<string> names = new List<string>();
            string language = string.Empty;
            try
            {
                string sparqlGetClassTemplates = String.Empty;
                string sparqlGetRelatedTemplates = String.Empty;
                string relativeUri = String.Empty;

                Query queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplatesCount").Query;
                QueryBindings queryBindingsGetClassTemplates = queryGetClassTemplates.Bindings;

                sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.FileName);
                sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

                Query queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplatesCount").Query;
                QueryBindings queryBindingsGetRelatedTemplates = queryGetRelatedTemplates.Bindings;

                sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.FileName);
                sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

                int count = 0;
                foreach (Repository repository in _repositories)
                {
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindingsGetRelatedTemplates, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            count = count + Convert.ToInt32(result["label"]);

                            //queryResult.Add(resultEntity);
                        }
                    }
                    else
                    {
                        SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);

                        List<Dictionary<string, string>> results = BindQueryResults(queryBindingsGetClassTemplates, sparqlResults);

                        foreach (Dictionary<string, string> result in results)
                        {
                            count = count + Convert.ToInt32(result["label"]);
                        }
                    }
                }
                Entity resultEntity = new Entity
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
                string sparql = String.Empty;
                string relativeUri = String.Empty;
                string sparqlQuery = string.Empty;
                List<string> names = new List<string>();

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
                        names = result["label"].Split('@').ToList();
                        name.value = names[0];
                        if (names.Count == 1)
                            name.lang = defaultLanguage;
                        else
                            name.lang = names[names.Count - 1];
                    }
                    if (result.ContainsKey("role"))
                    {
                        roleDefinition.identifier = result["role"];
                    }
                    if (result.ContainsKey("comment"))
                    {
                        names = result["comment"].Split('@').ToList();
                        roleDefinition.description.value = names[0];
                        if (names.Count == 1)
                            roleDefinition.description.lang = defaultLanguage;
                        else
                            roleDefinition.description.lang = names[names.Count - 1];
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
                List<string> names = new List<string>();

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
                            names = result["label"].Split('@').ToList();
                            name.value = names[0];
                            if (names.Count == 1)
                                name.lang = defaultLanguage;
                            else
                                name.lang = names[names.Count - 1];
                        }
                        if (result.ContainsKey("role"))
                        {
                            roleDefinition.identifier = result["role"];
                        }
                        if (result.ContainsKey("comment"))
                        {
                            names = result["comment"].Split('@').ToList();
                            roleDefinition.description.value = names[0];
                            if (names.Count == 1)
                                roleDefinition.description.lang = defaultLanguage;
                            else
                                roleDefinition.description.lang = names[names.Count - 1];
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

                List<string> names = new List<string>();

                string referenceSparql = String.Empty;
                string relativeUri1 = String.Empty;

                string valueSparql = String.Empty;
                string relativeUri2 = String.Empty;

                Description description = new Description();
                QMXFStatus status = new QMXFStatus();
                //List<Classification> classifications = new List<Classification>();

                List<RoleQualification> roleQualifications = new List<RoleQualification>();



                foreach (Repository repository in _repositories)
                {
                    if (rep != null)
                        if (rep.Name != repository.Name) continue;
                    switch (rep.RepositoryType)
                    {
                        case RepositoryType.Camelot:
                        case RepositoryType.RDSWIP:

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
                                    names = nameValue.Split('@').ToList();

                                    QMXFName name = new QMXFName();
                                    if (names.Count > 1)
                                        name.lang = names[names.Count - 1];
                                    else
                                        name.lang = defaultLanguage;

                                    name.value = names[0];

                                    roleQualification.name.Add(name);
                                }
                                else
                                {
                                    string nameValue = GetLabel(uri).Label;

                                    if (nameValue == String.Empty)
                                        nameValue = "tpl:" + Utility.GetIdFromURI(uri);

                                    QMXFName name = new QMXFName();
                                    names = nameValue.Split('@').ToList();

                                    if (names.Count > 1)
                                        name.lang = names[names.Count - 1];
                                    else
                                        name.lang = defaultLanguage;

                                    name.value = names[0];

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
                            break;
                        case RepositoryType.Part8:
                            RefDataEntities part8Entities = new RefDataEntities();
                            Query getPart8Roles = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Roles").Query;
                            QueryBindings getPart8RolesBindings = getPart8Roles.Bindings;

                            string part8RolesSparql = ReadSPARQL(getPart8Roles.FileName);
                            part8RolesSparql = part8RolesSparql.Replace("param1", id);
                            SPARQLResults part8RolesResults = QueryFromRepository(repository, part8RolesSparql);
                            List<Dictionary<string, string>> part8RolesBindingResults = BindQueryResults(getPart8RolesBindings, part8RolesResults);
                            foreach (Dictionary<string, string> result in part8RolesBindingResults)
                            {
                                if (result.ContainsKey("comment"))
                                { }

                                if (result.ContainsKey("type"))
                                { }

                                if (result.ContainsKey("label"))
                                { }

                                if (result.ContainsKey("index"))
                                { }

                                if (result.ContainsKey("role"))
                                { }
                                RoleQualification roleQualification = new RoleQualification();
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
            List<TemplateDefinition> templateDefinitionList = new List<TemplateDefinition>();
            TemplateDefinition templateDefinition = null;

            try
            {
                string sparql = String.Empty;
                string relativeUri = String.Empty;
                List<string> names = new List<string>();

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
                      if (result.Count == 0) continue;
                        templateDefinition = new TemplateDefinition();
                        QMXFName name = new QMXFName();

                        templateDefinition.repositoryName = repository.Name;

                        if (result.ContainsKey("label"))
                        {
                            names = result["label"].Split('@').ToList();
                            name.value = names[0];
                            if (names.Count == 1)
                                name.lang = defaultLanguage;
                            else
                                name.lang = names[names.Count - 1];
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
            List<string> names = new List<string>();

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
                                sparqlQuery = "GetTemplateQualification";
                                break;
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
                                names = result["name"].Split('@').ToList();
                                name.value = names[0];
                                if (names.Count == 1)
                                    name.lang = defaultLanguage;
                                else
                                    name.lang = names[names.Count - 1];

                            }
                            if (result.ContainsKey("description"))
                            {
                                names = result["description"].Split('@').ToList();
                                description.value = names[0];
                                if (names.Count == 1)
                                    description.lang = defaultLanguage;
                                else
                                    description.lang = names[names.Count - 1];

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

        public Response PostTemplate(QMXF qmxf)
        {
            Response response = new Response();
            response.Level = StatusLevel.Success;
            Repository repository = null;
            bool qn = false;
            BaseQueryProvider q = null ;
            q.PostToRepository(repository, "");
          try
            {
                repository = GetRepository(qmxf.targetRepository);
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
                    if (qmxf.templateDefinitions.Count > 0)
                    {
                        foreach (TemplateDefinition newTemplateDefinition in qmxf.templateDefinitions)
                        {
                            string language = string.Empty;
                            int roleCount = 0;
                            StringBuilder sparqlAdd = new StringBuilder();

                            sparqlAdd.AppendLine(insertData);
                            bool hasDeletes = false;
                            string templateName = string.Empty;
                            string identifier = string.Empty;
                            string generatedId = string.Empty;
                            string roleDefinition = string.Empty;
                            int index = 1;
                            if (!string.IsNullOrEmpty(newTemplateDefinition.identifier))
                                identifier = Utility.GetIdFromURI(newTemplateDefinition.identifier);

                            templateName = newTemplateDefinition.name[0].value;
                            //check for exisitng template
                            QMXF existingQmxf = new QMXF();
                            if (!String.IsNullOrEmpty(identifier))
                            {
                                existingQmxf = GetTemplate(identifier, QMXFType.Definition, repository);
                            }
                            else
                            {
                                if (_useExampleRegistryBase)
                                    generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                else
                                    generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                                identifier = Utility.GetIdFromURI(generatedId);
                            }
                            #region Form Delete/Insert SPARQL
                            if (existingQmxf.templateDefinitions.Count > 0)
                            {
                                StringBuilder sparqlStmts = new StringBuilder();

                                foreach (TemplateDefinition existingTemplate in existingQmxf.templateDefinitions)
                                {
                                    foreach (QMXFName name in newTemplateDefinition.name)
                                    {
                                        templateName = name.value;
                                        QMXFName existingName = existingTemplate.name.Find(n => n.lang == name.lang);
                                        if (string.IsNullOrEmpty(existingName.lang))
                                            language = "@" + defaultLanguage;
                                        else
                                            language = "@" + existingName.lang;

                                        if (existingName != null)
                                        {
                                            if (String.Compare(existingName.value, name.value, true) != 0)
                                            {
                                                hasDeletes = true;
                                                sparqlStmts.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", identifier, existingName.value, language));
                                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", identifier, name.value, language));
                                                if (repository.RepositoryType == RepositoryType.Part8)
                                                {
                                                  sparqlStmts.AppendLine(string.Format(" tpl:{0} rdfs:subClassOf p8:BaseTemplateStatement .", identifier));                    
                                                  sparqlAdd.AppendLine(string.Format(" tpl:{0} rdfs:subClassOf p8:BaseTemplateStatement .", identifier));
                                                }
                                                else
                                                {
                                                   sparqlStmts.AppendLine("tpl:R35529169909 \"" + existingTemplate.roleDefinition.Count + " .");
                                                   sparqlAdd.AppendLine("tpl:R35529169909 \"" + newTemplateDefinition.roleDefinition.Count + "\"^^xsd:int .");
                                                }

                                            }
                                        }
                                    }
                                    //append changing descriptions to each block
                                    foreach (Description description in newTemplateDefinition.description)
                                    {
                                        Description existingDescription = existingTemplate.description.Find(d => d.lang == description.lang);

                                        if (string.IsNullOrEmpty(existingDescription.lang))
                                            language = "@" + defaultLanguage;
                                        else
                                            language = "@" + existingDescription.lang;

                                        if (existingDescription != null)
                                        {
                                            if (String.Compare(existingDescription.value, description.value, true) != 0)
                                            {
                                                hasDeletes = true;
                                                sparqlStmts.AppendLine(string.Format("tpl:{0} rdfs:comment \'{1}\'{2} .", identifier, existingDescription.value, language));
                                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:comment \'{1}\'{2} .", identifier, description.value, language));
                                            }
                                        }
                                    }

                                    //role count
                                    if (existingTemplate.roleDefinition.Count != newTemplateDefinition.roleDefinition.Count)
                                    {
                                        hasDeletes = true;
                                        if (repository.RepositoryType == RepositoryType.Part8)
                                        {
                                          sparqlStmts.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1} .", identifier, existingTemplate.roleDefinition.Count));
                                          sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier, newTemplateDefinition.roleDefinition.Count));
                                        }
                                    }

                                    index = 1;
                                    foreach (RoleDefinition role in newTemplateDefinition.roleDefinition)
                                    {
                                        string roleIdentifier = role.identifier;
                                        hasDeletes = false;

                                        //get existing role if it exists
                                        RoleDefinition existingRole = existingTemplate.roleDefinition.Find(r => r.identifier == role.identifier);

                                        //remove existing role from existing template, leftovers will be deleted later
                                        existingTemplate.roleDefinition.Remove(existingRole);

                                        if (existingRole != null)
                                        {
                                            #region Process Changing Role
                                            string label = String.Empty;

                                            foreach (QMXFName name in role.name)
                                            {
                                                QMXFName existingName = existingRole.name.Find(n => n.lang == name.lang);

                                                if (existingName != null)
                                                {
                                                    if (String.Compare(existingName.value, name.value, true) != 0)
                                                    {
                                                        hasDeletes = true;
                                                        sparqlStmts.AppendLine(string.Format("tpl:{0}  rdfs:label \'{1}\'{2} .", existingRole.identifier, existingName.value, name.lang));
                                                        if (repository.RepositoryType == RepositoryType.Part8)
                                                        {
                                                          sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type owl:Class .", existingRole.identifier));
                                                        }
                                                        else
                                                        {

                                                        }
                                                    }
                                                    //index
                                                    if (existingRole.designation != null && existingRole.designation.value != index.ToString())
                                                    {
                                                        sparqlStmts.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1}^^xsd:int .", existingRole.identifier, existingRole.designation.value));
                                                    }
                                                }
                                            }
                                            if (existingRole.range != null)
                                            {
                                                if (string.Compare(existingRole.range, role.range, true) != 0)
                                                {
                                                    hasDeletes = true;
                                                    sparqlStmts.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", existingRole.identifier, existingRole.range));
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Insert New Role

                                            string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                            string roleID = string.Empty;
                                            generatedId = string.Empty;
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
                                            // sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type owl:Class .", roleID));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", roleID, roleLabel, language));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1} .", roleID, index));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));

                                            if (!string.IsNullOrEmpty(role.range))
                                            {
                                                qn = nsMap.ReduceToQName(role.range, out qName);
                                                sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                                            }

                                            foreach (PropertyRestriction restriction in role.restrictions)
                                            {

                                            }
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRole rdl:{1} .", identifier, roleID));
                                            #endregion
                                        }

                                        index++;
                                    }
                                }

                                sparqlDelete.Append(prefix);
                                sparqlDelete.AppendLine(deleteWhere);
                                sparqlDelete.Append(sparqlStmts);
                                sparqlDelete.AppendLine(" }; ");
                            }
                            #endregion

                            #region Form Insert SPARQL
                            if (hasDeletes)
                            {
                                sparqlAdd.AppendLine("}");
                            }
                            else
                            {
                                string label = String.Empty;
                                string labelSparql = String.Empty;
                                //form labels
                                foreach (QMXFName name in newTemplateDefinition.name)
                                {
                                    label = name.value.Split('@')[0];

                                    if (string.IsNullOrEmpty(name.lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = "@" + name.lang;
                                }

                                //sparqlAdd.AppendLine(string.Format("  tpl:{0} rdf:type owl:Class .", identifier));
                                sparqlAdd.AppendLine(string.Format("  tpl:{0} rdfs:label \'{1}\'{2} .", identifier, label, language));
                                sparqlAdd.AppendLine(string.Format("  tpl:{0} rdfs:subClassOf p8:BaseTemplateStatement .", identifier));
                                sparqlAdd.AppendLine(string.Format("  tpl:{0} rdf:type p8:Template .", identifier));

                                //add descriptions to sparql
                                foreach (Description descr in newTemplateDefinition.description)
                                {
                                    if (string.IsNullOrEmpty(descr.value))
                                        continue;
                                    else
                                    {
                                        if (string.IsNullOrEmpty(descr.lang))
                                            language = "@" + defaultLanguage;
                                        else
                                            language = "@" + descr.lang;

                                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:comment \'{1}\'{2} .", identifier, descr.value.Split('@')[0], language));
                                    }
                                }
                                sparqlAdd.AppendLine(string.Format(" tpl:{0} p8:valNumberOfRoles {1} .", identifier, newTemplateDefinition.roleDefinition.Count));
                                foreach (RoleDefinition role in newTemplateDefinition.roleDefinition)
                                {
                                    string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                    string roleID = string.Empty;
                                    generatedId = string.Empty;
                                    string genName = string.Empty;
                                    string range = role.range;

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
                                    //sparqlAdd.AppendLine(string.Format("  tpl:{0} rdf:type owl:Class .", roleID));
                                    sparqlAdd.AppendLine(string.Format("  tpl:{0} rdfs:label \'{1}\'{2} .", roleID, roleLabel, language));
                                    sparqlAdd.AppendLine(string.Format("  tpl:{0} p8:valRoleIndex {1} .", roleID, ++roleCount));
                                    sparqlAdd.AppendLine(string.Format("  tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));
                                    sparqlAdd.AppendLine(string.Format("  tpl:{0} p8:hasRole tpl:{1} .", identifier, roleID));

                                    if (!string.IsNullOrEmpty(role.range))
                                    {
                                        qn = nsMap.ReduceToQName(role.range, out qName);
                                        sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                                    }
                                }
                                sparqlAdd.AppendLine("}");
                            }
                            #endregion

                            // add prefix first
                            sparqlBuilder.Append(prefix);
                            sparqlBuilder.Append(sparqlDelete);
                            sparqlBuilder.Append(sparqlAdd);

                            string sparql = sparqlBuilder.ToString();
                            Response postResponse = PostToRepository(repository, sparql);
                            response.Append(postResponse);
                        }
                    }
                    #endregion Template Definitions

                    #region Template Qualification

                    if (qmxf.templateQualifications.Count > 0)
                    {
                        foreach (TemplateQualification newTemplateQualification in qmxf.templateQualifications)
                        {
                            string language = string.Empty;
                            int roleCount = 0;
                            StringBuilder sparqlAdd = new StringBuilder();

                            sparqlAdd.AppendLine(insertData);
                            bool hasDeletes = false;
                            string templateName = string.Empty;
                            string identifier = string.Empty;
                            string generatedId = string.Empty;
                            string roleQualification = string.Empty;
                            int index = 1;
                            if (!string.IsNullOrEmpty(newTemplateQualification.identifier))
                                identifier = Utility.GetIdFromURI(newTemplateQualification.identifier);

                            templateName = newTemplateQualification.name[0].value;
                            //check for exisitng template
                            QMXF existingQmxf = new QMXF();
                            if (!String.IsNullOrEmpty(identifier))
                            {
                                existingQmxf = GetTemplate(identifier, QMXFType.Qualification, repository);
                            }
                            else
                            {
                                if (_useExampleRegistryBase)
                                    generatedId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                                else
                                    generatedId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                                identifier = Utility.GetIdFromURI(generatedId);
                            }
                            #region Form Delete/Insert SPARQL
                            if (existingQmxf.templateQualifications.Count > 0)
                            {
                                StringBuilder sparqlStmts = new StringBuilder();

                                foreach (TemplateQualification existingTemplate in existingQmxf.templateQualifications)
                                {
                                    foreach (QMXFName name in newTemplateQualification.name)
                                    {
                                        templateName = name.value;
                                        QMXFName existingName = existingTemplate.name.Find(n => n.lang == name.lang);
                                        if (string.IsNullOrEmpty(existingName.lang))
                                            language = "@" + defaultLanguage;
                                        else
                                            language = "@" + existingName.lang;

                                        if (existingName != null)
                                        {
                                            if (String.Compare(existingName.value, name.value, true) != 0)
                                            {
                                                hasDeletes = true;
                                                sparqlStmts.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", identifier, existingName.value, language));
                                                sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));
                                                sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:hasTemplate p8:{1} .", identifier, existingName.value.Remove(0, existingName.value.LastIndexOf("_") + 1)));

                                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", identifier, name.value, language));
                                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));
                                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:hasTemplate p8:{1} .", identifier, name.value.Remove(0, name.value.LastIndexOf("_") + 1)));
                                            }
                                        }
                                    }

                                    //role count
                                    if (existingTemplate.roleQualification.Count != newTemplateQualification.roleQualification.Count)
                                    {
                                        hasDeletes = true;
                                        sparqlStmts.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier, existingTemplate.roleQualification.Count));
                                        sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier, newTemplateQualification.roleQualification.Count));
                                    }

                                    foreach (Specialization spec in newTemplateQualification.specialization)
                                    {
                                        Specialization existingSpecialization = existingTemplate.specialization.Find(n => n.reference == spec.reference);

                                        if (existingSpecialization != null)
                                        {
                                            string specialization = spec.reference;
                                            string existingSpec = existingSpecialization.reference;

                                            hasDeletes = true;
                                            sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateSpecialization .", existingSpec));
                                            sparqlStmts.AppendLine(string.Format("tpl:{0} rdfs:label \'{0}\'{1} .", existingSpec, existingSpecialization.label.Split('@')[0], language));
                                            sparqlStmts.AppendLine(string.Format("tpl:{0} p8:hasSuperTemplate tpl:{1} .", existingSpec, identifier));
                                            sparqlStmts.AppendLine(string.Format("tpl:{0} p8:hasSubTemplate tpl:{1} .", identifier, existingSpec));

                                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{0}\'{1} .", specialization, spec.label.Split('@')[0], language));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasSuperTemplate tpl:{1} .", specialization, identifier));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasSubTemplate tpl:{1} .", identifier, specialization));
                                        }
                                    }

                                    index = 1;
                                    foreach (RoleQualification role in newTemplateQualification.roleQualification)
                                    {
                                        string roleIdentifier = role.identifier;
                                        hasDeletes = false;

                                        //get existing role if it exists
                                        RoleQualification existingRole = existingTemplate.roleQualification.Find(r => r.identifier == role.identifier);

                                        //remove existing role from existing template, leftovers will be deleted later
                                        existingTemplate.roleQualification.Remove(existingRole);

                                        if (existingRole != null)
                                        {
                                            #region Process Changing Role
                                            string label = String.Empty;

                                            foreach (QMXFName name in role.name)
                                            {
                                                QMXFName existingName = existingRole.name.Find(n => n.lang == name.lang);

                                                if (existingName != null)
                                                {
                                                    if (String.Compare(existingName.value, name.value, true) != 0)
                                                    {
                                                        ///TODO: Why are we removing this? We should remove the role from the template only and not from the repository
                                                        hasDeletes = true;
                                                        sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type owl:Class .", existingRole.identifier));
                                                        sparqlStmts.AppendLine(string.Format("tpl:{0}  rdfs:label \'{1}\'{2} .", existingRole.identifier, existingName.value, name.lang));
                                                    }
                                                    //index
                                                    //if (existingRole.designation.value != index.ToString())
                                                    //{
                                                    //    sparqlStmts.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1}^^xsd:int .", existingRole.identifier, existingRole.designation.value));
                                                    //}
                                                }
                                            }
                                            if (existingRole.range != null)
                                            {
                                                if (string.Compare(existingRole.range, role.range, true) != 0)
                                                {
                                                    hasDeletes = true;
                                                    sparqlStmts.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", existingRole.identifier, existingRole.range));
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Insert New Role

                                            string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                            string roleID = string.Empty;
                                            generatedId = string.Empty;
                                            string genName = string.Empty;

                                            if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                                                language = "@" + defaultLanguage;
                                            else
                                                language = "@" + role.name.FirstOrDefault().lang;

                                            genName = "Role Qualification " + roleLabel;
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
                                            // sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type owl:Class .", roleID));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", roleID, roleLabel, language));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1} .", roleID, index));
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));

                                            if (!string.IsNullOrEmpty(role.range))
                                            {
                                                qn = nsMap.ReduceToQName(role.range, out qName);
                                                sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                                            }

                                            //foreach (PropertyRestriction restriction in role.restrictions)
                                            //{
                                            //
                                            //}
                                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRole rdl:{1} .", identifier, roleID));
                                            #endregion
                                        }

                                        index++;
                                    }
                                }

                                sparqlDelete.Append(prefix);
                                sparqlDelete.AppendLine(deleteWhere);
                                sparqlDelete.Append(sparqlStmts);
                                sparqlDelete.AppendLine(" }; ");
                            }
                            #endregion

                            #region Form Insert SPARQL
                            if (hasDeletes)
                            {
                                sparqlAdd.AppendLine("}");
                            }
                            else
                            {
                                string label = String.Empty;
                                string labelSparql = String.Empty;
                                //form labels
                                foreach (QMXFName name in newTemplateQualification.name)
                                {
                                    label = name.value.Split('@')[0];

                                    if (string.IsNullOrEmpty(name.lang))
                                        language = "@" + defaultLanguage;
                                    else
                                        language = "@" + name.lang;
                                }

                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", identifier, label, language));                                
                                sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1} .", identifier, newTemplateQualification.roleQualification.Count));

                                ///TODO: Template Description R# should go here instead
                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));

                                ///TODO: BIG QUESTION AROUND THIS
                                sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:hasTemplate p8:{1} .", identifier, label.Remove(0,label.LastIndexOf("_") + 1)));
                  
                                foreach (Specialization spec in newTemplateQualification.specialization)
                                {
                                    //sparqlStr = new StringBuilder();
                                    string specialization = spec.reference;
                                    sparqlAdd.Append(prefix);
                                    sparqlAdd.AppendLine(insertData);

                                    ///TODO: Generate an id for template specialization??

                                    //sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:subClassOf {1} .", identifier, specialization));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{0}\'{1} .", specialization, spec.label.Split('@')[0], language));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasSuperTemplate tpl:{1} .", specialization, identifier));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasSubTemplate tpl:{1} .", identifier, specialization));
                                }
                                
                                foreach (RoleQualification role in newTemplateQualification.roleQualification)
                                {
                                    string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                                    string roleID = string.Empty;
                                    generatedId = string.Empty;
                                    string genName = string.Empty;
                                    string range = role.range;

                                    if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                                        language = "@" + defaultLanguage;
                                    else
                                      language = "@" + role.name.FirstOrDefault().lang;

                                    genName = "Role Qualification " + roleLabel;
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

                                    ///TODO: p8:TemplateRoleDecription has to be replaced with R#
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateRoleDescription .", roleID));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", roleID, roleLabel, language));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1} .", roleID, ++roleCount));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));
                                    sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRole tpl:{1} .", identifier, roleID));

                                    if (!string.IsNullOrEmpty(role.range))
                                    {
                                        qn = nsMap.ReduceToQName(role.range, out qName);
                                        sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                                    }
                                }
                                sparqlAdd.AppendLine("}");
                            }
                            #endregion

                            // add prefix first
                            sparqlBuilder.Append(prefix);
                            sparqlBuilder.Append(sparqlDelete);
                            sparqlBuilder.Append(sparqlAdd);

                            string sparql = sparqlBuilder.ToString();
                            Response postResponse = PostToRepository(repository, sparql);
                            response.Append(postResponse);
                        }
                    }
                    #endregion
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

            return response;
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

                        string language = string.Empty;
                        List<string> names = new List<string>();
                        StringBuilder sparqlAdd = new StringBuilder();
                        sparqlAdd.AppendLine(insertData);
                        bool hasDeletes = false;
                        int classCount = 0;
                        string clsId = Utility.GetIdFromURI(clsDef.identifier);
                        QMXF existingQmxf = new QMXF();

                        if (!String.IsNullOrEmpty(clsId))
                        {
                            existingQmxf = GetClass(clsId, repository);
                        }

                        // delete class
                        if (existingQmxf.classDefinitions.Count > 0)
                        {
                            StringBuilder sparqlStmts = new StringBuilder();


                            foreach (ClassDefinition existingClsDef in existingQmxf.classDefinitions)
                            {
                                foreach (QMXFName clsName in clsDef.name)
                                {
                                    QMXFName existingName = existingClsDef.name.Find(n => n.lang == clsName.lang);
                                    if (existingName != null)
                                    {
                                        if (String.Compare(existingName.value, clsName.value, true) != 0)
                                        {
                                            hasDeletes = true;
                                            sparqlStmts.AppendLine(string.Format(" rdl:{0} rdfs:label \"{1}{2}\"^^xsd:string . ", clsId, existingName.value, clsName.lang));
                                            sparqlAdd.AppendLine(string.Format(" rdl:{0}  rdfs:label \"{1}{2}\"^^xsd:string .", clsId, clsName.value, clsName.lang));
                                        }
                                    }

                                    foreach (Description description in clsDef.description)
                                    {
                                        Description existingDescription = existingClsDef.description.Find(d => d.lang == description.lang);

                                        if (existingDescription != null)
                                        {
                                            if (String.Compare(existingDescription.value, description.value, true) != 0)
                                            {
                                                hasDeletes = true;
                                                sparqlStmts.AppendLine(string.Format(" rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string . ", clsId, existingDescription.value, description.lang));
                                                sparqlAdd.AppendLine(string.Format(" rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string .", clsId, description.value, description.lang));
                                            }
                                        }
                                    }

                                    // delete specialization
                                    foreach (Specialization spec in clsDef.specialization)
                                    {

                                        Specialization existingSpec = existingClsDef.specialization.Find(s => s.reference == spec.reference);

                                        if (existingSpec != null && existingSpec.reference != null)
                                        {
                                            if (String.Compare(existingSpec.reference, spec.reference, true) != 0)
                                            {
                                                hasDeletes = true;
                                                qn = nsMap.ReduceToQName(existingSpec.reference, out qName);
                                                sparqlStmts.AppendLine(string.Format("  ?a dm:hasSubclass {0} . ", qName));
                                                qn = nsMap.ReduceToQName(spec.reference, out qName);
                                                if (qn)
                                                    sparqlAdd.AppendLine(string.Format(" ?a rdfs:subClassOf {0} .", qName));
                                            }
                                        }
                                    }

                                    // delete classification
                                    foreach (Classification clsif in clsDef.classification)
                                    {
                                        Classification existingClasif = existingClsDef.classification.Find(c => c.reference == clsif.reference);
                                        if (existingClasif != null && existingClasif.reference != null)
                                        {
                                            if (string.Compare(existingClasif.reference, clsif.reference, true) != 0)
                                            {
                                                hasDeletes = true;
                                                qn = nsMap.ReduceToQName(existingClasif.reference, out qName);
                                                if (qn)
                                                {
                                                    sparqlStmts.AppendLine(string.Format(" ?a dm:hasClassified {0} .", qName));
                                                    sparqlStmts.AppendLine(string.Format(" ?a dm:hasClassifier {0} .", qName));
                                                }
                                                qn = nsMap.ReduceToQName(clsif.reference, out qName);
                                                if (qn)
                                                {
                                                    sparqlAdd.AppendLine(string.Format(" ?a dm:hasClassified {0} .", qName));
                                                    sparqlAdd.AppendLine(string.Format(" ?a dm:hasClassifier {0} .", qName));
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                            if (sparqlStmts.Length > 0)
                            {
                                sparqlDelete.AppendLine(deleteWhere);
                                sparqlDelete.Append(sparqlStmts);
                                sparqlDelete.AppendLine(" }; ");
                            }
                        }

                        // add class
                        if (hasDeletes)
                        {
                            sparqlAdd.AppendLine("}");
                        }
                        else
                            foreach (QMXFName clsName in clsDef.name)
                            {
                                string clsLabel = clsName.value.Split('@')[0];

                                if (string.IsNullOrEmpty(clsName.lang))
                                    language = "@" + defaultLanguage;
                                else
                                    language = "@" + clsName.lang;

                                if (string.IsNullOrEmpty(clsId))
                                {
                                    string newClsName = "Class definition " + clsLabel;

                                    clsId = CreateIdsAdiId(registry, newClsName);
                                    clsId = Utility.GetIdFromURI(clsId);
                                }

                                // append label
                                sparqlAdd.AppendLine(string.Format(" rdl:{0} rdf:type owl:Class .", clsId));
                                sparqlAdd.AppendLine(string.Format(" rdl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", clsId, clsLabel, language));

                                // append entity type
                                if (clsDef.entityType != null && !String.IsNullOrEmpty(clsDef.entityType.reference))
                                {
                                    qn = nsMap.ReduceToQName(clsDef.entityType.reference, out qName);

                                    if (qn)
                                        sparqlAdd.AppendLine(string.Format(" rdl:{0} rdf:type {1} .", clsId, qName));

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
                                        sparqlAdd.AppendLine(string.Format(" rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string . ", clsId, description, language));
                                    }
                                }

                                // append specialization
                                foreach (Specialization spec in clsDef.specialization)
                                {
                                    if (!String.IsNullOrEmpty(spec.reference))
                                    {
                                        qn = nsMap.ReduceToQName(spec.reference, out qName);
                                        if (qn)
                                            sparqlAdd.AppendLine(string.Format(" rdl:{0} rdfs:subClassOf {1} .", clsId, qName));
                                    }
                                }

                                classCount = clsDef.classification.Count;

                                // append classification
                                foreach (Classification clsif in clsDef.classification)
                                {
                                    if (!string.IsNullOrEmpty(clsif.reference))
                                    {
                                        qn = nsMap.ReduceToQName(clsif.reference, out qName);

                                        if (repository.RepositoryType == RepositoryType.Part8 && qn)
                                        {
                                            sparqlAdd.AppendLine(string.Format("rdl:{0} rdf:type {1} .", clsId, qName));

                                        }
                                        else if (repository.RepositoryType != RepositoryType.Part8 && qn)
                                        {
                                            sparqlAdd.AppendLine(string.Format("rdl:{0} dm:hasClassifier {1} .", clsId, qName));
                                            sparqlAdd.AppendLine(string.Format("{0} dm:hasClassified rdl:{1} .", qName, clsId));
                                        }
                                    }
                                }

                                sparqlAdd.AppendLine("}");
                            }
                        sparqlBuilder.Append(prefix);
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