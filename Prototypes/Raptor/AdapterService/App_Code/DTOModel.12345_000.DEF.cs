

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a DTOModel.tt.
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
using org.iringtools.library;
using org.iringtools.adapter.dataLayer;
using org.iringtools.utility;

namespace org.iringtools.adapter.proj_12345_000.DEF 
{
	[DataContract(Name = "Lines", Namespace = "http://def.bechtel.com/12345_000/data#")]
  [XmlRoot(Namespace = "http://def.bechtel.com/12345_000/data#")]
	public class Lines : DataTransferObject
	{
    [DataContract(Namespace = "http://def.bechtel.com/12345_000/data#")]
    [XmlRoot(Namespace = "http://def.bechtel.com/12345_000/data#")]
	  public class TemplateSystemPipingNetworkSystemAssembly
	{
    [DataContract(Namespace = "http://def.bechtel.com/12345_000/data#")]
    [XmlRoot(Namespace = "http://def.bechtel.com/12345_000/data#")]
		public class ClassSystem
		{
			[DataMember(EmitDefaultValue=false)]
			[XmlIgnore]
			public string Identifier { get; set; }
			
			[DataMember(EmitDefaultValue = false)]
			public String tpl_SystemName_identifier { get; set; }
					
		}

		[DataMember(Name = "hasClassOfWhole_rdl_System", EmitDefaultValue = false)]
		[XmlIgnore]
		public ClassSystem hasClassOfWhole_rdl_System { get; set; }
	}
	
	[DataMember(EmitDefaultValue = false)]
    [XmlIgnore]
    public TemplateSystemPipingNetworkSystemAssembly tpl_SystemPipingNetworkSystemAssembly { get; set; }     
       

		public Lines(string graphName, string identifier) 
			: base(graphName)
		{
      _properties.Add(new DTOProperty(@"tag", @"tpl_PipingNetworkSystemName_identifier", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"system", @"tpl_SystemName_identifier", null, typeof(String), false, false));

			Identifier = identifier;
		}
	
		public Lines(org.iringtools.adapter.proj_12345_000.DEF.Line dataObject)
			: this("Lines", null, dataObject) {}
			
		public Lines(string graphName, string identifier, org.iringtools.adapter.proj_12345_000.DEF.Line dataObject) 
			: this(graphName, identifier)
		{  
			if (dataObject != null)
			{
				tpl_PipingNetworkSystemName_identifier = (String)dataObject.tag;
				tpl_SystemName_identifier = (String)dataObject.system;
			}
			
			tpl_SystemPipingNetworkSystemAssembly = new TemplateSystemPipingNetworkSystemAssembly();
			tpl_SystemPipingNetworkSystemAssembly.hasClassOfWhole_rdl_System = new TemplateSystemPipingNetworkSystemAssembly.ClassSystem();
      tpl_SystemPipingNetworkSystemAssembly.hasClassOfWhole_rdl_System.Identifier = ((GetPropertyValue("tpl_PipingNetworkSystemName_identifier") != null) ? GetPropertyValue("tpl_PipingNetworkSystemName_identifier").ToString() : "");
			tpl_SystemPipingNetworkSystemAssembly.hasClassOfWhole_rdl_System.tpl_SystemName_identifier = tpl_SystemName_identifier;
	      
			_dataObject = dataObject;
		} 
		
		public Lines()
			: this("Lines", null) {}			

		[DataMember(Name = "tpl_PipingNetworkSystemName_identifier", EmitDefaultValue = false)]
		[XmlIgnore]
		public String tpl_PipingNetworkSystemName_identifier
		{
			get
			{
        return (String)GetPropertyValue("tpl_PipingNetworkSystemName_identifier");
			}

			set
			{
        SetPropertyValue("tpl_PipingNetworkSystemName_identifier", value);
			}
		}
		
		[XmlIgnore]
		public String tpl_SystemName_identifier
		{
			get
			{
        return (String)GetPropertyValue("tpl_SystemName_identifier");
			}

			set
			{
        SetPropertyValue("tpl_SystemName_identifier", value);
			}
		}
		
		public override object GetDataObject()
		{
				if (_dataObject == null)
				{
					_dataObject = new org.iringtools.adapter.proj_12345_000.DEF.Line();
				}
        ((org.iringtools.adapter.proj_12345_000.DEF.Line)_dataObject).tag = (String)this.tpl_PipingNetworkSystemName_identifier;
				((org.iringtools.adapter.proj_12345_000.DEF.Line)_dataObject).system = (String)this.tpl_SystemName_identifier;

			return _dataObject;
		}
		
		public override string Serialize()
		{
			return Utility.SerializeDataContract<Lines>(this);
		}

		public override void Write(string path)
		{
			Utility.Write<Lines>(this, path);
		}
	    
		public override T Transform<T>(string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer)
		{
			string dtoPath = xmlPath + this.GraphName + ".xml";
			Mapping mapping = Utility.Read<Mapping>(mappingUri, false);

			List<Lines> list = new List<Lines> { this };
			Utility.Write<List<Lines>>(list, dtoPath);

			XsltArgumentList xsltArgumentList = new XsltArgumentList();
			xsltArgumentList.AddParam("dtoFilename", "", dtoPath);

			return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);
		}		
	}
	
	
}

