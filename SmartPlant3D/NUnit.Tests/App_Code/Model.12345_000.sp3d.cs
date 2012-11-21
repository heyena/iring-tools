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
using System.Globalization;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer.proj_12345_000.sp3d
{
  public class Equipment : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String oid
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String DryWCGProperties { get; set; }
    public virtual String WetWCGProperties { get; set; }
    public virtual String UserSetWeightCG { get; set; }
    public virtual String Requirement { get; set; }
    public virtual String Type { get; set; }
    public virtual String Purpose { get; set; }
    public virtual String Material { get; set; }
    public virtual String IsInsulated { get; set; }
    public virtual String name { get; set; }
    public virtual String CPMachinerySystem_oid { get; set; }
    public virtual String CPMachinerySystem_name { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "DryWCGProperties": return DryWCGProperties;
        case "WetWCGProperties": return WetWCGProperties;
        case "UserSetWeightCG": return UserSetWeightCG;
        case "Requirement": return Requirement;
        case "Type": return Type;
        case "Purpose": return Purpose;
        case "Material": return Material;
        case "IsInsulated": return IsInsulated;
        case "oid": return oid;
        case "name": return name;
        case "CPMachinerySystem_oid": return CPMachinerySystem_oid;
        case "CPMachinerySystem_name": return CPMachinerySystem_name;
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
        case "DryWCGProperties":
          DryWCGProperties = Convert.ToString(value);
          break;
        case "WetWCGProperties":
          WetWCGProperties = Convert.ToString(value);
          break;
        case "UserSetWeightCG":
          UserSetWeightCG = Convert.ToString(value);
          break;
        case "Requirement":
          Requirement = Convert.ToString(value);
          break;
        case "Type":
          Type = Convert.ToString(value);
          break;
        case "Purpose":
          Purpose = Convert.ToString(value);
          break;
        case "Material":
          Material = Convert.ToString(value);
          break;
        case "IsInsulated":
          IsInsulated = Convert.ToString(value);
          break;
        case "oid":
          oid = Convert.ToString(value);
          break;
        case "name":
          name = Convert.ToString(value);
          break;
        case "CPMachinerySystem_oid":
          CPMachinerySystem_oid = Convert.ToString(value);
          break;
        case "CPMachinerySystem_name":
          CPMachinerySystem_name = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
  }
}
