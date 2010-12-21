using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/refdata/response")]
  public class RefDataEntities
  {
    public RefDataEntities()
    {
      Entities = new SortedList<string, Entity>();
    }

    [DataMember]
    public SortedList<string, Entity> Entities { get; set; }

    [DataMember]
    public int Total { get; set; }
  }


}
