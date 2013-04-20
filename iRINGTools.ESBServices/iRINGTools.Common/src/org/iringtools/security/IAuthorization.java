package org.iringtools.security;

import javax.servlet.ServletContext;
import javax.servlet.http.HttpSession;

public interface IAuthorization
{
  boolean authorize(ServletContext context, HttpSession session, String application, String user);
}
