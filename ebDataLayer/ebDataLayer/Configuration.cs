using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace org.iringtools.adaper.datalayer.eb.config
{
  [XmlType("configuration")]
  public class Configuration
  {
    [XmlElement("template")]
    public Template Template { get; set; }

    [XmlArray("mappings")]
    public List<Mapping> Mappings { get; set; }
  }

  [XmlType("template")]
  public class Template
  {
    [XmlElement("objecttype")]
    public string ObjectType { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlArray("placeholders")]
    public List<Placeholder> Placeholders { get; set; }
  }

  [XmlType("placeholder")]
  public class Placeholder
  {
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlText]
    public string Value { get; set; }
  }

  [XmlType("map")]
  public class Mapping
  {
    [XmlAttribute("columnname")]
    public string ColumnName { get; set; }

    [XmlAttribute("destination")]
    public string Destination { get; set; }

    [XmlArray("rules")]
    public List<Rule> Rules { get; set; }
  }

  [XmlType("rule")]
  public class Rule
  {
    [XmlAttribute("order")]
    public string Order { get; set; }

    [XmlText]
    public string Value { get; set; }
  }
}
