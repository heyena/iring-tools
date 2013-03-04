using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace org.iringtools.refdata.federation
{
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "federation")]
  public class Federation
  {
    [DataMember(Name = "idgeneratorlist")]
    public IdGenerators IdGenerators { get; set; }
    [DataMember(Name = "namespacelist")]
    public Namespaces Namespaces { get; set; }
    [DataMember(Name = "repositorylist")]
    public Repositories Repositories { get; set; }

    public Federation()
    {
      IdGenerators = new IdGenerators();
      Namespaces = new Namespaces();
      Repositories = new Repositories();
    }
  }
}
