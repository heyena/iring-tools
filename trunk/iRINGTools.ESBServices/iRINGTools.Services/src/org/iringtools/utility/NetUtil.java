package org.iringtools.utility;

import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.util.Hashtable;
import java.util.Map.Entry;

import javax.xml.bind.JAXBException;

public class NetUtil
{
  public static <T> T get(Class<T> clazz, String url) throws JAXBException, IOException 
  {    
    URLConnection conn = getConnection("GET", url);
    InputStream stream = conn.getInputStream();    
    return JaxbUtil.toObject(clazz, stream);
  }
  
  public static <T,R> R post(Class<R> returnClass, String url, T content) throws IOException, JAXBException 
  {    
    String requestEntity = JaxbUtil.toXml(content, false);
    
    URLConnection conn = getConnection("POST", url);    
    conn.setRequestProperty("Content-Type", "application/xml");
    conn.setRequestProperty("Content-Length", String.valueOf(requestEntity.length()));
    
    DataOutputStream outStream = new DataOutputStream(conn.getOutputStream());    
    outStream.writeBytes(requestEntity);        
    outStream.flush();
    outStream.close();
    
    InputStream receivingStream = conn.getInputStream();   
    return JaxbUtil.toObject(returnClass, receivingStream);
  }
  
  public static <T> T postMultipartMessage(Class<T> returnClass, String url, Hashtable<String,String> formData) throws IOException, JAXBException
  {
    URLConnection conn = getConnection("POST", url);
    conn.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
  
    StringBuilder requestEntity = new StringBuilder();
    for (Entry<String,String> pair : formData.entrySet())
    {
      if (requestEntity.length() > 0)
      {
        requestEntity.append('&');
      }      
      requestEntity.append(pair.getKey() + "=" + URLEncoder.encode(pair.getValue(), "UTF-8"));
    }
    
    DataOutputStream outStream = new DataOutputStream(conn.getOutputStream());    
    outStream.writeBytes(requestEntity.toString());        
    outStream.flush();
    outStream.close();
    
    InputStream receivingStream = conn.getInputStream();   
    return JaxbUtil.toObject(returnClass, receivingStream);
  }
  
  private static URLConnection getConnection(String method, String url) throws IOException
  {
    URLConnection conn =  new URL(url).openConnection();
    
    if (method.equalsIgnoreCase("POST"))
    {
      conn.setUseCaches(false);
      conn.setDoOutput(true);
      conn.setDoInput(true);
    }
    
    return conn;
  }
}
