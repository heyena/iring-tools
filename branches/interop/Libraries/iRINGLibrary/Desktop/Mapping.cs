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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using org.iringtools.protocol.manifest;

namespace org.iringtools.common.mapping
{
  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "mapping")]
  public class Mapping : RootBase
  {
    private static readonly string RDL_NS = "http://rdl.rdlfacade.org/data#";
    private static readonly string RDF_NIL = "rdf:nil";

    public Mapping() : base()
    {
      GraphMaps = new List<GraphMap>();
      ValueListMaps = new List<ValueListMap>();
    }

    public GraphMap FindGraphMap(string graphName)
    {
      foreach (GraphMap graph in GraphMaps)
      {
        if (graph.Name.ToLower() == graphName.ToLower())
        {
          if (graph.ClassTemplateMaps.Count == 0)
            throw new Exception("Graph [" + graphName + "] is empty.");

          return graph;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }

    [DataMember(Name = "graphMaps", Order = 1, EmitDefaultValue = false)]
    public List<GraphMap> GraphMaps { get; set; }

    [DataMember(Name = "valueListMaps", EmitDefaultValue = false, Order = 2)]
    public List<ValueListMap> ValueListMaps { get; set; }

    public string ResolveValueList(string valueListName, string value)
    {
      foreach (ValueListMap valueListMap in ValueListMaps)
      {
        if (valueListMap.Name == valueListName)
        {
          foreach (ValueMap valueMap in valueListMap.ValueMaps)
          {
            if (valueMap.InternalValue == value)
            {
              return valueMap.Uri.Replace("rdl:", RDL_NS);
            }
          }
        }
      }

      return RDF_NIL;
    }

    public string ResolveValueMap(string valueListName, string qualifiedUri)
    {
      string uri = qualifiedUri.Replace(RDL_NS, "rdl:");

      foreach (ValueListMap valueListMap in ValueListMaps)
      {
        if (valueListMap.Name == valueListName)
        {
          foreach (ValueMap valueMap in valueListMap.ValueMaps)
          {
            if (valueMap.Uri == uri)
            {
              return valueMap.InternalValue;
            }
          }
        }
      }

      return String.Empty;
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "graphMap")]
  public class GraphMap : GraphBase 
  {
    public GraphMap()
    {
      ClassTemplateMaps = new List<ClassTemplateMap>();
    }

    [DataMember(Name = "classTemplateMaps", Order = 1, EmitDefaultValue = false)]
    public List<ClassTemplateMap> ClassTemplateMaps { get; set; }

    [DataMember(Name = "dataObjectName", EmitDefaultValue = false, Order = 2)]
    public string DataObjectName { get; set; }

    public ClassTemplateMap GetClassTemplateMap(string classId)
    {
      foreach (ClassTemplateMap classTemplateMap in ClassTemplateMaps)
      {
        if (classTemplateMap.ClassMap.ClassId == classId)
          return classTemplateMap;
      }

      return default(ClassTemplateMap);
    }

    public void AddClassMap(RoleMap roleMap, ClassMap classMap)
    {
      ClassTemplateMap classTemplateListMap = GetClassTemplateMap(classMap.ClassId);

      if (classTemplateListMap.ClassMap == null)
      {
        ClassTemplateMaps.Add(
          new ClassTemplateMap
          {
            ClassMap = classMap,
            TemplateMaps = new List<TemplateMap>()
          }
        );

        if (roleMap != null)
          roleMap.ClassMap = classMap;
      }
    }

    public void AddTemplateMap(ClassMap classMap, TemplateMap templateMap)
    {
      AddClassMap(null, classMap);
      ClassTemplateMap classTemplateMap = ClassTemplateMaps.Where(c => c.ClassMap.ClassId == classMap.ClassId).FirstOrDefault();
      if (classTemplateMap.ClassMap != null)
        classTemplateMap.TemplateMaps.Add(templateMap);
    }

    public void DeleteClassMap(string classId)
    {
      ClassTemplateMap classTemplateMap = GetClassTemplateMap(classId);

      if (classTemplateMap.ClassMap != null)
      {
        List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
        foreach (TemplateMap templateMap in templateMaps)
        {
          RoleMap classRole = templateMap.RoleMaps.Where(c => c.ClassMap != null).FirstOrDefault();
          if (classRole != null)
          {
            DeleteClassMap(classRole.ClassMap.ClassId);
            classRole.ClassMap = null;
          }
        }
        templateMaps.Clear();
        ClassTemplateMaps.Remove(classTemplateMap);
      }
    }

    public void DeleteTemplateMap(string classId, string templateId)
    {
      ClassTemplateMap classTemplateMap = GetClassTemplateMap(classId);
      if (classTemplateMap.ClassMap != null)
      {
        List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
        TemplateMap templateMap = classTemplateMap.TemplateMaps.Where(c => c.TemplateId == templateId).FirstOrDefault();
        RoleMap classRole = templateMap.RoleMaps.Where(c => c.ClassMap != null).FirstOrDefault();
        if (classRole != null)
          DeleteClassMap(classRole.ClassMap.ClassId);

        templateMaps.Remove(templateMap);
      }
    }

    public void DeleteRoleMap(TemplateMap templateMap, string roleId)
    {
      RoleMap roleMap = templateMap.RoleMaps.Where(c => c.RoleId == roleId).FirstOrDefault();
      if (roleMap != null)
      {
        if (roleMap.ClassMap != null)
        {
          DeleteClassMap(roleMap.ClassMap.ClassId);
          roleMap.ClassMap = null;
        }
      }
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "classTemplateMap")]
  public class ClassTemplateMap
  {
    public ClassTemplateMap()
    {
      TemplateMaps = new List<TemplateMap>();
    }

    [DataMember(Name = "classMap", Order = 0, EmitDefaultValue = false)]
    public ClassMap ClassMap { get; set; }

    [DataMember(Name = "templateMaps", Order = 1, EmitDefaultValue = false)]
    public List<TemplateMap> TemplateMaps { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "classMap")]
  public class ClassMap : ClassBase
  {
    public ClassMap()
    {
      Identifiers = new List<string>();
    }

    public ClassMap(ClassMap classMap)
      : this()
    {
      ClassId = classMap.ClassId;
      Name = classMap.Name;
      IdentifierDelimiter = String.Empty;

      foreach (string identifier in classMap.Identifiers)
      {
        Identifiers.Add(identifier);
      }
    }
    
    [DataMember(Name = "identifierDelimiter", EmitDefaultValue = false, Order = 2)]
    public string IdentifierDelimiter { get; set; }

    [DataMember(Name = "identifiers", EmitDefaultValue = false, Order = 3)]
    public List<string> Identifiers { get; set; }

    [DataMember(Name = "identifierValue", EmitDefaultValue = false, Order = 4)]
    public string IdentifierValue { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "templateMap")]
  public class TemplateMap : TemplateBase
  {
    public TemplateMap()
    {
      RoleMaps = new List<RoleMap>();
    }

    public TemplateMap(TemplateMap templateMap)
      : this()
    {
      TemplateId = templateMap.TemplateId;
      Name = templateMap.Name;

      foreach (RoleMap roleMap in templateMap.RoleMaps)
      {
        RoleMaps.Add(new RoleMap(roleMap));
      }
    }

    [DataMember(Name = "roleMaps", Order = 1, EmitDefaultValue = false)]
    public List<RoleMap> RoleMaps { get; set; }

    [DataMember(Name = "templateType", EmitDefaultValue = false, Order = 2)]
    public TemplateType TemplateType { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "roleMap")]
  public class RoleMap : RoleBase
  {
    public RoleMap() { }

    public RoleMap(RoleMap roleMap)
    {
      Type = roleMap.Type;
      RoleId = roleMap.RoleId;
      Name = roleMap.Name;
      DataType = roleMap.DataType;
      PropertyName = roleMap.PropertyName;
      Value = roleMap.Value;
      ValueListName = roleMap.ValueListName;
      ClassMap = roleMap.ClassMap;
    }

    [DataMember(Name = "propertyName", EmitDefaultValue = false, Order = 4)]
    public string PropertyName { get; set; }

    [DataMember(Name = "valueListName", EmitDefaultValue = false, Order = 6)]
    public string ValueListName { get; set; }

    [DataMember(Name = "classMap", EmitDefaultValue = false, Order = 7)]
    public ClassMap ClassMap { get; set; }

    public bool IsMapped
    {
      get
      {
        return ClassMap != null || !String.IsNullOrEmpty(PropertyName) || !String.IsNullOrEmpty(Value);
      }
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "valueListMap")]
  public class ValueListMap 
  {
    public ValueListMap()
    {
      ValueMaps = new List<ValueMap>();
    }

    [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "valueMaps", EmitDefaultValue = false, Order = 1)]
    public List<ValueMap> ValueMaps { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "valueMap")]
  public class ValueMap
  {
    [DataMember(Name = "internalValue", EmitDefaultValue = false, Order = 0)]
    public string InternalValue { get; set; }

    [DataMember(Name = "uri", EmitDefaultValue = false, Order = 1)]
    public string Uri { get; set; }
  }
}
