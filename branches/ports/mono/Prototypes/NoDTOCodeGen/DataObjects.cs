using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdapterPrototype
{
  public interface IDataObject
  {
    object GetPropertyValue(string propertyName);
    void SetPropertyValue(string propertyName, object value);
  }

  public class Valve : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String tag
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String componentType { get; set; }
    public virtual Single? diameter { get; set; }
    public virtual Boolean? isCloned { get; set; }
    public virtual String lineTag { get; set; }
    public virtual String pid { get; set; }
    public virtual String projectNumber { get; set; }
    public virtual Int32? quantity { get; set; }
    public virtual String rating { get; set; }
    public virtual String system { get; set; }
    public virtual String unit { get; set; }
    public virtual String uomDiameter { get; set; }

    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "tag":
          return tag;
        case "componentType":
          return componentType;
        case "diameter":
          return diameter;
        case "isCloned":
          return isCloned;
        case "lineTag":
          return lineTag;
        case "pid":
          return pid;
        case "projectNumber":
          return projectNumber;
        case "quantity":
          return quantity;
        case "rating":
          return rating;
        case "system":
          return system;
        case "unit":
          return unit;
        case "uomDiameter":
          return uomDiameter;
        default:
          throw new Exception("property [" + propertyName + "] not found.");
      }
    }

    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "tag":
          tag = (String)value;
          break;
        case "componentType":
          componentType = (String)value;
          break;
        case "diameter":  // handle nullable type
          if (value != null)
            diameter = Convert.ToSingle(value);
          else
            diameter = null;
          break;
        case "isCloned":  // handle nullable type
          if (value != null)
            isCloned = Convert.ToBoolean(value);
          else
            isCloned = null;
          break;
        case "lineTag":
          lineTag = (String)value;
          break;
        case "pid":
          pid = (String)value;
          break;
        case "projectNumber":
          projectNumber = (String)value;
          break;
        case "quantity":  // handle nullable type
          if (value != null)
            quantity = Convert.ToInt32(value);
          else
            quantity = null;
          break;
        case "system":
          system = (String)value;
          break;
        case "rating":
          rating = (String)value;
          break;
        case "unit":
          unit = (String)value;
          break;
        case "uomDiameter":
          uomDiameter = (String)value;
          break;
        default:
          throw new Exception("property [" + propertyName + "] not found.");
      }
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
    public virtual Single? diameter { get; set; }
    public virtual String system { get; set; }
    public virtual String uomDiameter { get; set; }

    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "tag":
          return tag;
        case "diameter":
          return diameter;
        case "systen":
          return system;
        case "uomDiameter":
          return uomDiameter;
        default:
          throw new Exception("property [" + propertyName + "] not found.");
      }
    }

    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "tag":
          tag = (String)value;
          break;
        case "diameter":
          if (value != null)
            diameter = Convert.ToSingle(value);
          else
            diameter = null;
          break;
        case "system":
          system = (String)value;
          break;
        case "uomDiameter":
          uomDiameter = (String)value;
          break;
        default:
          throw new Exception("property [" + propertyName + "] not found.");
      }
    }
  }

  //public class Valve
  //{
  //  public virtual String Id { get; set; }
  //  public virtual String tag
  //  {
  //    get { return Id; }
  //    set { Id = value; }
  //  }
  //  public virtual String componentType { get; set; }
  //  public virtual Single? diameter { get; set; }
  //  public virtual Boolean? isCloned { get; set; }
  //  public virtual String lineTag { get; set; }
  //  public virtual String pid { get; set; }
  //  public virtual String projectNumber { get; set; }
  //  public virtual Int32? quantity { get; set; }
  //  public virtual String rating { get; set; }
  //  public virtual String system { get; set; }
  //  public virtual String unit { get; set; }
  //  public virtual String uomDiameter { get; set; }
  //}

  //public class Valve : DataObject
  //{
  //  public virtual String Id { get; set; }
  //  public virtual String tag
  //  {
  //    get { return Id; }
  //    set { Id = value; }
  //  }
  //  public virtual String componentType { get; set; }
  //  public virtual Single? diameter { get; set; }
  //  public virtual Boolean? isCloned { get; set; }
  //  public virtual String lineTag { get; set; }
  //  public virtual String pid { get; set; }
  //  public virtual String projectNumber { get; set; }
  //  public virtual Int32? quantity { get; set; }
  //  public virtual String rating { get; set; }
  //  public virtual String system { get; set; }
  //  public virtual String unit { get; set; }
  //  public virtual String uomDiameter { get; set; }

  //  public override object GetPropertyValue(string propertyName)
  //  {
  //    switch (propertyName)
  //    {
  //      case "tag":
  //        return tag;
  //      case "componentType":
  //        return componentType;
  //      case "diameter":
  //        return diameter;
  //      case "isCloned":
  //        return isCloned;
  //      case "lineTag":
  //        return lineTag;
  //      case "pid":
  //        return pid;
  //      case "projectNumber":
  //        return projectNumber;
  //      case "quantity":
  //        return quantity;
  //      case "rating":
  //        return rating;
  //      case "system":
  //        return system;
  //      case "unit":
  //        return unit;
  //      case "uomDiameter":
  //        return uomDiameter;
  //      default:
  //        throw new Exception("property [" + propertyName + "] not found.");
  //    }
  //  }

  //  public override void SetPropertyValue(string propertyName, object value)
  //  {
  //    switch (propertyName)
  //    {
  //      case "tag":
  //        tag = (String)value;
  //        break;
  //      case "componentType":
  //        componentType = (String)value;
  //        break;
  //      case "diameter":  // handle nullable type
  //        if (value != null)
  //          diameter = Convert.ToSingle(value);
  //        else
  //          diameter = null;
  //        break;
  //      case "isCloned":  // handle nullable type
  //        if (value != null)
  //          isCloned = Convert.ToBoolean(value);
  //        else
  //          isCloned = null;
  //        break;
  //      case "lineTag":
  //        lineTag = (String)value;
  //        break;
  //      case "pid":
  //        pid = (String)value;
  //        break;
  //      case "projectNumber":
  //        projectNumber = (String)value;
  //        break;
  //      case "quantity":  // handle nullable type
  //        if (value != null)
  //          quantity = Convert.ToInt32(value);
  //        else
  //          quantity = null;
  //        break;
  //      case "system":
  //        system = (String)value;
  //        break;
  //      case "rating":
  //        rating = (String)value;
  //        break;
  //      case "unit":
  //        unit = (String)value;
  //        break;
  //      case "uomDiameter":
  //        uomDiameter = (String)value;
  //        break;
  //      default:
  //        throw new Exception("property [" + propertyName + "] not found.");
  //    }
  //  }
  //}

  //public class Line : DataObject
  //{
  //  public virtual String Id { get; set; }
  //  public virtual String tag
  //  {
  //    get { return Id; }
  //    set { Id = value; }
  //  }
  //  public virtual Single? diameter { get; set; }
  //  public virtual String system { get; set; }
  //  public virtual String uomDiameter { get; set; }

  //  public override object GetPropertyValue(string propertyName)
  //  {
  //    switch (propertyName)
  //    {
  //      case "tag":
  //        return tag;
  //      case "diameter":
  //        return diameter;
  //      case "systen":
  //        return system;
  //      case "uomDiameter":
  //        return uomDiameter;
  //      default:
  //        throw new Exception("property [" + propertyName + "] not found.");
  //    }
  //  }

  //  public override void SetPropertyValue(string propertyName, object value)
  //  {
  //    switch (propertyName)
  //    {
  //      case "tag":
  //        tag = (String)value;
  //        break;
  //      case "diameter":
  //        if (value != null)
  //          diameter = Convert.ToSingle(value);
  //        else
  //          diameter = null;
  //        break;
  //      case "system":
  //        system = (String)value;
  //        break;
  //      case "uomDiameter":
  //        uomDiameter = (String)value;
  //        break;
  //      default:
  //        throw new Exception("property [" + propertyName + "] not found.");
  //    }
  //  }
  //}
}
