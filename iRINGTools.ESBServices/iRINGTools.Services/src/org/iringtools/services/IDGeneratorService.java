package org.iringtools.services;

import java.io.IOException;

import javax.servlet.Filter;
import javax.servlet.FilterChain;
import javax.servlet.FilterConfig;
import javax.servlet.ServletException;
import javax.servlet.ServletRequest;
import javax.servlet.ServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
//import org.ids_adi.rdf.registry.IDSADIRDFRegistry;
//import org.ids_adi.servlet.ConnectionRegistryFilterConfigSource;

//import org.ids_adi.config.Config;
//import org.iringtools.services.core.IdGenProvider;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class IDGeneratorService implements Filter{

	public IDGeneratorService(){
		System.out.println("Constructor IDGeneratorService");
	}
	
	//private Config config;
  //private IdGenProvider registry ;
    
	public void init(FilterConfig filterConfig) throws ServletException {
		System.out.println("IDGeneratorService:Inside init");
		/*this.config = new Config(new ConnectionRegistryFilterConfigSource(filterConfig, "org.ids-adi."));
	    System.out.println("init:config:"+config);		*/
	}
	private static final Logger logger = Logger.getLogger(IDGeneratorService.class);
	
	  @GET
	  @Path("/acquire")
	  //public String acquireIDForURL(@PathParam("url") String url, @PathParam("name") String name) throws JAXBException, IOException
	  public String acquireIDForURL() throws JAXBException, IOException
	  {
	    String id=null;

	    try
	    {
	      System.out.println("Inside acquireIDForURL");	
	      /*registry = new IdGenProvider();
	      System.out.println("registry:"+registry);
	      id = registry.acquireIDIn("http://rdl.rdlfacade.org/data#", "newly", "rashmi");
	      System.out.println("Id:"+id);*/
	    }
	    catch (Exception ex)
	    {
	      logger.error("Error getting federation information: " + ex);
	    }

	    return id;
	  }
	  public void destroy() {
	    }

	  public void doFilter(ServletRequest request,
		    ServletResponse response, FilterChain chain)
		    throws IOException, ServletException {
		  System.out.println("Inside DoFilter");
	  }

}
