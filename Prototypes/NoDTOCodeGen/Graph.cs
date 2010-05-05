using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AdapterPrototype
{
  [XmlRoot(ElementName = "qxf", Namespace = "http://ns.ids-adi.org/qxf/schema#")]
  public class Graph
  {
    [XmlIgnore]
    public string Id { get; set; }

    [XmlIgnore]
    public string Name { get; set; }

    [XmlIgnore]
    public string Identifier { get; set; }

    [XmlIgnore]
    public Dictionary<string, Template> Templates { get; set; }  // Dictionary<templateId, template>

    [XmlElement(ElementName = "relationship")]
    public List<Template> TemplateList
    {
      get
      {
        return Templates.Values.ToList<Template>();
      }
    }

    [XmlIgnore]
    public Dictionary<string, object> DataObjects { get; set; }  // Dictionary<name, dataobject>

    [XmlIgnore]
    public List<ValueMap> ValueMaps { get; set; }

    [XmlIgnore]
    public string Version { get; set; }
  }

  [XmlRoot]
  public class Template
  {
    [XmlAttribute(AttributeName = "instance-of")]
    public string Id { get; set; }

    [XmlIgnore]
    public string Name { get; set; }

    [XmlElement(Type = typeof(PropertyRole), ElementName = "property", Namespace="http://example.com/PropertyRole")]
    [XmlElement(Type = typeof(ReferenceRole), ElementName = "property", Namespace="http://example.com/ReferenceRole")]
    public List<Role> Roles { get; set; }
  }

  [XmlRoot]
  public abstract class Role
  {
    [XmlAttribute(AttributeName = "instance-of")]
    public string Id { get; set; }
    [XmlIgnore]
    public string Name { get; set; }
    [XmlIgnore]
    public string TemplateId { get; set; }
  }

  [XmlRoot]
  public class PropertyRole : Role
  {
    [XmlIgnore]
    public string Property { get; set; }
    [XmlAttribute(AttributeName = "as")]
    public string DataType { get; set; }
    [XmlText]
    public string Value { get; set; }
  }

  [XmlRoot]
  public class ReferenceRole : Role
  {
    [XmlAttribute(AttributeName = "reference")]
    public string Reference { get; set; }
  }

  public class ValueMap
  {
    public string ValueList { get; set; }
    public string InternalValue { get; set; }
    public string ModelUri { get; set; }
  }
}
