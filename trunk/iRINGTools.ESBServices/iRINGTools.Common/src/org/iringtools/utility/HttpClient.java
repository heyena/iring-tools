package org.iringtools.utility;

import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.apache.commons.codec.binary.Base64;

public class HttpClient
{
  private String baseUri;
  private NetworkCredentials networkCredentials = null;
  private Map<String, String> headers = null;

  private final static String GET_METHOD = "GET";
  private final static String POST_METHOD = "POST";
  
  private final static String SPARQL_QUERY = "query";
  private final static String SPARQL_UPDATE = "update";

  public HttpClient() 
  {
    this(null, null);
  }

  public HttpClient(String baseUri)
  {
    this(baseUri, null);
  }

  public HttpClient(String baseUri, NetworkCredentials networkCredentials)
  {
    setBaseUri(baseUri);
    setNetworkCredentials(networkCredentials);    
    headers = new HashMap<String, String>();
  }

  public <T> T get(Class<T> responseClass, String relativeUri) throws HttpClientException
  {
    try
    {
      URLConnection conn = getConnection(GET_METHOD, relativeUri);
      InputStream responseStream = conn.getInputStream();
      return JaxbUtils.toObject(responseClass, responseStream);
    }
    catch (Exception ex)
    {
      throw new HttpClientException(ex.toString());
    }
  }

  public <T> T get(Class<T> responseClass) throws HttpClientException
  {
    return get(responseClass, "");
  }

  public <T, R> R post(Class<R> responseClass, T requestEntity) throws HttpClientException
  {
    return post(responseClass, "", requestEntity);
  }

  public <T, R> R post(Class<R> responseClass, String relativeUri, T requestEntity) throws HttpClientException
  {
    try
    {
      String content = "";

      if (requestEntity != null && !requestEntity.getClass().getName().equals("java.lang.String"))
        content = JaxbUtils.toXml(requestEntity, false);

      URLConnection conn = getConnection(POST_METHOD, relativeUri);
      conn.setRequestProperty("Content-Type", "application/xml");
      conn.setRequestProperty("Content-Length", String.valueOf(content.length()));

      DataOutputStream requestStream = new DataOutputStream(conn.getOutputStream());
      requestStream.writeBytes(content);
      requestStream.flush();
      requestStream.close();

      InputStream responseStream = conn.getInputStream();
      R response = JaxbUtils.toObject(responseClass, responseStream);

      return response;
    }
    catch (Exception ex)
    {
      throw new HttpClientException(ex.toString());
    }
  }

  public <T> T postFormData(Class<T> responseClass, String relativeUri, Map<String, String> formData,
      Map<String, String> headers) throws HttpClientException
  {
    try
    {
      URLConnection conn = getConnection(POST_METHOD, relativeUri);

      for (Entry<String, String> pair : headers.entrySet())
      {
        conn.setRequestProperty(pair.getKey(), pair.getValue());
      }

      StringBuilder requestEntity = new StringBuilder();

      if (formData != null)
      {
        for (Entry<String, String> pair : formData.entrySet())
        {
          if (requestEntity.length() > 0)
          {
            requestEntity.append('&');
          }

          requestEntity.append(pair.getKey() + "=" + URLEncoder.encode(pair.getValue(), "UTF-8"));
        }
      }

      DataOutputStream requestStream = new DataOutputStream(conn.getOutputStream());
      requestStream.writeBytes(requestEntity.toString());
      requestStream.flush();
      requestStream.close();

      InputStream responseStream = conn.getInputStream();
      T response = JaxbUtils.toObject(responseClass, responseStream);

      return response;
    }
    catch (Exception ex)
    {
      throw new HttpClientException(ex.toString());
    }
  }

  public <T> T postFormData(Class<T> responseClass, String relativeUri, Map<String, String> formData)
      throws HttpClientException
  {
    Map<String, String> headers = new HashMap<String, String>();
    headers.put("Content-Type", "application/x-www-form-urlencoded");
    return postFormData(responseClass, relativeUri, formData, headers);
  }

  public <T> T postSparql(Class<T> responseClass, String relativeUri, String query, String defaultGraphUri)
      throws HttpClientException
  {
    return postSparql(responseClass, relativeUri, SPARQL_QUERY, query, defaultGraphUri);
  }
  
  public <T> T postSparqlUpdate(Class<T> responseClass, String relativeUri, String query, String defaultGraphUri)
      throws HttpClientException
  {
    return postSparql(responseClass, relativeUri, SPARQL_UPDATE, query, defaultGraphUri);
  }
  
  private <T> T postSparql(Class<T> responseClass, String relativeUri, String postType, String query, String defaultGraphUri)
      throws HttpClientException
  {
    Map<String, String> headers = new HashMap<String, String>();
    headers.put("Content-Type", "application/x-www-form-urlencoded");
    headers.put("Accept", "application/sparql-results+xml");

    Map<String, String> formData = new HashMap<String, String>();
    formData.put(postType, query);
    formData.put("default-graph-uri", (defaultGraphUri != null) ? defaultGraphUri : "");

    return postFormData(responseClass, relativeUri, formData, headers);    
  }

  public void setBaseUri(String baseUri)
  {
    this.baseUri = baseUri;
  }

  public String getBaseUri()
  {
    return baseUri;
  }

  public void setNetworkCredentials(NetworkCredentials networkCredentials)
  {
    this.networkCredentials = networkCredentials;
  }

  public NetworkCredentials getNetworkCredentials()
  {
    return networkCredentials;
  }
  
  public void addHeader(String name, String value)
  {
    headers.put(name, value);
  }

  private URLConnection getConnection(String method, String relativeUri) throws IOException, EncryptionException
  {
    if (baseUri == null)
      baseUri = "";

    URL url = new URL(baseUri + relativeUri);
    URLConnection conn = url.openConnection();
    
    String proxySet = System.getProperty("proxySet");
    if (proxySet != null && proxySet.equalsIgnoreCase("true"))
    {
      String proxyUserName = System.getProperty("http.proxyUserName");
      String proxyPassword = EncryptionUtils.decrypt(System.getProperty("http.proxyPassword"));
      String proxyDomain = System.getProperty("http.proxyDomain");      
      String proxyCredsToken = createCredentialsToken(proxyUserName, proxyPassword, proxyDomain);
      
      conn.setRequestProperty("Proxy-Authorization", "Basic " + proxyCredsToken);
    }

    if (networkCredentials != null)
    {
      String networkCredsToken = createCredentialsToken(networkCredentials.getUserName(),
          networkCredentials.getPassword(), networkCredentials.getDomain());
      conn.setRequestProperty("Authorization", "Basic " + networkCredsToken);
    }
    
    // add headers
    for (Entry<String, String> header : headers.entrySet())
    {
      conn.setRequestProperty(header.getKey(), header.getValue());
    }

    if (method.equalsIgnoreCase(POST_METHOD))
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
