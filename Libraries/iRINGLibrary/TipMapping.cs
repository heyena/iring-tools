using System.Runtime.Serialization;
using System.Collections.Generic;
using org.iringtools.library;
using System;

namespace org.iringtools.library
{
  [Serializable]
  [DataContractAttribute(Name = "tipMapping", Namespace = "http://www.iringtools.org/tipmapping")]
  public partial class TipMapping
  {
    public TipMapping()
    {
      tipMaps = new TipMaps();
    }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public TipMaps tipMaps { get; set; }

    public TipMap FindTipMap(string name)
    {
      TipMap tip = null;
      foreach (TipMap item in this.tipMaps)
      {
        if (item.graphName.ToLower() == name.ToLower())
        {
          tip = item;
          break;
        }
      }
      return tip;
    }
  }
  
  [Serializable]
  [CollectionDataContractAttribute(Name = "tipMaps", Namespace = "http://www.iringtools.org/tipmapping", ItemName = "tipMap")]
  public class TipMaps : List<TipMap>
  {    
  }
  

  [Serializable]
  [DataContractAttribute(Name = "tipMap", Namespace = "http://www.iringtools.org/tipmapping")]
  public partial class TipMap
  {

    public TipMap()
    {
        parameterMaps = new ParameterMaps();
    }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string id { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string name { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string description { get; set; }

    [DataMemberAttribute(EmitDefaultValue = true, Order = 3)]
    public string dataObjectName { get; set; }

    [DataMemberAttribute(EmitDefaultValue = true, Order = 4)]
    public string graphName { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 5)]
    public ParameterMaps parameterMaps { get; set; }
  }

  [Serializable]
  [DataContractAttribute(Name = "tipRequest", Namespace = "http://www.iringtools.org/tipmapping")]
  public class TipRequest
  {
      public TipRequest()
      {
          parameterMaps = new ParameterMaps();
      }

      [DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
      public ParameterMaps parameterMaps { get; set; }
  }
      
  [Serializable]
  [CollectionDataContractAttribute(Name = "parameterMaps", Namespace = "http://www.iringtools.org/tipmapping", ItemName = "parameterMap")]
  public class ParameterMaps : List<ParameterMap>
  {
  }


  [Serializable]
  [DataContractAttribute(Name = "parameterMap", Namespace = "http://www.iringtools.org/tipmapping")]
  public partial class ParameterMap
  {

    [DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string path { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string id { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string name { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public string tipId { get; set; }

    //[DataMemberAttribute(EmitDefaultValue = false, Order = 4)]
    //public string isMapped { get; set; }

    [DataMemberAttribute(EmitDefaultValue = true, Order = 4)]
    public string dataPropertyName { get; set; }

  }
}

