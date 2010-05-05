using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    /// <summary>
    /// This class represents a node in the Project.xml file with
    /// specific attention paid to the parent/child relationship between projects and applications
    /// </summary>
   [DataContract]
   public class ScopeProject
    {
       /// <summary>
       /// The name of the project
       /// </summary>
       /// <returns>a string</returns>
       [DataMember(Order=0)]
       public string Name { get; set; }
     
       /// <summary>
       /// The description of the project
       /// </summary>
       /// <returns>a string</returns>
       [DataMember(Order=1)]
       public string Description { get; set; }

       /// <summary>
       /// The collection of associated ScopeApplications
       /// </summary>
       /// <returns>A strongly type List of ScopeApplication objects</returns>
       [DataMember(Order = 2)]
       public List<ScopeApplication> Applications { get; set; }
    }
}
