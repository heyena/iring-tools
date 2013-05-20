package org.iringtools.services;

import java.util.List;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Response;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DfiRequest;
import org.iringtools.dxfr.request.DfoRequest;
import org.iringtools.library.exchange.DifferencingProvider;

@Path("/")
@Produces("application/xml")
@Consumes("application/xml")
public class DifferencingService extends AbstractService {
	private final String SERVICE_TYPE = "DifferencingService";

	@POST
	@Path("/dxi")
	public Response diff(DfiRequest dxiRequest)
	{
		DataTransferIndices dxis = null;
        
		try
		{
			initService(SERVICE_TYPE);
		}
		catch (Exception e)
		{
			return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
		}
    
		try
		{
			DataTransferIndices sourceDtis = null;
			DataTransferIndices targetDtis = null;
			List<DataTransferIndices> dtisList = dxiRequest.getDataTransferIndices();

			if (dtisList.get(0).getScopeName().equalsIgnoreCase(dxiRequest.getSourceScopeName())
					&& dtisList.get(0).getAppName().equalsIgnoreCase(dxiRequest.getSourceAppName()))
			{
				sourceDtis = dtisList.get(0);
				targetDtis = dtisList.get(1);
			}
			else
			{
				sourceDtis = dtisList.get(1);
				targetDtis = dtisList.get(0);
			}

			DifferencingProvider diffProvider = new DifferencingProvider();
			dxis = diffProvider.diff(sourceDtis, targetDtis);
			return Response.ok().entity(dxis).build();
		}
		catch (Exception e)
		{
			return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}
	}

	@POST
	@Path("/dxo")
	public Response diff(DfoRequest dxoRequest) {
		DataTransferObjects dxos = null;

		try {
			initService(SERVICE_TYPE);
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_UNAUTHORIZED, e);
		}

		try {
		    List<DataTransferObjects> dtosList = dxoRequest.getDataTransferObjects();
		    DataTransferObjects sourceDtos = null;
		    DataTransferObjects targetDtos = null;
		    Manifest manifest = dxoRequest.getManifest();
		    
		    if (dtosList.get(0).getScopeName().equalsIgnoreCase(dxoRequest.getSourceScopeName())
		        && dtosList.get(0).getAppName().equalsIgnoreCase(dxoRequest.getSourceAppName()))
		    {
		      sourceDtos = dtosList.get(0);
		      targetDtos = dtosList.get(1);
		    }
		    else
		    {
		      sourceDtos = dtosList.get(1);
		      targetDtos = dtosList.get(0);
		    }

			DifferencingProvider diffProvider = new DifferencingProvider();
			dxos = diffProvider.diff(manifest, sourceDtos, targetDtos);
			return Response.ok().entity(dxos).build();
		} catch (Exception e) {
			return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
		}
	}
}