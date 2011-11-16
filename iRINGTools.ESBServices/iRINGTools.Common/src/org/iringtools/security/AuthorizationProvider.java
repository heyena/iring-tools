package org.iringtools.security;

import java.util.Map;
import java.util.Properties;

public interface AuthorizationProvider
{
  void init(Properties properties) throws Exception;
  boolean isAuthorized(Map<String, String> claims);
}
