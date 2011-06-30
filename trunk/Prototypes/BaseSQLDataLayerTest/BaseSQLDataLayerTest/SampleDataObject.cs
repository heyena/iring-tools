using System;
using System.Collections.Generic;
using org.iringtools.library;

namespace com.example
{
  public class LINES : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String ID { get; set; }
    public virtual String AREA { get; set; }
    public virtual String TRAINNUMBER { get; set; }
    public virtual String SPEC { get; set; }
    public virtual String SYSTEM { get; set; }
    public virtual String LINENO { get; set; }
    public virtual Double? NOMDIAMETER { get; set; }
    public virtual String INSULATIONTYPE { get; set; }
    public virtual String HTRACED { get; set; }
    public virtual String CONSTTYPE { get; set; }
    public virtual String DESPRESSURE { get; set; }
    public virtual String TESTPRESSURE { get; set; }
    public virtual String PWHT { get; set; }
    public virtual String TESTMEDIA { get; set; }
    public virtual String MATLTYPE { get; set; }
    public virtual String NDT { get; set; }
    public virtual String NDE { get; set; }
    public virtual String PIPECLASS { get; set; }
    public virtual String PIDNUMBER { get; set; }
    public virtual String DESTEMPERATURE { get; set; }
    public virtual String PAINTSYSTEM { get; set; }
    public virtual String DESIGNCODE { get; set; }
    public virtual String COLOURCODE { get; set; }
    public virtual String EWP { get; set; }
    public virtual String USER1 { get; set; }
    public virtual String TAGSTATUS { get; set; }
    public virtual String FULLLINE { get; set; }
    public virtual String UOM_NOMDIAMETER { get; set; }
    public virtual String UOM_DESPRESSURE { get; set; }
    public virtual String UOM_DESTEMPERATURE { get; set; }

    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "TAG": return TAG;
        case "ID": return ID;
        case "AREA": return AREA;
        case "TRAINNUMBER": return TRAINNUMBER;
        case "SPEC": return SPEC;
        case "SYSTEM": return SYSTEM;
        case "LINENO": return LINENO;
        case "NOMDIAMETER": return NOMDIAMETER;
        case "INSULATIONTYPE": return INSULATIONTYPE;
        case "HTRACED": return HTRACED;
        case "CONSTTYPE": return CONSTTYPE;
        case "DESPRESSURE": return DESPRESSURE;
        case "TESTPRESSURE": return TESTPRESSURE;
        case "PWHT": return PWHT;
        case "TESTMEDIA": return TESTMEDIA;
        case "MATLTYPE": return MATLTYPE;
        case "NDT": return NDT;
        case "NDE": return NDE;
        case "PIPECLASS": return PIPECLASS;
        case "PIDNUMBER": return PIDNUMBER;
        case "DESTEMPERATURE": return DESTEMPERATURE;
        case "PAINTSYSTEM": return PAINTSYSTEM;
        case "DESIGNCODE": return DESIGNCODE;
        case "COLOURCODE": return COLOURCODE;
        case "EWP": return EWP;
        case "USER1": return USER1;
        case "TAGSTATUS": return TAGSTATUS;
        case "FULLLINE": return FULLLINE;
        case "UOM_NOMDIAMETER": return UOM_NOMDIAMETER;
        case "UOM_DESPRESSURE": return UOM_DESPRESSURE;
        case "UOM_DESTEMPERATURE": return UOM_DESTEMPERATURE;
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
          if (value != null) TAG = Convert.ToString(value);
          break;
        case "ID":
          if (value != null) ID = Convert.ToString(value);
          break;
        case "AREA":
          if (value != null) AREA = Convert.ToString(value);
          break;
        case "TRAINNUMBER":
          if (value != null) TRAINNUMBER = Convert.ToString(value);
          break;
        case "SPEC":
          if (value != null) SPEC = Convert.ToString(value);
          break;
        case "SYSTEM":
          if (value != null) SYSTEM = Convert.ToString(value);
          break;
        case "LINENO":
          if (value != null) LINENO = Convert.ToString(value);
          break;
        case "NOMDIAMETER":
          if (value != null) NOMDIAMETER = Convert.ToDouble(value);
          break;
        case "INSULATIONTYPE":
          if (value != null) INSULATIONTYPE = Convert.ToString(value);
          break;
        case "HTRACED":
          if (value != null) HTRACED = Convert.ToString(value);
          break;
        case "CONSTTYPE":
          if (value != null) CONSTTYPE = Convert.ToString(value);
          break;
        case "DESPRESSURE":
          if (value != null) DESPRESSURE = Convert.ToString(value);
          break;
        case "TESTPRESSURE":
          if (value != null) TESTPRESSURE = Convert.ToString(value);
          break;
        case "PWHT":
          if (value != null) PWHT = Convert.ToString(value);
          break;
        case "TESTMEDIA":
          if (value != null) TESTMEDIA = Convert.ToString(value);
          break;
        case "MATLTYPE":
          if (value != null) MATLTYPE = Convert.ToString(value);
          break;
        case "NDT":
          if (value != null) NDT = Convert.ToString(value);
          break;
        case "NDE":
          if (value != null) NDE = Convert.ToString(value);
          break;
        case "PIPECLASS":
          if (value != null) PIPECLASS = Convert.ToString(value);
          break;
        case "PIDNUMBER":
          if (value != null) PIDNUMBER = Convert.ToString(value);
          break;
        case "DESTEMPERATURE":
          if (value != null) DESTEMPERATURE = Convert.ToString(value);
          break;
        case "PAINTSYSTEM":
          if (value != null) PAINTSYSTEM = Convert.ToString(value);
          break;
        case "DESIGNCODE":
          if (value != null) DESIGNCODE = Convert.ToString(value);
          break;
        case "COLOURCODE":
          if (value != null) COLOURCODE = Convert.ToString(value);
          break;
        case "EWP":
          if (value != null) EWP = Convert.ToString(value);
          break;
        case "USER1":
          if (value != null) USER1 = Convert.ToString(value);
          break;
        case "TAGSTATUS":
          if (value != null) TAGSTATUS = Convert.ToString(value);
          break;
        case "FULLLINE":
          if (value != null) FULLLINE = Convert.ToString(value);
          break;
        case "UOM_NOMDIAMETER":
          if (value != null) UOM_NOMDIAMETER = Convert.ToString(value);
          break;
        case "UOM_DESPRESSURE":
          if (value != null) UOM_DESPRESSURE = Convert.ToString(value);
          break;
        case "UOM_DESTEMPERATURE":
          if (value != null) UOM_DESTEMPERATURE = Convert.ToString(value);
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
