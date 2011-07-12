package org.iringtools.services;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import javax.servlet.ServletContext;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.core.SecurityContext;

import org.apache.cxf.jaxrs.ext.MessageContext;
import org.iringtools.security.OAuthFilter;

public abstract class AbstractService
{
  @Context protected ServletContext servletContext; 
  @Context protected MessageContext messageContext; 
  @Context protected SecurityContext securityContext;
  
  protected Map<String, Object> settings;
  
  public void initService()
  {
    settings = java.util.Collections.synchronizedMap(new HashMap<String, Object>());
    
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
     * OAUTH SETTINGS
     */    
    //TODO: process authorization
    MultivaluedMap<String, String> headers = messageContext.getHttpHeaders().getRequestHeaders();
    
    List<String> authenticatedUser = headers.get(OAuthFilter.AUTHENTICATED_USER_KEY);    
    if (authenticatedUser != null && authenticatedUser.size() > 0)
    {
      settings.put(OAuthFilter.AUTHENTICATED_USER_KEY, authenticatedUser.get(0));
    }
    
    List<String> authorization = headers.get(OAuthFilter.AUTHORIZATION_TOKEN_KEY); 
    if (authorization != null && authorization.size() > 0)
    {
      settings.put(OAuthFilter.AUTHORIZATION_TOKEN_KEY, authorization.get(0));
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
    
    String proxyPassword = servletContext.getInitParameter("proxyPassword");
    if (proxyPassword != null && proxyPassword.length() > 0)
      sysProps.put("http.proxyPassword", proxyPassword);
    else
      proxyInfoValid = false;
    
    String proxyDomain = servletContext.getInitParameter("proxyDomain");
    if (proxyDomain == null)
      proxyDomain = "";
    sysProps.put("http.proxyDomain", proxyDomain);
    
    if (proxyInfoValid)
      sysProps.put("proxySet", "true");
    
    /*
     * LDAP SETTINGS
     */    
    String ldapConfigPath = servletContext.getInitParameter("ldapConfigPath");
    if (ldapConfigPath != null && ldapConfigPath.length() > 0)
      settings.put("ldapConfigPath", ldapConfigPath);
    else
      settings.put("ldapConfigPath", "WEB-INF/config/ldap.conf");
  }
}
