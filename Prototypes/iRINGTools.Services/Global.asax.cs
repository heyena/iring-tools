using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Services.Description;

namespace org.iringtools.adapter
{
  public class Global : HttpApplication
  {
    void Application_Start(object sender, EventArgs e)
    {
      RegisterRoutes();
    }

    private void RegisterRoutes()
    {
      // Edit the base address of AdapterService by replacing the "AdapterService" string below
      RouteTable.Routes.Add(new ServiceRoute("AdapterService", new WebServiceHostFactory(), typeof(org.iringtools.services.AdapterService)));
      RouteTable.Routes.Add(new ServiceRoute("ExchangeService", new WebServiceHostFactory(), typeof(org.iringtools.services.ExchangeService)));
      RouteTable.Routes.Add(new ServiceRoute("AppService", new WebServiceHostFactory(), typeof(org.iringtools.services.ApplicationService)));
      RouteTable.Routes.Add(new ServiceRoute("RefDataService", new WebServiceHostFactory(), typeof(org.iringtools.services.ReferenceDataService)));

    }
  }
}
