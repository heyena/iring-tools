package org.iringtools.filters;

import java.io.FileInputStream;
import java.io.IOException;
import java.net.URLEncoder;
import java.util.Arrays;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.directory.DirContext;
import javax.naming.directory.InitialDirContext;
import javax.naming.directory.SearchControls;
import javax.naming.directory.SearchResult;
import javax.servlet.Filter;
import javax.servlet.FilterChain;
import javax.servlet.FilterConfig;
import javax.servlet.ServletContext;
import javax.servlet.ServletException;
import javax.servlet.ServletRequest;
import javax.servlet.ServletResponse;
import javax.servlet.http.Cookie;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

import org.apache.log4j.Logger;
import org.apache.struts2.json.JSONException;
import org.apache.struts2.json.JSONUtil;
import org.iringtools.utility.EncryptionUtils;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.IOUtils;

public class OAuthFilter implements Filter 
{
  private static final Logger logger = Logger.getLogger(OAuthFilter.class);
  private static final String AUTH_COOKIE = "Auth";
  private static final int AUTH_COOKIE_EXPIRY = 28800;  // 8 hours
  private static final String AUTH_USER = "authenticatedUser";
  private static final String USERID_ATTR = "subject";
  private static final String REF_PARAM = "REF";
  private static final String URL_ENCODING = "UTF-8";
  
  private FilterConfig filterConfig;

  @Override
  public void doFilter(ServletRequest req, ServletResponse res, FilterChain filterChain) throws IOException,
      ServletException
  {
    HttpServletResponse response = (HttpServletResponse) res;
    HttpServletRequest request = (HttpServletRequest) req;
    HttpSession session = request.getSession();
        
    logHeaders(request);
    
    if (session.getAttribute(AUTH_USER) == null)
    {
      Cookie authCookie = getCookie(request.getCookies(), AUTH_COOKIE);
      
      if (authCookie == null || IOUtils.isNullOrEmpty(authCookie.getValue()))  // use has not signed on
      {
        String ref = request.getParameter(REF_PARAM);
        
        if (IOUtils.isNullOrEmpty(ref))  // no reference token, attempt to obtain one
        {
          String federationServiceUri = filterConfig.getInitParameter("federationServiceUri");
          String idpId = filterConfig.getInitParameter("idpId");
          String spFederationPath = filterConfig.getInitParameter("spFederationPath");          
          String returnPath = request.getRequestURL().toString();          
          
          String ssoUrl = federationServiceUri + spFederationPath + "?PartnerIdpId=" + 
            idpId + "&TargetResource=" + URLEncoder.encode(returnPath, URL_ENCODING);
          
          response.sendRedirect(ssoUrl);
        }
        else  // got reference ID, retrieve user info
        {
          String refResolverUri = filterConfig.getInitParameter("refResolverUri");
          String pingUserName = filterConfig.getInitParameter("pingUserName");
          String pingPassword = filterConfig.getInitParameter("pingPassword");
          String pingInstanceId = filterConfig.getInitParameter("pingInstanceId");
         
          HttpClient client = new HttpClient(refResolverUri + ref);
          client.addHeader("ping.uname", pingUserName);
          client.addHeader("ping.pwd", pingPassword);
          client.addHeader("ping.instanceId", pingInstanceId);
          
          String userInfoJson = null;
          
          try
          {
            userInfoJson = client.get(String.class);
          }
          catch (HttpClientException e)
          {
            logger.error(e);
          }
          
          if (userInfoJson == null)
          {
            response.sendError(HttpServletResponse.SC_UNAUTHORIZED);
          }
          else 
          {
            try
            {
              @SuppressWarnings("unchecked")
              Map<String, String> userInfo = (Map<String, String>) JSONUtil.deserialize(userInfoJson);
              String userInfoStr = userInfo.toString();
              userInfoStr = userInfoStr.substring(1, userInfoStr.length() - 1);
              
              authCookie = new Cookie(AUTH_COOKIE, userInfoStr);
              authCookie.setMaxAge(AUTH_COOKIE_EXPIRY);
              response.addCookie(authCookie);
              
              if (!isAuthorized(userInfo.get(USERID_ATTR)))
              {
                prepareErrorResponse(session, response);
                return;
              }
              else
              {
                session.setAttribute(AUTH_USER, USERID_ATTR);
              }
            }
            catch (JSONException e)
            {
              logger.error("Error deserializing user info json: " + e);
            }
          }
        }
      }
      else  // user signed on but session has not been validated
      {
        Map<String, String> userInfo = getCookieAttributes(authCookie.getValue());
        
        if (!isAuthorized(userInfo.get(USERID_ATTR)))
        {
          prepareErrorResponse(session, response);
          return;
        }
        else
        {
          session.setAttribute(AUTH_USER, USERID_ATTR);
        }
      }
    }

    filterChain.doFilter(request, response); 
  }

  @Override
  public void init(FilterConfig filterConfig) throws ServletException
  {
    this.filterConfig = filterConfig;
  }
  
  @Override
  public void destroy(){}
  
  private void prepareErrorResponse(HttpSession session, HttpServletResponse response) throws IOException
  {
    session.invalidate();
    response.setContentType("text/html");
    response.sendError(HttpServletResponse.SC_UNAUTHORIZED, "Unauthorized");
  }
  
  private boolean isAuthorized(String userId)
  {
    ServletContext context = filterConfig.getServletContext();
    String path = context.getRealPath("/") + "WEB-INF/config/ldap.properties";    
    Properties ldapProps = new Properties();
    
    try
    {
      ldapProps.load(new FileInputStream(path));      
      String credsProp = "java.naming.security.credentials";
      String password = EncryptionUtils.decrypt(ldapProps.getProperty(credsProp));
      ldapProps.put(credsProp, password);
    }
    catch (Exception ioe)
    {
      logger.error("Error loading ldap properties: " + ioe);
    }

    if (ldapProps.size() > 0)
    {
      DirContext dctx = null;

      try
      {
        dctx = new InitialDirContext(ldapProps);
      }
      catch (Exception e)
      {
        logger.error("Error initializating directory context: " + e);
      }

      if (dctx != null)
      {
        String baseDN = ldapProps.getProperty("baseDN");
        if (baseDN == null || baseDN.length() == 0)
        {
          baseDN = "ou=iringtools,dc=iringug,dc=org";
        }
                
        String authorizedGroupsProp = ldapProps.getProperty("authorizedGroups");
        List<String> authorizedGroups = null;
        
        if (!IOUtils.isNullOrEmpty(authorizedGroupsProp))
        {
          authorizedGroups = Arrays.asList(authorizedGroupsProp.split(" *, *"));          
        }

        String qualUserId = "uid=" + userId + ",cn=users," + baseDN;
        String filter = "(&(objectClass=groupOfEntries)(member=" + qualUserId + "))";
        
        SearchControls constraints = new SearchControls();
        constraints.setSearchScope(SearchControls.SUBTREE_SCOPE);
        
        try
        {
          NamingEnumeration<?> results = dctx.search(baseDN, filter, constraints);
          
          while (results != null && results.hasMore())
          {
            SearchResult result = (SearchResult) results.next(); 
            
            String group = result.getName();            
            group = group.substring(0, group.indexOf(','));
            group = group.substring(group.indexOf('=') + 1);
            
            if (authorizedGroups != null && authorizedGroups.contains(group))
            {
              return true;
            }
          }
        }
        catch (NamingException ex)
        {
          logger.error("Error getting user information: " + userId);
        }
      }
    }

    return false;
  }
  
  public Cookie getCookie(Cookie[] cookies, String cookieName)
  {   
    if (cookies != null && cookies.length > 0)
    {
      for (int i=0; i<cookies.length; i++) 
      {
        Cookie cookie = cookies[i];
        
        if (cookie.getName().equalsIgnoreCase(cookieName))
          return cookie;
      }
    }
    
    return null;
  }
  
  private Map<String, String> getCookieAttributes(String cookieValue)
  {
    Map<String, String> attrs = new HashMap<String, String>();
    
    if (!IOUtils.isNullOrEmpty(cookieValue))
    {
      String[] pairs = cookieValue.split(" *, *");
      
      for (String pair : pairs)
      {
        String[] parts = pair.split("=");
        attrs.put(parts[0], parts[1]);
      }
    }
    
    return attrs;
  }
  
  private void logHeaders(HttpServletRequest request)
  {
    logger.debug("HEADERS:");
    Enumeration<?> headers = request.getHeaderNames();
    
    while (headers.hasMoreElements())
    {
      String name = (String)headers.nextElement();
      String value = request.getHeader(name);
      logger.debug(name + ": " + value);
    }
  }
}
