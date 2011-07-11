package org.iringtools.security;

import java.util.Map;

public interface AuthorizationProvider
{
  void init(Map<String, String> settings) throws Exception;
  boolean isAuthorized(Map<String, String> claims);
}
