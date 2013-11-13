using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Xml.Linq;

namespace org.iringtools.library
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
    public ScopeProject()
    {
      Applications = new ScopeApplications();
    }

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

    [DataMember(Name = "displayName", Order = 3, EmitDefaultValue = false)]
    public string DisplayName { get; set; }

    [DataMember(Name = "configuration", Order = 4, EmitDefaultValue = false)]
    public Configuration Configuration { get; set; }

    [DataMember(Name = "permissionGroup", Order = 5, EmitDefaultValue = false)]
    public PermissionGroups PermissionGroup { get; set; }
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
    /// The assembly of the application represented by this instance
    /// </summary>
    /// <returns>A string of the assembly of the application</returns>
    [DataMember(Name = "assembly", Order = 2, EmitDefaultValue = false)]
    public string Assembly { get; set; }

    [DataMember(Name = "configuration", Order = 3, EmitDefaultValue = false)]
    public Configuration Configuration { get; set; }

    [DataMember(Name = "displayName", Order = 4, EmitDefaultValue = false)]
    public string DisplayName { get; set; }

    [DataMember(Name = "dataMode", Order = 5, EmitDefaultValue = false)]
    public DataMode DataMode { get; set; }

    [DataMember(Name = "cacheInfo", Order = 6, EmitDefaultValue = false)]
    public CacheInfo CacheInfo { get; set; }

    [DataMember(Name = "permissionGroup", Order = 7, EmitDefaultValue = false)]
    public PermissionGroups PermissionGroup { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "cacheInfo")]
  public class CacheInfo
  {
    [DataMember(Name = "importURI", Order = 0, EmitDefaultValue = false)]
    public string ImportURI { get; set; }

    [DataMember(Name = "timeout", Order = 1, EmitDefaultValue = false)]
    public long Timeout { get; set; }
    
    [DataMember(Name = "cacheEntries", Order = 2, EmitDefaultValue = false)]
    public CacheEntries CacheEntries { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "cacheEntries")]
  public class CacheEntries : List<CacheEntry> { }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "cacheEntry")]
  public class CacheEntry
  {
    [DataMember(Name = "objectName", Order = 0)]
    public string ObjectName { get; set; }

    [DataMember(Name = "lastUpdate", Order = 1, EmitDefaultValue = false)]
    public DateTime? LastUpdate { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library")]
  public enum DataMode 
  { 
    [EnumMember]
    Live, 
    [EnumMember]
    Cache
  }

  /// <summary>
  /// This class represents a permission group which is managed by Ldap for UI security.
  /// </summary>
  //[DataContract(Name = "permissionGroup", Namespace = "http://www.iringtools.org/library")]
  [CollectionDataContract(Name = "permissionGroup", Namespace = "http://www.iringtools.org/library", ItemName = "group")]
  public class PermissionGroups : List<String>
  {

  }
}
