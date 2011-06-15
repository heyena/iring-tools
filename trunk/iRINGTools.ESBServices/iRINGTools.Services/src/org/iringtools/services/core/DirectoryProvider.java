package org.iringtools.services.core;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import java.util.Properties;

import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.directory.Attributes;
import javax.naming.directory.DirContext;
import javax.naming.directory.InitialDirContext;
import javax.naming.directory.SearchControls;
import javax.naming.directory.SearchResult;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.directory.Groups;
import org.iringtools.directory.User;
import org.iringtools.utility.EncryptionUtils;
import org.iringtools.utility.JaxbUtils;

public class DirectoryProvider {
  private static final Logger logger = Logger.getLogger(DirectoryProvider.class);
  private Hashtable<String, String> settings;

	public DirectoryProvider(Hashtable<String, String> settings) {
		this.settings = settings;
	}

	public Directory getExchanges() throws JAXBException, IOException {
		String path = settings.get("baseDirectory")
				+ "/WEB-INF/data/directory.xml";
		return JaxbUtils.read(Directory.class, path);
	}

	public ExchangeDefinition getExchangeDefinition(String scope, String id)
			throws JAXBException, IOException {
		String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-"
				+ scope + "-" + id + ".xml";
		return JaxbUtils.read(ExchangeDefinition.class, path);
	}

	public User getUser(String userId) {
	  User user = new User();
		
	  Groups groups = new Groups();
		user.setGroups(groups);
		
		List<String> groupItems = new ArrayList<String>();
		groups.setItems(groupItems);
		
		Properties ldapProps = new Properties();
		String propsPath = settings.get("baseDirectory") + settings.get("ldapPropertiesPath");
		
		try
    {
      ldapProps.load(new FileInputStream(propsPath));
      
      // decrypt password
      String credsProp = "java.naming.security.credentials";
      String password = EncryptionUtils.decrypt(ldapProps.getProperty(credsProp));
      ldapProps.put(credsProp, password);      
    }
    catch (Exception ioe)
    {
      logger.error("Error loading ldap properties: " + ioe);
    }
		
    if (ldapProps.size() > 0)
    {
  		DirContext dctx = null;
  
  		try {
  			dctx = new InitialDirContext(ldapProps);
  		} 
  		catch (Exception e) {
  			logger.error("Error initializating directory context: " + e);
  		}
  
  		if (dctx != null) {
  			String baseDN = ldapProps.getProperty("baseDN");
  			
  			if (baseDN == null || baseDN.length() == 0)
  			  baseDN = "ou=iringtools,dc=iringug,dc=org";
  			
  			SearchControls constraints = new SearchControls();
  			constraints.setSearchScope(SearchControls.SUBTREE_SCOPE);
  			String filter = "(uid=" + userId + ")";
  
  			try {
  				NamingEnumeration<?> results = dctx.search(baseDN, filter, constraints);
  				
  				while (results.hasMore())
  				{					
  					SearchResult result = (SearchResult) results.next();
  					groupItems.add(((result.getName().split(","))[1].split("="))[1]);
  					
  					if (user.getUserId() == null)
  					{
  					  Attributes attrs = result.getAttributes();
  					  
              user.setUserId(attrs.get("uid").toString().substring(5));
              user.setFirstName(attrs.get("cn").toString().substring(4));
              user.setLastName(attrs.get("sn").toString().substring(4));
  					}
  				}
  			} 
  			catch (NamingException ex) {
  				logger.error("Error getting user information: " + userId);
  			}
  		}
    }
		
		return user;
	}
}
