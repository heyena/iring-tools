using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.UserSecurity;

namespace org.iringtools.applicationConfig
{
    [Serializable]
    [DataContract(Name = "dataDictionary", Namespace = "http://www.iringtools.org/library")]
    public class DataDictionary
    {
        public DataDictionary()
        {
            dataObjects = new List<DataObject>();
            picklists = new List<PicklistObject>();
            //unionObjects = new List<UnionObject>();
        }

        [DataMember(Order = 0)]
        public List<DataObject> dataObjects { get; set; }

        [DataMember(Order = 1)]
        public List<PicklistObject> picklists { get; set; }

        //[[New fields
        [DataMember(Order = 2)]
        public Guid dictionaryId { get; set; }

        [DataMember(Order = 3)]
        public Guid applicationId { get; set; }

        [DataMember(Order = 4)]
        public bool isDBDictionary { get; set; }
        //]]

        [DataMember(Order = 5)]
        public bool enableSearch { get; set; }

        [DataMember(Order = 6)]
        public bool enableSummary { get; set; }

        [DataMember(Order = 7)]
        public string dataVersion { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string description { get; set; }

        //UnionObjects are not handled at db level for the time being[17-Mar-2015]
        //[DataMember(IsRequired = false, Order = 9)]
        //public List<UnionObject> unionObjects { get; set; }

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
        //[[New Fields
        [DataMember(IsRequired = true, Order = 0)]
        public Guid dataObjectId { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public Guid dictionaryId { get; set; }
        //]]

        [DataMember(IsRequired = true, Order = 2)]
        public string tableName { get; set; }

        [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
        public string objectNamespace { get; set; }

        [DataMember(IsRequired = true, Order = 4)]
        public string objectName { get; set; }

        [DataMember(IsRequired = false, Order = 5, EmitDefaultValue = false)]
        public string keyDelimeter { get; set; }

        [DataMember(IsRequired = true, Order = 6)]
        public List<KeyProperty> keyProperties { get; set; }

        [DataMember(IsRequired = true, Order = 7)]
        public List<DataProperty> dataProperties { get; set; }

        [DataMember(IsRequired = false, Order = 8, EmitDefaultValue = false)]
        public List<DataRelationship> dataRelationships { get; set; }

        [DataMember(IsRequired = false, Order = 9, EmitDefaultValue = false)]
        public bool isReadOnly { get; set; }

        [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
        public bool hasContent { get; set; }

        [DataMember(IsRequired = false, Order = 11, EmitDefaultValue = false)]
        public bool isListOnly { get; set; }

        [DataMember(IsRequired = false, Order = 12, EmitDefaultValue = false)]
        public string defaultProjectionFormat { get; set; }

        [DataMember(IsRequired = false, Order = 13, EmitDefaultValue = false)]
        public string defaultListProjectionFormat { get; set; }

        [DataMember(IsRequired = false, Order = 14, EmitDefaultValue = false)]
        public string description { get; set; }

        [DataMember(IsRequired = false, Order = 15, EmitDefaultValue = false)]
        public bool isRelatedOnly { get; set; }

        [DataMember(IsRequired = false, Order = 16, EmitDefaultValue = false)]
        public string groupName { get; set; }

        [DataMember(IsRequired = false, Order = 17, EmitDefaultValue = false)]
        public Dictionary<string, string> aliasDictionary { get; set; }

        [DataMember(IsRequired = false, Order = 18, EmitDefaultValue = false)]
        public string version { get; set; }

        [DataMember(IsRequired = false, Order = 19, EmitDefaultValue = false)]
        public DataFilter dataFilter { get; set; }

        [DataMember(IsRequired = false, Order = 20, EmitDefaultValue = false)]
        public bool isHidden { get; set; }

        [DataMember(IsRequired = false, Order = 21)]
        public List<ExtensionProperty> extensionProperties { get; set; }


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

        //[[New Fields
        [DataMember(IsRequired = true, Order = 0)]
        public Guid dataPropertyId { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public Guid dataObjectId { get; set; }

        [DataMember(IsRequired = true, Order = 2)]
        public int pickListId { get; set; }
        //]]

        [DataMember(IsRequired = true, Order = 3)]
        public string columnName { get; set; }

        [DataMember(IsRequired = true, Order = 4)]
        public string propertyName { get; set; }

        [DataMember(IsRequired = true, Order = 5)]
        public DataType dataType { get; set; }

        [DataMember(IsRequired = true, Order = 6)]
        public int dataLength { get; set; }

        [DataMember(IsRequired = true, Order = 7)]
        public bool isNullable { get; set; }

        [DataMember(IsRequired = true, Order = 8)]
        public KeyType keyType { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 9)]
        public bool showOnIndex { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public int numberOfDecimals { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 11)]
        public bool isReadOnly { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 12)]
        public bool showOnSearch { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 13)]
        public bool isHidden { get; set; }

        [DataMember(IsRequired = false, Order = 14, EmitDefaultValue = false)]
        public string description { get; set; }

        [DataMember(IsRequired = false, Order = 15, EmitDefaultValue = false)]
        public Dictionary<string, string> aliasDictionary { get; set; }

        [DataMember(IsRequired = false, Order = 16, EmitDefaultValue = false)]
        public string referenceType { get; set; }

        [DataMember(IsRequired = false, Order = 17, EmitDefaultValue = false)]
        public bool isVirtual { get; set; }

        [DataMember(IsRequired = false, Order = 18)]
        public int precision { get; set; }

        [DataMember(IsRequired = false, Order = 19)]
        public int scale { get; set; }
    }

    [Serializable]
    [DataContract(Name = "extensionProperty", Namespace = "http://www.iringtools.org/library")]
    public class ExtensionProperty
    {
        //[[New Field 
        [DataMember(IsRequired = true, Order = 0)]
        public Guid extensionPropertyId { get; set; }

        [DataMember(IsRequired = true, Order = 1)]
        public Guid dataObjectId { get; set; }

        //]]

        [DataMember(IsRequired = true, Order = 2)]
        public string columnName { get; set; }

        [DataMember(IsRequired = true, Order = 3)]
        public string propertyName { get; set; }

        [DataMember(IsRequired = true, Order = 4)]
        public DataType dataType { get; set; }

        [DataMember(IsRequired = true, Order = 5)]
        public int dataLength { get; set; }

        [DataMember(IsRequired = true, Order = 6)]
        public bool isNullable { get; set; }

        [DataMember(IsRequired = true, Order = 7)]
        public KeyType keyType { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public bool showOnIndex { get; set; }

        [DataMember(IsRequired = false, Order = 9)]
        public int precision { get; set; }

        [DataMember(IsRequired = false, Order = 10)]
        public int scale { get; set; }

        [DataMember(IsRequired = false, Order = 11)]
        public string definition { get; set; }

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
        //[[New Field 
        [DataMember(IsRequired = true, Order = 0)]
        public Guid dataObjectId { get; set; }
        //]]

        [DataMember(IsRequired = true, Order = 1)]
        public string keyPropertyName { get; set; }
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
        //[[New Fields
        [DataMember(IsRequired = true, Order = 0)]
        public Guid relationshipId { get; set; }
        //]]
        [DataMember(IsRequired = true, Order = 1)]
        public string dataPropertyName { get; set; }

        [DataMember(IsRequired = true, Order = 2)]
        public string relatedPropertyName { get; set; }
    }

    [Serializable]
    [DataContract(Name = "dataRelationship", Namespace = "http://www.iringtools.org/library")]
    public class DataRelationship
    {
        public DataRelationship()
        {
            this.propertyMaps = new List<PropertyMap>();
        }
        //[[New Fields
        [DataMember(Order = 0, Name = "relationshipId", IsRequired = true)]
        public Guid relationshipId { get; set; }

        [DataMember(Order = 1, Name = "dataObjectId", IsRequired = true)]
        public Guid dataObjectId { get; set; }
        //]]

        [DataMember(Order = 2, Name = "propertyMaps", IsRequired = true)]
        public List<PropertyMap> propertyMaps { get; set; }

        [DataMember(Order = 3, Name = "relatedObjectName", IsRequired = true)]
        public string relatedObjectName { get; set; }

        [DataMember(Order = 4, Name = "relationshipName", IsRequired = true)]
        public string relationshipName { get; set; }

        [DataMember(Order = 5, Name = "relationshipType", IsRequired = true)]
        public RelationshipType relationshipType { get; set; }
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

        //[[New Fields
        [DataMember(IsRequired = true, Order = 0 )]
        public Guid dictionaryId { get; set; }

        [DataMember(IsRequired = true,Order = 1)]
        public Guid pickListId { get; set; }
        //]]

        [DataMember(IsRequired = true, Order = 2)]
        public string name { get; set; }

        [DataMember(IsRequired = false, Order = 3)]
        public string description { get; set; }

        [DataMember(IsRequired = false, Order = 4)]
        public int valuePropertyIndex { get; set; }

        [DataMember(IsRequired = false, Order = 5)]
        public string tableName { get; set; }

        [DataMember(IsRequired = false, Order = 6)]
        public List<DataProperty> pickListProperties { get; set; }
    }

    //not considerted for the time being
    //[Serializable]
    //[DataContract(Name = "unionObject", Namespace = "http://www.iringtools.org/library")]
    //public class UnionObject
    //{
    //    public UnionObject()
    //    {
    //        unionTypes = new List<UnionType>();
    //    }

    //    [DataMember(IsRequired = false, Order = 0)]
    //    public string unionName { get; set; }

    //    [DataMember(IsRequired = false, Order = 1)]
    //    public List<UnionType> unionTypes { get; set; }
    //}

    //[Serializable]
    //[DataContract(Name = "unionType", Namespace = "http://www.iringtools.org/library")]
    //public class UnionType
    //{
    //    [DataMember(IsRequired = false, Order = 0)]
    //    public string unionType { get; set; }
    //}
}
