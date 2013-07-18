using System;
using System.Collections.Generic;
using org.iringtools.library.presentation.configuration;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using PrismContrib.Errors;

namespace org.iringtools.library.configuration
{
    /// <summary>
    /// Server configuration - values are set in Web.Config 
    /// and Default.aspx code behind file
    /// </summary>
    public class AppConfiguration : IAppConfiguration
    {
        // Dependencies (need to be registered)
        private IUnityContainer container = null;
        //private ILoggerFacade logger = null;
        //private IError error = null;

        private IDictionary<string, string> constructorParam;

        /// <summary>
        /// Gets or sets the init param.
        /// </summary>
        /// <value>The init param.</value>
        public IDictionary<string, string> InitParam { get; set; }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetParameter(string key)
        {
            if (InitParam.ContainsKey(key))
                return InitParam[key];
            else
                return "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConfiguration"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public AppConfiguration(IUnityContainer container, IError error)
        {
            this.container = container;

            try
            {
                InitParam = new Dictionary<string, string>();

                // Expects an InitParameters dictionary object to be registered 
                // in the container
                constructorParam = container.Resolve<IDictionary<string, string>>("InitParameters");
                
                // Within the dictionary we are only interested in the
                // InitParameters key (same name) - it contains our configuration
                string paraString = constructorParam["InitParameters"];

                // Each parameter will be delimited by a tilde (uncommon character)
                string[] paraList = paraString.Split('~');

                // Each key/value pair will be delimited by a pound sign (uncommon character)
                foreach (string para in paraList)
                {
                    string[] keyValue = para.Split('#');
                    InitParam.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
                }
            }
            catch (Exception ex)
            {
                error.SetError(ex);
            }
        }


    }
}
