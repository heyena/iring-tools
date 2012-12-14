using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "requestStatus")]
  public class RequestStatus
  {
    [DataMember(Name = "state")]
    public State State { get; set; }

    [DataMember(Name = "percentComplete", EmitDefaultValue = false)]
    public int PercentComplete { get; set; }

    [DataMember(Name = "message", EmitDefaultValue = false)]
    public string Message { get; set; }

    [DataMember(Name = "responseText", EmitDefaultValue = false)]
    public string ResponseText { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "state")]
  public enum State
  {
    [EnumMember]
    InProgress,

    [EnumMember]
    Completed,

    [EnumMember]
    Error,

    [EnumMember]
    NotFound
  }
}
