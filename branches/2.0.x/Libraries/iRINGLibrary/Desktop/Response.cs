// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Linq;
using System;

namespace org.iringtools.library
{
  [XmlRoot]
  [DataContract(Namespace = "http://www.iringtools.org/common/response", Name = "response")]
  public class Response 
  {
    [XmlElement]
    [DataMember(Name = "level", Order = 0)]
    public StatusLevel Level { get; set; }

    [XmlElement]
    [DataMember(Name = "dateTimeStamp", Order = 1, EmitDefaultValue = false)]
    public DateTime DateTimeStamp { get; set; }

    [XmlElement]
    [DataMember(Name = "statusList", Order = 2)]
    public List<Status> StatusList { get; set; }

    public Response()
    {
      this.StatusList = new List<Status>();      
    }

    public void Append(Response response)
    {
      foreach (Status status in response.StatusList)
      {
        Append(status);
      }
    }

    public void Append(Status status)
    {
      Status foundStatus = null;
      bool wasFound = false;
      foreach (Status candidateStatus in StatusList)
      {
        if (status.Identifier == candidateStatus.Identifier)
        {
          foundStatus = candidateStatus;
          wasFound = true;
        }
      }

      if (!wasFound)
      {
        StatusList.Add(status);
      }
      else
      {
        if (foundStatus.Level < status.Level)
          foundStatus.Level = status.Level;

        foreach (string message in status.Messages)
        {
          foundStatus.Messages.Add(message);
        }
      }

      if (Level < status.Level)
        Level = status.Level;
    }

    public override string ToString()
    {
      string messages = String.Empty;

      foreach (Status status in StatusList)
      {
        foreach (string message in status.Messages)
        {
          messages += String.Format("{0} : {1}\\r\\n", status.Identifier, message);
        }
      }

      return messages;
    }
  }

  [XmlRoot]
  [DataContract(Namespace = "http://www.iringtools.org/common/response", Name = "status")]
  public class Status //: IXmlSerializable
  {
    [XmlElement]
    [DataMember(Name = "level", Order = 0)]
    public StatusLevel Level { get; set; }

    [XmlElement]
    [DataMember(Name = "identifier", Order = 1, EmitDefaultValue = false)]
    public string Identifier { get; set; }

    [XmlElement]
    [DataMember(Name = "results", Order = 2, EmitDefaultValue = false)]
    public Results Results { get; set; }

    [XmlElement]
    [DataMember(Name = "messages", Order = 3, EmitDefaultValue = false)]
    public Messages Messages { get; set; }

    public Status()
    {
      this.Messages = new Messages();
      this.Results = new Results();
    }
  }

  [XmlRoot]
  [CollectionDataContract(Namespace = "http://www.iringtools.org/common/response", Name = "results", ItemName="result", KeyName = "key", ValueName="value")]
  public class Results : Dictionary<string, string> {}
   
  [XmlRoot]
  [CollectionDataContract(Namespace = "http://www.iringtools.org/common/response", Name = "messages", ItemName = "message")]
  public class Messages : List<string> {}

  [XmlRoot]
  [DataContract(Namespace = "http://www.iringtools.org/common/response", Name = "level")]
  public enum StatusLevel
  {
    [XmlEnum]
    [EnumMember]
    Success,
    [XmlEnum]
    [EnumMember]
    Warning,
    [XmlEnum]
    [EnumMember]
    Error
  }
}
