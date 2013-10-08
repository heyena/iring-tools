package org.iringtools.controllers;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import javax.servlet.ServletContext;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.apache.log4j.Logger;
import org.apache.struts2.ServletActionContext;
import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.common.Configuration;
import org.iringtools.common.Setting;
import org.iringtools.security.IAuthHeaders;
import org.iringtools.security.IAuthorization;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.JaxbUtils;

import com.opensymphony.xwork2.ActionSupport;

public abstract class BaseController extends ActionSupport implements SessionAware
{
  private static final long serialVersionUID = 1L;
  private static final Logger logger = Logger.getLogger(BaseController.class);

  protected String authenticatedUser;
  protected Map<String, Object> settings;
  protected ServletContext context;
  protected HttpServletRequest request;
  protected HttpServletResponse response;
  protected Map<String, Object> session;
  
  public BaseController() throws Exception
  {
    settings = new HashMap<String, Object>();
    context = ServletActionContext.getServletContext();
    request = ServletActionContext.getRequest();
    response = ServletActionContext.getResponse();
    
    HttpUtils.prepareHttpProxy(context);
    
    String basePath = context.getRealPath("/");
    settings.put("basePath", basePath);
    
    File appConfig = new File(basePath.concat("WEB-INF/config/app.xml"));
    
    if (appConfig.exists())
    {
      Configuration config = JaxbUtils.read(Configuration.class, appConfig.getPath());
    
      for (Setting setting : config.getSetting())
      {
        settings.put(setting.getName(), setting.getValue());
      }
    }
  }

  @Override
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }

  protected void authorize(String app)
  {
    String user = request.getRemoteUser();
    
    //
    // process authorization if a provider is configured
    //
    Object authProviderName = context.getInitParameter("AuthorizationProvider");    
    if (authProviderName != null)
    {
      logger.info("Using authorization provider: " + authProviderName);
      
      try
      {        
        @SuppressWarnings("unchecked")
        Class<IAuthorization> authProviderCls = (Class<IAuthorization>)Class.forName(authProviderName.toString());
        IAuthorization authProvider = authProviderCls.newInstance();
        
        boolean authorized = authProvider.authorize(request.getSession(), app, user);        
        if (!authorized)
        {
          request.getSession().invalidate();
          response.sendError(HttpServletResponse.SC_UNAUTHORIZED, "Unauthorized");          
        }
      }
      catch (Exception e)
      {
        logger.info("Error authorizing user [" + user + "]: " + e.toString());
        try
        {
          request.getSession().invalidate();
          response.sendError(408, e.getMessage());
        }
        catch (IOException ioe)
        {
          ioe.printStackTrace();
        }
      }
    }
    
    //
    // prepare authorization headers if a provider is configured
    //
    Object headersProviderName = context.getInitParameter("AuthHeadersProvider");    
    if (headersProviderName != null)
    {
      logger.info("Using headers provider: " + headersProviderName);
      
      try
      {
        @SuppressWarnings("unchecked")
        Class<IAuthHeaders> headersProviderCls = (Class<IAuthHeaders>)Class.forName(headersProviderName.toString());
        if (headersProviderCls == null)
        {
          logger.error("Unable to resolve headers provider type.");
        }
        
        IAuthHeaders headersProvider = headersProviderCls.newInstance();
        if (headersProvider == null)
        {
          logger.error("Unable to instantiate headers provider instance.");
        }
        
        Map<String, String> headers = headersProvider.get(request);
        
        if (headers != null)
        {
          for (Entry<String, String> entry : headers.entrySet())
          {
            settings.put("http-header-" + entry.getKey(), entry.getValue());
          }
        }
      }
      catch (Exception e) {
        e.printStackTrace();
        logger.info("Error creating authorization headers: " + e.toString());
      }
    }
  }
}
