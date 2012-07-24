package org.iringtools.security;

import java.util.Properties;

public interface AuthorizationProvider
{
  void init(Properties properties) throws Exception;
  boolean isAuthorized(String userId);
}
