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

namespace org.iringtools.adapter.datalayer.proj_Showroom.EM
{
  public class LINE : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String DWG_NO { get; set; }
    public virtual String ERROR_STATUS { get; set; }
    public virtual String SENDER_LOCK_FLAG { get; set; }
    public virtual String SENDER_LOCK_REASON { get; set; }
    public virtual String DESIGN { get; set; }
    public virtual String CLIENT_SYSTEM { get; set; }
    public virtual String RATING { get; set; }
    public virtual String REV { get; set; }
    public virtual String PID { get; set; }
    public virtual String LOCATOR { get; set; }
    public virtual String HOLD { get; set; }
    public virtual String HEAT_TRACE { get; set; }
    public virtual String PROJ_NO { get; set; }
    public virtual String DWG_TAG { get; set; }
    public virtual String LINE_NUMBER { get; set; }
    public virtual String SPEC { get; set; }
    public virtual Single? DIAMETER { get; set; }
    public virtual String DIAMETER_UOM { get; set; }
    public virtual String REV_STATUS { get; set; }
    public virtual String SEQ_NO { get; set; }
    public virtual String WERE_CLONED_FROM { get; set; }
    public virtual String WERE_CLONED_TO { get; set; }
    public virtual String COM_GRP { get; set; }
    public virtual String CIN { get; set; }
    public virtual String SYSTEM { get; set; }
    public virtual String IS_CLONE { get; set; }
    public virtual String SEQUENCE_CODE { get; set; }
    public virtual String UNIT { get; set; }
    public virtual String WAS_CLONED { get; set; }
    public virtual String CLIENT_UNIT { get; set; }
    public virtual Double? INS_THK { get; set; }
    public virtual String PUB_FLAG { get; set; }
    public virtual String BSAP { get; set; }
    public virtual String ATTACHMENT_OBJECT_TYPE { get; set; }
    public virtual String ATTACHMENT_CHILD_ID { get; set; }
    public virtual Double? ATTACHMENT_ORDER { get; set; }
    public virtual String ATTACHMENT_RELATION_TYPE { get; set; }
    public virtual Double? ATTACHMENT_SIZE { get; set; }
    public virtual String ATTACHMENT_FILE_EXTENSION { get; set; }
    public virtual String ATTACHMENT_MIME_TYPE { get; set; }
    public virtual String ATTACHMENT_FILE_NAME { get; set; }
    public virtual String ATTACHMENT_REVISION_TYPE { get; set; }
    public virtual String ATTACHMENT_ROLE_FLAG { get; set; }
    public virtual String ATTACHMENT_CHECK_SUM { get; set; }
    public virtual String ATTACHMENT_DIFFERENCING_TOKEN { get; set; }
    public virtual DateTime? ATTACHMENT_LAST_UPDATED { get; set; }
    public virtual String ATTACHMENT_COMPRESSION_FLAG { get; set; }
    public virtual String ATTACHMENT_COMPRESSION_TYPE { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "DWG_NO": return DWG_NO;
        case "ERROR_STATUS": return ERROR_STATUS;
        case "SENDER_LOCK_FLAG": return SENDER_LOCK_FLAG;
        case "SENDER_LOCK_REASON": return SENDER_LOCK_REASON;
        case "DESIGN": return DESIGN;
        case "CLIENT_SYSTEM": return CLIENT_SYSTEM;
        case "RATING": return RATING;
        case "REV": return REV;
        case "PID": return PID;
        case "LOCATOR": return LOCATOR;
        case "HOLD": return HOLD;
        case "HEAT_TRACE": return HEAT_TRACE;
        case "PROJ_NO": return PROJ_NO;
        case "DWG_TAG": return DWG_TAG;
        case "LINE_NUMBER": return LINE_NUMBER;
        case "SPEC": return SPEC;
        case "DIAMETER": return DIAMETER;
        case "DIAMETER_UOM": return DIAMETER_UOM;
        case "REV_STATUS": return REV_STATUS;
        case "SEQ_NO": return SEQ_NO;
        case "WERE_CLONED_FROM": return WERE_CLONED_FROM;
        case "WERE_CLONED_TO": return WERE_CLONED_TO;
        case "COM_GRP": return COM_GRP;
        case "CIN": return CIN;
        case "SYSTEM": return SYSTEM;
        case "IS_CLONE": return IS_CLONE;
        case "SEQUENCE_CODE": return SEQUENCE_CODE;
        case "UNIT": return UNIT;
        case "WAS_CLONED": return WAS_CLONED;
        case "TAG": return TAG;
        case "CLIENT_UNIT": return CLIENT_UNIT;
        case "INS_THK": return INS_THK;
        case "PUB_FLAG": return PUB_FLAG;
        case "BSAP": return BSAP;
        case "ATTACHMENT_OBJECT_TYPE": return ATTACHMENT_OBJECT_TYPE;
        case "ATTACHMENT_CHILD_ID": return ATTACHMENT_CHILD_ID;
        case "ATTACHMENT_ORDER": return ATTACHMENT_ORDER;
        case "ATTACHMENT_RELATION_TYPE": return ATTACHMENT_RELATION_TYPE;
        case "ATTACHMENT_SIZE": return ATTACHMENT_SIZE;
        case "ATTACHMENT_FILE_EXTENSION": return ATTACHMENT_FILE_EXTENSION;
        case "ATTACHMENT_MIME_TYPE": return ATTACHMENT_MIME_TYPE;
        case "ATTACHMENT_FILE_NAME": return ATTACHMENT_FILE_NAME;
        case "ATTACHMENT_REVISION_TYPE": return ATTACHMENT_REVISION_TYPE;
        case "ATTACHMENT_ROLE_FLAG": return ATTACHMENT_ROLE_FLAG;
        case "ATTACHMENT_CHECK_SUM": return ATTACHMENT_CHECK_SUM;
        case "ATTACHMENT_DIFFERENCING_TOKEN": return ATTACHMENT_DIFFERENCING_TOKEN;
        case "ATTACHMENT_LAST_UPDATED": return ATTACHMENT_LAST_UPDATED;
        case "ATTACHMENT_COMPRESSION_FLAG": return ATTACHMENT_COMPRESSION_FLAG;
        case "ATTACHMENT_COMPRESSION_TYPE": return ATTACHMENT_COMPRESSION_TYPE;
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
        case "DWG_NO":
          if (value != null) DWG_NO = Convert.ToString(value);
          break;
        case "ERROR_STATUS":
          if (value != null) ERROR_STATUS = Convert.ToString(value);
          break;
        case "SENDER_LOCK_FLAG":
          if (value != null) SENDER_LOCK_FLAG = Convert.ToString(value);
          break;
        case "SENDER_LOCK_REASON":
          if (value != null) SENDER_LOCK_REASON = Convert.ToString(value);
          break;
        case "DESIGN":
          if (value != null) DESIGN = Convert.ToString(value);
          break;
        case "CLIENT_SYSTEM":
          if (value != null) CLIENT_SYSTEM = Convert.ToString(value);
          break;
        case "RATING":
          if (value != null) RATING = Convert.ToString(value);
          break;
        case "REV":
          if (value != null) REV = Convert.ToString(value);
          break;
        case "PID":
          if (value != null) PID = Convert.ToString(value);
          break;
        case "LOCATOR":
          if (value != null) LOCATOR = Convert.ToString(value);
          break;
        case "HOLD":
          if (value != null) HOLD = Convert.ToString(value);
          break;
        case "HEAT_TRACE":
          if (value != null) HEAT_TRACE = Convert.ToString(value);
          break;
        case "PROJ_NO":
          if (value != null) PROJ_NO = Convert.ToString(value);
          break;
        case "DWG_TAG":
          if (value != null) DWG_TAG = Convert.ToString(value);
          break;
        case "LINE_NUMBER":
          if (value != null) LINE_NUMBER = Convert.ToString(value);
          break;
        case "SPEC":
          if (value != null) SPEC = Convert.ToString(value);
          break;
        case "DIAMETER":
          if (value != null) DIAMETER = Convert.ToSingle(value);
          break;
        case "DIAMETER_UOM":
          if (value != null) DIAMETER_UOM = Convert.ToString(value);
          break;
        case "REV_STATUS":
          if (value != null) REV_STATUS = Convert.ToString(value);
          break;
        case "SEQ_NO":
          if (value != null) SEQ_NO = Convert.ToString(value);
          break;
        case "WERE_CLONED_FROM":
          if (value != null) WERE_CLONED_FROM = Convert.ToString(value);
          break;
        case "WERE_CLONED_TO":
          if (value != null) WERE_CLONED_TO = Convert.ToString(value);
          break;
        case "COM_GRP":
          if (value != null) COM_GRP = Convert.ToString(value);
          break;
        case "CIN":
          if (value != null) CIN = Convert.ToString(value);
          break;
        case "SYSTEM":
          if (value != null) SYSTEM = Convert.ToString(value);
          break;
        case "IS_CLONE":
          if (value != null) IS_CLONE = Convert.ToString(value);
          break;
        case "SEQUENCE_CODE":
          if (value != null) SEQUENCE_CODE = Convert.ToString(value);
          break;
        case "UNIT":
          if (value != null) UNIT = Convert.ToString(value);
          break;
        case "WAS_CLONED":
          if (value != null) WAS_CLONED = Convert.ToString(value);
          break;
        case "TAG":
          if (value != null) TAG = Convert.ToString(value);
          break;
        case "CLIENT_UNIT":
          if (value != null) CLIENT_UNIT = Convert.ToString(value);
          break;
        case "INS_THK":
          if (value != null) INS_THK = Convert.ToDouble(value);
          break;
        case "PUB_FLAG":
          if (value != null) PUB_FLAG = Convert.ToString(value);
          break;
        case "BSAP":
          if (value != null) BSAP = Convert.ToString(value);
          break;
        case "ATTACHMENT_OBJECT_TYPE":
          if (value != null) ATTACHMENT_OBJECT_TYPE = Convert.ToString(value);
          break;
        case "ATTACHMENT_CHILD_ID":
          if (value != null) ATTACHMENT_CHILD_ID = Convert.ToString(value);
          break;
        case "ATTACHMENT_ORDER":
          if (value != null) ATTACHMENT_ORDER = Convert.ToDouble(value);
          break;
        case "ATTACHMENT_RELATION_TYPE":
          if (value != null) ATTACHMENT_RELATION_TYPE = Convert.ToString(value);
          break;
        case "ATTACHMENT_SIZE":
          if (value != null) ATTACHMENT_SIZE = Convert.ToDouble(value);
          break;
        case "ATTACHMENT_FILE_EXTENSION":
          if (value != null) ATTACHMENT_FILE_EXTENSION = Convert.ToString(value);
          break;
        case "ATTACHMENT_MIME_TYPE":
          if (value != null) ATTACHMENT_MIME_TYPE = Convert.ToString(value);
          break;
        case "ATTACHMENT_FILE_NAME":
          if (value != null) ATTACHMENT_FILE_NAME = Convert.ToString(value);
          break;
        case "ATTACHMENT_REVISION_TYPE":
          if (value != null) ATTACHMENT_REVISION_TYPE = Convert.ToString(value);
          break;
        case "ATTACHMENT_ROLE_FLAG":
          if (value != null) ATTACHMENT_ROLE_FLAG = Convert.ToString(value);
          break;
        case "ATTACHMENT_CHECK_SUM":
          if (value != null) ATTACHMENT_CHECK_SUM = Convert.ToString(value);
          break;
        case "ATTACHMENT_DIFFERENCING_TOKEN":
          if (value != null) ATTACHMENT_DIFFERENCING_TOKEN = Convert.ToString(value);
          break;
        case "ATTACHMENT_LAST_UPDATED":
          if (value != null) ATTACHMENT_LAST_UPDATED = Convert.ToDateTime(value);
          break;
        case "ATTACHMENT_COMPRESSION_FLAG":
          if (value != null) ATTACHMENT_COMPRESSION_FLAG = Convert.ToString(value);
          break;
        case "ATTACHMENT_COMPRESSION_TYPE":
          if (value != null) ATTACHMENT_COMPRESSION_TYPE = Convert.ToString(value);
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
          throw new NotImplementedException("This method has been deprecated.");
        }
      }
    }
    
    public class VALVE : IDataObject
    {
      public virtual String Id { get; set; }
      public virtual String TAG
      {
        get { return Id; }
        set { Id = value; }
      }
      public virtual String DWG_NO { get; set; }
      public virtual String ERROR_STATUS { get; set; }
      public virtual String SENDER_LOCK { get; set; }
      public virtual String SENDER_LOCK_REASON { get; set; }
      public virtual String STARTUP { get; set; }
      public virtual String RATING { get; set; }
      public virtual String PID_REV { get; set; }
      public virtual String DES_RESP { get; set; }
      public virtual String PID { get; set; }
      public virtual String HOLD { get; set; }
      public virtual String HEAT_TRACE { get; set; }
      public virtual String PROJ_NO { get; set; }
      public virtual String LINE_NUMBER { get; set; }
      public virtual String MATERIAL_SPEC_LINE { get; set; }
      public virtual String LINE_TAG { get; set; }
      public virtual Double? QUANTITY { get; set; }
      public virtual Single? DIAMETER { get; set; }
      public virtual String DIAMETER_UOM { get; set; }
      public virtual String VALVE_LOCK_DEVICE { get; set; }
      public virtual String VALVE_NUMBER { get; set; }
      public virtual String SEQ_NO { get; set; }
      public virtual String CLIENT_SYSTEM { get; set; }
      public virtual String WERE_CLONED_FROM { get; set; }
      public virtual String WERE_CLONED_TO { get; set; }
      public virtual String COM_GRP { get; set; }
      public virtual String CIN { get; set; }
      public virtual String SYSTEM { get; set; }
      public virtual String IS_CLONED { get; set; }
      public virtual String PID_LOCATION { get; set; }
      public virtual String VALVE_TYPE { get; set; }
      public virtual String SEQUENCE { get; set; }
      public virtual String SUFFIX { get; set; }
      public virtual String UNIT { get; set; }
      public virtual String WAS_CLONED { get; set; }
      public virtual String CLIENT_UNIT { get; set; }
      
      public virtual object GetPropertyValue(string propertyName)
      {
        switch (propertyName)
        {
          case "Id": return Id;
          case "DWG_NO": return DWG_NO;
          case "ERROR_STATUS": return ERROR_STATUS;
          case "SENDER_LOCK": return SENDER_LOCK;
          case "SENDER_LOCK_REASON": return SENDER_LOCK_REASON;
          case "STARTUP": return STARTUP;
          case "RATING": return RATING;
          case "PID_REV": return PID_REV;
          case "DES_RESP": return DES_RESP;
          case "PID": return PID;
          case "HOLD": return HOLD;
          case "HEAT_TRACE": return HEAT_TRACE;
          case "PROJ_NO": return PROJ_NO;
          case "LINE_NUMBER": return LINE_NUMBER;
          case "MATERIAL_SPEC_LINE": return MATERIAL_SPEC_LINE;
          case "LINE_TAG": return LINE_TAG;
          case "QUANTITY": return QUANTITY;
          case "DIAMETER": return DIAMETER;
          case "DIAMETER_UOM": return DIAMETER_UOM;
          case "VALVE_LOCK_DEVICE": return VALVE_LOCK_DEVICE;
          case "VALVE_NUMBER": return VALVE_NUMBER;
          case "SEQ_NO": return SEQ_NO;
          case "CLIENT_SYSTEM": return CLIENT_SYSTEM;
          case "WERE_CLONED_FROM": return WERE_CLONED_FROM;
          case "WERE_CLONED_TO": return WERE_CLONED_TO;
          case "COM_GRP": return COM_GRP;
          case "CIN": return CIN;
          case "SYSTEM": return SYSTEM;
          case "IS_CLONED": return IS_CLONED;
          case "PID_LOCATION": return PID_LOCATION;
          case "VALVE_TYPE": return VALVE_TYPE;
          case "SEQUENCE": return SEQUENCE;
          case "SUFFIX": return SUFFIX;
          case "UNIT": return UNIT;
          case "WAS_CLONED": return WAS_CLONED;
          case "TAG": return TAG;
          case "CLIENT_UNIT": return CLIENT_UNIT;
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
          case "DWG_NO":
            if (value != null) DWG_NO = Convert.ToString(value);
            break;
          case "ERROR_STATUS":
            if (value != null) ERROR_STATUS = Convert.ToString(value);
            break;
          case "SENDER_LOCK":
            if (value != null) SENDER_LOCK = Convert.ToString(value);
            break;
          case "SENDER_LOCK_REASON":
            if (value != null) SENDER_LOCK_REASON = Convert.ToString(value);
            break;
          case "STARTUP":
            if (value != null) STARTUP = Convert.ToString(value);
            break;
          case "RATING":
            if (value != null) RATING = Convert.ToString(value);
            break;
          case "PID_REV":
            if (value != null) PID_REV = Convert.ToString(value);
            break;
          case "DES_RESP":
            if (value != null) DES_RESP = Convert.ToString(value);
            break;
          case "PID":
            if (value != null) PID = Convert.ToString(value);
            break;
          case "HOLD":
            if (value != null) HOLD = Convert.ToString(value);
            break;
          case "HEAT_TRACE":
            if (value != null) HEAT_TRACE = Convert.ToString(value);
            break;
          case "PROJ_NO":
            if (value != null) PROJ_NO = Convert.ToString(value);
            break;
          case "LINE_NUMBER":
            if (value != null) LINE_NUMBER = Convert.ToString(value);
            break;
          case "MATERIAL_SPEC_LINE":
            if (value != null) MATERIAL_SPEC_LINE = Convert.ToString(value);
            break;
          case "LINE_TAG":
            if (value != null) LINE_TAG = Convert.ToString(value);
            break;
          case "QUANTITY":
            if (value != null) QUANTITY = Convert.ToDouble(value);
            break;
          case "DIAMETER":
            if (value != null) DIAMETER = Convert.ToSingle(value);
            break;
          case "DIAMETER_UOM":
            if (value != null) DIAMETER_UOM = Convert.ToString(value);
            break;
          case "VALVE_LOCK_DEVICE":
            if (value != null) VALVE_LOCK_DEVICE = Convert.ToString(value);
            break;
          case "VALVE_NUMBER":
            if (value != null) VALVE_NUMBER = Convert.ToString(value);
            break;
          case "SEQ_NO":
            if (value != null) SEQ_NO = Convert.ToString(value);
            break;
          case "CLIENT_SYSTEM":
            if (value != null) CLIENT_SYSTEM = Convert.ToString(value);
            break;
          case "WERE_CLONED_FROM":
            if (value != null) WERE_CLONED_FROM = Convert.ToString(value);
            break;
          case "WERE_CLONED_TO":
            if (value != null) WERE_CLONED_TO = Convert.ToString(value);
            break;
          case "COM_GRP":
            if (value != null) COM_GRP = Convert.ToString(value);
            break;
          case "CIN":
            if (value != null) CIN = Convert.ToString(value);
            break;
          case "SYSTEM":
            if (value != null) SYSTEM = Convert.ToString(value);
            break;
          case "IS_CLONED":
            if (value != null) IS_CLONED = Convert.ToString(value);
            break;
          case "PID_LOCATION":
            if (value != null) PID_LOCATION = Convert.ToString(value);
            break;
          case "VALVE_TYPE":
            if (value != null) VALVE_TYPE = Convert.ToString(value);
            break;
          case "SEQUENCE":
            if (value != null) SEQUENCE = Convert.ToString(value);
            break;
          case "SUFFIX":
            if (value != null) SUFFIX = Convert.ToString(value);
            break;
          case "UNIT":
            if (value != null) UNIT = Convert.ToString(value);
            break;
          case "WAS_CLONED":
            if (value != null) WAS_CLONED = Convert.ToString(value);
            break;
          case "TAG":
            if (value != null) TAG = Convert.ToString(value);
            break;
          case "CLIENT_UNIT":
            if (value != null) CLIENT_UNIT = Convert.ToString(value);
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
            throw new NotImplementedException("This method has been deprecated.");
          }
        }
      }
    }
