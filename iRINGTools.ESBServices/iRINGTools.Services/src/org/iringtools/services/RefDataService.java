package org.iringtools.services;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import java.util.ResourceBundle;

import javax.servlet.ServletContext;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;


import org.iringtools.refdata.queries.Queries;
import org.iringtools.refdata.queries.Query;
import org.iringtools.refdata.queries.QueryItem;
import org.iringtools.services.core.RefDataProvider;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class RefDataService
{
  private static final Logger logger = Logger.getLogger(RefDataService.class);

  @Context
  private ServletContext context;
  private Query queries = null;
  private Hashtable<String, String> settings;
  private void init()
  {
	System.out.println("context.getRealPath : "+context.getRealPath("/"));
    settings.put("baseDirectory", context.getRealPath("/"));
  }
  public RefDataService()
  {
    settings = new Hashtable<String, String>();
  }
  
  @GET
  @Path("/classes/{id}/allsuperclasses")
  public List getClassDetails(@PathParam("id") String id) throws JAXBException, IOException
  {
    List directory = null;
    init();
    try
    {

      List specializations = GetSpecializations(id);
      //base case
      /*if (specializations.Count == 0)
      {
          return null;
      }

      foreach (Specialization specialization in specializations)
      {
          String uri = specialization.reference;
          String label = specialization.label;

          if (label == null)
              label = GetLabel(uri);

          Entity resultEntity = new Entity
          {
              uri = uri,
              label = label
          };

          String trimmedUri = null;
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
      }*/

      //list.Sort(Entity.sortAscending());
  }
  catch (Exception e)
  {
      logger.debug("Error in GetAllSuperClasses: " + e);
     // throw new Exception("Error while Finding " + id + ".\n" + e.toString(), e);
  }

    return directory;
  }
  
  @GET
  @Path("/classes/{id}/label")
  public String getClassLabel(@PathParam("id") String id) throws JAXBException, IOException
  {
    return GetLabel("http://rdl.rdlfacade.org/data#" + id);
  }
  
  private String GetLabel(String id){

    String label = null;
    String sparql = null;
    String relativeUri = null;

    try
      {
          /*Query query = _queries["GetLabel"];
          QueryBindings queryBindings = query.bindings;
    
          sparql = ReadSPARQL(query.fileName);
          sparql = sparql.Replace("param1", uri);
    
          foreach (Repository repository in _repositories)
          {
              SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);
    
              List<Dictionary<String, String>> results = BindQueryResults(queryBindings, sparqlResults);
    
              foreach (Dictionary<String, String> result in results)
              {
                  if (result.ContainsKey("label"))
                  {
                      label = result["label"];
                  }
              }
          }*/
    
          
      }
      catch (Exception e)
      {
        logger.debug("Error in GetLabel: " + e);
      }
      return label;
  }
  
  @GET
  @Path("/classes/{id}/subclasses")
  public List GetSubClasses(@PathParam("id") String id) throws JAXBException, IOException
        {
            List queryResult = new ArrayList();

            try
            {
                String sparql = null;
                String sparqlPart8 = null;
                String relativeUri = null;

                /*Query queryGetSubClasses = _queries["GetSubClasses"];
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

                        List<Dictionary<String, String>> results = BindQueryResults(queryBindingsPart8, sparqlResults);

                        foreach (Dictionary<String, String> result in results)
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

                        List<Dictionary<String, String>> results = BindQueryResults(queryBindings, sparqlResults);

                        foreach (Dictionary<String, String> result in results)
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
                }*/
            }
            catch (Exception e)
            {
                logger.debug("Error in GetSubClasses: " + e);
            }
            return queryResult;
        }
  
  
  @GET
  @Path("/classes/{id}/superclasses")
  public List GetSuperClasses(@PathParam("id") String id)
        {
	    	init();
            List queryResult = new ArrayList();

            try
            {
                List specializations = GetSpecializations(id);

                /*foreach (Specialization specialization in specializations)
                {
                    String uri = specialization.reference;
                    String label = specialization.label;

                    if (label == null)
                        label = GetLabel(uri);

                    Entity resultEntity = new Entity
                    {
                        uri = uri,
                        label = label
                    };
                    Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
                    //queryResult.Add(resultEntity);
                }*/
            }
            catch (Exception e)
            {
                logger.debug("Error in GetSuperClasses: " + e);
                
            }
            return queryResult;
        }
  
  /*
  @GET
  @Path("/classes/{id}/templates")
  public List<Entity> GetClassTemplates(@PathParam("id") String id)
        {
            List<Entity> queryResult = new List<Entity>();
            try
            {
                String sparqlGetClassTemplates = null;
                String sparqlGetRelatedTemplates = null;
                String relativeUri = null;

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

                        List<Dictionary<String, String>> results = BindQueryResults(queryBindingsGetRelatedTemplates, sparqlResults);

                        foreach (Dictionary<String, String> result in results)
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

                        List<Dictionary<String, String>> results = BindQueryResults(queryBindingsGetClassTemplates, sparqlResults);

                        foreach (Dictionary<String, String> result in results)
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
  
  
  @GET
  @Path("/find/{query}")
  public List<Entity> Find(@PathParam("query") String query)
        {
            List<Entity> queryResult = new List<Entity>();
            try
            {
                String sparql = null;
                String relativeUri = null;

                Query queryExactSearch = _queries["ExactSearch"];
                QueryBindings queryBindings = queryExactSearch.bindings;

                sparql = ReadSPARQL(queryExactSearch.fileName);

                sparql = sparql.Replace("param1", query);

                foreach (Repository repository in _repositories)
                {
                    SPARQLResults sparqlResults = QueryFromRepository(repository, sparql);

                    List<Dictionary<String, String>> results = BindQueryResults(queryBindings, sparqlResults);


                    foreach (Dictionary<String, String> result in results)
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
  
  
  @GET
  @Path("/repositories")
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
  
  
  @GET
  @Path("/search/{query}")
  public RefDataEntities Search(@PathParam("query") String query)
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
  
  @GET
  @Path("/search/{query}/{start}/{limit}")
  public RefDataEntities SearchPage(@PathParam("query") String query, @PathParam("start") int start, @PathParam("limit") int limit)
        {
                RefDataEntities entities = null;          
                int counter = 0;

                try
                {
                    String sparql1 = null;
                    String sparql2 = null;
                    String relativeUri = null;

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

                        Query queryTemplateContainsSearch = _queries["TemplateContainsSearch"];
                        QueryBindings queryBindings2 = queryTemplateContainsSearch.bindings;

                        sparql1 = ReadSPARQL(queryContainsSearch.fileName);
                        sparql1 = sparql1.Replace("param1", query);

                        sparql2 = ReadSPARQL(queryTemplateContainsSearch.fileName);
                        sparql2 = sparql2.Replace("param1", query);
                        sparql2 = sparql2.Replace("param2", "0"); //TODO:Remove hard-coded OFFSET parameter

                        foreach (Repository repository in _repositories)
                        {
                            SPARQLResults sparqlResults = QueryFromRepository(repository, sparql1);

                            List<Dictionary<String, String>> results = BindQueryResults(queryBindings, sparqlResults);
                            foreach (Dictionary<String, String> result in results)
                            {
                                Entity resultEntity = new Entity
                                {
                                    uri = result["uri"],
                                    label = result["label"],
                                    repository = repository.name
                                };

                                String key = resultEntity.label;

                                if (resultEntities.Entities.ContainsKey(key))
                                {
                                    key += ++counter;
                                }

                                resultEntities.Entities.Add(key, resultEntity);
                            }
                            results.Clear();

                            //for Camelot repositories, search using "TemplateContainsSearch" too
                            if (repository.repositoryType == RepositoryType.Camelot)
                            {
                                sparqlResults = QueryFromRepository(repository, sparql2);

                                results = BindQueryResults(queryBindings, sparqlResults);
                                foreach (Dictionary<String, String> result in results)
                                {
                                    Entity resultEntity = new Entity
                                    {
                                        uri = result["uri"],
                                        label = result["label"],
                                        repository = repository.name
                                    };

                                    String key = resultEntity.label;

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

                    _logger.Info(String.Format("SearchPage is returning {0} records", entities.Entities.Count));
                    return entities;
                }
                catch (Exception e)
                {
                    _logger.Error("Error in SearchPage: " + e);
                    throw new Exception("Error while Finding " + query + ".\n" + e.ToString(), e);
                }
        }

  
  @GET
  @Path("/search/{query}/{start}/{limit}/reset")
  public RefDataEntities SearchPageReset(@PathParam("query") String query, @PathParam("start") int start, @PathParam("limit") int limit)
        {
            Reset(query);

            return SearchPage(query, start, limit);
        }
  
  @GET
  @Path("/search/{query}/reset")
  public RefDataEntities SearchReset(@PathParam("query") String query)
        {
            Reset(query);

            return Search(query);
        }
  
  @GET
  @Path("/templates")
  public Response PostTemplate(QMXF qmxf)
        {
            Status status = new Status();

            try
            {
                Response response = null;
                String sparql = "PREFIX eg: <http://example.org/data#> "
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
                        String ID = null;
                        String id = null;
                        String label = null;
                        String description = null;
                        String generatedTempId = null;
                        String templateName = null;
                        String roleDefinition = null;
                        int index = 1;
                        String nameSparql = null;
                        String specSparql = null;
                        String classSparql = null;
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

                                    String roleID = null;
                                    String roleLabel = null;
                                    String roleDescription = null;
                                    String generatedId = null;
                                    String genName = null;

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
                            String rName = null;

                            sparql = sparql.Replace("INSERT DATA { ", "MODIFY DELETE { ");
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
                                //}
                            }
                            foreach (QMXFName name in template.name)
                            {
                                String gen = null;
                                String generatedId = null;
                                String roleID = null;

                                label = name.value;
                                nameSparql += " INSERT { " + ID + " rdfs:label \"" + label + "\"^^xsd:string ; ";
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
                            String ID = null;
                            String id = null;
                            String label = null;
                            String description = null;
                            //ID = template.identifier.Remove(0, 1);
                            String generatedTempId = null;
                            String templateName = null;
                            String roleDefinition = null;
                            String specialization = null;
                            String nameSparql = null;
                            String specSparql = null;
                            String classSparql = null;

                            ID = template.identifier;

                            QMXF q = new QMXF();
                            if (ID != null)
                            {
                                id = ID.Substring((ID.LastIndexOf("#") + 1), ID.Length - (ID.LastIndexOf("#") + 1));
                                q = GetTemplate(id);
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

                                        String roleID = null;
                                        String roleLabel = null;
                                        String roleDescription = null;
                                        String generatedId = null;
                                        String genName = null;                                        

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
                                            String roleValueAs = role.value.As;
                                            if (role.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
                                            {
                                                roleValueAs = role.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", null);
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
                                String roleID = null;
                                String roleLabel = null;
                                String roleDescription = null;
                                String generatedId = null;
                                String genName = null;
                                int i = 0;

                                TemplateQualification td = q.templateQualifications[0];
                                String rName = null;

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
                                                String roleValueAs = rd.value.As;
                                                if (rd.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
                                                {
                                                    roleValueAs = rd.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", null);
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
                                                String roleValueAs = rd.value.As;
                                                if (rd.value.As.StartsWith(@"http://www.w3.org/2001/XMLSchema#"))
                                                {
                                                    roleValueAs = rd.value.As.Replace(@"http://www.w3.org/2001/XMLSchema#", null);
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
  
  @GET
  @Path("/templates/{id}")  
  public QMXF GetTemplate(@PathParam("id") String id)
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
  
  @GET
  @Path("/version")  */

  
  public static void main(String args[]){
    System.out.println("GetSpecializations");
    RefDataService r = new RefDataService();
    r.GetSpecializations("h");
  }
  
  private List GetSpecializations(String id)
  {
    List specializations=null;
      try
      {
    	  //init();
          String sparql = null;
          String sparqlPart8 = null;
          String relativeUri = null;

          specializations = new ArrayList();

         /* ResourceBundle queryGetSpecialization = ResourceBundle.getBundle("queries");
         
          String filename = queryGetSpecialization.getString("GetSpecialization");
          System.out.println(filename);
          SparqlUtil.ReadQueryFromFile(filename);*/
          
          System.out.println("Calling RefDataProvider");
          RefDataProvider refDataProvider = new RefDataProvider(settings);
          Queries query = (Queries)refDataProvider.getQueryFileName();
          List<QueryItem> queryItems = new ArrayList<QueryItem>();
          queryItems=query.getQueryItems();
          for(int i =0;i<queryItems.size();i++){
        	  QueryItem qi = (QueryItem)queryItems.get(i);
        	  System.out.println("Key :"+i+":"+qi.getKey());
          }
          /*QueryBindings queryBindings = queryGetSpecialization.bindings;

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

                  List<Dictionary<String, String>> results = BindQueryResults(queryBindingsPart8, sparqlResults);

                  foreach (Dictionary<String, String> result in results)
                  {
                      Specialization specialization = new Specialization();

                      String uri = null;
                      String label = null;
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

                  List<Dictionary<String, String>> results = BindQueryResults(queryBindings, sparqlResults);

                  foreach (Dictionary<String, String> result in results)
                  {
                      Specialization specialization = new Specialization();

                      String uri = null;
                      String label = null;
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
          }*/

          
      }
      catch (Exception e)
      {
          System.out.println("Error in GetSpecializations: " + e);
          //throw new Exception("Error while Getting Class: " + id + ".\n" + e.toString(), e);
      }
        return specializations;
      
  }
  
  
  /*private List<Dictionary<String, String>> BindQueryResults(QueryBindings queryBindings, SPARQLResults sparqlResults)
  {
      try
      {
          List<Dictionary<String, String>> results = new List<Dictionary<String, String>>();

          foreach (SPARQLResult sparqlResult in sparqlResults.resultsElement.results)
          {
              Dictionary<String, String> result = new Dictionary<String, String>();

              String sortKey = null;

              foreach (SPARQLBinding sparqlBinding in sparqlResult.bindings)
              {
                  foreach (QueryBinding queryBinding in queryBindings)
                  {
                      if (queryBinding.name == sparqlBinding.name)
                      {
                          String key = queryBinding.name;

                          String value = null;
                          String dataType = null;
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

                          if (dataType != null && dataType != null)
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
  }*/
}
