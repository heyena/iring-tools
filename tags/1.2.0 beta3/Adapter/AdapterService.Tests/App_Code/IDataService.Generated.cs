//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace org.iringtools.adapter
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  public partial interface IDataService
  {
    [OperationContract]
    [ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Valves))]
    [ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Lines))]
    DTOListResponse GetDataList(DTORequest request);
    
    [OperationContract]
    [ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Valves))]
    [ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Lines))]
    DTOResponse GetData(DTORequest request);
  }
}
