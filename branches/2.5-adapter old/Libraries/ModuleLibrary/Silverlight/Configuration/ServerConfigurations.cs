using System;
using System.Collections.Generic;
using org.iringtools.library.presentation;
using org.iringtools.library.presentation.configuration;
using Microsoft.Practices.Unity;

namespace org.iringtools.library.configuration
{
    /// <summary>
    /// Server configuration - values are set in Web.Config 
    /// and Default.aspx code behind file
    /// </summary>
    public class ServerConfiguration : AppBase, IServerConfiguration
    {
        private IDictionary<string, string> constructorParam;
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
        public ServerConfiguration(IUnityContainer container) : base(container)
        {
            try
            {
                InitParam = new Dictionary<string, string>();
                constructorParam = Container.Resolve<IDictionary<string, string>>("InitParameters");
                string paraString = constructorParam["InitParameters"];

               string[] paraList = paraString.Split('~');
                foreach (string para in paraList)
                {
                    string[] keyValue = para.Split('#');
                    InitParam.Add(new
                        KeyValuePair<string, string>(keyValue[0], keyValue[1]));
                }
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        public string AdapterServiceUri
        {
          get { return GetParameter("AdapterServiceUri"); }
        }

        public string FacadeServiceUri
        {
            get { return GetParameter("FacadeServiceUri"); }
        }

        public string BaseAddress
        {
            get { return GetParameter("BaseAddress"); }
        }
        public string ReferenceDataServiceUri
        {
          get { return GetParameter("ReferenceDataServiceUri"); }
        }


        /// <summary>
        /// Gets the adapter service WCF.
        /// </summary>
        /// <value>The adapter service WCF.</value>
        public string AdapterProxy
        {
            get { return GetParameter("AdapterProxy"); }
        }

        /// <summary>
        /// Gets the ontology service WCF.
        /// </summary>
        /// <value>The ontology service WCF.</value>
        public string OntologyServiceWCF
        {
            get { return GetParameter("OntologyServiceWCF"); }
        }


        /// <summary>
        /// Gets the web server URL.
        /// </summary>
        /// <value>The web server URL.</value>
        public string WebServerURL
        {
            get { return GetParameter("WebServerURL"); }
        }

        /// <summary>
        /// Gets the web server path.
        /// </summary>
        /// <value>The web server path.</value>
        public string WebServerPath
        {
            get { return GetParameter("WebServerPath"); }
        }


        /// <summary>
        /// Gets the name of the web site.
        /// </summary>
        /// <value>The name of the web site.</value>
        public string WebSiteName
        {
            get { return GetParameter("WebSiteName"); }
        }

    }
}
