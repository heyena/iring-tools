package org.iringtools.security;

import java.io.FileInputStream;
import java.util.Map;
import java.util.Properties;
import java.util.Map.Entry;

import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.directory.DirContext;
import javax.naming.directory.InitialDirContext;
import javax.naming.directory.SearchControls;

import org.apache.log4j.Logger;
import org.iringtools.utility.EncryptionUtils;

public class LdapAuthorizationProvider implements AuthorizationProvider
{
  private static final Logger logger = Logger.getLogger(LdapAuthorizationProvider.class);
  private static final String USERID_KEY = "EmailAddress";
  private DirContext dctx;
  private String authorizedGroup;
  private Properties ldapConfig;

  public void init(Map<String, Object> settings)
  {
    String ldapConfigPath = (String)settings.get("ldapConfigPath");
    ldapConfig = new Properties();

    try
    {
      ldapConfig.load(new FileInputStream(ldapConfigPath));
      String credsProp = "java.naming.security.credentials";
      String password = EncryptionUtils.decrypt(ldapConfig.getProperty(credsProp));
      ldapConfig.put(credsProp, password);
    }
    catch (Exception ioe)
    {
      logger.error("Error loading ldap properties: " + ioe);
    }

    if (ldapConfig.size() > 0)
    {
      try
      {
        dctx = new InitialDirContext(ldapConfig);
      }
      catch (Exception e)
      {
        logger.error("Error initializating directory context: " + e);
      }
    }
  }
  
  public void setAuthorizedGroup(String authorizedGroup)
  {
    this.authorizedGroup = authorizedGroup;
  }

  public boolean isAuthorized(Map<String, String> claims)
  {
    String userId = getUserId(claims);

    if (userId != null && dctx != null)
    {
      String baseDN = ldapConfig.getProperty("baseDN");
      if (baseDN == null || baseDN.length() == 0)
      {
        baseDN = "ou=iringtools,dc=iringug,dc=org";
      }

      String groupDN = "cn=" + authorizedGroup + ",cn=groups," + baseDN;
      String qualUserId = "uid=" + userId + ",cn=users," + baseDN;
      String filter = "(member=" + qualUserId + ")";

      SearchControls constraints = new SearchControls();
      constraints.setSearchScope(SearchControls.SUBTREE_SCOPE);

      try
      {
        NamingEnumeration<?> results = dctx.search(groupDN, filter, constraints);

        if (results != null && results.hasMore())
        {
          return true;
        }
      }
      catch (NamingException ex)
      {
        logger.error("User [" + userId + "] not authorized.");
      }
    }

    return false;
  }

  private String getUserId(Map<String, String> claims)
  {
    for (Entry<String, String> entry : claims.entrySet())
    {
      if (entry.getKey().equalsIgnoreCase(USERID_KEY))
      {
        String userId = entry.getValue();
        return userId;
      }
    }

    return null;
  }
}
