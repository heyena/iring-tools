package org.iringtools.services;

import java.util.List;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;

import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.security.AuthorizationException;
import org.iringtools.services.core.IDGeneratorProvider;

@Path("/")
@Consumes("application/xml")
@Produces("application/xml")
public class IDGeneratorService extends AbstractService
{
  private final String SERVICE_NAME = "IDGeneratorService";
  
  public IDGeneratorService() {}

  @GET
  @Path("/acquireId")
  public String showAquireId() throws Exception
  {
    try
    {

      // IdGenProvider idGenProvider = new IdGenProvider();
      // System.out.println(uri);
      // generatedId = idGenProvider.generateRandomNumber(uri);
    }
    catch (Exception ex)
    {}

    // return generatedId;
    return "";
  }

  @GET
  @Path("/acquireId/{params}")
  // public String acquireId(@PathParam("uri") String uri) throws Exception
  public Response acquireId(@PathParam("params") String params, @QueryParam("uri") String uri,
      @QueryParam("comment") String comment) throws Exception
  {
    Response response = new Response();
    String generatedId = null;

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
      IDGeneratorProvider idGenProvider = new IDGeneratorProvider(settings);
      generatedId = idGenProvider.generateRandomNumber(uri + "#", comment);

      if (generatedId != null)
      {
        response.setLevel(Level.SUCCESS);
        // String responseMsg = "Generated Id : "+generatedId;
        Messages messages = new Messages();
        response.setMessages(messages);
        List<String> messageList = messages.getItems();
        messageList.add(generatedId);
      }
      else
      {
        response.setLevel(Level.ERROR);
        Messages messages = new Messages();
        response.setMessages(messages);
        List<String> messageList = messages.getItems();
        messageList.add("Error Generating ID");
      }
    }
    catch (Exception e)
    {
      prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }

    return response;
  }
}
