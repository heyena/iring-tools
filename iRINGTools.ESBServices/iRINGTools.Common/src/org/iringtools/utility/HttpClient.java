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
import java.util.Properties;

import org.apache.commons.codec.binary.Base64;

public class HttpClient
{
  private String baseUri;
  private HttpProxy httpProxy = null;
  private NetworkCredentials networkCredentials = null;

  private final static String GET = "GET";
  private final static String POST = "POST";
  
  private final static String SPARQL_QUERY = "query";
  private final static String SPARQL_UPDATE = "update";

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

  public <T> T get(Class<T> responseClass, String relativeUri) throws HttpClientException
  {
    try
    {
      URLConnection conn = getConnection(GET, relativeUri);
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

      URLConnection conn = getConnection(POST, relativeUri);
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
      URLConnection conn = getConnection(POST, relativeUri);

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
    if (baseUri == null)
      baseUri = "";

    URL url = new URL(baseUri + relativeUri);
    URLConnection conn = url.openConnection();

    if (httpProxy != null)
    {
      Properties properties = System.getProperties();
      properties.put("proxySet", "true");
      properties.put("http.proxyHost", httpProxy.getHost());
      properties.put("http.proxyPort", String.valueOf(httpProxy.getPort()));

      String proxyCredsToken = createCredentialsToken(httpProxy.getUserName(), httpProxy.getPassword(),
          httpProxy.getDomain());
      conn.setRequestProperty("Proxy-Authorization", "Basic " + proxyCredsToken);
    }

    if (networkCredentials != null)
    {
      String networkCredsToken = createCredentialsToken(networkCredentials.getUserName(),
          networkCredentials.getPassword(), networkCredentials.getDomain());
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
