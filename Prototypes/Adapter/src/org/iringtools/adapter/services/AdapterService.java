package org.iringtools.adapter.services;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;

import org.iringtools.adapter.services.core.AdapterProvider;
import org.iringtools.common.Version;
import org.iringtools.library.Scope;

@Path("/")
@Produces("application/xml")
public class AdapterService extends AbstractService {
	private AdapterProvider _adapterProvider = null;

	  public AdapterService()
	    {
		  try {
				//initService();
				_adapterProvider = new AdapterProvider();
				
			} catch (Exception ex) {
				//logger.error("Error getting federation information: " + ex);
			}
	    }


	@GET
	@Path("/version")
	@Produces("application/xml")
	public Version getVersion(){
		//Version version = _adapterProvider.getVersion();
		return _adapterProvider.getVersion();
	}

	@GET
	@Path("/scope")
	@Produces("application/xml")
	public Scope getScope(){
		//Scope scope = _adapterProvider.getScope();
		return _adapterProvider.getScope();
	}
}