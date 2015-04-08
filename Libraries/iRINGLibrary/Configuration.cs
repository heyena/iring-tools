using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    [XmlRoot(ElementName = "configuration")]
    [DataContract(Name = "configuration", Namespace = "http://www.iringtools.org/library")]
    public class Configuration
    {
        [XmlElement(ElementName = "appSettings")]
        [DataMember(Name = "appSettings", Order = 0, EmitDefaultValue = false)]
        public AppSettings AppSettings { get; set; }
    }

    [DataContract(Name = "appSettings", Namespace = "http://www.iringtools.org/library")]
    public class AppSettings
    {
        [XmlElement(ElementName = "add")]
        [DataMember(Name = "add", Order = 0, EmitDefaultValue = false)]
        public List<Setting> Settings { get; set; }
    }

    [DataContract(Name = "setting", Namespace = "http://www.iringtools.org/library")]
    public class Setting
    {
        [XmlAttribute(AttributeName = "key")]
        [DataMember(Name = "key", Order = 0, EmitDefaultValue = false)]
        public String Key { get; set; }

        [XmlAttribute(AttributeName = "value")]
        [DataMember(Name = "value", Order = 1, EmitDefaultValue = false)]
        public String Value { get; set; }
    }
}
