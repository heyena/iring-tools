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
using org.iringtools.dxfr.manifest;
using org.iringtools.utility;
using org.iringtools.library;
using log4net;

namespace org.iringtools.mapping
{
    public static class MappingExtensions
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MappingExtensions));

        public static readonly string RDL_NS = "http://rdl.rdlfacade.org/data#";
        public static readonly string RDF_NIL = "rdf:nil";

        public static Graph FindGraph(this Manifest manifest, string graphName)
        {
            foreach (Graph manifestGraph in manifest.graphs)
            {
                if (manifestGraph.name.ToLower() == graphName.ToLower())
                {
                    if (manifestGraph.classTemplatesList.Count == 0)
                    {
                        _logger.Warn("Graph [" + graphName + "] is empty.");
                    }

                    return manifestGraph;
                }
            }

            return null;
        }

        public static GraphMap FindGraphMap(this Mapping mapping, string graphName)
        {
            foreach (GraphMap graph in mapping.graphMaps)
            {
                if (graph.name.ToLower() == graphName.ToLower())
                {
                    if (graph.classTemplateMaps.Count == 0)
                        _logger.Warn("Graph [" + graphName + "] is empty.");

                    return graph;
                }
            }

            return null;
        }

        public static void DeleteRoleMap(this GraphMap graphMap, TemplateMap templateMap, string roleId)
        {
            RoleMap roleMap = templateMap.roleMaps.Where(c => c.id == roleId).FirstOrDefault();
            if (roleMap != null)
            {
                if (roleMap.classMap != null)
                {
                    graphMap.DeleteClassMap(roleMap.classMap.id, roleMap.classMap.index);
                    roleMap.value = null;
                    roleMap.classMap = null;
                }
            }
        }

        public static void DeleteClassMap(this GraphMap graphMap, string classId, int classIndex)
        {
            ClassTemplateMap classTemplateMap = graphMap.GetClassTemplateMap(classId, classIndex);

            if (classTemplateMap != null && classTemplateMap.classMap != null)
            {
                List<TemplateMap> templateMaps = classTemplateMap.templateMaps;

                foreach (TemplateMap templateMap in templateMaps)
                {
                    RoleMap classRole = templateMap.roleMaps.Where(c => c.classMap != null).FirstOrDefault();
                    if (classRole != null)
                    {
                        graphMap.DeleteClassMap(classRole.classMap.id, classRole.classMap.index);
                        classRole.classMap = null;
                    }
                }

                templateMaps.Clear();
                graphMap.classTemplateMaps.Remove(classTemplateMap);
            }

            RearrangeClassMapIndex(graphMap, classId);

        }

        public static void RearrangeClassMapIndex(this GraphMap graphMap, string classId)
        {

            IList<ClassMap> classMaps = (from ctm in graphMap.classTemplateMaps
                                         where ctm.classMap.id == classId
                                         select ctm.classMap).ToList();
            int startIndex = -1;

            foreach (ClassMap cm in classMaps)
            {
                SetNewIndexOfClass(graphMap, classId, cm.index, startIndex);
                startIndex--;
            }

            startIndex = 0;
            foreach (ClassMap cm in classMaps)
            {
                SetNewIndexOfClass(graphMap, classId, cm.index, startIndex);
                startIndex++;
            }
        }


        private static void SetNewIndexOfClass(GraphMap graphMap, string classId, int oldIndex, int newIndex)
        {
            foreach (ClassTemplateMap ctm in graphMap.classTemplateMaps)
            {
                if (ctm.classMap.id == classId && ctm.classMap.index == oldIndex)
                    ctm.classMap.index = newIndex;

                foreach (TemplateMap tm in ctm.templateMaps)
                    foreach (RoleMap rm in tm.roleMaps)
                        if (rm.classMap != null && rm.classMap.id == classId && rm.classMap.index == oldIndex)
                            rm.classMap.index = newIndex;

            }
        }
        //public static ClassTemplateMap GetClassTemplateMap(this GraphMap graphMap, string classId)
        //{
        //  foreach (ClassTemplateMap classTemplateMap in graphMap.classTemplateMaps)
        //  {
        //    if (classTemplateMap.classMap.id == classId)
        //      return classTemplateMap;
        //  }

        //  return default(ClassTemplateMap);
        //}

        public static ClassTemplateMap GetClassTemplateMap(this GraphMap graphMap, string classId, int index)
        {
            foreach (ClassTemplateMap classTemplateMap in graphMap.classTemplateMaps)
            {
                if (classTemplateMap.classMap.id == classId && classTemplateMap.classMap.index == index)
                    return classTemplateMap;
            }

            return default(ClassTemplateMap);
        }

        public static int GetClassMapMaxIndex(this GraphMap graphMap, string classId)
        {
            var list = from c in graphMap.classTemplateMaps
                       where c.classMap.id == classId
                       select c.classMap.index;

            if (list != null && list.Count() > 0)
                return list.Max();
            else
                return -1;

        }

        public static ClassTemplateMap GetClassTemplateMapByName(this GraphMap graphMap, string className)
        {
            if (!String.IsNullOrEmpty(className))
            {
                foreach (ClassTemplateMap classTemplateMap in graphMap.classTemplateMaps)
                {
                    if (classTemplateMap.classMap != null &&
                      Utility.TitleCase(classTemplateMap.classMap.name).ToLower() ==
                        Utility.TitleCase(className).ToLower())
                        return classTemplateMap;
                }
            }

            return default(ClassTemplateMap);
        }

        public static void AddClassMap(this GraphMap graphMap, RoleMap roleMap, ClassMap classMap)
        {
            ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classMap.id, classMap.index);

            if (ctm == null)
            {
                graphMap.classTemplateMaps.Add(
                new ClassTemplateMap
                {
                    classMap = classMap,
                    templateMaps = new TemplateMaps()
                }
              );
            }
            else
            {
                ctm.classMap = classMap;
            }

            if (roleMap != null)
                roleMap.classMap = classMap;

           
        }

        public static string BuildClassPath(this GraphMap graphMap, ClassMap parentClassMap, TemplateMap templateMap, RoleMap roleMap)
        {

            string path = string.Format("{0}/{1}/{2}({3})/{4}", parentClassMap.path, parentClassMap.id, templateMap.id, templateMap.index, roleMap.id).Trim('/');
            return path;
        }

        public static void SetClassPath(this GraphMap graphMap)
        {
            if (graphMap != null && graphMap.classTemplateMaps != null && graphMap.classTemplateMaps[0] != null)
            {
                ClassTemplateMap ctm = graphMap.classTemplateMaps[0];
                ctm.classMap.path = ""; // class path for root class
                SetClassPath(graphMap, ctm.classMap);
            }
        }

        private static void SetClassPath(this GraphMap graphMap, ClassMap parentClassMap)
        {
            if (parentClassMap.path == null)
                parentClassMap.path = "";

            ClassTemplateMap ctm = graphMap.classTemplateMaps.SingleOrDefault(x => x.classMap.id == parentClassMap.id && x.classMap.index == parentClassMap.index);
            if (ctm != null)
            {
                ctm.classMap.path = parentClassMap.path;
                foreach (TemplateMap tm in ctm.templateMaps)
                {
                    foreach (RoleMap rm in tm.roleMaps.Where(x => x.classMap != null))
                    {
                        rm.classMap.path = string.Format("{0}/{1}/{2}({3})/{4}", ctm.classMap.path, ctm.classMap.id, tm.id, tm.index, rm.id).Trim('/');

                        SetClassPath(graphMap, rm.classMap);
                    }
                }
            }
        }


        public static void AddTemplateMap(this GraphMap graphMap, ClassMap classMap, TemplateMap templateMap)
        {
            graphMap.AddClassMap(null, classMap);
            ClassTemplateMap classTemplateMap = graphMap.classTemplateMaps.Where(c => (c.classMap.id == classMap.id && c.classMap.index == classMap.index)).FirstOrDefault();

            templateMap.index = classTemplateMap.templateMaps.Where(x => x.id == templateMap.id).Count();

            if (classTemplateMap.classMap != null)
                classTemplateMap.templateMaps.Add(templateMap);
        }

        public static void DeleteTemplateMap(this GraphMap graphMap, string parentClassId, int parentClassIndex, int templateMapIndex)
        {
            ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId, parentClassIndex);
            TemplateMap tm = ctm.templateMaps[templateMapIndex];

            foreach (RoleMap rm in tm.roleMaps)
            {
                DeleteRoleMap(graphMap, tm, rm.id);
            }

            ctm.templateMaps.RemoveAt(templateMapIndex);

            int newIndex = 0;
            foreach (TemplateMap tMap in ctm.templateMaps.Where(x => x.id == tm.id))
            {
                tMap.index = newIndex++;
            }

            graphMap.SetClassPath();

        }

        //public static void DeleteTemplateMap(this GraphMap graphMap, string classId, TemplateMap templateMap)
        //{
        //  ClassTemplateMap classTemplateMap = graphMap.GetClassTemplateMap(classId);

        //  if (classTemplateMap != null && classTemplateMap.templateMaps != null)
        //  {
        //    foreach (TemplateMap tplMap in classTemplateMap.templateMaps)
        //    {
        //      if (tplMap.id == templateMap.id)
        //      {
        //        bool templateMatched = true;

        //        // a template is matched when its id and all the roles are matched
        //        foreach (RoleMap roleMap in templateMap.roleMaps)
        //        {
        //          foreach (RoleMap rlMap in tplMap.roleMaps)
        //          {
        //            if (rlMap.id == roleMap.id && rlMap.value != rlMap.value)
        //            {
        //              templateMatched = false;
        //              break;
        //            }
        //          }

        //          if (!templateMatched)
        //            break;
        //        }

        //        if (templateMatched)
        //        {
        //          List<RoleMap> classRoles = templateMap.roleMaps.Where(c => c.classMap != null).ToList<RoleMap>();

        //          if (classRoles != null)
        //          {
        //            foreach (RoleMap classRole in classRoles)
        //            {
        //              graphMap.DeleteClassMap(classRole.classMap.id,classRole.classMap.index);
        //            }
        //          }

        //          classTemplateMap.templateMaps.Remove(templateMap);
        //          break;
        //        }
        //      }
        //    }
        //  }
        //}

        public static bool IsMapped(this RoleMap roleMap)
        {
            if (roleMap.type == RoleType.Unknown)
            {
                return false;
            }

            if ((roleMap.type == RoleType.Property ||
                roleMap.type == RoleType.DataProperty ||
                roleMap.type == RoleType.ObjectProperty) &&
                string.IsNullOrEmpty(roleMap.propertyName))
            {
                return false;
            }

            return true;
        }

        public static string ResolveValueMap(this Mapping mapping, string valueListName, string qualifiedUri)
        {
            if (!String.IsNullOrEmpty(qualifiedUri))
            {
                string uri = qualifiedUri.Replace(RDL_NS, "rdl:");

                if (mapping.valueListMaps != null)
                {
                    foreach (ValueListMap valueListMap in mapping.valueListMaps)
                    {
                        if (valueListMap.name.ToLower() == valueListName.ToLower())
                        {
                            foreach (ValueMap valueMap in valueListMap.valueMaps)
                            {
                                if (valueMap.uri == uri)
                                {
                                    return valueMap.internalValue;
                                }
                            }
                        }
                    }
                }
            }

            return String.Empty;
        }

        public static string ResolveValueList(this Mapping mapping, string valueListName, string value)
        {
            if (mapping.valueListMaps != null)
            {
                foreach (ValueListMap valueListMap in mapping.valueListMaps)
                {
                    if (valueListMap.name.ToLower() == valueListName.ToLower())
                    {
                        foreach (ValueMap valueMap in valueListMap.valueMaps)
                        {
                            if (valueMap.internalValue.ToLower() == value.ToLower())
                            {
                                return valueMap.uri;  // uri with prefix
                            }
                        }
                    }
                }
            }

            return RDF_NIL;
        }

        public static ClassMap Clone(this ClassMap classMap)
        {
            ClassMap newClassMap = new ClassMap
            {
                id = classMap.id,
                name = classMap.name,
                identifierKeyMaps = classMap.identifierKeyMaps,
                identifierDelimiter = classMap.identifierDelimiter,
                identifiers = new Identifiers(),
            };

            foreach (string identifier in classMap.identifiers)
            {
                newClassMap.identifiers.Add(identifier);
            }

            return newClassMap;
        }

        public static KeyMaps CloneIdentifiers(this org.iringtools.dxfr.manifest.Keys identifiers)
        {
            KeyMaps newIdentifierMaps = new KeyMaps();

            foreach (Key identifier in identifiers)
            {
                KeyMap newIdentifierMap = new KeyMap
                {
                    classId = identifier.classId,
                    templateId = identifier.templateId,
                    roleId = identifier.roleId
                };

                newIdentifierMaps.Add(newIdentifierMap);
            }

            return newIdentifierMaps;
        }

        public static ClassMap CrossClassMap(this ClassMap classMap, GraphMap graphMap, Class manifestClass)
        {
            ClassMap newClassMap = new ClassMap
            {
                id = classMap.id,
                name = classMap.name,
                identifierKeyMaps = manifestClass.keys.CloneIdentifiers(),
                identifierDelimiter = classMap.identifierDelimiter,
                identifiers = new Identifiers(),
                index = classMap.index,
                path = classMap.path
            };

            //TODO: Resolve identifiers based on the manifestIdentifiers.

            foreach (Key key in manifestClass.keys)
            {
                foreach (ClassTemplateMap classTemplateMap in graphMap.classTemplateMaps)
                {
                    if (classTemplateMap.classMap.id == key.classId && classTemplateMap.classMap.index == manifestClass.index)
                    {
                        foreach (TemplateMap templateMap in classTemplateMap.templateMaps)
                        {
                            if (templateMap.id == key.templateId)
                            {
                                foreach (RoleMap roleMap in templateMap.roleMaps)
                                {
                                    if (roleMap.id == key.roleId)
                                    {
                                        newClassMap.identifiers.Add(roleMap.propertyName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return newClassMap;
        }

        public static TemplateMap Clone(this TemplateMap templateMap)
        {
            TemplateMap clone = new TemplateMap
            {
                id = templateMap.id,
                name = templateMap.name,
                roleMaps = new RoleMaps(),
            };

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
                clone.roleMaps.Add(roleMap.Clone());
            }

            return clone;
        }

        public static RoleMap Clone(this RoleMap roleMap)
        {
            RoleMap clone = new RoleMap
            {
                type = roleMap.type,
                id = roleMap.id,
                name = roleMap.name,
                dataType = roleMap.dataType,
                propertyName = roleMap.propertyName,
                value = roleMap.value,
                valueListName = roleMap.valueListName,
                classMap = roleMap.classMap,
            };

            return clone;
        }

        public static Cardinality GetCardinality(this GraphMap graphMap, RoleMap roleMap, DataDictionary dataDictionary, string fixedIdentifierBoundary)
        {
            ClassTemplateMap ctm = graphMap.GetClassTemplateMap(roleMap.classMap.id, roleMap.classMap.index);
            if (ctm == null || ctm.classMap == null)
                return Cardinality.Self;

            // determine cardinality to related class
            foreach (string identifier in roleMap.classMap.identifiers)
            {
                if (!(identifier.StartsWith(fixedIdentifierBoundary) && identifier.EndsWith(fixedIdentifierBoundary)))
                {
                    string[] propertyParts = identifier.Split('.');

                    if (propertyParts.Length > 2)
                    {
                        DataObject dataObject = dataDictionary.dataObjects.First(c => c.objectName == propertyParts[0]);
                        DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relationshipName == propertyParts[1]);

                        if (dataRelationship.relationshipType == RelationshipType.OneToMany)
                        {
                            return Cardinality.OneToMany;
                        }
                    }
                }
            }

            return Cardinality.OneToOne;
        }
    }

    //  public void DeleteRoleMap(TemplateMap templateMap, string roleId)
    //  {
    //    RoleMap roleMap = templateMap.RoleMaps.Where(c => c.RoleId == roleId).FirstOrDefault();
    //    if (roleMap != null)
    //    {
    //      if (roleMap.classMap != null)
    //      {
    //        DeleteClassMap(roleMap.classMap.classId);
    //        roleMap.classMap = null;
    //      }
    //    }
    //  }

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "mapping")]
    //public class Mapping : RootBase
    //{
    //  private static readonly string RDL_NS = "http://rdl.rdlfacade.org/data#";
    //  private static readonly string RDF_NIL = "rdf:nil";

    //  public Mapping()
    //    : base()
    //  {
    //    GraphMaps = new List<GraphMap>();
    //    ValueListMaps = new List<ValueListMap>();
    //  }

    //    throw new Exception("Graph [" + graphName + "] does not exist.");
    //  }

    //  [DataMember(Name = "graphMaps", Order = 1, EmitDefaultValue = false)]
    //  public List<GraphMap> GraphMaps { get; set; }

    //  [DataMember(Name = "valueListMaps", EmitDefaultValue = false, Order = 2)]
    //  public List<ValueListMap> ValueListMaps { get; set; }

    //  public string ResolveValueList(string valueListName, string value)
    //  {
    //    foreach (ValueListMap valueListMap in ValueListMaps)
    //    {
    //      if (valueListMap.Name == valueListName)
    //      {
    //        foreach (ValueMap valueMap in valueListMap.ValueMaps)
    //        {
    //          if (valueMap.InternalValue == value)
    //          {
    //            return valueMap.Uri.Replace("rdl:", RDL_NS);
    //          }
    //        }
    //      }
    //    }

    //    return RDF_NIL;
    //  }

    //  public string ResolveValueMap(string valueListName, string qualifiedUri)
    //  {
    //    string uri = qualifiedUri.Replace(RDL_NS, "rdl:");

    //    foreach (ValueListMap valueListMap in ValueListMaps)
    //    {
    //      if (valueListMap.Name == valueListName)
    //      {
    //        foreach (ValueMap valueMap in valueListMap.ValueMaps)
    //        {
    //          if (valueMap.Uri == uri)
    //          {
    //            return valueMap.InternalValue;
    //          }
    //        }
    //      }
    //    }

    //    return String.Empty;
    //  }
    //}

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "graphMap")]
    //public class GraphMap : GraphBase
    //{
    //  public GraphMap()
    //  {
    //    ClassTemplateMaps = new List<ClassTemplateMap>();
    //  }

    //  [DataMember(Name = "classTemplateMaps", Order = 1, EmitDefaultValue = false)]
    //  public List<ClassTemplateMap> ClassTemplateMaps { get; set; }

    //  [DataMember(Name = "dataObjectName", EmitDefaultValue = false, Order = 2)]
    //  public string DataObjectName { get; set; }

    // 

    //  public void AddClassMap(RoleMap roleMap, ClassMap classMap)
    //  {
    //    ClassTemplateMap classTemplateListMap = GetClassTemplateMap(classMap.classId);
    //    if (classTemplateListMap == null)
    //      classTemplateListMap = new ClassTemplateMap();

    //    if (classTemplateListMap.classMap == null)
    //    {
    //      ClassTemplateMaps.Add(
    //        new ClassTemplateMap
    //        {
    //          ClassMap = classMap,
    //          TemplateMaps = new List<TemplateMap>()
    //        }
    //      );

    //      if (roleMap != null)
    //        roleMap.classMap = classMap;
    //    }
    //  }

    //  public void AddTemplateMap(ClassMap classMap, TemplateMap templateMap)
    //  {
    //    AddClassMap(null, classMap);
    //    ClassTemplateMap classTemplateMap = ClassTemplateMaps.Where(c => c.classMap.classId == classMap.classId).FirstOrDefault();
    //    if (classTemplateMap.classMap != null)
    //      classTemplateMap.TemplateMaps.Add(templateMap);
    //  }

    //  public void DeleteClassMap(string classId)
    //  {
    //    ClassTemplateMap classTemplateMap = GetClassTemplateMap(classId);

    //    if (classTemplateMap.classMap != null)
    //    {
    //      List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
    //      foreach (TemplateMap templateMap in templateMaps)
    //      {
    //        RoleMap classRole = templateMap.RoleMaps.Where(c => c.classMap != null).FirstOrDefault();
    //        if (classRole != null)
    //        {
    //          DeleteClassMap(classRole.classMap.classId);
    //          classRole.classMap = null;
    //        }
    //      }
    //      templateMaps.Clear();
    //      ClassTemplateMaps.Remove(classTemplateMap);
    //    }
    //  }

    //  public void DeleteTemplateMap(string classId, string templateId)
    //  {
    //    ClassTemplateMap classTemplateMap = GetClassTemplateMap(classId);
    //    if (classTemplateMap.classMap != null)
    //    {
    //      List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
    //      TemplateMap templateMap = classTemplateMap.TemplateMaps.Where(c => c.TemplateId == templateId).FirstOrDefault();
    //      RoleMap classRole = templateMap.RoleMaps.Where(c => c.classMap != null).FirstOrDefault();
    //      if (classRole != null)
    //        DeleteClassMap(classRole.classMap.classId);

    //      templateMaps.Remove(templateMap);
    //    }
    //  }

    //  public void DeleteRoleMap(TemplateMap templateMap, string roleId)
    //  {
    //    RoleMap roleMap = templateMap.RoleMaps.Where(c => c.RoleId == roleId).FirstOrDefault();
    //    if (roleMap != null)
    //    {
    //      if (roleMap.classMap != null)
    //      {
    //        DeleteClassMap(roleMap.classMap.classId);
    //        roleMap.classMap = null;
    //      }
    //    }
    //  }
    //}

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "classTemplateMap")]
    //public class ClassTemplateMap
    //{
    //  public ClassTemplateMap()
    //  {
    //    TemplateMaps = new List<TemplateMap>();
    //  }

    //  [DataMember(Name = "classMap", Order = 0, EmitDefaultValue = false)]
    //  public ClassMap ClassMap { get; set; }

    //  [DataMember(Name = "templateMaps", Order = 1, EmitDefaultValue = false)]
    //  public List<TemplateMap> TemplateMaps { get; set; }
    //}

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "classMap")]
    //public class ClassMap : ClassBase
    //{
    //  public ClassMap()
    //  {
    //    Identifiers = new List<string>();
    //  }

    //  public ClassMap(ClassMap classMap)
    //    : this()
    //  {
    //    ClassId = classMap.classId;
    //    Name = classMap.name;
    //    IdentifierDelimiter = String.Empty;

    //    foreach (string identifier in classMap.Identifiers)
    //    {
    //      Identifiers.Add(identifier);
    //    }
    //  }

    //  [DataMember(Name = "identifierDelimiter", EmitDefaultValue = false, Order = 2)]
    //  public string IdentifierDelimiter { get; set; }

    //  [DataMember(Name = "identifiers", EmitDefaultValue = false, Order = 3)]
    //  public List<string> Identifiers { get; set; }

    //  [DataMember(Name = "identifierValue", EmitDefaultValue = false, Order = 4)]
    //  public string IdentifierValue { get; set; }
    //}

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "templateMap")]
    //public class TemplateMap : TemplateBase
    //{
    //  public TemplateMap()
    //  {
    //    RoleMaps = new List<RoleMap>();
    //  }

    //  public TemplateMap(TemplateMap templateMap)
    //    : this()
    //  {
    //    TemplateId = templateMap.TemplateId;
    //    Name = templateMap.Name;

    //    foreach (RoleMap roleMap in templateMap.RoleMaps)
    //    {
    //      RoleMaps.Add(new RoleMap(roleMap));
    //    }
    //  }

    //  [DataMember(Name = "roleMaps", Order = 1, EmitDefaultValue = false)]
    //  public List<RoleMap> RoleMaps { get; set; }

    //  [DataMember(Name = "templateType", EmitDefaultValue = false, Order = 2)]
    //  public TemplateType TemplateType { get; set; }
    //}

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "roleMap")]
    //public class RoleMap : RoleBase
    //{
    //  public RoleMap() { }

    //  public RoleMap(RoleMap roleMap)
    //  {
    //    Type = roleMap.Type;
    //    RoleId = roleMap.RoleId;
    //    Name = roleMap.Name;
    //    DataType = roleMap.DataType;
    //    PropertyName = roleMap.PropertyName;
    //    Value = roleMap.Value;
    //    ValueListName = roleMap.ValueListName;
    //    ClassMap = roleMap.classMap;
    //  }

    //  [DataMember(Name = "propertyName", EmitDefaultValue = false, Order = 4)]
    //  public string PropertyName { get; set; }

    //  [DataMember(Name = "valueListName", EmitDefaultValue = false, Order = 6)]
    //  public string ValueListName { get; set; }

    //  [DataMember(Name = "classMap", EmitDefaultValue = false, Order = 7)]
    //  public ClassMap ClassMap { get; set; }

    //  public bool IsMapped
    //  {
    //    get
    //    {
    //      return ClassMap != null || !String.IsNullOrEmpty(PropertyName) || !String.IsNullOrEmpty(Value);
    //    }
    //  }
    //}

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "valueListMap")]
    //public class ValueListMap
    //{
    //  public ValueListMap()
    //  {
    //    ValueMaps = new List<ValueMap>();
    //  }

    //  [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
    //  public string Name { get; set; }

    //  [DataMember(Name = "valueMaps", EmitDefaultValue = false, Order = 1)]
    //  public List<ValueMap> ValueMaps { get; set; }
    //}

    //[DataContract(Namespace = "http://www.iringtools.org/common/mapping", Name = "valueMap")]
    //public class ValueMap
    //{
    //  [DataMember(Name = "internalValue", EmitDefaultValue = false, Order = 0)]
    //  public string InternalValue { get; set; }

    //  [DataMember(Name = "uri", EmitDefaultValue = false, Order = 1)]
    //  public string Uri { get; set; }
    //}
}
