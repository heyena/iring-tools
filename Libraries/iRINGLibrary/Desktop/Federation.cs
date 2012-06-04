using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "federation")]
  public class Federation
  {
    [DataMember(Name = "idgeneratorlist")]
    public IDGenerators IDGenerators { get; set; }
    [DataMember(Name = "namespacelist")]
    public Namespaces Namespaces { get; set; }
    [DataMember(Name = "repositorylist")]
    public Repositories Repositories { get; set; }

    public Federation()
    {
      IDGenerators = new IDGenerators();
      Namespaces = new Namespaces();
      Repositories = new Repositories();
    }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "idgenerators")]
  public class IDGenerators : List<IDGenerator>
  {
    [DataMember(Name = "sequenceid")]
    public int SequenceId { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "idgenerator")]
  public class IDGenerator
  {
    [DataMember(Name = "id")]
    public string Id { get; set; }
    [DataMember(Name = "uri")]
    public string Uri { get; set; }
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "description")]
    public string Description { get; set; }
  }
 
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "repositories")]
  public class Repositories : List<Repository>
  {
    [DataMember(Name = "sequenceid")]
    public int SequenceId { get; set; }

  }

  /// <remarks/>
 [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repository")]
 public class Repository
  {
   public Repository()
   {
     Namespaces = new Namespaces();
   }
   [DataMember(Name="id")]
   public string Id { get; set; }
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
   public Namespaces Namespaces { get; set; }
   [DataMember(Name = "encryptedcredentials")]
   public string EncryptedCredentials { get; set; }
  }

  /// <remarks/>
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repositoryType")]
  public enum RepositoryType
  {
    [EnumMember]
    Part8,
    [EnumMember]
    RDSWIP,
    [EnumMember]
    Camelot,
    [EnumMember]
    JORD,
  }

  /// <remarks/>
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "namespaces")]
  public class Namespaces : List<Namespace>
  {
    [DataMember(Name = "sequenceid")]
    public int SequenceId { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "namespace")]
  public class Namespace
  {
    public Namespace()
    {
      IdGenerator = new IDGenerator();
    }
    [DataMember(Name = "id")]
    public string Id { get; set; }
    [DataMember(Name = "uri")]
    public string Uri { get; set; }
    [DataMember(Name = "alias")]
    public string Alias { get; set; }
    [DataMember(Name = "iswriteable")]
    public bool IsWriteable { get; set; }
    [DataMember(Name = "description")]
    public string Description { get; set; }
    [DataMember(Name = "idgenerator")]
    public IDGenerator IdGenerator { get; set; }

  }
}
