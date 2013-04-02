using System;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    [DataContract]
  public class ResponseViewModel 
  {
    [DataMember(Name = "success")]
    public bool Success { get; set; }

    [DataMember(Name = "total")]
    public long Total { get; set; }

    [DataMember(Name = "data")]
    public Array Data { get; set; }

    [DataMember(Name = "message")]
    public string Message { get; set; }
  }
}