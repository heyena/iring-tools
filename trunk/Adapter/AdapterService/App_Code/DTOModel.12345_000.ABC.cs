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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml.Xsl;
using org.iringtools.library;
using org.iringtools.utility;

namespace org.iringtools.adapter.proj_12345_000.ABC
{
  [DataContract(Name = "Valves", Namespace = "http://ABC.bechtel.com/12345_000/data#" )]
  [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
  public class Valves : DataTransferObject
  {
    [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    public class TemplateClientSystemAssembly
    {
      [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      public class ClassCLIENT_SYSTEM
      {
        [DataMember(EmitDefaultValue=false)]
        [XmlIgnore]
        public string Identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identificationType { get; set; }
      }
      
      [DataMember(Name = "tpl_whole_rdl_CLIENT_SYSTEM", EmitDefaultValue = false)]
      [XmlIgnore]
      public ClassCLIENT_SYSTEM tpl_whole_rdl_CLIENT_SYSTEM { get; set; }
    }
    
    [DataMember(EmitDefaultValue = false)]
    [XmlIgnore]
    public TemplateClientSystemAssembly tpl_ClientSystemAssembly { get; set; }
    
    [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    public class TemplateClientFunctionalUnitAssembly
    {
      [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      public class ClassCLIENT_FUNCTIONAL_UNIT
      {
        [DataMember(EmitDefaultValue=false)]
        [XmlIgnore]
        public string Identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identificationType { get; set; }
      }
      
      [DataMember(Name = "tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT", EmitDefaultValue = false)]
      [XmlIgnore]
      public ClassCLIENT_FUNCTIONAL_UNIT tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT { get; set; }
    }
    
    [DataMember(EmitDefaultValue = false)]
    [XmlIgnore]
    public TemplateClientFunctionalUnitAssembly tpl_ClientFunctionalUnitAssembly { get; set; }
    
    [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    public class TemplatePAndIDRepresentation
    {
      [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      public class ClassP_AND_I_DIAGRAM
      {
        [DataMember(EmitDefaultValue=false)]
        [XmlIgnore]
        public string Identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identificationType { get; set; }
      }
      
      [DataMember(Name = "tpl_representation_rdl_P_AND_I_DIAGRAM", EmitDefaultValue = false)]
      [XmlIgnore]
      public ClassP_AND_I_DIAGRAM tpl_representation_rdl_P_AND_I_DIAGRAM { get; set; }
    }
    
    [DataMember(EmitDefaultValue = false)]
    [XmlIgnore]
    public TemplatePAndIDRepresentation tpl_PAndIDRepresentation { get; set; }
    
    [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    public class TemplateProjectAssembly
    {
      [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      public class ClassPROJECT
      {
        [DataMember(EmitDefaultValue=false)]
        [XmlIgnore]
        public string Identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identificationType { get; set; }
      }
      
      [DataMember(Name = "tpl_whole_rdl_PROJECT", EmitDefaultValue = false)]
      [XmlIgnore]
      public ClassPROJECT tpl_whole_rdl_PROJECT { get; set; }
    }
    
    [DataMember(EmitDefaultValue = false)]
    [XmlIgnore]
    public TemplateProjectAssembly tpl_ProjectAssembly { get; set; }
    
    public Valves(string classId, string graphName, string identifier) : base(classId, graphName)
    {
      _properties.Add(new DTOProperty(@"tag", @"tpl:ClassifiedIdentification.tpl:identifier", null, typeof(String), true, true));
      _properties.Add(new DTOProperty(@"", @"tpl:ClassifiedIdentification.tpl:identificationType", @"<http://rdl.rdlfacade.org/data#R92093626759>", typeof(String), false, false));
      _properties.Add(new DTOProperty(@"", @"tpl:ClassifiedIdentification.tpl:fixedName1", @"fixedValue1", typeof(Int32), false, false));
      _properties.Add(new DTOProperty(@"system", @"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identifier", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"", @"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identificationType", @"<http://rdl.rdlfacade.org/data#R50548021125>", typeof(String), false, false));
      _properties.Add(new DTOProperty(@"unit", @"tpl:ClientFunctionalUnitAssembly.tpl:whole.rdl:CLIENT_FUNCTIONAL_UNIT.tpl:ClassifiedIdentification.tpl:identifier", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"", @"tpl:ClientFunctionalUnitAssembly.tpl:whole.rdl:CLIENT_FUNCTIONAL_UNIT.tpl:ClassifiedIdentification.tpl:identificationType", @"<http://rdl.rdlfacade.org/data#R82607969326>", typeof(String), false, false));
      _properties.Add(new DTOProperty(@"componentType", @"tpl:ClassifiedIdentification.tpl:identifier7", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"", @"tpl:ClassifiedIdentification.tpl:identificationType8", @"<http://rdl.rdlfacade.org/data#R99386812445>", typeof(String), false, false));
      _properties.Add(new DTOProperty(@"diameter", @"tpl:NominalDiameter.tpl:value", null, typeof(Double), false, false));
      _properties.Add(new DTOProperty(@"uomDiameter", @"tpl:NominalDiameter.tpl:scale", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"pid", @"tpl:PAndIDRepresentation.tpl:representation.rdl:P_AND_I_DIAGRAM.tpl:ClassifiedIdentification.tpl:identifier", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"", @"tpl:PAndIDRepresentation.tpl:representation.rdl:P_AND_I_DIAGRAM.tpl:ClassifiedIdentification.tpl:identificationType", @"<http://rdl.rdlfacade.org/data#R16893283050>", typeof(String), false, false));
      _properties.Add(new DTOProperty(@"projectNumber", @"tpl:ProjectAssembly.tpl:whole.rdl:PROJECT.tpl:ClassifiedIdentification.tpl:identifier", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"", @"tpl:ProjectAssembly.tpl:whole.rdl:PROJECT.tpl:ClassifiedIdentification.tpl:identificationType", @"<http://rdl.rdlfacade.org/data#R72529367339>", typeof(String), false, false));
      Identifier = identifier;
      ClassId = classId;
    }
    
    public Valves(org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent dataObject) : this("http://rdl.rdlfacade.org/data#R97295617945", "Valves", null, dataObject) {}
    
    public Valves(string classId, string graphName, string identifier, org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent dataObject) : this(classId, graphName, identifier)
    {
      if (dataObject != null)
      {
        tpl_ClassifiedIdentification_tpl_identifier = (String)dataObject.Id;
        tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier = (String)dataObject.system;
        tpl_ClientFunctionalUnitAssembly_tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT_tpl_ClassifiedIdentification_tpl_identifier = (String)dataObject.unit;
        tpl_ClassifiedIdentification_tpl_identifier7 = (String)dataObject.componentType;
        tpl_NominalDiameter_tpl_value = (Double)dataObject.diameter;
        tpl_NominalDiameter_tpl_scale = (String)dataObject.uomDiameter;
        tpl_PAndIDRepresentation_tpl_representation_rdl_P_AND_I_DIAGRAM_tpl_ClassifiedIdentification_tpl_identifier = (String)dataObject.pid;
        tpl_ProjectAssembly_tpl_whole_rdl_PROJECT_tpl_ClassifiedIdentification_tpl_identifier = (String)dataObject.projectNumber;
      }
      tpl_ClientSystemAssembly = new TemplateClientSystemAssembly();
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM = new TemplateClientSystemAssembly.ClassCLIENT_SYSTEM();
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM.Identifier = ((GetPropertyValueByInternalName("system") != null) ? GetPropertyValueByInternalName("system").ToString() : "");
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM.tpl_ClassifiedIdentification_tpl_identifier = tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier;
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM.tpl_ClassifiedIdentification_tpl_identificationType = tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identificationType;
      tpl_ClientFunctionalUnitAssembly = new TemplateClientFunctionalUnitAssembly();
      tpl_ClientFunctionalUnitAssembly.tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT = new TemplateClientFunctionalUnitAssembly.ClassCLIENT_FUNCTIONAL_UNIT();
      tpl_ClientFunctionalUnitAssembly.tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT.Identifier = ((GetPropertyValueByInternalName("unit") != null) ? GetPropertyValueByInternalName("unit").ToString() : "");
      tpl_ClientFunctionalUnitAssembly.tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT.tpl_ClassifiedIdentification_tpl_identifier = tpl_ClientFunctionalUnitAssembly_tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT_tpl_ClassifiedIdentification_tpl_identifier;
      tpl_ClientFunctionalUnitAssembly.tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT.tpl_ClassifiedIdentification_tpl_identificationType = tpl_ClientFunctionalUnitAssembly_tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT_tpl_ClassifiedIdentification_tpl_identificationType;
      tpl_PAndIDRepresentation = new TemplatePAndIDRepresentation();
      tpl_PAndIDRepresentation.tpl_representation_rdl_P_AND_I_DIAGRAM = new TemplatePAndIDRepresentation.ClassP_AND_I_DIAGRAM();
      tpl_PAndIDRepresentation.tpl_representation_rdl_P_AND_I_DIAGRAM.Identifier = ((GetPropertyValueByInternalName("pid") != null) ? GetPropertyValueByInternalName("pid").ToString() : "");
      tpl_PAndIDRepresentation.tpl_representation_rdl_P_AND_I_DIAGRAM.tpl_ClassifiedIdentification_tpl_identifier = tpl_PAndIDRepresentation_tpl_representation_rdl_P_AND_I_DIAGRAM_tpl_ClassifiedIdentification_tpl_identifier;
      tpl_PAndIDRepresentation.tpl_representation_rdl_P_AND_I_DIAGRAM.tpl_ClassifiedIdentification_tpl_identificationType = tpl_PAndIDRepresentation_tpl_representation_rdl_P_AND_I_DIAGRAM_tpl_ClassifiedIdentification_tpl_identificationType;
      tpl_ProjectAssembly = new TemplateProjectAssembly();
      tpl_ProjectAssembly.tpl_whole_rdl_PROJECT = new TemplateProjectAssembly.ClassPROJECT();
      tpl_ProjectAssembly.tpl_whole_rdl_PROJECT.Identifier = ((GetPropertyValueByInternalName("projectNumber") != null) ? GetPropertyValueByInternalName("projectNumber").ToString() : "");
      tpl_ProjectAssembly.tpl_whole_rdl_PROJECT.tpl_ClassifiedIdentification_tpl_identifier = tpl_ProjectAssembly_tpl_whole_rdl_PROJECT_tpl_ClassifiedIdentification_tpl_identifier;
      tpl_ProjectAssembly.tpl_whole_rdl_PROJECT.tpl_ClassifiedIdentification_tpl_identificationType = tpl_ProjectAssembly_tpl_whole_rdl_PROJECT_tpl_ClassifiedIdentification_tpl_identificationType;
      _dataObject = dataObject;
    }
    
    public Valves() : this("http://rdl.rdlfacade.org/data#R97295617945", "Valves", null) {}
    
    [DataMember(Name = "tpl_ClassifiedIdentification_tpl_identifier", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_ClassifiedIdentification_tpl_identifier
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClassifiedIdentification.tpl:identifier");
      }
      set
      {
        SetPropertyValue(@"tpl:ClassifiedIdentification.tpl:identifier", value);
      }
    }
    
    [DataMember(Name = "tpl_ClassifiedIdentification_tpl_identificationType", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_ClassifiedIdentification_tpl_identificationType
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClassifiedIdentification.tpl:identificationType");
      }
      set
      {
        SetPropertyValue(@"tpl:ClassifiedIdentification.tpl:identificationType", value);
      }
    }
    
    [DataMember(Name = "tpl_ClassifiedIdentification_tpl_fixedName1", EmitDefaultValue = false)]
    [XmlIgnore]
    public global::System.Nullable<Int32> tpl_ClassifiedIdentification_tpl_fixedName1
    {
      get
      {
        return (global::System.Nullable<Int32>)GetPropertyValue("tpl:ClassifiedIdentification.tpl:fixedName1");
      }
      set
      {
        SetPropertyValue(@"tpl:ClassifiedIdentification.tpl:fixedName1", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identifier");
      }
      set
      {
        SetPropertyValue(@"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identifier", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identificationType
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identificationType");
      }
      set
      {
        SetPropertyValue(@"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identificationType", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ClientFunctionalUnitAssembly_tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT_tpl_ClassifiedIdentification_tpl_identifier
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClientFunctionalUnitAssembly.tpl:whole.rdl:CLIENT_FUNCTIONAL_UNIT.tpl:ClassifiedIdentification.tpl:identifier");
      }
      set
      {
        SetPropertyValue(@"tpl:ClientFunctionalUnitAssembly.tpl:whole.rdl:CLIENT_FUNCTIONAL_UNIT.tpl:ClassifiedIdentification.tpl:identifier", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ClientFunctionalUnitAssembly_tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT_tpl_ClassifiedIdentification_tpl_identificationType
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClientFunctionalUnitAssembly.tpl:whole.rdl:CLIENT_FUNCTIONAL_UNIT.tpl:ClassifiedIdentification.tpl:identificationType");
      }
      set
      {
        SetPropertyValue(@"tpl:ClientFunctionalUnitAssembly.tpl:whole.rdl:CLIENT_FUNCTIONAL_UNIT.tpl:ClassifiedIdentification.tpl:identificationType", value);
      }
    }
    
    [DataMember(Name = "tpl_ClassifiedIdentification_tpl_identifier7", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_ClassifiedIdentification_tpl_identifier7
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClassifiedIdentification.tpl:identifier7");
      }
      set
      {
        SetPropertyValue(@"tpl:ClassifiedIdentification.tpl:identifier7", value);
      }
    }
    
    [DataMember(Name = "tpl_ClassifiedIdentification_tpl_identificationType8", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_ClassifiedIdentification_tpl_identificationType8
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClassifiedIdentification.tpl:identificationType8");
      }
      set
      {
        SetPropertyValue(@"tpl:ClassifiedIdentification.tpl:identificationType8", value);
      }
    }
    
    [DataMember(Name = "tpl_NominalDiameter_tpl_value", EmitDefaultValue = false)]
    [XmlIgnore]
    public global::System.Nullable<Double> tpl_NominalDiameter_tpl_value
    {
      get
      {
        return (global::System.Nullable<Double>)GetPropertyValue("tpl:NominalDiameter.tpl:value");
      }
      set
      {
        SetPropertyValue(@"tpl:NominalDiameter.tpl:value", value);
      }
    }
    
    [DataMember(Name = "tpl_NominalDiameter_tpl_scale", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_NominalDiameter_tpl_scale
    {
      get
      {
        return (String)GetPropertyValue("tpl:NominalDiameter.tpl:scale");
      }
      set
      {
        SetPropertyValue(@"tpl:NominalDiameter.tpl:scale", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_PAndIDRepresentation_tpl_representation_rdl_P_AND_I_DIAGRAM_tpl_ClassifiedIdentification_tpl_identifier
    {
      get
      {
        return (String)GetPropertyValue("tpl:PAndIDRepresentation.tpl:representation.rdl:P_AND_I_DIAGRAM.tpl:ClassifiedIdentification.tpl:identifier");
      }
      set
      {
        SetPropertyValue(@"tpl:PAndIDRepresentation.tpl:representation.rdl:P_AND_I_DIAGRAM.tpl:ClassifiedIdentification.tpl:identifier", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_PAndIDRepresentation_tpl_representation_rdl_P_AND_I_DIAGRAM_tpl_ClassifiedIdentification_tpl_identificationType
    {
      get
      {
        return (String)GetPropertyValue("tpl:PAndIDRepresentation.tpl:representation.rdl:P_AND_I_DIAGRAM.tpl:ClassifiedIdentification.tpl:identificationType");
      }
      set
      {
        SetPropertyValue(@"tpl:PAndIDRepresentation.tpl:representation.rdl:P_AND_I_DIAGRAM.tpl:ClassifiedIdentification.tpl:identificationType", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ProjectAssembly_tpl_whole_rdl_PROJECT_tpl_ClassifiedIdentification_tpl_identifier
    {
      get
      {
        return (String)GetPropertyValue("tpl:ProjectAssembly.tpl:whole.rdl:PROJECT.tpl:ClassifiedIdentification.tpl:identifier");
      }
      set
      {
        SetPropertyValue(@"tpl:ProjectAssembly.tpl:whole.rdl:PROJECT.tpl:ClassifiedIdentification.tpl:identifier", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ProjectAssembly_tpl_whole_rdl_PROJECT_tpl_ClassifiedIdentification_tpl_identificationType
    {
      get
      {
        return (String)GetPropertyValue("tpl:ProjectAssembly.tpl:whole.rdl:PROJECT.tpl:ClassifiedIdentification.tpl:identificationType");
      }
      set
      {
        SetPropertyValue(@"tpl:ProjectAssembly.tpl:whole.rdl:PROJECT.tpl:ClassifiedIdentification.tpl:identificationType", value);
      }
    }
    
    public override object GetDataObject()
    {
      if (_dataObject == null)
      {
        _dataObject = new org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent();
        ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).Id = (String)this.Identifier;
      }
      ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).system = (String)this.tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier;
      ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).unit = (String)this.tpl_ClientFunctionalUnitAssembly_tpl_whole_rdl_CLIENT_FUNCTIONAL_UNIT_tpl_ClassifiedIdentification_tpl_identifier;
      ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).componentType = (String)this.tpl_ClassifiedIdentification_tpl_identifier7;
      ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).diameter = (Double)this.tpl_NominalDiameter_tpl_value;
      ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).uomDiameter = (String)this.tpl_NominalDiameter_tpl_scale;
      ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).pid = (String)this.tpl_PAndIDRepresentation_tpl_representation_rdl_P_AND_I_DIAGRAM_tpl_ClassifiedIdentification_tpl_identifier;
      ((org.iringtools.adapter.proj_12345_000.ABC.InLinePipingComponent)_dataObject).projectNumber = (String)this.tpl_ProjectAssembly_tpl_whole_rdl_PROJECT_tpl_ClassifiedIdentification_tpl_identifier;
      return _dataObject;
    }
    
    public override string Serialize()
    {
      return Utility.SerializeDataContract<Valves>(this);
    }
    
    public override void Write(string path)
    {
      Utility.Write<Valves>(this, path);
    }
    
    public override T Transform<T>(string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer)
    {
      string dtoPath = xmlPath + this.GraphName + ".xml";
      Mapping mapping = Utility.Read<Mapping>(mappingUri, false);
      List<Valves> list = new List<Valves> { this };
      Utility.Write<List<Valves>>(list, dtoPath);
      XsltArgumentList xsltArgumentList = new XsltArgumentList();
      xsltArgumentList.AddParam("dtoFilename", String.Empty, dtoPath);
      return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);
    }
  }
  
  [DataContract(Name = "Lines", Namespace = "http://ABC.bechtel.com/12345_000/data#" )]
  [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
  public class Lines : DataTransferObject
  {
    [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
    public class TemplateClientSystemAssembly
    {
      [DataContract(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      [XmlRoot(Namespace = "http://ABC.bechtel.com/12345_000/data#")]
      public class ClassCLIENT_SYSTEM
      {
        [DataMember(EmitDefaultValue=false)]
        [XmlIgnore]
        public string Identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identifier { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public String tpl_ClassifiedIdentification_tpl_identificationType { get; set; }
      }
      
      [DataMember(Name = "tpl_whole_rdl_CLIENT_SYSTEM", EmitDefaultValue = false)]
      [XmlIgnore]
      public ClassCLIENT_SYSTEM tpl_whole_rdl_CLIENT_SYSTEM { get; set; }
    }
    
    [DataMember(EmitDefaultValue = false)]
    [XmlIgnore]
    public TemplateClientSystemAssembly tpl_ClientSystemAssembly { get; set; }
    
    public Lines(string classId, string graphName, string identifier) : base(classId, graphName)
    {
      _properties.Add(new DTOProperty(@"tag", @"tpl:ClassifiedIdentification.tpl:identifier", null, typeof(String), true, true));
      _properties.Add(new DTOProperty(@"", @"tpl:ClassifiedIdentification.tpl:identificationType", @"<http://rdl.rdlfacade.org/data#R92093626759>", typeof(String), false, false));
      _properties.Add(new DTOProperty(@"system", @"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identifier", null, typeof(String), false, false));
      _properties.Add(new DTOProperty(@"", @"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identificationType", @"<http://rdl.rdlfacade.org/data#R50548021125>", typeof(String), false, false));
      _properties.Add(new DTOProperty(@"diameter", @"tpl:NominalDiameter.tpl:value", null, typeof(Double), false, false));
      _properties.Add(new DTOProperty(@"uomDiameter", @"tpl:NominalDiameter.tpl:scale", null, typeof(String), false, false));
      Identifier = identifier;
      ClassId = classId;
    }
    
    public Lines(org.iringtools.adapter.proj_12345_000.ABC.Line dataObject) : this("http://rdl.rdlfacade.org/data#R19192462550", "Lines", null, dataObject) {}
    
    public Lines(string classId, string graphName, string identifier, org.iringtools.adapter.proj_12345_000.ABC.Line dataObject) : this(classId, graphName, identifier)
    {
      if (dataObject != null)
      {
        tpl_ClassifiedIdentification_tpl_identifier = (String)dataObject.Id;
        tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier = (String)dataObject.system;
        tpl_NominalDiameter_tpl_value = (Double)dataObject.diameter;
        tpl_NominalDiameter_tpl_scale = (String)dataObject.uomDiameter;
      }
      tpl_ClientSystemAssembly = new TemplateClientSystemAssembly();
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM = new TemplateClientSystemAssembly.ClassCLIENT_SYSTEM();
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM.Identifier = ((GetPropertyValueByInternalName("system") != null) ? GetPropertyValueByInternalName("system").ToString() : "");
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM.tpl_ClassifiedIdentification_tpl_identifier = tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier;
      tpl_ClientSystemAssembly.tpl_whole_rdl_CLIENT_SYSTEM.tpl_ClassifiedIdentification_tpl_identificationType = tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identificationType;
      _dataObject = dataObject;
    }
    
    public Lines() : this("http://rdl.rdlfacade.org/data#R19192462550", "Lines", null) {}
    
    [DataMember(Name = "tpl_ClassifiedIdentification_tpl_identifier", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_ClassifiedIdentification_tpl_identifier
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClassifiedIdentification.tpl:identifier");
      }
      set
      {
        SetPropertyValue(@"tpl:ClassifiedIdentification.tpl:identifier", value);
      }
    }
    
    [DataMember(Name = "tpl_ClassifiedIdentification_tpl_identificationType", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_ClassifiedIdentification_tpl_identificationType
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClassifiedIdentification.tpl:identificationType");
      }
      set
      {
        SetPropertyValue(@"tpl:ClassifiedIdentification.tpl:identificationType", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identifier");
      }
      set
      {
        SetPropertyValue(@"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identifier", value);
      }
    }
    
    [XmlIgnore]
    public String tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identificationType
    {
      get
      {
        return (String)GetPropertyValue("tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identificationType");
      }
      set
      {
        SetPropertyValue(@"tpl:ClientSystemAssembly.tpl:whole.rdl:CLIENT_SYSTEM.tpl:ClassifiedIdentification.tpl:identificationType", value);
      }
    }
    
    [DataMember(Name = "tpl_NominalDiameter_tpl_value", EmitDefaultValue = false)]
    [XmlIgnore]
    public global::System.Nullable<Double> tpl_NominalDiameter_tpl_value
    {
      get
      {
        return (global::System.Nullable<Double>)GetPropertyValue("tpl:NominalDiameter.tpl:value");
      }
      set
      {
        SetPropertyValue(@"tpl:NominalDiameter.tpl:value", value);
      }
    }
    
    [DataMember(Name = "tpl_NominalDiameter_tpl_scale", EmitDefaultValue = false)]
    [XmlIgnore]
    public String tpl_NominalDiameter_tpl_scale
    {
      get
      {
        return (String)GetPropertyValue("tpl:NominalDiameter.tpl:scale");
      }
      set
      {
        SetPropertyValue(@"tpl:NominalDiameter.tpl:scale", value);
      }
    }
    
    public override object GetDataObject()
    {
      if (_dataObject == null)
      {
        _dataObject = new org.iringtools.adapter.proj_12345_000.ABC.Line();
        ((org.iringtools.adapter.proj_12345_000.ABC.Line)_dataObject).Id = (String)this.Identifier;
      }
      ((org.iringtools.adapter.proj_12345_000.ABC.Line)_dataObject).system = (String)this.tpl_ClientSystemAssembly_tpl_whole_rdl_CLIENT_SYSTEM_tpl_ClassifiedIdentification_tpl_identifier;
      ((org.iringtools.adapter.proj_12345_000.ABC.Line)_dataObject).diameter = (Double)this.tpl_NominalDiameter_tpl_value;
      ((org.iringtools.adapter.proj_12345_000.ABC.Line)_dataObject).uomDiameter = (String)this.tpl_NominalDiameter_tpl_scale;
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
      xsltArgumentList.AddParam("dtoFilename", String.Empty, dtoPath);
      return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);
    }
  }
}
