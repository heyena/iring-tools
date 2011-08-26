using System;
using System.Collections.Generic;
using org.iringtools.library;

namespace WidgetService
{
  public class Widget : IDataObject
  {
    
    public virtual List<DESIGNPART> DESIGNPART { get; set; }
    public virtual List<TECHNICALATTRIBUTEDEFINITION> TECHNICALATTRIBUTEDEFINITION { get; set; }
    public virtual List<UNITOFMEASURE> UNITOFMEASURE { get; set; }
    public virtual List<OBJECT> OBJECT { get; set; }
    public virtual List<PLANT> PLANT { get; set; }
    public virtual List<OBJECTTECHNICALSPECIFICATION> OBJECTTECHNICALSPECIFICATION { get; set; }
    public virtual List<OBJECTBILLOFMATERIAL> OBJECTBILLOFMATERIAL { get; set; }
    public virtual List<STANDARD> STANDARD { get; set; }
    public virtual List<OBJECTFUNCTION> OBJECTFUNCTION { get; set; }
    public virtual List<BASEUNIT> BASEUNIT { get; set; }
    public virtual List<OBJECTDESIGNPART> OBJECTDESIGNPART { get; set; }
    
    public class DESIGNPART
    {
      public string STANDARD;
      public string VERSION;
      public string DESCRIPTION;
      public string KEYA;
      public string KEYA01;
      public string SITE;
      public string CLASS;
      public string UNITOFMEASURE;
      public string PARTNUMBER;
      public string MANUFACTURER;
      public string SUPPLIER;
      public string STATE;
      public string ISLATESTVERSION;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class TECHNICALATTRIBUTEDEFINITION
    {
      public string ATTRIBUTE;
      public string DESCRIPTION;
      public string TYPE;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class UNITOFMEASURE
    {
      public string CODE;
      public string DESCRIPTION;
      public string MULTIPLYFACTOR;
      public string DIVISIONFACTOR;
      public string TENPOWER;
      public string USERDEFINED;
      public string USEDINAPPLICATION;
      public string TYPE;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class OBJECT
    {
      public string PLANT;
      public string CLASS;
      public string DESCRIPTION;
      public string KEYA;
      public string KEYA01;
      public string KEYA02;
      public string KEYA03;
      public string KEYA04;
      public string REMARK2;
      public string SITE;
      public string STATUS;
      public string COSTCENTER;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class PLANT
    {
      public string NAME;
      public string DESCRIPTION;
      public string DESCRIPTION2;
      public string LONGDESCRIPTION;
      public string STANDARD;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class OBJECTTECHNICALSPECIFICATION
    {
      public string OBJECT;
      public string TECHNICALCLASS;
      public string TECHNICALATTRIBUTEVALUES;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class OBJECTBILLOFMATERIAL
    {
      public string DESIGNPART;
      public string OBJECT;
      public string SORTORDER;
      public string SPAREPARTQUANTITY;
      public string PURCHASEQUANTITY;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class STANDARD
    {
      public string NAME;
      public string DESCRIPTION;
      public string DESCRIPTION2;
      public string ARTICLESTANDARD;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class OBJECTFUNCTION
    {
      public string SOURCE;
      public string TARGET;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class BASEUNIT
    {
      public string SOURCE;
      public string TARGET;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public class OBJECTDESIGNPART
    {
      public string SOURCE;
      public string TARGET;
      public string ID;
      public string STATE;
      public string IMPORTSTATE;
    }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "DESIGNPART": return DESIGNPART;
        case "TECHNICALATTRIBUTEDEFINITION": return TECHNICALATTRIBUTEDEFINITION;
        case "UNITOFMEASURE": return UNITOFMEASURE;
        case "OBJECT": return OBJECT;
        case "PLANT": return PLANT;
        case "OBJECTTECHNICALSPECIFICATION": return OBJECTTECHNICALSPECIFICATION;
        case "OBJECTBILLOFMATERIAL": return OBJECTBILLOFMATERIAL;
        case "STANDARD": return STANDARD;
        case "OBJECTFUNCTION": return OBJECTFUNCTION;
        case "BASEUNIT": return BASEUNIT;
        case "OBJECTDESIGNPART": return OBJECTDESIGNPART;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
        }
      }
    
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {case "DESIGNPART":
          if (value != null) DESIGNPART =  Convert.ToString(value);
          break;
        case "TECHNICALATTRIBUTEDEFINITION":
          if (value != null) TECHNICALATTRIBUTEDEFINITION =  Convert.ToString(value);
          break;
        case "UNITOFMEASURE":
          if (value != null) UNITOFMEASURE =  Convert.ToString(value);
          break;
        case "OBJECT":
          if (value != null) OBJECT =  Convert.ToString(value);
          break;
        case "PLANT":
          if (value != null) PLANT =  Convert.ToString(value);
          break;
        case "OBJECTTECHNICALSPECIFICATION":
          if (value != null) OBJECTTECHNICALSPECIFICATION =  Convert.ToString(value);
          break;
        case "OBJECTBILLOFMATERIAL":
          if (value != null) OBJECTBILLOFMATERIAL =  Convert.ToString(value);
          break;
        case "STANDARD":
          if (value != null) STANDARD =  Convert.ToString(value);
          break;
        case "OBJECTFUNCTION":
          if (value != null) OBJECTFUNCTION =  Convert.ToString(value);
          break;
        case "BASEUNIT":
          if (value != null) BASEUNIT =  Convert.ToString(value);
          break;
        case "OBJECTDESIGNPART":
          if (value != null) OBJECTDESIGNPART =  Convert.ToString(value);
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
