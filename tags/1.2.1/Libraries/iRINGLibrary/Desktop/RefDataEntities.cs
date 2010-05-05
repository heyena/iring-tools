using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract]
  public class RefDataEntities : SortedList<string, Entity>
  {
    public int total { get; set; }
  }
}
