using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "namespace")]
  public class Namespace
  {
  
    [DataMember(Name = "id")]
    public int Id { get; set; }
    [DataMember(Name = "uri")]
    public string Uri { get; set; }
    [DataMember(Name = "prefix")]
    public string Prefix { get; set; }
    [DataMember(Name = "iswriteable")]
    public bool IsWriteable { get; set; }
    [DataMember(Name = "description")]
    public string Description { get; set; }
    [DataMember(Name = "idgenerator")]
    public int IdGenerator { get; set; }

  }
}