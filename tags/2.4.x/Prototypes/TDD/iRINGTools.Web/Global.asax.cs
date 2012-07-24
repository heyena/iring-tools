using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Ninject;
using Ninject.Web.Common;
using Ninject.Extensions.Wcf;

using iRINGTools.Data;
using iRINGTools.Services;
using iRINGTools.Tests;

using Ninject.Parameters;

namespace iRINGTools.Web
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : NinjectHttpApplication
  {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      //routes.Add(new ServiceRoute("AdapterService", new NinjectServiceHostFactory(), typeof(AdapterWCFService)));

      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
      );

    }

    public static void RegisterModuels(IKernel kernel)
    {
      kernel.Load(new ServiceModule());
    }

    protected override void OnApplicationStarted()
    {
      AreaRegistration.RegisterAllAreas();

      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);
    }

    protected override IKernel CreateKernel()
    {
      IKernel kernel = new StandardKernel();
      NinjectServiceHostFactory.SetKernel(kernel);

      RegisterModuels(kernel);

      return kernel;
    }
  }
}