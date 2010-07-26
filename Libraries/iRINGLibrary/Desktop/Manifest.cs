// Copyright (c) 2010, ids-adi.org /////////////////////////////////////////////
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
using System.Collections.Generic;

namespace org.iringtools.library.manifest
{
  [DataContract(Namespace = "http://iringtools.org/adapter/library/manifest", Name = "manifest")]
  public class Manifest
  {
    public Manifest()
    {
      Graphs = new List<Graph>();
    }

    [DataMember(Name = "graphs", EmitDefaultValue = false)]
    public List<Graph> Graphs { get; set; }

    [DataMember(Name = "version", EmitDefaultValue = false)]
    public string Version { get; set; }
  }

  [DataContract(Namespace = "http://iringtools.org/adapter/library/manifest", Name = "graph")]
  public class Graph
  {
    public Graph()
    {
      ClassTemplatesMaps = new List<ClassTemplatesMap>();
    }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "classTemplatesMaps", EmitDefaultValue = false)]
    public List<ClassTemplatesMap> ClassTemplatesMaps { get; set; }
  }
  
  [DataContract(Namespace = "http://iringtools.org/adapter/library/manifest", Name = "classTemplatesMap")]
  public class ClassTemplatesMap
  {
    public ClassTemplatesMap()
    {
      Templates = new List<Template>();
    }

    [DataMember(Name = "class", EmitDefaultValue = false)]
    public Class Class { get; set; }

    [DataMember(Name = "templates", EmitDefaultValue = false)]
    public List<Template> Templates { get; set; }
  }

  [DataContract(Namespace = "http://iringtools.org/adapter/library/manifest", Name = "class")]
  public class Class
  {
    [DataMember(Name = "classId", EmitDefaultValue = false)]
    public string ClassId { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "transferOption", EmitDefaultValue = false)]
    public TransferOption TransferOption { get; set; }
  }

  [DataContract(Namespace = "http://iringtools.org/adapter/library/manifest", Name = "template")]
  public class Template
  {
    public Template()
    {
      Roles = new List<Role>();
    }

    [DataMember(Name = "templateId", EmitDefaultValue = false)]
    public string TemplateId { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "roles", EmitDefaultValue = false)]
    public List<Role> Roles { get; set; }

    [DataMember(Name = "transferOption", EmitDefaultValue = false)]
    public TransferOption TransferOption { get; set; }
  }

  [DataContract(Namespace = "http://iringtools.org/adapter/library/manifest", Name = "role")]
  public class Role
  {
    [DataMember(Name = "type", EmitDefaultValue = false)]
    public string Type { get; set; }

    [DataMember(Name = "roleId", EmitDefaultValue = false)]
    public string RoleId { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "dataType", EmitDefaultValue = false)]
    public string DataType { get; set; }

    [DataMember(Name = "propertyName", EmitDefaultValue = false)]
    public string PropertyName { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; }

    [DataMember(Name = "class", EmitDefaultValue = false)]
    public Class Class { get; set; }
  }

  [DataContract]
  public enum TransferOption
  {
    [EnumMember]
    Desired,

    [EnumMember]
    Required,
  }

  [DataContract]
  public enum RoleType
  {
    [EnumMember]
    FixedValue,

    [EnumMember]
    Possessor,

    [EnumMember]
    Property,

    [EnumMember]
    Reference,
  }
}
