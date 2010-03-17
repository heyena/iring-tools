using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace XAppReportService
{
  [XmlRoot]
  public class XAppReport
  {
    [XmlArray]   
    public List<Report> reports { get; set; }
  }

  [XmlRoot]
  public class Report
  {
    [XmlAttribute]
    public string name { get; set; }

    [XmlArray]
    public List<Company> companies { get; set; }
  }

  [XmlRoot]
  public class Company
  {
    [XmlElement]
    public string name { get; set; }

    [XmlElement]
    public string dtoUrl { get; set; }

    [XmlArray]
    public List<DTOProperty> dtoProperties { get; set; }
  }

  [XmlRoot]
  public class DTOProperty
  {
    [XmlAttribute]
    public string label { get; set; }

    [XmlAttribute(AttributeName="xpath")]
    public string xPath { get; set; }

    [XmlArray]
    public List<string> values { get; set; }
  }
}