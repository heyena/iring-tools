package org.iringtools.security;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletRequestWrapper;

public class RequestWrapper extends HttpServletRequestWrapper
{
  private String user = "";
  private String host = "";

  public RequestWrapper(HttpServletRequest request, String domain, String user, String host)
  {
    super(request);

    if (domain != null && domain.length() > 0)
    {
      this.user = domain + "\\";
    }

    if (user != null)
    {
      this.user += user;
    }

    this.host = host;
  }

  public String getRemoteUser()
  {
    return user;
  }

  public String getRemoteHost()
  {
    return host;
  }
}