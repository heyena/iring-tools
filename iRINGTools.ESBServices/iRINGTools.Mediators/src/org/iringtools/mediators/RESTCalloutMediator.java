package org.iringtools.mediators;

import javax.xml.namespace.QName;
import org.apache.axis2.transport.http.HTTPConstants;
import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.impl.llom.OMElementImpl;
import org.apache.axiom.soap.SOAPBody;
import org.apache.axis2.AxisFault;
import org.apache.axis2.Constants;
import org.apache.axis2.Constants.Configuration;
import org.apache.axis2.addressing.EndpointReference;
import org.apache.axis2.client.Options;
import org.apache.axis2.client.ServiceClient;
import org.apache.axis2.context.ConfigurationContext;
import org.apache.axis2.context.ConfigurationContextFactory;
import org.apache.synapse.ManagedLifecycle;
import org.apache.synapse.MessageContext;
import org.apache.synapse.core.SynapseEnvironment;
import org.apache.synapse.mediators.AbstractMediator;
import org.apache.synapse.transport.nhttp.NhttpConstants;

public class RESTCalloutMediator extends AbstractMediator implements ManagedLifecycle
{
  private ServiceClient serviceClient = null;
  private String axis2ConfigFile = null;
  private String serviceURL = "http://";
  private String endpointEntry = null;
  private String method = "GET";
  private String contentType = HTTPConstants.MEDIA_TYPE_APPLICATION_XML;

  public boolean mediate(MessageContext mc)
  {
    try
    {      
      // initialize serviceClient
      ConfigurationContext cfgCtx = null;
      
      if (axis2ConfigFile == null || axis2ConfigFile.length() == 0)
      {        
        cfgCtx = ConfigurationContextFactory.createDefaultConfigurationContext();
      }
      else
      {
        cfgCtx = ConfigurationContextFactory.createConfigurationContextFromFileSystem(axis2ConfigFile);        
      }
      
      serviceClient = new ServiceClient(cfgCtx, null);
      
      EndpointReference endpointReference = null;
        
      if (serviceURL != null && serviceURL.length() > 0)
      {
        endpointReference = new EndpointReference(serviceURL);
      }
      else if (endpointEntry != null && endpointEntry.length() > 0)
      {
        OMElementImpl endpointOM = (OMElementImpl)mc.getEntry(endpointEntry);
        OMElement addressOM = (OMElement)endpointOM.getChildrenWithLocalName("address").next();
        String addressUri = addressOM.getAttribute(new QName(null, "uri")).getAttributeValue();
        
        String endpointPostfix = (String)mc.getProperty(NhttpConstants.REST_URL_POSTFIX);        
        if (endpointPostfix != null && endpointPostfix.length() > 0)
        {
          addressUri += endpointPostfix;
        }
        
        endpointReference = new EndpointReference(addressUri);
      }
      else
      {
        endpointReference = mc.getTo();
      }
      
      // initialize serviceClient options
      Options options = new Options();
      options.setTo(endpointReference);
      options.setProperty(Configuration.ENABLE_REST, Constants.VALUE_TRUE);
      
      if (method == null || method.length() == 0 || method.equalsIgnoreCase("GET"))
      {
        options.setProperty(Configuration.HTTP_METHOD, Configuration.HTTP_METHOD_GET);
      }
      else if (method.equalsIgnoreCase("POST"))
      {
        options.setProperty(Configuration.HTTP_METHOD, Configuration.HTTP_METHOD_POST);
        options.setProperty(HTTPConstants.CHUNKED, Boolean.FALSE);        
      }
      else if (method.equalsIgnoreCase("PUT"))
      {
        options.setProperty(Configuration.HTTP_METHOD, Configuration.HTTP_METHOD_PUT);
        options.setProperty(HTTPConstants.CHUNKED, Boolean.FALSE);        
      }
      else if (method.equalsIgnoreCase("DELETE"))
      {
        options.setProperty(Configuration.HTTP_METHOD, Configuration.HTTP_METHOD_DELETE);
      }
      
      if (contentType == null || contentType.length() == 0)
      {
        options.setProperty(Configuration.CONTENT_TYPE, HTTPConstants.MEDIA_TYPE_APPLICATION_XML);
      }
      else
      {
        options.setProperty(Configuration.CONTENT_TYPE, contentType);       
      }

      serviceClient.setOptions(options);
      
      // send message to serviceURL
      SOAPBody soapBody = mc.getEnvelope().getBody();
      OMElement payload = soapBody.getFirstElement();
      OMElement result = serviceClient.sendReceive(payload);      
      
      if (payload != null)
        payload.discard();
      
      soapBody.addChild(result);
    }
    catch (Exception e)
    {
      handleException("Error invoking service " + serviceURL, e, mc);
    }

    return true;
  }

  public void init(SynapseEnvironment synEnv) {}

  public void destroy()
  {
    try
    {
      if (serviceClient != null)
        serviceClient.cleanup();
    }
    catch (AxisFault ignore) {}
  }

  public String getServiceURL()
  {
    return serviceURL;
  }

  public void setServiceURL(String serviceURL)
  {
    this.serviceURL = (serviceURL != null) ? serviceURL.trim() : null;
  }

  public String getMethod()
  {
    return method;
  }

  public void setMethod(String method)
  {
    this.method = (method != null) ? method.trim() : null;
  }

  public String getContentType()
  {
    return contentType;
  }

  public void setContentType(String contentType)
  {
    this.contentType = (contentType != null) ? contentType.trim() : null;
  }

  public void setAxis2ConfigFile(String axis2ConfigFile)
  {
    this.axis2ConfigFile = (axis2ConfigFile != null) ? axis2ConfigFile.trim() : null;
  }

  public String getAxis2ConfigFile()
  {
    return axis2ConfigFile;
  }

  public void setEndpointEntry(String endpointEntry)
  {
    this.endpointEntry = endpointEntry;
  }

  public String getEndpointEntry()
  {
    return endpointEntry;
  }
}
