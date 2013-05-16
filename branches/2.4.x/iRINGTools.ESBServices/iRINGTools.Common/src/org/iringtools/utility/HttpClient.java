package org.iringtools.utility;

import java.io.ByteArrayOutputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.Authenticator;
import java.net.HttpURLConnection;
import java.net.InetSocketAddress;
import java.net.PasswordAuthentication;
import java.net.Proxy;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.security.cert.X509Certificate;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSocketFactory;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

import org.apache.commons.codec.binary.Base64;
import org.apache.log4j.Logger;

public class HttpClient
{
  private static final Logger logger = Logger.getLogger(HttpClient.class);
  
  private String baseUri;
  private NetworkCredentials networkCredentials = null;
  private Map<String, String> headers = new HashMap<String, String>();

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

  public HttpClient(String baseUri, boolean async)
  {
    this(baseUri, null, async);
  }
  
  public HttpClient(String baseUri, NetworkCredentials networkCredentials)
  {
    this(baseUri, networkCredentials, false);
  }
  
  public HttpClient(String baseUri, NetworkCredentials networkCredentials, boolean async)
  {
    setBaseUri(baseUri);
    setNetworkCredentials(networkCredentials);
    setAsync(async);
  }
  
  public void setAsync(boolean value)
  {
    if (value)
    {
      addHeader("async", "true");
    }
    else
    {
      addHeader("async", "false");
    }
  }

  @SuppressWarnings("unchecked")
  public <T> T get(Class<T> responseClass, String relativeUri) throws HttpClientException
  {
    HttpURLConnection conn = null;
    int responseCode = 0;

    try
    {
      conn = getConnection(GET_METHOD, relativeUri);
      
      responseCode = conn.getResponseCode();      
      logger.debug("Response Code: " + responseCode);

      if (responseCode == HttpURLConnection.HTTP_NO_CONTENT)
        return null;
      
      if (responseCode == HttpURLConnection.HTTP_ACCEPTED)
      {
        String statusURL = conn.getHeaderField("location");
        return (T)statusURL;
      }
      
      if (responseCode == HttpURLConnection.HTTP_OK)
      {
        InputStream responseStream = conn.getInputStream();
        logger.debug("Content Length: " + conn.getContentLength());
                
        if (conn.getContentLength() == 0)
          return null;
        
        if (responseClass == ByteArrayOutputStream.class)
        {
          ByteArrayOutputStream outStream = IOUtils.toByteArrayOutputStream(responseStream);
          return (T) outStream;
        }
        
        return JaxbUtils.toObject(responseClass, responseStream);
      }
      else
      {
        String error = "Request to URL [" + conn.getURL() + "] failed. ";
        
        try
        {
          InputStream errorStream = conn.getErrorStream();
          
          if (errorStream != null)
          {
            error += IOUtils.toString(errorStream);
          } 
        }
        catch (Exception e)
        {
          logger.debug(e.toString());
        }
        
        throw new HttpClientException(responseCode, error);
      }
    }
    catch (Exception e)
    {
      throw new HttpClientException(responseCode, e.toString());
    }
    finally
    {
      if (conn != null)
      {
        conn.disconnect();
        conn = null;
      }
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
    return post(responseClass, relativeUri, requestEntity, "application/xml");    
  }
  
  public <T, R> R post(Class<R> responseClass, T requestEntity, String contentType) throws HttpClientException
  {
    return post(responseClass, "", requestEntity, contentType);    
  }

  @SuppressWarnings("unchecked")
  public <T, R> R post(Class<R> responseClass, String relativeUri, T requestEntity, String contentType) throws HttpClientException
  {
    HttpURLConnection conn = null;
    int responseCode = 0;

    try
    {
      ByteArrayOutputStream requestStream = null;
      
      if (requestEntity.getClass() == ByteArrayOutputStream.class)
      {
        requestStream = (ByteArrayOutputStream) requestEntity;
      }
      else 
	  {
        requestStream = (ByteArrayOutputStream)JaxbUtils.toStream(requestEntity, false);
      }
      
      conn = getConnection(POST_METHOD, relativeUri);
      conn.setRequestProperty("Content-Type", contentType);
      conn.setRequestProperty("Content-Length", String.valueOf(requestStream.size()));
  
      DataOutputStream outputStream = new DataOutputStream(conn.getOutputStream());
      outputStream.write(requestStream.toByteArray());      
      outputStream.flush();
      outputStream.close();

      responseCode = conn.getResponseCode();
      logger.debug("Response Code: " + responseCode);

      if (responseCode == HttpURLConnection.HTTP_NO_CONTENT)
        return null;
      
      if (responseCode == HttpURLConnection.HTTP_ACCEPTED)
      {
        String statusURL = conn.getHeaderField("location");
        return (R)statusURL;
      }
      
      if (responseCode == HttpURLConnection.HTTP_OK)
      {
        InputStream responseStream = conn.getInputStream();        
        logger.debug("Content Length: " + conn.getContentLength());
        
        if (conn.getContentLength() == 0)
          return null;
                
        if (responseClass == ByteArrayOutputStream.class)
        {
          ByteArrayOutputStream outStream = IOUtils.toByteArrayOutputStream(responseStream);
          return (R) outStream;
        }
        
        return JaxbUtils.toObject(responseClass, responseStream);
      }
      else
      {
        String error = "Request to URL [" + conn.getURL() + "] failed. ";
        
        try
        {
          InputStream errorStream = conn.getErrorStream();
          
          if (errorStream != null)
          {
            error += IOUtils.toString(errorStream);
          } 
        }
        catch (Exception e)
        {
          logger.debug(e.toString());
        }
        
        throw new HttpClientException(responseCode, error);
      }
    }
    catch (Exception e)
    {
      throw new HttpClientException(responseCode, e.toString());
    }
    finally
    {
      if (conn != null)
      {
        conn.disconnect();
        conn = null;
      }
    }
  }

  public <R> R postByteData(Class<R> responseClass, String relativeUri, byte[] data) throws HttpClientException
  {
    HttpURLConnection conn = null;

    try
    {
      conn = getConnection(POST_METHOD, relativeUri);
      conn.setRequestProperty("Content-type", "application/octet-stream");
      conn.setRequestProperty("Content-length", String.valueOf(data.length));

      DataOutputStream requestStream = new DataOutputStream(conn.getOutputStream());
      requestStream.write(data);
      requestStream.flush();
      requestStream.close();

      if (conn.getResponseCode() == HttpURLConnection.HTTP_OK)
      {
        InputStream responseStream = conn.getInputStream();
        return JaxbUtils.toObject(responseClass, responseStream);
      }
      else
      {
        InputStream errorStream = conn.getErrorStream();
        String error = IOUtils.toString(errorStream);
        throw new HttpClientException(error);
      }
    }
    catch (Exception e)
    {
      throw new HttpClientException("Error posting to [" + conn.getURL().toString() + "]. " + e.toString());
    }
    finally
    {
      if (conn != null)
      {
        conn.disconnect();
        conn = null;
      }
    }
  }

  public <T> T postFormData(Class<T> responseClass, String relativeUri, Map<String, String> formData,
      Map<String, String> headers) throws HttpClientException
  {
    HttpURLConnection conn = null;

    try
    {
      conn = getConnection(POST_METHOD, relativeUri);

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

      if (conn.getResponseCode() == HttpURLConnection.HTTP_OK)
      {
        InputStream responseStream = conn.getInputStream();
        return JaxbUtils.toObject(responseClass, responseStream);
      }
      else
      {
        InputStream errorStream = conn.getErrorStream();
        String error = IOUtils.toString(errorStream);
        throw new HttpClientException(error);
      }
    }
    catch (Exception e)
    {
      throw new HttpClientException("Error posting to [" + conn.getURL().toString() + "]. " + e.toString());
    }
    finally
    {
      if (conn != null)
      {
        conn.disconnect();
        conn = null;
      }
    }
  }

  public <T> T postFormData(Class<T> responseClass, String relativeUri, Map<String, String> formData)
      throws HttpClientException
  {
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

  private <T> T postSparql(Class<T> responseClass, String relativeUri, String postType, String query,
      String defaultGraphUri) throws HttpClientException
  {
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

  private HttpURLConnection getConnection(String method, String relativeUri) throws IOException, EncryptionException
  {
    if (baseUri == null)
      baseUri = "";

    URL url = new URL(baseUri + relativeUri);
    logger.debug("Opening URL connection [" + url + "]");
    
    HttpURLConnection conn = null; 
    
    String proxySet = System.getProperty("proxySet");
    if (proxySet != null && proxySet.equalsIgnoreCase("true"))
    {
      String proxyHost = System.getProperty("http.proxyHost");
      int proxyPort = Integer.parseInt(System.getProperty("http.proxyPort"));
      String proxyUserName = System.getProperty("http.proxyUser");   
      
      String proxyDomain = System.getProperty("http.proxyDomain");
      if (proxyUserName != null && proxyDomain != null && proxyDomain.length() > 0)
        proxyUserName = proxyDomain + "\\" + proxyUserName;
      
      final String proxyUser = proxyUserName;
      final String proxyPassword = System.getProperty("http.proxyPassword"); 
      
      Proxy proxy = new Proxy(Proxy.Type.HTTP, new InetSocketAddress(proxyHost, proxyPort));
      conn = (HttpURLConnection) url.openConnection(proxy); 
      
      if (proxyUser != null && proxyPassword != null)
      {
        Authenticator.setDefault(new Authenticator() {
          public PasswordAuthentication getPasswordAuthentication() {
            return (new PasswordAuthentication(proxyUser, proxyPassword.toCharArray()));
          }
        });
      }
      
//      String proxyCredsToken = createCredentialsToken(proxyUserName, proxyPassword, proxyDomain);      
//      conn.setRequestProperty("Proxy-Authorization", "Basic " + proxyCredsToken);
      
      logger.debug("Connecting thru proxy server [" + proxyHost + "]");
    }
    else
    {
      conn = (HttpURLConnection) url.openConnection(); 
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
      String key = header.getKey();
      String value = header.getValue();
      
      // ignore accept header
      if (key.toLowerCase().equals("accept"))
      {
        continue;
      }
      
      logger.debug(key + ": " + value);
      conn.setRequestProperty(key, value);
    }

    if (method.equalsIgnoreCase(POST_METHOD))
    {
      conn.setUseCaches(false);
      conn.setDoOutput(true);
      conn.setDoInput(true);
    }
    
    /*if (url.getProtocol().equalsIgnoreCase("https"))
    {
      ignoreSslErrors(conn);
    }*/

    return conn;
  }

  public String createCredentialsToken(String userName, String password, String domain)
  {
    String creds = userName + ":" + password;

    if (domain != null && domain.length() > 0)
    {
      creds = domain + "\\\\" + creds;
    }

    return new String(Base64.encodeBase64(creds.getBytes()));
  }
  
  public void ignoreSslErrors(URLConnection connection) 
  {
    try
    {
      TrustManager[] trustAllCerts = new TrustManager[] { 
        new X509TrustManager()
        {
          @Override
          public void checkClientTrusted(X509Certificate[] chain, String authType){}
  
          @Override
          public void checkServerTrusted(X509Certificate[] chain, String authType){}
  
          @Override
          public X509Certificate[] getAcceptedIssuers()
          {
            return null;
          }
        } 
      };
  
      SSLContext sslContext = SSLContext.getInstance("SSL");
      sslContext.init(null, trustAllCerts, new java.security.SecureRandom());
      
      SSLSocketFactory sslSocketFactory = sslContext.getSocketFactory();  
      ((HttpsURLConnection) connection).setSSLSocketFactory(sslSocketFactory);      

      System.setProperty("sun.security.ssl.allowUnsafeRenegotiation", "true");
    }
    catch (Exception ex)
    {
      logger.error(ex.toString());
    }
  }
}
