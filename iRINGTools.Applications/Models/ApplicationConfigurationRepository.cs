using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.applicationConfig;
using org.iringtools.utility;
using library = org.iringtools.library;
using log4net;
using System.Net;
using System.Collections.Specialized;
using System.Configuration;

namespace iRINGTools.Web.Models
{
    internal class ApplicationConfigurationRepository : IApplicationConfigurationRepository
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ApplicationConfigurationRepository));
        private library.CustomError _CustomError = null;
        protected library.ServiceSettings _settings;
        protected string _proxyHost;
        protected string _proxyPort;
        //AdapterRepository adapter;

        string applicationConfigurationServiceUri = null;

        public ApplicationConfigurationRepository()
        {
            NameValueCollection settings = ConfigurationManager.AppSettings;

            _settings = new library.ServiceSettings();
            _settings.AppendSettings(settings);

            _proxyHost = _settings["ProxyHost"];
            _proxyPort = _settings["ProxyPort"];

            applicationConfigurationServiceUri = _settings["ApplicationConfigServiceUri"];
            if (applicationConfigurationServiceUri.EndsWith("/"))
                applicationConfigurationServiceUri = applicationConfigurationServiceUri.Remove(applicationConfigurationServiceUri.Length - 1);


        }

        public Folders GetFolders(string userName, int siteId, int platformId, Guid parentFolderId)
        {
            Folders folders = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                folders = client.Get<Folders>(String.Format("/folders/{0}?siteId={1}&platformId={2}&parentFolderId={3}&format=xml", userName, siteId, platformId, parentFolderId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;

            }

            return folders;
        }

        public library.Response AddFolder(string userName, Folder newFolder)
        {
           library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Post<Folder,library.Response>(String.Format("/insertFolder/{0}?format=xml", userName), newFolder, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response UpdateFolder(string userName, Folder updatedFolder)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Put<Folder, library.Response>(String.Format("/updateFolder/{0}?format=xml", userName), updatedFolder, true);
                
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response DeleteFolder(Folder folder)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Delete<Folder,library.Response>(String.Format("/deleteFolder/{0}?format=xml", folder.FolderId), folder, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public Contexts GetContexts(string userName, Guid parentFolderId)
        {
            Contexts contexts = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                contexts = client.Get<org.iringtools.applicationConfig.Contexts>(String.Format("/contexts/{0}?folderId={1}&format=xml", userName, parentFolderId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;

            }

            return contexts;
        }

        public library.Response AddContext(string userName, Context newContext)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Post<Context, library.Response>(String.Format("/insertContext/{0}?format=xml", userName), newContext, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response UpdateContext(string userName, Context updatedContext)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Put<Context, library.Response>(String.Format("/updateContext/{0}?format=xml", userName), updatedContext, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response DeleteContext(Context context)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Delete<Context, library.Response>(String.Format("/deleteContext/{0}?format=xml", context.ContextId), context, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public Applications GetApplications(string userName, Guid parentContextId)
        {
            Applications applications = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                applications = client.Get<org.iringtools.applicationConfig.Applications>(String.Format("/applications/{0}?contextId={1}&format=xml", userName, parentContextId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return applications;
        }

        public string AddApplication(string userName, Application newApplication)
        {
            string obj = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Post<Application>(String.Format("/insertApplication/{0}?format=xml", userName), newApplication, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string UpdateApplication(string userName, Application updatedApplication)
        {
            string obj = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Put<Application>(String.Format("/updateApplication/{0}?format=xml", userName), updatedApplication, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string DeleteApplication(Application application)
        {
            string obj = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Delete<Application>(String.Format("/deleteApplication/{0}?format=xml", application.ApplicationId), application, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        internal library.DataObjects GetDataObjectsForAnApplication(string userName, Guid guid)
        {
            library.DataObjects dataObjects = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                dataObjects = client.Get<library.DataObjects>(String.Format(""));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return dataObjects;
        }

        protected WebHttpClient CreateWebClient(string baseUri)
        {
            WebHttpClient client = null;
            client = new WebHttpClient(baseUri);
            return client;
        }
    }
}