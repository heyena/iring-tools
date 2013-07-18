using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "DataItems")]
  public class Picklists : DataItems
  {
    public Picklists() : base()
    {
        items = new List<DataItem>();
    }

    [DataMember(IsRequired = false, Order = 1)]
    public string title { get; set; }

    [DataMember(IsRequired = false, Order = 2)]
    public int valueColumnIndex { get; set; }
  }
}
