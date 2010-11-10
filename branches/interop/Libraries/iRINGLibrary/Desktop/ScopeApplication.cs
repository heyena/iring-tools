using System.Runtime.Serialization;
namespace org.iringtools.library
{
    /// <summary>
    /// This class represents an application that is using the data for a specific project
    /// </summary>
    [DataContract(Namespace = "http://www.iringtools.org/library", Name = "application")]
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
      [DataMember(Name = "description", Order = 1)]
      public string Description { get; set; }
    }
}
