using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  /// <remarks/>
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repository")]
  public class Repository
  {
    public Repository()
    {
      Namespaces = new List<Namespace>();
    }
    [DataMember(Name="id")]
    public int Id { get; set; }
    [DataMember(Name = "description")]
    public string Description { get; set; }
    [DataMember(Name = "isreadonly")]
    public bool IsReadOnly { get; set; }
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "repositorytype")]
    public RepositoryType RepositoryType { get; set; }
    [DataMember(Name = "uri")]
    public string Uri { get; set; }
    [DataMember(Name = "updateuri")]
    public string UpdateUri { get; set; }
    [DataMember(Name = "namespaces")]
    public List<Namespace> Namespaces { get; set; }
    [DataMember(Name = "encryptedcredentials")]
    public string EncryptedCredentials { get; set; }
  }
}