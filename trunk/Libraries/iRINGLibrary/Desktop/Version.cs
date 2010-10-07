using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://iringtools.org/common", Name = "version")]
  public class Version
  {
    [DataMember(Name = "major", Order = 0)]
    public int Major { get; set; }

    [DataMember(Name = "minor", Order = 1)]
    public int Minor { get; set; }

    [DataMember(Name = "build", Order = 2, EmitDefaultValue = false)]
    public int Build { get; set; }

    [DataMember(Name = "revision", Order = 3, EmitDefaultValue = false)]
    public int Revision { get; set; }
  }
}
