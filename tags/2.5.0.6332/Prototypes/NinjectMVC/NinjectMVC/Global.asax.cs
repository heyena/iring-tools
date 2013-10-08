using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Mvc;
using System.Web.Security;
using NinjectMVC.Models;

namespace NinjectMVC
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : NinjectHttpApplication //System.Web.HttpApplication
  {
    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
      );

      

    }

    protected override void OnApplicationStarted()
    {
      AreaRegistration.RegisterAllAreas();
      RegisterRoutes(RouteTable.Routes);
    }

    protected override IKernel CreateKernel()
    {
      var modules = new INinjectModule[]
      {
          new ServiceModule()
      };

      IKernel kernel = new StandardKernel(modules);
      kernel.Load("*.dll");
      
      return kernel;
    }
  }

  internal class ServiceModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IFormsAuthenticationService>().To<FormsAuthenticationService>();
      Bind<IMembershipService>().To<AccountMembershipService>();
      Bind<MembershipProvider>().ToConstant(Membership.Provider);      
    }
  }
}