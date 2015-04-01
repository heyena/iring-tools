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
using org.iringtools.web.Models;

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

        public library.Response AddFolder(Folder newFolder)
        {
           library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Post<Folder,library.Response>(String.Format("/insertFolder?format=xml"), newFolder, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response UpdateFolder(Folder updatedFolder)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Put<Folder, library.Response>(String.Format("/updateFolder?format=xml"), updatedFolder, true);
                
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

        public library.Response AddContext(Context newContext)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Post<Context, library.Response>(String.Format("/insertContext?format=xml"), newContext, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response UpdateContext(Context updatedContext)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Put<Context, library.Response>(String.Format("/updateContext?format=xml"), updatedContext, true);
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

   
        public library.Response DragAndDropEntity(string resourceType, Guid droppedEntityId, Guid destinationParentEntityId, int siteId, int platformId)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Get<library.Response>(String.Format("/entityAfterDrop?resourceType={0}&droppedEntityId={1}&destinationParentEntityId={2}&siteId={3}&platformId={4}&format=xml", resourceType, droppedEntityId, destinationParentEntityId, siteId, platformId));
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

        public library.Response AddApplication(Application newApplication)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Post<Application, library.Response>(String.Format("/insertApplication?format=xml"), newApplication, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response UpdateApplication(Application updatedApplication)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Put<Application, library.Response>(String.Format("/updateApplication?format=xml"), updatedApplication, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response DeleteApplication(Application application)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Delete<Application, library.Response>(String.Format("/deleteApplication/{0}?format=xml", application.ApplicationId), application, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public org.iringtools.applicationConfig.DataDictionary GetDictionary(Guid applicationId)
        {
            org.iringtools.applicationConfig.DataDictionary dataDictionary = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                dataDictionary = client.Get<org.iringtools.applicationConfig.DataDictionary>(String.Format("/GetDictionary?applicationId={0}&format=xml", applicationId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return dataDictionary;
        }

        public org.iringtools.applicationConfig.Graphs GetGraphs(string userName, Guid applicationId)
        {
            org.iringtools.applicationConfig.Graphs graphs = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                graphs = client.Get<org.iringtools.applicationConfig.Graphs>(String.Format("/graphs/{0}?applicationId={1}&format=xml", userName, applicationId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return graphs;
        }

        #region Protected Member Functions

        protected WebHttpClient CreateWebClient(string baseUri)
        {
            WebHttpClient client = null;
            client = new WebHttpClient(baseUri);
            return client;
        }

        #endregion
    }
}