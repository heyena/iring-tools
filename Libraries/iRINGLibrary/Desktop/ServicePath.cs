using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace org.iringtools.library
{
	[DataContract(Name = "servicepath", Namespace = "http://www.iringtools.org/library")]
	public class ServicePath
	{
		[DataMember(Name = "value", IsRequired = true, Order = 0)]
		public string value { get; set; }
	}

}