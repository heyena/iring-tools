package org.iringtools.services.core;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.Hashtable;
import java.util.Properties;

import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.directory.Attribute;
import javax.naming.directory.Attributes;
import javax.naming.directory.DirContext;
import javax.naming.directory.InitialDirContext;
import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.directory.Authorized;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.utility.EncryptionUtils;
import org.iringtools.utility.JaxbUtils;

public class DirectoryProvider
{
  private static final Logger logger = Logger.getLogger(DirectoryProvider.class);
  private Hashtable<String, String> settings;

  public DirectoryProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Directory getExchanges() throws JAXBException, IOException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/directory.xml";
    return JaxbUtils.read(Directory.class, path);
  }

  public ExchangeDefinition getExchangeDefinition(String scope, String id) throws JAXBException, IOException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-" + scope + "-" + id + ".xml";
    return JaxbUtils.read(ExchangeDefinition.class, path);
  }

  public Authorized isAuthorized(String scope, String app, String userId)
  {
    Authorized authorized = new Authorized();
    authorized.setValue(false);

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

      try
      {
        dctx = new InitialDirContext(ldapProps);
      }
      catch (Exception e)
      {
        logger.error("Error initializating directory context: " + e);
      }

      if (dctx != null)
      {
        String baseDN = ldapProps.getProperty("baseDN");

        if (baseDN == null || baseDN.length() == 0)
          baseDN = "ou=iringtools,dc=iringug,dc=org";
        
        String qualUserId = "uid=" + userId + ",cn=users," + baseDN;
        String appCN = "cn=" + app + ",cn=" + scope + ",cn=apps," + baseDN;

        try
        {
          Attributes groupAttrs = dctx.getAttributes(appCN, new String[]{ "member" });
          
          if (groupAttrs != null)
          {
            Attribute groupAttr = groupAttrs.get("member");           
            NamingEnumeration<?> groups = groupAttr.getAll();   
            
            while (groups.hasMore())
            {
              String groupCN = groups.next().toString();
              Attributes userAttrs = dctx.getAttributes(groupCN, new String[]{ "member" });
              
              if (userAttrs != null)
              {
                Attribute userAttr = userAttrs.get("member");
                NamingEnumeration<?> users = userAttr.getAll();  
                
                while (users.hasMore())
                {
                  if (users.next().toString().equalsIgnoreCase(qualUserId))
                  {
                    authorized.setValue(true);
                    return authorized;
                  }
                }
              }
            }
          }
        }
        catch (NamingException ex)
        {
          logger.error("Error getting user information: " + userId);
        }
      }
    }

    return authorized;
  }
}
