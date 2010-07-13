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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  public class Mapping
  {
    private static readonly string RDL_NS = "http://rdl.rdlfacade.org/data#";
    private static readonly string RDF_NIL = "rdf:nil";

    public Mapping()
    {
      graphMaps = new List<GraphMap>();
      valueLists = new List<ValueList>();
    }

    [DataMember(EmitDefaultValue = false, Order = 0)]
    public List<GraphMap> graphMaps { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 1)]
    public List<ValueList> valueLists { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 2)]
    public string version { get; set; }

    public GraphMap FindGraphMap(string graphName)
    {
      foreach (GraphMap graphMap in graphMaps)
      {
        if (graphMap.name.ToLower() == graphName.ToLower())
        {
          if (graphMap.classTemplateListMaps.Count == 0)
            throw new Exception("Graph [" + graphName + "] is empty.");

          return graphMap;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }

    public string ResolveValueList(string valueList, string value)
    {
      foreach (ValueList valueLst in valueLists)
      {
        if (valueLst.name == valueList)
        {
          foreach (ValueMap valueMap in valueLst.valueMaps)
          {
            if (valueMap.internalValue == value)
            {
              return valueMap.uri.Replace("rdl:", RDL_NS);
            }
          }
        }
      }

      return RDF_NIL;
    }

    public string ResolveValueMap(string valueList, string qualifiedUri)
    {
      string uri = qualifiedUri.Replace(RDL_NS, "rdl:");

      foreach (ValueList valueLst in valueLists)
      {
        if (valueLst.name == valueList)
        {
          foreach (ValueMap valueMap in valueLst.valueMaps)
          {
            if (valueMap.uri == uri)
            {
              return valueMap.internalValue;
            }
          }
        }
      }

      return String.Empty;
    }
  }

  [DataContract]
  public class GraphMap
  {
    public GraphMap()
    {
      classTemplateListMaps = new Dictionary<ClassMap, List<TemplateMap>>();
    }

    [DataMember(EmitDefaultValue = false, Order = 0)]
    public string name { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 1)]
    public Dictionary<ClassMap, List<TemplateMap>> classTemplateListMaps { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 2)]
    public string dataObjectMap { get; set; }  // top level data object

    public KeyValuePair<ClassMap, List<TemplateMap>> GetClassTemplateListMap(string classId)
    {
      foreach (var pair in classTemplateListMaps)
      {
        if (pair.Key.classId == classId)
          return pair;
      }

      return default(KeyValuePair<ClassMap, List<TemplateMap>>);
    }

    // roleMap is not required for root node
    public void AddClassMap(RoleMap roleMap, ClassMap classMap)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = GetClassTemplateListMap(classMap.classId);

      if (classTemplateListMap.Key == null)
      {
        classTemplateListMaps.Add(classMap, new List<TemplateMap>());

        if (roleMap != null)
          roleMap.classMap = classMap;
      }
    }

    // classMap can have more than one templateMaps of the same templateIds
    public void AddTemplateMap(ClassMap classMap, TemplateMap templateMap)
    {
      AddClassMap(null, classMap);
      KeyValuePair<ClassMap, List<TemplateMap>> pair = classTemplateListMaps.Where(c => c.Key.classId == classMap.classId).FirstOrDefault();
      if (pair.Key != null)
        pair.Value.Add(templateMap);
    }

    public void DeleteClassMap(string classId)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = GetClassTemplateListMap(classId);

      if (classTemplateListMap.Key != null)
      {
        List<TemplateMap> templateMaps = classTemplateListMap.Value;
        foreach (TemplateMap templateMap in templateMaps)
        {
          RoleMap classRole = templateMap.roleMaps.Where(c => c.classMap != null).FirstOrDefault();
          if (classRole != null)
          {
            DeleteClassMap(classRole.classMap.classId);
            classRole.classMap = null;
          }
        }
        templateMaps.Clear();
        classTemplateListMaps.Remove(classTemplateListMap.Key);
      }
    }

    public void DeleteTemplateMap(string classId, string templateId)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = GetClassTemplateListMap(classId);
      if (classTemplateListMap.Key != null)
      {
        List<TemplateMap> templateMaps = classTemplateListMap.Value;
        TemplateMap templateMap = classTemplateListMap.Value.Where(c => c.templateId == templateId).FirstOrDefault();
        RoleMap classRole = templateMap.roleMaps.Where(c => c.classMap != null).FirstOrDefault();
        if (classRole != null)
          DeleteClassMap(classRole.classMap.classId);

        templateMaps.Remove(templateMap);
      }
    }

    public void DeleteRoleMap(TemplateMap templateMap, string roleId)
    {
      RoleMap roleMap = templateMap.roleMaps.Where(c => c.roleId == roleId).FirstOrDefault();
      if (roleMap != null)
      {
        if (roleMap.classMap != null)
        {
          DeleteClassMap(roleMap.classMap.classId);
          roleMap.classMap = null;
        }
      }
    }
  }

  [DataContract]
  public class ClassMap
  {
    public ClassMap()
    {
      identifiers = new List<string>();
    }

    public ClassMap(ClassMap classMap)
      : this()
    {
      classId = classMap.classId;
      name = classMap.name;
      identifierDelimeter = String.Empty;

      foreach (string identifier in classMap.identifiers)
      {
        identifiers.Add(identifier);
      }
    }

    [DataMember(EmitDefaultValue = false, Order = 0)]
    public string classId { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 1)]
    public string name { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 2)]
    public string identifierDelimeter { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 3)]
    public List<string> identifiers { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 4)]
    public string identifierValue { get; set; }
  }

  [DataContract]
  public class TemplateMap
  {
    public TemplateMap()
    {
      roleMaps = new List<RoleMap>();
    }

    public TemplateMap(TemplateMap templateMap)
      : this()
    {
      templateId = templateMap.templateId;
      name = templateMap.name;

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        roleMaps.Add(new RoleMap(roleMap));
      }
    }

    public TemplateMap(TemplateMap templateMap, RoleMap roleMap)
      : this(templateMap)
    {
      AddRoleMap(roleMap);
    }

    [DataMember(EmitDefaultValue = false, Order = 0)]
    public string templateId { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 1)]
    public string name { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 2)]
    public List<RoleMap> roleMaps { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 3)]
    public TemplateType templateType { get; set; }

    // roleId is unique within templateMap scope
    public void AddRoleMap(RoleMap roleMap)
    {
      bool found = false;

      foreach (RoleMap role in roleMaps)
      {
        if (role.roleId == roleMap.roleId)
        {
          found = true;
          break;
        }
      }

      if (!found)
      {
        roleMaps.Add(roleMap);
      }
    }
  }

  [DataContract]
  public class RoleMap
  {
    public RoleMap() { }

    public RoleMap(RoleMap roleMap)
    {
      type = roleMap.type;
      roleId = roleMap.roleId;
      name = roleMap.name;
      dataType = roleMap.dataType;
      propertyName = roleMap.propertyName;
      value = roleMap.value;
      valueList = roleMap.valueList;
      classMap = roleMap.classMap;
    }

    [DataMember(Order = 0)]
    public RoleType type { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 1)]
    public string roleId { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 2)]
    public string name { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 3)]
    public string dataType { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 4)]
    public string propertyName { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 5)]
    public string value { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 6)]
    public string valueList { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 7)]
    public ClassMap classMap { get; set; }

    public bool isMapped
    {
      get
      {
        return classMap != null || !String.IsNullOrEmpty(propertyName) || !String.IsNullOrEmpty(value);
      }
    }
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
  public class ValueList 
  {
    public ValueList()
    {
      valueMaps = new List<ValueMap>();
    }

    [DataMember(EmitDefaultValue = false, Order = 0)]
    public string name { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 1)]
    public List<ValueMap> valueMaps { get; set; }
  }

  [DataContract]
  public class ValueMap
  {
    [DataMember(EmitDefaultValue = false, Order = 0)]
    public string internalValue { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 1)]
    public string uri { get; set; }
  }
}
