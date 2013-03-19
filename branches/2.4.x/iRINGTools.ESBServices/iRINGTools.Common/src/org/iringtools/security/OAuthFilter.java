package org.iringtools.security;

import java.io.IOException;
import java.net.URLEncoder;
import java.util.Map;

import javax.servlet.Filter;
import javax.servlet.FilterChain;
import javax.servlet.FilterConfig;
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
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;

public class OAuthFilter implements Filter 
{
  private static final Logger logger = Logger.getLogger(OAuthFilter.class);
  
  public static final int AUTH_COOKIE_EXPIRY = 28800;  // 8 hours
  public static final String REF_PARAM = "REF";
  public static final String URL_ENCODING = "UTF-8";
  public static final String OAUTH_TOKEN = "OAuthToken";
  public static final String AUTHENTICATED_USER = "Auth";
  public static final String AUTHORIZATION = "http-header-Authorization";
  public static final String APP_KEY = "http-header-X-myPSN-AppKey";
  public static final String USER_ID = "http-header-X-myPSN-UserId";
  
  private FilterConfig filterConfig;
  private HttpServletRequest request;
  private HttpServletResponse response; 
  private HttpSession session;
  
  @SuppressWarnings("unchecked")
  @Override
  public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain) throws IOException,
      ServletException
  {
    response = (HttpServletResponse) res;
    request = (HttpServletRequest) req;
    session = request.getSession();
    
    String appKey = filterConfig.getInitParameter("applicationKey");
    session.setAttribute(APP_KEY, appKey);    
    
    HttpUtils.prepareHttpProxy(filterConfig.getServletContext());    
    Cookie authCookie = HttpUtils.getCookie(request.getCookies(), AUTHENTICATED_USER);
    
    if (authCookie == null || IOUtils.isNullOrEmpty(authCookie.getValue()))
    {
      String ref = request.getParameter(REF_PARAM);
      authCookie.setMaxAge(AUTH_COOKIE_EXPIRY);
      
      if (IOUtils.isNullOrEmpty(ref))  // case 1: user needs to login
      {
        logger.debug("Process login");
        
        String federationServiceUri = filterConfig.getInitParameter("federationServiceUri");
        String idpId = filterConfig.getInitParameter("idpId");
        String spFederationPath = filterConfig.getInitParameter("spFederationPath");          
        
        String returnPath = request.getRequestURL().toString();
        logger.debug("Return Path: " + returnPath);
        
        String ssoUrl = federationServiceUri + spFederationPath + "?PartnerIdpId=" + 
          idpId + "&TargetResource=" + URLEncoder.encode(returnPath, URL_ENCODING);
        
        logger.debug("Requesting REF ID: " + ssoUrl);
        
        response.setContentType("text/html");
        response.sendRedirect(ssoUrl);
        return;
      }
      else  // case 2: the user has logged in but the application needs to process the SSO event
      {
        logger.debug("Get user info");
        
        String authenticationServiceUri = filterConfig.getInitParameter("authenticationServiceUri");
        String pingUserName = filterConfig.getInitParameter("pingUserName");
        String pingPassword = filterConfig.getInitParameter("pingPassword");
        String pingInstanceId = filterConfig.getInitParameter("pingInstanceId");
       
        String authServiceUrl = authenticationServiceUri + ref;
        HttpClient pingIdClient = new HttpClient(authServiceUrl);
        pingIdClient.addHeader("ping.uname", pingUserName);
        pingIdClient.addHeader("ping.pwd", pingPassword);
        pingIdClient.addHeader("ping.instanceId", pingInstanceId);
        
        logger.debug("Requesting identity: " + authServiceUrl);
        
        String userJson = null;
        
        try
        {
          userJson = pingIdClient.get(String.class);
          logger.debug("User JSON: " + userJson);
        }
        catch (HttpClientException e)
        {
          logger.error(e);
        }
        
        if (userJson == null)
        {
          response.sendError(HttpServletResponse.SC_UNAUTHORIZED);
        }
        else 
        {  
          Map<String, String> userAttrs = null;
          
          try
          {
            userAttrs = (Map<String, String>) JSONUtil.deserialize(userJson);
            
            String authCookieValue = HttpUtils.toQueryParams(userAttrs);          
            authCookie = new Cookie(AUTHENTICATED_USER, authCookieValue);
            authCookie.setMaxAge(AUTH_COOKIE_EXPIRY);
            response.addCookie(authCookie);
            
            session.setAttribute(AUTHORIZATION, userAttrs.get(OAUTH_TOKEN)); 
            session.setAttribute(USER_ID, getUserId(userAttrs));
          }
          catch (JSONException e)
          {
            String message = "Error deserializing user info json: " + e;
            logger.error(message);
            response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, message);
          }
          
          chain.doFilter(req, res);
          return;
        }
      }
    }
    else  // case 3: the user has already logged in and the application has already processed the SSO event
    {
      String authCookieMultiValue = authCookie.getValue();      
      logger.debug("User info [" + authCookieMultiValue + "]");
      
      Map<String, String> userAttrs = HttpUtils.fromQueryParams(authCookieMultiValue);
            
      session.setAttribute(AUTHORIZATION, userAttrs.get(OAUTH_TOKEN));
      session.setAttribute(USER_ID, getUserId(userAttrs));
      
      chain.doFilter(req, res);
      return;
    }
  }

  @Override
  public void init(FilterConfig filterConfig) throws ServletException
  {
    this.filterConfig = filterConfig;
  }
  
  @Override
  public void destroy(){}
  
  private String getUserId(Map<String, String> userAttrs) throws IOException
  {
    if (userAttrs.containsKey("BechtelUserName"))
      return userAttrs.get("BechtelUserName");
          
    return userAttrs.get("EMailAddress");
  }
}
