using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Services.Description;

namespace Service
{
  public class Global : System.Web.HttpApplication
  {
    void Application_Start(object sender, EventArgs e)
    {
      RouteTable.Routes.Add(new ServiceRoute("service", new WebServiceHostFactory(), typeof(WCFWithSQL.Service)));
    }
  }
}