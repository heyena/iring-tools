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
  public class INSTRUMENTS : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String KEYTAG { get; set; }
    public virtual String TAG_NO { get; set; }
    public virtual String TAG_CODE { get; set; }
    public virtual String ASSOC_EQ { get; set; }
    public virtual String IAREA { get; set; }
    public virtual String ITRAIN { get; set; }
    public virtual String ITYP { get; set; }
    public virtual String INUM { get; set; }
    public virtual String ISUFFIX { get; set; }
    public virtual String MODIFIER1 { get; set; }
    public virtual String MODIFIER2 { get; set; }
    public virtual String MODIFIER3 { get; set; }
    public virtual String MODIFIER4 { get; set; }
    public virtual String STD_DETAIL { get; set; }
    public virtual String DESCRIPT { get; set; }
    public virtual String TAG_TYPE { get; set; }
    public virtual String CONST_TYPE { get; set; }
    public virtual String COMP_ID { get; set; }
    public virtual String PROJ_STAT { get; set; }
    public virtual String PID_NO { get; set; }
    public virtual String LINE_NO { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "KEYTAG": return KEYTAG;
        case "TAG": return TAG;
        case "TAG_NO": return TAG_NO;
        case "TAG_CODE": return TAG_CODE;
        case "ASSOC_EQ": return ASSOC_EQ;
        case "IAREA": return IAREA;
        case "ITRAIN": return ITRAIN;
        case "ITYP": return ITYP;
        case "INUM": return INUM;
        case "ISUFFIX": return ISUFFIX;
        case "MODIFIER1": return MODIFIER1;
        case "MODIFIER2": return MODIFIER2;
        case "MODIFIER3": return MODIFIER3;
        case "MODIFIER4": return MODIFIER4;
        case "STD_DETAIL": return STD_DETAIL;
        case "DESCRIPT": return DESCRIPT;
        case "TAG_TYPE": return TAG_TYPE;
        case "CONST_TYPE": return CONST_TYPE;
        case "COMP_ID": return COMP_ID;
        case "PROJ_STAT": return PROJ_STAT;
        case "PID_NO": return PID_NO;
        case "LINE_NO": return LINE_NO;
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
        case "KEYTAG":
          if (value != null) KEYTAG = Convert.ToString(value);
          break;
        case "TAG":
          if (value != null) TAG = Convert.ToString(value);
          break;
        case "TAG_NO":
          if (value != null) TAG_NO = Convert.ToString(value);
          break;
        case "TAG_CODE":
          if (value != null) TAG_CODE = Convert.ToString(value);
          break;
        case "ASSOC_EQ":
          if (value != null) ASSOC_EQ = Convert.ToString(value);
          break;
        case "IAREA":
          if (value != null) IAREA = Convert.ToString(value);
          break;
        case "ITRAIN":
          if (value != null) ITRAIN = Convert.ToString(value);
          break;
        case "ITYP":
          if (value != null) ITYP = Convert.ToString(value);
          break;
        case "INUM":
          if (value != null) INUM = Convert.ToString(value);
          break;
        case "ISUFFIX":
          if (value != null) ISUFFIX = Convert.ToString(value);
          break;
        case "MODIFIER1":
          if (value != null) MODIFIER1 = Convert.ToString(value);
          break;
        case "MODIFIER2":
          if (value != null) MODIFIER2 = Convert.ToString(value);
          break;
        case "MODIFIER3":
          if (value != null) MODIFIER3 = Convert.ToString(value);
          break;
        case "MODIFIER4":
          if (value != null) MODIFIER4 = Convert.ToString(value);
          break;
        case "STD_DETAIL":
          if (value != null) STD_DETAIL = Convert.ToString(value);
          break;
        case "DESCRIPT":
          if (value != null) DESCRIPT = Convert.ToString(value);
          break;
        case "TAG_TYPE":
          if (value != null) TAG_TYPE = Convert.ToString(value);
          break;
        case "CONST_TYPE":
          if (value != null) CONST_TYPE = Convert.ToString(value);
          break;
        case "COMP_ID":
          if (value != null) COMP_ID = Convert.ToString(value);
          break;
        case "PROJ_STAT":
          if (value != null) PROJ_STAT = Convert.ToString(value);
          break;
        case "PID_NO":
          if (value != null) PID_NO = Convert.ToString(value);
          break;
        case "LINE_NO":
          if (value != null) LINE_NO = Convert.ToString(value);
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
      public virtual String NOMDIAMETER { get; set; }
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
            if (value != null) NOMDIAMETER = Convert.ToString(value);
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
      
      public class EQUIPMENT : IDataObject
      {
        public virtual String Id { get; set; }
        public virtual String TAG
        {
          get { return Id; }
          set { Id = value; }
        }
        public virtual String INTERNAL_TAG { get; set; }
        public virtual String ID { get; set; }
        public virtual String AREA { get; set; }
        public virtual String TRAINNUMBER { get; set; }
        public virtual String EQTYPE { get; set; }
        public virtual String EQPPREFIX { get; set; }
        public virtual String EQSEQNO { get; set; }
        public virtual String EQPSUFF { get; set; }
        public virtual String EQUIPDESC1 { get; set; }
        public virtual String EQUIPDESC2 { get; set; }
        public virtual String CONSTTYPE { get; set; }
        public virtual String EWP { get; set; }
        public virtual String USER1 { get; set; }
        public virtual String USER2 { get; set; }
        public virtual String USER3 { get; set; }
        public virtual String TAGSTATUS { get; set; }
        public virtual String COMMODITY { get; set; }
        
        public virtual object GetPropertyValue(string propertyName)
        {
          switch (propertyName)
          {
            case "Id": return Id;
            case "TAG": return TAG;
            case "INTERNAL_TAG": return INTERNAL_TAG;
            case "ID": return ID;
            case "AREA": return AREA;
            case "TRAINNUMBER": return TRAINNUMBER;
            case "EQTYPE": return EQTYPE;
            case "EQPPREFIX": return EQPPREFIX;
            case "EQSEQNO": return EQSEQNO;
            case "EQPSUFF": return EQPSUFF;
            case "EQUIPDESC1": return EQUIPDESC1;
            case "EQUIPDESC2": return EQUIPDESC2;
            case "CONSTTYPE": return CONSTTYPE;
            case "EWP": return EWP;
            case "USER1": return USER1;
            case "USER2": return USER2;
            case "USER3": return USER3;
            case "TAGSTATUS": return TAGSTATUS;
            case "COMMODITY": return COMMODITY;
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
            case "INTERNAL_TAG":
              if (value != null) INTERNAL_TAG = Convert.ToString(value);
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
            case "EQTYPE":
              if (value != null) EQTYPE = Convert.ToString(value);
              break;
            case "EQPPREFIX":
              if (value != null) EQPPREFIX = Convert.ToString(value);
              break;
            case "EQSEQNO":
              if (value != null) EQSEQNO = Convert.ToString(value);
              break;
            case "EQPSUFF":
              if (value != null) EQPSUFF = Convert.ToString(value);
              break;
            case "EQUIPDESC1":
              if (value != null) EQUIPDESC1 = Convert.ToString(value);
              break;
            case "EQUIPDESC2":
              if (value != null) EQUIPDESC2 = Convert.ToString(value);
              break;
            case "CONSTTYPE":
              if (value != null) CONSTTYPE = Convert.ToString(value);
              break;
            case "EWP":
              if (value != null) EWP = Convert.ToString(value);
              break;
            case "USER1":
              if (value != null) USER1 = Convert.ToString(value);
              break;
            case "USER2":
              if (value != null) USER2 = Convert.ToString(value);
              break;
            case "USER3":
              if (value != null) USER3 = Convert.ToString(value);
              break;
            case "TAGSTATUS":
              if (value != null) TAGSTATUS = Convert.ToString(value);
              break;
            case "COMMODITY":
              if (value != null) COMMODITY = Convert.ToString(value);
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