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
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;


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
        nsMap.AddNamespace("eg", new Uri("http://example.org/data#"));
        nsMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
        nsMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
        nsMap.AddNamespace("tpL", new Uri("http://tpl.rdlfacade.org/data#"));
        nsMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
        nsMap.AddNamespace("p8dm", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/data-model#"));
        nsMap.AddNamespace("owl2xml", new Uri("http://www.w3.org/2006/12/owl2-xml#"));
        nsMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
        nsMap.AddNamespace("templates", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/templates#"));
        foreach (string pref in nsMap.Prefixes)
        {
          prefix.AppendLine(String.Format("PREFIX {0}: <{1}>", pref, nsMap.GetNamespaceUri(pref).ToString()));
        }
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
      Entity resultEntity = null;
      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

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
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                {
                  resultEntity.Label = ((LiteralNode)result[v]).Value;
                  resultEntity.Lang = ((LiteralNode)result[v]).Language;
                  if (string.IsNullOrEmpty(resultEntity.Lang))
                    resultEntity.Lang = defaultLanguage;
                }
                else if ((INode)result[v] is UriNode && v.Equals("uri"))
                {
                  resultEntity.Uri = ((UriNode)result[v]).Uri.ToString();
                }
              }
              resultEntity.Repository = repository.Name;
              string key = resultEntity.Label;

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


    private Entity GetLabel(string uri)
    {
      Entity labelEntity = new Entity();

      try
      {
        string label = String.Empty;
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        Query query = (Query)_queries.FirstOrDefault(c => c.Key == "GetLabel").Query;

        sparql = ReadSPARQL(query.FileName);
        sparql = sparql.Replace("param1", uri);

        foreach (Repository repository in _repositories)
        {
          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            foreach(var v in result.Variables)
            {
              if ((INode)result[v] is LiteralNode && v.Equals("label"))
              {
                labelEntity.Label = ((LiteralNode)result[v]).Value;
                labelEntity.Lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(labelEntity.Lang))
                  labelEntity.Lang = defaultLanguage;
              }
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
      QMXF qmxf = new QMXF();

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        List<Classification> classifications = new List<Classification>();
        Query getClassification;

        foreach (Repository repository in _repositories)
        {
          if (rep != null)
            if (rep.Name != repository.Name) continue;

          switch (rep.RepositoryType)
          {
            case RepositoryType.Camelot:
            case RepositoryType.RDSWIP:
              getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassification").Query;

              sparql = ReadSPARQL(getClassification.FileName);
              sparql = sparql.Replace("param1", id);
              classifications = ProcessClassifications(rep, sparql);
              break;
            case RepositoryType.Part8:
              getClassification = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Classification").Query;

              sparql = ReadSPARQL(getClassification.FileName);
              sparql = sparql.Replace("param1", id);
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
      SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);
      List<Classification> classifications = new List<Classification>();
      List<string> names = new List<string>();
      string resultValue = string.Empty;

      foreach (SparqlResult result in sparqlResults)
      {
        Classification classification = new Classification();
        string uri = String.Empty;
        if(result.HasValue("uri"))
        {
         string pref = nsMap.GetPrefix(new Uri(result["uri"].ToString().Substring(0, result["uri"].ToString().IndexOf("#") + 1)));
          if (pref.Equals("owl") || pref.Equals("dm")) continue;
          uri = result["uri"].ToString();
          classification.reference = uri;
        }
        foreach (var v in result.Variables)
        {
          if ((INode)result[v] is LiteralNode && v.Equals("label"))
          {
            classification.label = ((LiteralNode)result[v]).Value;
            classification.lang = ((LiteralNode)result[v]).Language;
            if(string.IsNullOrEmpty(classification.label))
            {
              classification.label =  GetLabel(uri).Label;
              classification.lang = GetLabel(uri).Lang;
            }
            if (string.IsNullOrEmpty(classification.lang))
              classification.lang = defaultLanguage;
          }
        }
        
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

        sparql = ReadSPARQL(queryGetSpecialization.FileName);
        sparql = sparql.Replace("param1", id);

        Query queryGetSubClassOf = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOf").Query;

        sparqlPart8 = ReadSPARQL(queryGetSubClassOf.FileName);
        sparqlPart8 = sparqlPart8.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          if (rep != null)
            if (rep.Name != repository.Name) continue;

          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlPart8);

            foreach (SparqlResult result in sparqlResults)
            {
              Specialization specialization = new Specialization();
              string uri = string.Empty;

              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                {
                  specialization.label = ((LiteralNode)result[v]).Value;
                  specialization.lang = ((LiteralNode)result[v]).Language;
                }
                else if ((INode)result[v] is UriNode && v.Equals("uri"))
                {
                  specialization.reference = ((UriNode)result[v]).Uri.ToString();
                  uri = specialization.reference;
                }
              }
              if (string.IsNullOrEmpty(specialization.label))
              {
                specialization.label = GetLabel(uri).Label;
                specialization.lang = GetLabel(uri).Lang;
              }
              Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
            }
          }
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);
            foreach (SparqlResult result in sparqlResults)
            {
              Specialization specialization = new Specialization();
              string uri = string.Empty;

              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                {
                  specialization.label = ((LiteralNode)result[v]).Value;
                  specialization.lang = ((LiteralNode)result[v]).Language;
                }
                else if ((INode)result[v] is UriNode && v.Equals("uri"))
                {
                  specialization.reference = ((UriNode)result[v]).Uri.ToString();
                  uri = specialization.reference;
                }
              }
              if (string.IsNullOrEmpty(specialization.label))
              {
                specialization.label = GetLabel(uri).Label;
                specialization.lang = GetLabel(uri).Lang;
              }
              Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
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

        List<Classification> classifications = new List<Classification>();
        List<Specialization> specializations = new List<Specialization>();

        RefDataEntities resultEntities = new RefDataEntities();
        List<Entity> resultEnt = new List<Entity>();
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        string resultValue = string.Empty;
        string dataType = string.Empty;

        Query queryContainsSearch = (Query)_queries.FirstOrDefault(c => c.Key == "GetClass").Query;

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

          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);
          classifications = new List<Classification>();
          specializations = new List<Specialization>();

          foreach (SparqlResult result in sparqlResults)
          {
            classDefinition = new ClassDefinition();

            classDefinition.identifier = uri;
            classDefinition.repositoryName = repository.Name;
            name = new QMXFName();
            description = new Description();
            status = new QMXFStatus();
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
              else if ((INode)result[v] is LiteralNode && v.Equals("creator"))
              {
                status.authority = ((LiteralNode)result[v]).Value;
              }
              else if ((INode)result[v] is LiteralNode && v.Equals("creationDate"))
              {
                status.from = ((LiteralNode)result[v]).Value;
              }
              else if ((INode)result[v] is LiteralNode && v.Equals("class"))
              {
                status.Class = ((LiteralNode)result[v]).Value;
              }
              else if ((INode)result[v] is LiteralNode && v.Equals("comment"))
              {
                description.value = ((LiteralNode)result[v]).Value;
                description.lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(description.lang))
                  description.lang = defaultLanguage;
              }
              else if ((INode)result[v] is UriNode && v.Equals("type"))
              {
                string typeName = ((UriNode)result[v]).Uri.ToString();
                string pref = nsMap.GetPrefix(new Uri(typeName.Substring(0, typeName.IndexOf("#") + 1)));
                if (pref == "dm")
                  classDefinition.entityType = new EntityType { reference = typeName };
              }
              else if ((INode)result[v] is UriNode && v.Equals("authority"))
              {
                status.authority = ((UriNode)result[v]).Uri.ToString();
              }
              else if ((INode)result[v] is UriNode && v.Equals("recorded"))
              {
                status.Class = ((UriNode)result[v]).Uri.ToString();
              }
              else if ((INode)result[v] is LiteralNode && v.Equals("from"))
              {
                status.from = ((LiteralNode)result[v]).Value;
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
        Entity resultEntity = null;

        Query getMembers = (Query)_queries.FirstOrDefault(c => c.Key == "GetMembers").Query;
        sparql = ReadSPARQL(getMembers.FileName);
        sparql = sparql.Replace("param1", Id);

        foreach (Repository repository in _repositories)
        {
          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            resultEntity = new Entity();
            foreach (var v in result.Variables)
            {
              if ((INode)result[v] is LiteralNode)
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
        Entity resultEntity = null;

        Query queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClasses").Query;

        sparql = ReadSPARQL(queryGetSubClasses.FileName);
        sparql = sparql.Replace("param1", id);

        Query queryGetSubClassOfInverse = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOfInverse").Query;

        sparqlPart8 = ReadSPARQL(queryGetSubClassOfInverse.FileName);
        sparqlPart8 = sparqlPart8.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlPart8);

            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode)
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
              //queryResult.Add(resultEntity);
            }
          }
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode)
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

        Query queryGetSubClasses = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassesCount").Query;

        sparql = ReadSPARQL(queryGetSubClasses.FileName);
        sparql = sparql.Replace("param1", id);

        Query queryGetSubClassOfInverse = (Query)_queries.FirstOrDefault(c => c.Key == "GetSubClassOfInverseCount").Query;

        sparqlPart8 = ReadSPARQL(queryGetSubClassOfInverse.FileName);
        sparqlPart8 = sparqlPart8.Replace("param1", id);

        int count = 0;
        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlPart8);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
            }
          }
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
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
      Entity resultEntity = null;

      try
      {
        string sparqlGetClassTemplates = String.Empty;
        string sparqlGetRelatedTemplates = String.Empty;
        string relativeUri = String.Empty;

        Query queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplates").Query;

        sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.FileName);
        sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

        Query queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplates").Query;

        sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.FileName);
        sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

            foreach (SparqlResult result in sparqlResults.Results)
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
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);

            foreach (SparqlResult result in sparqlResults)
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
      Entities queryResult = new Entities();

      try
      {
        string sparqlGetClassTemplates = String.Empty;
        string sparqlGetRelatedTemplates = String.Empty;
        string relativeUri = String.Empty;

        Query queryGetClassTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetClassTemplatesCount").Query;

        sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.FileName);
        sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);

        Query queryGetRelatedTemplates = (Query)_queries.FirstOrDefault(c => c.Key == "GetRelatedTemplatesCount").Query;

        sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.FileName);
        sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

        int count = 0;
        foreach (Repository repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
            }
          }
          else
          {
            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);

            foreach (SparqlResult result in sparqlResults)
            {
              foreach (var v in result.Variables)
              {
                count += Convert.ToInt32(((LiteralNode)result[v]).Value);
              }
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

        sparql = ReadSPARQL(queryContainsSearch.FileName);
        sparql = sparql.Replace("param1", id);

        SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

        foreach (SparqlResult result in sparqlResults)
        {
          RoleDefinition roleDefinition = new RoleDefinition();
          QMXFName name = new QMXFName();
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
        string sparql = String.Empty;
        string relativeUri = String.Empty;
        string sparqlQuery = string.Empty;

        Description description = new Description();
        QMXFStatus status = new QMXFStatus();

        List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();

        RefDataEntities resultEntities = new RefDataEntities();

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

          sparql = ReadSPARQL(queryContainsSearch.FileName);
          sparql = sparql.Replace("param1", id);
          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            RoleDefinition roleDefinition = new RoleDefinition();
            QMXFName name = new QMXFName();

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
                description.value = ((LiteralNode)result[v]).Value;
                description.lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(description.lang))
                  description.lang = defaultLanguage;
              }
              else if ((INode)result[v] is LiteralNode && v.Equals("index"))
              {
                if (string.IsNullOrEmpty(description.value))
                {
                  description.value = ((LiteralNode)result[v]).Value;
                  description.lang = ((LiteralNode)result[v]).Language;
                  if (string.IsNullOrEmpty(description.lang))
                    description.lang = defaultLanguage;
                }
              }
              else if ((INode)result[v] is UriNode && v.Equals("role"))
              {
                roleDefinition.identifier = ((UriNode)result[v]).Uri.ToString();
              }
              else if ((INode)result[v] is UriNode && v.Equals("type"))
              {
                roleDefinition.range = ((UriNode)result[v]).Uri.ToString();
              }
            }
            roleDefinition.description = description;
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
        string uri = String.Empty;
        string nameValue = string.Empty;

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

              Query getReferenceRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetReferenceRestriction").Query;

              Query getValueRestriction = (Query)_queries.FirstOrDefault(c => c.Key == "GetValueRestriction").Query;

              rangeSparql = ReadSPARQL(getRangeRestriction.FileName);
              rangeSparql = rangeSparql.Replace("param1", id);

              referenceSparql = ReadSPARQL(getReferenceRestriction.FileName);
              referenceSparql = referenceSparql.Replace("param1", id);

              valueSparql = ReadSPARQL(getValueRestriction.FileName);
              valueSparql = valueSparql.Replace("param1", id);

              SparqlResultSet rangeSparqlResults = QueryFromRepository(repository, rangeSparql);
              SparqlResultSet referenceSparqlResults = QueryFromRepository(repository, referenceSparql);
              SparqlResultSet valueSparqlResults = QueryFromRepository(repository, valueSparql);

              SparqlResultSet combinedResults = rangeSparqlResults;
              combinedResults.Results.AddRange(referenceSparqlResults);
              combinedResults.Results.AddRange(valueSparqlResults);

              foreach (SparqlResult result in combinedResults)
              {
                RoleQualification roleQualification = new RoleQualification();
                QMXFName name = new QMXFName();
                QMXFValue refvalue = new QMXFValue();
                QMXFValue valvalue = new QMXFValue();
                foreach (var v in result.Variables)
                {
                  if ((INode)result[v] is UriNode && v.Equals("qualifies"))
                  {
                    uri = ((UriNode)result[v]).Uri.ToString();
                    roleQualification.qualifies = uri;
                    roleQualification.identifier = Utility.GetIdFromURI(uri);
                  }
                  else if ((INode)result[v] is LiteralNode && v.Equals("name"))
                  {
                    nameValue = ((LiteralNode)result[v]).Value;
                    if (string.IsNullOrEmpty(nameValue))
                    {
                      nameValue = GetLabel(uri).Label;
                    }
                    name.value = nameValue;
                    if (string.IsNullOrEmpty(((LiteralNode)result[v]).Language))
                    {
                      name.lang = defaultLanguage;
                    }
                    else
                    {
                      name.lang = ((LiteralNode)result[v]).Language;
                    }
                  } 
                  else if ((INode)result[v] is UriNode && v.Equals("range"))
                  {
                    roleQualification.range = ((UriNode)result[v]).Uri.ToString();
                  }
                  else if ((INode)result[v] is UriNode && v.Equals("reference"))
                  {
                    refvalue.reference = ((UriNode)result[v]).Uri.ToString();
                    roleQualification.value = refvalue;
                  }
                  else if ((INode)result[v] is LiteralNode && v.Equals("value"))
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
              RefDataEntities part8Entities = new RefDataEntities();
              Query getPart8Roles = (Query)_queries.FirstOrDefault(c => c.Key == "GetPart8Roles").Query;
              QueryBindings getPart8RolesBindings = getPart8Roles.Bindings;

              string part8RolesSparql = ReadSPARQL(getPart8Roles.FileName);
              part8RolesSparql = part8RolesSparql.Replace("param1", id);
              SparqlResultSet part8RolesResults = QueryFromRepository(repository, part8RolesSparql);
              foreach (SparqlResult result in part8RolesResults)
              {
                RoleQualification roleQualification = new RoleQualification();
                QMXFName name = new QMXFName();
                QMXFValue refvalue = new QMXFValue();
                QMXFValue valvalue = new QMXFValue();
                Description descr = new Description();

                foreach (var v in result.Variables)
                {
                  if ((INode)result[v] is UriNode && v.Equals("role"))
                  {
                    uri = ((UriNode)result[v]).Uri.ToString();
                    roleQualification.qualifies = uri;
                    roleQualification.identifier = Utility.GetIdFromURI(uri);
                  }
                  else if ((INode)result[v] is LiteralNode && v.Equals("comment"))
                  {
                    descr.value = ((LiteralNode)result[v]).Value;
                    if (string.IsNullOrEmpty(((LiteralNode)result[v]).Language))
                    {
                      descr.lang = defaultLanguage;
                    }
                    else
                    {
                      descr.lang = ((LiteralNode)result[v]).Language;
                    }
                  }
                  else if ((INode)result[v] is UriNode && v.Equals("type"))
                  {
                    roleQualification.range = ((UriNode)result[v]).Uri.ToString();
                  }
                  else if ((INode)result[v] is LiteralNode && v.Equals("label"))
                  {
                    if (string.IsNullOrEmpty(((LiteralNode)result[v]).Value))
                    {
                      name.value = GetLabel(uri).Label;
                      name.lang = GetLabel(uri).Lang;
                      if (string.IsNullOrEmpty(name.lang))
                        name.lang = defaultLanguage;
                    }
                    else
                    {
                      name.value = ((LiteralNode)result[v]).Value;
                      name.lang = ((LiteralNode)result[v]).Language;
                      if (string.IsNullOrEmpty(name.lang))
                        name.lang = defaultLanguage;
                    }
                  }
                  else if ((INode)result[v] is LiteralNode && v.Equals("index"))
                  {
                    ///TODO
                  }
                  roleQualification.name.Add(name);
                }


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
      List<TemplateDefinition> templateDefinitionList = new List<TemplateDefinition>();
      TemplateDefinition templateDefinition = null;

      try
      {
        string sparql = String.Empty;
        string relativeUri = String.Empty;

        Query queryContainsSearch = null;
        Description description = new Description();
        QMXFStatus status = new QMXFStatus();

        foreach (Repository repository in _repositories)
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

          sparql = ReadSPARQL(queryContainsSearch.FileName);
          sparql = sparql.Replace("param1", id);

          SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

          foreach (SparqlResult result in sparqlResults)
          {
            if (result.Count == 0) continue;
            templateDefinition = new TemplateDefinition();
            QMXFName name = new QMXFName();
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
                status.from = ((LiteralNode)result[v]).Value;
              }
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
        string dataType = string.Empty;

        Query getTemplateQualification = null;

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
                sparqlQuery = "GetTemplateQualificationPart8";
                break;
            }

            getTemplateQualification = (Query)_queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

            sparql = ReadSPARQL(getTemplateQualification.FileName);
            sparql = sparql.Replace("param1", id);

            SparqlResultSet sparqlResults = QueryFromRepository(repository, sparql);

            foreach (SparqlResult result in sparqlResults.Results)
            {
              templateQualification = new TemplateQualification();
              Description description = new Description();
              QMXFStatus status = new QMXFStatus();
              QMXFName name = new QMXFName();

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

    private SparqlResultSet QueryFromRepository(Repository repository, string sparql)
    {
      try
      {
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(repository.Uri));
        string encryptedCredentials = repository.EncryptedCredentials;
        WebCredentials credentials = new WebCredentials(encryptedCredentials);
        if (credentials.isEncrypted) credentials.Decrypt();
        ///TODO need to implement proxy credentials
        endpoint.Credentials = credentials.GetNetworkCredential();
        SparqlResultSet resultSet = endpoint.QueryWithResultSet(sparql);
        resultSet.Trim();
        return resultSet;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Failed to read repository['{0}']", repository.Uri), ex);
        return new SparqlResultSet();
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
        SparqlRemoteUpdateEndpoint endpoint = new SparqlRemoteUpdateEndpoint(repository.UpdateUri);
        ///TODO setting proxy when required
        //                if(_proxyCredentials != null)
        //                    endpoint.ProxyCredentials = _proxyCredentials.GetNetworkCredential();
        endpoint.Credentials = credentials.GetNetworkCredential();
        endpoint.Update(sparql);

        return response;
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

    public Response PostTemplate(QMXF qmxf)
    {

      Graph delete = new Graph();
      Graph insert = new Graph();
      //add namespaces to graphs 
      delete.NamespaceMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("tpl", new Uri("http://tpl.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
      delete.NamespaceMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
      insert.NamespaceMap.Import(delete.NamespaceMap);
      NTriplesFormatter formatter = new NTriplesFormatter();

      Response response = new Response();
      response.Level = StatusLevel.Success;
      Repository repository = null;
      bool qn = false;

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
                    if (existingName != null)
                    {
                      if (String.Compare(existingName.value, name.value, true) != 0)
                      {
                        IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", identifier));
                        IUriNode dpred = delete.CreateUriNode("rdfs:label");
                        ILiteralNode dobj = delete.CreateLiteralNode(existingName.value, string.IsNullOrEmpty(existingName.lang) ? existingName.lang : defaultLanguage);
                        delete.Assert(new Triple(dsubj, dpred, dobj));
                        IUriNode isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                        IUriNode ipred = insert.CreateUriNode("rdfs:label");
                        ILiteralNode iobj = insert.CreateLiteralNode(name.value, string.IsNullOrEmpty(name.lang) ? name.lang : defaultLanguage);
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                    }
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                      IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", identifier));
                      IUriNode dpred = delete.CreateUriNode("rdf:type");
                      IUriNode dobj  = delete.CreateUriNode("p8:TemplateDescription");
                      delete.Assert(new Triple(dsubj, dpred, dobj));
                      IUriNode isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                      IUriNode ipred = insert.CreateUriNode("rdf:type");
                      IUriNode iobj  = insert.CreateUriNode("p8:TemplateDescription");
                      insert.Assert(new Triple(isubj, ipred, iobj));
                      dobj = delete.CreateUriNode("p8:BaseTemplate");
                      delete.Assert(new Triple(dsubj, dpred, dobj));
                      iobj = insert.CreateUriNode("p8:BaseTemplate");
                      insert.Assert(new Triple(isubj, ipred, iobj));
                    }
                  }
                  //append changing descriptions to each block
                  foreach (Description description in newTemplateDefinition.description)
                  {
                    Description existingDescription = existingTemplate.description.Find(d => d.lang == description.lang);

                    if (existingDescription != null)
                    {
                      if (String.Compare(existingDescription.value, description.value, true) != 0)
                      {
                        IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", identifier));
                        IUriNode dpred = delete.CreateUriNode("rdfs:comment");
                        ILiteralNode dobj = delete.CreateLiteralNode(existingDescription.value, string.IsNullOrEmpty(existingDescription.lang) ? existingDescription.lang : defaultLanguage);
                        delete.Assert(new Triple(dsubj, dpred, dobj));
                        IUriNode isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                        IUriNode ipred = insert.CreateUriNode("rdfs:comment");
                        ILiteralNode iobj = insert.CreateLiteralNode(description.value, string.IsNullOrEmpty(description.lang) ? description.lang : defaultLanguage);
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                    }
                  }
                  //role count
                  if (existingTemplate.roleDefinition.Count != newTemplateDefinition.roleDefinition.Count)
                  {

                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                      IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", identifier));
                      IUriNode dpred = delete.CreateUriNode("p8:valNumberOfRoles");
                      ILiteralNode dobj = delete.CreateLiteralNode(Convert.ToString(existingTemplate.roleDefinition.Count), new Uri("xsd:integer"));
                      delete.Assert(new Triple(dsubj, dpred, dobj));
                      IUriNode isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                      IUriNode ipred = insert.CreateUriNode("p8:valNumberOfRoles");
                      ILiteralNode iobj = insert.CreateLiteralNode(Convert.ToString(newTemplateDefinition.roleDefinition.Count), new Uri("xsd:integer"));
                      insert.Assert(new Triple(isubj, ipred, iobj));
                    }
                    else
                    {
                      IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", identifier));
                      IUriNode dpred = delete.CreateUriNode("tpl:R35529169909");
                      ILiteralNode dobj = delete.CreateLiteralNode(Convert.ToString(existingTemplate.roleDefinition.Count), new Uri("xsd:integer"));
                      delete.Assert(new Triple(dsubj, dpred, dobj));
                      IUriNode isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                      IUriNode ipred = insert.CreateUriNode("tpl:R35529169909");
                      ILiteralNode iobj = insert.CreateLiteralNode(Convert.ToString(newTemplateDefinition.roleDefinition.Count), new Uri("xsd:integer"));
                      insert.Assert(new Triple(isubj, ipred, iobj));
                    }
                  }

                  index = 1;
                  foreach (RoleDefinition role in newTemplateDefinition.roleDefinition)
                  {
                    string roleIdentifier = role.identifier;
                    if (!role.identifier.Contains("#"))
                      roleIdentifier = nsMap.GetNamespaceUri("tpl") + "#" + role.identifier;
                    //get existing role if it exists
                    RoleDefinition existingRole = existingTemplate.roleDefinition.Find(r => r.identifier == role.identifier);

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
                            IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", existingRole.identifier));
                            IUriNode dpred = delete.CreateUriNode("rdfs:label");
                            ILiteralNode dobj = delete.CreateLiteralNode( existingName.value, string.IsNullOrEmpty(existingName.lang) ? existingName.lang : defaultLanguage);
                            delete.Assert(new Triple(dsubj, dpred, dobj));
                            if (repository.RepositoryType == RepositoryType.Part8)
                            {
                              dpred = delete.CreateUriNode(" rdf:type");
                              IUriNode udobj = delete.CreateUriNode("owl:Class");
                              delete.Assert(new Triple(dsubj, dpred, udobj));
                            }
                            else
                            {
                              IUriNode udobj = delete.CreateUriNode("tpl:R74478971040");
                              delete.Assert(new Triple(dsubj, dpred, udobj));
                              dpred = delete.CreateUriNode("rdfs:domain");
                              udobj = delete.CreateUriNode(string.Format("tpl:{0}",identifier));
                              delete.Assert(new Triple(dsubj, dpred, udobj));
                            }
                          }
                          //index
                          if (existingRole.designation != null && existingRole.designation.value != index.ToString())
                          {
                            if (repository.RepositoryType == RepositoryType.Part8)
                            {
                              IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", existingRole.identifier));
                              IUriNode dpred = delete.CreateUriNode("p8:valRoleIndex");
                              ILiteralNode dlobj = delete.CreateLiteralNode(Convert.ToString(existingRole.designation.value), new Uri("xsd:integer"));
                              delete.Assert(new Triple(dsubj, dpred, dlobj));
                            }
                            else
                            {
                              IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", existingRole.identifier));
                              IUriNode dpred = delete.CreateUriNode("tpl:R97483568938");
                              ILiteralNode dlobj = delete.CreateLiteralNode(Convert.ToString(existingRole.designation.value), new Uri("xsd:integer"));
                              delete.Assert(new Triple(dsubj, dpred, dlobj));
                            }
                          }
                        }
                      }
                      if (existingRole.range != null)
                      {
                        if (string.Compare(existingRole.range, role.range, true) != 0)
                        {
                          qn = nsMap.ReduceToQName(existingRole.range, out qName);
                          if (repository.RepositoryType == RepositoryType.Part8)
                          {
                            IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", existingRole.identifier));
                            IUriNode dpred = delete.CreateUriNode("p8:hasRoleFillerType");
                            IUriNode dobj = delete.CreateUriNode(qName);
                            delete.Assert(new Triple(dsubj, dpred, dobj));
                          }
                          else
                          {
                            if (qName.StartsWith("rdfs") || qName.StartsWith("rdf"))
                            {
                              IUriNode dsubj = delete.CreateUriNode(string.Format("tpl:{0}", existingRole.identifier));
                              IUriNode dpred = delete.CreateUriNode("rdf:type");
                              IUriNode dobj = delete.CreateUriNode("owl:DataTypeProperty");
                              delete.Assert(new Triple(dsubj, dpred, dobj));
                            }
                            else
                            {
                              IUriNode dsubj = delete.CreateUriNode(existingRole.identifier);
                              IUriNode dpred = delete.CreateUriNode("rdf:type");
                              IUriNode dobj = delete.CreateUriNode("owl:ObjectProperty");
                              delete.Assert(new Triple(dsubj, dpred, dobj));
                              dobj = delete.CreateUriNode("owl:FunctionalProperty");
                              delete.Assert(new Triple(dsubj, dpred, dobj));
                            }
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
                      IUriNode isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                      IUriNode ipred = insert.CreateUriNode("rdfs:label");
                      ILiteralNode ilobj = insert.CreateLiteralNode(role.name[0].value, string.IsNullOrEmpty(role.name[0].lang) ? role.name[0].lang : defaultLanguage);
                      insert.Assert(new Triple(isubj, ipred, ilobj));
                      if (repository.RepositoryType == RepositoryType.Part8)
                      {
                        isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                        ipred = insert.CreateUriNode("p8:valRoleIndex");
                        ilobj = insert.CreateLiteralNode(Convert.ToString(index), new Uri("xsd:integer"));
                        insert.Assert(new Triple(isubj, ipred, ilobj));
                        ipred = insert.CreateUriNode("p8:hasTemplate");
                        IUriNode iobj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                      else
                      {
                        ipred = insert.CreateUriNode("rdfs:domain");
                        IUriNode iobj = insert.CreateUriNode(string.Format("tpl:{0}",identifier));
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                      if (!string.IsNullOrEmpty(role.range))
                      {
                        qn = nsMap.ReduceToQName(role.range, out qName);
                        if (repository.RepositoryType == RepositoryType.Part8)
                        {
                          isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                          ipred = insert.CreateUriNode("p8:hasRoleFillerType");
                          IUriNode iobj = insert.CreateUriNode(qName);
                          insert.Assert(new Triple(isubj, ipred, iobj));
                        }
                        else
                        {
                          ipred = insert.CreateUriNode("rdf:type");
                          IUriNode iobj = insert.CreateUriNode("tpl:R74478971040");
                          insert.Assert(new Triple(isubj, ipred, iobj));
                          if (qName.StartsWith("rdfs") || qName.StartsWith("rdf"))
                          {
                            ipred = insert.CreateUriNode("rdf:type");
                            iobj = insert.CreateUriNode("owl:DataTypeProperty");
                            insert.Assert(new Triple(isubj, ipred, iobj));
                          }
                          else
                          {
                            ipred = insert.CreateUriNode("rdf:type");
                            iobj = insert.CreateUriNode("owl:ObjectProperty");
                            insert.Assert(new Triple(isubj, ipred, iobj));
                            iobj = insert.CreateUriNode("owl:FunctionalProperty");
                            insert.Assert(new Triple(isubj, ipred, iobj));
                            ipred = insert.CreateUriNode("rdfs:range");
                            iobj = insert.CreateUriNode(qName);
                            insert.Assert(new Triple(isubj, ipred, iobj));
                          }
                        }
                      }
                      if (repository.RepositoryType == RepositoryType.Part8)
                      {
                        isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                        ipred = insert.CreateUriNode("p8:hasRole");
                        IUriNode iobj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                      else
                      {
                        isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                        ipred = insert.CreateUriNode("rdfs:domain");
                        IUriNode iobj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                      #endregion
                    }

                    index++;
                  }
                }

                if (!delete.IsEmpty)
                {
                  sparqlDelete.AppendLine(deleteData);
                  foreach (Triple t in delete.Triples)
                  {
                    sparqlDelete.AppendLine(t.ToString(formatter));
                  }
                  sparqlDelete.AppendLine(" }; ");

                  if (!insert.IsEmpty)
                  {
                    foreach (Triple t in insert.Triples)
                    {
                      sparqlAdd.AppendLine(t.ToString(formatter));
                    } 
                  }
                }
              }
              #endregion

              #region Form Insert SPARQL
              if (!insert.IsEmpty)
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
                    language =  defaultLanguage;
                  else
                    language =  name.lang;
                }
                IUriNode isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                IUriNode ipred = insert.CreateUriNode("rdfs:label");
                ILiteralNode ilobj = insert.CreateLiteralNode(label, language);
                insert.Assert(new Triple(isubj, ipred, ilobj));
                if (repository.RepositoryType == RepositoryType.Part8)
                {
                  ipred = insert.CreateUriNode("rdf:type");
                  IUriNode iobj = insert.CreateUriNode("p8:TemplateDescription");
                  insert.Assert(new Triple(isubj, ipred, iobj));
                  iobj = insert.CreateUriNode("p8:BaseTemplate");
                  insert.Assert(new Triple(isubj, ipred, iobj));
                }
                //add descriptions to sparql
                foreach (Description descr in newTemplateDefinition.description)
                {
                  if (string.IsNullOrEmpty(descr.value))
                    continue;
                  else
                  {
                    if (string.IsNullOrEmpty(descr.lang))
                      language = defaultLanguage;
                    else
                      language = descr.lang;
                    isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                    ipred = insert.CreateUriNode("rdfs:comment");
                    ilobj = insert.CreateLiteralNode(descr.value, language);
                    insert.Assert(new Triple(isubj, ipred, ilobj));
                  }
                }
                if (repository.RepositoryType == RepositoryType.Part8)
                {
                  isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                  ipred = insert.CreateUriNode("p8:valNumberOfRoles");
                  ilobj = insert.CreateLiteralNode(Convert.ToString(newTemplateDefinition.roleDefinition.Count), new Uri("xsd:integer"));
                  insert.Assert(new Triple(isubj, ipred, ilobj));
                }
                else
                {
                  isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                  ipred = insert.CreateUriNode("tpl:R35529169909");
                  ilobj = insert.CreateLiteralNode(Convert.ToString(newTemplateDefinition.roleDefinition.Count), new Uri("xsd:integer"));
                  insert.Assert(new Triple(isubj, ipred, ilobj)); 
                }
                foreach (RoleDefinition role in newTemplateDefinition.roleDefinition)
                {
                  string roleLabel = role.name.FirstOrDefault().value.Split('@')[0];
                  string roleID = string.Empty;
                  generatedId = string.Empty;
                  string genName = string.Empty;
                  string range = role.range;

                  if (string.IsNullOrEmpty(role.name.FirstOrDefault().lang))
                    language = defaultLanguage;
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
                  isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                  ipred = insert.CreateUriNode("rdfs:label");
                  ilobj = insert.CreateLiteralNode(roleLabel, language);
                  insert.Assert(new Triple(isubj, ipred, ilobj));

                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                    ipred = insert.CreateUriNode("p8:valRoleIndex");
                    ilobj = insert.CreateLiteralNode(Convert.ToString(++roleCount), new Uri("xsd:integer"));
                    insert.Assert(new Triple(isubj, ipred, ilobj));
                    ipred = insert.CreateUriNode("p8:hasTemplate");
                    IUriNode iobj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                    insert.Assert(new Triple(isubj, ipred, iobj));
                    isubj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                    ipred = insert.CreateUriNode("p8:hasRole");
                    iobj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                    insert.Assert(new Triple(isubj, ipred, iobj));
                  }
                  else
                  {
                    isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                    ipred = insert.CreateUriNode("tpl:R97483568938");
                    ilobj = insert.CreateLiteralNode(Convert.ToString(++roleCount), new Uri("xsd:integer"));
                    insert.Assert(new Triple(isubj, ipred, ilobj));
                  }
                  if (!string.IsNullOrEmpty(role.range))
                  {
                    qn = nsMap.ReduceToQName(role.range, out qName);
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                      isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                      ipred = insert.CreateUriNode("p8:hasRoleFillerType");
                      IUriNode iobj = insert.CreateUriNode(qName);
                      insert.Assert(new Triple(isubj, ipred, iobj));
                    }
                    else
                    {
                      isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                      ipred = insert.CreateUriNode("rdf:type");
                      IUriNode iobj = insert.CreateUriNode("tpl:R74478971040");
                      insert.Assert(new Triple(isubj, ipred, iobj));
                      ipred = insert.CreateUriNode("rdfs:domain");
                      iobj = insert.CreateUriNode(string.Format("tpl:{0}", identifier));
                      insert.Assert(new Triple(isubj, ipred, iobj));
                      if (qName.StartsWith("rdfs") || qName.StartsWith("rdf"))
                      {
                        ipred = insert.CreateUriNode("rdf:type");
                        iobj = insert.CreateUriNode("owl:DataTypeProperty");
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                      else
                      {
                        isubj = insert.CreateUriNode(string.Format("tpl:{0}", roleID));
                        ipred = insert.CreateUriNode("rdf:type");
                        iobj = insert.CreateUriNode("owl:ObjectProperty");
                        insert.Assert(new Triple(isubj, ipred, iobj));
                        iobj = insert.CreateUriNode("owl:FunctionalProperty");
                        insert.Assert(new Triple(isubj, ipred, iobj));
                        ipred = insert.CreateUriNode("rdfs:range");
                        iobj = insert.CreateUriNode(qName);
                        insert.Assert(new Triple(isubj, ipred, iobj));
                      }
                    }
                  }
                }
              }
              #endregion

              sparqlBuilder.Append(sparqlDelete);
              if (delete.IsEmpty)
              {
                foreach (Triple t in insert.Triples)
                {
                  sparqlAdd.AppendLine(t.ToString(formatter));
                }
                sparqlAdd.AppendLine("}");
              }
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
                  qn = nsMap.ReduceToQName(existingTemplate.qualifies, out qName);
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
                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", identifier, name.value, language));
                      }
                    }
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                      sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));
                      sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type p8:CoreTemplate .", identifier));
                      sparqlStmts.AppendLine(string.Format("tpl:{0} p8:hasSuperTemplate {1} .", identifier, qName));
                      sparqlStmts.AppendLine(string.Format("{0} p8:hasSubTemplate tpl:{1} .", qName, identifier));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasSuperTemplate {1} .", identifier, qName));
                      sparqlAdd.AppendLine(string.Format("{0} p8:hasSubTemplate tpl:{1} .", qName, identifier));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:CoreTemplate .", identifier));
                    }
                    else
                    {
                      sparqlStmts.AppendLine(string.Format("{0} dm:hasSubclass tpl:{1} .", qName, identifier));
                      sparqlStmts.AppendLine(string.Format("tpl:{0} dm:hasSuperclass {1} .", identifier, qName));
                      sparqlAdd.AppendLine(string.Format("{0} dm:hasSubclass tpl:{1} .", qName, identifier));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} dm:hasSuperclass {1} .", identifier, qName));
                    }
                  }
                  //role count
                  if (existingTemplate.roleQualification.Count != newTemplateQualification.roleQualification.Count)
                  {
                    hasDeletes = true;
                    if (repository.RepositoryType == RepositoryType.Part8)
                    {
                      sparqlStmts.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1} .", identifier, existingTemplate.roleQualification.Count));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valNumberOfRoles {1} .", identifier, newTemplateQualification.roleQualification.Count));
                    }
                    else
                    {
                      sparqlStmts.AppendLine(string.Format("tpl:{0} tpl:R35529169909 {1} .", identifier, existingTemplate.roleQualification.Count));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R35529169909 {1} .", identifier, newTemplateQualification.roleQualification.Count));
                    }
                  }

                  foreach (Specialization spec in newTemplateQualification.specialization)
                  {
                    Specialization existingSpecialization = existingTemplate.specialization.Find(n => n.reference == spec.reference);

                    if (existingSpecialization != null)
                    {
                      string specialization = spec.reference;
                      string existingSpec = existingSpecialization.reference;

                      hasDeletes = true;

                      if (repository.RepositoryType == RepositoryType.Part8)
                      {
                        sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateSpecialization .", existingSpec));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                      }
                      else
                      {
                        sparqlStmts.AppendLine(string.Format("tpl:{0} rdf:type dm:Specialization .", identifier));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type dm:Specialization .", identifier));
                      }
                    }
                  }

                  index = 1;
                  foreach (RoleQualification role in newTemplateQualification.roleQualification)
                  {
                    string roleIdentifier = string.Empty;

                    roleIdentifier = role.identifier;

                    hasDeletes = false;

                    //get existing role if it exists
                    RoleQualification existingRole = existingTemplate.roleQualification.FirstOrDefault(r => r.identifier == roleIdentifier);

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
                            hasDeletes = true;
                            sparqlStmts.AppendLine(string.Format("tpl:{0}  rdfs:label \'{1}\'@{2} .", existingRole.identifier, existingName.value, name.lang));
                          }
                        }
                      }
                      if (existingRole.range != null)
                      {
                        qn = nsMap.ReduceToQName(existingRole.range, out qName);
                        if (string.Compare(existingRole.range, role.range, true) != 0)
                        {
                          hasDeletes = true;
                          if (repository.RepositoryType == RepositoryType.Part8)
                          {
                            sparqlStmts.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", existingRole.identifier, qName));
                          }
                          else
                          {
                            sparqlStmts.AppendLine(string.Format("tpl:{0} rdfs:range {1} .", existingRole.identifier, qName));
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
                      if (repository.RepositoryType == RepositoryType.Part8)
                      {
                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", roleID, roleLabel, language));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} p8:valRoleIndex {1} .", roleID, index));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));

                        if (!string.IsNullOrEmpty(role.range))
                        {
                          qn = nsMap.ReduceToQName(role.range, out qName);
                          sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                        }
                        else if (role.value != null)
                        {
                          if (role.value.reference != null)
                          {
                            qn = nsMap.ReduceToQName(role.value.reference, out qName);
                            sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                          }
                          else if (role.value.text != null)
                          {
                            ///TODO need to determine how to do this one
                          }
                        }

                        sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRole rdl:{1} .", identifier, roleID));
                      }
                      else
                      {
                        if (!string.IsNullOrEmpty(role.range))
                        {
                          qn = nsMap.ReduceToQName(role.range, out qName);
                          sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:range {1} .", roleID, qName));
                          sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R76288246068 .", roleID));
                          sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R99672026745 tpl:{1} .", roleID, identifier));
                          sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R91125890543 tpl:{1} .", roleID, role.qualifies.Split('#')[1]));
                          sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R98983340497 {1} .", roleID, qName));
                        }
                        else if (role.value != null)
                        {
                          if (role.value.reference != null)
                          {
                            qn = nsMap.ReduceToQName(role.value.reference, out qName);
                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R40103148466 .", roleID));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R49267603385 tpl:{1} .", roleID, identifier));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R30741601855 tpl:{1} .", roleID, role.qualifies.Split('#')[1]));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R21129944603 {1} .", roleID, qName));
                          }
                          else if(role.value.text != null)
                          {
                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R67036823327 .", roleID));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R56456315674 tpl:{1} .", roleID, identifier));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R89867215482 tpl:{1} .", roleID,  role.qualifies.Split('#')[1]));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R29577887690 {1} .", roleID, role.value.text));
                          }
                        }
                        else
                        {
                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R74478971040 .", roleID));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:domain tpl:{1} .", roleID, identifier));
                            sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R97483568938 {1} .", roleID, ++roleCount));
                        }
                       
                      }
                      #endregion
                    }

                    index++;
                  }
                }

                //sparqlDelete.Append(prefix);
                sparqlDelete.AppendLine(deleteData);
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
                string label = string.Empty;
                string templateLabel = String.Empty;
                string labelSparql = String.Empty;
                //form labels
                foreach (QMXFName name in newTemplateQualification.name)
                {
                  templateLabel = name.value.Split('@')[0];

                  if (string.IsNullOrEmpty(name.lang))
                    language = "@" + defaultLanguage;
                  else
                    language = "@" + name.lang;
                }

                sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:label \'{1}\'{2} .", identifier, templateLabel, language));
                foreach (Description desc in newTemplateQualification.description)
                {
                  label = string.Empty;
                  if (string.IsNullOrEmpty(desc.value)) continue;
                  label = desc.value.Split('@')[0];

                  if (string.IsNullOrEmpty(desc.lang))
                    language = "@" + defaultLanguage;
                  else
                    language = "@" + desc.lang;

                  sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:comment \'{1}\'{2} .", identifier, label, language));
                }

                if (repository.RepositoryType == RepositoryType.Part8)
                {

                  if (!string.IsNullOrEmpty(newTemplateQualification.qualifies))
                  {
                    qn = nsMap.ReduceToQName(newTemplateQualification.qualifies, out qName);
                    sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasSuperTemplate {1} .", identifier, qName));
                    sparqlAdd.AppendLine(string.Format("{0} p8:hasSubTemplate tpl:{1} .", qName, identifier));
                  }
                  sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));
                  sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:CoreTemplate .", identifier));
                  //sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:hasTemplate p8:{1} .", identifier, templateLabel));
                }
                else
                {
                  if (!string.IsNullOrEmpty(newTemplateQualification.qualifies))
                  {
                    qn = nsMap.ReduceToQName(newTemplateQualification.qualifies, out qName);
                    sparqlAdd.AppendLine(string.Format("{0} dm:hasSubclass tpl:{1} .", qName, identifier));
                    sparqlAdd.AppendLine(string.Format("tpl:{0} dm:hasSuperclass {1} .", identifier, qName));
                  }
                }
                foreach (Specialization spec in newTemplateQualification.specialization)
                {
                  //sparqlStr = new StringBuilder();
                  string specialization = spec.reference;
                  sparqlAdd.Append(prefix);
                  sparqlAdd.AppendLine(insertData);

                  ///TODO: Generate an id for template specialization??

                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                  }
                  else
                  {
                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type dm:Specialization .", identifier));
                  }
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

                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
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
                    else if (role.value != null)
                    {
                      if (role.value.reference != null)
                      {
                        qn = nsMap.ReduceToQName(role.value.reference, out qName);
                        sparqlAdd.AppendLine(string.Format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                      }
                      else if (role.value.text != null)
                      {
                        ///TODO
                      }
                    }
                  }
                  else
                  {

                    if (!string.IsNullOrEmpty(role.range)) //range restriction
                    {
                      qn = nsMap.ReduceToQName(role.range, out qName);
                      sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:range {1} .", roleID, qName));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R76288246068 .", roleID));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R99672026745 tpl:{1} .", roleID, identifier));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R91125890543 tpl:{1} .", roleID, role.qualifies.Split('#')[1]));
                      sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R98983340497 {1} .", roleID, qName));
                    }
                    else if (role.value != null)
                    {
                      if (role.value.reference != null) //reference restriction
                      {
                        qn = nsMap.ReduceToQName(role.value.reference, out qName);
                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R40103148466 .", roleID));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R49267603385 tpl:{1} .", roleID, identifier));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R30741601855 tpl:{1} .", roleID, role.qualifies.Split('#')[1]));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R21129944603 {1} .", roleID, qName));
                      }
                      else if (role.value.text != null)// value restriction
                      {
                        sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R67036823327 .", roleID));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R56456315674 tpl:{1} .", roleID, identifier));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R89867215482 tpl:{1} .", roleID, role.qualifies.Split('#')[1]));
                        sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R29577887690 {1} .", roleID, role.value.text));
                      }
                    }
                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdf:type tpl:R74478971040 .", roleID));
                    sparqlAdd.AppendLine(string.Format("tpl:{0} rdfs:domain tpl:{1} .", roleID, identifier));
                    sparqlAdd.AppendLine(string.Format("tpl:{0} tpl:R97483568938 {1} .", roleID, ++roleCount));

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

    public Response PostClass(QMXF qmxf)
    {

      Graph delete = new Graph();
      Graph insert = new Graph();
      //add namespaces to graphs 
      delete.NamespaceMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("tpl", new Uri("http://tpl.rdlfacade.org/data#"));
      delete.NamespaceMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
      delete.NamespaceMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));  
      delete.NamespaceMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
      insert.NamespaceMap.Import(delete.NamespaceMap);
      NTriplesFormatter formatter = new NTriplesFormatter();
      
      Triple triple = null;

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
                      IUriNode dsubj = delete.CreateUriNode(string.Format("rdl:{0}", clsId));
                      IUriNode dpred = delete.CreateUriNode("rdfs:label");
                      ILiteralNode dobj = delete.CreateLiteralNode(existingName.value, existingName.lang);
                      triple = new Triple(dsubj, dpred, dobj);
                      delete.Assert(triple);
                      IUriNode isub = insert.CreateUriNode(string.Format("rdl:{0}", clsId));
                      IUriNode ipred = insert.CreateUriNode("rdfs:label");
                      ILiteralNode iobj = insert.CreateLiteralNode(clsName.value, clsName.lang);
                      triple = new Triple(isub, ipred, iobj);
                      insert.Assert(triple);
                    }
                  }

                  foreach (Description description in clsDef.description)
                  {
                    Description existingDescription = existingClsDef.description.Find(d => d.lang == description.lang);

                    if (existingDescription != null)
                    {
                      if (String.Compare(existingDescription.value, description.value, true) != 0)
                      {
                        IUriNode dsubj = delete.CreateUriNode(string.Format("rdl:{0}", clsId));
                        IUriNode dpred = delete.CreateUriNode("rdfs:comment");
                        ILiteralNode dobj = delete.CreateLiteralNode(existingDescription.value, existingDescription.lang);
                        triple = new Triple(dsubj, dpred, dobj);
                        delete.Assert(triple);
                        IUriNode isubj = insert.CreateUriNode(string.Format("rdl:{0}", clsId));
                        IUriNode ipred = insert.CreateUriNode("rdfs:comment");
                        ILiteralNode iobj = insert.CreateLiteralNode(description.value, description.lang);
                        triple = new Triple(isubj, ipred, iobj);
                        insert.Assert(triple);
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
                        qn = nsMap.ReduceToQName(existingSpec.reference, out qName);
                        if (qn)
                        {
                          IUriNode dsubj = delete.CreateUriNode(string.Format("rdl:{0}", clsId));
                          IUriNode dpred = delete.CreateUriNode("dm:hasSubclass");
                          IUriNode dobj = delete.CreateUriNode(qName);
                          triple = new Triple(dsubj, dpred, dobj);
                          delete.Assert(triple);
                        }
                        qn = nsMap.ReduceToQName(spec.reference, out qName);
                        if (qn)
                        {
                          IUriNode isubj = insert.CreateUriNode(string.Format("rdl:{0}", clsId));
                          IUriNode ipred = insert.CreateUriNode("rdfs:subClassOf");
                          IUriNode iobj = insert.CreateUriNode(qName);
                          triple = new Triple(isubj, ipred, iobj);
                          insert.Assert(triple);
                        }
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
                        qn = nsMap.ReduceToQName(existingClasif.reference, out qName);
                        if (qn)
                        {
                          IUriNode dsubj = delete.CreateUriNode(string.Format("rdl:{0}", clsId));
                          IUriNode dpred = delete.CreateUriNode("dm:hasClassified");
                          IUriNode dobj = delete.CreateUriNode(qName);
                          triple = new Triple(dsubj, dpred, dobj);
                          delete.Assert(triple);
                          dpred = delete.CreateUriNode("dm:hasClassifier");
                          triple = new Triple(dsubj, dpred, dobj);
                          delete.Assert(triple);
                        }
                        qn = nsMap.ReduceToQName(clsif.reference, out qName);
                        if (qn)
                        {
                          IUriNode isubj = insert.CreateUriNode(string.Format("rdl:{0}", clsId));
                          IUriNode ipred = insert.CreateUriNode("dm:hasClassified");
                          IUriNode iobj = insert.CreateUriNode(qName);
                          triple = new Triple(isubj, ipred, iobj);
                          insert.Assert(triple);
                          ipred = insert.CreateUriNode("dm:hasClassifier");
                          triple = new Triple(isubj, ipred, iobj);
                          insert.Assert(triple);
                        }

                      }
                    }
                  }
                }
              }

              if (!delete.IsEmpty)
              {
                sparqlDelete.AppendLine(deleteData);
                foreach (Triple t in delete.Triples)
                {
                  sparqlDelete.AppendLine(t.ToString(formatter));
                }
                sparqlDelete.AppendLine(" }; ");

                sparqlAdd.AppendLine(insertData);
                foreach (Triple t in insert.Triples)
                {
                  sparqlAdd.AppendLine(t.ToString(formatter));
                }
              }
              else
              {
                response.Level = StatusLevel.Warning;
                response.Messages = new Messages();
                response.Messages.Add("No changes to be done");
                continue;//Nothing to be done
              }
            }

            // add class
            if (!delete.IsEmpty)
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
                IUriNode isubj = insert.CreateUriNode(string.Format("rdl:{0}", clsId));
                IUriNode ipred = insert.CreateUriNode("rdf:type");
                IUriNode iobj = insert.CreateUriNode("owl:Class");
                triple = new Triple(isubj, ipred, iobj);
                insert.Assert(triple);
                ipred = insert.CreateUriNode("rdfs:label");
                ILiteralNode liobj = insert.CreateLiteralNode(clsLabel, language);
                triple = new Triple(isubj, ipred, liobj);
                insert.Assert(triple);
                // append entity type
                if (clsDef.entityType != null && !String.IsNullOrEmpty(clsDef.entityType.reference))
                {
                  qn = nsMap.ReduceToQName(clsDef.entityType.reference, out qName);

                  if (qn)
                  {
                    iobj = insert.CreateUriNode(qName);
                    triple = new Triple(isubj, ipred, iobj);
                    insert.Assert(triple);
                  }
                }
                // append description
                foreach (Description desc in clsDef.description)
                {
                  if (!String.IsNullOrEmpty(desc.value))
                  {
                    if (string.IsNullOrEmpty(desc.lang))
                      language =  defaultLanguage;
                    else
                      language = desc.lang;
                    string description = desc.value.Split('@')[0];
                    ipred = insert.CreateUriNode("rdfs:comment");
                    liobj = insert.CreateLiteralNode(desc.value, language);
                    triple = new Triple(isubj, ipred, liobj);
                    insert.Assert(triple);
                  }
                }

                // append specialization
                foreach (Specialization spec in clsDef.specialization)
                {
                  if (!String.IsNullOrEmpty(spec.reference))
                  {
                    qn = nsMap.ReduceToQName(spec.reference, out qName);
                    if (qn)
                    {
                      ipred = insert.CreateUriNode("rdfs:subClassOf");
                      iobj = insert.CreateUriNode(qName);
                      triple = new Triple(isubj, ipred, iobj);
                      insert.Assert(triple);
                    }
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
                      ipred = insert.CreateUriNode("tdf:type");
                      iobj = insert.CreateUriNode(qName);
                      triple = new Triple(isubj, ipred, iobj);
                      insert.Assert(triple);
                    }
                    else if (repository.RepositoryType != RepositoryType.Part8 && qn)
                    {
                      ipred = insert.CreateUriNode("dm:hasClassifier");
                      triple = new Triple(isubj, ipred, iobj);
                      insert.Assert(triple);
                      isubj = insert.CreateUriNode(qName);
                      ipred = insert.CreateUriNode("dm:hasClassified");
                      iobj = insert.CreateUriNode(string.Format("rdl:{0}", clsId));
                      triple = new Triple(isubj, ipred, iobj);
                      insert.Assert(triple);
                    }
                  }
                }
              }
            sparqlBuilder.Append(sparqlDelete);
            sparqlBuilder.Append(sparqlAdd);
            if (delete.IsEmpty)
            {
              foreach (Triple t in insert.Triples)
              {
                sparqlBuilder.AppendLine(t.ToString(formatter));
              }
              sparqlBuilder.AppendLine("}");
            }

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