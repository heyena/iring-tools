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
using System;
using org.iringtools.common.mapping;

namespace org.iringtools.protocol.manifest
{
  [DataContract(Namespace = "http://www.iringtools.org/protocol/manifest", Name = "manifest")]
  public class Manifest : RootBase 
  {
    public Manifest()
    {
      Graphs = new List<Graph>();
    }

    [DataMember(Name = "graphs", Order = 1, EmitDefaultValue = false)]
    public List<Graph> Graphs { get; set; }

    public Graph FindGraph(string graphName)
    {
      foreach (Graph graph in Graphs)
      {
        if (graph.Name.ToLower() == graphName.ToLower())
        {
          if (graph.ClassTemplatesList.Count == 0)
            throw new Exception("Graph [" + graphName + "] is empty.");

          return graph;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/protocol/manifest", Name = "graph")]
  public class Graph : GraphBase 
  {
    public Graph()
    {
      ClassTemplatesList = new List<ClassTemplates>();
    }

    [DataMember(Name = "classTemplateList", Order = 1, EmitDefaultValue = false)]
    public List<ClassTemplates> ClassTemplatesList { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/protocol/manifest", Name = "classTemplates")]
  public class ClassTemplates
  { 
    public ClassTemplates()
    {
      Templates = new List<Template>();
    }

    [DataMember(Name = "class", Order = 0, EmitDefaultValue = false)]
    public Class Class { get; set; }

    [DataMember(Name = "templates", Order = 1, EmitDefaultValue = false)]
    public List<Template> Templates { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/protocol/manifest", Name = "class")]
  public class Class : ClassBase { }

  [DataContract(Namespace = "http://www.iringtools.org/protocol/manifest", Name = "template")]
  public class Template : TemplateBase 
  {
    public Template()
    {
      Roles = new List<Role>();
    }

    [DataMember(Name = "roles", Order = 1, EmitDefaultValue = false)]
    public List<Role> Roles { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/protocol/manifest", Name = "role")]
  public class Role : RoleBase 
  {
    [DataMember(Name = "class", Order = 5, EmitDefaultValue = false)]
    public Class Class { get; set; }
  }

  [DataContract]
  public enum RoleType
  {

    [EnumMember]
    Property,

    [EnumMember]
    Reference,

    [EnumMember]
    Possessor,

    [EnumMember]
    FixedValue,

    [EnumMember]
    DataProperty,

    [EnumMember]
    ObjectProperty,
  }

  [DataContract]
  public enum TemplateType
  {
    [EnumMember]
    Qualification,

    [EnumMember]
    Definition
  }

  [DataContract]
  public enum TransferOption
  {
    [EnumMember]
    Desired,

    [EnumMember]
    Required,
  }

  #region base classes
  [DataContract]
  public abstract class RootBase
  {
    [DataMember(Name = "version", Order = 0, EmitDefaultValue = false)]
    public string Version { get; set; }
  }

  public abstract class GraphBase 
  {
    [DataMember(Name = "name", Order = 0, EmitDefaultValue = false)]
    public string Name { get; set; }
  }

  public abstract class ClassBase
  {
    [DataMember(Name = "name", Order = 0, EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "classId", Order = 1, EmitDefaultValue = false)]
    public string ClassId { get; set; }
  }

  public abstract class TemplateBase
  {
    [DataMember(Name = "name", Order = 0, EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "templateId", Order = 1, EmitDefaultValue = false)]
    public string TemplateId { get; set; }

    [DataMember(Name = "transferOption", Order = 2)]
    public TransferOption TransferOption { get; set; }
  }

  public abstract class RoleBase
  {
    [DataMember(Name = "name", Order = 0, EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "type", Order = 1, EmitDefaultValue = false)]
    public RoleType Type { get; set; }

    [DataMember(Name = "roleId", Order = 2, EmitDefaultValue = false)]
    public string RoleId { get; set; }

    [DataMember(Name = "dataType", Order = 3, EmitDefaultValue = false)]
    public string DataType { get; set; }

    [DataMember(Name = "value", Order = 4, EmitDefaultValue = false)]
    public string Value { get; set; }
  }
  #endregion
}
