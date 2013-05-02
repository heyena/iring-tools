using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using System.Net;
using System.Configuration;
using org.iringtools.library;

namespace org.iringtools.web.controllers
{
  public abstract class BaseController : Controller
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseController));

    protected string _authenticatedUser;
    protected IDictionary<string, string> _authHeaders;

    public BaseController()
    {
      try
      {
        //
        // process authentication if a provider is configured
        //
        string authNProviderName = ConfigurationManager.AppSettings["AuthenticationProvider"];

        if (!string.IsNullOrEmpty(authNProviderName))
        {
          Type authNProviderType = Type.GetType(authNProviderName);
          if (authNProviderType == null)
          {
            Send401Error("Unable to load authentication provider.");
          }

          IAuthentication authNProvider = (IAuthentication)Activator.CreateInstance(authNProviderType);
          _authenticatedUser = authNProvider.Authenticate(System.Web.HttpContext.Current.Session);

          if (System.Web.HttpContext.Current.Response.IsRequestBeingRedirected)
            return;

          if (string.IsNullOrEmpty(_authenticatedUser))
          {
            Send401Error("Authentication failed.");
          }

          //
          // process authorization if a provider is configured
          //
          string authZProviderName = ConfigurationManager.AppSettings["AuthorizationProvider"];

          if (!string.IsNullOrEmpty(authZProviderName))
          {
            Type authZProviderType = Type.GetType(authZProviderName);

            if (authZProviderType == null)
            {
              Send401Error("Unable to load authorization provider.");
            }

            IAuthorization authZProvider = (IAuthorization)Activator.CreateInstance(authZProviderType);
            bool authorized = authZProvider.Authorize(System.Web.HttpContext.Current.Session, "adapterAdmins", _authenticatedUser);

            if (!authorized)
            {
              Send401Error("User [" + _authenticatedUser + "] not authorized.");
            }
          }

          //
          // get authorization headers if a provider is configured
          //
          try
          {
            string headersProviderName = ConfigurationManager.AppSettings["AuthHeadersProvider"];

            if (!string.IsNullOrEmpty(headersProviderName))
            {
              Type headersProviderType = Type.GetType(headersProviderName);

              if (headersProviderType == null)
              {
                Send401Error("Unable to load auth header provider.");
              }

              IAuthHeaders headersProvider = (IAuthHeaders)Activator.CreateInstance(headersProviderType);
              _authHeaders = headersProvider.Get(_authenticatedUser);
            }
          }
          catch (Exception e)
          {
            _logger.Error("Error getting authorization headers: " + e);
            throw e;
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Authentication error: " + e.Message + ": " + e.StackTrace.ToString());
        Send401Error(e.ToString());
      }
    }

    protected void Send401Error(string error)
    {
      System.Web.HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      System.Web.HttpContext.Current.Response.Write(error);
      System.Web.HttpContext.Current.Response.End();
    }
  }
}