package org.iringtools.services;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;

import org.iringtools.services.core.IdGenProvider;


@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class IDGeneratorService extends AbstractService{ 
	
	public IDGeneratorService(){
		System.out.println("Constructor IDGeneratorService");
		
	}

	 @GET
	 @Path("/acquireId")
	 public String showAquireId() throws Exception 
	    {
	      try
	      {
	    	  
	    	  IdGenProvider idGenProvider = new IdGenProvider();
	    	  //System.out.println(uri);
	    	  //generatedId = idGenProvider.generateRandomNumber(uri);
	      }
	      catch (Exception ex)
	      {
	      }

	     // return generatedId;
	      return "";
	    }
    @GET
    @Path("/acquireId/{uri}")
    public String acquireId(@PathParam("uri") String uri) throws Exception 
    {
      String generatedId = null;
      try
      {
    	  
    	  IdGenProvider idGenProvider = new IdGenProvider();
    	  System.out.println(uri);
    	  generatedId = idGenProvider.generateRandomNumber(uri);
      }
      catch (Exception ex)
      {
      }

      return generatedId;
    }
	  
	  
	  
}
