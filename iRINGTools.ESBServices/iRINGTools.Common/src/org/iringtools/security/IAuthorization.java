package org.iringtools.security;

import javax.servlet.http.HttpSession;

public interface IAuthorization
{
  boolean authorize(HttpSession session, String application, String user);
}
