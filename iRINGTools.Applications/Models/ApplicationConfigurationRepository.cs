using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.applicationConfig;
using org.iringtools.utility;
using log4net;
using System.Net;
using System.Collections.Specialized;
using System.Configuration;

namespace iRINGTools.Web.Models
{
    internal class ApplicationConfigurationRepository : IApplicationConfigurationRepository
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ApplicationConfigurationRepository));

        AdapterRepository adapter;

        string applicationConfigurationServiceUri = null;

        //internal ApplicationConfigurationRepository()
        //{
        //    NameValueCollection settings = ConfigurationManager.AppSettings;
        //    serviceSettings.AppendSettings(settings);

        //    proxyHost = settings["ProxyHost"];
        //    proxyPort = settings["ProxyPort"];

        //    applicationConfigurationServiceUri = settings["ApplicationConfigServiceUri"];

        //    if (applicationConfigurationServiceUri.EndsWith("/"))
        //    {
        //        applicationConfigurationServiceUri = applicationConfigurationServiceUri.Remove(applicationConfigurationServiceUri.Length - 1);
        //    }
        //}

        internal ApplicationConfigurationRepository(AdapterRepository _adapter)
        {
            adapter = _adapter;

            NameValueCollection settings = ConfigurationManager.AppSettings;
            
            applicationConfigurationServiceUri = settings["ApplicationConfigServiceUri"];

            if (applicationConfigurationServiceUri.EndsWith("/"))
            {
                applicationConfigurationServiceUri = applicationConfigurationServiceUri.Remove(applicationConfigurationServiceUri.Length - 1);
            }
        }

        public Folders GetFolders(string userName, Guid parentFolderId, int siteId)
        {
            Folders folders = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                folders = client.Get<Folders>(String.Format("/folders/{0}?siteId={1}&parentFolderId={2}&format=xml", userName, siteId, parentFolderId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;

            }

            return folders;
        }

        public string AddFolder(string userName, Folder newFolder)
        {
            string obj = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Post<Folder>(String.Format("/insertFolder/{0}?format=xml", userName), newFolder, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string UpdateFolder(string userName, Folder updatedFolder)
        {
            string obj = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Put<Folder>(String.Format("/updateFolder/{0}?format=xml", userName), updatedFolder, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string DeleteFolder(Folder folder)
        {
            string obj = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Delete<Folder>(String.Format("/deleteFolder/{0}?format=xml", folder.FolderId), folder, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public Contexts GetContexts(string userName, Guid parentFolderId, int siteId)
        {
            Contexts contexts = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                contexts = client.Get<org.iringtools.applicationConfig.Contexts>(String.Format("/contexts/{0}?siteId={1}&folderId={2}&format=xml", userName, siteId, parentFolderId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;

            }

            return contexts;
        }

        public string AddContext(string userName, Context newContext)
        {
            string obj = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Post<Context>(String.Format("/insertContext/{0}?format=xml", userName), newContext, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string UpdateContext(string userName, Context updatedContext)
        {
            string obj = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Put<Context>(String.Format("/updateContext/{0}?format=xml", userName), updatedContext, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public string DeleteContext(Context context)
        {
            string obj = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Delete<Context>(String.Format("/deleteContext/{0}?format=xml", context.ContextId), context, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }

        public Applications GetApplications(string userName, Guid parentContextId, int siteId)
        {
            Applications applications = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                applications = client.Get<org.iringtools.applicationConfig.Applications>(String.Format("/applications/{0}?siteId={1}&contextId={2}&format=xml", userName, siteId, parentContextId));
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
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
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
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
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
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                obj = client.Delete<Application>(String.Format("/deleteApplication/{0}?format=xml", application.ApplicationId), application, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return obj;
        }
    }
}