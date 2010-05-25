using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace AdapterPrototype
{
  [DataContract]
  [XmlRoot(ElementName = "qxf", Namespace = "http://ns.ids-adi.org/qxf/schema#")]
  public class Graph
  {
    public static string DM  = "http://dm.rdswip.org/data#";
    public static string RDL = "http://rdl.rdlfacade.org/data#";
    public static string TPL = "http://tpl.rdlfacade.org/data#";
    public static string OWL = "http://www.w3.org/2002/07/owl#";
    public static string EG  = "http://www.example.com/data#";
    public static string XSD = "http://www.w3.org/2001/XMLSchema#";
    
    public Graph()
    {
      Templates = new Dictionary<string, Template>();
      DataObjects = new Dictionary<string, object>();

      Classification = new Template();
      Classification.TemplateId = DM + "classification";

      Role classificationRole = new Role();
      classificationRole.RoleId = DM + "class";
      Classification.Roles.Add(classificationRole);

      Role graphInstanceRole = new Role();
      graphInstanceRole.RoleId = DM + "instance";
      Classification.Roles.Add(graphInstanceRole);

      //// classification template
      //Template classificationTemplate = new Template();
      //classificationTemplate.TemplateId = DM + "classification";

      //Role classificationRole = new Role();
      //classificationRole.RoleId = DM + "class";
      //classificationTemplate.Roles.Add(classificationRole);

      //Role graphInstanceRole = new Role();
      //graphInstanceRole.RoleId = DM + "instance";
      //classificationTemplate.Roles.Add(graphInstanceRole);
      //Templates.Add("guid1", classificationTemplate);
    }

    [XmlElement(ElementName = "relationship")]
    public Template Classification { get; set; }

    [DataMember]
    [XmlIgnore]
    public string ClassId
    {
      get
      {
        //return Templates.First().Value.Roles.First().Reference;
        return Classification.Roles.First().Reference;
      }
      set
      {
        //Templates.First().Value.Roles.First().Reference = value;
        Classification.Roles.First().Reference = value;
      }
    }

    [DataMember]
    [XmlIgnore]
    public string Name { get; set; }

    [DataMember]
    [XmlIgnore]
    public string Identifier {get; set;}

    [XmlIgnore]
    public string IdentifierValue
    {
      set
      {
        //Templates.First().Value.Roles[1].Reference = EG + value;
        Classification.Roles[1].Reference = EG + value;
      }
    }

    [DataMember]
    [XmlIgnore]
    public Dictionary<string, Template> Templates { get; set; }  // Dictionary<guid, template>

    [XmlElement(ElementName = "relationship")]
    public List<Template> TemplateList
    {
      get
      {
        return Templates.Values.ToList<Template>();
      }
    }

    [DataMember]
    [XmlIgnore]
    public Dictionary<string, object> DataObjects { get; set; }  // Dictionary<name, dataobject>

    [DataMember]
    [XmlIgnore]
    public List<ValueMap> ValueMaps { get; set; }

    [DataMember]
    [XmlIgnore]
    public string Version { get; set; }
  }

  [DataContract]
  [XmlRoot]
  public class Template
  {
    public Template()
    {
      Roles = new List<Role>();

      Role classRole = new Role();
      classRole.Reference = "instanceOf"; // need parent(class/graph)
      Roles.Add(classRole);
    }

    [DataMember]
    [XmlAttribute(AttributeName = "instance-of")]
    public string TemplateId { get; set; }

    [DataMember]
    [XmlIgnore]
    public string Name { get; set; }

    [DataMember]
    [XmlIgnore]
    public string ClassRoleId 
    {
      get
      {
        return Roles[0].RoleId;
      }

      set
      {
        Roles[0].RoleId = value;
      }    
    }

    //[XmlElement(Type = typeof(PropertyRole), ElementName = "property", Namespace="http://example.com/PropertyRole")]
    //[XmlElement(Type = typeof(ReferenceRole), ElementName = "property", Namespace="http://example.com/ReferenceRole")]
    [DataMember]
    [XmlElement(ElementName = "property")]
    public List<Role> Roles { get; set; }
  }

  [DataContract]
  [XmlRoot]
  public class Role
  {
    [DataMember]
    [XmlAttribute(AttributeName = "instance-of")]
    public string RoleId { get; set; }

    [DataMember]
    [XmlIgnore]
    public string Name { get; set; }

    [DataMember]
    [XmlIgnore]
    public string TemplateGuid { get; set; }

    [DataMember]
    [XmlIgnore]
    public string PropertyName { get; set; }

    [DataMember]
    [XmlAttribute(AttributeName = "as")]
    public string DataType { get; set; }

    [DataMember]
    [XmlText]
    public string Value { get; set; }

    [DataMember]
    [XmlAttribute(AttributeName = "reference")]
    public string Reference { get; set; }
  }

  [DataContract]
  [XmlRoot]
  public class ValueMap
  {
    [DataMember]
    public string ValueList { get; set; }
    [DataMember]
    public string InternalValue { get; set; }
    [DataMember]
    public string ModelUri { get; set; }
  }
}
