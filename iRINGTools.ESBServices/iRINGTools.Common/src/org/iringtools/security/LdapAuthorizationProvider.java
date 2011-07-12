package org.iringtools.security;

import java.util.Map;
import java.util.Map.Entry;
import java.util.Properties;

import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.directory.DirContext;
import javax.naming.directory.InitialDirContext;
import javax.naming.directory.SearchControls;

import org.apache.log4j.Logger;
import org.iringtools.utility.EncryptionException;
import org.iringtools.utility.EncryptionUtils;

public class LdapAuthorizationProvider implements AuthorizationProvider
{
  private static final Logger logger = Logger.getLogger(LdapAuthorizationProvider.class);
  
  private static final String BASE_DN = "ou=iringtools,dc=iringug,dc=org";
  private static final String USERID_KEY = "EmailAddress";
  
  private DirContext dctx;
  private String authorizedGroup;

  public void init(Properties properties) throws NamingException
  {
    String server = properties.getProperty("server");
    String portNumber = properties.getProperty("portNumber");
    String userName = properties.getProperty("userName");
    String password = properties.getProperty("password");
    
    authorizedGroup = properties.getProperty("authorizedGroup");
    
    try
    {
      password = EncryptionUtils.decrypt(password);
    }
    catch (EncryptionException e)
    {
      logger.error("Error decrypting ldap password [" + password + "]");
    }

    Properties ldapConfig = new Properties();
    ldapConfig.put("java.naming.factory.initial", "com.sun.jndi.ldap.LdapCtxFactory");
    ldapConfig.put("java.naming.provider.url", "ldap://" + server + ":" + portNumber);
    ldapConfig.put("java.naming.security.authentication", "simple");
    ldapConfig.put("java.naming.security.principal", userName);
    ldapConfig.put("java.naming.security.credentials", password);
    
    dctx = new InitialDirContext(ldapConfig);
  }
  
  public boolean isAuthorized(Map<String, String> claims)
  {
    String userId = getUserId(claims);

    if (userId != null && dctx != null)
    {
      String groupDN = "cn=" + authorizedGroup + ",cn=groups," + BASE_DN;
      String qualUserId = "uid=" + userId + ",cn=users," + BASE_DN;
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
        return entry.getValue();
      }
    }

    return null;
  }
}
