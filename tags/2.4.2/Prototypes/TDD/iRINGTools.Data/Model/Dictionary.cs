using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  [Serializable]
  public enum DataType
  {
    @Boolean,
    @Byte,
    @Char,
    @DateTime,
    @Decimal,
    @Double,
    @Int16,
    @Int32,
    @Int64,
    @Single,
    @String,
    @TimeSpan
  }

  [Serializable]
  public enum KeyType
  {
    Unassigned,
    Assigned,
    Foreign,
    Identity,
    Sequence
  }

  [Serializable]
  public enum RelationshipType
  {
    OneToOne,
    OneToMany,
    ManyToOne,
    ManyToMany
  }

  [Serializable]
  public class DataProperty
  {
    public string ColumnName { get; set; }
    public string PropertyName { get; set; }
    public DataType DataType { get; set; }
    public int DataLength { get; set; }
    public bool IsNullable { get; set; }
    public KeyType KeyType { get; set; }
    public bool ShowOnIndex { get; set; }
    public int NumberOfDecimals { get; set; }

    public DataProperty()
    {
    }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is DataProperty)
      {
        DataProperty compareTo = (DataProperty)obj;
        return compareTo.PropertyName == this.PropertyName;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.PropertyName;
    }
    public override int GetHashCode()
    {
      return this.PropertyName.GetHashCode();
    }
    #endregion
  }

  [Serializable]
  public class DataRelationship
  {
    public IList<PropertyMap> PropertyMaps { get; set; }
    public string RelatedObjectName { get; set; }
    public string RelationshipName { get; set; }
    public RelationshipType RelationshipType { get; set; }

    public DataRelationship()
    {
    }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is DataRelationship)
      {
        DataRelationship compareTo = (DataRelationship)obj;
        return compareTo.RelationshipName == this.RelationshipName;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.RelationshipName;
    }
    public override int GetHashCode()
    {
      return this.RelationshipName.GetHashCode();
    }
    #endregion
  }

  [Serializable]
  public class DictionaryObject
  {
    public string TableName { get; set; }
    public string ObjectNamespace { get; set; }
    public string ObjectName { get; set; }
    public string KeyDelimeter { get; set; }
    public IList<KeyProperty> KeyProperties { get; set; }
    public IList<DataProperty> DataProperties { get; set; }
    public IList<DataRelationship> DataRelationships { get; set; }

    public DictionaryObject()
    {
    }

    public bool isKeyProperty(string propertyName)
    {
      foreach (KeyProperty keyProperty in KeyProperties)
      {
        if (keyProperty.KeyPropertyName.ToLower() == propertyName.ToLower())
          return true;
      }

      return false;
    }

    public DataProperty GetKeyPropertyByName(string name)
    {
      return DataProperties.SingleOrDefault(c => c.PropertyName == name);
    }

    public bool RemoveProperty(DataProperty dataProperty)
    {
      foreach (var property in DataProperties)
      {
        if (dataProperty == property)
        {
          DataProperties.Remove(dataProperty);
          break;
        }
      }
      foreach (KeyProperty keyProperty in KeyProperties)
      {
        if (keyProperty.KeyPropertyName.ToLower() == dataProperty.PropertyName.ToLower())
        {
          KeyProperties.Remove(keyProperty);
          break;
        }
      }
      return true;
    }

    public bool AddKeyProperty(DataProperty keyProperty)
    {
      this.KeyProperties.Add(new KeyProperty { KeyPropertyName = keyProperty.PropertyName });
      this.DataProperties.Add(keyProperty);
      return true;
    }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is DictionaryObject)
      {
        DictionaryObject compareTo = (DictionaryObject)obj;
        return compareTo.ObjectName == this.ObjectName;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.ObjectName;
    }
    public override int GetHashCode()
    {
      return this.ObjectName.GetHashCode();
    }
    #endregion
  }

  [Serializable]
  public class KeyProperty
  {
    public string KeyPropertyName { get; set; }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is KeyProperty)
      {
        KeyProperty compareTo = (KeyProperty)obj;
        return compareTo.KeyPropertyName == this.KeyPropertyName;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.KeyPropertyName;
    }
    public override int GetHashCode()
    {
      return this.KeyPropertyName.GetHashCode();
    }
    #endregion
  }

  [Serializable]
  public class PropertyMap
  {
    public string DataPropertyName { get; set; }
    public string RelatedPropertyName { get; set; }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is PropertyMap)
      {
        PropertyMap compareTo = (PropertyMap)obj;
        return compareTo.DataPropertyName == this.DataPropertyName;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.DataPropertyName;
    }
    public override int GetHashCode()
    {
      return this.DataPropertyName.GetHashCode();
    }
    #endregion
  }

  [Serializable]
  public class Dictionary
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<DictionaryObject> DictionaryObjects { get; set; }

    public Dictionary()
    {
    }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is Dictionary)
      {
        Dictionary compareTo = (Dictionary)obj;
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
