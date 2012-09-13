using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "Picklists")]
  public class Picklists : DataItems
  {
    public Picklists() : base()
    {}

    [DataMember(IsRequired = false, Order = 1)]
    public string title { get; set; }

    [DataMember(IsRequired = false, Order = 2)]
    public int valueColumnIndex { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "PicklistItem")]
  public class PicklistItem : DataItem
  {}

  //public class PicklistColumn
  //{
  //  [DataMember(IsRequired = true, Order = 0)]
  //  public string columnName { get; set; }

  //  [DataMember(IsRequired = true, Order = 1)]
  //  public string columnValue { get; set; }
  //}
}
