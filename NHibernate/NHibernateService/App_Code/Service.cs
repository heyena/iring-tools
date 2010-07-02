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

        public DatabaseDictionary GetDbDictionary(string project, string application)
        {
            return _applicationProvider.GetDbDictionary(project, application);
        }

        public Response SaveDatabaseDictionary(string project, string application, DatabaseDictionary dict)
        {
            return _applicationProvider.SaveDatabaseDictionary(project, application, dict);
        }

        public Response UpdateDatabaseDictionary(string project, string application, DatabaseDictionary dict)
        {
          return _applicationProvider.UpdateDatabaseDictionary(project, application, dict);
        }

        public DatabaseDictionary GetDatabaseSchema(Request request)
        {
            return _applicationProvider.GetDatabaseSchema(request);
        }

       
        public List<string> GetExistingDbDictionaryFiles()
        {
            return _applicationProvider.GetExistingDbDictionaryFiles();
        }

        public String[] GetProviders()
        {
            return _applicationProvider.GetProviders();
        }
    }
}
