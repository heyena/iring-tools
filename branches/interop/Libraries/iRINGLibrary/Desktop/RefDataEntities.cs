using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "hhtp://www.iringtools.org/library", Name = "refDataEntities")]
  public class RefDataEntities
  {
    public RefDataEntities()
    {
      Entities = new SortedList<string, Entity>();
    }

    [DataMember(Name="entities")]
    public SortedList<string, Entity> Entities { get; set; }

    [DataMember(Name = "total")]
    public int Total { get; set; }
  }
}
