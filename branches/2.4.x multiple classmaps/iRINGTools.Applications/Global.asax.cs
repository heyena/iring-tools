using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using iRINGTools.Web.Models;
using System.Configuration;
using org.iringtools.library;

namespace iRINGTools.Web
{
  public class MvcApplication : HttpApplication
  {
    private void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

      routes.MapRoute(
          "MappingRoute", // Route name
          "mapping/{action}/{scope}/{application}/{graphMap}", // URL with parameters
          new { controller = "Mapping", action = "Index", scope = "", application = "", graphMap = UrlParameter.Optional } // Parameter defaults
      );

      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
      );
    }

    protected void Application_Start(object sender, EventArgs e)
    {
      AreaRegistration.RegisterAllAreas();
      RegisterRoutes(RouteTable.Routes);
      log4net.Config.XmlConfigurator.Configure();
    }

    protected void Session_Start(object sender, EventArgs e)
    {
      SessionState.Start(Session.SessionID);
    }

    protected void Session_End(object sender, EventArgs e)
    {
      SessionState.End(Session.SessionID);
      Session.Abandon();
    }

    protected void Application_End(object sender, EventArgs e)
    {
      if (System.Web.HttpContext.Current.Session != null)
        System.Web.HttpContext.Current.Session.Clear();
    }
  }
}