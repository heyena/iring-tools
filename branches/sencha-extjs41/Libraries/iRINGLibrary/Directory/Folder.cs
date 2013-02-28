using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "folder")]
  public class Folder
  {
    [DataMember(Name = "endpoints", Order = 0, EmitDefaultValue = false)]
    public Endpoints Endpoints { get; set; }

    [DataMember(Name = "folders", Order = 1, EmitDefaultValue = false)]
    public Folders Folders { get; set; }

    [DataMember(Name = "name", Order = 2)]
    public string Name { get; set; }

    [DataMember(Name = "type", Order = 3)]
    public string Type { get; set; }

    [DataMember(Name = "description", Order = 4, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "context", Order = 5, EmitDefaultValue = false)]
    public string Context { get; set; }

    [DataMember(Name = "securityRole", Order = 6, EmitDefaultValue = false)]
    public string SecurityRole { get; set; }

    [DataMember(Name = "user", Order = 7, EmitDefaultValue = false)]
    public string User { get; set; }
  }
}