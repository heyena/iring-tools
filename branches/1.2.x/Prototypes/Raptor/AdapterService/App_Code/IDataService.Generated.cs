

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by IDataService.tt.
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
	public interface IDataService
	{    
		[OperationContract]
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Valves))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Instruments))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Vessels))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Lines))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.DEF.Lines))]		
		DTOResponse GetData(DTORequest identifier);
		
		[OperationContract]
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Valves))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Instruments))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Vessels))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.ABC.Lines))]		
		[ServiceKnownType(typeof(org.iringtools.adapter.proj_12345_000.DEF.Lines))]		
		DTOListResponse GetDataList(DTORequest identifier);
	}
}