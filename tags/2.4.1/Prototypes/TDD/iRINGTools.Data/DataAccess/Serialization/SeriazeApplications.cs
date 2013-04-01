using System.Collections.Generic;
using System.Runtime.Serialization;

namespace iRINGTools.Data.Serliaze
{
  [CollectionDataContract(Name = "scopes", Namespace = "http://www.iringtools.org/library", ItemName = "scope")]
  public class ScopeProjects : List<ScopeProject>
  {
  }

  [CollectionDataContract(Name = "applications", Namespace = "http://www.iringtools.org/library", ItemName = "application")]
  public class ScopeApplications : List<ScopeApplication>
  {
  }

  /// <summary>
  /// This class represents a node in the Project.xml file with
  /// specific attention paid to the parent/child relationship between projects and applications
  /// </summary>
  [DataContract(Name = "scope", Namespace = "http://www.iringtools.org/library")]
  public class ScopeProject
  {
    /// <summary>
    /// The name of the project
    /// </summary>
    /// <returns>a string</returns>
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    /// <summary>
    /// The description of the project
    /// </summary>
    /// <returns>a string</returns>
    [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
    public string Description { get; set; }

    /// <summary>
    /// The collection of associated ScopeApplications
    /// </summary>
    /// <returns>A strongly type List of ScopeApplication objects</returns>
    [DataMember(Name = "applications", Order = 2)]
    public ScopeApplications Applications { get; set; }
  }

  [DataContract(Name = "application", Namespace = "http://www.iringtools.org/library")]
  public class ScopeApplication
  {
    /// <summary>
    /// The name of the application represented by this instance
    /// </summary>
    /// <returns>A string of the Name of the application</returns>
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    /// <summary>
    /// The description of the application represented by this instance
    /// </summary>
    /// <returns>A string of the Description of the application</returns>
    [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
    public string Description { get; set; }

    /// <summary>
    /// The description of the application represented by this instance
    /// </summary>
    /// <returns>A string of the Description of the application</returns>
    [DataMember(Name = "dataLayerName", Order = 2, EmitDefaultValue = false)]
    public string DataLayerName { get; set; }
  }
}
