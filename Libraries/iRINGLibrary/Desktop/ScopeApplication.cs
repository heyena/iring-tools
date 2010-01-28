namespace org.iringtools.library
{
    /// <summary>
    /// This class represents an application that is using the data for a specific project
    /// </summary>
    public class ScopeApplication
    {
        private string _name;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ScopeApplication()
        {
            _name = string.Empty;
        }

        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="name">The name of the application</param>
        public ScopeApplication(string name)
        {
            _name = name;
        }

        /// <summary>
        /// The name of the application represented by this instance
        /// </summary>
        /// <returns>A string of the Name of the application</returns>
        public string Name()
        {
            return _name;
        }

    }
}
