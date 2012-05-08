package org.iringtools.services.core;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.iringtools.directory.Application;
import org.iringtools.directory.Applications;
import org.iringtools.directory.Locator;
import org.iringtools.directory.Locators;
import org.iringtools.directory.Resource;

public class ResourceProvider
{
	protected Map<String, Object> settings;
  
  protected static Map<String, String> rightPriority;
  static
  {
  	rightPriority = new HashMap<String, String>();
  	rightPriority.put("rootadmin", "0");
  	rightPriority.put("treenodeadmin", "1");
  	rightPriority.put("exchange", "2");
  	rightPriority.put("user", "3");
  }
  
  public ResourceProvider()
  {};    
 
  protected boolean hasBaseUrl(List<Resource> resourceList, String baseUrl)
  {
  	for (Resource resource : resourceList)
  	{
  		if (resource.getBaseUrl().equals(baseUrl))
  			return true;
  	}
  	return false;
  }
  
  protected Resource getResource(List<Resource> resourceList, String baseUrl)
  {
  	for (Resource resource : resourceList)
  	{
  		if (resource.getBaseUrl().equals(baseUrl))
  			return resource;
  	}
  	return null;
  }
  
  protected boolean hasContext(List<Locator> locatorList, String context)
  {
  	for (Locator locator : locatorList)
  	{
  		if (locator.getContext().equals(context))
  			return true;
  	}
  	return false;
  }
  
  protected Locator getLocator(List<Locator> locatorList, String context)
  {
  	for (Locator locator : locatorList)
  	{
  		if (locator.getContext().equals(context))
  			return locator;
  	}
  	return null;
  }
  
  protected boolean hasEndpoint(List<Application> applicationList, String endpoint)
  {
  	for (Application application : applicationList)
  	{
  		if (application.getEndpoint().equals(endpoint))
  			return true;
  	}
  	return false;
  }   
  
  protected void setApplicationAtrributes(List<Application> applicationList, String endpoint, String assembly, String description, String lpath)
  {
  	Application application = new Application();
		applicationList.add(application);
		application.setEndpoint(endpoint);
		application.setAssembly(assembly);	 
		application.setDescription(description);	 
		application.setLpath(lpath);	
  }
  
  protected void setResource(List<Locator> locatorList, String context, String assembly, String endpoint, String description, String lpath)
  {  	
  	Locator locator = null;    
  	
  	if (!hasContext(locatorList, context))
		{
  		locator = new Locator();
  		locator.setContext(context);  
  		locatorList.add(locator); 
  		Applications applications = new Applications();
  		locator.setApplications(applications);
  		List<Application> applicationList = applications.getItems();
  		setApplicationAtrributes(applicationList, endpoint, assembly, description, lpath);   		
		}
		else
		{
			locator = getLocator(locatorList, context);
			List<Application> applicationList = locator.getApplications().getItems();
			
			if (!hasEndpoint(applicationList, endpoint))
			{    				
				setApplicationAtrributes(applicationList, endpoint, assembly, description, lpath);
			}    			
		}
  }
  
  protected List<Locator> createResource(List<Resource> resourceList, String baseUrl)
	{
		Resource resource = null;
		List<Locator> locatorList = null;
		
		if (!hasBaseUrl(resourceList, baseUrl))
    {
    	resource = new Resource();
    	resourceList.add(resource);
    	resource.setBaseUrl(baseUrl);	 
    	Locators locators = new Locators();
    	resource.setLocators(locators);
    	locatorList = locators.getItems();
    }
    else
    {
    	resource = getResource(resourceList, baseUrl);   
    	locatorList = resource.getLocators().getItems();
    }	
		
		return locatorList;
	}
}








