package org.iringtools.utility;

import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.util.Hashtable;
import java.util.Properties;
import java.util.Map.Entry;
import javax.xml.bind.JAXBException;
import org.apache.commons.codec.binary.Base64;

public class HttpClient
{
  private String baseUri;
  private HttpProxy httpProxy = null;
  private NetworkCredentials networkCredentials = null;
  
  public final static String GET = "GET";
  public final static String POST = "POST";
  
  public HttpClient() {}
  
  public HttpClient(String baseUri)
  {
    this(baseUri, null, null);
  }
  
  public HttpClient(String baseUri, HttpProxy httpProxy)
  {
    this(baseUri, httpProxy, null);
  }
  
  public HttpClient(String baseUri, NetworkCredentials networkCredentials)
  {
    this(baseUri, null, networkCredentials);
  }
  
  public HttpClient(String baseUri, HttpProxy httpProxy, NetworkCredentials networkCredentials)
  {
    setBaseUri(baseUri);
    setHttpProxy(httpProxy);
    setNetworkCredentials(networkCredentials);
  }
  
  public <T> T get(Class<T> responseClass, String relativeUri) throws JAXBException, IOException 
  {    
    URLConnection conn = getConnection(GET, relativeUri);
    InputStream responseStream = conn.getInputStream();    
    return JaxbUtil.toObject(responseClass, responseStream);
  }
  
  public <T,R> R post(Class<R> responseClass, String relativeUri, T requestEntity) throws IOException, JAXBException 
  {    
    String content = JaxbUtil.toXml(requestEntity, false);
    
    URLConnection conn = getConnection(POST, relativeUri);    
    conn.setRequestProperty("Content-Type", "application/xml");
    conn.setRequestProperty("Content-Length", String.valueOf(content.length()));
    
    DataOutputStream requestStream = new DataOutputStream(conn.getOutputStream());    
    requestStream.writeBytes(content);        
    requestStream.flush();
    requestStream.close();
    
    InputStream responseStream = conn.getInputStream();   
    return JaxbUtil.toObject(responseClass, responseStream);
  }
  
  public <T> T post(Class<T> responseClass, String relativeUri, Hashtable<String,String> formData) throws IOException, JAXBException
  {
    URLConnection conn = getConnection(POST, relativeUri);
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
    
    DataOutputStream requestStream = new DataOutputStream(conn.getOutputStream());    
    requestStream.writeBytes(requestEntity.toString());        
    requestStream.flush();
    requestStream.close();
    
    InputStream responseStream = conn.getInputStream();   
    return JaxbUtil.toObject(responseClass, responseStream);
  }
  
  public void setBaseUri(String baseUri)
  {
    this.baseUri = baseUri;
  }

  public String getBaseUri()
  {
    return baseUri;
  }

  public void setHttpProxy(HttpProxy httpProxy)
  {
    this.httpProxy = httpProxy;
  }

  public HttpProxy getHttpProxy()
  {
    return httpProxy;
  }

  public void setNetworkCredentials(NetworkCredentials networkCredentials)
  {
    this.networkCredentials = networkCredentials;
  }

  public NetworkCredentials getNetworkCredentials()
  {
    return networkCredentials;
  }
  
  private URLConnection getConnection(String method, String relativeUri) throws IOException
  {
    if (baseUri == null) baseUri = "";
    
    URL url = new URL(baseUri + relativeUri);
    URLConnection conn =  url.openConnection();
    
    if (httpProxy != null)
    {    
      Properties properties = System.getProperties();
      properties.put("proxySet", "true");
      properties.put("http.proxyHost", httpProxy.getHost());
      properties.put("http.proxyPort", String.valueOf(httpProxy.getPort()));
      
      String proxyCredsToken = createCredentialsToken(
          httpProxy.getUserName(), httpProxy.getPassword(), httpProxy.getDomain());       
      conn.setRequestProperty("Proxy-Authorization", "Basic " + proxyCredsToken);
    }
    
    if (networkCredentials != null)
    {
      String networkCredsToken = createCredentialsToken(
          networkCredentials.getUserName(), networkCredentials.getPassword(), networkCredentials.getDomain());       
      conn.setRequestProperty("Authorization", "Basic " + networkCredsToken);       
    }
    
    if (method.equalsIgnoreCase(POST))
    {
      conn.setUseCaches(false);
      conn.setDoOutput(true);
      conn.setDoInput(true);
    }
    
    return conn;
  }
  
  private String createCredentialsToken(String userName, String password, String domain)
  {
    String creds = userName + ":" + password;      
    
    if (domain != null && domain.length() > 0)
    {
      creds = domain + "\\\\" + creds;
    }
    
    return new String(Base64.encodeBase64(creds.getBytes()));
  }
}
