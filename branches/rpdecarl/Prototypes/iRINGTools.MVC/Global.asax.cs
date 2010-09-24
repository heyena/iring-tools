﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace iRINGTools.WebMVC
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : System.Web.HttpApplication
  {
    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapRoute(
          "Default_ScopeApplicationGraph", // Route name
          "{controller}/{scope}/{application}/{graph}/{action}", // URL with parameters
          new { controller = "Home", action = "Index", scope = UrlParameter.Optional, application = UrlParameter.Optional, graph = UrlParameter.Optional } // Parameter defaults
      );

      routes.MapRoute(
          "Default_ScopeApplication", // Route name
          "{controller}/{scope}/{application}/{action}", // URL with parameters
          new { controller = "Home", action = "Index", scope = UrlParameter.Optional, application = UrlParameter.Optional } // Parameter defaults
      );

      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
      );
    }

    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();

      RegisterRoutes(RouteTable.Routes);
    }
  }
}