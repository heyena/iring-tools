package org.iringtools.adapter.services;

import java.io.FileInputStream;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import javax.naming.NamingException;
import javax.servlet.ServletContext;
import javax.servlet.http.Cookie;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.core.SecurityContext;

import org.apache.cxf.jaxrs.ext.MessageContext;
import org.iringtools.security.AuthorizationException;
import org.iringtools.security.LdapAuthorizationProvider;
import org.iringtools.security.OAuthFilter;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;

public abstract class AbstractService
{
  @Context protected ServletContext servletContext; 
  @Context protected MessageContext messageContext; 
  @Context protected SecurityContext securityContext;
  
  private String serviceType;
  private String authUser;
  
  protected Map<String, Object> settings;
  protected HttpServletRequest httpRequest;
  protected HttpServletResponse httpResponse;   
  protected HttpSession httpSession;
  
  public void initService(String serviceType) throws AuthorizationException
  {
    this.serviceType = serviceType;
    
    settings = java.util.Collections.synchronizedMap(new HashMap<String, Object>());    
    httpRequest = messageContext.getHttpServletRequest();
    httpResponse = messageContext.getHttpServletResponse();   
    httpSession = httpRequest.getSession();
    
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
    MultivaluedMap<String, String> headers = messageContext.getHttpHeaders().getRequestHeaders();
    
    List<String> authHeader = headers.get(OAuthFilter.AUTHENTICATED_USER_KEY);    
    if (authHeader != null && authHeader.size() > 0)
    {
      authUser = authHeader.get(0);
      settings.put(OAuthFilter.AUTHENTICATED_USER_KEY, authUser);
    }
    
    List<String> tokenHeader = headers.get(OAuthFilter.AUTHORIZATION_TOKEN_KEY); 
    if (tokenHeader != null && tokenHeader.size() > 0)
    {
      settings.put(OAuthFilter.AUTHORIZATION_TOKEN_KEY, tokenHeader.get(0));
    }
    
    processAuthorization();    
    
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
  
  protected void processAuthorization() throws AuthorizationException
  {
    String authorizationEnabled = servletContext.getInitParameter("authorizationEnabled");
    
    if (!IOUtils.isNullOrEmpty(authorizationEnabled) && authorizationEnabled.equalsIgnoreCase("true"))
    {    
      String ldapConfigPath = servletContext.getRealPath("/") + "WEB-INF/config/ldap.conf";
      String authorizedGroup = serviceType + "Admins";
  
      Cookie[] cookies = httpRequest.getCookies();
      Cookie authorizedCookie = HttpUtils.getCookie(cookies, authorizedGroup);
  
      if (authorizedCookie == null)  // user not authorized, attempt to authorize
      {
        Map<String, String> userAttrs = null;
        
        // get user attributes
        try
        {
          userAttrs = HttpUtils.fromQueryParams(authUser);
        }
        catch (Exception e)
        {
          Cookie authUserCookie = HttpUtils.getCookie(cookies, OAuthFilter.AUTHENTICATED_USER_KEY);
          
          try
          {
            userAttrs = HttpUtils.fromQueryParams(authUserCookie.getValue());
          }
          catch (UnsupportedEncodingException ue)
          {
            throw new AuthorizationException("Error deserializing Auth cookie: " + ue);
          }
        }
        
        if (userAttrs != null && userAttrs.size() > 0)
        {
          Properties props = new Properties();
          
          try
          {
            props.load(new FileInputStream(ldapConfigPath));
          }
          catch (IOException ioe)
          {
            throw new AuthorizationException("Error loading LDAP properties: " + ioe);
          }
          
          props.put("authorizedGroup", authorizedGroup);
          
          LdapAuthorizationProvider authProvider = new LdapAuthorizationProvider();
          
          try
          {
            authProvider.init(props);
          }
          catch (NamingException ne)
          {
            throw new AuthorizationException("Error initializing authentication provider: " + ne);
          }
          
          if (userAttrs == null || !authProvider.isAuthorized(userAttrs))
          {
            throw new AuthorizationException("User not authorized");
          }
          else
          {
            httpResponse.addCookie(new Cookie(authorizedGroup, "authorized"));
          }
        }
        else
        {
          throw new AuthorizationException("Invalid identity.");
        }
      }
    }
  }
  
  protected void prepareErrorResponse(int errorCode, Exception e)
  {
    prepareErrorResponse(errorCode, e.toString());
  }
  
  protected void prepareErrorResponse(int errorCode, String errorMessage)
  {
    httpResponse.setContentType(MediaType.TEXT_XML);
    
    try
    {
      httpResponse.sendError(errorCode, errorMessage);
    }
    catch (IOException e)
    {
      e.printStackTrace();
    }
  }
}
