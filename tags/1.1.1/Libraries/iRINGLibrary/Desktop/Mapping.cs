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

using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace org.iringtools.library
{
  [XmlRoot(ElementName = "Mapping")]
  public class Mapping
  {
    [XmlArray(ElementName="GraphMaps")]
    public List<GraphMap> graphMaps { get; set; }

    [XmlArray(ElementName="ValueMaps")]
    public List<ValueMap> valueMaps { get; set; }

    [XmlElement(ElementName="Version")]
    public string version { get; set; }
  }

  [XmlRoot(ElementName = "GraphMap")]
  public class GraphMap : ClassMap
  {
    [XmlArray(ElementName = "DataObjectMaps")]
    public List<DataObjectMap> dataObjectMaps { get; set; }
  }

  [XmlRoot(ElementName = "ValueMap")]
  public class ValueMap
  {
    [XmlAttribute]
    public string valueList { get; set; }

    [XmlAttribute]
    public string internalValue { get; set; }

    [XmlAttribute]
    public string modelURI { get; set; }
  }

  [XmlRoot(ElementName = "TemplateMap")]
  public class TemplateMap
  {
    [XmlAttribute]
    public TemplateType type { get; set; }

    [XmlAttribute]
    public string templateId { get; set; }

    [XmlAttribute]
    public string name { get; set; }

    [XmlAttribute]
    public string classRole { get; set; }

    [XmlArray(ElementName="RoleMaps")]
    public List<RoleMap> roleMaps { get; set; }
  }

  [XmlRoot(ElementName = "RoleMap")]
  public class RoleMap
  {
    bool roleMapIsMapped = true;

    [XmlAttribute]
    public string roleId { get; set; }

    [XmlAttribute]
    public string name { get; set; }

    [XmlAttribute]
    public string dataType { get; set; }

    [XmlAttribute]
    public string propertyName { get; set; }

    [XmlAttribute]
    public string value { get; set; }

    [XmlAttribute]
    public string reference { get; set; }

    [XmlAttribute]
    public string valueList { get; set; }

    [XmlElement(ElementName="ClassMap")]
    public ClassMap classMap { get; set; }

    [XmlIgnore]
    public bool isMapped 
    {
      get { return roleMapIsMapped; }
      set { roleMapIsMapped = value; }
    }
  }

  [XmlRoot(ElementName = "ClassMap")]
  public class ClassMap
  {
    [XmlAttribute]
    public string name { get; set; }

    [XmlAttribute]
    public string classId { get; set; }

    [XmlAttribute]
    public string identifier { get; set; }

    [XmlArray(ElementName = "TemplateMaps")]
    public List<TemplateMap> templateMaps { get; set; }
  }

  public enum TemplateType
  {
    [XmlEnum]
    Property,

    [XmlEnum]
    Relationship,
  }

  [XmlRoot(ElementName = "DataObjectMap")]
  public class DataObjectMap
  {
    [XmlAttribute]
    public string name { get; set; }

    [XmlAttribute]
    public string inFilter { get; set; }

    [XmlAttribute]
    public string outFilter { get; set; }
  }
}

