using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iRINGTools.Web.Helpers
{
  public static class ViewExtentions
  {
    public static string GetSiteUrl(this ViewPage pg)
    {
      string Port = pg.ViewContext.HttpContext.Request.ServerVariables["SERVER_PORT"];
      if (Port == null || Port == "80" || Port == "443")
        Port = "";
      else
        Port = ":" + Port;

      string Protocol = pg.ViewContext.HttpContext.Request.ServerVariables["SERVER_PORT_SECURE"];
      if (Protocol == null || Protocol == "0")
        Protocol = "http://";
      else
        Protocol = "https://";

      string appPath = pg.ViewContext.HttpContext.Request.ApplicationPath;
      if (appPath == "/")
        appPath = "";

      string sOut = Protocol + pg.ViewContext.HttpContext.Request.ServerVariables["SERVER_NAME"] + Port + appPath;
      return sOut;
    }

    /// <summary>
    /// Creates a ReturnUrl for use with the Login page
    /// </summary>
    public static string GetReturnUrl(this ViewPage pg)
    {
      string returnUrl = "";

      if (pg.Html.ViewContext.HttpContext.Request.QueryString["ReturnUrl"] != null)
      {
        returnUrl = pg.Html.ViewContext.HttpContext.Request.QueryString["ReturnUrl"];
      }
      else
      {
        returnUrl = pg.Html.ViewContext.HttpContext.Request.Url.AbsoluteUri;
      }
      return returnUrl;
    }

    public static bool UserIsAdmin(this ViewPage pg)
    {
      return pg.User.IsInRole("Administrator");
    }

    public static bool UserIsAdmin(this ViewUserControl ctrl)
    {
      return ctrl.Page.User.IsInRole("Administrator");
    }
  }
}