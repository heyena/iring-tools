package org.iringtools.services;

import java.util.Hashtable;

import javax.annotation.Resource;
import javax.jws.WebMethod;
import javax.jws.WebParam;
import javax.jws.WebService;
import javax.servlet.ServletContext;
import javax.xml.ws.WebServiceContext;
import javax.xml.ws.handler.MessageContext;
import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.services.core.DirectoryProvider;

@WebService(targetNamespace = "http://services.iringtools.org/", portName = "DirectorySoapServicePort", serviceName = "DirectorySoapServiceService")
public class DirectorySoapService 
{
  private static final Logger logger = Logger.getLogger(DirectoryService.class);
  
  @Resource 
  private WebServiceContext context;
  private Hashtable<String, String> settings;
  
  public DirectorySoapService()
  {
    settings = new Hashtable<String, String>();
  }
  
  @WebMethod(operationName = "getDirectory", action = "urn:GetDirectory")
  public Directory getDirectory()
  {
    Directory directory = null;
    
    try
    {
      init();
      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      directory = directoryProvider.getExchanges();
    }
    catch (Exception ex)
    {
      logger.error("Error getting directory information: " + ex);
    }
    
    return directory;
  }
  
  @WebMethod(operationName = "getExchangeDefinition", action = "urn:GetExchangeDefinition")
  public ExchangeDefinition getExchangeDefinition(@WebParam(name = "scope") String scope, @WebParam(name = "exchangeId") String exchangeId)
  {
    ExchangeDefinition xdef = null;
    
    try
    {
      init();
      DirectoryProvider directoryProvider = new DirectoryProvider(settings);
      xdef = directoryProvider.getExchangeDefinition(scope, exchangeId);
    }
    catch (Exception ex)
    {
      logger.error("Error getting exchange definition for [" + exchangeId + "]: " + ex);
    }
    
    return xdef;
  }
  
  private void init()
  {
    ServletContext servletContext = 
      (ServletContext) context.getMessageContext().get(MessageContext.SERVLET_CONTEXT); 
    settings.put("baseDirectory", servletContext.getRealPath("/"));
  }
}
