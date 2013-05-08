using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace RestDataLayer
{
    public class RestConfigSettings
    {
        public List<ConfigDataObject> dataObjects { get; set; }
        public string BaseURL { get; set; }
        public string AppKey { get; set; }
        public string AccessToken { get; set; }
        public string ContentType { get; set; }
        public bool EnablePaging { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
    }

    public class ConfigDataObject
    {
        public ConfigDataObject() { }

        public string tableName { get; set; }
        public string objectName { get; set; }
        public string objectUrl { get; set; }
        public string keyDelimeter { get; set; }

        public List<ConfigKeyProperty> keyProperties { get; set; }
        public List<ConfigDataProperty> dataProperties { get; set; }

    }

    public class ConfigDataProperty
    {
        public ConfigDataProperty() { }

        public string columnName { get; set; }
        public string propertyName { get; set; }
        public DataType dataType { get; set; }
        public int dataLength { get; set; }
        public bool isNullable { get; set; }
        public KeyType keyType { get; set; }
    }

    public class ConfigKeyProperty
    {
        public ConfigKeyProperty() { }
        public string keyPropertyName { get; set; }
    }
}
