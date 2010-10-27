package com.example;

import java.io.IOException;
import javax.xml.stream.XMLStreamException;
import org.apache.axiom.om.OMElement;
import org.apache.axis2.AxisFault;
import org.apache.axis2.Constants;
import org.apache.axis2.Constants.Configuration;
import org.apache.axis2.addressing.EndpointReference;
import org.apache.axis2.client.Options;
import org.apache.axis2.client.ServiceClient;
import org.apache.axis2.transport.http.HTTPConstants;

public class TestClient
{
  private static String serviceUrl = "http://labst9414:8280/services/exchange";
  
  public static void main(String[] args) throws IOException, XMLStreamException
  {
    OMElement manifest = getManifest();
    System.out.println(manifest);
    
    OMElement dto = getDto(manifest);
    System.out.println(dto);
  }
  
  public static OMElement getManifest() throws AxisFault
  {
    ServiceClient serviceClient = new ServiceClient();
    
    Options options = new Options();
    options.setTo(new EndpointReference(serviceUrl));
    options.setAction("getManifest");
    serviceClient.setOptions(options);
    
    return serviceClient.sendReceive(null); 
  }
    
  public static OMElement getDto(OMElement manifest) throws IOException, XMLStreamException
  {
    ServiceClient serviceClient = new ServiceClient();
    
    Options options = new Options();
    options.setTo(new EndpointReference(serviceUrl));
    options.setAction("getDto");
    options.setProperty(Configuration.ENABLE_REST, Constants.VALUE_TRUE);
    options.setProperty(Configuration.CONTENT_TYPE, "application/xml");
    options.setProperty(HTTPConstants.CHUNKED, Boolean.FALSE);
    serviceClient.setOptions(options);
    
    return serviceClient.sendReceive(manifest);   
  }
}
