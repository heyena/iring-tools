using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;

namespace TestService
{
  [ServiceContract]
  public interface IService
  {
    [OperationContract]
    [WebGet(UriTemplate = "/hello")]
    string SayHello();

    [OperationContract]
    [FaultContract(typeof(ArithmeticFault))]
    [WebGet(UriTemplate = "/fault")]
    string GetFault();
  }
}
