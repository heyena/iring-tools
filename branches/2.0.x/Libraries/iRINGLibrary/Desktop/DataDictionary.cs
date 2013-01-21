﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
    [DataContract(Namespace = "http://ns.iringtools.org/library")]
    public class DataDictionary
    {
        public DataDictionary()
        {
            dataObjects = new List<DataObject>();
        }

        [DataMember]
        public List<DataObject> dataObjects { get; set; }
    }

    [DataContract(Namespace = "http://ns.iringtools.org/library")]
    public class DataObject
    {
        public DataObject()
        {
            keyProperties = new List<KeyProperty>();
            dataProperties = new List<DataProperty>();
            dataRelationships = new List<DataRelationship>();
        }

        [DataMember(IsRequired = true, Order = 0)]
        public string tableName { get; set; }

        [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
        public string objectNamespace { get; set; }

        [DataMember(IsRequired = true, Order = 2)]
        public string objectName { get; set; }

        [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
        public string keyDelimeter { get; set; }

        [DataMember(IsRequired = true, Order = 4)]
        public List<KeyProperty> keyProperties { get; set; }

        [DataMember(IsRequired = true, Order = 5)]
        public List<DataProperty> dataProperties { get; set; }

        [DataMember(IsRequired = false, Order = 6, EmitDefaultValue = false)]
        public List<DataRelationship> dataRelationships { get; set; }

        public bool isKeyProperty(string propertyName)
        {
            foreach (KeyProperty keyProperty in keyProperties)
            {
                if (keyProperty.keyPropertyName.ToLower() == propertyName.ToLower())
                    return true;
            }

            return false;
        }

        public DataProperty getKeyProperty(string keyPropertyName) 
        {
          return dataProperties.FirstOrDefault(c => c.propertyName == keyPropertyName);
        }

        public bool deleteProperty(DataProperty dataProperty)
        {
          foreach (DataProperty property in dataProperties)
          {
            if (dataProperty == property)
            {
              dataProperties.Remove(dataProperty);
              break;
            }
          }
          foreach (KeyProperty keyProperty in keyProperties)
          {
            if (keyProperty.keyPropertyName.ToLower() == dataProperty.propertyName.ToLower())
            {
              keyProperties.Remove(keyProperty);
              break;
            }
          }
            return true;
        }

        public bool addKeyProperty(DataProperty keyProperty)
        {
          this.keyProperties.Add(new KeyProperty { keyPropertyName = keyProperty.propertyName });
          this.dataProperties.Add(keyProperty);
          return true;
        }

    }

    [DataContract(Namespace = "http://ns.iringtools.org/library")]
    public class DataProperty
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string columnName { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public string propertyName { get; set; }

        [DataMember(IsRequired = true, Order = 2)]
        public DataType dataType { get; set; }

        [DataMember(IsRequired = true, Order = 3)]
        public int dataLength { get; set; }

        [DataMember(IsRequired = true, Order = 4)]
        public bool isNullable { get; set; }

        [DataMember(IsRequired = true, Order = 5)]
        public KeyType keyType { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public bool showOnIndex { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public int numberOfDecimals { get; set; }
    }

    [DataContract(Namespace = "http://ns.iringtools.org/library")]
    public class KeyProperty
    {
        [DataMember(IsRequired = true)]
        public string keyPropertyName { get; set; }
    }

    [DataContract(Namespace = "http://ns.iringtools.org/library")]
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

    [DataContract(Namespace = "http://ns.iringtools.org/library")]
    public class PropertyMap
    {
        [DataMember(IsRequired = true)]
        public string dataPropertyName { get; set; }

        [DataMember(IsRequired = true)]
        public string relatedPropertyName { get; set; }
    }

    [DataContract(Namespace = "http://ns.iringtools.org/library")]
    public class DataRelationship
    {
        public DataRelationship()
        {
          this.propertyMaps = new List<PropertyMap>();
        }

        [DataMember(IsRequired = true)]
        public string relationshipName { get; set; }

        [DataMember(IsRequired = true)]
        public RelationshipType relationshipType { get; set; }

        [DataMember(IsRequired = true)]
        public string relatedObjectName { get; set; }

        [DataMember(IsRequired = true)]
        public List<PropertyMap> propertyMaps { get; set; }
    }

    [DataContract(Namespace = "http://ns.iringtools.org/library")]
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