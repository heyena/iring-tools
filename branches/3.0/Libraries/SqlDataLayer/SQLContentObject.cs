namespace org.iringtools.adapter.datalayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using org.iringtools.library;
    using System.IO;
    using System.Data;

    /// <summary>
    /// 
    /// </summary>
    public class SQLContentObject : IContentObject
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();

        public SQLContentObject(DataObject dataObject, DataTable dataTable, byte[] streamLength)
        {
            foreach (DataProperty eachDataProperty in dataObject.dataProperties)
            {
                properties.Add(eachDataProperty.propertyName, dataTable.Rows[0][eachDataProperty.propertyName]);
            }

            Content = new MemoryStream(streamLength);
        }

        public Stream Content
        {
            get;
            set;
        }

        public string ContentType
        {
            get;
            set;
        }

        public string HashType
        {
            get;
            set;
        }

        public string HashValue
        {
            get;
            set;
        }

        public string Identifier
        {
            get;
            set;
        }

        public string ObjectType
        {
            get;
            set;
        }

        public string URL
        {
            get;
            set;
        }

        public object GetPropertyValue(string propertyName)
        {
            object propertyValue = properties[propertyName];

            return propertyValue;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            properties[propertyName] = value;
        }
    }
}
