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

        AdapterRepository adapter;

        string applicationConfigurationServiceUri = null;

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

        public library.Response AddFolder(string userName, Folder newFolder)
        {
           library.Response response = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
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
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
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
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                response = client.Delete<Folder,library.Response>(String.Format("/deleteFolder/{0}?format=xml", folder.FolderId), folder, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
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

        public library.Response AddContext(string userName, Context newContext)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
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
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
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
                WebHttpClient client = adapter.CreateWebClient(applicationConfigurationServiceUri);
                response = client.Delete<Context, library.Response>(String.Format("/deleteContext/{0}?format=xml", context.ContextId), context, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
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