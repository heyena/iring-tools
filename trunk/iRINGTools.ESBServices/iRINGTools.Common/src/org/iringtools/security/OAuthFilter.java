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
  public static final String AUTHENTICATED_USER_KEY = "authenticated-user";
  public static final String AUTHORIZATION_TOKEN_KEY = "Authorization";
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
    session = request.getSession();
    
    Cookie authCookie = HttpUtils.getCookie(request.getCookies(), AUTHENTICATED_USER_KEY);
    
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
      }
      else  // got reference ID, get user info
      {
        String authenticationServiceUri = filterConfig.getInitParameter("authenticationServiceUri");
        String pingUserName = filterConfig.getInitParameter("pingUserName");
        String pingPassword = filterConfig.getInitParameter("pingPassword");
        String pingInstanceId = filterConfig.getInitParameter("pingInstanceId");
       
        HttpClient pingIdClient = new HttpClient(authenticationServiceUri + ref);
        pingIdClient.addHeader("ping.uname", pingUserName);
        pingIdClient.addHeader("ping.pwd", pingPassword);
        pingIdClient.addHeader("ping.instanceId", pingInstanceId);
        
        String userAttrsJson = null;
        
        try
        {
          userAttrsJson = pingIdClient.get(String.class);
        }
        catch (HttpClientException e)
        {
          logger.error(e);
        }
        
        if (userAttrsJson == null)
        {
          response.sendError(HttpServletResponse.SC_UNAUTHORIZED);
        }
        else 
        {
          Map<String, String> userAttrs = null;
          
          try
          {
            userAttrs = (Map<String, String>) JSONUtil.deserialize(userAttrsJson);
          }
          catch (JSONException e)
          {
            String message = "Error deserializing user info json: " + e;
            logger.error(message);
            response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, message);
          }
          
          if (userAttrs != null)
          {
            String multiValue = userAttrs.toString();
            multiValue = multiValue.substring(1, multiValue.length() - 1);
            
            // make authenticated user attributes available in both session and cookie
            session.setAttribute(AUTHENTICATED_USER_KEY, multiValue);
            
            authCookie = new Cookie(AUTHENTICATED_USER_KEY, multiValue);
            authCookie.setMaxAge(AUTH_COOKIE_EXPIRY);
            response.addCookie(authCookie);
            
            if (!obtainOAuthToken(userAttrsJson))
            {
              String message = "Unable to obtain OAuth token.";
              response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, message);
            }
            else
            {
              chain.doFilter(req, res); 
            }
          }
        }
      }
    }
    else  // user signed on but session has not been validated
    {
      Map<String, String> userAttrs = HttpUtils.toMap(authCookie.getValue());
      
      try
      {
        String multiValue = userAttrs.toString();
        multiValue = multiValue.substring(1, multiValue.length() - 1);
        session.setAttribute(AUTHENTICATED_USER_KEY, multiValue);
        
        String userAttrsJson = JSONUtil.serialize(userAttrs);
        
        if (!obtainOAuthToken(userAttrsJson))
        {
          String message = "Unable to obtain OAuth token.";
          response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, message);
        }
        else
        {
          chain.doFilter(req, res);
        }
      }
      catch (JSONException e)
      {
        String message = "Error serializing user info: " + e;
        logger.error(message);
        response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, message);
      }        
    }
  }

  @Override
  public void init(FilterConfig filterConfig) throws ServletException
  {
    this.filterConfig = filterConfig;
  }
  
  @Override
  public void destroy(){}
  
  private boolean obtainOAuthToken(String userAttrsJson) throws IOException
  {
    String tokenServiceUri = filterConfig.getInitParameter("tokenServiceUri");
    String applicationKey = filterConfig.getInitParameter("applicationKey");
    
    if (session.getAttribute(AUTHORIZATION_TOKEN_KEY) != null)
    {
      response.addHeader(AUTHORIZATION_TOKEN_KEY, (String)session.getAttribute(AUTHORIZATION_TOKEN_KEY));      
    }
    else if (request.getHeader(AUTHORIZATION_TOKEN_KEY) == null &&
        !IOUtils.isNullOrEmpty(tokenServiceUri) && !IOUtils.isNullOrEmpty(applicationKey))
    {
      HttpClient apigeeClient = new HttpClient(tokenServiceUri + applicationKey);
      
      try
      {
        byte[] data = userAttrsJson.getBytes("utf8");
        String apigeeResponse = apigeeClient.postByteData(String.class, "", data);
        
        @SuppressWarnings("unchecked")
        Map<String, Map<String, String>> apigeeResponseObj = (Map<String, Map<String, String>>) JSONUtil.deserialize(apigeeResponse);
        Map<String, String> accessToken = apigeeResponseObj.get("accesstoken");  
        
        String oAuthToken = accessToken.get("token");
        session.setAttribute(AUTHORIZATION_TOKEN_KEY, oAuthToken);
        response.addHeader(AUTHORIZATION_TOKEN_KEY, oAuthToken);
      }
      catch (Exception ex)
      {
        logger.error("Error obtaining OAuth token: " + ex);
        return false;
      }
    }
    
    return true;
  } 
}
