package org.iringtools.security;

import java.io.IOException;
import java.net.URLEncoder;
import java.util.Enumeration;
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
  
  public static final String AUTH_COOKIE_NAME = "Auth";
  public static final int AUTH_COOKIE_EXPIRY = 28800;  // 8 hours
  public static final String AUTH_USER_KEY = "AuthenticatedUser";
  public static final String AUTH_TOKEN_NAME = "Authorization";
  public static final String USERID_KEY = "EMailAddress";
  public static final String REF_PARAM = "REF";
  public static final String URL_ENCODING = "UTF-8";
  
  private FilterConfig filterConfig;
  private HttpServletRequest request;
  private HttpSession session;
  private HttpServletResponse response; 
  
  @SuppressWarnings("unchecked")
  @Override
  public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain) throws IOException,
      ServletException
  {
    response = (HttpServletResponse) res;
    request = (HttpServletRequest) req;
    session = request.getSession(false);
        
    if (session == null)
    {
      session = request.getSession(true);
    }
    
    Cookie authCookie = HttpUtils.getCookie(request.getCookies(), AUTH_COOKIE_NAME);
    
    if (session.getAttribute(AUTH_USER_KEY) == null)
    {      
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
          
          response.setContentType("text/html");
          response.sendRedirect(ssoUrl);
          return;
        }
        else  // got reference ID, retrieve user info
        {
          String authenticationServiceUri = filterConfig.getInitParameter("authenticationServiceUri");
          String pingUserName = filterConfig.getInitParameter("pingUserName");
          String pingPassword = filterConfig.getInitParameter("pingPassword");
          String pingInstanceId = filterConfig.getInitParameter("pingInstanceId");
         
          HttpClient pingIdClient = new HttpClient(authenticationServiceUri + ref);
          pingIdClient.addHeader("ping.uname", pingUserName);
          pingIdClient.addHeader("ping.pwd", pingPassword);
          pingIdClient.addHeader("ping.instanceId", pingInstanceId);
          
          String userInfoJson = null;
          
          try
          {
            userInfoJson = pingIdClient.get(String.class);
          }
          catch (HttpClientException e)
          {
            logger.error(e);
          }
          
          if (userInfoJson == null)
          {
            session.invalidate();
            response.sendError(HttpServletResponse.SC_UNAUTHORIZED);
            return;
          }
          else 
          {
            Map<String, String> userInfo = null;
            
            try
            {
              userInfo = (Map<String, String>) JSONUtil.deserialize(userInfoJson);
            }
            catch (JSONException e)
            {
              logger.error("Error deserializing user info json: " + e);
              session.invalidate();
              response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR);
              return;
            }
            
            String userInfoStr = userInfo.toString();
            userInfoStr = userInfoStr.substring(1, userInfoStr.length() - 1);
            
            authCookie = new Cookie(AUTH_COOKIE_NAME, userInfoStr);
            authCookie.setMaxAge(AUTH_COOKIE_EXPIRY);
            response.addCookie(authCookie);
            
            obtainOAuthToken(userInfoJson);
          }
        }
      }
      else  // user signed on but session has not been validated
      {
        Map<String, String> userInfo = HttpUtils.getCookieAttributes(authCookie.getValue());
        session.setAttribute(AUTH_USER_KEY, userInfo.get(USERID_KEY));
        
        try
        {
          String userInfoJson = JSONUtil.serialize(userInfo);
          obtainOAuthToken(userInfoJson);
        }
        catch (JSONException e)
        {
          logger.error("Error serializing user info: " + e);
          session.invalidate();
          response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR);
          return;
        }        
      }
    }
    else
    {
      Map<String, String> userInfo = HttpUtils.getCookieAttributes(authCookie.getValue());
      
      try
      {
        String userInfoJson = JSONUtil.serialize(userInfo);
        obtainOAuthToken(userInfoJson);
      }
      catch (JSONException e)
      {
        logger.error("Error serializing user info: " + e);
        session.invalidate();
        response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR);
        return;
      }
    }

    chain.doFilter(request, response); 
  }

  @Override
  public void init(FilterConfig filterConfig) throws ServletException
  {
    this.filterConfig = filterConfig;
  }
  
  @Override
  public void destroy(){}
  
  private void obtainOAuthToken(String userInfoJson) throws IOException
  {
    String tokenServiceUri = filterConfig.getInitParameter("tokenServiceUri");
    String applicationKey = filterConfig.getInitParameter("applicationKey");
    
    if (session.getAttribute(AUTH_TOKEN_NAME) != null)
    {
      response.addHeader(AUTH_TOKEN_NAME, (String)session.getAttribute(AUTH_TOKEN_NAME));      
    }
    else if (request.getHeader(AUTH_TOKEN_NAME) == null &&
        !IOUtils.isNullOrEmpty(tokenServiceUri) && !IOUtils.isNullOrEmpty(applicationKey))
    {
      HttpClient apigeeClient = new HttpClient(tokenServiceUri + applicationKey);
      
      try
      {
        byte[] data = userInfoJson.getBytes("utf8");
        String apigeeResponse = apigeeClient.postByteData(String.class, "", data);
        
        @SuppressWarnings("unchecked")
        Map<String, Map<String, String>> apigeeResponseObj = (Map<String, Map<String, String>>) JSONUtil.deserialize(apigeeResponse);
        Map<String, String> accessToken = apigeeResponseObj.get("accesstoken");  
        
        String oAuthToken = accessToken.get("token");
        session.setAttribute(AUTH_TOKEN_NAME, oAuthToken);
        response.addHeader(AUTH_TOKEN_NAME, oAuthToken);            
      }
      catch (Exception ex)
      {
        logger.error("Error obtaining OAuth token from Apigee: " + ex);
        session.invalidate();
        response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR);
      }
    }
  }
  
  public void logHeaders(HttpServletRequest request)
  {
    logger.debug("HEADERS:");
    Enumeration<?> headers = request.getHeaderNames();
    
    while (headers != null && headers.hasMoreElements())
    {
      String name = (String)headers.nextElement();
      String value = request.getHeader(name);
      logger.debug(name + ": " + value);
    }
  } 
}
