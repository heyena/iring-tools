using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using org.iringtools.library;

namespace org.iringtools.adapter
{
  public partial interface IDataService
  {
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/pull")]
    Response Pull(Request request);
  }
}
