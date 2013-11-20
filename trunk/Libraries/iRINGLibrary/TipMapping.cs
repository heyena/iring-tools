using System.Runtime.Serialization;
using System.Collections.Generic;
using org.iringtools.library;
using System;

namespace org.iringtools.library.tip
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
        identifiers = new Identifiers();
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
    public string identifierDelimiter { get; set; }

    [DataMember(EmitDefaultValue = true, Order = 5)]
    public Identifiers identifiers { get; set; }

    [DataMemberAttribute(EmitDefaultValue = true, Order = 6)]
    public string graphName { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 7)]
    public ParameterMaps parameterMaps { get; set; }
  }

  [CollectionDataContractAttribute(Name = "identifiers", Namespace = "http://www.iringtools.org/tipmapping", ItemName = "identifier")]
  public class Identifiers : System.Collections.Generic.List<string>
  {
  }

  [Serializable]
  [DataContractAttribute(Name = "tipRequest", Namespace = "http://www.iringtools.org/tipmapping")]
  public class TipRequest
  {
      public TipRequest()
      {
          identifiers = new Identifiers();
          parameterMaps = new ParameterMaps();
      }

      [DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
      public ParameterMaps parameterMaps { get; set; }

      [DataMember(EmitDefaultValue = true, Order = 2)]
      public Identifiers identifiers { get; set; }
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

      public ParameterMap()
      {
          identifiers = new Identifiers();
          valueList = new ValueList();
      }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string path { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string id { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string name { get; set; }

    [DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public string tipId { get; set; }

    [DataMember(EmitDefaultValue = true, Order = 4)]
    public Identifiers identifiers { get; set; }

    [DataMemberAttribute(EmitDefaultValue = true, Order = 5)]
    public string dataPropertyName { get; set; }

    [DataMemberAttribute(EmitDefaultValue = true, Order = 6)]
    public ValueList valueList { get; set; }

  }


  [Serializable]
  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "values", Namespace = "http://www.iringtools.org/tipmapping", ItemName = "value")]
  public class ValueCollection : System.Collections.Generic.List<ValueItem>
  {
  }

  [Serializable]
  [System.Runtime.Serialization.DataContractAttribute(Name = "value", Namespace = "http://www.iringtools.org/tipmapping")]
  public partial class ValueItem
  {
      [DataMember(Name = "internalValue", EmitDefaultValue = false, Order = 0)]
      public string internalValue { get; set; }

      [DataMember(Name = "value", Order = 1)]
      public string value { get; set; }

      [DataMember(Name = "uri", Order = 2)]
      public string uri { get; set; }

      [DataMember(Name = "label", Order = 3)]
      public string label { get; set; }
  }

  [Serializable]
  [DataContract(Namespace = "http://www.iringtools.org/tipmapping", Name = "valueList")]
  public class ValueList
  {
      [DataMember(Name = "name", Order = 0)]
      public string name { get; set; }

      [DataMember(Name = "values", Order = 1)]
      public ValueCollection values { get; set; }

      public ValueList()
      {
          values = new ValueCollection();
      }
  }

  
  [DataContract(Namespace = "http://www.iringtools.org/tipmapping", Name = "valueListWithParameter")]
  public class ValueListWithParameter
  {
      [DataMember(Name = "parameterName", Order = 0)]
      public string parameterName { get; set; }

      [DataMember(Name = "valueList", Order = 1)]
      public ValueList valueList { get; set; }
  }
}

