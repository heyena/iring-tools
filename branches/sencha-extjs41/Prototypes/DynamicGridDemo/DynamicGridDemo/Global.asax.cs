using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DynamicGridDemo
{
  public class MvcApplication : HttpApplication
  {
    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      RegisterRoutes(RouteTable.Routes);
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapRoute(
          "DynamicGrid",  // Route name
          "{controller}/{action}/{dataObject}/{start}/{limit}/{sort}/{dir}/{filter}",  // URL with parameters
          new { controller = "DynamicGrid", action = "Index", dataObject = "", start = 0, limit = 25, 
            sort = "", dir = "", filter = "" }  // Parameter defaults
      );
    }
  }
}