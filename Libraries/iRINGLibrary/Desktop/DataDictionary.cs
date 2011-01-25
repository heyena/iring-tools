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
using System.Linq;

namespace org.iringtools.library
{
  [DataContract(Name = "dataDictionary", Namespace = "http://www.iringtools.org/library")]
  public class DataDictionary
  {
    public DataDictionary()
    {
      DataObjects = new List<DataObject>();
    }

    [DataMember(Name = "dataObjects")]
    public List<DataObject> DataObjects { get; set; }
  }

  [DataContract(Name = "dataObject", Namespace = "http://www.iringtools.org/library")]
  public class DataObject
  {
    public DataObject()
    {
      KeyProperties = new List<KeyProperty>();
      DataProperties = new List<DataProperty>();
      DataRelationships = new List<DataRelationship>();
    }

    [DataMember(Name = "tableName", IsRequired = true, Order = 0)]
    public string TableName { get; set; }

    [DataMember(Name = "objectNamespace", IsRequired = false, Order = 1, EmitDefaultValue = false)]
    public string ObjectNamespace { get; set; }

    [DataMember(Name = "objectName", IsRequired = true, Order = 2)]
    public string ObjectName { get; set; }

    [DataMember(Name = "keyDelimeter", IsRequired = false, Order = 3, EmitDefaultValue = false)]
    public string KeyDelimeter { get; set; }

    [DataMember(Name = "keyProperties", IsRequired = true, Order = 4)]
    public List<KeyProperty> KeyProperties { get; set; }

    [DataMember(Name = "dataProperties", IsRequired = true, Order = 5)]
    public List<DataProperty> DataProperties { get; set; }

    [DataMember(Name = "dataRelationships", IsRequired = false, Order = 6, EmitDefaultValue = false)]
    public List<DataRelationship> DataRelationships { get; set; }

    public bool IsKeyProperty(string propertyName)
    {
      foreach (KeyProperty keyProperty in KeyProperties)
      {
        if (keyProperty.KeyPropertyName.ToLower() == propertyName.ToLower())
          return true;
      }

      return false;
    }

    public DataProperty GetKeyProperty(string keyPropertyName)
    {
      return DataProperties.FirstOrDefault(c => c.PropertyName == keyPropertyName);
    }

    public bool DeleteProperty(DataProperty dataProperty)
    {
      foreach (DataProperty property in DataProperties)
      {
        if (dataProperty == property)
        {
          DataProperties.Remove(dataProperty);
          break;
        }
      }
      foreach (KeyProperty keyProperty in KeyProperties)
      {
        if (keyProperty.KeyPropertyName.ToLower() == dataProperty.PropertyName.ToLower())
        {
          KeyProperties.Remove(keyProperty);
          break;
        }
      }
      return true;
    }

    public bool AddKeyProperty(DataProperty keyProperty)
    {
      this.KeyProperties.Add(new KeyProperty { KeyPropertyName = keyProperty.PropertyName });
      this.DataProperties.Add(keyProperty);
      return true;
    }

  }

  [DataContract(Name = "dataProperty", Namespace = "http://www.iringtools.org/library")]
  public class DataProperty
  {
    [DataMember(Name = "columnName", IsRequired = true, Order = 0)]
    public string ColumnName { get; set; }

    [DataMember(Name = "propertyName", IsRequired = true, Order = 1)]
    public string PropertyName { get; set; }

    [DataMember(Name = "dataType", IsRequired = true, Order = 2)]
    public DataType DataType { get; set; }

    [DataMember(Name = "dataLength", IsRequired = true, Order = 3)]
    public int DataLength { get; set; }

    [DataMember(Name = "isNullable", IsRequired = true, Order = 4)]
    public bool IsNullable { get; set; }

    [DataMember(Name = "keyType", IsRequired = true, Order = 5)]
    public KeyType KeyType { get; set; }

    [DataMember(Name = "showOnIndex", EmitDefaultValue = false, Order = 6)]
    public bool ShowOnIndex { get; set; }
  }

  [DataContract(Name = "keyProperty", Namespace = "http://www.iringtools.org/library")]
  public class KeyProperty
  {
    [DataMember(Name = "keyPropertyName", IsRequired = true)]
    public string KeyPropertyName { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library")]
  public enum RelationshipType
  {
    [EnumMember]
    OneToOne,
    [EnumMember]
    OneToMany,
    //[EnumMember]
    //ManyToOne,
    //[EnumMember]
    //ManyToMany
  }

  [DataContract(Name = "propertyMap", Namespace = "http://www.iringtools.org/library")]
  public class PropertyMap
  {
    [DataMember(Name = "dataPropertyName", IsRequired = true)]
    public string DataPropertyName { get; set; }

    [DataMember(Name = "relatedPropertyName", IsRequired = true)]
    public string RelatedPropertyName { get; set; }
  }

  [DataContract(Name = "dataRelationship", Namespace = "http://www.iringtools.org/library")]
  public class DataRelationship
  {
    public DataRelationship()
    {
      this.PropertyMaps = new List<PropertyMap>();
    }

    [DataMember(Name = "relationshipName", IsRequired = true)]
    public string RelationshipName { get; set; }

    [DataMember(Name = "relationshipType", IsRequired = true)]
    public RelationshipType RelationshipType { get; set; }

    [DataMember(Name = "relatedObjectName", IsRequired = true)]
    public string RelatedObjectName { get; set; }

    [DataMember(Name = "propertyMaps", IsRequired = true)]
    public List<PropertyMap> PropertyMaps { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library")]
  public enum KeyType
  {
    [EnumMember]
    unassigned,
    [EnumMember]
    assigned,
    [EnumMember]
    foreign,
    [EnumMember]
    identity,
    [EnumMember]
    sequence
  }

  [DataContract(Namespace = "http://www.iringtools.org/library")]
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
