﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace Hatch.iPasXLDataLayer.API.Model_12345_000_API
{
  public class Equipment : IDataObject
  {
    public string Tag { get; set; }
    public string Description { get; set; }
    public string PumpType { get; set; }
    public string PumpDriverType { get; set; }
    public Double DesignTemp { get; set; }
    public Double DesignPressure { get; set; }
    public Double Capacity { get; set; }
    public Double SpecificGravity { get; set; }
    public Double DifferentialPressure { get; set; }

    public object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Tag":
          return Tag;

        case "Description":
          return Description;

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

        case "Description":
          Description = Convert.ToString(value);
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

    public IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      throw new NotImplementedException();
    }
  }
}
