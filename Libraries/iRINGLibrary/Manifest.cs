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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using org.iringtools.mapping;

namespace org.iringtools.dxfr.manifest
{
    [DataContract(Name = "manifest", Namespace = "http://www.iringtools.org/dxfr/manifest")]
    public partial class Manifest
    {
        public Manifest()
        {
            Graphs = new Graphs();
            ValueListMaps = new org.iringtools.mapping.ValueListMaps();
        }

        [DataMember(Name = "graphs", EmitDefaultValue = false, Order = 0)]
        public Graphs Graphs { get; set; }

        [DataMember(Name = "version", EmitDefaultValue = false, Order = 1)]
        public string Version { get; set; }

        [DataMember(Name = "valueListMaps", EmitDefaultValue = false, Order = 2)]
        public ValueListMaps ValueListMaps { get; set; }
    }

    [CollectionDataContract(Name = "graphs", Namespace = "http://www.iringtools.org/dxfr/manifest", ItemName = "graph")]
    public class Graphs : List<Graph>
    {
    }

    [DataContract(Name = "graph", Namespace = "http://www.iringtools.org/dxfr/manifest")]
    public partial class Graph
    {
        public Graph()
        {
            ClassTemplatesList = new ClassTemplatesList();
        }

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "classTemplatesList", EmitDefaultValue = false, Order = 1)]
        public ClassTemplatesList ClassTemplatesList { get; set; }
    }

    [CollectionDataContract(Name = "classTemplatesList", Namespace = "http://www.iringtools.org/dxfr/manifest",
        ItemName = "classTemplates")]
    public class ClassTemplatesList : List<ClassTemplates>
    {
    }

    [DataContract(Name = "classTemplates", Namespace = "http://www.iringtools.org/dxfr/manifest")]
    public partial class ClassTemplates
    {
        public ClassTemplates()
        {
            Templates = new Templates();
        }

        [DataMember(Name = "class", EmitDefaultValue = false, Order = 0)]
        public Class Class { get; set; }

        [DataMember(Name = "templates", EmitDefaultValue = false, Order = 1)]
        public Templates Templates { get; set; }
    }

    [DataContract(Name = "class", Namespace = "http://www.iringtools.org/dxfr/manifest")]
    public partial class Class
    {
        [DataMember(Name = "id", EmitDefaultValue = false, Order = 0)]
        public string Id { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }
    }

    [CollectionDataContract(Name = "templates", Namespace = "http://www.iringtools.org/dxfr/manifest",
        ItemName = "template")]
    public class Templates : List<Template>
    {
    }

    [DataContract(Name = "template", Namespace = "http://www.iringtools.org/dxfr/manifest")]
    public partial class Template
    {
        public Template()
        {
            Roles = new Roles();
        }

        [DataMember(Name = "id", EmitDefaultValue = false, Order = 0)]
        public string Id { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(Name = "roles", EmitDefaultValue = false, Order = 2)]
        public Roles Roles { get; set; }

        [DataMember(Name = "transferOption", EmitDefaultValue = false, Order = 3)]
        public TransferOption TransferOption { get; set; }
    }

    [CollectionDataContract(Name = "roles",
        Namespace = "http://www.iringtools.org/dxfr/manifest", ItemName = "role")]
    public class Roles : System.Collections.Generic.List<org.iringtools.dxfr.manifest.Role>
    {
    }

    [DataContractAttribute(Name = "role", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public partial class Role
  {
        [DataMember(Name = "type", Order = 0)]
        public RoleType Type { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false, Order = 1)]
        public string Id { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 2)]
        public string Name { get; set; }

        [DataMember(Name = "dataType", EmitDefaultValue = false, Order = 3)]
        public string DataType { get; set; }

        [DataMember(Name = "value", EmitDefaultValue = false, Order = 4)]
        public string Value { get; set; }

        [DataMember(Name = "class", EmitDefaultValue = false, Order = 5)]
        public Class Class { get; set; }

        [DataMember(Name = "cardinality", EmitDefaultValue = false, Order = 6)]
        public Cardinality Cardinality { get; set; }

        [DataMember(Name = "dataLength", EmitDefaultValue = false, Order = 7)]
        public int DataLength { get; set; }
  }

  [DataContract(Name = "transferOption", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public enum TransferOption
  {

    [EnumMember]
    Unknown,

    [EnumMember]
    Desired,

    [EnumMember]
    Required,
  }

  [DataContract(Name = "cardinality", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public enum Cardinality
  {

    [EnumMember]
    Unknown,

    [EnumMember]
    Self,

    [EnumMember]
    OneToOne,

    [EnumMember]
    OneToMany,
  }
}

namespace org.iringtools.mapping
{
  using System.Runtime.Serialization;
  using System.Collections.Generic;
  using org.iringtools.library;

  [DataContract(Name = "roleType", Namespace = "http://www.iringtools.org/mapping")]
  public enum RoleType
  {
    [EnumMember]
    Unknown,

    [EnumMember]
    Property,

    [EnumMember]
    Possessor,

    [EnumMember]
    Reference,

    [EnumMember]
    FixedValue,

    [EnumMember]
    DataProperty,

    [EnumMember]
    ObjectProperty,
  }

    [System.Runtime.Serialization.DataContractAttribute(Name = "mapping",
        Namespace = "http://www.iringtools.org/mapping")]
    public partial class Mapping
    {
        public Mapping()
        {
            GraphMaps = new GraphMaps();
            ValueListMaps = new ValueListMaps();
        }

        [DataMember(Name = "graphMaps", EmitDefaultValue = false, Order = 0)]
        public GraphMaps GraphMaps { get; set; }

        [DataMember(Name = "valueListMaps", EmitDefaultValue = false, Order = 1)]
        public ValueListMaps ValueListMaps { get; set; }

        [DataMember(Name = "version", EmitDefaultValue = false, Order = 2)]
        public string Version { get; set; }

        public GraphMap FindGraphMap(string name)
        {
            return this.GraphMaps.FirstOrDefault(item => item.Name.ToLower() == name.ToLower());
        }
    }

    [CollectionDataContract(Name = "graphMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "graphMap")]
    public class GraphMaps : List<GraphMap>
    {
    }

    [CollectionDataContract(Name = "valueListMaps", Namespace = "http://www.iringtools.org/mapping",
        ItemName = "valueListMap")]
    public class ValueListMaps : List<ValueListMap>
    {
    }

    [DataContract(Name = "graphMap", Namespace = "http://www.iringtools.org/mapping")]
    public partial class GraphMap
    {
        public GraphMap()
        {
            ClassTemplateMaps = new ClassTemplateMaps();
        }

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "classTemplateMaps", EmitDefaultValue = false, Order = 1)]
        public ClassTemplateMaps ClassTemplateMaps { get; set; }

        [DataMember(Name = "dataObjectName", EmitDefaultValue = false, Order = 2)]
        public string DataObjectName { get; set; }

        [DataMember(Name = "datFilter", EmitDefaultValue = false, Order = 3)]
        public DataFilter DataFilter { get; set; }
    }

    [CollectionDataContract(Name = "classTemplateMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "classTemplateMap")]
  public class ClassTemplateMaps : List<ClassTemplateMap>
  {
  }

    [DataContract(Name = "classTemplateMap", Namespace = "http://www.iringtools.org/mapping")]
    public partial class ClassTemplateMap
    {
        public ClassTemplateMap()
        {
            TemplateMaps = new TemplateMaps();
        }

        [DataMember(Name = "classMap", EmitDefaultValue = false, Order = 0)]
        public ClassMap ClassMap { get; set; }

        [DataMember(Name = "templateMaps", EmitDefaultValue = false, Order = 2)]
        public TemplateMaps TemplateMaps { get; set; }
    }

    [DataContract(Name = "classMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class ClassMap
  {
        public ClassMap()
    {
      Identifiers = new Identifiers();
    }

        [DataMember(Name = "id", EmitDefaultValue = false, Order = 0)]
        public string Id { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(Name = "identifierDelimiter", EmitDefaultValue = false, Order = 2)]
        public string IdentifierDelimiter { get; set; }

        [DataMember(Name = "identifiers", EmitDefaultValue = false, Order = 3)]
        public Identifiers Identifiers { get; set; }

        [DataMember(Name = "identifierValue", EmitDefaultValue = false, Order = 4)]
        public string IdentifierValue { get; set; }
  }

  [CollectionDataContract(Name = "templateMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "templateMap")]
  public class TemplateMaps : List<TemplateMap>
  {
  }

  [CollectionDataContract(Name = "identifiers", Namespace = "http://www.iringtools.org/mapping", ItemName = "identifier")]
  public class Identifiers : List<string>
  {
  }

  [DataContract(Name = "templateMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class TemplateMap
  {
      public TemplateMap()
    {
      RoleMaps = new RoleMaps();
    }

      [DataMember(Name = "type", EmitDefaultValue = false, Order = 0)]
      public TemplateType Type { get; set; }

      [DataMember(Name = "id", EmitDefaultValue = false, Order = 1)]
      public string Id { get; set; }

      [DataMember(Name = "name", EmitDefaultValue = false, Order = 2)]
      public string Name { get; set; }

      [DataMember(Name = "roleMaps", EmitDefaultValue = false, Order = 3)]
      public RoleMaps RoleMaps { get; set; }
  }

  [DataContract(Name = "TemplateType", Namespace = "http://www.iringtools.org/mapping")]
  public enum TemplateType
  {

    [EnumMember]
    Qualification,

    [EnumMember]
    Definition,
  }

  [CollectionDataContract(Name = "roleMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "roleMap")]
  public class RoleMaps : List<RoleMap>
  {
  }

  [DataContract(Name = "roleMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class RoleMap
  {
      [DataMember(Name = "type", Order = 0)]
      public RoleType Type { get; set; }

      [DataMember(Name = "id", EmitDefaultValue = false, Order = 1)]
      public string Id { get; set; }

      [DataMember(Name = "name", EmitDefaultValue = false, Order = 2)]
      public string Name { get; set; }

      [DataMember(Name = "dataType", EmitDefaultValue = false, Order = 3)]
      public string DataType { get; set; }

      [DataMember(Name = "value", EmitDefaultValue = false, Order = 4)]
      public string Value { get; set; }

      [DataMember(Name = "propertyName", EmitDefaultValue = false, Order = 5)]
      public string PropertyName { get; set; }

      [DataMember(Name = "valueListName", EmitDefaultValue = false, Order = 6)]
      public string ValueListName { get; set; }

      [DataMember(Name = "classMap", EmitDefaultValue = false, Order = 7)]
      public ClassMap ClassMap { get; set; }

      public int DataLength { get; set; }
  }

  [DataContract(Name = "valueListMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class ValueListMap
  {
      public ValueListMap()
    {
      ValueMaps = new ValueMaps();
    }

      [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
      public string Name { get; set; }

      [DataMember(Name = "valueMaps", EmitDefaultValue = false, Order = 1)]
      public ValueMaps ValueMaps { get; set; }
  }

  [CollectionDataContract(Name = "valueMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "valueMap")]
  public class ValueMaps : List<ValueMap>
  {
  }

  [DataContract(Name = "valueMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class ValueMap
  {
      [DataMember(Name = "internalValue", EmitDefaultValue = false, Order = 0)]
      public string InternalValue { get; set; }

      [DataMember(Name = "uri", EmitDefaultValue = false, Order = 1)]
      public string Uri { get; set; }

      [DataMember(Name = "label", EmitDefaultValue = false, Order = 2)]
      public string Label { get; set; }
  }

  [CollectionDataContract(ItemName = "id")]
  public class TemplateIds : List<string> { }

  [DataContract(Name = "classificationTemplate", Namespace = "http://www.iringtools.org/mapping")]
  public class ClassificationTemplate
  {
    [DataMember(Name = "templateIds", EmitDefaultValue = false, Order = 0)]
    public TemplateIds TemplateIds { get; set; }

    [DataMember(Name = "templateMap", EmitDefaultValue = false, Order = 1)]
    public TemplateMap TemplateMap { get; set; }
  }

  [DataContract(Name = "classificationStyle", Namespace = "http://www.iringtools.org/mapping")]
  public enum ClassificationStyle
  {
    [EnumMember]
    Type,

    [EnumMember]
    Template,

    [EnumMember]
    Both
  }
}
