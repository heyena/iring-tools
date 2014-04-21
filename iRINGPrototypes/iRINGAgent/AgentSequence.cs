using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    [Serializable]
    [DataContract(Namespace = "http://www.iringtools.org/library", Name = "sequence")]
    public class AgentSequence
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "tasks", Order = 1)]
        public List<Task> Tasks { get; set; }
    }

    [Serializable]
    [DataContract(Namespace = "http://www.iringtools.org/library", Name = "task")]
    public class Task
    {
        [DataMember(Name = "baseURL", Order = 0)]
        public string BaseURL { get; set; }

        [DataMember(Name = "scope", Order = 1)]
        public string Scope { get; set; }

        [DataMember(Name = "exchangeId", Order = 2)]
        public string ExchangeId { get; set; }

        [DataMember(Name = "params", Order = 3)]
        public Dictionary<string, string> taskParams { get; set; }
    }
}
