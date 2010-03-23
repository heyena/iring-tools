//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Ninject;
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
      string dtoPath = xmlPath + "Mapping.12345_000.ABC.DTO.xml";
      Mapping mapping = Utility.Read<Mapping>(mappingUri, false);
      
      switch (graphName)
      {
        case "Valves":
        {
          List<org.iringtools.adapter.proj_12345_000.ABC.Valves> doList = new List<org.iringtools.adapter.proj_12345_000.ABC.Valves>();
          
          foreach (DataTransferObject dto in dtoList)
          {
            doList.Add((org.iringtools.adapter.proj_12345_000.ABC.Valves)dto);
          }
          
          Utility.Write<List<org.iringtools.adapter.proj_12345_000.ABC.Valves>>(doList, dtoPath);
          break;
        }
        
        case "Lines":
        {
          List<org.iringtools.adapter.proj_12345_000.ABC.Lines> doList = new List<org.iringtools.adapter.proj_12345_000.ABC.Lines>();
          
          foreach (DataTransferObject dto in dtoList)
          {
            doList.Add((org.iringtools.adapter.proj_12345_000.ABC.Lines)dto);
          }
          
          Utility.Write<List<org.iringtools.adapter.proj_12345_000.ABC.Lines>>(doList, dtoPath);
          break;
        }
      }
      
      XsltArgumentList xsltArgumentList = new XsltArgumentList();
      xsltArgumentList.AddParam("dtoFilename", String.Empty, dtoPath);
      
      return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);
    }
    
    public DataTransferObject Create(string graphName, string identifier)
    {
      DataTransferObject dto = null;
      
      switch (graphName)
      {
        case "Valves":
          dto = new org.iringtools.adapter.proj_12345_000.ABC.Valves("http://rdl.rdlfacade.org/data#R97295617945", graphName, identifier);
          break;
        
        case "Lines":
          dto = new org.iringtools.adapter.proj_12345_000.ABC.Lines("http://rdl.rdlfacade.org/data#R19192462550", graphName, identifier);
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
        {
          var dataObject = 
            (from dataObjectList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
             where dataObjectList.tag == identifier
             select dataObjectList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>();   
        
          if (dataObject != default(org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent))
          {                        
            dto = new org.iringtools.adapter.proj_12345_000.ABC.Valves(dataObject);
            dto.Identifier = dataObject.tag;
            break; 
          }
          
          break;
        }
        
        case "Lines":
        {
          var dataObject = 
            (from dataObjectList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.Line>()
             where dataObjectList.tag == identifier
             select dataObjectList).FirstOrDefault<org.iringtools.adapter.proj_12345_000.ABC.Line>();   
        
          if (dataObject != default(org.iringtools.adapter.proj_12345_000.ABC.Line))
          {                        
            dto = new org.iringtools.adapter.proj_12345_000.ABC.Lines(dataObject);
            dto.Identifier = dataObject.tag;
            break; 
          }
          
          break;
        }
      }
      
      return dto;
    }
    
    public List<DataTransferObject> GetList(string graphName)
    {
      List<DataTransferObject> dtoList = new List<DataTransferObject>();
      
      switch (graphName)
      {
        case "Valves":
        {
          var dataObjectList = 
            from _dataObjectList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
            select _dataObjectList;  
    
          foreach (var dataObject in dataObjectList)
          {   					
            org.iringtools.adapter.proj_12345_000.ABC.Valves dto = new org.iringtools.adapter.proj_12345_000.ABC.Valves(dataObject);
            dto.Identifier = dataObject.tag;
            dtoList.Add(dto);
          }
          
          break;
        }
        
        case "Lines":
        {
          var dataObjectList = 
            from _dataObjectList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.Line>()
            select _dataObjectList;  
    
          foreach (var dataObject in dataObjectList)
          {   					
            org.iringtools.adapter.proj_12345_000.ABC.Lines dto = new org.iringtools.adapter.proj_12345_000.ABC.Lines(dataObject);
            dto.Identifier = dataObject.tag;
            dtoList.Add(dto);
          }
          
          break;
        }
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
        {
          var dataObjectList = 
            from _dataObjectList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>()
            select _dataObjectList;  

          foreach (var dataObject in dataObjectList)
          {
            string identifier = dataObject.tag;
            identifierUriPairs.Add(identifier, endpoint + "/" + graphName + "/" + identifier);  
          }
          
          break;
        }
        
        case "Lines":
        {
          var dataObjectList = 
            from _dataObjectList in _dataLayer.GetList<org.iringtools.adapter.proj_12345_000.ABC.Line>()
            select _dataObjectList;  

          foreach (var dataObject in dataObjectList)
          {
            string identifier = dataObject.tag;
            identifierUriPairs.Add(identifier, endpoint + "/" + graphName + "/" + identifier);  
          }
          
          break;
        }
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
          case "Valves":{
            org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent dataObject = (org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)dto.GetDataObject();
            response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>(dataObject));
            break;
          }
          case "Lines":{
            org.iringtools.adapter.proj_12345_000.ABC.Line dataObject = (org.iringtools.adapter.proj_12345_000.ABC.Line)dto.GetDataObject();
            response.Append(_dataLayer.Post<org.iringtools.adapter.proj_12345_000.ABC.Line>(dataObject));
            break;
          }}
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
          {
            List<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent> doList = new List<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>();

            foreach (DataTransferObject dto in dtoList)
            {
              doList.Add((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)dto.GetDataObject());
            }

            response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent>(doList));
            break;
          }
          
          case "Lines":
          {
            List<org.iringtools.adapter.proj_12345_000.ABC.Line> doList = new List<org.iringtools.adapter.proj_12345_000.ABC.Line>();

            foreach (DataTransferObject dto in dtoList)
            {
              doList.Add((org.iringtools.adapter.proj_12345_000.ABC.Line)dto.GetDataObject());
            }

            response.Append(_dataLayer.PostList<org.iringtools.adapter.proj_12345_000.ABC.Line>(doList));
            break;
          }
        }
      }
      
      return response;
    }
    
    public object CreateList(string graphName, string dtoListString)
    {
      List<DataTransferObject> dtoList = new List<DataTransferObject>();
      
      if (dtoListString != null && dtoListString != String.Empty)
      {
        switch (graphName)
        {
          case "Valves":
          {
            XmlReader reader = XmlReader.Create(new StringReader(dtoListString));
            XDocument file = XDocument.Load(reader);
            file = Utility.RemoveNamespace(file);
            List<org.iringtools.adapter.proj_12345_000.ABC.Valves> _dtoList = new List<org.iringtools.adapter.proj_12345_000.ABC.Valves>(); 
            var dtoResults = from c in file.Elements("Envelope").Elements("Payload").Elements("DataTransferObject") select c;

            foreach (var dtoResult in dtoResults)
            {
              var dtoProperties = from c in dtoResult.Elements("Properties").Elements("Property") select c;
              org.iringtools.adapter.proj_12345_000.ABC.Valves dto = new org.iringtools.adapter.proj_12345_000.ABC.Valves();

              foreach (var dtoProperty in dtoProperties)
              {
                for (int i = 0; i < dto._properties.Count; i++)
                {
                  if (dtoProperty.Attribute("name").Value == dto._properties[i].OIMProperty)
                  {
                    dto._properties[i].Value = dtoProperty.Attribute("value").Value.ToString();
                  }
                }
              }

              _dtoList.Add(dto);
            }

            foreach (org.iringtools.adapter.proj_12345_000.ABC.Valves dto in _dtoList)
            {
              dtoList.Add(dto);
            }
            
            break;
          }
          
          case "Lines":
          {
            XmlReader reader = XmlReader.Create(new StringReader(dtoListString));
            XDocument file = XDocument.Load(reader);
            file = Utility.RemoveNamespace(file);
            List<org.iringtools.adapter.proj_12345_000.ABC.Lines> _dtoList = new List<org.iringtools.adapter.proj_12345_000.ABC.Lines>(); 
            var dtoResults = from c in file.Elements("Envelope").Elements("Payload").Elements("DataTransferObject") select c;

            foreach (var dtoResult in dtoResults)
            {
              var dtoProperties = from c in dtoResult.Elements("Properties").Elements("Property") select c;
              org.iringtools.adapter.proj_12345_000.ABC.Lines dto = new org.iringtools.adapter.proj_12345_000.ABC.Lines();

              foreach (var dtoProperty in dtoProperties)
              {
                for (int i = 0; i < dto._properties.Count; i++)
                {
                  if (dtoProperty.Attribute("name").Value == dto._properties[i].OIMProperty)
                  {
                    dto._properties[i].Value = dtoProperty.Attribute("value").Value.ToString();
                  }
                }
              }

              _dtoList.Add(dto);
            }

            foreach (org.iringtools.adapter.proj_12345_000.ABC.Lines dto in _dtoList)
            {
              dtoList.Add(dto);
            }
            
            break;
          }
        }
      }
      
      return dtoList;
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
