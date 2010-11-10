using System.Collections.Generic;
using System.Runtime.Serialization;
using org.w3.sparql_results;
using System;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repository")]
  public class Repository
  {
    [DataMember(Name="name")]
    public string Name { get; set; }

    [DataMember(Name="uri")]
    public string Uri { get; set; }

    [DataMember(Name="updateUri", EmitDefaultValue=false)]
    public string UpdateUri { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }

    [DataMember(Name = "encryptedCredentials", EmitDefaultValue = false)]
    public string EncryptedCredentials { get; set; }

    [DataMember(Name = "isReadOnly")]
    public bool IsReadOnly { get; set; }

    [DataMember(Name = "repositoryType")]
    public RepositoryType RepositoryType { get; set; }

  }


  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repositoryType")]
  public enum RepositoryType
  {
    [EnumMember]
    RDSWIP,
    [EnumMember]
    Camelot,
    [EnumMember]
    Part8,
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "queries", ItemName = "queryItem")]
  public class Queries : Dictionary<string, Query>
  { }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "query")]
  public class Query
  {
    [DataMember(Name="fileName")]
    public string FileName { get; set; }

    [DataMember(Name="bindings")]
    public QueryBindings Bindings { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "queryBindings", ItemName = "queryBinding")]
  public class QueryBindings : List<QueryBinding>
  { }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "queryBinding")]
  public class QueryBinding
  {
    [DataMember(Name="name")]
    public string Name { get; set; }

    [DataMember(Name="type")]
    public SPARQLBindingType Type { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "entity")]
  public class Entity
  {
    [DataMember(Name="uri")]
    public string Uri { get; set; }

    [DataMember(Name="label")]
    public string Label { get; set; }

    [DataMember(Name="repository")]
    public string Repository { get; set; }

    public static IComparer<Entity> sortAscending()
    {
        return (IComparer<Entity>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<Entity>
    {
        int IComparer<Entity>.Compare(Entity e1, Entity e2)
        {
            return string.Compare(e1.Label, e2.Label);
        }
    }
  }

}
