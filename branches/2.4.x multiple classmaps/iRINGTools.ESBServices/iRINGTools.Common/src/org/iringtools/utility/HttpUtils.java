package org.iringtools.utility;

import java.io.UnsupportedEncodingException;
import java.net.URLDecoder;
import java.net.URLEncoder;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Properties;

import javax.servlet.ServletContext;
import javax.servlet.http.Cookie;

public final class HttpUtils
{
  public static void prepareHttpProxy(ServletContext context)
  {
    Properties sysProps = System.getProperties();

    String proxyHost = context.getInitParameter("proxyHost");
    if (proxyHost != null && proxyHost.length() > 0)
    {
      sysProps.put("http.proxyHost", proxyHost);
      sysProps.put("https.proxyHost", proxyHost);
    }
    else
      return;

    String proxyPort = context.getInitParameter("proxyPort");
    if (proxyPort != null && proxyPort.length() > 0)
    {
      sysProps.put("http.proxyPort", proxyPort);
      sysProps.put("https.proxyPort", proxyPort);
    }
    else
      return;

    String proxyUser = context.getInitParameter("proxyUser");
    if (proxyUser != null && proxyUser.length() > 0)
      sysProps.put("http.proxyUser", proxyUser);

    String encryptedProxyPassword = context.getInitParameter("proxyPassword");
    if (encryptedProxyPassword != null && encryptedProxyPassword.length() > 0)
    {
      String proxyKeyFile = context.getInitParameter("proxySecretKeyFile");

      try
      {
        String proxyPassword = (proxyKeyFile != null && proxyKeyFile.length() > 0) ? EncryptionUtils.decrypt(
            encryptedProxyPassword, proxyKeyFile) : EncryptionUtils.decrypt(encryptedProxyPassword);

        sysProps.put("http.proxyPassword", proxyPassword);
      }
      catch (EncryptionException e)
      {
        e.printStackTrace();
      }
    }
    
    String proxyDomain = context.getInitParameter("proxyDomain");
    if (proxyDomain == null)
      proxyDomain = "";
    sysProps.put("http.proxyDomain", proxyDomain);

    sysProps.put("proxySet", "true");
  }
  
  public static Cookie getCookie(Cookie[] cookies, String cookieName)
  {   
    if (cookies != null && cookies.length > 0)
    {
      for (int i=0; i<cookies.length; i++) 
      {
        Cookie cookie = cookies[i];
        
        if (cookie != null && cookie.getName().equalsIgnoreCase(cookieName))
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
  
  public static void addHttpHeaders(Map<String, Object> settings, HttpClient httpClient)
  {    
    String prefix = "http-header-";
    int index = prefix.length();
    
    for (Entry<String, Object> entry : settings.entrySet())
    {
      if (entry.getKey().startsWith(prefix))
      {
        if (entry.getKey() != null && entry.getValue() != null)
          httpClient.addHeader(entry.getKey().substring(index), entry.getValue().toString());
      }
    }
  }
}
