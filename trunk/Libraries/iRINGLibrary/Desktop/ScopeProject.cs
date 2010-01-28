using System.Collections.Generic;

namespace org.iringtools.library
{
    /// <summary>
    /// This class represents a node in the Project.xml file with
    /// specific attention paid to the parent/child relationship between projects and applications
    /// </summary>
   public class ScopeProject
    {
        private string _name;
        private List<ScopeApplication> _applications;

       /// <summary>
       /// Default constructor
       /// </summary>
        public ScopeProject()
        {
            _name = string.Empty;
            List<ScopeApplication> _applications = new List<ScopeApplication>();
        }

       /// <summary>
       /// Overloaded constructor allowing the project name to be set at instantitaion
       /// </summary>
       /// <param name="name">The name of the project</param>
        public ScopeProject(string name)
        {
            _name = name;
            List<ScopeApplication> _applications = new List<ScopeApplication>();
        }

       /// <summary>
       /// The name of the project
       /// </summary>
       /// <returns>a string</returns>
        public string Name()
        {
            return _name;
        }

       /// <summary>
       /// Public method to add a new application to the Applications collection
       /// </summary>
       /// <param name="application">A populated ScopeApplication object</param>
        public void AddApplication(ScopeApplication application)
        {
            _applications.Add(application);
        }

       /// <summary>
       /// The collection of associated ScopeApplications
       /// </summary>
       /// <returns>A strongly type List of ScopeApplication objects</returns>
        public List<ScopeApplication> Applications()
        {
            return _applications;
        }
    }
}
