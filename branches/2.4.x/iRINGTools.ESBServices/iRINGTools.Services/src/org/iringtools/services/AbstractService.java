package org.iringtools.services;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

import javax.servlet.ServletContext;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.SecurityContext;

import org.apache.cxf.jaxrs.ext.MessageContext;
import org.apache.log4j.Logger;
import org.iringtools.security.AuthorizationException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;

public abstract class AbstractService
{
  private static final Logger logger = Logger.getLogger(AbstractService.class);
  
  @Context protected ServletContext servletContext; 
  @Context protected MessageContext messageContext; 
  @Context protected SecurityContext securityContext;
  
  protected Map<String, Object> settings;
  protected HttpServletRequest request;
  protected HttpServletResponse response;
  
  public void initService(String serviceName) throws AuthorizationException
  {
    logger.info("Initializing " + serviceName);
    
    settings = java.util.Collections.synchronizedMap(new HashMap<String, Object>());    
    request = messageContext.getHttpServletRequest();
    response = messageContext.getHttpServletResponse(); 
    
    HttpUtils.prepareHttpProxy(servletContext);
    
    /*
     * PREPARE COMMON SETTINGS
     */
    settings.put("baseDirectory", servletContext.getRealPath("/"));

    String directoryServiceUri = servletContext.getInitParameter("directoryServiceUri");
    if (directoryServiceUri == null || directoryServiceUri.equals(""))
      directoryServiceUri = "http://localhost:8080/services/dir";
    settings.put("directoryServiceUri", directoryServiceUri);

    String differencingServiceUri = servletContext.getInitParameter("differencingServiceUri");
    if (differencingServiceUri == null || differencingServiceUri.equals(""))
      differencingServiceUri = "http://localhost:8080/services/diff";
    settings.put("differencingServiceUri", differencingServiceUri);

    String idGenServiceUri = servletContext.getInitParameter("IDGenServiceUri");
    if (idGenServiceUri == null || idGenServiceUri.equals(""))
    	idGenServiceUri = "http://localhost:8080/services/idgen";
    settings.put("idGenServiceUri", idGenServiceUri);
    
    /*
     * PREPARE REFERENCE DATA SETTINGS
     */    	
    /*String exampleRegistryBase = servletContext.getInitParameter("ExampleRegistryBase");
    if (exampleRegistryBase == null || exampleRegistryBase.equals(""))
    	exampleRegistryBase = "http://example.org/data#";
    settings.put("ExampleRegistryBase", exampleRegistryBase);

    String templateRegistryBase = servletContext.getInitParameter("TemplateRegistryBase");
    if (templateRegistryBase == null || templateRegistryBase.equals(""))
    	templateRegistryBase = "http://tpl.rdlfacade.org/data#";
    settings.put("TemplateRegistryBase", templateRegistryBase);

    String classRegistryBase = servletContext.getInitParameter("ClassRegistryBase");
    if (classRegistryBase == null || classRegistryBase.equals(""))
    	classRegistryBase = "http://rdl.rdlfacade.org/data#";
    settings.put("ClassRegistryBase", classRegistryBase);

    String useExampleRegistryBase = servletContext.getInitParameter("UseExampleRegistryBase");
    if (useExampleRegistryBase == null || useExampleRegistryBase.equals(""))
    	useExampleRegistryBase = "false";
    settings.put("UseExampleRegistryBase", useExampleRegistryBase);*/
        
    /*
     * PREPARE EXCHANGE SETTINGS
     */
    String poolSize = servletContext.getInitParameter("poolSize");
    if (poolSize == null || poolSize.equals(""))
      poolSize = "100";
    settings.put("poolSize", poolSize);
    
    String numOfExchangeLogFiles = servletContext.getInitParameter("numOfExchangeLogFiles");
    if (numOfExchangeLogFiles == null || numOfExchangeLogFiles.equals(""))
      numOfExchangeLogFiles = "10";
    settings.put("numOfExchangeLogFiles", numOfExchangeLogFiles);
    
    String manifestTaskTimeout = servletContext.getInitParameter("manifestTaskTimeout");
    if (manifestTaskTimeout == null || manifestTaskTimeout.equals(""))
      manifestTaskTimeout = "300";  // in seconds
    settings.put("manifestTaskTimeout", manifestTaskTimeout);
    
    String dtiTaskTimeout = servletContext.getInitParameter("dtiTaskTimeout");
    if (dtiTaskTimeout == null || dtiTaskTimeout.equals(""))
      dtiTaskTimeout = "1800";  // in seconds
    settings.put("dtiTaskTimeout", dtiTaskTimeout);
    
    String dtoTaskTimeout = servletContext.getInitParameter("dtoTaskTimeout");
    if (dtoTaskTimeout == null || dtoTaskTimeout.equals(""))
      dtoTaskTimeout = "600";  // in seconds
    settings.put("dtoTaskTimeout", dtoTaskTimeout);
    
    String asyncTimeout = servletContext.getInitParameter("AsyncTimeout");    
    settings.put("asyncTimeout", IOUtils.isNullOrEmpty(asyncTimeout) ? 1800 : Long.valueOf(asyncTimeout));
   
    String pollingInterval = servletContext.getInitParameter("PollingInterval");    
    settings.put("pollingInterval", IOUtils.isNullOrEmpty(pollingInterval) ? 2 : Long.valueOf(pollingInterval));
    
    /*
     * CARRY ON REQUEST HEADERS
     */    
    MultivaluedMap<String, String> headers = messageContext.getHttpHeaders().getRequestHeaders();
    
    for (Entry<String, List<String>> header : headers.entrySet())
    {
      List<String> values = header.getValue();
      
      if (values != null && values.size() > 0)
      {
        settings.put("http-header-" + header.getKey(), values.get(0));
      }
    }
  }
  
  protected Response prepareErrorResponse(int errorCode, Exception e)
  {
    logger.error(e.getMessage());
    logger.error(IOUtils.getStackTrace(e));
    return prepareErrorResponse(errorCode, e.getMessage());
  }
  
  protected Response prepareErrorResponse(int errorCode, String errorMessage)
  {
    return Response.status(errorCode).type(MediaType.TEXT_PLAIN).entity(errorMessage).build();
  }
}
