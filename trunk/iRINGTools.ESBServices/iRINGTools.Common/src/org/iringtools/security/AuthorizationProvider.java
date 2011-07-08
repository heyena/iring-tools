package org.iringtools.security;

import java.util.Map;

public interface AuthorizationProvider
{
  void init(Map<String, Object> settings);
  boolean isAuthorized(Map<String, String> claims);
}
