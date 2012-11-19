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
using System.Collections.Generic;
using org.iringtools.library;
using System.Linq;
using org.iringtools.utility;
using Ingr.SP3D.Common.Middle.Services;
using Ingr.SP3D.Common.Middle;

namespace iringtools.sdk.sp3ddatalayer
{
  [DataContract(Name = "businessObjectConfiguration", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessObjectConfiguration
  {
    public BusinessObjectConfiguration()
    {
      businessCommodities = new List<BusinessCommodity>();      
    }

    [DataMember(Order = 0)]
    public List<BusinessCommodity> businessCommodities { get; set; }    

    [DataMember(Name = "provider", IsRequired = true, Order = 1)]
    public string Provider { get; set; }

    [DataMember(Name = "connectionString", IsRequired = true, Order = 2)]
    public string ConnectionString { get; set; }    

    [DataMember(Name = "schemaName", IsRequired = true, Order = 3)]
    public string SchemaName { get; set; }

    [DataMember(Name = "stagingDataBaseName", IsRequired = true, Order = 4)]
    public string StagingDataBaseName { get; set; }

    [DataMember(Name = "siteDataBaseName", IsRequired = true, Order = 5)]
    public string SiteDataBaseName { get; set; }

    [DataMember(Name = "plantName", IsRequired = false, Order = 6, EmitDefaultValue = false)]
    public string PlantName { get; set; }     

    public BusinessCommodity GetBusinessCommoditiy(string name)
    {
      BusinessCommodity businessCommoditiy = null;
      businessCommoditiy = this.businessCommodities.FirstOrDefault<BusinessCommodity>(o => o.commodityName.ToLower() == name.ToLower());
      return businessCommoditiy;
    }
  }

  [DataContract(Name = "businessFilter", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessFilter
  {
    public BusinessFilter()
    {      
    }

    [DataMember(Order = 0)]
    public string filterName { get; set; }    
  }

  [DataContract(Name = "businessCommodity", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessCommodity
  {
    public BusinessCommodity()
    {
      businessObjects = new List<BusinessObject>();
      relatedObjects = new List<RelatedObject>();
    }

    [DataMember(Order = 0)]
    public string commodityName { get; set; }

    [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
    public string objectNamespace { get; set; }

    [DataMember(Order = 2)]
    public List<BusinessObject> businessObjects { get; set; }

    [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
    public List<RelatedObject> relatedObjects { get; set; }

    [DataMember(IsRequired = false, Order = 4, EmitDefaultValue = false)]
    public DataFilter dataFilter { get; set; }

    [DataMember(IsRequired = false, Order = 5, EmitDefaultValue = false)]
    public bool soleBusinessObject { get; set; }

    [DataMember(IsRequired = false, Order = 6, EmitDefaultValue = false)]
    public BusinessFilter businessFilter { get; set; }

    [DataMember(IsRequired = false, Order = 7, EmitDefaultValue = false)]
    public List<DataRelationship> dataRelationships { get; set; }

    public BusinessObject GetBusinessObject(string name)
    {
      BusinessObject BusinessObject = null;
      BusinessObject = this.businessObjects.FirstOrDefault<BusinessObject>(o => o.objectName.ToLower() == name.ToLower());
      return BusinessObject;
    }

    public BusinessObject GetBusinessObject(string objectName, string tableName)
    {
      BusinessObject BusinessObject = null;
      BusinessObject = this.businessObjects.FirstOrDefault<BusinessObject>(o => o.objectName.ToLower() == objectName.ToLower() && o.tableName.ToLower() == tableName.ToLower());
      return BusinessObject;
    }

    public bool hasMinusOrZeroRowNumbers()
    {
      foreach (BusinessObject businessObject in this.businessObjects)
      {
        if (businessObject.rowNumber <= 0)
          return true;
      }
      return false;
    }
  }

  [DataContract(Name = "businessInterface", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessInterface
  {
    public BusinessInterface()
    {
      businessProperties = new List<BusinessProperty>();
    }

    [DataMember(IsRequired = true, Order = 0)]
    public string interfaceName { get; set; }

    [DataMember(IsRequired = false, Order = 2, EmitDefaultValue = false)]
    public string tableName { get; set; }

    [DataMember(IsRequired = true, Order = 3)]
    public List<BusinessProperty> businessProperties { get; set; }

    public bool deleteProperty(BusinessProperty businessProperty)
    {
      foreach (BusinessProperty property in businessProperties)
      {
        if (businessProperty == property)
        {
          businessProperties.Remove(businessProperty);
          break;
        }
      }
      return true;
    }
  }

  [DataContract(Name = "businessObject", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessObject : RootBusinessObject
  {
    public BusinessObject()
    {
      nodeType = NodeType.StartObject;
    }
   
    public BusinessRelation GetRelation(string relationName)
    {
      BusinessRelation relation = null;
      relation = this.relations.FirstOrDefault<BusinessRelation>(o => o.relationName.ToLower() == relationName.ToLower());
      return relation;
    }

    public RelatedObject GetRelatedObject(string objectName)
    {
      RelatedObject relatedObject = null;
      relatedObject = this.relatedObjects.FirstOrDefault<RelatedObject>(o => o.objectName.ToLower() == objectName.ToLower());
      return relatedObject;
    }

    public bool setUniqueRelation(RelatedObject rObj)
    {
      int suffix = 0;
      bool unique = true;

      if (GetRelation(rObj.relationName) != null)
      {
        rObj.relationName += "_";
        unique = false;

        do
        {
          rObj.relationName = rObj.relationName.Substring(0, rObj.relationName.LastIndexOf('_'));
          rObj.relationName += "_" + suffix;
          suffix++;
        }
        while (GetRelation(rObj.relationName) != null);
      }
      return unique;
    }

    public BusinessRelation addUniqueRelation(RelatedObject rObj, RootBusinessObject parentNode)
    {
      BusinessRelation relation = null;
      string originRelationName = rObj.relationName;
      bool unique = setUniqueRelation(rObj);
      relation = rObj.createBusinessRelation(originRelationName);

      if (!unique)
        relation.unique = unique;

      relations.Add(relation);
        
      if (parentNode.nodeType == NodeType.StartObject)
      {
        rightClassNames.Add(rObj.relationName);
      }

      return relation;
    }

    public void addUniqueRelatedObject(RelatedObject rObj)
    {
      int suffix = 0;

      if (relatedObjects.Contains(rObj))
      {
        rObj.objectName += "_";

        do
        {
          rObj.objectName = rObj.objectName.Substring(0, rObj.objectName.LastIndexOf('_'));
          rObj.objectName += "_" + suffix;
          suffix++;
        }
        while (relatedObjects.Contains(rObj));
      }

      relatedObjects.Add(rObj);
    }
  }  

  [DataContract(Name = "rootBusinessObject", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class RootBusinessObject
  {
    public RootBusinessObject()
    {
      businessKeyProperties = new List<BusinessKeyProperty>();
      businessInterfaces = new List<BusinessInterface>();
      businessProperties = new List<BusinessProperty>();
      relatedObjects = new List<RelatedObject>();
      relations = new List<BusinessRelation>();
      leftClassNames = new List<string>();
      rightClassNames = new List<string>();
    }

    [DataMember(IsRequired = false, Order = 0, EmitDefaultValue = false)]
    public string objectName { get; set; }

    [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
    public string objectNamespace { get; set; }

    [DataMember(IsRequired = false, Order = 2, EmitDefaultValue = false)]
    public string tableName { get; set; }

    [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
    public string relationName { get; set; }

    [DataMember(IsRequired = false, Order = 4, EmitDefaultValue = false)]
    public string relationTableName { get; set; }

    [DataMember(IsRequired = false, Order = 5, EmitDefaultValue = false)]
    public string startObjectName { get; set; }

    [DataMember(IsRequired = false, Order = 6, EmitDefaultValue = false)]
    public NodeType nodeType { get; set; }

    [DataMember(IsRequired = false, Order = 7, EmitDefaultValue = false)]
    public List<BusinessInterface> businessInterfaces { get; set; }

    [DataMember(IsRequired = false, Order = 8, EmitDefaultValue = false)]
    public List<RelatedObject> relatedObjects { get; set; }

    [DataMember(IsRequired = false, Order = 9, EmitDefaultValue = false)]
    public List<BusinessRelation> relations { get; set; }

    [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
    public List<string> leftClassNames { get; set; }

    [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
    public List<string> rightClassNames { get; set; }

    [DataMember(IsRequired = false, Order = 11, EmitDefaultValue = false)]
    public List<BusinessKeyProperty> businessKeyProperties { get; set; }

    [DataMember(IsRequired = false, Order = 12, EmitDefaultValue = false)]
    public List<BusinessProperty> businessProperties { get; set; }    

    [DataMember(IsRequired = false, Order = 13, EmitDefaultValue = false)]
    public bool isReadOnly { get; set; }

    [DataMember(IsRequired = false, Order = 14, EmitDefaultValue = false)]
    public string description { get; set; }

    [DataMember(IsRequired = false, Order = 15, EmitDefaultValue = false)]
    public long rowNumber { get; set; }

    public bool isKeyProperty(string propertyName)
    {
      foreach (BusinessKeyProperty keyProperty in businessKeyProperties)
      {
        if (keyProperty.keyPropertyName.ToLower() == propertyName.ToLower())
          return true;
      }

      return false;
    }

    public BusinessKeyProperty getKeyProperty(string keyPropertyName)
    {
      BusinessKeyProperty keyProperty = null;
      keyProperty = this.businessKeyProperties.FirstOrDefault<BusinessKeyProperty>(o => o.keyPropertyName.ToLower() == keyPropertyName.ToLower());
      return keyProperty;
    }

    public BusinessProperty convertKeyPropertyToProperty(BusinessKeyProperty businessKeyProperty)
    {
      BusinessProperty businessProperty = new BusinessProperty();
      businessProperty.datatype = businessKeyProperty.datatype;
      businessProperty.dataType = businessKeyProperty.dataType;
      businessProperty.columnName = businessKeyProperty.columnName;
      businessProperty.description = businessKeyProperty.description;
      businessProperty.propertyName = businessKeyProperty.keyPropertyName;
      businessProperty.isNullable = businessKeyProperty.isNullable;
      businessProperty.isReadOnly = businessKeyProperty.isReadOnly;
      businessProperty.keyType = KeyType.assigned;
      businessProperty.hidden = businessKeyProperty.hidden;
      return businessProperty;
    }

    public BusinessProperty convertDataPropertyToProperyt(DataProperty dataProperty)
    {
      BusinessProperty businessProperty = new BusinessProperty();
      businessProperty.datatype = dataProperty.dataType.ToString();
      businessProperty.dataType = dataProperty.dataType;
      businessProperty.columnName = dataProperty.columnName;
      businessProperty.description = dataProperty.description;
      businessProperty.propertyName = dataProperty.propertyName;
      businessProperty.isNullable = dataProperty.isNullable;
      businessProperty.isReadOnly = dataProperty.isReadOnly;
      businessProperty.keyType = dataProperty.keyType;
      businessProperty.hidden = dataProperty.isHidden;
      return businessProperty;
    }

    public bool containProperty(BusinessProperty dataProperty)
    {
      if (businessInterfaces != null)
      {
        foreach (BusinessInterface businessInterface in businessInterfaces)
        {
          foreach (BusinessProperty property in businessInterface.businessProperties)
          {
            if (dataProperty.propertyName.ToLower() == property.propertyName.ToLower())
            {
              return true;
            }
          }
        }

        foreach (BusinessKeyProperty keyProperty in businessKeyProperties)
        {
          if (keyProperty.keyPropertyName.ToLower() == dataProperty.propertyName.ToLower())
          {
            return true;
          }
        }
      }
      return false;
    }

    public BusinessProperty getProperty(string dataProperty)
    {
      if (businessInterfaces != null)
      {
        foreach (BusinessInterface businessInterface in businessInterfaces)
        {
          foreach (BusinessProperty property in businessInterface.businessProperties)
          {
            if (dataProperty.ToLower() == property.propertyName.ToLower())
            {
              return property;
            }
          }
        }
      }

      return null;
    }

    public bool containProperty(string dataProperty)
    {
      if (businessInterfaces != null)
      {
        foreach (BusinessInterface businessInterface in businessInterfaces)
        {
          foreach (BusinessProperty property in businessInterface.businessProperties)
          {
            if (dataProperty.ToLower() == property.propertyName.ToLower())
            {
              return true;
            }
          }
        }

        foreach (BusinessKeyProperty keyProperty in businessKeyProperties)
        {
          if (keyProperty.keyPropertyName.ToLower() == dataProperty.ToLower())
          {
            return true;
          }
        }
      }
      return false;
    }

    public bool deleteProperty(BusinessProperty dataProperty)
    {
      if (businessInterfaces != null)
      {
        foreach (BusinessInterface businessInterface in businessInterfaces)
        {
          foreach (BusinessProperty property in businessInterface.businessProperties)
          {
            if (dataProperty == property)
            {
              businessInterface.businessProperties.Remove(dataProperty);
              break;
            }
          }
        }

        foreach (BusinessKeyProperty keyProperty in businessKeyProperties)
        {
          if (keyProperty.keyPropertyName.ToLower() == dataProperty.propertyName.ToLower())
          {
            businessKeyProperties.Remove(keyProperty);
            break;
          }
        }
      }
      return true;
    }

    public bool addKeyProperty(BusinessProperty keyProperty, string interfaceName)
    {
      foreach (BusinessInterface businessInterface in businessInterfaces)
      {
        if (businessInterface.interfaceName.ToLower() == interfaceName.ToLower())
        {
          businessInterface.businessProperties.Add(keyProperty);
        }
        return false;
      }

      this.businessKeyProperties.Add(new BusinessKeyProperty { keyPropertyName = keyProperty.propertyName });
      return true;
    }    
  }

  [DataContract(Name = "businessDataType", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessDataType
  {
    public DataType GetDatatype(string datatype)
    {
      if (datatype == null)
        return DataType.String;

      switch (datatype.ToLower())
      {
        case "string":
          return DataType.String;
        case "bool":
        case "boolean":
          return DataType.Boolean;
        case "float":
        case "decimal":
        case "double":
          return DataType.Double;
        case "integer":
        case "int":
        case "number":
          return DataType.Int64;
        default:
          return DataType.String;
      }
    }
  }

  [DataContract(Name = "businessKeyProperty", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessKeyProperty : BusinessDataType
  {
    [DataMember(IsRequired = true, Order = 0)]
    public string keyPropertyName { get; set; }

    [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
    public string datatype { get; set; }

    [DataMember(IsRequired = false, Order = 2, EmitDefaultValue = false)]
    public bool isNullable { get; set; }

    [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
    public bool isReadOnly { get; set; }

    [DataMember(IsRequired = false, Order = 4, EmitDefaultValue = false)]
    public string description { get; set; }

    [DataMember(IsRequired = false, Order = 5, EmitDefaultValue = false)]
    public string columnName { get; set; }

    [DataMember(IsRequired = false, Order = 6, EmitDefaultValue = false)]
    public KeyType keyType { get; set; }

    [DataMember(IsRequired = false, Order = 7, EmitDefaultValue = false)]
    public DataType dataType { get; set; }

    [DataMember(IsRequired = false, Order = 8, EmitDefaultValue = false)]
    public bool returnNull { get; set; }

    [DataMember(IsRequired = false, Order = 9, EmitDefaultValue = false)]
    public bool hidden { get; set; }

    public BusinessProperty convertKeyPropertyToProperty()
    {
      BusinessProperty businessProperty = new BusinessProperty();
      businessProperty.datatype = datatype;
      businessProperty.dataType = dataType;
      businessProperty.columnName = columnName;
      businessProperty.description = description;
      businessProperty.propertyName = keyPropertyName;
      businessProperty.isNullable = isNullable;
      businessProperty.isReadOnly = isReadOnly;
      businessProperty.keyType = KeyType.assigned;
      businessProperty.hidden = hidden;
      businessProperty.isKey = true;
      return businessProperty;
    }

    public DataProperty convertKeyPropertyToDataProperty()
    {
      DataProperty Property = new DataProperty();
      Property.dataType = GetDatatype(datatype);
      Property.dataType = dataType;
      Property.columnName = columnName;
      Property.description = description;
      Property.propertyName = keyPropertyName;
      Property.isNullable = isNullable;
      Property.isReadOnly = isReadOnly;
      Property.keyType = KeyType.assigned;
      Property.isHidden = hidden;
      return Property;
    }
  }

  [DataContract(Name = "businessProperty", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessProperty : BusinessDataType
  {
    public BusinessProperty()
    {
      codeList = null;
      isNative = true;
    }

    [DataMember(IsRequired = true, Order = 0)]
    public string propertyName { get; set; }

    [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
    public string datatype { get; set; }

    [DataMember(IsRequired = false, Order = 2, EmitDefaultValue = false)]
    public bool isNullable { get; set; }

    [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
    public bool isReadOnly { get; set; }

    [DataMember(IsRequired = false, Order = 4, EmitDefaultValue = false)]
    public string description { get; set; }

    [DataMember(IsRequired = false, Order = 5, EmitDefaultValue = false)]
    public string columnName { get; set; }

    [DataMember(IsRequired = false, Order = 6, EmitDefaultValue = false)]
    public KeyType keyType { get; set; }

    [DataMember(IsRequired = false, Order = 7, EmitDefaultValue = false)]
    public DataType dataType { get; set; }

    [DataMember(IsRequired = false, Order = 8, EmitDefaultValue = false)]
    public bool returnNull { get; set; }

    [DataMember(IsRequired = false, Order = 9, EmitDefaultValue = false)]
    public List<CodelistItem> codeList { get; set; }

    [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
    public bool isNative { get; set; }

    [DataMember(IsRequired = false, Order = 11, EmitDefaultValue = false)]
    public bool hidden { get; set; }

    [DataMember(IsRequired = false, Order = 12, EmitDefaultValue = false)]
    public bool isKey { get; set; }

    public DataProperty convertPropertyToDataProperty()
    {
      DataProperty Property = new DataProperty();
      Property.dataType = dataType;
      Property.columnName = columnName;
      Property.description = description;
      Property.propertyName = propertyName;
      Property.isNullable = isNullable;
      Property.isReadOnly = isReadOnly;
      Property.isHidden = hidden;
      return Property;
    }

    public BusinessProperty copyBusinessProperty()
    {
      BusinessProperty Property = new BusinessProperty();
      Property.dataType = dataType;
      Property.columnName = columnName;
      Property.description = description;
      Property.propertyName = propertyName;
      Property.isNullable = isNullable;
      Property.isReadOnly = isReadOnly;
      Property.hidden = hidden;
      return Property;
    }
  }

  [DataContract(Name = "relatedObject", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class RelatedObject : RootBusinessObject
  {
    public RelatedObject()
    {
      nodeType = NodeType.MiddleObject;      
    }    

    public DataObject convertRelatedObjectToDataObject()
    {
      DataObject dataObject = new DataObject();
      dataObject.objectNamespace = objectNamespace;
      dataObject.objectName = objectName;
      dataObject.tableName = tableName;

      if (businessKeyProperties != null)
        if (businessKeyProperties.Count > 0)
          foreach (BusinessKeyProperty bKey in businessKeyProperties)
            dataObject.addKeyProperty(bKey.convertKeyPropertyToDataProperty());

      if (businessInterfaces != null)
        if (businessInterfaces.Count > 0)
          foreach (BusinessInterface bint in businessInterfaces)
            if (bint.businessProperties != null)
              if (bint.businessProperties.Count > 0)
                foreach (BusinessProperty bprop in bint.businessProperties)
                  dataObject.dataProperties.Add(bprop.convertPropertyToDataProperty());

      return dataObject;
    }

    public BusinessRelation createBusinessRelation(string originRelationName)
    {
      BusinessRelation relation = new BusinessRelation();
      relation.relationName = relationName;
      relation.objectNamespace = objectNamespace;
      relation.leftClassNames = new List<string>();
      relation.rightClassNames = new List<string>();
      relation.rightClassNames.Add(objectName);
      relation.relationTableName = relationTableName;
      relation.nodeType = NodeType.Relation;
      relation.originRelationName = originRelationName;
      return relation;
    }    
  }

  [DataContract(Name = "businessRelation", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessRelation : RelatedObject
  {
    public BusinessRelation()
    {
      nodeType = NodeType.Relation;
      unique = true;
    }

    [DataMember(IsRequired = false, Order = 0, EmitDefaultValue = false)]
    public bool unique { get; set; }

    [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
    public string originRelationName { get; set; }
    
    public DataObject convertRelationToDataObject()
    {
      DataObject dataObject = new DataObject();
      dataObject.objectNamespace = objectNamespace;
      dataObject.objectName = relationName;
      dataObject.tableName = tableName;

      foreach (BusinessKeyProperty bKey in businessKeyProperties)
        dataObject.addKeyProperty(bKey.convertKeyPropertyToDataProperty());

      if (businessProperties != null)
        foreach (BusinessProperty bprop in businessProperties)
          dataObject.dataProperties.Add(bprop.convertPropertyToDataProperty());

      return dataObject;
    }

    public void createRelationBusinessProperty(string name)
    {
      BusinessProperty property = new BusinessProperty();
      property.datatype = "String";
      property.dataType = DataType.String;
      property.columnName = name;
      property.description = "";
      property.propertyName = name;
      property.isNullable = true;
      property.isReadOnly = false;
      property.keyType = KeyType.assigned;

      if (businessProperties == null)
      {
        businessProperties = new List<BusinessProperty>();
        businessProperties.Add(property);
      }
      else if (!businessProperties.Contains(property))
        this.businessProperties.Add(property);
    }
  }

  [DataContract(Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public enum NodeType
  {
    [EnumMember]
    StartObject = 0,
    [EnumMember]
    Relation = 1,
    [EnumMember]
    EndObject = 2,
    [EnumMember]
    MiddleObject = 3    
  }
}
