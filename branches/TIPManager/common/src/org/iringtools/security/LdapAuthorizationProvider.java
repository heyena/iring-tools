package org.iringtools.security;

import java.io.IOException;
import java.util.Properties;

import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.directory.DirContext;
import javax.naming.directory.SearchControls;

import org.apache.log4j.Logger;
import org.iringtools.utility.IOUtils;

public class LdapAuthorizationProvider implements AuthorizationProvider
{
  private static final Logger logger = Logger.getLogger(LdapAuthorizationProvider.class);  
  private static final String BASE_DN = "o=iringtools,dc=iringug,dc=org";
  
  private DirContext dctx;
  private String authorizedGroup;

  public void init(Properties properties) throws NamingException
  {
    try
    {
      dctx = IOUtils.initDirContext(properties);
    }
    catch (IOException e)
    {
      logger.error("Directory context failed to initialize: " + e.getMessage());
    }
  }
  
  public boolean isAuthorized(String userId)
  {
    logger.debug("Authorizing user [" + userId + "].");
    
    if (userId != null && dctx != null)
    {
      String groupDN = "cn=" + authorizedGroup + ",ou=groups," + BASE_DN;
      String qualUserId = "uid=" + userId + ",ou=users," + BASE_DN;
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
}
