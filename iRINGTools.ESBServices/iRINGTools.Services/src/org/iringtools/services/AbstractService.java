package org.iringtools.services;

import java.util.Hashtable;
import javax.servlet.ServletContext;
import javax.ws.rs.core.Context;
//import javax.ws.rs.core.SecurityContext;
//import org.apache.cxf.jaxrs.ext.MessageContext;

public abstract class AbstractService
{
  @Context private ServletContext servletContext;  
  //@Context private SecurityContext securityContext;
  //@Context private MessageContext messageContext;
  
  protected Hashtable<String, String> settings;
  
  public void initService()
  {
    settings = new Hashtable<String, String>();
    
    settings.put("baseDirectory", servletContext.getRealPath("/"));

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

    String dbConnectionString = servletContext.getInitParameter("dbConnectionString");
    if (dbConnectionString == null || dbConnectionString.equals(""))
    	dbConnectionString = "jdbc:sqlserver://127.0.0.1:1046;Database=ABC;User=abc; Password=abc";
    settings.put("dbConnectionString", dbConnectionString);
    	
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
    
    
    String poolSize = servletContext.getInitParameter("poolSize");
    if (poolSize == null || poolSize.equals(""))
      poolSize = "100";
    settings.put("poolSize", poolSize);
    
    String numOfExchangeLogFiles = servletContext.getInitParameter("numOfExchangeLogFiles");
    if (numOfExchangeLogFiles == null || numOfExchangeLogFiles.equals(""))
      numOfExchangeLogFiles = "10";
    settings.put("numOfExchangeLogFiles", numOfExchangeLogFiles);
    
    String proxyHost = servletContext.getInitParameter("proxyHost");
    if (proxyHost != null && proxyHost.length() > 0)
      settings.put("proxyHost", proxyHost);
    
    String proxyPort = servletContext.getInitParameter("proxyPort");
    if (proxyPort != null && proxyPort.length() > 0)
      settings.put("proxyPort", proxyPort);
    
    String proxyCredentialsToken = servletContext.getInitParameter("proxyCredentialsToken");
    if (proxyCredentialsToken != null && proxyCredentialsToken.length() > 0)
      settings.put("proxyCredentialsToken", proxyCredentialsToken);
    
    String ldapPropertiesPath = servletContext.getInitParameter("ldapPropertiesPath");
    if (ldapPropertiesPath != null && ldapPropertiesPath.length() > 0)
      settings.put("ldapPropertiesPath", ldapPropertiesPath);
    else
      settings.put("ldapPropertiesPath", "WEB-INF/ldap.properties");
  }
}
