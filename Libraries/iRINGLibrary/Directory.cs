using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "directory")]
  public class Directories : List<Folder>
  {    
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "folders")]
  public class Folders : List<Folder>
  {
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "endpoints")]
  public class Endpoints : List<Endpoint>
  {
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "folder")]
  public class Folder
  {
    [DataMember(Name = "endpoints", Order = 0, EmitDefaultValue = false)]
    public Endpoints endpoints { get; set; }

    [DataMember(Name = "folders", Order = 1, EmitDefaultValue = false)]
    public Folders folders { get; set; }

    [DataMember(Name = "name", Order = 2)]
    public string Name { get; set; }

    [DataMember(Name = "type", Order = 3)]
    public string type { get; set; }

    [DataMember(Name = "description", Order = 4, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "context", Order = 5, EmitDefaultValue = false)]
    public string context { get; set; }

    [DataMember(Name = "securityRole", Order = 6, EmitDefaultValue = false)]
    public string securityRole { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "endpoint")]
  public class Endpoint
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }   

    [DataMember(Name = "type", Order = 1, EmitDefaultValue = false)]
    public string type { get; set; }

    [DataMember(Name = "description", Order = 2, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "context", Order = 3, EmitDefaultValue = false)]
    public string context { get; set; }

    [DataMember(Name = "baseUrl", Order = 4, EmitDefaultValue = false)]
    public string baseUrl { get; set; }

    [DataMember(Name = "securityRole", Order = 5, EmitDefaultValue = false)]
    public string securityRole { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory")]
  public enum NodeIconCls
  {
    [EnumMember]
    @folder,
    [EnumMember]
    @project,
    [EnumMember]
    @application,
    [EnumMember]
    @resource,
    [EnumMember]
    @scope,
    [EnumMember]
    @key,
    [EnumMember]
    @property,
    [EnumMember]
    @relation
  }

}
