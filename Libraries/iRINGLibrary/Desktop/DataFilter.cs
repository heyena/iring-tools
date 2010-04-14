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

using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class DataFilter
  {
    [DataMember]
    public List<Expression> Expressions { get; set; }
  }

  [CollectionDataContract(Namespace = "http://ns.iringtools.org/library")]
  public class Values : List<string> { }


  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class Expression
  {
    [DataMember(Order = 0, EmitDefaultValue = false)]
    public int OpenGroupCount { get; set; }

    [DataMember(Order = 1)]
    public string PropertyName { get; set; }

    [DataMember(Order = 2)]
    public RelationalOperator RelationalOperator { get; set; }

    [DataMember(Order = 3)]
    public Values Values { get; set; }

    [DataMember(Order = 4, EmitDefaultValue = false)]
    public LogicalOperator LogicalOperator { get; set; }

    [DataMember(Order = 5, EmitDefaultValue = false)]
    public int CloseGroupCount { get; set; }
    
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public enum LogicalOperator
  {
    [EnumMember]
    And,
    [EnumMember]
    Or,
    [EnumMember]
    Not,
    [EnumMember]
    AndNot,
    [EnumMember]
    OrNot,
  };

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public enum RelationalOperator
  {
    [EnumMember]
    EqualTo,
    [EnumMember]
    NotEqualTo,
    [EnumMember]
    BeginsWith,
    [EnumMember]
    EndsWith,
    [EnumMember]
    Contains,
    [EnumMember]
    In,
    [EnumMember]
    GreaterThan,
    [EnumMember]
    GreaterThanOrEqual,
    [EnumMember]
    LesserThan,
    [EnumMember]
    LesserThanOrEqual,
  };
}
