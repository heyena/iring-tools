using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace org.iringtools.applicationConfig_Retire
{
    [Serializable]
    [DataContract(Name = "databaseDictionary", Namespace = "http://www.iringtools.org/library")]
    public class DatabaseDictionary : DataDictionary
    {
        [DataMember(Name = "provider", IsRequired = true, Order = 0)]
        public string Provider { get; set; }

        [DataMember(Name = "connectionString", IsRequired = true, Order = 1)]
        public string ConnectionString { get; set; }

        [DataMember(Name = "schemaName", IsRequired = true, Order = 2)]
        public string SchemaName { get; set; }

        //[DataMember(EmitDefaultValue = false, Order = 3)]
        //public IdentityConfiguration IdentityConfiguration { get; set; }
    }

    //[CollectionDataContract(Namespace = "http://www.iringtools.org/library", ItemName = "objectConfiguration",
    //  KeyName = "objectName", ValueName = "identityProperties")]
    //public class IdentityConfiguration : Dictionary<string, IdentityProperties>
    //{ }
    //[DataContract(Name = "identityProperties", Namespace = "http://www.iringtools.org/library")]
    //public class IdentityProperties
    //{
    //    [DataMember(Name = "useIdentityFilter", IsRequired = true, Order = 0)]
    //    public bool UseIdentityFilter { get; set; }

    //    [DataMember(Name = "identityProperty", IsRequired = true, Order = 1)]
    //    public string IdentityProperty { get; set; }

    //    [DataMember(Name = "keyRingProperty", IsRequired = true, Order = 2)]
    //    public string KeyRingProperty { get; set; }

    //    [DataMember(Name = "isCaseSensitive", Order = 3, EmitDefaultValue = false)]
    //    public bool IsCaseSensitive { get; set; }
    //}
    [DataContract(Namespace = "http://www.iringtools.org/library")]
    public enum Provider
    {
        [EnumMember]
        MsSql2000,
        [EnumMember]
        MsSql2005,
        [EnumMember]
        MsSql2008,
        [EnumMember]
        MsSql2012,
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
        Oracle11g,
        [EnumMember]
        Oracle12c,
        [EnumMember]
        OracleLite,
        [EnumMember]
        PostgresSql81,
        [EnumMember]
        PostgresSql82,
        [EnumMember]
        SqLite
    }
}
