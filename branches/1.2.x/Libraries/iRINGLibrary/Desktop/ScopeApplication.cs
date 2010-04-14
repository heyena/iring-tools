using System.Runtime.Serialization;
namespace org.iringtools.library
{
    /// <summary>
    /// This class represents an application that is using the data for a specific project
    /// </summary>
    [DataContract]
    public class ScopeApplication
    {
      /// <summary>
      /// The name of the application represented by this instance
      /// </summary>
      /// <returns>A string of the Name of the application</returns>
      [DataMember(Order = 0)]
      public string Name { get; set; }

      /// <summary>
      /// The description of the application represented by this instance
      /// </summary>
      /// <returns>A string of the Description of the application</returns>
      [DataMember(Order = 1)]
      public string Description { get; set; }

      /// <summary>
      /// The flag indicating whether dto code has been generated
      /// </summary>
      /// <returns>true if dto code has been generated</returns>
      [DataMember(Order = 2)]
      public bool hasDTOLayer { get; set; }
    }
}
