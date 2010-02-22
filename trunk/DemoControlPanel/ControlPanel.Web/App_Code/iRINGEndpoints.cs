using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using org.ids_adi.iring;
using System.Runtime.Serialization;
using org.ids_adi.camelot.utility;

namespace org.ids_adi.camelot.demo
{
  [CollectionDataContract]
  public class iRINGEndpoints : Collection<iRINGEndpoint>
  {}

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
