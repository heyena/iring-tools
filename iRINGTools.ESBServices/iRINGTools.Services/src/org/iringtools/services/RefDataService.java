package org.iringtools.services;

import java.util.List;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;

import org.ids_adi.ns.qxf.model.Qmxf;
import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IdGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.refdata.queries.Queries;
import org.iringtools.refdata.response.Entity;
import org.iringtools.security.AuthorizationException;
import org.iringtools.services.core.RefDataProvider;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class RefDataService extends AbstractService
{
  private final String SERVICE_NAME = "RefDataService";
  
  @POST
  @Path("/federation")
  public Response saveFederation(Federation federation)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveFederation(federation);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  @POST
  @Path("/namespace")
  public Response saveNamespace(Namespace namespace)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveNamespace(namespace, false);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  @POST
  @Path("/idgenerator")
  public Response saveIDGenerator(IdGenerator idgenerator)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveIdGenerator(idgenerator, false);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  @POST
  @Path("/repository")
  public Response saveRepository(Repository repository)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveRepository(repository, false);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  @POST
  @Path("/namespace/delete")
  public Response deleteNamespace(Namespace namespace)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveNamespace(namespace, true);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  @POST
  @Path("/idgenerator/delete")
  public Response deleteIDGenerator(IdGenerator idgenerator)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveIdGenerator(idgenerator, true);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  @POST
  @Path("/repository/delete")
  public Response deleteRepository(Repository repository)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.saveRepository(repository, true);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  /*
   * @GET
   * 
   * @Path("/version") public Version GetVersion() { OutgoingWebResponseContext context =
   * WebOperationContext.Current.OutgoingResponse; context.ContentType = "application/xml"; return
   * _referenceDataProvider.getVersion(); }
   */
  @GET
  @Path("/classes/{id}/label")
  @Produces("text/xml")
  public Entity getClassLabel(@PathParam("id") String id)
  {
    Entity classLabel = new Entity();
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      classLabel = refDataProvider.getClassLabel(id);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return classLabel;
  }

  @GET
  @Path("/classes/{id}/{namespace}")
  public Qmxf getClassFromRepository(@PathParam("id") String id, @PathParam("namespace") String namespace, Repository repository)
  {
    Qmxf qmxf = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      qmxf = refDataProvider.getClass(id, namespace, repository);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return qmxf;
  }
  
  @GET
  @Path("/classes/{id}/{namespace}")
  public Qmxf getClass(@PathParam("id") String id, @PathParam("namespace") String namespace)
  {
    Qmxf qmxf = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      qmxf = refDataProvider.getClass(id, namespace, null);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return qmxf;
  }

  @GET
  @Path("/templates/{id}")
  public Qmxf getTemplate(@PathParam("id") String id)
  {
    Qmxf qmxf = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      qmxf = refDataProvider.getTemplate(id);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return qmxf;
  }

  @GET
  @Path("/repositories")
  public List<Repository> getRepositories()
  {
    List<Repository> repositoryList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      repositoryList = refDataProvider.getRepositories();
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return repositoryList;
  }

  @GET
  @Path("/classes/{id}/superclasses")
  public org.iringtools.refdata.response.Response getSuperClasses(@PathParam("id") String id)
  {
    org.iringtools.refdata.response.Response entityList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      initService(SERVICE_NAME);
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      entityList = refdataProvider.getSuperClasses(id, null);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return entityList;
  }

  @GET
  @Path("/classes/{id}/subclasses")
  public org.iringtools.refdata.response.Response getSubClasses(@PathParam("id") String id)
  {
    org.iringtools.refdata.response.Response entityList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      entityList = refdataProvider.getSubClasses(id, null);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return entityList;
  }
  
  @GET
  @Path("/classes/{id}/subclasses")
  public org.iringtools.refdata.response.Response getSubClassesFromRepository(@PathParam("id") String id, Repository repository)
  {
    org.iringtools.refdata.response.Response entityList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      entityList = refdataProvider.getSubClasses(id, repository);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return entityList;
  }

  @GET
  @Path("/classes/{id}/allsuperclasses")
  public org.iringtools.refdata.response.Response getAllSuperClasses(@PathParam("id") String id)
  {
    org.iringtools.refdata.response.Response entityList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      entityList = refdataProvider.getAllSuperClasses(id);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return entityList;
  }

  @GET
  @Path("/classes/{id}/templates")
  public org.iringtools.refdata.response.Response getClassTemplates(@PathParam("id") String id)
  {
    org.iringtools.refdata.response.Response entityList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      entityList = refdataProvider.getClassTemplates(id);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return entityList;
  }

  @GET
  @Path("/classes/{id}/members")
  public org.iringtools.refdata.response.Response getClassMembers(@PathParam("id") String id)
  {
    org.iringtools.refdata.response.Response entityList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      entityList = refdataProvider.getClassMembers(id, null);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return entityList;
  }

  @GET
  @Path("/classes/{id}/members")
  public org.iringtools.refdata.response.Response getClassMembersFromRepository(@PathParam("id") String id, Repository repository)
  {
    org.iringtools.refdata.response.Response entityList = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      entityList = refdataProvider.getClassMembers(id, repository);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return entityList;
  }
  
  @GET
  @Path("/search/{query}/{start}/{limit}")
  public org.iringtools.refdata.response.Response searchPage(@PathParam("query") String query,
      @PathParam("start") String start, @PathParam("limit") String limit) 
  {
    org.iringtools.refdata.response.Response response = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {

      int startIdx = Integer.parseInt(start);
      int pageLimit = Integer.parseInt(limit);
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      response = refdataProvider.searchPage(query, startIdx, pageLimit);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return response;
  }

  /*
   * @GET
   * 
   * @Path("/repositories/{query}") public List<Entity> find(@PathParam("query") String query) { String classLabel = "";
   * try { initService(SERVICE_TYPE); RefDataProvider refDataProvider = new RefDataProvider(settings); classLabel =
   * refDataProvider.find(query); } catch (Exception ex) { logger.error("Error getting federation information: " + ex);
   * } return classLabel;
   * 
   * }
   */
  @GET
  @Path("/search/{query}")
  public org.iringtools.refdata.response.Response search(@PathParam("query") String query)
  {
    org.iringtools.refdata.response.Response response = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refdataProvider = new RefDataProvider(settings);
      response = refdataProvider.searchPage(query, 0, 0);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return response;
  }

  /*
   * @GET
   * 
   * @Path("/search/{query}/{start}/{limit}") public RefDataEntities searchPage(String query, String start, String
   * limit) { OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse; context.ContentType =
   * "application/xml"; int startIdx = 0; int pageLimit = 0; int.TryParse(start, out startIdx); int.TryParse(limit, out
   * pageLimit);
   * 
   * return _referenceDataProvider.searchPage(query, startIdx, pageLimit); }
   * 
   * @GET
   * 
   * @Path("/search/{query}/reset") public RefDataEntities searchReset(String query) { OutgoingWebResponseContext
   * context = WebOperationContext.Current.OutgoingResponse; context.ContentType = "application/xml";
   * 
   * return _referenceDataProvider.searchReset(query); }
   */

  @GET
  @Path("/queries")
  public Queries getQueries()
  {
    Queries queries = null;
    
    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      queries = refDataProvider.getQueries();
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
    
    return queries;
  }

  // @GET
  // @Path("/label/{query}")
  // public String getLabel(@PathParam("query") String query) 
  // {
  // String label = "";
  // try {
  // initService(SERVICE_TYPE);
  // RefDataProvider refDataProvider = new RefDataProvider(settings);
  // label = refDataProvider.getLabel(query);
  // } catch (RuntimeException ex) {
  // logger.error("Error getting getLabel information: " + ex);
  //
  // }
  // return label;
  // }

  @POST
  @Path("/class")
  public Response postClass(Qmxf qmxf)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.postClass(qmxf);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }

  @POST
  @Path("/template")
  public Response postTemplate(Qmxf qmxf)
  {
    Response response = null;

    try
    {
      initService(SERVICE_NAME);
    }
    catch (AuthorizationException e)
    {
      prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
    }

    try
    {
      RefDataProvider refDataProvider = new RefDataProvider(settings);
      response = refDataProvider.postTemplate(qmxf);
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }
}
