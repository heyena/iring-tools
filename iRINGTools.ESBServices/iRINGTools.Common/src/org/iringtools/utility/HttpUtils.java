package org.iringtools.utility;

import java.io.UnsupportedEncodingException;
import java.net.URLDecoder;
import java.net.URLEncoder;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import javax.servlet.http.Cookie;

import org.iringtools.security.OAuthFilter;

public final class HttpUtils
{
  public static Cookie getCookie(Cookie[] cookies, String cookieName)
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
  
  public static String toQueryParams(Map<String, String> keyValuePairs) throws UnsupportedEncodingException
  {
    StringBuilder queryParams = new StringBuilder();
    
    if (keyValuePairs != null && keyValuePairs.size() > 0)
    {
      for (Entry<String, String> entry : keyValuePairs.entrySet())
      {
        if (queryParams.length() > 0)
        {
          queryParams.append("&");
        }
        
        String name = entry.getKey();        
        String value = URLEncoder.encode(entry.getValue(), "UTF-8");
        queryParams.append(name + "=" + value);
      }
    }
    
    return queryParams.toString();
  }
  
  public static Map<String, String> fromQueryParams(String queryParams) throws UnsupportedEncodingException
  {
    Map<String, String> keyValuePairs = new HashMap<String, String>();
    
    if (!IOUtils.isNullOrEmpty(queryParams)) 
    {
      String[] pairs = queryParams.split("&");
      
      for (String pair : pairs)
      {
        String[] parts = pair.split("=");
        
        if (parts.length >= 2 && !IOUtils.isNullOrEmpty(parts[0]) && !IOUtils.isNullOrEmpty(parts[1]))
        {
          keyValuePairs.put(parts[0], URLDecoder.decode(parts[1], "UTF-8"));
        }
      }
    }
    
    return keyValuePairs;
  }
  
  public static void addOAuthHeaders(Map<String, Object> settings, HttpClient httpClient)
  {    
    String authorizationToken = (String)settings.get(OAuthFilter.AUTHORIZATION_TOKEN_KEY);
    if (!IOUtils.isNullOrEmpty(authorizationToken))
    {
      httpClient.addHeader(OAuthFilter.AUTHORIZATION_TOKEN_KEY, authorizationToken);
    }
    
    String applicationKey = (String)settings.get(OAuthFilter.APPLICATION_KEY);
    if (!IOUtils.isNullOrEmpty(applicationKey))
    {
      httpClient.addHeader(OAuthFilter.APPLICATION_KEY, applicationKey);
    }
    
    String authenticatedUser = (String)settings.get(OAuthFilter.AUTHENTICATED_USER_KEY);
    if (!IOUtils.isNullOrEmpty(authenticatedUser))
    {
      httpClient.addHeader(OAuthFilter.AUTHENTICATED_USER_KEY, authenticatedUser);
    }
  }
}
