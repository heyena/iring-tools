package org.iringtools.services;

import java.io.IOException;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

import javax.servlet.ServletContext;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.SecurityContext;
import javax.xml.bind.JAXBException;

import org.apache.cxf.jaxrs.ext.MessageContext;
import org.apache.log4j.Logger;
import org.iringtools.common.Config;
import org.iringtools.common.Constants;
import org.iringtools.common.Section;
import org.iringtools.common.Setting;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.JaxbUtils;

public abstract class AbstractService
{
  private static final Logger logger = Logger.getLogger(AbstractService.class);
  
  @Context protected ServletContext servletContext; 
  @Context protected MessageContext messageContext; 
  @Context protected SecurityContext securityContext;
  
  protected Map<String, Object> settings;
  protected HttpServletRequest request;
  protected HttpServletResponse response;
  protected Config serviceConfig;
  
  public void initService(String serviceName) throws JAXBException, IOException 
  {
    logger.info("Initializing " + serviceName);
    
    settings = java.util.Collections.synchronizedMap(new HashMap<String, Object>());    
    request = messageContext.getHttpServletRequest();
    response = messageContext.getHttpServletResponse(); 
    
    HttpUtils.prepareHttpProxy(servletContext);
    
    /*
     * PREPARE COMMON SETTINGS
     */    
    String basePath = servletContext.getRealPath("/");
    serviceConfig = JaxbUtils.read(Config.class, basePath + "WEB-INF/config/service_config.xml");
    settings.put("baseDirectory", basePath);
    
    String useLdap = servletContext.getInitParameter("useLDAP");
    if (useLdap.equals(""))
    	useLdap = "false";    
    settings.put("useLDAP", useLdap);

    String directoryServiceUri = servletContext.getInitParameter("directoryServiceUri");
    if (directoryServiceUri == null || directoryServiceUri.equals(""))
      directoryServiceUri = "http://localhost:8080/services/dir";
    settings.put("directoryServiceUri", directoryServiceUri);

    String differencingServiceUri = servletContext.getInitParameter("differencingServiceUri");
    if (differencingServiceUri == null || differencingServiceUri.equals(""))
      differencingServiceUri = "http://localhost:8080/services/diff";
    settings.put("differencingServiceUri", differencingServiceUri);

    String idGenServiceUri = servletContext.getInitParameter("IDGenServiceUri");
    if (idGenServiceUri == null || idGenServiceUri.equals(""))
    	idGenServiceUri = "http://localhost:8080/services/idgen";
    settings.put("idGenServiceUri", idGenServiceUri);
    
    /*
     * PREPARE REFERENCE DATA SETTINGS
     */    	
    String exampleRegistryBase = servletContext.getInitParameter("ExampleRegistryBase");
    if (exampleRegistryBase == null || exampleRegistryBase.equals(""))
    	exampleRegistryBase = "http://example.org/data#";
    settings.put("ExampleRegistryBase", exampleRegistryBase);

    String templateRegistryBase = servletContext.getInitParameter("TemplateRegistryBase");
    if (templateRegistryBase == null || templateRegistryBase.equals(""))
    	templateRegistryBase = "http://tpl.rdlfacade.org/data#";
    settings.put("TemplateRegistryBase", templateRegistryBase);

    String classRegistryBase = servletContext.getInitParameter("ClassRegistryBase");
    if (classRegistryBase == null || classRegistryBase.equals(""))
    	classRegistryBase = "http://rdl.rdlfacade.org/data#";
    settings.put("ClassRegistryBase", classRegistryBase);

    String useExampleRegistryBase = servletContext.getInitParameter("UseExampleRegistryBase");
    if (useExampleRegistryBase == null || useExampleRegistryBase.equals(""))
    	useExampleRegistryBase = "false";
    settings.put("UseExampleRegistryBase", useExampleRegistryBase);
        
    /*
     * PARSING SETTINGS FROM SERVICE CONFIG XML
     */  
    for (Section section : serviceConfig.getItems())
    {
      if (section.getName().equalsIgnoreCase("exchange service"))
      {
        for (Setting setting : section.getItems())
        {
          String settingName = setting.getName();

          if (settingName.equalsIgnoreCase("log-history"))
          {
            settings.put("numOfExchangeLogFiles", Integer.parseInt(setting.getValue()));
          }
          else if (settingName.equalsIgnoreCase("pool-size"))
          {
            settings.put("poolSize", Integer.parseInt(setting.getValue()));
          }
          else if (settingName.equalsIgnoreCase("manifest-task-timeout"))
          {
            settings.put("manifestTaskTimeout", Integer.parseInt(setting.getValue()));
          }
          else if (settingName.equalsIgnoreCase("dti-task-timeout"))
          {
            settings.put("dtiTaskTimeout", Integer.parseInt(setting.getValue()));
          }
          else if (settingName.equalsIgnoreCase("dto-task-timeout"))
          {
            settings.put("dtoTaskTimeout", Integer.parseInt(setting.getValue()));
          }
        }
        
        break;
      }
    }
    
    /*
     * CARRY ON REQUEST HEADERS IN SETTINGS
     */    
    MultivaluedMap<String, String> headers = messageContext.getHttpHeaders().getRequestHeaders();
    
    for (Entry<String, List<String>> header : headers.entrySet())
    {
      List<String> values = header.getValue();
      
      if (values != null && values.size() > 0)
      {
        settings.put(Constants.HTTP_HEADER_PREFIX + header.getKey(), values.get(0));
      }
    }
  }
  
  protected Response prepareErrorResponse(int errorCode, Exception e)
  {
    return prepareErrorResponse(errorCode, e.getMessage());
  }
  
  protected Response prepareErrorResponse(int errorCode, String errorMessage)
  {
    return Response.status(errorCode).type(MediaType.TEXT_PLAIN).entity(errorMessage).build();
  }
}
