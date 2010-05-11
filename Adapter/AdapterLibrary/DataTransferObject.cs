// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Linq;

namespace org.iringtools.adapter
{
  [XmlRoot(ElementName = "Envelope", Namespace = "http://dto.iringtools.org")]
  public class Envelope
  {
    [XmlArray]
    public List<DataTransferObject> Payload = new List<DataTransferObject>();
  }

  [DataContract]
  [XmlRoot]
  public class DTOProperty {
    public DTOProperty() { }

    public DTOProperty(string name, string oimProperty, object value, Type type, bool isIdentifier) {      
      this.Name = name;
      this.OIMProperty = oimProperty;
      this.Value = value;
      this.Type = type;
      this.isIdentifier = isIdentifier;
    }

    [XmlIgnore]
    public string Name { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string OIMProperty { get; set; }

    [XmlAttribute(AttributeName="value")]
    public string ValueString {
      get
      {
        return (Value != null) ? Value.ToString() : string.Empty;
      }

      set
      {
        Value = value;
      }
    }

    [XmlIgnore]
    public object Value { get; set; }

    [XmlIgnore]
    public Type Type { get; set; }

    [XmlIgnore]
    public bool isIdentifier { get; set; }
  }

  [DataContract(Namespace = "http://dto.iringtools.org")]
  [XmlRoot(Namespace="http://dto.iringtools.org")]
  public abstract class DataTransferObject : IDataTransferObject
  {
    private string _identifier = String.Empty;
    protected object _dataObject = null;

    public DataTransferObject(string classId, string graphName)
    {
      this._properties = new List<DTOProperty>();
      this.GraphName = graphName;
      this.ClassId = classId;
    }

    [XmlIgnore]
    public string GraphName { get; set; }

    [DataMember(Name = "ClassId", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "classId")]
    public string ClassId { get; set; }

    [DataMember(Name = "Identifier", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier
    {
      get
      {
        if (String.IsNullOrEmpty(_identifier))
        {
          _identifier = String.Empty;

          foreach (DTOProperty property in _properties)
          {
            if (property.isIdentifier && property.Value != null)
            {
              if (_identifier != String.Empty && property.Value.ToString().Trim() != String.Empty)
              {
                _identifier += "_";
              }

              _identifier += property.Value;
            }
          }
        }

        return _identifier;
      }

      set
      {
        _identifier = value;
      }
    }

    [XmlArray("Properties")]
    [XmlArrayItem(ElementName = "Property")]
    public List<DTOProperty> _properties = null;

    public object GetPropertyValue(string propertyName)
    {
      DTOProperty property = GetProperty(propertyName);
      object value = ConvertToObject(property);
      
      return value;
    }

    public object GetPropertyValueByInternalName(string propertyName)
    {
      DTOProperty property = GetPropertyByInternalName(propertyName);
      object value = ConvertToObject(property);

      return value;
    }

    private object ConvertToObject(DTOProperty property)
    {
      if (property != null)
      {
        switch (property.Type.Name)
        {
          case "Boolean": return Convert.ToBoolean(property.Value);
          case "Byte": return Convert.ToByte(property.Value);
          case "DateTime": return Convert.ToDateTime(property.Value).ToUniversalTime();
          case "Decimal": return Convert.ToDecimal(property.Value);          
          case "Double": return Convert.ToDouble(property.Value);
          case "Int16": return Convert.ToInt16(property.Value);
          case "Int32": return Convert.ToInt32(property.Value);
          case "Int64": return Convert.ToInt64(property.Value);
          case "SByte": return Convert.ToSByte(property.Value);
          case "Single": return Convert.ToSingle(property.Value);
          default: return (string)property.Value;
        }
      }

      return null;
    }

    public void SetPropertyValue(string propertyName, object value) {
      DTOProperty property = GetProperty(propertyName);

      if (property != null) {
        property.Value = value;
      }
    }

    public void SetPropertyValueByInternalName(string propertyName, object value)
    {
      DTOProperty property = GetPropertyByInternalName(propertyName);

      if (property != null)
      {
        property.Value = value;
      }
    }

    protected DTOProperty GetProperty(string propertyName)
    {
      DTOProperty namedProperty = 
					(from property in _properties
           where property.OIMProperty.ToUpper() == ((propertyName != null) ? propertyName.ToUpper() : string.Empty)
					 select property).FirstOrDefault<DTOProperty>();   

      return namedProperty;
    }

    protected DTOProperty GetPropertyByInternalName(string propertyName)
    {
      DTOProperty namedProperty =
          (from property in _properties
           where property.Name.ToUpper() == ((propertyName != null) ? propertyName.ToUpper() : string.Empty)
           select property).FirstOrDefault<DTOProperty>();

      return namedProperty;
    }

    public abstract object GetDataObject();
    public abstract void Write(string path);
    public abstract T Transform<T>(string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer);
    public abstract string Serialize();
  }

  [MessageContract(IsWrapped = true)]
  public class DTORequest
  {
    [MessageBodyMember]
    public string projectName { get; set; }

    [MessageBodyMember]
    public string applicationName { get; set; }
    
    [MessageBodyMember]
    public string graphName { get; set; }

    [MessageBodyMember]
    public string identifier { get; set; }
  }

  [MessageContract(IsWrapped = true)]
  public class DTOResponse
  {
    [MessageBodyMember]
    public DataTransferObject dto { get; set; }
  }

  [MessageContract(IsWrapped = true)]
  public class DTOListResponse
  {
    [MessageBodyMember]
    public List<DataTransferObject> dtoList { get; set; }
  }
}