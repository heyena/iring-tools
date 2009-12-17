

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

namespace org.iringtools.adapter.proj_12345_000.DEF
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
			case "Lines":
				var LineDO = 
					(from LineList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.DEF.Line>()
					where LineList.tag == identifier
					select LineList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.DEF.Line>();   
	            
				if (LineDO != default(org.iringtools.adapter.proj_12345_000.DEF.Line))
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
			case "Lines":
				var LineDOList = 
					from LineList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.DEF.Line>()
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
			case "Lines":
				var LineDOList = 
					from LineList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.DEF.Line>()
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
 
				case "Lines":
					org.iringtools.adapter.proj_12345_000.DEF.Line LinesDO = (org.iringtools.adapter.proj_12345_000.DEF.Line)dto.GetDataObject();
					response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.DEF.Line>(LinesDO));

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
 
				case "Lines":
					List<org.iringtools.adapter.proj_12345_000.DEF.Line> LinesDOList = new List<org.iringtools.adapter.proj_12345_000.DEF.Line>();

					foreach (DataTransferObject dto in dtoList)
					{
						LinesDOList.Add((org.iringtools.adapter.proj_12345_000.DEF.Line)dto.GetDataObject());
					}

					response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.DEF.Line>(LinesDOList));

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

    public object ConvertXmlToType(string graphName, string dtoListString)
    {
      return null;
    }
	}
}