using System.Collections.Generic;
using System.Runtime.Serialization;
using org.w3.sparql_results;
using System;

namespace org.iringtools.library
{
  [DataContract]
  public class Repository
  {
    [DataMember]
    public string name { get; set; }

    [DataMember]
    public string uri { get; set; }

    [DataMember]
    public string description { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string encryptedCredentials { get; set; }

    [DataMember]
    public bool isReadOnly { get; set; }
  }

  [CollectionDataContract(ValueName = "Query", ItemName = "QueryItem")]
  public class Queries : Dictionary<string, Query>
  { }

  [DataContract]
  public class Query
  {
    [DataMember]
    public string fileName { get; set; }

    [DataMember]
    public QueryBindings bindings { get; set; }
  }

  [CollectionDataContract]
  public class QueryBindings : List<QueryBinding>
  { }

  public class QueryBinding
  {
    [DataMember]
    public string name { get; set; }

    [DataMember]
    public SPARQLBindingType type { get; set; }
  }

  [DataContract]
  public class Entity : IComparable
  {
    [DataMember]
    public string uri { get; set; }

    [DataMember]
    public string label { get; set; }

    [DataMember]
    public string repository { get; set; }

    int IComparable.CompareTo(object obj)
    {
        Entity e = (Entity)obj;
        return string.Compare(this.label, e.label);
    }

    public static IComparer<Entity> sortAscending()
    {
        return (IComparer<Entity>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<Entity>
    {
        int IComparer<Entity>.Compare(Entity e1, Entity e2)
        {
            return string.Compare(e1.label, e2.label);
        }
    }

  }

}
