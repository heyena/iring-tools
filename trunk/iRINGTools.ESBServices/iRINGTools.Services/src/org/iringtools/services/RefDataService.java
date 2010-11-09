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
import org.iringtools.sparql.Query;
import org.iringtools.directory.Directory;
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
    settings.put("baseDirectory", context.getRealPath("/"));
  }
  public RefDataService()
  {
    settings = new Hashtable<String, String>();
    //init();
  }
  
  @GET
  @Path("/classes/{id}/allsuperclasses")
  public List getClassDetails(@PathParam("id") String id) throws JAXBException, IOException
  {
    List directory = null;

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
    
              List<Dictionary<string, string>> results = BindQueryResults(queryBindings, sparqlResults);
    
              foreach (Dictionary<string, string> result in results)
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
          System.out.println("###"+refDataProvider.getQueryFileName());
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
          logger.debug("Error in GetSpecializations: " + e);
          //throw new Exception("Error while Getting Class: " + id + ".\n" + e.toString(), e);
      }
        return specializations;
      
  }
  /*private List<Dictionary<string, string>> BindQueryResults(QueryBindings queryBindings, SPARQLResults sparqlResults)
  {
      try
      {
          List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

          foreach (SPARQLResult sparqlResult in sparqlResults.resultsElement.results)
          {
              Dictionary<string, string> result = new Dictionary<string, string>();

              string sortKey = null;

              foreach (SPARQLBinding sparqlBinding in sparqlResult.bindings)
              {
                  foreach (QueryBinding queryBinding in queryBindings)
                  {
                      if (queryBinding.name == sparqlBinding.name)
                      {
                          string key = queryBinding.name;

                          string value = null;
                          string dataType = null;
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
