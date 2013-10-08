using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace org.iringtools.library
{
  [XmlRoot(ElementName = "configuration")]
  public class Configuration
  {
    [XmlElement(ElementName = "appSettings")]
    public AppSettings AppSettings { get; set; }
  }

  public class AppSettings
  {
    [XmlElement(ElementName = "add")]
    public List<Setting> Settings { get; set; }
  }

  public class Setting
  {
    [XmlAttribute(AttributeName = "key")]
    public String Key { get; set; }

    [XmlAttribute(AttributeName = "value")]
    public String Value { get; set; }
  }
}
