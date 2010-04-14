using System.Collections.Generic;

namespace org.iringtools.library
{
    /// <summary>
    /// This class represents a node in the Project.xml file with
    /// specific attention paid to the parent/child relationship between projects and applications
    /// </summary>
   public class IntegrationProject
    {
        private string _name;
        private List<IntegrationApplication> _applications;

       /// <summary>
       /// Default constructor
       /// </summary>
        public IntegrationProject()
        {
            _name = string.Empty;
            List<IntegrationApplication> _applications = new List<IntegrationApplication>();
        }

       /// <summary>
       /// Overloaded constructor allowing the project name to be set at instantitaion
       /// </summary>
       /// <param name="name">The name of the project</param>
        public IntegrationProject(string name)
        {
            _name = name;
            List<IntegrationApplication> _applications = new List<IntegrationApplication>();
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
       /// <param name="application">A populated IntegrationApplication object</param>
        public void AddApplication(IntegrationApplication application)
        {
            _applications.Add(application);
        }

       /// <summary>
       /// The collection of associated IntegrationApplications
       /// </summary>
       /// <returns>A strongly type List of IntegrationApplication objects</returns>
        public List<IntegrationApplication> Applications()
        {
            return _applications;
        }
    }
}
