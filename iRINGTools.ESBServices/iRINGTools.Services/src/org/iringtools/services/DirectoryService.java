package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import org.iringtools.directory.Directory;
import org.iringtools.library.directory.DirectoryProvider;

@Path("/")
@Produces(MediaType.APPLICATION_XML)
public class DirectoryService extends AbstractService {
	private final String SERVICE_NAME = "DirectoryService";

	@GET
	@Path("/directory")
	public Response getDirectory() {
		Directory directory = null;

		try {
			initService(SERVICE_NAME);
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
		}

		DirectoryProvider directoryProvider = new DirectoryProvider(settings);

		try {
			directory = directoryProvider.getDirectory();
		} catch (Exception e) {
			return prepareErrorResponse(
					HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}

		return Response.ok().entity(directory).build();
	}
}