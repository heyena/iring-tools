package org.iringtools.services;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import javax.servlet.ServletContext;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.SecurityContext;

import org.apache.cxf.jaxrs.ext.MessageContext;
import org.iringtools.security.AuthorizationException;
import org.iringtools.security.OAuthFilter;
import org.iringtools.utility.EncryptionException;
import org.iringtools.utility.EncryptionUtils;

public abstract class AbstractService
{
  @Context protected ServletContext servletContext; 
  @Context protected MessageContext messageContext; 
  @Context protected SecurityContext securityContext;
  
  protected Map<String, Object> settings;
  protected HttpServletRequest request;
  protected HttpServletResponse response;   
  protected HttpSession session;
  
  public void initService(String serviceType) throws AuthorizationException
  {
    settings = java.util.Collections.synchronizedMap(new HashMap<String, Object>());    
    request = messageContext.getHttpServletRequest();
    response = messageContext.getHttpServletResponse();   
    session = request.getSession();
    
    /*
     * COMMON SETTINGS
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
     * REQUEST HEADERS
     */    
    MultivaluedMap<String, String> headers = messageContext.getHttpHeaders().getRequestHeaders();
    
    List<String> authorizationHeader = headers.get(OAuthFilter.AUTHORIZATION); 
    if (authorizationHeader != null && authorizationHeader.size() > 0)
    {
      settings.put(OAuthFilter.AUTHORIZATION, authorizationHeader.get(0));
    }   
    
    List<String> appKeyHeader = headers.get(OAuthFilter.APP_KEY);    
    if (appKeyHeader != null && appKeyHeader.size() > 0)
    {
      settings.put(OAuthFilter.APP_KEY, appKeyHeader.get(0));
    }
    
    /*
     * REFERENCE DATA SETTINGS
     */    	
    String exampleRegistryBase = servletContext.getInitParameter("ExampleRegistryBase");
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
    settings.put("UseExampleRegistryBase", useExampleRegistryBase);
        
    /*
     * EXCHANGE SETTINGS
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
    
    /* 
     * PROXY SETTINGS
     */
    Properties sysProps = System.getProperties();
    boolean proxyInfoValid = true;
    
    String proxyHost = servletContext.getInitParameter("proxyHost");
    if (proxyHost != null && proxyHost.length() > 0)
      sysProps.put("http.proxyHost", proxyHost);
    else
      proxyInfoValid = false;
    
    String proxyPort = servletContext.getInitParameter("proxyPort");
    if (proxyPort != null && proxyPort.length() > 0)
      sysProps.put("http.proxyPort", proxyPort);
    else
      proxyInfoValid = false;
    
    String proxyUserName = servletContext.getInitParameter("proxyUserName");
    if (proxyUserName != null && proxyUserName.length() > 0)
      sysProps.put("http.proxyUserName", proxyUserName);
    else
      proxyInfoValid = false;
    
    String encryptedProxyPassword = servletContext.getInitParameter("proxyPassword");
    if (encryptedProxyPassword != null && encryptedProxyPassword.length() > 0)
    {
      String proxyKeyFile = servletContext.getInitParameter("proxySecretKeyFile");
      
      try
      {
        String proxyPassword = (proxyKeyFile != null && proxyKeyFile.length() > 0)
          ? EncryptionUtils.decrypt(encryptedProxyPassword, proxyKeyFile)
          : EncryptionUtils.decrypt(encryptedProxyPassword);
          
        sysProps.put("http.proxyPassword", proxyPassword);
      }
      catch (EncryptionException e)
      {
        proxyInfoValid = false;
      }      
    }
    else
    {
      proxyInfoValid = false;
    }
    
    String proxyDomain = servletContext.getInitParameter("proxyDomain");
    if (proxyDomain == null)
      proxyDomain = "";
    sysProps.put("http.proxyDomain", proxyDomain);
    
    if (proxyInfoValid)
    {
      sysProps.put("proxySet", "true");      
    }
  }
  
  protected Response prepareErrorResponse(int errorCode, Exception e)
  {
    return prepareErrorResponse(errorCode, e.getMessage());
  }
  
  protected Response prepareErrorResponse(int errorCode, String errorMessage)
  {
    return Response.status(errorCode).type(MediaType.TEXT_PLAIN).entity(errorMessage).build();
  }
}
