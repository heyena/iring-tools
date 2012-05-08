package org.iringtools.tests;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.util.HashMap;
import java.util.Map;

import org.iringtools.directory.Directory;
import org.iringtools.directory.Resource;
import org.iringtools.services.core.DirectoryProvider;
import org.iringtools.services.core.DirectoryXMLProvider;
import org.iringtools.services.core.LdapProvider;
import org.iringtools.utility.JaxbUtils;
import org.junit.Test;

public class DirectoryTests {
	private String baseDir = "o=test,dc=iringug,dc=org";	
	private String baseUrl = "http:..localhost:54321.adapter";
	private String directoryBaseDir = "ou=directory,o=test,dc=iringug,dc=org";	
	private LdapProvider ldapProvider = null;
	private DirectoryXMLProvider directoryXMLProvider = null;
	private String user = "karthur";	
	private DirectoryProvider directoryProvider;

  
	public void init()
	{
		directoryXMLProvider = new DirectoryXMLProvider();
		ldapProvider = new LdapProvider();
		ldapProvider.setBaseUri(baseDir);  
	}	
	
	private void setup()
	{
		init();
		String fileDir = "/data/directory.xml";
		String fileXMLDir = "/data/directoryXML_input.xml";
  	
  	if (ldapProvider.hasLdapNode("cn=12345_000," + directoryBaseDir))
  		ldapProvider.deleteTree();
  	
  	if (directoryXMLProvider.getNode("12345_000", "12345_000", "folder"))
  		directoryXMLProvider.deleteTree();   	
  	
  	Map<String, Object> settings = new HashMap<String, Object>();  	
  	directoryProvider = new DirectoryProvider(settings);
		
  	String directoryPath = System.getProperty("user.dir") + fileDir;
    String directoryXMLPath = System.getProperty("user.dir") + fileXMLDir;    
    
    try
    {    
    	Directory directory = JaxbUtils.read(Directory.class, directoryPath);
  		directoryProvider.setBaseDir("ou=directory," + baseDir);
  		directoryProvider.postDirectory(directory, baseDir);
  	
  		Directory directoryXML = JaxbUtils.read(Directory.class, directoryXMLPath);
  		directoryXMLProvider.postDirectory(directoryXML);  		
    }
    catch (Exception ex)
    {
      assertFalse(true);
    }     
  }   
	
  @Test
  public void testReadDirectory()
  {  	
  	setup();
    
    try
    {  		
  		Directory readDirectory = ldapProvider.getDirectory(user);
  		Directory readDirectoryXML = directoryXMLProvider.readDirectoryFileToXML();
  		
  		assertTrue(readDirectory.getFolderList().size() > 0);
  		assertTrue(readDirectoryXML.getFolderList().size() > 0);
    }
    catch (Exception ex)
    {
      assertFalse(true);
    }     
  }
	
	@Test
  public void testAddingFolder()
  {
  	setup();
  	
  	try
  	{
  		ldapProvider.updateDirectoryNode(user, "54321_000.ADE", "ADE", "folder", "asdf", "asdf", "", "ad");
  		directoryXMLProvider.updateDirectoryNode("54321_000.ADE", "ADE", "folder", "asdf", "asdf", "", "ad");
  		assertTrue(ldapProvider.hasLdapNode("cn=ADE,cn=54321_000," + directoryBaseDir) == true);
  		assertTrue(directoryXMLProvider.getNode("54321_000.ADE", "ADE", "folder") == true);
  	}
    catch (Exception e)
    {
      assertFalse(true);
    }    
  }  
  
  @Test
  public void testUpdatingFolder()
  {
  	setup();
  	
  	try
    {
  	  //public void updateDirectoryNode(String user, String path, String name, String type, String description,
  	   //   String context, String baseUrl, String assembly)
  		ldapProvider.updateDirectoryNode(user, "54321_000.ABC", "DDD", "folder", "asdf", "asdf", "", "asdf");
  		directoryXMLProvider.updateDirectoryNode("54321_000.ABC", "DDD", "folder", "asdf", "asdf", "", "ad");
  		assertTrue(ldapProvider.hasLdapNode("cn=DDD,cn=54321_000," + directoryBaseDir) == true);
  		assertTrue(directoryXMLProvider.getNode("54321_000.DDD", "DDD", "folder") == true);
    }
    catch (Exception e)
    {
      assertFalse(true);
    }
  }    
  
  @Test
  public void testAddingEndpoint()
  {
  	setup();
  	
    try
    {
    	ldapProvider.updateDirectoryNode(user, "54321_000.ABC.CCC", "CCC", "endpoint", "asdf", "", "asdf", "asdf");
  		directoryXMLProvider.updateDirectoryNode("54321_000.ABC.CCC", "CCC", "endpoint", "asdf", "", "asdf", "asd");
  		assertTrue(ldapProvider.hasLdapNode("cn=CCC,cn=ABC,cn=54321_000," + directoryBaseDir) == true);
  		assertTrue(directoryXMLProvider.getNode("54321_000.ABC.CCC", "CCC", "endpoint") == true);
    }
    catch (Exception e)
    {
      assertFalse(true);
    }
  }  
  
  @Test
  public void testUpdatingEndpoint()
  {
  	setup();
  	
  	try
    {
  		ldapProvider.updateDirectoryNode(user, "54321_000.ABC.ABC", "EEE", "endpoint", "adsf", "", "asdf", "asdf");
  		directoryXMLProvider.updateDirectoryNode("54321_000.ABC.ABC", "EEE", "endpoint", "asdf", "", "asdf", "asdf");
  		assertTrue(ldapProvider.hasLdapNode("cn=EEE,cn=ABC,cn=54321_000," + directoryBaseDir) == true);
  		assertTrue(directoryXMLProvider.getNode("54321_000.ABC.EEE", "EEE", "endpoint") == true);
    }
    catch (Exception e)
    {
      assertFalse(true);
    }
  }
  
  @Test
  public void testDeletingEndpoint()
  {
  	setup();
  	
  	try
    {
  		ldapProvider.deleteLdapItem("54321_000.ABC.ABC");
  		directoryXMLProvider.deleteDirectoryItem("54321_000.ABC.ABC");  
  		assertTrue(ldapProvider.hasLdapNode("cn=ABC,cn=ABC,cn=54321_000," + directoryBaseDir) == false);
  		assertTrue(directoryXMLProvider.getNode("54321_000.ABC.ABC", "ABC", "endpoint") == false);
    }
    catch (Exception e)
    {
      assertFalse(true);
    }
  }  
  
  @Test
  public void testDeletingFolder()
  {
  	setup();
  	
  	try
    {
  		ldapProvider.deleteLdapItem("54321_000.DEF");
  		directoryXMLProvider.deleteDirectoryItem("54321_000.DEF");  
  		assertTrue(ldapProvider.hasLdapNode("cn=DEF,cn=54321_000," + directoryBaseDir) == false);
  		assertTrue(directoryXMLProvider.getNode("54321_000.DEF", "DEF", "folder") == false);
    }
    catch (Exception e)
    {
      assertFalse(true);
    }
  }  
  
  @Test
  public void testGettingResource()
  {
  	setup();
  	
  	try
    {
  		Resource resource = ldapProvider.getResource(baseUrl);
  		Resource resourceXML = directoryXMLProvider.getResource(baseUrl);
  		assertTrue(resource.getLocators().getItems().size() > 0);
  		assertTrue(resourceXML.getLocators().getItems().size() > 0);
    }
    catch (Exception e)
    {
      assertFalse(true);
    }
  }
}
