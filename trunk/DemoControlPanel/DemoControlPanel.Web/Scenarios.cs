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
    }

}

