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
using System.IO;
using System.Linq;
using VDS.RDF.Parsing;
using log4net;
using Ninject;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;
using System.Net;
using VDS.RDF.Update.Commands;
using VDS.RDF.Update.Protocol;
using org.iringtools.refdata.federation;


namespace org.iringtools.refdata
{
  public class ReferenceDataProvider
  {
    private static readonly ILog Logger = LogManager.GetLogger(typeof(ReferenceDataProvider));
    private readonly Response _response = null;
    private const string FederationFileName = "Federation.xml";
    private const string QueriesFileName = "Queries.xml";
    private NamespaceMapper nsMap = new NamespaceMapper();
    NTriplesFormatter formatter = new NTriplesFormatter();
    INode _subj, _pred, _obj;
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
        _pageSize = Convert.ToInt32(_settings["PageSize"]);
        _useExampleRegistryBase = Convert.ToBoolean(_settings["UseExampleRegistryBase"]);
        var queriesPath = _settings["AppDataPath"] + QueriesFileName;
        _queries = Utility.Read<Queries>(queriesPath);
        var federationPath = _settings["AppDataPath"] + FederationFileName;
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
        Logger.Error("Error in initializing ReferenceDataServiceProvider: " + ex);
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
        var repositories = _repositories;
        //Don't Expose Tokens
        foreach (var repository in repositories)
        {
          repository.EncryptedCredentials = null;
        }
        return repositories;
      }
      catch (Exception ex)
      {
        Logger.Error("Error in GetRepositories: " + ex);
        return null;
      }
    }

    private Repository GetRepository(string name)
    {
      return _repositories.Find(c => c.Name == name);
    }

    public RefDataEntities Search(string query)
    {
      try
      {
        return SearchPage(query, 0, 0);
      }
      catch (Exception ex)
      {
        Logger.Error("Error in Search: " + ex);
        throw new Exception("Error while Searching " + query + ".\n" + ex, ex);
      }
    }

    public RefDataEntities SearchPage(string query, int start, int limit)
    {
      Logger.Debug("SearchPage");

      var counter = 0;
      try
      {
        RefDataEntities entities = null;
        if (_searchHistory.ContainsKey(query))
        {
          Logger.Debug("SearchPage: Using History");

          entities = _searchHistory[query];
        }
        else
        {
          var resultEntities = new RefDataEntities();
          Logger.Debug("SearchPage: Preparing Queries");
          var firstOrDefault = _queries.FirstOrDefault(c => c.Key == "ContainsSearch");
          if (firstOrDefault != null)
          {
            var queryContainsSearch = firstOrDefault.Query;
            var queryItem = _queries.FirstOrDefault(c => c.Key == "ContainsSearchJORD");
            if (queryItem != null)
            {
              var queryContainsSearchJORD = queryItem.Query;
              foreach (var repository in _repositories)
              {
                string sparql;
                if (repository.RepositoryType == RepositoryType.JORD)
                {
                  sparql = ReadSparql(queryContainsSearchJORD.FileName);
                  sparql = sparql.Replace("param1", query);
                }
                else
                {
                  sparql = ReadSparql(queryContainsSearch.FileName);
                  sparql = sparql.Replace("param1", query);
                }
                var sparqlResults = QueryFromRepository(repository, sparql);
                foreach (var result in sparqlResults)
                {
                  var resultEntity = new Entity();
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
              }
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

        Logger.Info(string.Format("SearchPage is returning {0} records", entities.Entities.Count));
        return entities;
      }
      catch (Exception e)
      {
        Logger.Error("Error in SearchPage: " + e);
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
      var labelEntity = new Entity();
      try
      {
        var label = String.Empty;
        var sparql = String.Empty;
        var relativeUri = String.Empty;
        var query = _queries.FirstOrDefault(c => c.Key == "GetLabel").Query;
        var queryEquivalent = _queries.FirstOrDefault(c => c.Key == "GetLabelRdlEquivalent").Query;
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
            foreach (var v in result.Variables.Where(v => (INode) result[v] is LiteralNode && v.Equals("label")))
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
        Logger.Error("Error in GetLabel: " + e);
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
              getClassification = _queries.FirstOrDefault(c => c.Key == "GetClassification").Query;
              sparql = ReadSparql(getClassification.FileName).Replace("param1", id);
              classifications = ProcessClassifications(rep, sparql);
              break;
            case RepositoryType.JORD:
              getClassification = _queries.FirstOrDefault(c => c.Key == "GetClassificationJORD").Query;
              sparql = ReadSparql(getClassification.FileName).Replace("param1", id);
              classifications = ProcessClassifications(rep, sparql);
              break;
            case RepositoryType.Part8:
              getClassification = _queries.FirstOrDefault(c => c.Key == "GetPart8Classification").Query;
              sparql = ReadSparql(getClassification.FileName).Replace("param1", id);
              classifications = ProcessClassifications(rep, sparql);
              break;
          }
        }
        return classifications;
      }
      catch (Exception e)
      {
        Logger.Error("Error in GetClassifications: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<Classification> ProcessClassifications(Repository repository, string sparql)
    {
      var sparqlResults = QueryFromRepository(repository, sparql);
      var classifications = new List<Classification>();
      var names = new List<string>();
      var resultValue = string.Empty;
      foreach (var result in sparqlResults)
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
        foreach (var node in from v in result.Variables let node = result[v] where node is LiteralNode && v.Equals("label") select node)
        {
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
        var queryRdsWip = _queries.FirstOrDefault(c => c.Key == "GetSuperclass").Query;
        var queryJord = _queries.FirstOrDefault(c => c.Key == "GetSuperclassJORD").Query;
        var queryPart8 = _queries.FirstOrDefault(c => c.Key == "GetSuperClassOf").Query;
        foreach (var repository in _repositories)
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
        Logger.Error("Error in GetSpecializations: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    public Entity GetClassLabel(string id)
    {
      int number;
      var isNumber = int.TryParse(id.Substring(1, 1), out number);
      return isNumber ? GetLabel(this._namespaces.Find(ns => ns.Prefix == "rdl").Uri + id) : GetLabel(this._namespaces.Find(ns => ns.Prefix == "jordrdl").Uri + id);
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
        var classQuery = _queries.FirstOrDefault(c => c.Key == "GetClass").Query;
        var classQueryJord = _queries.FirstOrDefault(c => c.Key == "GetClassJORD").Query;

        
        namespaceUrl = _namespaces.Find(n => n.Prefix == "rdl").Uri;
        if (!id.StartsWith(namespaceUrl))
          uri = namespaceUrl + id;
        else
          uri = id;
        foreach (var repository in _repositories)
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
            classDefinition = new ClassDefinition {identifier = uri, repositoryName = repository.Name};
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
                var typeName = ((UriNode)node).Uri.ToString();
                var pref = nsMap.GetPrefix(new Uri(typeName.Substring(0, typeName.IndexOf("#") + 1)));
                if (pref.Contains("dm"))
                  classDefinition.entityType = new EntityType { reference = typeName };
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
        Logger.Error("Error in GetClass: " + e);
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
          var label = specialization.label;
          if (label == null)
            label = GetLabel(uri).Label;
          names = label.Split('@').ToList();
          language = names.Count == 1 ? defaultLanguage : names[names.Count - 1];

          var resultEntity = new Entity
          {
            Uri = uri,
            Label = names[0],
            Lang = language
          };
          Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending()); ;
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error in GetSuperClasses: " + e);
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
        var specializations = GetSpecializations(id, null);
        if (specializations.Count == 0)
        {
          return list;
        }

        foreach (var specialization in specializations)
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
          foreach (var entt in list.Where(entt => resultEntity.Uri.Equals(entt.Uri)))
          {
            found = true;
          }
          if (found) continue;
          trimmedUri = uri.Remove(0, uri.LastIndexOf('#') + 1);
          Utility.SearchAndInsert(list, resultEntity, Entity.sortAscending());
          GetAllSuperClasses(trimmedUri, list);
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error in GetAllSuperClasses: " + e);
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
        var getMembers = _queries.FirstOrDefault(c => c.Key == "GetMembers").Query;
        sparql = ReadSparql(getMembers.FileName);
        var getMembersP8 = _queries.FirstOrDefault(c => c.Key == "GetMembersPart8").Query;
        var getMembersJORD = _queries.FirstOrDefault(c => c.Key == "GetMembersJORD").Query;

        foreach (var repository in _repositories)
        {
          if (repository.Name != repo.Name) continue;
          switch (repository.RepositoryType)
          {
            case RepositoryType.Part8:
              sparql = ReadSparql(getMembersP8.FileName).Replace("param1", Id);
              break;
            case RepositoryType.JORD:
              sparql = ReadSparql(getMembersJORD.FileName).Replace("param1", Id);
              break;
            default:
              sparql = ReadSparql(getMembers.FileName).Replace("param1", Id);
              break;
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
        Logger.Error("Error in Getmembers: " + ex);
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
        var queryGetSubClasses = _queries.FirstOrDefault(c => c.Key == "GetSubClasses").Query;
        var queryGetSubClassesJORD = _queries.FirstOrDefault(c => c.Key == "GetSubClassesJORD").Query;
        var queryGetSubClassesP8 = _queries.FirstOrDefault(c => c.Key == "GetSubClassOf").Query;

        foreach (var repository in _repositories)
        {
          if (repository.Name != repo.Name) continue;

          if (repository.RepositoryType == RepositoryType.Part8)
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
          else if (repository.RepositoryType == RepositoryType.JORD)
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
          else
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
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error in GetSubClasses: " + e);
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
        var queryGetSubClasses = _queries.FirstOrDefault(c => c.Key == "GetSubClassesCount").Query;
        var queryGetSubClassesJord = _queries.FirstOrDefault(c => c.Key == "GetSubClassesCountJORD").Query;
        sparqlJord = ReadSparql(queryGetSubClassesJord.FileName).Replace("param1", id);
        sparql = ReadSparql(queryGetSubClasses.FileName).Replace("param1", id);
        var queryGetSubClassOfInverse = _queries.FirstOrDefault(c => c.Key == "GetSubClassOfCount").Query;
        sparqlPart8 = ReadSparql(queryGetSubClassOfInverse.FileName).Replace("param1", id);

        var count = 0;
        SparqlResultSet sparqlResults = null;
        foreach (var repository in _repositories)
        {
          switch (repository.RepositoryType)
          {
            case RepositoryType.Part8:
              {
                sparqlResults = QueryFromRepository(repository, sparqlPart8);
                count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode) result[v]).Value)));
              }
              break;
            case RepositoryType.JORD:
              {
                sparqlResults = QueryFromRepository(repository, sparqlJord);
                count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode) result[v]).Value)));
              }
              break;
            default:
              {
                sparqlResults = QueryFromRepository(repository, sparql);
                count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode) result[v]).Value)));
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
        Logger.Error("Error in GetSubClasses: " + e);
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
        var getEntities = _queries.FirstOrDefault(c => c.Key.Equals("GetEntityTypes")).Query;
        sparql = ReadSparql(getEntities.FileName);
        foreach (var rep in _repositories)
        {
          if (!rep.Name.Equals("EntityTypes")) continue;
          var sparqlResults = QueryFromRepository(rep, sparql);
          foreach (var result in sparqlResults)
          {
            var entity = new Entity();
            foreach (var e in from v in result.Variables where ((UriNode) result[v]).Uri != null select ((UriNode)result[v]).Uri)
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
        Logger.Error("Error in GetSubClasses: " + e);
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
        var queryGetClassTemplates = _queries.FirstOrDefault(c => c.Key == "GetClassTemplates").Query;
        sparqlGetClassTemplates = ReadSparql(queryGetClassTemplates.FileName);
        sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);
        var queryGetRelatedTemplates = _queries.FirstOrDefault(c => c.Key == "GetRelatedTemplates").Query;

        sparqlGetRelatedTemplates = ReadSparql(queryGetRelatedTemplates.FileName);
        sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);

        foreach (var repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            var sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);

            foreach (var result in sparqlResults.Results)
            {
              resultEntity = new Entity();
              foreach (var v in result.Variables)
              {
                if ((INode)result[v] is LiteralNode && v.Equals("label"))
                {
                  resultEntity.Label = ((LiteralNode)result[v]).Value;
                  resultEntity.Lang = string.IsNullOrEmpty(((LiteralNode) result[v]).Language)
                                        ? defaultLanguage
                                        : ((LiteralNode) result[v]).Language;
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
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error in GetClassTemplates: " + e);
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

        var queryGetClassTemplates = _queries.FirstOrDefault(c => c.Key == "GetClassTemplatesCount").Query;
        sparqlGetClassTemplates = ReadSparql(queryGetClassTemplates.FileName);
        sparqlGetClassTemplates = sparqlGetClassTemplates.Replace("param1", id);
        var queryGetRelatedTemplates = _queries.FirstOrDefault(c => c.Key == "GetRelatedTemplatesCount").Query;
        sparqlGetRelatedTemplates = ReadSparql(queryGetRelatedTemplates.FileName);
        sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.Replace("param1", id);
        var count = 0;
        foreach (var repository in _repositories)
        {
          if (repository.RepositoryType == RepositoryType.Part8)
          {
            var sparqlResults = QueryFromRepository(repository, sparqlGetRelatedTemplates);
            count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode) result[v]).Value)));
          }
          else
          {
            var sparqlResults = QueryFromRepository(repository, sparqlGetClassTemplates);
            count += sparqlResults.Sum(result => result.Variables.Sum(v => Convert.ToInt32(((LiteralNode) result[v]).Value)));
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
        Logger.Error("Error in GetClassTemplates: " + e);
        throw new Exception("Error while Finding " + id + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }

    private List<RoleDefinition> GetRoleDefinition(string id, Repository repository)
    {
      try
      {
        var qId = string.Empty;
        if (!id.Contains("http:"))
          qId = nsMap.GetNamespaceUri("tpl") + id;
        else
          qId = id;

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
        sparql = ReadSparql(queryContainsSearch.FileName).Replace("param1", qId);
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
        Logger.Error("Error in GetRoleDefinition: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<RoleDefinition> GetRoleDefinition(string id)
    {
      try
      {
        string qId;
        if (!id.Contains("http:"))
          qId = nsMap.GetNamespaceUri("tpl") + id;
        else
          qId = id;
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
          var queryContainsSearch = _queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;

          var sparql = ReadSparql(queryContainsSearch.FileName).Replace("param1", qId);
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
        Logger.Error("Error in GetRoleDefinition: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<RoleQualification> GetRoleQualification(string id, Repository rep)
    {
      try
      {
        string qId;
        if (!id.Contains("http:"))
          qId = nsMap.GetNamespaceUri("tpl") + id;
        else
          qId = id;
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
              var rangeResultEntities = new RefDataEntities();
              var referenceResultEntities = new RefDataEntities();
              var valueResultEntities = new RefDataEntities();
              var getRangeRestriction = _queries.FirstOrDefault(c => c.Key == "GetRangeRestriction").Query;
              var getReferenceRestriction = _queries.FirstOrDefault(c => c.Key == "GetReferenceRestriction").Query;
              var getValueRestriction = _queries.FirstOrDefault(c => c.Key == "GetValueRestriction").Query;
              var rangeSparql = ReadSparql(getRangeRestriction.FileName);
              rangeSparql = rangeSparql.Replace("param1", qId);
              var referenceSparql = ReadSparql(getReferenceRestriction.FileName);
              referenceSparql = referenceSparql.Replace("param1", qId);
              var valueSparql = ReadSparql(getValueRestriction.FileName);
              valueSparql = valueSparql.Replace("param1", qId);

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
                      var entity = GetLabel(uri);
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
              var getPart8Roles = _queries.FirstOrDefault(c => c.Key == "GetPart8Roles").Query;

              var part8RolesSparql = ReadSparql(getPart8Roles.FileName);
              part8RolesSparql = part8RolesSparql.Replace("param1", qId);
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
                  /// TODO
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
        Logger.Error("Error in GetRoleQualification: " + e);
        throw new Exception("Error while Getting Class: " + id + ".\n" + e.ToString(), e);
      }
    }

    private List<TemplateDefinition> GetTemplateDefinition(string id, Repository rep)
    {
      var templateDefinitionList = new List<TemplateDefinition>();
      try
      {
        var sparql = String.Empty;
        var relativeUri = String.Empty;
        var qId = string.Empty;
        Query queryContainsSearch = null;
        var description = new Description();
        var status = new QMXFStatus();

        if (!id.Contains("http:"))
          qId = nsMap.GetNamespaceUri("tpl") + id;
        else
          qId = id;

        foreach (var repository in _repositories)
        {
          if (rep != null)
            if (rep.Name != repository.Name) continue;

          queryContainsSearch = repository.RepositoryType == RepositoryType.Part8 ? _queries.FirstOrDefault(c => c.Key == "GetBaseTemplatePart8").Query : _queries.FirstOrDefault(c => c.Key == "GetTemplate").Query;
          sparql = ReadSparql(queryContainsSearch.FileName);
          sparql = sparql.Replace("param1", qId);
          var sparqlResults = QueryFromRepository(repository, sparql);

          foreach (var result in sparqlResults)
          {
            if (result.Count == 0) continue;
            var templateDefinition = new TemplateDefinition();
            var name = new QMXFName();
            templateDefinition.repositoryName = repository.Name;

            foreach (var v in result.Variables)
            {
              if (result[v] is LiteralNode && v.Equals("label"))
              {
                name.value = ((LiteralNode)result[v]).Value;
                name.lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(name.lang))
                  name.lang = defaultLanguage;
              }
              else if (result[v] is LiteralNode && v.Equals("definition"))
              {
                description.value = ((LiteralNode)result[v]).Value;
                description.lang = ((LiteralNode)result[v]).Language;
                if (string.IsNullOrEmpty(description.lang))
                  description.lang = defaultLanguage;
              }
              else if (result[v] is LiteralNode && v.Equals("creationDate"))
              {
                status.from = ((LiteralNode)result[v]).Value;
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
        Logger.Error("Error in GetTemplateDefinition: " + e);
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
        Logger.Error("Error in GetTemplate: " + ex);
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
        Logger.Error("Error in GetTemplate: " + ex);
      }

      return qmxf;
    }


    private List<TemplateQualification> GetTemplateQualification(string id, Repository rep)
    {
      var templateQualificationList = new List<TemplateQualification>();
      try
      {
        var sparqlQuery = string.Empty;
        string qId;

        if (!id.Contains("http:"))
          qId = nsMap.GetNamespaceUri("tpl") + id;
        else
          qId = id;

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

            var getTemplateQualification = _queries.FirstOrDefault(c => c.Key == sparqlQuery).Query;
            var sparql = ReadSparql(getTemplateQualification.FileName);
            sparql = sparql.Replace("param1", qId);

            var sparqlResults = QueryFromRepository(repository, sparql);

            foreach (var result in sparqlResults.Results)
            {
              var templateQualification = new TemplateQualification();
              var description = new Description();
              var status = new QMXFStatus();
              var name = new QMXFName();

              templateQualification.repositoryName = repository.Name;

              foreach (var v in result.Variables)
              {
                if (result[v] is LiteralNode && v.Equals("name"))
                {
                  name.value = ((LiteralNode)result[v]).Value;
                  name.lang = ((LiteralNode)result[v]).Language;
                  if (string.IsNullOrEmpty(name.lang))
                    name.lang = defaultLanguage;
                }
                else if (result[v] is LiteralNode && v.Equals("description"))
                {
                  description.value = ((LiteralNode)result[v]).Value;
                  description.lang = ((LiteralNode)result[v]).Language;
                  if (string.IsNullOrEmpty(description.lang))
                    description.lang = defaultLanguage;
                }
                else if (result[v] is UriNode && v.Equals("statusClass"))
                {
                  status.Class = ((UriNode)result[v]).Uri.ToString();
                }
                else if (result[v] is UriNode && v.Equals("statusAuthority"))
                {
                  status.authority = ((UriNode)result[v]).Uri.ToString();
                }
                else if (result[v] is UriNode && v.Equals("qualifies"))
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
        Logger.Error("Error in GetTemplateQualification: " + e);
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
        throw;
      }
    }

    /// <summary>
    ///  this will generate an id formatted as R + new Guid replacing '_' with blank '' space
    ///  example = RC2E15CCD8F104DD69188E6A5A23354B1
    /// </summary>
    /// <param name="registryBase"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private string CreateNewGuidId(string registryBase)//, string name)
    {
      if (!string.IsNullOrEmpty(registryBase))
        return string.Format("{0}R{1}", registryBase, Guid.NewGuid().ToString().Replace("_", "").Replace("-", "").ToUpper());
      else
      {
        Logger.Error("Failed to create id:");
        throw new Exception("CreateIdsAdiId: Failed to create id ");

      }

    }

    private List<Dictionary<string, string>> MergeLists(List<Dictionary<string, string>> a, IEnumerable<Dictionary<string, string>> b)
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
        var parser = new SparqlQueryParser();
        var query = parser.ParseFromString(sparql);

        var endpoint = new SparqlRemoteEndpoint(new Uri(repository.Uri)) {Timeout = 600000};
        var encryptedCredentials = repository.EncryptedCredentials;
        var cred = new WebCredentials(encryptedCredentials);
        if (cred.isEncrypted) cred.Decrypt();

        if (!string.IsNullOrEmpty(_settings["ProxyHost"])
          && !string.IsNullOrEmpty(_settings["ProxyPort"])
          && !string.IsNullOrEmpty(_settings["ProxyCredentialToken"])) 
        {
          var pcred = _settings.GetWebProxyCredentials();
          endpoint.Proxy = pcred.GetWebProxy() as WebProxy;
          endpoint.ProxyCredentials = pcred.GetNetworkCredential();
        }
        endpoint.Credentials = cred.GetNetworkCredential();

        var resultSet = endpoint.QueryWithResultSet(query.ToString());
        return resultSet;
      }
      catch (Exception ex)
      {
        Logger.Error(string.Format("Failed to read repository['{0}']", repository.Uri), ex);
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
          && !string.IsNullOrEmpty(_settings["ProxyCredentialToken"]))
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

    private static RefDataEntities GetRequestedPage(RefDataEntities rde, int startIdx, int pageSize)
    {
      try
      {
        var page = new RefDataEntities {Total = rde.Entities.Count};

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

    private static void Reset(string query)
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
          var status = new Status {Level = StatusLevel.Error};
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

        Logger.Error(errMsg);
      }

      return response;
    }

    private Response ProcessTemplateDefinitions(IEnumerable<TemplateDefinition> list, Repository repository)
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
        var templateId = string.Empty;
        string generatedId;
        if (!string.IsNullOrEmpty(newTDef.identifier))
          templateId = newTDef.identifier;

        var templateName = newTDef.name[0].value;
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
              if (System.String.Compare(oldName.value, newName.value, System.StringComparison.Ordinal) == 0)
                continue;
              GenerateTemplateHeader(ref delete, templateId, null, oldTDef);
              GenerateTemplateHeader(ref insert, templateId, null, newTDef);
            }

            var index = 1;
            if (oldTDef.roleDefinition.Count < newTDef.roleDefinition.Count) 
            {
              foreach (var nrd in newTDef.roleDefinition)
              {
                var roleName = nrd.name[0].value;
                var newRoleId = nrd.identifier;
                if (string.IsNullOrEmpty(newRoleId))
                {
                  generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
                  newRoleId = generatedId;
                }
                var ord = oldTDef.roleDefinition.Find(r => r.identifier == newRoleId);
                if (ord == null) 
                {
                  foreach (var name in nrd.name)
                  {
                    GenerateName(ref insert, name, newRoleId, nrd);
                  }
                  if (nrd.description != null)
                  {
                    GenerateDescription(ref insert, nrd.description, newRoleId);
                  }
                  GenerateTypesPart8(ref insert, newRoleId, templateId, nrd);
                  GenerateRoleIndexPart8(ref insert, newRoleId, index, nrd);
                }
                if (nrd.range != null)
                {
                  GenerateRoleFillerType(ref insert, newRoleId, nrd.range);
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
          //Nothing to be done
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
          GenerateTemplateHeader(ref insert, templateId, null, newTDef);
          GenerateTemplateDescription(ref insert, templateId, null, newTDef);

          foreach (var newRole in from newRole in newTDef.roleDefinition let roleLabel = newRole.name.FirstOrDefault().value select newRole)
          {
            string newRoleId;
            generatedId = string.Empty;
            var range = newRole.range;

            if (string.IsNullOrEmpty(newRole.identifier))
            {
              generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
              newRoleId = generatedId;
            }
            else
            {
              newRoleId = newRole.identifier;
            }
            GenerateTemplateRoleDescription(ref insert, newRoleId, newRole, newTDef);

          }
        }

        if (!delete.IsEmpty)
        {
          sparqlBuilder.AppendLine(deleteData);
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



    private Response ProcessTemplateQualifications(IEnumerable<TemplateQualification> list, Repository repository)
    {
      var response = new Response();
      var delete = new Graph();
      var insert = new Graph();
      delete.NamespaceMap.Import(nsMap);
      insert.NamespaceMap.Import(nsMap);

      foreach (var newTq in list)
      {
        var roleCount = 0;
        var templateId = string.Empty;
        var generatedId = string.Empty;
        var roleQualification = string.Empty;
        //int index = 1;
        if (!string.IsNullOrEmpty(newTq.identifier))
          templateId = newTq.identifier;

        string templateName = newTq.name[0].value;
        var oldQmxf = new QMXF();
        if (!String.IsNullOrEmpty(templateId))
        {
          oldQmxf = GetTemplate(templateId, QMXFType.Qualification, repository);
        }
        else
        {
          generatedId = CreateNewGuidId(_useExampleRegistryBase ? _settings["ExampleRegistryBase"] : _settings["TemplateRegistryBase"]);
          templateId = generatedId;
        }

        if (oldQmxf.templateQualifications.Count > 0)
        {
          foreach (var oldTq in oldQmxf.templateQualifications)
          {
            foreach (var nn in newTq.name)
            {
              templateName = nn.value;
              var on = oldTq.name.Find(n => n.lang == nn.lang);
              if (on != null)
              {
                if (System.String.Compare(@on.value, nn.value, System.StringComparison.OrdinalIgnoreCase) != 0)
                {
                  GenerateName(ref delete, on, templateId, oldTq);
                  GenerateName(ref insert, nn, templateId, newTq);
                }
              }
            }
            foreach (var nd in newTq.description)
            {
              if (nd.lang == null) nd.lang = defaultLanguage;
              Description od = null;
              od = oldTq.description.Find(d => d.lang == nd.lang);

              if (od != null && od.value != null)
              {
                if (System.String.Compare(od.value, nd.value, System.StringComparison.OrdinalIgnoreCase) != 0)
                {
                  GenerateDescription(ref delete, od, templateId);
                  GenerateDescription(ref insert, nd, templateId);
                }
              }
              else if (od == null && nd.value != null)
              {
                GenerateDescription(ref insert, nd, templateId);
              }
            }
            if (oldTq.roleQualification.Count != newTq.roleQualification.Count)
            {
              GenerateRoleCountPart8(ref delete, oldTq.roleQualification.Count, templateId, oldTq);
              GenerateRoleCountPart8(ref insert, newTq.roleQualification.Count, templateId, newTq);
            }

            foreach (var ns in newTq.specialization)
            {
              var os = oldTq.specialization.FirstOrDefault();

              if (os != null && os.reference != ns.reference)
              {
                //TODO
              }
            }
            if (oldTq.roleQualification.Count < newTq.roleQualification.Count)
            {
              int count = 0;
              foreach (var nrq in newTq.roleQualification)
              {
                var roleName = nrq.name[0].value;
                var newRoleId = nrq.identifier;

                if (string.IsNullOrEmpty(newRoleId))
                {
                  if (_useExampleRegistryBase)
                    generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);//, roleName);
                  else
                    generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);//, roleName);
                  newRoleId = generatedId;
                }
                var orq = oldTq.roleQualification.Find(r => r.identifier == newRoleId);
                if (orq == null)
                {

                  GenerateTypesPart8(ref insert, newRoleId, templateId, nrq);
                  foreach (var nn in nrq.name)
                  {
                    GenerateName(ref insert, nn, newRoleId, nrq);
                  }
                  GenerateRoleIndexPart8(ref insert, newRoleId, ++count, nrq);
                  GenerateHasTemplate(ref insert, newRoleId, templateId, nrq);
                  GenerateHasRole(ref insert, templateId, newRoleId, newTq);
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
            }
            else if (oldTq.roleQualification.Count > newTq.roleQualification.Count)
            {
              var count = 0;
              foreach (var orq in oldTq.roleQualification)
              {
                var roleName = orq.name[0].value;
                var nmespace = nsMap.GetNamespaceUri("tpl");
                var newRoleID = orq.identifier;
                if (!newRoleID.StartsWith("http://"))
                  newRoleID = nmespace + orq.identifier;

                if (string.IsNullOrEmpty(newRoleID))
                {
                  if (_useExampleRegistryBase)
                    generatedId = CreateNewGuidId(_settings["ExampleRegistryBase"]);
                  else
                    generatedId = CreateNewGuidId(_settings["TemplateRegistryBase"]);
                  newRoleID = generatedId;
                }
                var nrq = newTq.roleQualification.Find(r => r.identifier == newRoleID);
                if (nrq != null) continue;
                GenerateTypesPart8(ref delete, newRoleID, templateId, orq);
                foreach (var nn in orq.name)
                {
                  GenerateName(ref delete, nn, newRoleID, orq);
                }
                GenerateRoleIndexPart8(ref delete, newRoleID, ++count, orq);
                GenerateHasTemplate(ref delete, newRoleID, templateId, orq);
                GenerateHasRole(ref delete, templateId, newRoleID, oldTq);
                if (orq.value != null && !string.IsNullOrEmpty(orq.value.reference))
                {
                  GenerateRoleFillerType(ref delete, newRoleID, orq.value.reference);
                }
                else if (orq.range != null)
                {
                  GenerateRoleFillerType(ref delete, newRoleID, orq.range);
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
          GenerateTemplateHeader(ref insert, templateId, null, newTq);
          GenerateTemplateDescription(ref insert, templateId, null, newTq);
          

          foreach (var newRole in newTq.roleQualification)
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
            GenerateTemplateRoleDescription(ref insert, roleID, newRole, newTq);
          }
        }

        if (!delete.IsEmpty)
        {
          sparqlBuilder.Append(deleteData);
          foreach (var t in delete.Triples)
          {
            sparqlBuilder.AppendLine(t.ToString(formatter));
          }
          sparqlBuilder.AppendLine(!insert.IsEmpty ? "};" : "}");
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

    private void GenerateHasSuperTemplate(ref Graph work, string templateId, TemplateQualification newTq)
    {
      _subj = work.CreateUriNode(new Uri(templateId));
      _pred = work.CreateUriNode(rdfssubClassOf);
      _obj = work.CreateUriNode("p7tm:TemplateSpecialization");
      work.Assert(new Triple(_subj, _pred, _obj));
      _pred = work.CreateUriNode("p7tm:hasSubTemplate");
      _obj = work.CreateUriNode(new Uri(newTq.identifier));
      work.Assert(new Triple(_subj, _pred, _obj));
      _pred = work.CreateUriNode("p7tm:hasSuperTemplate");
      _obj = work.CreateUriNode(new Uri(newTq.qualifies));
      work.Assert(new Triple(_subj, _pred, _obj));
    }

    public Response PostClass(QMXF qmxf)
    {
      var parser = new SparqlUpdateParser();
      
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
                    if (od != null)
                    {
                      if (System.String.Compare(od.value, nd.value, System.StringComparison.Ordinal) != 0)
                      {
                        GenerateClassDescription(ref delete, od, clsId);
                        GenerateClassDescription(ref insert, nd, clsId);
                      }
                    }
                  }
                  if (newClsDef.specialization.Count == oldClsDef.specialization.Count)
                  {
                    continue;
                  }
                  else if (newClsDef.specialization.Count < oldClsDef.specialization.Count)
                  {
                    foreach (var os in oldClsDef.specialization)
                    {
                      var ns = newClsDef.specialization.Find(s => s.reference == os.reference);
                      if (ns == null)
                      {
                        GenerateRdfSubClass(ref delete, clsId, os.reference);
                      }
                    }
                  }
                  else if (newClsDef.specialization.Count > oldClsDef.specialization.Count)
                  {
                    foreach (var ns in newClsDef.specialization)
                    {
                      var os = oldClsDef.specialization.Find(s => s.reference == ns.reference);
                      if (os == null)
                      {
                        GenerateRdfSubClass(ref insert, clsId, ns.reference);
                      }
                    }
                  }
                  if (newClsDef.classification.Count == oldClsDef.classification.Count)
                  {
                    continue;
                  }
                  else if (newClsDef.classification.Count < oldClsDef.classification.Count)
                  {
                    foreach (var oc in oldClsDef.classification)
                    {
                      var nc = newClsDef.classification.Find(c => c.reference == oc.reference);
                      if (nc == null)
                      {
                        GenerateSuperClass(ref delete, oc.reference, clsId); ///delete from old
                      }
                    }
                  }
                  else if (newClsDef.classification.Count > oldClsDef.classification.Count)//some is added ... find added classifications
                  {
                    foreach (var nc in newClsDef.classification)
                    {
                      var oc = oldClsDef.classification.Find(c => c.reference == nc.reference);
                      if (oc == null)
                      {
                        GenerateSuperClass(ref insert, nc.reference, clsId); ///insert from new
                      }
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
                if (!String.IsNullOrEmpty(ns.reference))
                {
                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    GenerateRdfSubClass(ref insert, clsId, ns.reference);
                  }
                  else
                  {
                    throw new Exception("Repository updates not supported for repository type[" + repository.RepositoryType + "]");
                  }
                }
              }
              foreach (var nd in newClsDef.description)
              {
                if (!String.IsNullOrEmpty(nd.value))
                {
                  GenerateClassDescription(ref insert, nd, clsId);
                }
              }
              foreach (var nn in newClsDef.name)
              {
                GenerateClassName(ref insert, nn, clsId, newClsDef);
              }
              foreach (var nc in newClsDef.classification)
              {
                if (!string.IsNullOrEmpty(nc.reference))
                {
                  if (repository.RepositoryType == RepositoryType.Part8)
                  {
                    GenerateSuperClass(ref insert, nc.reference, clsId);
                  }
                  else
                  {
                    throw new Exception("Repository updates not supported for repository type[" + repository.RepositoryType + "]");
                  }
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
        Logger.Error(errMsg);
      }
      return response;
    }

    public List<Entity> Find(string queryString)
    {
      var queryResult = new List<Entity>();
      try
      {
        var sparql = String.Empty;
        var relativeUri = String.Empty;
        var queryExactSearch = (Query)_queries.FirstOrDefault(c => c.Key == "ExactSearch").Query;
        sparql = ReadSparql(queryExactSearch.FileName);
        sparql = sparql.Replace("param1", queryString);
        foreach (var repository in _repositories)
        {
          var sparqlResults = QueryFromRepository(repository, sparql);

          foreach (var result in sparqlResults)
          {
            var resultEntity = new Entity();
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
        Logger.Error("Error in Find: " + e);
        throw new Exception("Error while Finding " + queryString + ".\n" + e.ToString(), e);
      }
      return queryResult;
    }

    public VersionInfo GetVersion()
    {
      var version = this.GetType().Assembly.GetName().Version;

      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }

    private void GenerateHasRole(ref Graph work, string subjId, string objId, object gobj)
    {
      _subj = work.CreateUriNode(new Uri(subjId));
      _pred = work.CreateUriNode("p7tm:hasRole");
      _obj = work.CreateUriNode(new Uri(objId));
      work.Assert(new Triple(_subj, _pred, _obj));
    }

    private void GenerateHasTemplate(ref Graph work, string subjId, string objId, object gobj)
    {
        _subj = work.CreateUriNode(new Uri(subjId));
        _pred = work.CreateUriNode("p7tm:hasTemplate");
        _obj = work.CreateUriNode(new Uri(objId));
        work.Assert(new Triple(_subj, _pred, _obj));
    }

    private void GenerateRoleIndexPart8(ref Graph work, string subjId, int index, object gobj)
    {
      if (gobj is RoleDefinition || gobj is RoleQualification)
      {
        _subj = work.CreateUriNode(new Uri(subjId));
        _pred = work.CreateUriNode("p7tm:valRoleIndex");
        _obj = work.CreateLiteralNode(index.ToString(), new Uri("xsd:integer"));
        work.Assert(new Triple(_subj, _pred, _obj));
      }
    }

    private void GenerateRoleRestriction(ref Graph work, object role, object template)
    {
      var roleId = string.Empty;
      var templateId = string.Empty;
      var roleFillerType = string.Empty;
      if(role is RoleDefinition)
      {
        var rl = (RoleDefinition)role;
        roleId = rl.identifier;
        roleFillerType = rl.range;
      }
      else
      {
        var rl = (RoleQualification)role;
        roleId = rl.identifier;
        if (rl.value != null && rl.value.reference != null)
        {
          roleFillerType = rl.value.reference;
        }
        else if(rl.range != null)
        {
          roleFillerType = rl.range;
        }
      }
      if(template is TemplateDefinition)
      {
        var tp = (TemplateDefinition)template;
        templateId = tp.identifier;
      }
      else
      {
        var tp = (TemplateQualification)template;
        templateId = tp.identifier;
      }
      var ns = nsMap.GetPrefix(new Uri(roleFillerType.Substring(0, roleFillerType.IndexOf("#") + 1)));
      var bNode = work.CreateBlankNode();
      _subj = work.CreateUriNode(new Uri(templateId));
      _pred = work.CreateUriNode(rdfssubClassOf);
      _obj = bNode;
      work.Assert(new Triple(_subj, _pred, _obj));
      _subj = bNode;
      _pred = work.CreateUriNode(rdfType);
      _obj = work.CreateUriNode("owl:Restriction");
      work.Assert(new Triple(_subj, _pred, _obj));
      _pred = work.CreateUriNode("owl:onProperty");
      _obj = work.CreateUriNode(new Uri(roleId));
      work.Assert(new Triple(_subj, _pred, _obj));
      _pred = work.CreateUriNode("owl:someValuesFrom");
      _obj = work.CreateUriNode(new Uri(roleFillerType));
      work.Assert(new Triple(_subj, _pred, _obj));
    }

    private void GenerateRoleFillerType(ref Graph work, string subjId, object role)
    {
      var roleFillerType = string.Empty;
      if(role is RoleDefinition)
      {
         roleFillerType = ((RoleDefinition) role).range;
      }
      else
      {
        var rq = (RoleQualification) role;
        if(rq.value != null && rq.value.reference != null)
        {
          roleFillerType = rq.value.reference;
          //need something more here to identify value reference
        }
        else if (rq.range != null)
        {
          roleFillerType = rq.range;
        }
      }
      _subj = work.CreateUriNode(new Uri(subjId));
      _pred = work.CreateUriNode("p7tm:hasRoleFillerType");
      _obj = work.CreateUriNode(new Uri(roleFillerType));
      work.Assert(new Triple(_subj, _pred, _obj));
    }

    private void GenerateRoleCountPart8(ref Graph work, int rolecount, string subjId, object gobj)
    {
      if (!(gobj is TemplateDefinition) && !(gobj is TemplateQualification)) return;
      _subj = work.CreateUriNode(new Uri(subjId));
      _pred = work.CreateUriNode("p7tm:valNumberOfRoles");
      _obj = work.CreateLiteralNode(Convert.ToString(rolecount), new Uri("xsd:integer"));
      work.Assert(new Triple(_subj, _pred, _obj));
    }

    private void GenerateTypesPart8(ref Graph work, string subjId, string objectId, object gobj)
    {
      if (gobj is RoleQualification)
      {
        _subj = work.CreateUriNode(new Uri(subjId));
        _pred = work.CreateUriNode(rdfType);
        _obj = work.CreateUriNode("owl:Thing");
        work.Assert(new Triple(_subj, _pred, _obj));
        _obj = work.CreateUriNode("p7tm:TemplateRoleDescription");
        work.Assert(new Triple(_subj, _pred, _obj));

      }
      else if (gobj is RoleDefinition)
      {
        _subj = work.CreateUriNode(new Uri(subjId));
        _pred = work.CreateUriNode(rdfType);
        _obj = work.CreateUriNode("owl:Thing");
        work.Assert(new Triple(_subj, _pred, _obj));
        _obj = work.CreateUriNode("p7tm:TemplateRoleDescription");
        work.Assert(new Triple(_subj, _pred, _obj));
      }
      else if (gobj is ClassDefinition)
      {
        _subj = work.CreateUriNode(new Uri(subjId));
        _pred = work.CreateUriNode(rdfType);
        _obj = work.CreateUriNode(new Uri(objectId));
        work.Assert(new Triple(_subj, _pred, _obj));
        _pred = work.CreateUriNode(rdfType);
        _obj = work.CreateUriNode("owl:Class");
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

    private void GenerateTemplateRoleDescription(ref Graph work, string subjId, object role, object template)
    {
      var tmpDescrId = CreateNewGuidId("http://tpl.rdlfacade.org/data#");
      if(role is RoleDefinition)
      {
        var rd = (RoleDefinition)role;
        var td = (TemplateDefinition)template;

        GenerateRoleProperty(ref work, rd, td);

        var idx = td.roleDefinition.FindIndex(t => t.identifier.Equals(rd.identifier))+1;
        _subj = work.CreateUriNode(new Uri(tmpDescrId));
        _pred = work.CreateUriNode(rdfType);
        _obj = work.CreateUriNode("owl:Thing");
        work.Assert(new Triple(_subj, _pred, _obj));
        _obj = work.CreateUriNode("p7tm:TemplateRoleDescription");
        work.Assert(new Triple(_subj, _pred, _obj));
        _pred = work.CreateUriNode("rdfs:label");
        _obj = work.CreateLiteralNode(rd.name[0].value, rd.name[0].lang);
        work.Assert(new Triple(_subj, _pred, _obj));
        GenerateRoleIndexPart8(ref work, _subj.ToString(), idx, role);
        GenerateHasTemplate(ref work, _subj.ToString(), td.identifier, role);
        GenerateHasRole(ref work, _subj.ToString(), rd.identifier, null);
        GenerateRoleFillerType(ref work, _subj.ToString(), rd);
        GenerateRoleRestriction(ref work, rd, td);
      }
      else if(role is RoleQualification)
      {
        var rq = (RoleQualification)role;
        var tq = (TemplateQualification)template;
        var idx = tq.roleQualification.FindIndex(t => t.identifier.Equals(rq.identifier))+1;
        _subj = work.CreateUriNode(new Uri(tmpDescrId));
        _pred = work.CreateUriNode(rdfType);
        _obj = work.CreateUriNode("owl:Thing");
        work.Assert(new Triple(_subj, _pred, _obj));
        _obj = work.CreateUriNode("p7tm:TemplateRoleDescription");
        work.Assert(new Triple(_subj, _pred, _obj));
        _pred = work.CreateUriNode("rdfs:label");
        _obj = work.CreateLiteralNode(rq.name[0].value, rq.name[0].lang);
        work.Assert(new Triple(_subj, _pred, _obj));
        GenerateRoleIndexPart8(ref work, _subj.ToString(), idx, role);
        GenerateHasTemplate(ref work, _subj.ToString(), tq.identifier, role);
        GenerateHasRole(ref work, _subj.ToString(), rq.qualifies, null);
        GenerateRoleFillerType(ref work, _subj.ToString(), rq);
        GenerateRoleRestriction(ref work, rq, tq);
 
      }
      
    }

    private void GenerateRoleProperty(ref Graph work, RoleDefinition rd, TemplateDefinition td)
    {
      var namespce = nsMap.GetPrefix(new Uri(rd.range.Substring(0, rd.range.IndexOf('#') + 1)));
      _subj = work.CreateUriNode(new Uri(rd.identifier));
      _pred = work.CreateUriNode(rdfType);
      _obj = work.CreateUriNode(namespce.Equals("xsd") ? "owl:DatatypeProperty" : "owl:ObjectProperty");
      work.Assert(new Triple(_subj, _pred, _obj));
      _pred = work.CreateUriNode("rdfs:label");
      _obj = work.CreateLiteralNode(rd.name[0].value, rd.name[0].lang);
      work.Assert(new Triple(_subj, _pred, _obj));
      _pred = work.CreateUriNode("rdfs:subPropertyOf");
      _obj = work.CreateUriNode(namespce.Equals("xsd") ? "p7tm:valDataRoleFiller" : "p7tm:hasObjectRoleFiller");
      work.Assert(new Triple(_subj, _pred, _obj));

    }

    private void GenerateTemplateDescription(ref Graph work, string subjId, string objectId, object gobj)
    {
      var name = string.Empty;
      var templateDescrId = CreateNewGuidId("http://tpl.rdlfacade.org/data#");
      if (gobj is TemplateDefinition)
      {
        name = ((TemplateDefinition) gobj).name[0].value;
        var count = ((TemplateDefinition)gobj).roleDefinition.Count;
        GenerateRoleCountPart8(ref work, count, subjId, gobj);
      }
      else if (gobj is TemplateQualification)
      {
        name = ((TemplateQualification)gobj).name[0].value;
        var count = ((TemplateQualification)gobj).roleQualification.Count;
        GenerateRoleCountPart8(ref work, count, subjId, gobj);
        GenerateHasSuperTemplate(ref work, templateDescrId, (TemplateQualification) gobj);
      }
      _subj = work.CreateUriNode(new Uri(templateDescrId));
      _pred = work.CreateUriNode(rdfType);
      _obj = work.CreateUriNode("owl:Thing");
      work.Assert(new Triple(_subj, _pred, _obj));
      _obj = work.CreateUriNode("p7tm:TemplateDescription");
      work.Assert(new Triple(_subj, _pred, _obj));


      GenerateHasTemplate(ref work, _subj.ToString(), subjId,  gobj);
    }

    private void GenerateTemplateHeader(ref Graph work, string subjId, string objectId, object gobj)
    {
      _subj = work.CreateUriNode(new Uri(subjId));
      _pred = work.CreateUriNode(rdfType);
      _obj = work.CreateUriNode("owl:Class");
      work.Assert(new Triple(_subj, _pred, _obj));
      if (gobj is TemplateDefinition)
      {
        var td = (TemplateDefinition) gobj;
        _subj = work.CreateUriNode(new Uri(subjId));
        _pred = work.CreateUriNode("rdfs:label");
        _obj = work.CreateLiteralNode(td.name[0].value, string.IsNullOrEmpty(td.name[0].lang) ? defaultLanguage : td.name[0].lang);
        work.Assert(new Triple(_subj, _pred, _obj));
        _pred = work.CreateUriNode(rdfssubClassOf);
        _obj = work.CreateUriNode("p7tm:BaseTemplateStatement");
        work.Assert(new Triple(_subj, _pred, _obj));
      }
      else if(gobj is TemplateQualification)
      {
        var tq = (TemplateQualification) gobj;
        _subj = work.CreateUriNode(new Uri(subjId));
        _pred = work.CreateUriNode("rdfs:label");
        _obj = work.CreateLiteralNode(tq.name[0].value,
                                      string.IsNullOrEmpty(tq.name[0].lang) ? defaultLanguage : tq.name[0].lang);
        work.Assert(new Triple(_subj, _pred, _obj));
        _pred = work.CreateUriNode(rdfssubClassOf);
        _obj = work.CreateUriNode("p7tm:RDLTemplateStatement");
        work.Assert(new Triple(_subj, _pred, _obj));
        _pred = work.CreateUriNode(rdfssubClassOf);
        if (!string.IsNullOrEmpty(tq.qualifies))
        {
          _obj = work.CreateUriNode(new Uri(tq.qualifies));
          work.Assert(new Triple(_subj, _pred, _obj));
        }
      }
    }

    private void GenerateClassDescription(ref Graph work, Description descr, string subjectId)
    {
      _subj = work.CreateUriNode(new Uri(subjectId));
      _pred = work.CreateUriNode(rdfType);
      _obj = work.CreateUriNode("owl:Class");
      work.Assert((new Triple(_subj, _pred, _obj)));
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
  }
}