using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml.Serialization;

namespace org.iringtools.adapter.datalayer.proj_12345_000.API
{
  public class Equipment
  {
    public string Tag { get; set; }
    public string PumpType { get; set; }
    public string PumpDriverType { get; set; }
    public Double DesignTemp { get; set; }
    public Double DesignPressure { get; set; }
    public Double Capacity { get; set; }
    public Double SpecificGravity { get; set; }
    public Double DifferentialPressure { get; set; }
  }

  public class EquipmentDataObject : Equipment, IDataObject
  {
    public object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Tag":
          return Tag;

        case "PumpType":
          return PumpType;

        case "PumpDriverType":
          return PumpDriverType;

        case "DesignTemp":
          return DesignTemp;

        case "DesignPressure":
          return DesignPressure;

        case "Capacity":
          return Capacity;

        case "SpecificGravity":
          return SpecificGravity;

        case "DifferentialPressure":
          return DifferentialPressure;

        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "Tag":
          Tag = Convert.ToString(value);
          break;

        case "PumpType":
          PumpType = Convert.ToString(value);
          break;

        case "PumpDriverType":
          PumpDriverType = Convert.ToString(value);
          break;

        case "DesignTemp":
          DesignTemp = Convert.ToDouble(value);
          break;

        case "DesignPressure":
          DesignPressure = Convert.ToDouble(value);
          break;

        case "Capacity":
          Capacity = Convert.ToDouble(value);
          break;

        case "SpecificGravity":
          SpecificGravity = Convert.ToDouble(value);
          break;

        case "DifferentialPressure":
          DifferentialPressure = Convert.ToDouble(value);
          break;

        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
  }
}
