package org.iringtools.services;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.List;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.ids_adi.ns.qxf.model.Qmxf;
import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.refdata.queries.Queries;
import org.iringtools.refdata.response.Entity;
import org.iringtools.services.core.RefDataProvider;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class RefDataService extends AbstractService {
	private static final Logger logger = Logger.getLogger(RefDataService.class);

	@GET
	@Path("/federation")
	public Federation getFederation() throws JAXBException, IOException {
		Federation federation = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			federation = refDataProvider.getFederation();
		} catch (Exception ex) {
			logger.error("Error getting federation information: " + ex);
		}

		return federation;
	}

	@POST
	@Path("/federation")
	public Response saveFederation(Federation federation) {
		Response response = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			response = refDataProvider.saveFederation(federation);
		} catch (Exception ex) {
			logger.error("Error while saving federation xml: " + ex);
		}

		return response;
	}

	@POST
	@Path("/namespace")
	public Response saveNamespace(Namespace namespace) {
		Response response = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			response = refDataProvider.saveNamespace(namespace, false);
		} catch (Exception ex) {
			logger.error("Error while saving namespace: " + ex);
		}

		return response;
	}

	@POST
	@Path("/idgenerator")
	public Response saveIDGenerator(IDGenerator idgenerator) {
		Response response = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			response = refDataProvider.saveIdGenerator(idgenerator, false);
		} catch (Exception ex) {
			logger.error("Error while saving ID Generator: " + ex);
		}

		return response;
	}

	@POST
	@Path("/repository")
	public Response saveRepository(Repository repository) {
		Response response = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			response = refDataProvider.saveRepository(repository, false);
		} catch (Exception ex) {
			logger.error("Error while saving Repository: " + ex);
		}

		return response;
	}

	@POST
	@Path("/namespace/delete")
	public Response deleteNamespace(Namespace namespace) {
		Response response = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			response = refDataProvider.saveNamespace(namespace, true);
		} catch (Exception ex) {
			logger.error("Error while saving namespace: " + ex);
		}

		return response;
	}

	@POST
	@Path("/idgenerator/delete")
	public Response deleteIDGenerator(IDGenerator idgenerator) {
		Response response = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			response = refDataProvider.saveIdGenerator(idgenerator, true);
		} catch (Exception ex) {
			logger.error("Error while saving ID Generator: " + ex);
		}

		return response;
	}

	@POST
	@Path("/repository/delete")
	public Response deleteRepository(Repository repository) {
		Response response = null;

		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			response = refDataProvider.saveRepository(repository, true);
		} catch (Exception ex) {
			logger.error("Error while saving Repository: " + ex);
		}

		return response;
	}

	/*
	 * @GET
	 * 
	 * @Path("/version") public Version GetVersion() {
	 * OutgoingWebResponseContext context =
	 * WebOperationContext.Current.OutgoingResponse; context.ContentType =
	 * "application/xml"; return _referenceDataProvider.getVersion(); }
	 */
	@GET
	@Path("/classes/{id}/label")
	public String getClassLabel(@PathParam("id") String id) {
		String classLabel = "";
		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			classLabel = refDataProvider.getClassLabel(id);
		} catch (Exception ex) {
			logger.error("Error getting class label information: " + ex);
		}
		return classLabel;
	}

	/*
	 * @GET
	 * 
	 * @Path("/classes/{id}/{namespace}")
	 * 
	 * public Qmxf getClass(@PathParam("id") String id,@PathParam("namespace")
	 * String namespace) {
	 * 
	 * Qmxf qmxf= null; try { initService(); RefDataProvider refDataProvider =
	 * new RefDataProvider(settings); qmxf =
	 * refDataProvider.GetClass(id,namespace); } catch (Exception ex) {
	 * logger.error("Error getting federation information: " + ex); } return
	 * qmxf; }
	 * 
	 * @GET
	 * 
	 * @Path("/templates/{id}") public Qmxf getTemplate(@PathParam("id") String
	 * id) { Qmxf qmxf= null; try { initService(); RefDataProvider
	 * refDataProvider = new RefDataProvider(settings); qmxf =
	 * refDataProvider.GetTemplate(id); } catch (Exception ex) {
	 * logger.error("Error getting federation information: " + ex); } return
	 * qmxf; }
	 * 
	 * @POST
	 * 
	 * @Path("/templates") public Response PostTemplate(Qmxf qmxf) { Response
	 * response=null; try { initService(); RefDataProvider refDataProvider = new
	 * RefDataProvider(settings); response= refDataProvider.PostTemplate(qmxf);
	 * } catch (Exception ex) {
	 * logger.error("Error getting federation information: " + ex); } return
	 * response; }
	 * 
	 * @POST
	 * 
	 * @Path("/classes") public Response PostClass(Qmxf qmxf) { Response
	 * response=null; try { initService(); RefDataProvider refDataProvider = new
	 * RefDataProvider(settings); response= refDataProvider.PostClass(qmxf); }
	 * catch (Exception ex) {
	 * logger.error("Error getting federation information: " + ex); } return
	 * response; }
	 */
	@GET
	@Path("/repositories")
	public List<Repository> getRepositories() {
		List<Repository> repositoryList = null;
		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			repositoryList = refDataProvider.getRepositories();
		} catch (Exception ex) {
			logger.error("Error getting getRepositories information: " + ex);
		}
		return repositoryList;
	}

	public List<Entity> getSuperClasses(String id) {
		return null;
	}

	public List<Entity> getAllSuperClasses(String id) {
		return null;
	}

	public List<Entity> getSubClasses(String id) {
		return null;

	}

	public List<Entity> getClassTemplates(String id) {
		return null;

	}

	/*
	 * @GET
	 * 
	 * @Path("/search/{query}/{start}/{limit}/reset") public RefDataEntities
	 * searchPageReset(String query, String start, String limit) {
	 * OutgoingWebResponseContext context =
	 * WebOperationContext.Current.OutgoingResponse; context.ContentType =
	 * "application/xml";
	 * 
	 * int startIdx = 0; int pageLimit = 0; int.TryParse(start, out startIdx);
	 * int.TryParse(limit, out pageLimit);
	 * 
	 * return _referenceDataProvider.searchPageReset(query, startIdx,
	 * pageLimit); }
	 * 
	 * @GET
	 * 
	 * @Path("/repositories/{query}") public List<Entity>
	 * find(@PathParam("query") String query) { String classLabel = ""; try {
	 * initService(); RefDataProvider refDataProvider = new
	 * RefDataProvider(settings); classLabel = refDataProvider.find(query); }
	 * catch (Exception ex) {
	 * logger.error("Error getting federation information: " + ex); } return
	 * classLabel;
	 * 
	 * }
	 * 
	 * @GET
	 * 
	 * @Path("/search/{query}") public RefDataEntities search(String query) {
	 * OutgoingWebResponseContext context =
	 * WebOperationContext.Current.OutgoingResponse; context.ContentType =
	 * "application/xml"; return _referenceDataProvider.search(query); }
	 * 
	 * @GET
	 * 
	 * @Path("/search/{query}/{start}/{limit}") public RefDataEntities
	 * searchPage(String query, String start, String limit) {
	 * OutgoingWebResponseContext context =
	 * WebOperationContext.Current.OutgoingResponse; context.ContentType =
	 * "application/xml"; int startIdx = 0; int pageLimit = 0;
	 * int.TryParse(start, out startIdx); int.TryParse(limit, out pageLimit);
	 * 
	 * return _referenceDataProvider.searchPage(query, startIdx, pageLimit); }
	 * 
	 * @GET
	 * 
	 * @Path("/search/{query}/reset") public RefDataEntities searchReset(String
	 * query) { OutgoingWebResponseContext context =
	 * WebOperationContext.Current.OutgoingResponse; context.ContentType =
	 * "application/xml";
	 * 
	 * return _referenceDataProvider.searchReset(query); }
	 */

	@GET
	@Path("/queries")
	public Queries getQueries() {
		Queries queries = null;
		try {
			initService();
			RefDataProvider refDataProvider = new RefDataProvider(settings);
			queries = refDataProvider.getQueries();
		} catch (Exception ex) {
			logger.error("Error getting getRepositories information: " + ex);
		}
		return queries;
	}

	// @GET
	// @Path("/label/{query}")
	// public String getLabel(@PathParam("query") String query) throws Exception
	// {
	// String label = "";
	// try {
	// initService();
	// RefDataProvider refDataProvider = new RefDataProvider(settings);
	// label = refDataProvider.getLabel(query);
	// } catch (RuntimeException ex) {
	// logger.error("Error getting getLabel information: " + ex);
	//
	// }
	// return label;
	// }
}
