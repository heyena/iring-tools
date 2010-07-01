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
using org.iringtools.nhibernate;

namespace org.iringtools.nhibernate
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Service : IService
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Service));
        private NHibernateProvider _nhibernateProvider = null;
       

        public Service()
        {
            _nhibernateProvider = new NHibernateProvider(WebConfigurationManager.AppSettings);
        }

        public DatabaseDictionary GetDbDictionary(string project, string application)
        {
            return _nhibernateProvider.GetDbDictionary(project, application);
        }

        public Response SaveDatabaseDictionary(string project, string application, DatabaseDictionary dict)
        {
            return _nhibernateProvider.SaveDatabaseDictionary(project, application, dict);
        }

        public DatabaseDictionary GetDatabaseSchema(Request request)
        {
            return _nhibernateProvider.GetDatabaseSchema(request);
        }

       
        public List<string> GetExistingDbDictionaryFiles()
        {
            return _nhibernateProvider.GetExistingDbDictionaryFiles();
        }

        public String[] GetProviders()
        {
            return _nhibernateProvider.GetProviders();
        }
    }
}
