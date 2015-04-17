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
using org.iringtools.mapping;
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
    internal class ApplicationConfigurationRepository : IApplicationConfigurationRepository
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ApplicationConfigurationRepository));
        //private library.CustomError _CustomError = null;
        protected library.ServiceSettings _settings;
        protected string _proxyHost;
        protected string _proxyPort;

        string applicationConfigurationServiceUri = null;
        string dictionaryServiceUri = null;

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

            dictionaryServiceUri = _settings["DictionaryServiceUri"];
            if (dictionaryServiceUri.EndsWith("/"))
                dictionaryServiceUri = dictionaryServiceUri.Remove(dictionaryServiceUri.Length - 1);
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

        public org.iringtools.applicationConfig.Contexts GetContexts(string userName, Guid parentFolderId)
        {
            org.iringtools.applicationConfig.Contexts contexts = null;

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

        public library.Response AddContext(org.iringtools.applicationConfig.Context newContext)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Post<org.iringtools.applicationConfig.Context, library.Response>(String.Format("/insertContext?format=xml"), newContext, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response UpdateContext(org.iringtools.applicationConfig.Context updatedContext)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Put<org.iringtools.applicationConfig.Context, library.Response>(String.Format("/updateContext?format=xml"), updatedContext, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }

        public library.Response DeleteContext(org.iringtools.applicationConfig.Context context)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Delete<org.iringtools.applicationConfig.Context, library.Response>(String.Format("/deleteContext/{0}?format=xml", context.ContextId), context, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return response;
        }
   
        public library.Response DragAndDropEntity(string resourceType, Guid sourceId, Guid destinationId, int siteId, int platformId)
        {
            library.Response response = null;

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                response = client.Get<library.Response>(String.Format("/DragAndDropEntity?resourceType={0}&SourceId={1}&DestinationId={2}&siteId={3}&platformId={4}&format=xml", resourceType, sourceId, destinationId, siteId, platformId));
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

        public org.iringtools.library.DataDictionary GetDictionary(Guid applicationId)
        {
            org.iringtools.library.DataDictionary dataDictionary = new org.iringtools.library.DataDictionary();

            try
            {
                WebHttpClient client = CreateWebClient(dictionaryServiceUri);
                org.iringtools.library.DatabaseDictionary databaseDictionary = client.Get<org.iringtools.library.DatabaseDictionary>(String.Format("/GetDictionary?applicationId={0}&format=xml", applicationId));
                dataDictionary = Utility.Deserialize<org.iringtools.library.DataDictionary>((Utility.Serialize<org.iringtools.library.DatabaseDictionary>(databaseDictionary)).ToString(), true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return dataDictionary;
        }

        public Graphs GetGraphs(string userName, Guid applicationId)
        {
            org.iringtools.applicationConfig.Graphs graphs = new Graphs();

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

        public ValueListMaps GetValueListMaps(string userName, Guid applicationId)
        {
            ValueListMaps valueListMaps = new ValueListMaps();

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                valueListMaps = client.Get<ValueListMaps>(String.Format("/valueList/{0}?applicationId={1}&format=xml", userName, applicationId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return valueListMaps;
        }

        public ApplicationSettings GetAppSettings()
        {
            ApplicationSettings items = new ApplicationSettings();

            logger.Debug("In ApplicatonConfigRepository GetAppSettings");
            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                items = client.Get<ApplicationSettings>("/appSettings?format=xml");

                logger.Debug("Successfully called Adpter Service.");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw ex;

            }
            return items;
        }

        public DataLayers GetDataLayers(int siteId, int platformId)
        {
            DataLayers dataLayers = new DataLayers();

            try
            {
                WebHttpClient client = CreateWebClient(applicationConfigurationServiceUri);
                dataLayers = client.Get<DataLayers>(String.Format("/GetDataLayers?siteId={0}&platformId={1}&format=xml", siteId, platformId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return dataLayers;
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