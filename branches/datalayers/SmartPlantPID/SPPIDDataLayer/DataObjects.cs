using System;
using System.Collections.Generic;
using org.iringtools.library;

namespace iRINGTools.SDK.SPPIDDataLayer
{
    public class SPPIDDataObject : Dictionary<string, object>, IDataObject
    {
        public object GetPropertyValue(string propertyName)
        {
            return this[propertyName];
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            this[propertyName] = value;
        }
    }
}
