using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  [Serializable]
  public enum RoleType
  {
    Property,
    Possessor,
    Reference,
    FixedValue,
    DataProperty,
    ObjectProperty
  }

  [Serializable]
  public enum TemplateType
  {
    Qualification,
    Definition
  }

  [Serializable]
  public class RoleMap
  {
    public RoleType Type { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string DataType { get; set; }
    public string Value { get; set; }
    public string PropertyName { get; set; }
    public string ValueListName { get; set; }
    public ClassMap ClassMap { get; set; }
  }

  [Serializable]
  public class ClassMap
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string IdentifierDelimiter { get; set; }
    public LazyList<string> Identifiers { get; set; }
    public string IdentifierValue { get; set; }
  }

  [Serializable]
  public class TemplateMap
  {
    public string Id { get; set; }
    public TemplateType Type { get; set; }
    public string Name { get; set; }
    public LazyList<RoleMap> RoleMaps { get; set; }
  }

  [Serializable]
  public class ClassTemplateMap
  {
    public ClassMap ClassMap { get; set; }
    public LazyList<TemplateMap> TemplateMaps { get; set; }
  }

  [Serializable]
  public class GraphMap
  {
    public string Name { get; set; }
    public LazyList<ClassTemplateMap> ClassTemplateMaps { get; set; }
    public string DictionaryObjectName { get; set; }
  }

  [Serializable]
  public class ValueListMap
  {
    public string Name { get; set; }
    public LazyList<ValueMap> ValueMaps { get; set; }
  }

  [Serializable]
  public class ValueMap
  {
    public string InternalValue { get; set; }
    public string Uri { get; set; }
  }

  [Serializable]
  public class Mapping
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public LazyList<GraphMap> GraphMaps { get; set; }
    public LazyList<ValueListMap> ValueListMaps { get; set; }
    public string Version { get; set; }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is Mapping)
      {
        Mapping compareTo = (Mapping)obj;
        return compareTo.Id == this.Id;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.Id.ToString();
    }

    public override int GetHashCode()
    {
      return this.Id.GetHashCode();
    }
    #endregion
  }
}
