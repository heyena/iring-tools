package org.iringtools.utility;

import java.util.HashMap;
import java.util.Map;

import javax.servlet.http.Cookie;

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
  
  public static synchronized Map<String, String> getCookieAttributes(String cookieValue)
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
}
