using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library.iip
{
    [CollectionDataContract(Name = "dataObjects", Namespace = "http://www.iringtools.org/library", ItemName = "dataObject")]
    public class AppDataObjects : List<AppDataObject>
    {
    }

    [CollectionDataContract(Name = "properties", Namespace = "http://www.iringtools.org/library", ItemName = "property")]
    public class AppProperties : List<AppProperty>
    {
    }

    /// <summary>
    /// This class represents a data object node in the Configuration.[app].xml file
    /// </summary>
    [DataContract(Name = "dataObject", Namespace = "http://www.iringtools.org/library")]
    public class AppDataObject
    {
        /// <summary>
        /// The name of the data object
        /// </summary>
        /// <returns>a string</returns>
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        /// <summary>
        /// The description of the data object
        /// </summary>
        /// <returns>a string</returns>
        [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// An optional SPPID filter string
        /// </summary>
        /// <returns>a string</returns>
        [DataMember(Name = "filter", Order = 2, EmitDefaultValue = false)]
        public string Filter { get; set; }

        /// <summary>
        /// The collection of associated AppProperties
        /// </summary>
        /// <returns>A strongly type List of AppProperty objects</returns>
        [DataMember(Name = "applications", Order = 3)]
        public AppProperties Applications { get; set; }
    }

    [DataContract(Name = "property", Namespace = "http://www.iringtools.org/library")]
    public class AppProperty
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        /// <returns>the property Name</returns>
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        /// <summary>
        /// The display name of the property
        /// </summary>
        /// <returns>the property display name</returns>
        [DataMember(Name = "displayName", Order = 1, EmitDefaultValue = false)]
        public string DisplayName { get; set; }

        /// <summary>
        /// The SPPID subclass of the property
        /// </summary>
        /// <returns>the SPPID subclass</returns>
        [DataMember(Name = "subclass", Order = 2, EmitDefaultValue = false)]
        public string Subclass { get; set; }

        /// <summary>
        /// The source of the property
        /// </summary>
        /// <returns>the application source</returns>
        [DataMember(Name = "appSource", Order = 3, EmitDefaultValue = false)]
        public string ApplicationSource { get; set; }

        /// <summary>
        /// The native name of the property
        /// </summary>
        /// <returns>the native property name</returns>
        [DataMember(Name = "nativeName", Order = 4, EmitDefaultValue = false)]
        public string NativeName { get; set; }

        /// <summary>
        /// The data type of the property
        /// </summary>
        /// <returns>the property data type</returns>
        [DataMember(Name = "dataType", Order = 5, EmitDefaultValue = false)]
        public string DataType { get; set; }

        /// <summary>
        /// The data length of the property
        /// </summary>
        /// <returns>the property data length</returns>
        [DataMember(Name = "length", Order = 6, EmitDefaultValue = false)]
        public string DataLength { get; set; }

        /// <summary>
        /// The number of decimal places of the property
        /// </summary>
        /// <returns>the property decimals</returns>
        [DataMember(Name = "decimals", Order = 7, EmitDefaultValue = false)]
        public string DataDecimals { get; set; }

        /// <summary>
        /// The property read-only flag
        /// </summary>
        /// <returns>is the property read-only</returns>
        [DataMember(Name = "readOnly", Order = 8, EmitDefaultValue = false)]
        public string ReadOnly { get; set; }

        /// <summary>
        /// The property codelist option
        /// </summary>
        /// <returns>an option if the property has a codelist</returns>
        [DataMember(Name = "codelistOption", Order = 9, EmitDefaultValue = false)]
        public string CodelistOption { get; set; }

        /// <summary>
        /// The property key flag
        /// </summary>
        /// <returns>is the property a key</returns>
        [DataMember(Name = "isKey", Order = 10, EmitDefaultValue = false)]
        public bool IsKey { get; set; }

        /// <summary>
        /// The property parse option
        /// </summary>
        /// <returns>a parse option for the property</returns>
        [DataMember(Name = "parse", Order = 11, EmitDefaultValue = false)]
        public string Parse { get; set; }

        /// <summary>
        /// The receiving scope for the property
        /// </summary>
        /// <returns>the property receive scope</returns>
        [DataMember(Name = "receiveScope", Order = 12, EmitDefaultValue = false)]
        public string ReceiveScope { get; set; }

        /// <summary>
        /// Is the property a symbol key
        /// </summary>
        /// <returns>is the property a key for symbol selection mapping</returns>
        [DataMember(Name = "symbolKey", Order = 13, EmitDefaultValue = false)]
        public bool SymbolKey { get; set; }
    }
}
