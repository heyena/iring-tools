using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    [Serializable]
    [DataContract(Name = "Locator", Namespace = "http://www.iringtools.org/library")]
    public class Locator
    {
        public Locator()
        {
            instances = new List<Instance>();
        }

        [DataMember(Order = 0)]
        public string version { get; set; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Order = 2)]
        public string name { get; set; }

        [DataMember(Order = 3)]
        public List<Instance> instances { get; set; }

        [DataMember(Order = 4)]
        public Array tags { get; set; }

        [DataMember(Order = 5)]
        public bool Private { get; set; }

        [DataMember(Order = 6)]
        public bool resource { get; set; }

        [DataMember(Order = 7)]
        public DateTime updated { get; set; }

        [DataMember(Order = 8)]
        public DateTime created { get; set; }

        [DataMember(Order = 9)]
        public string description { get; set; }

        [DataMember(Order = 10)]
        public string[] authors { get; set; }
    }


    [Serializable]
    [DataContract(Name = "Instance", Namespace = "http://www.iringtools.org/library")]
    public class Instance
    {
        public Instance()
        {
            endpoints = new List<Endpoint>();
            path = new path();
        }

        [DataMember(Order = 0, EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Order = 1)]
        public bool released { get; set; }

        [DataMember(Order = 2)]
        public string server { get; set; }

        [DataMember(Order = 3)]
        public string shortName { get; set; }

        [DataMember(Order = 4)]
        public List<Endpoint> endpoints { get; set; }

        [DataMember(Order = 5)]
        public path path { get; set; }

        [DataMember(Order = 6)]
        public string platform { get; set; }

        [DataMember(Order = 7)]
        public bool Private { get; set; }

        [DataMember(Order = 8)]
        public bool beta { get; set; }

        [DataMember(Order = 9)]
        public DateTime updated { get; set; }

        [DataMember(Order = 10)]
        public DateTime created { get; set; }

        [DataMember(Order = 11)]
        public Array certificates { get; set; }

        [DataMember(Order = 12)]
        public string version { get; set; }

        [DataMember(Order = 13)]
        public string type { get; set; }
    }


    [Serializable]
    [DataContract(Name = "Endpoint", Namespace = "http://www.iringtools.org/library")]
    public class Endpoint
    {
        public Endpoint()
        {
            operations = new List<Operation>();
        }

        [DataMember(Order = 0)]
        public string path { get; set; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Order = 2)]
        public List<Operation> operations { get; set; }

        [DataMember(Order = 3)]
        public bool Private { get; set; }

        [DataMember(Order = 4)]
        public string description { get; set; }
    }


    [Serializable]
    [DataContract(Name = "Operation", Namespace = "http://www.iringtools.org/library")]
    public class Operation
    {
        public Operation()
        {
            parameters = new List<Parameter>();
        }

        [DataMember(Order = 0)]
        public string httpMethod { get; set; }

        [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Order = 2)]
        public List<string> supports { get; set; }

        [DataMember(Order = 3)]
        public string responseClass { get; set; }

        [DataMember(Order = 4)]
        public DateTime updated { get; set; }

        [DataMember(Order = 5)]
        public DateTime created { get; set; }

        [DataMember(Order = 6)]
        public bool Private { get; set; }

        [DataMember(Order = 7)]
        public List<Parameter> parameters { get; set; }

        [DataMember(Order = 8)]
        public string summary { get; set; }

        [DataMember(Order = 9)]
        public string nickname { get; set; }
    }


    [Serializable]
    [DataContract(Name = "Parameter", Namespace = "http://www.iringtools.org/library")]
    public class Parameter
    {
        [DataMember(Order = 0)]
        public string name { get; set; }

        [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Order = 2)]
        public Array Enum  { get; set; }

        [DataMember(Order = 3)]
        public string dataType { get; set; }

        [DataMember(Order = 4)]
        public bool required { get; set; }

        [DataMember(Order = 5)]
        public bool Private { get; set; }

        [DataMember(Order = 6)]
        public string description { get; set; }

        [DataMember(Order = 7)]
        public string paramType { get; set; }
    }

    [Serializable]
    [DataContract(Name = "path", Namespace = "http://www.iringtools.org/library")]
    public class path
    {
        [DataMember(Order = 0)]
        public string Internal { get; set; }

        [DataMember(Order = 1)]
        public string external { get; set; }
    }
}
