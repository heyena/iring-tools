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

using System;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.Serialization;

namespace org.iringtools.library.manifest
{
  [DataContract(Namespace = "http://www.iringtools.org/dxfr/manifest", Name = "manifest")]
  public class Manifest
  {
    public Manifest()
    {
      Graphs = new List<Graph>();
    }

    [DataMember(Name = "graphs", Order = 0, EmitDefaultValue = false)]
    public List<Graph> Graphs { get; set; }

    [DataMember(Name = "version", Order = 1, EmitDefaultValue = false)]
    public string Version { get; set; }

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

#if !SILVERLIGHT
    public Graph FindFirstGraphByClassId(string classId)
    {
      try
      {
        Graph graph = Graphs.Find(g => g.ClassTemplatesList.FirstOrDefault().Class.ClassId == classId);
        if (graph != null)
          return graph;
        else
          throw new Exception("Class [" + classId + "] does not exist.");
      }
      catch (Exception ex)
      {
        throw new Exception("Error while finding an appropriate graph map in a manifest.", ex);
      }
    }
#endif
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/manifest", Name = "graph")]
  public class Graph
  {
    public Graph()
    {
      ClassTemplatesList = new List<ClassTemplates>();
    }

    [DataMember(Name = "name", Order = 0, EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "classTemplatesList", Order = 1, EmitDefaultValue = false)]
    public List<ClassTemplates> ClassTemplatesList { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/manifest", Name = "classTemplates")]
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

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/manifest", Name = "class")]
  public class Class
  {
    [DataMember(Name = "classId", Order = 0, EmitDefaultValue = false)]
    public string ClassId { get; set; }

    [DataMember(Name = "name", Order = 1, EmitDefaultValue = false)]
    public string Name { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/manifest", Name = "template")]
  public class Template
  {
    public Template()
    {
      Roles = new List<Role>();
    }

    [DataMember(Name = "templateId", Order = 0, EmitDefaultValue = false)]
    public string TemplateId { get; set; }

    [DataMember(Name = "name", Order = 1, EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "roles", Order = 2, EmitDefaultValue = false)]
    public List<Role> Roles { get; set; }

    [DataMember(Name = "transferOption", Order = 3)]
    public TransferOption TransferOption { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/manifest", Name = "role")]
  public class Role
  {
    [DataMember(Name = "type", Order = 0, EmitDefaultValue = false)]
    public RoleType Type { get; set; }

    [DataMember(Name = "roleId", Order = 1, EmitDefaultValue = false)]
    public string RoleId { get; set; }

    [DataMember(Name = "name", Order = 2, EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "dataType", Order = 3, EmitDefaultValue = false)]
    public string DataType { get; set; }

    [DataMember(Name = "value", Order = 4, EmitDefaultValue = false)]
    public string Value { get; set; }

    [DataMember(Name = "class", Order = 5, EmitDefaultValue = false)]
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
}
