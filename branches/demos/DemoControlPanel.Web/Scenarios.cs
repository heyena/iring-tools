using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace DemoControlPanel.Web
{

    [CollectionDataContract]
    public class Scenarios : Collection<Scenario>
    {
    }

    [DataContract]
    public class Scenario
    {
        [DataMember]
        public string scenarioName { get; set; }

        [DataMember]
        public string receiverAdapterServiceId { get; set; }

        [DataMember]
        public string senderAdapterServiceId { get; set; }

        [DataMember]
        public string interfaceServiceId { get; set; }

        [DataMember]
        public string sender { get; set; }

        [DataMember]
        public string receiver { get; set; }

        [DataMember]
        public string senderProjectName { get; set; }

        [DataMember]
        public string senderApplicationName { get; set; }

        [DataMember]
        public string senderGraphName { get; set; }

        [DataMember]
        public string receiverProjectName { get; set; }

        [DataMember]
        public string receiverApplicationName { get; set; }

        [DataMember]
        public string receiverGraphName { get; set; }

        [DataMember]
        public bool importEnabled { get; set; }

        [DataMember]
        public bool exportEnabled { get; set; }

    }

}

