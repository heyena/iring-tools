using System;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    [DataContract]
  public class ResponseViewModel 
  {
    [DataMember(Name = "success")]
    public bool success { get; set; }

    [DataMember(Name = "total")]
    public long total { get; set; }

    [DataMember(Name = "data")]
    public Array data { get; set; }

    [DataMember(Name = "message")]
    public string message { get; set; }
  }
}