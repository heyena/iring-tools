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
using System.Web;
using log4net;
using Ninject;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using org.w3.sparql_results;

namespace org.iringtools.referenceData
{
  // NOTE: If you change the class name "Service" here, you must also update the reference to "Service" in Web.config and in the associated .svc file.
  public class ReferenceDataProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ReferenceDataProvider));

    private Response _response = null;

    private const string REPOSITORIES_FILE_NAME = "Repositories.xml";
    private const string QUERIES_FILE_NAME = "Queries.xml";

    private int _pageSize = 0;

    private bool _useExampleRegistryBase = false;

    private WebCredentials _registryCredentials = null;

    private WebProxyCredentials _proxyCredentials = null;

    private List<Repository> _repositories = null;

    private Queries _queries = null;

    private static Dictionary<string, RefDataEntities> _searchHistory = new Dictionary<string, RefDataEntities>();

    private IKernel _kernel = null;
    private ReferenceDataSettings _settings = null;

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

        string repositoriesPath = _settings["XmlPath"] + REPOSITORIES_FILE_NAME;
        _repositories = Utility.Read<List<Repository>>(repositoriesPath);

        string queriesPath = _settings["XmlPath"] + QUERIES_FILE_NAME;
        _queries = Utility.Read<Queries>(queriesPath);

        _response = new Response();
        _kernel.Bind<Response>().ToConstant(_response);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in initializing ReferenceDataServiceProvider: " + ex);
      }
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

    public List<Repository> GetRepositories()
    {
      try
      {
        List<Repository> repositories;

        repositories = _repositories;

        //Don't Expose Tokens
        foreach (Repository repository in repositories)
        {
          repository.encryptedCredentials = null;
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

        Query queryExactSearch = _queries["ExactSearch"];
        QueryBindings queryBindings = queryExactSearch.bindings;

        sparql = ReadSPARQL(queryExactSearch.fileName);

        sparql = sparql.Replace("param1", query);

        foreach (Repository repository in _repositories)
        {
          SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

          List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);


          foreach (Dictionary<string, string> result in results)
          {
            Entity resultEntity = new Entity
            {
              uri = result["uri"],
              label = result["label"],
              repository = repository.name
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

          Query queryContainsSearch = _queries["ContainsSearch"];
          QueryBindings queryBindings = queryContainsSearch.bindings;

          sparql = ReadSPARQL(queryContainsSearch.fileName);
          sparql = sparql.Replace("param1", query);

          foreach (Repository repository in _repositories)
          {
            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

            List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
            foreach (Dictionary<string, string> result in results)
            {
              Entity resultEntity = new Entity
              {
                uri = result["uri"],
                label = result["label"],
                repository = repository.name
              };

              string key = resultEntity.label;

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

    private string GetLabel(string uri)
    {
      try
      {
        string label = String.Empty;
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        Query query = _queries["GetLabel"];
        QueryBindings queryBindings = query.bindings;

        sparql = ReadSPARQL(query.fileName);
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


        Query queryContainsSearch = _queries["GetClassification"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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

        Query queryGetSpecialization = _queries["GetSpecialization"];
        QueryBindings queryBindings = queryGetSpecialization.bindings;

        sparql = ReadSPARQL(queryGetSpecialization.fileName);
        sparql = sparql.Replace("param1", id);

        Query queryGetSubClassOf = _queries["GetSubClassOf"];
        QueryBindings queryBindingsPart8 = queryGetSubClassOf.bindings;

        sparqlPart8 = ReadSPARQL(queryGetSubClassOf.fileName);
        sparqlPart8 = sparqlPart8.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          if (repository.repositoryType == RepositoryType.Part8)
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

        Query queryContainsSearch = _queries["GetClass"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);

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
            classDefinition.repositoryName = repository.name;
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
            uri = uri,
            label = label
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
            uri = uri,
            label = label
          };

          string trimmedUri = string.Empty;
          bool found = false;
          foreach (Entity entt in list)
          {
            if (resultEntity.uri.Equals(entt.uri))
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

        Query queryGetSubClasses = _queries["GetSubClasses"];
        QueryBindings queryBindings = queryGetSubClasses.bindings;

        sparql = ReadSPARQL(queryGetSubClasses.fileName);
        sparql = sparql.Replace("param1", id);

        Query queryGetSubClassOfInverse = _queries["GetSubClassOfInverse"];
        QueryBindings queryBindingsPart8 = queryGetSubClassOfInverse.bindings;

        sparqlPart8 = ReadSPARQL(queryGetSubClassOfInverse.fileName);
        sparqlPart8 = sparqlPart8.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          if (repository.repositoryType == RepositoryType.Part8)
          {
            SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlPart8);

            List<Dictionary<string, string>> results = BindQueryResults(queryBindingsPart8, sparqlResults);

            foreach (Dictionary<string, string> result in results)
            {
              Entity resultEntity = new Entity
              {
                uri = result["uri"],
                label = result["label"],
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
                uri = result["uri"],
                label = result["label"],
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

        Query queryGetClassTemplates = _queries["GetClassTemplates"];
        QueryBindings queryBindingsGetClassTemplates = queryGetClassTemplates.bindings;

        sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.fileName);
        sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

        Query queryGetRelatedTemplates = _queries["GetRelatedTemplates"];
        QueryBindings queryBindingsGetRelatedTemplates = queryGetRelatedTemplates.bindings;

        sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.fileName);
        sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          if (repository.repositoryType == RepositoryType.Part8)
          {
            SPARQLResults sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

            List<Dictionary<string, string>> results = BindQueryResults(queryBindingsGetRelatedTemplates, sparqlResults);

            foreach (Dictionary<string, string> result in results)
            {
              Entity resultEntity = new Entity
              {
                uri = result["uri"],
                label = result["label"],
                repository = repository.name,
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
                uri = result["uri"],
                label = result["label"],
                repository = repository.name,
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

        Query queryContainsSearch = _queries["GetRoles"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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
              roleDefinition.index = result["index"].ToString();
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
        string rangeSparql = String.Empty;
        string rangeRelativeUri = String.Empty;

        string referenceSparql = String.Empty;
        string referenceRelativeUri = String.Empty;

        string valueSparql = String.Empty;
        string valueRelativeUri = String.Empty;

        Description description = new Description();
        QMXFStatus status = new QMXFStatus();
        //List<Classification> classifications = new List<Classification>();

        List<RoleQualification> roleQualifications = new List<RoleQualification>();

        RefDataEntities rangeResultEntities = new RefDataEntities();
        RefDataEntities referenceResultEntities = new RefDataEntities();
        RefDataEntities valueResultEntities = new RefDataEntities();

        Query getRangeRestriction = _queries["GetRangeRestriction"];
        QueryBindings rangeRestrictionQueryBindings = getRangeRestriction.bindings;

        Query getReferenceRestriction = _queries["GetReferenceRestriction"];
        QueryBindings getReferenceRestrictionQueryBindings = getReferenceRestriction.bindings;

        Query getValueRestriction = _queries["GetValueRestriction"];
        QueryBindings getValueRestrictionQueryBinding = getValueRestriction.bindings;

        rangeSparql = ReadSPARQL(getRangeRestriction.fileName);
        rangeSparql = rangeSparql.Replace("param1", id);

        referenceSparql = ReadSPARQL(getReferenceRestriction.fileName);
        referenceSparql = referenceSparql.Replace("param1", id);

        valueSparql = ReadSPARQL(getValueRestriction.fileName);
        valueSparql = valueSparql.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          SPARQLResults rangeSparqlResults = QueryFromRepository(repository, rangeSparql);
          SPARQLResults referenceSparqlResults = QueryFromRepository(repository, referenceSparql);
          SPARQLResults valueSparqlResults = QueryFromRepository(repository, valueSparql);

          List<Dictionary<string, string>> rangeResults = BindQueryResults(rangeRestrictionQueryBindings, rangeSparqlResults);
          List<Dictionary<string, string>> referenceResults = BindQueryResults(getReferenceRestrictionQueryBindings, referenceSparqlResults);
          List<Dictionary<string, string>> valueResults = BindQueryResults(getValueRestrictionQueryBinding, valueSparqlResults);

          List<Dictionary<string, string>> combinedResults = MergeLists(MergeLists(rangeResults, referenceResults), valueResults);

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

    private TemplateDefinition GetTemplateDefinition(string id)
    {
      TemplateDefinition templateDefinition = null;

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        Description description = new Description();
        QMXFStatus status = new QMXFStatus();

        RefDataEntities resultEntities = new RefDataEntities();

        Query queryContainsSearch = _queries["GetTemplate"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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

            if (result.ContainsKey("creationDate"))
            {
              status.from = result["creationDate"];
            }
            templateDefinition.identifier = "tpl:" + id;
            templateDefinition.name.Add(name);
            templateDefinition.description.Add(description);
            templateDefinition.status.Add(status);
            templateDefinition.repositoryName = repository.name;

            templateDefinition.roleDefinition = GetRoleDefintion(id);
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

    public QMXF GetTemplate(string id)
    {
      QMXF qmxf = new QMXF();

      try
      {
        TemplateQualification templateQualification = GetTemplateQualification(id);

        if (templateQualification != null)
        {
          qmxf.templateQualifications.Add(templateQualification);
        }
        else
        {
          TemplateDefinition templateDefinition = GetTemplateDefinition(id);
          qmxf.templateDefinitions.Add(templateDefinition);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetTemplate: " + ex);
      }

      return qmxf;
    }

    private TemplateQualification GetTemplateQualification(string id)
    {
      TemplateQualification templateQualification = null;

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        RefDataEntities resultEntities = new RefDataEntities();

        Query queryContainsSearch = _queries["GetTemplateQualification"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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
            //templateQualification.repositoryName = repository.name;

            templateQualification.roleQualification = GetRoleQualification(id);
          }
        }

        return templateQualification;
      }
      catch (Exception e)
      {
        _logger.Error("Error in GetTemplateQualification: " + e);
        throw new Exception("Error while Getting Template: " + id + ".\n" + e.ToString(), e);
      }
    }

    public Response PostTemplate(QMXF qmxf)
    {
      Response response = null;
      Status status = null;

      try
      {
        //form prefix sparql
        string prefixSparql = "PREFIX eg: <http://example.org/data#>\n"
                        + "PREFIX rdl: <http://rdl.rdlfacade.org/data#>\n"
                        + "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\n"
                        + "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>\n"
                        + "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>\n"
                        + "PREFIX dm: <http://dm.rdlfacade.org/data#>\n"
                        + "PREFIX tpl: <http://tpl.rdlfacade.org/data#>\n"
                        + "PREFIX owl: <http://www.w3.org/2002/07/owl#>\n";

        //prepare repository
        int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
        Repository source = _repositories[repository];

        //check if we can update this repository
        if (source.isReadOnly)
        {
          status = new Status
          {
            Level = StatusLevel.Error
          };

          status.Messages.Add("Repository, " + source.name + ", is read only.");

          _response.Append(status);

          throw new Exception("Cannot POST template to read only repository.");
        }

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

            identifier = template.identifier;

            //check for exisitng template
            QMXF existingQmxf = null;
            if (identifier != null)
            {
              string id = String.Empty;
              if (!identifier.StartsWith("tpl:"))
              {
                id = identifier.Substring((identifier.LastIndexOf("#") + 1), identifier.Length - (identifier.LastIndexOf("#") + 1));
              }
              else
              {
                id = identifier.Substring(4, (identifier.Length - 4));
              }
              identifier = "<" + "http://tpl.rdlfacade.org/data#" + id + ">";

              existingQmxf = GetTemplate(id);
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
              response = PostToRepository(source, sparql);

              _response.Append(response);
              if (response.Level == StatusLevel.Error)
              {
                throw new Exception("Error while Inserting a new template.");
              }

              status = new Status
              {
                Level = StatusLevel.Success,
              };

              status.Messages.Add("Successfully added template to repository, " + source.name + ".");
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

                  if (existingName.value.ToUpper() != name.value.ToUpper())
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
                  if (existingDescription.value.ToUpper() != description.value.ToUpper())
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
                      if (existingName.value.ToUpper() != name.value.ToUpper())
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

                  if (existingDescription.value.ToUpper() != role.description.value.ToUpper())
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
                      deleteRoleSparql += " rdf:type owl:ObjectProperty ; "
                              + " rdf:type owl:FunctionalProperty ; ";
                    }

                    deleteRoleSparql += "rdfs:range <" + existingRole.range + "> ; ";

                    if (role.range != null && role.range != String.Empty)
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
              response = PostToRepository(source, sparql);

              _response.Append(response);
              if (response.Level == StatusLevel.Error)
              {
                throw new Exception("Error while Updating an existing template.");
              }

              status = new Status
              {
                Level = StatusLevel.Success,
              };

              status.Messages.Add("Successfully updated template in repository, " + source.name + ".");
            }
          }
        }
        #endregion Template Definitions

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
              //string description = string.Empty;
              //ID = template.identifier.Remove(0, 1);
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
              sparql += "INSERT DATA }\n";
              QMXF existingQmxf = new QMXF();
              if (ID != null)
              {
                id = ID.Substring((ID.LastIndexOf("#") + 1), ID.Length - (ID.LastIndexOf("#") + 1));
                existingQmxf = GetTemplate(id);
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
                  //                  prefixSparql = prefixSparql.Insert(prefixSparql.LastIndexOf("."), "}").Remove(prefixSparql.Length - 1);
                  response = PostToRepository(source, prefixSparql);
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

                    if (existingName.value.ToUpper() != name.value.ToUpper())
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
                    if (existingDescription.value.ToUpper() != description.value.ToUpper())
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
                        if (existingName.value.ToUpper() != name.value.ToUpper())
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
                     if (existingRole.description[i].value.ToUpper() != role.description[i].value.ToUpper())
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
                        deleteRoleSparql += " rdf:type owl:ObjectProperty ; "
                                + " rdf:type owl:FunctionalProperty ; ";
                      }

                      deleteRoleSparql += "rdfs:range <" + existingRole.range + "> ; ";

                      if (role.range != null && role.range != String.Empty)
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
                    if (role.qualifies == null || role.qualifies == String.Empty)
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
                    deleteRoleSparql += " rdf:type owl:ObjectProperty ; "
                            + " rdf:type owl:FunctionalProperty ; ";
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
                response = PostToRepository(source, sparql);

                _response.Append(response);
                if (response.Level == StatusLevel.Error)
                {
                  throw new Exception("Error while Updating an existing template.");
                }

                status = new Status
                {
                  Level = StatusLevel.Success,
                };

                status.Messages.Add("Successfully updated template in repository, " + source.name + ".");

                #endregion
              }
            }
          }
        }
        #endregion Template Qualifications
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PostTemplate: " + ex);

        status = new Status
        {
          Level = StatusLevel.Error,
        };

        status.Messages.Add(ex.ToString());
        _response.Append(status);
      }
      return _response;
    }

    private int getIndexFromName(string name)
    {
      try
      {
        int index = 0;
        foreach (Repository repository in _repositories)
        {
          if (repository.name.Equals(name))
          {
            index = _repositories.IndexOf(repository);
            return index;
          }
        }
        foreach (Repository repository in _repositories)
        {
          if (!repository.isReadOnly)
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

          if (source.isReadOnly)
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
              if (classFound.repositoryName.Equals(_repositories[repository].name))
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

        string encryptedCredentials = repository.encryptedCredentials;

        WebCredentials credentials = new WebCredentials(encryptedCredentials);
        if (credentials.isEncrypted) credentials.Decrypt();

        //sparqlResults = SPARQLClient.PostQuery(repository.uri, sparql, credentials, _proxyCredentials);
        sparqlResults = SPARQLClient.Query(repository.uri, sparql, credentials, _proxyCredentials);

        return sparqlResults;
      }
      catch (Exception ex)
      {
        throw ex;
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
      Response response = new Response();
      Status status = null;

      try
      {
        string encryptedCredentials = repository.encryptedCredentials;
        string uri = string.IsNullOrEmpty(repository.updateUri) ? repository.uri : repository.updateUri;

        WebCredentials credentials = new WebCredentials(encryptedCredentials);
        if (credentials.isEncrypted) credentials.Decrypt();

        SPARQLClient.PostQueryAsMultipartMessage(uri, sparql, credentials, _proxyCredentials);


        status = new Status
        {
          Level = StatusLevel.Success,
          Messages = { "Successfully updated Repository" },
        };

      }
      catch (Exception ex)
      {
        status = new Status
        {
          Level = StatusLevel.Error,
          Messages = { ex.ToString() },
        };
      }

      response.DateTimeStamp = DateTime.Now;
      response.Append(status);

      return response;
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
              if (queryBinding.name == sparqlBinding.name)
              {
                string key = queryBinding.name;

                string value = String.Empty;
                string dataType = String.Empty;
                if (queryBinding.type == SPARQLBindingType.Uri)
                {
                  value = sparqlBinding.uri;
                }
                else if (queryBinding.type == SPARQLBindingType.Literal)
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


        Query queryContainsSearch = _queries["GetTemplateClassification"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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


        Query queryContainsSearch = _queries["GetTemplateSpecialization"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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

        Query queryContainsSearch = _queries["GetPart8Template"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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

        Query queryContainsSearch = _queries["GetPart8Roles"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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

        Query queryContainsSearch = _queries["GetPart8RoleRestrictions"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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

      try
      {
        Response response = null;
        string sparql = "PREFIX eg: <http://example.org/data#> "
                        + "PREFIX rdl: <http://rdl.rdlfacade.org/data#> "
                        + "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> "
                        + "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> "
                        + "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#> "
                        + "PREFIX dm: <http://dm.rdlfacade.org/data#> "
                        + "PREFIX part8: <http://tpl.rdlfacade.org/data#> "
                        + "PREFIX owl: <http://www.w3.org/2002/07/owl#> "
                        + "PREFIX owl2xml: <http://www.w3.org/2006/12/owl2-xml#> "
                        + "PREFIX p8: <http://standards.tc184-sc4.org/iso/15926/-8/template-model#> "
                        + "PREFIX templates: <http://standards.tc184-sc4.org/iso/15926/-8/templates#> "
                        + "PREFIX data-model: <http://standards.tc184-sc4.org/iso/15926/-8/data-model#> "
                        + "INSERT DATA { ";

        int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
        Repository source = _repositories[repository];

        if (source.isReadOnly)
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
              q = GetTemplate(id);
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

                sparql += ID + " rdf:type owl:class ; ";
                //append description to sparql query
                int descrCount = template.description.Count;
                if (descrCount == 0)
                {
                  sparql += " rdfs:label \"" + label + "\"^^xsd:string . ";
                }
                else
                {
                  sparql += " rdfs:label \"" + label + "\"^^xsd:string ; ";
                }
                foreach (Description descr in template.description)
                {
                  description = descr.value;

                  if (--descrCount > 0)
                    sparql += " rdfs:comment \"" + description + "\"^^xsd:string ; ";
                  else
                    sparql += " rdfs:comment \"" + description + "\"^^xsd:string . ";
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
                  sparql += roleID + "rdfs:subClassOf _:b" + blankNodeCount + " . " +
                            "_:b" + blankNodeCount++ + " owl:type owl:Class ; " +
                                 "owl:intersectionOf _:b" + blankNodeCount + " . ";

                  int restrictionCount = role.restrictions.Count;
                  foreach (PropertyRestriction restriction in role.restrictions)
                  {
                    sparql += "_:b" + blankNodeCount++ + " rdf:first _:b" + blankNodeCount + " ; ";
                    if (--restrictionCount > 0)
                      sparql += "rdf:rest _:b" + ++blankNodeCount + " . ";
                    else
                      sparql += "rdf:rest rdf:nil .";

                    //now add the first restriction at blankNodeCount-1
                    sparql += "_:b" + blankNodeCount + " rdf:type owl:restriction ; " +
                                    "owl:onProperty " + roleID + " ; " +
                                    "owl:" + restriction.type + " " + restriction.value + " . ";
                  }
                }
                #endregion roles

                sparql = sparql.Insert(sparql.LastIndexOf("."), "}").Remove(sparql.Length - 1);
                response = PostToRepository(source, sparql);
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

    public Response PostPart8Template2(QMXF qmxf)
    {
      Status status = new Status();

      try
      {
        //Response response = null;
        string sparql = string.Empty;
        string tempSparql = string.Empty;

        Query query1 = _queries["PostPart8Template_1"];
        string sparql1 = ReadSPARQL(query1.fileName);

        Query query2 = _queries["PostPart8Template_2"];
        string sparql2 = ReadSPARQL(query2.fileName);

        Query query3 = _queries["PostPart8Template_3"];
        string sparql3 = ReadSPARQL(query3.fileName);

        Query query4 = _queries["PostPart8Template_4"];
        string sparql4 = ReadSPARQL(query4.fileName);

        Query query5_1 = _queries["PostPart8Template_5_1"];
        string sparql5_1 = ReadSPARQL(query5_1.fileName);

        Query query5_2 = _queries["PostPart8Template_5_2"];
        string sparql5_2 = ReadSPARQL(query5_2.fileName);

        int repository = qmxf.targetRepository != null ? getIndexFromName(qmxf.targetRepository) : 0;
        Repository source = _repositories[repository];

        if (source.isReadOnly)
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
              q = GetTemplate(id);
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

        Query queryContainsSearch = _queries["GetPart8Class"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
        sparql = sparql.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

          List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
          // classifications = new List<Classification>();
          specializations = new List<Specialization>();

          classDefinition = new ClassDefinition();

          classDefinition.identifier = "http://rdl.rdlfacade.org/data#" + id;
          classDefinition.repositoryName = repository.name;
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

        Query queryContainsSearch = _queries["GetPart8Specialization"];
        QueryBindings queryBindings = queryContainsSearch.bindings;

        sparql = ReadSPARQL(queryContainsSearch.fileName);
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

          Query queryContainsSearch = _queries["Part8ContainsSearch"];
          QueryBindings queryBindings = queryContainsSearch.bindings;

          sparql = ReadSPARQL(queryContainsSearch.fileName);
          sparql = sparql.Replace("param1", query);

          foreach (Repository repository in _repositories)
          {
            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);


            List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
            foreach (Dictionary<string, string> result in results)
            {
              Entity resultEntity = new Entity
              {
                uri = result["uri"],
                label = result["label"],
                repository = repository.name
              };

              string key = resultEntity.label;

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
        /*SPARQL sparqlQuery = new SPARQL();

        Clause insertClause = new Clause();
        insertClause.SetSPARQLType(Clause.SPARQLType.Insert);

        sparqlQuery.AddClause(insertClause);*/

        string sparql = "PREFIX eg: <http://example.org/data#> "
                        + "PREFIX rdl: <http://rdl.rdlfacade.org/data#> "
                        + "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> "
                        + "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> "
                        + "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#> "
                        + "PREFIX part8: <http://tpl.rdlfacade.org/data#> "
                        + "PREFIX owl: <http://www.w3.org/2002/07/owl#> "
                        + "PREFIX owl2xml: <http://www.w3.org/2006/12/owl2-xml#> "
                        + "PREFIX p8: <http://standards.tc184-sc4.org/iso/15926/-8/template-model#> "
                        + "PREFIX templates: <http://standards.tc184-sc4.org/iso/15926/-8/templates#> "
                        + "PREFIX dm: <http://standards.tc184-sc4.org/iso/15926/-8/data-model#> "
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

          if (source.isReadOnly)
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
              if (classFound.repositoryName.Equals(_repositories[repository].name))
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

              sparql += ID + " rdf:type owl:Class ; ";
              //ID = Class.identifier.Remove(0, 1);

              //if (Class.description.Count == 0)
              //{
              //    sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string . ";
              //}
              //else
              //{
              //    sparql += ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
              //}
              foreach (Description descr in Class.description)
              {
                description = descr.value;
                sparql += "rdfs:comment \"" + description + "\"^^xsd:string ; ";
              }

              //append Specialization to sparql query
              foreach (Specialization spec in Class.specialization)
              {
                /// Note: QMXF contains only superclass info while qxf and rdf contain superclass and subclass info
                specialization = spec.reference;
                sparql += "rdfs:subClassOf " + specialization + "; ";
              }

              //remove last semi-colon
              sparql = sparql.Insert(sparql.LastIndexOf("."), "}").Remove(sparql.Length - 1);
              if (!label.Equals(String.Empty))
              {
                Reset(label);
              }
              _response = PostToRepository(source, sparql);
            }
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

    #endregion Part8
  }
}