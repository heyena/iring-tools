using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using org.iringtools.utility;

namespace DemoControlPanel.Web
{
    [CollectionDataContract]
    public class iRINGEndpoints : Collection<iRINGEndpoint>
    {
    }

    [DataContract]
    public class iRINGEndpoint
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string serviceUri { get; set; }
                
        [DataMember]
        public WebCredentials credentials { get; set; }

    }
}

