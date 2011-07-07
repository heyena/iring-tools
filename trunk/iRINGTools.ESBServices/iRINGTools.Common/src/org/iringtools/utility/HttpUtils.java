package org.iringtools.utility;

import java.util.HashMap;
import java.util.Map;

import javax.servlet.http.Cookie;

import org.iringtools.security.OAuthFilter;

public class HttpUtils
{
  public static synchronized Cookie getCookie(Cookie[] cookies, String cookieName)
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
  
  public static synchronized Map<String, String> toMap(String multiValue)
  {
    Map<String, String> attrs = new HashMap<String, String>();
    
    if (!IOUtils.isNullOrEmpty(multiValue))
    {
      String[] pairs = multiValue.split(" *, *");
      
      for (String pair : pairs)
      {
        String[] parts = pair.split("=");
        attrs.put(parts[0], parts[1]);
      }
    }
    
    return attrs;
  }
  
  public static synchronized void addOAuthHeaders(Map<String, Object> settings, HttpClient httpClient)
  {
    String authenticatedUser = (String)settings.get(OAuthFilter.AUTHENTICATED_USER_KEY);
    if (!IOUtils.isNullOrEmpty(authenticatedUser))
    {
      httpClient.addHeader(OAuthFilter.AUTHENTICATED_USER_KEY, authenticatedUser);
    }
    
    String authorizationToken = (String)settings.get(OAuthFilter.AUTHORIZATION_TOKEN_KEY);
    if (!IOUtils.isNullOrEmpty(authorizationToken))
    {
      httpClient.addHeader(OAuthFilter.AUTHORIZATION_TOKEN_KEY, authorizationToken);
    }
  }
}
