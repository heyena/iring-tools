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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class DatabaseDictionary
  {
    [DataMember(IsRequired = true)]
    public Provider provider { get; set; }

    [DataMember(IsRequired = true)]
    public string connectionString { get; set; }

    [DataMember]
    public List<Table> tables { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class Table
  {
    [DataMember(IsRequired = true)]
    public string tableName { get; set; }

    [DataMember]
    public string entityName { get; set; }

    [DataMember(IsRequired = true)]
    public List<Key> keys { get; set; }

    [DataMember]
    public List<Column> columns { get; set; }

    [DataMember]
    public List<Association> associations { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class Column
  {
    [DataMember(IsRequired = true)]
    public string columnName { get; set; }

    [DataMember]
    public string propertyName { get; set; }

    [DataMember(IsRequired = true)]
    public ColumnType columnType { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class Key : Column
  {
    [DataMember(IsRequired = true)]
    public KeyType keyType { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  [KnownType(typeof(OneToOneAssociation))]
  [KnownType(typeof(OneToManyAssociation))]
  [KnownType(typeof(ManyToOneAssociation))]
  public abstract class Association
  {
    [DataMember(IsRequired = true)]
    public string associatedTableName { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class OneToOneAssociation : Association
  {
    [DataMember(IsRequired = true)]
    // Key is from other table
    public bool constrained { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class OneToManyAssociation : Association
  {
    [DataMember]
    public string associatedColumnName { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public class ManyToOneAssociation : Association
  {
    [DataMember]
    public string columnName { get; set; }
  }

  [DataContract(Namespace = "http://ns.iringtools.org/library")]
  public enum Provider
  {
    [EnumMember]
    MsSql2000,
    [EnumMember]
    MsSql2005,
    [EnumMember]
    MsSql2008,
    [EnumMember]
    MySql3,
    [EnumMember]
    MySql4,
    [EnumMember]
    MySql5,
    [EnumMember]
    Oracle8i,
    [EnumMember]
    Oracle9i,
    [EnumMember]
    Oracle10g,
    [EnumMember]
    OracleLite,
    [EnumMember]
    PostgresSql81,
    [EnumMember]
    PostgresSql82,
    [EnumMember]
    SqLite
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
  public enum ColumnType
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
    @Double,
    [EnumMember]
    @Int16,
    [EnumMember]
    @Int32,
    [EnumMember]
    @Int64,
    [EnumMember]
    @Object,
    [EnumMember]
    @String,
    [EnumMember]
    @TimeSpan
  }
}
