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
    public List<Map> Mappings { get; set; }
  }

  [XmlType("template")]
  public class Template
  {
    [XmlElement("objecttype")]
    public int ObjectType { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlArray("placeholders")]
    public List<Placeholder> Placeholders { get; set; }
  }

  [XmlType("placeholder")]
  public class Placeholder
  {
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlText]
    public string Value { get; set; }
  }

  [XmlType("map")]
  public class Map
  {
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("type")]
    public PropertyType Type { get; set; }

    [XmlElement("rule")]
    public List<RuleRef> RuleRefs { get; set; }
  }

  [XmlType("rule")]
  public class RuleRef
  {
    [XmlAttribute("order")]
    public int Order { get; set; }

    [XmlText]
    public int Value { get; set; }
  }

  [XmlType("type")]
  public enum PropertyType
  {
    None = 0,
    Attribute = 1,
    Relationship = 2,
    Code = 3,
    Name = 4,
    Title = 5,
    Revision = 6
  };

  public class PlaceHolderComparer : IComparer<Placeholder>
  {
    public int Compare(Placeholder left, Placeholder right)
    {
      if (left == null)
      {
        if (right == null)
        {
          return 0;
        }
        else
        {
          return -1;
        }
      }
      else
      {
        if (right == null)
        {
          return 1;
        }
        else
        {
          if (left.Id > right.Id)
            return 1;

          if (left.Id < right.Id)
            return -1;

          return 0;
        }
      }
    }
  }

  public class RuleRefComparer : IComparer<RuleRef>
  {
    public int Compare(RuleRef left, RuleRef right)
    {
      if (left == null)
      {
        if (right == null)
        {
          return 0;
        }
        else
        {
          return -1;
        }
      }
      else
      {
        if (right == null)
        {
          return 1;
        }
        else
        {
          if (left.Order > right.Order)
            return 1;

          if (left.Order < right.Order)
            return -1;

          return 0;
        }
      }
    }
  }
}
