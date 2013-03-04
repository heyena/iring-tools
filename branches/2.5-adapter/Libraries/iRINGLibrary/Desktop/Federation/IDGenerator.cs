using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "idgenerator")]
  public class IdGenerator
  {
    [DataMember(Name = "id")]
    public int Id { get; set; }
    [DataMember(Name = "uri")]
    public string Uri { get; set; }
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "description")]
    public string Description { get; set; }
  }
}