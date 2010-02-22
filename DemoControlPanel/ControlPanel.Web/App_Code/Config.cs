using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace org.ids_adi.camelot.demo
{
    [DataContract]
    public class Config
    {
       [DataMember]
        public Scenarios scenarios { get; set; }

       [DataMember]
       public iRINGEndpoints interfaceEndpoints { get; set; }

       [DataMember]
       public iRINGEndpoints adapterEndpoints { get; set; }
    }
}
