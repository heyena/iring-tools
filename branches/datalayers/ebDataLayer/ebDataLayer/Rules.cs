﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace org.iringtools.adaper.datalayer.eb
{
  [XmlType("rules")]
  public class Rules : List<Rule> {}

  [XmlType("rule")]
  public class Rule
  {
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("relationshiptemplate")]
    public string RelationshipTemplate { get; set; }

    [XmlElement("relatedobjecttype")]
    public int RelatedObjectType { get; set; }

    [XmlElement("eql")]
    public string Eql { get; set; }

    [XmlArray("parameters")]
    public List<Parameter> Parameters { get; set; }

    [XmlArray("selfchecks", IsNullable = true)]
    public List<SelfCheck> SelfChecks { get; set; }

    [XmlElement("create")]
    public bool Create { get; set; }

    [XmlElement("createtemplate", IsNullable = true)]
    public string CreateTemplate { get; set; }
  }

  [XmlType("parameter")]
  public class Parameter
  {
    [XmlAttribute(AttributeName = "position")]
    public int Position { get; set; }

    [XmlAttribute(AttributeName = "placeholder")]
    public string Placeholder { get; set; }

    [XmlAttribute(AttributeName = "seperator")]
    public char Seperator { get; set; }

    [XmlText]
    public string Value { get; set; }
  }

  [XmlType("check")]
  public class SelfCheck
  {
    [XmlAttribute(AttributeName = "column")]
    public string Column { get; set; }

    [XmlAttribute(AttributeName = "value")]
    public string Value { get; set; }

    [XmlAttribute(AttributeName = "operator")]
    public string Operator { get; set; }
  }
}
