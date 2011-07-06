package org.iringtools.security;

import java.util.Map;

public interface AuthorizationProvider
{
  boolean isAuthorized(Map<String, String> claims);
}
