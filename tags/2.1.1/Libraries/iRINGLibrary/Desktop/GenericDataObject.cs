﻿using System;
using System.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.library
{
    public class GenericDataObject : Dictionary<string, object>, IDataObject
    {
        public object GetPropertyValue(string propertyName)
        {
            return this[propertyName];
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            this[propertyName] = value;
        }

        public string ObjectType { get; set; }
    }
}