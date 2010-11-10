﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
    /// <summary>
    /// This class represents a node in the Project.xml file with
    /// specific attention paid to the parent/child relationship between projects and applications
    /// </summary>
   [DataContract(Namespace = "http://www.iringtools.org/library", Name = "project")]
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
       [DataMember(Name = "description", Order = 1)]
       public string Description { get; set; }

       /// <summary>
       /// The collection of associated ScopeApplications
       /// </summary>
       /// <returns>A strongly type List of ScopeApplication objects</returns>
       [DataMember(Name = "applications", Order = 2)]
       public List<ScopeApplication> Applications { get; set; }
    }
}
