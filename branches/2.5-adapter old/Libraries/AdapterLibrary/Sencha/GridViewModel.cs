using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.adapter
{
    [DataContract(Name = "Column", Namespace = "http://www.iringtools.org/library")]
    public class ColumnVM
    {
        [DataMember(Name = "name", IsRequired = true, Order = 0)]
        public string Name { get; set; }  // header

        [DataMember(Name = "dataIndex", IsRequired = true, Order = 1)]
        public string DataIndex { get; set; }

        [DataMember(Name = "type", IsRequired = true, Order = 2)]
        public string Type { get; set; }  // data type

        [DataMember(Name = "width", IsRequired = true, Order = 3)]
        public int Width { get; set; }

        [DataMember(Name = "fixed", IsRequired = true, Order = 4)]
        public bool @Fixed { get; set; }

        [DataMember(Name = "filterable", IsRequired = true, Order = 5)]
        public bool Filterable { get; set; }

        [DataMember(Name = "sortable", IsRequired = true, Order = 6)]
        public bool Sortable { get; set; }

        [DataMember(Name = "keytype", IsRequired = true, Order = 7)]
        public string KeyType { get; set; }

        [DataMember(Name = "hidden", EmitDefaultValue = false, Order = 8)]
        public bool Hidden { get; set; }
    }

    [DataContract(Name = "Field", Namespace = "http://www.iringtools.org/library")]
    public class FieldVM
    {
        [DataMember(Name = "text", IsRequired = true, Order = 0)]
        public string Text { get; set; }  // header

        [DataMember(Name = "dataIndex", IsRequired = true, Order = 1)]
        public string DataIndex { get; set; }
    }

    [DataContract(Name = "metaData", Namespace = "http://www.iringtools.org/library")]
    public class MetaDataVM
    {
        [DataMember(Name = "fields", IsRequired = true, Order = 0)]
        IList<FieldVM> fields {get; set;}

        [DataMember(Name = "columns", IsRequired = true, Order = 1)]
        IList<ColumnVM> columns { get; set; }
    }

    [DataContract(Name = "GridResponse", Namespace = "http://www.iringtools.org/library")]
    public class GridResponseVM
    {
        [DataMember(Name = "total", IsRequired = true, Order = 0)]
        public long total { get; set; }

        [DataMember(Name = "data", IsRequired = true, Order = 1)]
        IList<IDictionary<string, string>> data {get; set;}

        [DataMember(Name = "metaData", IsRequired = true, Order = 2)]
        MetaDataVM MetaData { get; set; }
    }
}
