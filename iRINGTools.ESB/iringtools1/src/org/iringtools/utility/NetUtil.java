package org.iringtools.utility;

import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import javax.xml.bind.JAXBException;

//TODO: handle credentials
public class NetUtil
{
  public static <T> T get(Class<T> clazz, String url) throws JAXBException, IOException 
  {    
    URL urlObj = new URL(url);    
    URLConnection conn = urlObj.openConnection();
    conn.setUseCaches(false);  
    InputStream stream = conn.getInputStream();    
    return JaxbUtil.toObject(clazz, stream);
  }
  
  public static <T,R> R post(Class<R> returnClass, String url, T content) throws IOException, JAXBException 
  {    
    URL urlObj = new URL(url);    
    URLConnection conn = urlObj.openConnection();
    conn.setUseCaches(false);
    conn.setDoOutput(true);
    conn.setDoInput(true);
    
    String data = JaxbUtil.toXml(content);
    conn.setRequestProperty("Content-Type", "application/xml");
    conn.setRequestProperty("Content-Length", String.valueOf(data.length()));
    DataOutputStream outStream = new DataOutputStream(conn.getOutputStream());    
    outStream.writeBytes(data);        
    outStream.flush();
    outStream.close();
    
    InputStream receivingStream = conn.getInputStream();   
    return JaxbUtil.toObject(returnClass, receivingStream);
  }
}
