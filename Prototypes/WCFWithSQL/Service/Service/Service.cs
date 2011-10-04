using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace WCFWithSQL
{
  [ServiceContract]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class Service
  {
    [WebGet(UriTemplate = "/reset")]
    public string Reset()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      Provider provider = new Provider();
      bool success = provider.ResetDB();

      return success ? "DB reset successfully" : "Error reseting DB";
    }
  }
}