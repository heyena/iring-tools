package org.iringtools.controllers;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.Map;
import java.util.Properties;

import javax.naming.NamingException;
import javax.servlet.ServletContext;
import javax.servlet.http.Cookie;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.apache.log4j.Logger;
import org.apache.struts2.ServletActionContext;
import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.security.LdapAuthorizationProvider;
import org.iringtools.security.OAuthFilter;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;

import com.opensymphony.xwork2.ActionSupport;

public abstract class AbstractController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private static final Logger logger = Logger.getLogger(AbstractController.class);

  protected Map<String, Object> session;

  protected ServletContext context;
  protected HttpServletRequest request;
  protected HttpServletResponse response;

  public AbstractController()
  {
    context = ServletActionContext.getServletContext();
    request = ServletActionContext.getRequest();
    response = ServletActionContext.getResponse();
    
    HttpUtils.prepareHttpProxy(context);
  }

  @Override
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
    
    this.session.put(OAuthFilter.AUTHORIZATION, request.getAttribute(OAuthFilter.AUTHORIZATION));
    this.session.put(OAuthFilter.APP_KEY, request.getAttribute(OAuthFilter.APP_KEY));
  }

  protected void authorize(String app, String group)
  {
    String authorizationEnabled = context.getInitParameter("AuthorizationEnabled");

    if (!IOUtils.isNullOrEmpty(authorizationEnabled) && authorizationEnabled.equalsIgnoreCase("true"))
    {
      String ldapConfigPath = context.getRealPath("/") + "WEB-INF/config/ldap.conf";

      if (!IOUtils.fileExists(ldapConfigPath))
      {
        try
        {
          response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, "Ldap configuration not found.");
        }
        catch (IOException unexpectedException)
        {
          logger.error(unexpectedException.toString());
        }
      }

      Cookie[] cookies = request.getCookies();
      Cookie authorizedCookie = HttpUtils.getCookie(cookies, app);

      if (authorizedCookie == null) // user not authorized, attempt to authorize
      {
        Properties props = new Properties();

        try
        {
          props.load(new FileInputStream(ldapConfigPath));
        }
        catch (IOException ioe)
        {
          try
          {
            response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, "Error loading LDAP properties: " + ioe);
          }
          catch (IOException unexpectedException)
          {
            logger.error(unexpectedException.toString());
          }
        }

        props.put("authorizedGroup", group);

        LdapAuthorizationProvider authProvider = new LdapAuthorizationProvider();

        try
        {
          authProvider.init(props);
        }
        catch (NamingException ne)
        {
          try
          {
            response.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR,
                "Error initializing authentication provider: " + ne);
          }
          catch (IOException unexpectedException)
          {
            logger.error(unexpectedException.toString());
          }
        }
        
        String userId = request.getAttribute(OAuthFilter.USER_ID).toString();

        if (!authProvider.isAuthorized(userId))
        {
          try
          {
            String errorMessage = "User [" + userId + "] not authorized.";
            response.sendError(HttpServletResponse.SC_UNAUTHORIZED, errorMessage);
          }
          catch (Exception e)
          {
            logger.error(e.toString());
          }
        }
        else
        {
          response.addCookie(new Cookie(app, "authorized"));
        }
      }
    }
  }

  protected void prepareErrorResponse(int errorCode, String errorMessage)
  {
    request.setAttribute("javax.servlet.error.message", errorMessage);
    response.setStatus(errorCode);
  }
}
