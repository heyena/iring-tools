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
using org.iringtools.library;

namespace org.iringtools.adapter
{
  [CollectionDataContract(Namespace = "http://iringtools.org/adapter/library/dto", Name = "dataTransferObjects")]
  public class DataTransferObjects : List<DataTransferObject> {}

  [DataContract(Namespace = "http://iringtools.org/adapter/library/dto", Name = "dataTransferObject")]
  public class DataTransferObject
  {
    public DataTransferObject()
    {
      classObjects = new List<ClassObject>();
    }

    [DataMember(Order = 0)]
    public List<ClassObject> classObjects { get; set; }

    [DataMember(Order = 1, EmitDefaultValue = false)]
    public TransferType transferType { get; set; }

    public ClassObject GetClassObject(string classId)
    {
      foreach (ClassObject classObject in classObjects)
      {
        if (classObject.classId == classId)
        {
          return classObject;
        }
      }

      return null;
    }
  }

  [DataContract(Namespace = "http://iringtools.org/adapter/library/dto", Name = "classObject")]
  public class ClassObject
  {
    public ClassObject()
    {
      templateObjects = new List<TemplateObject>();
    }

    [DataMember(Order = 0)]
    public string classId { get; set; }

    [DataMember(Order = 1)]
    public string name { get; set; }

    [DataMember(Order = 2)]
    public string identifier { get; set; }

    [DataMember(Order = 3)]
    public List<TemplateObject> templateObjects { get; set; }

    [DataMember(Order = 4, EmitDefaultValue = false)]
    public TransferType transferType { get; set; }

    public TemplateObject GetTemplateObject(TemplateMap templateMap)
    {
      foreach (TemplateObject templateObject in templateObjects)
      {
        if (templateObject.templateId == templateMap.templateId)
        {
          string roleMapRef = String.Empty;

          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            if (roleMap.type == RoleType.Reference)
            {
              roleMapRef = roleMap.value;
            }
          }

          if (roleMapRef != String.Empty)
          {
            foreach (RoleObject roleObject in templateObject.roleObjects)
            {
              if (!String.IsNullOrEmpty(roleObject.reference) && roleObject.reference == roleMapRef)
                return templateObject;
            }
          }
        }
      }

      return null;
    }
  }

  [DataContract(Namespace = "http://iringtools.org/adapter/library/dto", Name = "templateObject")]
  public class TemplateObject
  {
    public TemplateObject()
    {
      roleObjects = new List<RoleObject>();
    }

    [DataMember(Order = 0)]
    public string templateId { get; set; }

    [DataMember(Order = 1)]
    public string name { get; set; }

    [DataMember(Order = 2)]
    public List<RoleObject> roleObjects { get; set; }

    [DataMember(Order = 3, EmitDefaultValue = false)]
    public TransferType transferType { get; set; }
  }

  [DataContract(Namespace = "http://iringtools.org/adapter/library/dto", Name = "roleObject")]
  public class RoleObject
  {
    [DataMember(Order = 0)]
    public RoleType type { get; set; }

    [DataMember(Order = 1)]
    public string roleId { get; set; }

    [DataMember(Order = 2)]
    public string name { get; set; }

    [DataMember(Order = 3, EmitDefaultValue = false)]
    public string value { get; set; }

    [DataMember(Order = 4, EmitDefaultValue = false)]
    public string reference { get; set; }
  }

  [DataContract]
  public enum TransferType
  {
    [EnumMember]
    Sync,
    [EnumMember]
    Add,
    [EnumMember]
    Change,
    [EnumMember]
    Delete,
  }

  [CollectionDataContract(Namespace = "http://iringtools.org/adapter/library/dto", Name = "dataTransferIndices", ItemName = "dataTransferIndex", KeyName = "identifier", ValueName = "hashValue")]
  public class DataTransferIndices : Dictionary<string, string> {}

  [DataContract]
  public enum HashAlgorithm
  {
    [EnumMember]    
    MD5,
    [EnumMember]
    SHA1,
  }
}
