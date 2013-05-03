package org.iringtools.filter;

import java.io.IOException;

import javax.servlet.Filter;
import javax.servlet.FilterChain;
import javax.servlet.FilterConfig;
import javax.servlet.ServletException;
import javax.servlet.ServletRequest;
import javax.servlet.ServletResponse;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

import org.apache.log4j.Logger;

public class SessionFilter implements Filter
{
  private static final Logger logger = Logger.getLogger(SessionFilter.class);
  protected FilterConfig fConfig = null;

  public SessionFilter() {}

  public void init(FilterConfig fConfig) throws ServletException
  {
    this.fConfig = fConfig;
  }

  public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain) throws IOException,
      ServletException
  {
    HttpServletRequest request = (HttpServletRequest) req;
    HttpServletResponse response = (HttpServletResponse) res;
    
    HttpSession session = ((HttpServletRequest) request).getSession();    
    String reqType = request.getHeader("x-requested-with"); 
    
    if (session != null && session.isNew())
    {
      logger.info("New session: " + session.getId());   
      
      if (reqType != null && reqType.equalsIgnoreCase("XMLHttpRequest"))
        response.sendError(408, "Session timed out.");
    }
    
    if (!response.isCommitted())
      chain.doFilter(request, response);
  }

  public void destroy()
  {
    fConfig = null;
  }
}
