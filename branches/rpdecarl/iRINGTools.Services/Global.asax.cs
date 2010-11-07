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
      RouteTable.Routes.Add(new ServiceRoute("adapter", new WebServiceHostFactory(), typeof(org.iringtools.services.AdapterService)));
      RouteTable.Routes.Add(new ServiceRoute("hibernate", new WebServiceHostFactory(), typeof(org.iringtools.services.HibernateService)));
      RouteTable.Routes.Add(new ServiceRoute("refdata", new WebServiceHostFactory(), typeof(org.iringtools.services.ReferenceDataService)));
      RouteTable.Routes.Add(new ServiceRoute("dxfr", new WebServiceHostFactory(), typeof(org.iringtools.services.DtoService)));
    }
  }
}
