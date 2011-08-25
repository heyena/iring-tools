using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace iRINGTools.Web.Services
{
  [ServiceContract]
  public interface IAdapterWCFService
  {
    [OperationContract]
    void DoWork();
  }
}
