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

namespace org.iringtools.adapter.datalayer.proj_Jak.JakApp
{
  public class LINE2 : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual Decimal? SUN { get; set; }
    public virtual String UNIT { get; set; }
    public virtual String ENG_SYS { get; set; }
    public virtual String ENG_SYS_CLIENT { get; set; }
    public virtual String CODE_COM_GRP { get; set; }
    public virtual String CODE_COM_GRP_CLIENT { get; set; }
    public virtual Decimal? SEQUENCE_NUMBER { get; set; }
    public virtual String MATERIAL_SPEC_LINE { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "TAG": return TAG;
        case "SUN": return SUN;
        case "UNIT": return UNIT;
        case "ENG_SYS": return ENG_SYS;
        case "ENG_SYS_CLIENT": return ENG_SYS_CLIENT;
        case "CODE_COM_GRP": return CODE_COM_GRP;
        case "CODE_COM_GRP_CLIENT": return CODE_COM_GRP_CLIENT;
        case "SEQUENCE_NUMBER": return SEQUENCE_NUMBER;
        case "MATERIAL_SPEC_LINE": return MATERIAL_SPEC_LINE;
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
        case "TAG":
          TAG = Convert.ToString(value);
          break;
        case "SUN":
          SUN = Convert.ToDecimal(value);
          break;
        case "UNIT":
          UNIT = Convert.ToString(value);
          break;
        case "ENG_SYS":
          ENG_SYS = Convert.ToString(value);
          break;
        case "ENG_SYS_CLIENT":
          ENG_SYS_CLIENT = Convert.ToString(value);
          break;
        case "CODE_COM_GRP":
          CODE_COM_GRP = Convert.ToString(value);
          break;
        case "CODE_COM_GRP_CLIENT":
          CODE_COM_GRP_CLIENT = Convert.ToString(value);
          break;
        case "SEQUENCE_NUMBER":
          SEQUENCE_NUMBER = Convert.ToDecimal(value);
          break;
        case "MATERIAL_SPEC_LINE":
          MATERIAL_SPEC_LINE = Convert.ToString(value);
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

namespace org.iringtools.adapter.datalayer.proj_Jak.JakApp
{
  public class LINES : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TRAINNUMBER
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String TAG { get; set; }
    public virtual String ID { get; set; }
    public virtual String AREA { get; set; }
    public virtual String SPEC { get; set; }
    public virtual String SYSTEM { get; set; }
    public virtual String LINENO { get; set; }
    public virtual Single? NOMDIAMETER { get; set; }
    public virtual String INSULATIONTYPE { get; set; }
    public virtual String HTRACED { get; set; }
    public virtual String CONSTTYPE { get; set; }
    public virtual String DESPRESSURE { get; set; }
    public virtual String TESTPRESSURE { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "TRAINNUMBER": return TRAINNUMBER;
        case "TAG": return TAG;
        case "ID": return ID;
        case "AREA": return AREA;
        case "SPEC": return SPEC;
        case "SYSTEM": return SYSTEM;
        case "LINENO": return LINENO;
        case "NOMDIAMETER": return NOMDIAMETER;
        case "INSULATIONTYPE": return INSULATIONTYPE;
        case "HTRACED": return HTRACED;
        case "CONSTTYPE": return CONSTTYPE;
        case "DESPRESSURE": return DESPRESSURE;
        case "TESTPRESSURE": return TESTPRESSURE;
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
        case "TRAINNUMBER":
          TRAINNUMBER = Convert.ToString(value);
          break;
        case "TAG":
          TAG = Convert.ToString(value);
          break;
        case "ID":
          ID = Convert.ToString(value);
          break;
        case "AREA":
          AREA = Convert.ToString(value);
          break;
        case "SPEC":
          SPEC = Convert.ToString(value);
          break;
        case "SYSTEM":
          SYSTEM = Convert.ToString(value);
          break;
        case "LINENO":
          LINENO = Convert.ToString(value);
          break;
        case "NOMDIAMETER":
          NOMDIAMETER = Convert.ToSingle(value);
          break;
        case "INSULATIONTYPE":
          INSULATIONTYPE = Convert.ToString(value);
          break;
        case "HTRACED":
          HTRACED = Convert.ToString(value);
          break;
        case "CONSTTYPE":
          CONSTTYPE = Convert.ToString(value);
          break;
        case "DESPRESSURE":
          DESPRESSURE = Convert.ToString(value);
          break;
        case "TESTPRESSURE":
          TESTPRESSURE = Convert.ToString(value);
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
