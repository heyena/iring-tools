package org.iringtools.utility;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import java.util.Map.Entry;

import javax.xml.bind.JAXBException;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.auth.AuthScope;
import org.apache.http.auth.NTCredentials;
import org.apache.http.auth.UsernamePasswordCredentials;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.FileEntity;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.protocol.HTTP;

public class WebClient
{
  private String baseUri;
  private NTCredentials ntCredentials;
  private WebProxy webProxy;
  private DefaultHttpClient httpClient;
  
  public WebClient() {}
  
  public WebClient(String baseUri)
  {
    setBaseUri(baseUri);
  }
  
  public WebClient(String baseUri, NTCredentials ntCredentials)
  {
    this(baseUri);
    setNTCredentials(ntCredentials);
  }
  
  public WebClient(String baseUri, WebProxy webProxy)
  {
    this(baseUri);
    setWebProxy(webProxy);
  }
  
  public WebClient(String baseUri, NTCredentials ntCredentials, WebProxy webProxy)
  {
    this(baseUri);
    setNTCredentials(ntCredentials);
    setWebProxy(webProxy);
  }

  public <T> T get(Class<T> clazz, String relativeUri) throws WebClientException 
  {
    T response = null;    

    try
    {
      HttpGet httpGet = new HttpGet(baseUri + relativeUri);
      
      httpClient = new DefaultHttpClient();
      prepareCredentials();

      HttpResponse httpResponse = httpClient.execute(httpGet);
      response = getResponse(clazz, httpResponse);
    }
    catch (Exception ex)
    {
      throw new WebClientException(ex.getMessage());
    }
    finally
    {
      httpClient.getConnectionManager().shutdown();
    }

    return response;
  }

  public String getMessage(String relativeUri) throws WebClientException  
  {
    return get(String.class, relativeUri);
  }

  public <T,R> R post(Class<R> clazz, String relativeUri, T requestEntity) throws WebClientException 
  {
    R response = null;
    
    try
    {
      String content = JaxbUtil.toXml(requestEntity, false);
      StringEntity entity = new StringEntity(content);
      entity.setContentType("application/xml");
      
      HttpPost httpPost = new HttpPost(baseUri + relativeUri);      
      httpPost.setEntity(entity);

      httpClient = new DefaultHttpClient();
      prepareCredentials();

      HttpResponse httpResponse = httpClient.execute(httpPost);
      response = getResponse(clazz, httpResponse);
    }
    catch (Exception ex)
    {
      throw new WebClientException(ex.getMessage());
    }
    finally
    {
      httpClient.getConnectionManager().shutdown();
    }

    return response;
  }
  
  public <T> T postMessage(Class<T> clazz, String relativeUri, String message) throws WebClientException 
  {
    return post(clazz, relativeUri, message);
  }

  public <T> T postMultipartMessage(Class<T> clazz, String relativeUri, Hashtable<String,String> keyValuePairs) throws WebClientException 
  {
    T response = null;
    
    try
    {
      List<NameValuePair> formData = new ArrayList<NameValuePair>();      
      for (Entry<String,String> pair : keyValuePairs.entrySet())
      {
        formData.add(new BasicNameValuePair(pair.getKey(), pair.getValue()));
      }
      
      HttpPost httpPost = new HttpPost(baseUri + relativeUri);
      httpPost.setEntity(new UrlEncodedFormEntity(formData, HTTP.UTF_8));

      httpClient = new DefaultHttpClient();
      prepareCredentials();

      HttpResponse httpResponse = httpClient.execute(httpPost);
      response = getResponse(clazz, httpResponse);
    }
    catch (Exception ex)
    {
      throw new WebClientException(ex.getMessage());
    }
    finally
    {
      httpClient.getConnectionManager().shutdown();
    }

    return response;
  }
  
  public <T> T postMultipartMessage(Class<T> clazz, String relativeUri, String filePath) throws WebClientException 
  {
    T response = null;
    
    try
    {
      File file = new File(filePath);
      FileEntity entity = new FileEntity(file, "text/plain; charset=\"UTF-8\"");
      
      HttpPost httpPost = new HttpPost(baseUri + relativeUri);
      httpPost.setEntity(entity);

      httpClient = new DefaultHttpClient();
      prepareCredentials();

      HttpResponse httpResponse = httpClient.execute(httpPost);
      response = getResponse(clazz, httpResponse);
    }
    catch (Exception ex)
    {
      throw new WebClientException(ex.getMessage());
    }
    finally
    {
      httpClient.getConnectionManager().shutdown();
    }

    return response;
  }

  public void setBaseUri(String baseUri)
  {
    this.baseUri = baseUri;
  }

  public String getBaseUri()
  {
    return baseUri;
  }

  public void setNTCredentials(NTCredentials ntCredentials)
  {
    this.ntCredentials = ntCredentials;
  }

  public NTCredentials getNTCredentials()
  {
    return ntCredentials;
  }

  public void setWebProxy(WebProxy webProxy)
  {
    this.webProxy = webProxy;
  }

  public WebProxy getWebProxy()
  {
    return webProxy;
  }

  private void prepareCredentials()
  {
    if (ntCredentials != null)
    {
      httpClient.getCredentialsProvider().setCredentials(AuthScope.ANY, ntCredentials);
    }
    
    if (webProxy != null)
    {
      UsernamePasswordCredentials proxyCredentials = new UsernamePasswordCredentials(webProxy.getUsername(), webProxy.getPassword());
      AuthScope authScope = new AuthScope(webProxy.getHost(), webProxy.getPort());
      httpClient.getCredentialsProvider().setCredentials(authScope, proxyCredentials);
    }
  }
  
  private <T> T getResponse(Class<T> clazz, HttpResponse httpResponse) throws IllegalStateException, JAXBException, IOException 
  {
    HttpEntity responseEntity = httpResponse.getEntity();
    return JaxbUtil.toObject(clazz, responseEntity.getContent());
  }
}
