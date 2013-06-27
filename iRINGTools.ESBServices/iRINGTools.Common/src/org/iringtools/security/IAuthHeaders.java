package org.iringtools.security;

import java.util.Map;

import javax.servlet.http.HttpServletRequest;

public interface IAuthHeaders
{
  Map<String, String> get(HttpServletRequest request);
}
