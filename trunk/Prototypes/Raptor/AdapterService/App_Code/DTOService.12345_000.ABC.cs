

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a DTOService.tt.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Xml.Xsl;
using Ninject;
using Ninject.Modules;
using org.iringtools.adapter;
using org.iringtools.adapter.dataLayer;
using org.iringtools.library;
using org.iringtools.utility;

namespace org.iringtools.adapter.proj_12345_000.ABC
{
	public class DTOService : IDTOService
	{
		IKernel _kernel = null;
		IDataLayer _dataLayer = null;
		AdapterSettings _settings = null;

		[Inject]
		public DTOService(IKernel kernel, IDataLayer dataLayer, AdapterSettings settings)
		{
		  _kernel = kernel;
		  _dataLayer = dataLayer;
		  _settings = settings;
		}

		public T TransformList<T>(string graphName, List<DataTransferObject> dtoList, string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer)
		{
			string dtoPath = xmlPath + graphName + "DTO.xml";
			Mapping mapping = Utility.Read<Mapping>(mappingUri, false);

			switch (graphName)
			{ 
			case "Valves":
				List<Valves> ValvesList = new List<Valves>();

				foreach (DataTransferObject dto in dtoList)
				{
					ValvesList.Add((Valves)dto);
				}

				Utility.Write<List<Valves>>(ValvesList, dtoPath);
				break;
				
			case "Instruments":
				List<Instruments> InstrumentsList = new List<Instruments>();

				foreach (DataTransferObject dto in dtoList)
				{
					InstrumentsList.Add((Instruments)dto);
				}

				Utility.Write<List<Instruments>>(InstrumentsList, dtoPath);
				break;
				
			case "Vessels":
				List<Vessels> VesselsList = new List<Vessels>();

				foreach (DataTransferObject dto in dtoList)
				{
					VesselsList.Add((Vessels)dto);
				}

				Utility.Write<List<Vessels>>(VesselsList, dtoPath);
				break;
				
			case "Lines":
				List<Lines> LinesList = new List<Lines>();

				foreach (DataTransferObject dto in dtoList)
				{
					LinesList.Add((Lines)dto);
				}

				Utility.Write<List<Lines>>(LinesList, dtoPath);
				break;
				
			}

			XsltArgumentList xsltArgumentList = new XsltArgumentList();
			xsltArgumentList.AddParam("dtoFilename", "", dtoPath);

			return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);
		}
    
		public DataTransferObject Create(string graphName, string identifier)
		{
			DataTransferObject dto = null;

			switch (graphName)
			{ 
			case "Valves":				
			  dto = new Valves("http://rdl.rdlfacade.org/data#R97295617945", graphName, identifier);
			  break;
			  
			case "Instruments":				
			  dto = new Instruments("http://rdl.rdlfacade.org/data#R49707845396", graphName, identifier);
			  break;
			  
			case "Vessels":				
			  dto = new Vessels("http://rdl.rdlfacade.org/data#R75598586594", graphName, identifier);
			  break;
			  
			case "Lines":				
			  dto = new Lines("http://rdl.rdlfacade.org/data#R19192462550", graphName, identifier);
			  break;
			  
			}

			return dto;
		}
    
		public List<DataTransferObject> CreateList(string graphName, List<string> identifiers)
		{
			List<DataTransferObject> dtoList = new List<DataTransferObject>();

			foreach (string identifier in identifiers)
			{
				dtoList.Add(Create(graphName, identifier));
			}

			return dtoList;
		}

		public DataTransferObject GetDTO(string graphName, string identifier)
		{
			DataTransferObject dto = null;		

			switch (graphName)
			{ 
			case "Valves":
				var ValvesDO = 
					(from InLinePipingComponentList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
					where InLinePipingComponentList.tag == identifier && InLinePipingComponentList.componentType.ToUpper() == "VALVE"  // outFilter
					select InLinePipingComponentList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>();
	            
				if (ValvesDO != default(org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent))
				{
					dto = new Valves(ValvesDO);
					dto.Identifier = ValvesDO.Id;
				}
				
				break;

			case "Instruments":
				var InstrumentsDO = 
					(from InLinePipingComponentList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
					where InLinePipingComponentList.tag == identifier && InLinePipingComponentList.componentType.ToUpper() == "INSTRUMENT"  // outFilter
					select InLinePipingComponentList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>();
	            
				if (InstrumentsDO != default(org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent))
				{
					dto = new Instruments(InstrumentsDO);
					dto.Identifier = InstrumentsDO.Id;
				}
				
				break;

			case "Vessels":
				var KOPotDO = 
					(from KOPotList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.KOPot>()
					where KOPotList.tag == identifier
					select KOPotList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.ABC.KOPot>();   
	            
				if (KOPotDO != default(org.iringtools.adapter.proj_12345_000.ABC.KOPot))
				{                        
					dto = new Vessels(KOPotDO);
					dto.Identifier = KOPotDO.Id;
					break; 
				}
								
				var VacuumTowerDO = 
					(from VacuumTowerList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower>()
					where VacuumTowerList.tag == identifier
					select VacuumTowerList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower>();   
	            
				if (VacuumTowerDO != default(org.iringtools.adapter.proj_12345_000.ABC.VacuumTower))
				{                        
					dto = new Vessels(VacuumTowerDO);
					dto.Identifier = VacuumTowerDO.Id;
					break; 
				}
								
				break;

			case "Lines":
				var LineDO = 
					(from LineList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.Line>()
					where LineList.tag == identifier
					select LineList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.ABC.Line>();   
	            
				if (LineDO != default(org.iringtools.adapter.proj_12345_000.ABC.Line))
				{                        
					dto = new Lines(LineDO);
					dto.Identifier = LineDO.Id;
					break; 
				}
								
				break;

			}
      
			return dto;
		}
		
		public List<DataTransferObject> GetList(string graphName)
		{
			List<DataTransferObject> dtoList = new List<DataTransferObject>();

			switch (graphName)
			{ 
			case "Valves":
				var ValvesDOList = 
					from InLinePipingComponentList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
					where InLinePipingComponentList.componentType.ToUpper() == "VALVE"  // outFilter
					select InLinePipingComponentList;

				foreach (var ValvesDO in ValvesDOList)
				{   					
					Valves dto = new Valves(ValvesDO);
					dto.Identifier = ValvesDO.Id;
					dtoList.Add(dto);
				}   
         
				break;
				
			case "Instruments":
				var InstrumentsDOList = 
					from InLinePipingComponentList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
					where InLinePipingComponentList.componentType.ToUpper() == "INSTRUMENT"  // outFilter
					select InLinePipingComponentList;

				foreach (var InstrumentsDO in InstrumentsDOList)
				{   					
					Instruments dto = new Instruments(InstrumentsDO);
					dto.Identifier = InstrumentsDO.Id;
					dtoList.Add(dto);
				}   
         
				break;
				
			case "Vessels":
				var KOPotDOList = 
					from KOPotList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.KOPot>()
					select KOPotList;  
			    
				foreach (var KOPotDO in KOPotDOList)
				{   					
					Vessels dto = new Vessels(KOPotDO);
					dto.Identifier = KOPotDO.Id;
					dtoList.Add(dto);
				}   
				var VacuumTowerDOList = 
					from VacuumTowerList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower>()
					select VacuumTowerList;  
			    
				foreach (var VacuumTowerDO in VacuumTowerDOList)
				{   					
					Vessels dto = new Vessels(VacuumTowerDO);
					dto.Identifier = VacuumTowerDO.Id;
					dtoList.Add(dto);
				}   
         
				break;
				
			case "Lines":
				var LineDOList = 
					from LineList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.Line>()
					select LineList;  
			    
				foreach (var LineDO in LineDOList)
				{   					
					Lines dto = new Lines(LineDO);
					dto.Identifier = LineDO.Id;
					dtoList.Add(dto);
				}   
         
				break;
				
     
			}

			return dtoList;
		}
		
		public Dictionary<string, string> GetListREST(string graphName)
		{
			Dictionary<string, string> identifierUriPairs = new Dictionary<string, string>();
			String endpoint = OperationContext.Current.Channel.LocalAddress.ToString();
			
			switch (graphName)
			{ 
			case "Valves":
				var ValvesDOList = 
					from InLinePipingComponentList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
					where InLinePipingComponentList.componentType.ToUpper() == "VALVE"  // outFilter
					select InLinePipingComponentList;
			    
				foreach (var ValvesDO in ValvesDOList)
				{   
					string identifier = ValvesDO.Id;
					identifierUriPairs.Add(identifier, endpoint + "/" + graphName + "/" + identifier);            
				}   

				break;
				
			case "Instruments":
				var InstrumentsDOList = 
					from InLinePipingComponentList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
					where InLinePipingComponentList.componentType.ToUpper() == "INSTRUMENT"  // outFilter
					select InLinePipingComponentList;
			    
				foreach (var InstrumentsDO in InstrumentsDOList)
				{   
					string identifier = InstrumentsDO.Id;
					identifierUriPairs.Add(identifier, endpoint + "/" + graphName + "/" + identifier);            
				}   

				break;
				
			case "Vessels":
				var KOPotDOList = 
					from KOPotList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.KOPot>()
					select KOPotList;  

				foreach (var KOPotDO in KOPotDOList)
				{
					string identifier = KOPotDO.Id;
					identifierUriPairs.Add(identifier, endpoint + "/" + graphName + "/" + identifier);  
				}  

				var VacuumTowerDOList = 
					from VacuumTowerList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower>()
					select VacuumTowerList;  

				foreach (var VacuumTowerDO in VacuumTowerDOList)
				{
					string identifier = VacuumTowerDO.Id;
					identifierUriPairs.Add(identifier, endpoint + "/" + graphName + "/" + identifier);  
				}  

				break;
				
			case "Lines":
				var LineDOList = 
					from LineList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.Line>()
					select LineList;  

				foreach (var LineDO in LineDOList)
				{
					string identifier = LineDO.Id;
					identifierUriPairs.Add(identifier, endpoint + "/" + graphName + "/" + identifier);  
				}  

				break;
				
			}
			
			return identifierUriPairs;
		}
    
		public Response Post(string graphName, DataTransferObject dto)
		{
			Response response = new Response();

			if (dto != null)
			{
				switch (graphName)
				{ 
 
				case "Valves":
					org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent ValvesDO = (org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)dto.GetDataObject();
					response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>(ValvesDO));

					break;
 
				case "Instruments":
					org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent InstrumentsDO = (org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)dto.GetDataObject();
					response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>(InstrumentsDO));

					break;
 
				case "Vessels":
					Vessels VesselsObj = (Vessels)dto;  
					
					if (VesselsObj.tpl_FluidContainerDescription_description.ToUpper() == "KNOCK OUT VESSEL") // inFilter
					{
						org.iringtools.adapter.proj_12345_000.ABC.KOPot KOPotDO = (org.iringtools.adapter.proj_12345_000.ABC.KOPot)VesselsObj.GetDataObject();
						response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.ABC.KOPot>(KOPotDO));
					}   
					else if (VesselsObj.tpl_FluidContainerDescription_description.ToUpper() == "VACUUM VESSEL") // inFilter
					{
						org.iringtools.adapter.proj_12345_000.ABC.VacuumTower VacuumTowerDO = (org.iringtools.adapter.proj_12345_000.ABC.VacuumTower)VesselsObj.GetDataObject();
						response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower>(VacuumTowerDO));
					}   

					break;
 
				case "Lines":
					org.iringtools.adapter.proj_12345_000.ABC.Line LinesDO = (org.iringtools.adapter.proj_12345_000.ABC.Line)dto.GetDataObject();
					response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.ABC.Line>(LinesDO));

					break;
       
				}
			}

			return response;
		}
		
		public Response PostList(string graphName, List<DataTransferObject> dtoList)
		{
			Response response = new Response();

			if (dtoList != null && dtoList.Count<DataTransferObject>() > 0)
			{
				switch (graphName)
				{ 
 
				case "Valves":
					List<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent> ValvesDOList = new List<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>();

					foreach (DataTransferObject dto in dtoList)
					{
						ValvesDOList.Add((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)dto.GetDataObject());
					}

					response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>(ValvesDOList));

					break;
 
				case "Instruments":
					List<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent> InstrumentsDOList = new List<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>();

					foreach (DataTransferObject dto in dtoList)
					{
						InstrumentsDOList.Add((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)dto.GetDataObject());
					}

					response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>(InstrumentsDOList));

					break;
 
				case "Vessels":
					List<org.iringtools.adapter.proj_12345_000.ABC.KOPot> KOPotDOList = new List<org.iringtools.adapter.proj_12345_000.ABC.KOPot>();
					List<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower> VacuumTowerDOList = new List<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower>();
				    
					foreach (Vessels dto in dtoList)
					{   
						if (dto.tpl_FluidContainerDescription_description.ToUpper() == "KNOCK OUT VESSEL") // inFilter
						{
							KOPotDOList.Add((org.iringtools.adapter.proj_12345_000.ABC.KOPot)dto.GetDataObject());
						} 
						else if (dto.tpl_FluidContainerDescription_description.ToUpper() == "VACUUM VESSEL") // inFilter
						{
							VacuumTowerDOList.Add((org.iringtools.adapter.proj_12345_000.ABC.VacuumTower)dto.GetDataObject());
						} 
					}
					
					response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.ABC.KOPot>(KOPotDOList));
					response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.ABC.VacuumTower>(VacuumTowerDOList));

					break;
 
				case "Lines":
					List<org.iringtools.adapter.proj_12345_000.ABC.Line> LinesDOList = new List<org.iringtools.adapter.proj_12345_000.ABC.Line>();

					foreach (DataTransferObject dto in dtoList)
					{
						LinesDOList.Add((org.iringtools.adapter.proj_12345_000.ABC.Line)dto.GetDataObject());
					}

					response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.ABC.Line>(LinesDOList));

					break;
				}
			}

			return response;
		}
    
		public DataDictionary GetDictionary()
		{
			return _dataLayer.GetDictionary();
		}

		public Response RefreshDictionary()
		{
			return _dataLayer.RefreshDictionary();
		}
	}
}