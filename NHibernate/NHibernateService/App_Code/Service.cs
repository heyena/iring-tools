using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web.Configuration;
using org.iringtools.utility;
using org.iringtools.library;
using NHibernate;
using log4net;
using System.ServiceModel.Activation;
using org.iringtools.application;

namespace org.iringtools.application
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Service : IService
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Service));
        private ApplicationProvider _applicationProvider = null;
       

        public Service()
        {
            _applicationProvider = new ApplicationProvider(WebConfigurationManager.AppSettings);
        }

        public DatabaseDictionary GetDictionary(string project, string application)
        {
            return _applicationProvider.GetDictionary(project, application);
        }

        public Response PostDictionary(string project, string application, DatabaseDictionary dict)
        {
            return _applicationProvider.PostDictionary(project, application, dict);
        }

        public Response Generate(string project, string application)
        {
            return _applicationProvider.Generate(project, application);
        }

        public DatabaseDictionary GetDatabaseSchema(string project, string application)
        {
            return _applicationProvider.GetDatabaseSchema(project, application);
        }
        
        public String[] GetProviders()
        {
            return _applicationProvider.GetProviders();
        }        

    }
}
