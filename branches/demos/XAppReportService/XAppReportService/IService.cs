using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace XAppReportService
{
 [ServiceContract]
  public interface IService
  {
    [OperationContract]
    [WebGet(UriTemplate = "/list", ResponseFormat = WebMessageFormat.Json)]
    List<string> GetReportNames(); 

    [OperationContract]
    [WebGet(UriTemplate = "/report?name={name}", ResponseFormat = WebMessageFormat.Json)]
    Report GetReport(string name);  
  }
}
