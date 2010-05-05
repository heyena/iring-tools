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
  public class DataDictionary
  {
    [DataMember]
    public List<DataObject> dataObjects { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class DataObject
  {
    [DataMember]
    public List<DataProperty> dataProperties { get; set; }

    [DataMember]
    public List<DataRelationship> dataRelationships { get; set; }

    [DataMember]
    public string objectName { get; set; }

    [DataMember]
    public string objectNamespace { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class DataProperty
  {
    [DataMember]
    public string propertyName { get; set; }

    [DataMember]
    public bool isPropertyKey { get; set; }

    [DataMember]
    public bool isRequired { get; set; }

    [DataMember]
    public string dataType { get; set; }

    [DataMember]
    public string dataLength { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class DataRelationship
  {
    [DataMember]
    public string relatedObject { get; set; }

    [DataMember]
    public string graphProperty { get; set; }

    [DataMember]
    public Cardinality cardinality { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public enum Cardinality
  {
    [EnumMember]
    OneToMany,
    [EnumMember]
    ManyToOne,
    [EnumMember]
    OneToOne,
    [EnumMember]
    ManyToMany
  }
}
