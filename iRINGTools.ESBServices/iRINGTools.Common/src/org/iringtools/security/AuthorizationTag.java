package org.iringtools.security;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Properties;

import javax.naming.NamingException;
import javax.servlet.http.Cookie;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;
import javax.servlet.jsp.JspException;
import javax.servlet.jsp.tagext.TagSupport;

import org.iringtools.utility.HttpUtils;

public class AuthorizationTag extends TagSupport
{
  private static final long serialVersionUID = 1L;

  public int doStartTag() throws JspException
  {
    HttpSession session = pageContext.getSession();
    HttpServletRequest request = (HttpServletRequest) pageContext.getRequest();
    HttpServletResponse response = (HttpServletResponse) pageContext.getResponse();
    
    String ldapConfigPath = pageContext.getServletContext().getRealPath("/") + "WEB-INF/config/ldap.conf";
    String appName = (String)session.getAttribute("appName");
    String authorizedGroupName = (String)session.getAttribute("authorizedGroupName");

    Cookie[] cookies = request.getCookies();
    Cookie authorizedCookie = HttpUtils.getCookie(cookies, appName);

    if (authorizedCookie == null)  // user not authorized, attempt to authorize
    {
      Map<String, String> userAttrs = null;
      
      // get user attributes
      try
      {
        String authUser = (String)session.getAttribute(OAuthFilter.AUTHENTICATED_USER_KEY);
        userAttrs = HttpUtils.toMap(authUser);
      }
      catch (Exception e)
      {
        Cookie authUserCookie = HttpUtils.getCookie(cookies, OAuthFilter.AUTHENTICATED_USER_KEY);
        userAttrs = HttpUtils.toMap(authUserCookie.getValue());
      }
      
      Map<String, String> settings = new HashMap<String, String>();
      
      try
      {
        Properties props = new Properties();
        props.load(new FileInputStream(ldapConfigPath));
        
        for (Entry<Object, Object> prop : props.entrySet())
        {
          settings.put((String)prop.getKey(), (String)prop.getValue());
        }
      }
      catch (IOException ioe)
      {
        throw new JspException("Error loading LDAP properties: " + ioe);
      }
      
      settings.put("authorizedGroup", authorizedGroupName);
      
      LdapAuthorizationProvider authProvider = new LdapAuthorizationProvider();
      
      try
      {
        authProvider.init(settings);
      }
      catch (NamingException ne)
      {
        throw new JspException("Error initializing authentication provider: " + ne);
      }
      
      if (userAttrs == null || !authProvider.isAuthorized(userAttrs))
      {
        try
        {
          response.sendError(HttpServletResponse.SC_UNAUTHORIZED);
        }
        catch (IOException e)
        {
          e.printStackTrace();
        }
      }
      else
      {
        response.addCookie(new Cookie(appName, "authorized"));
      }
    }
    
    return SKIP_BODY;
  }

  public int doEndTag()
  {
    return EVAL_PAGE;
  }
}
