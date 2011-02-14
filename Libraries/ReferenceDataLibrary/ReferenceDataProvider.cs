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
          label = result["label"];
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
          Classification classification = null;
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

    /*private QMXF GetTemplate(string id, Repository repository)
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
        }*/

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

                  sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateSpecialization_of_{0}_to_{1}{2}\"^^xds:string ;", specialization, label, language));
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

                  sparqlStr.AppendLine(string.Format("  tpl:TemplateSpecialization_of_{0}_to_{1} rdf:type p8:TemplateSpecialization ;", spec.label.Split('@')[0], label));
                  sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateSpecialization_of_{0}_to_{1}{2}\"^^xds:string ;", spec.label.Split('@')[0], label, language));
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
                sparqlStr.AppendLine(string.Format("  p8:valNumberOfRoles {0} ;", template.roleQualification.Count));
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

                  sparqlStr.AppendLine(string.Format("  tpl:TemplateSpecialization_of_{0}_to_{1} rdf:type p8:TemplateSpecialization ;", spec.label, label));
                  sparqlStr.AppendLine(string.Format("  rdfs:label \"TemplateSpecialization_of_{0}_to_{1} \"^^xds:string ;", spec.label.Split('@')[0], label, language));
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
      bool qn = false;

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
              string language = string.Empty;
              int roleCount = 0;
              StringBuilder sparqlAdd = new StringBuilder();

              sparqlAdd.AppendLine(insertData);
              bool hasDeletes = false;
              string templateName = string.Empty;

              string generatedTempId = string.Empty;
              string roleDefinition = string.Empty;
              int index = 1;

              string identifier = Utility.GetIdFromURI(template.identifier);

              //check for exisitng template
              QMXF existingQmxf = null;
              if (!String.IsNullOrEmpty(identifier))
              {
                existingQmxf = GetTemplate(identifier, QMXFType.Definition, repository);
              }

              #region Form Delete/Insert SPARQL
              if (existingQmxf.templateDefinitions.Count > 0)
              {
                StringBuilder sparqlStmts = new StringBuilder();

                foreach (TemplateDefinition existingTemplate in existingQmxf.templateDefinitions)
                {
                  foreach (QMXFName name in template.name)
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
                        sparqlStmts.AppendLine(string.Format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", identifier, existingName.value, language));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string ;", identifier, name.value, language));
                        sparqlAdd.AppendLine("  rdf:type p8:TemplateDescription ;");
                        sparqlAdd.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string .", name.value, language));
                      }
                    }
                  }

                  //append changing descriptions to each block
                  foreach (Description description in template.description)
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
                        sparqlStmts.AppendLine(string.Format("rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string .", identifier, existingDescription.value, language));
                        sparqlAdd.AppendLine(string.Format("rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string .", identifier, description.value, language));
                      }
                    }
                  }

                  //role count
                  if (existingTemplate.roleDefinition.Count != template.roleDefinition.Count)
                  {
                    hasDeletes = true;
                    //   sparqlStmts.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier, existingTemplate.roleDefinition.Count));
                    //    sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier, template.roleDefinition.Count));
                  }

                  index = 1;
                  foreach (RoleDefinition role in template.roleDefinition)
                  {
                    //string roleIdentifier = "<" + role.identifier + ">";
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
                            sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateRoleDescription ;", existingRole.identifier));
                            sparqlStmts.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string .", existingName.value, name.lang));
                            sparqlAdd.AppendLine("  rdf:type p8:TemplateRoleDescription ;");
                            sparqlAdd.AppendLine(string.Format("rdfs:label \"{0}{1}\"^^xsd:string .", name.value, name.lang));
                          }
                          //index
                          if (existingRole.designation.value != index.ToString())
                          {
                            //                  sparqlStmts.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1}^^xsd:int .", roleIdentifier, existingRole.designation.value));
                            //                 sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1}^^xsd:int .", roleIdentifier, name.value, index));
                          }
                        }
                      }

                      #endregion
                    }
                    else
                    {
                      #region Insert New Role

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

                      sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateRoleDescription ;", roleID));// could get roleIndex from QMXF?
                      sparqlAdd.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", roleLabel, language));
                      sparqlAdd.AppendLine(string.Format("  p8:valRoleIndex {0} ;", index));
                      sparqlAdd.AppendLine(string.Format("  p8:hasTemplate tpl:{0} ;", identifier));

                      if (!string.IsNullOrEmpty(role.range))
                      {
                        qn = nsMap.ReduceToQName(role.range, out qName);
                        sparqlAdd.AppendLine(string.Format("  p8:hasRoleFillerType {0} ;", qName));
                      }

                      sparqlAdd.AppendLine(string.Format("  p8:hasRole rdl:{0} .", roleID));

                      //sparqlStr.AppendLine("  p8:hasRoleFillerType " + range + " ;");
                      #endregion
                    }

                    index++;
                  }

                  /*foreach (RoleDefinition role in existingTemplate.roleDefinition)
                  {
                      #region Delete Leftover Roles
                                    

                      #endregion
                  }*/
                }
                //sparqlDelete.AppendLine(" DELETE { ");
                //sparqlDelete.AppendLine(sparqlStmts);
                //sparqlDelete.AppendLine(" } ");

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
                foreach (QMXFName name in template.name)
                {
                  label = name.value.Split('@')[0];

                  if (string.IsNullOrEmpty(name.lang))
                    language = "@" + defaultLanguage;
                  else
                    language = "@" + name.lang;
                }

                //ID generator
                templateName = "Template definition " + label;
                if (identifier == null || identifier == string.Empty)
                {
                  if (_useExampleRegistryBase)
                    generatedTempId = CreateIdsAdiId(_settings["ExampleRegistryBase"], templateName);
                  else
                    generatedTempId = CreateIdsAdiId(_settings["TemplateRegistryBase"], templateName);

                  identifier = Utility.GetIdFromURI(generatedTempId);

                  Utility.WriteString("\n" + identifier + "\t" + label, "TempDef IDs.log", true);
                }

                sparqlAdd.AppendLine(string.Format("  tpl:{0} rdf:type owl:class ;", identifier));
                sparqlAdd.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));
                sparqlAdd.AppendLine("  rdfs:subClassOf p8:BaseTemplateStatement ;");
                sparqlAdd.AppendLine("  rdf:type p8:Template ;");

                //add descriptions to sparql
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

                    sparqlAdd.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", descr.value.Split('@')[0], language));
                  }
                }

                sparqlAdd.AppendLine(string.Format("  tpl:{0} rdf:type p8:TemplateDescription ;", identifier));
                sparqlAdd.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));
                sparqlAdd.AppendLine(string.Format("  p8:valNumberOfRoles {0} ;", template.roleDefinition.Count));
                // sparqlAdd.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .", identifier));

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

                  sparqlAdd.AppendLine(string.Format("  tpl:{0} rdf:type p8:TemplateRoleDescription ;", roleID));// could get roleIndex from QMXF?
                  sparqlAdd.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", roleLabel, language));
                  sparqlAdd.AppendLine(string.Format("  p8:valRoleIndex {0} ;", ++roleCount));
                  sparqlAdd.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .", identifier));
                  sparqlAdd.AppendLine(string.Format("  tpl:{0} p8:hasRole tpl:{1} .", identifier, roleID));
                  ///TODO
                  //need to work out how to get id for this
                  //  sparqlAdd.AppendLine("  p8:hasRoleFillerType " + range + " ;");
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

          else if (qmxf.templateQualifications.Count > 0)
          {
            bool existInTarget = false;
            foreach (TemplateQualification template in qmxf.templateQualifications)
            {
              string ID = string.Empty;
              string id = string.Empty;
              string label = string.Empty;
              string language = string.Empty;
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
              QMXF existingQmxf = new QMXF();

              if (ID != null)
              {
                id = Utility.GetIdFromURI(ID);
                ID = id;

                existingQmxf = GetTemplate(id, QMXFType.Qualification, repository);
                foreach (TemplateQualification templateFound in existingQmxf.templateQualifications)
                {
                  templateIndex++;
                  if (templateFound.repositoryName.Equals(repository))
                  {
                    existInTarget = true;
                    Utility.WriteString("Template found: " + existingQmxf.templateDefinitions[templateIndex].name[0].value, "stats.log", true);
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

                  sparqlStr.Append(prefix);
                  sparqlStr.AppendLine(insertData);
                  sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type owl:class .", ID));

                  int descrCount = template.description.Count;

                  sparqlStr.AppendLine(string.Format("  tpl:{0}  rdfs:label \"{1}{2}\"^^xsd:string .", ID, label, language));
                  sparqlStr.AppendLine(string.Format("  tpl:{0}  rdf:type p8:Template .", ID));

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

                      sparqlStr.AppendLine(string.Format("  tpl:{0}  rdfs:comment \"{0}{1}\"^^xsd:string .", ID, descr.value.Split('@')[0], language));
                    }

                  }

                  sparqlStr.AppendLine(string.Format("  tpl:{0}  rdf:type p8:TemplateDescription .", ID));
                  sparqlStr.AppendLine(string.Format("  tpl:{0}  rdfs:label \"{1}{2}\"^^xsd:string .", ID, label, language));
                  sparqlStr.AppendLine(string.Format("  tpl:{0}  p8:valNumberOfRoles {1} .", ID, template.roleQualification.Count));
                  // sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .", ID));

                  sparqlStr.AppendLine("}");

                  response = PostToRepository(repository, sparqlStr.ToString());

                  foreach (Specialization spec in template.specialization)
                  {
                    sparqlStr = new StringBuilder();
                    specialization = spec.reference;
                    sparqlStr.Append(prefix);
                    sparqlStr.AppendLine(insertData);

                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:subClassOf {1} .", ID, specialization));

                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:label \"{0}{1}\"^^xds:string .", specialization, spec.label.Split('@')[0], language));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasSuperTemplate tpl:{1} .", specialization, ID));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasSubTemplate tpl:{1} .", ID, specialization));
                    sparqlStr.AppendLine("}");

                    response = PostToRepository(repository, sparqlStr.ToString());
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
                      language = "@" + role.name.FirstOrDefault().lang;

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
                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type p8:TemplateRoleDescription .", roleID));// could get roleIndex from QMXF?
                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", roleID, roleLabel, language));
                    sparqlStr.AppendLine(string.Format("  tpl:{0}  p8:valRoleIndex {1} .", roleID, ++roleCount));
                    sparqlStr.AppendLine(string.Format("  tpl:{0}  p8:hasTemplate tpl:{1} .", roleID, ID));
                    if (!string.IsNullOrEmpty(role.range))
                    {
                      qn = nsMap.ReduceToQName(role.range, out qName);
                      if (qn)
                        sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                    }
                    if (role.value != null)
                    {
                      qn = nsMap.ReduceToQName(role.value.reference, out qName);
                      if (qn)
                        sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                    }
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasRole tpl:{1} .", ID, roleID));
                    sparqlStr.AppendLine("}");

                    response = PostToRepository(repository, sparqlStr.ToString());
                  }
                }
              }
              //else edit template
              else
              {
                string identifier = string.Empty;
                roleCount = 0;

                TemplateDefinition td = existingQmxf.templateDefinitions[templateIndex];
                string rName = string.Empty;

                sparqlStr = new StringBuilder();
                sparqlStr.Append(prefix);
                sparqlStr.AppendLine(deleteWhere);

                identifier = td.identifier;

                sparqlStr.AppendLine(string.Format("{0} rdf:type owl:class ;", identifier));
                sparqlStr.AppendLine(" ?property ?value . ");


                label = td.name[0].value.Split('@')[0];

                sparqlStr.AppendLine(string.Format(" tpl:{0} rdf:type p8:TemplateDescription ;", identifier));
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
                  sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasRole {1} ;", ID, roleID));
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
                response = PostToRepository(repository, sparqlStr.ToString());

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

                  sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type p8:TemplateDescription ;", ID));
                  // sparql.AppendLine(" rdf:type owl:thing ;");
                  sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));
                  sparqlStr.AppendLine(string.Format("  p8:valNumberOfRoles {0} ;", template.roleQualification.Count));
                  //sparqlStr.AppendLine(string.Format("  p8:hasTemplate tpl:{0} .", ID));

                  sparqlStr.AppendLine("}");
                  response = PostToRepository(repository, sparqlStr.ToString());

                  foreach (Specialization spec in template.specialization)
                  {
                    sparqlStr = new StringBuilder();
                    specialization = spec.reference;
                    sparqlStr.Append(prefix);
                    sparqlStr.AppendLine(insertData);

                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:subClassOf {1} .", ID, specialization));

                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:label \"{1}{2}\"^^xds:string .", specialization, spec.label.Split('@')[0], language));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasSuperTemplate {1} .", ID, specialization));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasSubTemplate tpl:{1} .", specialization, ID));
                    sparqlStr.AppendLine("}");

                    response = PostToRepository(repository, sparqlStr.ToString());
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
                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdf:type p8:TemplateRoleDescription .", roleID));// could get roleIndex from QMXF?
                    sparqlStr.AppendLine(string.Format("  tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", roleID, roleLabel, language));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:valRoleIndex {1} .", roleID, roleCount));
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasTemplate tpl:{1} .", roleID, ID));
                    if (!string.IsNullOrEmpty(role.range))
                    {
                      qn = nsMap.ReduceToQName(role.range, out qName);
                      if (qn)
                        sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                    }
                    if (role.value != null)
                    {
                      qn = nsMap.ReduceToQName(role.value.reference, out qName);
                      if (qn)
                        sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                    }
                    sparqlStr.AppendLine(string.Format("  tpl:{0} p8:hasRole rdl:{1} .", ID, roleID));
                    sparqlStr.AppendLine("}");

                    response = PostToRepository(repository, sparqlStr.ToString());
                  }
                }
              }
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

              sparqlStr.AppendLine(string.Format("  rdfs:label \"{0}{1}\"^^xsd:string ;", label, language));

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

                  sparqlStr.AppendLine(string.Format("  rdfs:comment \"{0}{1}\"^^xsd:string ;", description, language));
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
                  sparqlStr.AppendLine(string.Format("  rdfs:subClassOf {0} ;", qName));
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

            string language = string.Empty;
            List<string> names = new List<string>();
            StringBuilder sparqlAdd = new StringBuilder();
            sparqlAdd.AppendLine(insertData);
            bool hasDeletes = false;
            int specCount = 0;
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
                //sparqlDelete.AppendLine(" DELETE { ");
                //sparqlDelete.AppendLine(sparqlStmts);
                //sparqlDelete.AppendLine(" } ");

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