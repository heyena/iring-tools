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
using System;

namespace org.iringtools.library
{
    [Serializable]
    [DataContract(Name = "dataDictionary", Namespace = "http://www.iringtools.org/library")]
    public class DataDictionary
    {
        public DataDictionary()
        {
            dataObjects = new List<DataObject>();
            picklists = new List<PicklistObject>();
            unionObjects = new List<UnionObject>();
        }

        [DataMember(Order = 0)]
        public List<DataObject> dataObjects { get; set; }

        [DataMember(Order = 1)]
        public List<PicklistObject> picklists { get; set; }

        [DataMember(Order = 2)]
        public bool enableSearch { get; set; }

        [DataMember(Order = 3)]
        public bool enableSummary { get; set; }

        [DataMember(Order = 4)]
        public string dataVersion { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string description { get; set; }

        [DataMember(IsRequired = false, Order = 6)]
        public List<UnionObject> unionObjects { get; set; }

        //[[New fields
        [DataMember(Order = 7)]
        public Guid dictionaryId { get; set; }

        [DataMember(Order = 8)]
        public Guid applicationId { get; set; }

        [DataMember(Order = 9)]
        public bool isDBDictionary { get; set; }
        //]]

        public static bool IsNumeric(DataType dataType)
        {
            bool isNumeric = false;

            var numericTypes = new DataType[] {
          DataType.Byte,
          DataType.Decimal,
          DataType.Double,
          DataType.Int16,
          DataType.Int32, 
          DataType.Int64,
          DataType.Single,
      };
            if (numericTypes.Contains(dataType))
            {
                isNumeric = true;
            }

            return isNumeric;
        }

        public DataObject GetDataObject(string name)
        {
            DataObject dataObject = null;
            dataObject = this.dataObjects.FirstOrDefault<DataObject>(o => o.objectName.ToLower() == name.ToLower());
            return dataObject;
        }

        public DataObject GetTableObject(string name)
        {
            DataObject dataObject = null;
            dataObject = this.dataObjects.FirstOrDefault<DataObject>(o => o.tableName.ToLower() == name.ToLower());
            return dataObject;
        }
    }

    [Serializable]
    [DataContract(Name = "dataObject", Namespace = "http://www.iringtools.org/library")]
    public class DataObject
    {
        public DataObject()
        {
            keyProperties = new List<KeyProperty>();
            dataProperties = new List<DataProperty>();
            dataRelationships = new List<DataRelationship>();
            extensionProperties = new List<ExtensionProperty>();
            aliasDictionary = new Dictionary<string, string>();
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

        [DataMember(IsRequired = false, Order = 7, EmitDefaultValue = false)]
        public bool isReadOnly { get; set; }

        [DataMember(IsRequired = false, Order = 8, EmitDefaultValue = false)]
        public bool hasContent { get; set; }

        [DataMember(IsRequired = false, Order = 9, EmitDefaultValue = false)]
        public bool isListOnly { get; set; }

        [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
        public string defaultProjectionFormat { get; set; }

        [DataMember(IsRequired = false, Order = 11, EmitDefaultValue = false)]
        public string defaultListProjectionFormat { get; set; }

        [DataMember(IsRequired = false, Order = 12, EmitDefaultValue = false)]
        public string description { get; set; }

        [DataMember(IsRequired = false, Order = 13, EmitDefaultValue = false)]
        public bool isRelatedOnly { get; set; }

        [DataMember(IsRequired = false, Order = 14, EmitDefaultValue = false)]
        public string groupName { get; set; }

        [DataMember(IsRequired = false, Order = 15, EmitDefaultValue = false)]
        public Dictionary<string, string> aliasDictionary { get; set; }

        [DataMember(IsRequired = false, Order = 16, EmitDefaultValue = false)]
        public string version { get; set; }

        [DataMember(IsRequired = false, Order = 17, EmitDefaultValue = false)]
        public DataFilter dataFilter { get; set; }

        [DataMember(IsRequired = false, Order = 18, EmitDefaultValue = false)]
        public bool isHidden { get; set; }

        [DataMember(IsRequired = false, Order = 19)]
        public List<ExtensionProperty> extensionProperties { get; set; }

        //[[New Fields
        [DataMember(Order = 20)]
        public Guid dataObjectId { get; set; }

        [DataMember(Order = 21)]
        public Guid dictionaryId { get; set; }
        //]]

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

    [Serializable]
    [DataContract(Name = "dataProperty", Namespace = "http://www.iringtools.org/library")]
    public class DataProperty
    {
        public DataProperty()
        {
            aliasDictionary = new Dictionary<string, string>();
        }

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

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public bool isReadOnly { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 9)]
        public bool showOnSearch { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public bool isHidden { get; set; }

        [DataMember(IsRequired = false, Order = 11, EmitDefaultValue = false)]
        public string description { get; set; }

        [DataMember(IsRequired = false, Order = 12, EmitDefaultValue = false)]
        public Dictionary<string, string> aliasDictionary { get; set; }

        [DataMember(IsRequired = false, Order = 13, EmitDefaultValue = false)]
        public string referenceType { get; set; }

        [DataMember(IsRequired = false, Order = 14, EmitDefaultValue = false)]
        public bool isVirtual { get; set; }

        [DataMember(IsRequired = false, Order = 15)]
        public int precision { get; set; }

        [DataMember(IsRequired = false, Order = 16)]
        public int scale { get; set; }

        //[[New Fields
        [DataMember(Order = 17)]
        public Guid dataPropertyId { get; set; }

        [DataMember(Order = 18)]
        public Guid dataObjectId { get; set; }

        [DataMember(Order = 19)]
        public Guid pickListId { get; set; }
        //]]
    }

    [Serializable]
    [DataContract(Name = "extensionProperty", Namespace = "http://www.iringtools.org/library")]
    public class ExtensionProperty
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

        [DataMember(IsRequired = false, Order = 7)]
        public int precision { get; set; }

        [DataMember(IsRequired = false, Order = 8)]
        public int scale { get; set; }

        [DataMember(IsRequired = false, Order = 9)]
        public string definition { get; set; }

        //[[New Field 
        [DataMember(Order = 10)]
        public int numberOfDecimals { get; set; }

        [DataMember(Order = 11)]
        public Guid extensionPropertyId { get; set; }

        [DataMember(Order = 12)]
        public Guid dataObjectId { get; set; }
        //]]

        //[[By Deepak 12-Dec14 Statrs
        //[DataMember(IsRequired = false, Order = 10)]
        //public List<ExtensionParameter> parameters { get; set; }
        // Ends]]
    }

    [Serializable]
    [DataContract(Name = "aliasDictionary", Namespace = "http://www.iringtools.org/library")]
    public class aliasDictionary
    {
        [DataMember(IsRequired = false, Order = 0)]
        public string key { get; set; }

        [DataMember(IsRequired = false, Order = 1)]
        public string value { get; set; }
    }

    //[[By Deepak 12-Dec14 Statrs
    //[Serializable]
    //[DataContract(Name = "parameter", Namespace = "http://www.iringtools.org/library")]
    //public class ExtensionParameter
    //{
    //    [DataMember(IsRequired = false, Order = 0)]
    //    public string key { get; set; }

    //    [DataMember(IsRequired = false, Order = 1)]
    //    public string value { get; set; }
    //}
    // Ends]]

    [Serializable]
    [DataContract(Name = "keyProperty", Namespace = "http://www.iringtools.org/library")]
    public class KeyProperty
    {
        [DataMember(IsRequired = true,Order = 0)]
        public string keyPropertyName { get; set; }

        //[[New Field 
        [DataMember(Order = 1)]
        public Guid dataObjectId { get; set; }
        //]]
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

    [Serializable]
    [DataContract(Name = "propertyMap", Namespace = "http://www.iringtools.org/library")]
    public class PropertyMap
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string dataPropertyName { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public string relatedPropertyName { get; set; }

        //[[New Fields
        [DataMember(Order = 2)]
        public Guid relationshipId { get; set; }
        //]]
    }

    [Serializable]
    [DataContract(Name = "dataRelationship", Namespace = "http://www.iringtools.org/library")]
    public class DataRelationship
    {
        public DataRelationship()
        {
            this.propertyMaps = new List<PropertyMap>();
        }

        [DataMember(Order = 0, Name = "propertyMaps", IsRequired = true)]
        public List<PropertyMap> propertyMaps { get; set; }

        [DataMember(Order = 1, Name = "relatedObjectName", IsRequired = true)]
        public string relatedObjectName { get; set; }

        [DataMember(Order = 2, Name = "relationshipName", IsRequired = true)]
        public string relationshipName { get; set; }

        [DataMember(Order = 3, Name = "relationshipType", IsRequired = true)]
        public RelationshipType relationshipType { get; set; }

        //[[New Fields
        [DataMember(Order = 4, Name = "relationshipId")]
        public Guid relationshipId { get; set; }

        [DataMember(Order = 5, Name = "dataObjectId")]
        public Guid dataObjectId { get; set; }
        //]]
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
        @TimeStamp,
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
        @Reference,
        [EnumMember]
        @Date
    }

    [Serializable]
    [DataContract(Namespace = "http://www.iringtools.org/library")]
    public class PicklistObject
    {
        public PicklistObject()
        {
            pickListProperties = new List<DataProperty>();
        }

        [DataMember(IsRequired = true, Order = 0)]
        public string name { get; set; }

        [DataMember(IsRequired = false, Order = 1)]
        public string description { get; set; }

        [DataMember(IsRequired = false, Order = 2)]
        public int valuePropertyIndex { get; set; }

        [DataMember(IsRequired = false, Order = 3)]
        public string tableName { get; set; }

        [DataMember(IsRequired = false, Order = 4)]
        public List<DataProperty> pickListProperties { get; set; }

        //[[New Fields
        [DataMember(Order = 5)]
        public Guid dictionaryId { get; set; }

        [DataMember(Order = 6)]
        public Guid pickListId { get; set; }
        //]]
    }

    [Serializable]
    [DataContract(Name = "unionObject", Namespace = "http://www.iringtools.org/library")]
    public class UnionObject
    {
        public UnionObject()
        {
            unionTypes = new List<UnionType>();
        }

        [DataMember(IsRequired = false, Order = 0)]
        public string unionName { get; set; }

        [DataMember(IsRequired = false, Order = 1)]
        public List<UnionType> unionTypes { get; set; }
    }

    [Serializable]
    [DataContract(Name = "unionType", Namespace = "http://www.iringtools.org/library")]
    public class UnionType
    {
        [DataMember(IsRequired = false, Order = 0)]
        public string unionType { get; set; }
    }
}
