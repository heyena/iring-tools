
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace org.iringtools.refdata.response
{
    [DataContract(Name = "response", Namespace = "http://www.iringtools.org/refdata/response")]
    public class Response 
    {
        [DataMember(Name = "entities", Order = 0)]
        public Entities Entities { get; set; }
        [DataMember(Name = "total", Order = 1)]
        public int Total { get; set; }
    }

    [DataContract(Name = "entity", Namespace = "http://www.iringtools.org/refdata/response")]
    public class Entity
    {
        [DataMember(Name = "uri", Order = 0)]
        public string Uri { get; set; }

        [DataMember(Name = "rdsuri", Order = 1)]
        public string RdsUri { get; set; }

        [DataMember(Name = "label", Order = 2)]
        public string Label { get; set; }

        [DataMember(Name = "lang", Order = 3)]
        public string Lang { get; set; }

       [DataMember(Name = "repository", Order = 4)]
        public string Repository { get; set; }
    }

    [CollectionDataContract(Name = "entities", Namespace = "http://www.iringtools.org/refdata/response")]
    public  class Entities : List<Entity>
    {

    }
}