﻿using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace org.iringtools.library
{
	[DataContract(Name = "Grid", Namespace = "http://www.iringtools.org/library")]
	public class Grid
	{
		[DataMember(Name = "total", IsRequired = true, Order = 0)]
		public long Total { get; set; }
		
		[DataMember(Name = "fields", IsRequired = true, Order = 1)]
		public List<Field> Fields { get; set; }

		[DataMember(Name = "data", IsRequired = true, Order = 2)]
		public List<List<String>> Data { get; set; }		
	}

}