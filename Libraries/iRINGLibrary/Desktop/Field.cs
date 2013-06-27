using System.Runtime.Serialization;
using System.ComponentModel;

namespace org.iringtools.library
{
	[DataContract(Name = "Field", Namespace = "http://www.iringtools.org/library")]
	public class Field
	{
		[DataMember(Name = "name", IsRequired = true, Order = 0)]
		public string name { get; set; }  // header

		[DataMember(Name = "dataIndex", IsRequired = true, Order = 1)]
		public string dataIndex { get; set; }
    
		[DataMember(Name = "type", IsRequired = true, Order = 2)]
		public string type { get; set; }  // data type

		[DataMember(Name = "width", IsRequired = true, Order = 3)]
		public int width { get; set; }
    
    [DataMember(Name = "fixed", IsRequired = true, Order = 4)]
    public bool @fixed { get; set; }

		[DataMember(Name = "filterable", IsRequired = true, Order = 5)]
    public bool filterable { get; set; }

		[DataMember(Name = "sortable", IsRequired = true, Order = 6)]
    public bool sortable { get; set; }

    [DataMember(Name = "keytype", IsRequired = true, Order = 7)]
    public string keytype { get; set; }

    [DataMember(Name = "hidden", EmitDefaultValue = false, Order = 8)]
    public bool hidden { get; set; }
	}
}