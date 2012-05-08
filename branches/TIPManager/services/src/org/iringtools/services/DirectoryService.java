package org.iringtools.services;

import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import org.iringtools.common.Constants;
import org.iringtools.common.Section;
import org.iringtools.common.Setting;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.directory.Resource;
import org.iringtools.directory.Resources;
import org.iringtools.services.core.DirectoryProvider;
import org.iringtools.services.core.DirectoryXMLProvider;
import org.iringtools.services.core.LdapProvider;
import org.iringtools.utility.IOUtils;

@Path("/")
@Produces(MediaType.APPLICATION_XML)
public class DirectoryService extends AbstractService
{
  private final String SERVICE_NAME = "DirectoryService";

  private boolean useLdap;
  private String description = "", baseUrl = "", assembly = "", context = "";
  
  @POST
  @Path("/directory/endpoint/{path}/{endpointName}/{type}/{baseUrl}/{assembly}")
  public Response updateEndpoint(@PathParam("path") String path, @PathParam("endpointName") String endpointName,
      @PathParam("type") String type, @PathParam("baseUrl") String baseUrl, @PathParam("assembly") String assembly, String description)
  {
    try
    {
      initService(SERVICE_NAME);
      prepareProvider(description, baseUrl, assembly, "");

      if (useLdap)
      {
        LdapProvider ldapProvider = new LdapProvider(settings);
        String userId = getUserId();
        ldapProvider.updateDirectoryNode(userId, path, endpointName, type, this.description, "", this.baseUrl, this.assembly);
      }
      else
      {
        DirectoryXMLProvider xmlProvider = new DirectoryXMLProvider(settings);
        xmlProvider.updateDirectoryNode(path, endpointName, type, description, "", this.baseUrl, this.assembly);
      }

      return Response.ok().type(MediaType.TEXT_PLAIN).entity("endpoint updated successfully").build();
    }
    catch (Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }

  @POST
  @Path("/directory/folder/{path}/{folderName}/{type}/{context}")
  public Response updateFolder(@PathParam("path") String path, @PathParam("folderName") String folderName,
      @PathParam("type") String type, @PathParam("context") String context, String description)
  {
    try
    {
      initService(SERVICE_NAME);
      prepareProvider(description, baseUrl, assembly, context);

      if (useLdap)
      {
        LdapProvider ldapProvider = new LdapProvider(settings);
        String userId = getUserId();

        ldapProvider.updateDirectoryNode(userId, path, folderName, type, description, this.context, "", "");
      }
      else
      {
        DirectoryXMLProvider xmlProvider = new DirectoryXMLProvider(settings);
        xmlProvider.updateDirectoryNode(path, folderName, type, description, context, "", "");
      }

      return Response.ok().type(MediaType.TEXT_PLAIN).entity("folder updated successfully").build();
    }
    catch (Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }
  
  @POST
  @Path("/directory/{path}")
  @Produces(MediaType.APPLICATION_XML)
  public Response deleteDirectoryItem(@PathParam("path") String path)
  {
    try
    {
      initService(SERVICE_NAME);
      prepareProvider(description, baseUrl, assembly, "");
      
      if (useLdap)
      {
        LdapProvider ldapProvider = new LdapProvider(settings);
        ldapProvider.deleteLdapItem(path);
      }
      else
      {
        DirectoryXMLProvider xmlProvider = new DirectoryXMLProvider(settings);
        xmlProvider.deleteDirectoryItem(path);
      }
      
      return Response.ok().type(MediaType.TEXT_PLAIN).entity("directory item updated successfully").build();
    }
    catch(Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }   
  
  @POST
  @Path("/directory")
  @Consumes(MediaType.APPLICATION_XML)
  public Response postDirectory(Directory directory)
  {   
    try
    {
      initService(SERVICE_NAME);
      prepareProvider(description, baseUrl, assembly, "");
      
      if (useLdap)
      {
        DirectoryProvider directoryProvider = new DirectoryProvider(settings);        
        directoryProvider.postDirectory(directory, null);
      }
      else
      {
        DirectoryXMLProvider xmlProvider = new DirectoryXMLProvider(settings);
        xmlProvider.postDirectory(directory);
      }     
      
      return Response.ok().entity(directory).build();
    }
    catch(Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }
  
  @GET
  @Path("/directory/userldap")
  @Produces(MediaType.APPLICATION_XML)
  public Response userldap()
  {
  	try
    {
  		String ifUseLdap = "false";
      initService(SERVICE_NAME);
      getUserLdap();   
      
      if (useLdap)      
        ifUseLdap = "true";      
      
      return Response.ok().entity(ifUseLdap).build();
    }
    catch(Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }
  
  @GET
  @Path("/directory/security")
  @Produces(MediaType.APPLICATION_XML)
  public Response getSecurityRole()
  {
    try
    {
      initService(SERVICE_NAME);
      prepareProvider(description, baseUrl, assembly, "");
      
      String sercurityRole = "";
      
      if (useLdap)
      {
        LdapProvider ldapProvider = new LdapProvider(settings);
        String userId = getUserId();
        sercurityRole = ldapProvider.getSecurityRole(userId);
      }
      
      return Response.ok().entity(sercurityRole).build();
    }
    catch(Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }
  
  @GET
  @Path("/directory")
  @Produces(MediaType.APPLICATION_XML)
  public Response getDirectory()
  {
    try
    {
    	initService(SERVICE_NAME);
      getUserLdap();
      Directory directory = null;
      
      if (useLdap)
      {
      	LdapProvider ldapProvider = new LdapProvider(settings);
        String userId = getUserId();
        directory = ldapProvider.getDirectory(userId);
      }
      else
      {
        DirectoryXMLProvider xmlProvider = new DirectoryXMLProvider(settings);
        directory = xmlProvider.readDirectoryFileToXML();        
      }
      
      return Response.ok().entity(directory).build();
    }
    catch(Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }
  
  @GET
  @Path("/directory/resources")
  @Produces(MediaType.APPLICATION_XML)
  public Response getResources()
  {   
    try
    {
      initService(SERVICE_NAME);  
      getUserLdap();
      Resources resources = null;
      
      if (useLdap)
      {
        LdapProvider ldapProvider = new LdapProvider(settings);
        resources = ldapProvider.getResources();
      }
      else
      {
        DirectoryXMLProvider xmlProvider = new DirectoryXMLProvider(settings);
        resources = xmlProvider.getResources();
      }
      
      return Response.ok().entity(resources).build();
    }
    catch(Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }
  
  @POST
  @Path("/directory/resource")
  @Produces(MediaType.APPLICATION_XML)
  public Response getResource(String baseUrl)
  {   
    try
    {
      initService(SERVICE_NAME);
      prepareProvider(description, baseUrl, "", "");
      
      Resource resource = null;
      
      if (useLdap)
      {
        LdapProvider ldapProvider = new LdapProvider(settings);
        resource = ldapProvider.getResource(this.baseUrl);
      }
      else
      {
        DirectoryXMLProvider xmlProvider = new DirectoryXMLProvider(settings);
        resource = xmlProvider.getResource(this.baseUrl);
      }
      
      return Response.ok().entity(resource).build();
    }
    catch(Exception ex)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, ex);
    }
  }
  
  @GET
  @Path("/{scope}/exchanges/{exchangeId}")
  @Produces(MediaType.APPLICATION_XML)
  public Response getExchangeDefintion(@PathParam("scope") String scope, @PathParam("exchangeId") String exchangeId)
  {
    try
    {
      initService(SERVICE_NAME);

      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      ExchangeDefinition xdef = directoryProvider.getExchangeDefinition(scope, exchangeId);

      return Response.ok().entity(xdef).build();
    }
    catch (Exception e)
    {
      return prepareErrorResponse(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e);
    }
  }

  private void getUserLdap()
  {
  	/*sectionLoop: for (Section section : serviceConfig.getItems())
    {
      if (section.getName().equalsIgnoreCase("directory service"))
      {
        for (Setting setting : section.getItems())
        {
          if (setting.getName().equalsIgnoreCase("useLDAP"))
          {
            useLdap = Boolean.parseBoolean(setting.getValue());
            break sectionLoop;
          }
        }
      }
    }*/
  	useLdap = Boolean.parseBoolean(settings.get("useLDAP").toString());  	
  	
  }
  
  private void prepareProvider(String description, String baseUrl, String assembly, String context)
  {
  	getUserLdap();

    if (IOUtils.isNullOrEmpty(description))
      this.description = ".";
    else
      this.description = description;

    if (IOUtils.isNullOrEmpty(baseUrl))
      this.baseUrl = ".";
    else
      this.baseUrl = baseUrl.replace('.', '/');
    
    if (IOUtils.isNullOrEmpty(assembly))
      this.assembly = ".";
    else
      this.assembly = assembly;
    
    if (IOUtils.isNullOrEmpty(context))
      this.context = ".";
    else
      this.context = context;
  }

  private String getUserId()
  {
    for (Section section : serviceConfig.getItems())
    {
      if (section.getName().equalsIgnoreCase("global"))
      {
        for (Setting setting : section.getItems())
        {
          if (setting.getName().equalsIgnoreCase("userid-header"))
          {
            String userIdHeader = setting.getValue();
            Object userId = settings.get(Constants.HTTP_HEADER_PREFIX + userIdHeader);

            if (userId != null)
              return userId.toString();
          }
        }
      }
    }

    return "guest";
  }
}