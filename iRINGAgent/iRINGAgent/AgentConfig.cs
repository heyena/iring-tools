using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.utility;
using org.iringtools.library;
using System.Collections.ObjectModel;


namespace org.iringtools.agent
{
    [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "agentconfig")]
    public class AgentConfig : List<TaskSequence> { }

    [Serializable]
    [DataContract(Namespace = "http://www.iringtools.org/library", Name = "tasksequence")]
    public class TaskSequence
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

        [DataMember(Name = "assembly", Order = 1)]
        public string Assembly { get; set; }

        [DataMember(Name = "scope", Order = 2)]
        public string Scope { get; set; }

        [DataMember(Name = "app", Order = 3)]
        public string App { get; set; }

        [DataMember(Name = "params", Order = 4)]
        public Dictionary<string, string> taskParams { get; set; }
    }
}
