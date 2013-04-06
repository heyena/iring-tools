﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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

namespace org.iringtools.adapter
{
  [XmlRoot(ElementName = "Envelope")]
  public class Envelope
  {
    [XmlArray]
    public List<DataTransferObject> Payload = new List<DataTransferObject>();
  }

  [DataContract]
  [XmlRoot]
  public class DTOProperty {
    public DTOProperty() { }

    public DTOProperty(string name, string oimProperty, object value, Type type, bool isIdentifier, bool isNullable) {      
      this.Name = name;
      this.OIMProperty = oimProperty;
      this.Value = value;
      this.Type = type;
      this.isIdentifier = isIdentifier;
      this.isNullable = isNullable;
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
        ValueString = value;
      }
    }

    [XmlIgnore]
    public object Value { get; set; }

    [XmlIgnore]
    public Type Type { get; set; }

    [XmlIgnore]
    public bool isIdentifier { get; set; }

    [XmlIgnore]
    public bool isNullable { get; set; }
  }

  [DataContract(Namespace = "http://rdl.rdlfacade.org/data#")]
  [XmlRoot]
  public abstract class DataTransferObject
  {
    protected string _identifier;
    protected object _dataObject = null;

    public DataTransferObject(string graphName)
    {
      this._properties = new List<DTOProperty>();
      this.GraphName = graphName;
    }

    [XmlIgnore]
    public string GraphName { get; set; }
    
    [DataMember(Name = "Identifier", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier
    {
      get
      {
        if (_identifier == null)
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
      object value = null;
      
      if (property != null)
      {
        switch (property.Type.Name)
        {
          case "Boolean":
            value = Convert.ToBoolean(property.Value);
            break;

          case "Int16":
            value = Convert.ToInt16(property.Value);
            break;

          case "Int32":
            value = Convert.ToInt32(property.Value);
            break;

          case "Int64":
            value = Convert.ToInt64(property.Value);
            break;

          case "Double":
            value = Convert.ToDouble(property.Value);
            break;

          case "DateTime":
            value = Convert.ToDateTime(property.Value).ToUniversalTime();
            break;

          default:
            value = (string)property.Value;
            break;
        }
      }

      return value;
    }

    public void SetPropertyValue(string propertyName, object value) {
      DTOProperty property = GetProperty(propertyName);

      if (property != null) {
        property.Value = value;
      }
    }

    protected DTOProperty GetProperty(string propertyName)
    {
      foreach (DTOProperty property in _properties)
      {
        if (property.Name == propertyName)
        {
          return property;
        }
      }

      return null;
    }

    public abstract object GetDataObject();
    public abstract void Write(string path);
    public abstract T Transform<T>(string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer);
    public abstract string Serialize();
  }
}