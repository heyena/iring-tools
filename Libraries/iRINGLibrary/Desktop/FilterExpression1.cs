using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.Generic;

namespace org.iringtools.library
{
	[DataContract(Name = "FilterExpression1", Namespace = "http://www.iringtools.org/library")]
	public class FilterExpression1
	{
		[DataMember(Name = "type", IsRequired = true, Order = 0)]
		public string type { get; set; }

		[DataMember(Name = "value", IsRequired = true, Order = 1)]
		public string value { get; set; }

		[DataMember(Name = "field", IsRequired = true, Order = 2)]
		public string field { get; set; }
	}
}