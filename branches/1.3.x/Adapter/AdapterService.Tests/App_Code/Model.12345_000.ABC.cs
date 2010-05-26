//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer.proj_12345_000.ABC
{
  public class InLinePipingComponent : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String tag
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String componentType { get; set; }
    public virtual Single diameter { get; set; }
    public virtual Boolean isCloned { get; set; }
    public virtual String lineTag { get; set; }
    public virtual String pid { get; set; }
    public virtual String projectNumber { get; set; }
    public virtual Int32 quantity { get; set; }
    public virtual String rating { get; set; }
    public virtual String system { get; set; }
    public virtual String unit { get; set; }
    public virtual String uomDiameter { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "tag": return tag;
        case "componentType": return componentType;
        case "diameter": return diameter;
        case "isCloned": return isCloned;
        case "lineTag": return lineTag;
        case "pid": return pid;
        case "projectNumber": return projectNumber;
        case "quantity": return quantity;
        case "rating": return rating;
        case "system": return system;
        case "unit": return unit;
        case "uomDiameter": return uomDiameter;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "Id":
          Id = Convert.ToString(value);
          break;
        case "tag":
          if (value != null) tag = Convert.ToString(value);
          break;
        case "componentType":
          if (value != null) componentType = Convert.ToString(value);
          break;
        case "diameter":
          if (value != null) diameter = Convert.ToSingle(value);
          break;
        case "isCloned":
          if (value != null) isCloned = Convert.ToBoolean(value);
          break;
        case "lineTag":
          if (value != null) lineTag = Convert.ToString(value);
          break;
        case "pid":
          if (value != null) pid = Convert.ToString(value);
          break;
        case "projectNumber":
          if (value != null) projectNumber = Convert.ToString(value);
          break;
        case "quantity":
          if (value != null) quantity = Convert.ToInt32(value);
          break;
        case "rating":
          if (value != null) rating = Convert.ToString(value);
          break;
        case "system":
          if (value != null) system = Convert.ToString(value);
          break;
        case "unit":
          if (value != null) unit = Convert.ToString(value);
          break;
        case "uomDiameter":
          if (value != null) uomDiameter = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      throw new NotImplementedException();
    }
  }
  
  public class Line : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String tag
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual Single diameter { get; set; }
    public virtual String system { get; set; }
    public virtual String uomDiameter { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "tag": return tag;
        case "diameter": return diameter;
        case "system": return system;
        case "uomDiameter": return uomDiameter;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "Id":
          Id = Convert.ToString(value);
          break;
        case "tag":
          if (value != null) tag = Convert.ToString(value);
          break;
        case "diameter":
          if (value != null) diameter = Convert.ToSingle(value);
          break;
        case "system":
          if (value != null) system = Convert.ToString(value);
          break;
        case "uomDiameter":
          if (value != null) uomDiameter = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      throw new NotImplementedException();
    }
  }
}
