using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "exchangeConfig")]
    public class ExchangeConfig : List<Sequence> {}

    [DataContract(Namespace = "http://www.iringtools.org/library", Name = "sequence")]
    public class Sequence
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "exchanges", Order = 1)]
        public List<Exchange> Exchanges { get; set; }
    }

    [DataContract(Namespace = "http://www.iringtools.org/library", Name = "exchange")]
    public class Exchange
    {
        [DataMember(Name = "baseURL", Order = 0)]
        public string BaseURL { get; set; }

        [DataMember(Name = "scope", Order = 1)]
        public string Scope { get; set; }

        [DataMember(Name = "exchangeId", Order = 2)]
        public string ExchangeId { get; set; }
    }
}
