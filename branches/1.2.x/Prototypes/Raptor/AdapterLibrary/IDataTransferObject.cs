using System;

namespace org.iringtools.adapter
{
  public interface IDataTransferObject
  {
    object GetDataObject();
    object GetPropertyValue(string propertyName);
    string GraphName { get; set; }
    string Identifier { get; set; }
    string Serialize();
    void SetPropertyValue(string propertyName, object value);
    T Transform<T>(string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer);
    void Write(string path);
  }
}
