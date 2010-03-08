using System.Runtime.Serialization;

namespace DemoControlPanel.Web
{
    [DataContract]
    public class ConfiguredEndpoints
    {
        [DataMember]
        public Scenarios scenarios { get; set; }

        [DataMember]
        public iRINGEndpoints interfaceEndpoints { get; set; }

        [DataMember]
        public iRINGEndpoints adapterEndpoints { get; set; }
    }
}
