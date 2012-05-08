package org.iringtools.services.core;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Endpoint;
import org.iringtools.directory.Endpoints;
import org.iringtools.directory.Folder;
import org.iringtools.directory.Folders;
import org.iringtools.directory.Locator;
import org.iringtools.directory.Locators;
import org.iringtools.directory.Resource;
import org.iringtools.directory.Resources;
import org.iringtools.utility.JaxbUtils;

public class DirectoryXMLProvider extends ResourceProvider
{
  private static final Logger logger = Logger.getLogger(DirectoryXMLProvider.class);
  private String directoryXMLPath = null;
  private Directory directory;  
  private boolean ifHas;
  
  public DirectoryXMLProvider(Map<String, Object> settings)
  {
  	super();
  	this.settings = settings;
  	setPath();
    directory = readDirectoryFileToXML();
  }   
  
  public DirectoryXMLProvider()
  {
  	super();
  	setPath();
  	directory = readDirectoryFileToXML();
  }
  
  public void setDirectoryXMLProvider(Map<String, Object> settings)
  {
    this.settings = settings;
    directory = readDirectoryFileToXML();
  } 
  
  private void setPath()
  {
  	if (directoryXMLPath == null)
  	{
	  	if (settings == null)  	
	  		directoryXMLPath = System.getProperty("user.dir") + "/data/directoryXML.xml";
	  	else
	  		directoryXMLPath = this.settings.get("baseDirectory").toString()+ "/WEB-INF/data/directory.xml";
  	}
  }
  
  public void deleteTree()
  {
  	directory = new Directory();
  	writeXMLToFile();
  }
  
  public Directory readDirectoryFileToXML()
  {
  	directory = null;  	
    
  	try
    {
      directory = JaxbUtils.read(Directory.class, directoryXMLPath);
    }
    catch (Exception ex)
    {
    	String message = ex.getMessage().toString();
      logger.error(message);      
    }
    
    return directory;
  }
  
  public void deleteDirectoryItem(String path)
  {
  	path = path.replace('.', '/');
  	String[] level = path.split("/");
  	
  	for (Folder folder : directory.getFolderList())
  	{
  		if (folder.getName().compareTo(level[0]) == 0)
  		{
  			if (level.length > 1)
  			{
	  			int inDepth = 1;	  			
	  			traverseDirectory(folder, level, inDepth);
  			}
  			else
  			{
  				directory.getFolderList().remove(folder);
  				folder = null;  				
  			}
  			break;
  		}  		
  	} 
  	
  	writeXMLToFile();
  }  
  
  private void traverseDirectory(Folder folder, String[] level, int inDepth)
  {
  	if (folder.getFolders() == null)
  	{	  	
	  	Endpoints endpoints = folder.getEndpoints();
	  	
	  	if (endpoints != null)
	  	{
	  		for (Endpoint endpoint : endpoints.getItems())
	  		{
	  			if (endpoint.getName().compareTo(level[inDepth]) == 0)
	  			{
	  				endpoints.getItems().remove(endpoint);
	  				endpoint = null;
	  				return;
	  			}
	  		}	  		
	  	}	  	
  	}
  	else
  	{
  		for(Folder subFolder : folder.getFolders().getItems())
  		{
  			if(subFolder.getName().compareTo(level[inDepth]) == 0)
  			{
  				inDepth++;
  				if (inDepth < level.length)
  				{ 					
  					traverseDirectory(subFolder, level, inDepth);
  				}
  				else
  				{
  					folder.getFolders().getItems().remove(subFolder);
  					subFolder = null;
  					return;
  				}
  			}
  		}  		
  	}
  }  
  
  private boolean findNode(Folder folder, String name, String type, String[]level, int depth)
  {  	
  	if (level.length == depth + 1 && type.equals("endpoint"))
  	{
  		Endpoints endpoints = folder.getEndpoints();
	  	if (endpoints != null && type.equals("endpoint"))
	  	{
	  		for (Endpoint endpoint : endpoints.getItems())
	  		{
	  			if (endpoint.getName().equals(name))
	  			{
	  				ifHas = true;
	  				return ifHas;
	  			}
	  		}
	  	}
  	}  	
  	
  	Folders folders = folder.getFolders();
  	if (folders != null)  	
  	{
  		for (Folder subFolder : folders.getItems())
  		{  
  			if (subFolder.getName().equals(level[depth]))
  			{
  				if(level.length > depth + 1)
	  			{
	  				depth++;
	  				findNode(subFolder, name, type, level, depth);  				
	  			}
  				else if (type.equals("folder"))
  				{
  					ifHas = true;
  					return ifHas;
  				}  			
  			}
  		}
  	}
  	else
  	{  
  		ifHas = false;
  		return ifHas;  	
  	}
  	
  	return ifHas;
  }
  
  public boolean getNode(String path, String name, String type)
  {
  	ifHas = false;  	
  	path = path.replace('.', '/');
  	String[] level = path.split("/");
  	int depth;
  	
  	if (directory != null)
			for (Folder folder : directory.getFolderList())
	  	{
				if (folder.getName().compareTo(level[0]) == 0)
	  		{
					depth = 1;
					if (level.length > depth)
						ifHas = findNode(folder, name, type, level, depth);	  		
					else
						ifHas = true;
	  		}
	  	}	
		
  	return ifHas;
  }
  
  public void updateDirectoryNode(String path, String name, String type, String description, String context, String baseUrl, String assembly)
  {
  	path = path.replace('.', '/');
  	String[] level = path.split("/");
  	ifHas = false;  	
  	
  	for (Folder folder : directory.getFolderList())
  	{
  		if (folder.getName().compareTo(level[0]) == 0)
  		{  			
  			if (level.length > 1)
  			{
	  			int inDepth = 1;		  			
	  			traverseDirectoryUpdateNode(folder, level, name, type, description, context, inDepth, baseUrl, assembly);	  			
  			}
  			else
  			{
  				updateFolder(folder, name, description, context);
  				String oldContext = getOldContext(folder);			
  				
  				if (oldContext != null && context != null && !oldContext.equalsIgnoreCase(context))
  					traverseDirectoryChangeContext(folder, context);
  				
  				ifHas = true;
  			}
  			
  			break;	
  		}  		
  	} 
  	
  	if (!ifHas && type.compareTo("folder") == 0 && level.length == 1)
  	{
  		directory.getFolderList().add(createNewFolder(name, description, context));  		
  	}  
  	
  	writeXMLToFile();
  }
  
  public void postDirectory(Directory directory)
  {
  	this.directory = directory;
  	writeXMLToFile();
  }
  
  private String getOldContext(Folder folder)
  {
  	String oldContext = null;
		
		if (folder.getFolders() != null)  				
			oldContext = folder.getFolders().getItems().get(0).getContext();  				
		else if (folder.getEndpoints() != null)
			oldContext = folder.getEndpoints().getItems().get(0).getContext();  		
		
		return oldContext;
  }
  
  private void getAllApplications(List<Endpoint> endpointList, Folder folder, String lpath, List<Resource> resrouceList)
  {
  	String itemPath;
  	Endpoints endpoints = folder.getEndpoints();
  	if (endpoints != null)  	
  		for (Endpoint endpoint : endpoints.getItems())  		
  		{
  			itemPath = lpath + "/" + endpoint.getName();
  			addApplication(endpoint, resrouceList, itemPath);
  		}
  	
  	Folders folders = folder.getFolders();
  	if (folders != null)  	  	
  		for (Folder subFolder : folders.getItems())  		
  		{
  			itemPath = lpath + "/" + subFolder.getName();
  			getAllApplications(endpointList, subFolder, itemPath, resrouceList);  	
  		}  	
  }
  
  private void getAllApps(List<Endpoint> endpointList, Folder folder, String lpath, List<Locator> locatorList)
  {
  	String itemPath;
  	Endpoints endpoints = folder.getEndpoints();
  	if (endpoints != null)  	
  		for (Endpoint endpoint : endpoints.getItems())  		
  		{
  			itemPath = lpath + "/" + endpoint.getName();
  			prepareResource(endpoint, locatorList, itemPath);
  		}
  	
  	Folders folders = folder.getFolders();
  	if (folders != null)  	  	
  		for (Folder subFolder : folders.getItems())  		
  		{
  			itemPath = lpath + "/" + subFolder.getName();
  			getAllApps(endpointList, subFolder, itemPath, locatorList);  	
  		}  	
  }
  
  public Resources getResources()
  {
  	Resources recources = new Resources();  	  
  	List<Resource> resourceList = recources.getResourceList();
  	String lpath = "";
  	
    try
  	{  		
    	List<Endpoint> endpointList = new ArrayList<Endpoint>();
    	
    	for (Folder folder : directory.getFolderList())    	
    	{
    		lpath = folder.getName();
    		getAllApplications(endpointList, folder, lpath, resourceList);
    	}
    	
    	return recources;
  	}
    catch(Exception ex)
  	{
  		logger.error(ex.getMessage().toString());
  	}
    			
  	return null;
  }
  
  private void addApplication(Endpoint itemEndpoint, List<Resource> resourceList, String lpath)
  {  	
		if (itemEndpoint.getContext() == null || itemEndpoint.getName() == null || itemEndpoint.getBaseUrl() == null)
			return;		
		
		String baseUrl = itemEndpoint.getBaseUrl();
		List<Locator> locatorList = createResource(resourceList, baseUrl);		
		prepareResource(itemEndpoint, locatorList, lpath);
  }
  
  private void prepareResource(Endpoint itemEndpoint, List<Locator> locatorList, String lpath)
  {
  	String description = "", assembly = "";    
  	String context = itemEndpoint.getContext();
		String endpoint = itemEndpoint.getName(); 
		
		if (itemEndpoint.getAssembly() != null)
			assembly = itemEndpoint.getAssembly();
		
		if (itemEndpoint.getDescription() != null)
			description = itemEndpoint.getDescription();
    
		setResource(locatorList, context, assembly, endpoint, description, lpath);
  }
  
  public Resource getResource(String baseUrl)
  {
  	String lpath = "";
  	Resource recource = new Resource();  	
  	recource.setBaseUrl(baseUrl);    	
  	Locators locators = new Locators();	
  	List<Locator> locatorList = locators.getItems();  
  	recource.setLocators(locators); 	
  	
    try
  	{  		
    	List<Endpoint> endpointList = new ArrayList<Endpoint>();
    	
    	for (Folder folder : directory.getFolderList())    	
    	{
    		lpath = folder.getName();
    		getAllApps(endpointList, folder, lpath, locatorList);
    	}
    	
    	return recource;
  	}
    catch(Exception ex)
  	{
  		logger.error(ex.getMessage().toString());
  	}
    			
  	return null;
  }
  
  private void traverseDirectoryChangeContext(Folder folder, String context)
  {
  	Endpoints endpoints = folder.getEndpoints();
  	if (endpoints != null)
  	{
  		for (Endpoint endpoint : endpoints.getItems())
  		{  			
		  	endpoint.setContext(context);
  		}
  	}
  	
  	Folders folders = folder.getFolders();
  	if (folders != null)  	
  	{
  		for (Folder subFolder : folders.getItems())
  		{  			
  			traverseDirectoryChangeContext(subFolder, context);
  		}
  	}
  	else
  	{  		
  		folder.setContext(context);
  		return;  	
  	}
  }
  
  private void traverseDirectoryUpdateNode(Folder folder, String[] level, String name, String type, String description, String context, int inDepth, String baseUrl, String assembly)
  {
  	if (folder.getFolders() == null)
  	{
	  	if (type.compareTo("endpoint") == 0)
	  	{
		  	Endpoints endpoints = folder.getEndpoints();
		  	String endpointContext = folder.getContext();
		  	
		  	if (endpoints != null)
		  	{
		  		for (Endpoint endpoint : endpoints.getItems())
		  		{
		  			String endpointName = endpoint.getName();
		  			if (endpointName.compareTo(level[inDepth]) == 0)
		  			{
		  				ifHas = true;
		  				updateEndpoint(endpoint, name, description, endpointContext, baseUrl, assembly);		  				
		  				return;
		  			}		  			
		  		}
		  		if (!ifHas)
		  		{
		  			endpoints.getItems().add(createNewEndpoint(name, description, endpointContext, baseUrl, assembly));
		  			return;
		  		}
		  	}
		  	else
		  	{
		  		Endpoints newEndpoints = new Endpoints();
		  		folder.setEndpoints(newEndpoints);
		  		List<Endpoint> endpointList = newEndpoints.getItems();		  		
		  		endpointList.add(createNewEndpoint(name, description, endpointContext, baseUrl, assembly));
		  		return;
		  	}
	  	}
	  	else if (type.compareTo("folder") == 0)
	  	{	  		
	  		Folders newFolders = new Folders();
	  		List<Folder> folderList = newFolders.getItems();	  		
	  		folderList.add(createNewFolder(name, description, context));
	  		folder.setFolders(newFolders);
	  		ifHas = true;
	  		return;
	  	}
  	}
  	else
  	{
  		for(Folder subFolder : folder.getFolders().getItems())
  		{
  			if(subFolder.getName().compareTo(level[inDepth]) == 0)
  			{
  				inDepth++;
  				if (inDepth < level.length)
  				{  					
  					traverseDirectoryUpdateNode(subFolder, level, name, type, description, context, inDepth, baseUrl, assembly);  					
  				}
  				else
  				{
  					ifHas = true;
  					updateFolder(subFolder, name, description, context);  	
  					String oldContext = getOldContext(subFolder);	
  					
  					if (oldContext != null && context != null && !oldContext.equalsIgnoreCase(context))
  						traverseDirectoryChangeContext(subFolder, context);
  					
  					return;
  				} 	

  				break;
  			}
  		}
  		
  		if (!ifHas && inDepth == level.length - 1 && type.compareTo("folder") == 0)
  		{
  			folder.getFolders().getItems().add(createNewFolder(name, description, context));
  			return;
  		}
  	}
  }  
  
  private void writeXMLToFile()
  {
  	setPath();
  	
  	try
  	{
  		JaxbUtils.write(directory, directoryXMLPath, true);
  	}
  	catch(Exception ex)
  	{
  		logger.error(ex.getMessage().toString());
  	}  	
  }  
  
  private Folder createNewFolder(String name, String description, String context)
  {
  	Folder folder = new Folder();
  	folder = updateFolder(folder, name, description, context);
  	folder.setType("folder");
  	return folder;
  }
  
  private Endpoint createNewEndpoint(String name, String description, String context, String baseUrl, String assembly)
  {
  	Endpoint endpoint = new Endpoint();
  	endpoint = updateEndpoint(endpoint, name, description, context, baseUrl, assembly);
  	endpoint.setType("endpoint");
  	return endpoint;
  }
  
  private Folder updateFolder(Folder folder, String name, String description, String context)
  {
  	folder.setName(name);
  	folder.setDescription(description);
  	folder.setContext(context);  
  	return folder;
  }
  
  private Endpoint updateEndpoint(Endpoint endpoint, String name, String description, String context, String baseUrl, String assembly)
  {
  	endpoint.setName(name);
  	endpoint.setDescription(description);
  	if (context != "")
  		endpoint.setContext(context);
  	endpoint.setBaseUrl(baseUrl);
  	endpoint.setAssembly(assembly);
  	return endpoint;
  }
}

