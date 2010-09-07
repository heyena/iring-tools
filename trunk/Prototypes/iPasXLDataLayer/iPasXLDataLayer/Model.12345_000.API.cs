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
using org.iringtools.library;

namespace Hatch.iPasXLDataLayer.Model_12345_000.API
{
  public class Equipment : IDataObject
  {
    public virtual String Tag { get; set; }
    public virtual String Description { get; set; }
    public virtual String PumpType { get; set; }
    public virtual String PumpDriverType { get; set; }
    public virtual String DesignTemp { get; set; }
    public virtual String DesignPressure { get; set; }
    public virtual String Capacity { get; set; }
    public virtual String SpecificGravity { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Tag": return Tag;
        case "Description": return Description;
        case "PumpType": return PumpType;
        case "PumpDriverType": return PumpDriverType;
        case "DesignTemp": return DesignTemp;
        case "DesignPressure": return DesignPressure;
        case "Capacity": return Capacity;
        case "SpecificGravity": return SpecificGravity;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {case "Tag":
          if (value != null) Tag = Convert.ToString(value);
          break;
        case "Description":
          if (value != null) Description = Convert.ToString(value);
          break;
        case "PumpType":
          if (value != null) PumpType = Convert.ToString(value);
          break;
        case "PumpDriverType":
          if (value != null) PumpDriverType = Convert.ToString(value);
          break;
        case "DesignTemp":
          if (value != null) DesignTemp = Convert.ToString(value);
          break;
        case "DesignPressure":
          if (value != null) DesignPressure = Convert.ToString(value);
          break;
        case "Capacity":
          if (value != null) Capacity = Convert.ToString(value);
          break;
        case "SpecificGravity":
          if (value != null) SpecificGravity = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
        }
      }
    }
  }
