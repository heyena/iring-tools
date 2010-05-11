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
    public string tableName { get; set; }

    [DataMember(IsRequired = false)]
    public string objectNamespace { get; set; }

    [DataMember]
    public string objectName { get; set; }

    [DataMember]
    public KeyProperties keyProperties { get; set; }

    [DataMember]
    public List<DataProperty> dataProperties { get; set; }

    [DataMember]
    public List<DataRelationship> dataRelationships { get; set; }

    public bool isKeyProperty(string propertyName)
    {
      foreach (KeyProperty keyProperty in keyProperties)
      {
        if (keyProperty.propertyName.ToLower() == propertyName.ToLower())
          return true;
      }

      return false;
    }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class DataProperty
  {
    [DataMember]
    public string columnName { get; set; }

    [DataMember]
    public string propertyName { get; set; }

    [DataMember]
    public DataType dataType { get; set; }

    [DataMember]
    public int dataLength { get; set; }

    [DataMember]
    public bool isNullable { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class KeyProperty : DataProperty
  {
    [DataMember(IsRequired = true)]
    public KeyType keyType { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class KeyProperties : List<KeyProperty>
  {
    [DataMember(IsRequired = true)]
    public string keyDelimeter { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  [KnownType(typeof(OneToOneRelationship))]
  [KnownType(typeof(OneToManyRelationship))]
  [KnownType(typeof(ManyToOneRelationship))]
  public abstract class DataRelationship
  {
    [DataMember(IsRequired = true)]
    public string relatedTableName { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class OneToOneRelationship : DataRelationship
  {
    [DataMember(IsRequired = true)]
    public bool isKeyConstrained { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class OneToManyRelationship : DataRelationship
  {
    [DataMember(IsRequired = true)]
    public string relatedColumnName { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class ManyToOneRelationship : DataRelationship
  {
    [DataMember(IsRequired = true)]
    public string columnName { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public enum KeyType
  {
    [EnumMember]
    assigned,
    [EnumMember]
    foreign,
    [EnumMember]
    identity,
    [EnumMember]
    sequence
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public enum DataType
  {
    [EnumMember]
    @Boolean,
    [EnumMember]
    @Byte,
    [EnumMember]
    @Char,
    [EnumMember]
    @DateTime,
    [EnumMember]
    @Decimal,
    [EnumMember]
    @Double,
    [EnumMember]
    @Int16,
    [EnumMember]
    @Int32,
    [EnumMember]
    @Int64,
    [EnumMember]
    @Single,
    [EnumMember]
    @String,
    [EnumMember]
    @TimeSpan,
  }
}
