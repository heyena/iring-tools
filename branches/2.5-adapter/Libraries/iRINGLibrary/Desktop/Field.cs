using System.Runtime.Serialization;
using System.ComponentModel;

namespace org.iringtools.library
{
	[DataContract(Name = "Field", Namespace = "http://www.iringtools.org/library")]
	public class Field
	{
		[DataMember(Name = "text", IsRequired = true, Order = 0)]
		public string Name { get; set; }  // header

		[DataMember(Name = "dataIndex", IsRequired = true, Order = 1)]
		public string DataIndex { get; set; }

		[DataMember(Name = "type", IsRequired = true, Order = 2)]
		public string Type { get; set; }  // data type

		[DataMember(Name = "width", IsRequired = true, Order = 3)]
		public int Width { get; set; }

    [DataMember(Name = "fix", IsRequired = true, Order = 4, EmitDefaultValue = false)]
		public bool Fix = false;  // fixed width

    [DataMember(Name = "filterable", IsRequired = true, Order = 5, EmitDefaultValue = false)]
		public bool Filterable = true;

		[DataMember(Name = "sortable", IsRequired = true, Order = 6, EmitDefaultValue = false)]
		public bool Sortable = true;

    [DataMember(Name = "keytype", IsRequired = true, Order = 7, EmitDefaultValue = false)]
    public string Keytype = null;

  
	}
}