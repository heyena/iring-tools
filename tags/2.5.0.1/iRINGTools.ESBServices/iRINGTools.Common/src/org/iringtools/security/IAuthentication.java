package org.iringtools.security;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

public interface IAuthentication
{
  String authenticate(HttpServletRequest request, HttpServletResponse response, HttpSession session);
}
